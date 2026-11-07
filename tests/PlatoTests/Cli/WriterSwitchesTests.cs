// Pins how the CLI chooses the C# writer switches: defaults, --no-x, the umbrella, dependencies and targets.
using Ara3D.Geometry.CLI;

namespace PlatoTests.Cli;

public class WriterSwitchesTests
{
    private static readonly string[] Pinned =
    {
        "loops", "fuse", "fuse-consumers", "exact-output", "lean-inlining", "share-calls", "lower-scalars",
        "concrete-arrays", "presize", "in-place", "sink-bindings", "bind-scrutinee", "share-calls-wide",
        "closure-free", "in-params", "switch-match", "hoist-invariants", "cache-constants", "primitive-compare",
        "lower-more", "specialize-callbacks",
    };

    private static string[] On(ResolvedWriterSwitches r) => Pinned.Where(n => r[n]).ToArray();

    [Test]
    public void TableHoldsTheTwentyOneRegenerationSwitchesDefaultedOnAndTheUnpinnedOff()
    {
        // --specialize-callbacks-lean (not yet measured as a set) and TKT-1152's switches stay off by default until plato-brep measures and pins them.
        Assert.That(WriterSwitches.Table.Where(s => s.Default).Select(s => s.Name), Is.EquivalentTo(Pinned));
        Assert.That(WriterSwitches.Table.Where(s => !s.Default && s.Targets.HasFlag(WriterTarget.CSharp)).Select(s => s.Name), Is.EquivalentTo(new[] { "specialize-callbacks-lean", "scalar-arrays", "owned-arrays", "stack-arrays", "span-params" }));
        var names = WriterSwitches.Table.Select(s => s.Name).ToList();
        foreach (var s in WriterSwitches.Table.Where(s => s.Requires != null))
            Assert.That(names.IndexOf(s.Requires!), Is.LessThan(names.IndexOf(s.Name)), s.Name + " must follow " + s.Requires);
    }

    [Test]
    public void CSharpTargetDefaultsEverySwitchOn()
        => Assert.That(On(WriterSwitches.Resolve(Array.Empty<string>(), true)), Is.EquivalentTo(Pinned));

    [Test]
    public void OtherTargetsStartWithEverySwitchOff()
        => Assert.That(On(WriterSwitches.Resolve(new[] { "--typescript" }, false)), Is.Empty);

    [Test]
    public void ExplicitSwitchOnOtherTargetsStaysOn()
        => Assert.That(On(WriterSwitches.Resolve(new[] { "--cpp", "--loops", "--fuse" }, false)), Is.EquivalentTo(new[] { "loops", "fuse" }));

    [Test]
    public void NoSpellingTurnsOneSwitchOff()
    {
        var on = On(WriterSwitches.Resolve(new[] { "--no-presize" }, true));
        Assert.That(on, Is.EquivalentTo(Pinned.Where(n => n != "presize")));
    }

    [Test]
    public void LastSpellingWins()
    {
        Assert.That(WriterSwitches.Resolve(new[] { "--presize", "--no-presize" }, true)["presize"], Is.False);
        Assert.That(WriterSwitches.Resolve(new[] { "--no-presize", "--presize" }, true)["presize"], Is.True);
        Assert.That(WriterSwitches.Resolve(new[] { "--no-presize", "--presize" }, false)["presize"], Is.True);
    }

    [Test]
    public void UmbrellaTurnsEveryDefaultOffButKeepsExplicitSwitches()
    {
        Assert.That(On(WriterSwitches.Resolve(new[] { WriterSwitches.Umbrella }, true)), Is.Empty);
        var r = WriterSwitches.Resolve(new[] { WriterSwitches.Umbrella, "--loops", "--fuse" }, true);
        Assert.That(On(r), Is.EquivalentTo(new[] { "loops", "fuse" }));
        var after = WriterSwitches.Resolve(new[] { "--loops", WriterSwitches.Umbrella }, true);
        Assert.That(On(after), Is.EquivalentTo(new[] { "loops" }));
    }

