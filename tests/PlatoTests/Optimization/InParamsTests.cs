// Checks the --in-params switch (TKT-1066): large readonly structs pass by reference.
using System.Reflection;
using Ara3D.Geometry.CSharpWriter;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Framework;
using RuntimeInteger = Ara3D.Geometry.Integer;
using RuntimeNumber = Ara3D.Geometry.Number;

namespace PlatoTests.Optimization;

/// <summary>
/// TKT-1066: with <c>--in-params</c> a struct of at least 256 flattened bytes is read through a
/// reference: <c>in</c> parameters (the <c>this</c> receiver included) unless a lambda captures them,
/// a <c>ref readonly</c> table accessor, <c>ref readonly var</c> locals, and element reads through
/// <c>RefAccess.RefAt</c>. Off, the output has none of it. Both compile and compute the same values,
/// over an array table and over a list that is not an array (the copying slow path).
/// </summary>
[TestFixture]
public static class InParamsTests
{
    private const string Probe = "InParamsProbe.g.cs";
    private static readonly string[] Records = { "_InWide.g.cs", "_InNarrow.g.cs", "_InTable.g.cs" };
    private sealed record Program(CSharpWriter Writer, string Text, Assembly Assembly);
    private static readonly Dictionary<(bool, bool), Program> Programs = new();

    private static readonly Lazy<Ara3D.Geometry.Compiler.Compilation> Compilation = new(() =>
    {
        var foundation = Path.Combine(CheckerTestSupport.FindForwardStdLib(), "foundation");
        var fixture = Path.Combine(CheckerTestSupport.FindFolder("PlatoTests"), "Optimization", "InParamsFixtures", "in-params.plato");
        return new Ara3D.Geometry.Compiler.Compilation(Ara3D.Logging.Logger.Null,
            Directory.GetFiles(foundation, "*.plato").Select(CheckerTestSupport.ParseFile)
                .Append(CheckerTestSupport.ParseFile(fixture)).ToList());
    });

    private static HashSet<string> ProbeNames
        => Compilation.Value.LibraryDefinitionsByName["InParamsProbe"].Functions.Select(f => f.Name).ToHashSet();

    private static Program Get(bool enabled, bool loops)
    {
        if (Programs.TryGetValue((enabled, loops), out var cached))
            return cached;
        var writer = new CSharpWriter(Compilation.Value, "unused-in-params")
            { ExtensionStyle = true, LowerLoops = loops, InParameters = enabled };
        writer.WriteAll();
        var names = ProbeNames;
        // The probe's own methods, plus the array Map it calls when loops are not lowered.
        var probe = KeepMethods(writer.Files[Probe].ToString(), names.Contains);
        var map = KeepMethods(writer.Files["Extensions.g.cs"].ToString(), n => n == "Map");
        var sources = Records.Select(f => writer.Files[f].ToString()).Append(probe).Append(map)
            .Concat(writer.Files.TryGetValue(LargeStructPassing.HelperFile, out var helper) ? new[] { helper.ToString() } : Array.Empty<string>())
            .Append(NumberOperatorAdapters(writer))
            .Prepend("global using System; global using System.Collections.Generic;").ToArray();
        var assembly = CSharpTestCompilation.CompileAndLoad(sources, LanguageVersion.CSharp12, OptimizationLevel.Release);
        var result = new Program(writer, probe, assembly);
        Programs[(enabled, loops)] = result;
        return result;
    }

