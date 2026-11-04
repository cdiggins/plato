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
        Assert.That(WriterSwitches.Table.Where(s => !s.Default).Select(s => s.Name), Is.EquivalentTo(new[] { "specialize-callbacks-lean", "scalar-arrays", "owned-arrays", "stack-arrays", "span-params" }));
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
    public void CommandsRejectAnUnknownNegationBeforeReadingSources()
    {
        Assert.That(Program.Main(new[] { "no-such-folder", "--no-bogus" }), Is.EqualTo(1));
        Assert.That(Program.Main(new[] { "run", "no-such-folder", "--laws", "--no-bogus" }), Is.EqualTo(1));
        Assert.That(Program.Main(new[] { "no-such-folder", "--no-inline" }), Is.EqualTo(1));
    }
}