    [Test]
    public void DefaultedDependentsFollowTheirDependency()
    {
        var on = On(WriterSwitches.Resolve(new[] { "--no-loops" }, true));
        Assert.That(on, Is.EquivalentTo(Pinned.Except(new[] { "loops", "fuse-consumers", "exact-output", "hoist-invariants", "lower-more" })));
    }

    [Test]
    public void ExplicitDependentStaysOnWithoutItsDependencySoTheCommandCanReportIt()
    {
        var r = WriterSwitches.Resolve(new[] { "--no-loops", "--lower-more" }, true);
        Assert.That(r["loops"], Is.False);
        Assert.That(r["lower-more"], Is.True);
    }

    [Test]
    public void UnknownNegationIsFoundAndKnownSpellingsAreNot()
    {
        Assert.That(WriterSwitches.UnknownNegation(new[] { "--no-presize", "--no-writer-defaults", "--no-bogus", "--no-also" }), Is.EqualTo("--no-bogus"));
        Assert.That(WriterSwitches.UnknownNegation(new[] { "--no-presize", "--presize", "--no-writer-defaults" }), Is.Null);
        Assert.That(WriterSwitches.IsSpelling("--presize") && WriterSwitches.IsSpelling("--no-presize") && !WriterSwitches.IsSpelling("--no-inline"));
    }

    [Test]
    public void ResolvedSwitchesMapToLoopOptions()
    {
        var all = WriterSwitches.ToLoopOptions(WriterSwitches.Resolve(Array.Empty<string>(), true));
        Assert.That(all.LowerMore && all.SpecializeCallbacks && all.CacheConstants && all.Presize && all.FuseConsumers);
        var none = WriterSwitches.ToLoopOptions(WriterSwitches.Resolve(new[] { WriterSwitches.Umbrella }, true));
        Assert.That(none.LowerMore || none.SpecializeCallbacks || none.CacheConstants || none.Presize || none.FuseConsumers, Is.False);
    }

    [Test]
    public void CppTargetDefaultsTheTimingWindowSetAndAcceptsItsRows()
    {
        var w2 = new[] { "--cpp", "--loops", "--fuse", "--fuse-consumers", "--exact-output", "--cpp-borrow-params", "--cpp-borrow-reads", "--cpp-borrow-nontrivial", "--cpp-borrow-min-bytes=32", "--cpp-roots=A,B" };
        // Timing windows 1 and 2 (2026-11-04): W2, both sum flags, static strings, the C1-a passes and scalar-arrays are on for Cpu, scalar regions since 2026-11-06 (TKT-1211), and
        // loops with them, so hoist-invariants is not silently off; --no-loops turns it off, and --no-writer-defaults still gives the empty set.
        var cpu = WriterSwitches.Resolve(new[] { "--cpp" }, WriterTarget.Cpu);
        Assert.That(cpu.Switches.Names, Is.EquivalentTo(new[] { "loops", "hoist-invariants", "share-calls", "sink-bindings", "bind-scrutinee", "share-calls-wide",
            "cpp-borrow-params", "cpp-borrow-reads", "cpp-borrow-nontrivial", "cpp-sum-switch", "cpp-sum-compact", "cpp-static-strings", "cpp-string-pool", "cpp-borrow-min-bytes=32", "scalar-arrays", "cpp-scalar-regions" }));
        Assert.That(cpu.Switches.Int("cpp-borrow-min-bytes", 256), Is.EqualTo(32));
        Assert.That(cpu["hoist-invariants"]);
        Assert.That(WriterSwitches.Resolve(new[] { "--cpp", "--no-loops" }, WriterTarget.Cpu)["hoist-invariants"], Is.False);
        var none = WriterSwitches.Resolve(new[] { "--cpp", WriterSwitches.Umbrella }, WriterTarget.Cpu);
        Assert.That(none.Switches.Names, Is.Empty);
        Assert.That(none.Switches.Int("cpp-borrow-min-bytes", 256), Is.EqualTo(256));
        Assert.That(WriterSwitches.Resolve(new[] { "--cpp", "--no-cpp-borrow-min-bytes" }, WriterTarget.Cpu).Switches.Int("cpp-borrow-min-bytes", 256), Is.EqualTo(256));
        Assert.That(WriterSwitches.Check(cpu, WriterTarget.Cpu), Is.Null);
        var r = WriterSwitches.Resolve(w2, WriterTarget.Cpu);
        Assert.That(WriterSwitches.Check(r, WriterTarget.Cpu), Is.Null);
        Assert.That(r.Switches.Int("cpp-borrow-min-bytes", 256), Is.EqualTo(32));
        Assert.That(r.Value("cpp-roots"), Is.EqualTo("A,B"));
        Assert.That(r["cpp-borrow-reads"] && r["loops"] && !r["presize"]);
        Assert.That(w2.All(a => a == "--cpp" || WriterSwitches.IsSpelling(a)));
    }

