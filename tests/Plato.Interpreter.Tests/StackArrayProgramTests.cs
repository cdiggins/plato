// Runs the --stack-arrays fixture interpreted and compiled, with --owned-arrays alone and with --stack-arrays, and checks the verdicts are equal.
using Ara3D.Geometry.Compiler.Types;
using Ara3D.Geometry.CSharpWriter;
using Ara3D.Geometry.Interpreter;
using Ara3D.Geometry.Laws;

namespace Plato.Interpreter.Tests;

/// <summary>
/// TKT-1152 C6: <c>tests/PlatoTests/Emission/OwnedArrayFixtures/stack.plato</c> states, as laws, the result the immutable
/// meaning gives for bounded lowered-loop temporaries: a constant count, a count under, at and past the slot capacity,
/// zero and negative counts, a slot reused by the passes of an outer loop, ten windows past the 1 KiB budget, recursive
/// bodies, and arrays that are passed on, returned, captured or assigned again. Storage must never change a value: every
/// ordinary law passes in the interpreter and in the compiled program built with the nineteen pinned flags plus
/// <c>--owned-arrays</c>, with and without <c>--stack-arrays</c>, and the three verdicts are equal. The law whose callback
/// divides by zero for the third element faults in all three.
/// </summary>
[TestFixture, Category("Slow")]
public class StackArrayProgramTests
{
    private const string FaultingLaw = "StackProbe.Law_FaultMidFill";

    private static readonly Lazy<(IReadOnlyList<LawResult> Interpreted, IReadOnlyList<LawResult> Owned, IReadOnlyList<LawResult> Stack)> Runs = new(() =>
    {
        var folder = Path.Combine(TestSupport.RepoRoot, "tests", "PlatoTests", "Emission", "OwnedArrayFixtures");
        var compilation = LawRunnerSelectionTests.CompileSource(File.ReadAllText(Path.Combine(folder, "stack.plato")));
        var options = new LawRunOptions { Filters = new[] { "StackProbe" } };
        var interp = new Ara3D.Geometry.Interpreter.Interpreter(compilation, NumberProfile.Cpu64);
        var interpreted = new LawRunner(interp, options).RunAll();

        IReadOnlyList<LawResult> Compiled(bool stack)
        {
            var flags = new LoopOptimizationOptions(FuseConsumers: true, ExactOutputAllocation: true, ConcreteArrays: true,
                LeanInlining: true, Presize: true, InPlace: true, ShareCalls: true, LowerScalars: true, SinkBindings: true,
                BindScrutinees: true, ShareCallsWide: true, ClosureFree: true, HoistInvariants: true, CacheConstants: true,
                PrimitiveCompare: true)
            {
                InParameters = true, SwitchMatch = true, LowerMore = true, SpecializeCallbacks = true, OwnedArrays = true, StackArrays = stack,
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
    public void Every_law_but_the_faulting_one_passes_in_the_interpreter_and_compiled_with_and_without_stack_arrays()
    {
        var (interpreted, owned, stack) = Runs.Value;
        Assert.That(interpreted.Select(r => r.Name), Does.Contain(FaultingLaw));
        Assert.That(interpreted.Concat(owned).Concat(stack).Where(r => r.Name != FaultingLaw && r.Status != LawStatus.Pass).Select(r => r.Name), Is.Empty);
        Assert.That(interpreted.Count(r => r.Name != FaultingLaw), Is.EqualTo(17));
    }

    [Test]
    public void The_callback_that_faults_mid_fill_faults_alike_in_the_three_runs()
    {
        var (interpreted, owned, stack) = Runs.Value;
        var statuses = new[] { interpreted, owned, stack }.Select(run => run.Single(r => r.Name == FaultingLaw).Status).ToList();
        Assert.That(statuses[0], Is.Not.EqualTo(LawStatus.Pass));
        Assert.That(statuses[1], Is.EqualTo(statuses[0]));
        Assert.That(statuses[2], Is.EqualTo(statuses[0]));
    }

    [Test]
    public void Verdicts_are_equal_in_the_three_runs()
    {
        var (interpreted, owned, stack) = Runs.Value;
        Assert.That(Verdicts(owned), Is.EqualTo(Verdicts(interpreted)));
        Assert.That(Verdicts(stack), Is.EqualTo(Verdicts(interpreted)));
    }
}
