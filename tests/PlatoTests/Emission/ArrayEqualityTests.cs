// Checks that == and != on arrays emit element-wise comparison.
using System.Collections;
using Ara3D.Geometry;
using NUnit.Framework;

namespace PlatoTests.Emission;

using Boolean = Ara3D.Geometry.Boolean;
using Integer = Ara3D.Geometry.Integer;

/// <summary>
/// TKT-1040: IReadOnlyList's Equals is reference equality, so `a == b` on two separately built
/// arrays emitted as `a.Equals(b)` returned false in compiled C# where the interpreter returns true.
/// Plato arrays are values: the emitted comparison is Intrinsics.ArrayEquals, which compares the
/// count and then each element with the element's own equality, and a nested array element
/// element-wise in turn. That is the comparison a generated record already uses for an array field.
/// </summary>
[TestFixture]
public static class ArrayEqualityTests
{
    private static readonly Lazy<EmittedLibrary> Library = new(() => EmittedLibrary.Build(
        "ArrayEqualityFixtures", "equality.plato", name => name.StartsWith("Same") || name.StartsWith("Different"),
        new[] { "ArrayEqualityProbe.g.cs", "Extensions.g.cs" }, "_Tri.g.cs"));

    private static Integer[] Ints(params int[] xs) => xs.Select(x => (Integer)x).ToArray();

    // Each call builds its own arrays, so reference equality cannot hold.
    private static object Tris(params (int a, int b, int c)[] xs)
    {
        var tri = Library.Value.Assembly.GetType("Ara3D.Geometry.Tri", true)!;
        var array = Array.CreateInstance(tri, xs.Length);
        for (var i = 0; i < xs.Length; i++)
            array.SetValue(Activator.CreateInstance(tri, (Integer)xs[i].a, (Integer)xs[i].b, (Integer)xs[i].c), i);
        return array;
    }

    private static IReadOnlyList<IReadOnlyList<Integer>> Rows(params int[][] rows)
        => rows.Select(r => (IReadOnlyList<Integer>)Ints(r)).ToArray();

    private static bool Run(string name, params object[] args)
        => (bool)(Boolean)Library.Value.Method(name).Invoke(null, args)!;

    [Test]
    public static void IntegerArraysCompareByElements()
    {
        Assert.That(Run("SameInts", Ints(1, 2, 3), Ints(1, 2, 3)), Is.True);
        Assert.That(Run("SameInts", Ints(), Ints()), Is.True);
        Assert.That(Run("SameInts", Ints(1, 2, 3), Ints(1, 2, 4)), Is.False, "element differs");
        Assert.That(Run("SameInts", Ints(1, 2, 3), Ints(1, 2)), Is.False, "count differs");
        Assert.That(Run("SameInts", Ints(1, 2), Ints(1, 2, 3)), Is.False, "count differs");
        Assert.That(Run("DifferentInts", Ints(1, 2, 3), Ints(1, 2, 3)), Is.False);
        Assert.That(Run("DifferentInts", Ints(1, 2, 3), Ints(1, 2, 4)), Is.True);
        Assert.That(Run("DifferentInts", Ints(1), Ints()), Is.True);
    }

    [Test]
    public static void RecordArraysCompareByElements()
    {
        Assert.That(Run("SameTris", Tris((0, 1, 2), (2, 3, 0)), Tris((0, 1, 2), (2, 3, 0))), Is.True);
        Assert.That(Run("SameTris", Tris(), Tris()), Is.True);
        Assert.That(Run("SameTris", Tris((0, 1, 2), (2, 3, 0)), Tris((0, 1, 2), (2, 3, 1))), Is.False, "element differs");
        Assert.That(Run("SameTris", Tris((0, 1, 2), (2, 3, 0)), Tris((2, 3, 0), (0, 1, 2))), Is.False, "order differs");
        Assert.That(Run("SameTris", Tris((0, 1, 2)), Tris((0, 1, 2), (2, 3, 0))), Is.False, "count differs");
        Assert.That(Run("DifferentTris", Tris((0, 1, 2)), Tris((0, 1, 2))), Is.False);
        Assert.That(Run("DifferentTris", Tris((0, 1, 2)), Tris((0, 1, 3))), Is.True);
    }

    [Test]
    public static void NestedArraysCompareRowByRow()
    {
        Assert.That(Run("SameRows", Rows(new[] { 1 }, new[] { 2, 3 }), Rows(new[] { 1 }, new[] { 2, 3 })), Is.True);
        Assert.That(Run("SameRows", Rows(new[] { 1 }, new[] { 2, 3 }), Rows(new[] { 1 }, new[] { 2, 4 })), Is.False, "inner element differs");
        Assert.That(Run("SameRows", Rows(new[] { 1 }, new[] { 2, 3 }), Rows(new[] { 1, 2 }, new[] { 3 })), Is.False, "row split differs");
        Assert.That(Run("DifferentRows", Rows(new[] { 1 }), Rows(new[] { 1 })), Is.False);
        Assert.That(Run("DifferentRows", Rows(new[] { 1 }), Rows()), Is.True);
    }

    [Test]
    public static void AComparisonUnderAConditionKeepsItsMeaning()
    {
        Assert.That(Run("SameWhenCounted", (Integer)6, Ints(1, 2), Ints(1, 2)), Is.True);
        Assert.That(Run("SameWhenCounted", (Integer)6, Ints(1, 2), Ints(2, 1)), Is.False);
        Assert.That(Run("SameWhenCounted", (Integer)5, Ints(1, 2), Ints(1, 2)), Is.False);
    }

    [Test]
    public static void TheEmittedComparisonIsStructural()
        => Assert.That(Library.Value.Body("SameInts"), Does.Contain("Intrinsics.ArrayEquals").And.Not.Contain(".Equals("));
}
