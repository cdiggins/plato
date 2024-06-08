public static class NumericsAdapters
{
    public static System.Numerics.Quaternion ToNumerics(this Ara3D.SinglePrecision.Quaternion self) => new(self.X, self.Y, self.Z, self.W);
    public static System.Numerics.Vector2 ToNumerics(this Ara3D.SinglePrecision.Vector2D self) => new(self.X, self.Y);
    public static System.Numerics.Vector3 ToNumerics(this Ara3D.SinglePrecision.Vector3D self) => new(self.X, self.Y, self.Z);
    public static System.Numerics.Vector4 ToNumerics(this Ara3D.SinglePrecision.Vector4D self) => new(self.X, self.Y, self.Z, self.W);
    public static System.Numerics.Plane ToNumerics(this Ara3D.SinglePrecision.Plane self) => new(self.Normal.ToNumerics(), self.D);
    public static System.Numerics.Complex ToNumerics(this Ara3D.SinglePrecision.Complex self) => new(self.Real, self.Imaginary);

    public static System.Numerics.Matrix4x4 ToNumerics(this Ara3D.SinglePrecision.Matrix3D self) => new(
        self.M11, self.M12, self.M13, self.M14,
        self.M21, self.M22, self.M23, self.M24,
        self.M31, self.M32, self.M33, self.M34,
        self.M41, self.M42, self.M43, self.M44);

    public static System.Numerics.Quaternion ToNumerics(this Ara3D.DoublePrecision.Quaternion self) => self.ChangePrecision().ToNumerics();
    public static System.Numerics.Vector2 ToNumerics(this Ara3D.DoublePrecision.Vector2D self) => self.ChangePrecision().ToNumerics();
    public static System.Numerics.Vector3 ToNumerics(this Ara3D.DoublePrecision.Vector3D self) => self.ChangePrecision().ToNumerics();
    public static System.Numerics.Vector4 ToNumerics(this Ara3D.DoublePrecision.Vector4D self) => self.ChangePrecision().ToNumerics();
    public static System.Numerics.Plane ToNumerics(this Ara3D.DoublePrecision.Plane self) => self.ChangePrecision().ToNumerics();
    public static System.Numerics.Complex ToNumerics(this Ara3D.DoublePrecision.Complex self) => self.ChangePrecision().ToNumerics();
    public static System.Numerics.Matrix4x4 ToNumerics(this Ara3D.DoublePrecision.Matrix3D self) => self.ChangePrecision().ToNumerics();
}

public static class WpfAdapters
{
    // https://learn.microsoft.com/en-us/dotnet/api/system.windows?view=windowsdesktop-8.0
    /*
     *  * System.Windows.Media.Media3D namespace
     *
     * AffineTransform3D
     * AxisAngleRotation3D
     * Matrix3D
     * MeshGeometry3D
     * Point3D
     * Point3DCollection
     * Point4D
     * Point4DCollection
     * Quaternion
     * QuaternionRotation3D
     * Rect3D
     * RotateTransform3D
     * Rotation3D
     * ScaleTransform3D
     * Size3D
     * Transform3D
     * TranslateTransform3D
     * Vector3D
     *
     * System.Windows namespace
     */
    public static System.Windows.Point ToWpf(this Ara3D.DoublePrecision.Point2D self) => new(self.X, self.Y);
    public static System.Windows.Rect ToWpf(this Ara3D.DoublePrecision.Rect2D self) => new(self.Center.ToWpf(), self.Size.ToWpf());
    public static System.Windows.Size ToWpf(this Ara3D.DoublePrecision.Size2D self) => new(self.Width, self.Height);
    public static System.Windows.Media.Media3D.Point3D ToWpf(this Ara3D.DoublePrecision.Point3D self) => new(self.X, self.Y, self.Z);
    public static System.Windows.Media.Media3D.Point4D ToWpf(this Ara3D.DoublePrecision.Point4D self) => new(self.X, self.Y, self.Z, self.W);
    public static System.Windows.Media.Media3D.Quaternion ToWpf(this Ara3D.DoublePrecision.Quaternion self) => new(self.X, self.Y, self.Z, self.W);
    public static System.Windows.Media.Media3D.Point3D ToWpf(this Ara3D.DoublePrecision.Vector3D self) => new(self.X, self.Y, self.Z);
    public static System.Windows.Media.Media3D.Size3D ToWpf(this Ara3D.DoublePrecision.Size3D self) => new(self.Width, self.Depth, self.Height);

