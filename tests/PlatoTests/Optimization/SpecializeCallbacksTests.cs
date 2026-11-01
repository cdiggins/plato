// Checks --specialize-callbacks: a lambda passed to a statement-body callee becomes a call to a delegate-free clone.
using System.Reflection;
using Ara3D.Geometry.CSharpWriter;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Framework;
using RuntimeNumber = Ara3D.Geometry.Number;

namespace PlatoTests.Optimization;

/// <summary>
/// plato-brep lambda removal L2: with <c>--specialize-callbacks</c> a call that passes a lambda literal to a
/// function-typed parameter of a callee the inliner refuses (a statement body) calls a clone of the callee
/// in <c>CallbackSpecializations.g.cs</c>, with the lambda body in place of every invocation and the captured
/// values as extra parameters. A callee that forwards the callback is specialized at the next depth; one that
/// stores it is refused and keeps its lambda. Off, nothing changes. Both compile and compute the same values.
/// </summary>
[TestFixture]
public static class SpecializeCallbacksTests
{
    private const string Probe = "CallbackProbe.g.cs";
    private sealed record Program(CSharpWriter Writer, string Text, Assembly Assembly);
    private static readonly Dictionary<(bool, bool), Program> Programs = new();

    private static readonly Lazy<Ara3D.Geometry.Compiler.Compilation> Compilation = new(() =>
    {
        var foundation = Path.Combine(CheckerTestSupport.FindForwardStdLib(), "foundation");
        var fixture = Path.Combine(CheckerTestSupport.FindFolder("PlatoTests"), "Optimization", "SpecializeCallbacksFixtures", "callbacks.plato");
        return new Ara3D.Geometry.Compiler.Compilation(Ara3D.Logging.Logger.Null,
            Directory.GetFiles(foundation, "*.plato").Select(CheckerTestSupport.ParseFile)
                .Append(CheckerTestSupport.ParseFile(fixture)).ToList());
    });

    private static HashSet<string> ProbeNames
        => Compilation.Value.LibraryDefinitionsByName["CallbackProbe"].Functions
            .Concat(Compilation.Value.LibraryDefinitionsByName["CallbackProbeGeneric"].Functions).Select(f => f.Name).ToHashSet();

    private static Program Get(bool enabled, bool inParams = false)
    {
        if (Programs.TryGetValue((enabled, inParams), out var cached))
            return cached;
        var writer = new CSharpWriter(Compilation.Value, "unused-specialize-callbacks")
            { ExtensionStyle = true, SpecializeCallbacks = enabled, InParameters = inParams };
        writer.WriteAll();
        var names = ProbeNames;
        // The probe's methods: those over a Number receiver in its own library file, those over a
        // Function1 receiver (Apply) among the runtime-class extensions of Extensions.g.cs.
        var sources = new List<string> { "global using System; global using System.Collections.Generic;", writer.Files["_PairA.g.cs"].ToString(), writer.Files["_PairB.g.cs"].ToString() };
        sources.AddRange(new[] { Probe, "Extensions.g.cs" }.Select(f => KeepMethods(writer.Files[f].ToString(), names.Contains)));
        // The clones of the probe's callees only: the foundation's own clones name types the test runtime lacks.
        if (writer.Files.TryGetValue(CallbackSpecializations.FileName, out var clones))
            sources.Add(KeepMethods(clones.ToString(), n => names.Any(p => n.StartsWith(p + "_"))));
        if (writer.Files.TryGetValue(LargeStructPassing.HelperFile, out var helper))
            sources.Add(helper.ToString());
        sources.Add(InParamsTests.NumberOperatorAdapters(writer));
        var assembly = CSharpTestCompilation.CompileAndLoad(sources.ToArray(), LanguageVersion.CSharp12, OptimizationLevel.Release);
        return Programs[(enabled, inParams)] = new Program(writer, string.Join(Environment.NewLine, sources), assembly);
    }

    private static string KeepMethods(string source, Func<string, bool> keep)
    {
        var root = CSharpSyntaxTree.ParseText(source).GetRoot();
        return root.RemoveNodes(root.DescendantNodes().OfType<MethodDeclarationSyntax>()
            .Where(m => !keep(m.Identifier.Text)), SyntaxRemoveOptions.KeepNoTrivia)!.ToFullString();
    }

