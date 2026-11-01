// Runs the --scalar-arrays fixture compiled with the flag off and on, and interpreted, and compares values, call order and verdicts.
using System.Linq.Expressions;
using System.Reflection;
using Ara3D.Geometry.Compiler.Types;
using Ara3D.Geometry.CSharpWriter;
using Ara3D.Geometry.Interpreter;
using Ara3D.Geometry.Laws;

namespace Plato.Interpreter.Tests;

/// <summary>
/// TKT-1152 chunk C0. The fixture <c>tests/PlatoTests/Optimization/ScalarArraysFixtures/scalar-arrays.plato</c> consumes
/// the component array of a <c>Vector3D</c> in every shape the rewrite handles. The whole program is compiled with the
/// nineteen pinned switches of the plato-brep regeneration plus <c>--fuse</c>, with <c>--scalar-arrays</c> off and on, and
/// the compiled probes are called with the same vectors: the results must be bit-equal, a callback must be called with
/// the same arguments in the same order, and a callback that throws must be the last one called. Every law must pass in
/// the interpreter and in both compiled arms with equal verdicts.
/// </summary>
[TestFixture, Category("Slow")]
public class ScalarArraysProgramTests
{
    private sealed record Run(IReadOnlyList<LawResult> Results, CSharpProgram Program);

    private static readonly Lazy<(IReadOnlyList<LawResult> Interpreted, Run Off, Run On)> Runs = new(() =>
    {
        var path = Path.Combine(TestSupport.RepoRoot, "tests", "PlatoTests", "Optimization", "ScalarArraysFixtures", "scalar-arrays.plato");
        var compilation = LawRunnerSelectionTests.CompileSource(File.ReadAllText(path));
        var options = new LawRunOptions { Filters = new[] { "ScalarArrayProbe" } };
        var interp = new Ara3D.Geometry.Interpreter.Interpreter(compilation, NumberProfile.Cpu64);
        var interpreted = new LawRunner(interp, options).RunAll();

        Run Compiled(bool scalarArrays)
        {
            var program = CSharpProgram.Build(compilation, NumberProfile.Cpu64, lowerLoops: true, fuse: true,
                loopOptimizations: new LoopOptimizationOptions(
                    FuseConsumers: true, ExactOutputAllocation: true, ConcreteArrays: true, LeanInlining: true, Presize: true,
                    InPlace: true, ShareCalls: true, LowerScalars: true, SinkBindings: true, BindScrutinees: true,
                    ShareCallsWide: true, ClosureFree: true, HoistInvariants: true, CacheConstants: true, PrimitiveCompare: true)
                {
                    InParameters = true, SwitchMatch = true, LowerMore = true, SpecializeCallbacks = true, ScalarArrays = scalarArrays,
                });
            var results = new LawRunner(CSharpBackend.Over(program, interp), interp, options).RunAll();
            return new Run(results, program);
        }

        return (interpreted, Compiled(false), Compiled(true));
    });

    [OneTimeTearDown]
    public void Unload()
    {
        if (!Runs.IsValueCreated)
            return;
        Runs.Value.Off.Program.Unload();
        Runs.Value.On.Program.Unload();
    }

    private static string Verdicts(IEnumerable<LawResult> results)
        => string.Join("\n", results.OrderBy(r => r.Name, StringComparer.Ordinal)
            .Select(r => $"{r.Name} {r.Status.ToString().ToLowerInvariant()} {r.Trials}"));

    [Test]
    public void Every_law_passes_in_the_interpreter_and_compiled_with_the_flag_off_and_on()
    {
        var (interpreted, off, on) = Runs.Value;
        Assert.That(interpreted, Has.Count.EqualTo(13));
        Assert.That(interpreted.Concat(off.Results).Concat(on.Results).Where(r => r.Status != LawStatus.Pass).Select(r => r.Name), Is.Empty);
    }

    [Test]
    public void Verdicts_are_equal_in_the_three_runs()
    {
        var (interpreted, off, on) = Runs.Value;
        Assert.That(Verdicts(off.Results), Is.EqualTo(Verdicts(interpreted)));
        Assert.That(Verdicts(on.Results), Is.EqualTo(Verdicts(interpreted)));
    }

    // ---- calling the compiled probes ----------------------------------------------------------------------

    private static Type Named(CSharpProgram p, string name) => p.Assembly.GetType("Ara3D.Geometry." + name, true)!;

    private static MethodInfo Probe(CSharpProgram p, string name)
        => p.Assembly.GetTypes().Single(t => t.Name == "ScalarArrayProbe")
            .GetMethods(BindingFlags.Public | BindingFlags.Static).Single(m => m.Name == name);

