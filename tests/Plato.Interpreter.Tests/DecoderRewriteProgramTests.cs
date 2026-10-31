// Runs the decoder-wave rewrite fixture interpreted and compiled, with the switches off and on (TKT-1138, TKT-1142).
using System.Reflection;
using Ara3D.Geometry.Compiler.Checking;
using Ara3D.Geometry.Compiler.Types;
using Ara3D.Geometry.CSharpWriter;
using Ara3D.Geometry.Interpreter;
using Ara3D.Geometry.Laws;

namespace Plato.Interpreter.Tests;

/// <summary>
/// The fixture <c>tests/PlatoTests/Optimization/DecoderRewriteFixtures/rewrites.plato</c> states, as laws, the value of
/// each case read plainly. It is run four times, interpreted and compiled with <c>--bind-scrutinee</c>,
/// <c>--sink-bindings</c> and <c>--share-calls-wide</c> off and on; every law must pass in all four and the verdicts
/// must be equal. The interpreter counts how often it evaluates <c>Cost</c> and <c>Row</c> in the probes, and the compiled
/// code is read for the number of call sites of <c>Cost</c> and <c>IsKind</c>.
/// </summary>
[TestFixture, Category("Slow")]
public class DecoderRewriteProgramTests
{
    private const string Library = "RewriteProbe";

    private sealed record Run(Ara3D.Geometry.Compiler.Compilation Compilation, Ara3D.Geometry.Interpreter.Interpreter Interpreter,
        IReadOnlyList<LawResult> Interpreted, IReadOnlyList<LawResult> Compiled, CSharpProgram Program);

    private static readonly Lazy<(Run Off, Run On)> Runs = new(() =>
    {
        var path = Path.Combine(TestSupport.RepoRoot, "tests", "PlatoTests", "Optimization", "DecoderRewriteFixtures", "rewrites.plato");
        var source = File.ReadAllText(path);
        Run Make(bool on)
        {
            var compilation = LawRunnerSelectionTests.CompileSource(source);
            var optimizations = new LoopOptimizationOptions(ShareCalls: true, SinkBindings: on, BindScrutinees: on, ShareCallsWide: on);
            var interp = new Ara3D.Geometry.Interpreter.Interpreter(compilation, NumberProfile.Cpu64, optimizations.Elaboration) { Memo = null };
            var options = new LawRunOptions { Filters = new[] { Library } };
            var interpreted = new LawRunner(interp, options).RunAll();
            var program = CSharpProgram.Build(compilation, NumberProfile.Cpu64, lowerLoops: true, fuse: true, loopOptimizations: optimizations);
            var compiled = new LawRunner(CSharpBackend.Over(program, interp), interp, options).RunAll();
            return new Run(compilation, interp, interpreted, compiled, program);
        }
        return (Make(false), Make(true));
    });

    private static string Verdicts(IEnumerable<LawResult> results)
        => string.Join("\n", results.OrderBy(r => r.Name, StringComparer.Ordinal)
            .Select(r => $"{r.Name} {r.Status.ToString().ToLowerInvariant()} {r.Trials}"));

    [Test]
    public void Every_law_passes_interpreted_and_compiled_with_the_switches_off_and_on()
    {
        var (off, on) = Runs.Value;
        Assert.That(off.Interpreted, Has.Count.EqualTo(25));
        var all = off.Interpreted.Concat(off.Compiled).Concat(on.Interpreted).Concat(on.Compiled);
        Assert.That(all.Where(r => r.Status != LawStatus.Pass).Select(r => r.Name), Is.Empty);
    }

    [Test]
    public void Verdicts_are_equal_in_the_four_runs()
    {
        var (off, on) = Runs.Value;
        var expected = Verdicts(off.Interpreted);
        Assert.That(Verdicts(off.Compiled), Is.EqualTo(expected));
        Assert.That(Verdicts(on.Interpreted), Is.EqualTo(expected));
        Assert.That(Verdicts(on.Compiled), Is.EqualTo(expected));
    }