    private static string Method(Program p, string name)
        => CSharpSyntaxTree.ParseText(p.Text).GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>()
            .Single(m => m.Identifier.Text == name).ToString();

    private static IReadOnlyList<string> Clones(Program p, string callee)
        => p.Writer.Specializations.Clones.Where(c => c.Callee.Name == callee).Select(c => c.Name).ToList();

    [Test]
    public static void OffKeepsEveryLambda()
    {
        var off = Get(false);
        Assert.That(off.Writer.Specializations.Clones, Is.Empty);
        Assert.That(off.Writer.Files.ContainsKey(CallbackSpecializations.FileName), Is.False);
        Assert.That(off.Text, Does.Not.Contain(CallbackSpecializations.ClassName));
        Assert.That(Method(off, "Scaled"), Does.Contain(".Iterate((a)"));
        Assert.That(Method(off, "Iterate"), Does.Contain("System.Func<Number, Number> f").And.Contain("f.Invoke("));
    }

    [Test]
    public static void OnCallsCloneWithCapturesAsParameters()
    {
        var on = Get(true);
        var registry = on.Writer.Specializations;
        Assert.That(on.Writer.Files.ContainsKey(CallbackSpecializations.FileName), Is.True);
        // Scaled and ScaledTwice share the Iterate clone: the same shape with captures abstracted. Doubled is
        // another shape of Iterate, Applied a clone of Apply, ScaledTwice a clone of IterateTwice.
        Assert.That(Clones(on, "Iterate"), Has.Count.EqualTo(2));
        Assert.That(Clones(on, "IterateTwice"), Has.Count.EqualTo(1));
        Assert.That(Clones(on, "Apply"), Has.Count.EqualTo(1));
        Assert.That(Clones(on, "Stored"), Is.Empty);
        Assert.That(registry.Refusals.Keys, Does.Contain("callback stored or escapes"));

        var scaled = Method(on, "Scaled");
        var iterate = registry.Clones.Single(c => c.Callee.Name == "Iterate" && c.ParameterTypes.Count == 3);
        Assert.That(scaled, Does.Contain($"{CallbackSpecializations.ClassName}.{iterate.Name}(x, k, x)"));
        Assert.That(scaled, Does.Not.Contain("=> a").And.Not.Contain("(a)"));
        Assert.That(iterate.Name, Does.StartWith("Iterate_"));
        Assert.That(iterate.ParameterTypes, Is.EqualTo(new[] { "Number", "Number", "Number" }));
        var clone = Method(on, iterate.Name);
        Assert.That(clone, Does.Contain($"public static Number {iterate.Name}(Number x, Number _cap0, Number _cap1)"));
        Assert.That(clone, Does.Contain("x.Multiply(_cap0).Add(_cap1)").And.Contain("once.Multiply(_cap0).Add(_cap1)"));

        // The forwarding clone calls the inner clone: no delegate anywhere in the file.
        var twice = registry.Clones.Single(c => c.Callee.Name == "IterateTwice");
        Assert.That(Method(on, twice.Name), Does.Contain($"{CallbackSpecializations.ClassName}.{iterate.Name}("));
        Assert.That(twice.Depth, Is.EqualTo(1));
        var clonesFile = on.Writer.Files[CallbackSpecializations.FileName].ToString();
        Assert.That(clonesFile, Does.Not.Contain("Func<").And.Not.Contain(".Invoke("));

        // The receiver-position callback: the clone's first parameter is the next one.
        var apply = registry.Clones.Single(c => c.Callee.Name == "Apply");
        Assert.That(Method(on, apply.Name), Does.Contain($"{apply.Name}(Number x, Number _cap0)"));
        Assert.That(Method(on, "Applied"), Does.Contain($"{CallbackSpecializations.ClassName}.{apply.Name}(x, k)"));

        // The stored callback keeps its lambda.
        Assert.That(Method(on, "StoredDoubled"), Does.Contain(".Stored((a)"));
        Assert.That(Method(on, "Stored"), Does.Contain("System.Func<Number, Number> f"));
    }

