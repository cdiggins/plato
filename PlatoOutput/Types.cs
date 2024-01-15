using System;
public partial class Number: Numerical<Number>
{
    public double Value { get; }
    public Number WithValue(double value) => (value);
    public Number(double value) => (Value) = (value);
    public Number() { }
    public static Number Default = new Number();
    public static Number New(double value) => new Number(value);
    public static implicit operator double(Number self) => self.Value;
    public static implicit operator Number(double value) => new Number(value);
}
public partial class Integer: Numerical<Integer>
{
    public int Value { get; }
    public Integer WithValue(int value) => (value);
    public Integer(int value) => (Value) = (value);
    public Integer() { }
    public static Integer Default = new Integer();
    public static Integer New(int value) => new Integer(value);
    public static implicit operator int(Integer self) => self.Value;
    public static implicit operator Integer(int value) => new Integer(value);
}
public partial class String: Value<String>, Array<Character>
{
    public string Value { get; }
    public String WithValue(string value) => (value);
    public String(string value) => (Value) = (value);
    public String() { }
    public static String Default = new String();
    public static String New(string value) => new String(value);
    public static implicit operator string(String self) => self.Value;
    public static implicit operator String(string value) => new String(value);
}
public partial class Boolean: BooleanOperations<Boolean>
{
    public bool Value { get; }
    public Boolean WithValue(bool value) => (value);
    public Boolean(bool value) => (Value) = (value);
    public Boolean() { }
    public static Boolean Default = new Boolean();
    public static Boolean New(bool value) => new Boolean(value);
    public static implicit operator bool(Boolean self) => self.Value;
    public static implicit operator Boolean(bool value) => new Boolean(value);
}
public partial class Character: Value<Character>
{
    public char Value { get; }
    public Character WithValue(char value) => (value);
    public Character(char value) => (Value) = (value);
    public Character() { }
    public static Character Default = new Character();
    public static Character New(char value) => new Character(value);
    public static implicit operator char(Character self) => self.Value;
    public static implicit operator Character(char value) => new Character(value);
}
public partial class Count: Numerical<Count>
{
    public Integer Value { get; }
    public Count WithValue(Integer value) => (value);
    public Count(Integer value) => (Value) = (value);
    public Count() { }
    public static Count Default = new Count();
    public static Count New(Integer value) => new Count(value);
    public static implicit operator Integer(Count self) => self.Value;
    public static implicit operator Count(Integer value) => new Count(value);
}
public partial class Index: Value<Index>
{
    public Integer Value { get; }
    public Index WithValue(Integer value) => (value);
    public Index(Integer value) => (Value) = (value);
    public Index() { }
    public static Index Default = new Index();
    public static Index New(Integer value) => new Index(value);
    public static implicit operator Integer(Index self) => self.Value;
    public static implicit operator Index(Integer value) => new Index(value);
}
public partial class Unit: Numerical<Unit>
{
    public Number Value { get; }
    public Unit WithValue(Number value) => (value);
    public Unit(Number value) => (Value) = (value);
    public Unit() { }
    public static Unit Default = new Unit();
    public static Unit New(Number value) => new Unit(value);
    public static implicit operator Number(Unit self) => self.Value;
    public static implicit operator Unit(Number value) => new Unit(value);
}
public partial class Percent: Numerical<Percent>
{
    public Number Value { get; }
    public Percent WithValue(Number value) => (value);
    public Percent(Number value) => (Value) = (value);
    public Percent() { }
    public static Percent Default = new Percent();
    public static Percent New(Number value) => new Percent(value);
    public static implicit operator Number(Percent self) => self.Value;
    public static implicit operator Percent(Number value) => new Percent(value);
}
public partial class Quaternion: Value<Quaternion>
{
    public Number X { get; }
    public Number Y { get; }
    public Number Z { get; }
    public Number W { get; }
    public Quaternion WithX(Number x) => (x, Y, Z, W);
    public Quaternion WithY(Number y) => (X, y, Z, W);
    public Quaternion WithZ(Number z) => (X, Y, z, W);
    public Quaternion WithW(Number w) => (X, Y, Z, w);
    public Quaternion(Number x, Number y, Number z, Number w) => (X, Y, Z, W) = (x, y, z, w);
    public Quaternion() { }
    public static Quaternion Default = new Quaternion();
    public static Quaternion New(Number x, Number y, Number z, Number w) => new Quaternion(x, y, z, w);
    public static implicit operator (Number, Number, Number, Number)(Quaternion self) => (self.X, self.Y, self.Z, self.W);
    public static implicit operator Quaternion((Number, Number, Number, Number) value) => new Quaternion(value.Item1, value.Item2, value.Item3, value.Item4);
    public void Deconstruct(out Number x, out Number y, out Number z, out Number w) { x = X; y = Y; z = Z; w = W; }
}
public partial class Unit2D: Value<Unit2D>
{
    public Unit X { get; }
    public Unit Y { get; }
    public Unit2D WithX(Unit x) => (x, Y);
    public Unit2D WithY(Unit y) => (X, y);
    public Unit2D(Unit x, Unit y) => (X, Y) = (x, y);
    public Unit2D() { }
    public static Unit2D Default = new Unit2D();
    public static Unit2D New(Unit x, Unit y) => new Unit2D(x, y);
    public static implicit operator (Unit, Unit)(Unit2D self) => (self.X, self.Y);
    public static implicit operator Unit2D((Unit, Unit) value) => new Unit2D(value.Item1, value.Item2);
    public void Deconstruct(out Unit x, out Unit y) { x = X; y = Y; }
}
public partial class Unit3D: Value<Unit3D>
{
    public Unit X { get; }
    public Unit Y { get; }
    public Unit Z { get; }
    public Unit3D WithX(Unit x) => (x, Y, Z);
    public Unit3D WithY(Unit y) => (X, y, Z);
    public Unit3D WithZ(Unit z) => (X, Y, z);
    public Unit3D(Unit x, Unit y, Unit z) => (X, Y, Z) = (x, y, z);
    public Unit3D() { }
    public static Unit3D Default = new Unit3D();
    public static Unit3D New(Unit x, Unit y, Unit z) => new Unit3D(x, y, z);
    public static implicit operator (Unit, Unit, Unit)(Unit3D self) => (self.X, self.Y, self.Z);
    public static implicit operator Unit3D((Unit, Unit, Unit) value) => new Unit3D(value.Item1, value.Item2, value.Item3);
    public void Deconstruct(out Unit x, out Unit y, out Unit z) { x = X; y = Y; z = Z; }
}
public partial class Direction3D: Value<Direction3D>
{
    public Unit3D Value { get; }
    public Direction3D WithValue(Unit3D value) => (value);
    public Direction3D(Unit3D value) => (Value) = (value);
    public Direction3D() { }
    public static Direction3D Default = new Direction3D();
    public static Direction3D New(Unit3D value) => new Direction3D(value);
    public static implicit operator Unit3D(Direction3D self) => self.Value;
    public static implicit operator Direction3D(Unit3D value) => new Direction3D(value);
}
public partial class AxisAngle: Value<AxisAngle>
{
    public Unit3D Axis { get; }
    public Angle Angle { get; }
    public AxisAngle WithAxis(Unit3D axis) => (axis, Angle);
    public AxisAngle WithAngle(Angle angle) => (Axis, angle);
    public AxisAngle(Unit3D axis, Angle angle) => (Axis, Angle) = (axis, angle);
    public AxisAngle() { }
    public static AxisAngle Default = new AxisAngle();
    public static AxisAngle New(Unit3D axis, Angle angle) => new AxisAngle(axis, angle);
    public static implicit operator (Unit3D, Angle)(AxisAngle self) => (self.Axis, self.Angle);
    public static implicit operator AxisAngle((Unit3D, Angle) value) => new AxisAngle(value.Item1, value.Item2);
    public void Deconstruct(out Unit3D axis, out Angle angle) { axis = Axis; angle = Angle; }
}
public partial class EulerAngles: Value<EulerAngles>
{
    public Angle Yaw { get; }
    public Angle Pitch { get; }
    public Angle Roll { get; }
    public EulerAngles WithYaw(Angle yaw) => (yaw, Pitch, Roll);
    public EulerAngles WithPitch(Angle pitch) => (Yaw, pitch, Roll);
    public EulerAngles WithRoll(Angle roll) => (Yaw, Pitch, roll);
    public EulerAngles(Angle yaw, Angle pitch, Angle roll) => (Yaw, Pitch, Roll) = (yaw, pitch, roll);
    public EulerAngles() { }
    public static EulerAngles Default = new EulerAngles();
    public static EulerAngles New(Angle yaw, Angle pitch, Angle roll) => new EulerAngles(yaw, pitch, roll);
    public static implicit operator (Angle, Angle, Angle)(EulerAngles self) => (self.Yaw, self.Pitch, self.Roll);
    public static implicit operator EulerAngles((Angle, Angle, Angle) value) => new EulerAngles(value.Item1, value.Item2, value.Item3);
    public void Deconstruct(out Angle yaw, out Angle pitch, out Angle roll) { yaw = Yaw; pitch = Pitch; roll = Roll; }
}
public partial class Rotation3D: Value<Rotation3D>
{
    public Quaternion Quaternion { get; }
    public Rotation3D WithQuaternion(Quaternion quaternion) => (quaternion);
    public Rotation3D(Quaternion quaternion) => (Quaternion) = (quaternion);
    public Rotation3D() { }
    public static Rotation3D Default = new Rotation3D();
    public static Rotation3D New(Quaternion quaternion) => new Rotation3D(quaternion);
    public static implicit operator Quaternion(Rotation3D self) => self.Quaternion;
    public static implicit operator Rotation3D(Quaternion value) => new Rotation3D(value);
}
public partial class Vector2D: Vector<Vector2D>
{
    public Number X { get; }
    public Number Y { get; }
    public Vector2D WithX(Number x) => (x, Y);
    public Vector2D WithY(Number y) => (X, y);
    public Vector2D(Number x, Number y) => (X, Y) = (x, y);
    public Vector2D() { }
    public static Vector2D Default = new Vector2D();
    public static Vector2D New(Number x, Number y) => new Vector2D(x, y);
    public static implicit operator (Number, Number)(Vector2D self) => (self.X, self.Y);
    public static implicit operator Vector2D((Number, Number) value) => new Vector2D(value.Item1, value.Item2);
    public void Deconstruct(out Number x, out Number y) { x = X; y = Y; }
}
public partial class Vector3D: Vector<Vector3D>
{
    public Number X { get; }
    public Number Y { get; }
    public Number Z { get; }
    public Vector3D WithX(Number x) => (x, Y, Z);
    public Vector3D WithY(Number y) => (X, y, Z);
    public Vector3D WithZ(Number z) => (X, Y, z);
    public Vector3D(Number x, Number y, Number z) => (X, Y, Z) = (x, y, z);
    public Vector3D() { }
    public static Vector3D Default = new Vector3D();
    public static Vector3D New(Number x, Number y, Number z) => new Vector3D(x, y, z);
    public static implicit operator (Number, Number, Number)(Vector3D self) => (self.X, self.Y, self.Z);
    public static implicit operator Vector3D((Number, Number, Number) value) => new Vector3D(value.Item1, value.Item2, value.Item3);
    public void Deconstruct(out Number x, out Number y, out Number z) { x = X; y = Y; z = Z; }
}
public partial class Vector4D: Vector<Vector4D>
{
    public Number X { get; }
    public Number Y { get; }
    public Number Z { get; }
    public Number W { get; }
    public Vector4D WithX(Number x) => (x, Y, Z, W);
    public Vector4D WithY(Number y) => (X, y, Z, W);
    public Vector4D WithZ(Number z) => (X, Y, z, W);
    public Vector4D WithW(Number w) => (X, Y, Z, w);
    public Vector4D(Number x, Number y, Number z, Number w) => (X, Y, Z, W) = (x, y, z, w);
    public Vector4D() { }
    public static Vector4D Default = new Vector4D();
    public static Vector4D New(Number x, Number y, Number z, Number w) => new Vector4D(x, y, z, w);
    public static implicit operator (Number, Number, Number, Number)(Vector4D self) => (self.X, self.Y, self.Z, self.W);
    public static implicit operator Vector4D((Number, Number, Number, Number) value) => new Vector4D(value.Item1, value.Item2, value.Item3, value.Item4);
    public void Deconstruct(out Number x, out Number y, out Number z, out Number w) { x = X; y = Y; z = Z; w = W; }
}
public partial class Orientation3D: Value<Orientation3D>
{
    public Rotation3D Value { get; }
    public Orientation3D WithValue(Rotation3D value) => (value);
    public Orientation3D(Rotation3D value) => (Value) = (value);
    public Orientation3D() { }
    public static Orientation3D Default = new Orientation3D();
    public static Orientation3D New(Rotation3D value) => new Orientation3D(value);
    public static implicit operator Rotation3D(Orientation3D self) => self.Value;
    public static implicit operator Orientation3D(Rotation3D value) => new Orientation3D(value);
}
public partial class Pose2D: Value<Pose2D>
{
    public Vector3D Position { get; }
    public Orientation3D Orientation { get; }
    public Pose2D WithPosition(Vector3D position) => (position, Orientation);
    public Pose2D WithOrientation(Orientation3D orientation) => (Position, orientation);
    public Pose2D(Vector3D position, Orientation3D orientation) => (Position, Orientation) = (position, orientation);
    public Pose2D() { }
    public static Pose2D Default = new Pose2D();
    public static Pose2D New(Vector3D position, Orientation3D orientation) => new Pose2D(position, orientation);
    public static implicit operator (Vector3D, Orientation3D)(Pose2D self) => (self.Position, self.Orientation);
    public static implicit operator Pose2D((Vector3D, Orientation3D) value) => new Pose2D(value.Item1, value.Item2);
    public void Deconstruct(out Vector3D position, out Orientation3D orientation) { position = Position; orientation = Orientation; }
}
public partial class Pose3D: Value<Pose3D>
{
    public Vector3D Position { get; }
    public Orientation3D Orientation { get; }
    public Pose3D WithPosition(Vector3D position) => (position, Orientation);
    public Pose3D WithOrientation(Orientation3D orientation) => (Position, orientation);
    public Pose3D(Vector3D position, Orientation3D orientation) => (Position, Orientation) = (position, orientation);
    public Pose3D() { }
    public static Pose3D Default = new Pose3D();
    public static Pose3D New(Vector3D position, Orientation3D orientation) => new Pose3D(position, orientation);
    public static implicit operator (Vector3D, Orientation3D)(Pose3D self) => (self.Position, self.Orientation);
    public static implicit operator Pose3D((Vector3D, Orientation3D) value) => new Pose3D(value.Item1, value.Item2);
    public void Deconstruct(out Vector3D position, out Orientation3D orientation) { position = Position; orientation = Orientation; }
}
public partial class Transform3D: Value<Transform3D>
{
    public Vector3D Translation { get; }
    public Rotation3D Rotation { get; }
    public Vector3D Scale { get; }
    public Transform3D WithTranslation(Vector3D translation) => (translation, Rotation, Scale);
    public Transform3D WithRotation(Rotation3D rotation) => (Translation, rotation, Scale);
    public Transform3D WithScale(Vector3D scale) => (Translation, Rotation, scale);
    public Transform3D(Vector3D translation, Rotation3D rotation, Vector3D scale) => (Translation, Rotation, Scale) = (translation, rotation, scale);
    public Transform3D() { }
    public static Transform3D Default = new Transform3D();
    public static Transform3D New(Vector3D translation, Rotation3D rotation, Vector3D scale) => new Transform3D(translation, rotation, scale);
    public static implicit operator (Vector3D, Rotation3D, Vector3D)(Transform3D self) => (self.Translation, self.Rotation, self.Scale);
    public static implicit operator Transform3D((Vector3D, Rotation3D, Vector3D) value) => new Transform3D(value.Item1, value.Item2, value.Item3);
    public void Deconstruct(out Vector3D translation, out Rotation3D rotation, out Vector3D scale) { translation = Translation; rotation = Rotation; scale = Scale; }
}
public partial class Transform2D: Value<Transform2D>
{
    public Vector2D Translation { get; }
    public Angle Rotation { get; }
    public Vector2D Scale { get; }
    public Transform2D WithTranslation(Vector2D translation) => (translation, Rotation, Scale);
    public Transform2D WithRotation(Angle rotation) => (Translation, rotation, Scale);
    public Transform2D WithScale(Vector2D scale) => (Translation, Rotation, scale);
    public Transform2D(Vector2D translation, Angle rotation, Vector2D scale) => (Translation, Rotation, Scale) = (translation, rotation, scale);
    public Transform2D() { }
    public static Transform2D Default = new Transform2D();
    public static Transform2D New(Vector2D translation, Angle rotation, Vector2D scale) => new Transform2D(translation, rotation, scale);
    public static implicit operator (Vector2D, Angle, Vector2D)(Transform2D self) => (self.Translation, self.Rotation, self.Scale);
    public static implicit operator Transform2D((Vector2D, Angle, Vector2D) value) => new Transform2D(value.Item1, value.Item2, value.Item3);
    public void Deconstruct(out Vector2D translation, out Angle rotation, out Vector2D scale) { translation = Translation; rotation = Rotation; scale = Scale; }
}
public partial class AlignedBox2D: Interval<Point2D, Vector2D>
{
    public Point2D A { get; }
    public Point2D B { get; }
    public AlignedBox2D WithA(Point2D a) => (a, B);
    public AlignedBox2D WithB(Point2D b) => (A, b);
    public AlignedBox2D(Point2D a, Point2D b) => (A, B) = (a, b);
    public AlignedBox2D() { }
    public static AlignedBox2D Default = new AlignedBox2D();
    public static AlignedBox2D New(Point2D a, Point2D b) => new AlignedBox2D(a, b);
    public static implicit operator (Point2D, Point2D)(AlignedBox2D self) => (self.A, self.B);
    public static implicit operator AlignedBox2D((Point2D, Point2D) value) => new AlignedBox2D(value.Item1, value.Item2);
    public void Deconstruct(out Point2D a, out Point2D b) { a = A; b = B; }
}
public partial class AlignedBox3D: Interval<Point3D, Vector3D>
{
    public Point3D A { get; }
    public Point3D B { get; }
    public AlignedBox3D WithA(Point3D a) => (a, B);
    public AlignedBox3D WithB(Point3D b) => (A, b);
    public AlignedBox3D(Point3D a, Point3D b) => (A, B) = (a, b);
    public AlignedBox3D() { }
    public static AlignedBox3D Default = new AlignedBox3D();
    public static AlignedBox3D New(Point3D a, Point3D b) => new AlignedBox3D(a, b);
    public static implicit operator (Point3D, Point3D)(AlignedBox3D self) => (self.A, self.B);
    public static implicit operator AlignedBox3D((Point3D, Point3D) value) => new AlignedBox3D(value.Item1, value.Item2);
    public void Deconstruct(out Point3D a, out Point3D b) { a = A; b = B; }
}
public partial class Complex: Vector<Complex>
{
    public Number Real { get; }
    public Number Imaginary { get; }
    public Complex WithReal(Number real) => (real, Imaginary);
    public Complex WithImaginary(Number imaginary) => (Real, imaginary);
    public Complex(Number real, Number imaginary) => (Real, Imaginary) = (real, imaginary);
    public Complex() { }
    public static Complex Default = new Complex();
    public static Complex New(Number real, Number imaginary) => new Complex(real, imaginary);
    public static implicit operator (Number, Number)(Complex self) => (self.Real, self.Imaginary);
    public static implicit operator Complex((Number, Number) value) => new Complex(value.Item1, value.Item2);
    public void Deconstruct(out Number real, out Number imaginary) { real = Real; imaginary = Imaginary; }
}
public partial class Ray3D: Value<Ray3D>
{
    public Vector3D Direction { get; }
    public Point3D Position { get; }
    public Ray3D WithDirection(Vector3D direction) => (direction, Position);
    public Ray3D WithPosition(Point3D position) => (Direction, position);
    public Ray3D(Vector3D direction, Point3D position) => (Direction, Position) = (direction, position);
    public Ray3D() { }
    public static Ray3D Default = new Ray3D();
    public static Ray3D New(Vector3D direction, Point3D position) => new Ray3D(direction, position);
    public static implicit operator (Vector3D, Point3D)(Ray3D self) => (self.Direction, self.Position);
    public static implicit operator Ray3D((Vector3D, Point3D) value) => new Ray3D(value.Item1, value.Item2);
    public void Deconstruct(out Vector3D direction, out Point3D position) { direction = Direction; position = Position; }
}
public partial class Ray2D: Value<Ray2D>
{
    public Vector2D Direction { get; }
    public Point2D Position { get; }
    public Ray2D WithDirection(Vector2D direction) => (direction, Position);
    public Ray2D WithPosition(Point2D position) => (Direction, position);
    public Ray2D(Vector2D direction, Point2D position) => (Direction, Position) = (direction, position);
    public Ray2D() { }
    public static Ray2D Default = new Ray2D();
    public static Ray2D New(Vector2D direction, Point2D position) => new Ray2D(direction, position);
    public static implicit operator (Vector2D, Point2D)(Ray2D self) => (self.Direction, self.Position);
    public static implicit operator Ray2D((Vector2D, Point2D) value) => new Ray2D(value.Item1, value.Item2);
    public void Deconstruct(out Vector2D direction, out Point2D position) { direction = Direction; position = Position; }
}
public partial class Sphere: Value<Sphere>
{
    public Point3D Center { get; }
    public Number Radius { get; }
    public Sphere WithCenter(Point3D center) => (center, Radius);
    public Sphere WithRadius(Number radius) => (Center, radius);
    public Sphere(Point3D center, Number radius) => (Center, Radius) = (center, radius);
    public Sphere() { }
    public static Sphere Default = new Sphere();
    public static Sphere New(Point3D center, Number radius) => new Sphere(center, radius);
    public static implicit operator (Point3D, Number)(Sphere self) => (self.Center, self.Radius);
    public static implicit operator Sphere((Point3D, Number) value) => new Sphere(value.Item1, value.Item2);
    public void Deconstruct(out Point3D center, out Number radius) { center = Center; radius = Radius; }
}
public partial class Plane: Value<Plane>
{
    public Unit3D Normal { get; }
    public Number D { get; }
    public Plane WithNormal(Unit3D normal) => (normal, D);
    public Plane WithD(Number d) => (Normal, d);
    public Plane(Unit3D normal, Number d) => (Normal, D) = (normal, d);
    public Plane() { }
    public static Plane Default = new Plane();
    public static Plane New(Unit3D normal, Number d) => new Plane(normal, d);
    public static implicit operator (Unit3D, Number)(Plane self) => (self.Normal, self.D);
    public static implicit operator Plane((Unit3D, Number) value) => new Plane(value.Item1, value.Item2);
    public void Deconstruct(out Unit3D normal, out Number d) { normal = Normal; d = D; }
}
public partial class Triangle2D: Value<Triangle2D>
{
    public Point2D A { get; }
    public Point2D B { get; }
    public Point2D C { get; }
    public Triangle2D WithA(Point2D a) => (a, B, C);
    public Triangle2D WithB(Point2D b) => (A, b, C);
    public Triangle2D WithC(Point2D c) => (A, B, c);
    public Triangle2D(Point2D a, Point2D b, Point2D c) => (A, B, C) = (a, b, c);
    public Triangle2D() { }
    public static Triangle2D Default = new Triangle2D();
    public static Triangle2D New(Point2D a, Point2D b, Point2D c) => new Triangle2D(a, b, c);
    public static implicit operator (Point2D, Point2D, Point2D)(Triangle2D self) => (self.A, self.B, self.C);
    public static implicit operator Triangle2D((Point2D, Point2D, Point2D) value) => new Triangle2D(value.Item1, value.Item2, value.Item3);
    public void Deconstruct(out Point2D a, out Point2D b, out Point2D c) { a = A; b = B; c = C; }
}
public partial class Triangle3D: Value<Triangle3D>
{
    public Point3D A { get; }
    public Point3D B { get; }
    public Point3D C { get; }
    public Triangle3D WithA(Point3D a) => (a, B, C);
    public Triangle3D WithB(Point3D b) => (A, b, C);
    public Triangle3D WithC(Point3D c) => (A, B, c);
    public Triangle3D(Point3D a, Point3D b, Point3D c) => (A, B, C) = (a, b, c);
    public Triangle3D() { }
    public static Triangle3D Default = new Triangle3D();
    public static Triangle3D New(Point3D a, Point3D b, Point3D c) => new Triangle3D(a, b, c);
    public static implicit operator (Point3D, Point3D, Point3D)(Triangle3D self) => (self.A, self.B, self.C);
    public static implicit operator Triangle3D((Point3D, Point3D, Point3D) value) => new Triangle3D(value.Item1, value.Item2, value.Item3);
    public void Deconstruct(out Point3D a, out Point3D b, out Point3D c) { a = A; b = B; c = C; }
}
public partial class Quad2D: Value<Quad2D>
{
    public Point2D A { get; }
    public Point2D B { get; }
    public Point2D C { get; }
    public Point2D D { get; }
    public Quad2D WithA(Point2D a) => (a, B, C, D);
    public Quad2D WithB(Point2D b) => (A, b, C, D);
    public Quad2D WithC(Point2D c) => (A, B, c, D);
    public Quad2D WithD(Point2D d) => (A, B, C, d);
    public Quad2D(Point2D a, Point2D b, Point2D c, Point2D d) => (A, B, C, D) = (a, b, c, d);
    public Quad2D() { }
    public static Quad2D Default = new Quad2D();
    public static Quad2D New(Point2D a, Point2D b, Point2D c, Point2D d) => new Quad2D(a, b, c, d);
    public static implicit operator (Point2D, Point2D, Point2D, Point2D)(Quad2D self) => (self.A, self.B, self.C, self.D);
    public static implicit operator Quad2D((Point2D, Point2D, Point2D, Point2D) value) => new Quad2D(value.Item1, value.Item2, value.Item3, value.Item4);
    public void Deconstruct(out Point2D a, out Point2D b, out Point2D c, out Point2D d) { a = A; b = B; c = C; d = D; }
}
public partial class Quad3D: Value<Quad3D>
{
    public Point3D A { get; }
    public Point3D B { get; }
    public Point3D C { get; }
    public Point3D D { get; }
    public Quad3D WithA(Point3D a) => (a, B, C, D);
    public Quad3D WithB(Point3D b) => (A, b, C, D);
    public Quad3D WithC(Point3D c) => (A, B, c, D);
    public Quad3D WithD(Point3D d) => (A, B, C, d);
    public Quad3D(Point3D a, Point3D b, Point3D c, Point3D d) => (A, B, C, D) = (a, b, c, d);
    public Quad3D() { }
    public static Quad3D Default = new Quad3D();
    public static Quad3D New(Point3D a, Point3D b, Point3D c, Point3D d) => new Quad3D(a, b, c, d);
    public static implicit operator (Point3D, Point3D, Point3D, Point3D)(Quad3D self) => (self.A, self.B, self.C, self.D);
    public static implicit operator Quad3D((Point3D, Point3D, Point3D, Point3D) value) => new Quad3D(value.Item1, value.Item2, value.Item3, value.Item4);
    public void Deconstruct(out Point3D a, out Point3D b, out Point3D c, out Point3D d) { a = A; b = B; c = C; d = D; }
}
public partial class Point2D: Coordinate<Point2D>
{
    public Number X { get; }
    public Number Y { get; }
    public Point2D WithX(Number x) => (x, Y);
    public Point2D WithY(Number y) => (X, y);
    public Point2D(Number x, Number y) => (X, Y) = (x, y);
    public Point2D() { }
    public static Point2D Default = new Point2D();
    public static Point2D New(Number x, Number y) => new Point2D(x, y);
    public static implicit operator (Number, Number)(Point2D self) => (self.X, self.Y);
    public static implicit operator Point2D((Number, Number) value) => new Point2D(value.Item1, value.Item2);
    public void Deconstruct(out Number x, out Number y) { x = X; y = Y; }
}
public partial class Point3D: Coordinate<Point3D>
{
    public Number X { get; }
    public Number Y { get; }
    public Number Z { get; }
    public Point3D WithX(Number x) => (x, Y, Z);
    public Point3D WithY(Number y) => (X, y, Z);
    public Point3D WithZ(Number z) => (X, Y, z);
    public Point3D(Number x, Number y, Number z) => (X, Y, Z) = (x, y, z);
    public Point3D() { }
    public static Point3D Default = new Point3D();
    public static Point3D New(Number x, Number y, Number z) => new Point3D(x, y, z);
    public static implicit operator (Number, Number, Number)(Point3D self) => (self.X, self.Y, self.Z);
    public static implicit operator Point3D((Number, Number, Number) value) => new Point3D(value.Item1, value.Item2, value.Item3);
    public void Deconstruct(out Number x, out Number y, out Number z) { x = X; y = Y; z = Z; }
}
public partial class Line2D: Interval<Point2D, Vector3D>
{
    public Point2D A { get; }
    public Point2D B { get; }
    public Line2D WithA(Point2D a) => (a, B);
    public Line2D WithB(Point2D b) => (A, b);
    public Line2D(Point2D a, Point2D b) => (A, B) = (a, b);
    public Line2D() { }
    public static Line2D Default = new Line2D();
    public static Line2D New(Point2D a, Point2D b) => new Line2D(a, b);
    public static implicit operator (Point2D, Point2D)(Line2D self) => (self.A, self.B);
    public static implicit operator Line2D((Point2D, Point2D) value) => new Line2D(value.Item1, value.Item2);
    public void Deconstruct(out Point2D a, out Point2D b) { a = A; b = B; }
}
public partial class Line3D: Interval<Point3D, Vector3D>
{
    public Point3D A { get; }
    public Point3D B { get; }
    public Line3D WithA(Point3D a) => (a, B);
    public Line3D WithB(Point3D b) => (A, b);
    public Line3D(Point3D a, Point3D b) => (A, B) = (a, b);
    public Line3D() { }
    public static Line3D Default = new Line3D();
    public static Line3D New(Point3D a, Point3D b) => new Line3D(a, b);
    public static implicit operator (Point3D, Point3D)(Line3D self) => (self.A, self.B);
    public static implicit operator Line3D((Point3D, Point3D) value) => new Line3D(value.Item1, value.Item2);
    public void Deconstruct(out Point3D a, out Point3D b) { a = A; b = B; }
}
public partial class Color: Value<Color>
{
    public Unit R { get; }
    public Unit G { get; }
    public Unit B { get; }
    public Unit A { get; }
    public Color WithR(Unit r) => (r, G, B, A);
    public Color WithG(Unit g) => (R, g, B, A);
    public Color WithB(Unit b) => (R, G, b, A);
    public Color WithA(Unit a) => (R, G, B, a);
    public Color(Unit r, Unit g, Unit b, Unit a) => (R, G, B, A) = (r, g, b, a);
    public Color() { }
    public static Color Default = new Color();
    public static Color New(Unit r, Unit g, Unit b, Unit a) => new Color(r, g, b, a);
    public static implicit operator (Unit, Unit, Unit, Unit)(Color self) => (self.R, self.G, self.B, self.A);
    public static implicit operator Color((Unit, Unit, Unit, Unit) value) => new Color(value.Item1, value.Item2, value.Item3, value.Item4);
    public void Deconstruct(out Unit r, out Unit g, out Unit b, out Unit a) { r = R; g = G; b = B; a = A; }
}
public partial class ColorLUV: Value<ColorLUV>
{
    public Percent Lightness { get; }
    public Unit U { get; }
    public Unit V { get; }
    public ColorLUV WithLightness(Percent lightness) => (lightness, U, V);
    public ColorLUV WithU(Unit u) => (Lightness, u, V);
    public ColorLUV WithV(Unit v) => (Lightness, U, v);
    public ColorLUV(Percent lightness, Unit u, Unit v) => (Lightness, U, V) = (lightness, u, v);
    public ColorLUV() { }
    public static ColorLUV Default = new ColorLUV();
    public static ColorLUV New(Percent lightness, Unit u, Unit v) => new ColorLUV(lightness, u, v);
    public static implicit operator (Percent, Unit, Unit)(ColorLUV self) => (self.Lightness, self.U, self.V);
    public static implicit operator ColorLUV((Percent, Unit, Unit) value) => new ColorLUV(value.Item1, value.Item2, value.Item3);
    public void Deconstruct(out Percent lightness, out Unit u, out Unit v) { lightness = Lightness; u = U; v = V; }
}
public partial class ColorLAB: Value<ColorLAB>
{
    public Percent Lightness { get; }
    public Integer A { get; }
    public Integer B { get; }
    public ColorLAB WithLightness(Percent lightness) => (lightness, A, B);
    public ColorLAB WithA(Integer a) => (Lightness, a, B);
    public ColorLAB WithB(Integer b) => (Lightness, A, b);
    public ColorLAB(Percent lightness, Integer a, Integer b) => (Lightness, A, B) = (lightness, a, b);
    public ColorLAB() { }
    public static ColorLAB Default = new ColorLAB();
    public static ColorLAB New(Percent lightness, Integer a, Integer b) => new ColorLAB(lightness, a, b);
    public static implicit operator (Percent, Integer, Integer)(ColorLAB self) => (self.Lightness, self.A, self.B);
    public static implicit operator ColorLAB((Percent, Integer, Integer) value) => new ColorLAB(value.Item1, value.Item2, value.Item3);
    public void Deconstruct(out Percent lightness, out Integer a, out Integer b) { lightness = Lightness; a = A; b = B; }
}
public partial class ColorLCh: Value<ColorLCh>
{
    public Percent Lightness { get; }
    public PolarCoordinate ChromaHue { get; }
    public ColorLCh WithLightness(Percent lightness) => (lightness, ChromaHue);
    public ColorLCh WithChromaHue(PolarCoordinate chromaHue) => (Lightness, chromaHue);
    public ColorLCh(Percent lightness, PolarCoordinate chromaHue) => (Lightness, ChromaHue) = (lightness, chromaHue);
    public ColorLCh() { }
    public static ColorLCh Default = new ColorLCh();
    public static ColorLCh New(Percent lightness, PolarCoordinate chromaHue) => new ColorLCh(lightness, chromaHue);
    public static implicit operator (Percent, PolarCoordinate)(ColorLCh self) => (self.Lightness, self.ChromaHue);
    public static implicit operator ColorLCh((Percent, PolarCoordinate) value) => new ColorLCh(value.Item1, value.Item2);
    public void Deconstruct(out Percent lightness, out PolarCoordinate chromaHue) { lightness = Lightness; chromaHue = ChromaHue; }
}
public partial class ColorHSV: Value<ColorHSV>
{
    public Angle Hue { get; }
    public Unit S { get; }
    public Unit V { get; }
    public ColorHSV WithHue(Angle hue) => (hue, S, V);
    public ColorHSV WithS(Unit s) => (Hue, s, V);
    public ColorHSV WithV(Unit v) => (Hue, S, v);
    public ColorHSV(Angle hue, Unit s, Unit v) => (Hue, S, V) = (hue, s, v);
    public ColorHSV() { }
    public static ColorHSV Default = new ColorHSV();
    public static ColorHSV New(Angle hue, Unit s, Unit v) => new ColorHSV(hue, s, v);
    public static implicit operator (Angle, Unit, Unit)(ColorHSV self) => (self.Hue, self.S, self.V);
    public static implicit operator ColorHSV((Angle, Unit, Unit) value) => new ColorHSV(value.Item1, value.Item2, value.Item3);
    public void Deconstruct(out Angle hue, out Unit s, out Unit v) { hue = Hue; s = S; v = V; }
}
public partial class ColorHSL: Value<ColorHSL>
{
    public Angle Hue { get; }
    public Unit Saturation { get; }
    public Unit Luminance { get; }
    public ColorHSL WithHue(Angle hue) => (hue, Saturation, Luminance);
    public ColorHSL WithSaturation(Unit saturation) => (Hue, saturation, Luminance);
    public ColorHSL WithLuminance(Unit luminance) => (Hue, Saturation, luminance);
    public ColorHSL(Angle hue, Unit saturation, Unit luminance) => (Hue, Saturation, Luminance) = (hue, saturation, luminance);
    public ColorHSL() { }
    public static ColorHSL Default = new ColorHSL();
    public static ColorHSL New(Angle hue, Unit saturation, Unit luminance) => new ColorHSL(hue, saturation, luminance);
    public static implicit operator (Angle, Unit, Unit)(ColorHSL self) => (self.Hue, self.Saturation, self.Luminance);
    public static implicit operator ColorHSL((Angle, Unit, Unit) value) => new ColorHSL(value.Item1, value.Item2, value.Item3);
    public void Deconstruct(out Angle hue, out Unit saturation, out Unit luminance) { hue = Hue; saturation = Saturation; luminance = Luminance; }
}
public partial class ColorYCbCr: Value<ColorYCbCr>
{
    public Unit Y { get; }
    public Unit Cb { get; }
    public Unit Cr { get; }
    public ColorYCbCr WithY(Unit y) => (y, Cb, Cr);
    public ColorYCbCr WithCb(Unit cb) => (Y, cb, Cr);
    public ColorYCbCr WithCr(Unit cr) => (Y, Cb, cr);
    public ColorYCbCr(Unit y, Unit cb, Unit cr) => (Y, Cb, Cr) = (y, cb, cr);
    public ColorYCbCr() { }
    public static ColorYCbCr Default = new ColorYCbCr();
    public static ColorYCbCr New(Unit y, Unit cb, Unit cr) => new ColorYCbCr(y, cb, cr);
    public static implicit operator (Unit, Unit, Unit)(ColorYCbCr self) => (self.Y, self.Cb, self.Cr);
    public static implicit operator ColorYCbCr((Unit, Unit, Unit) value) => new ColorYCbCr(value.Item1, value.Item2, value.Item3);
    public void Deconstruct(out Unit y, out Unit cb, out Unit cr) { y = Y; cb = Cb; cr = Cr; }
}
public partial class SphericalCoordinate: Value<SphericalCoordinate>
{
    public Number Radius { get; }
    public Angle Azimuth { get; }
    public Angle Polar { get; }
    public SphericalCoordinate WithRadius(Number radius) => (radius, Azimuth, Polar);
    public SphericalCoordinate WithAzimuth(Angle azimuth) => (Radius, azimuth, Polar);
    public SphericalCoordinate WithPolar(Angle polar) => (Radius, Azimuth, polar);
    public SphericalCoordinate(Number radius, Angle azimuth, Angle polar) => (Radius, Azimuth, Polar) = (radius, azimuth, polar);
    public SphericalCoordinate() { }
    public static SphericalCoordinate Default = new SphericalCoordinate();
    public static SphericalCoordinate New(Number radius, Angle azimuth, Angle polar) => new SphericalCoordinate(radius, azimuth, polar);
    public static implicit operator (Number, Angle, Angle)(SphericalCoordinate self) => (self.Radius, self.Azimuth, self.Polar);
    public static implicit operator SphericalCoordinate((Number, Angle, Angle) value) => new SphericalCoordinate(value.Item1, value.Item2, value.Item3);
    public void Deconstruct(out Number radius, out Angle azimuth, out Angle polar) { radius = Radius; azimuth = Azimuth; polar = Polar; }
}
public partial class PolarCoordinate: Value<PolarCoordinate>
{
    public Number Radius { get; }
    public Angle Angle { get; }
    public PolarCoordinate WithRadius(Number radius) => (radius, Angle);
    public PolarCoordinate WithAngle(Angle angle) => (Radius, angle);
    public PolarCoordinate(Number radius, Angle angle) => (Radius, Angle) = (radius, angle);
    public PolarCoordinate() { }
    public static PolarCoordinate Default = new PolarCoordinate();
    public static PolarCoordinate New(Number radius, Angle angle) => new PolarCoordinate(radius, angle);
    public static implicit operator (Number, Angle)(PolarCoordinate self) => (self.Radius, self.Angle);
    public static implicit operator PolarCoordinate((Number, Angle) value) => new PolarCoordinate(value.Item1, value.Item2);
    public void Deconstruct(out Number radius, out Angle angle) { radius = Radius; angle = Angle; }
}
public partial class LogPolarCoordinate: Value<LogPolarCoordinate>
{
    public Number Rho { get; }
    public Angle Azimuth { get; }
    public LogPolarCoordinate WithRho(Number rho) => (rho, Azimuth);
    public LogPolarCoordinate WithAzimuth(Angle azimuth) => (Rho, azimuth);
    public LogPolarCoordinate(Number rho, Angle azimuth) => (Rho, Azimuth) = (rho, azimuth);
    public LogPolarCoordinate() { }
    public static LogPolarCoordinate Default = new LogPolarCoordinate();
    public static LogPolarCoordinate New(Number rho, Angle azimuth) => new LogPolarCoordinate(rho, azimuth);
    public static implicit operator (Number, Angle)(LogPolarCoordinate self) => (self.Rho, self.Azimuth);
    public static implicit operator LogPolarCoordinate((Number, Angle) value) => new LogPolarCoordinate(value.Item1, value.Item2);
    public void Deconstruct(out Number rho, out Angle azimuth) { rho = Rho; azimuth = Azimuth; }
}
public partial class CylindricalCoordinate: Value<CylindricalCoordinate>
{
    public Number RadialDistance { get; }
    public Angle Azimuth { get; }
    public Number Height { get; }
    public CylindricalCoordinate WithRadialDistance(Number radialDistance) => (radialDistance, Azimuth, Height);
    public CylindricalCoordinate WithAzimuth(Angle azimuth) => (RadialDistance, azimuth, Height);
    public CylindricalCoordinate WithHeight(Number height) => (RadialDistance, Azimuth, height);
    public CylindricalCoordinate(Number radialDistance, Angle azimuth, Number height) => (RadialDistance, Azimuth, Height) = (radialDistance, azimuth, height);
    public CylindricalCoordinate() { }
    public static CylindricalCoordinate Default = new CylindricalCoordinate();
    public static CylindricalCoordinate New(Number radialDistance, Angle azimuth, Number height) => new CylindricalCoordinate(radialDistance, azimuth, height);
    public static implicit operator (Number, Angle, Number)(CylindricalCoordinate self) => (self.RadialDistance, self.Azimuth, self.Height);
    public static implicit operator CylindricalCoordinate((Number, Angle, Number) value) => new CylindricalCoordinate(value.Item1, value.Item2, value.Item3);
    public void Deconstruct(out Number radialDistance, out Angle azimuth, out Number height) { radialDistance = RadialDistance; azimuth = Azimuth; height = Height; }
}
public partial class HorizontalCoordinate: Value<HorizontalCoordinate>
{
    public Number Radius { get; }
    public Angle Azimuth { get; }
    public Number Height { get; }
    public HorizontalCoordinate WithRadius(Number radius) => (radius, Azimuth, Height);
    public HorizontalCoordinate WithAzimuth(Angle azimuth) => (Radius, azimuth, Height);
    public HorizontalCoordinate WithHeight(Number height) => (Radius, Azimuth, height);
    public HorizontalCoordinate(Number radius, Angle azimuth, Number height) => (Radius, Azimuth, Height) = (radius, azimuth, height);
    public HorizontalCoordinate() { }
    public static HorizontalCoordinate Default = new HorizontalCoordinate();
    public static HorizontalCoordinate New(Number radius, Angle azimuth, Number height) => new HorizontalCoordinate(radius, azimuth, height);
    public static implicit operator (Number, Angle, Number)(HorizontalCoordinate self) => (self.Radius, self.Azimuth, self.Height);
    public static implicit operator HorizontalCoordinate((Number, Angle, Number) value) => new HorizontalCoordinate(value.Item1, value.Item2, value.Item3);
    public void Deconstruct(out Number radius, out Angle azimuth, out Number height) { radius = Radius; azimuth = Azimuth; height = Height; }
}
public partial class GeoCoordinate: Value<GeoCoordinate>
{
    public Angle Latitude { get; }
    public Angle Longitude { get; }
    public GeoCoordinate WithLatitude(Angle latitude) => (latitude, Longitude);
    public GeoCoordinate WithLongitude(Angle longitude) => (Latitude, longitude);
    public GeoCoordinate(Angle latitude, Angle longitude) => (Latitude, Longitude) = (latitude, longitude);
    public GeoCoordinate() { }
    public static GeoCoordinate Default = new GeoCoordinate();
    public static GeoCoordinate New(Angle latitude, Angle longitude) => new GeoCoordinate(latitude, longitude);
    public static implicit operator (Angle, Angle)(GeoCoordinate self) => (self.Latitude, self.Longitude);
    public static implicit operator GeoCoordinate((Angle, Angle) value) => new GeoCoordinate(value.Item1, value.Item2);
    public void Deconstruct(out Angle latitude, out Angle longitude) { latitude = Latitude; longitude = Longitude; }
}
public partial class GeoCoordinateWithAltitude: Value<GeoCoordinateWithAltitude>
{
    public GeoCoordinate Coordinate { get; }
    public Number Altitude { get; }
    public GeoCoordinateWithAltitude WithCoordinate(GeoCoordinate coordinate) => (coordinate, Altitude);
    public GeoCoordinateWithAltitude WithAltitude(Number altitude) => (Coordinate, altitude);
    public GeoCoordinateWithAltitude(GeoCoordinate coordinate, Number altitude) => (Coordinate, Altitude) = (coordinate, altitude);
    public GeoCoordinateWithAltitude() { }
    public static GeoCoordinateWithAltitude Default = new GeoCoordinateWithAltitude();
    public static GeoCoordinateWithAltitude New(GeoCoordinate coordinate, Number altitude) => new GeoCoordinateWithAltitude(coordinate, altitude);
    public static implicit operator (GeoCoordinate, Number)(GeoCoordinateWithAltitude self) => (self.Coordinate, self.Altitude);
    public static implicit operator GeoCoordinateWithAltitude((GeoCoordinate, Number) value) => new GeoCoordinateWithAltitude(value.Item1, value.Item2);
    public void Deconstruct(out GeoCoordinate coordinate, out Number altitude) { coordinate = Coordinate; altitude = Altitude; }
}
public partial class Circle: Value<Circle>
{
    public Point2D Center { get; }
    public Number Radius { get; }
    public Circle WithCenter(Point2D center) => (center, Radius);
    public Circle WithRadius(Number radius) => (Center, radius);
    public Circle(Point2D center, Number radius) => (Center, Radius) = (center, radius);
    public Circle() { }
    public static Circle Default = new Circle();
    public static Circle New(Point2D center, Number radius) => new Circle(center, radius);
    public static implicit operator (Point2D, Number)(Circle self) => (self.Center, self.Radius);
    public static implicit operator Circle((Point2D, Number) value) => new Circle(value.Item1, value.Item2);
    public void Deconstruct(out Point2D center, out Number radius) { center = Center; radius = Radius; }
}
public partial class Chord: Value<Chord>
{
    public Circle Circle { get; }
    public Arc Arc { get; }
    public Chord WithCircle(Circle circle) => (circle, Arc);
    public Chord WithArc(Arc arc) => (Circle, arc);
    public Chord(Circle circle, Arc arc) => (Circle, Arc) = (circle, arc);
    public Chord() { }
    public static Chord Default = new Chord();
    public static Chord New(Circle circle, Arc arc) => new Chord(circle, arc);
    public static implicit operator (Circle, Arc)(Chord self) => (self.Circle, self.Arc);
    public static implicit operator Chord((Circle, Arc) value) => new Chord(value.Item1, value.Item2);
    public void Deconstruct(out Circle circle, out Arc arc) { circle = Circle; arc = Arc; }
}
public partial class Size2D: Value<Size2D>
{
    public Number Width { get; }
    public Number Height { get; }
    public Size2D WithWidth(Number width) => (width, Height);
    public Size2D WithHeight(Number height) => (Width, height);
    public Size2D(Number width, Number height) => (Width, Height) = (width, height);
    public Size2D() { }
    public static Size2D Default = new Size2D();
    public static Size2D New(Number width, Number height) => new Size2D(width, height);
    public static implicit operator (Number, Number)(Size2D self) => (self.Width, self.Height);
    public static implicit operator Size2D((Number, Number) value) => new Size2D(value.Item1, value.Item2);
    public void Deconstruct(out Number width, out Number height) { width = Width; height = Height; }
}
public partial class Size3D: Value<Size3D>
{
    public Number Width { get; }
    public Number Height { get; }
    public Number Depth { get; }
    public Size3D WithWidth(Number width) => (width, Height, Depth);
    public Size3D WithHeight(Number height) => (Width, height, Depth);
    public Size3D WithDepth(Number depth) => (Width, Height, depth);
    public Size3D(Number width, Number height, Number depth) => (Width, Height, Depth) = (width, height, depth);
    public Size3D() { }
    public static Size3D Default = new Size3D();
    public static Size3D New(Number width, Number height, Number depth) => new Size3D(width, height, depth);
    public static implicit operator (Number, Number, Number)(Size3D self) => (self.Width, self.Height, self.Depth);
    public static implicit operator Size3D((Number, Number, Number) value) => new Size3D(value.Item1, value.Item2, value.Item3);
    public void Deconstruct(out Number width, out Number height, out Number depth) { width = Width; height = Height; depth = Depth; }
}
public partial class Rectangle2D: Value<Rectangle2D>
{
    public Point2D Center { get; }
    public Size2D Size { get; }
    public Rectangle2D WithCenter(Point2D center) => (center, Size);
    public Rectangle2D WithSize(Size2D size) => (Center, size);
    public Rectangle2D(Point2D center, Size2D size) => (Center, Size) = (center, size);
    public Rectangle2D() { }
    public static Rectangle2D Default = new Rectangle2D();
    public static Rectangle2D New(Point2D center, Size2D size) => new Rectangle2D(center, size);
    public static implicit operator (Point2D, Size2D)(Rectangle2D self) => (self.Center, self.Size);
    public static implicit operator Rectangle2D((Point2D, Size2D) value) => new Rectangle2D(value.Item1, value.Item2);
    public void Deconstruct(out Point2D center, out Size2D size) { center = Center; size = Size; }
}
public partial class Proportion: Numerical<Proportion>
{
    public Number Value { get; }
    public Proportion WithValue(Number value) => (value);
    public Proportion(Number value) => (Value) = (value);
    public Proportion() { }
    public static Proportion Default = new Proportion();
    public static Proportion New(Number value) => new Proportion(value);
    public static implicit operator Number(Proportion self) => self.Value;
    public static implicit operator Proportion(Number value) => new Proportion(value);
}
public partial class Fraction: Value<Fraction>
{
    public Number Numerator { get; }
    public Number Denominator { get; }
    public Fraction WithNumerator(Number numerator) => (numerator, Denominator);
    public Fraction WithDenominator(Number denominator) => (Numerator, denominator);
    public Fraction(Number numerator, Number denominator) => (Numerator, Denominator) = (numerator, denominator);
    public Fraction() { }
    public static Fraction Default = new Fraction();
    public static Fraction New(Number numerator, Number denominator) => new Fraction(numerator, denominator);
    public static implicit operator (Number, Number)(Fraction self) => (self.Numerator, self.Denominator);
    public static implicit operator Fraction((Number, Number) value) => new Fraction(value.Item1, value.Item2);
    public void Deconstruct(out Number numerator, out Number denominator) { numerator = Numerator; denominator = Denominator; }
}
public partial class Angle: Coordinate<Angle>
{
    public Number Radians { get; }
    public Angle WithRadians(Number radians) => (radians);
    public Angle(Number radians) => (Radians) = (radians);
    public Angle() { }
    public static Angle Default = new Angle();
    public static Angle New(Number radians) => new Angle(radians);
    public static implicit operator Number(Angle self) => self.Radians;
    public static implicit operator Angle(Number value) => new Angle(value);
}
public partial class Length: Measure<Length>
{
    public Number Meters { get; }
    public Length WithMeters(Number meters) => (meters);
    public Length(Number meters) => (Meters) = (meters);
    public Length() { }
    public static Length Default = new Length();
    public static Length New(Number meters) => new Length(meters);
    public static implicit operator Number(Length self) => self.Meters;
    public static implicit operator Length(Number value) => new Length(value);
}
public partial class Mass: Measure<Mass>
{
    public Number Kilograms { get; }
    public Mass WithKilograms(Number kilograms) => (kilograms);
    public Mass(Number kilograms) => (Kilograms) = (kilograms);
    public Mass() { }
    public static Mass Default = new Mass();
    public static Mass New(Number kilograms) => new Mass(kilograms);
    public static implicit operator Number(Mass self) => self.Kilograms;
    public static implicit operator Mass(Number value) => new Mass(value);
}
public partial class Temperature: Measure<Temperature>
{
    public Number Celsius { get; }
    public Temperature WithCelsius(Number celsius) => (celsius);
    public Temperature(Number celsius) => (Celsius) = (celsius);
    public Temperature() { }
    public static Temperature Default = new Temperature();
    public static Temperature New(Number celsius) => new Temperature(celsius);
    public static implicit operator Number(Temperature self) => self.Celsius;
    public static implicit operator Temperature(Number value) => new Temperature(value);
}
public partial class Time: Measure<Time>
{
    public Number Seconds { get; }
    public Time WithSeconds(Number seconds) => (seconds);
    public Time(Number seconds) => (Seconds) = (seconds);
    public Time() { }
    public static Time Default = new Time();
    public static Time New(Number seconds) => new Time(seconds);
    public static implicit operator Number(Time self) => self.Seconds;
    public static implicit operator Time(Number value) => new Time(value);
}
public partial class TimeRange: Interval<DateTime, Time>
{
    public DateTime Begin { get; }
    public DateTime End { get; }
    public TimeRange WithBegin(DateTime begin) => (begin, End);
    public TimeRange WithEnd(DateTime end) => (Begin, end);
    public TimeRange(DateTime begin, DateTime end) => (Begin, End) = (begin, end);
    public TimeRange() { }
    public static TimeRange Default = new TimeRange();
    public static TimeRange New(DateTime begin, DateTime end) => new TimeRange(begin, end);
    public static implicit operator (DateTime, DateTime)(TimeRange self) => (self.Begin, self.End);
    public static implicit operator TimeRange((DateTime, DateTime) value) => new TimeRange(value.Item1, value.Item2);
    public void Deconstruct(out DateTime begin, out DateTime end) { begin = Begin; end = End; }
}
public partial class DateTime: Coordinate<DateTime>
{
    public Integer Year { get; }
    public Integer Month { get; }
    public Integer TimeZoneOffset { get; }
    public Integer Day { get; }
    public Integer Minute { get; }
    public Integer Second { get; }
    public Number Milliseconds { get; }
    public DateTime WithYear(Integer year) => (year, Month, TimeZoneOffset, Day, Minute, Second, Milliseconds);
    public DateTime WithMonth(Integer month) => (Year, month, TimeZoneOffset, Day, Minute, Second, Milliseconds);
    public DateTime WithTimeZoneOffset(Integer timeZoneOffset) => (Year, Month, timeZoneOffset, Day, Minute, Second, Milliseconds);
    public DateTime WithDay(Integer day) => (Year, Month, TimeZoneOffset, day, Minute, Second, Milliseconds);
    public DateTime WithMinute(Integer minute) => (Year, Month, TimeZoneOffset, Day, minute, Second, Milliseconds);
    public DateTime WithSecond(Integer second) => (Year, Month, TimeZoneOffset, Day, Minute, second, Milliseconds);
    public DateTime WithMilliseconds(Number milliseconds) => (Year, Month, TimeZoneOffset, Day, Minute, Second, milliseconds);
    public DateTime(Integer year, Integer month, Integer timeZoneOffset, Integer day, Integer minute, Integer second, Number milliseconds) => (Year, Month, TimeZoneOffset, Day, Minute, Second, Milliseconds) = (year, month, timeZoneOffset, day, minute, second, milliseconds);
    public DateTime() { }
    public static DateTime Default = new DateTime();
    public static DateTime New(Integer year, Integer month, Integer timeZoneOffset, Integer day, Integer minute, Integer second, Number milliseconds) => new DateTime(year, month, timeZoneOffset, day, minute, second, milliseconds);
    public static implicit operator (Integer, Integer, Integer, Integer, Integer, Integer, Number)(DateTime self) => (self.Year, self.Month, self.TimeZoneOffset, self.Day, self.Minute, self.Second, self.Milliseconds);
    public static implicit operator DateTime((Integer, Integer, Integer, Integer, Integer, Integer, Number) value) => new DateTime(value.Item1, value.Item2, value.Item3, value.Item4, value.Item5, value.Item6, value.Item7);
    public void Deconstruct(out Integer year, out Integer month, out Integer timeZoneOffset, out Integer day, out Integer minute, out Integer second, out Number milliseconds) { year = Year; month = Month; timeZoneOffset = TimeZoneOffset; day = Day; minute = Minute; second = Second; milliseconds = Milliseconds; }
}
public partial class AnglePair: Interval<Angle, Angle>
{
    public Angle Start { get; }
    public Angle End { get; }
    public AnglePair WithStart(Angle start) => (start, End);
    public AnglePair WithEnd(Angle end) => (Start, end);
    public AnglePair(Angle start, Angle end) => (Start, End) = (start, end);
    public AnglePair() { }
    public static AnglePair Default = new AnglePair();
    public static AnglePair New(Angle start, Angle end) => new AnglePair(start, end);
    public static implicit operator (Angle, Angle)(AnglePair self) => (self.Start, self.End);
    public static implicit operator AnglePair((Angle, Angle) value) => new AnglePair(value.Item1, value.Item2);
    public void Deconstruct(out Angle start, out Angle end) { start = Start; end = End; }
}
public partial class Ring: Value<Ring>
{
    public Circle Circle { get; }
    public Number InnerRadius { get; }
    public Ring WithCircle(Circle circle) => (circle, InnerRadius);
    public Ring WithInnerRadius(Number innerRadius) => (Circle, innerRadius);
    public Ring(Circle circle, Number innerRadius) => (Circle, InnerRadius) = (circle, innerRadius);
    public Ring() { }
    public static Ring Default = new Ring();
    public static Ring New(Circle circle, Number innerRadius) => new Ring(circle, innerRadius);
    public static implicit operator (Circle, Number)(Ring self) => (self.Circle, self.InnerRadius);
    public static implicit operator Ring((Circle, Number) value) => new Ring(value.Item1, value.Item2);
    public void Deconstruct(out Circle circle, out Number innerRadius) { circle = Circle; innerRadius = InnerRadius; }
}
public partial class Arc: Value<Arc>
{
    public AnglePair Angles { get; }
    public Circle Cirlce { get; }
    public Arc WithAngles(AnglePair angles) => (angles, Cirlce);
    public Arc WithCirlce(Circle cirlce) => (Angles, cirlce);
    public Arc(AnglePair angles, Circle cirlce) => (Angles, Cirlce) = (angles, cirlce);
    public Arc() { }
    public static Arc Default = new Arc();
    public static Arc New(AnglePair angles, Circle cirlce) => new Arc(angles, cirlce);
    public static implicit operator (AnglePair, Circle)(Arc self) => (self.Angles, self.Cirlce);
    public static implicit operator Arc((AnglePair, Circle) value) => new Arc(value.Item1, value.Item2);
    public void Deconstruct(out AnglePair angles, out Circle cirlce) { angles = Angles; cirlce = Cirlce; }
}
public partial class RealInterval: Interval<Number, Number>
{
    public Number A { get; }
    public Number B { get; }
    public RealInterval WithA(Number a) => (a, B);
    public RealInterval WithB(Number b) => (A, b);
    public RealInterval(Number a, Number b) => (A, B) = (a, b);
    public RealInterval() { }
    public static RealInterval Default = new RealInterval();
    public static RealInterval New(Number a, Number b) => new RealInterval(a, b);
    public static implicit operator (Number, Number)(RealInterval self) => (self.A, self.B);
    public static implicit operator RealInterval((Number, Number) value) => new RealInterval(value.Item1, value.Item2);
    public void Deconstruct(out Number a, out Number b) { a = A; b = B; }
}
public partial class Capsule: Value<Capsule>
{
    public Line3D Line { get; }
    public Number Radius { get; }
    public Capsule WithLine(Line3D line) => (line, Radius);
    public Capsule WithRadius(Number radius) => (Line, radius);
    public Capsule(Line3D line, Number radius) => (Line, Radius) = (line, radius);
    public Capsule() { }
    public static Capsule Default = new Capsule();
    public static Capsule New(Line3D line, Number radius) => new Capsule(line, radius);
    public static implicit operator (Line3D, Number)(Capsule self) => (self.Line, self.Radius);
    public static implicit operator Capsule((Line3D, Number) value) => new Capsule(value.Item1, value.Item2);
    public void Deconstruct(out Line3D line, out Number radius) { line = Line; radius = Radius; }
}
public partial class Matrix3D: Value<Matrix3D>
{
    public Vector4D Column1 { get; }
    public Vector4D Column2 { get; }
    public Vector4D Column3 { get; }
    public Vector4D Column4 { get; }
    public Matrix3D WithColumn1(Vector4D column1) => (column1, Column2, Column3, Column4);
    public Matrix3D WithColumn2(Vector4D column2) => (Column1, column2, Column3, Column4);
    public Matrix3D WithColumn3(Vector4D column3) => (Column1, Column2, column3, Column4);
    public Matrix3D WithColumn4(Vector4D column4) => (Column1, Column2, Column3, column4);
    public Matrix3D(Vector4D column1, Vector4D column2, Vector4D column3, Vector4D column4) => (Column1, Column2, Column3, Column4) = (column1, column2, column3, column4);
    public Matrix3D() { }
    public static Matrix3D Default = new Matrix3D();
    public static Matrix3D New(Vector4D column1, Vector4D column2, Vector4D column3, Vector4D column4) => new Matrix3D(column1, column2, column3, column4);
    public static implicit operator (Vector4D, Vector4D, Vector4D, Vector4D)(Matrix3D self) => (self.Column1, self.Column2, self.Column3, self.Column4);
    public static implicit operator Matrix3D((Vector4D, Vector4D, Vector4D, Vector4D) value) => new Matrix3D(value.Item1, value.Item2, value.Item3, value.Item4);
    public void Deconstruct(out Vector4D column1, out Vector4D column2, out Vector4D column3, out Vector4D column4) { column1 = Column1; column2 = Column2; column3 = Column3; column4 = Column4; }
}
public partial class Cylinder: Value<Cylinder>
{
    public Line3D Line { get; }
    public Number Radius { get; }
    public Cylinder WithLine(Line3D line) => (line, Radius);
    public Cylinder WithRadius(Number radius) => (Line, radius);
    public Cylinder(Line3D line, Number radius) => (Line, Radius) = (line, radius);
    public Cylinder() { }
    public static Cylinder Default = new Cylinder();
    public static Cylinder New(Line3D line, Number radius) => new Cylinder(line, radius);
    public static implicit operator (Line3D, Number)(Cylinder self) => (self.Line, self.Radius);
    public static implicit operator Cylinder((Line3D, Number) value) => new Cylinder(value.Item1, value.Item2);
    public void Deconstruct(out Line3D line, out Number radius) { line = Line; radius = Radius; }
}
public partial class Cone: Value<Cone>
{
    public Line3D Line { get; }
    public Number Radius { get; }
    public Cone WithLine(Line3D line) => (line, Radius);
    public Cone WithRadius(Number radius) => (Line, radius);
    public Cone(Line3D line, Number radius) => (Line, Radius) = (line, radius);
    public Cone() { }
    public static Cone Default = new Cone();
    public static Cone New(Line3D line, Number radius) => new Cone(line, radius);
    public static implicit operator (Line3D, Number)(Cone self) => (self.Line, self.Radius);
    public static implicit operator Cone((Line3D, Number) value) => new Cone(value.Item1, value.Item2);
    public void Deconstruct(out Line3D line, out Number radius) { line = Line; radius = Radius; }
}
public partial class Tube: Value<Tube>
{
    public Line3D Line { get; }
    public Number InnerRadius { get; }
    public Number OuterRadius { get; }
    public Tube WithLine(Line3D line) => (line, InnerRadius, OuterRadius);
    public Tube WithInnerRadius(Number innerRadius) => (Line, innerRadius, OuterRadius);
    public Tube WithOuterRadius(Number outerRadius) => (Line, InnerRadius, outerRadius);
    public Tube(Line3D line, Number innerRadius, Number outerRadius) => (Line, InnerRadius, OuterRadius) = (line, innerRadius, outerRadius);
    public Tube() { }
    public static Tube Default = new Tube();
    public static Tube New(Line3D line, Number innerRadius, Number outerRadius) => new Tube(line, innerRadius, outerRadius);
    public static implicit operator (Line3D, Number, Number)(Tube self) => (self.Line, self.InnerRadius, self.OuterRadius);
    public static implicit operator Tube((Line3D, Number, Number) value) => new Tube(value.Item1, value.Item2, value.Item3);
    public void Deconstruct(out Line3D line, out Number innerRadius, out Number outerRadius) { line = Line; innerRadius = InnerRadius; outerRadius = OuterRadius; }
}
public partial class ConeSegment: Value<ConeSegment>
{
    public Line3D Line { get; }
    public Number Radius1 { get; }
    public Number Radius2 { get; }
    public ConeSegment WithLine(Line3D line) => (line, Radius1, Radius2);
    public ConeSegment WithRadius1(Number radius1) => (Line, radius1, Radius2);
    public ConeSegment WithRadius2(Number radius2) => (Line, Radius1, radius2);
    public ConeSegment(Line3D line, Number radius1, Number radius2) => (Line, Radius1, Radius2) = (line, radius1, radius2);
    public ConeSegment() { }
    public static ConeSegment Default = new ConeSegment();
    public static ConeSegment New(Line3D line, Number radius1, Number radius2) => new ConeSegment(line, radius1, radius2);
    public static implicit operator (Line3D, Number, Number)(ConeSegment self) => (self.Line, self.Radius1, self.Radius2);
    public static implicit operator ConeSegment((Line3D, Number, Number) value) => new ConeSegment(value.Item1, value.Item2, value.Item3);
    public void Deconstruct(out Line3D line, out Number radius1, out Number radius2) { line = Line; radius1 = Radius1; radius2 = Radius2; }
}
public partial class Box2D: Value<Box2D>
{
    public Point2D Center { get; }
    public Angle Rotation { get; }
    public Size2D Extent { get; }
    public Box2D WithCenter(Point2D center) => (center, Rotation, Extent);
    public Box2D WithRotation(Angle rotation) => (Center, rotation, Extent);
    public Box2D WithExtent(Size2D extent) => (Center, Rotation, extent);
    public Box2D(Point2D center, Angle rotation, Size2D extent) => (Center, Rotation, Extent) = (center, rotation, extent);
    public Box2D() { }
    public static Box2D Default = new Box2D();
    public static Box2D New(Point2D center, Angle rotation, Size2D extent) => new Box2D(center, rotation, extent);
    public static implicit operator (Point2D, Angle, Size2D)(Box2D self) => (self.Center, self.Rotation, self.Extent);
    public static implicit operator Box2D((Point2D, Angle, Size2D) value) => new Box2D(value.Item1, value.Item2, value.Item3);
    public void Deconstruct(out Point2D center, out Angle rotation, out Size2D extent) { center = Center; rotation = Rotation; extent = Extent; }
}
public partial class Box3D: Value<Box3D>
{
    public Point3D Center { get; }
    public Rotation3D Rotation { get; }
    public Size3D Extent { get; }
    public Box3D WithCenter(Point3D center) => (center, Rotation, Extent);
    public Box3D WithRotation(Rotation3D rotation) => (Center, rotation, Extent);
    public Box3D WithExtent(Size3D extent) => (Center, Rotation, extent);
    public Box3D(Point3D center, Rotation3D rotation, Size3D extent) => (Center, Rotation, Extent) = (center, rotation, extent);
    public Box3D() { }
    public static Box3D Default = new Box3D();
    public static Box3D New(Point3D center, Rotation3D rotation, Size3D extent) => new Box3D(center, rotation, extent);
    public static implicit operator (Point3D, Rotation3D, Size3D)(Box3D self) => (self.Center, self.Rotation, self.Extent);
    public static implicit operator Box3D((Point3D, Rotation3D, Size3D) value) => new Box3D(value.Item1, value.Item2, value.Item3);
    public void Deconstruct(out Point3D center, out Rotation3D rotation, out Size3D extent) { center = Center; rotation = Rotation; extent = Extent; }
}
public partial class UV: Vector<UV>
{
    public Unit U { get; }
    public Unit V { get; }
    public UV WithU(Unit u) => (u, V);
    public UV WithV(Unit v) => (U, v);
    public UV(Unit u, Unit v) => (U, V) = (u, v);
    public UV() { }
    public static UV Default = new UV();
    public static UV New(Unit u, Unit v) => new UV(u, v);
    public static implicit operator (Unit, Unit)(UV self) => (self.U, self.V);
    public static implicit operator UV((Unit, Unit) value) => new UV(value.Item1, value.Item2);
    public void Deconstruct(out Unit u, out Unit v) { u = U; v = V; }
}
public partial class UVW: Vector<UVW>
{
    public Unit U { get; }
    public Unit V { get; }
    public Unit W { get; }
    public UVW WithU(Unit u) => (u, V, W);
    public UVW WithV(Unit v) => (U, v, W);
    public UVW WithW(Unit w) => (U, V, w);
    public UVW(Unit u, Unit v, Unit w) => (U, V, W) = (u, v, w);
    public UVW() { }
    public static UVW Default = new UVW();
    public static UVW New(Unit u, Unit v, Unit w) => new UVW(u, v, w);
    public static implicit operator (Unit, Unit, Unit)(UVW self) => (self.U, self.V, self.W);
    public static implicit operator UVW((Unit, Unit, Unit) value) => new UVW(value.Item1, value.Item2, value.Item3);
    public void Deconstruct(out Unit u, out Unit v, out Unit w) { u = U; v = V; w = W; }
}
public partial class CubicBezier2D: Value<CubicBezier2D>
{
    public Point2D A { get; }
    public Point2D B { get; }
    public Point2D C { get; }
    public Point2D D { get; }
    public CubicBezier2D WithA(Point2D a) => (a, B, C, D);
    public CubicBezier2D WithB(Point2D b) => (A, b, C, D);
    public CubicBezier2D WithC(Point2D c) => (A, B, c, D);
    public CubicBezier2D WithD(Point2D d) => (A, B, C, d);
    public CubicBezier2D(Point2D a, Point2D b, Point2D c, Point2D d) => (A, B, C, D) = (a, b, c, d);
    public CubicBezier2D() { }
    public static CubicBezier2D Default = new CubicBezier2D();
    public static CubicBezier2D New(Point2D a, Point2D b, Point2D c, Point2D d) => new CubicBezier2D(a, b, c, d);
    public static implicit operator (Point2D, Point2D, Point2D, Point2D)(CubicBezier2D self) => (self.A, self.B, self.C, self.D);
    public static implicit operator CubicBezier2D((Point2D, Point2D, Point2D, Point2D) value) => new CubicBezier2D(value.Item1, value.Item2, value.Item3, value.Item4);
    public void Deconstruct(out Point2D a, out Point2D b, out Point2D c, out Point2D d) { a = A; b = B; c = C; d = D; }
}
public partial class CubicBezier3D: Value<CubicBezier3D>
{
    public Point3D A { get; }
    public Point3D B { get; }
    public Point3D C { get; }
    public Point3D D { get; }
    public CubicBezier3D WithA(Point3D a) => (a, B, C, D);
    public CubicBezier3D WithB(Point3D b) => (A, b, C, D);
    public CubicBezier3D WithC(Point3D c) => (A, B, c, D);
    public CubicBezier3D WithD(Point3D d) => (A, B, C, d);
    public CubicBezier3D(Point3D a, Point3D b, Point3D c, Point3D d) => (A, B, C, D) = (a, b, c, d);
    public CubicBezier3D() { }
    public static CubicBezier3D Default = new CubicBezier3D();
    public static CubicBezier3D New(Point3D a, Point3D b, Point3D c, Point3D d) => new CubicBezier3D(a, b, c, d);
    public static implicit operator (Point3D, Point3D, Point3D, Point3D)(CubicBezier3D self) => (self.A, self.B, self.C, self.D);
    public static implicit operator CubicBezier3D((Point3D, Point3D, Point3D, Point3D) value) => new CubicBezier3D(value.Item1, value.Item2, value.Item3, value.Item4);
    public void Deconstruct(out Point3D a, out Point3D b, out Point3D c, out Point3D d) { a = A; b = B; c = C; d = D; }
}
public partial class QuadraticBezier2D: Value<QuadraticBezier2D>
{
    public Point2D A { get; }
    public Point2D B { get; }
    public Point2D C { get; }
    public QuadraticBezier2D WithA(Point2D a) => (a, B, C);
    public QuadraticBezier2D WithB(Point2D b) => (A, b, C);
    public QuadraticBezier2D WithC(Point2D c) => (A, B, c);
    public QuadraticBezier2D(Point2D a, Point2D b, Point2D c) => (A, B, C) = (a, b, c);
    public QuadraticBezier2D() { }
    public static QuadraticBezier2D Default = new QuadraticBezier2D();
    public static QuadraticBezier2D New(Point2D a, Point2D b, Point2D c) => new QuadraticBezier2D(a, b, c);
    public static implicit operator (Point2D, Point2D, Point2D)(QuadraticBezier2D self) => (self.A, self.B, self.C);
    public static implicit operator QuadraticBezier2D((Point2D, Point2D, Point2D) value) => new QuadraticBezier2D(value.Item1, value.Item2, value.Item3);
    public void Deconstruct(out Point2D a, out Point2D b, out Point2D c) { a = A; b = B; c = C; }
}
public partial class QuadraticBezier3D: Value<QuadraticBezier3D>
{
    public Point3D A { get; }
    public Point3D B { get; }
    public Point3D C { get; }
    public QuadraticBezier3D WithA(Point3D a) => (a, B, C);
    public QuadraticBezier3D WithB(Point3D b) => (A, b, C);
    public QuadraticBezier3D WithC(Point3D c) => (A, B, c);
    public QuadraticBezier3D(Point3D a, Point3D b, Point3D c) => (A, B, C) = (a, b, c);
    public QuadraticBezier3D() { }
    public static QuadraticBezier3D Default = new QuadraticBezier3D();
    public static QuadraticBezier3D New(Point3D a, Point3D b, Point3D c) => new QuadraticBezier3D(a, b, c);
    public static implicit operator (Point3D, Point3D, Point3D)(QuadraticBezier3D self) => (self.A, self.B, self.C);
    public static implicit operator QuadraticBezier3D((Point3D, Point3D, Point3D) value) => new QuadraticBezier3D(value.Item1, value.Item2, value.Item3);
    public void Deconstruct(out Point3D a, out Point3D b, out Point3D c) { a = A; b = B; c = C; }
}
public partial class Area: Measure<Area>
{
    public Number MetersSquared { get; }
    public Area WithMetersSquared(Number metersSquared) => (metersSquared);
    public Area(Number metersSquared) => (MetersSquared) = (metersSquared);
    public Area() { }
    public static Area Default = new Area();
    public static Area New(Number metersSquared) => new Area(metersSquared);
    public static implicit operator Number(Area self) => self.MetersSquared;
    public static implicit operator Area(Number value) => new Area(value);
}
public partial class Volume: Measure<Volume>
{
    public Number MetersCubed { get; }
    public Volume WithMetersCubed(Number metersCubed) => (metersCubed);
    public Volume(Number metersCubed) => (MetersCubed) = (metersCubed);
    public Volume() { }
    public static Volume Default = new Volume();
    public static Volume New(Number metersCubed) => new Volume(metersCubed);
    public static implicit operator Number(Volume self) => self.MetersCubed;
    public static implicit operator Volume(Number value) => new Volume(value);
}
public partial class Velocity: Measure<Velocity>
{
    public Number MetersPerSecond { get; }
    public Velocity WithMetersPerSecond(Number metersPerSecond) => (metersPerSecond);
    public Velocity(Number metersPerSecond) => (MetersPerSecond) = (metersPerSecond);
    public Velocity() { }
    public static Velocity Default = new Velocity();
    public static Velocity New(Number metersPerSecond) => new Velocity(metersPerSecond);
    public static implicit operator Number(Velocity self) => self.MetersPerSecond;
    public static implicit operator Velocity(Number value) => new Velocity(value);
}
public partial class Acceleration: Measure<Acceleration>
{
    public Number MetersPerSecondSquared { get; }
    public Acceleration WithMetersPerSecondSquared(Number metersPerSecondSquared) => (metersPerSecondSquared);
    public Acceleration(Number metersPerSecondSquared) => (MetersPerSecondSquared) = (metersPerSecondSquared);
    public Acceleration() { }
    public static Acceleration Default = new Acceleration();
    public static Acceleration New(Number metersPerSecondSquared) => new Acceleration(metersPerSecondSquared);
    public static implicit operator Number(Acceleration self) => self.MetersPerSecondSquared;
    public static implicit operator Acceleration(Number value) => new Acceleration(value);
}
public partial class Force: Measure<Force>
{
    public Number Newtons { get; }
    public Force WithNewtons(Number newtons) => (newtons);
    public Force(Number newtons) => (Newtons) = (newtons);
    public Force() { }
    public static Force Default = new Force();
    public static Force New(Number newtons) => new Force(newtons);
    public static implicit operator Number(Force self) => self.Newtons;
    public static implicit operator Force(Number value) => new Force(value);
}
public partial class Pressure: Measure<Pressure>
{
    public Number Pascals { get; }
    public Pressure WithPascals(Number pascals) => (pascals);
    public Pressure(Number pascals) => (Pascals) = (pascals);
    public Pressure() { }
    public static Pressure Default = new Pressure();
    public static Pressure New(Number pascals) => new Pressure(pascals);
    public static implicit operator Number(Pressure self) => self.Pascals;
    public static implicit operator Pressure(Number value) => new Pressure(value);
}
public partial class Energy: Measure<Energy>
{
    public Number Joules { get; }
    public Energy WithJoules(Number joules) => (joules);
    public Energy(Number joules) => (Joules) = (joules);
    public Energy() { }
    public static Energy Default = new Energy();
    public static Energy New(Number joules) => new Energy(joules);
    public static implicit operator Number(Energy self) => self.Joules;
    public static implicit operator Energy(Number value) => new Energy(value);
}
public partial class Memory: Measure<Memory>
{
    public Count Bytes { get; }
    public Memory WithBytes(Count bytes) => (bytes);
    public Memory(Count bytes) => (Bytes) = (bytes);
    public Memory() { }
    public static Memory Default = new Memory();
    public static Memory New(Count bytes) => new Memory(bytes);
    public static implicit operator Count(Memory self) => self.Bytes;
    public static implicit operator Memory(Count value) => new Memory(value);
}
public partial class Frequency: Measure<Frequency>
{
    public Number Hertz { get; }
    public Frequency WithHertz(Number hertz) => (hertz);
    public Frequency(Number hertz) => (Hertz) = (hertz);
    public Frequency() { }
    public static Frequency Default = new Frequency();
    public static Frequency New(Number hertz) => new Frequency(hertz);
    public static implicit operator Number(Frequency self) => self.Hertz;
    public static implicit operator Frequency(Number value) => new Frequency(value);
}
public partial class Loudness: Measure<Loudness>
{
    public Number Decibels { get; }
    public Loudness WithDecibels(Number decibels) => (decibels);
    public Loudness(Number decibels) => (Decibels) = (decibels);
    public Loudness() { }
    public static Loudness Default = new Loudness();
    public static Loudness New(Number decibels) => new Loudness(decibels);
    public static implicit operator Number(Loudness self) => self.Decibels;
    public static implicit operator Loudness(Number value) => new Loudness(value);
}
public partial class LuminousIntensity: Measure<LuminousIntensity>
{
    public Number Candelas { get; }
    public LuminousIntensity WithCandelas(Number candelas) => (candelas);
    public LuminousIntensity(Number candelas) => (Candelas) = (candelas);
    public LuminousIntensity() { }
    public static LuminousIntensity Default = new LuminousIntensity();
    public static LuminousIntensity New(Number candelas) => new LuminousIntensity(candelas);
    public static implicit operator Number(LuminousIntensity self) => self.Candelas;
    public static implicit operator LuminousIntensity(Number value) => new LuminousIntensity(value);
}
public partial class ElectricPotential: Measure<ElectricPotential>
{
    public Number Volts { get; }
    public ElectricPotential WithVolts(Number volts) => (volts);
    public ElectricPotential(Number volts) => (Volts) = (volts);
    public ElectricPotential() { }
    public static ElectricPotential Default = new ElectricPotential();
    public static ElectricPotential New(Number volts) => new ElectricPotential(volts);
    public static implicit operator Number(ElectricPotential self) => self.Volts;
    public static implicit operator ElectricPotential(Number value) => new ElectricPotential(value);
}
public partial class ElectricCharge: Measure<ElectricCharge>
{
    public Number Columbs { get; }
    public ElectricCharge WithColumbs(Number columbs) => (columbs);
    public ElectricCharge(Number columbs) => (Columbs) = (columbs);
    public ElectricCharge() { }
    public static ElectricCharge Default = new ElectricCharge();
    public static ElectricCharge New(Number columbs) => new ElectricCharge(columbs);
    public static implicit operator Number(ElectricCharge self) => self.Columbs;
    public static implicit operator ElectricCharge(Number value) => new ElectricCharge(value);
}
public partial class ElectricCurrent: Measure<ElectricCurrent>
{
    public Number Amperes { get; }
    public ElectricCurrent WithAmperes(Number amperes) => (amperes);
    public ElectricCurrent(Number amperes) => (Amperes) = (amperes);
    public ElectricCurrent() { }
    public static ElectricCurrent Default = new ElectricCurrent();
    public static ElectricCurrent New(Number amperes) => new ElectricCurrent(amperes);
    public static implicit operator Number(ElectricCurrent self) => self.Amperes;
    public static implicit operator ElectricCurrent(Number value) => new ElectricCurrent(value);
}
public partial class ElectricResistance: Measure<ElectricResistance>
{
    public Number Ohms { get; }
    public ElectricResistance WithOhms(Number ohms) => (ohms);
    public ElectricResistance(Number ohms) => (Ohms) = (ohms);
    public ElectricResistance() { }
    public static ElectricResistance Default = new ElectricResistance();
    public static ElectricResistance New(Number ohms) => new ElectricResistance(ohms);
    public static implicit operator Number(ElectricResistance self) => self.Ohms;
    public static implicit operator ElectricResistance(Number value) => new ElectricResistance(value);
}
public partial class Power: Measure<Power>
{
    public Number Watts { get; }
    public Power WithWatts(Number watts) => (watts);
    public Power(Number watts) => (Watts) = (watts);
    public Power() { }
    public static Power Default = new Power();
    public static Power New(Number watts) => new Power(watts);
    public static implicit operator Number(Power self) => self.Watts;
    public static implicit operator Power(Number value) => new Power(value);
}
public partial class Density: Measure<Density>
{
    public Number KilogramsPerMeterCubed { get; }
    public Density WithKilogramsPerMeterCubed(Number kilogramsPerMeterCubed) => (kilogramsPerMeterCubed);
    public Density(Number kilogramsPerMeterCubed) => (KilogramsPerMeterCubed) = (kilogramsPerMeterCubed);
    public Density() { }
    public static Density Default = new Density();
    public static Density New(Number kilogramsPerMeterCubed) => new Density(kilogramsPerMeterCubed);
    public static implicit operator Number(Density self) => self.KilogramsPerMeterCubed;
    public static implicit operator Density(Number value) => new Density(value);
}
public partial class NormalDistribution: Value<NormalDistribution>
{
    public Number Mean { get; }
    public Number StandardDeviation { get; }
    public NormalDistribution WithMean(Number mean) => (mean, StandardDeviation);
    public NormalDistribution WithStandardDeviation(Number standardDeviation) => (Mean, standardDeviation);
    public NormalDistribution(Number mean, Number standardDeviation) => (Mean, StandardDeviation) = (mean, standardDeviation);
    public NormalDistribution() { }
    public static NormalDistribution Default = new NormalDistribution();
    public static NormalDistribution New(Number mean, Number standardDeviation) => new NormalDistribution(mean, standardDeviation);
    public static implicit operator (Number, Number)(NormalDistribution self) => (self.Mean, self.StandardDeviation);
    public static implicit operator NormalDistribution((Number, Number) value) => new NormalDistribution(value.Item1, value.Item2);
    public void Deconstruct(out Number mean, out Number standardDeviation) { mean = Mean; standardDeviation = StandardDeviation; }
}
public partial class PoissonDistribution: Value<PoissonDistribution>
{
    public Number Expected { get; }
    public Count Occurrences { get; }
    public PoissonDistribution WithExpected(Number expected) => (expected, Occurrences);
    public PoissonDistribution WithOccurrences(Count occurrences) => (Expected, occurrences);
    public PoissonDistribution(Number expected, Count occurrences) => (Expected, Occurrences) = (expected, occurrences);
    public PoissonDistribution() { }
    public static PoissonDistribution Default = new PoissonDistribution();
    public static PoissonDistribution New(Number expected, Count occurrences) => new PoissonDistribution(expected, occurrences);
    public static implicit operator (Number, Count)(PoissonDistribution self) => (self.Expected, self.Occurrences);
    public static implicit operator PoissonDistribution((Number, Count) value) => new PoissonDistribution(value.Item1, value.Item2);
    public void Deconstruct(out Number expected, out Count occurrences) { expected = Expected; occurrences = Occurrences; }
}
public partial class BernoulliDistribution: Value<BernoulliDistribution>
{
    public Probability P { get; }
    public BernoulliDistribution WithP(Probability p) => (p);
    public BernoulliDistribution(Probability p) => (P) = (p);
    public BernoulliDistribution() { }
    public static BernoulliDistribution Default = new BernoulliDistribution();
    public static BernoulliDistribution New(Probability p) => new BernoulliDistribution(p);
    public static implicit operator Probability(BernoulliDistribution self) => self.P;
    public static implicit operator BernoulliDistribution(Probability value) => new BernoulliDistribution(value);
}
public partial class Probability: Numerical<Probability>
{
    public Number Value { get; }
    public Probability WithValue(Number value) => (value);
    public Probability(Number value) => (Value) = (value);
    public Probability() { }
    public static Probability Default = new Probability();
    public static Probability New(Number value) => new Probability(value);
    public static implicit operator Number(Probability self) => self.Value;
    public static implicit operator Probability(Number value) => new Probability(value);
}
public partial class BinomialDistribution: Value<BinomialDistribution>
{
    public Count Trials { get; }
    public Probability P { get; }
    public BinomialDistribution WithTrials(Count trials) => (trials, P);
    public BinomialDistribution WithP(Probability p) => (Trials, p);
    public BinomialDistribution(Count trials, Probability p) => (Trials, P) = (trials, p);
    public BinomialDistribution() { }
    public static BinomialDistribution Default = new BinomialDistribution();
    public static BinomialDistribution New(Count trials, Probability p) => new BinomialDistribution(trials, p);
    public static implicit operator (Count, Probability)(BinomialDistribution self) => (self.Trials, self.P);
    public static implicit operator BinomialDistribution((Count, Probability) value) => new BinomialDistribution(value.Item1, value.Item2);
    public void Deconstruct(out Count trials, out Probability p) { trials = Trials; p = P; }
}