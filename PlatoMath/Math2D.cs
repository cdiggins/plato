namespace PlatoMath;

public partial interface INumeric<T>
{
    T op_Add(T v);
    T op_Subtract(T v);
    T op_Multiply(T v);
    T op_Divide(T v);
    T op_Negate();
}

public partial interface IBoolean<T>
{
    T op_And(T v);
    T op_Or(T v);
    T op_XOr(T v);
    T op_Not();
}

public partial interface IDistance<T> : IMembership<T>, IBoolean<IDistance<T>>
{
    double Distance(T point);
    bool Contains(T point, double epsilon) => Distance(point) <= epsilon;
}

public partial interface IMembership<T> : IBoolean<IMembership<T>>
{
    bool Contains(T item, double epsilon);
}

public partial interface IIntersection<TSelf, TResult>
{
    TResult? Intersection(TSelf other);
}

public partial interface ITransformable2D<TSelf> 
{
    TSelf Transform(Matrix3x3 matrix);
}

public partial interface IEquatable<TSelf>
{ 
    bool Equals(TSelf other);
}

public partial interface IOrderable<TSelf>
{
    int CompareTo(TSelf other);
}

public partial interface IArray<T>
{
    T this[int index] { get; }
    int Count { get; }
}

public partial interface IMagnitude 
{
    double Magnitude { get; }
}

public partial interface IVector<T>
    : IOrderable<T>, ITransformable2D<T>, IMagnitude, IEquatable<T>, IArray<double>, ILerpable<T> where T: IVector<T>
{
    int CompareTo(T other) => 
        Magnitude > other.Magnitude ? 1 : 
        Magnitude < other.Magnitude ? -1 : 
        0;
}

public partial class Vector2D : IVector<Vector2D>, INumeric<Vector2D>
{
    public double X { get; init; }
    public double Y { get; init; }
}

public partial class Vector3D : IVector<Vector3D>, INumeric<Vector3D>
{
    public double X { get; init; }
    public double Y { get; init; }
    public double Z { get; init; }
}

public partial class Point2D : Vector2D, IDistance<Point2D>, ITransformable2D<Point2D>
{
}

public partial class Point3D : Vector3D, IDistance<Point3D>, ITransformable2D<Point3D>
{
}

public partial class Size2D : ILerpable<Size2D>
{
    public double Width { get; init; }
    public double Height { get; init; }
}

public partial class Interval<T> : ILerpable<T>
{
    public T From { get; init; }
    public T To { get; init; }
}

public partial class DecimalFraction : INumeric<DecimalFraction>
{
    public double Value { get; }
}

public partial class Angle : INumeric<Angle>
{
    public double Radian { get; init; }
}

public partial interface ILerpable<T>
{
    T Lerp(T other, DecimalFraction amount);
}

public partial interface ICurve2D : ILerpable<Point2D>, IDistance<Point2D>
{
    bool Closed { get; }
    double Length { get; }
    Point2D Closest(Point2D point);
}

public partial interface IOpenCurve2D : ICurve2D
{
    new bool Closed => false;
}

public partial interface ICurveSection2D : IOpenCurve2D
{
    ICurve2D Curve { get; }
    DecimalFraction From { get; init; }
    DecimalFraction To { get; init; }
}

public partial interface IPoints2D : IArray<Point2D>, IDistance<Point2D>
{ }

public partial interface IDiscreteCurve2D : IPoints2D, ICurve2D
{ }

public partial interface IClosedCurve2D : ICurve2D
{
    new bool Closed => true;
    double Area { get; }    
}

public partial interface IDiscreteClosedCurve2D : IDiscreteCurve2D, IClosedCurve2D
{
}

public partial interface IDiscreteOpenCurve2D : IDiscreteCurve2D, IOpenCurve2D
{
}

public partial interface IShape2D : IDistance<Point2D>
{
    Point2D Closest(Point2D point);
    IClosedCurve2D Perimeter { get; }
    bool IgnoresRotation { get; }
    IShape2D Transform(Matrix3x3 matrix);
}

public partial interface IShape2D<T> : IShape2D, IIntersection<LineSegment2D, LineSegment2D>, IEquatable<T>, ITransformable2D<T>
{
}

public partial interface IClosedShape2D<T> : IMembership<Point2D>
{ 
    public double Perimeter { get; }
}

public partial class LineSegment2D : IShape2D<LineSegment2D>, IDiscreteOpenCurve2D
{
    public Point2D A { get; init; }
    public Point2D B { get; init; }
}

public partial class Triangle2D : IShape2D<Triangle2D>, IDiscreteClosedCurve2D
{
    public Point2D A { get; init; }
    public Point2D B { get; init; }
    public Point2D C { get; init; }
}

public partial class Quad2D : IShape2D<Quad2D>, IDiscreteClosedCurve2D
{
    public Point2D A { get; init; }
    public Point2D B { get; init; }
    public Point2D C { get; init; }
    public Point2D D { get; init; }
}

public partial class Circle : IShape2D<Circle>, IClosedCurve2D
{
    public Point2D Center { get; init; }
    public double Radius { get; init; }
    public bool Contains(Point2D point) => throw new NotImplementedException();
}

public partial class Arc : ICurveSection2D, IShape2D<Circle>
{
    public Circle Circle { get; init; }
    public Interval<Angle> Interval { get; init; }
}

public partial class Rect : IShape2D<Rect>, IClosedCurve2D
{
    public Point2D TopLeft { get; init; }
    public Size2D Size { get; init; }
}

public partial class Ellipse2D : IShape2D<Ellipse2D>
{
    public Point2D Center { get; init; }
    public Size2D Size { get; init; }
}

public partial class Ray2D
{
    public Point2D Point { get; init; }
    public Vector2D Direction { get; init; }
}

public partial interface ITransform2D
{    
    Matrix3x3 Matrix { get; }
}

public partial class Translation2D : ITransform2D
{ 
    public Vector2D Vector { get; init; }
}

public partial class Scale2D : ITransform2D
{
    public Vector2D Vector { get; init; }
}

public partial class Skew2D : ITransform2D
{
    public Vector2D Vector { get; init; }
}

public partial class Pose2D : ITransform2D
{
    public Translation2D Translation { get; init; }
    public Rotation2D Rotation { get; init; }
}

public partial class Transform2D : ITransform2D
{
    public Translation2D Translation { get; init; }
    public Rotation2D Rotation { get; init; }
    public Scale2D Scale { get; init; }
    public Skew2D Skew { get; init; }
}

public partial class Matrix3x3 
{
    public Vector3D Row0 { get; init; }
    public Vector3D Row1 { get; init; }
    public Vector3D Row2 { get; init; }
}

public partial class Rotation2D : ITransform2D
{
    public Point2D Point { get; init; }
    public Angle Angle { get; init; }
}

public partial class CubicBezier2D : ICurve2D
{
    public Point2D Start { get; init; }
    public Point2D End { get; init; }
    public Point2D ControlA { get; init; }
    public Point2D ControlB { get; init; }
}

public partial class Polygon2D : IShape2D<Polygon2D>
{
    public IArray<Point2D> Points { get; init; }
}

public partial class CompoundShape2D : IShape2D<CompoundShape2D>
{
    public IArray<IShape2D> Shapes { get; init; }
}

public partial class CompoundCurve2D : ICurve2D
{
    public IArray<ICurve2D> Curves { get; init; }
}
