using NUnit.Framework;
using Ara3D.Geometry;

namespace Plato.Generated.V3.Tests;

/// <summary>
/// Smoke tests for the v3 generated library (Plato.Generated.V3) running on the
/// Plato.Intrinsics.V3 runtime: vector math, sum-type cases, and transforms.
/// </summary>
public static class SmokeTests
{
    private const float Tolerance = 1e-5f;

    [Test]
    public static void Vector3_BasicMath()
    {
        var a = new Vector3(1, 2, 3);
        var b = new Vector3(4, 5, 6);

        Assert.That((a + b).Equals(new Vector3(5, 7, 9)));
        Assert.That(a.Dot(b), Is.EqualTo(32f).Within(Tolerance));
        Assert.That(a.Cross(b).Equals(new Vector3(-3, 6, -3)));
        Assert.That(new Vector3(3, 4, 0).Magnitude, Is.EqualTo(5f).Within(Tolerance));
        Assert.That(a.MagnitudeSquared(), Is.EqualTo(14f).Within(Tolerance));
        Assert.That(a.Lerp(b, 0.5f).Equals(new Vector3(2.5f, 3.5f, 4.5f)));
        Assert.That(a.Zero().Equals(new Vector3(0, 0, 0)));
        Assert.That(a.One().Equals(new Vector3(1, 1, 1)));
        Assert.That(a.Hash(), Is.EqualTo(new Vector3(1, 2, 3).Hash()));
    }

    [Test]
    public static void Vector2_And_Vector4()
    {
        var v2 = new Vector2(3, 4);
        Assert.That(v2.Magnitude, Is.EqualTo(5f).Within(Tolerance));
        Assert.That(v2.Dot(new Vector2(1, 0)), Is.EqualTo(3f).Within(Tolerance));

        var v4 = new Vector4(1, 0, 0, 0);
        Assert.That(v4.Lerp(new Vector4(0, 1, 0, 0), 0.5f).Equals(new Vector4(0.5f, 0.5f, 0, 0)));
    }

    [Test]
    public static void Number_And_Angle_Extensions()
    {
        Assert.That(2f.Add(3f), Is.EqualTo(5f).Within(Tolerance));
        Assert.That(10f.Lerp(20f, 0.25f), Is.EqualTo(12.5f).Within(Tolerance));
        var a = new Angle(MathF.PI);
        var b = new Angle(0f);
        Assert.That(b.Lerp(a, 0.5f).Radians, Is.EqualTo(MathF.PI / 2).Within(Tolerance));
        Assert.That(b.Compare(a), Is.LessThan(0));
    }

    [Test]
    public static void SumType_PlatonicSolidKind()
    {
        var cube = PlatonicSolidKind.Cube();
        var tet = PlatonicSolidKind.Tetrahedron();

        Assert.That(cube.IsCube(), Is.True);
        Assert.That(cube.IsTetrahedron(), Is.False);
        Assert.That(tet.IsTetrahedron(), Is.True);
        Assert.That(cube, Is.Not.EqualTo(tet));
        Assert.That(cube, Is.EqualTo(PlatonicSolidKind.Cube()));
        Assert.That(cube.ToString(), Is.EqualTo("Cube"));
    }

    [Test]
    public static void SumType_PayloadCase_ClassicEasing()
    {
        var linear = ClassicEasing.Linear();
        var eased = ClassicEasing.Eased(EasingFamily.Cubic(), EasingPhase.Default);

        Assert.That(linear.IsLinear(), Is.True);
        Assert.That(eased.IsEased(), Is.True);
        Assert.That(eased.Eased_Family, Is.EqualTo(EasingFamily.Cubic()));
        Assert.That(linear, Is.Not.EqualTo(eased));
        Assert.That(eased, Is.EqualTo(ClassicEasing.Eased(EasingFamily.Cubic(), EasingPhase.Default)));
    }

    [Test]
    public static void Transforms_QuaternionAndMatrix()
    {
        // 90-degree rotation about Z maps +X to +Y.
        var q = System.Numerics.Quaternion.CreateFromAxisAngle(new System.Numerics.Vector3(0, 0, 1), MathF.PI / 2);
        var rotated = new Vector3(1, 0, 0).Transform(new Quaternion(q));
        Assert.That((float)rotated.X, Is.EqualTo(0f).Within(Tolerance));
        Assert.That((float)rotated.Y, Is.EqualTo(1f).Within(Tolerance));

        var m = new Matrix4x4(System.Numerics.Matrix4x4.CreateTranslation(1, 2, 3));
        var moved = new Vector3(0, 0, 0).Transform(m);
        Assert.That(moved.Equals(new Vector3(1, 2, 3)));
        Assert.That(m.ElementAt(3, 0), Is.EqualTo(1f).Within(Tolerance));
        Assert.That(m.RowCount, Is.EqualTo(4));
        Assert.That(m.ColumnCount(), Is.EqualTo(4));

        var t = Transform3D.Create(new Vector3(1, 2, 3), new Quaternion(q), new Vector3(1, 1, 1));
        Assert.That(t.Translation.Equals(new Vector3(1, 2, 3)));
    }

    [Test]
    public static void Plane_ClosestPoint()
    {
        // XY plane (normal +Z through origin): closest point drops the Z coordinate.
        var plane = new Plane(new Vector3(0, 0, 1), 0);
        var p = plane.ClosestPoint(new Point3D(1, 2, 5));
        Assert.That(p.X, Is.EqualTo(1f).Within(Tolerance));
        Assert.That(p.Y, Is.EqualTo(2f).Within(Tolerance));
        Assert.That(p.Z, Is.EqualTo(0f).Within(Tolerance));
        Assert.That(plane.Distance, Is.EqualTo(0f).Within(Tolerance));
    }
}
