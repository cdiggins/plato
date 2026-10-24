// Checks lazy evaluation of Boolean branches in the interpreter.
using Ara3D.Geometry.Compiler.Types;
using Ara3D.Geometry.Interpreter;
using Ara3D.Geometry.Laws;

namespace Plato.Interpreter.Tests;

/// <summary>
/// TKT-442: Boolean <c>&amp;&amp;</c> evaluates its right operand only when the left is true, and
/// <c>||</c> only when the left is false (docs/SEMANTICS.md section 6), so a count test guards the
/// index on its right. The interpreter and the compiled C# agree on it; an <c>And</c> a library
/// defines for its own type stays an ordinary call that evaluates both sides.
/// </summary>
[TestFixture]
public class ShortCircuitTests
{
    private const string Source = """
        type Flag
        {
            Value: Integer;
        }

        library ShortCircuitFixture
        {
            SecondIsZero(xs: Array<Integer>): Boolean => xs.Count == 2 && xs[1] == 0;
            EmptyOrFirstIsZero(xs: Array<Integer>): Boolean => xs.Count == 0 || xs[0] == 0;
            And(a: Flag, b: Flag): Flag => new Flag(a.Value * b.Value);
            FlagAt(xs: Array<Integer>, i: Integer): Flag => new Flag(xs[i]);
            BothFlags(xs: Array<Integer>): Flag => new Flag(0) && FlagAt(xs, 3);
            Law_AndGuardsTheIndex(k: Integer): Boolean => !SecondIsZero(MapRange(1, i => k));
            Law_OrGuardsTheIndex(k: Integer): Boolean => EmptyOrFirstIsZero(MapRange(0, i => k));
            Law_AndStillTestsTheRight(k: Integer): Boolean => SecondIsZero(MapRange(2, i => i * k)) == (k == 0);
        }
        """;

    private static readonly Lazy<Ara3D.Geometry.Interpreter.Interpreter> _interp = new(() =>
        new Ara3D.Geometry.Interpreter.Interpreter(LawRunnerSelectionTests.CompileSource(Source), NumberProfile.Cpu64));

    private static object Call(string name, params object[] args) => _interp.Value.Call(name, args);

    private static PlatoArray Ints(params long[] xs)
        => new(null, xs.Select(x => (object)x).ToArray());

    [Test]
    public void And_skips_the_index_when_the_count_test_is_false()
    {
        Assert.That(Call("SecondIsZero", Ints(7)), Is.EqualTo(false));
        Assert.That(Call("SecondIsZero", Ints(7, 0)), Is.EqualTo(true));
        Assert.That(Call("SecondIsZero", Ints(7, 1)), Is.EqualTo(false));
    }

    [Test]
    public void Or_skips_the_index_when_the_count_test_is_true()
    {
        Assert.That(Call("EmptyOrFirstIsZero", Ints()), Is.EqualTo(true));
        Assert.That(Call("EmptyOrFirstIsZero", Ints(0)), Is.EqualTo(true));
        Assert.That(Call("EmptyOrFirstIsZero", Ints(4)), Is.EqualTo(false));
    }

    [Test]
    public void A_library_And_over_its_own_type_evaluates_both_sides()
        => Assert.That(() => Call("BothFlags", Ints(1)), Throws.Exception);

    [Test]
    public void Interpreter_and_compiled_csharp_agree_on_the_guarded_laws()
    {
        var compilation = LawRunnerSelectionTests.CompileSource(Source);
        var interp = new Ara3D.Geometry.Interpreter.Interpreter(compilation, NumberProfile.Cpu64);
        var backend = new DifferentialBackend(new InterpreterBackend(interp), CSharpBackend.Build(interp));
        var runner = new LawRunner(backend, interp, new LawRunOptions { Filters = new[] { "ShortCircuitFixture" } });
        var results = runner.RunAll();
        TestContext.Out.WriteLine(string.Join("\n", results.Select(r => r.ToString())));
        Assert.That(backend.Mismatches, Is.Empty);
        Assert.That(results.Select(r => r.Law.Name), Is.EquivalentTo(new[]
            { "Law_AndGuardsTheIndex", "Law_OrGuardsTheIndex", "Law_AndStillTestsTheRight" }));
        Assert.That(results.Select(r => r.Status), Has.All.EqualTo(LawStatus.Pass));
    }
}