    // Partial structs cannot extend the runtime in another assembly: the generated Number.Add and
    // Number.Multiply bodies become test-local extension methods (as in MultiConsumerFusionTests).
    internal static string NumberOperatorAdapters(CSharpWriter writer)
    {
        var methods = CSharpSyntaxTree.ParseText(writer.Files["_Number.g.cs"].ToString()).GetRoot()
            .DescendantNodes().OfType<MethodDeclarationSyntax>()
            .Where(m => m.Identifier.Text is "Add" or "Multiply"
                && !m.Modifiers.Any(SyntaxKind.StaticKeyword) && m.ParameterList.Parameters.Count == 1
                && m.ParameterList.Parameters[0].Type!.ToString() == "Number").ToList();
        Assert.That(methods, Has.Count.EqualTo(2));
        var receiver = SyntaxFactory.Parameter(SyntaxFactory.Identifier("self"))
            .WithType(SyntaxFactory.ParseTypeName("Number"))
            .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.ThisKeyword)));
        var adapted = methods.Select(method => method
            .ReplaceNodes(method.DescendantNodes().OfType<ThisExpressionSyntax>(), (_, _) => SyntaxFactory.IdentifierName("self"))
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.StaticKeyword))
            .WithParameterList(method.ParameterList.WithParameters(method.ParameterList.Parameters.Insert(0, receiver)))
            .NormalizeWhitespace().ToFullString());
        return "using System.Runtime.CompilerServices; using static System.Runtime.CompilerServices.MethodImplOptions; "
            + "namespace Ara3D.Geometry { public static class NumberAdapters { " + string.Join(" ", adapted) + " } }";
    }

    private static string KeepMethods(string source, Func<string, bool> keep)
    {
        var root = CSharpSyntaxTree.ParseText(source).GetRoot();
        return root.RemoveNodes(root.DescendantNodes().OfType<MethodDeclarationSyntax>()
            .Where(m => !keep(m.Identifier.Text)), SyntaxRemoveOptions.KeepNoTrivia)!.ToFullString();
    }

    private static MethodDeclarationSyntax Method(Program p, string name)
        => CSharpSyntaxTree.ParseText(p.Text).GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>()
            .Single(m => m.Identifier.Text == name);

    private static string Modifiers(Program p, string name)
        => string.Join(",", Method(p, name).ParameterList.Parameters
            .Select(x => string.Join(" ", x.Modifiers.Select(m => m.Text))));

    [Test]
    public static void SizesAreTheFlattenedFields()
    {
        var sizes = Get(true, true).Writer.LargeStructs.Sizes;
        Assert.That(sizes["InWide"], Is.EqualTo(320));
        Assert.That(sizes["InNarrow"], Is.EqualTo(16));
        Assert.That(sizes["InTable"], Is.EqualTo(8));
        Assert.That(LargeStructPassing.ThresholdBytes, Is.EqualTo(256));
    }

    [TestCase(true)]
    [TestCase(false)]
    public static void OffEmitsNoneOfIt(bool loops)
    {
        var off = Get(false, loops);
        Assert.That(off.Writer.LargeStructs, Is.Null);
        Assert.That(off.Writer.Files.ContainsKey(LargeStructPassing.HelperFile), Is.False);
        Assert.That(off.Text, Does.Not.Contain("ref readonly").And.Not.Contain("RefAccess").And.Not.Contain(" in "));
        foreach (var name in ProbeNames)
            Assert.That(Modifiers(off, name), Does.Not.Contain("in"), name);
    }

    [TestCase(true)]
    [TestCase(false)]
    public static void OnPassesLargeStructsByReference(bool loops)
    {
        var on = Get(true, loops);
        Assert.That(on.Writer.Files.ContainsKey(LargeStructPassing.HelperFile), Is.True);
        Assert.That(Method(on, "RowAt").ToString(),
            Does.Contain("public static ref readonly InWide RowAt(this InTable t, Integer i) => ref RefAccess.RefAt(t.Rows, i);"));
        Assert.That(Modifiers(on, "Total"), Is.EqualTo("this in"));
        Assert.That(Modifiers(on, "Scaled"), Is.EqualTo("this in,,"));
        Assert.That(Modifiers(on, "Shifted"), Is.EqualTo("this,"), "a lambda captures w");
        Assert.That(Modifiers(on, "RowTotal"), Is.EqualTo("this,"), "InTable is small");
        Assert.That(Method(on, "LocalTotal").ToString(), Does.Contain("ref readonly var w = ref t.RowAt(i);"));
        Assert.That(Method(on, "CapturedLocal").ToString(), Does.Contain("var w = t.RowAt(i);").And.Not.Contain("ref readonly var"));
        Assert.That(Method(on, "DirectTotal").ToString(), Does.Contain("RefAccess.RefAt(t.Rows, i).Total()"));
        Assert.That(Method(on, "DirectArgument").ToString(), Does.Contain("RefAccess.RefAt(t.Rows, i)"));
        Assert.That(Method(on, "DirectLocal").ToString(), Does.Contain("ref readonly var w = ref RefAccess.RefAt(t.Rows, i);"));
    }

    private static object Wide(Assembly a, int seed)
        => Activator.CreateInstance(a.GetType("Ara3D.Geometry.InWide", true)!,
            Enumerable.Range(0, 40).Select(k => (object)(RuntimeNumber)(seed * 100.0 + k + 0.25)).ToArray())!;

    private static object Table(Assembly a, bool array)
    {
        var wide = a.GetType("Ara3D.Geometry.InWide", true)!;
        var rows = Array.CreateInstance(wide, 3);
        for (var i = 0; i < 3; i++)
            rows.SetValue(Wide(a, i + 1), i);
        // A list that is not an array takes RefAt's copying path.
        var list = array ? (object)rows
            : Activator.CreateInstance(typeof(System.Collections.ObjectModel.ReadOnlyCollection<>).MakeGenericType(wide), rows)!;
        return Activator.CreateInstance(a.GetType("Ara3D.Geometry.InTable", true)!, list)!;
    }

    private static object Narrow(Assembly a)
        => Activator.CreateInstance(a.GetType("Ara3D.Geometry.InNarrow", true)!, (RuntimeNumber)7.5, (RuntimeNumber)(-1.0))!;

    private static string Run(Program p, string name, Func<Assembly, object[]> args)
    {
        var method = p.Assembly.GetTypes().SelectMany(t => t.GetMethods()).Single(m => m.Name == name);
        var result = method.Invoke(null, args(p.Assembly));
        return result is System.Collections.IEnumerable list
            ? string.Join(";", list.Cast<object>().Select(x => BitConverter.DoubleToInt64Bits((double)(RuntimeNumber)x)))
            : BitConverter.DoubleToInt64Bits((double)(RuntimeNumber)result!).ToString();
    }

    [TestCase(true, true)]
    [TestCase(true, false)]
    [TestCase(false, true)]
    [TestCase(false, false)]
    public static void OnComputesWhatOffComputes(bool loops, bool arrayTable)
    {
        var off = Get(false, loops);
        var on = Get(true, loops);
        var xs = new[] { (RuntimeNumber)0.5, (RuntimeNumber)(-3.0) };
        var cases = new (string Name, Func<Assembly, object[]> Args)[]
        {
            ("Total", a => new[] { Wide(a, 4) }),
            ("Scaled", a => new[] { Wide(a, 5), Narrow(a), (RuntimeNumber)3.0 }),
            ("Shifted", a => new object[] { Wide(a, 6), xs }),
            ("RowTotal", a => new[] { Table(a, arrayTable), (RuntimeInteger)1 }),
            ("LocalTotal", a => new[] { Table(a, arrayTable), (RuntimeInteger)2 }),
            ("CapturedLocal", a => new object[] { Table(a, arrayTable), (RuntimeInteger)0, xs }),
            ("DirectTotal", a => new[] { Table(a, arrayTable), (RuntimeInteger)1 }),
            ("DirectArgument", a => new[] { Table(a, arrayTable), (RuntimeInteger)2, Narrow(a) }),
            ("DirectLocal", a => new[] { Table(a, arrayTable), (RuntimeInteger)0 }),
            ("Totals", a => new[] { Table(a, arrayTable) }),
        };
        foreach (var (name, args) in cases)
            Assert.That(Run(on, name, args), Is.EqualTo(Run(off, name, args)), name);
    }
}