    [Test]
    public void CheckRejectsASwitchOutsideItsTargetsAMissingDependencyAndABadValue()
    {
        Assert.That(WriterSwitches.Check(WriterSwitches.Resolve(new[] { "--presize" }, WriterTarget.Cpu), WriterTarget.Cpu), Does.StartWith("--presize applies to C#"));
        Assert.That(WriterSwitches.Check(WriterSwitches.Resolve(new[] { "--cpp-borrow-reads" }, WriterTarget.CSharp), WriterTarget.CSharp), Does.StartWith("--cpp-borrow-reads applies to C++"));
        Assert.That(WriterSwitches.Check(WriterSwitches.Resolve(new[] { "--no-loops", "--lower-more" }, WriterTarget.CSharp), WriterTarget.CSharp), Is.EqualTo("--lower-more requires --loops."));
        Assert.That(WriterSwitches.Check(WriterSwitches.Resolve(new[] { "--cpp-borrow-min-bytes=0" }, WriterTarget.Cpu), WriterTarget.Cpu), Is.EqualTo("--cpp-borrow-min-bytes requires a positive integer."));
        Assert.That(WriterSwitches.Check(WriterSwitches.Resolve(Array.Empty<string>(), WriterTarget.CSharp), WriterTarget.CSharp), Is.Null);
        Assert.That(WriterSwitches.Check(WriterSwitches.Resolve(new[] { "--loops" }, WriterTarget.Other), WriterTarget.Other), Does.StartWith("--loops applies to"));
    }

    [Test]
    public void ParameterisedSwitchesNeedAValueAndTheLastSpellingWins()
    {
        Assert.That(WriterSwitches.IsSpelling("--cpp-borrow-min-bytes=8") && !WriterSwitches.IsSpelling("--cpp-borrow-min-bytes") && !WriterSwitches.IsSpelling("--presize=1"));
        var r = WriterSwitches.Resolve(new[] { "--cpp-borrow-min-bytes=8", "--cpp-borrow-min-bytes=64" }, WriterTarget.Cpu);
        Assert.That(r.Switches.Int("cpp-borrow-min-bytes", 256), Is.EqualTo(64));
        var dropped = WriterSwitches.Resolve(new[] { "--cpp-borrow-min-bytes=8", "--no-cpp-borrow-min-bytes" }, WriterTarget.Cpu);
        Assert.That(dropped["cpp-borrow-min-bytes"], Is.False);
        Assert.That(dropped.Switches.Int("cpp-borrow-min-bytes", 256), Is.EqualTo(256));
    }

