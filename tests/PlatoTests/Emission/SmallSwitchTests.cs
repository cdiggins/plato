// Checks the C# that each TKT-1095 emission switch writes, and that it computes the same values.
using System.Reflection;
using System.Text.RegularExpressions;
using Ara3D.Geometry.CSharpWriter;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Framework;

namespace PlatoTests.Emission;

using Boolean = Ara3D.Geometry.Boolean;
using Integer = Ara3D.Geometry.Integer;
using Number = Ara3D.Geometry.Number;

/// <summary>
/// TKT-1095: emission switches, each off by default. A probe function per switch is emitted
/// with the switch off and on; the test reads the emitted method's text and runs both builds.
/// </summary>
[TestFixture]
public static class SmallSwitchTests
{
    private sealed record Build(string Text, Assembly Assembly, IReadOnlyDictionary<string, string> Files)
    {
        public MethodInfo Method(string name)
            => Assembly.GetTypes().SelectMany(t => t.GetMethods()).Single(m => m.Name == name);

        public string Body(string name)
            => CSharpSyntaxTree.ParseText(Text).GetRoot().DescendantNodes()
                .OfType<MethodDeclarationSyntax>().Single(m => m.Identifier.Text == name).ToString();

        // A method of the emitted probe library, compiled or not.
        public string ProbeBody(string name)
            => CSharpSyntaxTree.ParseText(Files["SmallSwitchProbe.g.cs"]).GetRoot().DescendantNodes()
                .OfType<MethodDeclarationSyntax>().Single(m => m.Identifier.Text == name).ToString();
    }

    private static readonly string[] Probes = { "QuarterSquare", "TwiceQuarterSquare", "PlainHalf", "UsesConstants" };
    private static readonly Dictionary<string, Build> Builds = new();

    // --primitive-compare probes: compiled only when the switch is on, since the wrapper methods
    // the off build calls live in the generated partial structs the test assembly does not carry.
    private static readonly string[] Comparisons = { "Less", "LessOrEqual", "Greater", "GreaterOrEqual", "Same", "Different" };
    private static readonly string[] CompareProbes
        = Comparisons.Select(c => "Number" + c).Concat(Comparisons.Select(c => "Integer" + c)).Append("IndexInRange").ToArray();

    private static readonly Lazy<Ara3D.Geometry.Compiler.Compilation> Compilation = new(() =>
    {
        var foundation = Path.Combine(CheckerTestSupport.FindForwardStdLib(), "foundation");
        var fixture = Path.Combine(CheckerTestSupport.FindFolder("PlatoTests"), "Emission", "SmallSwitchFixtures", "switches.plato");
        return new Ara3D.Geometry.Compiler.Compilation(Ara3D.Logging.Logger.Null,
            Directory.GetFiles(foundation, "*.plato").Select(CheckerTestSupport.ParseFile)
                .Append(CheckerTestSupport.ParseFile(fixture)).ToList());
    });

    // key names the switches set, e.g. "cache" or "loops+cache"; "" is the default writer.
    // Without --loops the probes call the generated array combinators, which the test assembly
    // does not carry, so such a build is only read, not compiled.
    private static Build Emit(string key)
    {
        if (Builds.TryGetValue(key, out var built))
            return built;
        var on = key.Split('+').ToHashSet();
        var writer = new CSharpWriter(Compilation.Value, "unused-small-switches")
        {
            ExtensionStyle = true,
            LowerLoops = on.Contains("loops"),
            CacheConstants = on.Contains("cache"),
            PrimitiveCompare = on.Contains("cmp"),
        };
        writer.WriteAll();
        var keep = Probes.Concat(on.Contains("cmp") ? CompareProbes : Array.Empty<string>()).ToHashSet();
        var sources = new[] { "SmallSwitchProbe.g.cs", "Extensions.g.cs", "Constants.g.cs" }
            .Where(writer.Files.ContainsKey)
            .Select(f => KeepMethods(writer.Files[f].ToString(), keep.Contains))
            .Prepend("global using System; global using System.Collections.Generic;")
            .Append(NumberAdapters(writer))
            .ToArray();
        var assembly = on.Contains("loops")
            ? CSharpTestCompilation.CompileAndLoad(sources, LanguageVersion.CSharp12, OptimizationLevel.Release)
            : null;
        built = new Build(string.Join(Environment.NewLine, sources), assembly,
            writer.Files.ToDictionary(kv => kv.Key, kv => kv.Value.ToString()));
        Builds.Add(key, built);
        return built;
    }