    private static object Num(CSharpProgram p, double value)
        => Named(p, "Number").GetMethods().Single(m => m.Name == "op_Implicit" && m.GetParameters()[0].ParameterType == typeof(double)
            && m.ReturnType == Named(p, "Number")).Invoke(null, new object[] { value })!;

    private static double Val(CSharpProgram p, object number)
        => (double)Named(p, "Number").GetMethods().Single(m => m.Name == "op_Implicit" && m.ReturnType == typeof(double)).Invoke(null, new[] { number })!;

    private static object Vec(CSharpProgram p, (double X, double Y, double Z) v)
        => Activator.CreateInstance(Named(p, "Vector3D"), Num(p, v.X), Num(p, v.Y), Num(p, v.Z))!;

    private static double[] Parts(CSharpProgram p, object vector)
        => new[] { "X", "Y", "Z" }.Select(n => Val(p, Named(p, "Vector3D").GetField(n)!.GetValue(vector)!)).ToArray();

    private static string Bits(IEnumerable<double> values) => string.Join(" ", values.Select(d => BitConverter.DoubleToInt64Bits(d).ToString("x16")));

    /// <summary>Calls the probe and returns what it computed as text that holds the bits of every double.</summary>
    private static string Call(CSharpProgram p, string name, params object[] args)
    {
        var result = Probe(p, name).Invoke(null, args)!;
        return result.GetType().Name switch
        {
            "Vector3D" => Bits(Parts(p, result)),
            "Number" => Bits(new[] { Val(p, result) }),
            _ => ((dynamic)result).ToString(),
        };
    }

    private static readonly (double X, double Y, double Z)[] Vectors =
    {
        (1.5, -2.0, 3.25), (3.0, -2.0, 1.0), (-0.0, 0.0, -0.0), (0.0, -0.0, 0.0), (double.NaN, 1.0, 2.0), (1.0, double.NaN, -1.0),
        (1.0, 2.0, double.NaN), (double.PositiveInfinity, double.NegativeInfinity, 0.0), (1e308, 1e308, -1e308), (5e-324, -5e-324, 1e-310),
        (0.1, 0.2, 0.3), (-7.0, -7.0, -7.0),
    };

    private static IEnumerable<TestCaseData> EveryVectorWith(params string[] names)
        => names.SelectMany(n => Vectors.Select((v, i) => new TestCaseData(n, i).SetName($"{n}_{i}")));

    [TestCaseSource(nameof(OneVectorCases))]
    public void A_probe_of_one_vector_gives_bit_equal_results(string name, int index)
    {
        var (_, off, on) = Runs.Value;
        Assert.That(Call(on.Program, name, Vec(on.Program, Vectors[index])), Is.EqualTo(Call(off.Program, name, Vec(off.Program, Vectors[index]))), name);
    }

    private static IEnumerable<TestCaseData> OneVectorCases()
        => EveryVectorWith("SumSqr", "Smallest", "Largest", "Rebuilt", "AllPositive", "AnyNegative", "Arity", "Middle");

    [TestCaseSource(nameof(TwoVectorCases))]
    public void A_zip_of_two_vectors_gives_bit_equal_results(string name, int index)
    {
        var (_, off, on) = Runs.Value;
        object[] Args(CSharpProgram p) => new[] { Vec(p, Vectors[index]), Vec(p, Vectors[(index + 5) % Vectors.Length]) };
        Assert.That(Call(on.Program, name, Args(on.Program)), Is.EqualTo(Call(off.Program, name, Args(off.Program))), name);
    }

    private static IEnumerable<TestCaseData> TwoVectorCases() => EveryVectorWith("Summed", "AllLess");

    [TestCaseSource(nameof(ThreeVectorCases))]
    public void The_three_way_zip_and_the_capturing_map_give_bit_equal_results(string name, int index)
    {
        var (_, off, on) = Runs.Value;
        object[] Args(CSharpProgram p) => name == "Scaled"
            ? new[] { Vec(p, Vectors[index]), Num(p, 0.1) }
            : new[] { Vec(p, Vectors[index]), Vec(p, Vectors[(index + 3) % Vectors.Length]), Vec(p, Vectors[(index + 7) % Vectors.Length]) };
        Assert.That(Call(on.Program, name, Args(on.Program)), Is.EqualTo(Call(off.Program, name, Args(off.Program))), name);
    }

    private static IEnumerable<TestCaseData> ThreeVectorCases() => EveryVectorWith("Blended", "Scaled");

    // ---- callbacks: the order of the calls, and a fault in the middle ----------------------------------------

