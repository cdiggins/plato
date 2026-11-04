// Checks --scalar-arrays (TKT-1152 chunk C0): the constant Count of a fixed-arity struct folds, and the component
// arrays built over it unroll, so ComponentArray and its consumers allocate no array.
using Ara3D.Geometry.CSharpWriter;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Framework;

namespace PlatoTests.Optimization;

/// <summary>
/// With <c>--scalar-arrays</c> the stdlib's <c>ComponentArray(v)</c> consumers (<c>SumSqrComponents</c>,
/// <c>MinComponent</c>, <c>MapComponents</c>, <c>ZipComponents</c>, <c>FromComponents</c>, ...) over a
/// <c>Vector3D</c> are straight-line scalar code. Every program here is written with the nineteen pinned
/// switches of the plato-brep regeneration plus <c>--fuse</c>, because the rewrite meets
/// <c>--lower-scalars</c>, <c>--specialize-callbacks</c> and <c>--in-params</c> in the real output. These tests
/// read the emitted text; the values, the fault order and the three-way law run are in
/// <c>tests/Plato.Interpreter.Tests/ScalarArraysProgramTests.cs</c>, which compiles the whole program.
/// </summary>
[TestFixture]
public static class ScalarArraysTests
{
    private sealed record Program(CSharpWriter Writer, string Source);
    private static readonly Dictionary<bool, Program> Programs = new();

    private static readonly Lazy<Ara3D.Geometry.Compiler.Compilation> Compilation = new(() =>
    {
        var foundation = Path.Combine(CheckerTestSupport.FindForwardStdLib(), "foundation");
        var fixture = Path.Combine(Path.GetDirectoryName(SourceFile())!, "ScalarArraysFixtures", "scalar-arrays.plato");
        return new Ara3D.Geometry.Compiler.Compilation(Ara3D.Logging.Logger.Null,
            Directory.GetFiles(foundation, "*.plato").Select(CheckerTestSupport.ParseFile)
                .Append(CheckerTestSupport.ParseFile(fixture)).ToList());
    });

    // Found from this source file, not from the test output folder, so the test also runs from a build under --artifacts-path.
    private static string SourceFile([System.Runtime.CompilerServices.CallerFilePath] string path = "") => path;

    /// <summary>The pinned switches of the regeneration (tools/regen-step-brep.ps1 in plato-brep) and --fuse, which
    /// the regeneration's own flag list carries too, plus the one under test.</summary>
    internal static void Pin(CSharpWriter writer, bool scalarArrays)
    {
        writer.ExtensionStyle = true;
        writer.LowerLoops = true;
        writer.FuseCombinators = true;
        writer.FuseConsumers = true;
        writer.ExactOutputAllocation = true;
        writer.LeanInlining = true;
        writer.ShareCalls = true;
        writer.LowerScalars = true;
        writer.ConcreteArrays = true;
        writer.Presize = true;
        writer.InPlaceUpdates = true;
        writer.SinkBindings = true;
        writer.BindScrutinees = true;
        writer.ShareCallsWide = true;
        writer.ClosureFree = true;
        writer.InParameters = true;
        writer.SwitchMatch = true;
        writer.HoistInvariants = true;
        writer.CacheConstants = true;
        writer.PrimitiveCompare = true;
        writer.LowerMore = true;
        writer.SpecializeCallbacks = true;
        writer.ScalarArrays = scalarArrays;
    }

    private static Program Get(bool scalarArrays)
    {
        lock (Programs)
        {
            if (Programs.TryGetValue(scalarArrays, out var cached))
                return cached;
            var writer = new CSharpWriter(Compilation.Value, "unused-scalar-arrays");
            Pin(writer, scalarArrays);
            writer.WriteAll();
            return Programs[scalarArrays] = new Program(writer, string.Join("\n", writer.Files.Values.Select(f => f.ToString())));
        }
    }

    /// <summary>The method with this name and first parameter type, from the emitted file that holds it.</summary>
    private static string Method(Program p, string file, string name, string firstParameter)
        => CSharpSyntaxTree.ParseText(p.Writer.Files[file].ToString()).GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>()
            .Single(m => m.Identifier.Text == name && m.ParameterList.Parameters.Count > 0
                && m.ParameterList.Parameters[0].Type!.ToString() == firstParameter).ToString();

    private static IEnumerable<MethodDeclarationSyntax> Methods(Program p, string file, string name, string firstParameter)
        => CSharpSyntaxTree.ParseText(p.Writer.Files[file].ToString()).GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>()
            .Where(m => m.Identifier.Text == name && m.ParameterList.Parameters.Count > 0
                && m.ParameterList.Parameters[0].Type!.ToString() == firstParameter);

    private static string Probe(Program p, string name)
        => Methods(p, "ScalarArrayProbe.g.cs", name, name is "ArityN" ? "VectorN" : "Vector3D").Single().ToString();

    private static readonly string[] Allocations = { "new Number[", "MakeArray", "ComponentArray", "for (", "ConcreteArrayAccess" };

    /// <summary>The emitted text builds no array and loops over none: the tokens of the array path are absent.</summary>
    private static void AssertStraightLine(string text, string label)
        => Assert.That(Allocations.Where(text.Contains), Is.Empty, label + ": " + text);

    [Test]
    public static void OffKeepsEveryComponentArray()
    {
        var off = Get(false);
        Assert.That(off.Writer.ScalarArrays, Is.False);
        Assert.That(Probe(off, "Rebuilt"), Does.Contain("ComponentArray"));
        Assert.That(Probe(off, "Arity"), Does.Contain(".Count"));
        Assert.That(Method(off, "NumericStructures.g.cs", "ComponentArray", "Vector3D"), Does.Contain("new Number[").And.Contain("for ("));
        Assert.That(CloneBody(off, "Vector3D", "(_eval1_0 * _eval1_0)"), Does.Contain("ComponentArray").And.Contain("for ("));
    }

