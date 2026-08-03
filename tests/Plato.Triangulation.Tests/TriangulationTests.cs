using NUnit.Framework;

namespace Ara3D.Geometry.Tests;

/// <summary>
/// Executes the emitted polygon triangulator over inputs whose answer is known.
///
/// The assertion throughout is the one the mapbox/earcut suite uses: the emitted triangles
/// TILE the polygon. Their signed areas sum to the polygon's area — which catches both a
/// missing triangle and an overlapping one, since an overlap shows up as excess area — and
/// every face is counter-clockwise, which is the winding convention the library fixes.
///
/// Area is the assertion rather than face count because face count is legitimately variable:
/// a flat corner is clipped without emitting, so a ring with duplicate or collinear vertices
/// yields fewer than n - 2 faces. Where the count IS determined, it is asserted too.
///
/// Structural properties that hold for any input at all — faces indexing real points, corners
/// distinct — live in stdlib/tests/triangulation.laws.plato instead, so they run against every
/// generated instance once plato-308 clears.
/// </summary>
[TestFixture]
public class TriangulationTests
{
    private const double Tolerance = 1e-4;

    private static Point2D P(double x, double y) => new((Number)(float)x, (Number)(float)y);

    private static IReadOnlyList<Point2D> Ring(params (double X, double Y)[] points)
        => points.Select(p => P(p.X, p.Y)).ToList();

    private static Polygon2D Poly(params (double X, double Y)[] points) => new(Ring(points));

    /// <summary>Twice the signed area of one face, read straight off the mesh.</summary>
    private static double SignedArea(TriangleMesh2D mesh, TriangleFace f)
    {
        var a = mesh.Positions[f.A.Value];
        var b = mesh.Positions[f.B.Value];
        var c = mesh.Positions[f.C.Value];
        return 0.5 * (((float)b.X - (float)a.X) * ((float)c.Y - (float)a.Y)
                    - ((float)b.Y - (float)a.Y) * ((float)c.X - (float)a.X));
    }

    /// <summary>
    /// Asserts the mesh tiles a region of <paramref name="expectedArea"/>: areas sum to it, and
    /// no face is clockwise or degenerate.
    /// </summary>
    private static void AssertTiles(TriangleMesh2D mesh, double expectedArea, string what)
    {
        double total = 0;
        for (var i = 0; i < mesh.Faces.Count; i++)
        {
            var area = SignedArea(mesh, mesh.Faces[i]);
            Assert.That(area, Is.GreaterThan(0),
                $"{what}: face {i} is clockwise or degenerate (area {area})");
            total += area;
        }

        Assert.That(total, Is.EqualTo(expectedArea).Within(Tolerance),
            $"{what}: triangles do not tile the polygon "
            + $"({mesh.Faces.Count} faces covering {total}, expected {expectedArea})");
    }

    // ---- simple polygons --------------------------------------------------

    [Test]
    public void SquareCounterClockwise()
    {
        var mesh = Poly((0, 0), (1, 0), (1, 1), (0, 1)).Triangulate();
        Assert.That(mesh.Faces.Count, Is.EqualTo(2));
        AssertTiles(mesh, 1.0, "unit square");
    }

    [Test]
    public void SquareClockwiseIsRewound()
    {
        // Stored the wrong way round: the library rewinds each ring, so the result is identical
        // to the counter-clockwise case rather than inverted.
        var mesh = Poly((0, 1), (1, 1), (1, 0), (0, 0)).Triangulate();
        Assert.That(mesh.Faces.Count, Is.EqualTo(2));
        AssertTiles(mesh, 1.0, "clockwise unit square");
    }

    [Test]
    public void ConcaveShapeIsNotFanned()
    {
        // An L. A fan from vertex 0 would cover ground outside the polygon, so this fails loudly
        // if the ear test is ever weakened to "any convex corner".
        var mesh = Poly((0, 0), (2, 0), (2, 1), (1, 1), (1, 2), (0, 2)).Triangulate();
        Assert.That(mesh.Faces.Count, Is.EqualTo(4));
        AssertTiles(mesh, 3.0, "L-shape");
    }

