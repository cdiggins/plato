using Ara3D.Geometry.Compiler.Types;
using Ara3D.Geometry.Interpreter;

namespace Plato.Interpreter.Tests;

/// <summary>Runs every Law_* of stdlib foundation, geometry, graphics and the law packet
/// stdlib/tests through the interpreter under cpu64. An Error is an interpreter defect and fails
/// the test; a Fail is a law the stdlib itself breaks and is listed in the output; obligations
/// and skips are counted and listed.</summary>
[TestFixture]
public class StdlibLawsTests
{
    /// <summary>Laws that fault in the stdlib itself, not in the interpreter: the generated
    /// input breaks a documented precondition of a function the law reaches. The C# build
    /// throws the same way. Each entry names the precondition.</summary>
    private static readonly Dictionary<string, string> KnownStdlibFaults = new()
    {
        ["TriangulationLaws.Law_HoledTriangulationFacesIndexInputPoints"] = "LeftmostOfRange requires a non-empty ring; a generated hole can be empty",
        ["TriangulationLaws.Law_HoledTriangulationPreservesPositions"] = "LeftmostOfRange requires a non-empty ring; a generated hole can be empty",
        ["TriangulationLaws.Law_HoledTriangulationFacesHaveDistinctCorners"] = "LeftmostOfRange requires a non-empty ring; a generated hole can be empty",
    };

    [Test]
    public void No_law_errors_under_cpu64()
    {
        var interp = new Ara3D.Geometry.Interpreter.Interpreter(TestSupport.FullStdlib, NumberProfile.Cpu64);
        var results = RunOnBigStack(() => new LawRunner(interp).RunAll());

        var byStatus = results.GroupBy(r => r.Status).ToDictionary(g => g.Key, g => g.ToList());
        foreach (var status in Enum.GetValues<LawStatus>())
            TestContext.Out.WriteLine($"{status,-10} {byStatus.GetValueOrDefault(status)?.Count ?? 0}");
        TestContext.Out.WriteLine($"{results.Count} laws, {interp.ElaboratedCount} bodies elaborated");
        TestContext.Out.WriteLine();

        foreach (var status in new[] { LawStatus.Error, LawStatus.Fail, LawStatus.Obligation, LawStatus.Skipped })
            foreach (var r in byStatus.GetValueOrDefault(status) ?? new List<LawResult>())
                TestContext.Out.WriteLine(r.ToString());

        var errors = byStatus.GetValueOrDefault(LawStatus.Error) ?? new List<LawResult>();
        var interpreterErrors = errors.Where(r => !KnownStdlibFaults.ContainsKey(r.Name)).Select(r => r.ToString()).ToList();
        foreach (var r in errors.Where(r => KnownStdlibFaults.ContainsKey(r.Name)))
            TestContext.Out.WriteLine($"known stdlib fault: {r.Name}: {KnownStdlibFaults[r.Name]}");

        Assert.That(results, Is.Not.Empty);
        Assert.That(interpreterErrors, Is.Empty, "laws that faulted in the interpreter");
    }

    /// <summary>Deep Plato recursion needs more stack than the test thread has.</summary>
    private static T RunOnBigStack<T>(Func<T> f)
    {
        T result = default;
        Exception error = null;
        var thread = new Thread(() =>
        {
            try { result = f(); }
            catch (Exception e) { error = e; }
        }, 256 * 1024 * 1024);
        thread.Start();
        thread.Join();
        if (error != null)
            throw new Exception("law run faulted", error);
        return result;
    }
}
