namespace Ara3D.SinglePrecision
{
    public interface Any
    {
        Array<String> FieldNames { get; }
        Array<Dynamic> FieldValues { get; }
        String TypeName { get; }
    }
    public interface Value<Self>: Any, Equatable<Self>
    {
    }
    public interface Numerical<Self>: Value<Self>, Betweenable<Self>, Equatable<Self>, Interpolatable<Self>
    {
        Self Zero { get; }
        Self One { get; }
        Self MinValue { get; }
        Self MaxValue { get; }
    }
    public interface Real<Self>: Numerical<Self>, Comparable<Self>, ScalarArithmetic<Self>, Difference<Self, Number>
    {
        Number Value { get; }
    }
    public interface Array<T>
    {
        Integer Count { get; }
        T At(Integer n);
    }
    public interface Array2D<T>: Array<T>
    {
        Integer RowCount { get; }
        Integer ColumnCount { get; }
        T At(Integer column, Integer row);
    }
    public interface Array3D<T>: Array<T>
    {
        Integer RowCount { get; }
        Integer ColumnCount { get; }
        Integer LayerCount { get; }
        T At(Integer column, Integer row, Integer layer);
    }
    public interface Measure<Self>: Real<Self>, Difference<Self, Number>
    {
    }
    public interface Vector<Self>: Array<Number>, Numerical<Self>, Arithmetic<Self>, ScalarArithmetic<Self>
    {
        Number Magnitude { get; }
    }
    public interface Coordinate<Self>: Value<Self>, Interpolatable<Self>, Betweenable<Self>
    {
    }
    public interface WholeNumber<Self>: Numerical<Self>, Comparable<Self>, AdditiveArithmetic<Self, Self>, MultiplicativeArithmetic<Self, Self>, AdditiveInverse<Self>
    {
        Integer Value { get; }
    }
    public interface Comparable<Self>
    {
        Integer Compare(Self y);
    }
    public interface Equatable<Self>
    {
        Boolean Equals(Self b);
        Boolean NotEquals(Self b);
    }
    public interface Arithmetic<Self>: AdditiveArithmetic<Self, Self>, MultiplicativeArithmetic<Self, Self>, AdditiveInverse<Self>, MultiplicativeInverse<Self>
    {
    }
    public interface AdditiveInverse<Self>
    {
        Self Negative { get; }
    }
    public interface MultiplicativeInverse<Self>
    {
        Self Reciprocal { get; }
    }
    public interface AdditiveArithmetic<Self, T>
    {
        Self Add(T other);
        Self Subtract(T other);
    }
    public interface Difference<Self, T>: AdditiveArithmetic<Self, T>
    {
        T Subtract(Self other);
    }
    public interface MultiplicativeArithmetic<Self, T>
    {
        Self Multiply(T other);
        Self Divide(T other);
        Self Modulo(T other);
    }
    public interface ScalarArithmetic<Self>: AdditiveInverse<Self>, MultiplicativeInverse<Self>
    {
        Self Add(Number other);
        Self Subtract(Number other);
        Self Multiply(Number other);
        Self Divide(Number other);
        Self Modulo(Number other);
    }
    public interface BooleanOperations<Self>
    {
        Self And(Self b);
        Self Or(Self b);
        Self Not { get; }
    }
    public interface Interval<Self, TValue, TSize>: Equatable<Self>, Value<Self>
    {
        TValue Min { get; }
        TValue Max { get; }
        TSize Size { get; }
    }
    public interface Interpolatable<Self>
    {
        Self Lerp(Self b, Number amount);
    }
    public interface Betweenable<Self>
    {
        Boolean Between(Self a, Self b);
        Self Clamp(Self a, Self b);
    }
    public interface Bounded2D
    {
        Bounds2D Bounds { get; }
    }
    public interface Bounded3D
    {
        Bounds3D Bounds { get; }
    }
    public interface Transformable2D<Self>
    {
        Self Transform(Matrix2D matrix);
    }
    public interface Transformable3D<Self>
    {
        Self Transform(Matrix3D matrix);
    }
    public interface Deformable2D<Self>
    {
        Self Deform(System.Func<Vector2D, Vector2D> f);
    }
    public interface OpenClosedShape
    {
        Boolean Closed { get; }
    }
    public interface Deformable3D<Self>: Transformable3D<Self>
    {
        Self Deform(System.Func<Vector3D, Vector3D> f);
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
        TRange Eval(TDomain amount);
    }
    public interface Curve<TRange>: Procedural<Number, TRange>, OpenClosedShape
    {
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
        Boolean PeriodicU { get; }
        Boolean PeriodicV { get; }
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
        Boolean Eval(TDomain amount, TDomain epsilon);
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
        Array<Point2D> Points { get; }
    }
    public interface Points3D: Geometry3D
    {
        Array<Point3D> Points { get; }
    }
    public interface BezierPatch: Points3D, Surface, Array2D<Point3D>
    {
    }
    public interface PolyhederalFace
    {
        Integer FaceIndex { get; }
        Array<Integer> VertexIndices { get; }
        Polyhedron Polyhedron { get; }
    }
    public interface Polyhedron: Surface, Points3D
    {
        Array<PolyhederalFace> Faces { get; }
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
        Boolean ClosedX { get; }
        Boolean ClosedY { get; }
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
    public readonly partial struct Point2D: Coordinate<Point2D>, Difference<Point2D, Vector2D>
    {
        public readonly Number X;
        public readonly Number Y;
        public Point2D WithX(Number x) => (x, Y);
        public Point2D WithY(Number y) => (X, y);
        public Point2D(Number x, Number y) => (X, Y) = (x, y);
        public static Point2D Default = new Point2D();
        public static Point2D New(Number x, Number y) => new Point2D(x, y);
        public Ara3D.DoublePrecision.Point2D ChangePrecision() => (X.ChangePrecision(), Y.ChangePrecision());
        public static implicit operator Ara3D.DoublePrecision.Point2D(Point2D self) => self.ChangePrecision();
        public static implicit operator (Number, Number)(Point2D self) => (self.X, self.Y);
        public static implicit operator Point2D((Number, Number) value) => new Point2D(value.Item1, value.Item2);
        public void Deconstruct(out Number x, out Number y) { x = X; y = Y; }
        public override bool Equals(object obj) { if (!(obj is Point2D)) return false; var other = (Point2D)obj; return X.Equals(other.X) && Y.Equals(other.Y); }
        public override int GetHashCode() => Intrinsics.CombineHashCodes(X, Y);
        public override string ToString() => Intrinsics.MakeString(TypeName, FieldNames, FieldValues);
        public static implicit operator Dynamic(Point2D self) => new Dynamic(self);
        public static implicit operator Point2D(Dynamic value) => value.As<Point2D>();
        public String TypeName => "Point2D";
        public Array<String> FieldNames => Intrinsics.MakeArray<String>((String)"X", (String)"Y");
        public Array<Dynamic> FieldValues => Intrinsics.MakeArray<Dynamic>(new Dynamic(X), new Dynamic(Y));
        // Unimplemented concept functions
        public Boolean Between(Point2D a, Point2D b) => (X.Between(a.X, b.X) & Y.Between(a.Y, b.Y));
        public Point2D Clamp(Point2D a, Point2D b) => (X.Clamp(a.X, b.X), Y.Clamp(a.Y, b.Y));
        public Point2D Lerp(Point2D b, Number amount) => (X.Lerp(b.X, amount), Y.Lerp(b.Y, amount));
        public Boolean Equals(Point2D b) => (X.Equals(b.X) & Y.Equals(b.Y));
        public static Boolean operator ==(Point2D a, Point2D b) => a.Equals(b);
        public Boolean NotEquals(Point2D b) => (X.NotEquals(b.X) & Y.NotEquals(b.Y));
        public static Boolean operator !=(Point2D a, Point2D b) => a.NotEquals(b);
    }
    public readonly partial struct Transform2D: Value<Transform2D>
    {
        public readonly Vector2D Translation;
        public readonly Angle Rotation;
        public readonly Vector2D Scale;
        public Transform2D WithTranslation(Vector2D translation) => (translation, Rotation, Scale);
        public Transform2D WithRotation(Angle rotation) => (Translation, rotation, Scale);
        public Transform2D WithScale(Vector2D scale) => (Translation, Rotation, scale);
        public Transform2D(Vector2D translation, Angle rotation, Vector2D scale) => (Translation, Rotation, Scale) = (translation, rotation, scale);
        public static Transform2D Default = new Transform2D();
        public static Transform2D New(Vector2D translation, Angle rotation, Vector2D scale) => new Transform2D(translation, rotation, scale);
        public Ara3D.DoublePrecision.Transform2D ChangePrecision() => (Translation.ChangePrecision(), Rotation.ChangePrecision(), Scale.ChangePrecision());
        public static implicit operator Ara3D.DoublePrecision.Transform2D(Transform2D self) => self.ChangePrecision();
        public static implicit operator (Vector2D, Angle, Vector2D)(Transform2D self) => (self.Translation, self.Rotation, self.Scale);
        public static implicit operator Transform2D((Vector2D, Angle, Vector2D) value) => new Transform2D(value.Item1, value.Item2, value.Item3);
        public void Deconstruct(out Vector2D translation, out Angle rotation, out Vector2D scale) { translation = Translation; rotation = Rotation; scale = Scale; }
        public override bool Equals(object obj) { if (!(obj is Transform2D)) return false; var other = (Transform2D)obj; return Translation.Equals(other.Translation) && Rotation.Equals(other.Rotation) && Scale.Equals(other.Scale); }
        public override int GetHashCode() => Intrinsics.CombineHashCodes(Translation, Rotation, Scale);
        public override string ToString() => Intrinsics.MakeString(TypeName, FieldNames, FieldValues);
        public static implicit operator Dynamic(Transform2D self) => new Dynamic(self);
        public static implicit operator Transform2D(Dynamic value) => value.As<Transform2D>();
        public String TypeName => "Transform2D";
        public Array<String> FieldNames => Intrinsics.MakeArray<String>((String)"Translation", (String)"Rotation", (String)"Scale");
        public Array<Dynamic> FieldValues => Intrinsics.MakeArray<Dynamic>(new Dynamic(Translation), new Dynamic(Rotation), new Dynamic(Scale));
        // Unimplemented concept functions
        public Boolean Equals(Transform2D b) => (Translation.Equals(b.Translation) & Rotation.Equals(b.Rotation) & Scale.Equals(b.Scale));
        public static Boolean operator ==(Transform2D a, Transform2D b) => a.Equals(b);
        public Boolean NotEquals(Transform2D b) => (Translation.NotEquals(b.Translation) & Rotation.NotEquals(b.Rotation) & Scale.NotEquals(b.Scale));
        public static Boolean operator !=(Transform2D a, Transform2D b) => a.NotEquals(b);
    }
    public readonly partial struct Pose2D: Value<Pose2D>
    {
        public readonly Vector2D Position;
        public readonly Angle Orientation;
        public Pose2D WithPosition(Vector2D position) => (position, Orientation);
        public Pose2D WithOrientation(Angle orientation) => (Position, orientation);
        public Pose2D(Vector2D position, Angle orientation) => (Position, Orientation) = (position, orientation);
        public static Pose2D Default = new Pose2D();
        public static Pose2D New(Vector2D position, Angle orientation) => new Pose2D(position, orientation);
        public Ara3D.DoublePrecision.Pose2D ChangePrecision() => (Position.ChangePrecision(), Orientation.ChangePrecision());
        public static implicit operator Ara3D.DoublePrecision.Pose2D(Pose2D self) => self.ChangePrecision();
        public static implicit operator (Vector2D, Angle)(Pose2D self) => (self.Position, self.Orientation);
        public static implicit operator Pose2D((Vector2D, Angle) value) => new Pose2D(value.Item1, value.Item2);
        public void Deconstruct(out Vector2D position, out Angle orientation) { position = Position; orientation = Orientation; }
        public override bool Equals(object obj) { if (!(obj is Pose2D)) return false; var other = (Pose2D)obj; return Position.Equals(other.Position) && Orientation.Equals(other.Orientation); }
        public override int GetHashCode() => Intrinsics.CombineHashCodes(Position, Orientation);
        public override string ToString() => Intrinsics.MakeString(TypeName, FieldNames, FieldValues);
        public static implicit operator Dynamic(Pose2D self) => new Dynamic(self);
        public static implicit operator Pose2D(Dynamic value) => value.As<Pose2D>();
        public String TypeName => "Pose2D";
        public Array<String> FieldNames => Intrinsics.MakeArray<String>((String)"Position", (String)"Orientation");
        public Array<Dynamic> FieldValues => Intrinsics.MakeArray<Dynamic>(new Dynamic(Position), new Dynamic(Orientation));
        // Unimplemented concept functions
        public Boolean Equals(Pose2D b) => (Position.Equals(b.Position) & Orientation.Equals(b.Orientation));
        public static Boolean operator ==(Pose2D a, Pose2D b) => a.Equals(b);
        public Boolean NotEquals(Pose2D b) => (Position.NotEquals(b.Position) & Orientation.NotEquals(b.Orientation));
        public static Boolean operator !=(Pose2D a, Pose2D b) => a.NotEquals(b);
    }
    public readonly partial struct Bounds2D: Interval<Bounds2D, Point2D, Vector2D>
    {
        public readonly Point2D Min;
        public readonly Point2D Max;
        public Bounds2D WithMin(Point2D min) => (min, Max);
        public Bounds2D WithMax(Point2D max) => (Min, max);
        public Bounds2D(Point2D min, Point2D max) => (Min, Max) = (min, max);
        public static Bounds2D Default = new Bounds2D();
        public static Bounds2D New(Point2D min, Point2D max) => new Bounds2D(min, max);
        public Ara3D.DoublePrecision.Bounds2D ChangePrecision() => (Min.ChangePrecision(), Max.ChangePrecision());
        public static implicit operator Ara3D.DoublePrecision.Bounds2D(Bounds2D self) => self.ChangePrecision();
        public static implicit operator (Point2D, Point2D)(Bounds2D self) => (self.Min, self.Max);
        public static implicit operator Bounds2D((Point2D, Point2D) value) => new Bounds2D(value.Item1, value.Item2);
        public void Deconstruct(out Point2D min, out Point2D max) { min = Min; max = Max; }
        public override bool Equals(object obj) { if (!(obj is Bounds2D)) return false; var other = (Bounds2D)obj; return Min.Equals(other.Min) && Max.Equals(other.Max); }
        public override int GetHashCode() => Intrinsics.CombineHashCodes(Min, Max);
        public override string ToString() => Intrinsics.MakeString(TypeName, FieldNames, FieldValues);
        public static implicit operator Dynamic(Bounds2D self) => new Dynamic(self);
        public static implicit operator Bounds2D(Dynamic value) => value.As<Bounds2D>();
        public String TypeName => "Bounds2D";
        public Array<String> FieldNames => Intrinsics.MakeArray<String>((String)"Min", (String)"Max");
        public Array<Dynamic> FieldValues => Intrinsics.MakeArray<Dynamic>(new Dynamic(Min), new Dynamic(Max));
        Point2D Interval<Bounds2D, Point2D, Vector2D>.Min => Min;
        Point2D Interval<Bounds2D, Point2D, Vector2D>.Max => Max;
        // Unimplemented concept functions
        public Boolean Equals(Bounds2D b) => (Min.Equals(b.Min) & Max.Equals(b.Max));
        public static Boolean operator ==(Bounds2D a, Bounds2D b) => a.Equals(b);
        public Boolean NotEquals(Bounds2D b) => (Min.NotEquals(b.Min) & Max.NotEquals(b.Max));
        public static Boolean operator !=(Bounds2D a, Bounds2D b) => a.NotEquals(b);
    }
    public readonly partial struct Ray2D: Value<Ray2D>
    {
        public readonly Vector2D Direction;
        public readonly Point2D Position;
        public Ray2D WithDirection(Vector2D direction) => (direction, Position);
        public Ray2D WithPosition(Point2D position) => (Direction, position);
        public Ray2D(Vector2D direction, Point2D position) => (Direction, Position) = (direction, position);
        public static Ray2D Default = new Ray2D();
        public static Ray2D New(Vector2D direction, Point2D position) => new Ray2D(direction, position);
        public Ara3D.DoublePrecision.Ray2D ChangePrecision() => (Direction.ChangePrecision(), Position.ChangePrecision());
        public static implicit operator Ara3D.DoublePrecision.Ray2D(Ray2D self) => self.ChangePrecision();
        public static implicit operator (Vector2D, Point2D)(Ray2D self) => (self.Direction, self.Position);
        public static implicit operator Ray2D((Vector2D, Point2D) value) => new Ray2D(value.Item1, value.Item2);
        public void Deconstruct(out Vector2D direction, out Point2D position) { direction = Direction; position = Position; }
        public override bool Equals(object obj) { if (!(obj is Ray2D)) return false; var other = (Ray2D)obj; return Direction.Equals(other.Direction) && Position.Equals(other.Position); }
        public override int GetHashCode() => Intrinsics.CombineHashCodes(Direction, Position);
        public override string ToString() => Intrinsics.MakeString(TypeName, FieldNames, FieldValues);
        public static implicit operator Dynamic(Ray2D self) => new Dynamic(self);
        public static implicit operator Ray2D(Dynamic value) => value.As<Ray2D>();
        public String TypeName => "Ray2D";
        public Array<String> FieldNames => Intrinsics.MakeArray<String>((String)"Direction", (String)"Position");
        public Array<Dynamic> FieldValues => Intrinsics.MakeArray<Dynamic>(new Dynamic(Direction), new Dynamic(Position));
        // Unimplemented concept functions
        public Boolean Equals(Ray2D b) => (Direction.Equals(b.Direction) & Position.Equals(b.Position));
        public static Boolean operator ==(Ray2D a, Ray2D b) => a.Equals(b);
        public Boolean NotEquals(Ray2D b) => (Direction.NotEquals(b.Direction) & Position.NotEquals(b.Position));
        public static Boolean operator !=(Ray2D a, Ray2D b) => a.NotEquals(b);
    }
    public readonly partial struct Sphere: Value<Sphere>
    {
        public readonly Point3D Center;
        public readonly Number Radius;
        public Sphere WithCenter(Point3D center) => (center, Radius);
        public Sphere WithRadius(Number radius) => (Center, radius);
        public Sphere(Point3D center, Number radius) => (Center, Radius) = (center, radius);
        public static Sphere Default = new Sphere();
        public static Sphere New(Point3D center, Number radius) => new Sphere(center, radius);
        public Ara3D.DoublePrecision.Sphere ChangePrecision() => (Center.ChangePrecision(), Radius.ChangePrecision());
        public static implicit operator Ara3D.DoublePrecision.Sphere(Sphere self) => self.ChangePrecision();
        public static implicit operator (Point3D, Number)(Sphere self) => (self.Center, self.Radius);
        public static implicit operator Sphere((Point3D, Number) value) => new Sphere(value.Item1, value.Item2);
        public void Deconstruct(out Point3D center, out Number radius) { center = Center; radius = Radius; }
        public override bool Equals(object obj) { if (!(obj is Sphere)) return false; var other = (Sphere)obj; return Center.Equals(other.Center) && Radius.Equals(other.Radius); }
        public override int GetHashCode() => Intrinsics.CombineHashCodes(Center, Radius);
        public override string ToString() => Intrinsics.MakeString(TypeName, FieldNames, FieldValues);
        public static implicit operator Dynamic(Sphere self) => new Dynamic(self);
        public static implicit operator Sphere(Dynamic value) => value.As<Sphere>();
        public String TypeName => "Sphere";
        public Array<String> FieldNames => Intrinsics.MakeArray<String>((String)"Center", (String)"Radius");
        public Array<Dynamic> FieldValues => Intrinsics.MakeArray<Dynamic>(new Dynamic(Center), new Dynamic(Radius));
        // Unimplemented concept functions
        public Boolean Equals(Sphere b) => (Center.Equals(b.Center) & Radius.Equals(b.Radius));
        public static Boolean operator ==(Sphere a, Sphere b) => a.Equals(b);
        public Boolean NotEquals(Sphere b) => (Center.NotEquals(b.Center) & Radius.NotEquals(b.Radius));
        public static Boolean operator !=(Sphere a, Sphere b) => a.NotEquals(b);
    }
    public readonly partial struct Plane: Value<Plane>
    {
        public readonly Vector3D Normal;
        public readonly Number D;
        public Plane WithNormal(Vector3D normal) => (normal, D);
        public Plane WithD(Number d) => (Normal, d);
        public Plane(Vector3D normal, Number d) => (Normal, D) = (normal, d);
        public static Plane Default = new Plane();
        public static Plane New(Vector3D normal, Number d) => new Plane(normal, d);
        public Ara3D.DoublePrecision.Plane ChangePrecision() => (Normal.ChangePrecision(), D.ChangePrecision());
        public static implicit operator Ara3D.DoublePrecision.Plane(Plane self) => self.ChangePrecision();
        public static implicit operator (Vector3D, Number)(Plane self) => (self.Normal, self.D);
        public static implicit operator Plane((Vector3D, Number) value) => new Plane(value.Item1, value.Item2);
        public void Deconstruct(out Vector3D normal, out Number d) { normal = Normal; d = D; }
        public override bool Equals(object obj) { if (!(obj is Plane)) return false; var other = (Plane)obj; return Normal.Equals(other.Normal) && D.Equals(other.D); }
        public override int GetHashCode() => Intrinsics.CombineHashCodes(Normal, D);
        public override string ToString() => Intrinsics.MakeString(TypeName, FieldNames, FieldValues);
        public static implicit operator Dynamic(Plane self) => new Dynamic(self);
        public static implicit operator Plane(Dynamic value) => value.As<Plane>();
        public String TypeName => "Plane";
        public Array<String> FieldNames => Intrinsics.MakeArray<String>((String)"Normal", (String)"D");
        public Array<Dynamic> FieldValues => Intrinsics.MakeArray<Dynamic>(new Dynamic(Normal), new Dynamic(D));
        // Unimplemented concept functions
        public Boolean Equals(Plane b) => (Normal.Equals(b.Normal) & D.Equals(b.D));
        public static Boolean operator ==(Plane a, Plane b) => a.Equals(b);
        public Boolean NotEquals(Plane b) => (Normal.NotEquals(b.Normal) & D.NotEquals(b.D));
        public static Boolean operator !=(Plane a, Plane b) => a.NotEquals(b);
    }
    public readonly partial struct Triangle2D: Value<Triangle2D>, Array<Point2D>
    {
        public readonly Point2D A;
        public readonly Point2D B;
        public readonly Point2D C;
        public Triangle2D WithA(Point2D a) => (a, B, C);
        public Triangle2D WithB(Point2D b) => (A, b, C);
        public Triangle2D WithC(Point2D c) => (A, B, c);
        public Triangle2D(Point2D a, Point2D b, Point2D c) => (A, B, C) = (a, b, c);
        public static Triangle2D Default = new Triangle2D();
        public static Triangle2D New(Point2D a, Point2D b, Point2D c) => new Triangle2D(a, b, c);
        public Ara3D.DoublePrecision.Triangle2D ChangePrecision() => (A.ChangePrecision(), B.ChangePrecision(), C.ChangePrecision());
        public static implicit operator Ara3D.DoublePrecision.Triangle2D(Triangle2D self) => self.ChangePrecision();
        public static implicit operator (Point2D, Point2D, Point2D)(Triangle2D self) => (self.A, self.B, self.C);
        public static implicit operator Triangle2D((Point2D, Point2D, Point2D) value) => new Triangle2D(value.Item1, value.Item2, value.Item3);
        public void Deconstruct(out Point2D a, out Point2D b, out Point2D c) { a = A; b = B; c = C; }
        public override bool Equals(object obj) { if (!(obj is Triangle2D)) return false; var other = (Triangle2D)obj; return A.Equals(other.A) && B.Equals(other.B) && C.Equals(other.C); }
        public override int GetHashCode() => Intrinsics.CombineHashCodes(A, B, C);
        public override string ToString() => Intrinsics.MakeString(TypeName, FieldNames, FieldValues);
        public static implicit operator Dynamic(Triangle2D self) => new Dynamic(self);
        public static implicit operator Triangle2D(Dynamic value) => value.As<Triangle2D>();
        public String TypeName => "Triangle2D";
        public Array<String> FieldNames => Intrinsics.MakeArray<String>((String)"A", (String)"B", (String)"C");
        public Array<Dynamic> FieldValues => Intrinsics.MakeArray<Dynamic>(new Dynamic(A), new Dynamic(B), new Dynamic(C));
        // Unimplemented concept functions
        public Boolean Equals(Triangle2D b) => (A.Equals(b.A) & B.Equals(b.B) & C.Equals(b.C));
        public static Boolean operator ==(Triangle2D a, Triangle2D b) => a.Equals(b);
        public Boolean NotEquals(Triangle2D b) => (A.NotEquals(b.A) & B.NotEquals(b.B) & C.NotEquals(b.C));
        public static Boolean operator !=(Triangle2D a, Triangle2D b) => a.NotEquals(b);
        public Integer Count => 3;
        public Point2D At(Integer n) => n == 0 ? A : n == 1 ? B : n == 2 ? C : throw new System.IndexOutOfRangeException();
        public Point2D this[Integer n] => At(n);
    }
    public readonly partial struct Quad2D: Value<Quad2D>, Array<Point2D>
    {
        public readonly Point2D A;
        public readonly Point2D B;
        public readonly Point2D C;
        public readonly Point2D D;
        public Quad2D WithA(Point2D a) => (a, B, C, D);
        public Quad2D WithB(Point2D b) => (A, b, C, D);
        public Quad2D WithC(Point2D c) => (A, B, c, D);
        public Quad2D WithD(Point2D d) => (A, B, C, d);
        public Quad2D(Point2D a, Point2D b, Point2D c, Point2D d) => (A, B, C, D) = (a, b, c, d);
        public static Quad2D Default = new Quad2D();
        public static Quad2D New(Point2D a, Point2D b, Point2D c, Point2D d) => new Quad2D(a, b, c, d);
        public Ara3D.DoublePrecision.Quad2D ChangePrecision() => (A.ChangePrecision(), B.ChangePrecision(), C.ChangePrecision(), D.ChangePrecision());
        public static implicit operator Ara3D.DoublePrecision.Quad2D(Quad2D self) => self.ChangePrecision();
        public static implicit operator (Point2D, Point2D, Point2D, Point2D)(Quad2D self) => (self.A, self.B, self.C, self.D);
        public static implicit operator Quad2D((Point2D, Point2D, Point2D, Point2D) value) => new Quad2D(value.Item1, value.Item2, value.Item3, value.Item4);
        public void Deconstruct(out Point2D a, out Point2D b, out Point2D c, out Point2D d) { a = A; b = B; c = C; d = D; }
        public override bool Equals(object obj) { if (!(obj is Quad2D)) return false; var other = (Quad2D)obj; return A.Equals(other.A) && B.Equals(other.B) && C.Equals(other.C) && D.Equals(other.D); }
        public override int GetHashCode() => Intrinsics.CombineHashCodes(A, B, C, D);
        public override string ToString() => Intrinsics.MakeString(TypeName, FieldNames, FieldValues);
        public static implicit operator Dynamic(Quad2D self) => new Dynamic(self);
        public static implicit operator Quad2D(Dynamic value) => value.As<Quad2D>();
        public String TypeName => "Quad2D";
        public Array<String> FieldNames => Intrinsics.MakeArray<String>((String)"A", (String)"B", (String)"C", (String)"D");
        public Array<Dynamic> FieldValues => Intrinsics.MakeArray<Dynamic>(new Dynamic(A), new Dynamic(B), new Dynamic(C), new Dynamic(D));
        // Unimplemented concept functions
        public Boolean Equals(Quad2D b) => (A.Equals(b.A) & B.Equals(b.B) & C.Equals(b.C) & D.Equals(b.D));
        public static Boolean operator ==(Quad2D a, Quad2D b) => a.Equals(b);
        public Boolean NotEquals(Quad2D b) => (A.NotEquals(b.A) & B.NotEquals(b.B) & C.NotEquals(b.C) & D.NotEquals(b.D));
        public static Boolean operator !=(Quad2D a, Quad2D b) => a.NotEquals(b);
        public Integer Count => 4;
        public Point2D At(Integer n) => n == 0 ? A : n == 1 ? B : n == 2 ? C : n == 3 ? D : throw new System.IndexOutOfRangeException();
        public Point2D this[Integer n] => At(n);
    }
    public readonly partial struct Line2D: PolygonalChain2D, Array<Point2D>
    {
        public readonly Point2D A;
        public readonly Point2D B;
        public Line2D WithA(Point2D a) => (a, B);
        public Line2D WithB(Point2D b) => (A, b);
        public Line2D(Point2D a, Point2D b) => (A, B) = (a, b);
        public static Line2D Default = new Line2D();
        public static Line2D New(Point2D a, Point2D b) => new Line2D(a, b);
        public Ara3D.DoublePrecision.Line2D ChangePrecision() => (A.ChangePrecision(), B.ChangePrecision());
        public static implicit operator Ara3D.DoublePrecision.Line2D(Line2D self) => self.ChangePrecision();
        public static implicit operator (Point2D, Point2D)(Line2D self) => (self.A, self.B);
        public static implicit operator Line2D((Point2D, Point2D) value) => new Line2D(value.Item1, value.Item2);
        public void Deconstruct(out Point2D a, out Point2D b) { a = A; b = B; }
        public override bool Equals(object obj) { if (!(obj is Line2D)) return false; var other = (Line2D)obj; return A.Equals(other.A) && B.Equals(other.B); }
        public override int GetHashCode() => Intrinsics.CombineHashCodes(A, B);
        public override string ToString() => Intrinsics.MakeString(TypeName, FieldNames, FieldValues);
        public static implicit operator Dynamic(Line2D self) => new Dynamic(self);
        public static implicit operator Line2D(Dynamic value) => value.As<Line2D>();
        public String TypeName => "Line2D";
        public Array<String> FieldNames => Intrinsics.MakeArray<String>((String)"A", (String)"B");
        public Array<Dynamic> FieldValues => Intrinsics.MakeArray<Dynamic>(new Dynamic(A), new Dynamic(B));
        // Unimplemented concept functions
        public Integer Count => 2;
        public Point2D At(Integer n) => n == 0 ? A : n == 1 ? B : throw new System.IndexOutOfRangeException();
        public Point2D this[Integer n] => At(n);
    }
    public readonly partial struct Circle: ClosedShape2D
    {
        public readonly Point2D Center;
        public readonly Number Radius;
        public Circle WithCenter(Point2D center) => (center, Radius);
        public Circle WithRadius(Number radius) => (Center, radius);
        public Circle(Point2D center, Number radius) => (Center, Radius) = (center, radius);
        public static Circle Default = new Circle();
        public static Circle New(Point2D center, Number radius) => new Circle(center, radius);
        public Ara3D.DoublePrecision.Circle ChangePrecision() => (Center.ChangePrecision(), Radius.ChangePrecision());
        public static implicit operator Ara3D.DoublePrecision.Circle(Circle self) => self.ChangePrecision();
        public static implicit operator (Point2D, Number)(Circle self) => (self.Center, self.Radius);
        public static implicit operator Circle((Point2D, Number) value) => new Circle(value.Item1, value.Item2);
        public void Deconstruct(out Point2D center, out Number radius) { center = Center; radius = Radius; }
        public override bool Equals(object obj) { if (!(obj is Circle)) return false; var other = (Circle)obj; return Center.Equals(other.Center) && Radius.Equals(other.Radius); }
        public override int GetHashCode() => Intrinsics.CombineHashCodes(Center, Radius);
        public override string ToString() => Intrinsics.MakeString(TypeName, FieldNames, FieldValues);
        public static implicit operator Dynamic(Circle self) => new Dynamic(self);
        public static implicit operator Circle(Dynamic value) => value.As<Circle>();
        public String TypeName => "Circle";
        public Array<String> FieldNames => Intrinsics.MakeArray<String>((String)"Center", (String)"Radius");
        public Array<Dynamic> FieldValues => Intrinsics.MakeArray<Dynamic>(new Dynamic(Center), new Dynamic(Radius));
        // Unimplemented concept functions
    }
    public readonly partial struct Lens: ClosedShape2D
    {
        public readonly Circle A;
        public readonly Circle B;
        public Lens WithA(Circle a) => (a, B);
        public Lens WithB(Circle b) => (A, b);
        public Lens(Circle a, Circle b) => (A, B) = (a, b);
        public static Lens Default = new Lens();
        public static Lens New(Circle a, Circle b) => new Lens(a, b);
        public Ara3D.DoublePrecision.Lens ChangePrecision() => (A.ChangePrecision(), B.ChangePrecision());
        public static implicit operator Ara3D.DoublePrecision.Lens(Lens self) => self.ChangePrecision();
        public static implicit operator (Circle, Circle)(Lens self) => (self.A, self.B);
        public static implicit operator Lens((Circle, Circle) value) => new Lens(value.Item1, value.Item2);
        public void Deconstruct(out Circle a, out Circle b) { a = A; b = B; }
        public override bool Equals(object obj) { if (!(obj is Lens)) return false; var other = (Lens)obj; return A.Equals(other.A) && B.Equals(other.B); }
        public override int GetHashCode() => Intrinsics.CombineHashCodes(A, B);
        public override string ToString() => Intrinsics.MakeString(TypeName, FieldNames, FieldValues);
        public static implicit operator Dynamic(Lens self) => new Dynamic(self);
        public static implicit operator Lens(Dynamic value) => value.As<Lens>();
        public String TypeName => "Lens";
        public Array<String> FieldNames => Intrinsics.MakeArray<String>((String)"A", (String)"B");
        public Array<Dynamic> FieldValues => Intrinsics.MakeArray<Dynamic>(new Dynamic(A), new Dynamic(B));
        // Unimplemented concept functions
    }
    public readonly partial struct PolygonFace
    {
        public readonly Integer FaceIndex;
        public readonly Array<Integer> PointIndices;
        public PolygonFace WithFaceIndex(Integer faceIndex) => (faceIndex, PointIndices);
        public PolygonFace WithPointIndices(Array<Integer> pointIndices) => (FaceIndex, pointIndices);
        public PolygonFace(Integer faceIndex, Array<Integer> pointIndices) => (FaceIndex, PointIndices) = (faceIndex, pointIndices);
        public static PolygonFace Default = new PolygonFace();
        public static PolygonFace New(Integer faceIndex, Array<Integer> pointIndices) => new PolygonFace(faceIndex, pointIndices);
        public static implicit operator (Integer, Array<Integer>)(PolygonFace self) => (self.FaceIndex, self.PointIndices);
        public static implicit operator PolygonFace((Integer, Array<Integer>) value) => new PolygonFace(value.Item1, value.Item2);
        public void Deconstruct(out Integer faceIndex, out Array<Integer> pointIndices) { faceIndex = FaceIndex; pointIndices = PointIndices; }
        public override bool Equals(object obj) { if (!(obj is PolygonFace)) return false; var other = (PolygonFace)obj; return FaceIndex.Equals(other.FaceIndex) && PointIndices.Equals(other.PointIndices); }
        public override int GetHashCode() => Intrinsics.CombineHashCodes(FaceIndex, PointIndices);
        public override string ToString() => Intrinsics.MakeString(TypeName, FieldNames, FieldValues);
        public static implicit operator Dynamic(PolygonFace self) => new Dynamic(self);
        public static implicit operator PolygonFace(Dynamic value) => value.As<PolygonFace>();
        public String TypeName => "PolygonFace";
        public Array<String> FieldNames => Intrinsics.MakeArray<String>((String)"FaceIndex", (String)"PointIndices");
        public Array<Dynamic> FieldValues => Intrinsics.MakeArray<Dynamic>(new Dynamic(FaceIndex), new Dynamic(PointIndices));
        // Unimplemented concept functions
    }
    public readonly partial struct Rect2D: Polygon2D
    {
        public readonly Point2D Center;
        public readonly Size2D Size;
        public Rect2D WithCenter(Point2D center) => (center, Size);
        public Rect2D WithSize(Size2D size) => (Center, size);
        public Rect2D(Point2D center, Size2D size) => (Center, Size) = (center, size);
        public static Rect2D Default = new Rect2D();
        public static Rect2D New(Point2D center, Size2D size) => new Rect2D(center, size);
        public Ara3D.DoublePrecision.Rect2D ChangePrecision() => (Center.ChangePrecision(), Size.ChangePrecision());
        public static implicit operator Ara3D.DoublePrecision.Rect2D(Rect2D self) => self.ChangePrecision();
        public static implicit operator (Point2D, Size2D)(Rect2D self) => (self.Center, self.Size);
        public static implicit operator Rect2D((Point2D, Size2D) value) => new Rect2D(value.Item1, value.Item2);
        public void Deconstruct(out Point2D center, out Size2D size) { center = Center; size = Size; }
        public override bool Equals(object obj) { if (!(obj is Rect2D)) return false; var other = (Rect2D)obj; return Center.Equals(other.Center) && Size.Equals(other.Size); }
        public override int GetHashCode() => Intrinsics.CombineHashCodes(Center, Size);
        public override string ToString() => Intrinsics.MakeString(TypeName, FieldNames, FieldValues);
        public static implicit operator Dynamic(Rect2D self) => new Dynamic(self);
        public static implicit operator Rect2D(Dynamic value) => value.As<Rect2D>();
        public String TypeName => "Rect2D";
        public Array<String> FieldNames => Intrinsics.MakeArray<String>((String)"Center", (String)"Size");
        public Array<Dynamic> FieldValues => Intrinsics.MakeArray<Dynamic>(new Dynamic(Center), new Dynamic(Size));
        // Unimplemented concept functions
    }
    public readonly partial struct Ellipse: Curve2D
    {
        public readonly Point2D Center;
        public readonly Size2D Size;
        public Ellipse WithCenter(Point2D center) => (center, Size);
        public Ellipse WithSize(Size2D size) => (Center, size);
        public Ellipse(Point2D center, Size2D size) => (Center, Size) = (center, size);
        public static Ellipse Default = new Ellipse();
        public static Ellipse New(Point2D center, Size2D size) => new Ellipse(center, size);
        public Ara3D.DoublePrecision.Ellipse ChangePrecision() => (Center.ChangePrecision(), Size.ChangePrecision());
        public static implicit operator Ara3D.DoublePrecision.Ellipse(Ellipse self) => self.ChangePrecision();
        public static implicit operator (Point2D, Size2D)(Ellipse self) => (self.Center, self.Size);
        public static implicit operator Ellipse((Point2D, Size2D) value) => new Ellipse(value.Item1, value.Item2);
        public void Deconstruct(out Point2D center, out Size2D size) { center = Center; size = Size; }
        public override bool Equals(object obj) { if (!(obj is Ellipse)) return false; var other = (Ellipse)obj; return Center.Equals(other.Center) && Size.Equals(other.Size); }
        public override int GetHashCode() => Intrinsics.CombineHashCodes(Center, Size);
        public override string ToString() => Intrinsics.MakeString(TypeName, FieldNames, FieldValues);
        public static implicit operator Dynamic(Ellipse self) => new Dynamic(self);
        public static implicit operator Ellipse(Dynamic value) => value.As<Ellipse>();
        public String TypeName => "Ellipse";
        public Array<String> FieldNames => Intrinsics.MakeArray<String>((String)"Center", (String)"Size");
        public Array<Dynamic> FieldValues => Intrinsics.MakeArray<Dynamic>(new Dynamic(Center), new Dynamic(Size));
        // Unimplemented concept functions
    }
    public readonly partial struct Ring: ClosedShape2D
    {
        public readonly Point2D Center;
        public readonly Number InnerRadius;
        public readonly Number OuterRadius;
        public Ring WithCenter(Point2D center) => (center, InnerRadius, OuterRadius);
        public Ring WithInnerRadius(Number innerRadius) => (Center, innerRadius, OuterRadius);
        public Ring WithOuterRadius(Number outerRadius) => (Center, InnerRadius, outerRadius);
        public Ring(Point2D center, Number innerRadius, Number outerRadius) => (Center, InnerRadius, OuterRadius) = (center, innerRadius, outerRadius);
        public static Ring Default = new Ring();
        public static Ring New(Point2D center, Number innerRadius, Number outerRadius) => new Ring(center, innerRadius, outerRadius);
        public Ara3D.DoublePrecision.Ring ChangePrecision() => (Center.ChangePrecision(), InnerRadius.ChangePrecision(), OuterRadius.ChangePrecision());
        public static implicit operator Ara3D.DoublePrecision.Ring(Ring self) => self.ChangePrecision();
        public static implicit operator (Point2D, Number, Number)(Ring self) => (self.Center, self.InnerRadius, self.OuterRadius);
        public static implicit operator Ring((Point2D, Number, Number) value) => new Ring(value.Item1, value.Item2, value.Item3);
        public void Deconstruct(out Point2D center, out Number innerRadius, out Number outerRadius) { center = Center; innerRadius = InnerRadius; outerRadius = OuterRadius; }
        public override bool Equals(object obj) { if (!(obj is Ring)) return false; var other = (Ring)obj; return Center.Equals(other.Center) && InnerRadius.Equals(other.InnerRadius) && OuterRadius.Equals(other.OuterRadius); }
        public override int GetHashCode() => Intrinsics.CombineHashCodes(Center, InnerRadius, OuterRadius);
        public override string ToString() => Intrinsics.MakeString(TypeName, FieldNames, FieldValues);
        public static implicit operator Dynamic(Ring self) => new Dynamic(self);
        public static implicit operator Ring(Dynamic value) => value.As<Ring>();
        public String TypeName => "Ring";
        public Array<String> FieldNames => Intrinsics.MakeArray<String>((String)"Center", (String)"InnerRadius", (String)"OuterRadius");
        public Array<Dynamic> FieldValues => Intrinsics.MakeArray<Dynamic>(new Dynamic(Center), new Dynamic(InnerRadius), new Dynamic(OuterRadius));
        // Unimplemented concept functions
    }
    public readonly partial struct Arc: OpenShape2D
    {
        public readonly AnglePair Angles;
        public readonly Circle Circle;
        public Arc WithAngles(AnglePair angles) => (angles, Circle);
        public Arc WithCircle(Circle circle) => (Angles, circle);
        public Arc(AnglePair angles, Circle circle) => (Angles, Circle) = (angles, circle);
        public static Arc Default = new Arc();
        public static Arc New(AnglePair angles, Circle circle) => new Arc(angles, circle);
        public Ara3D.DoublePrecision.Arc ChangePrecision() => (Angles.ChangePrecision(), Circle.ChangePrecision());
        public static implicit operator Ara3D.DoublePrecision.Arc(Arc self) => self.ChangePrecision();
        public static implicit operator (AnglePair, Circle)(Arc self) => (self.Angles, self.Circle);
        public static implicit operator Arc((AnglePair, Circle) value) => new Arc(value.Item1, value.Item2);
        public void Deconstruct(out AnglePair angles, out Circle circle) { angles = Angles; circle = Circle; }
        public override bool Equals(object obj) { if (!(obj is Arc)) return false; var other = (Arc)obj; return Angles.Equals(other.Angles) && Circle.Equals(other.Circle); }
        public override int GetHashCode() => Intrinsics.CombineHashCodes(Angles, Circle);
        public override string ToString() => Intrinsics.MakeString(TypeName, FieldNames, FieldValues);
        public static implicit operator Dynamic(Arc self) => new Dynamic(self);
        public static implicit operator Arc(Dynamic value) => value.As<Arc>();
        public String TypeName => "Arc";
        public Array<String> FieldNames => Intrinsics.MakeArray<String>((String)"Angles", (String)"Circle");
        public Array<Dynamic> FieldValues => Intrinsics.MakeArray<Dynamic>(new Dynamic(Angles), new Dynamic(Circle));
        // Unimplemented concept functions
    }
    public readonly partial struct Sector: ClosedShape2D
    {
        public readonly Arc Arc;
        public Sector WithArc(Arc arc) => (arc);
        public Sector(Arc arc) => (Arc) = (arc);
        public static Sector Default = new Sector();
        public static Sector New(Arc arc) => new Sector(arc);
        public Ara3D.DoublePrecision.Sector ChangePrecision() => (Arc.ChangePrecision());
        public static implicit operator Ara3D.DoublePrecision.Sector(Sector self) => self.ChangePrecision();
        public static implicit operator Arc(Sector self) => self.Arc;
        public static implicit operator Sector(Arc value) => new Sector(value);
        public override bool Equals(object obj) { if (!(obj is Sector)) return false; var other = (Sector)obj; return Arc.Equals(other.Arc); }
        public override int GetHashCode() => Intrinsics.CombineHashCodes(Arc);
        public override string ToString() => Intrinsics.MakeString(TypeName, FieldNames, FieldValues);
        public static implicit operator Dynamic(Sector self) => new Dynamic(self);
        public static implicit operator Sector(Dynamic value) => value.As<Sector>();
        public String TypeName => "Sector";
        public Array<String> FieldNames => Intrinsics.MakeArray<String>((String)"Arc");
        public Array<Dynamic> FieldValues => Intrinsics.MakeArray<Dynamic>(new Dynamic(Arc));
        // Unimplemented concept functions
    }
    public readonly partial struct Chord: ClosedShape2D
    {
        public readonly Arc Arc;
        public Chord WithArc(Arc arc) => (arc);
        public Chord(Arc arc) => (Arc) = (arc);
        public static Chord Default = new Chord();
        public static Chord New(Arc arc) => new Chord(arc);
        public Ara3D.DoublePrecision.Chord ChangePrecision() => (Arc.ChangePrecision());
        public static implicit operator Ara3D.DoublePrecision.Chord(Chord self) => self.ChangePrecision();
        public static implicit operator Arc(Chord self) => self.Arc;
        public static implicit operator Chord(Arc value) => new Chord(value);
        public override bool Equals(object obj) { if (!(obj is Chord)) return false; var other = (Chord)obj; return Arc.Equals(other.Arc); }
        public override int GetHashCode() => Intrinsics.CombineHashCodes(Arc);
        public override string ToString() => Intrinsics.MakeString(TypeName, FieldNames, FieldValues);
        public static implicit operator Dynamic(Chord self) => new Dynamic(self);
        public static implicit operator Chord(Dynamic value) => value.As<Chord>();
        public String TypeName => "Chord";
        public Array<String> FieldNames => Intrinsics.MakeArray<String>((String)"Arc");
        public Array<Dynamic> FieldValues => Intrinsics.MakeArray<Dynamic>(new Dynamic(Arc));
        // Unimplemented concept functions
    }
    public readonly partial struct Segment: ClosedShape2D
    {
        public readonly Arc Arc;
        public Segment WithArc(Arc arc) => (arc);
        public Segment(Arc arc) => (Arc) = (arc);
        public static Segment Default = new Segment();
        public static Segment New(Arc arc) => new Segment(arc);
        public Ara3D.DoublePrecision.Segment ChangePrecision() => (Arc.ChangePrecision());
        public static implicit operator Ara3D.DoublePrecision.Segment(Segment self) => self.ChangePrecision();
        public static implicit operator Arc(Segment self) => self.Arc;
        public static implicit operator Segment(Arc value) => new Segment(value);
        public override bool Equals(object obj) { if (!(obj is Segment)) return false; var other = (Segment)obj; return Arc.Equals(other.Arc); }
        public override int GetHashCode() => Intrinsics.CombineHashCodes(Arc);
        public override string ToString() => Intrinsics.MakeString(TypeName, FieldNames, FieldValues);
        public static implicit operator Dynamic(Segment self) => new Dynamic(self);
        public static implicit operator Segment(Dynamic value) => value.As<Segment>();
        public String TypeName => "Segment";
        public Array<String> FieldNames => Intrinsics.MakeArray<String>((String)"Arc");
        public Array<Dynamic> FieldValues => Intrinsics.MakeArray<Dynamic>(new Dynamic(Arc));
        // Unimplemented concept functions
    }
    public readonly partial struct RegularPolygon: Polygon2D
    {
        public readonly Integer NumPoints;
        public RegularPolygon WithNumPoints(Integer numPoints) => (numPoints);
        public RegularPolygon(Integer numPoints) => (NumPoints) = (numPoints);
        public static RegularPolygon Default = new RegularPolygon();
        public static RegularPolygon New(Integer numPoints) => new RegularPolygon(numPoints);
        public Ara3D.DoublePrecision.RegularPolygon ChangePrecision() => (NumPoints.ChangePrecision());
        public static implicit operator Ara3D.DoublePrecision.RegularPolygon(RegularPolygon self) => self.ChangePrecision();
        public static implicit operator Integer(RegularPolygon self) => self.NumPoints;
        public static implicit operator RegularPolygon(Integer value) => new RegularPolygon(value);
        public override bool Equals(object obj) { if (!(obj is RegularPolygon)) return false; var other = (RegularPolygon)obj; return NumPoints.Equals(other.NumPoints); }
        public override int GetHashCode() => Intrinsics.CombineHashCodes(NumPoints);
        public override string ToString() => Intrinsics.MakeString(TypeName, FieldNames, FieldValues);
        public static implicit operator Dynamic(RegularPolygon self) => new Dynamic(self);
        public static implicit operator RegularPolygon(Dynamic value) => value.As<RegularPolygon>();
        public String TypeName => "RegularPolygon";
        public Array<String> FieldNames => Intrinsics.MakeArray<String>((String)"NumPoints");
        public Array<Dynamic> FieldValues => Intrinsics.MakeArray<Dynamic>(new Dynamic(NumPoints));
        // Unimplemented concept functions
    }
    public readonly partial struct Box2D: Shape2D
    {
        public readonly Point2D Center;
        public readonly Angle Rotation;
        public readonly Size2D Extent;
        public Box2D WithCenter(Point2D center) => (center, Rotation, Extent);
        public Box2D WithRotation(Angle rotation) => (Center, rotation, Extent);
        public Box2D WithExtent(Size2D extent) => (Center, Rotation, extent);
        public Box2D(Point2D center, Angle rotation, Size2D extent) => (Center, Rotation, Extent) = (center, rotation, extent);
        public static Box2D Default = new Box2D();
        public static Box2D New(Point2D center, Angle rotation, Size2D extent) => new Box2D(center, rotation, extent);
        public Ara3D.DoublePrecision.Box2D ChangePrecision() => (Center.ChangePrecision(), Rotation.ChangePrecision(), Extent.ChangePrecision());
        public static implicit operator Ara3D.DoublePrecision.Box2D(Box2D self) => self.ChangePrecision();
        public static implicit operator (Point2D, Angle, Size2D)(Box2D self) => (self.Center, self.Rotation, self.Extent);
        public static implicit operator Box2D((Point2D, Angle, Size2D) value) => new Box2D(value.Item1, value.Item2, value.Item3);
        public void Deconstruct(out Point2D center, out Angle rotation, out Size2D extent) { center = Center; rotation = Rotation; extent = Extent; }
        public override bool Equals(object obj) { if (!(obj is Box2D)) return false; var other = (Box2D)obj; return Center.Equals(other.Center) && Rotation.Equals(other.Rotation) && Extent.Equals(other.Extent); }
        public override int GetHashCode() => Intrinsics.CombineHashCodes(Center, Rotation, Extent);
        public override string ToString() => Intrinsics.MakeString(TypeName, FieldNames, FieldValues);
        public static implicit operator Dynamic(Box2D self) => new Dynamic(self);
        public static implicit operator Box2D(Dynamic value) => value.As<Box2D>();
        public String TypeName => "Box2D";
        public Array<String> FieldNames => Intrinsics.MakeArray<String>((String)"Center", (String)"Rotation", (String)"Extent");
        public Array<Dynamic> FieldValues => Intrinsics.MakeArray<Dynamic>(new Dynamic(Center), new Dynamic(Rotation), new Dynamic(Extent));
        // Unimplemented concept functions
    }
    public readonly partial struct Point3D: Coordinate<Point3D>, Difference<Point3D, Vector3D>, Array<Number>
    {
        public readonly Number X;
        public readonly Number Y;
        public readonly Number Z;
        public Point3D WithX(Number x) => (x, Y, Z);
        public Point3D WithY(Number y) => (X, y, Z);
        public Point3D WithZ(Number z) => (X, Y, z);
        public Point3D(Number x, Number y, Number z) => (X, Y, Z) = (x, y, z);
        public static Point3D Default = new Point3D();
        public static Point3D New(Number x, Number y, Number z) => new Point3D(x, y, z);
        public Ara3D.DoublePrecision.Point3D ChangePrecision() => (X.ChangePrecision(), Y.ChangePrecision(), Z.ChangePrecision());
        public static implicit operator Ara3D.DoublePrecision.Point3D(Point3D self) => self.ChangePrecision();
        public static implicit operator (Number, Number, Number)(Point3D self) => (self.X, self.Y, self.Z);
        public static implicit operator Point3D((Number, Number, Number) value) => new Point3D(value.Item1, value.Item2, value.Item3);
        public void Deconstruct(out Number x, out Number y, out Number z) { x = X; y = Y; z = Z; }
        public override bool Equals(object obj) { if (!(obj is Point3D)) return false; var other = (Point3D)obj; return X.Equals(other.X) && Y.Equals(other.Y) && Z.Equals(other.Z); }
        public override int GetHashCode() => Intrinsics.CombineHashCodes(X, Y, Z);
        public override string ToString() => Intrinsics.MakeString(TypeName, FieldNames, FieldValues);
        public static implicit operator Dynamic(Point3D self) => new Dynamic(self);
        public static implicit operator Point3D(Dynamic value) => value.As<Point3D>();
        public String TypeName => "Point3D";
        public Array<String> FieldNames => Intrinsics.MakeArray<String>((String)"X", (String)"Y", (String)"Z");
        public Array<Dynamic> FieldValues => Intrinsics.MakeArray<Dynamic>(new Dynamic(X), new Dynamic(Y), new Dynamic(Z));
        // Unimplemented concept functions
        public Boolean Between(Point3D a, Point3D b) => (X.Between(a.X, b.X) & Y.Between(a.Y, b.Y) & Z.Between(a.Z, b.Z));
        public Point3D Clamp(Point3D a, Point3D b) => (X.Clamp(a.X, b.X), Y.Clamp(a.Y, b.Y), Z.Clamp(a.Z, b.Z));
        public Point3D Lerp(Point3D b, Number amount) => (X.Lerp(b.X, amount), Y.Lerp(b.Y, amount), Z.Lerp(b.Z, amount));
        public Boolean Equals(Point3D b) => (X.Equals(b.X) & Y.Equals(b.Y) & Z.Equals(b.Z));
        public static Boolean operator ==(Point3D a, Point3D b) => a.Equals(b);
        public Boolean NotEquals(Point3D b) => (X.NotEquals(b.X) & Y.NotEquals(b.Y) & Z.NotEquals(b.Z));
        public static Boolean operator !=(Point3D a, Point3D b) => a.NotEquals(b);
        public Integer Count => 3;
        public Number At(Integer n) => n == 0 ? X : n == 1 ? Y : n == 2 ? Z : throw new System.IndexOutOfRangeException();
        public Number this[Integer n] => At(n);
    }
    public readonly partial struct Transform3D: Value<Transform3D>
    {
        public readonly Vector3D Translation;
        public readonly Rotation3D Rotation;
        public readonly Vector3D Scale;
        public Transform3D WithTranslation(Vector3D translation) => (translation, Rotation, Scale);
        public Transform3D WithRotation(Rotation3D rotation) => (Translation, rotation, Scale);
        public Transform3D WithScale(Vector3D scale) => (Translation, Rotation, scale);
        public Transform3D(Vector3D translation, Rotation3D rotation, Vector3D scale) => (Translation, Rotation, Scale) = (translation, rotation, scale);
        public static Transform3D Default = new Transform3D();
        public static Transform3D New(Vector3D translation, Rotation3D rotation, Vector3D scale) => new Transform3D(translation, rotation, scale);
        public Ara3D.DoublePrecision.Transform3D ChangePrecision() => (Translation.ChangePrecision(), Rotation.ChangePrecision(), Scale.ChangePrecision());
        public static implicit operator Ara3D.DoublePrecision.Transform3D(Transform3D self) => self.ChangePrecision();
        public static implicit operator (Vector3D, Rotation3D, Vector3D)(Transform3D self) => (self.Translation, self.Rotation, self.Scale);
        public static implicit operator Transform3D((Vector3D, Rotation3D, Vector3D) value) => new Transform3D(value.Item1, value.Item2, value.Item3);
        public void Deconstruct(out Vector3D translation, out Rotation3D rotation, out Vector3D scale) { translation = Translation; rotation = Rotation; scale = Scale; }
        public override bool Equals(object obj) { if (!(obj is Transform3D)) return false; var other = (Transform3D)obj; return Translation.Equals(other.Translation) && Rotation.Equals(other.Rotation) && Scale.Equals(other.Scale); }
        public override int GetHashCode() => Intrinsics.CombineHashCodes(Translation, Rotation, Scale);
        public override string ToString() => Intrinsics.MakeString(TypeName, FieldNames, FieldValues);
        public static implicit operator Dynamic(Transform3D self) => new Dynamic(self);
        public static implicit operator Transform3D(Dynamic value) => value.As<Transform3D>();
        public String TypeName => "Transform3D";
        public Array<String> FieldNames => Intrinsics.MakeArray<String>((String)"Translation", (String)"Rotation", (String)"Scale");
        public Array<Dynamic> FieldValues => Intrinsics.MakeArray<Dynamic>(new Dynamic(Translation), new Dynamic(Rotation), new Dynamic(Scale));
        // Unimplemented concept functions
        public Boolean Equals(Transform3D b) => (Translation.Equals(b.Translation) & Rotation.Equals(b.Rotation) & Scale.Equals(b.Scale));
        public static Boolean operator ==(Transform3D a, Transform3D b) => a.Equals(b);
        public Boolean NotEquals(Transform3D b) => (Translation.NotEquals(b.Translation) & Rotation.NotEquals(b.Rotation) & Scale.NotEquals(b.Scale));
        public static Boolean operator !=(Transform3D a, Transform3D b) => a.NotEquals(b);
    }
    public readonly partial struct Pose3D: Value<Pose3D>
    {
        public readonly Vector3D Position;
        public readonly Orientation3D Orientation;
        public Pose3D WithPosition(Vector3D position) => (position, Orientation);
        public Pose3D WithOrientation(Orientation3D orientation) => (Position, orientation);
        public Pose3D(Vector3D position, Orientation3D orientation) => (Position, Orientation) = (position, orientation);
        public static Pose3D Default = new Pose3D();
        public static Pose3D New(Vector3D position, Orientation3D orientation) => new Pose3D(position, orientation);
        public Ara3D.DoublePrecision.Pose3D ChangePrecision() => (Position.ChangePrecision(), Orientation.ChangePrecision());
        public static implicit operator Ara3D.DoublePrecision.Pose3D(Pose3D self) => self.ChangePrecision();
        public static implicit operator (Vector3D, Orientation3D)(Pose3D self) => (self.Position, self.Orientation);
        public static implicit operator Pose3D((Vector3D, Orientation3D) value) => new Pose3D(value.Item1, value.Item2);
        public void Deconstruct(out Vector3D position, out Orientation3D orientation) { position = Position; orientation = Orientation; }
        public override bool Equals(object obj) { if (!(obj is Pose3D)) return false; var other = (Pose3D)obj; return Position.Equals(other.Position) && Orientation.Equals(other.Orientation); }
        public override int GetHashCode() => Intrinsics.CombineHashCodes(Position, Orientation);
        public override string ToString() => Intrinsics.MakeString(TypeName, FieldNames, FieldValues);
        public static implicit operator Dynamic(Pose3D self) => new Dynamic(self);
        public static implicit operator Pose3D(Dynamic value) => value.As<Pose3D>();
        public String TypeName => "Pose3D";
        public Array<String> FieldNames => Intrinsics.MakeArray<String>((String)"Position", (String)"Orientation");
        public Array<Dynamic> FieldValues => Intrinsics.MakeArray<Dynamic>(new Dynamic(Position), new Dynamic(Orientation));
        // Unimplemented concept functions
        public Boolean Equals(Pose3D b) => (Position.Equals(b.Position) & Orientation.Equals(b.Orientation));
        public static Boolean operator ==(Pose3D a, Pose3D b) => a.Equals(b);
        public Boolean NotEquals(Pose3D b) => (Position.NotEquals(b.Position) & Orientation.NotEquals(b.Orientation));
        public static Boolean operator !=(Pose3D a, Pose3D b) => a.NotEquals(b);
    }
    public readonly partial struct Frame3D
    {
        public readonly Vector3D Forward;
        public readonly Vector3D Up;
        public readonly Vector3D Right;
        public Frame3D WithForward(Vector3D forward) => (forward, Up, Right);
        public Frame3D WithUp(Vector3D up) => (Forward, up, Right);
        public Frame3D WithRight(Vector3D right) => (Forward, Up, right);
        public Frame3D(Vector3D forward, Vector3D up, Vector3D right) => (Forward, Up, Right) = (forward, up, right);
        public static Frame3D Default = new Frame3D();
        public static Frame3D New(Vector3D forward, Vector3D up, Vector3D right) => new Frame3D(forward, up, right);
        public Ara3D.DoublePrecision.Frame3D ChangePrecision() => (Forward.ChangePrecision(), Up.ChangePrecision(), Right.ChangePrecision());
        public static implicit operator Ara3D.DoublePrecision.Frame3D(Frame3D self) => self.ChangePrecision();
        public static implicit operator (Vector3D, Vector3D, Vector3D)(Frame3D self) => (self.Forward, self.Up, self.Right);
        public static implicit operator Frame3D((Vector3D, Vector3D, Vector3D) value) => new Frame3D(value.Item1, value.Item2, value.Item3);
        public void Deconstruct(out Vector3D forward, out Vector3D up, out Vector3D right) { forward = Forward; up = Up; right = Right; }
        public override bool Equals(object obj) { if (!(obj is Frame3D)) return false; var other = (Frame3D)obj; return Forward.Equals(other.Forward) && Up.Equals(other.Up) && Right.Equals(other.Right); }
        public override int GetHashCode() => Intrinsics.CombineHashCodes(Forward, Up, Right);
        public override string ToString() => Intrinsics.MakeString(TypeName, FieldNames, FieldValues);
        public static implicit operator Dynamic(Frame3D self) => new Dynamic(self);
        public static implicit operator Frame3D(Dynamic value) => value.As<Frame3D>();
        public String TypeName => "Frame3D";
        public Array<String> FieldNames => Intrinsics.MakeArray<String>((String)"Forward", (String)"Up", (String)"Right");
        public Array<Dynamic> FieldValues => Intrinsics.MakeArray<Dynamic>(new Dynamic(Forward), new Dynamic(Up), new Dynamic(Right));
        // Unimplemented concept functions
    }
    public readonly partial struct Bounds3D: Interval<Bounds3D, Point3D, Vector3D>
    {
        public readonly Point3D Min;
        public readonly Point3D Max;
        public Bounds3D WithMin(Point3D min) => (min, Max);
        public Bounds3D WithMax(Point3D max) => (Min, max);
        public Bounds3D(Point3D min, Point3D max) => (Min, Max) = (min, max);
        public static Bounds3D Default = new Bounds3D();
        public static Bounds3D New(Point3D min, Point3D max) => new Bounds3D(min, max);
        public Ara3D.DoublePrecision.Bounds3D ChangePrecision() => (Min.ChangePrecision(), Max.ChangePrecision());
        public static implicit operator Ara3D.DoublePrecision.Bounds3D(Bounds3D self) => self.ChangePrecision();
        public static implicit operator (Point3D, Point3D)(Bounds3D self) => (self.Min, self.Max);
        public static implicit operator Bounds3D((Point3D, Point3D) value) => new Bounds3D(value.Item1, value.Item2);
        public void Deconstruct(out Point3D min, out Point3D max) { min = Min; max = Max; }
        public override bool Equals(object obj) { if (!(obj is Bounds3D)) return false; var other = (Bounds3D)obj; return Min.Equals(other.Min) && Max.Equals(other.Max); }
        public override int GetHashCode() => Intrinsics.CombineHashCodes(Min, Max);
        public override string ToString() => Intrinsics.MakeString(TypeName, FieldNames, FieldValues);
        public static implicit operator Dynamic(Bounds3D self) => new Dynamic(self);
        public static implicit operator Bounds3D(Dynamic value) => value.As<Bounds3D>();
        public String TypeName => "Bounds3D";
        public Array<String> FieldNames => Intrinsics.MakeArray<String>((String)"Min", (String)"Max");
        public Array<Dynamic> FieldValues => Intrinsics.MakeArray<Dynamic>(new Dynamic(Min), new Dynamic(Max));
        Point3D Interval<Bounds3D, Point3D, Vector3D>.Min => Min;
        Point3D Interval<Bounds3D, Point3D, Vector3D>.Max => Max;
        // Unimplemented concept functions
        public Boolean Equals(Bounds3D b) => (Min.Equals(b.Min) & Max.Equals(b.Max));
        public static Boolean operator ==(Bounds3D a, Bounds3D b) => a.Equals(b);
        public Boolean NotEquals(Bounds3D b) => (Min.NotEquals(b.Min) & Max.NotEquals(b.Max));
        public static Boolean operator !=(Bounds3D a, Bounds3D b) => a.NotEquals(b);
    }
    public readonly partial struct Line3D: PolygonalChain3D, Array<Point3D>
    {
        public readonly Point3D A;
        public readonly Point3D B;
        public Line3D WithA(Point3D a) => (a, B);
        public Line3D WithB(Point3D b) => (A, b);
        public Line3D(Point3D a, Point3D b) => (A, B) = (a, b);
        public static Line3D Default = new Line3D();
        public static Line3D New(Point3D a, Point3D b) => new Line3D(a, b);
        public Ara3D.DoublePrecision.Line3D ChangePrecision() => (A.ChangePrecision(), B.ChangePrecision());
        public static implicit operator Ara3D.DoublePrecision.Line3D(Line3D self) => self.ChangePrecision();
        public static implicit operator (Point3D, Point3D)(Line3D self) => (self.A, self.B);
        public static implicit operator Line3D((Point3D, Point3D) value) => new Line3D(value.Item1, value.Item2);
        public void Deconstruct(out Point3D a, out Point3D b) { a = A; b = B; }
        public override bool Equals(object obj) { if (!(obj is Line3D)) return false; var other = (Line3D)obj; return A.Equals(other.A) && B.Equals(other.B); }
        public override int GetHashCode() => Intrinsics.CombineHashCodes(A, B);
        public override string ToString() => Intrinsics.MakeString(TypeName, FieldNames, FieldValues);
        public static implicit operator Dynamic(Line3D self) => new Dynamic(self);
        public static implicit operator Line3D(Dynamic value) => value.As<Line3D>();
        public String TypeName => "Line3D";
        public Array<String> FieldNames => Intrinsics.MakeArray<String>((String)"A", (String)"B");
        public Array<Dynamic> FieldValues => Intrinsics.MakeArray<Dynamic>(new Dynamic(A), new Dynamic(B));
        // Unimplemented concept functions
        public Integer Count => 2;
        public Point3D At(Integer n) => n == 0 ? A : n == 1 ? B : throw new System.IndexOutOfRangeException();
        public Point3D this[Integer n] => At(n);
    }
    public readonly partial struct Ray3D: Value<Ray3D>
    {
        public readonly Vector3D Direction;
        public readonly Point3D Position;
        public Ray3D WithDirection(Vector3D direction) => (direction, Position);
        public Ray3D WithPosition(Point3D position) => (Direction, position);
        public Ray3D(Vector3D direction, Point3D position) => (Direction, Position) = (direction, position);
        public static Ray3D Default = new Ray3D();
        public static Ray3D New(Vector3D direction, Point3D position) => new Ray3D(direction, position);
        public Ara3D.DoublePrecision.Ray3D ChangePrecision() => (Direction.ChangePrecision(), Position.ChangePrecision());
        public static implicit operator Ara3D.DoublePrecision.Ray3D(Ray3D self) => self.ChangePrecision();
        public static implicit operator (Vector3D, Point3D)(Ray3D self) => (self.Direction, self.Position);
        public static implicit operator Ray3D((Vector3D, Point3D) value) => new Ray3D(value.Item1, value.Item2);
        public void Deconstruct(out Vector3D direction, out Point3D position) { direction = Direction; position = Position; }
        public override bool Equals(object obj) { if (!(obj is Ray3D)) return false; var other = (Ray3D)obj; return Direction.Equals(other.Direction) && Position.Equals(other.Position); }
        public override int GetHashCode() => Intrinsics.CombineHashCodes(Direction, Position);
        public override string ToString() => Intrinsics.MakeString(TypeName, FieldNames, FieldValues);
        public static implicit operator Dynamic(Ray3D self) => new Dynamic(self);
        public static implicit operator Ray3D(Dynamic value) => value.As<Ray3D>();
        public String TypeName => "Ray3D";
        public Array<String> FieldNames => Intrinsics.MakeArray<String>((String)"Direction", (String)"Position");
        public Array<Dynamic> FieldValues => Intrinsics.MakeArray<Dynamic>(new Dynamic(Direction), new Dynamic(Position));
        // Unimplemented concept functions
        public Boolean Equals(Ray3D b) => (Direction.Equals(b.Direction) & Position.Equals(b.Position));
        public static Boolean operator ==(Ray3D a, Ray3D b) => a.Equals(b);
        public Boolean NotEquals(Ray3D b) => (Direction.NotEquals(b.Direction) & Position.NotEquals(b.Position));
        public static Boolean operator !=(Ray3D a, Ray3D b) => a.NotEquals(b);
    }
    public readonly partial struct Triangle3D: Value<Triangle3D>, Array<Point3D>
    {
        public readonly Point3D A;
        public readonly Point3D B;
        public readonly Point3D C;
        public Triangle3D WithA(Point3D a) => (a, B, C);
        public Triangle3D WithB(Point3D b) => (A, b, C);
        public Triangle3D WithC(Point3D c) => (A, B, c);
        public Triangle3D(Point3D a, Point3D b, Point3D c) => (A, B, C) = (a, b, c);
        public static Triangle3D Default = new Triangle3D();
        public static Triangle3D New(Point3D a, Point3D b, Point3D c) => new Triangle3D(a, b, c);
        public Ara3D.DoublePrecision.Triangle3D ChangePrecision() => (A.ChangePrecision(), B.ChangePrecision(), C.ChangePrecision());
        public static implicit operator Ara3D.DoublePrecision.Triangle3D(Triangle3D self) => self.ChangePrecision();
        public static implicit operator (Point3D, Point3D, Point3D)(Triangle3D self) => (self.A, self.B, self.C);
        public static implicit operator Triangle3D((Point3D, Point3D, Point3D) value) => new Triangle3D(value.Item1, value.Item2, value.Item3);
        public void Deconstruct(out Point3D a, out Point3D b, out Point3D c) { a = A; b = B; c = C; }
        public override bool Equals(object obj) { if (!(obj is Triangle3D)) return false; var other = (Triangle3D)obj; return A.Equals(other.A) && B.Equals(other.B) && C.Equals(other.C); }
        public override int GetHashCode() => Intrinsics.CombineHashCodes(A, B, C);
        public override string ToString() => Intrinsics.MakeString(TypeName, FieldNames, FieldValues);
        public static implicit operator Dynamic(Triangle3D self) => new Dynamic(self);
        public static implicit operator Triangle3D(Dynamic value) => value.As<Triangle3D>();
        public String TypeName => "Triangle3D";
        public Array<String> FieldNames => Intrinsics.MakeArray<String>((String)"A", (String)"B", (String)"C");
        public Array<Dynamic> FieldValues => Intrinsics.MakeArray<Dynamic>(new Dynamic(A), new Dynamic(B), new Dynamic(C));
        // Unimplemented concept functions
        public Boolean Equals(Triangle3D b) => (A.Equals(b.A) & B.Equals(b.B) & C.Equals(b.C));
        public static Boolean operator ==(Triangle3D a, Triangle3D b) => a.Equals(b);
        public Boolean NotEquals(Triangle3D b) => (A.NotEquals(b.A) & B.NotEquals(b.B) & C.NotEquals(b.C));
        public static Boolean operator !=(Triangle3D a, Triangle3D b) => a.NotEquals(b);
        public Integer Count => 3;
        public Point3D At(Integer n) => n == 0 ? A : n == 1 ? B : n == 2 ? C : throw new System.IndexOutOfRangeException();
        public Point3D this[Integer n] => At(n);
    }
    public readonly partial struct Quad3D: Value<Quad3D>, Array<Point3D>
    {
        public readonly Point3D A;
        public readonly Point3D B;
        public readonly Point3D C;
        public readonly Point3D D;
        public Quad3D WithA(Point3D a) => (a, B, C, D);
        public Quad3D WithB(Point3D b) => (A, b, C, D);
        public Quad3D WithC(Point3D c) => (A, B, c, D);
        public Quad3D WithD(Point3D d) => (A, B, C, d);
        public Quad3D(Point3D a, Point3D b, Point3D c, Point3D d) => (A, B, C, D) = (a, b, c, d);
        public static Quad3D Default = new Quad3D();
        public static Quad3D New(Point3D a, Point3D b, Point3D c, Point3D d) => new Quad3D(a, b, c, d);
        public Ara3D.DoublePrecision.Quad3D ChangePrecision() => (A.ChangePrecision(), B.ChangePrecision(), C.ChangePrecision(), D.ChangePrecision());
        public static implicit operator Ara3D.DoublePrecision.Quad3D(Quad3D self) => self.ChangePrecision();
        public static implicit operator (Point3D, Point3D, Point3D, Point3D)(Quad3D self) => (self.A, self.B, self.C, self.D);
        public static implicit operator Quad3D((Point3D, Point3D, Point3D, Point3D) value) => new Quad3D(value.Item1, value.Item2, value.Item3, value.Item4);
        public void Deconstruct(out Point3D a, out Point3D b, out Point3D c, out Point3D d) { a = A; b = B; c = C; d = D; }
        public override bool Equals(object obj) { if (!(obj is Quad3D)) return false; var other = (Quad3D)obj; return A.Equals(other.A) && B.Equals(other.B) && C.Equals(other.C) && D.Equals(other.D); }
        public override int GetHashCode() => Intrinsics.CombineHashCodes(A, B, C, D);
        public override string ToString() => Intrinsics.MakeString(TypeName, FieldNames, FieldValues);
        public static implicit operator Dynamic(Quad3D self) => new Dynamic(self);
        public static implicit operator Quad3D(Dynamic value) => value.As<Quad3D>();
        public String TypeName => "Quad3D";
        public Array<String> FieldNames => Intrinsics.MakeArray<String>((String)"A", (String)"B", (String)"C", (String)"D");
        public Array<Dynamic> FieldValues => Intrinsics.MakeArray<Dynamic>(new Dynamic(A), new Dynamic(B), new Dynamic(C), new Dynamic(D));
        // Unimplemented concept functions
        public Boolean Equals(Quad3D b) => (A.Equals(b.A) & B.Equals(b.B) & C.Equals(b.C) & D.Equals(b.D));
        public static Boolean operator ==(Quad3D a, Quad3D b) => a.Equals(b);
        public Boolean NotEquals(Quad3D b) => (A.NotEquals(b.A) & B.NotEquals(b.B) & C.NotEquals(b.C) & D.NotEquals(b.D));
        public static Boolean operator !=(Quad3D a, Quad3D b) => a.NotEquals(b);
        public Integer Count => 4;
        public Point3D At(Integer n) => n == 0 ? A : n == 1 ? B : n == 2 ? C : n == 3 ? D : throw new System.IndexOutOfRangeException();
        public Point3D this[Integer n] => At(n);
    }
    public readonly partial struct Capsule: Shape3D
    {
        public readonly Line3D Line;
        public readonly Number Radius;
        public Capsule WithLine(Line3D line) => (line, Radius);
        public Capsule WithRadius(Number radius) => (Line, radius);
        public Capsule(Line3D line, Number radius) => (Line, Radius) = (line, radius);
        public static Capsule Default = new Capsule();
        public static Capsule New(Line3D line, Number radius) => new Capsule(line, radius);
        public Ara3D.DoublePrecision.Capsule ChangePrecision() => (Line.ChangePrecision(), Radius.ChangePrecision());
        public static implicit operator Ara3D.DoublePrecision.Capsule(Capsule self) => self.ChangePrecision();
        public static implicit operator (Line3D, Number)(Capsule self) => (self.Line, self.Radius);
        public static implicit operator Capsule((Line3D, Number) value) => new Capsule(value.Item1, value.Item2);
        public void Deconstruct(out Line3D line, out Number radius) { line = Line; radius = Radius; }
        public override bool Equals(object obj) { if (!(obj is Capsule)) return false; var other = (Capsule)obj; return Line.Equals(other.Line) && Radius.Equals(other.Radius); }
        public override int GetHashCode() => Intrinsics.CombineHashCodes(Line, Radius);
        public override string ToString() => Intrinsics.MakeString(TypeName, FieldNames, FieldValues);
        public static implicit operator Dynamic(Capsule self) => new Dynamic(self);
        public static implicit operator Capsule(Dynamic value) => value.As<Capsule>();
        public String TypeName => "Capsule";
        public Array<String> FieldNames => Intrinsics.MakeArray<String>((String)"Line", (String)"Radius");
        public Array<Dynamic> FieldValues => Intrinsics.MakeArray<Dynamic>(new Dynamic(Line), new Dynamic(Radius));
        // Unimplemented concept functions
    }
    public readonly partial struct Cylinder: Shape3D
    {
        public readonly Line3D Line;
        public readonly Number Radius;
        public Cylinder WithLine(Line3D line) => (line, Radius);
        public Cylinder WithRadius(Number radius) => (Line, radius);
        public Cylinder(Line3D line, Number radius) => (Line, Radius) = (line, radius);
        public static Cylinder Default = new Cylinder();
        public static Cylinder New(Line3D line, Number radius) => new Cylinder(line, radius);
        public Ara3D.DoublePrecision.Cylinder ChangePrecision() => (Line.ChangePrecision(), Radius.ChangePrecision());
        public static implicit operator Ara3D.DoublePrecision.Cylinder(Cylinder self) => self.ChangePrecision();
        public static implicit operator (Line3D, Number)(Cylinder self) => (self.Line, self.Radius);
        public static implicit operator Cylinder((Line3D, Number) value) => new Cylinder(value.Item1, value.Item2);
        public void Deconstruct(out Line3D line, out Number radius) { line = Line; radius = Radius; }
        public override bool Equals(object obj) { if (!(obj is Cylinder)) return false; var other = (Cylinder)obj; return Line.Equals(other.Line) && Radius.Equals(other.Radius); }
        public override int GetHashCode() => Intrinsics.CombineHashCodes(Line, Radius);
        public override string ToString() => Intrinsics.MakeString(TypeName, FieldNames, FieldValues);
        public static implicit operator Dynamic(Cylinder self) => new Dynamic(self);
        public static implicit operator Cylinder(Dynamic value) => value.As<Cylinder>();
        public String TypeName => "Cylinder";
        public Array<String> FieldNames => Intrinsics.MakeArray<String>((String)"Line", (String)"Radius");
        public Array<Dynamic> FieldValues => Intrinsics.MakeArray<Dynamic>(new Dynamic(Line), new Dynamic(Radius));
        // Unimplemented concept functions
    }
    public readonly partial struct Cone: Shape3D
    {
        public readonly Line3D Line;
        public readonly Number Radius;
        public Cone WithLine(Line3D line) => (line, Radius);
        public Cone WithRadius(Number radius) => (Line, radius);
        public Cone(Line3D line, Number radius) => (Line, Radius) = (line, radius);
        public static Cone Default = new Cone();
        public static Cone New(Line3D line, Number radius) => new Cone(line, radius);
        public Ara3D.DoublePrecision.Cone ChangePrecision() => (Line.ChangePrecision(), Radius.ChangePrecision());
        public static implicit operator Ara3D.DoublePrecision.Cone(Cone self) => self.ChangePrecision();
        public static implicit operator (Line3D, Number)(Cone self) => (self.Line, self.Radius);
        public static implicit operator Cone((Line3D, Number) value) => new Cone(value.Item1, value.Item2);
        public void Deconstruct(out Line3D line, out Number radius) { line = Line; radius = Radius; }
        public override bool Equals(object obj) { if (!(obj is Cone)) return false; var other = (Cone)obj; return Line.Equals(other.Line) && Radius.Equals(other.Radius); }
        public override int GetHashCode() => Intrinsics.CombineHashCodes(Line, Radius);
        public override string ToString() => Intrinsics.MakeString(TypeName, FieldNames, FieldValues);
        public static implicit operator Dynamic(Cone self) => new Dynamic(self);
        public static implicit operator Cone(Dynamic value) => value.As<Cone>();
        public String TypeName => "Cone";
        public Array<String> FieldNames => Intrinsics.MakeArray<String>((String)"Line", (String)"Radius");
        public Array<Dynamic> FieldValues => Intrinsics.MakeArray<Dynamic>(new Dynamic(Line), new Dynamic(Radius));
        // Unimplemented concept functions
    }
    public readonly partial struct Tube: Shape3D
    {
        public readonly Line3D Line;
        public readonly Number InnerRadius;
        public readonly Number OuterRadius;
        public Tube WithLine(Line3D line) => (line, InnerRadius, OuterRadius);
        public Tube WithInnerRadius(Number innerRadius) => (Line, innerRadius, OuterRadius);
        public Tube WithOuterRadius(Number outerRadius) => (Line, InnerRadius, outerRadius);
        public Tube(Line3D line, Number innerRadius, Number outerRadius) => (Line, InnerRadius, OuterRadius) = (line, innerRadius, outerRadius);
        public static Tube Default = new Tube();
        public static Tube New(Line3D line, Number innerRadius, Number outerRadius) => new Tube(line, innerRadius, outerRadius);
        public Ara3D.DoublePrecision.Tube ChangePrecision() => (Line.ChangePrecision(), InnerRadius.ChangePrecision(), OuterRadius.ChangePrecision());
        public static implicit operator Ara3D.DoublePrecision.Tube(Tube self) => self.ChangePrecision();
        public static implicit operator (Line3D, Number, Number)(Tube self) => (self.Line, self.InnerRadius, self.OuterRadius);
        public static implicit operator Tube((Line3D, Number, Number) value) => new Tube(value.Item1, value.Item2, value.Item3);
        public void Deconstruct(out Line3D line, out Number innerRadius, out Number outerRadius) { line = Line; innerRadius = InnerRadius; outerRadius = OuterRadius; }
        public override bool Equals(object obj) { if (!(obj is Tube)) return false; var other = (Tube)obj; return Line.Equals(other.Line) && InnerRadius.Equals(other.InnerRadius) && OuterRadius.Equals(other.OuterRadius); }
        public override int GetHashCode() => Intrinsics.CombineHashCodes(Line, InnerRadius, OuterRadius);
        public override string ToString() => Intrinsics.MakeString(TypeName, FieldNames, FieldValues);
        public static implicit operator Dynamic(Tube self) => new Dynamic(self);
        public static implicit operator Tube(Dynamic value) => value.As<Tube>();
        public String TypeName => "Tube";
        public Array<String> FieldNames => Intrinsics.MakeArray<String>((String)"Line", (String)"InnerRadius", (String)"OuterRadius");
        public Array<Dynamic> FieldValues => Intrinsics.MakeArray<Dynamic>(new Dynamic(Line), new Dynamic(InnerRadius), new Dynamic(OuterRadius));
        // Unimplemented concept functions
    }
    public readonly partial struct ConeSegment: Shape3D
    {
        public readonly Line3D Line;
        public readonly Number Radius1;
        public readonly Number Radius2;
        public ConeSegment WithLine(Line3D line) => (line, Radius1, Radius2);
        public ConeSegment WithRadius1(Number radius1) => (Line, radius1, Radius2);
        public ConeSegment WithRadius2(Number radius2) => (Line, Radius1, radius2);
        public ConeSegment(Line3D line, Number radius1, Number radius2) => (Line, Radius1, Radius2) = (line, radius1, radius2);
        public static ConeSegment Default = new ConeSegment();
        public static ConeSegment New(Line3D line, Number radius1, Number radius2) => new ConeSegment(line, radius1, radius2);
        public Ara3D.DoublePrecision.ConeSegment ChangePrecision() => (Line.ChangePrecision(), Radius1.ChangePrecision(), Radius2.ChangePrecision());
        public static implicit operator Ara3D.DoublePrecision.ConeSegment(ConeSegment self) => self.ChangePrecision();
        public static implicit operator (Line3D, Number, Number)(ConeSegment self) => (self.Line, self.Radius1, self.Radius2);
        public static implicit operator ConeSegment((Line3D, Number, Number) value) => new ConeSegment(value.Item1, value.Item2, value.Item3);
        public void Deconstruct(out Line3D line, out Number radius1, out Number radius2) { line = Line; radius1 = Radius1; radius2 = Radius2; }
        public override bool Equals(object obj) { if (!(obj is ConeSegment)) return false; var other = (ConeSegment)obj; return Line.Equals(other.Line) && Radius1.Equals(other.Radius1) && Radius2.Equals(other.Radius2); }
        public override int GetHashCode() => Intrinsics.CombineHashCodes(Line, Radius1, Radius2);
        public override string ToString() => Intrinsics.MakeString(TypeName, FieldNames, FieldValues);
        public static implicit operator Dynamic(ConeSegment self) => new Dynamic(self);
        public static implicit operator ConeSegment(Dynamic value) => value.As<ConeSegment>();
        public String TypeName => "ConeSegment";
        public Array<String> FieldNames => Intrinsics.MakeArray<String>((String)"Line", (String)"Radius1", (String)"Radius2");
        public Array<Dynamic> FieldValues => Intrinsics.MakeArray<Dynamic>(new Dynamic(Line), new Dynamic(Radius1), new Dynamic(Radius2));
        // Unimplemented concept functions
    }
    public readonly partial struct Box3D: Shape3D
    {
        public readonly Point3D Center;
        public readonly Rotation3D Rotation;
        public readonly Size3D Extent;
        public Box3D WithCenter(Point3D center) => (center, Rotation, Extent);
        public Box3D WithRotation(Rotation3D rotation) => (Center, rotation, Extent);
        public Box3D WithExtent(Size3D extent) => (Center, Rotation, extent);
        public Box3D(Point3D center, Rotation3D rotation, Size3D extent) => (Center, Rotation, Extent) = (center, rotation, extent);
        public static Box3D Default = new Box3D();
        public static Box3D New(Point3D center, Rotation3D rotation, Size3D extent) => new Box3D(center, rotation, extent);
        public Ara3D.DoublePrecision.Box3D ChangePrecision() => (Center.ChangePrecision(), Rotation.ChangePrecision(), Extent.ChangePrecision());
        public static implicit operator Ara3D.DoublePrecision.Box3D(Box3D self) => self.ChangePrecision();
        public static implicit operator (Point3D, Rotation3D, Size3D)(Box3D self) => (self.Center, self.Rotation, self.Extent);
        public static implicit operator Box3D((Point3D, Rotation3D, Size3D) value) => new Box3D(value.Item1, value.Item2, value.Item3);
        public void Deconstruct(out Point3D center, out Rotation3D rotation, out Size3D extent) { center = Center; rotation = Rotation; extent = Extent; }
        public override bool Equals(object obj) { if (!(obj is Box3D)) return false; var other = (Box3D)obj; return Center.Equals(other.Center) && Rotation.Equals(other.Rotation) && Extent.Equals(other.Extent); }
        public override int GetHashCode() => Intrinsics.CombineHashCodes(Center, Rotation, Extent);
        public override string ToString() => Intrinsics.MakeString(TypeName, FieldNames, FieldValues);
        public static implicit operator Dynamic(Box3D self) => new Dynamic(self);
        public static implicit operator Box3D(Dynamic value) => value.As<Box3D>();
        public String TypeName => "Box3D";
        public Array<String> FieldNames => Intrinsics.MakeArray<String>((String)"Center", (String)"Rotation", (String)"Extent");
        public Array<Dynamic> FieldValues => Intrinsics.MakeArray<Dynamic>(new Dynamic(Center), new Dynamic(Rotation), new Dynamic(Extent));
        // Unimplemented concept functions
    }
    public readonly partial struct CubicBezier2D: Array<Point2D>
    {
        public readonly Point2D A;
        public readonly Point2D B;
        public readonly Point2D C;
        public readonly Point2D D;
        public CubicBezier2D WithA(Point2D a) => (a, B, C, D);
        public CubicBezier2D WithB(Point2D b) => (A, b, C, D);
        public CubicBezier2D WithC(Point2D c) => (A, B, c, D);
        public CubicBezier2D WithD(Point2D d) => (A, B, C, d);
        public CubicBezier2D(Point2D a, Point2D b, Point2D c, Point2D d) => (A, B, C, D) = (a, b, c, d);
        public static CubicBezier2D Default = new CubicBezier2D();
        public static CubicBezier2D New(Point2D a, Point2D b, Point2D c, Point2D d) => new CubicBezier2D(a, b, c, d);
        public Ara3D.DoublePrecision.CubicBezier2D ChangePrecision() => (A.ChangePrecision(), B.ChangePrecision(), C.ChangePrecision(), D.ChangePrecision());
        public static implicit operator Ara3D.DoublePrecision.CubicBezier2D(CubicBezier2D self) => self.ChangePrecision();
        public static implicit operator (Point2D, Point2D, Point2D, Point2D)(CubicBezier2D self) => (self.A, self.B, self.C, self.D);
        public static implicit operator CubicBezier2D((Point2D, Point2D, Point2D, Point2D) value) => new CubicBezier2D(value.Item1, value.Item2, value.Item3, value.Item4);
        public void Deconstruct(out Point2D a, out Point2D b, out Point2D c, out Point2D d) { a = A; b = B; c = C; d = D; }
        public override bool Equals(object obj) { if (!(obj is CubicBezier2D)) return false; var other = (CubicBezier2D)obj; return A.Equals(other.A) && B.Equals(other.B) && C.Equals(other.C) && D.Equals(other.D); }
        public override int GetHashCode() => Intrinsics.CombineHashCodes(A, B, C, D);
        public override string ToString() => Intrinsics.MakeString(TypeName, FieldNames, FieldValues);
        public static implicit operator Dynamic(CubicBezier2D self) => new Dynamic(self);
        public static implicit operator CubicBezier2D(Dynamic value) => value.As<CubicBezier2D>();
        public String TypeName => "CubicBezier2D";
        public Array<String> FieldNames => Intrinsics.MakeArray<String>((String)"A", (String)"B", (String)"C", (String)"D");
        public Array<Dynamic> FieldValues => Intrinsics.MakeArray<Dynamic>(new Dynamic(A), new Dynamic(B), new Dynamic(C), new Dynamic(D));
        // Unimplemented concept functions
        public Integer Count => 4;
        public Point2D At(Integer n) => n == 0 ? A : n == 1 ? B : n == 2 ? C : n == 3 ? D : throw new System.IndexOutOfRangeException();
        public Point2D this[Integer n] => At(n);
    }
    public readonly partial struct CubicBezier3D: Array<Point3D>
    {
        public readonly Point3D A;
        public readonly Point3D B;
        public readonly Point3D C;
        public readonly Point3D D;
        public CubicBezier3D WithA(Point3D a) => (a, B, C, D);
        public CubicBezier3D WithB(Point3D b) => (A, b, C, D);
        public CubicBezier3D WithC(Point3D c) => (A, B, c, D);
        public CubicBezier3D WithD(Point3D d) => (A, B, C, d);
        public CubicBezier3D(Point3D a, Point3D b, Point3D c, Point3D d) => (A, B, C, D) = (a, b, c, d);
        public static CubicBezier3D Default = new CubicBezier3D();
        public static CubicBezier3D New(Point3D a, Point3D b, Point3D c, Point3D d) => new CubicBezier3D(a, b, c, d);
        public Ara3D.DoublePrecision.CubicBezier3D ChangePrecision() => (A.ChangePrecision(), B.ChangePrecision(), C.ChangePrecision(), D.ChangePrecision());
        public static implicit operator Ara3D.DoublePrecision.CubicBezier3D(CubicBezier3D self) => self.ChangePrecision();
        public static implicit operator (Point3D, Point3D, Point3D, Point3D)(CubicBezier3D self) => (self.A, self.B, self.C, self.D);
        public static implicit operator CubicBezier3D((Point3D, Point3D, Point3D, Point3D) value) => new CubicBezier3D(value.Item1, value.Item2, value.Item3, value.Item4);
        public void Deconstruct(out Point3D a, out Point3D b, out Point3D c, out Point3D d) { a = A; b = B; c = C; d = D; }
        public override bool Equals(object obj) { if (!(obj is CubicBezier3D)) return false; var other = (CubicBezier3D)obj; return A.Equals(other.A) && B.Equals(other.B) && C.Equals(other.C) && D.Equals(other.D); }
        public override int GetHashCode() => Intrinsics.CombineHashCodes(A, B, C, D);
        public override string ToString() => Intrinsics.MakeString(TypeName, FieldNames, FieldValues);
        public static implicit operator Dynamic(CubicBezier3D self) => new Dynamic(self);
        public static implicit operator CubicBezier3D(Dynamic value) => value.As<CubicBezier3D>();
        public String TypeName => "CubicBezier3D";
        public Array<String> FieldNames => Intrinsics.MakeArray<String>((String)"A", (String)"B", (String)"C", (String)"D");
        public Array<Dynamic> FieldValues => Intrinsics.MakeArray<Dynamic>(new Dynamic(A), new Dynamic(B), new Dynamic(C), new Dynamic(D));
        // Unimplemented concept functions
        public Integer Count => 4;
        public Point3D At(Integer n) => n == 0 ? A : n == 1 ? B : n == 2 ? C : n == 3 ? D : throw new System.IndexOutOfRangeException();
        public Point3D this[Integer n] => At(n);
    }
    public readonly partial struct QuadraticBezier2D: Array<Point2D>
    {
        public readonly Point2D A;
        public readonly Point2D B;
        public readonly Point2D C;
        public QuadraticBezier2D WithA(Point2D a) => (a, B, C);
        public QuadraticBezier2D WithB(Point2D b) => (A, b, C);
        public QuadraticBezier2D WithC(Point2D c) => (A, B, c);
        public QuadraticBezier2D(Point2D a, Point2D b, Point2D c) => (A, B, C) = (a, b, c);
        public static QuadraticBezier2D Default = new QuadraticBezier2D();
        public static QuadraticBezier2D New(Point2D a, Point2D b, Point2D c) => new QuadraticBezier2D(a, b, c);
        public Ara3D.DoublePrecision.QuadraticBezier2D ChangePrecision() => (A.ChangePrecision(), B.ChangePrecision(), C.ChangePrecision());
        public static implicit operator Ara3D.DoublePrecision.QuadraticBezier2D(QuadraticBezier2D self) => self.ChangePrecision();
        public static implicit operator (Point2D, Point2D, Point2D)(QuadraticBezier2D self) => (self.A, self.B, self.C);
        public static implicit operator QuadraticBezier2D((Point2D, Point2D, Point2D) value) => new QuadraticBezier2D(value.Item1, value.Item2, value.Item3);
        public void Deconstruct(out Point2D a, out Point2D b, out Point2D c) { a = A; b = B; c = C; }
        public override bool Equals(object obj) { if (!(obj is QuadraticBezier2D)) return false; var other = (QuadraticBezier2D)obj; return A.Equals(other.A) && B.Equals(other.B) && C.Equals(other.C); }
        public override int GetHashCode() => Intrinsics.CombineHashCodes(A, B, C);
        public override string ToString() => Intrinsics.MakeString(TypeName, FieldNames, FieldValues);
        public static implicit operator Dynamic(QuadraticBezier2D self) => new Dynamic(self);
        public static implicit operator QuadraticBezier2D(Dynamic value) => value.As<QuadraticBezier2D>();
        public String TypeName => "QuadraticBezier2D";
        public Array<String> FieldNames => Intrinsics.MakeArray<String>((String)"A", (String)"B", (String)"C");
        public Array<Dynamic> FieldValues => Intrinsics.MakeArray<Dynamic>(new Dynamic(A), new Dynamic(B), new Dynamic(C));
        // Unimplemented concept functions
        public Integer Count => 3;
        public Point2D At(Integer n) => n == 0 ? A : n == 1 ? B : n == 2 ? C : throw new System.IndexOutOfRangeException();
        public Point2D this[Integer n] => At(n);
    }
    public readonly partial struct QuadraticBezier3D: Array<Point3D>
    {
        public readonly Point3D A;
        public readonly Point3D B;
        public readonly Point3D C;
        public QuadraticBezier3D WithA(Point3D a) => (a, B, C);
        public QuadraticBezier3D WithB(Point3D b) => (A, b, C);
        public QuadraticBezier3D WithC(Point3D c) => (A, B, c);
        public QuadraticBezier3D(Point3D a, Point3D b, Point3D c) => (A, B, C) = (a, b, c);
        public static QuadraticBezier3D Default = new QuadraticBezier3D();
        public static QuadraticBezier3D New(Point3D a, Point3D b, Point3D c) => new QuadraticBezier3D(a, b, c);
        public Ara3D.DoublePrecision.QuadraticBezier3D ChangePrecision() => (A.ChangePrecision(), B.ChangePrecision(), C.ChangePrecision());
        public static implicit operator Ara3D.DoublePrecision.QuadraticBezier3D(QuadraticBezier3D self) => self.ChangePrecision();
        public static implicit operator (Point3D, Point3D, Point3D)(QuadraticBezier3D self) => (self.A, self.B, self.C);
        public static implicit operator QuadraticBezier3D((Point3D, Point3D, Point3D) value) => new QuadraticBezier3D(value.Item1, value.Item2, value.Item3);
        public void Deconstruct(out Point3D a, out Point3D b, out Point3D c) { a = A; b = B; c = C; }
        public override bool Equals(object obj) { if (!(obj is QuadraticBezier3D)) return false; var other = (QuadraticBezier3D)obj; return A.Equals(other.A) && B.Equals(other.B) && C.Equals(other.C); }
        public override int GetHashCode() => Intrinsics.CombineHashCodes(A, B, C);
        public override string ToString() => Intrinsics.MakeString(TypeName, FieldNames, FieldValues);
        public static implicit operator Dynamic(QuadraticBezier3D self) => new Dynamic(self);
        public static implicit operator QuadraticBezier3D(Dynamic value) => value.As<QuadraticBezier3D>();
        public String TypeName => "QuadraticBezier3D";
        public Array<String> FieldNames => Intrinsics.MakeArray<String>((String)"A", (String)"B", (String)"C");
        public Array<Dynamic> FieldValues => Intrinsics.MakeArray<Dynamic>(new Dynamic(A), new Dynamic(B), new Dynamic(C));
        // Unimplemented concept functions
        public Integer Count => 3;
        public Point3D At(Integer n) => n == 0 ? A : n == 1 ? B : n == 2 ? C : throw new System.IndexOutOfRangeException();
        public Point3D this[Integer n] => At(n);
    }
    public readonly partial struct Quaternion: Vector<Quaternion>
    {
        public readonly Number X;
        public readonly Number Y;
        public readonly Number Z;
        public readonly Number W;
        public Quaternion WithX(Number x) => (x, Y, Z, W);
        public Quaternion WithY(Number y) => (X, y, Z, W);
        public Quaternion WithZ(Number z) => (X, Y, z, W);
        public Quaternion WithW(Number w) => (X, Y, Z, w);
        public Quaternion(Number x, Number y, Number z, Number w) => (X, Y, Z, W) = (x, y, z, w);
        public static Quaternion Default = new Quaternion();
        public static Quaternion New(Number x, Number y, Number z, Number w) => new Quaternion(x, y, z, w);
        public Ara3D.DoublePrecision.Quaternion ChangePrecision() => (X.ChangePrecision(), Y.ChangePrecision(), Z.ChangePrecision(), W.ChangePrecision());
        public static implicit operator Ara3D.DoublePrecision.Quaternion(Quaternion self) => self.ChangePrecision();
        public static implicit operator (Number, Number, Number, Number)(Quaternion self) => (self.X, self.Y, self.Z, self.W);
        public static implicit operator Quaternion((Number, Number, Number, Number) value) => new Quaternion(value.Item1, value.Item2, value.Item3, value.Item4);
        public void Deconstruct(out Number x, out Number y, out Number z, out Number w) { x = X; y = Y; z = Z; w = W; }
        public override bool Equals(object obj) { if (!(obj is Quaternion)) return false; var other = (Quaternion)obj; return X.Equals(other.X) && Y.Equals(other.Y) && Z.Equals(other.Z) && W.Equals(other.W); }
        public override int GetHashCode() => Intrinsics.CombineHashCodes(X, Y, Z, W);
        public override string ToString() => Intrinsics.MakeString(TypeName, FieldNames, FieldValues);
        public static implicit operator Dynamic(Quaternion self) => new Dynamic(self);
        public static implicit operator Quaternion(Dynamic value) => value.As<Quaternion>();
        public String TypeName => "Quaternion";
        public Array<String> FieldNames => Intrinsics.MakeArray<String>((String)"X", (String)"Y", (String)"Z", (String)"W");
        public Array<Dynamic> FieldValues => Intrinsics.MakeArray<Dynamic>(new Dynamic(X), new Dynamic(Y), new Dynamic(Z), new Dynamic(W));
        // Unimplemented concept functions
        public Quaternion Add(Number other) => (X.Add(other), Y.Add(other), Z.Add(other), W.Add(other));
        public static Quaternion operator +(Quaternion self, Number other) => self.Add(other);
        public Quaternion Subtract(Number other) => (X.Subtract(other), Y.Subtract(other), Z.Subtract(other), W.Subtract(other));
        public static Quaternion operator -(Quaternion self, Number other) => self.Subtract(other);
        public Quaternion Multiply(Number other) => (X.Multiply(other), Y.Multiply(other), Z.Multiply(other), W.Multiply(other));
        public static Quaternion operator *(Quaternion self, Number other) => self.Multiply(other);
        public Quaternion Divide(Number other) => (X.Divide(other), Y.Divide(other), Z.Divide(other), W.Divide(other));
        public static Quaternion operator /(Quaternion self, Number other) => self.Divide(other);
        public Quaternion Modulo(Number other) => (X.Modulo(other), Y.Modulo(other), Z.Modulo(other), W.Modulo(other));
        public static Quaternion operator %(Quaternion self, Number other) => self.Modulo(other);
        public Quaternion Reciprocal => (X.Reciprocal, Y.Reciprocal, Z.Reciprocal, W.Reciprocal);
        public Quaternion Negative => (X.Negative, Y.Negative, Z.Negative, W.Negative);
        public static Quaternion operator -(Quaternion self) => self.Negative;
        public Quaternion Multiply(Quaternion other) => (X.Multiply(other.X), Y.Multiply(other.Y), Z.Multiply(other.Z), W.Multiply(other.W));
        public static Quaternion operator *(Quaternion self, Quaternion other) => self.Multiply(other);
        public Quaternion Divide(Quaternion other) => (X.Divide(other.X), Y.Divide(other.Y), Z.Divide(other.Z), W.Divide(other.W));
        public static Quaternion operator /(Quaternion self, Quaternion other) => self.Divide(other);
        public Quaternion Modulo(Quaternion other) => (X.Modulo(other.X), Y.Modulo(other.Y), Z.Modulo(other.Z), W.Modulo(other.W));
        public static Quaternion operator %(Quaternion self, Quaternion other) => self.Modulo(other);
        public Quaternion Add(Quaternion other) => (X.Add(other.X), Y.Add(other.Y), Z.Add(other.Z), W.Add(other.W));
        public static Quaternion operator +(Quaternion self, Quaternion other) => self.Add(other);
        public Quaternion Subtract(Quaternion other) => (X.Subtract(other.X), Y.Subtract(other.Y), Z.Subtract(other.Z), W.Subtract(other.W));
        public static Quaternion operator -(Quaternion self, Quaternion other) => self.Subtract(other);
        public Quaternion Zero => (X.Zero, Y.Zero, Z.Zero, W.Zero);
        public Quaternion One => (X.One, Y.One, Z.One, W.One);
        public Quaternion MinValue => (X.MinValue, Y.MinValue, Z.MinValue, W.MinValue);
        public Quaternion MaxValue => (X.MaxValue, Y.MaxValue, Z.MaxValue, W.MaxValue);
        public Boolean Equals(Quaternion b) => (X.Equals(b.X) & Y.Equals(b.Y) & Z.Equals(b.Z) & W.Equals(b.W));
        public static Boolean operator ==(Quaternion a, Quaternion b) => a.Equals(b);
        public Boolean NotEquals(Quaternion b) => (X.NotEquals(b.X) & Y.NotEquals(b.Y) & Z.NotEquals(b.Z) & W.NotEquals(b.W));
        public static Boolean operator !=(Quaternion a, Quaternion b) => a.NotEquals(b);
        public Boolean Between(Quaternion a, Quaternion b) => (X.Between(a.X, b.X) & Y.Between(a.Y, b.Y) & Z.Between(a.Z, b.Z) & W.Between(a.W, b.W));
        public Quaternion Clamp(Quaternion a, Quaternion b) => (X.Clamp(a.X, b.X), Y.Clamp(a.Y, b.Y), Z.Clamp(a.Z, b.Z), W.Clamp(a.W, b.W));
        public Integer Count => 4;
        public Number At(Integer n) => n == 0 ? X : n == 1 ? Y : n == 2 ? Z : n == 3 ? W : throw new System.IndexOutOfRangeException();
        public Number this[Integer n] => At(n);
    }
    public readonly partial struct AxisAngle: Value<AxisAngle>
    {
        public readonly Vector3D Axis;
        public readonly Angle Angle;
        public AxisAngle WithAxis(Vector3D axis) => (axis, Angle);
        public AxisAngle WithAngle(Angle angle) => (Axis, angle);
        public AxisAngle(Vector3D axis, Angle angle) => (Axis, Angle) = (axis, angle);
        public static AxisAngle Default = new AxisAngle();
        public static AxisAngle New(Vector3D axis, Angle angle) => new AxisAngle(axis, angle);
        public Ara3D.DoublePrecision.AxisAngle ChangePrecision() => (Axis.ChangePrecision(), Angle.ChangePrecision());
        public static implicit operator Ara3D.DoublePrecision.AxisAngle(AxisAngle self) => self.ChangePrecision();
        public static implicit operator (Vector3D, Angle)(AxisAngle self) => (self.Axis, self.Angle);
        public static implicit operator AxisAngle((Vector3D, Angle) value) => new AxisAngle(value.Item1, value.Item2);
        public void Deconstruct(out Vector3D axis, out Angle angle) { axis = Axis; angle = Angle; }
        public override bool Equals(object obj) { if (!(obj is AxisAngle)) return false; var other = (AxisAngle)obj; return Axis.Equals(other.Axis) && Angle.Equals(other.Angle); }
        public override int GetHashCode() => Intrinsics.CombineHashCodes(Axis, Angle);
        public override string ToString() => Intrinsics.MakeString(TypeName, FieldNames, FieldValues);
        public static implicit operator Dynamic(AxisAngle self) => new Dynamic(self);
        public static implicit operator AxisAngle(Dynamic value) => value.As<AxisAngle>();
        public String TypeName => "AxisAngle";
        public Array<String> FieldNames => Intrinsics.MakeArray<String>((String)"Axis", (String)"Angle");
        public Array<Dynamic> FieldValues => Intrinsics.MakeArray<Dynamic>(new Dynamic(Axis), new Dynamic(Angle));
        // Unimplemented concept functions
        public Boolean Equals(AxisAngle b) => (Axis.Equals(b.Axis) & Angle.Equals(b.Angle));
        public static Boolean operator ==(AxisAngle a, AxisAngle b) => a.Equals(b);
        public Boolean NotEquals(AxisAngle b) => (Axis.NotEquals(b.Axis) & Angle.NotEquals(b.Angle));
        public static Boolean operator !=(AxisAngle a, AxisAngle b) => a.NotEquals(b);
    }
    public readonly partial struct EulerAngles: Value<EulerAngles>
    {
        public readonly Angle Yaw;
        public readonly Angle Pitch;
        public readonly Angle Roll;
        public EulerAngles WithYaw(Angle yaw) => (yaw, Pitch, Roll);
        public EulerAngles WithPitch(Angle pitch) => (Yaw, pitch, Roll);
        public EulerAngles WithRoll(Angle roll) => (Yaw, Pitch, roll);
        public EulerAngles(Angle yaw, Angle pitch, Angle roll) => (Yaw, Pitch, Roll) = (yaw, pitch, roll);
        public static EulerAngles Default = new EulerAngles();
        public static EulerAngles New(Angle yaw, Angle pitch, Angle roll) => new EulerAngles(yaw, pitch, roll);
        public Ara3D.DoublePrecision.EulerAngles ChangePrecision() => (Yaw.ChangePrecision(), Pitch.ChangePrecision(), Roll.ChangePrecision());
        public static implicit operator Ara3D.DoublePrecision.EulerAngles(EulerAngles self) => self.ChangePrecision();
        public static implicit operator (Angle, Angle, Angle)(EulerAngles self) => (self.Yaw, self.Pitch, self.Roll);
        public static implicit operator EulerAngles((Angle, Angle, Angle) value) => new EulerAngles(value.Item1, value.Item2, value.Item3);
        public void Deconstruct(out Angle yaw, out Angle pitch, out Angle roll) { yaw = Yaw; pitch = Pitch; roll = Roll; }
        public override bool Equals(object obj) { if (!(obj is EulerAngles)) return false; var other = (EulerAngles)obj; return Yaw.Equals(other.Yaw) && Pitch.Equals(other.Pitch) && Roll.Equals(other.Roll); }
        public override int GetHashCode() => Intrinsics.CombineHashCodes(Yaw, Pitch, Roll);
        public override string ToString() => Intrinsics.MakeString(TypeName, FieldNames, FieldValues);
        public static implicit operator Dynamic(EulerAngles self) => new Dynamic(self);
        public static implicit operator EulerAngles(Dynamic value) => value.As<EulerAngles>();
        public String TypeName => "EulerAngles";
        public Array<String> FieldNames => Intrinsics.MakeArray<String>((String)"Yaw", (String)"Pitch", (String)"Roll");
        public Array<Dynamic> FieldValues => Intrinsics.MakeArray<Dynamic>(new Dynamic(Yaw), new Dynamic(Pitch), new Dynamic(Roll));
        // Unimplemented concept functions
        public Boolean Equals(EulerAngles b) => (Yaw.Equals(b.Yaw) & Pitch.Equals(b.Pitch) & Roll.Equals(b.Roll));
        public static Boolean operator ==(EulerAngles a, EulerAngles b) => a.Equals(b);
        public Boolean NotEquals(EulerAngles b) => (Yaw.NotEquals(b.Yaw) & Pitch.NotEquals(b.Pitch) & Roll.NotEquals(b.Roll));
        public static Boolean operator !=(EulerAngles a, EulerAngles b) => a.NotEquals(b);
    }
    public readonly partial struct Rotation3D: Value<Rotation3D>
    {
        public readonly Quaternion Quaternion;
        public Rotation3D WithQuaternion(Quaternion quaternion) => (quaternion);
        public Rotation3D(Quaternion quaternion) => (Quaternion) = (quaternion);
        public static Rotation3D Default = new Rotation3D();
        public static Rotation3D New(Quaternion quaternion) => new Rotation3D(quaternion);
        public Ara3D.DoublePrecision.Rotation3D ChangePrecision() => (Quaternion.ChangePrecision());
        public static implicit operator Ara3D.DoublePrecision.Rotation3D(Rotation3D self) => self.ChangePrecision();
        public static implicit operator Quaternion(Rotation3D self) => self.Quaternion;
        public static implicit operator Rotation3D(Quaternion value) => new Rotation3D(value);
        public override bool Equals(object obj) { if (!(obj is Rotation3D)) return false; var other = (Rotation3D)obj; return Quaternion.Equals(other.Quaternion); }
        public override int GetHashCode() => Intrinsics.CombineHashCodes(Quaternion);
        public override string ToString() => Intrinsics.MakeString(TypeName, FieldNames, FieldValues);
        public static implicit operator Dynamic(Rotation3D self) => new Dynamic(self);
        public static implicit operator Rotation3D(Dynamic value) => value.As<Rotation3D>();
        public String TypeName => "Rotation3D";
        public Array<String> FieldNames => Intrinsics.MakeArray<String>((String)"Quaternion");
        public Array<Dynamic> FieldValues => Intrinsics.MakeArray<Dynamic>(new Dynamic(Quaternion));
        // Unimplemented concept functions
        public Boolean Equals(Rotation3D b) => (Quaternion.Equals(b.Quaternion));
        public static Boolean operator ==(Rotation3D a, Rotation3D b) => a.Equals(b);
        public Boolean NotEquals(Rotation3D b) => (Quaternion.NotEquals(b.Quaternion));
        public static Boolean operator !=(Rotation3D a, Rotation3D b) => a.NotEquals(b);
    }
    public readonly partial struct Orientation3D: Value<Orientation3D>
    {
        public readonly Rotation3D Value;
        public Orientation3D WithValue(Rotation3D value) => (value);
        public Orientation3D(Rotation3D value) => (Value) = (value);
        public static Orientation3D Default = new Orientation3D();
        public static Orientation3D New(Rotation3D value) => new Orientation3D(value);
        public Ara3D.DoublePrecision.Orientation3D ChangePrecision() => (Value.ChangePrecision());
        public static implicit operator Ara3D.DoublePrecision.Orientation3D(Orientation3D self) => self.ChangePrecision();
        public static implicit operator Rotation3D(Orientation3D self) => self.Value;
        public static implicit operator Orientation3D(Rotation3D value) => new Orientation3D(value);
        public override bool Equals(object obj) { if (!(obj is Orientation3D)) return false; var other = (Orientation3D)obj; return Value.Equals(other.Value); }
        public override int GetHashCode() => Intrinsics.CombineHashCodes(Value);
        public override string ToString() => Intrinsics.MakeString(TypeName, FieldNames, FieldValues);
        public static implicit operator Dynamic(Orientation3D self) => new Dynamic(self);
        public static implicit operator Orientation3D(Dynamic value) => value.As<Orientation3D>();
        public String TypeName => "Orientation3D";
        public Array<String> FieldNames => Intrinsics.MakeArray<String>((String)"Value");
        public Array<Dynamic> FieldValues => Intrinsics.MakeArray<Dynamic>(new Dynamic(Value));
        // Unimplemented concept functions
        public Boolean Equals(Orientation3D b) => (Value.Equals(b.Value));
        public static Boolean operator ==(Orientation3D a, Orientation3D b) => a.Equals(b);
        public Boolean NotEquals(Orientation3D b) => (Value.NotEquals(b.Value));
        public static Boolean operator !=(Orientation3D a, Orientation3D b) => a.NotEquals(b);
    }
    public readonly partial struct Point4D: Coordinate<Point4D>, Difference<Point4D, Vector4D>, Array<Number>
    {
        public readonly Number X;
        public readonly Number Y;
        public readonly Number Z;
        public readonly Number W;
        public Point4D WithX(Number x) => (x, Y, Z, W);
        public Point4D WithY(Number y) => (X, y, Z, W);
        public Point4D WithZ(Number z) => (X, Y, z, W);
        public Point4D WithW(Number w) => (X, Y, Z, w);
        public Point4D(Number x, Number y, Number z, Number w) => (X, Y, Z, W) = (x, y, z, w);
        public static Point4D Default = new Point4D();
        public static Point4D New(Number x, Number y, Number z, Number w) => new Point4D(x, y, z, w);
        public Ara3D.DoublePrecision.Point4D ChangePrecision() => (X.ChangePrecision(), Y.ChangePrecision(), Z.ChangePrecision(), W.ChangePrecision());
        public static implicit operator Ara3D.DoublePrecision.Point4D(Point4D self) => self.ChangePrecision();
        public static implicit operator (Number, Number, Number, Number)(Point4D self) => (self.X, self.Y, self.Z, self.W);
        public static implicit operator Point4D((Number, Number, Number, Number) value) => new Point4D(value.Item1, value.Item2, value.Item3, value.Item4);
        public void Deconstruct(out Number x, out Number y, out Number z, out Number w) { x = X; y = Y; z = Z; w = W; }
        public override bool Equals(object obj) { if (!(obj is Point4D)) return false; var other = (Point4D)obj; return X.Equals(other.X) && Y.Equals(other.Y) && Z.Equals(other.Z) && W.Equals(other.W); }
        public override int GetHashCode() => Intrinsics.CombineHashCodes(X, Y, Z, W);
        public override string ToString() => Intrinsics.MakeString(TypeName, FieldNames, FieldValues);
        public static implicit operator Dynamic(Point4D self) => new Dynamic(self);
        public static implicit operator Point4D(Dynamic value) => value.As<Point4D>();
        public String TypeName => "Point4D";
        public Array<String> FieldNames => Intrinsics.MakeArray<String>((String)"X", (String)"Y", (String)"Z", (String)"W");
        public Array<Dynamic> FieldValues => Intrinsics.MakeArray<Dynamic>(new Dynamic(X), new Dynamic(Y), new Dynamic(Z), new Dynamic(W));
        // Unimplemented concept functions
        public Boolean Between(Point4D a, Point4D b) => (X.Between(a.X, b.X) & Y.Between(a.Y, b.Y) & Z.Between(a.Z, b.Z) & W.Between(a.W, b.W));
        public Point4D Clamp(Point4D a, Point4D b) => (X.Clamp(a.X, b.X), Y.Clamp(a.Y, b.Y), Z.Clamp(a.Z, b.Z), W.Clamp(a.W, b.W));
        public Point4D Lerp(Point4D b, Number amount) => (X.Lerp(b.X, amount), Y.Lerp(b.Y, amount), Z.Lerp(b.Z, amount), W.Lerp(b.W, amount));
        public Boolean Equals(Point4D b) => (X.Equals(b.X) & Y.Equals(b.Y) & Z.Equals(b.Z) & W.Equals(b.W));
        public static Boolean operator ==(Point4D a, Point4D b) => a.Equals(b);
        public Boolean NotEquals(Point4D b) => (X.NotEquals(b.X) & Y.NotEquals(b.Y) & Z.NotEquals(b.Z) & W.NotEquals(b.W));
        public static Boolean operator !=(Point4D a, Point4D b) => a.NotEquals(b);
        public Integer Count => 4;
        public Number At(Integer n) => n == 0 ? X : n == 1 ? Y : n == 2 ? Z : n == 3 ? W : throw new System.IndexOutOfRangeException();
        public Number this[Integer n] => At(n);
    }
    public readonly partial struct Line4D: Value<Line4D>, Array<Point4D>
    {
        public readonly Point4D A;
        public readonly Point4D B;
        public Line4D WithA(Point4D a) => (a, B);
        public Line4D WithB(Point4D b) => (A, b);
        public Line4D(Point4D a, Point4D b) => (A, B) = (a, b);
        public static Line4D Default = new Line4D();
        public static Line4D New(Point4D a, Point4D b) => new Line4D(a, b);
        public Ara3D.DoublePrecision.Line4D ChangePrecision() => (A.ChangePrecision(), B.ChangePrecision());
        public static implicit operator Ara3D.DoublePrecision.Line4D(Line4D self) => self.ChangePrecision();
        public static implicit operator (Point4D, Point4D)(Line4D self) => (self.A, self.B);
        public static implicit operator Line4D((Point4D, Point4D) value) => new Line4D(value.Item1, value.Item2);
        public void Deconstruct(out Point4D a, out Point4D b) { a = A; b = B; }
        public override bool Equals(object obj) { if (!(obj is Line4D)) return false; var other = (Line4D)obj; return A.Equals(other.A) && B.Equals(other.B); }
        public override int GetHashCode() => Intrinsics.CombineHashCodes(A, B);
        public override string ToString() => Intrinsics.MakeString(TypeName, FieldNames, FieldValues);
        public static implicit operator Dynamic(Line4D self) => new Dynamic(self);
        public static implicit operator Line4D(Dynamic value) => value.As<Line4D>();
        public String TypeName => "Line4D";
        public Array<String> FieldNames => Intrinsics.MakeArray<String>((String)"A", (String)"B");
        public Array<Dynamic> FieldValues => Intrinsics.MakeArray<Dynamic>(new Dynamic(A), new Dynamic(B));
        // Unimplemented concept functions
        public Boolean Equals(Line4D b) => (A.Equals(b.A) & B.Equals(b.B));
        public static Boolean operator ==(Line4D a, Line4D b) => a.Equals(b);
        public Boolean NotEquals(Line4D b) => (A.NotEquals(b.A) & B.NotEquals(b.B));
        public static Boolean operator !=(Line4D a, Line4D b) => a.NotEquals(b);
        public Integer Count => 2;
        public Point4D At(Integer n) => n == 0 ? A : n == 1 ? B : throw new System.IndexOutOfRangeException();
        public Point4D this[Integer n] => At(n);
    }
    public readonly partial struct Quadray: Vector<Quadray>
    {
        public readonly Number X;
        public readonly Number Y;
        public readonly Number Z;
        public readonly Number W;
        public Quadray WithX(Number x) => (x, Y, Z, W);
        public Quadray WithY(Number y) => (X, y, Z, W);
        public Quadray WithZ(Number z) => (X, Y, z, W);
        public Quadray WithW(Number w) => (X, Y, Z, w);
        public Quadray(Number x, Number y, Number z, Number w) => (X, Y, Z, W) = (x, y, z, w);
        public static Quadray Default = new Quadray();
        public static Quadray New(Number x, Number y, Number z, Number w) => new Quadray(x, y, z, w);
        public Ara3D.DoublePrecision.Quadray ChangePrecision() => (X.ChangePrecision(), Y.ChangePrecision(), Z.ChangePrecision(), W.ChangePrecision());
        public static implicit operator Ara3D.DoublePrecision.Quadray(Quadray self) => self.ChangePrecision();
        public static implicit operator (Number, Number, Number, Number)(Quadray self) => (self.X, self.Y, self.Z, self.W);
        public static implicit operator Quadray((Number, Number, Number, Number) value) => new Quadray(value.Item1, value.Item2, value.Item3, value.Item4);
        public void Deconstruct(out Number x, out Number y, out Number z, out Number w) { x = X; y = Y; z = Z; w = W; }
        public override bool Equals(object obj) { if (!(obj is Quadray)) return false; var other = (Quadray)obj; return X.Equals(other.X) && Y.Equals(other.Y) && Z.Equals(other.Z) && W.Equals(other.W); }
        public override int GetHashCode() => Intrinsics.CombineHashCodes(X, Y, Z, W);
        public override string ToString() => Intrinsics.MakeString(TypeName, FieldNames, FieldValues);
        public static implicit operator Dynamic(Quadray self) => new Dynamic(self);
        public static implicit operator Quadray(Dynamic value) => value.As<Quadray>();
        public String TypeName => "Quadray";
        public Array<String> FieldNames => Intrinsics.MakeArray<String>((String)"X", (String)"Y", (String)"Z", (String)"W");
        public Array<Dynamic> FieldValues => Intrinsics.MakeArray<Dynamic>(new Dynamic(X), new Dynamic(Y), new Dynamic(Z), new Dynamic(W));
        // Unimplemented concept functions
        public Quadray Add(Number other) => (X.Add(other), Y.Add(other), Z.Add(other), W.Add(other));
        public static Quadray operator +(Quadray self, Number other) => self.Add(other);
        public Quadray Subtract(Number other) => (X.Subtract(other), Y.Subtract(other), Z.Subtract(other), W.Subtract(other));
        public static Quadray operator -(Quadray self, Number other) => self.Subtract(other);
        public Quadray Multiply(Number other) => (X.Multiply(other), Y.Multiply(other), Z.Multiply(other), W.Multiply(other));
        public static Quadray operator *(Quadray self, Number other) => self.Multiply(other);
        public Quadray Divide(Number other) => (X.Divide(other), Y.Divide(other), Z.Divide(other), W.Divide(other));
        public static Quadray operator /(Quadray self, Number other) => self.Divide(other);
        public Quadray Modulo(Number other) => (X.Modulo(other), Y.Modulo(other), Z.Modulo(other), W.Modulo(other));
        public static Quadray operator %(Quadray self, Number other) => self.Modulo(other);
        public Quadray Reciprocal => (X.Reciprocal, Y.Reciprocal, Z.Reciprocal, W.Reciprocal);
        public Quadray Negative => (X.Negative, Y.Negative, Z.Negative, W.Negative);
        public static Quadray operator -(Quadray self) => self.Negative;
        public Quadray Multiply(Quadray other) => (X.Multiply(other.X), Y.Multiply(other.Y), Z.Multiply(other.Z), W.Multiply(other.W));
        public static Quadray operator *(Quadray self, Quadray other) => self.Multiply(other);
        public Quadray Divide(Quadray other) => (X.Divide(other.X), Y.Divide(other.Y), Z.Divide(other.Z), W.Divide(other.W));
        public static Quadray operator /(Quadray self, Quadray other) => self.Divide(other);
        public Quadray Modulo(Quadray other) => (X.Modulo(other.X), Y.Modulo(other.Y), Z.Modulo(other.Z), W.Modulo(other.W));
        public static Quadray operator %(Quadray self, Quadray other) => self.Modulo(other);
        public Quadray Add(Quadray other) => (X.Add(other.X), Y.Add(other.Y), Z.Add(other.Z), W.Add(other.W));
        public static Quadray operator +(Quadray self, Quadray other) => self.Add(other);
        public Quadray Subtract(Quadray other) => (X.Subtract(other.X), Y.Subtract(other.Y), Z.Subtract(other.Z), W.Subtract(other.W));
        public static Quadray operator -(Quadray self, Quadray other) => self.Subtract(other);
        public Quadray Zero => (X.Zero, Y.Zero, Z.Zero, W.Zero);
        public Quadray One => (X.One, Y.One, Z.One, W.One);
        public Quadray MinValue => (X.MinValue, Y.MinValue, Z.MinValue, W.MinValue);
        public Quadray MaxValue => (X.MaxValue, Y.MaxValue, Z.MaxValue, W.MaxValue);
        public Boolean Equals(Quadray b) => (X.Equals(b.X) & Y.Equals(b.Y) & Z.Equals(b.Z) & W.Equals(b.W));
        public static Boolean operator ==(Quadray a, Quadray b) => a.Equals(b);
        public Boolean NotEquals(Quadray b) => (X.NotEquals(b.X) & Y.NotEquals(b.Y) & Z.NotEquals(b.Z) & W.NotEquals(b.W));
        public static Boolean operator !=(Quadray a, Quadray b) => a.NotEquals(b);
        public Boolean Between(Quadray a, Quadray b) => (X.Between(a.X, b.X) & Y.Between(a.Y, b.Y) & Z.Between(a.Z, b.Z) & W.Between(a.W, b.W));
        public Quadray Clamp(Quadray a, Quadray b) => (X.Clamp(a.X, b.X), Y.Clamp(a.Y, b.Y), Z.Clamp(a.Z, b.Z), W.Clamp(a.W, b.W));
        public Integer Count => 4;
        public Number At(Integer n) => n == 0 ? X : n == 1 ? Y : n == 2 ? Z : n == 3 ? W : throw new System.IndexOutOfRangeException();
        public Number this[Integer n] => At(n);
    }
    public readonly partial struct Number: Real<Number>, Numerical<Number>, Arithmetic<Number>, Comparable<Number>
    {
        public readonly float Value;
        public Number WithValue(float value) => (value);
        public Number(float value) => (Value) = (value);
        public static Number Default = new Number();
        public static Number New(float value) => new Number(value);
        public Ara3D.DoublePrecision.Number ChangePrecision() => (Value.ChangePrecision());
        public static implicit operator Ara3D.DoublePrecision.Number(Number self) => self.ChangePrecision();
        public static implicit operator float(Number self) => self.Value;
        public static implicit operator Number(float value) => new Number(value);
        public override bool Equals(object obj) { if (!(obj is Number)) return false; var other = (Number)obj; return Value.Equals(other.Value); }
        public override int GetHashCode() => Intrinsics.CombineHashCodes(Value);
        public override string ToString() => Intrinsics.MakeString(TypeName, FieldNames, FieldValues);
        public static implicit operator Dynamic(Number self) => new Dynamic(self);
        public static implicit operator Number(Dynamic value) => value.As<Number>();
        public String TypeName => "Number";
        public Array<String> FieldNames => Intrinsics.MakeArray<String>((String)"Value");
        public Array<Dynamic> FieldValues => Intrinsics.MakeArray<Dynamic>(new Dynamic(Value));
        Number Real<Number>.Value => Value;
    }
    public readonly partial struct Integer: WholeNumber<Integer>
    {
        public readonly int Value;
        public Integer WithValue(int value) => (value);
        public Integer(int value) => (Value) = (value);
        public static Integer Default = new Integer();
        public static Integer New(int value) => new Integer(value);
        public Ara3D.DoublePrecision.Integer ChangePrecision() => (Value.ChangePrecision());
        public static implicit operator Ara3D.DoublePrecision.Integer(Integer self) => self.ChangePrecision();
        public static implicit operator int(Integer self) => self.Value;
        public static implicit operator Integer(int value) => new Integer(value);
        public override bool Equals(object obj) { if (!(obj is Integer)) return false; var other = (Integer)obj; return Value.Equals(other.Value); }
        public override int GetHashCode() => Intrinsics.CombineHashCodes(Value);
        public override string ToString() => Intrinsics.MakeString(TypeName, FieldNames, FieldValues);
        public static implicit operator Dynamic(Integer self) => new Dynamic(self);
        public static implicit operator Integer(Dynamic value) => value.As<Integer>();
        public String TypeName => "Integer";
        public Array<String> FieldNames => Intrinsics.MakeArray<String>((String)"Value");
        public Array<Dynamic> FieldValues => Intrinsics.MakeArray<Dynamic>(new Dynamic(Value));
        Integer WholeNumber<Integer>.Value => Value;
    }
    public readonly partial struct String: Array<Character>, Comparable<String>, Equatable<String>
    {
        public readonly string Value;
        public String WithValue(string value) => (value);
        public String(string value) => (Value) = (value);
        public static String Default = new String();
        public static String New(string value) => new String(value);
        public Ara3D.DoublePrecision.String ChangePrecision() => (Value.ChangePrecision());
        public static implicit operator Ara3D.DoublePrecision.String(String self) => self.ChangePrecision();
        public static implicit operator string(String self) => self.Value;
        public static implicit operator String(string value) => new String(value);
        public override bool Equals(object obj) { if (!(obj is String)) return false; var other = (String)obj; return Value.Equals(other.Value); }
        public override int GetHashCode() => Intrinsics.CombineHashCodes(Value);
        public override string ToString() => Intrinsics.MakeString(TypeName, FieldNames, FieldValues);
        public static implicit operator Dynamic(String self) => new Dynamic(self);
        public static implicit operator String(Dynamic value) => value.As<String>();
        public String TypeName => "String";
        public Array<String> FieldNames => Intrinsics.MakeArray<String>((String)"Value");
        public Array<Dynamic> FieldValues => Intrinsics.MakeArray<Dynamic>(new Dynamic(Value));
    }
    public readonly partial struct Boolean: BooleanOperations<Boolean>
    {
        public readonly bool Value;
        public Boolean WithValue(bool value) => (value);
        public Boolean(bool value) => (Value) = (value);
        public static Boolean Default = new Boolean();
        public static Boolean New(bool value) => new Boolean(value);
        public Ara3D.DoublePrecision.Boolean ChangePrecision() => (Value.ChangePrecision());
        public static implicit operator Ara3D.DoublePrecision.Boolean(Boolean self) => self.ChangePrecision();
        public static implicit operator bool(Boolean self) => self.Value;
        public static implicit operator Boolean(bool value) => new Boolean(value);
        public override bool Equals(object obj) { if (!(obj is Boolean)) return false; var other = (Boolean)obj; return Value.Equals(other.Value); }
        public override int GetHashCode() => Intrinsics.CombineHashCodes(Value);
        public override string ToString() => Intrinsics.MakeString(TypeName, FieldNames, FieldValues);
        public static implicit operator Dynamic(Boolean self) => new Dynamic(self);
        public static implicit operator Boolean(Dynamic value) => value.As<Boolean>();
        public String TypeName => "Boolean";
        public Array<String> FieldNames => Intrinsics.MakeArray<String>((String)"Value");
        public Array<Dynamic> FieldValues => Intrinsics.MakeArray<Dynamic>(new Dynamic(Value));
    }
    public readonly partial struct Character: Value<Character>
    {
        public readonly char Value;
        public Character WithValue(char value) => (value);
        public Character(char value) => (Value) = (value);
        public static Character Default = new Character();
        public static Character New(char value) => new Character(value);
        public Ara3D.DoublePrecision.Character ChangePrecision() => (Value.ChangePrecision());
        public static implicit operator Ara3D.DoublePrecision.Character(Character self) => self.ChangePrecision();
        public static implicit operator char(Character self) => self.Value;
        public static implicit operator Character(char value) => new Character(value);
        public override bool Equals(object obj) { if (!(obj is Character)) return false; var other = (Character)obj; return Value.Equals(other.Value); }
        public override int GetHashCode() => Intrinsics.CombineHashCodes(Value);
        public override string ToString() => Intrinsics.MakeString(TypeName, FieldNames, FieldValues);
        public static implicit operator Dynamic(Character self) => new Dynamic(self);
        public static implicit operator Character(Dynamic value) => value.As<Character>();
        public String TypeName => "Character";
        public Array<String> FieldNames => Intrinsics.MakeArray<String>((String)"Value");
        public Array<Dynamic> FieldValues => Intrinsics.MakeArray<Dynamic>(new Dynamic(Value));
    }
    public readonly partial struct Type
    {
        public readonly System.Type Value;
        public Type WithValue(System.Type value) => (value);
        public Type(System.Type value) => (Value) = (value);
        public static Type Default = new Type();
        public static Type New(System.Type value) => new Type(value);
        public Ara3D.DoublePrecision.Type ChangePrecision() => (Value.ChangePrecision());
        public static implicit operator Ara3D.DoublePrecision.Type(Type self) => self.ChangePrecision();
        public static implicit operator System.Type(Type self) => self.Value;
        public static implicit operator Type(System.Type value) => new Type(value);
        public override bool Equals(object obj) { if (!(obj is Type)) return false; var other = (Type)obj; return Value.Equals(other.Value); }
        public override int GetHashCode() => Intrinsics.CombineHashCodes(Value);
        public override string ToString() => Intrinsics.MakeString(TypeName, FieldNames, FieldValues);
        public static implicit operator Dynamic(Type self) => new Dynamic(self);
        public static implicit operator Type(Dynamic value) => value.As<Type>();
        public String TypeName => "Type";
        public Array<String> FieldNames => Intrinsics.MakeArray<String>((String)"Value");
        public Array<Dynamic> FieldValues => Intrinsics.MakeArray<Dynamic>(new Dynamic(Value));
    }
    public readonly partial struct Error
    {
        public static Error Default = new Error();
        public static Error New() => new Error();
        public override bool Equals(object obj) => true;public override int GetHashCode() => Intrinsics.CombineHashCodes();
        public override string ToString() => Intrinsics.MakeString(TypeName, FieldNames, FieldValues);
        public static implicit operator Dynamic(Error self) => new Dynamic(self);
        public static implicit operator Error(Dynamic value) => value.As<Error>();
        public String TypeName => "Error";
        public Array<String> FieldNames => Intrinsics.MakeArray<String>();
        public Array<Dynamic> FieldValues => Intrinsics.MakeArray<Dynamic>();
        // Unimplemented concept functions
    }
    public readonly partial struct Tuple2<T0, T1>
    {
        public readonly T0 X0;
        public readonly T1 X1;
        public Tuple2<T0, T1> WithX0(T0 x0) => (x0, X1);
        public Tuple2<T0, T1> WithX1(T1 x1) => (X0, x1);
        public Tuple2(T0 x0, T1 x1) => (X0, X1) = (x0, x1);
        public static Tuple2<T0, T1> Default = new Tuple2<T0, T1>();
        public static Tuple2<T0, T1> New(T0 x0, T1 x1) => new Tuple2<T0, T1>(x0, x1);
        public static implicit operator (T0, T1)(Tuple2<T0, T1> self) => (self.X0, self.X1);
        public static implicit operator Tuple2<T0, T1>((T0, T1) value) => new Tuple2<T0, T1>(value.Item1, value.Item2);
        public void Deconstruct(out T0 x0, out T1 x1) { x0 = X0; x1 = X1; }
        public override bool Equals(object obj) { if (!(obj is Tuple2<T0, T1>)) return false; var other = (Tuple2<T0, T1>)obj; return X0.Equals(other.X0) && X1.Equals(other.X1); }
        public override int GetHashCode() => Intrinsics.CombineHashCodes(X0, X1);
        public override string ToString() => Intrinsics.MakeString(TypeName, FieldNames, FieldValues);
        public static implicit operator Dynamic(Tuple2<T0, T1> self) => new Dynamic(self);
        public static implicit operator Tuple2<T0, T1>(Dynamic value) => value.As<Tuple2<T0, T1>>();
        public String TypeName => "Tuple2<T0, T1>";
        public Array<String> FieldNames => Intrinsics.MakeArray<String>((String)"X0", (String)"X1");
        public Array<Dynamic> FieldValues => Intrinsics.MakeArray<Dynamic>(new Dynamic(X0), new Dynamic(X1));
        // Unimplemented concept functions
    }
    public readonly partial struct Tuple3<T0, T1, T2>
    {
        public readonly T0 X0;
        public readonly T1 X1;
        public readonly T2 X2;
        public Tuple3<T0, T1, T2> WithX0(T0 x0) => (x0, X1, X2);
        public Tuple3<T0, T1, T2> WithX1(T1 x1) => (X0, x1, X2);
        public Tuple3<T0, T1, T2> WithX2(T2 x2) => (X0, X1, x2);
        public Tuple3(T0 x0, T1 x1, T2 x2) => (X0, X1, X2) = (x0, x1, x2);
        public static Tuple3<T0, T1, T2> Default = new Tuple3<T0, T1, T2>();
        public static Tuple3<T0, T1, T2> New(T0 x0, T1 x1, T2 x2) => new Tuple3<T0, T1, T2>(x0, x1, x2);
        public static implicit operator (T0, T1, T2)(Tuple3<T0, T1, T2> self) => (self.X0, self.X1, self.X2);
        public static implicit operator Tuple3<T0, T1, T2>((T0, T1, T2) value) => new Tuple3<T0, T1, T2>(value.Item1, value.Item2, value.Item3);
        public void Deconstruct(out T0 x0, out T1 x1, out T2 x2) { x0 = X0; x1 = X1; x2 = X2; }
        public override bool Equals(object obj) { if (!(obj is Tuple3<T0, T1, T2>)) return false; var other = (Tuple3<T0, T1, T2>)obj; return X0.Equals(other.X0) && X1.Equals(other.X1) && X2.Equals(other.X2); }
        public override int GetHashCode() => Intrinsics.CombineHashCodes(X0, X1, X2);
        public override string ToString() => Intrinsics.MakeString(TypeName, FieldNames, FieldValues);
        public static implicit operator Dynamic(Tuple3<T0, T1, T2> self) => new Dynamic(self);
        public static implicit operator Tuple3<T0, T1, T2>(Dynamic value) => value.As<Tuple3<T0, T1, T2>>();
        public String TypeName => "Tuple3<T0, T1, T2>";
        public Array<String> FieldNames => Intrinsics.MakeArray<String>((String)"X0", (String)"X1", (String)"X2");
        public Array<Dynamic> FieldValues => Intrinsics.MakeArray<Dynamic>(new Dynamic(X0), new Dynamic(X1), new Dynamic(X2));
        // Unimplemented concept functions
    }
    public readonly partial struct Tuple4<T0, T1, T2, T3>
    {
        public readonly T0 X0;
        public readonly T1 X1;
        public readonly T2 X2;
        public readonly T3 X3;
        public Tuple4<T0, T1, T2, T3> WithX0(T0 x0) => (x0, X1, X2, X3);
        public Tuple4<T0, T1, T2, T3> WithX1(T1 x1) => (X0, x1, X2, X3);
        public Tuple4<T0, T1, T2, T3> WithX2(T2 x2) => (X0, X1, x2, X3);
        public Tuple4<T0, T1, T2, T3> WithX3(T3 x3) => (X0, X1, X2, x3);
        public Tuple4(T0 x0, T1 x1, T2 x2, T3 x3) => (X0, X1, X2, X3) = (x0, x1, x2, x3);
        public static Tuple4<T0, T1, T2, T3> Default = new Tuple4<T0, T1, T2, T3>();
        public static Tuple4<T0, T1, T2, T3> New(T0 x0, T1 x1, T2 x2, T3 x3) => new Tuple4<T0, T1, T2, T3>(x0, x1, x2, x3);
        public static implicit operator (T0, T1, T2, T3)(Tuple4<T0, T1, T2, T3> self) => (self.X0, self.X1, self.X2, self.X3);
        public static implicit operator Tuple4<T0, T1, T2, T3>((T0, T1, T2, T3) value) => new Tuple4<T0, T1, T2, T3>(value.Item1, value.Item2, value.Item3, value.Item4);
        public void Deconstruct(out T0 x0, out T1 x1, out T2 x2, out T3 x3) { x0 = X0; x1 = X1; x2 = X2; x3 = X3; }
        public override bool Equals(object obj) { if (!(obj is Tuple4<T0, T1, T2, T3>)) return false; var other = (Tuple4<T0, T1, T2, T3>)obj; return X0.Equals(other.X0) && X1.Equals(other.X1) && X2.Equals(other.X2) && X3.Equals(other.X3); }
        public override int GetHashCode() => Intrinsics.CombineHashCodes(X0, X1, X2, X3);
        public override string ToString() => Intrinsics.MakeString(TypeName, FieldNames, FieldValues);
        public static implicit operator Dynamic(Tuple4<T0, T1, T2, T3> self) => new Dynamic(self);
        public static implicit operator Tuple4<T0, T1, T2, T3>(Dynamic value) => value.As<Tuple4<T0, T1, T2, T3>>();
        public String TypeName => "Tuple4<T0, T1, T2, T3>";
        public Array<String> FieldNames => Intrinsics.MakeArray<String>((String)"X0", (String)"X1", (String)"X2", (String)"X3");
        public Array<Dynamic> FieldValues => Intrinsics.MakeArray<Dynamic>(new Dynamic(X0), new Dynamic(X1), new Dynamic(X2), new Dynamic(X3));
        // Unimplemented concept functions
    }
    public readonly partial struct Tuple5<T0, T1, T2, T3, T4>
    {
        public readonly T0 X0;
        public readonly T1 X1;
        public readonly T2 X2;
        public readonly T3 X3;
        public readonly T4 X4;
        public Tuple5<T0, T1, T2, T3, T4> WithX0(T0 x0) => (x0, X1, X2, X3, X4);
        public Tuple5<T0, T1, T2, T3, T4> WithX1(T1 x1) => (X0, x1, X2, X3, X4);
        public Tuple5<T0, T1, T2, T3, T4> WithX2(T2 x2) => (X0, X1, x2, X3, X4);
        public Tuple5<T0, T1, T2, T3, T4> WithX3(T3 x3) => (X0, X1, X2, x3, X4);
        public Tuple5<T0, T1, T2, T3, T4> WithX4(T4 x4) => (X0, X1, X2, X3, x4);
        public Tuple5(T0 x0, T1 x1, T2 x2, T3 x3, T4 x4) => (X0, X1, X2, X3, X4) = (x0, x1, x2, x3, x4);
        public static Tuple5<T0, T1, T2, T3, T4> Default = new Tuple5<T0, T1, T2, T3, T4>();
        public static Tuple5<T0, T1, T2, T3, T4> New(T0 x0, T1 x1, T2 x2, T3 x3, T4 x4) => new Tuple5<T0, T1, T2, T3, T4>(x0, x1, x2, x3, x4);
        public static implicit operator (T0, T1, T2, T3, T4)(Tuple5<T0, T1, T2, T3, T4> self) => (self.X0, self.X1, self.X2, self.X3, self.X4);
        public static implicit operator Tuple5<T0, T1, T2, T3, T4>((T0, T1, T2, T3, T4) value) => new Tuple5<T0, T1, T2, T3, T4>(value.Item1, value.Item2, value.Item3, value.Item4, value.Item5);
        public void Deconstruct(out T0 x0, out T1 x1, out T2 x2, out T3 x3, out T4 x4) { x0 = X0; x1 = X1; x2 = X2; x3 = X3; x4 = X4; }
        public override bool Equals(object obj) { if (!(obj is Tuple5<T0, T1, T2, T3, T4>)) return false; var other = (Tuple5<T0, T1, T2, T3, T4>)obj; return X0.Equals(other.X0) && X1.Equals(other.X1) && X2.Equals(other.X2) && X3.Equals(other.X3) && X4.Equals(other.X4); }
        public override int GetHashCode() => Intrinsics.CombineHashCodes(X0, X1, X2, X3, X4);
        public override string ToString() => Intrinsics.MakeString(TypeName, FieldNames, FieldValues);
        public static implicit operator Dynamic(Tuple5<T0, T1, T2, T3, T4> self) => new Dynamic(self);
        public static implicit operator Tuple5<T0, T1, T2, T3, T4>(Dynamic value) => value.As<Tuple5<T0, T1, T2, T3, T4>>();
        public String TypeName => "Tuple5<T0, T1, T2, T3, T4>";
        public Array<String> FieldNames => Intrinsics.MakeArray<String>((String)"X0", (String)"X1", (String)"X2", (String)"X3", (String)"X4");
        public Array<Dynamic> FieldValues => Intrinsics.MakeArray<Dynamic>(new Dynamic(X0), new Dynamic(X1), new Dynamic(X2), new Dynamic(X3), new Dynamic(X4));
        // Unimplemented concept functions
    }
    public readonly partial struct Tuple6<T0, T1, T2, T3, T4, T5>
    {
        public readonly T0 X0;
        public readonly T1 X1;
        public readonly T2 X2;
        public readonly T3 X3;
        public readonly T4 X4;
        public readonly T5 X5;
        public Tuple6<T0, T1, T2, T3, T4, T5> WithX0(T0 x0) => (x0, X1, X2, X3, X4, X5);
        public Tuple6<T0, T1, T2, T3, T4, T5> WithX1(T1 x1) => (X0, x1, X2, X3, X4, X5);
        public Tuple6<T0, T1, T2, T3, T4, T5> WithX2(T2 x2) => (X0, X1, x2, X3, X4, X5);
        public Tuple6<T0, T1, T2, T3, T4, T5> WithX3(T3 x3) => (X0, X1, X2, x3, X4, X5);
        public Tuple6<T0, T1, T2, T3, T4, T5> WithX4(T4 x4) => (X0, X1, X2, X3, x4, X5);
        public Tuple6<T0, T1, T2, T3, T4, T5> WithX5(T5 x5) => (X0, X1, X2, X3, X4, x5);
        public Tuple6(T0 x0, T1 x1, T2 x2, T3 x3, T4 x4, T5 x5) => (X0, X1, X2, X3, X4, X5) = (x0, x1, x2, x3, x4, x5);
        public static Tuple6<T0, T1, T2, T3, T4, T5> Default = new Tuple6<T0, T1, T2, T3, T4, T5>();
        public static Tuple6<T0, T1, T2, T3, T4, T5> New(T0 x0, T1 x1, T2 x2, T3 x3, T4 x4, T5 x5) => new Tuple6<T0, T1, T2, T3, T4, T5>(x0, x1, x2, x3, x4, x5);
        public static implicit operator (T0, T1, T2, T3, T4, T5)(Tuple6<T0, T1, T2, T3, T4, T5> self) => (self.X0, self.X1, self.X2, self.X3, self.X4, self.X5);
        public static implicit operator Tuple6<T0, T1, T2, T3, T4, T5>((T0, T1, T2, T3, T4, T5) value) => new Tuple6<T0, T1, T2, T3, T4, T5>(value.Item1, value.Item2, value.Item3, value.Item4, value.Item5, value.Item6);
        public void Deconstruct(out T0 x0, out T1 x1, out T2 x2, out T3 x3, out T4 x4, out T5 x5) { x0 = X0; x1 = X1; x2 = X2; x3 = X3; x4 = X4; x5 = X5; }
        public override bool Equals(object obj) { if (!(obj is Tuple6<T0, T1, T2, T3, T4, T5>)) return false; var other = (Tuple6<T0, T1, T2, T3, T4, T5>)obj; return X0.Equals(other.X0) && X1.Equals(other.X1) && X2.Equals(other.X2) && X3.Equals(other.X3) && X4.Equals(other.X4) && X5.Equals(other.X5); }
        public override int GetHashCode() => Intrinsics.CombineHashCodes(X0, X1, X2, X3, X4, X5);
        public override string ToString() => Intrinsics.MakeString(TypeName, FieldNames, FieldValues);
        public static implicit operator Dynamic(Tuple6<T0, T1, T2, T3, T4, T5> self) => new Dynamic(self);
        public static implicit operator Tuple6<T0, T1, T2, T3, T4, T5>(Dynamic value) => value.As<Tuple6<T0, T1, T2, T3, T4, T5>>();
        public String TypeName => "Tuple6<T0, T1, T2, T3, T4, T5>";
        public Array<String> FieldNames => Intrinsics.MakeArray<String>((String)"X0", (String)"X1", (String)"X2", (String)"X3", (String)"X4", (String)"X5");
        public Array<Dynamic> FieldValues => Intrinsics.MakeArray<Dynamic>(new Dynamic(X0), new Dynamic(X1), new Dynamic(X2), new Dynamic(X3), new Dynamic(X4), new Dynamic(X5));
        // Unimplemented concept functions
    }
    public readonly partial struct Tuple7<T0, T1, T2, T3, T4, T5, T6>
    {
        public readonly T0 X0;
        public readonly T1 X1;
        public readonly T2 X2;
        public readonly T3 X3;
        public readonly T4 X4;
        public readonly T5 X5;
        public readonly T6 X6;
        public Tuple7<T0, T1, T2, T3, T4, T5, T6> WithX0(T0 x0) => (x0, X1, X2, X3, X4, X5, X6);
        public Tuple7<T0, T1, T2, T3, T4, T5, T6> WithX1(T1 x1) => (X0, x1, X2, X3, X4, X5, X6);
        public Tuple7<T0, T1, T2, T3, T4, T5, T6> WithX2(T2 x2) => (X0, X1, x2, X3, X4, X5, X6);
        public Tuple7<T0, T1, T2, T3, T4, T5, T6> WithX3(T3 x3) => (X0, X1, X2, x3, X4, X5, X6);
        public Tuple7<T0, T1, T2, T3, T4, T5, T6> WithX4(T4 x4) => (X0, X1, X2, X3, x4, X5, X6);
        public Tuple7<T0, T1, T2, T3, T4, T5, T6> WithX5(T5 x5) => (X0, X1, X2, X3, X4, x5, X6);
        public Tuple7<T0, T1, T2, T3, T4, T5, T6> WithX6(T6 x6) => (X0, X1, X2, X3, X4, X5, x6);
        public Tuple7(T0 x0, T1 x1, T2 x2, T3 x3, T4 x4, T5 x5, T6 x6) => (X0, X1, X2, X3, X4, X5, X6) = (x0, x1, x2, x3, x4, x5, x6);
        public static Tuple7<T0, T1, T2, T3, T4, T5, T6> Default = new Tuple7<T0, T1, T2, T3, T4, T5, T6>();
        public static Tuple7<T0, T1, T2, T3, T4, T5, T6> New(T0 x0, T1 x1, T2 x2, T3 x3, T4 x4, T5 x5, T6 x6) => new Tuple7<T0, T1, T2, T3, T4, T5, T6>(x0, x1, x2, x3, x4, x5, x6);
        public static implicit operator (T0, T1, T2, T3, T4, T5, T6)(Tuple7<T0, T1, T2, T3, T4, T5, T6> self) => (self.X0, self.X1, self.X2, self.X3, self.X4, self.X5, self.X6);
        public static implicit operator Tuple7<T0, T1, T2, T3, T4, T5, T6>((T0, T1, T2, T3, T4, T5, T6) value) => new Tuple7<T0, T1, T2, T3, T4, T5, T6>(value.Item1, value.Item2, value.Item3, value.Item4, value.Item5, value.Item6, value.Item7);
        public void Deconstruct(out T0 x0, out T1 x1, out T2 x2, out T3 x3, out T4 x4, out T5 x5, out T6 x6) { x0 = X0; x1 = X1; x2 = X2; x3 = X3; x4 = X4; x5 = X5; x6 = X6; }
        public override bool Equals(object obj) { if (!(obj is Tuple7<T0, T1, T2, T3, T4, T5, T6>)) return false; var other = (Tuple7<T0, T1, T2, T3, T4, T5, T6>)obj; return X0.Equals(other.X0) && X1.Equals(other.X1) && X2.Equals(other.X2) && X3.Equals(other.X3) && X4.Equals(other.X4) && X5.Equals(other.X5) && X6.Equals(other.X6); }
        public override int GetHashCode() => Intrinsics.CombineHashCodes(X0, X1, X2, X3, X4, X5, X6);
        public override string ToString() => Intrinsics.MakeString(TypeName, FieldNames, FieldValues);
        public static implicit operator Dynamic(Tuple7<T0, T1, T2, T3, T4, T5, T6> self) => new Dynamic(self);
        public static implicit operator Tuple7<T0, T1, T2, T3, T4, T5, T6>(Dynamic value) => value.As<Tuple7<T0, T1, T2, T3, T4, T5, T6>>();
        public String TypeName => "Tuple7<T0, T1, T2, T3, T4, T5, T6>";
        public Array<String> FieldNames => Intrinsics.MakeArray<String>((String)"X0", (String)"X1", (String)"X2", (String)"X3", (String)"X4", (String)"X5", (String)"X6");
        public Array<Dynamic> FieldValues => Intrinsics.MakeArray<Dynamic>(new Dynamic(X0), new Dynamic(X1), new Dynamic(X2), new Dynamic(X3), new Dynamic(X4), new Dynamic(X5), new Dynamic(X6));
        // Unimplemented concept functions
    }
    public readonly partial struct Tuple8<T0, T1, T2, T3, T4, T5, T6, T7>
    {
        public readonly T0 X0;
        public readonly T1 X1;
        public readonly T2 X2;
        public readonly T3 X3;
        public readonly T4 X4;
        public readonly T5 X5;
        public readonly T6 X6;
        public readonly T7 X7;
        public Tuple8<T0, T1, T2, T3, T4, T5, T6, T7> WithX0(T0 x0) => (x0, X1, X2, X3, X4, X5, X6, X7);
        public Tuple8<T0, T1, T2, T3, T4, T5, T6, T7> WithX1(T1 x1) => (X0, x1, X2, X3, X4, X5, X6, X7);
        public Tuple8<T0, T1, T2, T3, T4, T5, T6, T7> WithX2(T2 x2) => (X0, X1, x2, X3, X4, X5, X6, X7);
        public Tuple8<T0, T1, T2, T3, T4, T5, T6, T7> WithX3(T3 x3) => (X0, X1, X2, x3, X4, X5, X6, X7);
        public Tuple8<T0, T1, T2, T3, T4, T5, T6, T7> WithX4(T4 x4) => (X0, X1, X2, X3, x4, X5, X6, X7);
        public Tuple8<T0, T1, T2, T3, T4, T5, T6, T7> WithX5(T5 x5) => (X0, X1, X2, X3, X4, x5, X6, X7);
        public Tuple8<T0, T1, T2, T3, T4, T5, T6, T7> WithX6(T6 x6) => (X0, X1, X2, X3, X4, X5, x6, X7);
        public Tuple8<T0, T1, T2, T3, T4, T5, T6, T7> WithX7(T7 x7) => (X0, X1, X2, X3, X4, X5, X6, x7);
        public Tuple8(T0 x0, T1 x1, T2 x2, T3 x3, T4 x4, T5 x5, T6 x6, T7 x7) => (X0, X1, X2, X3, X4, X5, X6, X7) = (x0, x1, x2, x3, x4, x5, x6, x7);
        public static Tuple8<T0, T1, T2, T3, T4, T5, T6, T7> Default = new Tuple8<T0, T1, T2, T3, T4, T5, T6, T7>();
        public static Tuple8<T0, T1, T2, T3, T4, T5, T6, T7> New(T0 x0, T1 x1, T2 x2, T3 x3, T4 x4, T5 x5, T6 x6, T7 x7) => new Tuple8<T0, T1, T2, T3, T4, T5, T6, T7>(x0, x1, x2, x3, x4, x5, x6, x7);
        public static implicit operator (T0, T1, T2, T3, T4, T5, T6, T7)(Tuple8<T0, T1, T2, T3, T4, T5, T6, T7> self) => (self.X0, self.X1, self.X2, self.X3, self.X4, self.X5, self.X6, self.X7);
        public static implicit operator Tuple8<T0, T1, T2, T3, T4, T5, T6, T7>((T0, T1, T2, T3, T4, T5, T6, T7) value) => new Tuple8<T0, T1, T2, T3, T4, T5, T6, T7>(value.Item1, value.Item2, value.Item3, value.Item4, value.Item5, value.Item6, value.Item7, value.Item8);
        public void Deconstruct(out T0 x0, out T1 x1, out T2 x2, out T3 x3, out T4 x4, out T5 x5, out T6 x6, out T7 x7) { x0 = X0; x1 = X1; x2 = X2; x3 = X3; x4 = X4; x5 = X5; x6 = X6; x7 = X7; }
        public override bool Equals(object obj) { if (!(obj is Tuple8<T0, T1, T2, T3, T4, T5, T6, T7>)) return false; var other = (Tuple8<T0, T1, T2, T3, T4, T5, T6, T7>)obj; return X0.Equals(other.X0) && X1.Equals(other.X1) && X2.Equals(other.X2) && X3.Equals(other.X3) && X4.Equals(other.X4) && X5.Equals(other.X5) && X6.Equals(other.X6) && X7.Equals(other.X7); }
        public override int GetHashCode() => Intrinsics.CombineHashCodes(X0, X1, X2, X3, X4, X5, X6, X7);
        public override string ToString() => Intrinsics.MakeString(TypeName, FieldNames, FieldValues);
        public static implicit operator Dynamic(Tuple8<T0, T1, T2, T3, T4, T5, T6, T7> self) => new Dynamic(self);
        public static implicit operator Tuple8<T0, T1, T2, T3, T4, T5, T6, T7>(Dynamic value) => value.As<Tuple8<T0, T1, T2, T3, T4, T5, T6, T7>>();
        public String TypeName => "Tuple8<T0, T1, T2, T3, T4, T5, T6, T7>";
        public Array<String> FieldNames => Intrinsics.MakeArray<String>((String)"X0", (String)"X1", (String)"X2", (String)"X3", (String)"X4", (String)"X5", (String)"X6", (String)"X7");
        public Array<Dynamic> FieldValues => Intrinsics.MakeArray<Dynamic>(new Dynamic(X0), new Dynamic(X1), new Dynamic(X2), new Dynamic(X3), new Dynamic(X4), new Dynamic(X5), new Dynamic(X6), new Dynamic(X7));
        // Unimplemented concept functions
    }
    public readonly partial struct Tuple9<T0, T1, T2, T3, T4, T5, T6, T7, T8>
    {
        public readonly T0 X0;
        public readonly T1 X1;
        public readonly T2 X2;
        public readonly T3 X3;
        public readonly T4 X4;
        public readonly T5 X5;
        public readonly T6 X6;
        public readonly T7 X7;
        public readonly T8 X8;
        public Tuple9<T0, T1, T2, T3, T4, T5, T6, T7, T8> WithX0(T0 x0) => (x0, X1, X2, X3, X4, X5, X6, X7, X8);
        public Tuple9<T0, T1, T2, T3, T4, T5, T6, T7, T8> WithX1(T1 x1) => (X0, x1, X2, X3, X4, X5, X6, X7, X8);
        public Tuple9<T0, T1, T2, T3, T4, T5, T6, T7, T8> WithX2(T2 x2) => (X0, X1, x2, X3, X4, X5, X6, X7, X8);
        public Tuple9<T0, T1, T2, T3, T4, T5, T6, T7, T8> WithX3(T3 x3) => (X0, X1, X2, x3, X4, X5, X6, X7, X8);
        public Tuple9<T0, T1, T2, T3, T4, T5, T6, T7, T8> WithX4(T4 x4) => (X0, X1, X2, X3, x4, X5, X6, X7, X8);
        public Tuple9<T0, T1, T2, T3, T4, T5, T6, T7, T8> WithX5(T5 x5) => (X0, X1, X2, X3, X4, x5, X6, X7, X8);
        public Tuple9<T0, T1, T2, T3, T4, T5, T6, T7, T8> WithX6(T6 x6) => (X0, X1, X2, X3, X4, X5, x6, X7, X8);
        public Tuple9<T0, T1, T2, T3, T4, T5, T6, T7, T8> WithX7(T7 x7) => (X0, X1, X2, X3, X4, X5, X6, x7, X8);
        public Tuple9<T0, T1, T2, T3, T4, T5, T6, T7, T8> WithX8(T8 x8) => (X0, X1, X2, X3, X4, X5, X6, X7, x8);
        public Tuple9(T0 x0, T1 x1, T2 x2, T3 x3, T4 x4, T5 x5, T6 x6, T7 x7, T8 x8) => (X0, X1, X2, X3, X4, X5, X6, X7, X8) = (x0, x1, x2, x3, x4, x5, x6, x7, x8);
        public static Tuple9<T0, T1, T2, T3, T4, T5, T6, T7, T8> Default = new Tuple9<T0, T1, T2, T3, T4, T5, T6, T7, T8>();
        public static Tuple9<T0, T1, T2, T3, T4, T5, T6, T7, T8> New(T0 x0, T1 x1, T2 x2, T3 x3, T4 x4, T5 x5, T6 x6, T7 x7, T8 x8) => new Tuple9<T0, T1, T2, T3, T4, T5, T6, T7, T8>(x0, x1, x2, x3, x4, x5, x6, x7, x8);
        public static implicit operator (T0, T1, T2, T3, T4, T5, T6, T7, T8)(Tuple9<T0, T1, T2, T3, T4, T5, T6, T7, T8> self) => (self.X0, self.X1, self.X2, self.X3, self.X4, self.X5, self.X6, self.X7, self.X8);
        public static implicit operator Tuple9<T0, T1, T2, T3, T4, T5, T6, T7, T8>((T0, T1, T2, T3, T4, T5, T6, T7, T8) value) => new Tuple9<T0, T1, T2, T3, T4, T5, T6, T7, T8>(value.Item1, value.Item2, value.Item3, value.Item4, value.Item5, value.Item6, value.Item7, value.Item8, value.Item9);
        public void Deconstruct(out T0 x0, out T1 x1, out T2 x2, out T3 x3, out T4 x4, out T5 x5, out T6 x6, out T7 x7, out T8 x8) { x0 = X0; x1 = X1; x2 = X2; x3 = X3; x4 = X4; x5 = X5; x6 = X6; x7 = X7; x8 = X8; }
        public override bool Equals(object obj) { if (!(obj is Tuple9<T0, T1, T2, T3, T4, T5, T6, T7, T8>)) return false; var other = (Tuple9<T0, T1, T2, T3, T4, T5, T6, T7, T8>)obj; return X0.Equals(other.X0) && X1.Equals(other.X1) && X2.Equals(other.X2) && X3.Equals(other.X3) && X4.Equals(other.X4) && X5.Equals(other.X5) && X6.Equals(other.X6) && X7.Equals(other.X7) && X8.Equals(other.X8); }
        public override int GetHashCode() => Intrinsics.CombineHashCodes(X0, X1, X2, X3, X4, X5, X6, X7, X8);
        public override string ToString() => Intrinsics.MakeString(TypeName, FieldNames, FieldValues);
        public static implicit operator Dynamic(Tuple9<T0, T1, T2, T3, T4, T5, T6, T7, T8> self) => new Dynamic(self);
        public static implicit operator Tuple9<T0, T1, T2, T3, T4, T5, T6, T7, T8>(Dynamic value) => value.As<Tuple9<T0, T1, T2, T3, T4, T5, T6, T7, T8>>();
        public String TypeName => "Tuple9<T0, T1, T2, T3, T4, T5, T6, T7, T8>";
        public Array<String> FieldNames => Intrinsics.MakeArray<String>((String)"X0", (String)"X1", (String)"X2", (String)"X3", (String)"X4", (String)"X5", (String)"X6", (String)"X7", (String)"X8");
        public Array<Dynamic> FieldValues => Intrinsics.MakeArray<Dynamic>(new Dynamic(X0), new Dynamic(X1), new Dynamic(X2), new Dynamic(X3), new Dynamic(X4), new Dynamic(X5), new Dynamic(X6), new Dynamic(X7), new Dynamic(X8));
        // Unimplemented concept functions
    }
    public readonly partial struct Tuple10<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>
    {
        public readonly T0 X0;
        public readonly T1 X1;
        public readonly T2 X2;
        public readonly T3 X3;
        public readonly T4 X4;
        public readonly T5 X5;
        public readonly T6 X6;
        public readonly T7 X7;
        public readonly T8 X8;
        public readonly T9 X9;
        public Tuple10<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9> WithX0(T0 x0) => (x0, X1, X2, X3, X4, X5, X6, X7, X8, X9);
        public Tuple10<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9> WithX1(T1 x1) => (X0, x1, X2, X3, X4, X5, X6, X7, X8, X9);
        public Tuple10<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9> WithX2(T2 x2) => (X0, X1, x2, X3, X4, X5, X6, X7, X8, X9);
        public Tuple10<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9> WithX3(T3 x3) => (X0, X1, X2, x3, X4, X5, X6, X7, X8, X9);
        public Tuple10<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9> WithX4(T4 x4) => (X0, X1, X2, X3, x4, X5, X6, X7, X8, X9);
        public Tuple10<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9> WithX5(T5 x5) => (X0, X1, X2, X3, X4, x5, X6, X7, X8, X9);
        public Tuple10<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9> WithX6(T6 x6) => (X0, X1, X2, X3, X4, X5, x6, X7, X8, X9);
        public Tuple10<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9> WithX7(T7 x7) => (X0, X1, X2, X3, X4, X5, X6, x7, X8, X9);
        public Tuple10<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9> WithX8(T8 x8) => (X0, X1, X2, X3, X4, X5, X6, X7, x8, X9);
        public Tuple10<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9> WithX9(T9 x9) => (X0, X1, X2, X3, X4, X5, X6, X7, X8, x9);
        public Tuple10(T0 x0, T1 x1, T2 x2, T3 x3, T4 x4, T5 x5, T6 x6, T7 x7, T8 x8, T9 x9) => (X0, X1, X2, X3, X4, X5, X6, X7, X8, X9) = (x0, x1, x2, x3, x4, x5, x6, x7, x8, x9);
        public static Tuple10<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9> Default = new Tuple10<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>();
        public static Tuple10<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9> New(T0 x0, T1 x1, T2 x2, T3 x3, T4 x4, T5 x5, T6 x6, T7 x7, T8 x8, T9 x9) => new Tuple10<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>(x0, x1, x2, x3, x4, x5, x6, x7, x8, x9);
        public static implicit operator (T0, T1, T2, T3, T4, T5, T6, T7, T8, T9)(Tuple10<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9> self) => (self.X0, self.X1, self.X2, self.X3, self.X4, self.X5, self.X6, self.X7, self.X8, self.X9);
        public static implicit operator Tuple10<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>((T0, T1, T2, T3, T4, T5, T6, T7, T8, T9) value) => new Tuple10<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>(value.Item1, value.Item2, value.Item3, value.Item4, value.Item5, value.Item6, value.Item7, value.Item8, value.Item9, value.Item10);
        public void Deconstruct(out T0 x0, out T1 x1, out T2 x2, out T3 x3, out T4 x4, out T5 x5, out T6 x6, out T7 x7, out T8 x8, out T9 x9) { x0 = X0; x1 = X1; x2 = X2; x3 = X3; x4 = X4; x5 = X5; x6 = X6; x7 = X7; x8 = X8; x9 = X9; }
        public override bool Equals(object obj) { if (!(obj is Tuple10<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>)) return false; var other = (Tuple10<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>)obj; return X0.Equals(other.X0) && X1.Equals(other.X1) && X2.Equals(other.X2) && X3.Equals(other.X3) && X4.Equals(other.X4) && X5.Equals(other.X5) && X6.Equals(other.X6) && X7.Equals(other.X7) && X8.Equals(other.X8) && X9.Equals(other.X9); }
        public override int GetHashCode() => Intrinsics.CombineHashCodes(X0, X1, X2, X3, X4, X5, X6, X7, X8, X9);
        public override string ToString() => Intrinsics.MakeString(TypeName, FieldNames, FieldValues);
        public static implicit operator Dynamic(Tuple10<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9> self) => new Dynamic(self);
        public static implicit operator Tuple10<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>(Dynamic value) => value.As<Tuple10<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>>();
        public String TypeName => "Tuple10<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>";
        public Array<String> FieldNames => Intrinsics.MakeArray<String>((String)"X0", (String)"X1", (String)"X2", (String)"X3", (String)"X4", (String)"X5", (String)"X6", (String)"X7", (String)"X8", (String)"X9");
        public Array<Dynamic> FieldValues => Intrinsics.MakeArray<Dynamic>(new Dynamic(X0), new Dynamic(X1), new Dynamic(X2), new Dynamic(X3), new Dynamic(X4), new Dynamic(X5), new Dynamic(X6), new Dynamic(X7), new Dynamic(X8), new Dynamic(X9));
        // Unimplemented concept functions
    }
    public readonly partial struct Function4<T0, T1, T2, T3, TR>
    {
        public static Function4<T0, T1, T2, T3, TR> Default = new Function4<T0, T1, T2, T3, TR>();
        public static Function4<T0, T1, T2, T3, TR> New() => new Function4<T0, T1, T2, T3, TR>();
        public override bool Equals(object obj) => true;public override int GetHashCode() => Intrinsics.CombineHashCodes();
        public override string ToString() => Intrinsics.MakeString(TypeName, FieldNames, FieldValues);
        public static implicit operator Dynamic(Function4<T0, T1, T2, T3, TR> self) => new Dynamic(self);
        public static implicit operator Function4<T0, T1, T2, T3, TR>(Dynamic value) => value.As<Function4<T0, T1, T2, T3, TR>>();
        public String TypeName => "Function4<T0, T1, T2, T3, TR>";
        public Array<String> FieldNames => Intrinsics.MakeArray<String>();
        public Array<Dynamic> FieldValues => Intrinsics.MakeArray<Dynamic>();
        // Unimplemented concept functions
    }
    public readonly partial struct Function5<T0, T1, T2, T3, T4, TR>
    {
        public static Function5<T0, T1, T2, T3, T4, TR> Default = new Function5<T0, T1, T2, T3, T4, TR>();
        public static Function5<T0, T1, T2, T3, T4, TR> New() => new Function5<T0, T1, T2, T3, T4, TR>();
        public override bool Equals(object obj) => true;public override int GetHashCode() => Intrinsics.CombineHashCodes();
        public override string ToString() => Intrinsics.MakeString(TypeName, FieldNames, FieldValues);
        public static implicit operator Dynamic(Function5<T0, T1, T2, T3, T4, TR> self) => new Dynamic(self);
        public static implicit operator Function5<T0, T1, T2, T3, T4, TR>(Dynamic value) => value.As<Function5<T0, T1, T2, T3, T4, TR>>();
        public String TypeName => "Function5<T0, T1, T2, T3, T4, TR>";
        public Array<String> FieldNames => Intrinsics.MakeArray<String>();
        public Array<Dynamic> FieldValues => Intrinsics.MakeArray<Dynamic>();
        // Unimplemented concept functions
    }
    public readonly partial struct Function6<T0, T1, T2, T3, T4, T5, TR>
    {
        public static Function6<T0, T1, T2, T3, T4, T5, TR> Default = new Function6<T0, T1, T2, T3, T4, T5, TR>();
        public static Function6<T0, T1, T2, T3, T4, T5, TR> New() => new Function6<T0, T1, T2, T3, T4, T5, TR>();
        public override bool Equals(object obj) => true;public override int GetHashCode() => Intrinsics.CombineHashCodes();
        public override string ToString() => Intrinsics.MakeString(TypeName, FieldNames, FieldValues);
        public static implicit operator Dynamic(Function6<T0, T1, T2, T3, T4, T5, TR> self) => new Dynamic(self);
        public static implicit operator Function6<T0, T1, T2, T3, T4, T5, TR>(Dynamic value) => value.As<Function6<T0, T1, T2, T3, T4, T5, TR>>();
        public String TypeName => "Function6<T0, T1, T2, T3, T4, T5, TR>";
        public Array<String> FieldNames => Intrinsics.MakeArray<String>();
        public Array<Dynamic> FieldValues => Intrinsics.MakeArray<Dynamic>();
        // Unimplemented concept functions
    }
    public readonly partial struct Function7<T0, T1, T2, T3, T4, T5, T6, TR>
    {
        public static Function7<T0, T1, T2, T3, T4, T5, T6, TR> Default = new Function7<T0, T1, T2, T3, T4, T5, T6, TR>();
        public static Function7<T0, T1, T2, T3, T4, T5, T6, TR> New() => new Function7<T0, T1, T2, T3, T4, T5, T6, TR>();
        public override bool Equals(object obj) => true;public override int GetHashCode() => Intrinsics.CombineHashCodes();
        public override string ToString() => Intrinsics.MakeString(TypeName, FieldNames, FieldValues);
        public static implicit operator Dynamic(Function7<T0, T1, T2, T3, T4, T5, T6, TR> self) => new Dynamic(self);
        public static implicit operator Function7<T0, T1, T2, T3, T4, T5, T6, TR>(Dynamic value) => value.As<Function7<T0, T1, T2, T3, T4, T5, T6, TR>>();
        public String TypeName => "Function7<T0, T1, T2, T3, T4, T5, T6, TR>";
        public Array<String> FieldNames => Intrinsics.MakeArray<String>();
        public Array<Dynamic> FieldValues => Intrinsics.MakeArray<Dynamic>();
        // Unimplemented concept functions
    }
    public readonly partial struct Function8<T0, T1, T2, T3, T4, T5, T6, T7, TR>
    {
        public static Function8<T0, T1, T2, T3, T4, T5, T6, T7, TR> Default = new Function8<T0, T1, T2, T3, T4, T5, T6, T7, TR>();
        public static Function8<T0, T1, T2, T3, T4, T5, T6, T7, TR> New() => new Function8<T0, T1, T2, T3, T4, T5, T6, T7, TR>();
        public override bool Equals(object obj) => true;public override int GetHashCode() => Intrinsics.CombineHashCodes();
        public override string ToString() => Intrinsics.MakeString(TypeName, FieldNames, FieldValues);
        public static implicit operator Dynamic(Function8<T0, T1, T2, T3, T4, T5, T6, T7, TR> self) => new Dynamic(self);
        public static implicit operator Function8<T0, T1, T2, T3, T4, T5, T6, T7, TR>(Dynamic value) => value.As<Function8<T0, T1, T2, T3, T4, T5, T6, T7, TR>>();
        public String TypeName => "Function8<T0, T1, T2, T3, T4, T5, T6, T7, TR>";
        public Array<String> FieldNames => Intrinsics.MakeArray<String>();
        public Array<Dynamic> FieldValues => Intrinsics.MakeArray<Dynamic>();
        // Unimplemented concept functions
    }
    public readonly partial struct Function9<T0, T1, T2, T3, T4, T5, T6, T7, T8, TR>
    {
        public static Function9<T0, T1, T2, T3, T4, T5, T6, T7, T8, TR> Default = new Function9<T0, T1, T2, T3, T4, T5, T6, T7, T8, TR>();
        public static Function9<T0, T1, T2, T3, T4, T5, T6, T7, T8, TR> New() => new Function9<T0, T1, T2, T3, T4, T5, T6, T7, T8, TR>();
        public override bool Equals(object obj) => true;public override int GetHashCode() => Intrinsics.CombineHashCodes();
        public override string ToString() => Intrinsics.MakeString(TypeName, FieldNames, FieldValues);
        public static implicit operator Dynamic(Function9<T0, T1, T2, T3, T4, T5, T6, T7, T8, TR> self) => new Dynamic(self);
        public static implicit operator Function9<T0, T1, T2, T3, T4, T5, T6, T7, T8, TR>(Dynamic value) => value.As<Function9<T0, T1, T2, T3, T4, T5, T6, T7, T8, TR>>();
        public String TypeName => "Function9<T0, T1, T2, T3, T4, T5, T6, T7, T8, TR>";
        public Array<String> FieldNames => Intrinsics.MakeArray<String>();
        public Array<Dynamic> FieldValues => Intrinsics.MakeArray<Dynamic>();
        // Unimplemented concept functions
    }
    public readonly partial struct Function10<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, TR>
    {
        public static Function10<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, TR> Default = new Function10<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, TR>();
        public static Function10<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, TR> New() => new Function10<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, TR>();
        public override bool Equals(object obj) => true;public override int GetHashCode() => Intrinsics.CombineHashCodes();
        public override string ToString() => Intrinsics.MakeString(TypeName, FieldNames, FieldValues);
        public static implicit operator Dynamic(Function10<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, TR> self) => new Dynamic(self);
        public static implicit operator Function10<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, TR>(Dynamic value) => value.As<Function10<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, TR>>();
        public String TypeName => "Function10<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, TR>";
        public Array<String> FieldNames => Intrinsics.MakeArray<String>();
        public Array<Dynamic> FieldValues => Intrinsics.MakeArray<Dynamic>();
        // Unimplemented concept functions
    }
    public readonly partial struct Count: WholeNumber<Count>
    {
        public readonly Integer Value;
        public Count WithValue(Integer value) => (value);
        public Count(Integer value) => (Value) = (value);
        public static Count Default = new Count();
        public static Count New(Integer value) => new Count(value);
        public Ara3D.DoublePrecision.Count ChangePrecision() => (Value.ChangePrecision());
        public static implicit operator Ara3D.DoublePrecision.Count(Count self) => self.ChangePrecision();
        public static implicit operator Integer(Count self) => self.Value;
        public static implicit operator Count(Integer value) => new Count(value);
        public override bool Equals(object obj) { if (!(obj is Count)) return false; var other = (Count)obj; return Value.Equals(other.Value); }
        public override int GetHashCode() => Intrinsics.CombineHashCodes(Value);
        public override string ToString() => Intrinsics.MakeString(TypeName, FieldNames, FieldValues);
        public static implicit operator Dynamic(Count self) => new Dynamic(self);
        public static implicit operator Count(Dynamic value) => value.As<Count>();
        public String TypeName => "Count";
        public Array<String> FieldNames => Intrinsics.MakeArray<String>((String)"Value");
        public Array<Dynamic> FieldValues => Intrinsics.MakeArray<Dynamic>(new Dynamic(Value));
        Integer WholeNumber<Count>.Value => Value;
        // Unimplemented concept functions
        public Count Negative => (Value.Negative);
        public static Count operator -(Count self) => self.Negative;
        public Count Multiply(Count other) => (Value.Multiply(other.Value));
        public static Count operator *(Count self, Count other) => self.Multiply(other);
        public Count Divide(Count other) => (Value.Divide(other.Value));
        public static Count operator /(Count self, Count other) => self.Divide(other);
        public Count Modulo(Count other) => (Value.Modulo(other.Value));
        public static Count operator %(Count self, Count other) => self.Modulo(other);
        public Count Add(Count other) => (Value.Add(other.Value));
        public static Count operator +(Count self, Count other) => self.Add(other);
        public Count Subtract(Count other) => (Value.Subtract(other.Value));
        public static Count operator -(Count self, Count other) => self.Subtract(other);
        public Integer Compare(Count y) => (Value.Compare(y.Value));
        public Count Zero => (Value.Zero);
        public Count One => (Value.One);
        public Count MinValue => (Value.MinValue);
        public Count MaxValue => (Value.MaxValue);
        public Count Lerp(Count b, Number amount) => (Value.Lerp(b.Value, amount));
    }
    public readonly partial struct Cardinal: WholeNumber<Cardinal>
    {
        public readonly Integer Value;
        public Cardinal WithValue(Integer value) => (value);
        public Cardinal(Integer value) => (Value) = (value);
        public static Cardinal Default = new Cardinal();
        public static Cardinal New(Integer value) => new Cardinal(value);
        public Ara3D.DoublePrecision.Cardinal ChangePrecision() => (Value.ChangePrecision());
        public static implicit operator Ara3D.DoublePrecision.Cardinal(Cardinal self) => self.ChangePrecision();
        public static implicit operator Integer(Cardinal self) => self.Value;
        public static implicit operator Cardinal(Integer value) => new Cardinal(value);
        public override bool Equals(object obj) { if (!(obj is Cardinal)) return false; var other = (Cardinal)obj; return Value.Equals(other.Value); }
        public override int GetHashCode() => Intrinsics.CombineHashCodes(Value);
        public override string ToString() => Intrinsics.MakeString(TypeName, FieldNames, FieldValues);
        public static implicit operator Dynamic(Cardinal self) => new Dynamic(self);
        public static implicit operator Cardinal(Dynamic value) => value.As<Cardinal>();
        public String TypeName => "Cardinal";
        public Array<String> FieldNames => Intrinsics.MakeArray<String>((String)"Value");
        public Array<Dynamic> FieldValues => Intrinsics.MakeArray<Dynamic>(new Dynamic(Value));
        Integer WholeNumber<Cardinal>.Value => Value;
        // Unimplemented concept functions
        public Cardinal Negative => (Value.Negative);
        public static Cardinal operator -(Cardinal self) => self.Negative;
        public Cardinal Multiply(Cardinal other) => (Value.Multiply(other.Value));
        public static Cardinal operator *(Cardinal self, Cardinal other) => self.Multiply(other);
        public Cardinal Divide(Cardinal other) => (Value.Divide(other.Value));
        public static Cardinal operator /(Cardinal self, Cardinal other) => self.Divide(other);
        public Cardinal Modulo(Cardinal other) => (Value.Modulo(other.Value));
        public static Cardinal operator %(Cardinal self, Cardinal other) => self.Modulo(other);
        public Cardinal Add(Cardinal other) => (Value.Add(other.Value));
        public static Cardinal operator +(Cardinal self, Cardinal other) => self.Add(other);
        public Cardinal Subtract(Cardinal other) => (Value.Subtract(other.Value));
        public static Cardinal operator -(Cardinal self, Cardinal other) => self.Subtract(other);
        public Integer Compare(Cardinal y) => (Value.Compare(y.Value));
        public Cardinal Zero => (Value.Zero);
        public Cardinal One => (Value.One);
        public Cardinal MinValue => (Value.MinValue);
        public Cardinal MaxValue => (Value.MaxValue);
        public Cardinal Lerp(Cardinal b, Number amount) => (Value.Lerp(b.Value, amount));
    }
    public readonly partial struct Index: WholeNumber<Index>
    {
        public readonly Integer Value;
        public Index WithValue(Integer value) => (value);
        public Index(Integer value) => (Value) = (value);
        public static Index Default = new Index();
        public static Index New(Integer value) => new Index(value);
        public Ara3D.DoublePrecision.Index ChangePrecision() => (Value.ChangePrecision());
        public static implicit operator Ara3D.DoublePrecision.Index(Index self) => self.ChangePrecision();
        public static implicit operator Integer(Index self) => self.Value;
        public static implicit operator Index(Integer value) => new Index(value);
        public override bool Equals(object obj) { if (!(obj is Index)) return false; var other = (Index)obj; return Value.Equals(other.Value); }
        public override int GetHashCode() => Intrinsics.CombineHashCodes(Value);
        public override string ToString() => Intrinsics.MakeString(TypeName, FieldNames, FieldValues);
        public static implicit operator Dynamic(Index self) => new Dynamic(self);
        public static implicit operator Index(Dynamic value) => value.As<Index>();
        public String TypeName => "Index";
        public Array<String> FieldNames => Intrinsics.MakeArray<String>((String)"Value");
        public Array<Dynamic> FieldValues => Intrinsics.MakeArray<Dynamic>(new Dynamic(Value));
        Integer WholeNumber<Index>.Value => Value;
        // Unimplemented concept functions
        public Index Negative => (Value.Negative);
        public static Index operator -(Index self) => self.Negative;
        public Index Multiply(Index other) => (Value.Multiply(other.Value));
        public static Index operator *(Index self, Index other) => self.Multiply(other);
        public Index Divide(Index other) => (Value.Divide(other.Value));
        public static Index operator /(Index self, Index other) => self.Divide(other);
        public Index Modulo(Index other) => (Value.Modulo(other.Value));
        public static Index operator %(Index self, Index other) => self.Modulo(other);
        public Index Add(Index other) => (Value.Add(other.Value));
        public static Index operator +(Index self, Index other) => self.Add(other);
        public Index Subtract(Index other) => (Value.Subtract(other.Value));
        public static Index operator -(Index self, Index other) => self.Subtract(other);
        public Integer Compare(Index y) => (Value.Compare(y.Value));
        public Index Zero => (Value.Zero);
        public Index One => (Value.One);
        public Index MinValue => (Value.MinValue);
        public Index MaxValue => (Value.MaxValue);
        public Index Lerp(Index b, Number amount) => (Value.Lerp(b.Value, amount));
    }
    public readonly partial struct Unit: Real<Unit>, Numerical<Unit>
    {
        public readonly Number Value;
        public Unit WithValue(Number value) => (value);
        public Unit(Number value) => (Value) = (value);
        public static Unit Default = new Unit();
        public static Unit New(Number value) => new Unit(value);
        public Ara3D.DoublePrecision.Unit ChangePrecision() => (Value.ChangePrecision());
        public static implicit operator Ara3D.DoublePrecision.Unit(Unit self) => self.ChangePrecision();
        public static implicit operator Number(Unit self) => self.Value;
        public static implicit operator Unit(Number value) => new Unit(value);
        public override bool Equals(object obj) { if (!(obj is Unit)) return false; var other = (Unit)obj; return Value.Equals(other.Value); }
        public override int GetHashCode() => Intrinsics.CombineHashCodes(Value);
        public override string ToString() => Intrinsics.MakeString(TypeName, FieldNames, FieldValues);
        public static implicit operator Dynamic(Unit self) => new Dynamic(self);
        public static implicit operator Unit(Dynamic value) => value.As<Unit>();
        public String TypeName => "Unit";
        public Array<String> FieldNames => Intrinsics.MakeArray<String>((String)"Value");
        public Array<Dynamic> FieldValues => Intrinsics.MakeArray<Dynamic>(new Dynamic(Value));
        Number Real<Unit>.Value => Value;
        // Unimplemented concept functions
        public Number Subtract(Unit other) => (Value.Subtract(other.Value));
        public static Number operator -(Unit self, Unit other) => self.Subtract(other);
        public Unit Add(Number other) => (Value.Add(other));
        public static Unit operator +(Unit self, Number other) => self.Add(other);
        public Unit Subtract(Number other) => (Value.Subtract(other));
        public static Unit operator -(Unit self, Number other) => self.Subtract(other);
        public Unit Multiply(Number other) => (Value.Multiply(other));
        public static Unit operator *(Unit self, Number other) => self.Multiply(other);
        public Unit Divide(Number other) => (Value.Divide(other));
        public static Unit operator /(Unit self, Number other) => self.Divide(other);
        public Unit Modulo(Number other) => (Value.Modulo(other));
        public static Unit operator %(Unit self, Number other) => self.Modulo(other);
        public Unit Reciprocal => (Value.Reciprocal);
        public Unit Negative => (Value.Negative);
        public static Unit operator -(Unit self) => self.Negative;
        public Integer Compare(Unit y) => (Value.Compare(y.Value));
        public Unit Zero => (Value.Zero);
        public Unit One => (Value.One);
        public Unit MinValue => (Value.MinValue);
        public Unit MaxValue => (Value.MaxValue);
    }
    public readonly partial struct Probability: Real<Probability>, Numerical<Probability>
    {
        public readonly Number Value;
        public Probability WithValue(Number value) => (value);
        public Probability(Number value) => (Value) = (value);
        public static Probability Default = new Probability();
        public static Probability New(Number value) => new Probability(value);
        public Ara3D.DoublePrecision.Probability ChangePrecision() => (Value.ChangePrecision());
        public static implicit operator Ara3D.DoublePrecision.Probability(Probability self) => self.ChangePrecision();
        public static implicit operator Number(Probability self) => self.Value;
        public static implicit operator Probability(Number value) => new Probability(value);
        public override bool Equals(object obj) { if (!(obj is Probability)) return false; var other = (Probability)obj; return Value.Equals(other.Value); }
        public override int GetHashCode() => Intrinsics.CombineHashCodes(Value);
        public override string ToString() => Intrinsics.MakeString(TypeName, FieldNames, FieldValues);
        public static implicit operator Dynamic(Probability self) => new Dynamic(self);
        public static implicit operator Probability(Dynamic value) => value.As<Probability>();
        public String TypeName => "Probability";
        public Array<String> FieldNames => Intrinsics.MakeArray<String>((String)"Value");
        public Array<Dynamic> FieldValues => Intrinsics.MakeArray<Dynamic>(new Dynamic(Value));
        Number Real<Probability>.Value => Value;
        // Unimplemented concept functions
        public Number Subtract(Probability other) => (Value.Subtract(other.Value));
        public static Number operator -(Probability self, Probability other) => self.Subtract(other);
        public Probability Add(Number other) => (Value.Add(other));
        public static Probability operator +(Probability self, Number other) => self.Add(other);
        public Probability Subtract(Number other) => (Value.Subtract(other));
        public static Probability operator -(Probability self, Number other) => self.Subtract(other);
        public Probability Multiply(Number other) => (Value.Multiply(other));
        public static Probability operator *(Probability self, Number other) => self.Multiply(other);
        public Probability Divide(Number other) => (Value.Divide(other));
        public static Probability operator /(Probability self, Number other) => self.Divide(other);
        public Probability Modulo(Number other) => (Value.Modulo(other));
        public static Probability operator %(Probability self, Number other) => self.Modulo(other);
        public Probability Reciprocal => (Value.Reciprocal);
        public Probability Negative => (Value.Negative);
        public static Probability operator -(Probability self) => self.Negative;
        public Integer Compare(Probability y) => (Value.Compare(y.Value));
        public Probability Zero => (Value.Zero);
        public Probability One => (Value.One);
        public Probability MinValue => (Value.MinValue);
        public Probability MaxValue => (Value.MaxValue);
    }
    public readonly partial struct Complex: Vector<Complex>
    {
        public readonly Number Real;
        public readonly Number Imaginary;
        public Complex WithReal(Number real) => (real, Imaginary);
        public Complex WithImaginary(Number imaginary) => (Real, imaginary);
        public Complex(Number real, Number imaginary) => (Real, Imaginary) = (real, imaginary);
        public static Complex Default = new Complex();
        public static Complex New(Number real, Number imaginary) => new Complex(real, imaginary);
        public Ara3D.DoublePrecision.Complex ChangePrecision() => (Real.ChangePrecision(), Imaginary.ChangePrecision());
        public static implicit operator Ara3D.DoublePrecision.Complex(Complex self) => self.ChangePrecision();
        public static implicit operator (Number, Number)(Complex self) => (self.Real, self.Imaginary);
        public static implicit operator Complex((Number, Number) value) => new Complex(value.Item1, value.Item2);
        public void Deconstruct(out Number real, out Number imaginary) { real = Real; imaginary = Imaginary; }
        public override bool Equals(object obj) { if (!(obj is Complex)) return false; var other = (Complex)obj; return Real.Equals(other.Real) && Imaginary.Equals(other.Imaginary); }
        public override int GetHashCode() => Intrinsics.CombineHashCodes(Real, Imaginary);
        public override string ToString() => Intrinsics.MakeString(TypeName, FieldNames, FieldValues);
        public static implicit operator Dynamic(Complex self) => new Dynamic(self);
        public static implicit operator Complex(Dynamic value) => value.As<Complex>();
        public String TypeName => "Complex";
        public Array<String> FieldNames => Intrinsics.MakeArray<String>((String)"Real", (String)"Imaginary");
        public Array<Dynamic> FieldValues => Intrinsics.MakeArray<Dynamic>(new Dynamic(Real), new Dynamic(Imaginary));
        // Unimplemented concept functions
        public Complex Add(Number other) => (Real.Add(other), Imaginary.Add(other));
        public static Complex operator +(Complex self, Number other) => self.Add(other);
        public Complex Subtract(Number other) => (Real.Subtract(other), Imaginary.Subtract(other));
        public static Complex operator -(Complex self, Number other) => self.Subtract(other);
        public Complex Multiply(Number other) => (Real.Multiply(other), Imaginary.Multiply(other));
        public static Complex operator *(Complex self, Number other) => self.Multiply(other);
        public Complex Divide(Number other) => (Real.Divide(other), Imaginary.Divide(other));
        public static Complex operator /(Complex self, Number other) => self.Divide(other);
        public Complex Modulo(Number other) => (Real.Modulo(other), Imaginary.Modulo(other));
        public static Complex operator %(Complex self, Number other) => self.Modulo(other);
        public Complex Reciprocal => (Real.Reciprocal, Imaginary.Reciprocal);
        public Complex Negative => (Real.Negative, Imaginary.Negative);
        public static Complex operator -(Complex self) => self.Negative;
        public Complex Multiply(Complex other) => (Real.Multiply(other.Real), Imaginary.Multiply(other.Imaginary));
        public static Complex operator *(Complex self, Complex other) => self.Multiply(other);
        public Complex Divide(Complex other) => (Real.Divide(other.Real), Imaginary.Divide(other.Imaginary));
        public static Complex operator /(Complex self, Complex other) => self.Divide(other);
        public Complex Modulo(Complex other) => (Real.Modulo(other.Real), Imaginary.Modulo(other.Imaginary));
        public static Complex operator %(Complex self, Complex other) => self.Modulo(other);
        public Complex Add(Complex other) => (Real.Add(other.Real), Imaginary.Add(other.Imaginary));
        public static Complex operator +(Complex self, Complex other) => self.Add(other);
        public Complex Subtract(Complex other) => (Real.Subtract(other.Real), Imaginary.Subtract(other.Imaginary));
        public static Complex operator -(Complex self, Complex other) => self.Subtract(other);
        public Complex Zero => (Real.Zero, Imaginary.Zero);
        public Complex One => (Real.One, Imaginary.One);
        public Complex MinValue => (Real.MinValue, Imaginary.MinValue);
        public Complex MaxValue => (Real.MaxValue, Imaginary.MaxValue);
        public Boolean Equals(Complex b) => (Real.Equals(b.Real) & Imaginary.Equals(b.Imaginary));
        public static Boolean operator ==(Complex a, Complex b) => a.Equals(b);
        public Boolean NotEquals(Complex b) => (Real.NotEquals(b.Real) & Imaginary.NotEquals(b.Imaginary));
        public static Boolean operator !=(Complex a, Complex b) => a.NotEquals(b);
        public Boolean Between(Complex a, Complex b) => (Real.Between(a.Real, b.Real) & Imaginary.Between(a.Imaginary, b.Imaginary));
        public Complex Clamp(Complex a, Complex b) => (Real.Clamp(a.Real, b.Real), Imaginary.Clamp(a.Imaginary, b.Imaginary));
    }
    public readonly partial struct Integer2: Array<Integer>
    {
        public readonly Integer A;
        public readonly Integer B;
        public Integer2 WithA(Integer a) => (a, B);
        public Integer2 WithB(Integer b) => (A, b);
        public Integer2(Integer a, Integer b) => (A, B) = (a, b);
        public static Integer2 Default = new Integer2();
        public static Integer2 New(Integer a, Integer b) => new Integer2(a, b);
        public Ara3D.DoublePrecision.Integer2 ChangePrecision() => (A.ChangePrecision(), B.ChangePrecision());
        public static implicit operator Ara3D.DoublePrecision.Integer2(Integer2 self) => self.ChangePrecision();
        public static implicit operator (Integer, Integer)(Integer2 self) => (self.A, self.B);
        public static implicit operator Integer2((Integer, Integer) value) => new Integer2(value.Item1, value.Item2);
        public void Deconstruct(out Integer a, out Integer b) { a = A; b = B; }
        public override bool Equals(object obj) { if (!(obj is Integer2)) return false; var other = (Integer2)obj; return A.Equals(other.A) && B.Equals(other.B); }
        public override int GetHashCode() => Intrinsics.CombineHashCodes(A, B);
        public override string ToString() => Intrinsics.MakeString(TypeName, FieldNames, FieldValues);
        public static implicit operator Dynamic(Integer2 self) => new Dynamic(self);
        public static implicit operator Integer2(Dynamic value) => value.As<Integer2>();
        public String TypeName => "Integer2";
        public Array<String> FieldNames => Intrinsics.MakeArray<String>((String)"A", (String)"B");
        public Array<Dynamic> FieldValues => Intrinsics.MakeArray<Dynamic>(new Dynamic(A), new Dynamic(B));
        // Unimplemented concept functions
        public Integer Count => 2;
        public Integer At(Integer n) => n == 0 ? A : n == 1 ? B : throw new System.IndexOutOfRangeException();
        public Integer this[Integer n] => At(n);
    }
    public readonly partial struct Integer3: Array<Integer>
    {
        public readonly Integer A;
        public readonly Integer B;
        public readonly Integer C;
        public Integer3 WithA(Integer a) => (a, B, C);
        public Integer3 WithB(Integer b) => (A, b, C);
        public Integer3 WithC(Integer c) => (A, B, c);
        public Integer3(Integer a, Integer b, Integer c) => (A, B, C) = (a, b, c);
        public static Integer3 Default = new Integer3();
        public static Integer3 New(Integer a, Integer b, Integer c) => new Integer3(a, b, c);
        public Ara3D.DoublePrecision.Integer3 ChangePrecision() => (A.ChangePrecision(), B.ChangePrecision(), C.ChangePrecision());
        public static implicit operator Ara3D.DoublePrecision.Integer3(Integer3 self) => self.ChangePrecision();
        public static implicit operator (Integer, Integer, Integer)(Integer3 self) => (self.A, self.B, self.C);
        public static implicit operator Integer3((Integer, Integer, Integer) value) => new Integer3(value.Item1, value.Item2, value.Item3);
        public void Deconstruct(out Integer a, out Integer b, out Integer c) { a = A; b = B; c = C; }
        public override bool Equals(object obj) { if (!(obj is Integer3)) return false; var other = (Integer3)obj; return A.Equals(other.A) && B.Equals(other.B) && C.Equals(other.C); }
        public override int GetHashCode() => Intrinsics.CombineHashCodes(A, B, C);
        public override string ToString() => Intrinsics.MakeString(TypeName, FieldNames, FieldValues);
        public static implicit operator Dynamic(Integer3 self) => new Dynamic(self);
        public static implicit operator Integer3(Dynamic value) => value.As<Integer3>();
        public String TypeName => "Integer3";
        public Array<String> FieldNames => Intrinsics.MakeArray<String>((String)"A", (String)"B", (String)"C");
        public Array<Dynamic> FieldValues => Intrinsics.MakeArray<Dynamic>(new Dynamic(A), new Dynamic(B), new Dynamic(C));
        // Unimplemented concept functions
        public Integer Count => 3;
        public Integer At(Integer n) => n == 0 ? A : n == 1 ? B : n == 2 ? C : throw new System.IndexOutOfRangeException();
        public Integer this[Integer n] => At(n);
    }
    public readonly partial struct Integer4: Array<Integer>
    {
        public readonly Integer A;
        public readonly Integer B;
        public readonly Integer C;
        public readonly Integer D;
        public Integer4 WithA(Integer a) => (a, B, C, D);
        public Integer4 WithB(Integer b) => (A, b, C, D);
        public Integer4 WithC(Integer c) => (A, B, c, D);
        public Integer4 WithD(Integer d) => (A, B, C, d);
        public Integer4(Integer a, Integer b, Integer c, Integer d) => (A, B, C, D) = (a, b, c, d);
        public static Integer4 Default = new Integer4();
        public static Integer4 New(Integer a, Integer b, Integer c, Integer d) => new Integer4(a, b, c, d);
        public Ara3D.DoublePrecision.Integer4 ChangePrecision() => (A.ChangePrecision(), B.ChangePrecision(), C.ChangePrecision(), D.ChangePrecision());
        public static implicit operator Ara3D.DoublePrecision.Integer4(Integer4 self) => self.ChangePrecision();
        public static implicit operator (Integer, Integer, Integer, Integer)(Integer4 self) => (self.A, self.B, self.C, self.D);
        public static implicit operator Integer4((Integer, Integer, Integer, Integer) value) => new Integer4(value.Item1, value.Item2, value.Item3, value.Item4);
        public void Deconstruct(out Integer a, out Integer b, out Integer c, out Integer d) { a = A; b = B; c = C; d = D; }
        public override bool Equals(object obj) { if (!(obj is Integer4)) return false; var other = (Integer4)obj; return A.Equals(other.A) && B.Equals(other.B) && C.Equals(other.C) && D.Equals(other.D); }
        public override int GetHashCode() => Intrinsics.CombineHashCodes(A, B, C, D);
        public override string ToString() => Intrinsics.MakeString(TypeName, FieldNames, FieldValues);
        public static implicit operator Dynamic(Integer4 self) => new Dynamic(self);
        public static implicit operator Integer4(Dynamic value) => value.As<Integer4>();
        public String TypeName => "Integer4";
        public Array<String> FieldNames => Intrinsics.MakeArray<String>((String)"A", (String)"B", (String)"C", (String)"D");
        public Array<Dynamic> FieldValues => Intrinsics.MakeArray<Dynamic>(new Dynamic(A), new Dynamic(B), new Dynamic(C), new Dynamic(D));
        // Unimplemented concept functions
        public Integer Count => 4;
        public Integer At(Integer n) => n == 0 ? A : n == 1 ? B : n == 2 ? C : n == 3 ? D : throw new System.IndexOutOfRangeException();
        public Integer this[Integer n] => At(n);
    }
    public readonly partial struct Color: Coordinate<Color>
    {
        public readonly Unit R;
        public readonly Unit G;
        public readonly Unit B;
        public readonly Unit A;
        public Color WithR(Unit r) => (r, G, B, A);
        public Color WithG(Unit g) => (R, g, B, A);
        public Color WithB(Unit b) => (R, G, b, A);
        public Color WithA(Unit a) => (R, G, B, a);
        public Color(Unit r, Unit g, Unit b, Unit a) => (R, G, B, A) = (r, g, b, a);
        public static Color Default = new Color();
        public static Color New(Unit r, Unit g, Unit b, Unit a) => new Color(r, g, b, a);
        public Ara3D.DoublePrecision.Color ChangePrecision() => (R.ChangePrecision(), G.ChangePrecision(), B.ChangePrecision(), A.ChangePrecision());
        public static implicit operator Ara3D.DoublePrecision.Color(Color self) => self.ChangePrecision();
        public static implicit operator (Unit, Unit, Unit, Unit)(Color self) => (self.R, self.G, self.B, self.A);
        public static implicit operator Color((Unit, Unit, Unit, Unit) value) => new Color(value.Item1, value.Item2, value.Item3, value.Item4);
        public void Deconstruct(out Unit r, out Unit g, out Unit b, out Unit a) { r = R; g = G; b = B; a = A; }
        public override bool Equals(object obj) { if (!(obj is Color)) return false; var other = (Color)obj; return R.Equals(other.R) && G.Equals(other.G) && B.Equals(other.B) && A.Equals(other.A); }
        public override int GetHashCode() => Intrinsics.CombineHashCodes(R, G, B, A);
        public override string ToString() => Intrinsics.MakeString(TypeName, FieldNames, FieldValues);
        public static implicit operator Dynamic(Color self) => new Dynamic(self);
        public static implicit operator Color(Dynamic value) => value.As<Color>();
        public String TypeName => "Color";
        public Array<String> FieldNames => Intrinsics.MakeArray<String>((String)"R", (String)"G", (String)"B", (String)"A");
        public Array<Dynamic> FieldValues => Intrinsics.MakeArray<Dynamic>(new Dynamic(R), new Dynamic(G), new Dynamic(B), new Dynamic(A));
        // Unimplemented concept functions
        public Boolean Between(Color a, Color b) => (R.Between(a.R, b.R) & G.Between(a.G, b.G) & B.Between(a.B, b.B) & A.Between(a.A, b.A));
        public Color Clamp(Color a, Color b) => (R.Clamp(a.R, b.R), G.Clamp(a.G, b.G), B.Clamp(a.B, b.B), A.Clamp(a.A, b.A));
        public Color Lerp(Color b, Number amount) => (R.Lerp(b.R, amount), G.Lerp(b.G, amount), B.Lerp(b.B, amount), A.Lerp(b.A, amount));
        public Boolean Equals(Color b) => (R.Equals(b.R) & G.Equals(b.G) & B.Equals(b.B) & A.Equals(b.A));
        public static Boolean operator ==(Color a, Color b) => a.Equals(b);
        public Boolean NotEquals(Color b) => (R.NotEquals(b.R) & G.NotEquals(b.G) & B.NotEquals(b.B) & A.NotEquals(b.A));
        public static Boolean operator !=(Color a, Color b) => a.NotEquals(b);
    }
    public readonly partial struct ColorLUV: Coordinate<ColorLUV>
    {
        public readonly Unit Lightness;
        public readonly Unit U;
        public readonly Unit V;
        public ColorLUV WithLightness(Unit lightness) => (lightness, U, V);
        public ColorLUV WithU(Unit u) => (Lightness, u, V);
        public ColorLUV WithV(Unit v) => (Lightness, U, v);
        public ColorLUV(Unit lightness, Unit u, Unit v) => (Lightness, U, V) = (lightness, u, v);
        public static ColorLUV Default = new ColorLUV();
        public static ColorLUV New(Unit lightness, Unit u, Unit v) => new ColorLUV(lightness, u, v);
        public Ara3D.DoublePrecision.ColorLUV ChangePrecision() => (Lightness.ChangePrecision(), U.ChangePrecision(), V.ChangePrecision());
        public static implicit operator Ara3D.DoublePrecision.ColorLUV(ColorLUV self) => self.ChangePrecision();
        public static implicit operator (Unit, Unit, Unit)(ColorLUV self) => (self.Lightness, self.U, self.V);
        public static implicit operator ColorLUV((Unit, Unit, Unit) value) => new ColorLUV(value.Item1, value.Item2, value.Item3);
        public void Deconstruct(out Unit lightness, out Unit u, out Unit v) { lightness = Lightness; u = U; v = V; }
        public override bool Equals(object obj) { if (!(obj is ColorLUV)) return false; var other = (ColorLUV)obj; return Lightness.Equals(other.Lightness) && U.Equals(other.U) && V.Equals(other.V); }
        public override int GetHashCode() => Intrinsics.CombineHashCodes(Lightness, U, V);
        public override string ToString() => Intrinsics.MakeString(TypeName, FieldNames, FieldValues);
        public static implicit operator Dynamic(ColorLUV self) => new Dynamic(self);
        public static implicit operator ColorLUV(Dynamic value) => value.As<ColorLUV>();
        public String TypeName => "ColorLUV";
        public Array<String> FieldNames => Intrinsics.MakeArray<String>((String)"Lightness", (String)"U", (String)"V");
        public Array<Dynamic> FieldValues => Intrinsics.MakeArray<Dynamic>(new Dynamic(Lightness), new Dynamic(U), new Dynamic(V));
        // Unimplemented concept functions
        public Boolean Between(ColorLUV a, ColorLUV b) => (Lightness.Between(a.Lightness, b.Lightness) & U.Between(a.U, b.U) & V.Between(a.V, b.V));
        public ColorLUV Clamp(ColorLUV a, ColorLUV b) => (Lightness.Clamp(a.Lightness, b.Lightness), U.Clamp(a.U, b.U), V.Clamp(a.V, b.V));
        public ColorLUV Lerp(ColorLUV b, Number amount) => (Lightness.Lerp(b.Lightness, amount), U.Lerp(b.U, amount), V.Lerp(b.V, amount));
        public Boolean Equals(ColorLUV b) => (Lightness.Equals(b.Lightness) & U.Equals(b.U) & V.Equals(b.V));
        public static Boolean operator ==(ColorLUV a, ColorLUV b) => a.Equals(b);
        public Boolean NotEquals(ColorLUV b) => (Lightness.NotEquals(b.Lightness) & U.NotEquals(b.U) & V.NotEquals(b.V));
        public static Boolean operator !=(ColorLUV a, ColorLUV b) => a.NotEquals(b);
    }
    public readonly partial struct ColorLAB: Coordinate<ColorLAB>
    {
        public readonly Unit Lightness;
        public readonly Number A;
        public readonly Number B;
        public ColorLAB WithLightness(Unit lightness) => (lightness, A, B);
        public ColorLAB WithA(Number a) => (Lightness, a, B);
        public ColorLAB WithB(Number b) => (Lightness, A, b);
        public ColorLAB(Unit lightness, Number a, Number b) => (Lightness, A, B) = (lightness, a, b);
        public static ColorLAB Default = new ColorLAB();
        public static ColorLAB New(Unit lightness, Number a, Number b) => new ColorLAB(lightness, a, b);
        public Ara3D.DoublePrecision.ColorLAB ChangePrecision() => (Lightness.ChangePrecision(), A.ChangePrecision(), B.ChangePrecision());
        public static implicit operator Ara3D.DoublePrecision.ColorLAB(ColorLAB self) => self.ChangePrecision();
        public static implicit operator (Unit, Number, Number)(ColorLAB self) => (self.Lightness, self.A, self.B);
        public static implicit operator ColorLAB((Unit, Number, Number) value) => new ColorLAB(value.Item1, value.Item2, value.Item3);
        public void Deconstruct(out Unit lightness, out Number a, out Number b) { lightness = Lightness; a = A; b = B; }
        public override bool Equals(object obj) { if (!(obj is ColorLAB)) return false; var other = (ColorLAB)obj; return Lightness.Equals(other.Lightness) && A.Equals(other.A) && B.Equals(other.B); }
        public override int GetHashCode() => Intrinsics.CombineHashCodes(Lightness, A, B);
        public override string ToString() => Intrinsics.MakeString(TypeName, FieldNames, FieldValues);
        public static implicit operator Dynamic(ColorLAB self) => new Dynamic(self);
        public static implicit operator ColorLAB(Dynamic value) => value.As<ColorLAB>();
        public String TypeName => "ColorLAB";
        public Array<String> FieldNames => Intrinsics.MakeArray<String>((String)"Lightness", (String)"A", (String)"B");
        public Array<Dynamic> FieldValues => Intrinsics.MakeArray<Dynamic>(new Dynamic(Lightness), new Dynamic(A), new Dynamic(B));
        // Unimplemented concept functions
        public Boolean Between(ColorLAB a, ColorLAB b) => (Lightness.Between(a.Lightness, b.Lightness) & A.Between(a.A, b.A) & B.Between(a.B, b.B));
        public ColorLAB Clamp(ColorLAB a, ColorLAB b) => (Lightness.Clamp(a.Lightness, b.Lightness), A.Clamp(a.A, b.A), B.Clamp(a.B, b.B));
        public ColorLAB Lerp(ColorLAB b, Number amount) => (Lightness.Lerp(b.Lightness, amount), A.Lerp(b.A, amount), B.Lerp(b.B, amount));
        public Boolean Equals(ColorLAB b) => (Lightness.Equals(b.Lightness) & A.Equals(b.A) & B.Equals(b.B));
        public static Boolean operator ==(ColorLAB a, ColorLAB b) => a.Equals(b);
        public Boolean NotEquals(ColorLAB b) => (Lightness.NotEquals(b.Lightness) & A.NotEquals(b.A) & B.NotEquals(b.B));
        public static Boolean operator !=(ColorLAB a, ColorLAB b) => a.NotEquals(b);
    }
    public readonly partial struct ColorLCh: Coordinate<ColorLCh>
    {
        public readonly Unit Lightness;
        public readonly PolarCoordinate ChromaHue;
        public ColorLCh WithLightness(Unit lightness) => (lightness, ChromaHue);
        public ColorLCh WithChromaHue(PolarCoordinate chromaHue) => (Lightness, chromaHue);
        public ColorLCh(Unit lightness, PolarCoordinate chromaHue) => (Lightness, ChromaHue) = (lightness, chromaHue);
        public static ColorLCh Default = new ColorLCh();
        public static ColorLCh New(Unit lightness, PolarCoordinate chromaHue) => new ColorLCh(lightness, chromaHue);
        public Ara3D.DoublePrecision.ColorLCh ChangePrecision() => (Lightness.ChangePrecision(), ChromaHue.ChangePrecision());
        public static implicit operator Ara3D.DoublePrecision.ColorLCh(ColorLCh self) => self.ChangePrecision();
        public static implicit operator (Unit, PolarCoordinate)(ColorLCh self) => (self.Lightness, self.ChromaHue);
        public static implicit operator ColorLCh((Unit, PolarCoordinate) value) => new ColorLCh(value.Item1, value.Item2);
        public void Deconstruct(out Unit lightness, out PolarCoordinate chromaHue) { lightness = Lightness; chromaHue = ChromaHue; }
        public override bool Equals(object obj) { if (!(obj is ColorLCh)) return false; var other = (ColorLCh)obj; return Lightness.Equals(other.Lightness) && ChromaHue.Equals(other.ChromaHue); }
        public override int GetHashCode() => Intrinsics.CombineHashCodes(Lightness, ChromaHue);
        public override string ToString() => Intrinsics.MakeString(TypeName, FieldNames, FieldValues);
        public static implicit operator Dynamic(ColorLCh self) => new Dynamic(self);
        public static implicit operator ColorLCh(Dynamic value) => value.As<ColorLCh>();
        public String TypeName => "ColorLCh";
        public Array<String> FieldNames => Intrinsics.MakeArray<String>((String)"Lightness", (String)"ChromaHue");
        public Array<Dynamic> FieldValues => Intrinsics.MakeArray<Dynamic>(new Dynamic(Lightness), new Dynamic(ChromaHue));
        // Unimplemented concept functions
        public Boolean Between(ColorLCh a, ColorLCh b) => (Lightness.Between(a.Lightness, b.Lightness) & ChromaHue.Between(a.ChromaHue, b.ChromaHue));
        public ColorLCh Clamp(ColorLCh a, ColorLCh b) => (Lightness.Clamp(a.Lightness, b.Lightness), ChromaHue.Clamp(a.ChromaHue, b.ChromaHue));
        public ColorLCh Lerp(ColorLCh b, Number amount) => (Lightness.Lerp(b.Lightness, amount), ChromaHue.Lerp(b.ChromaHue, amount));
        public Boolean Equals(ColorLCh b) => (Lightness.Equals(b.Lightness) & ChromaHue.Equals(b.ChromaHue));
        public static Boolean operator ==(ColorLCh a, ColorLCh b) => a.Equals(b);
        public Boolean NotEquals(ColorLCh b) => (Lightness.NotEquals(b.Lightness) & ChromaHue.NotEquals(b.ChromaHue));
        public static Boolean operator !=(ColorLCh a, ColorLCh b) => a.NotEquals(b);
    }
    public readonly partial struct ColorHSV: Coordinate<ColorHSV>
    {
        public readonly Angle Hue;
        public readonly Unit S;
        public readonly Unit V;
        public ColorHSV WithHue(Angle hue) => (hue, S, V);
        public ColorHSV WithS(Unit s) => (Hue, s, V);
        public ColorHSV WithV(Unit v) => (Hue, S, v);
        public ColorHSV(Angle hue, Unit s, Unit v) => (Hue, S, V) = (hue, s, v);
        public static ColorHSV Default = new ColorHSV();
        public static ColorHSV New(Angle hue, Unit s, Unit v) => new ColorHSV(hue, s, v);
        public Ara3D.DoublePrecision.ColorHSV ChangePrecision() => (Hue.ChangePrecision(), S.ChangePrecision(), V.ChangePrecision());
        public static implicit operator Ara3D.DoublePrecision.ColorHSV(ColorHSV self) => self.ChangePrecision();
        public static implicit operator (Angle, Unit, Unit)(ColorHSV self) => (self.Hue, self.S, self.V);
        public static implicit operator ColorHSV((Angle, Unit, Unit) value) => new ColorHSV(value.Item1, value.Item2, value.Item3);
        public void Deconstruct(out Angle hue, out Unit s, out Unit v) { hue = Hue; s = S; v = V; }
        public override bool Equals(object obj) { if (!(obj is ColorHSV)) return false; var other = (ColorHSV)obj; return Hue.Equals(other.Hue) && S.Equals(other.S) && V.Equals(other.V); }
        public override int GetHashCode() => Intrinsics.CombineHashCodes(Hue, S, V);
        public override string ToString() => Intrinsics.MakeString(TypeName, FieldNames, FieldValues);
        public static implicit operator Dynamic(ColorHSV self) => new Dynamic(self);
        public static implicit operator ColorHSV(Dynamic value) => value.As<ColorHSV>();
        public String TypeName => "ColorHSV";
        public Array<String> FieldNames => Intrinsics.MakeArray<String>((String)"Hue", (String)"S", (String)"V");
        public Array<Dynamic> FieldValues => Intrinsics.MakeArray<Dynamic>(new Dynamic(Hue), new Dynamic(S), new Dynamic(V));
        // Unimplemented concept functions
        public Boolean Between(ColorHSV a, ColorHSV b) => (Hue.Between(a.Hue, b.Hue) & S.Between(a.S, b.S) & V.Between(a.V, b.V));
        public ColorHSV Clamp(ColorHSV a, ColorHSV b) => (Hue.Clamp(a.Hue, b.Hue), S.Clamp(a.S, b.S), V.Clamp(a.V, b.V));
        public ColorHSV Lerp(ColorHSV b, Number amount) => (Hue.Lerp(b.Hue, amount), S.Lerp(b.S, amount), V.Lerp(b.V, amount));
        public Boolean Equals(ColorHSV b) => (Hue.Equals(b.Hue) & S.Equals(b.S) & V.Equals(b.V));
        public static Boolean operator ==(ColorHSV a, ColorHSV b) => a.Equals(b);
        public Boolean NotEquals(ColorHSV b) => (Hue.NotEquals(b.Hue) & S.NotEquals(b.S) & V.NotEquals(b.V));
        public static Boolean operator !=(ColorHSV a, ColorHSV b) => a.NotEquals(b);
    }
    public readonly partial struct ColorHSL: Coordinate<ColorHSL>
    {
        public readonly Angle Hue;
        public readonly Unit Saturation;
        public readonly Unit Luminance;
        public ColorHSL WithHue(Angle hue) => (hue, Saturation, Luminance);
        public ColorHSL WithSaturation(Unit saturation) => (Hue, saturation, Luminance);
        public ColorHSL WithLuminance(Unit luminance) => (Hue, Saturation, luminance);
        public ColorHSL(Angle hue, Unit saturation, Unit luminance) => (Hue, Saturation, Luminance) = (hue, saturation, luminance);
        public static ColorHSL Default = new ColorHSL();
        public static ColorHSL New(Angle hue, Unit saturation, Unit luminance) => new ColorHSL(hue, saturation, luminance);
        public Ara3D.DoublePrecision.ColorHSL ChangePrecision() => (Hue.ChangePrecision(), Saturation.ChangePrecision(), Luminance.ChangePrecision());
        public static implicit operator Ara3D.DoublePrecision.ColorHSL(ColorHSL self) => self.ChangePrecision();
        public static implicit operator (Angle, Unit, Unit)(ColorHSL self) => (self.Hue, self.Saturation, self.Luminance);
        public static implicit operator ColorHSL((Angle, Unit, Unit) value) => new ColorHSL(value.Item1, value.Item2, value.Item3);
        public void Deconstruct(out Angle hue, out Unit saturation, out Unit luminance) { hue = Hue; saturation = Saturation; luminance = Luminance; }
        public override bool Equals(object obj) { if (!(obj is ColorHSL)) return false; var other = (ColorHSL)obj; return Hue.Equals(other.Hue) && Saturation.Equals(other.Saturation) && Luminance.Equals(other.Luminance); }
        public override int GetHashCode() => Intrinsics.CombineHashCodes(Hue, Saturation, Luminance);
        public override string ToString() => Intrinsics.MakeString(TypeName, FieldNames, FieldValues);
        public static implicit operator Dynamic(ColorHSL self) => new Dynamic(self);
        public static implicit operator ColorHSL(Dynamic value) => value.As<ColorHSL>();
        public String TypeName => "ColorHSL";
        public Array<String> FieldNames => Intrinsics.MakeArray<String>((String)"Hue", (String)"Saturation", (String)"Luminance");
        public Array<Dynamic> FieldValues => Intrinsics.MakeArray<Dynamic>(new Dynamic(Hue), new Dynamic(Saturation), new Dynamic(Luminance));
        // Unimplemented concept functions
        public Boolean Between(ColorHSL a, ColorHSL b) => (Hue.Between(a.Hue, b.Hue) & Saturation.Between(a.Saturation, b.Saturation) & Luminance.Between(a.Luminance, b.Luminance));
        public ColorHSL Clamp(ColorHSL a, ColorHSL b) => (Hue.Clamp(a.Hue, b.Hue), Saturation.Clamp(a.Saturation, b.Saturation), Luminance.Clamp(a.Luminance, b.Luminance));
        public ColorHSL Lerp(ColorHSL b, Number amount) => (Hue.Lerp(b.Hue, amount), Saturation.Lerp(b.Saturation, amount), Luminance.Lerp(b.Luminance, amount));
        public Boolean Equals(ColorHSL b) => (Hue.Equals(b.Hue) & Saturation.Equals(b.Saturation) & Luminance.Equals(b.Luminance));
        public static Boolean operator ==(ColorHSL a, ColorHSL b) => a.Equals(b);
        public Boolean NotEquals(ColorHSL b) => (Hue.NotEquals(b.Hue) & Saturation.NotEquals(b.Saturation) & Luminance.NotEquals(b.Luminance));
        public static Boolean operator !=(ColorHSL a, ColorHSL b) => a.NotEquals(b);
    }
    public readonly partial struct ColorYCbCr: Coordinate<ColorYCbCr>
    {
        public readonly Unit Y;
        public readonly Unit Cb;
        public readonly Unit Cr;
        public ColorYCbCr WithY(Unit y) => (y, Cb, Cr);
        public ColorYCbCr WithCb(Unit cb) => (Y, cb, Cr);
        public ColorYCbCr WithCr(Unit cr) => (Y, Cb, cr);
        public ColorYCbCr(Unit y, Unit cb, Unit cr) => (Y, Cb, Cr) = (y, cb, cr);
        public static ColorYCbCr Default = new ColorYCbCr();
        public static ColorYCbCr New(Unit y, Unit cb, Unit cr) => new ColorYCbCr(y, cb, cr);
        public Ara3D.DoublePrecision.ColorYCbCr ChangePrecision() => (Y.ChangePrecision(), Cb.ChangePrecision(), Cr.ChangePrecision());
        public static implicit operator Ara3D.DoublePrecision.ColorYCbCr(ColorYCbCr self) => self.ChangePrecision();
        public static implicit operator (Unit, Unit, Unit)(ColorYCbCr self) => (self.Y, self.Cb, self.Cr);
        public static implicit operator ColorYCbCr((Unit, Unit, Unit) value) => new ColorYCbCr(value.Item1, value.Item2, value.Item3);
        public void Deconstruct(out Unit y, out Unit cb, out Unit cr) { y = Y; cb = Cb; cr = Cr; }
        public override bool Equals(object obj) { if (!(obj is ColorYCbCr)) return false; var other = (ColorYCbCr)obj; return Y.Equals(other.Y) && Cb.Equals(other.Cb) && Cr.Equals(other.Cr); }
        public override int GetHashCode() => Intrinsics.CombineHashCodes(Y, Cb, Cr);
        public override string ToString() => Intrinsics.MakeString(TypeName, FieldNames, FieldValues);
        public static implicit operator Dynamic(ColorYCbCr self) => new Dynamic(self);
        public static implicit operator ColorYCbCr(Dynamic value) => value.As<ColorYCbCr>();
        public String TypeName => "ColorYCbCr";
        public Array<String> FieldNames => Intrinsics.MakeArray<String>((String)"Y", (String)"Cb", (String)"Cr");
        public Array<Dynamic> FieldValues => Intrinsics.MakeArray<Dynamic>(new Dynamic(Y), new Dynamic(Cb), new Dynamic(Cr));
        // Unimplemented concept functions
        public Boolean Between(ColorYCbCr a, ColorYCbCr b) => (Y.Between(a.Y, b.Y) & Cb.Between(a.Cb, b.Cb) & Cr.Between(a.Cr, b.Cr));
        public ColorYCbCr Clamp(ColorYCbCr a, ColorYCbCr b) => (Y.Clamp(a.Y, b.Y), Cb.Clamp(a.Cb, b.Cb), Cr.Clamp(a.Cr, b.Cr));
        public ColorYCbCr Lerp(ColorYCbCr b, Number amount) => (Y.Lerp(b.Y, amount), Cb.Lerp(b.Cb, amount), Cr.Lerp(b.Cr, amount));
        public Boolean Equals(ColorYCbCr b) => (Y.Equals(b.Y) & Cb.Equals(b.Cb) & Cr.Equals(b.Cr));
        public static Boolean operator ==(ColorYCbCr a, ColorYCbCr b) => a.Equals(b);
        public Boolean NotEquals(ColorYCbCr b) => (Y.NotEquals(b.Y) & Cb.NotEquals(b.Cb) & Cr.NotEquals(b.Cr));
        public static Boolean operator !=(ColorYCbCr a, ColorYCbCr b) => a.NotEquals(b);
    }
    public readonly partial struct SphericalCoordinate: Coordinate<SphericalCoordinate>
    {
        public readonly Number Radius;
        public readonly Angle Azimuth;
        public readonly Angle Polar;
        public SphericalCoordinate WithRadius(Number radius) => (radius, Azimuth, Polar);
        public SphericalCoordinate WithAzimuth(Angle azimuth) => (Radius, azimuth, Polar);
        public SphericalCoordinate WithPolar(Angle polar) => (Radius, Azimuth, polar);
        public SphericalCoordinate(Number radius, Angle azimuth, Angle polar) => (Radius, Azimuth, Polar) = (radius, azimuth, polar);
        public static SphericalCoordinate Default = new SphericalCoordinate();
        public static SphericalCoordinate New(Number radius, Angle azimuth, Angle polar) => new SphericalCoordinate(radius, azimuth, polar);
        public Ara3D.DoublePrecision.SphericalCoordinate ChangePrecision() => (Radius.ChangePrecision(), Azimuth.ChangePrecision(), Polar.ChangePrecision());
        public static implicit operator Ara3D.DoublePrecision.SphericalCoordinate(SphericalCoordinate self) => self.ChangePrecision();
        public static implicit operator (Number, Angle, Angle)(SphericalCoordinate self) => (self.Radius, self.Azimuth, self.Polar);
        public static implicit operator SphericalCoordinate((Number, Angle, Angle) value) => new SphericalCoordinate(value.Item1, value.Item2, value.Item3);
        public void Deconstruct(out Number radius, out Angle azimuth, out Angle polar) { radius = Radius; azimuth = Azimuth; polar = Polar; }
        public override bool Equals(object obj) { if (!(obj is SphericalCoordinate)) return false; var other = (SphericalCoordinate)obj; return Radius.Equals(other.Radius) && Azimuth.Equals(other.Azimuth) && Polar.Equals(other.Polar); }
        public override int GetHashCode() => Intrinsics.CombineHashCodes(Radius, Azimuth, Polar);
        public override string ToString() => Intrinsics.MakeString(TypeName, FieldNames, FieldValues);
        public static implicit operator Dynamic(SphericalCoordinate self) => new Dynamic(self);
        public static implicit operator SphericalCoordinate(Dynamic value) => value.As<SphericalCoordinate>();
        public String TypeName => "SphericalCoordinate";
        public Array<String> FieldNames => Intrinsics.MakeArray<String>((String)"Radius", (String)"Azimuth", (String)"Polar");
        public Array<Dynamic> FieldValues => Intrinsics.MakeArray<Dynamic>(new Dynamic(Radius), new Dynamic(Azimuth), new Dynamic(Polar));
        // Unimplemented concept functions
        public Boolean Between(SphericalCoordinate a, SphericalCoordinate b) => (Radius.Between(a.Radius, b.Radius) & Azimuth.Between(a.Azimuth, b.Azimuth) & Polar.Between(a.Polar, b.Polar));
        public SphericalCoordinate Clamp(SphericalCoordinate a, SphericalCoordinate b) => (Radius.Clamp(a.Radius, b.Radius), Azimuth.Clamp(a.Azimuth, b.Azimuth), Polar.Clamp(a.Polar, b.Polar));
        public SphericalCoordinate Lerp(SphericalCoordinate b, Number amount) => (Radius.Lerp(b.Radius, amount), Azimuth.Lerp(b.Azimuth, amount), Polar.Lerp(b.Polar, amount));
        public Boolean Equals(SphericalCoordinate b) => (Radius.Equals(b.Radius) & Azimuth.Equals(b.Azimuth) & Polar.Equals(b.Polar));
        public static Boolean operator ==(SphericalCoordinate a, SphericalCoordinate b) => a.Equals(b);
        public Boolean NotEquals(SphericalCoordinate b) => (Radius.NotEquals(b.Radius) & Azimuth.NotEquals(b.Azimuth) & Polar.NotEquals(b.Polar));
        public static Boolean operator !=(SphericalCoordinate a, SphericalCoordinate b) => a.NotEquals(b);
    }
    public readonly partial struct PolarCoordinate: Coordinate<PolarCoordinate>
    {
        public readonly Number Radius;
        public readonly Angle Angle;
        public PolarCoordinate WithRadius(Number radius) => (radius, Angle);
        public PolarCoordinate WithAngle(Angle angle) => (Radius, angle);
        public PolarCoordinate(Number radius, Angle angle) => (Radius, Angle) = (radius, angle);
        public static PolarCoordinate Default = new PolarCoordinate();
        public static PolarCoordinate New(Number radius, Angle angle) => new PolarCoordinate(radius, angle);
        public Ara3D.DoublePrecision.PolarCoordinate ChangePrecision() => (Radius.ChangePrecision(), Angle.ChangePrecision());
        public static implicit operator Ara3D.DoublePrecision.PolarCoordinate(PolarCoordinate self) => self.ChangePrecision();
        public static implicit operator (Number, Angle)(PolarCoordinate self) => (self.Radius, self.Angle);
        public static implicit operator PolarCoordinate((Number, Angle) value) => new PolarCoordinate(value.Item1, value.Item2);
        public void Deconstruct(out Number radius, out Angle angle) { radius = Radius; angle = Angle; }
        public override bool Equals(object obj) { if (!(obj is PolarCoordinate)) return false; var other = (PolarCoordinate)obj; return Radius.Equals(other.Radius) && Angle.Equals(other.Angle); }
        public override int GetHashCode() => Intrinsics.CombineHashCodes(Radius, Angle);
        public override string ToString() => Intrinsics.MakeString(TypeName, FieldNames, FieldValues);
        public static implicit operator Dynamic(PolarCoordinate self) => new Dynamic(self);
        public static implicit operator PolarCoordinate(Dynamic value) => value.As<PolarCoordinate>();
        public String TypeName => "PolarCoordinate";
        public Array<String> FieldNames => Intrinsics.MakeArray<String>((String)"Radius", (String)"Angle");
        public Array<Dynamic> FieldValues => Intrinsics.MakeArray<Dynamic>(new Dynamic(Radius), new Dynamic(Angle));
        // Unimplemented concept functions
        public Boolean Between(PolarCoordinate a, PolarCoordinate b) => (Radius.Between(a.Radius, b.Radius) & Angle.Between(a.Angle, b.Angle));
        public PolarCoordinate Clamp(PolarCoordinate a, PolarCoordinate b) => (Radius.Clamp(a.Radius, b.Radius), Angle.Clamp(a.Angle, b.Angle));
        public PolarCoordinate Lerp(PolarCoordinate b, Number amount) => (Radius.Lerp(b.Radius, amount), Angle.Lerp(b.Angle, amount));
        public Boolean Equals(PolarCoordinate b) => (Radius.Equals(b.Radius) & Angle.Equals(b.Angle));
        public static Boolean operator ==(PolarCoordinate a, PolarCoordinate b) => a.Equals(b);
        public Boolean NotEquals(PolarCoordinate b) => (Radius.NotEquals(b.Radius) & Angle.NotEquals(b.Angle));
        public static Boolean operator !=(PolarCoordinate a, PolarCoordinate b) => a.NotEquals(b);
    }
    public readonly partial struct LogPolarCoordinate: Coordinate<LogPolarCoordinate>
    {
        public readonly Number Rho;
        public readonly Angle Azimuth;
        public LogPolarCoordinate WithRho(Number rho) => (rho, Azimuth);
        public LogPolarCoordinate WithAzimuth(Angle azimuth) => (Rho, azimuth);
        public LogPolarCoordinate(Number rho, Angle azimuth) => (Rho, Azimuth) = (rho, azimuth);
        public static LogPolarCoordinate Default = new LogPolarCoordinate();
        public static LogPolarCoordinate New(Number rho, Angle azimuth) => new LogPolarCoordinate(rho, azimuth);
        public Ara3D.DoublePrecision.LogPolarCoordinate ChangePrecision() => (Rho.ChangePrecision(), Azimuth.ChangePrecision());
        public static implicit operator Ara3D.DoublePrecision.LogPolarCoordinate(LogPolarCoordinate self) => self.ChangePrecision();
        public static implicit operator (Number, Angle)(LogPolarCoordinate self) => (self.Rho, self.Azimuth);
        public static implicit operator LogPolarCoordinate((Number, Angle) value) => new LogPolarCoordinate(value.Item1, value.Item2);
        public void Deconstruct(out Number rho, out Angle azimuth) { rho = Rho; azimuth = Azimuth; }
        public override bool Equals(object obj) { if (!(obj is LogPolarCoordinate)) return false; var other = (LogPolarCoordinate)obj; return Rho.Equals(other.Rho) && Azimuth.Equals(other.Azimuth); }
        public override int GetHashCode() => Intrinsics.CombineHashCodes(Rho, Azimuth);
        public override string ToString() => Intrinsics.MakeString(TypeName, FieldNames, FieldValues);
        public static implicit operator Dynamic(LogPolarCoordinate self) => new Dynamic(self);
        public static implicit operator LogPolarCoordinate(Dynamic value) => value.As<LogPolarCoordinate>();
        public String TypeName => "LogPolarCoordinate";
        public Array<String> FieldNames => Intrinsics.MakeArray<String>((String)"Rho", (String)"Azimuth");
        public Array<Dynamic> FieldValues => Intrinsics.MakeArray<Dynamic>(new Dynamic(Rho), new Dynamic(Azimuth));
        // Unimplemented concept functions
        public Boolean Between(LogPolarCoordinate a, LogPolarCoordinate b) => (Rho.Between(a.Rho, b.Rho) & Azimuth.Between(a.Azimuth, b.Azimuth));
        public LogPolarCoordinate Clamp(LogPolarCoordinate a, LogPolarCoordinate b) => (Rho.Clamp(a.Rho, b.Rho), Azimuth.Clamp(a.Azimuth, b.Azimuth));
        public LogPolarCoordinate Lerp(LogPolarCoordinate b, Number amount) => (Rho.Lerp(b.Rho, amount), Azimuth.Lerp(b.Azimuth, amount));
        public Boolean Equals(LogPolarCoordinate b) => (Rho.Equals(b.Rho) & Azimuth.Equals(b.Azimuth));
        public static Boolean operator ==(LogPolarCoordinate a, LogPolarCoordinate b) => a.Equals(b);
        public Boolean NotEquals(LogPolarCoordinate b) => (Rho.NotEquals(b.Rho) & Azimuth.NotEquals(b.Azimuth));
        public static Boolean operator !=(LogPolarCoordinate a, LogPolarCoordinate b) => a.NotEquals(b);
    }
    public readonly partial struct CylindricalCoordinate: Coordinate<CylindricalCoordinate>
    {
        public readonly Number RadialDistance;
        public readonly Angle Azimuth;
        public readonly Number Height;
        public CylindricalCoordinate WithRadialDistance(Number radialDistance) => (radialDistance, Azimuth, Height);
        public CylindricalCoordinate WithAzimuth(Angle azimuth) => (RadialDistance, azimuth, Height);
        public CylindricalCoordinate WithHeight(Number height) => (RadialDistance, Azimuth, height);
        public CylindricalCoordinate(Number radialDistance, Angle azimuth, Number height) => (RadialDistance, Azimuth, Height) = (radialDistance, azimuth, height);
        public static CylindricalCoordinate Default = new CylindricalCoordinate();
        public static CylindricalCoordinate New(Number radialDistance, Angle azimuth, Number height) => new CylindricalCoordinate(radialDistance, azimuth, height);
        public Ara3D.DoublePrecision.CylindricalCoordinate ChangePrecision() => (RadialDistance.ChangePrecision(), Azimuth.ChangePrecision(), Height.ChangePrecision());
        public static implicit operator Ara3D.DoublePrecision.CylindricalCoordinate(CylindricalCoordinate self) => self.ChangePrecision();
        public static implicit operator (Number, Angle, Number)(CylindricalCoordinate self) => (self.RadialDistance, self.Azimuth, self.Height);
        public static implicit operator CylindricalCoordinate((Number, Angle, Number) value) => new CylindricalCoordinate(value.Item1, value.Item2, value.Item3);
        public void Deconstruct(out Number radialDistance, out Angle azimuth, out Number height) { radialDistance = RadialDistance; azimuth = Azimuth; height = Height; }
        public override bool Equals(object obj) { if (!(obj is CylindricalCoordinate)) return false; var other = (CylindricalCoordinate)obj; return RadialDistance.Equals(other.RadialDistance) && Azimuth.Equals(other.Azimuth) && Height.Equals(other.Height); }
        public override int GetHashCode() => Intrinsics.CombineHashCodes(RadialDistance, Azimuth, Height);
        public override string ToString() => Intrinsics.MakeString(TypeName, FieldNames, FieldValues);
        public static implicit operator Dynamic(CylindricalCoordinate self) => new Dynamic(self);
        public static implicit operator CylindricalCoordinate(Dynamic value) => value.As<CylindricalCoordinate>();
        public String TypeName => "CylindricalCoordinate";
        public Array<String> FieldNames => Intrinsics.MakeArray<String>((String)"RadialDistance", (String)"Azimuth", (String)"Height");
        public Array<Dynamic> FieldValues => Intrinsics.MakeArray<Dynamic>(new Dynamic(RadialDistance), new Dynamic(Azimuth), new Dynamic(Height));
        // Unimplemented concept functions
        public Boolean Between(CylindricalCoordinate a, CylindricalCoordinate b) => (RadialDistance.Between(a.RadialDistance, b.RadialDistance) & Azimuth.Between(a.Azimuth, b.Azimuth) & Height.Between(a.Height, b.Height));
        public CylindricalCoordinate Clamp(CylindricalCoordinate a, CylindricalCoordinate b) => (RadialDistance.Clamp(a.RadialDistance, b.RadialDistance), Azimuth.Clamp(a.Azimuth, b.Azimuth), Height.Clamp(a.Height, b.Height));
        public CylindricalCoordinate Lerp(CylindricalCoordinate b, Number amount) => (RadialDistance.Lerp(b.RadialDistance, amount), Azimuth.Lerp(b.Azimuth, amount), Height.Lerp(b.Height, amount));
        public Boolean Equals(CylindricalCoordinate b) => (RadialDistance.Equals(b.RadialDistance) & Azimuth.Equals(b.Azimuth) & Height.Equals(b.Height));
        public static Boolean operator ==(CylindricalCoordinate a, CylindricalCoordinate b) => a.Equals(b);
        public Boolean NotEquals(CylindricalCoordinate b) => (RadialDistance.NotEquals(b.RadialDistance) & Azimuth.NotEquals(b.Azimuth) & Height.NotEquals(b.Height));
        public static Boolean operator !=(CylindricalCoordinate a, CylindricalCoordinate b) => a.NotEquals(b);
    }
    public readonly partial struct HorizontalCoordinate: Coordinate<HorizontalCoordinate>
    {
        public readonly Number Radius;
        public readonly Angle Azimuth;
        public readonly Number Height;
        public HorizontalCoordinate WithRadius(Number radius) => (radius, Azimuth, Height);
        public HorizontalCoordinate WithAzimuth(Angle azimuth) => (Radius, azimuth, Height);
        public HorizontalCoordinate WithHeight(Number height) => (Radius, Azimuth, height);
        public HorizontalCoordinate(Number radius, Angle azimuth, Number height) => (Radius, Azimuth, Height) = (radius, azimuth, height);
        public static HorizontalCoordinate Default = new HorizontalCoordinate();
        public static HorizontalCoordinate New(Number radius, Angle azimuth, Number height) => new HorizontalCoordinate(radius, azimuth, height);
        public Ara3D.DoublePrecision.HorizontalCoordinate ChangePrecision() => (Radius.ChangePrecision(), Azimuth.ChangePrecision(), Height.ChangePrecision());
        public static implicit operator Ara3D.DoublePrecision.HorizontalCoordinate(HorizontalCoordinate self) => self.ChangePrecision();
        public static implicit operator (Number, Angle, Number)(HorizontalCoordinate self) => (self.Radius, self.Azimuth, self.Height);
        public static implicit operator HorizontalCoordinate((Number, Angle, Number) value) => new HorizontalCoordinate(value.Item1, value.Item2, value.Item3);
        public void Deconstruct(out Number radius, out Angle azimuth, out Number height) { radius = Radius; azimuth = Azimuth; height = Height; }
        public override bool Equals(object obj) { if (!(obj is HorizontalCoordinate)) return false; var other = (HorizontalCoordinate)obj; return Radius.Equals(other.Radius) && Azimuth.Equals(other.Azimuth) && Height.Equals(other.Height); }
        public override int GetHashCode() => Intrinsics.CombineHashCodes(Radius, Azimuth, Height);
        public override string ToString() => Intrinsics.MakeString(TypeName, FieldNames, FieldValues);
        public static implicit operator Dynamic(HorizontalCoordinate self) => new Dynamic(self);
        public static implicit operator HorizontalCoordinate(Dynamic value) => value.As<HorizontalCoordinate>();
        public String TypeName => "HorizontalCoordinate";
        public Array<String> FieldNames => Intrinsics.MakeArray<String>((String)"Radius", (String)"Azimuth", (String)"Height");
        public Array<Dynamic> FieldValues => Intrinsics.MakeArray<Dynamic>(new Dynamic(Radius), new Dynamic(Azimuth), new Dynamic(Height));
        // Unimplemented concept functions
        public Boolean Between(HorizontalCoordinate a, HorizontalCoordinate b) => (Radius.Between(a.Radius, b.Radius) & Azimuth.Between(a.Azimuth, b.Azimuth) & Height.Between(a.Height, b.Height));
        public HorizontalCoordinate Clamp(HorizontalCoordinate a, HorizontalCoordinate b) => (Radius.Clamp(a.Radius, b.Radius), Azimuth.Clamp(a.Azimuth, b.Azimuth), Height.Clamp(a.Height, b.Height));
        public HorizontalCoordinate Lerp(HorizontalCoordinate b, Number amount) => (Radius.Lerp(b.Radius, amount), Azimuth.Lerp(b.Azimuth, amount), Height.Lerp(b.Height, amount));
        public Boolean Equals(HorizontalCoordinate b) => (Radius.Equals(b.Radius) & Azimuth.Equals(b.Azimuth) & Height.Equals(b.Height));
        public static Boolean operator ==(HorizontalCoordinate a, HorizontalCoordinate b) => a.Equals(b);
        public Boolean NotEquals(HorizontalCoordinate b) => (Radius.NotEquals(b.Radius) & Azimuth.NotEquals(b.Azimuth) & Height.NotEquals(b.Height));
        public static Boolean operator !=(HorizontalCoordinate a, HorizontalCoordinate b) => a.NotEquals(b);
    }
    public readonly partial struct GeoCoordinate: Coordinate<GeoCoordinate>
    {
        public readonly Angle Latitude;
        public readonly Angle Longitude;
        public GeoCoordinate WithLatitude(Angle latitude) => (latitude, Longitude);
        public GeoCoordinate WithLongitude(Angle longitude) => (Latitude, longitude);
        public GeoCoordinate(Angle latitude, Angle longitude) => (Latitude, Longitude) = (latitude, longitude);
        public static GeoCoordinate Default = new GeoCoordinate();
        public static GeoCoordinate New(Angle latitude, Angle longitude) => new GeoCoordinate(latitude, longitude);
        public Ara3D.DoublePrecision.GeoCoordinate ChangePrecision() => (Latitude.ChangePrecision(), Longitude.ChangePrecision());
        public static implicit operator Ara3D.DoublePrecision.GeoCoordinate(GeoCoordinate self) => self.ChangePrecision();
        public static implicit operator (Angle, Angle)(GeoCoordinate self) => (self.Latitude, self.Longitude);
        public static implicit operator GeoCoordinate((Angle, Angle) value) => new GeoCoordinate(value.Item1, value.Item2);
        public void Deconstruct(out Angle latitude, out Angle longitude) { latitude = Latitude; longitude = Longitude; }
        public override bool Equals(object obj) { if (!(obj is GeoCoordinate)) return false; var other = (GeoCoordinate)obj; return Latitude.Equals(other.Latitude) && Longitude.Equals(other.Longitude); }
        public override int GetHashCode() => Intrinsics.CombineHashCodes(Latitude, Longitude);
        public override string ToString() => Intrinsics.MakeString(TypeName, FieldNames, FieldValues);
        public static implicit operator Dynamic(GeoCoordinate self) => new Dynamic(self);
        public static implicit operator GeoCoordinate(Dynamic value) => value.As<GeoCoordinate>();
        public String TypeName => "GeoCoordinate";
        public Array<String> FieldNames => Intrinsics.MakeArray<String>((String)"Latitude", (String)"Longitude");
        public Array<Dynamic> FieldValues => Intrinsics.MakeArray<Dynamic>(new Dynamic(Latitude), new Dynamic(Longitude));
        // Unimplemented concept functions
        public Boolean Between(GeoCoordinate a, GeoCoordinate b) => (Latitude.Between(a.Latitude, b.Latitude) & Longitude.Between(a.Longitude, b.Longitude));
        public GeoCoordinate Clamp(GeoCoordinate a, GeoCoordinate b) => (Latitude.Clamp(a.Latitude, b.Latitude), Longitude.Clamp(a.Longitude, b.Longitude));
        public GeoCoordinate Lerp(GeoCoordinate b, Number amount) => (Latitude.Lerp(b.Latitude, amount), Longitude.Lerp(b.Longitude, amount));
        public Boolean Equals(GeoCoordinate b) => (Latitude.Equals(b.Latitude) & Longitude.Equals(b.Longitude));
        public static Boolean operator ==(GeoCoordinate a, GeoCoordinate b) => a.Equals(b);
        public Boolean NotEquals(GeoCoordinate b) => (Latitude.NotEquals(b.Latitude) & Longitude.NotEquals(b.Longitude));
        public static Boolean operator !=(GeoCoordinate a, GeoCoordinate b) => a.NotEquals(b);
    }
    public readonly partial struct GeoCoordinateWithAltitude: Coordinate<GeoCoordinateWithAltitude>
    {
        public readonly GeoCoordinate Coordinate;
        public readonly Number Altitude;
        public GeoCoordinateWithAltitude WithCoordinate(GeoCoordinate coordinate) => (coordinate, Altitude);
        public GeoCoordinateWithAltitude WithAltitude(Number altitude) => (Coordinate, altitude);
        public GeoCoordinateWithAltitude(GeoCoordinate coordinate, Number altitude) => (Coordinate, Altitude) = (coordinate, altitude);
        public static GeoCoordinateWithAltitude Default = new GeoCoordinateWithAltitude();
        public static GeoCoordinateWithAltitude New(GeoCoordinate coordinate, Number altitude) => new GeoCoordinateWithAltitude(coordinate, altitude);
        public Ara3D.DoublePrecision.GeoCoordinateWithAltitude ChangePrecision() => (Coordinate.ChangePrecision(), Altitude.ChangePrecision());
        public static implicit operator Ara3D.DoublePrecision.GeoCoordinateWithAltitude(GeoCoordinateWithAltitude self) => self.ChangePrecision();
        public static implicit operator (GeoCoordinate, Number)(GeoCoordinateWithAltitude self) => (self.Coordinate, self.Altitude);
        public static implicit operator GeoCoordinateWithAltitude((GeoCoordinate, Number) value) => new GeoCoordinateWithAltitude(value.Item1, value.Item2);
        public void Deconstruct(out GeoCoordinate coordinate, out Number altitude) { coordinate = Coordinate; altitude = Altitude; }
        public override bool Equals(object obj) { if (!(obj is GeoCoordinateWithAltitude)) return false; var other = (GeoCoordinateWithAltitude)obj; return Coordinate.Equals(other.Coordinate) && Altitude.Equals(other.Altitude); }
        public override int GetHashCode() => Intrinsics.CombineHashCodes(Coordinate, Altitude);
        public override string ToString() => Intrinsics.MakeString(TypeName, FieldNames, FieldValues);
        public static implicit operator Dynamic(GeoCoordinateWithAltitude self) => new Dynamic(self);
        public static implicit operator GeoCoordinateWithAltitude(Dynamic value) => value.As<GeoCoordinateWithAltitude>();
        public String TypeName => "GeoCoordinateWithAltitude";
        public Array<String> FieldNames => Intrinsics.MakeArray<String>((String)"Coordinate", (String)"Altitude");
        public Array<Dynamic> FieldValues => Intrinsics.MakeArray<Dynamic>(new Dynamic(Coordinate), new Dynamic(Altitude));
        // Unimplemented concept functions
        public Boolean Between(GeoCoordinateWithAltitude a, GeoCoordinateWithAltitude b) => (Coordinate.Between(a.Coordinate, b.Coordinate) & Altitude.Between(a.Altitude, b.Altitude));
        public GeoCoordinateWithAltitude Clamp(GeoCoordinateWithAltitude a, GeoCoordinateWithAltitude b) => (Coordinate.Clamp(a.Coordinate, b.Coordinate), Altitude.Clamp(a.Altitude, b.Altitude));
        public GeoCoordinateWithAltitude Lerp(GeoCoordinateWithAltitude b, Number amount) => (Coordinate.Lerp(b.Coordinate, amount), Altitude.Lerp(b.Altitude, amount));
        public Boolean Equals(GeoCoordinateWithAltitude b) => (Coordinate.Equals(b.Coordinate) & Altitude.Equals(b.Altitude));
        public static Boolean operator ==(GeoCoordinateWithAltitude a, GeoCoordinateWithAltitude b) => a.Equals(b);
        public Boolean NotEquals(GeoCoordinateWithAltitude b) => (Coordinate.NotEquals(b.Coordinate) & Altitude.NotEquals(b.Altitude));
        public static Boolean operator !=(GeoCoordinateWithAltitude a, GeoCoordinateWithAltitude b) => a.NotEquals(b);
    }
    public readonly partial struct Size2D: Value<Size2D>
    {
        public readonly Number Width;
        public readonly Number Height;
        public Size2D WithWidth(Number width) => (width, Height);
        public Size2D WithHeight(Number height) => (Width, height);
        public Size2D(Number width, Number height) => (Width, Height) = (width, height);
        public static Size2D Default = new Size2D();
        public static Size2D New(Number width, Number height) => new Size2D(width, height);
        public Ara3D.DoublePrecision.Size2D ChangePrecision() => (Width.ChangePrecision(), Height.ChangePrecision());
        public static implicit operator Ara3D.DoublePrecision.Size2D(Size2D self) => self.ChangePrecision();
        public static implicit operator (Number, Number)(Size2D self) => (self.Width, self.Height);
        public static implicit operator Size2D((Number, Number) value) => new Size2D(value.Item1, value.Item2);
        public void Deconstruct(out Number width, out Number height) { width = Width; height = Height; }
        public override bool Equals(object obj) { if (!(obj is Size2D)) return false; var other = (Size2D)obj; return Width.Equals(other.Width) && Height.Equals(other.Height); }
        public override int GetHashCode() => Intrinsics.CombineHashCodes(Width, Height);
        public override string ToString() => Intrinsics.MakeString(TypeName, FieldNames, FieldValues);
        public static implicit operator Dynamic(Size2D self) => new Dynamic(self);
        public static implicit operator Size2D(Dynamic value) => value.As<Size2D>();
        public String TypeName => "Size2D";
        public Array<String> FieldNames => Intrinsics.MakeArray<String>((String)"Width", (String)"Height");
        public Array<Dynamic> FieldValues => Intrinsics.MakeArray<Dynamic>(new Dynamic(Width), new Dynamic(Height));
        // Unimplemented concept functions
        public Boolean Equals(Size2D b) => (Width.Equals(b.Width) & Height.Equals(b.Height));
        public static Boolean operator ==(Size2D a, Size2D b) => a.Equals(b);
        public Boolean NotEquals(Size2D b) => (Width.NotEquals(b.Width) & Height.NotEquals(b.Height));
        public static Boolean operator !=(Size2D a, Size2D b) => a.NotEquals(b);
    }
    public readonly partial struct Size3D: Value<Size3D>
    {
        public readonly Number Width;
        public readonly Number Height;
        public readonly Number Depth;
        public Size3D WithWidth(Number width) => (width, Height, Depth);
        public Size3D WithHeight(Number height) => (Width, height, Depth);
        public Size3D WithDepth(Number depth) => (Width, Height, depth);
        public Size3D(Number width, Number height, Number depth) => (Width, Height, Depth) = (width, height, depth);
        public static Size3D Default = new Size3D();
        public static Size3D New(Number width, Number height, Number depth) => new Size3D(width, height, depth);
        public Ara3D.DoublePrecision.Size3D ChangePrecision() => (Width.ChangePrecision(), Height.ChangePrecision(), Depth.ChangePrecision());
        public static implicit operator Ara3D.DoublePrecision.Size3D(Size3D self) => self.ChangePrecision();
        public static implicit operator (Number, Number, Number)(Size3D self) => (self.Width, self.Height, self.Depth);
        public static implicit operator Size3D((Number, Number, Number) value) => new Size3D(value.Item1, value.Item2, value.Item3);
        public void Deconstruct(out Number width, out Number height, out Number depth) { width = Width; height = Height; depth = Depth; }
        public override bool Equals(object obj) { if (!(obj is Size3D)) return false; var other = (Size3D)obj; return Width.Equals(other.Width) && Height.Equals(other.Height) && Depth.Equals(other.Depth); }
        public override int GetHashCode() => Intrinsics.CombineHashCodes(Width, Height, Depth);
        public override string ToString() => Intrinsics.MakeString(TypeName, FieldNames, FieldValues);
        public static implicit operator Dynamic(Size3D self) => new Dynamic(self);
        public static implicit operator Size3D(Dynamic value) => value.As<Size3D>();
        public String TypeName => "Size3D";
        public Array<String> FieldNames => Intrinsics.MakeArray<String>((String)"Width", (String)"Height", (String)"Depth");
        public Array<Dynamic> FieldValues => Intrinsics.MakeArray<Dynamic>(new Dynamic(Width), new Dynamic(Height), new Dynamic(Depth));
        // Unimplemented concept functions
        public Boolean Equals(Size3D b) => (Width.Equals(b.Width) & Height.Equals(b.Height) & Depth.Equals(b.Depth));
        public static Boolean operator ==(Size3D a, Size3D b) => a.Equals(b);
        public Boolean NotEquals(Size3D b) => (Width.NotEquals(b.Width) & Height.NotEquals(b.Height) & Depth.NotEquals(b.Depth));
        public static Boolean operator !=(Size3D a, Size3D b) => a.NotEquals(b);
    }
    public readonly partial struct Fraction: Value<Fraction>
    {
        public readonly Number Numerator;
        public readonly Number Denominator;
        public Fraction WithNumerator(Number numerator) => (numerator, Denominator);
        public Fraction WithDenominator(Number denominator) => (Numerator, denominator);
        public Fraction(Number numerator, Number denominator) => (Numerator, Denominator) = (numerator, denominator);
        public static Fraction Default = new Fraction();
        public static Fraction New(Number numerator, Number denominator) => new Fraction(numerator, denominator);
        public Ara3D.DoublePrecision.Fraction ChangePrecision() => (Numerator.ChangePrecision(), Denominator.ChangePrecision());
        public static implicit operator Ara3D.DoublePrecision.Fraction(Fraction self) => self.ChangePrecision();
        public static implicit operator (Number, Number)(Fraction self) => (self.Numerator, self.Denominator);
        public static implicit operator Fraction((Number, Number) value) => new Fraction(value.Item1, value.Item2);
        public void Deconstruct(out Number numerator, out Number denominator) { numerator = Numerator; denominator = Denominator; }
        public override bool Equals(object obj) { if (!(obj is Fraction)) return false; var other = (Fraction)obj; return Numerator.Equals(other.Numerator) && Denominator.Equals(other.Denominator); }
        public override int GetHashCode() => Intrinsics.CombineHashCodes(Numerator, Denominator);
        public override string ToString() => Intrinsics.MakeString(TypeName, FieldNames, FieldValues);
        public static implicit operator Dynamic(Fraction self) => new Dynamic(self);
        public static implicit operator Fraction(Dynamic value) => value.As<Fraction>();
        public String TypeName => "Fraction";
        public Array<String> FieldNames => Intrinsics.MakeArray<String>((String)"Numerator", (String)"Denominator");
        public Array<Dynamic> FieldValues => Intrinsics.MakeArray<Dynamic>(new Dynamic(Numerator), new Dynamic(Denominator));
        // Unimplemented concept functions
        public Boolean Equals(Fraction b) => (Numerator.Equals(b.Numerator) & Denominator.Equals(b.Denominator));
        public static Boolean operator ==(Fraction a, Fraction b) => a.Equals(b);
        public Boolean NotEquals(Fraction b) => (Numerator.NotEquals(b.Numerator) & Denominator.NotEquals(b.Denominator));
        public static Boolean operator !=(Fraction a, Fraction b) => a.NotEquals(b);
    }
    public readonly partial struct Angle: Measure<Angle>
    {
        public readonly Number Radians;
        public Angle WithRadians(Number radians) => (radians);
        public Angle(Number radians) => (Radians) = (radians);
        public static Angle Default = new Angle();
        public static Angle New(Number radians) => new Angle(radians);
        public Ara3D.DoublePrecision.Angle ChangePrecision() => (Radians.ChangePrecision());
        public static implicit operator Ara3D.DoublePrecision.Angle(Angle self) => self.ChangePrecision();
        public static implicit operator Number(Angle self) => self.Radians;
        public static implicit operator Angle(Number value) => new Angle(value);
        public override bool Equals(object obj) { if (!(obj is Angle)) return false; var other = (Angle)obj; return Radians.Equals(other.Radians); }
        public override int GetHashCode() => Intrinsics.CombineHashCodes(Radians);
        public override string ToString() => Intrinsics.MakeString(TypeName, FieldNames, FieldValues);
        public static implicit operator Dynamic(Angle self) => new Dynamic(self);
        public static implicit operator Angle(Dynamic value) => value.As<Angle>();
        public String TypeName => "Angle";
        public Array<String> FieldNames => Intrinsics.MakeArray<String>((String)"Radians");
        public Array<Dynamic> FieldValues => Intrinsics.MakeArray<Dynamic>(new Dynamic(Radians));
        // Unimplemented concept functions
        public Number Subtract(Angle other) => (Radians.Subtract(other.Radians));
        public static Number operator -(Angle self, Angle other) => self.Subtract(other);
        public Angle Add(Number other) => (Radians.Add(other));
        public static Angle operator +(Angle self, Number other) => self.Add(other);
        public Angle Subtract(Number other) => (Radians.Subtract(other));
        public static Angle operator -(Angle self, Number other) => self.Subtract(other);
        public Number Value => (Radians.Value);
        public Angle Multiply(Number other) => (Radians.Multiply(other));
        public static Angle operator *(Angle self, Number other) => self.Multiply(other);
        public Angle Divide(Number other) => (Radians.Divide(other));
        public static Angle operator /(Angle self, Number other) => self.Divide(other);
        public Angle Modulo(Number other) => (Radians.Modulo(other));
        public static Angle operator %(Angle self, Number other) => self.Modulo(other);
        public Angle Reciprocal => (Radians.Reciprocal);
        public Angle Negative => (Radians.Negative);
        public static Angle operator -(Angle self) => self.Negative;
        public Integer Compare(Angle y) => (Radians.Compare(y.Radians));
        public Angle Zero => (Radians.Zero);
        public Angle One => (Radians.One);
        public Angle MinValue => (Radians.MinValue);
        public Angle MaxValue => (Radians.MaxValue);
    }
    public readonly partial struct Length: Measure<Length>
    {
        public readonly Number Meters;
        public Length WithMeters(Number meters) => (meters);
        public Length(Number meters) => (Meters) = (meters);
        public static Length Default = new Length();
        public static Length New(Number meters) => new Length(meters);
        public Ara3D.DoublePrecision.Length ChangePrecision() => (Meters.ChangePrecision());
        public static implicit operator Ara3D.DoublePrecision.Length(Length self) => self.ChangePrecision();
        public static implicit operator Number(Length self) => self.Meters;
        public static implicit operator Length(Number value) => new Length(value);
        public override bool Equals(object obj) { if (!(obj is Length)) return false; var other = (Length)obj; return Meters.Equals(other.Meters); }
        public override int GetHashCode() => Intrinsics.CombineHashCodes(Meters);
        public override string ToString() => Intrinsics.MakeString(TypeName, FieldNames, FieldValues);
        public static implicit operator Dynamic(Length self) => new Dynamic(self);
        public static implicit operator Length(Dynamic value) => value.As<Length>();
        public String TypeName => "Length";
        public Array<String> FieldNames => Intrinsics.MakeArray<String>((String)"Meters");
        public Array<Dynamic> FieldValues => Intrinsics.MakeArray<Dynamic>(new Dynamic(Meters));
        // Unimplemented concept functions
        public Number Subtract(Length other) => (Meters.Subtract(other.Meters));
        public static Number operator -(Length self, Length other) => self.Subtract(other);
        public Length Add(Number other) => (Meters.Add(other));
        public static Length operator +(Length self, Number other) => self.Add(other);
        public Length Subtract(Number other) => (Meters.Subtract(other));
        public static Length operator -(Length self, Number other) => self.Subtract(other);
        public Number Value => (Meters.Value);
        public Length Multiply(Number other) => (Meters.Multiply(other));
        public static Length operator *(Length self, Number other) => self.Multiply(other);
        public Length Divide(Number other) => (Meters.Divide(other));
        public static Length operator /(Length self, Number other) => self.Divide(other);
        public Length Modulo(Number other) => (Meters.Modulo(other));
        public static Length operator %(Length self, Number other) => self.Modulo(other);
        public Length Reciprocal => (Meters.Reciprocal);
        public Length Negative => (Meters.Negative);
        public static Length operator -(Length self) => self.Negative;
        public Integer Compare(Length y) => (Meters.Compare(y.Meters));
        public Length Zero => (Meters.Zero);
        public Length One => (Meters.One);
        public Length MinValue => (Meters.MinValue);
        public Length MaxValue => (Meters.MaxValue);
    }
    public readonly partial struct Mass: Measure<Mass>
    {
        public readonly Number Kilograms;
        public Mass WithKilograms(Number kilograms) => (kilograms);
        public Mass(Number kilograms) => (Kilograms) = (kilograms);
        public static Mass Default = new Mass();
        public static Mass New(Number kilograms) => new Mass(kilograms);
        public Ara3D.DoublePrecision.Mass ChangePrecision() => (Kilograms.ChangePrecision());
        public static implicit operator Ara3D.DoublePrecision.Mass(Mass self) => self.ChangePrecision();
        public static implicit operator Number(Mass self) => self.Kilograms;
        public static implicit operator Mass(Number value) => new Mass(value);
        public override bool Equals(object obj) { if (!(obj is Mass)) return false; var other = (Mass)obj; return Kilograms.Equals(other.Kilograms); }
        public override int GetHashCode() => Intrinsics.CombineHashCodes(Kilograms);
        public override string ToString() => Intrinsics.MakeString(TypeName, FieldNames, FieldValues);
        public static implicit operator Dynamic(Mass self) => new Dynamic(self);
        public static implicit operator Mass(Dynamic value) => value.As<Mass>();
        public String TypeName => "Mass";
        public Array<String> FieldNames => Intrinsics.MakeArray<String>((String)"Kilograms");
        public Array<Dynamic> FieldValues => Intrinsics.MakeArray<Dynamic>(new Dynamic(Kilograms));
        // Unimplemented concept functions
        public Number Subtract(Mass other) => (Kilograms.Subtract(other.Kilograms));
        public static Number operator -(Mass self, Mass other) => self.Subtract(other);
        public Mass Add(Number other) => (Kilograms.Add(other));
        public static Mass operator +(Mass self, Number other) => self.Add(other);
        public Mass Subtract(Number other) => (Kilograms.Subtract(other));
        public static Mass operator -(Mass self, Number other) => self.Subtract(other);
        public Number Value => (Kilograms.Value);
        public Mass Multiply(Number other) => (Kilograms.Multiply(other));
        public static Mass operator *(Mass self, Number other) => self.Multiply(other);
        public Mass Divide(Number other) => (Kilograms.Divide(other));
        public static Mass operator /(Mass self, Number other) => self.Divide(other);
        public Mass Modulo(Number other) => (Kilograms.Modulo(other));
        public static Mass operator %(Mass self, Number other) => self.Modulo(other);
        public Mass Reciprocal => (Kilograms.Reciprocal);
        public Mass Negative => (Kilograms.Negative);
        public static Mass operator -(Mass self) => self.Negative;
        public Integer Compare(Mass y) => (Kilograms.Compare(y.Kilograms));
        public Mass Zero => (Kilograms.Zero);
        public Mass One => (Kilograms.One);
        public Mass MinValue => (Kilograms.MinValue);
        public Mass MaxValue => (Kilograms.MaxValue);
    }
    public readonly partial struct Temperature: Measure<Temperature>
    {
        public readonly Number Celsius;
        public Temperature WithCelsius(Number celsius) => (celsius);
        public Temperature(Number celsius) => (Celsius) = (celsius);
        public static Temperature Default = new Temperature();
        public static Temperature New(Number celsius) => new Temperature(celsius);
        public Ara3D.DoublePrecision.Temperature ChangePrecision() => (Celsius.ChangePrecision());
        public static implicit operator Ara3D.DoublePrecision.Temperature(Temperature self) => self.ChangePrecision();
        public static implicit operator Number(Temperature self) => self.Celsius;
        public static implicit operator Temperature(Number value) => new Temperature(value);
        public override bool Equals(object obj) { if (!(obj is Temperature)) return false; var other = (Temperature)obj; return Celsius.Equals(other.Celsius); }
        public override int GetHashCode() => Intrinsics.CombineHashCodes(Celsius);
        public override string ToString() => Intrinsics.MakeString(TypeName, FieldNames, FieldValues);
        public static implicit operator Dynamic(Temperature self) => new Dynamic(self);
        public static implicit operator Temperature(Dynamic value) => value.As<Temperature>();
        public String TypeName => "Temperature";
        public Array<String> FieldNames => Intrinsics.MakeArray<String>((String)"Celsius");
        public Array<Dynamic> FieldValues => Intrinsics.MakeArray<Dynamic>(new Dynamic(Celsius));
        // Unimplemented concept functions
        public Number Subtract(Temperature other) => (Celsius.Subtract(other.Celsius));
        public static Number operator -(Temperature self, Temperature other) => self.Subtract(other);
        public Temperature Add(Number other) => (Celsius.Add(other));
        public static Temperature operator +(Temperature self, Number other) => self.Add(other);
        public Temperature Subtract(Number other) => (Celsius.Subtract(other));
        public static Temperature operator -(Temperature self, Number other) => self.Subtract(other);
        public Number Value => (Celsius.Value);
        public Temperature Multiply(Number other) => (Celsius.Multiply(other));
        public static Temperature operator *(Temperature self, Number other) => self.Multiply(other);
        public Temperature Divide(Number other) => (Celsius.Divide(other));
        public static Temperature operator /(Temperature self, Number other) => self.Divide(other);
        public Temperature Modulo(Number other) => (Celsius.Modulo(other));
        public static Temperature operator %(Temperature self, Number other) => self.Modulo(other);
        public Temperature Reciprocal => (Celsius.Reciprocal);
        public Temperature Negative => (Celsius.Negative);
        public static Temperature operator -(Temperature self) => self.Negative;
        public Integer Compare(Temperature y) => (Celsius.Compare(y.Celsius));
        public Temperature Zero => (Celsius.Zero);
        public Temperature One => (Celsius.One);
        public Temperature MinValue => (Celsius.MinValue);
        public Temperature MaxValue => (Celsius.MaxValue);
    }
    public readonly partial struct Time: Measure<Time>
    {
        public readonly Number Seconds;
        public Time WithSeconds(Number seconds) => (seconds);
        public Time(Number seconds) => (Seconds) = (seconds);
        public static Time Default = new Time();
        public static Time New(Number seconds) => new Time(seconds);
        public Ara3D.DoublePrecision.Time ChangePrecision() => (Seconds.ChangePrecision());
        public static implicit operator Ara3D.DoublePrecision.Time(Time self) => self.ChangePrecision();
        public static implicit operator Number(Time self) => self.Seconds;
        public static implicit operator Time(Number value) => new Time(value);
        public override bool Equals(object obj) { if (!(obj is Time)) return false; var other = (Time)obj; return Seconds.Equals(other.Seconds); }
        public override int GetHashCode() => Intrinsics.CombineHashCodes(Seconds);
        public override string ToString() => Intrinsics.MakeString(TypeName, FieldNames, FieldValues);
        public static implicit operator Dynamic(Time self) => new Dynamic(self);
        public static implicit operator Time(Dynamic value) => value.As<Time>();
        public String TypeName => "Time";
        public Array<String> FieldNames => Intrinsics.MakeArray<String>((String)"Seconds");
        public Array<Dynamic> FieldValues => Intrinsics.MakeArray<Dynamic>(new Dynamic(Seconds));
        // Unimplemented concept functions
        public Number Subtract(Time other) => (Seconds.Subtract(other.Seconds));
        public static Number operator -(Time self, Time other) => self.Subtract(other);
        public Time Add(Number other) => (Seconds.Add(other));
        public static Time operator +(Time self, Number other) => self.Add(other);
        public Time Subtract(Number other) => (Seconds.Subtract(other));
        public static Time operator -(Time self, Number other) => self.Subtract(other);
        public Number Value => (Seconds.Value);
        public Time Multiply(Number other) => (Seconds.Multiply(other));
        public static Time operator *(Time self, Number other) => self.Multiply(other);
        public Time Divide(Number other) => (Seconds.Divide(other));
        public static Time operator /(Time self, Number other) => self.Divide(other);
        public Time Modulo(Number other) => (Seconds.Modulo(other));
        public static Time operator %(Time self, Number other) => self.Modulo(other);
        public Time Reciprocal => (Seconds.Reciprocal);
        public Time Negative => (Seconds.Negative);
        public static Time operator -(Time self) => self.Negative;
        public Integer Compare(Time y) => (Seconds.Compare(y.Seconds));
        public Time Zero => (Seconds.Zero);
        public Time One => (Seconds.One);
        public Time MinValue => (Seconds.MinValue);
        public Time MaxValue => (Seconds.MaxValue);
    }
    public readonly partial struct TimeRange: Interval<TimeRange, DateTime, Time>
    {
        public readonly DateTime Min;
        public readonly DateTime Max;
        public TimeRange WithMin(DateTime min) => (min, Max);
        public TimeRange WithMax(DateTime max) => (Min, max);
        public TimeRange(DateTime min, DateTime max) => (Min, Max) = (min, max);
        public static TimeRange Default = new TimeRange();
        public static TimeRange New(DateTime min, DateTime max) => new TimeRange(min, max);
        public Ara3D.DoublePrecision.TimeRange ChangePrecision() => (Min.ChangePrecision(), Max.ChangePrecision());
        public static implicit operator Ara3D.DoublePrecision.TimeRange(TimeRange self) => self.ChangePrecision();
        public static implicit operator (DateTime, DateTime)(TimeRange self) => (self.Min, self.Max);
        public static implicit operator TimeRange((DateTime, DateTime) value) => new TimeRange(value.Item1, value.Item2);
        public void Deconstruct(out DateTime min, out DateTime max) { min = Min; max = Max; }
        public override bool Equals(object obj) { if (!(obj is TimeRange)) return false; var other = (TimeRange)obj; return Min.Equals(other.Min) && Max.Equals(other.Max); }
        public override int GetHashCode() => Intrinsics.CombineHashCodes(Min, Max);
        public override string ToString() => Intrinsics.MakeString(TypeName, FieldNames, FieldValues);
        public static implicit operator Dynamic(TimeRange self) => new Dynamic(self);
        public static implicit operator TimeRange(Dynamic value) => value.As<TimeRange>();
        public String TypeName => "TimeRange";
        public Array<String> FieldNames => Intrinsics.MakeArray<String>((String)"Min", (String)"Max");
        public Array<Dynamic> FieldValues => Intrinsics.MakeArray<Dynamic>(new Dynamic(Min), new Dynamic(Max));
        DateTime Interval<TimeRange, DateTime, Time>.Min => Min;
        DateTime Interval<TimeRange, DateTime, Time>.Max => Max;
        // Unimplemented concept functions
        public Boolean Equals(TimeRange b) => (Min.Equals(b.Min) & Max.Equals(b.Max));
        public static Boolean operator ==(TimeRange a, TimeRange b) => a.Equals(b);
        public Boolean NotEquals(TimeRange b) => (Min.NotEquals(b.Min) & Max.NotEquals(b.Max));
        public static Boolean operator !=(TimeRange a, TimeRange b) => a.NotEquals(b);
    }
    public readonly partial struct DateTime: Coordinate<DateTime>, Difference<DateTime, Time>
    {
        public readonly Number Value;
        public DateTime WithValue(Number value) => (value);
        public DateTime(Number value) => (Value) = (value);
        public static DateTime Default = new DateTime();
        public static DateTime New(Number value) => new DateTime(value);
        public Ara3D.DoublePrecision.DateTime ChangePrecision() => (Value.ChangePrecision());
        public static implicit operator Ara3D.DoublePrecision.DateTime(DateTime self) => self.ChangePrecision();
        public static implicit operator Number(DateTime self) => self.Value;
        public static implicit operator DateTime(Number value) => new DateTime(value);
        public override bool Equals(object obj) { if (!(obj is DateTime)) return false; var other = (DateTime)obj; return Value.Equals(other.Value); }
        public override int GetHashCode() => Intrinsics.CombineHashCodes(Value);
        public override string ToString() => Intrinsics.MakeString(TypeName, FieldNames, FieldValues);
        public static implicit operator Dynamic(DateTime self) => new Dynamic(self);
        public static implicit operator DateTime(Dynamic value) => value.As<DateTime>();
        public String TypeName => "DateTime";
        public Array<String> FieldNames => Intrinsics.MakeArray<String>((String)"Value");
        public Array<Dynamic> FieldValues => Intrinsics.MakeArray<Dynamic>(new Dynamic(Value));
        // Unimplemented concept functions
        public Boolean Between(DateTime a, DateTime b) => (Value.Between(a.Value, b.Value));
        public DateTime Clamp(DateTime a, DateTime b) => (Value.Clamp(a.Value, b.Value));
        public DateTime Lerp(DateTime b, Number amount) => (Value.Lerp(b.Value, amount));
        public Boolean Equals(DateTime b) => (Value.Equals(b.Value));
        public static Boolean operator ==(DateTime a, DateTime b) => a.Equals(b);
        public Boolean NotEquals(DateTime b) => (Value.NotEquals(b.Value));
        public static Boolean operator !=(DateTime a, DateTime b) => a.NotEquals(b);
        public Time Subtract(DateTime other) => (Value.Subtract(other.Value));
        public static Time operator -(DateTime self, DateTime other) => self.Subtract(other);
        public DateTime Add(Time other) => (Value.Add(other));
        public static DateTime operator +(DateTime self, Time other) => self.Add(other);
        public DateTime Subtract(Time other) => (Value.Subtract(other));
        public static DateTime operator -(DateTime self, Time other) => self.Subtract(other);
    }
    public readonly partial struct AnglePair: Interval<AnglePair, Angle, Angle>
    {
        public readonly Angle Min;
        public readonly Angle Max;
        public AnglePair WithMin(Angle min) => (min, Max);
        public AnglePair WithMax(Angle max) => (Min, max);
        public AnglePair(Angle min, Angle max) => (Min, Max) = (min, max);
        public static AnglePair Default = new AnglePair();
        public static AnglePair New(Angle min, Angle max) => new AnglePair(min, max);
        public Ara3D.DoublePrecision.AnglePair ChangePrecision() => (Min.ChangePrecision(), Max.ChangePrecision());
        public static implicit operator Ara3D.DoublePrecision.AnglePair(AnglePair self) => self.ChangePrecision();
        public static implicit operator (Angle, Angle)(AnglePair self) => (self.Min, self.Max);
        public static implicit operator AnglePair((Angle, Angle) value) => new AnglePair(value.Item1, value.Item2);
        public void Deconstruct(out Angle min, out Angle max) { min = Min; max = Max; }
        public override bool Equals(object obj) { if (!(obj is AnglePair)) return false; var other = (AnglePair)obj; return Min.Equals(other.Min) && Max.Equals(other.Max); }
        public override int GetHashCode() => Intrinsics.CombineHashCodes(Min, Max);
        public override string ToString() => Intrinsics.MakeString(TypeName, FieldNames, FieldValues);
        public static implicit operator Dynamic(AnglePair self) => new Dynamic(self);
        public static implicit operator AnglePair(Dynamic value) => value.As<AnglePair>();
        public String TypeName => "AnglePair";
        public Array<String> FieldNames => Intrinsics.MakeArray<String>((String)"Min", (String)"Max");
        public Array<Dynamic> FieldValues => Intrinsics.MakeArray<Dynamic>(new Dynamic(Min), new Dynamic(Max));
        Angle Interval<AnglePair, Angle, Angle>.Min => Min;
        Angle Interval<AnglePair, Angle, Angle>.Max => Max;
        // Unimplemented concept functions
        public Boolean Equals(AnglePair b) => (Min.Equals(b.Min) & Max.Equals(b.Max));
        public static Boolean operator ==(AnglePair a, AnglePair b) => a.Equals(b);
        public Boolean NotEquals(AnglePair b) => (Min.NotEquals(b.Min) & Max.NotEquals(b.Max));
        public static Boolean operator !=(AnglePair a, AnglePair b) => a.NotEquals(b);
    }
    public readonly partial struct NumberInterval: Interval<NumberInterval, Number, Number>
    {
        public readonly Number Min;
        public readonly Number Max;
        public NumberInterval WithMin(Number min) => (min, Max);
        public NumberInterval WithMax(Number max) => (Min, max);
        public NumberInterval(Number min, Number max) => (Min, Max) = (min, max);
        public static NumberInterval Default = new NumberInterval();
        public static NumberInterval New(Number min, Number max) => new NumberInterval(min, max);
        public Ara3D.DoublePrecision.NumberInterval ChangePrecision() => (Min.ChangePrecision(), Max.ChangePrecision());
        public static implicit operator Ara3D.DoublePrecision.NumberInterval(NumberInterval self) => self.ChangePrecision();
        public static implicit operator (Number, Number)(NumberInterval self) => (self.Min, self.Max);
        public static implicit operator NumberInterval((Number, Number) value) => new NumberInterval(value.Item1, value.Item2);
        public void Deconstruct(out Number min, out Number max) { min = Min; max = Max; }
        public override bool Equals(object obj) { if (!(obj is NumberInterval)) return false; var other = (NumberInterval)obj; return Min.Equals(other.Min) && Max.Equals(other.Max); }
        public override int GetHashCode() => Intrinsics.CombineHashCodes(Min, Max);
        public override string ToString() => Intrinsics.MakeString(TypeName, FieldNames, FieldValues);
        public static implicit operator Dynamic(NumberInterval self) => new Dynamic(self);
        public static implicit operator NumberInterval(Dynamic value) => value.As<NumberInterval>();
        public String TypeName => "NumberInterval";
        public Array<String> FieldNames => Intrinsics.MakeArray<String>((String)"Min", (String)"Max");
        public Array<Dynamic> FieldValues => Intrinsics.MakeArray<Dynamic>(new Dynamic(Min), new Dynamic(Max));
        Number Interval<NumberInterval, Number, Number>.Min => Min;
        Number Interval<NumberInterval, Number, Number>.Max => Max;
        // Unimplemented concept functions
        public Boolean Equals(NumberInterval b) => (Min.Equals(b.Min) & Max.Equals(b.Max));
        public static Boolean operator ==(NumberInterval a, NumberInterval b) => a.Equals(b);
        public Boolean NotEquals(NumberInterval b) => (Min.NotEquals(b.Min) & Max.NotEquals(b.Max));
        public static Boolean operator !=(NumberInterval a, NumberInterval b) => a.NotEquals(b);
    }
    public readonly partial struct Matrix2D: Value<Matrix2D>, Array<Vector3D>
    {
        public readonly Vector3D Column1;
        public readonly Vector3D Column2;
        public readonly Vector3D Column3;
        public Matrix2D WithColumn1(Vector3D column1) => (column1, Column2, Column3);
        public Matrix2D WithColumn2(Vector3D column2) => (Column1, column2, Column3);
        public Matrix2D WithColumn3(Vector3D column3) => (Column1, Column2, column3);
        public Matrix2D(Vector3D column1, Vector3D column2, Vector3D column3) => (Column1, Column2, Column3) = (column1, column2, column3);
        public static Matrix2D Default = new Matrix2D();
        public static Matrix2D New(Vector3D column1, Vector3D column2, Vector3D column3) => new Matrix2D(column1, column2, column3);
        public Ara3D.DoublePrecision.Matrix2D ChangePrecision() => (Column1.ChangePrecision(), Column2.ChangePrecision(), Column3.ChangePrecision());
        public static implicit operator Ara3D.DoublePrecision.Matrix2D(Matrix2D self) => self.ChangePrecision();
        public static implicit operator (Vector3D, Vector3D, Vector3D)(Matrix2D self) => (self.Column1, self.Column2, self.Column3);
        public static implicit operator Matrix2D((Vector3D, Vector3D, Vector3D) value) => new Matrix2D(value.Item1, value.Item2, value.Item3);
        public void Deconstruct(out Vector3D column1, out Vector3D column2, out Vector3D column3) { column1 = Column1; column2 = Column2; column3 = Column3; }
        public override bool Equals(object obj) { if (!(obj is Matrix2D)) return false; var other = (Matrix2D)obj; return Column1.Equals(other.Column1) && Column2.Equals(other.Column2) && Column3.Equals(other.Column3); }
        public override int GetHashCode() => Intrinsics.CombineHashCodes(Column1, Column2, Column3);
        public override string ToString() => Intrinsics.MakeString(TypeName, FieldNames, FieldValues);
        public static implicit operator Dynamic(Matrix2D self) => new Dynamic(self);
        public static implicit operator Matrix2D(Dynamic value) => value.As<Matrix2D>();
        public String TypeName => "Matrix2D";
        public Array<String> FieldNames => Intrinsics.MakeArray<String>((String)"Column1", (String)"Column2", (String)"Column3");
        public Array<Dynamic> FieldValues => Intrinsics.MakeArray<Dynamic>(new Dynamic(Column1), new Dynamic(Column2), new Dynamic(Column3));
        // Unimplemented concept functions
        public Boolean Equals(Matrix2D b) => (Column1.Equals(b.Column1) & Column2.Equals(b.Column2) & Column3.Equals(b.Column3));
        public static Boolean operator ==(Matrix2D a, Matrix2D b) => a.Equals(b);
        public Boolean NotEquals(Matrix2D b) => (Column1.NotEquals(b.Column1) & Column2.NotEquals(b.Column2) & Column3.NotEquals(b.Column3));
        public static Boolean operator !=(Matrix2D a, Matrix2D b) => a.NotEquals(b);
        public Integer Count => 3;
        public Vector3D At(Integer n) => n == 0 ? Column1 : n == 1 ? Column2 : n == 2 ? Column3 : throw new System.IndexOutOfRangeException();
        public Vector3D this[Integer n] => At(n);
    }
    public readonly partial struct Matrix3D: Value<Matrix3D>, Array<Vector4D>
    {
        public readonly Vector4D Column1;
        public readonly Vector4D Column2;
        public readonly Vector4D Column3;
        public readonly Vector4D Column4;
        public Matrix3D WithColumn1(Vector4D column1) => (column1, Column2, Column3, Column4);
        public Matrix3D WithColumn2(Vector4D column2) => (Column1, column2, Column3, Column4);
        public Matrix3D WithColumn3(Vector4D column3) => (Column1, Column2, column3, Column4);
        public Matrix3D WithColumn4(Vector4D column4) => (Column1, Column2, Column3, column4);
        public Matrix3D(Vector4D column1, Vector4D column2, Vector4D column3, Vector4D column4) => (Column1, Column2, Column3, Column4) = (column1, column2, column3, column4);
        public static Matrix3D Default = new Matrix3D();
        public static Matrix3D New(Vector4D column1, Vector4D column2, Vector4D column3, Vector4D column4) => new Matrix3D(column1, column2, column3, column4);
        public Ara3D.DoublePrecision.Matrix3D ChangePrecision() => (Column1.ChangePrecision(), Column2.ChangePrecision(), Column3.ChangePrecision(), Column4.ChangePrecision());
        public static implicit operator Ara3D.DoublePrecision.Matrix3D(Matrix3D self) => self.ChangePrecision();
        public static implicit operator (Vector4D, Vector4D, Vector4D, Vector4D)(Matrix3D self) => (self.Column1, self.Column2, self.Column3, self.Column4);
        public static implicit operator Matrix3D((Vector4D, Vector4D, Vector4D, Vector4D) value) => new Matrix3D(value.Item1, value.Item2, value.Item3, value.Item4);
        public void Deconstruct(out Vector4D column1, out Vector4D column2, out Vector4D column3, out Vector4D column4) { column1 = Column1; column2 = Column2; column3 = Column3; column4 = Column4; }
        public override bool Equals(object obj) { if (!(obj is Matrix3D)) return false; var other = (Matrix3D)obj; return Column1.Equals(other.Column1) && Column2.Equals(other.Column2) && Column3.Equals(other.Column3) && Column4.Equals(other.Column4); }
        public override int GetHashCode() => Intrinsics.CombineHashCodes(Column1, Column2, Column3, Column4);
        public override string ToString() => Intrinsics.MakeString(TypeName, FieldNames, FieldValues);
        public static implicit operator Dynamic(Matrix3D self) => new Dynamic(self);
        public static implicit operator Matrix3D(Dynamic value) => value.As<Matrix3D>();
        public String TypeName => "Matrix3D";
        public Array<String> FieldNames => Intrinsics.MakeArray<String>((String)"Column1", (String)"Column2", (String)"Column3", (String)"Column4");
        public Array<Dynamic> FieldValues => Intrinsics.MakeArray<Dynamic>(new Dynamic(Column1), new Dynamic(Column2), new Dynamic(Column3), new Dynamic(Column4));
        // Unimplemented concept functions
        public Boolean Equals(Matrix3D b) => (Column1.Equals(b.Column1) & Column2.Equals(b.Column2) & Column3.Equals(b.Column3) & Column4.Equals(b.Column4));
        public static Boolean operator ==(Matrix3D a, Matrix3D b) => a.Equals(b);
        public Boolean NotEquals(Matrix3D b) => (Column1.NotEquals(b.Column1) & Column2.NotEquals(b.Column2) & Column3.NotEquals(b.Column3) & Column4.NotEquals(b.Column4));
        public static Boolean operator !=(Matrix3D a, Matrix3D b) => a.NotEquals(b);
        public Integer Count => 4;
        public Vector4D At(Integer n) => n == 0 ? Column1 : n == 1 ? Column2 : n == 2 ? Column3 : n == 3 ? Column4 : throw new System.IndexOutOfRangeException();
        public Vector4D this[Integer n] => At(n);
    }
    public readonly partial struct UV: Vector<UV>
    {
        public readonly Number U;
        public readonly Number V;
        public UV WithU(Number u) => (u, V);
        public UV WithV(Number v) => (U, v);
        public UV(Number u, Number v) => (U, V) = (u, v);
        public static UV Default = new UV();
        public static UV New(Number u, Number v) => new UV(u, v);
        public Ara3D.DoublePrecision.UV ChangePrecision() => (U.ChangePrecision(), V.ChangePrecision());
        public static implicit operator Ara3D.DoublePrecision.UV(UV self) => self.ChangePrecision();
        public static implicit operator (Number, Number)(UV self) => (self.U, self.V);
        public static implicit operator UV((Number, Number) value) => new UV(value.Item1, value.Item2);
        public void Deconstruct(out Number u, out Number v) { u = U; v = V; }
        public override bool Equals(object obj) { if (!(obj is UV)) return false; var other = (UV)obj; return U.Equals(other.U) && V.Equals(other.V); }
        public override int GetHashCode() => Intrinsics.CombineHashCodes(U, V);
        public override string ToString() => Intrinsics.MakeString(TypeName, FieldNames, FieldValues);
        public static implicit operator Dynamic(UV self) => new Dynamic(self);
        public static implicit operator UV(Dynamic value) => value.As<UV>();
        public String TypeName => "UV";
        public Array<String> FieldNames => Intrinsics.MakeArray<String>((String)"U", (String)"V");
        public Array<Dynamic> FieldValues => Intrinsics.MakeArray<Dynamic>(new Dynamic(U), new Dynamic(V));
        // Unimplemented concept functions
        public UV Add(Number other) => (U.Add(other), V.Add(other));
        public static UV operator +(UV self, Number other) => self.Add(other);
        public UV Subtract(Number other) => (U.Subtract(other), V.Subtract(other));
        public static UV operator -(UV self, Number other) => self.Subtract(other);
        public UV Multiply(Number other) => (U.Multiply(other), V.Multiply(other));
        public static UV operator *(UV self, Number other) => self.Multiply(other);
        public UV Divide(Number other) => (U.Divide(other), V.Divide(other));
        public static UV operator /(UV self, Number other) => self.Divide(other);
        public UV Modulo(Number other) => (U.Modulo(other), V.Modulo(other));
        public static UV operator %(UV self, Number other) => self.Modulo(other);
        public UV Reciprocal => (U.Reciprocal, V.Reciprocal);
        public UV Negative => (U.Negative, V.Negative);
        public static UV operator -(UV self) => self.Negative;
        public UV Multiply(UV other) => (U.Multiply(other.U), V.Multiply(other.V));
        public static UV operator *(UV self, UV other) => self.Multiply(other);
        public UV Divide(UV other) => (U.Divide(other.U), V.Divide(other.V));
        public static UV operator /(UV self, UV other) => self.Divide(other);
        public UV Modulo(UV other) => (U.Modulo(other.U), V.Modulo(other.V));
        public static UV operator %(UV self, UV other) => self.Modulo(other);
        public UV Add(UV other) => (U.Add(other.U), V.Add(other.V));
        public static UV operator +(UV self, UV other) => self.Add(other);
        public UV Subtract(UV other) => (U.Subtract(other.U), V.Subtract(other.V));
        public static UV operator -(UV self, UV other) => self.Subtract(other);
        public UV Zero => (U.Zero, V.Zero);
        public UV One => (U.One, V.One);
        public UV MinValue => (U.MinValue, V.MinValue);
        public UV MaxValue => (U.MaxValue, V.MaxValue);
        public Boolean Equals(UV b) => (U.Equals(b.U) & V.Equals(b.V));
        public static Boolean operator ==(UV a, UV b) => a.Equals(b);
        public Boolean NotEquals(UV b) => (U.NotEquals(b.U) & V.NotEquals(b.V));
        public static Boolean operator !=(UV a, UV b) => a.NotEquals(b);
        public Boolean Between(UV a, UV b) => (U.Between(a.U, b.U) & V.Between(a.V, b.V));
        public UV Clamp(UV a, UV b) => (U.Clamp(a.U, b.U), V.Clamp(a.V, b.V));
    }
    public readonly partial struct UVW: Vector<UVW>
    {
        public readonly Number U;
        public readonly Number V;
        public readonly Number W;
        public UVW WithU(Number u) => (u, V, W);
        public UVW WithV(Number v) => (U, v, W);
        public UVW WithW(Number w) => (U, V, w);
        public UVW(Number u, Number v, Number w) => (U, V, W) = (u, v, w);
        public static UVW Default = new UVW();
        public static UVW New(Number u, Number v, Number w) => new UVW(u, v, w);
        public Ara3D.DoublePrecision.UVW ChangePrecision() => (U.ChangePrecision(), V.ChangePrecision(), W.ChangePrecision());
        public static implicit operator Ara3D.DoublePrecision.UVW(UVW self) => self.ChangePrecision();
        public static implicit operator (Number, Number, Number)(UVW self) => (self.U, self.V, self.W);
        public static implicit operator UVW((Number, Number, Number) value) => new UVW(value.Item1, value.Item2, value.Item3);
        public void Deconstruct(out Number u, out Number v, out Number w) { u = U; v = V; w = W; }
        public override bool Equals(object obj) { if (!(obj is UVW)) return false; var other = (UVW)obj; return U.Equals(other.U) && V.Equals(other.V) && W.Equals(other.W); }
        public override int GetHashCode() => Intrinsics.CombineHashCodes(U, V, W);
        public override string ToString() => Intrinsics.MakeString(TypeName, FieldNames, FieldValues);
        public static implicit operator Dynamic(UVW self) => new Dynamic(self);
        public static implicit operator UVW(Dynamic value) => value.As<UVW>();
        public String TypeName => "UVW";
        public Array<String> FieldNames => Intrinsics.MakeArray<String>((String)"U", (String)"V", (String)"W");
        public Array<Dynamic> FieldValues => Intrinsics.MakeArray<Dynamic>(new Dynamic(U), new Dynamic(V), new Dynamic(W));
        // Unimplemented concept functions
        public UVW Add(Number other) => (U.Add(other), V.Add(other), W.Add(other));
        public static UVW operator +(UVW self, Number other) => self.Add(other);
        public UVW Subtract(Number other) => (U.Subtract(other), V.Subtract(other), W.Subtract(other));
        public static UVW operator -(UVW self, Number other) => self.Subtract(other);
        public UVW Multiply(Number other) => (U.Multiply(other), V.Multiply(other), W.Multiply(other));
        public static UVW operator *(UVW self, Number other) => self.Multiply(other);
        public UVW Divide(Number other) => (U.Divide(other), V.Divide(other), W.Divide(other));
        public static UVW operator /(UVW self, Number other) => self.Divide(other);
        public UVW Modulo(Number other) => (U.Modulo(other), V.Modulo(other), W.Modulo(other));
        public static UVW operator %(UVW self, Number other) => self.Modulo(other);
        public UVW Reciprocal => (U.Reciprocal, V.Reciprocal, W.Reciprocal);
        public UVW Negative => (U.Negative, V.Negative, W.Negative);
        public static UVW operator -(UVW self) => self.Negative;
        public UVW Multiply(UVW other) => (U.Multiply(other.U), V.Multiply(other.V), W.Multiply(other.W));
        public static UVW operator *(UVW self, UVW other) => self.Multiply(other);
        public UVW Divide(UVW other) => (U.Divide(other.U), V.Divide(other.V), W.Divide(other.W));
        public static UVW operator /(UVW self, UVW other) => self.Divide(other);
        public UVW Modulo(UVW other) => (U.Modulo(other.U), V.Modulo(other.V), W.Modulo(other.W));
        public static UVW operator %(UVW self, UVW other) => self.Modulo(other);
        public UVW Add(UVW other) => (U.Add(other.U), V.Add(other.V), W.Add(other.W));
        public static UVW operator +(UVW self, UVW other) => self.Add(other);
        public UVW Subtract(UVW other) => (U.Subtract(other.U), V.Subtract(other.V), W.Subtract(other.W));
        public static UVW operator -(UVW self, UVW other) => self.Subtract(other);
        public UVW Zero => (U.Zero, V.Zero, W.Zero);
        public UVW One => (U.One, V.One, W.One);
        public UVW MinValue => (U.MinValue, V.MinValue, W.MinValue);
        public UVW MaxValue => (U.MaxValue, V.MaxValue, W.MaxValue);
        public Boolean Equals(UVW b) => (U.Equals(b.U) & V.Equals(b.V) & W.Equals(b.W));
        public static Boolean operator ==(UVW a, UVW b) => a.Equals(b);
        public Boolean NotEquals(UVW b) => (U.NotEquals(b.U) & V.NotEquals(b.V) & W.NotEquals(b.W));
        public static Boolean operator !=(UVW a, UVW b) => a.NotEquals(b);
        public Boolean Between(UVW a, UVW b) => (U.Between(a.U, b.U) & V.Between(a.V, b.V) & W.Between(a.W, b.W));
        public UVW Clamp(UVW a, UVW b) => (U.Clamp(a.U, b.U), V.Clamp(a.V, b.V), W.Clamp(a.W, b.W));
    }
    public readonly partial struct Vector2D: Vector<Vector2D>
    {
        public readonly Number X;
        public readonly Number Y;
        public Vector2D WithX(Number x) => (x, Y);
        public Vector2D WithY(Number y) => (X, y);
        public Vector2D(Number x, Number y) => (X, Y) = (x, y);
        public static Vector2D Default = new Vector2D();
        public static Vector2D New(Number x, Number y) => new Vector2D(x, y);
        public Ara3D.DoublePrecision.Vector2D ChangePrecision() => (X.ChangePrecision(), Y.ChangePrecision());
        public static implicit operator Ara3D.DoublePrecision.Vector2D(Vector2D self) => self.ChangePrecision();
        public static implicit operator (Number, Number)(Vector2D self) => (self.X, self.Y);
        public static implicit operator Vector2D((Number, Number) value) => new Vector2D(value.Item1, value.Item2);
        public void Deconstruct(out Number x, out Number y) { x = X; y = Y; }
        public override bool Equals(object obj) { if (!(obj is Vector2D)) return false; var other = (Vector2D)obj; return X.Equals(other.X) && Y.Equals(other.Y); }
        public override int GetHashCode() => Intrinsics.CombineHashCodes(X, Y);
        public override string ToString() => Intrinsics.MakeString(TypeName, FieldNames, FieldValues);
        public static implicit operator Dynamic(Vector2D self) => new Dynamic(self);
        public static implicit operator Vector2D(Dynamic value) => value.As<Vector2D>();
        public String TypeName => "Vector2D";
        public Array<String> FieldNames => Intrinsics.MakeArray<String>((String)"X", (String)"Y");
        public Array<Dynamic> FieldValues => Intrinsics.MakeArray<Dynamic>(new Dynamic(X), new Dynamic(Y));
        // Unimplemented concept functions
        public Vector2D Add(Number other) => (X.Add(other), Y.Add(other));
        public static Vector2D operator +(Vector2D self, Number other) => self.Add(other);
        public Vector2D Subtract(Number other) => (X.Subtract(other), Y.Subtract(other));
        public static Vector2D operator -(Vector2D self, Number other) => self.Subtract(other);
        public Vector2D Multiply(Number other) => (X.Multiply(other), Y.Multiply(other));
        public static Vector2D operator *(Vector2D self, Number other) => self.Multiply(other);
        public Vector2D Divide(Number other) => (X.Divide(other), Y.Divide(other));
        public static Vector2D operator /(Vector2D self, Number other) => self.Divide(other);
        public Vector2D Modulo(Number other) => (X.Modulo(other), Y.Modulo(other));
        public static Vector2D operator %(Vector2D self, Number other) => self.Modulo(other);
        public Vector2D Reciprocal => (X.Reciprocal, Y.Reciprocal);
        public Vector2D Negative => (X.Negative, Y.Negative);
        public static Vector2D operator -(Vector2D self) => self.Negative;
        public Vector2D Multiply(Vector2D other) => (X.Multiply(other.X), Y.Multiply(other.Y));
        public static Vector2D operator *(Vector2D self, Vector2D other) => self.Multiply(other);
        public Vector2D Divide(Vector2D other) => (X.Divide(other.X), Y.Divide(other.Y));
        public static Vector2D operator /(Vector2D self, Vector2D other) => self.Divide(other);
        public Vector2D Modulo(Vector2D other) => (X.Modulo(other.X), Y.Modulo(other.Y));
        public static Vector2D operator %(Vector2D self, Vector2D other) => self.Modulo(other);
        public Vector2D Add(Vector2D other) => (X.Add(other.X), Y.Add(other.Y));
        public static Vector2D operator +(Vector2D self, Vector2D other) => self.Add(other);
        public Vector2D Subtract(Vector2D other) => (X.Subtract(other.X), Y.Subtract(other.Y));
        public static Vector2D operator -(Vector2D self, Vector2D other) => self.Subtract(other);
        public Vector2D Zero => (X.Zero, Y.Zero);
        public Vector2D One => (X.One, Y.One);
        public Vector2D MinValue => (X.MinValue, Y.MinValue);
        public Vector2D MaxValue => (X.MaxValue, Y.MaxValue);
        public Boolean Equals(Vector2D b) => (X.Equals(b.X) & Y.Equals(b.Y));
        public static Boolean operator ==(Vector2D a, Vector2D b) => a.Equals(b);
        public Boolean NotEquals(Vector2D b) => (X.NotEquals(b.X) & Y.NotEquals(b.Y));
        public static Boolean operator !=(Vector2D a, Vector2D b) => a.NotEquals(b);
        public Boolean Between(Vector2D a, Vector2D b) => (X.Between(a.X, b.X) & Y.Between(a.Y, b.Y));
        public Vector2D Clamp(Vector2D a, Vector2D b) => (X.Clamp(a.X, b.X), Y.Clamp(a.Y, b.Y));
    }
    public readonly partial struct Vector3D: Vector<Vector3D>
    {
        public readonly Number X;
        public readonly Number Y;
        public readonly Number Z;
        public Vector3D WithX(Number x) => (x, Y, Z);
        public Vector3D WithY(Number y) => (X, y, Z);
        public Vector3D WithZ(Number z) => (X, Y, z);
        public Vector3D(Number x, Number y, Number z) => (X, Y, Z) = (x, y, z);
        public static Vector3D Default = new Vector3D();
        public static Vector3D New(Number x, Number y, Number z) => new Vector3D(x, y, z);
        public Ara3D.DoublePrecision.Vector3D ChangePrecision() => (X.ChangePrecision(), Y.ChangePrecision(), Z.ChangePrecision());
        public static implicit operator Ara3D.DoublePrecision.Vector3D(Vector3D self) => self.ChangePrecision();
        public static implicit operator (Number, Number, Number)(Vector3D self) => (self.X, self.Y, self.Z);
        public static implicit operator Vector3D((Number, Number, Number) value) => new Vector3D(value.Item1, value.Item2, value.Item3);
        public void Deconstruct(out Number x, out Number y, out Number z) { x = X; y = Y; z = Z; }
        public override bool Equals(object obj) { if (!(obj is Vector3D)) return false; var other = (Vector3D)obj; return X.Equals(other.X) && Y.Equals(other.Y) && Z.Equals(other.Z); }
        public override int GetHashCode() => Intrinsics.CombineHashCodes(X, Y, Z);
        public override string ToString() => Intrinsics.MakeString(TypeName, FieldNames, FieldValues);
        public static implicit operator Dynamic(Vector3D self) => new Dynamic(self);
        public static implicit operator Vector3D(Dynamic value) => value.As<Vector3D>();
        public String TypeName => "Vector3D";
        public Array<String> FieldNames => Intrinsics.MakeArray<String>((String)"X", (String)"Y", (String)"Z");
        public Array<Dynamic> FieldValues => Intrinsics.MakeArray<Dynamic>(new Dynamic(X), new Dynamic(Y), new Dynamic(Z));
        // Unimplemented concept functions
        public Vector3D Add(Number other) => (X.Add(other), Y.Add(other), Z.Add(other));
        public static Vector3D operator +(Vector3D self, Number other) => self.Add(other);
        public Vector3D Subtract(Number other) => (X.Subtract(other), Y.Subtract(other), Z.Subtract(other));
        public static Vector3D operator -(Vector3D self, Number other) => self.Subtract(other);
        public Vector3D Multiply(Number other) => (X.Multiply(other), Y.Multiply(other), Z.Multiply(other));
        public static Vector3D operator *(Vector3D self, Number other) => self.Multiply(other);
        public Vector3D Divide(Number other) => (X.Divide(other), Y.Divide(other), Z.Divide(other));
        public static Vector3D operator /(Vector3D self, Number other) => self.Divide(other);
        public Vector3D Modulo(Number other) => (X.Modulo(other), Y.Modulo(other), Z.Modulo(other));
        public static Vector3D operator %(Vector3D self, Number other) => self.Modulo(other);
        public Vector3D Reciprocal => (X.Reciprocal, Y.Reciprocal, Z.Reciprocal);
        public Vector3D Negative => (X.Negative, Y.Negative, Z.Negative);
        public static Vector3D operator -(Vector3D self) => self.Negative;
        public Vector3D Multiply(Vector3D other) => (X.Multiply(other.X), Y.Multiply(other.Y), Z.Multiply(other.Z));
        public static Vector3D operator *(Vector3D self, Vector3D other) => self.Multiply(other);
        public Vector3D Divide(Vector3D other) => (X.Divide(other.X), Y.Divide(other.Y), Z.Divide(other.Z));
        public static Vector3D operator /(Vector3D self, Vector3D other) => self.Divide(other);
        public Vector3D Modulo(Vector3D other) => (X.Modulo(other.X), Y.Modulo(other.Y), Z.Modulo(other.Z));
        public static Vector3D operator %(Vector3D self, Vector3D other) => self.Modulo(other);
        public Vector3D Add(Vector3D other) => (X.Add(other.X), Y.Add(other.Y), Z.Add(other.Z));
        public static Vector3D operator +(Vector3D self, Vector3D other) => self.Add(other);
        public Vector3D Subtract(Vector3D other) => (X.Subtract(other.X), Y.Subtract(other.Y), Z.Subtract(other.Z));
        public static Vector3D operator -(Vector3D self, Vector3D other) => self.Subtract(other);
        public Vector3D Zero => (X.Zero, Y.Zero, Z.Zero);
        public Vector3D One => (X.One, Y.One, Z.One);
        public Vector3D MinValue => (X.MinValue, Y.MinValue, Z.MinValue);
        public Vector3D MaxValue => (X.MaxValue, Y.MaxValue, Z.MaxValue);
        public Boolean Equals(Vector3D b) => (X.Equals(b.X) & Y.Equals(b.Y) & Z.Equals(b.Z));
        public static Boolean operator ==(Vector3D a, Vector3D b) => a.Equals(b);
        public Boolean NotEquals(Vector3D b) => (X.NotEquals(b.X) & Y.NotEquals(b.Y) & Z.NotEquals(b.Z));
        public static Boolean operator !=(Vector3D a, Vector3D b) => a.NotEquals(b);
        public Boolean Between(Vector3D a, Vector3D b) => (X.Between(a.X, b.X) & Y.Between(a.Y, b.Y) & Z.Between(a.Z, b.Z));
        public Vector3D Clamp(Vector3D a, Vector3D b) => (X.Clamp(a.X, b.X), Y.Clamp(a.Y, b.Y), Z.Clamp(a.Z, b.Z));
    }
    public readonly partial struct Vector4D: Vector<Vector4D>
    {
        public readonly Number X;
        public readonly Number Y;
        public readonly Number Z;
        public readonly Number W;
        public Vector4D WithX(Number x) => (x, Y, Z, W);
        public Vector4D WithY(Number y) => (X, y, Z, W);
        public Vector4D WithZ(Number z) => (X, Y, z, W);
        public Vector4D WithW(Number w) => (X, Y, Z, w);
        public Vector4D(Number x, Number y, Number z, Number w) => (X, Y, Z, W) = (x, y, z, w);
        public static Vector4D Default = new Vector4D();
        public static Vector4D New(Number x, Number y, Number z, Number w) => new Vector4D(x, y, z, w);
        public Ara3D.DoublePrecision.Vector4D ChangePrecision() => (X.ChangePrecision(), Y.ChangePrecision(), Z.ChangePrecision(), W.ChangePrecision());
        public static implicit operator Ara3D.DoublePrecision.Vector4D(Vector4D self) => self.ChangePrecision();
        public static implicit operator (Number, Number, Number, Number)(Vector4D self) => (self.X, self.Y, self.Z, self.W);
        public static implicit operator Vector4D((Number, Number, Number, Number) value) => new Vector4D(value.Item1, value.Item2, value.Item3, value.Item4);
        public void Deconstruct(out Number x, out Number y, out Number z, out Number w) { x = X; y = Y; z = Z; w = W; }
        public override bool Equals(object obj) { if (!(obj is Vector4D)) return false; var other = (Vector4D)obj; return X.Equals(other.X) && Y.Equals(other.Y) && Z.Equals(other.Z) && W.Equals(other.W); }
        public override int GetHashCode() => Intrinsics.CombineHashCodes(X, Y, Z, W);
        public override string ToString() => Intrinsics.MakeString(TypeName, FieldNames, FieldValues);
        public static implicit operator Dynamic(Vector4D self) => new Dynamic(self);
        public static implicit operator Vector4D(Dynamic value) => value.As<Vector4D>();
        public String TypeName => "Vector4D";
        public Array<String> FieldNames => Intrinsics.MakeArray<String>((String)"X", (String)"Y", (String)"Z", (String)"W");
        public Array<Dynamic> FieldValues => Intrinsics.MakeArray<Dynamic>(new Dynamic(X), new Dynamic(Y), new Dynamic(Z), new Dynamic(W));
        // Unimplemented concept functions
        public Vector4D Add(Number other) => (X.Add(other), Y.Add(other), Z.Add(other), W.Add(other));
        public static Vector4D operator +(Vector4D self, Number other) => self.Add(other);
        public Vector4D Subtract(Number other) => (X.Subtract(other), Y.Subtract(other), Z.Subtract(other), W.Subtract(other));
        public static Vector4D operator -(Vector4D self, Number other) => self.Subtract(other);
        public Vector4D Multiply(Number other) => (X.Multiply(other), Y.Multiply(other), Z.Multiply(other), W.Multiply(other));
        public static Vector4D operator *(Vector4D self, Number other) => self.Multiply(other);
        public Vector4D Divide(Number other) => (X.Divide(other), Y.Divide(other), Z.Divide(other), W.Divide(other));
        public static Vector4D operator /(Vector4D self, Number other) => self.Divide(other);
        public Vector4D Modulo(Number other) => (X.Modulo(other), Y.Modulo(other), Z.Modulo(other), W.Modulo(other));
        public static Vector4D operator %(Vector4D self, Number other) => self.Modulo(other);
        public Vector4D Reciprocal => (X.Reciprocal, Y.Reciprocal, Z.Reciprocal, W.Reciprocal);
        public Vector4D Negative => (X.Negative, Y.Negative, Z.Negative, W.Negative);
        public static Vector4D operator -(Vector4D self) => self.Negative;
        public Vector4D Multiply(Vector4D other) => (X.Multiply(other.X), Y.Multiply(other.Y), Z.Multiply(other.Z), W.Multiply(other.W));
        public static Vector4D operator *(Vector4D self, Vector4D other) => self.Multiply(other);
        public Vector4D Divide(Vector4D other) => (X.Divide(other.X), Y.Divide(other.Y), Z.Divide(other.Z), W.Divide(other.W));
        public static Vector4D operator /(Vector4D self, Vector4D other) => self.Divide(other);
        public Vector4D Modulo(Vector4D other) => (X.Modulo(other.X), Y.Modulo(other.Y), Z.Modulo(other.Z), W.Modulo(other.W));
        public static Vector4D operator %(Vector4D self, Vector4D other) => self.Modulo(other);
        public Vector4D Add(Vector4D other) => (X.Add(other.X), Y.Add(other.Y), Z.Add(other.Z), W.Add(other.W));
        public static Vector4D operator +(Vector4D self, Vector4D other) => self.Add(other);
        public Vector4D Subtract(Vector4D other) => (X.Subtract(other.X), Y.Subtract(other.Y), Z.Subtract(other.Z), W.Subtract(other.W));
        public static Vector4D operator -(Vector4D self, Vector4D other) => self.Subtract(other);
        public Vector4D Zero => (X.Zero, Y.Zero, Z.Zero, W.Zero);
        public Vector4D One => (X.One, Y.One, Z.One, W.One);
        public Vector4D MinValue => (X.MinValue, Y.MinValue, Z.MinValue, W.MinValue);
        public Vector4D MaxValue => (X.MaxValue, Y.MaxValue, Z.MaxValue, W.MaxValue);
        public Boolean Equals(Vector4D b) => (X.Equals(b.X) & Y.Equals(b.Y) & Z.Equals(b.Z) & W.Equals(b.W));
        public static Boolean operator ==(Vector4D a, Vector4D b) => a.Equals(b);
        public Boolean NotEquals(Vector4D b) => (X.NotEquals(b.X) & Y.NotEquals(b.Y) & Z.NotEquals(b.Z) & W.NotEquals(b.W));
        public static Boolean operator !=(Vector4D a, Vector4D b) => a.NotEquals(b);
        public Boolean Between(Vector4D a, Vector4D b) => (X.Between(a.X, b.X) & Y.Between(a.Y, b.Y) & Z.Between(a.Z, b.Z) & W.Between(a.W, b.W));
        public Vector4D Clamp(Vector4D a, Vector4D b) => (X.Clamp(a.X, b.X), Y.Clamp(a.Y, b.Y), Z.Clamp(a.Z, b.Z), W.Clamp(a.W, b.W));
    }
    public static class Constants
    {
        public static Number Pi => ((Number)3.1415926535897);
        public static Number TwoPi => Constants.Pi.Twice;
        public static Number HalfPi => Constants.Pi.Half;
        public static Number Epsilon => ((Number)1E-15);
        public static Number FeetPerMeter => ((Number)3.280839895);
        public static Number FeetPerMile => ((Integer)5280);
        public static Number MetersPerLightyear => ((Number)9460730472580000);
        public static Number MetersPerAU => ((Number)149597870691);
        public static Number DaltonPerKilogram => ((Number)1.66053E-27);
        public static Number PoundPerKilogram => ((Number)0.45359237);
        public static Number PoundPerTon => ((Integer)2000);
        public static Number KilogramPerSolarMass => ((Number)1.9889200011446E+30);
        public static Number JulianYearSeconds => ((Integer)31557600);
        public static Number GregorianYearDays => ((Number)365.2425);
    }
    public readonly partial struct Point2D
    {
        public static implicit operator Point3D(Point2D x)  => x.X.Tuple3(x.Y, ((Number)0));
        public static implicit operator Vector3D(Point2D x)  => x.X.Tuple3(x.Y, ((Number)0));
        public static implicit operator Vector2D(Point2D x)  => x.X.Tuple2(x.Y);
        public Point3D Deform(System.Func<Vector3D, Vector3D> f) => f(this);
        public Vector2D ToVector => this;
        public Point2D Add(Vector2D b) => this.ToVector.Add(b);
        public static Point2D operator +(Point2D a, Vector2D b) => a.Add(b);
        public Point2D Subtract(Vector2D b) => this.ToVector.Subtract(b);
        public static Point2D operator -(Point2D a, Vector2D b) => a.Subtract(b);
        public Vector2D Subtract(Point2D b) => this.ToVector.Subtract(b.ToVector);
        public static Vector2D operator -(Point2D a, Point2D b) => a.Subtract(b);
    }
    public readonly partial struct Transform2D
    {
    }
    public readonly partial struct Pose2D
    {
    }
    public readonly partial struct Bounds2D
    {
        public Vector2D Size => this.Max.Subtract(this.Min);
        public Point2D Lerp(Number amount) => this.Min.Lerp(this.Max, amount);
        public Bounds2D Reverse => this.Max.Tuple2(this.Min);
        public Point2D Center => this.Lerp(((Number)0.5));
        public Boolean Contains(Point2D value) => value.Between(this.Min, this.Max);
        public Boolean Contains(Bounds2D y) => this.Contains(y.Min).And(this.Contains(y.Max));
        public Boolean Overlaps(Bounds2D y) => this.Contains(y.Min).Or(this.Contains(y.Max).Or(y.Contains(this.Min).Or(y.Contains(this.Max))));
        public Tuple2<Bounds2D, Bounds2D> SplitAt(Number t) => this.Left(t).Tuple2(this.Right(t));
        public Tuple2<Bounds2D, Bounds2D> Split => this.SplitAt(((Number)0.5));
        public Bounds2D Left(Number t) => this.Min.Tuple2(this.Lerp(t));
        public Bounds2D Right(Number t) => this.Lerp(t).Tuple2(this.Max);
        public Bounds2D MoveTo(Point2D v) => v.Tuple2(v.Add(this.Size));
        public Bounds2D LeftHalf => this.Left(((Number)0.5));
        public Bounds2D RightHalf => this.Right(((Number)0.5));
        public Bounds2D Recenter(Point2D c) => c.Subtract(this.Size.Half).Tuple2(c.Add(this.Size.Half));
        public Bounds2D Clamp(Bounds2D y) => this.Clamp(y.Min).Tuple2(this.Clamp(y.Max));
        public Point2D Clamp(Point2D value) => value.Clamp(this.Min, this.Max);
    }
    public readonly partial struct Ray2D
    {
    }
    public readonly partial struct Sphere
    {
    }
    public readonly partial struct Plane
    {
    }
    public readonly partial struct Triangle2D
    {
        public Array<Point2D> Points => ((Integer)3).CirclePoints;
        public TAcc Reduce<TAcc>(TAcc init, System.Func<TAcc, Point2D, TAcc> f) => f(f(f(init, A), B), C);
        public Triangle2D Map(System.Func<Point2D, Point2D> f) => (f(A), f(B), f(C));
    }
    public readonly partial struct Quad2D
    {
        public TAcc Reduce<TAcc>(TAcc init, System.Func<TAcc, Point2D, TAcc> f) => f(f(f(f(init, A), B), C), D);
        public Quad2D Map(System.Func<Point2D, Point2D> f) => (f(A), f(B), f(C), f(D));
    }
    public readonly partial struct Line2D
    {
        public Boolean Closed => ((Boolean)false);
        public Array<Point2D> Points => this;
        public TAcc Reduce<TAcc>(TAcc init, System.Func<TAcc, Point2D, TAcc> f) => f(f(init, A), B);
        public Line2D Map(System.Func<Point2D, Point2D> f) => (f(A), f(B));
    }
    public readonly partial struct Circle
    {
        public Boolean Closed => ((Boolean)true);
    }
    public readonly partial struct Lens
    {
        public Boolean Closed => ((Boolean)true);
    }
    public readonly partial struct PolygonFace
    {
    }
    public readonly partial struct Rect2D
    {
        public Number Width => this.Size.Width;
        public Number Height => this.Size.Height;
        public Number HalfWidth => this.Width.Half;
        public Number HalfHeight => this.Height.Half;
        public Number Top => this.Center.Y.Add(HalfHeight);
        public Number Bottom => this.Top.Add(this.Height);
        public Number Left => this.Center.X.Subtract(HalfWidth);
        public Number Right => this.Left.Add(this.Width);
        public Point2D TopLeft => this.Left.Tuple2(this.Top);
        public Point2D TopRight => this.Right.Tuple2(this.Top);
        public Point2D BottomRight => this.Right.Tuple2(this.Bottom);
        public Point2D BottomLeft => this.Left.Tuple2(this.Bottom);
        public Array<Point2D> Points => Intrinsics.MakeArray(this.TopLeft, this.TopRight, this.BottomRight, this.BottomLeft);
        public Boolean Closed => ((Boolean)true);
    }
    public readonly partial struct Ellipse
    {
        public Boolean Closed => ((Boolean)true);
        public Point2D Eval(Number t) => t.Turns.CirclePoint.ToVector.Multiply(this.Size).Add(this.Center);
    }
    public readonly partial struct Ring
    {
        public Boolean Closed => ((Boolean)true);
    }
    public readonly partial struct Arc
    {
        public Boolean Closed => ((Boolean)false);
    }
    public readonly partial struct Sector
    {
        public Boolean Closed => ((Boolean)true);
    }
    public readonly partial struct Chord
    {
        public Boolean Closed => ((Boolean)true);
    }
    public readonly partial struct Segment
    {
        public Boolean Closed => ((Boolean)true);
    }
    public readonly partial struct RegularPolygon
    {
        public Array<Point2D> Points => this.NumPoints.CirclePoints;
        public Boolean Closed => ((Boolean)true);
    }
    public readonly partial struct Box2D
    {
    }
    public readonly partial struct Point3D
    {
        public static implicit operator Vector3D(Point3D x)  => x.X.Tuple3(x.Y, x.Z);
        public Point3D Deform(System.Func<Vector3D, Vector3D> f) => f(this);
        public Vector3D ToVector => this;
        public Point3D Add(Vector3D b) => this.ToVector.Add(b);
        public static Point3D operator +(Point3D a, Vector3D b) => a.Add(b);
        public Point3D Subtract(Vector3D b) => this.ToVector.Subtract(b);
        public static Point3D operator -(Point3D a, Vector3D b) => a.Subtract(b);
        public Vector3D Subtract(Point3D b) => this.ToVector.Subtract(b.ToVector);
        public static Vector3D operator -(Point3D a, Point3D b) => a.Subtract(b);
        public TAcc Reduce<TAcc>(TAcc init, System.Func<TAcc, Number, TAcc> f) => f(f(f(init, X), Y), Z);
        public Point3D Map(System.Func<Number, Number> f) => (f(X), f(Y), f(Z));
    }
    public readonly partial struct Transform3D
    {
    }
    public readonly partial struct Pose3D
    {
    }
    public readonly partial struct Frame3D
    {
    }
    public readonly partial struct Bounds3D
    {
        public Vector3D Size => this.Max.Subtract(this.Min);
        public Point3D Lerp(Number amount) => this.Min.Lerp(this.Max, amount);
        public Bounds3D Reverse => this.Max.Tuple2(this.Min);
        public Point3D Center => this.Lerp(((Number)0.5));
        public Boolean Contains(Point3D value) => value.Between(this.Min, this.Max);
        public Boolean Contains(Bounds3D y) => this.Contains(y.Min).And(this.Contains(y.Max));
        public Boolean Overlaps(Bounds3D y) => this.Contains(y.Min).Or(this.Contains(y.Max).Or(y.Contains(this.Min).Or(y.Contains(this.Max))));
        public Tuple2<Bounds3D, Bounds3D> SplitAt(Number t) => this.Left(t).Tuple2(this.Right(t));
        public Tuple2<Bounds3D, Bounds3D> Split => this.SplitAt(((Number)0.5));
        public Bounds3D Left(Number t) => this.Min.Tuple2(this.Lerp(t));
        public Bounds3D Right(Number t) => this.Lerp(t).Tuple2(this.Max);
        public Bounds3D MoveTo(Point3D v) => v.Tuple2(v.Add(this.Size));
        public Bounds3D LeftHalf => this.Left(((Number)0.5));
        public Bounds3D RightHalf => this.Right(((Number)0.5));
        public Bounds3D Recenter(Point3D c) => c.Subtract(this.Size.Half).Tuple2(c.Add(this.Size.Half));
        public Bounds3D Clamp(Bounds3D y) => this.Clamp(y.Min).Tuple2(this.Clamp(y.Max));
        public Point3D Clamp(Point3D value) => value.Clamp(this.Min, this.Max);
    }
    public readonly partial struct Line3D
    {
        public Boolean Closed => ((Boolean)false);
        public Array<Point3D> Points => this;
        public TAcc Reduce<TAcc>(TAcc init, System.Func<TAcc, Point3D, TAcc> f) => f(f(init, A), B);
        public Line3D Map(System.Func<Point3D, Point3D> f) => (f(A), f(B));
    }
    public readonly partial struct Ray3D
    {
    }
    public readonly partial struct Triangle3D
    {
        public TAcc Reduce<TAcc>(TAcc init, System.Func<TAcc, Point3D, TAcc> f) => f(f(f(init, A), B), C);
        public Triangle3D Map(System.Func<Point3D, Point3D> f) => (f(A), f(B), f(C));
    }
    public readonly partial struct Quad3D
    {
        public TAcc Reduce<TAcc>(TAcc init, System.Func<TAcc, Point3D, TAcc> f) => f(f(f(f(init, A), B), C), D);
        public Quad3D Map(System.Func<Point3D, Point3D> f) => (f(A), f(B), f(C), f(D));
    }
    public readonly partial struct Capsule
    {
    }
    public readonly partial struct Cylinder
    {
    }
    public readonly partial struct Cone
    {
    }
    public readonly partial struct Tube
    {
    }
    public readonly partial struct ConeSegment
    {
    }
    public readonly partial struct Box3D
    {
    }
    public readonly partial struct CubicBezier2D
    {
        public TAcc Reduce<TAcc>(TAcc init, System.Func<TAcc, Point2D, TAcc> f) => f(f(f(f(init, A), B), C), D);
        public CubicBezier2D Map(System.Func<Point2D, Point2D> f) => (f(A), f(B), f(C), f(D));
    }
    public readonly partial struct CubicBezier3D
    {
        public TAcc Reduce<TAcc>(TAcc init, System.Func<TAcc, Point3D, TAcc> f) => f(f(f(f(init, A), B), C), D);
        public CubicBezier3D Map(System.Func<Point3D, Point3D> f) => (f(A), f(B), f(C), f(D));
    }
    public readonly partial struct QuadraticBezier2D
    {
        public TAcc Reduce<TAcc>(TAcc init, System.Func<TAcc, Point2D, TAcc> f) => f(f(f(init, A), B), C);
        public QuadraticBezier2D Map(System.Func<Point2D, Point2D> f) => (f(A), f(B), f(C));
    }
    public readonly partial struct QuadraticBezier3D
    {
        public TAcc Reduce<TAcc>(TAcc init, System.Func<TAcc, Point3D, TAcc> f) => f(f(f(init, A), B), C);
        public QuadraticBezier3D Map(System.Func<Point3D, Point3D> f) => (f(A), f(B), f(C));
    }
    public readonly partial struct Quaternion
    {
        public Number Sum => this.Reduce(((Number)0), (a, b) => a.Add(b));
        public Number SumSquares => this.Square.Sum;
        public Number MagnitudeSquared => this.SumSquares;
        public Number Magnitude => this.MagnitudeSquared.SquareRoot;
        public Number Dot(Quaternion v2) => this.Multiply(v2).Sum;
        public Quaternion Normal => this.Divide(this.Magnitude);
        public Number Average => this.Sum.Divide(this.Count);
        public TAcc Reduce<TAcc>(TAcc init, System.Func<TAcc, Number, TAcc> f) => f(f(f(f(init, X), Y), Z), W);
        public Quaternion Map(System.Func<Number, Number> f) => (f(X), f(Y), f(Z), f(W));
        public Quaternion PlusOne => this.Add(this.One);
        public Quaternion MinusOne => this.Subtract(this.One);
        public Quaternion FromOne => this.One.Subtract(this);
        public Quaternion Pow2 => this.Multiply(this);
        public Quaternion Pow3 => this.Pow2.Multiply(this);
        public Quaternion Pow4 => this.Pow3.Multiply(this);
        public Quaternion Pow5 => this.Pow4.Multiply(this);
        public Quaternion Square => this.Pow2;
        public Quaternion Half => this.Divide(((Number)2));
        public Quaternion Quarter => this.Divide(((Number)4));
        public Quaternion Tenth => this.Divide(((Number)10));
        public Quaternion Twice => this.Multiply(((Number)2));
        public Quaternion Lerp(Quaternion b, Number t) => this.Multiply(t.FromOne).Add(b.Multiply(t));
    }
    public readonly partial struct AxisAngle
    {
    }
    public readonly partial struct EulerAngles
    {
    }
    public readonly partial struct Rotation3D
    {
    }
    public readonly partial struct Orientation3D
    {
    }
    public readonly partial struct Point4D
    {
        public static implicit operator Vector4D(Point4D x)  => x.X.Tuple4(x.Y, x.Z, x.W);
        public Vector4D ToVector => this;
        public Point4D Add(Vector4D b) => this.ToVector.Add(b);
        public static Point4D operator +(Point4D a, Vector4D b) => a.Add(b);
        public Point4D Subtract(Vector4D b) => this.ToVector.Subtract(b);
        public static Point4D operator -(Point4D a, Vector4D b) => a.Subtract(b);
        public Vector4D Subtract(Point4D b) => this.ToVector.Subtract(b.ToVector);
        public static Vector4D operator -(Point4D a, Point4D b) => a.Subtract(b);
        public TAcc Reduce<TAcc>(TAcc init, System.Func<TAcc, Number, TAcc> f) => f(f(f(f(init, X), Y), Z), W);
        public Point4D Map(System.Func<Number, Number> f) => (f(X), f(Y), f(Z), f(W));
    }
    public readonly partial struct Line4D
    {
        public TAcc Reduce<TAcc>(TAcc init, System.Func<TAcc, Point4D, TAcc> f) => f(f(init, A), B);
        public Line4D Map(System.Func<Point4D, Point4D> f) => (f(A), f(B));
    }
    public readonly partial struct Quadray
    {
        public Number Sum => this.Reduce(((Number)0), (a, b) => a.Add(b));
        public Number SumSquares => this.Square.Sum;
        public Number MagnitudeSquared => this.SumSquares;
        public Number Magnitude => this.MagnitudeSquared.SquareRoot;
        public Number Dot(Quadray v2) => this.Multiply(v2).Sum;
        public Quadray Normal => this.Divide(this.Magnitude);
        public Number Average => this.Sum.Divide(this.Count);
        public TAcc Reduce<TAcc>(TAcc init, System.Func<TAcc, Number, TAcc> f) => f(f(f(f(init, X), Y), Z), W);
        public Quadray Map(System.Func<Number, Number> f) => (f(X), f(Y), f(Z), f(W));
        public Quadray PlusOne => this.Add(this.One);
        public Quadray MinusOne => this.Subtract(this.One);
        public Quadray FromOne => this.One.Subtract(this);
        public Quadray Pow2 => this.Multiply(this);
        public Quadray Pow3 => this.Pow2.Multiply(this);
        public Quadray Pow4 => this.Pow3.Multiply(this);
        public Quadray Pow5 => this.Pow4.Multiply(this);
        public Quadray Square => this.Pow2;
        public Quadray Half => this.Divide(((Number)2));
        public Quadray Quarter => this.Divide(((Number)4));
        public Quadray Tenth => this.Divide(((Number)10));
        public Quadray Twice => this.Multiply(((Number)2));
        public Quadray Lerp(Quadray b, Number t) => this.Multiply(t.FromOne).Add(b.Multiply(t));
    }
    public readonly partial struct Number
    {
        public Angle Turns => this;
        public Angle Acos => Intrinsics.Acos(this);
        public Angle Asin => Intrinsics.Asin(this);
        public Angle Atan => Intrinsics.Atan(this);
        public Number Pow(Number y) => Intrinsics.Pow(this, y);
        public Number Log(Number y) => Intrinsics.Log(this, y);
        public Number Ln => Intrinsics.Ln(this);
        public Number Exp => Intrinsics.Exp(this);
        public Number Add(Number y) => Intrinsics.Add(this, y);
        public static Number operator +(Number x, Number y) => x.Add(y);
        public Number Subtract(Number y) => Intrinsics.Subtract(this, y);
        public static Number operator -(Number x, Number y) => x.Subtract(y);
        public Number Divide(Number y) => Intrinsics.Divide(this, y);
        public static Number operator /(Number x, Number y) => x.Divide(y);
        public Number Multiply(Number y) => Intrinsics.Multiply(this, y);
        public static Number operator *(Number x, Number y) => x.Multiply(y);
        public Number Modulo(Number y) => Intrinsics.Modulo(this, y);
        public static Number operator %(Number x, Number y) => x.Modulo(y);
        public Number Negative => Intrinsics.Negative(this);
        public static Number operator -(Number x) => x.Negative;
        public Number Hundred => this.Multiply(((Integer)100));
        public Number Thousand => this.Multiply(((Integer)1000));
        public Number Million => this.Thousand.Thousand;
        public Number Billion => this.Thousand.Million;
        public Number OunceToGram => this.Multiply(((Number)28.349523125));
        public Number TroyOunceToGram => this.Multiply(((Number)31.1034768));
        public Number GrainToMilligram => this.Multiply(((Number)64.79891));
        public Number Mole => this.Multiply(((Number)6.02214076E+23));
        public Number Inverse => ((Number)1).Divide(this);
        public Number Reciprocal => this.Inverse;
        public Number SquareRoot => this.Pow(((Number)0.5));
        public Number Sqrt => this.SquareRoot;
        public Number SmoothStep => this.Square.Multiply(((Number)3).Subtract(this.Twice));
        public Number MultiplyEpsilon(Number y) => this.Abs.Greater(y.Abs).Multiply(Constants.Epsilon);
        public Boolean AlmostEqual(Number y) => this.Subtract(y).Abs.LessThanOrEquals(this.MultiplyEpsilon(y));
        public Boolean AlmostZero => this.Abs.LessThan(Constants.Epsilon);
        public Boolean AlmostZeroOrOne => this.AlmostEqual(((Integer)0)).Or(this.AlmostEqual(((Integer)1)));
        public Number InverseLerp(Number b, Number v) => v.Subtract(this).Divide(b.Subtract(this));
        public Number Remap(Number bIn, Number aOut, Number bOut, Number v) => aOut.Lerp(bOut, this.InverseLerp(aOut, v));
        public Number Magnitude => this.Value;
        public Number ClampOne => this.Clamp(this.Zero, this.One);
        public Boolean GtZ => this.GreaterThan(this.Zero);
        public Boolean LtZ => this.LessThan(this.Zero);
        public Boolean GtEqZ => this.GreaterThanOrEquals(this.Zero);
        public Boolean LtEqZ => this.LessThanOrEquals(this.Zero);
        public Boolean IsPositive => this.GtEqZ;
        public Boolean IsNegative => this.LtZ;
        public Integer Sign => this.LtZ ? ((Integer)1).Negative : this.GtZ ? ((Integer)1) : ((Integer)0);
        public Number Abs => this.LtZ ? this.Negative : this;
        public Number PlusOne => this.Add(this.One);
        public Number MinusOne => this.Subtract(this.One);
        public Number FromOne => this.One.Subtract(this);
        public Number Clamp(Number a, Number b) => this.Greater(a).Lesser(b);
        public Boolean Equals(Number b) => this.Compare(b).Equals(((Integer)0));
        public static Boolean operator ==(Number a, Number b) => a.Equals(b);
        public Boolean NotEquals(Number b) => this.Compare(b).NotEquals(((Integer)0));
        public static Boolean operator !=(Number a, Number b) => a.NotEquals(b);
        public Boolean LessThan(Number b) => this.Compare(b).LessThan(((Integer)0));
        public static Boolean operator <(Number a, Number b) => a.LessThan(b);
        public Boolean LessThanOrEquals(Number b) => this.Compare(b).LessThanOrEquals(((Integer)0));
        public static Boolean operator <=(Number a, Number b) => a.LessThanOrEquals(b);
        public Boolean GreaterThan(Number b) => this.Compare(b).GreaterThan(((Integer)0));
        public static Boolean operator >(Number a, Number b) => a.GreaterThan(b);
        public Boolean GreaterThanOrEquals(Number b) => this.Compare(b).GreaterThanOrEquals(((Integer)0));
        public static Boolean operator >=(Number a, Number b) => a.GreaterThanOrEquals(b);
        public Number Lesser(Number b) => this.LessThanOrEquals(b) ? this : b;
        public Number Greater(Number b) => this.GreaterThanOrEquals(b) ? this : b;
        public Boolean Between(Number min, Number max) => this.GreaterThanOrEquals(min).And(this.LessThanOrEquals(max));
        public Number Half => this.Divide(((Number)2));
        public Number Quarter => this.Divide(((Number)4));
        public Number Tenth => this.Divide(((Number)10));
        public Number Twice => this.Multiply(((Number)2));
        public Number Lerp(Number b, Number t) => this.Multiply(t.FromOne).Add(b.Multiply(t));
        public Number Pow2 => this.Multiply(this);
        public Number Pow3 => this.Pow2.Multiply(this);
        public Number Pow4 => this.Pow3.Multiply(this);
        public Number Pow5 => this.Pow4.Multiply(this);
        public Number Square => this.Pow2;
    }
    public readonly partial struct Integer
    {
        public Number FloatDivision(Integer y) => this.ToNumber.Divide(y.ToNumber);
        public Array<Number> Fractions
        {
            get
            {
                var _var0 = this;
                return this.Range.Map((i) => i.FloatDivision(_var0));
            }
        }
        public Array<Point2D> CirclePoints => this.Fractions.Map((x) => x.Turns.CirclePoint);
        public Integer Add(Integer y) => Intrinsics.Add(this, y);
        public static Integer operator +(Integer x, Integer y) => x.Add(y);
        public Integer Subtract(Integer y) => Intrinsics.Subtract(this, y);
        public static Integer operator -(Integer x, Integer y) => x.Subtract(y);
        public Integer Divide(Integer y) => Intrinsics.Divide(this, y);
        public static Integer operator /(Integer x, Integer y) => x.Divide(y);
        public Integer Multiply(Integer y) => Intrinsics.Multiply(this, y);
        public static Integer operator *(Integer x, Integer y) => x.Multiply(y);
        public Integer Modulo(Integer y) => Intrinsics.Modulo(this, y);
        public static Integer operator %(Integer x, Integer y) => x.Modulo(y);
        public Integer Negative => Intrinsics.Negative(this);
        public static Integer operator -(Integer x) => x.Negative;
        public Number ToNumber => Intrinsics.ToNumber(this);
        public Array<Integer> Range => Intrinsics.Range(this);
        public Integer PlusOne => this.Add(this.One);
        public Integer MinusOne => this.Subtract(this.One);
        public Integer FromOne => this.One.Subtract(this);
        public Integer Clamp(Integer a, Integer b) => this.Greater(a).Lesser(b);
        public Boolean Equals(Integer b) => this.Compare(b).Equals(((Integer)0));
        public static Boolean operator ==(Integer a, Integer b) => a.Equals(b);
        public Boolean NotEquals(Integer b) => this.Compare(b).NotEquals(((Integer)0));
        public static Boolean operator !=(Integer a, Integer b) => a.NotEquals(b);
        public Boolean LessThan(Integer b) => this.Compare(b).LessThan(((Integer)0));
        public static Boolean operator <(Integer a, Integer b) => a.LessThan(b);
        public Boolean LessThanOrEquals(Integer b) => this.Compare(b).LessThanOrEquals(((Integer)0));
        public static Boolean operator <=(Integer a, Integer b) => a.LessThanOrEquals(b);
        public Boolean GreaterThan(Integer b) => this.Compare(b).GreaterThan(((Integer)0));
        public static Boolean operator >(Integer a, Integer b) => a.GreaterThan(b);
        public Boolean GreaterThanOrEquals(Integer b) => this.Compare(b).GreaterThanOrEquals(((Integer)0));
        public static Boolean operator >=(Integer a, Integer b) => a.GreaterThanOrEquals(b);
        public Integer Lesser(Integer b) => this.LessThanOrEquals(b) ? this : b;
        public Integer Greater(Integer b) => this.GreaterThanOrEquals(b) ? this : b;
        public Boolean Between(Integer min, Integer max) => this.GreaterThanOrEquals(min).And(this.LessThanOrEquals(max));
    }
    public readonly partial struct String
    {
        public String Clamp(String a, String b) => this.Greater(a).Lesser(b);
        public Boolean Equals(String b) => this.Compare(b).Equals(((Integer)0));
        public static Boolean operator ==(String a, String b) => a.Equals(b);
        public Boolean NotEquals(String b) => this.Compare(b).NotEquals(((Integer)0));
        public static Boolean operator !=(String a, String b) => a.NotEquals(b);
        public Boolean LessThan(String b) => this.Compare(b).LessThan(((Integer)0));
        public static Boolean operator <(String a, String b) => a.LessThan(b);
        public Boolean LessThanOrEquals(String b) => this.Compare(b).LessThanOrEquals(((Integer)0));
        public static Boolean operator <=(String a, String b) => a.LessThanOrEquals(b);
        public Boolean GreaterThan(String b) => this.Compare(b).GreaterThan(((Integer)0));
        public static Boolean operator >(String a, String b) => a.GreaterThan(b);
        public Boolean GreaterThanOrEquals(String b) => this.Compare(b).GreaterThanOrEquals(((Integer)0));
        public static Boolean operator >=(String a, String b) => a.GreaterThanOrEquals(b);
        public String Lesser(String b) => this.LessThanOrEquals(b) ? this : b;
        public String Greater(String b) => this.GreaterThanOrEquals(b) ? this : b;
        public Boolean Between(String min, String max) => this.GreaterThanOrEquals(min).And(this.LessThanOrEquals(max));
    }
    public readonly partial struct Boolean
    {
        public Boolean And(Boolean y) => Intrinsics.And(this, y);
        public static Boolean operator &(Boolean x, Boolean y) => x.And(y);
        public Boolean Or(Boolean y) => Intrinsics.Or(this, y);
        public static Boolean operator |(Boolean x, Boolean y) => x.Or(y);
        public Boolean Not => Intrinsics.Not(this);
        public static Boolean operator !(Boolean x) => x.Not;
    }
    public readonly partial struct Character
    {
    }
    public readonly partial struct Dynamic
    {
    }
    public readonly partial struct Type
    {
    }
    public readonly partial struct Error
    {
    }
    public readonly partial struct Tuple2<T0, T1>
    {
    }
    public readonly partial struct Tuple3<T0, T1, T2>
    {
    }
    public readonly partial struct Tuple4<T0, T1, T2, T3>
    {
    }
    public readonly partial struct Tuple5<T0, T1, T2, T3, T4>
    {
    }
    public readonly partial struct Tuple6<T0, T1, T2, T3, T4, T5>
    {
    }
    public readonly partial struct Tuple7<T0, T1, T2, T3, T4, T5, T6>
    {
    }
    public readonly partial struct Tuple8<T0, T1, T2, T3, T4, T5, T6, T7>
    {
    }
    public readonly partial struct Tuple9<T0, T1, T2, T3, T4, T5, T6, T7, T8>
    {
    }
    public readonly partial struct Tuple10<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>
    {
    }
    public readonly partial struct Function0<TR>
    {
    }
    public readonly partial struct Function1<T0, TR>
    {
    }
    public readonly partial struct Function2<T0, T1, TR>
    {
    }
    public readonly partial struct Function3<T0, T1, T2, TR>
    {
    }
    public readonly partial struct Function4<T0, T1, T2, T3, TR>
    {
    }
    public readonly partial struct Function5<T0, T1, T2, T3, T4, TR>
    {
    }
    public readonly partial struct Function6<T0, T1, T2, T3, T4, T5, TR>
    {
    }
    public readonly partial struct Function7<T0, T1, T2, T3, T4, T5, T6, TR>
    {
    }
    public readonly partial struct Function8<T0, T1, T2, T3, T4, T5, T6, T7, TR>
    {
    }
    public readonly partial struct Function9<T0, T1, T2, T3, T4, T5, T6, T7, T8, TR>
    {
    }
    public readonly partial struct Function10<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, TR>
    {
    }
    public readonly partial struct Count
    {
        public Count PlusOne => this.Add(this.One);
        public Count MinusOne => this.Subtract(this.One);
        public Count FromOne => this.One.Subtract(this);
        public Count Clamp(Count a, Count b) => this.Greater(a).Lesser(b);
        public Boolean Equals(Count b) => this.Compare(b).Equals(((Integer)0));
        public static Boolean operator ==(Count a, Count b) => a.Equals(b);
        public Boolean NotEquals(Count b) => this.Compare(b).NotEquals(((Integer)0));
        public static Boolean operator !=(Count a, Count b) => a.NotEquals(b);
        public Boolean LessThan(Count b) => this.Compare(b).LessThan(((Integer)0));
        public static Boolean operator <(Count a, Count b) => a.LessThan(b);
        public Boolean LessThanOrEquals(Count b) => this.Compare(b).LessThanOrEquals(((Integer)0));
        public static Boolean operator <=(Count a, Count b) => a.LessThanOrEquals(b);
        public Boolean GreaterThan(Count b) => this.Compare(b).GreaterThan(((Integer)0));
        public static Boolean operator >(Count a, Count b) => a.GreaterThan(b);
        public Boolean GreaterThanOrEquals(Count b) => this.Compare(b).GreaterThanOrEquals(((Integer)0));
        public static Boolean operator >=(Count a, Count b) => a.GreaterThanOrEquals(b);
        public Count Lesser(Count b) => this.LessThanOrEquals(b) ? this : b;
        public Count Greater(Count b) => this.GreaterThanOrEquals(b) ? this : b;
        public Boolean Between(Count min, Count max) => this.GreaterThanOrEquals(min).And(this.LessThanOrEquals(max));
    }
    public readonly partial struct Cardinal
    {
        public Cardinal PlusOne => this.Add(this.One);
        public Cardinal MinusOne => this.Subtract(this.One);
        public Cardinal FromOne => this.One.Subtract(this);
        public Cardinal Clamp(Cardinal a, Cardinal b) => this.Greater(a).Lesser(b);
        public Boolean Equals(Cardinal b) => this.Compare(b).Equals(((Integer)0));
        public static Boolean operator ==(Cardinal a, Cardinal b) => a.Equals(b);
        public Boolean NotEquals(Cardinal b) => this.Compare(b).NotEquals(((Integer)0));
        public static Boolean operator !=(Cardinal a, Cardinal b) => a.NotEquals(b);
        public Boolean LessThan(Cardinal b) => this.Compare(b).LessThan(((Integer)0));
        public static Boolean operator <(Cardinal a, Cardinal b) => a.LessThan(b);
        public Boolean LessThanOrEquals(Cardinal b) => this.Compare(b).LessThanOrEquals(((Integer)0));
        public static Boolean operator <=(Cardinal a, Cardinal b) => a.LessThanOrEquals(b);
        public Boolean GreaterThan(Cardinal b) => this.Compare(b).GreaterThan(((Integer)0));
        public static Boolean operator >(Cardinal a, Cardinal b) => a.GreaterThan(b);
        public Boolean GreaterThanOrEquals(Cardinal b) => this.Compare(b).GreaterThanOrEquals(((Integer)0));
        public static Boolean operator >=(Cardinal a, Cardinal b) => a.GreaterThanOrEquals(b);
        public Cardinal Lesser(Cardinal b) => this.LessThanOrEquals(b) ? this : b;
        public Cardinal Greater(Cardinal b) => this.GreaterThanOrEquals(b) ? this : b;
        public Boolean Between(Cardinal min, Cardinal max) => this.GreaterThanOrEquals(min).And(this.LessThanOrEquals(max));
    }
    public readonly partial struct Index
    {
        public Index PlusOne => this.Add(this.One);
        public Index MinusOne => this.Subtract(this.One);
        public Index FromOne => this.One.Subtract(this);
        public Index Clamp(Index a, Index b) => this.Greater(a).Lesser(b);
        public Boolean Equals(Index b) => this.Compare(b).Equals(((Integer)0));
        public static Boolean operator ==(Index a, Index b) => a.Equals(b);
        public Boolean NotEquals(Index b) => this.Compare(b).NotEquals(((Integer)0));
        public static Boolean operator !=(Index a, Index b) => a.NotEquals(b);
        public Boolean LessThan(Index b) => this.Compare(b).LessThan(((Integer)0));
        public static Boolean operator <(Index a, Index b) => a.LessThan(b);
        public Boolean LessThanOrEquals(Index b) => this.Compare(b).LessThanOrEquals(((Integer)0));
        public static Boolean operator <=(Index a, Index b) => a.LessThanOrEquals(b);
        public Boolean GreaterThan(Index b) => this.Compare(b).GreaterThan(((Integer)0));
        public static Boolean operator >(Index a, Index b) => a.GreaterThan(b);
        public Boolean GreaterThanOrEquals(Index b) => this.Compare(b).GreaterThanOrEquals(((Integer)0));
        public static Boolean operator >=(Index a, Index b) => a.GreaterThanOrEquals(b);
        public Index Lesser(Index b) => this.LessThanOrEquals(b) ? this : b;
        public Index Greater(Index b) => this.GreaterThanOrEquals(b) ? this : b;
        public Boolean Between(Index min, Index max) => this.GreaterThanOrEquals(min).And(this.LessThanOrEquals(max));
    }
    public readonly partial struct Unit
    {
        public Number Magnitude => this.Value;
        public Unit ClampOne => this.Clamp(this.Zero, this.One);
        public Boolean GtZ => this.GreaterThan(this.Zero);
        public Boolean LtZ => this.LessThan(this.Zero);
        public Boolean GtEqZ => this.GreaterThanOrEquals(this.Zero);
        public Boolean LtEqZ => this.LessThanOrEquals(this.Zero);
        public Boolean IsPositive => this.GtEqZ;
        public Boolean IsNegative => this.LtZ;
        public Integer Sign => this.LtZ ? ((Integer)1).Negative : this.GtZ ? ((Integer)1) : ((Integer)0);
        public Unit Abs => this.LtZ ? this.Negative : this;
        public Unit PlusOne => this.Add(this.One);
        public Unit MinusOne => this.Subtract(this.One);
        public Unit FromOne => this.One.Subtract(this);
        public Unit Clamp(Unit a, Unit b) => this.Greater(a).Lesser(b);
        public Boolean Equals(Unit b) => this.Compare(b).Equals(((Integer)0));
        public static Boolean operator ==(Unit a, Unit b) => a.Equals(b);
        public Boolean NotEquals(Unit b) => this.Compare(b).NotEquals(((Integer)0));
        public static Boolean operator !=(Unit a, Unit b) => a.NotEquals(b);
        public Boolean LessThan(Unit b) => this.Compare(b).LessThan(((Integer)0));
        public static Boolean operator <(Unit a, Unit b) => a.LessThan(b);
        public Boolean LessThanOrEquals(Unit b) => this.Compare(b).LessThanOrEquals(((Integer)0));
        public static Boolean operator <=(Unit a, Unit b) => a.LessThanOrEquals(b);
        public Boolean GreaterThan(Unit b) => this.Compare(b).GreaterThan(((Integer)0));
        public static Boolean operator >(Unit a, Unit b) => a.GreaterThan(b);
        public Boolean GreaterThanOrEquals(Unit b) => this.Compare(b).GreaterThanOrEquals(((Integer)0));
        public static Boolean operator >=(Unit a, Unit b) => a.GreaterThanOrEquals(b);
        public Unit Lesser(Unit b) => this.LessThanOrEquals(b) ? this : b;
        public Unit Greater(Unit b) => this.GreaterThanOrEquals(b) ? this : b;
        public Boolean Between(Unit min, Unit max) => this.GreaterThanOrEquals(min).And(this.LessThanOrEquals(max));
        public Unit Half => this.Divide(((Number)2));
        public Unit Quarter => this.Divide(((Number)4));
        public Unit Tenth => this.Divide(((Number)10));
        public Unit Twice => this.Multiply(((Number)2));
        public Unit Lerp(Unit b, Number t) => this.Multiply(t.FromOne).Add(b.Multiply(t));
    }
    public readonly partial struct Probability
    {
        public Number Magnitude => this.Value;
        public Probability ClampOne => this.Clamp(this.Zero, this.One);
        public Boolean GtZ => this.GreaterThan(this.Zero);
        public Boolean LtZ => this.LessThan(this.Zero);
        public Boolean GtEqZ => this.GreaterThanOrEquals(this.Zero);
        public Boolean LtEqZ => this.LessThanOrEquals(this.Zero);
        public Boolean IsPositive => this.GtEqZ;
        public Boolean IsNegative => this.LtZ;
        public Integer Sign => this.LtZ ? ((Integer)1).Negative : this.GtZ ? ((Integer)1) : ((Integer)0);
        public Probability Abs => this.LtZ ? this.Negative : this;
        public Probability PlusOne => this.Add(this.One);
        public Probability MinusOne => this.Subtract(this.One);
        public Probability FromOne => this.One.Subtract(this);
        public Probability Clamp(Probability a, Probability b) => this.Greater(a).Lesser(b);
        public Boolean Equals(Probability b) => this.Compare(b).Equals(((Integer)0));
        public static Boolean operator ==(Probability a, Probability b) => a.Equals(b);
        public Boolean NotEquals(Probability b) => this.Compare(b).NotEquals(((Integer)0));
        public static Boolean operator !=(Probability a, Probability b) => a.NotEquals(b);
        public Boolean LessThan(Probability b) => this.Compare(b).LessThan(((Integer)0));
        public static Boolean operator <(Probability a, Probability b) => a.LessThan(b);
        public Boolean LessThanOrEquals(Probability b) => this.Compare(b).LessThanOrEquals(((Integer)0));
        public static Boolean operator <=(Probability a, Probability b) => a.LessThanOrEquals(b);
        public Boolean GreaterThan(Probability b) => this.Compare(b).GreaterThan(((Integer)0));
        public static Boolean operator >(Probability a, Probability b) => a.GreaterThan(b);
        public Boolean GreaterThanOrEquals(Probability b) => this.Compare(b).GreaterThanOrEquals(((Integer)0));
        public static Boolean operator >=(Probability a, Probability b) => a.GreaterThanOrEquals(b);
        public Probability Lesser(Probability b) => this.LessThanOrEquals(b) ? this : b;
        public Probability Greater(Probability b) => this.GreaterThanOrEquals(b) ? this : b;
        public Boolean Between(Probability min, Probability max) => this.GreaterThanOrEquals(min).And(this.LessThanOrEquals(max));
        public Probability Half => this.Divide(((Number)2));
        public Probability Quarter => this.Divide(((Number)4));
        public Probability Tenth => this.Divide(((Number)10));
        public Probability Twice => this.Multiply(((Number)2));
        public Probability Lerp(Probability b, Number t) => this.Multiply(t.FromOne).Add(b.Multiply(t));
    }
    public readonly partial struct Complex
    {
        public Integer Count => ((Integer)2);
        public Number At(Integer n) => n.Equals(((Integer)0)) ? this.Real : this.Imaginary;
        public Number this[Integer n] => At(n);
        public Number Sum => this.Reduce(((Number)0), (a, b) => a.Add(b));
        public Number SumSquares => this.Square.Sum;
        public Number MagnitudeSquared => this.SumSquares;
        public Number Magnitude => this.MagnitudeSquared.SquareRoot;
        public Number Dot(Complex v2) => this.Multiply(v2).Sum;
        public Complex Normal => this.Divide(this.Magnitude);
        public Number Average => this.Sum.Divide(this.Count);
        public TAcc Reduce<TAcc>(TAcc init, System.Func<TAcc, Number, TAcc> f) => f(f(init, Real), Imaginary);
        public Complex Map(System.Func<Number, Number> f) => (f(Real), f(Imaginary));
        public Complex PlusOne => this.Add(this.One);
        public Complex MinusOne => this.Subtract(this.One);
        public Complex FromOne => this.One.Subtract(this);
        public Complex Pow2 => this.Multiply(this);
        public Complex Pow3 => this.Pow2.Multiply(this);
        public Complex Pow4 => this.Pow3.Multiply(this);
        public Complex Pow5 => this.Pow4.Multiply(this);
        public Complex Square => this.Pow2;
        public Complex Half => this.Divide(((Number)2));
        public Complex Quarter => this.Divide(((Number)4));
        public Complex Tenth => this.Divide(((Number)10));
        public Complex Twice => this.Multiply(((Number)2));
        public Complex Lerp(Complex b, Number t) => this.Multiply(t.FromOne).Add(b.Multiply(t));
    }
    public readonly partial struct Integer2
    {
        public TAcc Reduce<TAcc>(TAcc init, System.Func<TAcc, Integer, TAcc> f) => f(f(init, A), B);
        public Integer2 Map(System.Func<Integer, Integer> f) => (f(A), f(B));
    }
    public readonly partial struct Integer3
    {
        public TAcc Reduce<TAcc>(TAcc init, System.Func<TAcc, Integer, TAcc> f) => f(f(f(init, A), B), C);
        public Integer3 Map(System.Func<Integer, Integer> f) => (f(A), f(B), f(C));
    }
    public readonly partial struct Integer4
    {
        public TAcc Reduce<TAcc>(TAcc init, System.Func<TAcc, Integer, TAcc> f) => f(f(f(f(init, A), B), C), D);
        public Integer4 Map(System.Func<Integer, Integer> f) => (f(A), f(B), f(C), f(D));
    }
    public readonly partial struct Color
    {
    }
    public readonly partial struct ColorLUV
    {
    }
    public readonly partial struct ColorLAB
    {
    }
    public readonly partial struct ColorLCh
    {
    }
    public readonly partial struct ColorHSV
    {
    }
    public readonly partial struct ColorHSL
    {
    }
    public readonly partial struct ColorYCbCr
    {
    }
    public readonly partial struct SphericalCoordinate
    {
    }
    public readonly partial struct PolarCoordinate
    {
    }
    public readonly partial struct LogPolarCoordinate
    {
    }
    public readonly partial struct CylindricalCoordinate
    {
    }
    public readonly partial struct HorizontalCoordinate
    {
    }
    public readonly partial struct GeoCoordinate
    {
    }
    public readonly partial struct GeoCoordinateWithAltitude
    {
    }
    public readonly partial struct Size2D
    {
        public static implicit operator Vector2D(Size2D a)  => a.Width.Tuple2(a.Height);
    }
    public readonly partial struct Size3D
    {
    }
    public readonly partial struct Fraction
    {
    }
    public readonly partial struct Angle
    {
        public Point2D CirclePoint => this.Sin.Tuple2(this.Cos);
        public Number Cos => Intrinsics.Cos(this);
        public Number Sin => Intrinsics.Sin(this);
        public Number Tan => Intrinsics.Tan(this);
        public Number Magnitude => this.Value;
        public Angle ClampOne => this.Clamp(this.Zero, this.One);
        public Boolean GtZ => this.GreaterThan(this.Zero);
        public Boolean LtZ => this.LessThan(this.Zero);
        public Boolean GtEqZ => this.GreaterThanOrEquals(this.Zero);
        public Boolean LtEqZ => this.LessThanOrEquals(this.Zero);
        public Boolean IsPositive => this.GtEqZ;
        public Boolean IsNegative => this.LtZ;
        public Integer Sign => this.LtZ ? ((Integer)1).Negative : this.GtZ ? ((Integer)1) : ((Integer)0);
        public Angle Abs => this.LtZ ? this.Negative : this;
        public Angle PlusOne => this.Add(this.One);
        public Angle MinusOne => this.Subtract(this.One);
        public Angle FromOne => this.One.Subtract(this);
        public Angle Clamp(Angle a, Angle b) => this.Greater(a).Lesser(b);
        public Boolean Equals(Angle b) => this.Compare(b).Equals(((Integer)0));
        public static Boolean operator ==(Angle a, Angle b) => a.Equals(b);
        public Boolean NotEquals(Angle b) => this.Compare(b).NotEquals(((Integer)0));
        public static Boolean operator !=(Angle a, Angle b) => a.NotEquals(b);
        public Boolean LessThan(Angle b) => this.Compare(b).LessThan(((Integer)0));
        public static Boolean operator <(Angle a, Angle b) => a.LessThan(b);
        public Boolean LessThanOrEquals(Angle b) => this.Compare(b).LessThanOrEquals(((Integer)0));
        public static Boolean operator <=(Angle a, Angle b) => a.LessThanOrEquals(b);
        public Boolean GreaterThan(Angle b) => this.Compare(b).GreaterThan(((Integer)0));
        public static Boolean operator >(Angle a, Angle b) => a.GreaterThan(b);
        public Boolean GreaterThanOrEquals(Angle b) => this.Compare(b).GreaterThanOrEquals(((Integer)0));
        public static Boolean operator >=(Angle a, Angle b) => a.GreaterThanOrEquals(b);
        public Angle Lesser(Angle b) => this.LessThanOrEquals(b) ? this : b;
        public Angle Greater(Angle b) => this.GreaterThanOrEquals(b) ? this : b;
        public Boolean Between(Angle min, Angle max) => this.GreaterThanOrEquals(min).And(this.LessThanOrEquals(max));
        public Angle Half => this.Divide(((Number)2));
        public Angle Quarter => this.Divide(((Number)4));
        public Angle Tenth => this.Divide(((Number)10));
        public Angle Twice => this.Multiply(((Number)2));
        public Angle Lerp(Angle b, Number t) => this.Multiply(t.FromOne).Add(b.Multiply(t));
    }
    public readonly partial struct Length
    {
        public Number Magnitude => this.Value;
        public Length ClampOne => this.Clamp(this.Zero, this.One);
        public Boolean GtZ => this.GreaterThan(this.Zero);
        public Boolean LtZ => this.LessThan(this.Zero);
        public Boolean GtEqZ => this.GreaterThanOrEquals(this.Zero);
        public Boolean LtEqZ => this.LessThanOrEquals(this.Zero);
        public Boolean IsPositive => this.GtEqZ;
        public Boolean IsNegative => this.LtZ;
        public Integer Sign => this.LtZ ? ((Integer)1).Negative : this.GtZ ? ((Integer)1) : ((Integer)0);
        public Length Abs => this.LtZ ? this.Negative : this;
        public Length PlusOne => this.Add(this.One);
        public Length MinusOne => this.Subtract(this.One);
        public Length FromOne => this.One.Subtract(this);
        public Length Clamp(Length a, Length b) => this.Greater(a).Lesser(b);
        public Boolean Equals(Length b) => this.Compare(b).Equals(((Integer)0));
        public static Boolean operator ==(Length a, Length b) => a.Equals(b);
        public Boolean NotEquals(Length b) => this.Compare(b).NotEquals(((Integer)0));
        public static Boolean operator !=(Length a, Length b) => a.NotEquals(b);
        public Boolean LessThan(Length b) => this.Compare(b).LessThan(((Integer)0));
        public static Boolean operator <(Length a, Length b) => a.LessThan(b);
        public Boolean LessThanOrEquals(Length b) => this.Compare(b).LessThanOrEquals(((Integer)0));
        public static Boolean operator <=(Length a, Length b) => a.LessThanOrEquals(b);
        public Boolean GreaterThan(Length b) => this.Compare(b).GreaterThan(((Integer)0));
        public static Boolean operator >(Length a, Length b) => a.GreaterThan(b);
        public Boolean GreaterThanOrEquals(Length b) => this.Compare(b).GreaterThanOrEquals(((Integer)0));
        public static Boolean operator >=(Length a, Length b) => a.GreaterThanOrEquals(b);
        public Length Lesser(Length b) => this.LessThanOrEquals(b) ? this : b;
        public Length Greater(Length b) => this.GreaterThanOrEquals(b) ? this : b;
        public Boolean Between(Length min, Length max) => this.GreaterThanOrEquals(min).And(this.LessThanOrEquals(max));
        public Length Half => this.Divide(((Number)2));
        public Length Quarter => this.Divide(((Number)4));
        public Length Tenth => this.Divide(((Number)10));
        public Length Twice => this.Multiply(((Number)2));
        public Length Lerp(Length b, Number t) => this.Multiply(t.FromOne).Add(b.Multiply(t));
    }
    public readonly partial struct Mass
    {
        public Number Magnitude => this.Value;
        public Mass ClampOne => this.Clamp(this.Zero, this.One);
        public Boolean GtZ => this.GreaterThan(this.Zero);
        public Boolean LtZ => this.LessThan(this.Zero);
        public Boolean GtEqZ => this.GreaterThanOrEquals(this.Zero);
        public Boolean LtEqZ => this.LessThanOrEquals(this.Zero);
        public Boolean IsPositive => this.GtEqZ;
        public Boolean IsNegative => this.LtZ;
        public Integer Sign => this.LtZ ? ((Integer)1).Negative : this.GtZ ? ((Integer)1) : ((Integer)0);
        public Mass Abs => this.LtZ ? this.Negative : this;
        public Mass PlusOne => this.Add(this.One);
        public Mass MinusOne => this.Subtract(this.One);
        public Mass FromOne => this.One.Subtract(this);
        public Mass Clamp(Mass a, Mass b) => this.Greater(a).Lesser(b);
        public Boolean Equals(Mass b) => this.Compare(b).Equals(((Integer)0));
        public static Boolean operator ==(Mass a, Mass b) => a.Equals(b);
        public Boolean NotEquals(Mass b) => this.Compare(b).NotEquals(((Integer)0));
        public static Boolean operator !=(Mass a, Mass b) => a.NotEquals(b);
        public Boolean LessThan(Mass b) => this.Compare(b).LessThan(((Integer)0));
        public static Boolean operator <(Mass a, Mass b) => a.LessThan(b);
        public Boolean LessThanOrEquals(Mass b) => this.Compare(b).LessThanOrEquals(((Integer)0));
        public static Boolean operator <=(Mass a, Mass b) => a.LessThanOrEquals(b);
        public Boolean GreaterThan(Mass b) => this.Compare(b).GreaterThan(((Integer)0));
        public static Boolean operator >(Mass a, Mass b) => a.GreaterThan(b);
        public Boolean GreaterThanOrEquals(Mass b) => this.Compare(b).GreaterThanOrEquals(((Integer)0));
        public static Boolean operator >=(Mass a, Mass b) => a.GreaterThanOrEquals(b);
        public Mass Lesser(Mass b) => this.LessThanOrEquals(b) ? this : b;
        public Mass Greater(Mass b) => this.GreaterThanOrEquals(b) ? this : b;
        public Boolean Between(Mass min, Mass max) => this.GreaterThanOrEquals(min).And(this.LessThanOrEquals(max));
        public Mass Half => this.Divide(((Number)2));
        public Mass Quarter => this.Divide(((Number)4));
        public Mass Tenth => this.Divide(((Number)10));
        public Mass Twice => this.Multiply(((Number)2));
        public Mass Lerp(Mass b, Number t) => this.Multiply(t.FromOne).Add(b.Multiply(t));
    }
    public readonly partial struct Temperature
    {
        public Number Magnitude => this.Value;
        public Temperature ClampOne => this.Clamp(this.Zero, this.One);
        public Boolean GtZ => this.GreaterThan(this.Zero);
        public Boolean LtZ => this.LessThan(this.Zero);
        public Boolean GtEqZ => this.GreaterThanOrEquals(this.Zero);
        public Boolean LtEqZ => this.LessThanOrEquals(this.Zero);
        public Boolean IsPositive => this.GtEqZ;
        public Boolean IsNegative => this.LtZ;
        public Integer Sign => this.LtZ ? ((Integer)1).Negative : this.GtZ ? ((Integer)1) : ((Integer)0);
        public Temperature Abs => this.LtZ ? this.Negative : this;
        public Temperature PlusOne => this.Add(this.One);
        public Temperature MinusOne => this.Subtract(this.One);
        public Temperature FromOne => this.One.Subtract(this);
        public Temperature Clamp(Temperature a, Temperature b) => this.Greater(a).Lesser(b);
        public Boolean Equals(Temperature b) => this.Compare(b).Equals(((Integer)0));
        public static Boolean operator ==(Temperature a, Temperature b) => a.Equals(b);
        public Boolean NotEquals(Temperature b) => this.Compare(b).NotEquals(((Integer)0));
        public static Boolean operator !=(Temperature a, Temperature b) => a.NotEquals(b);
        public Boolean LessThan(Temperature b) => this.Compare(b).LessThan(((Integer)0));
        public static Boolean operator <(Temperature a, Temperature b) => a.LessThan(b);
        public Boolean LessThanOrEquals(Temperature b) => this.Compare(b).LessThanOrEquals(((Integer)0));
        public static Boolean operator <=(Temperature a, Temperature b) => a.LessThanOrEquals(b);
        public Boolean GreaterThan(Temperature b) => this.Compare(b).GreaterThan(((Integer)0));
        public static Boolean operator >(Temperature a, Temperature b) => a.GreaterThan(b);
        public Boolean GreaterThanOrEquals(Temperature b) => this.Compare(b).GreaterThanOrEquals(((Integer)0));
        public static Boolean operator >=(Temperature a, Temperature b) => a.GreaterThanOrEquals(b);
        public Temperature Lesser(Temperature b) => this.LessThanOrEquals(b) ? this : b;
        public Temperature Greater(Temperature b) => this.GreaterThanOrEquals(b) ? this : b;
        public Boolean Between(Temperature min, Temperature max) => this.GreaterThanOrEquals(min).And(this.LessThanOrEquals(max));
        public Temperature Half => this.Divide(((Number)2));
        public Temperature Quarter => this.Divide(((Number)4));
        public Temperature Tenth => this.Divide(((Number)10));
        public Temperature Twice => this.Multiply(((Number)2));
        public Temperature Lerp(Temperature b, Number t) => this.Multiply(t.FromOne).Add(b.Multiply(t));
    }
    public readonly partial struct Time
    {
        public Number Magnitude => this.Value;
        public Time ClampOne => this.Clamp(this.Zero, this.One);
        public Boolean GtZ => this.GreaterThan(this.Zero);
        public Boolean LtZ => this.LessThan(this.Zero);
        public Boolean GtEqZ => this.GreaterThanOrEquals(this.Zero);
        public Boolean LtEqZ => this.LessThanOrEquals(this.Zero);
        public Boolean IsPositive => this.GtEqZ;
        public Boolean IsNegative => this.LtZ;
        public Integer Sign => this.LtZ ? ((Integer)1).Negative : this.GtZ ? ((Integer)1) : ((Integer)0);
        public Time Abs => this.LtZ ? this.Negative : this;
        public Time PlusOne => this.Add(this.One);
        public Time MinusOne => this.Subtract(this.One);
        public Time FromOne => this.One.Subtract(this);
        public Time Clamp(Time a, Time b) => this.Greater(a).Lesser(b);
        public Boolean Equals(Time b) => this.Compare(b).Equals(((Integer)0));
        public static Boolean operator ==(Time a, Time b) => a.Equals(b);
        public Boolean NotEquals(Time b) => this.Compare(b).NotEquals(((Integer)0));
        public static Boolean operator !=(Time a, Time b) => a.NotEquals(b);
        public Boolean LessThan(Time b) => this.Compare(b).LessThan(((Integer)0));
        public static Boolean operator <(Time a, Time b) => a.LessThan(b);
        public Boolean LessThanOrEquals(Time b) => this.Compare(b).LessThanOrEquals(((Integer)0));
        public static Boolean operator <=(Time a, Time b) => a.LessThanOrEquals(b);
        public Boolean GreaterThan(Time b) => this.Compare(b).GreaterThan(((Integer)0));
        public static Boolean operator >(Time a, Time b) => a.GreaterThan(b);
        public Boolean GreaterThanOrEquals(Time b) => this.Compare(b).GreaterThanOrEquals(((Integer)0));
        public static Boolean operator >=(Time a, Time b) => a.GreaterThanOrEquals(b);
        public Time Lesser(Time b) => this.LessThanOrEquals(b) ? this : b;
        public Time Greater(Time b) => this.GreaterThanOrEquals(b) ? this : b;
        public Boolean Between(Time min, Time max) => this.GreaterThanOrEquals(min).And(this.LessThanOrEquals(max));
        public Time Half => this.Divide(((Number)2));
        public Time Quarter => this.Divide(((Number)4));
        public Time Tenth => this.Divide(((Number)10));
        public Time Twice => this.Multiply(((Number)2));
        public Time Lerp(Time b, Number t) => this.Multiply(t.FromOne).Add(b.Multiply(t));
    }
    public readonly partial struct TimeRange
    {
        public Time Size => this.Max.Subtract(this.Min);
        public DateTime Lerp(Number amount) => this.Min.Lerp(this.Max, amount);
        public TimeRange Reverse => this.Max.Tuple2(this.Min);
        public DateTime Center => this.Lerp(((Number)0.5));
        public Boolean Contains(DateTime value) => value.Between(this.Min, this.Max);
        public Boolean Contains(TimeRange y) => this.Contains(y.Min).And(this.Contains(y.Max));
        public Boolean Overlaps(TimeRange y) => this.Contains(y.Min).Or(this.Contains(y.Max).Or(y.Contains(this.Min).Or(y.Contains(this.Max))));
        public Tuple2<TimeRange, TimeRange> SplitAt(Number t) => this.Left(t).Tuple2(this.Right(t));
        public Tuple2<TimeRange, TimeRange> Split => this.SplitAt(((Number)0.5));
        public TimeRange Left(Number t) => this.Min.Tuple2(this.Lerp(t));
        public TimeRange Right(Number t) => this.Lerp(t).Tuple2(this.Max);
        public TimeRange MoveTo(DateTime v) => v.Tuple2(v.Add(this.Size));
        public TimeRange LeftHalf => this.Left(((Number)0.5));
        public TimeRange RightHalf => this.Right(((Number)0.5));
        public TimeRange Recenter(DateTime c) => c.Subtract(this.Size.Half).Tuple2(c.Add(this.Size.Half));
        public TimeRange Clamp(TimeRange y) => this.Clamp(y.Min).Tuple2(this.Clamp(y.Max));
        public DateTime Clamp(DateTime value) => value.Clamp(this.Min, this.Max);
    }
    public readonly partial struct DateTime
    {
    }
    public readonly partial struct AnglePair
    {
        public Angle Size => this.Max.Subtract(this.Min);
        public Angle Lerp(Number amount) => this.Min.Lerp(this.Max, amount);
        public AnglePair Reverse => this.Max.Tuple2(this.Min);
        public Angle Center => this.Lerp(((Number)0.5));
        public Boolean Contains(Angle value) => value.Between(this.Min, this.Max);
        public Boolean Contains(AnglePair y) => this.Contains(y.Min).And(this.Contains(y.Max));
        public Boolean Overlaps(AnglePair y) => this.Contains(y.Min).Or(this.Contains(y.Max).Or(y.Contains(this.Min).Or(y.Contains(this.Max))));
        public Tuple2<AnglePair, AnglePair> SplitAt(Number t) => this.Left(t).Tuple2(this.Right(t));
        public Tuple2<AnglePair, AnglePair> Split => this.SplitAt(((Number)0.5));
        public AnglePair Left(Number t) => this.Min.Tuple2(this.Lerp(t));
        public AnglePair Right(Number t) => this.Lerp(t).Tuple2(this.Max);
        public AnglePair MoveTo(Angle v) => v.Tuple2(v.Add(this.Size));
        public AnglePair LeftHalf => this.Left(((Number)0.5));
        public AnglePair RightHalf => this.Right(((Number)0.5));
        public AnglePair Recenter(Angle c) => c.Subtract(this.Size.Half).Tuple2(c.Add(this.Size.Half));
        public AnglePair Clamp(AnglePair y) => this.Clamp(y.Min).Tuple2(this.Clamp(y.Max));
        public Angle Clamp(Angle value) => value.Clamp(this.Min, this.Max);
    }
    public readonly partial struct NumberInterval
    {
        public Number Size => this.Max.Subtract(this.Min);
        public Number Lerp(Number amount) => this.Min.Lerp(this.Max, amount);
        public NumberInterval Reverse => this.Max.Tuple2(this.Min);
        public Number Center => this.Lerp(((Number)0.5));
        public Boolean Contains(Number value) => value.Between(this.Min, this.Max);
        public Boolean Contains(NumberInterval y) => this.Contains(y.Min).And(this.Contains(y.Max));
        public Boolean Overlaps(NumberInterval y) => this.Contains(y.Min).Or(this.Contains(y.Max).Or(y.Contains(this.Min).Or(y.Contains(this.Max))));
        public Tuple2<NumberInterval, NumberInterval> SplitAt(Number t) => this.Left(t).Tuple2(this.Right(t));
        public Tuple2<NumberInterval, NumberInterval> Split => this.SplitAt(((Number)0.5));
        public NumberInterval Left(Number t) => this.Min.Tuple2(this.Lerp(t));
        public NumberInterval Right(Number t) => this.Lerp(t).Tuple2(this.Max);
        public NumberInterval MoveTo(Number v) => v.Tuple2(v.Add(this.Size));
        public NumberInterval LeftHalf => this.Left(((Number)0.5));
        public NumberInterval RightHalf => this.Right(((Number)0.5));
        public NumberInterval Recenter(Number c) => c.Subtract(this.Size.Half).Tuple2(c.Add(this.Size.Half));
        public NumberInterval Clamp(NumberInterval y) => this.Clamp(y.Min).Tuple2(this.Clamp(y.Max));
        public Number Clamp(Number value) => value.Clamp(this.Min, this.Max);
    }
    public readonly partial struct Matrix2D
    {
        public TAcc Reduce<TAcc>(TAcc init, System.Func<TAcc, Vector3D, TAcc> f) => f(f(f(init, Column1), Column2), Column3);
        public Matrix2D Map(System.Func<Vector3D, Vector3D> f) => (f(Column1), f(Column2), f(Column3));
    }
    public readonly partial struct Matrix3D
    {
        public Number M11 => this.Column1.X;
        public Number M12 => this.Column2.X;
        public Number M13 => this.Column3.X;
        public Number M14 => this.Column4.X;
        public Number M21 => this.Column1.Y;
        public Number M22 => this.Column2.Y;
        public Number M23 => this.Column3.Y;
        public Number M24 => this.Column4.Y;
        public Number M31 => this.Column1.Z;
        public Number M32 => this.Column2.Z;
        public Number M33 => this.Column3.Z;
        public Number M34 => this.Column4.Z;
        public Number M41 => this.Column1.W;
        public Number M42 => this.Column2.W;
        public Number M43 => this.Column3.W;
        public Number M44 => this.Column4.W;
        public Vector3D Multiply(Vector3D v) => v.X.Multiply(this.M11).Add(v.Y.Multiply(this.M21).Add(v.Z.Multiply(this.M31).Add(this.M41))).Tuple3(v.X.Multiply(this.M12).Add(v.Y.Multiply(this.M22).Add(v.Z.Multiply(this.M32).Add(this.M42))), v.X.Multiply(this.M13).Add(v.Y.Multiply(this.M23).Add(v.Z.Multiply(this.M33).Add(this.M43))));
        public static Vector3D operator *(Matrix3D m, Vector3D v) => m.Multiply(v);
        public TAcc Reduce<TAcc>(TAcc init, System.Func<TAcc, Vector4D, TAcc> f) => f(f(f(f(init, Column1), Column2), Column3), Column4);
        public Matrix3D Map(System.Func<Vector4D, Vector4D> f) => (f(Column1), f(Column2), f(Column3), f(Column4));
    }
    public readonly partial struct UV
    {
        public Integer Count => ((Integer)2);
        public Number At(Integer n) => n.Equals(((Integer)0)) ? this.U : this.V;
        public Number this[Integer n] => At(n);
        public Number Sum => this.Reduce(((Number)0), (a, b) => a.Add(b));
        public Number SumSquares => this.Square.Sum;
        public Number MagnitudeSquared => this.SumSquares;
        public Number Magnitude => this.MagnitudeSquared.SquareRoot;
        public Number Dot(UV v2) => this.Multiply(v2).Sum;
        public UV Normal => this.Divide(this.Magnitude);
        public Number Average => this.Sum.Divide(this.Count);
        public TAcc Reduce<TAcc>(TAcc init, System.Func<TAcc, Number, TAcc> f) => f(f(init, U), V);
        public UV Map(System.Func<Number, Number> f) => (f(U), f(V));
        public UV PlusOne => this.Add(this.One);
        public UV MinusOne => this.Subtract(this.One);
        public UV FromOne => this.One.Subtract(this);
        public UV Pow2 => this.Multiply(this);
        public UV Pow3 => this.Pow2.Multiply(this);
        public UV Pow4 => this.Pow3.Multiply(this);
        public UV Pow5 => this.Pow4.Multiply(this);
        public UV Square => this.Pow2;
        public UV Half => this.Divide(((Number)2));
        public UV Quarter => this.Divide(((Number)4));
        public UV Tenth => this.Divide(((Number)10));
        public UV Twice => this.Multiply(((Number)2));
        public UV Lerp(UV b, Number t) => this.Multiply(t.FromOne).Add(b.Multiply(t));
    }
    public readonly partial struct UVW
    {
        public Integer Count => ((Integer)3);
        public Number At(Integer n) => n.Equals(((Integer)0)) ? this.U : n.Equals(((Integer)1)) ? this.V : this.W;
        public Number this[Integer n] => At(n);
        public Number Sum => this.Reduce(((Number)0), (a, b) => a.Add(b));
        public Number SumSquares => this.Square.Sum;
        public Number MagnitudeSquared => this.SumSquares;
        public Number Magnitude => this.MagnitudeSquared.SquareRoot;
        public Number Dot(UVW v2) => this.Multiply(v2).Sum;
        public UVW Normal => this.Divide(this.Magnitude);
        public Number Average => this.Sum.Divide(this.Count);
        public TAcc Reduce<TAcc>(TAcc init, System.Func<TAcc, Number, TAcc> f) => f(f(f(init, U), V), W);
        public UVW Map(System.Func<Number, Number> f) => (f(U), f(V), f(W));
        public UVW PlusOne => this.Add(this.One);
        public UVW MinusOne => this.Subtract(this.One);
        public UVW FromOne => this.One.Subtract(this);
        public UVW Pow2 => this.Multiply(this);
        public UVW Pow3 => this.Pow2.Multiply(this);
        public UVW Pow4 => this.Pow3.Multiply(this);
        public UVW Pow5 => this.Pow4.Multiply(this);
        public UVW Square => this.Pow2;
        public UVW Half => this.Divide(((Number)2));
        public UVW Quarter => this.Divide(((Number)4));
        public UVW Tenth => this.Divide(((Number)10));
        public UVW Twice => this.Multiply(((Number)2));
        public UVW Lerp(UVW b, Number t) => this.Multiply(t.FromOne).Add(b.Multiply(t));
    }
    public readonly partial struct Vector2D
    {
        public static implicit operator Point2D(Vector2D x)  => x.X.Tuple2(x.Y);
        public Integer Count => ((Integer)2);
        public Number At(Integer n) => n.Equals(((Integer)0)) ? this.X : this.Y;
        public Number this[Integer n] => At(n);
        public Number Sum => this.Reduce(((Number)0), (a, b) => a.Add(b));
        public Number SumSquares => this.Square.Sum;
        public Number MagnitudeSquared => this.SumSquares;
        public Number Magnitude => this.MagnitudeSquared.SquareRoot;
        public Number Dot(Vector2D v2) => this.Multiply(v2).Sum;
        public Vector2D Normal => this.Divide(this.Magnitude);
        public Number Average => this.Sum.Divide(this.Count);
        public TAcc Reduce<TAcc>(TAcc init, System.Func<TAcc, Number, TAcc> f) => f(f(init, X), Y);
        public Vector2D Map(System.Func<Number, Number> f) => (f(X), f(Y));
        public Vector2D PlusOne => this.Add(this.One);
        public Vector2D MinusOne => this.Subtract(this.One);
        public Vector2D FromOne => this.One.Subtract(this);
        public Vector2D Pow2 => this.Multiply(this);
        public Vector2D Pow3 => this.Pow2.Multiply(this);
        public Vector2D Pow4 => this.Pow3.Multiply(this);
        public Vector2D Pow5 => this.Pow4.Multiply(this);
        public Vector2D Square => this.Pow2;
        public Vector2D Half => this.Divide(((Number)2));
        public Vector2D Quarter => this.Divide(((Number)4));
        public Vector2D Tenth => this.Divide(((Number)10));
        public Vector2D Twice => this.Multiply(((Number)2));
        public Vector2D Lerp(Vector2D b, Number t) => this.Multiply(t.FromOne).Add(b.Multiply(t));
    }
    public readonly partial struct Vector3D
    {
        public static implicit operator Point3D(Vector3D x)  => x.X.Tuple3(x.Y, x.Z);
        public Integer Count => ((Integer)3);
        public Number At(Integer n) => n.Equals(((Integer)0)) ? this.X : n.Equals(((Integer)1)) ? this.Y : this.Z;
        public Number this[Integer n] => At(n);
        public Number Sum => this.Reduce(((Number)0), (a, b) => a.Add(b));
        public Number SumSquares => this.Square.Sum;
        public Number MagnitudeSquared => this.SumSquares;
        public Number Magnitude => this.MagnitudeSquared.SquareRoot;
        public Number Dot(Vector3D v2) => this.Multiply(v2).Sum;
        public Vector3D Normal => this.Divide(this.Magnitude);
        public Number Average => this.Sum.Divide(this.Count);
        public TAcc Reduce<TAcc>(TAcc init, System.Func<TAcc, Number, TAcc> f) => f(f(f(init, X), Y), Z);
        public Vector3D Map(System.Func<Number, Number> f) => (f(X), f(Y), f(Z));
        public Vector3D PlusOne => this.Add(this.One);
        public Vector3D MinusOne => this.Subtract(this.One);
        public Vector3D FromOne => this.One.Subtract(this);
        public Vector3D Pow2 => this.Multiply(this);
        public Vector3D Pow3 => this.Pow2.Multiply(this);
        public Vector3D Pow4 => this.Pow3.Multiply(this);
        public Vector3D Pow5 => this.Pow4.Multiply(this);
        public Vector3D Square => this.Pow2;
        public Vector3D Half => this.Divide(((Number)2));
        public Vector3D Quarter => this.Divide(((Number)4));
        public Vector3D Tenth => this.Divide(((Number)10));
        public Vector3D Twice => this.Multiply(((Number)2));
        public Vector3D Lerp(Vector3D b, Number t) => this.Multiply(t.FromOne).Add(b.Multiply(t));
    }
    public readonly partial struct Vector4D
    {
        public static implicit operator Point4D(Vector4D x)  => x.X.Tuple4(x.Y, x.Z, x.W);
        public Integer Count => ((Integer)4);
        public Number At(Integer n) => n.Equals(((Integer)0)) ? this.X : n.Equals(((Integer)1)) ? this.Y : n.Equals(((Integer)2)) ? this.Z : this.W;
        public Number this[Integer n] => At(n);
        public Number Sum => this.Reduce(((Number)0), (a, b) => a.Add(b));
        public Number SumSquares => this.Square.Sum;
        public Number MagnitudeSquared => this.SumSquares;
        public Number Magnitude => this.MagnitudeSquared.SquareRoot;
        public Number Dot(Vector4D v2) => this.Multiply(v2).Sum;
        public Vector4D Normal => this.Divide(this.Magnitude);
        public Number Average => this.Sum.Divide(this.Count);
        public TAcc Reduce<TAcc>(TAcc init, System.Func<TAcc, Number, TAcc> f) => f(f(f(f(init, X), Y), Z), W);
        public Vector4D Map(System.Func<Number, Number> f) => (f(X), f(Y), f(Z), f(W));
        public Vector4D PlusOne => this.Add(this.One);
        public Vector4D MinusOne => this.Subtract(this.One);
        public Vector4D FromOne => this.One.Subtract(this);
        public Vector4D Pow2 => this.Multiply(this);
        public Vector4D Pow3 => this.Pow2.Multiply(this);
        public Vector4D Pow4 => this.Pow3.Multiply(this);
        public Vector4D Pow5 => this.Pow4.Multiply(this);
        public Vector4D Square => this.Pow2;
        public Vector4D Half => this.Divide(((Number)2));
        public Vector4D Quarter => this.Divide(((Number)4));
        public Vector4D Tenth => this.Divide(((Number)10));
        public Vector4D Twice => this.Multiply(((Number)2));
        public Vector4D Lerp(Vector4D b, Number t) => this.Multiply(t.FromOne).Add(b.Multiply(t));
    }
        public static class Intrinsics
    {
        public static Number Cos(Angle x) => (float)System.Math.Cos(x.Value);
        public static Number Sin(Angle x) => (float)System.Math.Sin(x.Value);
        public static Number Tan(Angle x) => (float)System.Math.Tan(x.Value);

        public static Number Ln(Number x) => (float)System.Math.Log(x.Value);
        public static Number Exp(Number x) => (float)System.Math.Exp(x.Value);

        public static Angle Acos(Number x) => new Angle((float)System.Math.Acos(x));
        public static Angle Asin(Number x) => new Angle((float)System.Math.Asin(x));
        public static Angle Atan(Number x) => new Angle((float)System.Math.Atan(x));

        public static Number Pow(Number x, Number y) => (float)System.Math.Pow(x, y);
        public static Number Log(Number x, Number y) => (float)System.Math.Log(x, y);
        public static Number NaturalLog(Number x) => (float)System.Math.Log(x);
        public static Number NaturalPower(Number x) => (float)System.Math.Pow(x, System.Math.E);

        public static Number Add(Number x, Number y) => x.Value + y.Value;
        public static Number Subtract(Number x, Number y) => x.Value - y.Value;
        public static Number Divide(Number x, Number y) => x.Value / y.Value;
        public static Number Multiply(Number x, Number y) => x.Value * y.Value;
        public static Number Modulo(Number x, Number y) => x.Value % y.Value;
        public static Number Negative(Number x) => -x.Value;

        public static Integer Add(Integer x, Integer y) => x.Value + y.Value;
        public static Integer Subtract(Integer x, Integer y) => x.Value - y.Value;
        public static Integer Divide(Integer x, Integer y) => x.Value / y.Value;
        public static Integer Multiply(Integer x, Integer y) => x.Value * y.Value;
        public static Integer Modulo(Integer x, Integer y) => x.Value % y.Value;
        public static Integer Negative(Integer x) => -x.Value;

        public static Array<Integer> Range(Integer x) => new RangeStruct(x);

        public static Boolean And(Boolean x, Boolean y) => x.Value && y.Value;
        public static Boolean Or(Boolean x, Boolean y) => x.Value || y.Value;
        public static Boolean Not(Boolean x) => !x.Value;

        public static Number ToNumber(Integer x) => x.Value;

        public static string MakeString(string typeName, Array<String> fieldNames, Array<Dynamic> fieldValues)
        {
            var sb = new System.Text.StringBuilder();
            sb.Append($"{{ _type=\"{typeName}\" ");
            for (var i = 0; i < fieldNames.Count; i++)
                sb.Append(", ").Append(fieldNames.At(i).Value).Append(" = ").Append(fieldValues.At(i).Value);
            sb.Append(" }");
            return sb.ToString();
        }

        public static int CombineHashCodes(params object[] objects)
        {
            if (objects.Length == 0) return 0;
            var r = objects[0].GetHashCode();
            for (var i = 1; i < objects.Length; ++i)
                r = CombineHashCodes(r, objects[i].GetHashCode());
            return r;
        }

        public static (T0, T1) Tuple2<T0, T1>(this T0 item0, T1 item1)
            => (item0, item1);

        public static (T0, T1, T2) Tuple3<T0, T1, T2>(this T0 item0, T1 item1, T2 item2)
            => (item0, item1, item2);

        public static (T0, T1, T2, T3) Tuple4<T0, T1, T2, T3>(this T0 item0, T1 item1, T2 item2, T3 item3)
            => (item0, item1, item2, item3);

        public static Array<T> MakeArray<T>(params T[] args)
            => new PrimitiveArray<T>(args);

        public static int CombineHashCodes(int h1, int h2)
        {
            unchecked
            {
                var rol5 = ((uint)h1 << 5) | ((uint)h1 >> 27);
                return ((int)rol5 + h1) ^ h2;
            }
        }

        public static Array<T1> Map<T0, T1>(this Array<T0> self, System.Func<T0, T1> f)
            => new MappedArray<T0, T1>(self, f);

        public static TAcc Reduce<T, TAcc>(this Array<T> self, TAcc init, System.Func<TAcc, T, TAcc> f)
        {
            for (var i = 0; i < self.Count; ++i)
                init = f(init, self.At(i));
            return init;
        }
    }

    public readonly struct PrimitiveArray<T> : Array<T>
    {
        private readonly T[] _data;
        public Integer Count => _data.Length;
        public T At(Integer n) => _data[n];
        public PrimitiveArray(T[] data) => _data = data;
        public static Array<T> Default = new PrimitiveArray<T>(System.Array.Empty<T>());
    }

    public readonly struct MappedArray<T0, T1> : Array<T1>
    {
        public System.Func<T0, T1> MapFunc { get; }
        public Array<T0> Original { get; }
        public Integer Count => Original.Count;
        public T1 At(Integer n) => MapFunc(Original.At(n));

        public MappedArray(Array<T0> input, System.Func<T0, T1> f)
        {
            Original = input;
            MapFunc = f;
        }
    }

    public readonly struct RangeStruct : Array<Integer>
    {
        public Integer Count { get; }
        public Integer At(Integer n) => n;
        public RangeStruct(Integer n) => Count = n;
    }

    public readonly partial struct String
    {
        public Integer Compare(String other) => Value.CompareTo(other.Value);
        public Character At(Integer n) => Value[n];
        public Integer Count => Value.Length;
    }

    public readonly partial struct Boolean
    {
        public static bool operator true(Boolean b) => b.Value;
        public static bool operator false(Boolean b) => !b.Value;
    }

    public readonly partial struct Number
    {
        public Number Zero => 0;
        public Number One => 1;
        public Number MinValue => float.MinValue;
        public Number MaxValue => float.MaxValue;
        public Integer Compare(Number other) => Value.CompareTo(other.Value);
        public Number Unlerp(Number a, Number b) => (float)(this - a) / (float)(b - a);
    }

    public readonly partial struct Integer
    {
        public Integer Zero => 0;
        public Integer One => 1;
        public Integer MinValue => int.MinValue;
        public Integer MaxValue => int.MaxValue;
        public Number Magnitude => Value;
        public static implicit operator Number(Integer self) => self.Value;
        public Integer Compare(Integer other) => Value.CompareTo(other.Value);
        public Integer Lerp(Integer b, Number t) => (int)(Value * (1.0 - t) + b * t);
        public Number Unlerp(Integer a, Integer b) => (float)(this - a) / (float)(b - a);
    }

    public readonly partial struct Character
    {
        public Character Zero => (char)0;
        public Character One => (char)1;
        public Character MinValue => char.MinValue;
        public Character MaxValue => char.MaxValue;
        public Number Magnitude => Value;
        public static implicit operator Number(Character self) => self.Value;
        public Integer Compare(Character other) => Value.CompareTo(other.Value);
        public Number Unlerp(Character a, Character b) => (float)(this - a) / (float)(b - a);
        public Boolean Equals(Character x) => Value.Equals(x.Value);
        public Boolean NotEquals(Character x) => !Equals(x);
    }

    public readonly partial struct Dynamic
    {
        public readonly object Value;
        public Dynamic WithValue(object value) => new Dynamic(value);
        public Dynamic(object value) => (Value) = (value);
        public static Dynamic Default = new Dynamic();
        public static Dynamic New(object value) => new Dynamic(value);
        public String TypeName => "Dynamic";
        public Array<String> FieldNames => Intrinsics.MakeArray<String>("Value");
        public Array<Dynamic> FieldValues => Intrinsics.MakeArray<Dynamic>(new Dynamic(Value));
        public T As<T>() => (T)Value;
    }

}