    [Test]
    public static void CloneNamesAreTheShapeHash()
    {
        var on = Get(true);
        foreach (var clone in on.Writer.Specializations.Clones)
            Assert.That(clone.Name, Does.Match($"^{clone.Callee.Name}_[0-9a-f]{{8}}$"), clone.Name);
        // A second generation names the same clones in the same order.
        var again = new CSharpWriter(Compilation.Value, "unused-specialize-callbacks") { ExtensionStyle = true, SpecializeCallbacks = true };
        again.WriteAll();
        Assert.That(again.Specializations.Clones.Select(c => c.Name), Is.EqualTo(on.Writer.Specializations.Clones.Select(c => c.Name)));
        Assert.That(again.Files[CallbackSpecializations.FileName].ToString(), Is.EqualTo(on.Writer.Files[CallbackSpecializations.FileName].ToString()));
    }

    [Test]
    public static void GenericCalleeIsClonedPerElementType()
    {
        var on = Get(true);
        // The same lambda shape over two element types: two clones, each call bound to its own.
        var clones = on.Writer.Specializations.Clones.Where(c => c.Callee.Name == "Agree").ToList();
        Assert.That(clones, Has.Count.EqualTo(2));
        Assert.That(clones.Select(c => c.ParameterTypes[0]), Is.EquivalentTo(new[] { "IReadOnlyList<PairA>", "IReadOnlyList<PairB>" }));
        var a = clones.Single(c => c.ParameterTypes[0] == "IReadOnlyList<PairA>");
        var b = clones.Single(c => c.ParameterTypes[0] == "IReadOnlyList<PairB>");
        Assert.That(Method(on, "AgreeA"), Does.Contain($"{CallbackSpecializations.ClassName}.{a.Name}(xs, ys)"));
        Assert.That(Method(on, "AgreeB"), Does.Contain($"{CallbackSpecializations.ClassName}.{b.Name}(xs, ys)"));
        Assert.That(a.Name, Is.Not.EqualTo(b.Name));
        var pairA = on.Assembly.GetType("Ara3D.Geometry.PairA", true)!;
        var xs = Array.CreateInstance(pairA, 2);
        xs.SetValue(Activator.CreateInstance(pairA, (RuntimeNumber)1.0, (RuntimeNumber)1.5), 0);
        xs.SetValue(Activator.CreateInstance(pairA, (RuntimeNumber)2.0, (RuntimeNumber)2.5), 1);
        var method = on.Assembly.GetTypes().SelectMany(t => t.GetMethods()).Single(m => m.Name == "AgreeA");
        Assert.That((bool)(Ara3D.Geometry.Boolean)method.Invoke(null, new object[] { xs, xs })!, Is.True);
    }

    private static double Run(Program p, string name, params double[] args)
    {
        var method = p.Assembly.GetTypes().SelectMany(t => t.GetMethods()).Single(m => m.Name == name);
        return (double)(RuntimeNumber)method.Invoke(null, args.Select(a => (object)(RuntimeNumber)a).ToArray())!;
    }

    [TestCase(false)]
    [TestCase(true)]
    public static void OnComputesWhatOffComputes(bool inParams)
    {
        var off = Get(false);
        var on = Get(true, inParams);
        var cases = new (string Name, double[] Args)[]
        {
            ("Scaled", new[] { 1.5, 3.0 }),
            ("ScaledTwice", new[] { -2.0, 0.5 }),
            ("Doubled", new[] { 7.25 }),
            ("Applied", new[] { 2.0, 4.0 }),
            ("StoredDoubled", new[] { 0.75 }),
        };
        foreach (var (name, args) in cases)
            Assert.That(Run(on, name, args), Is.EqualTo(Run(off, name, args)), name);
        Assert.That(Run(on, "Scaled", 1.5, 3.0), Is.EqualTo((1.5 * 3.0 + 1.5) * 3.0 + 1.5 + (1.5 * 3.0 + 1.5)));
        Assert.That(Run(on, "Applied", 2.0, 4.0), Is.EqualTo(16.0));
    }
}