    // Partial structs cannot extend the runtime in another assembly: the generated Number members
    // the probes call are kept with their bodies, as extension methods on a test-local receiver.
    private static readonly string[] NumberMembers = { "Add", "Multiply" };

    private static string NumberAdapters(CSharpWriter writer)
    {
        var methods = CSharpSyntaxTree.ParseText(writer.Files["_Number.g.cs"].ToString()).GetRoot()
            .DescendantNodes().OfType<MethodDeclarationSyntax>()
            .Where(m => NumberMembers.Contains(m.Identifier.Text)
                && !m.Modifiers.Any(SyntaxKind.StaticKeyword) && m.ParameterList.Parameters.Count == 1
                && m.ParameterList.Parameters[0].Type!.ToString() == "Number").ToList();
        Assert.That(methods, Has.Count.EqualTo(NumberMembers.Length));
        var receiver = SyntaxFactory.Parameter(SyntaxFactory.Identifier("self"))
            .WithType(SyntaxFactory.ParseTypeName("Number"))
            .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.ThisKeyword)));
        var adapted = methods.Select(method => method
            .ReplaceNodes(method.DescendantNodes().OfType<ThisExpressionSyntax>(), (_, _) => SyntaxFactory.IdentifierName("self"))
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.StaticKeyword))
            .WithParameterList(method.ParameterList.WithParameters(method.ParameterList.Parameters.Insert(0, receiver)))
            .NormalizeWhitespace().ToFullString());
        return "using System.Runtime.CompilerServices; using static System.Runtime.CompilerServices.MethodImplOptions; "
            + "namespace Ara3D.Geometry { public static class SmallSwitchAdapters { " + string.Join(" ", adapted) + " } }";
    }

    // Keeps the kept methods and the --cache-constants holder classes they read.
    private static string KeepMethods(string source, Func<string, bool> keep)
    {
        var root = CSharpSyntaxTree.ParseText(source).GetRoot();
        var drop = root.DescendantNodes().OfType<MethodDeclarationSyntax>()
            .Where(m => !keep(m.Identifier.Text)
                // IndexInRange is emitted per IIndexable implementor; only the list one is compiled.
                || m.Identifier.Text == "IndexInRange" && !m.ParameterList.ToString().Contains("IReadOnlyList"))
            .Cast<SyntaxNode>()
            .Concat(root.DescendantNodes().OfType<ClassDeclarationSyntax>()
                .Where(c => c.Identifier.Text.StartsWith("_Const_") && !keep(c.Identifier.Text["_Const_".Length..])));
        return root.RemoveNodes(drop, SyntaxRemoveOptions.KeepNoTrivia)!.ToFullString();
    }

    private static Number[] Numbers(params double[] xs) => xs.Select(x => (Number)x).ToArray();

    private static long Bits(object number) => BitConverter.DoubleToInt64Bits((double)((Number)number!).Value);

    [Test]
    public static void CacheConstantsReadsClosedConstantsFromAHolder()
    {
        var off = Emit("loops");
        var on = Emit("loops+cache");
        foreach (var name in new[] { "QuarterSquare", "TwiceQuarterSquare", "PlainHalf" })
            Assert.That(off.Body(name), Does.Not.Contain("_Const_"), name);
        Assert.That(on.Body("QuarterSquare"), Does.Contain("=> _Const_QuarterSquare.Value;"));
        Assert.That(on.Body("TwiceQuarterSquare"), Does.Contain("=> _Const_TwiceQuarterSquare.Value;"));
        Assert.That(on.Body("PlainHalf"), Does.Not.Contain("_Const_"), "a bare literal is left as it is");
        Assert.That(on.Body("UsesConstants"), Is.EqualTo(off.Body("UsesConstants")), "call sites are unchanged");
        // The holder is precise (an explicit static constructor), so its value is computed on first read.
        Assert.That(on.Text, Does.Contain("private static class _Const_QuarterSquare { static _Const_QuarterSquare() { } internal static readonly Number Value = "));
        // A type-level constant (`_: Number` receiver) on the partial struct is cached the same way.
        Assert.That(on.Files["_Number.g.cs"], Does.Contain("public static Number ProbeHalfUnit() => _Const_ProbeHalfUnit.Value;"));
        Assert.That(off.Files["_Number.g.cs"], Does.Not.Contain("_Const_"));

        foreach (var name in new[] { "QuarterSquare", "TwiceQuarterSquare", "PlainHalf" })
            Assert.That(Bits(on.Method(name).Invoke(null, null)), Is.EqualTo(Bits(off.Method(name).Invoke(null, null))), name);
        foreach (var x in new[] { 0.0, -0.0, 1.5, double.NaN, double.NegativeInfinity, 1e308 })
        {
            var args = new object[] { (Number)x };
            Assert.That(Bits(on.Method("UsesConstants").Invoke(null, args)), Is.EqualTo(Bits(off.Method("UsesConstants").Invoke(null, args))), $"x={x}");
        }
    }

    // The wrappers' own definitions: LessThanOrEquals is `<=` on the values, Equals is
    // double.Equals, LessThan is `a <= b && !a.Equals(b)`, and the Greater forms swap the operands.
    private static bool Expected(string comparison, double a, double b) => comparison switch
    {
        "Less" => a <= b && !a.Equals(b),
        "LessOrEqual" => a <= b,
        "Greater" => b <= a && !b.Equals(a),
        "GreaterOrEqual" => b <= a,
        "Same" => a.Equals(b),
        "Different" => !a.Equals(b),
        _ => throw new ArgumentException(comparison),
    };

    private static readonly string[] Wrappers = { "LessThan", "LessThanOrEquals", "GreaterThan", "GreaterThanOrEquals", "Equals", "NotEquals" };

    [Test]
    public static void PrimitiveCompareKeepsTheWrapperSemantics()
    {
        var off = Emit("loops");
        var on = Emit("loops+cmp");
        for (var i = 0; i < Comparisons.Length; i++)
            foreach (var type in new[] { "Number", "Integer" })
            {
                var name = type + Comparisons[i];
                Assert.That(off.ProbeBody(name), Does.Contain($".{Wrappers[i]}("), name);
                Assert.That(on.ProbeBody(name), Does.Contain(".Value").And.Not.Contain($".{Wrappers[i]}(b)"), name);
            }
        Assert.That(on.ProbeBody("NumberLess"), Does.Contain("((Boolean)(a.Value < b.Value))"));
        Assert.That(on.ProbeBody("NumberSame"), Does.Contain("((Boolean)(a.Value.Equals(b.Value)))"));
        Assert.That(on.ProbeBody("IntegerSame"), Does.Contain("((Boolean)(a.Value == b.Value))"));
        // A comparison across a conversion keeps the wrapper call.
        Assert.That(on.ProbeBody("MixedLess"), Is.EqualTo(off.ProbeBody("MixedLess")));

        var numbers = new[] { 0.0, -0.0, 1.0, -1.0, double.Epsilon, 1e-310, double.MaxValue,
            double.PositiveInfinity, double.NegativeInfinity, double.NaN, -double.NaN };
        foreach (var c in Comparisons)
        {
            var method = on.Method("Number" + c);
            foreach (var a in numbers)
            foreach (var b in numbers)
                Assert.That((bool)(Boolean)method.Invoke(null, new object[] { (Number)a, (Number)b })!,
                    Is.EqualTo(Expected(c, a, b)), $"Number{c}({a}, {b})");
        }
        Assert.That(on.Body("IndexInRange"), Does.Contain("((Integer)(xs.Count)).Value"), "the list's int Count is cast to Integer");
        var inRange = on.Method("IndexInRange").MakeGenericMethod(typeof(Number));
        foreach (var i in new[] { -1, 0, 2, 3, 4 })
            Assert.That((bool)(Boolean)inRange.Invoke(null, new object[] { Numbers(1, 2, 3), (Integer)i })!,
                Is.EqualTo(i >= 0 && i < 3), $"IndexInRange({i})");

        var integers = new[] { 0, 1, -1, int.MaxValue, int.MinValue, 7 };
        foreach (var c in Comparisons)
        {
            var method = on.Method("Integer" + c);
            foreach (var a in integers)
            foreach (var b in integers)
                Assert.That((bool)(Boolean)method.Invoke(null, new object[] { (Integer)a, (Integer)b })!,
                    Is.EqualTo(Expected(c, a, b)), $"Integer{c}({a}, {b})");
        }
    }
}
