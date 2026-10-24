// Checks interpretation and compilation of generic sum values.
using Ara3D.Geometry.Compiler.Symbols;
using Ara3D.Geometry.Compiler.Types;
using Ara3D.Geometry.Interpreter;
using Ara3D.Geometry.Laws;

namespace Plato.Interpreter.Tests;

/// <summary>
/// TKT-443: a generic sum, <c>Maybe&lt;T&gt;</c>, over a Number, an Integer, an index type (a
/// one-field record), a record, an array and itself: the interpreter matches it, the law runner
/// draws it for a law's parameter (a type argument with a <c>Generate_</c> function or a sealed
/// type argument is drawn by its own rule, TKT-446 and TKT-444; a sealed generic sum comes from its
/// constructor, carrying the instance it built), and the compiled C# agrees with the interpreter
/// law by law.
/// </summary>
[TestFixture]
public class GenericSumTests
{
    private const string Source = """
        type Maybe<T> = Just(Value: T) | Nothing;

        type Spot { X: Number; Y: Number; }
        type EdgeIndex { Value: Integer; }
        type Tol sealed by MakeTol { Eps: Number; }
        type Box<T> sealed by MakeBox = Full(Value: T) | Empty;

        library GenericSumFixture
        {
            ValueOr(m: Maybe<$T>, fallback: $T): $T =>
                match (m) {
                    Just(v) => v;
                    Nothing => fallback;
                };

            HasValue(m: Maybe<$T>): Boolean =>
                match (m) {
                    Just(v) => true;
                    Nothing => false;
                };

            HalfIfEven(n: Integer): Maybe<Integer> => (n % 2 == 0) ? Maybe.Just(n / 2) : Maybe.Nothing;
            FindEdge(n: Integer): Maybe<EdgeIndex> => (n > 0) ? Maybe.Just(EdgeIndex(n)) : Maybe.Nothing;
            MakeTol(x: Number): Tol => Tol(x * x + 1.0);
            Generate_EdgeIndex(seed: Integer): EdgeIndex => EdgeIndex(seed + 100);
            MakeBox(x: Number): Box<Number> => Box.Full(x * x);
            JustOf(n: Integer, v: $T): Maybe<$T> => Maybe.Just(v);
            Generate_MaybeSpot(seed: Integer): Maybe<Spot> => JustOf(seed, Spot(seed.ToNumber, 0.0));

            Law_Number(x: Number): Boolean => Maybe.Just(x).ValueOr(0.0) == x;
            Law_Integer(n: Integer): Boolean => HalfIfEven(n * 2).ValueOr(-1) == n;
            Law_Nothing(n: Integer): Boolean => HalfIfEven(n * 2 + 1).HasValue == false;
            Law_Index(n: Integer): Boolean => FindEdge(n).HasValue == (n > 0);
            Law_Record(s: Spot): Boolean => Maybe.Just(s).ValueOr(Spot(0.0, 0.0)).X == s.X;
            Law_Array(n: Integer): Boolean => Maybe.Just(n.MapRange(i => i)).ValueOr(0.MapRange(i => i)).Count == n.Max(0);
            Law_NumberParam(m: Maybe<Number>): Boolean => m.HasValue || m.ValueOr(3.0) == 3.0;
            Law_IndexParam(m: Maybe<EdgeIndex>): Boolean => m.ValueOr(EdgeIndex(200)).Value >= 100;
            Law_SealedParam(m: Maybe<Tol>): Boolean => m.ValueOr(MakeTol(1.0)).Eps >= 1.0;
            Law_ArrayParam(m: Maybe<Array<Number>>): Boolean => m.HasValue || m.ValueOr([1.0]).Count == 1;
            Law_NestedParam(m: Maybe<Maybe<Integer>>): Boolean => m.HasValue || m.ValueOr(Maybe.Just(3)).ValueOr(0) == 3;
            Law_SealedSum(b: Box<Number>): Boolean => match (b) { Full(v) => v >= 0.0; Empty => false; };
            Law_GeneratedThroughAGenericHelper(m: Maybe<Spot>): Boolean => m.HasValue;
        }
        """;

    private static readonly Lazy<Ara3D.Geometry.Interpreter.Interpreter> _interp = new(() =>
        new Ara3D.Geometry.Interpreter.Interpreter(LawRunnerSelectionTests.CompileSource(Source), NumberProfile.Cpu64));

