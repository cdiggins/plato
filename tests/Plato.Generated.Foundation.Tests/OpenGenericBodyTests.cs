using Ara3D.Geometry;
using NUnit.Framework;

namespace Plato.Generated.Foundation.Tests;

/// <summary>
/// Executes the emitted OPEN-GENERIC bodies — the ones that used to be throwing stubs because the
/// foundation tier contains no call site at a concrete type argument, so the monomorphizer never
/// stamped a ground instantiation (`DEGRADED ... not monomorphized`). These members are now emitted
/// once as ordinary open-generic C#; the point of the suite is that they RUN, not merely compile.
/// </summary>
[TestFixture]
public class OpenGenericBodyTests
{
    // cell (c, r) = r * 10 + c
    private static Array2D<Integer> Grid(int columns, int rows)
        => ((Integer)columns).MakeArray2D(rows, (c, r) => (Integer)(r.Value * 10 + c.Value));

    [Test]
    public void MakeArray2D_BuildsRowMajorGrid()
    {
        var g = Grid(3, 2);
        Assert.That(g.ColumnCount.Value, Is.EqualTo(3));
        Assert.That(g.RowCount.Value, Is.EqualTo(2));
        Assert.That(g.Count.Value, Is.EqualTo(6));
        Assert.That(g.At(0, 0).Value, Is.EqualTo(0));
        Assert.That(g.At(2, 0).Value, Is.EqualTo(2));
        Assert.That(g.At(1, 1).Value, Is.EqualTo(11));
        // Flat (row-major) index agrees with the (column, row) access.
        Assert.That(g.At((Integer)4).Value, Is.EqualTo(g.At(1, 1).Value));
        Assert.That(g.FlattenIndex(1, 1).Value, Is.EqualTo(4));
    }

    [Test]
    public void Array2D_Map_AppliesToEveryCellAndKeepsShape()
    {
        var mapped = Grid(3, 2).Map(x => (Integer)(x.Value * 2));
        Assert.That(mapped.ColumnCount.Value, Is.EqualTo(3));
        Assert.That(mapped.RowCount.Value, Is.EqualTo(2));
        Assert.That(mapped.At(1, 1).Value, Is.EqualTo(22));
        Assert.That(mapped.At(2, 0).Value, Is.EqualTo(4));
    }

    [Test]
    public void Array2D_Map_ChangesTheElementType()
    {
        // The open-generic result parameter is a genuinely different type here (Integer -> Number).
        var mapped = Grid(2, 2).Map(x => (Number)(x.Value + 0.5f));
        Assert.That(mapped.At(1, 1).Value, Is.EqualTo(11.5f).Within(1e-6f));
    }

    [Test]
    public void Array2D_RowAndColumnSlices()
    {
        var g = Grid(3, 2);
        var row = g.Row((Integer)1);
        Assert.That(row.Count, Is.EqualTo(3));
        Assert.That(row[0].Value, Is.EqualTo(10));
        Assert.That(row[2].Value, Is.EqualTo(12));

        var col = g.Column((Integer)2);
        Assert.That(col.Count, Is.EqualTo(2));
        Assert.That(col[0].Value, Is.EqualTo(2));
        Assert.That(col[1].Value, Is.EqualTo(12));
    }

    [Test]
    public void Array2D_ReduceAndPredicates()
    {
        var g = Grid(3, 2);
        // 0+1+2 + 10+11+12 = 36
        var sum = g.Reduce((Integer)0, (acc, x) => (Integer)(acc.Value + x.Value));
        Assert.That(sum.Value, Is.EqualTo(36));
        Assert.That(g.All(x => (Ara3D.Geometry.Boolean)(x.Value >= 0)).Value, Is.True);
        Assert.That(g.Any(x => (Ara3D.Geometry.Boolean)(x.Value == 12)).Value, Is.True);
        Assert.That(g.Any(x => (Ara3D.Geometry.Boolean)(x.Value == 99)).Value, Is.False);
        Assert.That(g.IsEmpty().Value, Is.False);
        Assert.That(g.IsValidCell(2, 1).Value, Is.True);
        Assert.That(g.IsValidCell(3, 1).Value, Is.False);
    }

    [Test]
    public void MakeArray3D_BuildsRowMajorVolume()
    {
        // cell (c, r, l) = l * 100 + r * 10 + c
        var v = ((Integer)2).MakeArray3D(3, 2, (c, r, l) => (Integer)(l.Value * 100 + r.Value * 10 + c.Value));
        Assert.That(v.Count.Value, Is.EqualTo(12));
        Assert.That(v.At(1, 2, 1).Value, Is.EqualTo(121));
        Assert.That(v.At(0, 0, 0).Value, Is.EqualTo(0));
        // row-major: (layer * RowCount + row) * ColumnCount + column = (1*3 + 2)*2 + 1
        Assert.That(v.FlattenIndex(1, 2, 1).Value, Is.EqualTo(11));
        Assert.That(v.At((Integer)11).Value, Is.EqualTo(121));
    }

    [Test]
    public void Repeat_FillsAnArray()
    {
        var xs = ((Integer)4).Repeat((Integer)7);
        Assert.That(xs.Count, Is.EqualTo(4));
        Assert.That(xs[0].Value, Is.EqualTo(7));
        Assert.That(xs[3].Value, Is.EqualTo(7));
    }

    [Test]
    public void VectorReceiver_ReduceAndMap()
    {
        var v = new Vector3D(1f, 2f, 3f);
        var sum = v.Reduce((Number)0f, (acc, x) => (Number)(acc.Value + x.Value));
        Assert.That(sum.Value, Is.EqualTo(6f).Within(1e-6f));
        var doubled = v.Map(x => (Number)(x.Value * 2f));
        Assert.That(doubled.Count, Is.EqualTo(3));
        Assert.That(doubled[2].Value, Is.EqualTo(6f).Within(1e-6f));
    }
}
