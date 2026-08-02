using Ara3D.Collections;
using Ara3D.Geometry;
using NUnit.Framework;

namespace Plato.IntrinsicsTests;

/// <summary>
/// <see cref="GridExtensions"/> binds the Indexable2D/3D interface spellings
/// (<c>ColumnCount</c>/<c>RowCount</c>/<c>LayerCount</c>) to the collection interfaces'
/// <c>NumColumns</c>/<c>NumRows</c>/<c>NumLayers</c>. The whole contract is that the two
/// spellings agree.
/// </summary>
[TestFixture]
public static class GridExtensionsTests
{
    private static ReadOnlyList2D<int> Grid2D(int columns, int rows)
        => new(Enumerable.Range(0, columns * rows).ToList(), columns, rows);

    private static ReadOnlyList3D<int> Grid3D(int columns, int rows, int layers)
        => new(Enumerable.Range(0, columns * rows * layers).ToList(), columns, rows, layers);

    [TestCase(1, 1)]
    [TestCase(3, 2)]
    [TestCase(5, 7)]
    public static void TwoDimensionalExtentsAgreeWithTheCollectionSpelling(int columns, int rows)
    {
        var g = Grid2D(columns, rows);
        Assert.Multiple(() =>
        {
            Assert.That(g.ColumnCount(), Is.EqualTo(g.NumColumns));
            Assert.That(g.RowCount(), Is.EqualTo(g.NumRows));
            Assert.That(g.ColumnCount(), Is.EqualTo(columns));
            Assert.That(g.RowCount(), Is.EqualTo(rows));
            Assert.That(g.Count, Is.EqualTo(columns * rows));
        });
    }

    [TestCase(1, 1, 1)]
    [TestCase(2, 3, 4)]
    public static void ThreeDimensionalExtentsAgreeWithTheCollectionSpelling(
        int columns, int rows, int layers)
    {
        var g = Grid3D(columns, rows, layers);
        Assert.Multiple(() =>
        {
            Assert.That(g.ColumnCount(), Is.EqualTo(g.NumColumns));
            Assert.That(g.RowCount(), Is.EqualTo(g.NumRows));
            Assert.That(g.LayerCount(), Is.EqualTo(g.NumLayers));
            Assert.That(g.ColumnCount(), Is.EqualTo(columns));
            Assert.That(g.RowCount(), Is.EqualTo(rows));
            Assert.That(g.LayerCount(), Is.EqualTo(layers));
        });
    }
}
