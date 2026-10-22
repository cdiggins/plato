using Ara3D.Geometry.Compiler.Types;
using Ara3D.Geometry.Interpreter;
using Ara3D.Geometry.Laws;

namespace Plato.Interpreter.Tests;

/// <summary>
/// TKT-447: the stdlib's TriangleFace and QuadFace indexers returned the last corner for every
/// index past the end (and for -1), so an off-by-one in a corner walk read a wrong vertex with
/// no error. Each in-range law passes and each out-of-range law is an error, on both backends.
/// </summary>
[TestFixture]
public class FaceCornerRangeTests
{
    [Test]
    public void Corners_out_of_range_are_errors_on_both_backends()
    {
        var compilation = TestSupport.Compile(
            new[] { TestSupport.StdlibFolder("foundation"), TestSupport.StdlibFolder("geometry") },
            TestSupport.FixturePath("face-corners.plato"));
        var options = new LawRunOptions { Filters = new[] { "FaceCornerFixture" } };
        var interp = new Ara3D.Geometry.Interpreter.Interpreter(compilation, NumberProfile.Cpu64);
        var interpreted = new LawRunner(interp, options).RunAll();
        var compiled = new LawRunner(LawBackends.Create(LawBackendKind.CSharp, interp), interp, options).RunAll();

        foreach (var (backend, results) in new[] { ("interpreter", interpreted), ("csharp", compiled) })
        {
            string Verdicts() => string.Join("\n", results.OrderBy(r => r.Name, StringComparer.Ordinal)
                .Select(r => $"{backend} {r.Name} {r.Status} {r.Message}"));
            TestContext.Out.WriteLine(Verdicts());
            Assert.That(results, Has.Count.EqualTo(6), Verdicts());
            Assert.That(results.Where(r => r.Name.EndsWith("InRange")).Select(r => r.Status),
                Has.All.EqualTo(LawStatus.Pass), Verdicts());
            Assert.That(results.Where(r => r.Name.EndsWith("IsRefused")).Select(r => r.Status),
                Has.All.EqualTo(LawStatus.Error), Verdicts());
        }
    }
}
