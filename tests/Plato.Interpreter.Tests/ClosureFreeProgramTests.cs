// Runs the --closure-free fixture interpreted and compiled with the flag off and on, and checks verdicts, calls and faults.
using System.Reflection;
using Ara3D.Geometry.Compiler.Types;
using Ara3D.Geometry.CSharpWriter;
using Ara3D.Geometry.Interpreter;
using Ara3D.Geometry.Laws;

namespace Plato.Interpreter.Tests;

/// <summary>
/// TKT-1149: the fixture <c>tests/PlatoTests/Optimization/ClosureFreeFixtures/closure-free.plato</c> states, as laws,
/// the value of each copy-shaped Array function and of Sort and SortedIndices (ties included, so stability is pinned).
/// The program is compiled with the flag off and on; every law must pass in all three runs, the verdicts must be equal,
/// and the compiled code must have lost the closure, the delegate per element and the delegate per compare.
/// </summary>
[TestFixture, Category("Slow")]
public class ClosureFreeProgramTests
{
    private sealed record Run(IReadOnlyList<LawResult> Results, CSharpProgram Program);

    private static readonly Lazy<(IReadOnlyList<LawResult> Interpreted, Run Off, Run On)> Runs = new(() =>
    {
        var path = Path.Combine(TestSupport.RepoRoot, "tests", "PlatoTests", "Optimization", "ClosureFreeFixtures", "closure-free.plato");
        var compilation = LawRunnerSelectionTests.CompileSource(File.ReadAllText(path));
        var options = new LawRunOptions { Filters = new[] { "ClosureFreeProbe" } };
        var interp = new Ara3D.Geometry.Interpreter.Interpreter(compilation, NumberProfile.Cpu64);
        var interpreted = new LawRunner(interp, options).RunAll();

        Run Compiled(bool closureFree)
        {
            var program = CSharpProgram.Build(compilation, NumberProfile.Cpu64, lowerLoops: true, fuse: true,
                loopOptimizations: new LoopOptimizationOptions(ClosureFree: closureFree));
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
        string Failing(string run, IEnumerable<LawResult> results)
            => string.Join(" | ", results.Where(r => r.Status != LawStatus.Pass).Select(r => $"{run}: {r.Name} {r.Status}: {r.Message}"));
        Assert.That(Failing("interpreted", interpreted) + Failing("off", off.Results) + Failing("on", on.Results), Is.Empty);
    }

    [Test]
    public void Verdicts_are_equal_in_the_three_runs()
    {
        var (interpreted, off, on) = Runs.Value;
        Assert.That(Verdicts(off.Results), Is.EqualTo(Verdicts(interpreted)));
        Assert.That(Verdicts(on.Results), Is.EqualTo(Verdicts(interpreted)));
    }

    /// <summary>The methods a compiled method calls as Type.Method, read from its IL (call and callvirt tokens).</summary>
    private static List<string> Callees(MethodInfo m)
    {
        var il = m.GetMethodBody()!.GetILAsByteArray()!;
        var typeArgs = m.DeclaringType!.IsGenericType ? m.DeclaringType.GetGenericArguments() : null;
        var methodArgs = m.IsGenericMethod ? m.GetGenericArguments() : null;
        var names = new List<string>();
        for (var i = 0; i + 4 < il.Length; i++)
        {
            if (il[i] != 0x28 && il[i] != 0x6F)
                continue;
            var token = BitConverter.ToInt32(il, i + 1);
            if ((token >> 24) is not (0x06 or 0x0A or 0x2B))
                continue;
            try
            {
                var callee = m.Module.ResolveMethod(token, typeArgs, methodArgs)!;
                names.Add($"{callee.DeclaringType!.Name}.{callee.Name}");
            }
            catch (Exception e) when (e is ArgumentException or NotSupportedException) { }
        }
        return names;
    }

    private static MethodInfo Probe(Run run, string name)
    {
        // A function on an Array receiver is an extension on IReadOnlyList, so it is written in Extensions.
        var probe = run.Program.Assembly.GetType($"{CSharpProgram.Namespace}.Extensions")!;
        return probe.GetMethods(BindingFlags.Public | BindingFlags.Static).Single(x => x.Name == name);
    }

    private static MethodInfo Extension(Run run, string name)
    {
        var extensions = run.Program.Assembly.GetType($"{CSharpProgram.Namespace}.Extensions")!;
        return extensions.GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == name && m.IsGenericMethodDefinition
                && m.GetParameters()[0].ParameterType.IsGenericType
                && m.GetParameters()[0].ParameterType.GetGenericTypeDefinition() == typeof(IReadOnlyList<>));
    }

    [TestCase("Append", "Append"), TestCase("Prepend", "Prepend"), TestCase("Concatenate", "Concatenate"),
     TestCase("Take", "Take"), TestCase("Skip", "Skip"), TestCase("TakeLast", "TakeLast"),
     TestCase("SubArray", "SubArray"), TestCase("Slice", "Slice"), TestCase("SetAt", "SetAt")]
    public void A_copy_shaped_function_calls_the_block_copy_and_no_longer_a_map_range(string name, string helper)
    {
        var (_, off, on) = Runs.Value;
        var before = Callees(Extension(off, name));
        Assert.That(before, Has.Some.Contains("MapRange"), "flag off");
        Assert.That(before, Has.None.StartsWith("ClosureFreeArrays."), "flag off");
        var after = Callees(Extension(on, name));
        Assert.That(after, Has.Some.EqualTo($"ClosureFreeArrays.{helper}"), "flag on");
        Assert.That(after, Has.None.Contains("MapRange"), "flag on");
    }

    [TestCase("SortedIndicesOf"), TestCase("SortedTags"), TestCase("SortedByModulus")]
    public void A_sort_with_a_known_comparator_is_a_loop_with_no_call_per_compare(string method)
    {
        var (_, off, on) = Runs.Value;
        Assert.That(Callees(Probe(off, method)), Has.Some.Matches<string>(c => c is "Extensions.Sort" or "Extensions.SortedIndices"), "flag off");
        var calls = Callees(Probe(on, method));
        Assert.That(calls, Has.None.Matches<string>(c => c is "Extensions.Sort" or "Extensions.SortedIndices"), "flag on");
        Assert.That(calls, Has.None.EqualTo("Func`3.Invoke"), "flag on");
    }

    [Test]
    public void A_sort_through_a_function_value_is_still_a_loop_and_calls_the_function_per_compare()
    {
        var (_, _, on) = Runs.Value;
        var calls = Callees(Probe(on, "SortedBy"));
        Assert.That(calls, Has.None.EqualTo("Extensions.Sort"));
        Assert.That(calls, Has.Some.EqualTo("Func`3.Invoke"));
    }

    [Test]
    public void A_window_outside_the_source_faults_as_it_did()
    {
        var (_, off, on) = Runs.Value;
        foreach (var name in new[] { "Take", "TakeLast", "Skip" })
        {
            var faults = new[] { off, on }.Select(run =>
            {
                var integer = run.Program.Assembly.GetType($"{CSharpProgram.Namespace}.Integer")!;
                var arg = integer.GetMethod("op_Implicit", new[] { typeof(int) })!.Invoke(null, new object[] { name == "Skip" ? -2 : 9 })!;
                var xs = Array.CreateInstance(integer, 3);
                var method = Extension(run, name).MakeGenericMethod(integer);
                var e = Assert.Throws<TargetInvocationException>(() => method.Invoke(null, new[] { xs, arg }), name);
                return e!.InnerException!.GetType();
            }).ToList();
            Assert.That(faults[1], Is.EqualTo(faults[0]), name);
        }
    }
}