    /// <summary>A delegate of the program's own Func type that calls <paramref name="body"/> with the argument values
    /// (as doubles) and returns its double, converted back to Number. The calls it saw are in <paramref name="log"/>.</summary>
    private static Delegate Callback(CSharpProgram p, Type delegateType, List<string> log, Func<double[], double> body)
    {
        var parameters = delegateType.GetMethod("Invoke")!.GetParameters().Select(x => Expression.Parameter(x.ParameterType)).ToArray();
        var recorder = new Recorder(p, log, body);
        var call = Expression.Call(Expression.Constant(recorder), nameof(Recorder.Call), null,
            Expression.NewArrayInit(typeof(object), parameters.Select(x => Expression.Convert(x, typeof(object)))));
        return Expression.Lambda(delegateType, Expression.Convert(call, Named(p, "Number")), parameters).Compile();
    }

    private sealed class Recorder
    {
        private readonly CSharpProgram _p; private readonly List<string> _log; private readonly Func<double[], double> _body;
        public Recorder(CSharpProgram p, List<string> log, Func<double[], double> body) => (_p, _log, _body) = (p, log, body);

        public object Call(object[] arguments)
        {
            var values = arguments.Select(a => Val(_p, a)).ToArray();
            _log.Add(Bits(values));
            return Num(_p, _body(values));
        }
    }

    private sealed record Outcome(string Result, IReadOnlyList<string> Calls, string Fault);

    private static void AssertSame(Outcome actual, Outcome expected)
    {
        Assert.That(actual.Result, Is.EqualTo(expected.Result), "the result");
        Assert.That(actual.Fault, Is.EqualTo(expected.Fault), "the fault");
        Assert.That(actual.Calls, Is.EqualTo(expected.Calls), "the calls, in order");
    }

    private static Outcome Observe(CSharpProgram p, string name, int index, int arity, Func<double[], double> body)
    {
        var log = new List<string>();
        var method = Probe(p, name);
        var callback = Callback(p, method.GetParameters()[^1].ParameterType, log, body);
        try { return new Outcome(Call(p, name, Vec(p, Vectors[index]), callback), log, ""); }
        catch (TargetInvocationException e) { return new Outcome("", log, e.InnerException!.GetType().Name + ": " + e.InnerException.Message); }
    }

    [TestCase("Mapped", 1, 1)]
    [TestCase("Mapped", 4, 1)]
    [TestCase("Folded", 0, 2)]
    [TestCase("Folded", 7, 2)]
    public void A_callback_is_called_with_the_same_arguments_in_the_same_order(string name, int index, int arity)
    {
        var (_, off, on) = Runs.Value;
        double Body(double[] a) => a.Length == 1 ? a[0] * a[0] + 1.0 : a[0] * 2.0 + a[1];
        var expected = Observe(off.Program, name, index, arity, Body);
        var actual = Observe(on.Program, name, index, arity, Body);
        AssertSame(actual, expected);
        Assert.That(actual.Calls, Has.Count.EqualTo(3), "one call per component");
    }

    [TestCase("Mapped", 1)]
    [TestCase("Mapped", 2)]
    [TestCase("Folded", 1)]
    [TestCase("Folded", 2)]
    public void A_callback_that_throws_on_the_second_component_is_the_last_one_called(string name, int failAt)
    {
        var (_, off, on) = Runs.Value;
        // The vector is (1.5, -2.0, 3.25): throw when the component handed over is the one at the given position.
        var component = new[] { 1.5, -2.0, 3.25 }[failAt];
        double Body(double[] a)
        {
            if (a[^1] == component)
                throw new InvalidOperationException("fault at component " + failAt);
            return a[^1];
        }
        var expected = Observe(off.Program, name, 0, name == "Mapped" ? 1 : 2, Body);
        var actual = Observe(on.Program, name, 0, name == "Mapped" ? 1 : 2, Body);
        Assert.That(expected.Fault, Does.Contain("fault at component " + failAt));
        AssertSame(actual, expected);
        Assert.That(actual.Calls, Has.Count.EqualTo(failAt + 1), "no component after the fault is read");
    }

    // ---- the flag changed the compiled code ---------------------------------------------------------------------

    /// <summary>The names of the methods a compiled method calls, read from its IL (call and callvirt tokens).</summary>
    private static IReadOnlyList<string> Calls(CSharpProgram program, string name)
    {
        var method = Probe(program, name);
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

    [Test]
    public void The_flag_removes_the_call_that_makes_the_component_array()
    {
        Assert.That(Calls(Runs.Value.Off.Program, "Rebuilt"), Does.Contain("ComponentArray"));
        Assert.That(Calls(Runs.Value.On.Program, "Rebuilt"), Does.Not.Contain("ComponentArray").And.Not.Contain("FromComponents"));
    }
}