    public static System.Windows.Point ToWpf(this Ara3D.SinglePrecision.Point2D self) => new(self.X, self.Y);
    public static System.Windows.Rect ToWpf(this Ara3D.SinglePrecision.Rect2D self) => new(self.Center.ToWpf(), self.Size.ToWpf());
    public static System.Windows.Size ToWpf(this Ara3D.SinglePrecision.Size2D self) => new(self.Width, self.Height);
    public static System.Windows.Media.Media3D.Point3D ToWpf(this Ara3D.SinglePrecision.Point3D self) => new(self.X, self.Y, self.Z);
    public static System.Windows.Media.Media3D.Point4D ToWpf(this Ara3D.SinglePrecision.Point4D self) => new(self.X, self.Y, self.Z, self.W);
    public static System.Windows.Media.Media3D.Quaternion ToWpf(this Ara3D.SinglePrecision.Quaternion self) => new(self.X, self.Y, self.Z, self.W);
    public static System.Windows.Media.Media3D.Point3D ToWpf(this Ara3D.SinglePrecision.Vector3D self) => new(self.X, self.Y, self.Z);
    public static System.Windows.Media.Media3D.Size3D ToWpf(this Ara3D.SinglePrecision.Size3D self) => new(self.Width, self.Depth, self.Height);
}

public static class UnityAdapters
{
    public static UnityEngine.Vector2 ToUnity(this Ara3D.SinglePrecision.Vector2D self) => new(self.X, self.Y);
    public static UnityEngine.Vector3 ToUnity(this Ara3D.SinglePrecision.Vector3D self) => new(self.X, self.Y, self.Z);
    public static UnityEngine.Vector4 ToUnity(this Ara3D.SinglePrecision.Vector4D self) => new(self.X, self.Y, self.Z, self.W);
    public static UnityEngine.Quaternion ToUnity(this Ara3D.SinglePrecision.Quaternion self) => new(self.X, self.Y, self.Z, self.W);
    public static UnityEngine.Vector3 ToUnity(this Ara3D.SinglePrecision.Point3D self) => new(self.X, self.Y, self.Z);
    public static UnityEngine.Vector2 ToUnity(this Ara3D.SinglePrecision.Size2D self) => new(self.Width, self.Height);
    public static UnityEngine.Vector3 ToUnity(this Ara3D.SinglePrecision.Size3D self) => new(self.Width, self.Depth, self.Height);
    public static UnityEngine.Matrix4x4 ToUnity(this Ara3D.SinglePrecision.Matrix3D self) => new(self.Column1.ToUnity(), self.Column2.ToUnity(),  self.Column3.ToUnity(), self.Column4.ToUnity());

    public static UnityEngine.Vector2 ToUnity(this Ara3D.DoublePrecision.Vector2D self) => self.ChangePrecision().ToUnity();
    public static UnityEngine.Vector3 ToUnity(this Ara3D.DoublePrecision.Vector3D self) => self.ChangePrecision().ToUnity();
    public static UnityEngine.Vector4 ToUnity(this Ara3D.DoublePrecision.Vector4D self) => self.ChangePrecision().ToUnity();
    public static UnityEngine.Quaternion ToUnity(this Ara3D.DoublePrecision.Quaternion self) => self.ChangePrecision().ToUnity();
    public static UnityEngine.Vector3 ToUnity(this Ara3D.DoublePrecision.Point3D self) => self.ChangePrecision().ToUnity();
    public static UnityEngine.Vector2 ToUnity(this Ara3D.DoublePrecision.Size2D self) => self.ChangePrecision().ToUnity();
    public static UnityEngine.Vector3 ToUnity(this Ara3D.DoublePrecision.Size3D self) => self.ChangePrecision().ToUnity();
    public static UnityEngine.Matrix4x4 ToUnity(this Ara3D.DoublePrecision.Matrix3D self) => self.ChangePrecision().ToUnity();
}

public static class Geometry3SharpAdapters
{
    public static 
}

public static class SpeckleAdapters
{
    public static Objects.Geometry.Vector ToSpeckle(this Ara3D.DoublePrecision.Vector3D self) => new(self.X, self.Y, self.Z);
    public static Objects.Geometry.Point ToSpeckle(this Ara3D.DoublePrecision.Point3D self) => new(self.X, self.Y, self.Z);
    
    // https://github.com/specklesystems/speckle-sharp/blob/main/Objects/Objects/Geometry/Line.cs
    public static Objects.Geometry.Line ToSpeckle(this Ara3D.DoublePrecision.Line3D self) => new(self.A.ToSpeckle(), self.B.ToSpeckle());
    
