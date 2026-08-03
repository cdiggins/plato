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

    // ---- multi-component sets ---------------------------------------------

    private static PolygonWithHoles2D Region(Polygon2D boundary, params Polygon2D[] holes)
        => new(boundary, holes);

    [Test]
    public void PolygonSetTilesEveryComponent()
    {
        // Two disjoint squares, the second with a hole. Faces index one shared pool, so this
        // fails if a component's indices are not shifted past the earlier components'.
        var left = Poly((0, 0), (1, 0), (1, 1), (0, 1));
        var right = Poly((3, 0), (5, 0), (5, 2), (3, 2));
        var rightHole = Poly((3.5, 0.5), (3.5, 1.5), (4.5, 1.5), (4.5, 0.5));

        var set = new PolygonSet2D(new[] { Region(left), Region(right, rightHole) });
        var mesh = set.Triangulate();

        Assert.That(mesh.Positions.Count, Is.EqualTo(4 + 4 + 4),
            "the pool is every component's vertices, once each");
        AssertTiles(mesh, 1.0 + (4.0 - 1.0), "two-component set, one component holed");
    }

    [Test]
    public void EmptyPolygonSetYieldsEmptyMesh()
    {
        // Boolean operations produce empty results routinely, so this is an answer rather than
        // a precondition violation.
        var mesh = new PolygonSet2D(Array.Empty<PolygonWithHoles2D>()).Triangulate();
        Assert.Multiple(() =>
        {
            Assert.That(mesh.Faces.Count, Is.Zero);
            Assert.That(mesh.Positions.Count, Is.Zero);
        });
    }

    // ---- planar polygons in space ------------------------------------------

    [Test]
    public void PlanarPolygonInSpaceTilesAndFacesTheNormal()
    {
        // A unit square on the plane z = x, tilted 45 degrees, wound so its right-hand normal
        // points up-and-back. Area is sqrt(2); each face must agree with that normal.
        var polygon = new Polygon3D(new List<Point3D>
        {
            new((Number)0f, (Number)0f, (Number)0f),
            new((Number)1f, (Number)0f, (Number)1f),
            new((Number)1f, (Number)1f, (Number)1f),
            new((Number)0f, (Number)1f, (Number)0f),
        });

        var mesh = polygon.ToTriangleMesh();
        var normal = polygon.Normal().Vector;      // Direction3D wraps a Vector3D

        Assert.That(mesh.Faces.Count, Is.EqualTo(2));
        Assert.That(mesh.Positions.Count, Is.EqualTo(4),
            "the projection is index-preserving, so faces index the original space points");

        double total = 0;
        for (var i = 0; i < mesh.Faces.Count; i++)
        {
            var f = mesh.Faces[i];
            var a = mesh.Positions[f.A.Value];
            var b = mesh.Positions[f.B.Value];
            var c = mesh.Positions[f.C.Value];

            // Twice the face's vector area, by hand, so the assertion does not lean on the
            // library it is testing.
            var (ux, uy, uz) = ((float)b.X - (float)a.X, (float)b.Y - (float)a.Y, (float)b.Z - (float)a.Z);
            var (vx, vy, vz) = ((float)c.X - (float)a.X, (float)c.Y - (float)a.Y, (float)c.Z - (float)a.Z);
            var (nx, ny, nz) = (uy * vz - uz * vy, uz * vx - ux * vz, ux * vy - uy * vx);

            var alignment = nx * (float)normal.X + ny * (float)normal.Y + nz * (float)normal.Z;
            Assert.That(alignment, Is.GreaterThan(0),
                $"face {i} is wound against the polygon's normal");

            total += 0.5 * Math.Sqrt(nx * nx + ny * ny + nz * nz);
        }

        Assert.That(total, Is.EqualTo(Math.Sqrt(2)).Within(Tolerance),
            "the faces cover the polygon's area");
    }

    [Test]
    public void ConcavePolygonInSpaceIsNotFanned()
    {
        // The L again, lifted onto the plane y = 0 and wound so its normal is +Y. A fan would
        // cover ground outside the polygon; the area check catches it.
        (double X, double Z)[] flat = [(0, 0), (2, 0), (2, 1), (1, 1), (1, 2), (0, 2)];
        var polygon = new Polygon3D(flat
            .Select(p => new Point3D((Number)(float)p.X, (Number)0f, (Number)(float)p.Z))
            .ToList());

        var mesh = polygon.ToTriangleMesh();
        Assert.That(mesh.Faces.Count, Is.EqualTo(4));

        double total = 0;
        for (var i = 0; i < mesh.Faces.Count; i++)
        {
            var f = mesh.Faces[i];
            var a = mesh.Positions[f.A.Value];
            var b = mesh.Positions[f.B.Value];
            var c = mesh.Positions[f.C.Value];
            total += Math.Abs(0.5 * (((float)b.X - (float)a.X) * ((float)c.Z - (float)a.Z)
                                   - ((float)b.Z - (float)a.Z) * ((float)c.X - (float)a.X)));
        }

        Assert.That(total, Is.EqualTo(3.0).Within(Tolerance));
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

    // ---- predicates and repair ---------------------------------------------

    [Test]
    public void SimpleRingsAreRecognised()
    {
        Assert.Multiple(() =>
        {
            Assert.That((bool)Poly((0, 0), (1, 0), (1, 1), (0, 1)).IsSimple(), Is.True,
                "a square");
            Assert.That((bool)Poly((0, 1), (1, 1), (1, 0), (0, 0)).IsSimple(), Is.True,
                "winding does not affect simplicity");
            Assert.That((bool)Poly((0, 0), (2, 0), (2, 1), (1, 1), (1, 2), (0, 2)).IsSimple(),
                Is.True, "a concave L is still simple");
            Assert.That((bool)Poly((0, 0), (1, 0), (0.5, 1)).IsSimple(), Is.True, "a triangle");
        });
    }

    [Test]
    public void NonSimpleRingsAreRejected()
    {
        Assert.Multiple(() =>
        {
            Assert.That((bool)Poly((0, 0), (2, 2), (2, 0), (0, 2)).IsSimple(), Is.False,
                "a bowtie crosses itself");
            Assert.That((bool)Poly((0, 0), (1, 0)).IsSimple(), Is.False,
                "two vertices are not a polygon");
            Assert.That((bool)Poly((0, 0), (1, 1), (2, 2)).IsSimple(), Is.False,
                "collinear vertices enclose nothing");
            Assert.That((bool)Poly((0, 0), (1, 0), (1, 0), (1, 1)).IsSimple(), Is.False,
                "a repeated vertex is a degenerate edge");
            Assert.That((bool)Poly((0, 0), (2, 0), (1, 0), (1, 1)).IsSimple(), Is.False,
                "a spike doubles an edge back over itself");
        });
    }

    [Test]
    public void SelfIntersectionCountLocatesEveryBadPair()
    {
        Assert.Multiple(() =>
        {
            Assert.That((int)Poly((0, 0), (1, 0), (1, 1), (0, 1)).SelfIntersectionCount(),
                Is.Zero);
            Assert.That((int)Poly((0, 0), (2, 2), (2, 0), (0, 2)).SelfIntersectionCount(),
                Is.EqualTo(1), "the bowtie has exactly one crossing pair");
        });
    }

    [Test]
    public void WindingIsReadAndCorrected()
    {
        var ccw = Poly((0, 0), (1, 0), (1, 1), (0, 1));
        var cw = Poly((0, 1), (1, 1), (1, 0), (0, 0));

        Assert.Multiple(() =>
        {
            Assert.That((bool)ccw.Winding().IsCounterClockwise(), Is.True);
            Assert.That((bool)cw.Winding().IsClockwise(), Is.True);
            Assert.That((bool)cw.EnsureCounterClockwise().Winding().IsCounterClockwise(), Is.True);
            Assert.That((float)ccw.EnsureCounterClockwise().SignedArea(),
                Is.EqualTo((float)ccw.SignedArea()),
                "a ring already counter-clockwise is left alone");
        });
    }

    [Test]
    public void RepairsDropDegeneraciesAndPreserveArea()
    {
        // A square whose edges carry a repeated vertex and two collinear midpoints.
        var messy = Poly((0, 0), (1, 0), (2, 0), (2, 0), (2, 2), (1, 2), (0, 2));

        Assert.Multiple(() =>
        {
            Assert.That((int)messy.RemoveDuplicateVertices().Points.Count, Is.EqualTo(6),
                "one repeated vertex goes");
            Assert.That((int)messy.RemoveCollinearVertices().Points.Count, Is.EqualTo(4),
                "collinear midpoints and the repeat go, the corners stay");

            var canonical = messy.Canonical();
            Assert.That((int)canonical.Points.Count, Is.EqualTo(4));
            Assert.That((float)canonical.Area(), Is.EqualTo((float)messy.Area()).Within(1e-5),
                "repair is area-preserving");
            Assert.That((bool)canonical.Winding().IsCounterClockwise(), Is.True);
            Assert.That((bool)canonical.IsSimple(), Is.True,
                "and it turns a ring the predicate rejected into one it accepts");
        });
    }

    [Test]
    public void CanonicalFormOfADegenerateRingIsNotAPolygon()
    {
        // Honest rather than convenient: a zero-width spike encloses nothing, so repair
        // reduces it below a polygon rather than inventing one. The caller learns this from
        // IsSimple, not from an exception.
        var spike = Poly((0, 0), (1, 1), (2, 2), (1, 1)).Canonical();
        Assert.Multiple(() =>
        {
            Assert.That((int)spike.Points.Count, Is.LessThan(3));
            Assert.That((bool)spike.IsSimple(), Is.False);
        });
    }

    [Test]
    public void CollinearRemovalSurvivesARepeatedCorner()
    {
        // The regression test for a one-sweep removal: (2,0) appears twice and has zero turn
        // at BOTH copies — once from the collinear run it sits in, once from the repeat — so a
        // sweep deletes the corner outright and the square loses half its area.
        var repeated = Poly((0, 0), (1, 0), (2, 0), (2, 0), (2, 2), (1, 2), (0, 2));
        var cleaned = repeated.RemoveCollinearVertices();

        Assert.Multiple(() =>
        {
            Assert.That((int)cleaned.Points.Count, Is.EqualTo(4), "the corner survives, once");
            Assert.That((float)cleaned.Area(), Is.EqualTo(4f).Within(1e-5f),
                "and the region is unchanged");
        });
    }

    [Test]
    public void RegionInvariantsAreChecked()
    {
        var boundary = Poly((0, 0), (4, 0), (4, 4), (0, 4));
        var inside = Poly((1, 1), (1, 3), (3, 3), (3, 1));
        var overlapping = Poly((3, 1), (3, 3), (5, 3), (5, 1));   // crosses the boundary
        var outside = Poly((6, 1), (6, 3), (7, 3), (7, 1));       // simple, but not inside

        Assert.Multiple(() =>
        {
            var good = new PolygonWithHoles2D(boundary, new[] { inside });
            Assert.That((bool)good.IsSimple(), Is.True);
            Assert.That((bool)good.HolesLieInside(), Is.True);

            var crossing = new PolygonWithHoles2D(boundary, new[] { overlapping });
            Assert.That((bool)crossing.IsSimple(), Is.False,
                "a hole crossing the boundary is caught by the ring-disjointness test");

            var escaped = new PolygonWithHoles2D(boundary, new[] { outside });
            Assert.That((bool)escaped.IsSimple(), Is.True,
                "nothing crosses, so the structural test passes");
            Assert.That((bool)escaped.HolesLieInside(), Is.False,
                "but the containment test catches it");

            var a = Poly((0.5, 0.5), (0.5, 1.5), (1.5, 1.5), (1.5, 0.5));
            var b = Poly((1.0, 1.0), (1.0, 2.0), (2.0, 2.0), (2.0, 1.0));
            Assert.That((bool)new PolygonWithHoles2D(boundary, new[] { a, b }).IsSimple(),
                Is.False, "two holes overlapping each other is caught too");
        });
    }

    [Test]
    public void CanonicalRegionWindsHolesAgainstTheBoundary()
    {
        var boundary = Poly((0, 1), (1, 1), (1, 0), (0, 0));            // clockwise
        var hole = Poly((0.2, 0.2), (0.4, 0.2), (0.4, 0.4), (0.2, 0.4)); // counter-clockwise
        var canonical = new PolygonWithHoles2D(boundary, new[] { hole }).Canonical();

        Assert.Multiple(() =>
        {
            Assert.That((bool)canonical.Boundary.Winding().IsCounterClockwise(), Is.True);
            Assert.That((bool)canonical.Holes[0].Winding().IsClockwise(), Is.True,
                "a hole winds opposite to the boundary containing it");
        });
    }

    [Test]
    public void PredicateAgreesWithTheTriangulatorOnRandomInput()
    {
        // The predicate's reason to exist is that the triangulator's precondition can be
        // honoured. This checks the two agree: every ring IsSimple accepts must tile.
        var random = new System.Random(20260804);

        for (var trial = 0; trial < 200; trial++)
        {
            var n = 3 + random.Next(10);
            var points = new List<(double X, double Y)>();
            for (var i = 0; i < n; i++)
                points.Add((Math.Round(random.NextDouble() * 6), Math.Round(random.NextDouble() * 6)));

            var polygon = new Polygon2D(Ring(points.ToArray()));
            if (!(bool)polygon.IsSimple()) continue;   // the predicate refuses it; nothing to check

            double shoelace = 0;
            for (var i = 0; i < n; i++)
            {
                var (x0, y0) = points[i];
                var (x1, y1) = points[(i + 1) % n];
                shoelace += x0 * y1 - x1 * y0;
            }

            var mesh = polygon.Triangulate();
            double total = 0;
            for (var i = 0; i < mesh.Faces.Count; i++) total += SignedArea(mesh, mesh.Faces[i]);

            Assert.That(total, Is.EqualTo(Math.Abs(shoelace) / 2).Within(1e-3),
                $"trial {trial} (seed 20260804): IsSimple accepted a ring the triangulator "
                + "does not tile — the predicate and the precondition disagree");
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
