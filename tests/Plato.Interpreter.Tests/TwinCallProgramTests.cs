// Runs the --span-params call routing fixture interpreted and compiled, with and without twins, and checks the verdicts are equal.
using Ara3D.Geometry.Compiler.Types;
using Ara3D.Geometry.CSharpWriter;
using Ara3D.Geometry.Interpreter;
using Ara3D.Geometry.Laws;

namespace Plato.Interpreter.Tests;

/// <summary>
/// TKT-1152 C8b: <c>tests/PlatoTests/Emission/OwnedArrayFixtures/twin-calls.plato</c> states, as laws, the result the immutable
/// meaning gives for calls that hand an owned array to a function that only reads it (a slot lent to two callees, a heap
/// array, two arrays in one call, a parameter array, a generic callee, a recursive caller). Routing a call to a span twin must
/// never change a value: every ordinary law passes in the interpreter and in the compiled program built with the nineteen pinned
/// flags plus <c>--owned-arrays --stack-arrays</c>, with and without <c>--span-params</c>, and the three verdicts are equal.
/// The law that reads past the end of the array inside a twin faults in all three.
/// </summary>
[TestFixture, Category("Slow")]
public class TwinCallProgramTests
{
    private const string FaultingLaw = "TwinProbe.Law_PastEnd";

    private static readonly Lazy<(IReadOnlyList<LawResult> Interpreted, IReadOnlyList<LawResult> Plain, IReadOnlyList<LawResult> Twins)> Runs = new(() =>
    {
        var folder = Path.Combine(TestSupport.RepoRoot, "tests", "PlatoTests", "Emission", "OwnedArrayFixtures");
        var compilation = LawRunnerSelectionTests.CompileSource(File.ReadAllText(Path.Combine(folder, "twin-calls.plato")));
        var options = new LawRunOptions { Filters = new[] { "TwinProbe" } };
        var interp = new Ara3D.Geometry.Interpreter.Interpreter(compilation, NumberProfile.Cpu64);
        var interpreted = new LawRunner(interp, options).RunAll();

        IReadOnlyList<LawResult> Compiled(bool twins)
        {
            var flags = new LoopOptimizationOptions(FuseConsumers: true, ExactOutputAllocation: true, ConcreteArrays: true,
                LeanInlining: true, Presize: true, InPlace: true, ShareCalls: true, LowerScalars: true, SinkBindings: true,
                BindScrutinees: true, ShareCallsWide: true, ClosureFree: true, HoistInvariants: true, CacheConstants: true,
                PrimitiveCompare: true)
            {
                InParameters = true, SwitchMatch = true, LowerMore = true, SpecializeCallbacks = true, OwnedArrays = true,
                StackArrays = true, SpanParams = twins,
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
    public void Every_law_but_the_faulting_one_passes_in_the_interpreter_and_compiled_with_and_without_twins()
    {
        var (interpreted, plain, twins) = Runs.Value;
        Assert.That(interpreted.Select(r => r.Name), Does.Contain(FaultingLaw));
        Assert.That(interpreted.Concat(plain).Concat(twins).Where(r => r.Name != FaultingLaw && r.Status != LawStatus.Pass).Select(r => r.Name), Is.Empty);
        Assert.That(interpreted.Count(r => r.Name != FaultingLaw), Is.EqualTo(10));
    }

    [Test]
    public void The_read_past_the_end_inside_a_twin_faults_alike_in_the_three_runs()
    {
        var (interpreted, plain, twins) = Runs.Value;
        var statuses = new[] { interpreted, plain, twins }.Select(run => run.Single(r => r.Name == FaultingLaw).Status).ToList();
        Assert.That(statuses[0], Is.Not.EqualTo(LawStatus.Pass));
        Assert.That(statuses[1], Is.EqualTo(statuses[0]));
        Assert.That(statuses[2], Is.EqualTo(statuses[0]));
    }

    [Test]
    public void Verdicts_are_equal_in_the_three_runs()
    {
        var (interpreted, plain, twins) = Runs.Value;
        Assert.That(Verdicts(plain), Is.EqualTo(Verdicts(interpreted)));
        Assert.That(Verdicts(twins), Is.EqualTo(Verdicts(interpreted)));
    }
}