    [Test]
    public static void OnFoldsTheCountToTheArity()
    {
        var on = Get(true);
        Assert.That(Probe(on, "Arity"), Does.Not.Contain(".Count").And.Match(@"BitCast<\w+, Integer>\(3L?\)"));
        Assert.That(Probe(on, "Middle"), Does.Not.Contain(".Count"));
        // A VectorN is not fixed-arity: its Count is the length of its array.
        Assert.That(Probe(on, "ArityN"), Does.Contain(".Count"));
        // The one array the library body still makes is a literal of three reads, not a loop.
        var componentArray = Method(on, "NumericStructures.g.cs", "ComponentArray", "Vector3D");
        Assert.That(componentArray, Does.Not.Contain("for (").And.Not.Contain("new Number["));
    }

    [Test]
    public static void OnUnrollsTheConsumersOfTheComponentArray()
    {
        var on = Get(true);
        foreach (var name in new[] { "MapComponents", "ZipComponents", "ReduceComponents", "AllComponents", "AnyComponent", "AllZipComponents" })
        {
            var methods = Methods(on, "NumericStructures.g.cs", name, "Vector3D").ToList();
            Assert.That(methods, Is.Not.Empty, name);
            foreach (var m in methods)
                AssertStraightLine(m.ToString(), name);
        }
        Assert.That(Method(on, "NumericStructures.g.cs", "MapComponents", "Vector3D"),
            Does.Contain("new Ara3D.Geometry.Vector3D(f.Invoke(self.X), f.Invoke(self.Y), f.Invoke(self.Z))"));
        Assert.That(Method(on, "NumericStructures.g.cs", "ReduceComponents", "Vector3D"),
            Does.Contain("f.Invoke(f.Invoke(f.Invoke(initial, self.X), self.Y), self.Z)"));
    }

    [Test]
    public static void OnMakesTheSpecializedCloneStraightLine()
    {
        var on = Get(true);
        // SumSqrComponents: acc + x * x, once per component.
        var sumSqr = CloneBody(on, "Vector3D", "(self.X.Value * self.X.Value)");
        AssertStraightLine(sumSqr, "SumSqrComponents clone");
        Assert.That(sumSqr, Does.Contain("(self.Y.Value * self.Y.Value)").And.Contain("(self.Z.Value * self.Z.Value)"));
        // MinComponent and MaxComponent read the accumulator twice a step; three steps stay within the budget.
        foreach (var step in new[] { "<=" })
        {
            var clones = Clones(on, "Vector3D").Where(c => c.Contains(step) && !c.Contains("*")).ToList();
            Assert.That(clones, Has.Count.EqualTo(2), "the Min and Max clones");
            foreach (var c in clones)
            {
                AssertStraightLine(c, "Min or Max clone");
                Assert.That(c, Does.Contain("self.Z.Value"));
            }
        }
    }

    [Test]
    public static void OnBuildsTheVectorFromItsComponents()
    {
        var on = Get(true);
        Assert.That(Probe(on, "Rebuilt"), Does.Contain("new Ara3D.Geometry.Vector3D(v.X, v.Y, v.Z)"));
        foreach (var name in new[] { "Rebuilt", "Summed", "Blended", "Scaled", "AllPositive", "AnyNegative", "AllLess" })
            AssertStraightLine(Probe(on, name), name);
        // Summed and Blended call the library's ZipComponents, whose body is the straight-line code.
        var zips = Methods(on, "NumericStructures.g.cs", "ZipComponents", "Vector3D").Select(m => m.ToString()).ToList();
        Assert.That(zips, Has.Count.EqualTo(2));
        Assert.That(zips[0], Does.Contain("f.Invoke(a.X, b.X)").And.Contain("f.Invoke(a.Z, b.Z)"));
        Assert.That(zips[1], Does.Contain("f.Invoke(a.X, b.X, c.X)").And.Contain("f.Invoke(a.Z, b.Z, c.Z)"));
    }

    [Test]
    public static void OnKeepsTheCallOrderOfADelegateAcrossTheComponents()
    {
        var on = Get(true);
        // f is a parameter, not a lambda: its calls stay, X then Y then Z, nested for a fold. Mapped and Folded call
        // the library functions that make them, and the run in Plato.Interpreter.Tests observes the order.
        Assert.That(Method(on, "NumericStructures.g.cs", "MapComponents", "Vector3D"),
            Does.Match(@"f\.Invoke\(self\.X\).*f\.Invoke\(self\.Y\).*f\.Invoke\(self\.Z\)"));
        Assert.That(Method(on, "NumericStructures.g.cs", "ReduceComponents", "Vector3D"),
            Does.Contain("f.Invoke(f.Invoke(f.Invoke(initial, self.X), self.Y), self.Z)"));
    }

    private static IEnumerable<string> Clones(Program p, string receiver)
        => CSharpSyntaxTree.ParseText(p.Writer.Files[CallbackSpecializations.FileName].ToString()).GetRoot().DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .Where(m => m.Identifier.Text.StartsWith("ReduceComponents_") && m.ParameterList.Parameters[0].Type!.ToString() == receiver)
            .Select(m => m.ToString());

    private static string CloneBody(Program p, string receiver, string marker)
        => Clones(p, receiver).Single(c => c.Contains(marker));
}