    public static Objects.Geometry.Mesh ToSpeckle(this Ara3D.DoublePrecision.TriMesh self)
    {
        var verts = new List<double>();
        for (var i = 0; i < self.Points.Count; ++i)
        {
            var pt = self.Points.At(i);
            verts.Add(pt.X);
            verts.Add(pt.Y);
            verts.Add(pt.Z);
        }

        var faces = new List<int>();
        for (var i = 0; i < self.Faces.Count; ++i)
        {
            var face = self.Faces.At(i);
            if (face.VertexIndices.Count != 3)
                throw new Exception("Internal error: expected all faces of a tri mesh to have three points");
            for (var j=0; j < 3; j++)
                faces.Add(face.VertexIndices.At(j));
        }

        // Optional:
        //var colors = new List<int>();
        //var uvs = new List<double>();

        var r = new Objects.Geometry.Mesh(verts, faces);
        return r;
    }

    //public static Objects.Geometry.Plane ToSpeckle(this Ara3D.DoublePrecision.Plane self) => new(self.Normal * self.D, self.Normal, );

    // ??
    //public static Objects.Geometry.Box ToSpeckle(this Ara3D.DoublePrecision.Box3D self) => new()

    /* https://github.com/specklesystems/speckle-sharp/tree/main/Objects/Objects/Geometry
    * Arc.cs
    Box.cs
    Brep.cs
    BrepEdge.cs
    BrepFace.cs
    BrepLoop.cs
    BrepTrim.cs
    Circle.cs
    ControlPoint.cs
    Curve.cs
    Ellipse.cs
    Extrusion.cs
    Line.cs
    Mesh.cs
    Plane.cs
    Point.cs
    Pointcloud.cs
    Polycurve.cs
    Polyline.cs
    PolylineExtensions.cs
    Spiral.cs
    Surface.cs
    Vector.cs
    */


}

