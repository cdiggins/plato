// Runs the --share-calls fixture compiled and interpreted, and checks the two give the same verdicts and values.
using System.Reflection;
using Ara3D.Geometry.Compiler.Types;
using Ara3D.Geometry.CSharpWriter;
using Ara3D.Geometry.Interpreter;
using Ara3D.Geometry.Laws;

namespace Plato.Interpreter.Tests;

/// <summary>
/// TKT-1136: the fixture <c>tests/PlatoTests/Optimization/SharedCallsFixtures/shared-calls.plato</c> states, as
/// laws, the value of each expression read plainly. The program is compiled with the flag off and on; every law
/// must pass in all three runs, the verdicts must be equal, and the compiled code of the shared cases must call
/// <c>Cost</c> and <c>Row</c> fewer times with the flag on.
/// </summary>
[TestFixture, Category("Slow")]
public class SharedCallsProgramTests
{
    private sealed record Run(IReadOnlyList<LawResult> Results, CSharpProgram Program);

    private static readonly Lazy<(IReadOnlyList<LawResult> Interpreted, Run Off, Run On)> Runs = new(() =>
    {
        var path = Path.Combine(TestSupport.RepoRoot, "tests", "PlatoTests", "Optimization", "SharedCallsFixtures", "shared-calls.plato");
        var compilation = LawRunnerSelectionTests.CompileSource(File.ReadAllText(path));
        var options = new LawRunOptions { Filters = new[] { "SharedProbe" } };
        var interp = new Ara3D.Geometry.Interpreter.Interpreter(compilation, NumberProfile.Cpu64);
        var interpreted = new LawRunner(interp, options).RunAll();

        Run Compiled(bool share)
        {
            var program = CSharpProgram.Build(compilation, NumberProfile.Cpu64, lowerLoops: true, fuse: true,
                loopOptimizations: new LoopOptimizationOptions(ShareCalls: share));
            var results = new LawRunner(CSharpBackend.Over(program, interp), interp, options).RunAll();
            return new Run(results, program);
        }

        return (interpreted, Compiled(false), Compiled(true));
    });

    private static string Verdicts(IEnumerable<LawResult> results)
        => string.Join("\n", results.OrderBy(r => r.Name, StringComparer.Ordinal)
            .Select(r => $"{r.Name} {r.Status.ToString().ToLowerInvariant()} {r.Trials}"));

    [Test]
    public void Every_law_passes_in_the_interpreter_and_compiled_with_the_flag_off_and_on()
    {
        var (interpreted, off, on) = Runs.Value;
        Assert.That(interpreted, Has.Count.EqualTo(11));
        Assert.That(interpreted.Concat(off.Results).Concat(on.Results).Where(r => r.Status != LawStatus.Pass).Select(r => r.Name), Is.Empty);
    }

    [Test]
    public void Verdicts_are_equal_in_the_three_runs()
    {
        var (interpreted, off, on) = Runs.Value;
        Assert.That(Verdicts(off.Results), Is.EqualTo(Verdicts(interpreted)));
        Assert.That(Verdicts(on.Results), Is.EqualTo(Verdicts(interpreted)));
    }

    /// <summary>How many times a compiled method calls a method of the given name, read from its IL (call and callvirt tokens).</summary>
    private static int CallsTo(CSharpProgram program, string method, string callee)
    {
        var probe = program.Assembly.GetTypes().Single(t => t.Name == "SharedProbe");
        var m = probe.GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(x => x.Name == method && x.DeclaringType == probe);
        var il = m.GetMethodBody()!.GetILAsByteArray()!;
        var n = 0;
        for (var i = 0; i + 4 < il.Length; i++)
        {
            if (il[i] != 0x28 && il[i] != 0x6F)
                continue;
            var token = BitConverter.ToInt32(il, i + 1);
            if ((token >> 24) is not (0x06 or 0x0A or 0x2B))
                continue;
            try { if (m.Module.ResolveMethod(token)!.Name == callee) n++; }
            catch (Exception e) when (e is ArgumentException or NotSupportedException) { }
        }
        return n;
    }

    [TestCase("MatchOnCall", "Row", 4, 1)]
    [TestCase("Twice", "Cost", 2, 1)]
    [TestCase("InConditionAndBranch", "Cost", 3, 1)]
    [TestCase("Nested", "Cost", 4, 2)]
    [TestCase("NestedAndInner", "Cost", 5, 2)]
    [TestCase("TwoKinds", "Cost", 4, 2)]
    [TestCase("OnlyInBranches", "Cost", 2, 2)]
    [TestCase("Different", "Cost", 2, 2)]
    [TestCase("ArrayResult", "Make", 2, 2)]
    public void The_compiled_call_counts_are_as_the_rule_says(string method, string callee, int off, int on)
    {
        var (_, offRun, onRun) = Runs.Value;
        Assert.That(CallsTo(offRun.Program, method, callee), Is.EqualTo(off), "flag off");
        Assert.That(CallsTo(onRun.Program, method, callee), Is.EqualTo(on), "flag on");
    }
}
