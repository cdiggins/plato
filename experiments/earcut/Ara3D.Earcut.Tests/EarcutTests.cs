namespace Ara3D.Geometry;

/// <summary>
/// Tests for the Plato earcut port (earcut.plato). The central check is the one the
/// original earcut suite uses: the triangles must exactly tile the polygon, so the sum
/// of their areas equals the polygon's area (outer ring minus holes), every triangle
/// is counter-clockwise with distinct vertex indices, and the count never exceeds n - 2.
/// </summary>
[TestFixture]
public static class EarcutTests
{
    public static Point2D P(float x, float y)
        => new(x, y);

    public static PolygonWithHoles Poly(Point2D[] points, params int[] holeStarts)
        => new(points, holeStarts);

    public static float SignedRingArea(IReadOnlyList<Point2D> pts, int from, int to)
    {
        var sum = 0f;
        for (var i = from; i < to; i++)
        {
            var a = pts[i];
            var b = pts[i + 1 < to ? i + 1 : from];
            sum += a.X * b.Y - b.X * a.Y;
        }
        return sum / 2;
    }

    /// <summary>Area enclosed by the polygon: |outer| minus the |hole|s, winding-independent.</summary>
    public static float ExpectedArea(PolygonWithHoles p)
    {
        var area = 0f;
        for (var r = 0; r <= p.HoleStarts.Count; r++)
        {
            var from = r == 0 ? 0 : p.HoleStarts[r - 1];
            var to = r == p.HoleStarts.Count ? p.Points.Count : p.HoleStarts[r];
            area += (r == 0 ? 1 : -1) * Math.Abs(SignedRingArea(p.Points, from, to));
        }
        return area;
    }

    public static float SignedArea(PolygonWithHoles p, Integer3 t)
    {
        var (a, b, c) = (p.Points[t.A], p.Points[t.B], p.Points[t.C]);
        return ((b.X - a.X) * (c.Y - a.Y) - (b.Y - a.Y) * (c.X - a.X)) / 2;
    }

    public static IReadOnlyList<Integer3> AssertTriangulates(PolygonWithHoles p, int? expectedCount = null)
    {
        var tris = p.Triangulate();
        var totalArea = 0f;
        foreach (var t in tris)
        {
            Assert.That(new[] { t.A, t.B, t.C }, Is.All.InRange(0, p.Points.Count - 1));
            Assert.That(new[] { t.A, t.B, t.C }, Is.Unique, "degenerate triangle");
            var area = SignedArea(p, t);
            Assert.That(area, Is.GreaterThan(0f), $"triangle ({t.A},{t.B},{t.C}) is not counter-clockwise");
            totalArea += area;
        }
        var expected = ExpectedArea(p);
        var tolerance = 1e-4f * Math.Max(1f, expected);
        Assert.That(totalArea, Is.EqualTo(expected).Within(tolerance), "triangles do not tile the polygon");
        Assert.That(tris.Count, Is.LessThanOrEqualTo(Math.Max(0, p.Points.Count + 2 * p.HoleStarts.Count - 2)));
        if (expectedCount != null)
            Assert.That(tris.Count, Is.EqualTo(expectedCount));
        return tris;
    }

    public static Point2D[] Square(float size = 1, float x = 0, float y = 0)
        => new[] { P(x, y), P(x + size, y), P(x + size, y + size), P(x, y + size) };

    [Test]
    public static void Triangle()
    {
        var tris = AssertTriangulates(Poly(new[] { P(0, 0), P(1, 0), P(0, 1) }), 1);
        Assert.That((tris[0].A, tris[0].B, tris[0].C), Is.EqualTo((2, 0, 1)));
    }

    [Test]
    public static void SquareYieldsTwoTriangles()
        => AssertTriangulates(Poly(Square()), 2);

    [Test]
    public static void WindingIsNormalized()
        => AssertTriangulates(Poly(Square().Reverse().ToArray()), 2);

    [Test]
    public static void ConvexPolygon()
    {
        var octagon = Enumerable.Range(0, 8)
            .Select(i => P(MathF.Cos(i * MathF.Tau / 8), MathF.Sin(i * MathF.Tau / 8)))
            .ToArray();
        AssertTriangulates(Poly(octagon), 6);
    }

    [Test]
    public static void ConcavePolygon()
        => AssertTriangulates(
            Poly(new[] { P(0, 0), P(4, 0), P(4, 4), P(2, 4), P(2, 2), P(0, 2) }), 4);

    [Test]
    public static void StarPolygon()
    {
        var star = Enumerable.Range(0, 20)
            .Select(i => (angle: i * MathF.Tau / 20, radius: i % 2 == 0 ? 2f : 0.5f))
            .Select(v => P(v.radius * MathF.Cos(v.angle), v.radius * MathF.Sin(v.angle)))
            .ToArray();
        AssertTriangulates(Poly(star), 18);
    }

    [Test]
    public static void SquareWithSquareHole()
        => AssertTriangulates(Poly(Square(3).Concat(Square(1, 1, 1)).ToArray(), 4), 8);

