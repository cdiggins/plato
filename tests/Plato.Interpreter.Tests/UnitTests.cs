// Exercises expression evaluation and runtime values in the interpreter.
using Ara3D.Geometry.Compiler.Types;
using Ara3D.Geometry.Interpreter;

namespace Plato.Interpreter.Tests;

/// <summary>One exact expected value per language feature, over fixtures/unit.plato compiled
/// with stdlib/foundation.</summary>
[TestFixture]
public class UnitTests
{
    private Ara3D.Geometry.Interpreter.Interpreter _interp;

    [OneTimeSetUp]
    public void SetUp() => _interp = TestSupport.UnitInterpreter();

    private object Call(string name, params object[] args) => _interp.Call(name, args);

    private static long[] Longs(object v)
        => ((PlatoArray)v).Items.Select(x => (long)x).ToArray();

    private static PlatoArray Ints(params long[] xs)
        => new(null, xs.Select(x => (object)x).ToArray());

    private static PlatoArray Nums(params double[] xs)
        => new(null, xs.Select(x => (object)x).ToArray());

    // --- products -------------------------------------------------------------

    [Test]
    public void Product_construct_and_read_fields()
    {
        var p = (PlatoRecord)Call("MakePair", 1.5, 2L);
        Assert.That(p.Def.Name, Is.EqualTo("UPair"));
        Assert.That(p.Fields, Is.EqualTo(new object[] { 1.5, 2L }));
        Assert.That(Call("PairA", p), Is.EqualTo(1.5));
        Assert.That(Call("PairB", p), Is.EqualTo(2L));
    }

    [Test]
    public void Tuple_expression_returned_as_struct()
    {
        var p = (PlatoRecord)Call("PairAsTuple", 4.0, 5L);
        Assert.That(p.Def.Name, Is.EqualTo("UPair"));
        Assert.That(p.Fields, Is.EqualTo(new object[] { 4.0, 5L }));
    }

    // --- sums ----------------------------------------------------------------

    [Test]
    public void Sum_qualified_construction_with_payload()
    {
        var s = (PlatoSum)Call("MakeCircle", 2.0);
        Assert.That(s.Case.Name, Is.EqualTo("Circle"));
        Assert.That(s.Fields, Is.EqualTo(new object[] { 2.0 }));
    }

    [Test]
    public void Sum_payload_free_case()
    {
        var s = (PlatoSum)Call("MakeEmpty");
        Assert.That(s.Case.Name, Is.EqualTo("Empty"));
        Assert.That(s.Fields, Is.Empty);
        Assert.That(Call("IsEmpty", s), Is.EqualTo(true));
        Assert.That(Call("IsEmpty", Call("MakeCircle", 1.0)), Is.EqualTo(false));
    }

    [Test]
    public void Match_with_binders()
    {
        Assert.That(Call("Area", Call("MakeCircle", 2.0)), Is.EqualTo(12.0));
        Assert.That(Call("Area", Call("MakeRect", 2.0, 3.5)), Is.EqualTo(7.0));
        Assert.That(Call("Area", Call("MakeEmpty")), Is.EqualTo(0.0));
    }

    [Test]
    public void Nested_match()
    {
        var c1 = Call("MakeCircle", 1.0);
        var c2 = Call("MakeCircle", 2.5);
        Assert.That(Call("RadiusSum", c1, c2), Is.EqualTo(3.5));
        Assert.That(Call("RadiusSum", c1, Call("MakeRect", 9.0, 9.0)), Is.EqualTo(1.0));
        Assert.That(Call("RadiusSum", Call("MakeRect", 4.0, 9.0), c1), Is.EqualTo(4.0));
    }

    // --- arrays --------------------------------------------------------------

    [Test]
    public void Array_literal() => Assert.That(Longs(Call("ArrayLiteral")), Is.EqualTo(new long[] { 1, 2, 3 }));

    [Test]
    public void Array_indexing() => Assert.That(Call("ArrayAt", 1L), Is.EqualTo(20L));

    [Test]
    public void MapRange() => Assert.That(Longs(Call("Squares", 4L)), Is.EqualTo(new long[] { 0, 1, 4, 9 }));

    [Test]
    public void Reduce() => Assert.That(Call("SumBelow", 5L), Is.EqualTo(10L));

    [Test]
    public void FlatMap() => Assert.That(Longs(Call("Duplicate", Ints(1, 2))), Is.EqualTo(new long[] { 1, 1, 2, 2 }));

    [Test]
    public void Map_from_stdlib_body() => Assert.That(Longs(Call("Doubled", Ints(1, 2, 3))), Is.EqualTo(new long[] { 2, 4, 6 }));

    [Test]
    public void All_from_stdlib_body()
    {
        Assert.That(Call("AllEven", Ints(2, 4)), Is.EqualTo(true));
        Assert.That(Call("AllEven", Ints(2, 3)), Is.EqualTo(false));
    }

    // --- lambdas, closures, blocks --------------------------------------------

