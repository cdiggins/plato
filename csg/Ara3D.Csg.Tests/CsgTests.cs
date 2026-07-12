namespace Ara3D.Geometry;

using Ara3D.Collections;

/// <summary>
/// Tests for the Plato CSG port (csg.plato). The central check mirrors the earcut
/// port's area invariant, one dimension up: the boolean of two axis-aligned boxes
/// is a closed solid whose enclosed volume is known analytically, and the enclosed
/// volume of the returned facet soup — computed by the divergence theorem over its
/// fan-triangulated, outward-oriented faces — must match it.
///
/// Overlap offsets are deliberately chosen so no face of one box is coplanar with a
/// face of the other: exactly coplanar shared faces are the one case a centroid
/// ray-parity classifier cannot orient (see ../FINDINGS.md).
/// </summary>
[TestFixture]
public static class CsgTests
{
    // ---- construction ------------------------------------------------------

    public static Point3D P(float x, float y, float z) => new(x, y, z);

    public static Facet F(params Point3D[] pts) => new(pts);

    /// <summary>An axis-aligned box [cx±h, cy±h, cz±h] as six outward-facing CCW quads.</summary>
    public static Solid Box(float h, float cx = 0, float cy = 0, float cz = 0)
    {
        float x0 = cx - h, x1 = cx + h, y0 = cy - h, y1 = cy + h, z0 = cz - h, z1 = cz + h;
        var faces = new[]
        {
            F(P(x1, y0, z0), P(x1, y1, z0), P(x1, y1, z1), P(x1, y0, z1)), // +X
            F(P(x0, y0, z0), P(x0, y0, z1), P(x0, y1, z1), P(x0, y1, z0)), // -X
            F(P(x0, y1, z0), P(x0, y1, z1), P(x1, y1, z1), P(x1, y1, z0)), // +Y
            F(P(x0, y0, z0), P(x1, y0, z0), P(x1, y0, z1), P(x0, y0, z1)), // -Y
            F(P(x0, y0, z1), P(x1, y0, z1), P(x1, y1, z1), P(x0, y1, z1)), // +Z
            F(P(x0, y0, z0), P(x0, y1, z0), P(x1, y1, z0), P(x1, y0, z0)), // -Z
        };
        return new Solid(faces);
    }

    // ---- the invariant -----------------------------------------------------

    /// <summary>Signed volume enclosed by a soup of outward-oriented facets (divergence theorem).</summary>
    public static double EnclosedVolume(Solid s)
    {
        double v = 0;
        foreach (var f in s.Faces)
        {
            var pts = f.Points;
            for (var i = 1; i + 1 < pts.Count; i++)
            {
                var a = pts[0]; var b = pts[i]; var c = pts[i + 1];
                // a · (b × c)
                var cx = (double)b.Y * c.Z - (double)b.Z * c.Y;
                var cy = (double)b.Z * c.X - (double)b.X * c.Z;
                var cz = (double)b.X * c.Y - (double)b.Y * c.X;
                v += a.X * cx + a.Y * cy + a.Z * cz;
            }
        }
        return v / 6.0;
    }

    public static void AssertVolume(Solid s, double expected)
    {
        foreach (var f in s.Faces)
            Assert.That(f.Points.Count, Is.GreaterThanOrEqualTo(3), "degenerate facet in output");
        var tol = 1e-3 * Math.Max(1.0, Math.Abs(expected));
        Assert.That(EnclosedVolume(s), Is.EqualTo(expected).Within(tol));
    }

    // ---- sanity: the inputs themselves -------------------------------------

    [Test]
    public static void UnitBoxHasKnownVolume()
        => AssertVolume(Box(1), 8.0); // side 2 -> 2^3

    [Test]
    public static void BiggerBoxHasKnownVolume()
        => AssertVolume(Box(2), 64.0); // side 4 -> 4^3

    // ---- overlapping boxes, no coplanar faces (offset by 0.5 on every axis) -
    // A = [-1,1]^3 (vol 8), B = [-0.5,1.5]^3 (vol 8), overlap [-0.5,1]^3 (1.5^3 = 3.375).

    private static Solid A => Box(1);
    private static Solid B => Box(1, 0.5f, 0.5f, 0.5f);

    [Test]
    public static void OverlapUnion()
        => AssertVolume(A.Union(B), 8.0 + 8.0 - 3.375);

    [Test]
    public static void OverlapIntersect()
        => AssertVolume(A.Intersect(B), 3.375);

    [Test]
    public static void OverlapSubtract()
        => AssertVolume(A.Subtract(B), 8.0 - 3.375);

    [Test]
    public static void OverlapSubtractReversed()
        => AssertVolume(B.Subtract(A), 8.0 - 3.375);

    // ---- fully disjoint boxes ----------------------------------------------

    private static Solid Far => Box(1, 4, 0, 0); // [3,5]x[-1,1]^2, no contact with A

    [Test]
    public static void DisjointUnion()
        => AssertVolume(A.Union(Far), 16.0);

    [Test]
    public static void DisjointIntersect()
        => AssertVolume(A.Intersect(Far), 0.0);

    [Test]
    public static void DisjointSubtract()
        => AssertVolume(A.Subtract(Far), 8.0);

    // ---- containment: small box strictly inside a large one ----------------
    // Big = [-2,2]^3 (vol 64), Small = [-1,1]^3 (vol 8).

    private static Solid Big => Box(2);
    private static Solid Small => Box(1);

    [Test]
    public static void ContainedUnion()
        => AssertVolume(Big.Union(Small), 64.0);

    [Test]
    public static void ContainedIntersect()
        => AssertVolume(Big.Intersect(Small), 8.0);

    [Test]
    public static void ContainedSubtract() // solid with a cubic cavity
        => AssertVolume(Big.Subtract(Small), 64.0 - 8.0);

    // ---- rotated overlap: neither box is axis-aligned to the other ---------
    // A rotated 45 deg about Z still has volume 8; overlapping an axis box exercises
    // oblique plane cuts rather than the easy axis-aligned arrangement.

    public static Solid RotatedZ(Solid s, double deg)
    {
        var r = deg * Math.PI / 180.0;
        float c = (float)Math.Cos(r), sn = (float)Math.Sin(r);
        Point3D Rot(Point3D p) => P(c * p.X - sn * p.Y, sn * p.X + c * p.Y, p.Z);
        return new Solid(s.Faces.Select(f => (Facet)new(f.Points.Select(Rot).ToList())).ToList());
    }

    [Test]
    public static void RotatedIntersectHasPositiveVolume()
    {
        // A (axis, vol 8) intersected with a 45deg-rotated copy: an octagonal prism,
        // symmetric, volume strictly between 0 and 8. Exact value: 8*(2*sqrt2 - 2).
        var result = A.Intersect(RotatedZ(A, 45));
        AssertVolume(result, 8.0 * (2.0 * Math.Sqrt(2.0) - 2.0));
    }

    // ---- coplanar shared face (documents a known limitation) ---------------
    // Two boxes meeting exactly on x = 1. The union is a 2x1x1... i.e. a 4x2x2 box
    // of volume 16. The centroid of a fragment on the shared face lies exactly on
    // the other box's boundary plane, where ray parity is ambiguous; this is the
    // one case the fold formulation does not resolve. Recorded, not asserted green.

    [Test, Explicit("Documents the coplanar-shared-face limitation; see FINDINGS.md")]
    public static void CoplanarSharedFaceUnion()
    {
        var right = Box(1, 2, 0, 0); // [1,3] x [-1,1]^2, shares face x = 1 with A
        AssertVolume(A.Union(right), 16.0);
    }
}
