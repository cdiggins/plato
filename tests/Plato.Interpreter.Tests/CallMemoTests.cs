// Checks interpreter call memoization and its value keys.
using Ara3D.Geometry.Compiler.Types;
using Ara3D.Geometry.Interpreter;

namespace Plato.Interpreter.Tests;

/// <summary>The memo of expensive calls (CallMemo) answers exactly as evaluation does: equal
/// values with and without it, distinct keys for values Plato's equality conflates, and no
/// memo over the mutable builders.</summary>
[TestFixture]
public class CallMemoTests
{
    /// <summary>An interpreter that remembers every call, however cheap.</summary>
    private static Ara3D.Geometry.Interpreter.Interpreter Remembering()
    {
        var interp = TestSupport.UnitInterpreter();
        interp.Memo = new CallMemo { ThresholdMicroseconds = 0 };
        return interp;
    }

    [Test]
    public void A_repeated_call_is_answered_from_the_memo()
    {
        var interp = Remembering();
        Assert.That(interp.Call("TriangleSum", 5L), Is.EqualTo(10L));
        var hits = interp.Memo.Hits;
        Assert.That(interp.Call("TriangleSum", 5L), Is.EqualTo(10L));
        Assert.That(interp.Memo.Hits, Is.GreaterThan(hits));
    }

    [Test]
    public void Remembered_answers_equal_evaluated_ones()
    {
        var remembering = Remembering();
        var plain = TestSupport.UnitInterpreter();
        plain.Memo = null;
        foreach (var n in new[] { 0L, 1L, 3L, 7L, 3L, 0L })
        {
            Assert.That(remembering.Call("TriangleSum", n), Is.EqualTo(plain.Call("TriangleSum", n)));
            Assert.That(((PlatoArray)remembering.Call("BufferCountdown", n)).Items,
                Is.EqualTo(((PlatoArray)plain.Call("BufferCountdown", n)).Items));
        }
    }

    [Test]
    public void Zero_and_negative_zero_are_different_keys()
    {
        var interp = Remembering();
        Assert.That(interp.Call("Reciprocal", 0.0), Is.EqualTo(double.PositiveInfinity));
        Assert.That(interp.Call("Reciprocal", -0.0), Is.EqualTo(double.NegativeInfinity));
    }

    [Test]
    public void A_builder_is_never_a_key_or_a_remembered_result()
    {
        var builder = new PlatoBuilder(null, false, 0);
        Assert.That(MemoValue.KeyOf(new object[] { 1L, builder }), Is.Null);
        Assert.That(MemoValue.TryHash(new PlatoArray(null, new object[] { builder }), out _), Is.False);
        Assert.That(MemoValue.KeyOf(new object[] { 1L, 2.0, "s" }), Is.Not.Null);
    }

    [Test]
    public void Arrays_of_different_element_types_are_different_keys()
    {
        var interp = TestSupport.UnitInterpreter();
        var ints = new PlatoArray(interp.PrimitiveType("Integer"), Array.Empty<object>());
        var numbers = new PlatoArray(interp.PrimitiveType("Number"), Array.Empty<object>());
        Assert.That(ValueEquality.Equals(ints, numbers), Is.True);
        Assert.That(MemoValue.Equals(ints, numbers), Is.False);
    }

    [Test]
    public void Structurally_equal_records_are_one_key()
    {
        var interp = TestSupport.UnitInterpreter();
        interp.Memo = null;
        var a = interp.Call("MakePair", 1.5, 2L);
        var b = interp.Call("MakePair", 1.5, 2L);
        Assert.That(ReferenceEquals(a, b), Is.False);
        Assert.That(MemoValue.KeyComparer.Instance.Equals(new[] { a }, new[] { b }), Is.True);
        Assert.That(MemoValue.KeyComparer.Instance.GetHashCode(new[] { a }),
            Is.EqualTo(MemoValue.KeyComparer.Instance.GetHashCode(new[] { b })));
    }
}
