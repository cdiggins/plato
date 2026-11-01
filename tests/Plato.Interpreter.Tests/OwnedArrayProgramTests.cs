// Runs the --owned-arrays fixture interpreted and compiled, flag off and on, and checks the verdicts are equal.
using Ara3D.Geometry.Compiler.Types;
using Ara3D.Geometry.CSharpWriter;
using Ara3D.Geometry.Interpreter;
using Ara3D.Geometry.Laws;

namespace Plato.Interpreter.Tests;

/// <summary>
/// TKT-1152 C2b: the fixture <c>tests/PlatoTests/Emission/OwnedArrayFixtures/owned.plato</c> states, as laws, the
/// result the immutable meaning gives for each place a lowered-loop temporary can sit (read directly, seeding a
/// Reduce, sourcing a FlatMap, joined by a conditional or a match, passed to a generic function, returned, captured,
/// carried round a loop). Storage must never change a value: every ordinary law passes in the interpreter and in the
/// compiled program with the flag off and on, and the three verdicts are equal. The law that reads past the end of an
/// owned temporary faults in all three; the compiled arm throws <c>IndexOutOfRangeException</c> where the interface
/// throws <c>ArgumentOutOfRangeException</c>, and the runner does not tell them apart.
/// </summary>
[TestFixture, Category("Slow")]
public class OwnedArrayProgramTests
{
    private const string FaultingLaw = "OwnedProbe.Law_ReadPastTheEnd";

    private static readonly Lazy<(IReadOnlyList<LawResult> Interpreted, IReadOnlyList<LawResult> Off, IReadOnlyList<LawResult> On)> Runs = new(() =>
    {
        var path = Path.Combine(TestSupport.RepoRoot, "tests", "PlatoTests", "Emission", "OwnedArrayFixtures", "owned.plato");
        var compilation = LawRunnerSelectionTests.CompileSource(File.ReadAllText(path));
        var options = new LawRunOptions { Filters = new[] { "OwnedProbe" } };
        var interp = new Ara3D.Geometry.Interpreter.Interpreter(compilation, NumberProfile.Cpu64);
        var interpreted = new LawRunner(interp, options).RunAll();

        IReadOnlyList<LawResult> Compiled(bool owned)
        {
            // The nineteen pinned flags of the regeneration, plus the storage layer when asked.
            var flags = new LoopOptimizationOptions(FuseConsumers: true, ExactOutputAllocation: true, ConcreteArrays: true,
                LeanInlining: true, Presize: true, InPlace: true, ShareCalls: true, LowerScalars: true, SinkBindings: true,
                BindScrutinees: true, ShareCallsWide: true, ClosureFree: true, HoistInvariants: true, CacheConstants: true,
                PrimitiveCompare: true)
            {
                InParameters = true, SwitchMatch = true, LowerMore = true, SpecializeCallbacks = true, OwnedArrays = owned,
            };
            var program = CSharpProgram.Build(compilation, NumberProfile.Cpu64, lowerLoops: true, fuse: true, loopOptimizations: flags);
            return new LawRunner(CSharpBackend.Over(program, interp), interp, options).RunAll();
        }

        return (interpreted, Compiled(false), Compiled(true));
    });

    private static string Verdicts(IEnumerable<LawResult> results)
        => string.Join("\n", results.OrderBy(r => r.Name, StringComparer.Ordinal)
            .Select(r => $"{r.Name} {r.Status.ToString().ToLowerInvariant()} {r.Trials}"));

    [Test]
    public void Every_law_but_the_faulting_one_passes_in_the_interpreter_and_compiled_with_the_flag_off_and_on()
    {
        var (interpreted, off, on) = Runs.Value;
        Assert.That(interpreted.Select(r => r.Name), Does.Contain(FaultingLaw));
        Assert.That(interpreted.Concat(off).Concat(on).Where(r => r.Name != FaultingLaw && r.Status != LawStatus.Pass).Select(r => r.Name), Is.Empty);
        Assert.That(interpreted.Count(r => r.Name != FaultingLaw), Is.EqualTo(19));
    }

    [Test]
    public void The_read_past_the_end_faults_alike_in_the_three_runs()
    {
        var (interpreted, off, on) = Runs.Value;
        var statuses = new[] { interpreted, off, on }.Select(run => run.Single(r => r.Name == FaultingLaw).Status).ToList();
        Assert.That(statuses[0], Is.Not.EqualTo(LawStatus.Pass));
        Assert.That(statuses[1], Is.EqualTo(statuses[0]));
        Assert.That(statuses[2], Is.EqualTo(statuses[0]));
    }

    [Test]
    public void Verdicts_are_equal_in_the_three_runs()
    {
        var (interpreted, off, on) = Runs.Value;
        Assert.That(Verdicts(off), Is.EqualTo(Verdicts(interpreted)));
        Assert.That(Verdicts(on), Is.EqualTo(Verdicts(interpreted)));
    }
}