    [Test]
    public void Closure_captures_var_local()
    {
        var r = ((PlatoArray)Call("ScaledBy", Nums(1.0, 2.5), 2.0)).Items;
        Assert.That(r, Is.EqualTo(new object[] { 4.0, 10.0 }));
    }

    [Test]
    public void Block_with_var_assignment_and_while() => Assert.That(Call("TriangleSum", 5L), Is.EqualTo(10L));

    [Test]
    public void Block_with_if()
    {
        Assert.That(Call("AbsByIf", -3.0), Is.EqualTo(3.0));
        Assert.That(Call("AbsByIf", 2.0), Is.EqualTo(2.0));
    }

    [Test]
    public void List_builder_freeze() => Assert.That(Longs(Call("ListTens", 3L)), Is.EqualTo(new long[] { 0, 10, 20 }));

    [Test]
    public void Buffer_builder_freeze() => Assert.That(Longs(Call("BufferCountdown", 3L)), Is.EqualTo(new long[] { 3, 2, 1 }));

    [Test]
    public void Buffer_slots_never_set_hold_the_default() => Assert.That(Call("BufferMarkedCount", 4L, 2L), Is.EqualTo(1L));

    // --- numbers --------------------------------------------------------------

    [Test]
    public void Integer_literal_into_Number_parameter() => Assert.That(Call("HalfOfThree"), Is.EqualTo(1.5));

    [Test]
    public void Integer_division_truncates()
    {
        Assert.That(Call("IntegerQuotient", 7L, 2L), Is.EqualTo(3L));
        Assert.That(Call("IntegerQuotient", -7L, 2L), Is.EqualTo(-3L));
    }

    [Test]
    public void Cpu64_sum_is_double() => Assert.That(Call("PointOnePlusPointTwo"), Is.EqualTo(0.1 + 0.2));

    [Test]
    public void Cpu64_integer_does_not_wrap() => Assert.That(Call("MultiplyIntegers", 65536L, 32768L), Is.EqualTo(2147483648L));

    [Test]
    public void Gpu32_sum_is_float()
    {
        var g = TestSupport.UnitInterpreter(NumberProfile.Gpu32);
        Assert.That(g.Call("PointOnePlusPointTwo"), Is.EqualTo((double)(0.1f + 0.2f)));
        Assert.That(g.Call("AddNumbers", 0.1, 0.2), Is.EqualTo((double)(float)(0.1 + 0.2)));
    }

    [Test]
    public void Gpu32_integer_wraps_at_2_pow_31()
    {
        var g = TestSupport.UnitInterpreter(NumberProfile.Gpu32);
        Assert.That(g.Call("MultiplyIntegers", 65536L, 32768L), Is.EqualTo((long)int.MinValue));
    }

    [Test]
    public void And_evaluates_both_operands()
        => Assert.That(Call("BothSides", true, false), Is.EqualTo(false));

    // --- generics and dispatch ------------------------------------------------

    [Test]
    public void Generic_function_with_lambda() => Assert.That(Call("PlusTwo", 5L), Is.EqualTo(7L));

    [Test]
    public void Interface_dispatch_through_generic_body()
        => Assert.That(Call("UnitIntervalContainsCenter"), Is.EqualTo(true));

    [Test]
    public void Generic_body_invoked_on_concrete_value()
    {
        var interval = Call("NumberInterval", 0.0, 10.0);
        Assert.That(Call("ContainsCenter", interval), Is.EqualTo(true));
    }

    [Test]
    public void Underscore_receiver_static() => Assert.That(Call("TwoPi"), Is.EqualTo(Math.PI * 2.0));

    [Test]
    public void User_witness_with_underscore_receiver()
    {
        Assert.That(Call("WitnessValue"), Is.EqualTo(7L));
        var type = TestSupport.FoundationWithUnitFixture.GetTypeDef("UCounter").ToTypeExpression();
        var w = (PlatoRecord)Call("Witness_UCounter", new TypeToken(type));
        Assert.That(w.Fields, Is.EqualTo(new object[] { 7L }));
    }

    // --- default, recursion, obligations -------------------------------------

    [Test]
    public void Default_value_of_product()
    {
        var p = (PlatoRecord)Call("DefaultPair");
        Assert.That(p.Def.Name, Is.EqualTo("UPair"));
        Assert.That(p.Fields, Is.EqualTo(new object[] { 0.0, 0L }));
    }

    [Test]
    public void Recursion() => Assert.That(Call("RecursiveFactorial", 10L), Is.EqualTo(3628800L));

    [Test]
    public void Bodiless_function_is_a_host_obligation()
    {
        var e = Assert.Throws<HostObligationException>(() => Call("CallsMystery", 1.0));
        Assert.That(e.Obligation, Does.Contain("Mystery"));
    }

    [Test]
    public void Invoke_by_definition()
    {
        var f = _interp.FunctionsNamed("RecursiveFactorial").Single();
        Assert.That(_interp.Invoke(f, 4L), Is.EqualTo(24L));
    }
}
