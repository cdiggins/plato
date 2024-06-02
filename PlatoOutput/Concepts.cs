public interface Any
{
    Array<String>  FieldNames { get; }
    Array<Dynamic>  FieldValues { get; }
    String  TypeName { get; }
}
public interface Value<Self>: Any, Equatable<Self>
{
}
public interface Numerical<Self>: Value<Self>, Betweenable<Self>, Equatable<Self>, Interpolatable<Self>
{
    Self  Zero { get; }
    Self  One { get; }
    Self  MinValue { get; }
    Self  MaxValue { get; }
}
public interface Real<Self>: Numerical<Self>, Comparable<Self>, ScalarArithmetic<Self>, Difference<Self, Number>
{
    Number  Value { get; }
}
public interface Array<T>
{
    Integer  Count { get; }
    T  At(Integer n);
}
public interface Array2D<T>: Array<T>
{
    Integer  RowCount { get; }
    Integer  ColumnCount { get; }
    T  At(Integer column, Integer row);
}
public interface Array3D<T>: Array<T>
{
    Integer  RowCount { get; }
    Integer  ColumnCount { get; }
    Integer  LayerCount { get; }
    T  At(Integer column, Integer row, Integer layer);
}
public interface Measure<Self>: Real<Self>, Difference<Self, Number>
{
}
public interface Vector<Self>: Array<Number>, Numerical<Self>, Arithmetic<Self>, ScalarArithmetic<Self>
{
    Number  Magnitude { get; }
}
public interface Coordinate<Self>: Value<Self>, Interpolatable<Self>, Betweenable<Self>
{
}
public interface WholeNumber<Self>: Numerical<Self>, Comparable<Self>, Arithmetic<Self>
{
    Integer  Value { get; }
}
public interface Comparable<Self>
{
    Integer  Compare(Self y);
}
public interface Equatable<Self>
{
    Boolean  Equals(Self b);
    Boolean  NotEquals(Self b);
}
public interface Arithmetic<Self>: AdditiveArithmetic<Self, Self>, MultiplicativeArithmetic<Self, Self>, AdditiveInverse<Self>, MultiplicativeInverse<Self>
{
}
public interface AdditiveInverse<Self>
{
    Self  Negative { get; }
}
public interface MultiplicativeInverse<Self>
{
    Self  Reciprocal { get; }
}
public interface AdditiveArithmetic<Self, T>
{
    Self  Add(T other);
    Self  Subtract(T other);
}
public interface Difference<Self, T>: AdditiveArithmetic<Self, T>
{
    T  Subtract(Self other);
}
public interface MultiplicativeArithmetic<Self, T>
{
    Self  Multiply(T other);
    Self  Divide(T other);
    Self  Modulo(T other);
}
public interface ScalarArithmetic<Self>: AdditiveArithmetic<Self, Number>, MultiplicativeArithmetic<Self, Number>, AdditiveInverse<Self>, MultiplicativeInverse<Self>
{
}
public interface BooleanOperations<Self>
{
    Self  And(Self b);
    Self  Or(Self b);
    Self  Not { get; }
}
public interface Interval<Self, TValue, TSize>: Equatable<Self>, Value<Self>
{
    TValue  Min { get; }
    TValue  Max { get; }
    TSize  Size { get; }
}
public interface Interpolatable<Self>
{
    Self  Lerp(Self b, Number amount);
}
public interface Betweenable<Self>
{
    Boolean  Between(Self a, Self b);
    Self  Clamp(Self a, Self b);
}
public interface Bounded2D
{
    Bounds2D  Bounds { get; }
}
public interface Bounded3D
{
    Bounds3D  Bounds { get; }
}
public interface Transformable2D<Self>
{
    Self  Transform(Matrix2D matrix);
}
public interface Transformable3D<Self>
{
    Self  Transform(Matrix3D matrix);
}
public interface Deformable2D<Self>
{
    Self  Deform(Function1<Vector2D, Vector2D> f);
}
public interface OpenClosedShape
{
    Boolean  Closed { get; }
}
public interface Deformable3D<Self>: Transformable3D<Self>
{
    Self  Deform(Function1<Vector3D, Vector3D> f);
}
public interface Geometry
{
}
public interface Geometry2D: Geometry
{
}
public interface Geometry3D: Geometry
{
}
public interface Shape2D: Geometry2D
{
}
public interface Shape3D: Geometry3D
{
}
public interface OpenShape2D: Geometry2D, OpenClosedShape
{
}
public interface ClosedShape2D: Geometry2D, OpenClosedShape
{
}
public interface OpenShape3D: Geometry3D, OpenClosedShape
{
}
public interface ClosedShape3D: Geometry3D, OpenClosedShape
{
}
public interface Procedural<TDomain, TRange>
{
    TRange  Eval(TDomain amount);
}
public interface Curve<TRange>: Procedural<Unit, TRange>, OpenClosedShape
{
    Boolean  Periodic { get; }
}
public interface Curve1D: Curve<Number>
{
}
public interface Curve2D: Geometry2D, Curve<Point2D>
{
}
public interface Curve3D: Geometry3D, Curve<Point3D>
{
}
public interface Surface: Geometry3D
{
}
public interface ParametricSurface: Procedural<UV, Point3D>, Surface
{
    Boolean  PeriodicU { get; }
    Boolean  PeriodicV { get; }
}
public interface ExplicitSurface: Procedural<UV, Number>, Surface
{
}
public interface DistanceField<TDomain>: Procedural<TDomain, Number>
{
}
public interface Field2D<T>: Geometry2D, Procedural<Point2D, T>
{
}
public interface Field3D<T>: Geometry3D, Procedural<Point3D, T>
{
}
public interface ScalarField2D: Field2D<Number>
{
}
public interface ScalarField3D: Field3D<Number>
{
}
public interface DistanceField2D: ScalarField2D
{
}
public interface DistanceField3D: ScalarField3D
{
}
public interface Vector3Field2D: Field2D<Vector3D>
{
}
public interface Vector4Field2D: Field2D<Vector4D>
{
}
public interface Vector2Field3D: Field3D<Vector2D>
{
}
public interface Vector3Field3D: Field3D<Vector3D>
{
}
public interface Vector4Field3D: Field3D<Vector4D>
{
}
public interface ImplicitProcedural<TDomain>
{
    Boolean  Eval(TDomain amount, TDomain epsilon);
}
public interface ImplicitSurface: Surface, ImplicitProcedural<Point3D>
{
}
public interface ImplicitCurve2D: Geometry2D, ImplicitProcedural<Point2D>
{
}
public interface ImplicitVolume: Geometry3D, ImplicitProcedural<Point3D>
{
}
public interface Points2D: Geometry2D
{
    Array<Point2D>  Points { get; }
}
public interface Points3D: Geometry3D
{
    Array<Point3D>  Points { get; }
}
public interface BezierPatch: Points3D, Surface, Array2D<Point3D>
{
}
public interface PolyhederalFace
{
    Integer  FaceIndex { get; }
    Array<Integer>  VertexIndices { get; }
    Polyhedron  Polyhedron { get; }
}
public interface Polyhedron: Surface, Points3D
{
    Array<PolyhederalFace>  Faces { get; }
}
public interface ConvexPolyhedron: Polyhedron
{
}
public interface SolidPolyhedron: Polyhedron
{
}
public interface TriMesh: Polyhedron
{
}
public interface QuadMesh: Polyhedron
{
}
public interface Grid2D: Array2D<Point2D>
{
}
public interface QuadGrid: Array2D<Point3D>
{
    Boolean  ClosedX { get; }
    Boolean  ClosedY { get; }
}
public interface PolygonalChain2D: Points2D, OpenClosedShape
{
}
public interface PolygonalChain3D: Points3D, OpenClosedShape
{
}
public interface ClosedPolygonalChain2D: PolygonalChain2D, ClosedShape2D
{
}
public interface ClosedPolygonalChain3D: PolygonalChain3D
{
}
public interface Polygon2D: PolygonalChain2D
{
}
public interface Polygon3D: PolygonalChain3D
{
}