    [Test]
    public void UnitLinesIsACppParameterWithTheWritersDefault()
    {
        var r = WriterSwitches.Resolve(new[] { "--cpp-unit-lines=1234" }, WriterTarget.Cpu);
        Assert.That(r.Switches.Int("cpp-unit-lines", 5000), Is.EqualTo(1234));
        Assert.That(WriterSwitches.Check(r, WriterTarget.Cpu), Is.Null);
        Assert.That(WriterSwitches.Check(WriterSwitches.Resolve(new[] { "--cpp-unit-lines=0" }, WriterTarget.Cpu), WriterTarget.Cpu), Is.EqualTo("--cpp-unit-lines requires a positive integer."));
        Assert.That(WriterSwitches.Check(WriterSwitches.Resolve(new[] { "--cpp-unit-lines=9" }, WriterTarget.CSharp), WriterTarget.CSharp), Does.StartWith("--cpp-unit-lines applies to C++"));
    }

    [Test]
    public void PruneToRootsIsACppParameterThatIsOffUnlessSpelled()
    {
        Assert.That(WriterSwitches.Resolve(new[] { "--cpp" }, WriterTarget.Cpu).Value("cpp-prune-to-roots"), Is.Null);
        var r = WriterSwitches.Resolve(new[] { "--cpp-prune-to-roots=A,B" }, WriterTarget.Cpu);
        Assert.That(r.Value("cpp-prune-to-roots"), Is.EqualTo("A,B"));
        Assert.That(WriterSwitches.Check(r, WriterTarget.Cpu), Is.Null);
        Assert.That(WriterSwitches.Check(WriterSwitches.Resolve(new[] { "--cpp-prune-to-roots=A" }, WriterTarget.CSharp), WriterTarget.CSharp), Does.StartWith("--cpp-prune-to-roots applies to C++"));
    }

    [Test]
    public void SumLayoutRowsAreCppOnlyAndOnByDefaultForCpu()
    {
        var off = WriterSwitches.Resolve(new[] { "--cpp", "--no-cpp-sum-switch", "--no-cpp-sum-compact", "--no-cpp-static-strings" }, WriterTarget.Cpu);
        Assert.That(WriterSwitches.Resolve(new[] { "--cpp" }, WriterTarget.Cpu) is var d && d["cpp-sum-switch"] && d["cpp-sum-compact"] && d["cpp-static-strings"]);
        Assert.That(off["cpp-sum-switch"] || off["cpp-sum-compact"], Is.False);
        var on = WriterSwitches.Resolve(new[] { "--cpp-sum-switch", "--cpp-sum-compact", "--cpp-static-strings" }, WriterTarget.Cpu);
        Assert.That(on["cpp-sum-switch"] && on["cpp-sum-compact"] && on["cpp-static-strings"] && !off["cpp-static-strings"]);
        Assert.That(WriterSwitches.Check(on, WriterTarget.Cpu), Is.Null);
        Assert.That(WriterSwitches.Check(WriterSwitches.Resolve(new[] { "--cpp-sum-compact" }, WriterTarget.CSharp), WriterTarget.CSharp), Does.StartWith("--cpp-sum-compact applies to C++"));
    }

    [Test]
    public void EveryRowDefaultsOnlyWhereItApplies()
    {
        foreach (var s in WriterSwitches.Table)
        {
            Assert.That(s.Targets.HasFlag(s.Defaults), s.Name);
            Assert.That(s.Parameter == null || (s.Defaults == WriterTarget.None) == (s.DefaultValue == null), s.Name);
        }
    }

    [Test]
    public void CommandsRejectAnUnknownNegationBeforeReadingSources()
    {
        Assert.That(Program.Main(new[] { "no-such-folder", "--no-bogus" }), Is.EqualTo(1));
        Assert.That(Program.Main(new[] { "run", "no-such-folder", "--laws", "--no-bogus" }), Is.EqualTo(1));
        Assert.That(Program.Main(new[] { "no-such-folder", "--no-inline" }), Is.EqualTo(1));
    }
}
