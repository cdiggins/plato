// Runs the --in-place fixture compiled and interpreted, and checks the two give the same verdicts and values.
using System.Reflection;
using Ara3D.Geometry.Compiler.Types;
using Ara3D.Geometry.CSharpWriter;
using Ara3D.Geometry.Interpreter;
using Ara3D.Geometry.Laws;

namespace Plato.Interpreter.Tests;

/// <summary>
/// TKT-1132: the fixture <c>tests/PlatoTests/Optimization/InPlaceFixtures/in-place.plato</c> states, as
/// laws, the result the immutable meaning of each array update gives. A wrong liveness proof writes into
/// an array something still reads, so a law fails in the compiled arm and passes in the interpreter,
/// which copies. The program is compiled with the flag off and on; every law must pass in all three and the
/// verdicts must be equal, and the flag must have changed the compiled code of the cases that are in place.
/// </summary>
[TestFixture, Category("Slow")]
public class InPlaceProgramTests
{
    private sealed record Run(IReadOnlyList<LawResult> Results, CSharpProgram Program);

    private static readonly Lazy<(IReadOnlyList<LawResult> Interpreted, Run Off, Run On)> Runs = new(() =>
    {
        var path = Path.Combine(TestSupport.RepoRoot, "tests", "PlatoTests", "Optimization", "InPlaceFixtures", "in-place.plato");
        var compilation = LawRunnerSelectionTests.CompileSource(File.ReadAllText(path));
        var options = new LawRunOptions { Filters = new[] { "InPlaceProbe" } };
        var interp = new Ara3D.Geometry.Interpreter.Interpreter(compilation, NumberProfile.Cpu64);
        var interpreted = new LawRunner(interp, options).RunAll();

        Run Compiled(bool inPlace)
        {
            var program = CSharpProgram.Build(compilation, NumberProfile.Cpu64, lowerLoops: true, fuse: true,
                loopOptimizations: new LoopOptimizationOptions(InPlace: inPlace));
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
        Assert.That(interpreted, Has.Count.EqualTo(14));
        Assert.That(interpreted.Concat(off.Results).Concat(on.Results).Where(r => r.Status != LawStatus.Pass).Select(r => r.Name), Is.Empty);
    }

    [Test]
    public void Verdicts_are_equal_in_the_three_runs()
    {
        var (interpreted, off, on) = Runs.Value;
        Assert.That(Verdicts(off.Results), Is.EqualTo(Verdicts(interpreted)));
        Assert.That(Verdicts(on.Results), Is.EqualTo(Verdicts(interpreted)));
    }

    /// <summary>The names of the methods a compiled method calls, read from its IL (call and callvirt tokens).</summary>
    private static IReadOnlyList<string> Calls(CSharpProgram program, string name)
    {
        var probe = program.Assembly.GetTypes().Single(t => t.Name == "InPlaceProbe");
        var method = probe.GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == name && m.DeclaringType == probe);
        var il = method.GetMethodBody()!.GetILAsByteArray()!;
        var names = new List<string>();
        for (var i = 0; i + 4 < il.Length; i++)
        {
            if (il[i] != 0x28 && il[i] != 0x6F)
                continue;
            var token = BitConverter.ToInt32(il, i + 1);
            if ((token >> 24) is not (0x06 or 0x0A or 0x2B))
                continue;
            try { names.Add(method.Module.ResolveMethod(token)!.Name); }
            catch (Exception e) when (e is ArgumentException or NotSupportedException) { }
        }
        return names;
    }

    [TestCase("Threaded")]
    [TestCase("Chained")]
    [TestCase("HandOff")]
    [TestCase("Alternating")]
    [TestCase("PerIteration")]
    public void The_flag_makes_the_compiled_update_of_a_kept_case_write_in_place(string name)
    {
        Assert.That(Calls(Runs.Value.Off.Program, name), Does.Contain("SetAt").And.Not.Contain("SetAtInPlace"));
        Assert.That(Calls(Runs.Value.On.Program, name), Does.Contain("SetAtInPlace").And.Not.Contain("SetAt"));
    }

    [TestCase("LaterUse")]
    [TestCase("Aliased")]
    [TestCase("Captured")]
    [TestCase("Escaped")]
    [TestCase("NotThreaded")]
    [TestCase("OfField")]
    public void The_flag_leaves_the_compiled_update_of_a_rejected_case_a_copy(string name)
    {
        Assert.That(Calls(Runs.Value.On.Program, name), Does.Contain("SetAt").And.Not.Contain("SetAtInPlace"));
        Assert.That(Calls(Runs.Value.On.Program, name), Is.EqualTo(Calls(Runs.Value.Off.Program, name)));
    }
}