    [Test]
    public void FivePointedStar()
    {
        // Ten vertices, five of them reflex — the classic ear-clipping stress shape.
        var points = new List<(double, double)>();
        for (var i = 0; i < 10; i++)
        {
            var radius = i % 2 == 0 ? 1.0 : 0.4;
            var angle = Math.PI / 2 + i * Math.PI / 5;
            points.Add((radius * Math.Cos(angle), radius * Math.Sin(angle)));
        }

        var mesh = new Polygon2D(Ring(points.ToArray())).Triangulate();
        Assert.That(mesh.Faces.Count, Is.EqualTo(8));
        AssertTiles(mesh, 5 * 1.0 * 0.4 * Math.Sin(Math.PI / 5), "five-pointed star");
    }

    [Test]
    public void DegenerateCornersAreFiltered()
    {
        // A duplicated vertex and a collinear midpoint. Both are dropped before clipping, so the
        // face count falls below n - 2 while the area is untouched.
        var mesh = Poly((0, 0), (1, 0), (1, 0), (2, 0), (2, 2), (0, 2)).Triangulate();
        Assert.That(mesh.Faces.Count, Is.LessThan(4));
        AssertTiles(mesh, 4.0, "ring with duplicate and collinear vertices");
    }

    // ---- the convex fan path ----------------------------------------------

    [Test]
    public void ConvexPolygonIsFanned()
    {
        var points = new List<(double, double)>();
        for (var i = 0; i < 6; i++)
            points.Add((Math.Cos(i * Math.PI / 3), Math.Sin(i * Math.PI / 3)));

        var mesh = new ConvexPolygon2D(Ring(points.ToArray())).Triangulate();
        Assert.That(mesh.Faces.Count, Is.EqualTo(4), "a convex n-gon fans into exactly n - 2");
        AssertTiles(mesh, 6 * 0.5 * Math.Sin(Math.PI / 3), "convex hexagon");
    }

    // ---- holes ------------------------------------------------------------

    private static readonly Polygon2D Outer = Poly((0, 0), (4, 0), (4, 4), (0, 4));

    [Test]
    public void SquareWithOneHole()
    {
        var hole = Poly((1, 1), (1, 3), (3, 3), (3, 1));            // clockwise, as declared
        var mesh = new PolygonWithHoles2D(Outer, new[] { hole }).Triangulate();
        AssertTiles(mesh, 16.0 - 4.0, "square with one hole");
    }

    [Test]
    public void HoleStoredCounterClockwiseIsRewound()
    {
        var hole = Poly((1, 1), (3, 1), (3, 3), (1, 3));            // wrong winding
        var mesh = new PolygonWithHoles2D(Outer, new[] { hole }).Triangulate();
        AssertTiles(mesh, 16.0 - 4.0, "hole stored counter-clockwise");
    }

    [Test]
    public void SquareWithTwoHoles()
    {
        // The case that exposed the unsound ear cache (plato-417): it came back with 20 units of
        // area over a region of 14, because stale blocker counts sent the clipper down its
        // degenerate-input path and it emitted overlapping triangles.
        var a = Poly((0.5, 0.5), (0.5, 1.5), (1.5, 1.5), (1.5, 0.5));
        var b = Poly((2.5, 2.5), (2.5, 3.5), (3.5, 3.5), (3.5, 2.5));
        var mesh = new PolygonWithHoles2D(Outer, new[] { a, b }).Triangulate();
        AssertTiles(mesh, 16.0 - 2.0, "square with two holes");
    }

    [Test]
    public void SquareWithThreeHoles()
    {
        var a = Poly((0.5, 0.5), (0.5, 1.5), (1.5, 1.5), (1.5, 0.5));
        var b = Poly((2.5, 2.5), (2.5, 3.5), (3.5, 3.5), (3.5, 2.5));
        var c = Poly((0.5, 2.5), (0.5, 3.5), (1.5, 3.5), (1.5, 2.5));
        var mesh = new PolygonWithHoles2D(Outer, new[] { a, b, c }).Triangulate();
        AssertTiles(mesh, 16.0 - 3.0, "square with three holes");
    }

