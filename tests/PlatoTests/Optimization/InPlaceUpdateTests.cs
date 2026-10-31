// Checks which SetAt calls the --in-place pass (TKT-1132) writes in place, clause by clause of its rule.
using Ara3D.Geometry.Compiler.Checking;
using Ara3D.Geometry.CSharpWriter;
using NUnit.Framework;

namespace PlatoTests.Optimization;

/// <summary>
/// Each function of <c>InPlaceFixtures/in-place.plato</c> is one case of the rule in
/// <c>writers/Plato.CSharpWriter/Optimization/README.md</c>. The tests read the tree the pass returns
/// and the C# the writer prints for it: a call is in place (a <see cref="TirInPlaceSet"/>, printed as
/// <c>Intrinsics.SetAtInPlace</c>) where the rule holds and stays a copy (<c>.SetAt(</c>) where it does
/// not. That the compiled result equals the interpreted one is checked on the same fixture by
/// Plato.Interpreter.Tests.InPlaceProgramTests.
/// </summary>
[TestFixture]
public static class InPlaceUpdateTests
{
    private static readonly Lazy<Ara3D.Geometry.Compiler.Compilation> Compilation = new(() =>
    {
        var foundation = Path.Combine(CheckerTestSupport.FindForwardStdLib(), "foundation");
        var fixture = Path.Combine(CheckerTestSupport.FindFolder("PlatoTests"), "Optimization", "InPlaceFixtures", "in-place.plato");
        var asts = Directory.GetFiles(foundation, "*.plato").Select(CheckerTestSupport.ParseFile)
            .Append(CheckerTestSupport.ParseFile(fixture));
        return new Ara3D.Geometry.Compiler.Compilation(Ara3D.Logging.Logger.Null, asts.ToList());
    });

    private static (CSharpWriter Writer, TirFunction Tir) Lower(string name, bool inPlace, bool loops = true)
    {
        // The lowerer numbers its temporaries from a counter shared by every emission.
        TirLoopLowerer.NextId = 0;
        var writer = new CSharpWriter(Compilation.Value, "unused-in-place")
            { ExtensionStyle = true, LowerLoops = loops, FuseCombinators = true, InPlaceUpdates = inPlace };
        var definition = Compilation.Value.LibraryDefinitionsByName["InPlaceProbe"].Functions.Single(f => f.Name == name);
        return (writer, writer.RunOptimizerPasses(writer.TryGetStaticTir(definition), null));
    }

    /// <summary>The C# of the body. Not for a body that reads a record field: the writer has not planned
    /// the extension names a field access needs until it has written the whole library.</summary>
    private static string Print(string name, bool inPlace, bool loops = true)
    {
        var (writer, tir) = Lower(name, inPlace, loops);
        return new TirCSharpBodyWriter(new CSharpTypeWriter(writer, null), tir, isStatic: true).ToString();
    }

    private static int InPlaceSites(string name, bool loops = true)
        => Lower(name, true, loops).Tir.Body.Descendants().OfType<TirInPlaceSet>().Count();

    private static int CopySites(string name, bool loops = true)
        => Lower(name, true, loops).Tir.Body.Descendants().OfType<TirCall>().Count(c => c.Name == "SetAt");

    private static int Count(string text, string word)
        => System.Text.RegularExpressions.Regex.Matches(text, System.Text.RegularExpressions.Regex.Escape(word)).Count;

    // The rule holds: the update is written into the array.

    [TestCase("Threaded", 1, Description = "xs = xs.SetAt(...) in a loop: the variable holds the next version")]
    [TestCase("Chained", 2, Description = "the receiver of each update is a new array nothing names")]
    [TestCase("HandOff", 2, Description = "each array is dead once the next is made from it")]
    [TestCase("Alternating", 2, Description = "two arrays threaded through one loop, each read by the other")]
    [TestCase("PerIteration", 1, Description = "declared in the loop body, so a new array each pass")]
    [TestCase("Law_OutOfRange", 2, Description = "a literal receiver, an index outside the array")]
    public static void KeptInPlace(string name, int sites)
    {
        Assert.That(InPlaceSites(name), Is.EqualTo(sites));
        Assert.That(CopySites(name), Is.Zero);
        Assert.That(Count(Print(name, true), "Intrinsics.SetAtInPlace"), Is.EqualTo(sites));
    }

    // One test per clause that makes the old value observable: the update stays a copy.

    [TestCase("LaterUse", Description = "the old binding is read after the update")]
    [TestCase("Aliased", Description = "another let holds the same array")]
    [TestCase("Captured", Description = "a closure that is passed on reads the array")]
    [TestCase("Escaped", Description = "the array was handed to a literal before the update")]
    [TestCase("NotThreaded", Description = "a loop updates the same array and keeps each result")]
    [TestCase("OfParameter", Description = "a parameter belongs to the caller")]
    [TestCase("OfCall", Description = "a call may return its argument")]
    [TestCase("OfField", Description = "an array held by a record belongs to its holder")]
    public static void KeptACopy(string name)
    {
        Assert.That(InPlaceSites(name), Is.Zero);
        Assert.That(CopySites(name), Is.EqualTo(1));
    }

    [Test]
    public static void FlagOffEmitsNoMarkerAndEveryUpdateIsACopy()
    {
        foreach (var name in new[] { "Threaded", "Chained", "HandOff", "Alternating", "PerIteration", "LaterUse", "OfParameter" })
        {
            var (_, tir) = Lower(name, false);
            Assert.That(tir.Body.Descendants().OfType<TirInPlaceSet>(), Is.Empty, name);
            var body = Print(name, false);
            Assert.That(body, Does.Not.Contain("SetAtInPlace"), name);
            Assert.That(body, Does.Contain(".SetAt("), name);
        }
    }

    [Test]
    public static void FlagOnLeavesTheRejectedBodiesUnchanged()
    {
        foreach (var name in new[] { "LaterUse", "Aliased", "Captured", "Escaped", "NotThreaded", "OfParameter", "OfCall" })
            Assert.That(Print(name, true), Is.EqualTo(Print(name, false)), name);
    }

    [Test]
    public static void FlagOnChangesOnlyTheCallInAThreadedBody()
    {
        var off = Print("Threaded", false);
        var on = Print("Threaded", true);
        Assert.That(on.Replace("Intrinsics.SetAtInPlace<Integer>(xs, k, k.Multiply(k))", "xs.SetAt(k, k.Multiply(k))"), Is.EqualTo(off));
    }

    [Test]
    public static void WithoutLoopsTheChainIsStillInPlace()
    {
        // Loop lowering is not what the proof rests on: a chain of updates on a new array is.
        Assert.That(InPlaceSites("Chained", loops: false), Is.EqualTo(2));
    }

    [Test]
    public static void OptionRecordCarriesTheSwitch()
    {
        Assert.That(new LoopOptimizationOptions().InPlace, Is.False);
        Assert.That(new LoopOptimizationOptions(InPlace: true).InPlace, Is.True);
    }
}