    [Test]
    public static void HoleWindingIsNormalized()
        => AssertTriangulates(
            Poly(Square(3).Concat(Square(1, 1, 1).Reverse()).ToArray(), 4), 8);

    [Test]
    public static void TwoHoles()
        => AssertTriangulates(
            Poly(Square(7).Concat(Square(1, 1, 1)).Concat(Square(1, 4, 4)).ToArray(), 4, 8));

    [Test]
    public static void HoleTouchingConcavity()
        => AssertTriangulates(
            Poly(new[] { P(0, 0), P(6, 0), P(6, 6), P(3, 5), P(0, 6) }
                .Concat(Square(1, 1, 1)).ToArray(), 5));

    [Test]
    public static void CollinearVerticesAreSkipped()
    {
        // A square with a redundant midpoint on each edge: the zero-area corners
        // must not become triangles, and the area must survive.
        var square = new[]
        {
            P(0, 0), P(1, 0), P(2, 0), P(2, 1), P(2, 2), P(1, 2), P(0, 2), P(0, 1),
        };
        AssertTriangulates(Poly(square));
    }

    [Test]
    public static void FewerThanThreePointsYieldsNothing()
    {
        Assert.That(Poly(Array.Empty<Point2D>()).Triangulate(), Is.Empty);
        Assert.That(Poly(new[] { P(0, 0) }).Triangulate(), Is.Empty);
        Assert.That(Poly(new[] { P(0, 0), P(1, 1) }).Triangulate(), Is.Empty);
    }

    [Test]
    public static void ZeroAreaPolygonYieldsNothing()
        => Assert.That(Poly(new[] { P(0, 0), P(1, 0), P(2, 0) }).Triangulate(), Is.Empty);

    [Test]
    public static void FlatCoordinateApi()
    {
        // The quad from the earcut README: earcut([10,0, 0,50, 60,60, 70,10]).
        // Either diagonal is a correct triangulation, so assert structure, not
        // earcut's exact ear order.
        var coords = new float[] { 10, 0, 0, 50, 60, 60, 70, 10 };
        var tris = coords.Triangulate(Array.Empty<int>());
        AssertTriangulates(new PolygonWithHoles(coords.MapPairs((x, y) => new Point2D(x, y)), Array.Empty<int>()), 2);
        Assert.That(tris.Count, Is.EqualTo(2));
    }

    [Test]
    public static void FlatCoordinateApiWithHole()
    {
        // The example from the earcut README: a square with a square hole.
        var coords = new float[] { 0, 0, 100, 0, 100, 100, 0, 100, 20, 20, 80, 20, 80, 80, 20, 80 };
        var tris = coords.Triangulate(new[] { 4 });
        AssertTriangulates(new PolygonWithHoles(coords.MapPairs((x, y) => new Point2D(x, y)), new[] { 4 }), 8);
        Assert.That(tris.Count, Is.EqualTo(8));
    }

    /// <summary>
    /// For inputs outside the algorithm's contract (self-intersecting or zero-area
    /// rings) the guarantee is weaker, matching earcut: terminate and emit only
    /// well-formed triangles. No claim is made about which region gets covered.
    /// </summary>
    public static void AssertDegradesGracefully(PolygonWithHoles p)
    {
        var tris = p.Triangulate();
        foreach (var t in tris)
        {
            Assert.That(new[] { t.A, t.B, t.C }, Is.All.InRange(0, p.Points.Count - 1));
            Assert.That(SignedArea(p, t), Is.GreaterThan(0f));
        }
        Assert.That(tris.Count, Is.LessThanOrEqualTo(Math.Max(0, p.Points.Count + 2 * p.HoleStarts.Count - 2)));
    }

    [Test]
    public static void DuplicateConsecutiveVertices()
        => AssertTriangulates(Poly(new[] { P(0, 0), P(1, 0), P(1, 0), P(1, 1), P(0, 1) }));

    [Test]
    public static void ZeroWidthSpike()
        => AssertTriangulates(
            Poly(new[] { P(0, 0), P(2, 0), P(2, 2), P(1, 2), P(1, 3), P(1, 2), P(0, 2) }));

    [Test]
    public static void BowtieDegradesGracefully()
        => AssertDegradesGracefully(Poly(new[] { P(0, 0), P(2, 2), P(2, 0), P(0, 2) }));

    [Test]
    public static void SelfIntersectingQuadDegradesGracefully()
        => AssertDegradesGracefully(Poly(new[] { P(0, 0), P(4, 0), P(1, 2), P(3, 2) }));

    [Test]
    public static void RandomStarShapedPolygons()
    {
        // Deterministic pseudo-random radial polygons (always simple by construction).
        var rng = new Random(12345);
        for (var trial = 0; trial < 10; trial++)
        {
            var n = 5 + rng.Next(30);
            var points = Enumerable.Range(0, n)
                .Select(i => (angle: i * MathF.Tau / n, radius: 1f + 4f * (float)rng.NextDouble()))
                .Select(v => P(v.radius * MathF.Cos(v.angle), v.radius * MathF.Sin(v.angle)))
                .ToArray();
            AssertTriangulates(Poly(points), n - 2);
        }
    }
}

