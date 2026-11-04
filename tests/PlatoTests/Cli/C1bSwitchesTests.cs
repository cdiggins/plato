// Pins the lane C1-b rows (TKT-1183): presize and cache-constants reach the Cpu target, off by default there, on for C#.
using Ara3D.Geometry.CLI;

namespace PlatoTests.Cli;

public class C1bSwitchesTests
{
    [Test]
    public void PresizeAndCacheConstantsApplyToCppAndAreOffThereByDefault()
    {
        var off = WriterSwitches.Resolve(new[] { "--cpp" }, WriterTarget.Cpu);
        Assert.That(off["presize"] || off["cache-constants"], Is.False);
        var on = WriterSwitches.Resolve(new[] { "--presize", "--cache-constants" }, WriterTarget.Cpu);
        Assert.That(on["presize"] && on["cache-constants"]);
        Assert.That(WriterSwitches.Check(on, WriterTarget.Cpu), Is.Null);
    }

    [Test]
    public void CSharpKeepsBothOnByDefault()
    {
        var cs = WriterSwitches.Resolve(System.Array.Empty<string>(), WriterTarget.CSharp);
        Assert.That(cs["presize"] && cs["cache-constants"]);
    }
}