    [Test]
    public void HoleInsideAConcavity()
    {
        // A C-shape whose notch is [0,3] x [1,3]; the hole sits in the remaining right column, so
        // the bridge has to leave the boundary at a vertex the notch does not hide.
        var shape = Poly((0, 0), (4, 0), (4, 4), (0, 4), (0, 3), (3, 3), (3, 1), (0, 1));
        var hole = Poly((3.2, 1.4), (3.2, 2.6), (3.8, 2.6), (3.8, 1.4));
        var mesh = new PolygonWithHoles2D(shape, new[] { hole }).Triangulate();
        AssertTiles(mesh, 16.0 - 6.0 - 0.72, "hole inside a concavity");
    }

    // ---- randomized ------------------------------------------------------

    [Test]
    public void RandomStarShapedPolygonsTile()
    {
        // Star-shaped polygons are the earcut suite's fuzz case: sampling a radius per angle
        // guarantees a simple ring, so the expected area is exactly the shoelace value and any
        // disagreement is the triangulator's fault rather than the input's. Seeded, so a failure
        // reproduces.
        // System-qualified: the generated namespace has its own `Random` (the probability
        // distribution library), which wins over the BCL type inside Ara3D.Geometry.
        var random = new System.Random(20260803);

        for (var trial = 0; trial < 500; trial++)
        {
            var n = 3 + random.Next(38);
            var points = new List<(double X, double Y)>();
            for (var i = 0; i < n; i++)
            {
                var radius = 0.2 + random.NextDouble();
                var angle = 2 * Math.PI * i / n;
                points.Add((radius * Math.Cos(angle), radius * Math.Sin(angle)));
            }

            if (random.Next(2) == 0) points.Reverse();   // exercise both windings

            double shoelace = 0;
            for (var i = 0; i < n; i++)
            {
                var (x0, y0) = points[i];
                var (x1, y1) = points[(i + 1) % n];
                shoelace += x0 * y1 - x1 * y0;
            }

            var mesh = new Polygon2D(Ring(points.ToArray())).Triangulate();

            double total = 0;
            for (var i = 0; i < mesh.Faces.Count; i++)
            {
                var area = SignedArea(mesh, mesh.Faces[i]);
                Assert.That(area, Is.GreaterThan(0),
                    $"trial {trial} (n={n}, seed 20260803): face {i} is clockwise or degenerate");
                total += area;
            }

            var expected = Math.Abs(shoelace) / 2;
            Assert.That(total, Is.EqualTo(expected).Within(1e-3 * Math.Max(1, expected)),
                $"trial {trial} (n={n}, seed 20260803): triangles do not tile the polygon");
        }
    }

    // ---- input that breaks the declared invariants -------------------------

    [Test]
    public void DegenerateInputTerminatesWithoutThrowing()
    {
        // None of these is a legal Polygon2D. The contract is only that the clipper terminates
        // and returns something — a ring that cannot enclose area returns no faces at all. This
        // is the regression test for the guard that has to stay nested, because `&&` is `And`
        // and does not short-circuit (docs/SEMANTICS.md section 6).
        Assert.Multiple(() =>
        {
            Assert.That(Poly((0, 0), (1, 1), (2, 2)).Triangulate().Faces.Count, Is.Zero,
                "three collinear points enclose nothing");
            Assert.That(Poly((0, 0), (1, 0)).Triangulate().Faces.Count, Is.Zero,
                "two points are not a ring");
            Assert.That(Poly((0, 0), (0, 0), (0, 0), (0, 0)).Triangulate().Faces.Count, Is.Zero,
                "one point repeated is not a ring");
            Assert.DoesNotThrow(() => Poly((0, 0), (2, 2), (2, 0), (0, 2)).Triangulate(),
                "a self-intersecting bowtie must degrade, not throw");
        });
    }
}
