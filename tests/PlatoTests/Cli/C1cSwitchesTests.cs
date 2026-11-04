// Pins the lane C1-c row (TKT-1186): scalar-arrays reaches the Cpu target and is on there since timing window 2.
// lower-more is C# only again and filter-lowering is gone (both rejected for Cpu in timing window 2, 0.996).
using Ara3D.Geometry.CLI;

namespace PlatoTests.Cli;

public class C1cSwitchesTests
{
    [Test]
    public void ScalarArraysIsOnForCpu()
    {
        var cpu = WriterSwitches.Resolve(new[] { "--cpp" }, WriterTarget.Cpu);
        Assert.That(cpu["scalar-arrays"], "on for Cpu since timing window 2");
        Assert.That(cpu["lower-more"], Is.False);
        Assert.That(WriterSwitches.Check(cpu, WriterTarget.Cpu), Is.Null);
    }

    [Test]
    public void LowerMoreIsCSharpOnlyAndFilterLoweringIsGone()
    {
        Assert.That(WriterSwitches.Check(WriterSwitches.Resolve(new[] { "--loops", "--lower-more" }, WriterTarget.Cpu), WriterTarget.Cpu),
            Does.StartWith("--lower-more applies to C#"));
        Assert.That(WriterSwitches.IsSwitch("filter-lowering"), Is.False);
    }

    [Test]
    public void CSharpKeepsItsDefaults()
    {
        var cs = WriterSwitches.Resolve(System.Array.Empty<string>(), WriterTarget.CSharp);
        Assert.That(cs["lower-more"]);
        Assert.That(cs["scalar-arrays"], Is.False);
    }
}