    /// <summary>How often the interpreter evaluates a callee while it runs one nullary probe.</summary>
    private static long Evaluations(Run run, string probe, string callee)
    {
        var profile = new CallProfile();
        run.Interpreter.Profiler = profile;
        try
        {
            var function = run.Compilation.LibraryDefinitionsByName[Library].Functions.Single(f => f.Name == probe);
            run.Interpreter.Invoke(function);
        }
        finally
        {
            run.Interpreter.Profiler = null;
        }
        return profile.Entries.Where(e => e.Function.Name == callee).Sum(e => e.Calls);
    }

    [TestCase("Probe_MatchBody", "Row", 3, 1, Description = "the test and two field reads name the scrutinee")]
    [TestCase("Probe_MatchInLocal", "Row", 3, 1)]
    [TestCase("Probe_MatchOperand", "Row", 3, 1)]
    [TestCase("Probe_MatchInArm", "Row", 3, 3, Description = "an arm of a conditional keeps its scrutinee as written")]
    [TestCase("Probe_SunkOneArm", "Cost", 1, 0, Description = "the arm is not taken: the local is not evaluated")]
    [TestCase("Probe_SunkChain", "Cost", 1, 0)]
    [TestCase("Probe_SunkTwoReads", "Cost", 1, 0)]
    [TestCase("Probe_SunkIfBranch", "Cost", 1, 0)]
    [TestCase("Probe_SunkChained", "Cost", 2, 0, Description = "the local and the local that reads it")]
    [TestCase("Probe_SunkKeptCondition", "Cost", 1, 1, Description = "the condition reads it: evaluated on every path")]
    public void The_interpreter_evaluates_as_the_rules_say(string probe, string callee, int off, int on)
    {
        var (offRun, onRun) = Runs.Value;
        Assert.That(Evaluations(offRun, probe, callee), Is.EqualTo(off), "switches off");
        Assert.That(Evaluations(onRun, probe, callee), Is.EqualTo(on), "switches on");
    }

    /// <summary>How many times a compiled method calls a method of the given name, read from its IL (call and callvirt tokens).</summary>
    private static int CallsTo(CSharpProgram program, string method, string callee)
    {
        var probe = program.Assembly.GetTypes().Single(t => t.Name == Library);
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

    // Both runs have --share-calls, as the pinned flags do, so a match on a call is already shared in the compiled code.
    [TestCase("MatchBody", "Row", 1, 1)]
    [TestCase("MatchInLocal", "Row", 1, 1)]
    [TestCase("MatchOperand", "Row", 1, 1)]
    [TestCase("MatchInArm", "Row", 4, 4)]
    [TestCase("AcrossStatements", "Cost", 2, 1)]
    [TestCase("AcrossBranch", "Cost", 2, 1)]
    [TestCase("AcrossKind", "IsKind", 2, 1)]
    [TestCase("AcrossKept", "Cost", 2, 2)]
    [TestCase("AcrossDifferent", "Cost", 2, 2)]
    [TestCase("InLambdaOuter", "Cost", 2, 1)]
    public void The_compiled_call_sites_are_as_the_rules_say(string method, string callee, int off, int on)
    {
        var (offRun, onRun) = Runs.Value;
        Assert.That(CallsTo(offRun.Program, method, callee), Is.EqualTo(off), "switches off");
        Assert.That(CallsTo(onRun.Program, method, callee), Is.EqualTo(on), "switches on");
    }

    /// <summary>The elaborator's binding alone, with no <c>--share-calls</c> to hide the duplicate evaluation (TKT-1138).</summary>
    [TestCase("MatchBody", "Row", 4, 1)]
    [TestCase("MatchInLocal", "Row", 4, 1)]
    [TestCase("MatchOperand", "Row", 4, 1)]
    [TestCase("MatchInArm", "Row", 4, 4)]
    public void The_elaborator_binds_a_scrutinee_without_the_writer_pass(string method, string callee, int off, int on)
    {
        var compilation = Runs.Value.Off.Compilation;
        CSharpProgram Build(bool bind) => CSharpProgram.Build(compilation, NumberProfile.Cpu64, lowerLoops: true, fuse: true,
            loopOptimizations: new LoopOptimizationOptions(BindScrutinees: bind));
        Assert.That(CallsTo(Build(false), method, callee), Is.EqualTo(off), "switch off");
        Assert.That(CallsTo(Build(true), method, callee), Is.EqualTo(on), "switch on");
    }
}