/*
 *     
     *
     * https://github.com/MonoGame/MonoGame
     * https://github.com/MonoGame/MonoGame/blob/develop/MonoGame.Framework/Vector2.cs
     * BoundingBOx
     * BoundingFrustum
     * Boundingsphere
     * Color
     * Curve
     * Matrix
     * PLane
     * Point
     * Quaternion
     * Ray
     * Rectangle
     * Vector2
     * Vector3
     * Vector4
     *
     * https://learn.microsoft.com/en-us/dotnet/api/system.numerics?view=net-8.0
     * Complex
     * Matrix3x2
     * Matrix4x4
     * Plane
     * Quaternion
     * Vector2
     * Vector3
     * Vector4
     *
     * https://github.com/sharpdx/SharpDX
     * https://github.com/sharpdx/SharpDX/blob/master/Source/SharpDX/Size2.cs
     * https://github.com/sharpdx/SharpDX/tree/master/Source/SharpDX.Mathematics
     * https://www.nuget.org/packages/SharpDX
     * Size2
     * Size2F
     * Bool4
     * BoundingBox
     * BoundingFrustum
     * BoundingSphere
     * Collision
     * Color
     * Color3
     * Color4
     * Int3
     * Int4
     * Matrix
     * Matrix3x2
     * Matrix3x3
     * Matrix5x4
     * OrientedBoundingBOx
     * Plane
     * Point
     * Quaternion
     * Ray
     * Rectangle
     * RectangleF
     * Vector2
     * Vector3
     * Vector4
     *
     * https://opentk.net/
     * https://github.com/opentk/opentk
     * https://github.com/opentk/opentk/tree/master/src/OpenTK.Mathematics/Vector
     * Vector2.cs
     * Vector2d.cs
     * Vector2h.cs
     * Vector2i.cs
     * Vector3.cs
     * Vector3d.cs
     * Vector3h.cs
     * Vector3i.cs
     * Vector4.cs
     * Vector4d.cs
     * Vector4h.cs
     * Vector4i.cs
     * https://github.com/opentk/opentk/tree/master/src/OpenTK.Mathematics/Matrix
     * So MANY matrices!!
     * BezierCurve
     * BezierCurveCubic
     * BezierCurveQuadric
     * Box2
     * Box2d
     * Box2i
     * Box3
     * Box3d
     * Box3i
     * Color
     * Half
     * Quaternion
     * Quaterniond
     *
     * https://www.stride3d.net/
     *
     * https://github.com/bepu/bepuphysics2/blob/master/BepuUtilities/BoundingBox.cs
     * BoundingBox
     * BoundingSphere
     * Int2
     * Int3
     * Int4
     * Matrix
     * Quaternion
     * QuaternionWide
     * Vector2Wide
     * Vector3Wide
     * Vector4Wide
     *
     * https://github.com/Unity-Technologies/Unity.Mathematics/tree/master/src/Unity.Mathematics
     * So many types!
     * Some interesting ones
     * SVD
     * RigidTransform
     * Quaternion
     * Matrix
     * AffineTransform
     * Plane
     * MinMaxAABB
     * boolNxM
     * intNxM
     * floatNxM
     * doubleNxM
     * halfNxM
     * Half
     *
     * https://github.com/gradientspace/geometry3Sharp
     * https://github.com/gradientspace/geometry3Sharp/tree/master/math
     * 
AxisAlignedBox2d.cs
AxisAlignedBox2f.cs
AxisAlignedBox2i.cs
AxisAlignedBox3d.cs
AxisAlignedBox3f.cs
AxisAlignedBox3i.cs
BoundsUtil.cs
Box2.cs
Box3.cs
FastWindingMath.cs
Frame3f.cs
IndexTypes.cs
IndexUtil.cs
Integrate1d.cs
Interval1d.cs
Interval1i.cs
Line2.cs
Line3.cs
MathUtil.cs
Matrix2d.cs
Matrix2f.cs
Matrix3d.cs
Matrix3f.cs
MatrixUtil.cs
Plane3.cs
PrimalQuery2d.cs
Quaterniond.cs
Quaternionf.cs
Query2.cs
Query2Integer.cs
QueryTuple2d.cs
Ray3.cs
ScalarMap.cs
Segment2.cs
Segment3.cs
TransformSequence.cs
TransformSequence2.cs
Triangle2.cs
Triangle3.cs
Vector2d.cs
Vector2f.cs
Vector2i.cs
Vector3d.cs
Vector3f.cs
Vector3i.cs
Vector4d.cs
Vector4f.cs
VectorTuple.cs

    * https://github.com/hypar-io/Elements/tree/master/Elements/src/Geometry
     * Arc.cs
BBox3.cs
Bezier.cs
BooleanMode.cs
BoundedCurve.cs
Box.cs
Circle.cs
Clipper.cs
Color.cs
Colors.cs
Containment.cs
Contour.cs
ConvexHull.cs
CsgExtensions.cs
Curve.cs
Ellipse.cs
EllipticalArc.cs
EndType.cs
GraphicsBuffers.cs
IGraphicsBuffers.cs
IndexedPolycurve.cs
InfiniteLine.cs
Kernel.cs
Line.cs
LineSegmentExtensions.cs
Matrix.cs
Mesh.cs
Meshes.cs
NormalizationType.cs
Plane.cs
Polygon.cs
PolygonExtensions.cs
Polygons.cs
Polyline.cs
PolylineExtensions.cs
Profile.cs
Quaternion.cs
README.md
Ray.cs
RayIntersectionResult.cs
RelationToPlane.cs
Representation.cs
ThickenedPolyline.cs
Transform.cs
Triangle.cs
TrimmedCurve.cs
UV.cs
Vector3.cs
Vector3Extensions.cs
Vertex.cs
VoidTreatment.cs
    *
     * Helix Toolkit 
     * https://github.com/helix-toolkit/helix-toolkit/blob/develop/Source/HelixToolkit.Wpf.Shared/Geometry/BoundingSphere.cs
     * Mesh3D
     * Plane3D
     * LineSegment
     * Ray3D
     * Triangle
     *
 
     *
     * https://github.com/dotnet/Silk.NET/tree/main/src/Maths/Silk.NET.Maths
     *Box2D.cs
Box3D.cs
Circle.cs
Cube.cs
Matrix2X2.Ops.cs
Matrix2X2.cs
Matrix2X3.Ops.cs
Matrix2X3.cs
Matrix2X4.Ops.cs
Matrix2X4.cs
Matrix3X2.Ops.cs
Matrix3X2.cs
Matrix3X3.Ops.cs
Matrix3X3.cs
Matrix3X4.Ops.cs
Matrix3X4.cs
Matrix4X2.Ops.cs
Matrix4X2.cs
Matrix4X3.Ops.cs
Matrix4X3.cs
Matrix4X4.Ops.cs
Matrix4X4.cs
Matrix5X4.Ops.cs
Matrix5X4.cs
Plane.Ops.cs
Plane.cs
PotentionalAdditions.md
Quaternion.cs
Ray2D.cs
Ray3D.cs
Rectangle.Ops.cs
Rectangle.cs
Scalar.As.cs
Scalar.As.tt
Scalar.BaseOps.cs
Scalar.Constants.cs
Scalar.Conversions.cs
Scalar.Exp.cs
Scalar.Inverse.cs
Scalar.Log.cs
Scalar.MathFPort.MissingMethods.cs
Scalar.MathFPort.cs
Scalar.Pow.cs
Scalar.Sin.cs
Silk.NET.Maths.csproj
Sphere.cs
SystemNumericsExtensions.cs
Vector2D.Ops.cs
Vector2D.cs
Vector3D.Ops.cs
Vector3D.cs
Vector4D.Ops.cs
Vector4D.cs
     *
     * https://github.com/mathnet/mathnet-spatial/tree/master/src
     * https://github.com/mathnet/mathnet-spatial/blob/master/src/Spatial/Euclidean/Vector3D.cs
Circle2D.cs
Circle3D.cs
CoordinateSystem.cs
EulerAngles.cs
Line2D.cs
Line3D.cs
LineSegment2D.cs
LineSegment3D.cs
Matrix2D.cs
Matrix3D.cs
Plane.cs
Point2D.cs
Point3D.cs
PolyLine2D.cs
PolyLine3D.cs
Polygon2D.cs
Quaternion.cs
Ray3D.cs
UnitVector3D.cs
Vector2D.cs
Vector3D.cs

    * https://github.com/GSharker/G-Shark/tree/master/src/GShark/Geometry
     * Arc.cs
BoundingBox.cs
Circle.cs
ConvexHull.cs
Line.cs
Mesh.cs
MeshCorner.cs
MeshEdge.cs
MeshFace.cs
MeshGeometry.cs
MeshHalfEdge.cs
MeshTopology.cs
MeshVertex.cs
NurbsBase.cs
NurbsCurve.cs
NurbsSurface.cs
Plane.cs
Point3.cs
Point4.cs
PolyCurve.cs
Polygon.cs
Polyline.cs
Ray.cs
Vector.cs
Vector3.cs

    https://github.com/Habrador/Computational-geometry


    // https://github.com/RiSearcher/GeometRi.CSharp/blob/master/GeometRi/Vector3D.cs
    AABB.cs
AbstractClass.cs
Box3d.cs
Circle3D.cs
ConvexPolyhedron.cs
Coord3D.cs
Ellipse.cs
Ellipsoid.cs
GeometRi.csproj
GeometRi3D.cs
Interface.cs
Line3D.cs
Matrix3D.cs
Plane3D.cs
Point3D.cs
Quaternion.cs
Ray3D.cs
Rotation.cs
Segment3D.cs
Sphere.cs
Tetrahedron.cs
Triangle.cs
Vector3D.cs

    // https://github.com/mcneel/rhino3dm
    // https://www.nuget.org/packages/Rhino3dm/
    opennurbs_3dm.cs
opennurbs_arc.cs
opennurbs_arccurve.cs
opennurbs_bounding_box.cs
opennurbs_box.cs
opennurbs_brep.cs
opennurbs_circle.cs
opennurbs_color.cs
opennurbs_cone.cs
opennurbs_curve.cs
opennurbs_curveonsurface.cs
opennurbs_cylinder.cs
opennurbs_ellipse.cs
opennurbs_fpoint.cs
opennurbs_geometry.cs
opennurbs_ground_plane.cs
opennurbs_knot.cs
opennurbs_layer.cs
opennurbs_line.cs
opennurbs_linecurve.cs
opennurbs_matrix.cs
opennurbs_mesh.cs
opennurbs_nurbscurve.cs
opennurbs_nurbssurface.cs
opennurbs_offsetsurface.cs
opennurbs_plane.cs
opennurbs_planesurface.cs
opennurbs_point.cs
opennurbs_pointcloud.cs
opennurbs_pointgeometry.cs
opennurbs_pointgrid.cs
opennurbs_polycurve.cs
opennurbs_polyedgecurve.cs
opennurbs_polyline.cs
opennurbs_polylinecurve.cs
opennurbs_quaternion.cs
opennurbs_rectangle.cs
opennurbs_revsurface.cs
opennurbs_sphere.cs
opennurbs_subd.cs
opennurbs_surface.cs
opennurbs_xform.cs

    https://github.com/NetTopologySuite/NetTopologySuite
    https://github.com/NetTopologySuite/NetTopologySuite/tree/develop/src/NetTopologySuite/Mathematics
    DD (Double Double)
    Matrix
    Vector2D
    Vector3D
    Plane3D
    // https://github.com/NetTopologySuite/NetTopologySuite/tree/develop/src/NetTopologySuite/Geometries
    LineSegment
    MultiPoint
    MultiPolygon
    Point
    Polygon
    Position
    Quadrant
    Triangle
    // https://github.com/NetTopologySuite/NetTopologySuite/tree/develop/src/NetTopologySuite/Shape
    CubicBezierCurve
    // other 
    Interval
     */


