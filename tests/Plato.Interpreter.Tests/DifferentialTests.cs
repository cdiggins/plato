// Compares interpreter execution with generated C# results.
using System.Text;
using Ara3D.Geometry.Compiler.Types;
using Ara3D.Geometry.Laws;

namespace Plato.Interpreter.Tests;

/// <summary>
/// TKT-317, criterion 3: the interpreter and the C# backend agree. The stdlib's laws run both
/// ways on the same witnesses, under each Number profile, and every case must say the same thing
/// in both arms. A known, explained divergence is listed in <see cref="Allowed"/> with its reason;
/// an entry whose law no longer diverges fails the test so the list cannot go stale.
/// </summary>
[TestFixture]
public class DifferentialTests
{
    /// <summary>Law key (C# declaring type and method) to the reason its arms may disagree.</summary>
    private static readonly IReadOnlyDictionary<string, string> Allowed = new Dictionary<string, string>(StringComparer.Ordinal)
    {
    };

    [TestCase("cpu64")]
    [TestCase("gpu32")]
    public void InterpreterAndCSharpAgree(string profileName)
    {
        var profile = NumberProfile.TryParse(profileName);
        var program = DifferentialProgram.Instance;
        var run = OnLargeStack(() => DifferentialRun.Execute(program.Compilation, profile));
        TestContext.Out.Write(Report(program, run));

        var unexplained = run.Mismatches.Where(m => !Allowed.ContainsKey(m.Law.Key)).ToList();
        var stale = Allowed.Keys.Where(k => run.Compared.Any(l => l.Key == k) && run.Mismatches.All(m => m.Law.Key != k)).ToList();
        Assert.Multiple(() =>
        {
            Assert.That(unexplained, Is.Empty, $"{unexplained.Count} unexplained mismatch(es) under {profile}; the table above lists them");
            Assert.That(stale, Is.Empty, "allowed divergences that no longer occur: " + string.Join(", ", stale));
        });
    }

    private static T OnLargeStack<T>(Func<T> f)
    {
        T result = default;
        Exception failure = null;
        var thread = new Thread(() =>
        {
            try { result = f(); }
            catch (Exception e) { failure = e; }
        }, 512 * 1024 * 1024);
        thread.Start();
        thread.Join();
        if (failure != null)
            throw new InvalidOperationException("the differential run failed: " + failure.Message, failure);
        return result;
    }

    private static string Report(DifferentialProgram program, DifferentialRun run)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"== differential run, profile {run.Profile} ==");
        sb.AppendLine($"program: {program.LawFiles.Count} laws files over foundation, compiled in {program.Elapsed.TotalSeconds:F1}s");
        foreach (var note in program.Notes)
            sb.AppendLine("  " + note);
        sb.AppendLine($"C# arm: {run.Backend.FileCount} files emitted in {run.Backend.EmitElapsed.TotalSeconds:F1}s, compiled in {run.Backend.CompileElapsed.TotalSeconds:F1}s"
                      + (run.Backend.DegradedBodies.Count > 0 ? $", {run.Backend.DegradedBodies.Count} degraded bodies" : ""));

        var compared = run.Compared.ToList();
        var cases = compared.Sum(l => l.Cases.Count);
        var mismatches = run.Mismatches.ToList();
        sb.AppendLine($"laws found: {run.Laws.Count} (+{run.GenericSkipped} generic, not run); compared: {compared.Count}; skipped: {run.Laws.Count - compared.Count}");
        sb.AppendLine($"cases: {cases}; matches: {cases - mismatches.Count}; mismatches: {mismatches.Count}"
                      + $"; interpreter arm {compared.Sum(l => l.InterpreterTime.TotalSeconds):F2}s, C# arm {compared.Sum(l => l.CSharpTime.TotalSeconds):F3}s, whole run {run.Elapsed.TotalSeconds:F1}s");

        sb.AppendLine();
        sb.AppendLine($"{"law",-60} {"cases",5} {"ok",5} {"bad",5} {"interp",8} {"c#",8}");
        foreach (var l in compared)
            sb.AppendLine($"{l.Key,-60} {l.Cases.Count,5} {l.Cases.Count(c => c.Match),5} {l.Mismatches.Count(),5} {l.InterpreterTime.TotalMilliseconds,7:F0}ms {l.CSharpTime.TotalMilliseconds,7:F1}ms");

        if (mismatches.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("mismatches:");
            foreach (var (law, c) in mismatches)
            {
                var allowed = Allowed.TryGetValue(law.Key, out var reason) ? $"  [allowed: {reason}]" : "";
                sb.AppendLine($"  {law.Key} case {c.Index}: interpreter {c.Interpreter} | C# {c.CSharp}{allowed}");
                sb.AppendLine($"    witness: {c.Witness}");
            }
        }

        var skipped = run.Laws.Where(l => l.Skipped != null).ToList();
        if (skipped.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("skipped:");
            foreach (var l in skipped)
                sb.AppendLine($"  {l.Key}: {l.Skipped}");
        }
        return sb.ToString();
    }
}