    private static object Call(string name, params object[] args) => _interp.Value.Call(name, args);

    private static TypeExpression Type(string name, params TypeExpression[] args)
        => new(_interp.Value.Compilation.GetTypeDef(name), args);

    [Test]
    public void The_interpreter_matches_a_generic_sum_over_each_type_argument()
    {
        var half = (PlatoSum)Call("HalfIfEven", 4L);
        Assert.That(half.Case.Name, Is.EqualTo("Just"));
        Assert.That(half.Fields, Is.EqualTo(new object[] { 2L }));
        Assert.That(((PlatoSum)Call("HalfIfEven", 3L)).Case.Name, Is.EqualTo("Nothing"));
        Assert.That(Call("ValueOr", Call("HalfIfEven", 3L), -1L), Is.EqualTo(-1L));

        var edge = (PlatoRecord)Call("ValueOr", Call("FindEdge", 7L), new PlatoRecord(Type("EdgeIndex"), new object[] { 0L }));
        Assert.That(edge.Fields, Is.EqualTo(new object[] { 7L }));
        Assert.That(Call("HasValue", Call("FindEdge", -2L)), Is.EqualTo(false));
    }

    [Test]
    public void The_law_runner_draws_a_generic_sum_with_its_type_arguments_substituted()
    {
        var gen = new ValueGenerator(_interp.Value);
        foreach (var t in new[]
                 {
                     Type("Maybe", Type("Number")),
                     Type("Maybe", Type("EdgeIndex")),
                     Type("Maybe", Type("Array", Type("Number"))),
                     Type("Maybe", Type("Maybe", Type("Integer"))),
                 })
            Assert.That(gen.CanGenerate(t, out var reason), Is.True, $"{t}: {reason}");

        var rng = new Random(443);
        var arrays = Enumerable.Range(0, 40).Select(_ => (PlatoSum)gen.Generate(Type("Maybe", Type("Array", Type("Number"))), rng)).ToList();
        var payloads = arrays.Where(s => s.Case.Name == "Just").Select(s => s.Fields[0]).ToList();
        Assert.That(payloads, Is.Not.Empty.And.All.InstanceOf<PlatoArray>());
        Assert.That(arrays.Select(s => s.Case.Name).Distinct(), Is.EquivalentTo(new[] { "Just", "Nothing" }));
    }

    [Test]
    public void A_type_argument_keeps_its_own_generator_and_sealed_rule()
    {
        var gen = new ValueGenerator(_interp.Value);
        var rng = new Random(446);
        var edges = Enumerable.Range(0, 40).Select(_ => (PlatoSum)gen.Generate(Type("Maybe", Type("EdgeIndex")), rng))
            .Where(s => s.Case.Name == "Just").Select(s => (long)((PlatoRecord)s.Fields[0]).Fields[0]).ToList();
        Assert.That(edges, Is.Not.Empty.And.All.InRange(100L, 100L + ValueGenerator.GeneratorSeeds - 1));

        var tols = Enumerable.Range(0, 40).Select(_ => (PlatoSum)gen.Generate(Type("Maybe", Type("Tol")), rng))
            .Where(s => s.Case.Name == "Just").Select(s => (double)((PlatoRecord)s.Fields[0]).Fields[0]).ToList();
        Assert.That(tols, Is.Not.Empty.And.All.GreaterThanOrEqualTo(1.0), "a sealed Tol comes only from MakeTol");
    }

    [Test]
    public void Interpreter_and_compiled_csharp_agree_on_every_generic_sum_law()
    {
        var compilation = LawRunnerSelectionTests.CompileSource(Source);
        var interp = new Ara3D.Geometry.Interpreter.Interpreter(compilation, NumberProfile.Cpu64);
        var backend = new DifferentialBackend(new InterpreterBackend(interp), CSharpBackend.Build(interp));
        var runner = new LawRunner(backend, interp, new LawRunOptions { Filters = new[] { "GenericSumFixture" } });
        var results = runner.RunAll();
        TestContext.Out.WriteLine(string.Join("\n", results.Select(r => r.ToString())));
        Assert.That(backend.Mismatches, Is.Empty);
        Assert.That(results, Has.Count.EqualTo(13));
        Assert.That(results.Select(r => r.Status), Has.All.EqualTo(LawStatus.Pass));
    }
}
