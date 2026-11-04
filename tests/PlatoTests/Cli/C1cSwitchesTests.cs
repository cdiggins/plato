// Pins the lane C1-c rows (TKT-1186): scalar-arrays and lower-more reach the Cpu target, filter-lowering is Cpu only; all off for Cpu.
using Ara3D.Geometry.CLI;

namespace PlatoTests.Cli;

public class C1cSwitchesTests
{
    [Test]
    public void TheThreeSwitchesApplyToCppAndAreOffThereByDefault()
    {
        var off = WriterSwitches.Resolve(new[] { "--cpp" }, WriterTarget.Cpu);
        Assert.That(off["scalar-arrays"] || off["lower-more"] || off["filter-lowering"], Is.False);
        var on = WriterSwitches.Resolve(new[] { "--loops", "--scalar-arrays", "--lower-more", "--filter-lowering" }, WriterTarget.Cpu);
        Assert.That(on["scalar-arrays"] && on["lower-more"] && on["filter-lowering"]);
        Assert.That(WriterSwitches.Check(on, WriterTarget.Cpu), Is.Null);
    }

    [Test]
    public void FilterLoweringNeedsLowerMore()
    {
        var args = WriterSwitches.Resolve(new[] { "--filter-lowering" }, WriterTarget.Cpu);
        Assert.That(WriterSwitches.Check(args, WriterTarget.Cpu), Is.EqualTo("--filter-lowering requires --lower-more."));
    }

    [Test]
    public void CSharpKeepsItsDefaults()
    {
        var cs = WriterSwitches.Resolve(System.Array.Empty<string>(), WriterTarget.CSharp);
        Assert.That(cs["lower-more"]);
        Assert.That(cs["scalar-arrays"], Is.False);
    }
}
