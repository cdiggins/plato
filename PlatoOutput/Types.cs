using System;
public readonly partial struct Number: Numerical<Number>
{
    public readonly double Value;
    public Number WithValue(double value) => (value);
    public Number(double value) => (Value) = (value);
    public static Number Default = new Number();
    public static Number New(double value) => new Number(value);
    public static implicit operator double(Number self) => self.Value;
    public static implicit operator Number(double value) => new Number(value);
    public String TypeName => "Number";
    public Array<String> FieldNames => (Array1<String>)new[] { (String)"Value" };
    public Array<Dynamic> FieldValues => (Array1<Dynamic>)new[] { new Dynamic(Value) };
}
public readonly partial struct Integer: WholeNumber<Integer>
{
    public readonly int Value;
    public Integer WithValue(int value) => (value);
    public Integer(int value) => (Value) = (value);
    public static Integer Default = new Integer();
    public static Integer New(int value) => new Integer(value);
    public static implicit operator int(Integer self) => self.Value;
    public static implicit operator Integer(int value) => new Integer(value);
    public String TypeName => "Integer";
    public Array<String> FieldNames => (Array1<String>)new[] { (String)"Value" };
    public Array<Dynamic> FieldValues => (Array1<Dynamic>)new[] { new Dynamic(Value) };
}
public readonly partial struct String: Array<Character>, Comparable<String>, Equatable<String>
{
    public readonly string Value;
    public String WithValue(string value) => (value);
    public String(string value) => (Value) = (value);
    public static String Default = new String();
    public static String New(string value) => new String(value);
    public static implicit operator string(String self) => self.Value;
    public static implicit operator String(string value) => new String(value);
    public String TypeName => "String";
    public Array<String> FieldNames => (Array1<String>)new[] { (String)"Value" };
    public Array<Dynamic> FieldValues => (Array1<Dynamic>)new[] { new Dynamic(Value) };
}
public readonly partial struct Boolean: BooleanOperations<Boolean>
{
    public readonly bool Value;
    public Boolean WithValue(bool value) => (value);
    public Boolean(bool value) => (Value) = (value);
    public static Boolean Default = new Boolean();
    public static Boolean New(bool value) => new Boolean(value);
    public static implicit operator bool(Boolean self) => self.Value;
    public static implicit operator Boolean(bool value) => new Boolean(value);
    public String TypeName => "Boolean";
    public Array<String> FieldNames => (Array1<String>)new[] { (String)"Value" };
    public Array<Dynamic> FieldValues => (Array1<Dynamic>)new[] { new Dynamic(Value) };
}
public readonly partial struct Character: Value<Character>
{
    public readonly char Value;
    public Character WithValue(char value) => (value);
    public Character(char value) => (Value) = (value);
    public static Character Default = new Character();
    public static Character New(char value) => new Character(value);
    public static implicit operator char(Character self) => self.Value;
    public static implicit operator Character(char value) => new Character(value);
    public String TypeName => "Character";
    public Array<String> FieldNames => (Array1<String>)new[] { (String)"Value" };
    public Array<Dynamic> FieldValues => (Array1<Dynamic>)new[] { new Dynamic(Value) };
}
public readonly partial struct Cardinal: WholeNumber<Cardinal>
{
    public readonly Integer Value;
    public Cardinal WithValue(Integer value) => (value);
    public Cardinal(Integer value) => (Value) = (value);
    public static Cardinal Default = new Cardinal();
    public static Cardinal New(Integer value) => new Cardinal(value);
    public static implicit operator Integer(Cardinal self) => self.Value;
    public static implicit operator Cardinal(Integer value) => new Cardinal(value);
    public String TypeName => "Cardinal";
    public Array<String> FieldNames => (Array1<String>)new[] { (String)"Value" };
    public Array<Dynamic> FieldValues => (Array1<Dynamic>)new[] { new Dynamic(Value) };
    // Unimplemented concept functions
    public Number Magnitude => throw new NotImplementedException();
    public Integer Compare(Cardinal y) => throw new NotImplementedException();
    public Cardinal Reciprocal => (Value.Reciprocal);
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
}
public readonly partial struct Index: WholeNumber<Index>
{
    public readonly Integer Value;
    public Index WithValue(Integer value) => (value);
    public Index(Integer value) => (Value) = (value);
    public static Index Default = new Index();
    public static Index New(Integer value) => new Index(value);
    public static implicit operator Integer(Index self) => self.Value;
    public static implicit operator Index(Integer value) => new Index(value);
    public String TypeName => "Index";
    public Array<String> FieldNames => (Array1<String>)new[] { (String)"Value" };
    public Array<Dynamic> FieldValues => (Array1<Dynamic>)new[] { new Dynamic(Value) };
    // Unimplemented concept functions
    public Number Magnitude => throw new NotImplementedException();
    public Integer Compare(Index y) => throw new NotImplementedException();
    public Index Reciprocal => (Value.Reciprocal);
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
}
public readonly partial struct Unit: Numerical<Unit>
{
    public readonly Number Value;
    public Unit WithValue(Number value) => (value);
    public Unit(Number value) => (Value) = (value);
    public static Unit Default = new Unit();
    public static Unit New(Number value) => new Unit(value);
    public static implicit operator Number(Unit self) => self.Value;
    public static implicit operator Unit(Number value) => new Unit(value);
    public String TypeName => "Unit";
    public Array<String> FieldNames => (Array1<String>)new[] { (String)"Value" };
    public Array<Dynamic> FieldValues => (Array1<Dynamic>)new[] { new Dynamic(Value) };
    // Unimplemented concept functions
    public Number Unlerp(Unit a, Unit b) => throw new NotImplementedException();
    public Number Magnitude => throw new NotImplementedException();
    public Integer Compare(Unit y) => throw new NotImplementedException();
    public Unit Reciprocal => (Value.Reciprocal);
    public Unit Negative => (Value.Negative);
    public static Unit operator -(Unit self) => self.Negative;
    public Unit Multiply(Unit other) => (Value.Multiply(other.Value));
    public static Unit operator *(Unit self, Unit other) => self.Multiply(other);
    public Unit Divide(Unit other) => (Value.Divide(other.Value));
    public static Unit operator /(Unit self, Unit other) => self.Divide(other);
    public Unit Modulo(Unit other) => (Value.Modulo(other.Value));
    public static Unit operator %(Unit self, Unit other) => self.Modulo(other);
    public Unit Add(Unit other) => (Value.Add(other.Value));
    public static Unit operator +(Unit self, Unit other) => self.Add(other);
    public Unit Subtract(Unit other) => (Value.Subtract(other.Value));
    public static Unit operator -(Unit self, Unit other) => self.Subtract(other);
    public Unit Zero => (Value.Zero);
    public Unit One => (Value.One);
    public Unit MinValue => (Value.MinValue);
    public Unit MaxValue => (Value.MaxValue);
}
public readonly partial struct Percent: Numerical<Percent>
{
    public readonly Number Value;
    public Percent WithValue(Number value) => (value);
    public Percent(Number value) => (Value) = (value);
    public static Percent Default = new Percent();
    public static Percent New(Number value) => new Percent(value);
    public static implicit operator Number(Percent self) => self.Value;
    public static implicit operator Percent(Number value) => new Percent(value);
    public String TypeName => "Percent";
    public Array<String> FieldNames => (Array1<String>)new[] { (String)"Value" };
    public Array<Dynamic> FieldValues => (Array1<Dynamic>)new[] { new Dynamic(Value) };
    // Unimplemented concept functions
    public Number Unlerp(Percent a, Percent b) => throw new NotImplementedException();
    public Number Magnitude => throw new NotImplementedException();
    public Integer Compare(Percent y) => throw new NotImplementedException();
    public Percent Reciprocal => (Value.Reciprocal);
    public Percent Negative => (Value.Negative);
    public static Percent operator -(Percent self) => self.Negative;
    public Percent Multiply(Percent other) => (Value.Multiply(other.Value));
    public static Percent operator *(Percent self, Percent other) => self.Multiply(other);
    public Percent Divide(Percent other) => (Value.Divide(other.Value));
    public static Percent operator /(Percent self, Percent other) => self.Divide(other);
    public Percent Modulo(Percent other) => (Value.Modulo(other.Value));
    public static Percent operator %(Percent self, Percent other) => self.Modulo(other);
    public Percent Add(Percent other) => (Value.Add(other.Value));
    public static Percent operator +(Percent self, Percent other) => self.Add(other);
    public Percent Subtract(Percent other) => (Value.Subtract(other.Value));
    public static Percent operator -(Percent self, Percent other) => self.Subtract(other);
    public Percent Zero => (Value.Zero);
    public Percent One => (Value.One);
    public Percent MinValue => (Value.MinValue);
    public Percent MaxValue => (Value.MaxValue);
}
public readonly partial struct Quaternion: Value<Quaternion>
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
    public static implicit operator (Number, Number, Number, Number)(Quaternion self) => (self.X, self.Y, self.Z, self.W);
    public static implicit operator Quaternion((Number, Number, Number, Number) value) => new Quaternion(value.Item1, value.Item2, value.Item3, value.Item4);
    public void Deconstruct(out Number x, out Number y, out Number z, out Number w) { x = X; y = Y; z = Z; w = W; }
    public String TypeName => "Quaternion";
    public Array<String> FieldNames => (Array1<String>)new[] { (String)"X", (String)"Y", (String)"Z", (String)"W" };
    public Array<Dynamic> FieldValues => (Array1<Dynamic>)new[] { new Dynamic(X), new Dynamic(Y), new Dynamic(Z), new Dynamic(W) };
    // Unimplemented concept functions
    public Quaternion Zero => (X.Zero, Y.Zero, Z.Zero, W.Zero);
    public Quaternion One => (X.One, Y.One, Z.One, W.One);
    public Quaternion MinValue => (X.MinValue, Y.MinValue, Z.MinValue, W.MinValue);
    public Quaternion MaxValue => (X.MaxValue, Y.MaxValue, Z.MaxValue, W.MaxValue);
    public Boolean Equals(Quaternion b) => (X.Equals(b.X) & Y.Equals(b.Y) & Z.Equals(b.Z) & W.Equals(b.W));
    public static Boolean operator ==(Quaternion a, Quaternion b) => a.Equals(b);
    public Boolean NotEquals(Quaternion b) => (X.NotEquals(b.X) & Y.NotEquals(b.Y) & Z.NotEquals(b.Z) & W.NotEquals(b.W));
    public static Boolean operator !=(Quaternion a, Quaternion b) => a.NotEquals(b);
}
public readonly partial struct Unit2D: Value<Unit2D>
{
    public readonly Unit X;
    public readonly Unit Y;
    public Unit2D WithX(Unit x) => (x, Y);
    public Unit2D WithY(Unit y) => (X, y);
    public Unit2D(Unit x, Unit y) => (X, Y) = (x, y);
    public static Unit2D Default = new Unit2D();
    public static Unit2D New(Unit x, Unit y) => new Unit2D(x, y);
    public static implicit operator (Unit, Unit)(Unit2D self) => (self.X, self.Y);
    public static implicit operator Unit2D((Unit, Unit) value) => new Unit2D(value.Item1, value.Item2);
    public void Deconstruct(out Unit x, out Unit y) { x = X; y = Y; }
    public String TypeName => "Unit2D";
    public Array<String> FieldNames => (Array1<String>)new[] { (String)"X", (String)"Y" };
    public Array<Dynamic> FieldValues => (Array1<Dynamic>)new[] { new Dynamic(X), new Dynamic(Y) };
    // Unimplemented concept functions
    public Unit2D Zero => (X.Zero, Y.Zero);
    public Unit2D One => (X.One, Y.One);
    public Unit2D MinValue => (X.MinValue, Y.MinValue);
    public Unit2D MaxValue => (X.MaxValue, Y.MaxValue);
    public Boolean Equals(Unit2D b) => (X.Equals(b.X) & Y.Equals(b.Y));
    public static Boolean operator ==(Unit2D a, Unit2D b) => a.Equals(b);
    public Boolean NotEquals(Unit2D b) => (X.NotEquals(b.X) & Y.NotEquals(b.Y));
    public static Boolean operator !=(Unit2D a, Unit2D b) => a.NotEquals(b);
}
public readonly partial struct Unit3D: Value<Unit3D>
{
    public readonly Unit X;
    public readonly Unit Y;
    public readonly Unit Z;
    public Unit3D WithX(Unit x) => (x, Y, Z);
    public Unit3D WithY(Unit y) => (X, y, Z);
    public Unit3D WithZ(Unit z) => (X, Y, z);
    public Unit3D(Unit x, Unit y, Unit z) => (X, Y, Z) = (x, y, z);
    public static Unit3D Default = new Unit3D();
    public static Unit3D New(Unit x, Unit y, Unit z) => new Unit3D(x, y, z);
    public static implicit operator (Unit, Unit, Unit)(Unit3D self) => (self.X, self.Y, self.Z);
    public static implicit operator Unit3D((Unit, Unit, Unit) value) => new Unit3D(value.Item1, value.Item2, value.Item3);
    public void Deconstruct(out Unit x, out Unit y, out Unit z) { x = X; y = Y; z = Z; }
    public String TypeName => "Unit3D";
    public Array<String> FieldNames => (Array1<String>)new[] { (String)"X", (String)"Y", (String)"Z" };
    public Array<Dynamic> FieldValues => (Array1<Dynamic>)new[] { new Dynamic(X), new Dynamic(Y), new Dynamic(Z) };
    // Unimplemented concept functions
    public Unit3D Zero => (X.Zero, Y.Zero, Z.Zero);
    public Unit3D One => (X.One, Y.One, Z.One);
    public Unit3D MinValue => (X.MinValue, Y.MinValue, Z.MinValue);
    public Unit3D MaxValue => (X.MaxValue, Y.MaxValue, Z.MaxValue);
    public Boolean Equals(Unit3D b) => (X.Equals(b.X) & Y.Equals(b.Y) & Z.Equals(b.Z));
    public static Boolean operator ==(Unit3D a, Unit3D b) => a.Equals(b);
    public Boolean NotEquals(Unit3D b) => (X.NotEquals(b.X) & Y.NotEquals(b.Y) & Z.NotEquals(b.Z));
    public static Boolean operator !=(Unit3D a, Unit3D b) => a.NotEquals(b);
}
public readonly partial struct Direction3D: Value<Direction3D>
{
    public readonly Unit3D Value;
    public Direction3D WithValue(Unit3D value) => (value);
    public Direction3D(Unit3D value) => (Value) = (value);
    public static Direction3D Default = new Direction3D();
    public static Direction3D New(Unit3D value) => new Direction3D(value);
    public static implicit operator Unit3D(Direction3D self) => self.Value;
    public static implicit operator Direction3D(Unit3D value) => new Direction3D(value);
    public String TypeName => "Direction3D";
    public Array<String> FieldNames => (Array1<String>)new[] { (String)"Value" };
    public Array<Dynamic> FieldValues => (Array1<Dynamic>)new[] { new Dynamic(Value) };
    // Unimplemented concept functions
    public Direction3D Zero => (Value.Zero);
    public Direction3D One => (Value.One);
    public Direction3D MinValue => (Value.MinValue);
    public Direction3D MaxValue => (Value.MaxValue);
    public Boolean Equals(Direction3D b) => (Value.Equals(b.Value));
    public static Boolean operator ==(Direction3D a, Direction3D b) => a.Equals(b);
    public Boolean NotEquals(Direction3D b) => (Value.NotEquals(b.Value));
    public static Boolean operator !=(Direction3D a, Direction3D b) => a.NotEquals(b);
}
public readonly partial struct AxisAngle: Value<AxisAngle>
{
    public readonly Unit3D Axis;
    public readonly Angle Angle;
    public AxisAngle WithAxis(Unit3D axis) => (axis, Angle);
    public AxisAngle WithAngle(Angle angle) => (Axis, angle);
    public AxisAngle(Unit3D axis, Angle angle) => (Axis, Angle) = (axis, angle);
    public static AxisAngle Default = new AxisAngle();
    public static AxisAngle New(Unit3D axis, Angle angle) => new AxisAngle(axis, angle);
    public static implicit operator (Unit3D, Angle)(AxisAngle self) => (self.Axis, self.Angle);
    public static implicit operator AxisAngle((Unit3D, Angle) value) => new AxisAngle(value.Item1, value.Item2);
    public void Deconstruct(out Unit3D axis, out Angle angle) { axis = Axis; angle = Angle; }
    public String TypeName => "AxisAngle";
    public Array<String> FieldNames => (Array1<String>)new[] { (String)"Axis", (String)"Angle" };
    public Array<Dynamic> FieldValues => (Array1<Dynamic>)new[] { new Dynamic(Axis), new Dynamic(Angle) };
    // Unimplemented concept functions
    public AxisAngle Zero => (Axis.Zero, Angle.Zero);
    public AxisAngle One => (Axis.One, Angle.One);
    public AxisAngle MinValue => (Axis.MinValue, Angle.MinValue);
    public AxisAngle MaxValue => (Axis.MaxValue, Angle.MaxValue);
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
    public static implicit operator (Angle, Angle, Angle)(EulerAngles self) => (self.Yaw, self.Pitch, self.Roll);
    public static implicit operator EulerAngles((Angle, Angle, Angle) value) => new EulerAngles(value.Item1, value.Item2, value.Item3);
    public void Deconstruct(out Angle yaw, out Angle pitch, out Angle roll) { yaw = Yaw; pitch = Pitch; roll = Roll; }
    public String TypeName => "EulerAngles";
    public Array<String> FieldNames => (Array1<String>)new[] { (String)"Yaw", (String)"Pitch", (String)"Roll" };
    public Array<Dynamic> FieldValues => (Array1<Dynamic>)new[] { new Dynamic(Yaw), new Dynamic(Pitch), new Dynamic(Roll) };
    // Unimplemented concept functions
    public EulerAngles Zero => (Yaw.Zero, Pitch.Zero, Roll.Zero);
    public EulerAngles One => (Yaw.One, Pitch.One, Roll.One);
    public EulerAngles MinValue => (Yaw.MinValue, Pitch.MinValue, Roll.MinValue);
    public EulerAngles MaxValue => (Yaw.MaxValue, Pitch.MaxValue, Roll.MaxValue);
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
    public static implicit operator Quaternion(Rotation3D self) => self.Quaternion;
    public static implicit operator Rotation3D(Quaternion value) => new Rotation3D(value);
    public String TypeName => "Rotation3D";
    public Array<String> FieldNames => (Array1<String>)new[] { (String)"Quaternion" };
    public Array<Dynamic> FieldValues => (Array1<Dynamic>)new[] { new Dynamic(Quaternion) };
    // Unimplemented concept functions
    public Rotation3D Zero => (Quaternion.Zero);
    public Rotation3D One => (Quaternion.One);
    public Rotation3D MinValue => (Quaternion.MinValue);
    public Rotation3D MaxValue => (Quaternion.MaxValue);
    public Boolean Equals(Rotation3D b) => (Quaternion.Equals(b.Quaternion));
    public static Boolean operator ==(Rotation3D a, Rotation3D b) => a.Equals(b);
    public Boolean NotEquals(Rotation3D b) => (Quaternion.NotEquals(b.Quaternion));
    public static Boolean operator !=(Rotation3D a, Rotation3D b) => a.NotEquals(b);
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
    public static implicit operator (Number, Number)(Vector2D self) => (self.X, self.Y);
    public static implicit operator Vector2D((Number, Number) value) => new Vector2D(value.Item1, value.Item2);
    public void Deconstruct(out Number x, out Number y) { x = X; y = Y; }
    public String TypeName => "Vector2D";
    public Array<String> FieldNames => (Array1<String>)new[] { (String)"X", (String)"Y" };
    public Array<Dynamic> FieldValues => (Array1<Dynamic>)new[] { new Dynamic(X), new Dynamic(Y) };
    // Unimplemented concept functions
    public Vector2D Multiply(Number other) => throw new NotImplementedException();
    public static Vector2D operator *(Vector2D self, Number other) => self.Multiply(other);
    public Vector2D Divide(Number other) => throw new NotImplementedException();
    public static Vector2D operator /(Vector2D self, Number other) => self.Divide(other);
    public Vector2D Modulo(Number other) => throw new NotImplementedException();
    public static Vector2D operator %(Vector2D self, Number other) => self.Modulo(other);
    public Vector2D Add(Number other) => throw new NotImplementedException();
    public static Vector2D operator +(Vector2D self, Number other) => self.Add(other);
    public Vector2D Subtract(Number other) => throw new NotImplementedException();
    public static Vector2D operator -(Vector2D self, Number other) => self.Subtract(other);
    public Number Unlerp(Vector2D a, Vector2D b) => throw new NotImplementedException();
    public Integer Compare(Vector2D y) => throw new NotImplementedException();
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
    public Integer Count => throw new NotImplementedException();
    public Number At(Integer n) => throw new NotImplementedException();
    public Number this[Integer n] => At(n);
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
    public static implicit operator (Number, Number, Number)(Vector3D self) => (self.X, self.Y, self.Z);
    public static implicit operator Vector3D((Number, Number, Number) value) => new Vector3D(value.Item1, value.Item2, value.Item3);
    public void Deconstruct(out Number x, out Number y, out Number z) { x = X; y = Y; z = Z; }
    public String TypeName => "Vector3D";
    public Array<String> FieldNames => (Array1<String>)new[] { (String)"X", (String)"Y", (String)"Z" };
    public Array<Dynamic> FieldValues => (Array1<Dynamic>)new[] { new Dynamic(X), new Dynamic(Y), new Dynamic(Z) };
    // Unimplemented concept functions
    public Vector3D Multiply(Number other) => throw new NotImplementedException();
    public static Vector3D operator *(Vector3D self, Number other) => self.Multiply(other);
    public Vector3D Divide(Number other) => throw new NotImplementedException();
    public static Vector3D operator /(Vector3D self, Number other) => self.Divide(other);
    public Vector3D Modulo(Number other) => throw new NotImplementedException();
    public static Vector3D operator %(Vector3D self, Number other) => self.Modulo(other);
    public Vector3D Add(Number other) => throw new NotImplementedException();
    public static Vector3D operator +(Vector3D self, Number other) => self.Add(other);
    public Vector3D Subtract(Number other) => throw new NotImplementedException();
    public static Vector3D operator -(Vector3D self, Number other) => self.Subtract(other);
    public Number Unlerp(Vector3D a, Vector3D b) => throw new NotImplementedException();
    public Integer Compare(Vector3D y) => throw new NotImplementedException();
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
    public Integer Count => throw new NotImplementedException();
    public Number At(Integer n) => throw new NotImplementedException();
    public Number this[Integer n] => At(n);
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
    public static implicit operator (Number, Number, Number, Number)(Vector4D self) => (self.X, self.Y, self.Z, self.W);
    public static implicit operator Vector4D((Number, Number, Number, Number) value) => new Vector4D(value.Item1, value.Item2, value.Item3, value.Item4);
    public void Deconstruct(out Number x, out Number y, out Number z, out Number w) { x = X; y = Y; z = Z; w = W; }
    public String TypeName => "Vector4D";
    public Array<String> FieldNames => (Array1<String>)new[] { (String)"X", (String)"Y", (String)"Z", (String)"W" };
    public Array<Dynamic> FieldValues => (Array1<Dynamic>)new[] { new Dynamic(X), new Dynamic(Y), new Dynamic(Z), new Dynamic(W) };
    // Unimplemented concept functions
    public Vector4D Multiply(Number other) => throw new NotImplementedException();
    public static Vector4D operator *(Vector4D self, Number other) => self.Multiply(other);
    public Vector4D Divide(Number other) => throw new NotImplementedException();
    public static Vector4D operator /(Vector4D self, Number other) => self.Divide(other);
    public Vector4D Modulo(Number other) => throw new NotImplementedException();
    public static Vector4D operator %(Vector4D self, Number other) => self.Modulo(other);
    public Vector4D Add(Number other) => throw new NotImplementedException();
    public static Vector4D operator +(Vector4D self, Number other) => self.Add(other);
    public Vector4D Subtract(Number other) => throw new NotImplementedException();
    public static Vector4D operator -(Vector4D self, Number other) => self.Subtract(other);
    public Number Unlerp(Vector4D a, Vector4D b) => throw new NotImplementedException();
    public Integer Compare(Vector4D y) => throw new NotImplementedException();
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
    public Integer Count => throw new NotImplementedException();
    public Number At(Integer n) => throw new NotImplementedException();
    public Number this[Integer n] => At(n);
}
public readonly partial struct Orientation3D: Value<Orientation3D>
{
    public readonly Rotation3D Value;
    public Orientation3D WithValue(Rotation3D value) => (value);
    public Orientation3D(Rotation3D value) => (Value) = (value);
    public static Orientation3D Default = new Orientation3D();
    public static Orientation3D New(Rotation3D value) => new Orientation3D(value);
    public static implicit operator Rotation3D(Orientation3D self) => self.Value;
    public static implicit operator Orientation3D(Rotation3D value) => new Orientation3D(value);
    public String TypeName => "Orientation3D";
    public Array<String> FieldNames => (Array1<String>)new[] { (String)"Value" };
    public Array<Dynamic> FieldValues => (Array1<Dynamic>)new[] { new Dynamic(Value) };
    // Unimplemented concept functions
    public Orientation3D Zero => (Value.Zero);
    public Orientation3D One => (Value.One);
    public Orientation3D MinValue => (Value.MinValue);
    public Orientation3D MaxValue => (Value.MaxValue);
    public Boolean Equals(Orientation3D b) => (Value.Equals(b.Value));
    public static Boolean operator ==(Orientation3D a, Orientation3D b) => a.Equals(b);
    public Boolean NotEquals(Orientation3D b) => (Value.NotEquals(b.Value));
    public static Boolean operator !=(Orientation3D a, Orientation3D b) => a.NotEquals(b);
}
public readonly partial struct Pose2D: Value<Pose2D>
{
    public readonly Vector3D Position;
    public readonly Orientation3D Orientation;
    public Pose2D WithPosition(Vector3D position) => (position, Orientation);
    public Pose2D WithOrientation(Orientation3D orientation) => (Position, orientation);
    public Pose2D(Vector3D position, Orientation3D orientation) => (Position, Orientation) = (position, orientation);
    public static Pose2D Default = new Pose2D();
    public static Pose2D New(Vector3D position, Orientation3D orientation) => new Pose2D(position, orientation);
    public static implicit operator (Vector3D, Orientation3D)(Pose2D self) => (self.Position, self.Orientation);
    public static implicit operator Pose2D((Vector3D, Orientation3D) value) => new Pose2D(value.Item1, value.Item2);
    public void Deconstruct(out Vector3D position, out Orientation3D orientation) { position = Position; orientation = Orientation; }
    public String TypeName => "Pose2D";
    public Array<String> FieldNames => (Array1<String>)new[] { (String)"Position", (String)"Orientation" };
    public Array<Dynamic> FieldValues => (Array1<Dynamic>)new[] { new Dynamic(Position), new Dynamic(Orientation) };
    // Unimplemented concept functions
    public Pose2D Zero => (Position.Zero, Orientation.Zero);
    public Pose2D One => (Position.One, Orientation.One);
    public Pose2D MinValue => (Position.MinValue, Orientation.MinValue);
    public Pose2D MaxValue => (Position.MaxValue, Orientation.MaxValue);
    public Boolean Equals(Pose2D b) => (Position.Equals(b.Position) & Orientation.Equals(b.Orientation));
    public static Boolean operator ==(Pose2D a, Pose2D b) => a.Equals(b);
    public Boolean NotEquals(Pose2D b) => (Position.NotEquals(b.Position) & Orientation.NotEquals(b.Orientation));
    public static Boolean operator !=(Pose2D a, Pose2D b) => a.NotEquals(b);
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
    public static implicit operator (Vector3D, Orientation3D)(Pose3D self) => (self.Position, self.Orientation);
    public static implicit operator Pose3D((Vector3D, Orientation3D) value) => new Pose3D(value.Item1, value.Item2);
    public void Deconstruct(out Vector3D position, out Orientation3D orientation) { position = Position; orientation = Orientation; }
    public String TypeName => "Pose3D";
    public Array<String> FieldNames => (Array1<String>)new[] { (String)"Position", (String)"Orientation" };
    public Array<Dynamic> FieldValues => (Array1<Dynamic>)new[] { new Dynamic(Position), new Dynamic(Orientation) };
    // Unimplemented concept functions
    public Pose3D Zero => (Position.Zero, Orientation.Zero);
    public Pose3D One => (Position.One, Orientation.One);
    public Pose3D MinValue => (Position.MinValue, Orientation.MinValue);
    public Pose3D MaxValue => (Position.MaxValue, Orientation.MaxValue);
    public Boolean Equals(Pose3D b) => (Position.Equals(b.Position) & Orientation.Equals(b.Orientation));
    public static Boolean operator ==(Pose3D a, Pose3D b) => a.Equals(b);
    public Boolean NotEquals(Pose3D b) => (Position.NotEquals(b.Position) & Orientation.NotEquals(b.Orientation));
    public static Boolean operator !=(Pose3D a, Pose3D b) => a.NotEquals(b);
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
    public static implicit operator (Vector3D, Rotation3D, Vector3D)(Transform3D self) => (self.Translation, self.Rotation, self.Scale);
    public static implicit operator Transform3D((Vector3D, Rotation3D, Vector3D) value) => new Transform3D(value.Item1, value.Item2, value.Item3);
    public void Deconstruct(out Vector3D translation, out Rotation3D rotation, out Vector3D scale) { translation = Translation; rotation = Rotation; scale = Scale; }
    public String TypeName => "Transform3D";
    public Array<String> FieldNames => (Array1<String>)new[] { (String)"Translation", (String)"Rotation", (String)"Scale" };
    public Array<Dynamic> FieldValues => (Array1<Dynamic>)new[] { new Dynamic(Translation), new Dynamic(Rotation), new Dynamic(Scale) };
    // Unimplemented concept functions
    public Transform3D Zero => (Translation.Zero, Rotation.Zero, Scale.Zero);
    public Transform3D One => (Translation.One, Rotation.One, Scale.One);
    public Transform3D MinValue => (Translation.MinValue, Rotation.MinValue, Scale.MinValue);
    public Transform3D MaxValue => (Translation.MaxValue, Rotation.MaxValue, Scale.MaxValue);
    public Boolean Equals(Transform3D b) => (Translation.Equals(b.Translation) & Rotation.Equals(b.Rotation) & Scale.Equals(b.Scale));
    public static Boolean operator ==(Transform3D a, Transform3D b) => a.Equals(b);
    public Boolean NotEquals(Transform3D b) => (Translation.NotEquals(b.Translation) & Rotation.NotEquals(b.Rotation) & Scale.NotEquals(b.Scale));
    public static Boolean operator !=(Transform3D a, Transform3D b) => a.NotEquals(b);
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
    public static implicit operator (Vector2D, Angle, Vector2D)(Transform2D self) => (self.Translation, self.Rotation, self.Scale);
    public static implicit operator Transform2D((Vector2D, Angle, Vector2D) value) => new Transform2D(value.Item1, value.Item2, value.Item3);
    public void Deconstruct(out Vector2D translation, out Angle rotation, out Vector2D scale) { translation = Translation; rotation = Rotation; scale = Scale; }
    public String TypeName => "Transform2D";
    public Array<String> FieldNames => (Array1<String>)new[] { (String)"Translation", (String)"Rotation", (String)"Scale" };
    public Array<Dynamic> FieldValues => (Array1<Dynamic>)new[] { new Dynamic(Translation), new Dynamic(Rotation), new Dynamic(Scale) };
    // Unimplemented concept functions
    public Transform2D Zero => (Translation.Zero, Rotation.Zero, Scale.Zero);
    public Transform2D One => (Translation.One, Rotation.One, Scale.One);
    public Transform2D MinValue => (Translation.MinValue, Rotation.MinValue, Scale.MinValue);
    public Transform2D MaxValue => (Translation.MaxValue, Rotation.MaxValue, Scale.MaxValue);
    public Boolean Equals(Transform2D b) => (Translation.Equals(b.Translation) & Rotation.Equals(b.Rotation) & Scale.Equals(b.Scale));
    public static Boolean operator ==(Transform2D a, Transform2D b) => a.Equals(b);
    public Boolean NotEquals(Transform2D b) => (Translation.NotEquals(b.Translation) & Rotation.NotEquals(b.Rotation) & Scale.NotEquals(b.Scale));
    public static Boolean operator !=(Transform2D a, Transform2D b) => a.NotEquals(b);
}
public readonly partial struct AlignedBox2D: Interval<AlignedBox2D, Point2D, Vector2D>
{
    public readonly Point2D A;
    public readonly Point2D B;
    public AlignedBox2D WithA(Point2D a) => (a, B);
    public AlignedBox2D WithB(Point2D b) => (A, b);
    public AlignedBox2D(Point2D a, Point2D b) => (A, B) = (a, b);
    public static AlignedBox2D Default = new AlignedBox2D();
    public static AlignedBox2D New(Point2D a, Point2D b) => new AlignedBox2D(a, b);
    public static implicit operator (Point2D, Point2D)(AlignedBox2D self) => (self.A, self.B);
    public static implicit operator AlignedBox2D((Point2D, Point2D) value) => new AlignedBox2D(value.Item1, value.Item2);
    public void Deconstruct(out Point2D a, out Point2D b) { a = A; b = B; }
    public String TypeName => "AlignedBox2D";
    public Array<String> FieldNames => (Array1<String>)new[] { (String)"A", (String)"B" };
    public Array<Dynamic> FieldValues => (Array1<Dynamic>)new[] { new Dynamic(A), new Dynamic(B) };
    // Unimplemented concept functions
    public Point2D Min => throw new NotImplementedException();
    public Point2D Max => throw new NotImplementedException();
    public Vector2D Size => throw new NotImplementedException();
    public AlignedBox2D Zero => (A.Zero, B.Zero);
    public AlignedBox2D One => (A.One, B.One);
    public AlignedBox2D MinValue => (A.MinValue, B.MinValue);
    public AlignedBox2D MaxValue => (A.MaxValue, B.MaxValue);
    public Boolean Equals(AlignedBox2D b) => (A.Equals(b.A) & B.Equals(b.B));
    public static Boolean operator ==(AlignedBox2D a, AlignedBox2D b) => a.Equals(b);
    public Boolean NotEquals(AlignedBox2D b) => (A.NotEquals(b.A) & B.NotEquals(b.B));
    public static Boolean operator !=(AlignedBox2D a, AlignedBox2D b) => a.NotEquals(b);
}
public readonly partial struct AlignedBox3D: Interval<AlignedBox3D, Point3D, Vector3D>
{
    public readonly Point3D A;
    public readonly Point3D B;
    public AlignedBox3D WithA(Point3D a) => (a, B);
    public AlignedBox3D WithB(Point3D b) => (A, b);
    public AlignedBox3D(Point3D a, Point3D b) => (A, B) = (a, b);
    public static AlignedBox3D Default = new AlignedBox3D();
    public static AlignedBox3D New(Point3D a, Point3D b) => new AlignedBox3D(a, b);
    public static implicit operator (Point3D, Point3D)(AlignedBox3D self) => (self.A, self.B);
    public static implicit operator AlignedBox3D((Point3D, Point3D) value) => new AlignedBox3D(value.Item1, value.Item2);
    public void Deconstruct(out Point3D a, out Point3D b) { a = A; b = B; }
    public String TypeName => "AlignedBox3D";
    public Array<String> FieldNames => (Array1<String>)new[] { (String)"A", (String)"B" };
    public Array<Dynamic> FieldValues => (Array1<Dynamic>)new[] { new Dynamic(A), new Dynamic(B) };
    // Unimplemented concept functions
    public Point3D Min => throw new NotImplementedException();
    public Point3D Max => throw new NotImplementedException();
    public Vector3D Size => throw new NotImplementedException();
    public AlignedBox3D Zero => (A.Zero, B.Zero);
    public AlignedBox3D One => (A.One, B.One);
    public AlignedBox3D MinValue => (A.MinValue, B.MinValue);
    public AlignedBox3D MaxValue => (A.MaxValue, B.MaxValue);
    public Boolean Equals(AlignedBox3D b) => (A.Equals(b.A) & B.Equals(b.B));
    public static Boolean operator ==(AlignedBox3D a, AlignedBox3D b) => a.Equals(b);
    public Boolean NotEquals(AlignedBox3D b) => (A.NotEquals(b.A) & B.NotEquals(b.B));
    public static Boolean operator !=(AlignedBox3D a, AlignedBox3D b) => a.NotEquals(b);
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
    public static implicit operator (Number, Number)(Complex self) => (self.Real, self.Imaginary);
    public static implicit operator Complex((Number, Number) value) => new Complex(value.Item1, value.Item2);
    public void Deconstruct(out Number real, out Number imaginary) { real = Real; imaginary = Imaginary; }
    public String TypeName => "Complex";
    public Array<String> FieldNames => (Array1<String>)new[] { (String)"Real", (String)"Imaginary" };
    public Array<Dynamic> FieldValues => (Array1<Dynamic>)new[] { new Dynamic(Real), new Dynamic(Imaginary) };
    // Unimplemented concept functions
    public Complex Multiply(Number other) => throw new NotImplementedException();
    public static Complex operator *(Complex self, Number other) => self.Multiply(other);
    public Complex Divide(Number other) => throw new NotImplementedException();
    public static Complex operator /(Complex self, Number other) => self.Divide(other);
    public Complex Modulo(Number other) => throw new NotImplementedException();
    public static Complex operator %(Complex self, Number other) => self.Modulo(other);
    public Complex Add(Number other) => throw new NotImplementedException();
    public static Complex operator +(Complex self, Number other) => self.Add(other);
    public Complex Subtract(Number other) => throw new NotImplementedException();
    public static Complex operator -(Complex self, Number other) => self.Subtract(other);
    public Number Unlerp(Complex a, Complex b) => throw new NotImplementedException();
    public Integer Compare(Complex y) => throw new NotImplementedException();
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
    public Integer Count => throw new NotImplementedException();
    public Number At(Integer n) => throw new NotImplementedException();
    public Number this[Integer n] => At(n);
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
    public static implicit operator (Vector3D, Point3D)(Ray3D self) => (self.Direction, self.Position);
    public static implicit operator Ray3D((Vector3D, Point3D) value) => new Ray3D(value.Item1, value.Item2);
    public void Deconstruct(out Vector3D direction, out Point3D position) { direction = Direction; position = Position; }
    public String TypeName => "Ray3D";
    public Array<String> FieldNames => (Array1<String>)new[] { (String)"Direction", (String)"Position" };
    public Array<Dynamic> FieldValues => (Array1<Dynamic>)new[] { new Dynamic(Direction), new Dynamic(Position) };
    // Unimplemented concept functions
    public Ray3D Zero => (Direction.Zero, Position.Zero);
    public Ray3D One => (Direction.One, Position.One);
    public Ray3D MinValue => (Direction.MinValue, Position.MinValue);
    public Ray3D MaxValue => (Direction.MaxValue, Position.MaxValue);
    public Boolean Equals(Ray3D b) => (Direction.Equals(b.Direction) & Position.Equals(b.Position));
    public static Boolean operator ==(Ray3D a, Ray3D b) => a.Equals(b);
    public Boolean NotEquals(Ray3D b) => (Direction.NotEquals(b.Direction) & Position.NotEquals(b.Position));
    public static Boolean operator !=(Ray3D a, Ray3D b) => a.NotEquals(b);
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
    public static implicit operator (Vector2D, Point2D)(Ray2D self) => (self.Direction, self.Position);
    public static implicit operator Ray2D((Vector2D, Point2D) value) => new Ray2D(value.Item1, value.Item2);
    public void Deconstruct(out Vector2D direction, out Point2D position) { direction = Direction; position = Position; }
    public String TypeName => "Ray2D";
    public Array<String> FieldNames => (Array1<String>)new[] { (String)"Direction", (String)"Position" };
    public Array<Dynamic> FieldValues => (Array1<Dynamic>)new[] { new Dynamic(Direction), new Dynamic(Position) };
    // Unimplemented concept functions
    public Ray2D Zero => (Direction.Zero, Position.Zero);
    public Ray2D One => (Direction.One, Position.One);
    public Ray2D MinValue => (Direction.MinValue, Position.MinValue);
    public Ray2D MaxValue => (Direction.MaxValue, Position.MaxValue);
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
    public static implicit operator (Point3D, Number)(Sphere self) => (self.Center, self.Radius);
    public static implicit operator Sphere((Point3D, Number) value) => new Sphere(value.Item1, value.Item2);
    public void Deconstruct(out Point3D center, out Number radius) { center = Center; radius = Radius; }
    public String TypeName => "Sphere";
    public Array<String> FieldNames => (Array1<String>)new[] { (String)"Center", (String)"Radius" };
    public Array<Dynamic> FieldValues => (Array1<Dynamic>)new[] { new Dynamic(Center), new Dynamic(Radius) };
    // Unimplemented concept functions
    public Sphere Zero => (Center.Zero, Radius.Zero);
    public Sphere One => (Center.One, Radius.One);
    public Sphere MinValue => (Center.MinValue, Radius.MinValue);
    public Sphere MaxValue => (Center.MaxValue, Radius.MaxValue);
    public Boolean Equals(Sphere b) => (Center.Equals(b.Center) & Radius.Equals(b.Radius));
    public static Boolean operator ==(Sphere a, Sphere b) => a.Equals(b);
    public Boolean NotEquals(Sphere b) => (Center.NotEquals(b.Center) & Radius.NotEquals(b.Radius));
    public static Boolean operator !=(Sphere a, Sphere b) => a.NotEquals(b);
}
public readonly partial struct Plane: Value<Plane>
{
    public readonly Unit3D Normal;
    public readonly Number D;
    public Plane WithNormal(Unit3D normal) => (normal, D);
    public Plane WithD(Number d) => (Normal, d);
    public Plane(Unit3D normal, Number d) => (Normal, D) = (normal, d);
    public static Plane Default = new Plane();
    public static Plane New(Unit3D normal, Number d) => new Plane(normal, d);
    public static implicit operator (Unit3D, Number)(Plane self) => (self.Normal, self.D);
    public static implicit operator Plane((Unit3D, Number) value) => new Plane(value.Item1, value.Item2);
    public void Deconstruct(out Unit3D normal, out Number d) { normal = Normal; d = D; }
    public String TypeName => "Plane";
    public Array<String> FieldNames => (Array1<String>)new[] { (String)"Normal", (String)"D" };
    public Array<Dynamic> FieldValues => (Array1<Dynamic>)new[] { new Dynamic(Normal), new Dynamic(D) };
    // Unimplemented concept functions
    public Plane Zero => (Normal.Zero, D.Zero);
    public Plane One => (Normal.One, D.One);
    public Plane MinValue => (Normal.MinValue, D.MinValue);
    public Plane MaxValue => (Normal.MaxValue, D.MaxValue);
    public Boolean Equals(Plane b) => (Normal.Equals(b.Normal) & D.Equals(b.D));
    public static Boolean operator ==(Plane a, Plane b) => a.Equals(b);
    public Boolean NotEquals(Plane b) => (Normal.NotEquals(b.Normal) & D.NotEquals(b.D));
    public static Boolean operator !=(Plane a, Plane b) => a.NotEquals(b);
}
public readonly partial struct Triangle2D: Value<Triangle2D>
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
    public static implicit operator (Point2D, Point2D, Point2D)(Triangle2D self) => (self.A, self.B, self.C);
    public static implicit operator Triangle2D((Point2D, Point2D, Point2D) value) => new Triangle2D(value.Item1, value.Item2, value.Item3);
    public void Deconstruct(out Point2D a, out Point2D b, out Point2D c) { a = A; b = B; c = C; }
    public String TypeName => "Triangle2D";
    public Array<String> FieldNames => (Array1<String>)new[] { (String)"A", (String)"B", (String)"C" };
    public Array<Dynamic> FieldValues => (Array1<Dynamic>)new[] { new Dynamic(A), new Dynamic(B), new Dynamic(C) };
    // Unimplemented concept functions
    public Triangle2D Zero => (A.Zero, B.Zero, C.Zero);
    public Triangle2D One => (A.One, B.One, C.One);
    public Triangle2D MinValue => (A.MinValue, B.MinValue, C.MinValue);
    public Triangle2D MaxValue => (A.MaxValue, B.MaxValue, C.MaxValue);
    public Boolean Equals(Triangle2D b) => (A.Equals(b.A) & B.Equals(b.B) & C.Equals(b.C));
    public static Boolean operator ==(Triangle2D a, Triangle2D b) => a.Equals(b);
    public Boolean NotEquals(Triangle2D b) => (A.NotEquals(b.A) & B.NotEquals(b.B) & C.NotEquals(b.C));
    public static Boolean operator !=(Triangle2D a, Triangle2D b) => a.NotEquals(b);
}
public readonly partial struct Triangle3D: Value<Triangle3D>
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
    public static implicit operator (Point3D, Point3D, Point3D)(Triangle3D self) => (self.A, self.B, self.C);
    public static implicit operator Triangle3D((Point3D, Point3D, Point3D) value) => new Triangle3D(value.Item1, value.Item2, value.Item3);
    public void Deconstruct(out Point3D a, out Point3D b, out Point3D c) { a = A; b = B; c = C; }
    public String TypeName => "Triangle3D";
    public Array<String> FieldNames => (Array1<String>)new[] { (String)"A", (String)"B", (String)"C" };
    public Array<Dynamic> FieldValues => (Array1<Dynamic>)new[] { new Dynamic(A), new Dynamic(B), new Dynamic(C) };
    // Unimplemented concept functions
    public Triangle3D Zero => (A.Zero, B.Zero, C.Zero);
    public Triangle3D One => (A.One, B.One, C.One);
    public Triangle3D MinValue => (A.MinValue, B.MinValue, C.MinValue);
    public Triangle3D MaxValue => (A.MaxValue, B.MaxValue, C.MaxValue);
    public Boolean Equals(Triangle3D b) => (A.Equals(b.A) & B.Equals(b.B) & C.Equals(b.C));
    public static Boolean operator ==(Triangle3D a, Triangle3D b) => a.Equals(b);
    public Boolean NotEquals(Triangle3D b) => (A.NotEquals(b.A) & B.NotEquals(b.B) & C.NotEquals(b.C));
    public static Boolean operator !=(Triangle3D a, Triangle3D b) => a.NotEquals(b);
}
public readonly partial struct Quad2D: Value<Quad2D>
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
    public static implicit operator (Point2D, Point2D, Point2D, Point2D)(Quad2D self) => (self.A, self.B, self.C, self.D);
    public static implicit operator Quad2D((Point2D, Point2D, Point2D, Point2D) value) => new Quad2D(value.Item1, value.Item2, value.Item3, value.Item4);
    public void Deconstruct(out Point2D a, out Point2D b, out Point2D c, out Point2D d) { a = A; b = B; c = C; d = D; }
    public String TypeName => "Quad2D";
    public Array<String> FieldNames => (Array1<String>)new[] { (String)"A", (String)"B", (String)"C", (String)"D" };
    public Array<Dynamic> FieldValues => (Array1<Dynamic>)new[] { new Dynamic(A), new Dynamic(B), new Dynamic(C), new Dynamic(D) };
    // Unimplemented concept functions
    public Quad2D Zero => (A.Zero, B.Zero, C.Zero, D.Zero);
    public Quad2D One => (A.One, B.One, C.One, D.One);
    public Quad2D MinValue => (A.MinValue, B.MinValue, C.MinValue, D.MinValue);
    public Quad2D MaxValue => (A.MaxValue, B.MaxValue, C.MaxValue, D.MaxValue);
    public Boolean Equals(Quad2D b) => (A.Equals(b.A) & B.Equals(b.B) & C.Equals(b.C) & D.Equals(b.D));
    public static Boolean operator ==(Quad2D a, Quad2D b) => a.Equals(b);
    public Boolean NotEquals(Quad2D b) => (A.NotEquals(b.A) & B.NotEquals(b.B) & C.NotEquals(b.C) & D.NotEquals(b.D));
    public static Boolean operator !=(Quad2D a, Quad2D b) => a.NotEquals(b);
}
public readonly partial struct Quad3D: Value<Quad3D>
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
    public static implicit operator (Point3D, Point3D, Point3D, Point3D)(Quad3D self) => (self.A, self.B, self.C, self.D);
    public static implicit operator Quad3D((Point3D, Point3D, Point3D, Point3D) value) => new Quad3D(value.Item1, value.Item2, value.Item3, value.Item4);
    public void Deconstruct(out Point3D a, out Point3D b, out Point3D c, out Point3D d) { a = A; b = B; c = C; d = D; }
    public String TypeName => "Quad3D";
    public Array<String> FieldNames => (Array1<String>)new[] { (String)"A", (String)"B", (String)"C", (String)"D" };
    public Array<Dynamic> FieldValues => (Array1<Dynamic>)new[] { new Dynamic(A), new Dynamic(B), new Dynamic(C), new Dynamic(D) };
    // Unimplemented concept functions
    public Quad3D Zero => (A.Zero, B.Zero, C.Zero, D.Zero);
    public Quad3D One => (A.One, B.One, C.One, D.One);
    public Quad3D MinValue => (A.MinValue, B.MinValue, C.MinValue, D.MinValue);
    public Quad3D MaxValue => (A.MaxValue, B.MaxValue, C.MaxValue, D.MaxValue);
    public Boolean Equals(Quad3D b) => (A.Equals(b.A) & B.Equals(b.B) & C.Equals(b.C) & D.Equals(b.D));
    public static Boolean operator ==(Quad3D a, Quad3D b) => a.Equals(b);
    public Boolean NotEquals(Quad3D b) => (A.NotEquals(b.A) & B.NotEquals(b.B) & C.NotEquals(b.C) & D.NotEquals(b.D));
    public static Boolean operator !=(Quad3D a, Quad3D b) => a.NotEquals(b);
}
public readonly partial struct Point2D: Coordinate<Point2D>, AdditiveArithmetic<Point2D, Vector2D>
{
    public readonly Number X;
    public readonly Number Y;
    public Point2D WithX(Number x) => (x, Y);
    public Point2D WithY(Number y) => (X, y);
    public Point2D(Number x, Number y) => (X, Y) = (x, y);
    public static Point2D Default = new Point2D();
    public static Point2D New(Number x, Number y) => new Point2D(x, y);
    public static implicit operator (Number, Number)(Point2D self) => (self.X, self.Y);
    public static implicit operator Point2D((Number, Number) value) => new Point2D(value.Item1, value.Item2);
    public void Deconstruct(out Number x, out Number y) { x = X; y = Y; }
    public String TypeName => "Point2D";
    public Array<String> FieldNames => (Array1<String>)new[] { (String)"X", (String)"Y" };
    public Array<Dynamic> FieldValues => (Array1<Dynamic>)new[] { new Dynamic(X), new Dynamic(Y) };
    // Unimplemented concept functions
    public Point2D Zero => (X.Zero, Y.Zero);
    public Point2D One => (X.One, Y.One);
    public Point2D MinValue => (X.MinValue, Y.MinValue);
    public Point2D MaxValue => (X.MaxValue, Y.MaxValue);
    public Integer Compare(Point2D y) => throw new NotImplementedException();
    public Point2D Lerp(Point2D b, Number amount) => throw new NotImplementedException();
    public Number Unlerp(Point2D a, Point2D b) => throw new NotImplementedException();
    public Point2D Add(Vector2D other) => throw new NotImplementedException();
    public static Point2D operator +(Point2D self, Vector2D other) => self.Add(other);
    public Point2D Subtract(Vector2D other) => throw new NotImplementedException();
    public static Point2D operator -(Point2D self, Vector2D other) => self.Subtract(other);
}
public readonly partial struct Point3D: Coordinate<Point3D>, AdditiveArithmetic<Point3D, Vector3D>
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
    public static implicit operator (Number, Number, Number)(Point3D self) => (self.X, self.Y, self.Z);
    public static implicit operator Point3D((Number, Number, Number) value) => new Point3D(value.Item1, value.Item2, value.Item3);
    public void Deconstruct(out Number x, out Number y, out Number z) { x = X; y = Y; z = Z; }
    public String TypeName => "Point3D";
    public Array<String> FieldNames => (Array1<String>)new[] { (String)"X", (String)"Y", (String)"Z" };
    public Array<Dynamic> FieldValues => (Array1<Dynamic>)new[] { new Dynamic(X), new Dynamic(Y), new Dynamic(Z) };
    // Unimplemented concept functions
    public Point3D Zero => (X.Zero, Y.Zero, Z.Zero);
    public Point3D One => (X.One, Y.One, Z.One);
    public Point3D MinValue => (X.MinValue, Y.MinValue, Z.MinValue);
    public Point3D MaxValue => (X.MaxValue, Y.MaxValue, Z.MaxValue);
    public Integer Compare(Point3D y) => throw new NotImplementedException();
    public Point3D Lerp(Point3D b, Number amount) => throw new NotImplementedException();
    public Number Unlerp(Point3D a, Point3D b) => throw new NotImplementedException();
    public Point3D Add(Vector3D other) => throw new NotImplementedException();
    public static Point3D operator +(Point3D self, Vector3D other) => self.Add(other);
    public Point3D Subtract(Vector3D other) => throw new NotImplementedException();
    public static Point3D operator -(Point3D self, Vector3D other) => self.Subtract(other);
}
public readonly partial struct Point4D: Coordinate<Point4D>, AdditiveArithmetic<Point4D, Vector4D>
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
    public static implicit operator (Number, Number, Number, Number)(Point4D self) => (self.X, self.Y, self.Z, self.W);
    public static implicit operator Point4D((Number, Number, Number, Number) value) => new Point4D(value.Item1, value.Item2, value.Item3, value.Item4);
    public void Deconstruct(out Number x, out Number y, out Number z, out Number w) { x = X; y = Y; z = Z; w = W; }
    public String TypeName => "Point4D";
    public Array<String> FieldNames => (Array1<String>)new[] { (String)"X", (String)"Y", (String)"Z", (String)"W" };
    public Array<Dynamic> FieldValues => (Array1<Dynamic>)new[] { new Dynamic(X), new Dynamic(Y), new Dynamic(Z), new Dynamic(W) };
    // Unimplemented concept functions
    public Point4D Zero => (X.Zero, Y.Zero, Z.Zero, W.Zero);
    public Point4D One => (X.One, Y.One, Z.One, W.One);
    public Point4D MinValue => (X.MinValue, Y.MinValue, Z.MinValue, W.MinValue);
    public Point4D MaxValue => (X.MaxValue, Y.MaxValue, Z.MaxValue, W.MaxValue);
    public Integer Compare(Point4D y) => throw new NotImplementedException();
    public Point4D Lerp(Point4D b, Number amount) => throw new NotImplementedException();
    public Number Unlerp(Point4D a, Point4D b) => throw new NotImplementedException();
    public Point4D Add(Vector4D other) => throw new NotImplementedException();
    public static Point4D operator +(Point4D self, Vector4D other) => self.Add(other);
    public Point4D Subtract(Vector4D other) => throw new NotImplementedException();
    public static Point4D operator -(Point4D self, Vector4D other) => self.Subtract(other);
}
public readonly partial struct Line2D: Interval<Line2D, Point2D, Vector2D>
{
    public readonly Point2D A;
    public readonly Point2D B;
    public Line2D WithA(Point2D a) => (a, B);
    public Line2D WithB(Point2D b) => (A, b);
    public Line2D(Point2D a, Point2D b) => (A, B) = (a, b);
    public static Line2D Default = new Line2D();
    public static Line2D New(Point2D a, Point2D b) => new Line2D(a, b);
    public static implicit operator (Point2D, Point2D)(Line2D self) => (self.A, self.B);
    public static implicit operator Line2D((Point2D, Point2D) value) => new Line2D(value.Item1, value.Item2);
    public void Deconstruct(out Point2D a, out Point2D b) { a = A; b = B; }
    public String TypeName => "Line2D";
    public Array<String> FieldNames => (Array1<String>)new[] { (String)"A", (String)"B" };
    public Array<Dynamic> FieldValues => (Array1<Dynamic>)new[] { new Dynamic(A), new Dynamic(B) };
    // Unimplemented concept functions
    public Point2D Min => throw new NotImplementedException();
    public Point2D Max => throw new NotImplementedException();
    public Vector2D Size => throw new NotImplementedException();
    public Line2D Zero => (A.Zero, B.Zero);
    public Line2D One => (A.One, B.One);
    public Line2D MinValue => (A.MinValue, B.MinValue);
    public Line2D MaxValue => (A.MaxValue, B.MaxValue);
    public Boolean Equals(Line2D b) => (A.Equals(b.A) & B.Equals(b.B));
    public static Boolean operator ==(Line2D a, Line2D b) => a.Equals(b);
    public Boolean NotEquals(Line2D b) => (A.NotEquals(b.A) & B.NotEquals(b.B));
    public static Boolean operator !=(Line2D a, Line2D b) => a.NotEquals(b);
}
public readonly partial struct Line3D: Interval<Line3D, Point3D, Vector3D>
{
    public readonly Point3D A;
    public readonly Point3D B;
    public Line3D WithA(Point3D a) => (a, B);
    public Line3D WithB(Point3D b) => (A, b);
    public Line3D(Point3D a, Point3D b) => (A, B) = (a, b);
    public static Line3D Default = new Line3D();
    public static Line3D New(Point3D a, Point3D b) => new Line3D(a, b);
    public static implicit operator (Point3D, Point3D)(Line3D self) => (self.A, self.B);
    public static implicit operator Line3D((Point3D, Point3D) value) => new Line3D(value.Item1, value.Item2);
    public void Deconstruct(out Point3D a, out Point3D b) { a = A; b = B; }
    public String TypeName => "Line3D";
    public Array<String> FieldNames => (Array1<String>)new[] { (String)"A", (String)"B" };
    public Array<Dynamic> FieldValues => (Array1<Dynamic>)new[] { new Dynamic(A), new Dynamic(B) };
    // Unimplemented concept functions
    public Point3D Min => throw new NotImplementedException();
    public Point3D Max => throw new NotImplementedException();
    public Vector3D Size => throw new NotImplementedException();
    public Line3D Zero => (A.Zero, B.Zero);
    public Line3D One => (A.One, B.One);
    public Line3D MinValue => (A.MinValue, B.MinValue);
    public Line3D MaxValue => (A.MaxValue, B.MaxValue);
    public Boolean Equals(Line3D b) => (A.Equals(b.A) & B.Equals(b.B));
    public static Boolean operator ==(Line3D a, Line3D b) => a.Equals(b);
    public Boolean NotEquals(Line3D b) => (A.NotEquals(b.A) & B.NotEquals(b.B));
    public static Boolean operator !=(Line3D a, Line3D b) => a.NotEquals(b);
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
    public static implicit operator (Unit, Unit, Unit, Unit)(Color self) => (self.R, self.G, self.B, self.A);
    public static implicit operator Color((Unit, Unit, Unit, Unit) value) => new Color(value.Item1, value.Item2, value.Item3, value.Item4);
    public void Deconstruct(out Unit r, out Unit g, out Unit b, out Unit a) { r = R; g = G; b = B; a = A; }
    public String TypeName => "Color";
    public Array<String> FieldNames => (Array1<String>)new[] { (String)"R", (String)"G", (String)"B", (String)"A" };
    public Array<Dynamic> FieldValues => (Array1<Dynamic>)new[] { new Dynamic(R), new Dynamic(G), new Dynamic(B), new Dynamic(A) };
    // Unimplemented concept functions
    public Color Zero => (R.Zero, G.Zero, B.Zero, A.Zero);
    public Color One => (R.One, G.One, B.One, A.One);
    public Color MinValue => (R.MinValue, G.MinValue, B.MinValue, A.MinValue);
    public Color MaxValue => (R.MaxValue, G.MaxValue, B.MaxValue, A.MaxValue);
    public Integer Compare(Color y) => throw new NotImplementedException();
    public Color Lerp(Color b, Number amount) => throw new NotImplementedException();
    public Number Unlerp(Color a, Color b) => throw new NotImplementedException();
}
public readonly partial struct ColorLUV: Coordinate<ColorLUV>
{
    public readonly Percent Lightness;
    public readonly Unit U;
    public readonly Unit V;
    public ColorLUV WithLightness(Percent lightness) => (lightness, U, V);
    public ColorLUV WithU(Unit u) => (Lightness, u, V);
    public ColorLUV WithV(Unit v) => (Lightness, U, v);
    public ColorLUV(Percent lightness, Unit u, Unit v) => (Lightness, U, V) = (lightness, u, v);
    public static ColorLUV Default = new ColorLUV();
    public static ColorLUV New(Percent lightness, Unit u, Unit v) => new ColorLUV(lightness, u, v);
    public static implicit operator (Percent, Unit, Unit)(ColorLUV self) => (self.Lightness, self.U, self.V);
    public static implicit operator ColorLUV((Percent, Unit, Unit) value) => new ColorLUV(value.Item1, value.Item2, value.Item3);
    public void Deconstruct(out Percent lightness, out Unit u, out Unit v) { lightness = Lightness; u = U; v = V; }
    public String TypeName => "ColorLUV";
    public Array<String> FieldNames => (Array1<String>)new[] { (String)"Lightness", (String)"U", (String)"V" };
    public Array<Dynamic> FieldValues => (Array1<Dynamic>)new[] { new Dynamic(Lightness), new Dynamic(U), new Dynamic(V) };
    // Unimplemented concept functions
    public ColorLUV Zero => (Lightness.Zero, U.Zero, V.Zero);
    public ColorLUV One => (Lightness.One, U.One, V.One);
    public ColorLUV MinValue => (Lightness.MinValue, U.MinValue, V.MinValue);
    public ColorLUV MaxValue => (Lightness.MaxValue, U.MaxValue, V.MaxValue);
    public Integer Compare(ColorLUV y) => throw new NotImplementedException();
    public ColorLUV Lerp(ColorLUV b, Number amount) => throw new NotImplementedException();
    public Number Unlerp(ColorLUV a, ColorLUV b) => throw new NotImplementedException();
}
public readonly partial struct ColorLAB: Coordinate<ColorLAB>
{
    public readonly Percent Lightness;
    public readonly Integer A;
    public readonly Integer B;
    public ColorLAB WithLightness(Percent lightness) => (lightness, A, B);
    public ColorLAB WithA(Integer a) => (Lightness, a, B);
    public ColorLAB WithB(Integer b) => (Lightness, A, b);
    public ColorLAB(Percent lightness, Integer a, Integer b) => (Lightness, A, B) = (lightness, a, b);
    public static ColorLAB Default = new ColorLAB();
    public static ColorLAB New(Percent lightness, Integer a, Integer b) => new ColorLAB(lightness, a, b);
    public static implicit operator (Percent, Integer, Integer)(ColorLAB self) => (self.Lightness, self.A, self.B);
    public static implicit operator ColorLAB((Percent, Integer, Integer) value) => new ColorLAB(value.Item1, value.Item2, value.Item3);
    public void Deconstruct(out Percent lightness, out Integer a, out Integer b) { lightness = Lightness; a = A; b = B; }
    public String TypeName => "ColorLAB";
    public Array<String> FieldNames => (Array1<String>)new[] { (String)"Lightness", (String)"A", (String)"B" };
    public Array<Dynamic> FieldValues => (Array1<Dynamic>)new[] { new Dynamic(Lightness), new Dynamic(A), new Dynamic(B) };
    // Unimplemented concept functions
    public ColorLAB Zero => (Lightness.Zero, A.Zero, B.Zero);
    public ColorLAB One => (Lightness.One, A.One, B.One);
    public ColorLAB MinValue => (Lightness.MinValue, A.MinValue, B.MinValue);
    public ColorLAB MaxValue => (Lightness.MaxValue, A.MaxValue, B.MaxValue);
    public Integer Compare(ColorLAB y) => throw new NotImplementedException();
    public ColorLAB Lerp(ColorLAB b, Number amount) => throw new NotImplementedException();
    public Number Unlerp(ColorLAB a, ColorLAB b) => throw new NotImplementedException();
}
public readonly partial struct ColorLCh: Coordinate<ColorLCh>
{
    public readonly Percent Lightness;
    public readonly PolarCoordinate ChromaHue;
    public ColorLCh WithLightness(Percent lightness) => (lightness, ChromaHue);
    public ColorLCh WithChromaHue(PolarCoordinate chromaHue) => (Lightness, chromaHue);
    public ColorLCh(Percent lightness, PolarCoordinate chromaHue) => (Lightness, ChromaHue) = (lightness, chromaHue);
    public static ColorLCh Default = new ColorLCh();
    public static ColorLCh New(Percent lightness, PolarCoordinate chromaHue) => new ColorLCh(lightness, chromaHue);
    public static implicit operator (Percent, PolarCoordinate)(ColorLCh self) => (self.Lightness, self.ChromaHue);
    public static implicit operator ColorLCh((Percent, PolarCoordinate) value) => new ColorLCh(value.Item1, value.Item2);
    public void Deconstruct(out Percent lightness, out PolarCoordinate chromaHue) { lightness = Lightness; chromaHue = ChromaHue; }
    public String TypeName => "ColorLCh";
    public Array<String> FieldNames => (Array1<String>)new[] { (String)"Lightness", (String)"ChromaHue" };
    public Array<Dynamic> FieldValues => (Array1<Dynamic>)new[] { new Dynamic(Lightness), new Dynamic(ChromaHue) };
    // Unimplemented concept functions
    public ColorLCh Zero => (Lightness.Zero, ChromaHue.Zero);
    public ColorLCh One => (Lightness.One, ChromaHue.One);
    public ColorLCh MinValue => (Lightness.MinValue, ChromaHue.MinValue);
    public ColorLCh MaxValue => (Lightness.MaxValue, ChromaHue.MaxValue);
    public Integer Compare(ColorLCh y) => throw new NotImplementedException();
    public ColorLCh Lerp(ColorLCh b, Number amount) => throw new NotImplementedException();
    public Number Unlerp(ColorLCh a, ColorLCh b) => throw new NotImplementedException();
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
    public static implicit operator (Angle, Unit, Unit)(ColorHSV self) => (self.Hue, self.S, self.V);
    public static implicit operator ColorHSV((Angle, Unit, Unit) value) => new ColorHSV(value.Item1, value.Item2, value.Item3);
    public void Deconstruct(out Angle hue, out Unit s, out Unit v) { hue = Hue; s = S; v = V; }
    public String TypeName => "ColorHSV";
    public Array<String> FieldNames => (Array1<String>)new[] { (String)"Hue", (String)"S", (String)"V" };
    public Array<Dynamic> FieldValues => (Array1<Dynamic>)new[] { new Dynamic(Hue), new Dynamic(S), new Dynamic(V) };
    // Unimplemented concept functions
    public ColorHSV Zero => (Hue.Zero, S.Zero, V.Zero);
    public ColorHSV One => (Hue.One, S.One, V.One);
    public ColorHSV MinValue => (Hue.MinValue, S.MinValue, V.MinValue);
    public ColorHSV MaxValue => (Hue.MaxValue, S.MaxValue, V.MaxValue);
    public Integer Compare(ColorHSV y) => throw new NotImplementedException();
    public ColorHSV Lerp(ColorHSV b, Number amount) => throw new NotImplementedException();
    public Number Unlerp(ColorHSV a, ColorHSV b) => throw new NotImplementedException();
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
    public static implicit operator (Angle, Unit, Unit)(ColorHSL self) => (self.Hue, self.Saturation, self.Luminance);
    public static implicit operator ColorHSL((Angle, Unit, Unit) value) => new ColorHSL(value.Item1, value.Item2, value.Item3);
    public void Deconstruct(out Angle hue, out Unit saturation, out Unit luminance) { hue = Hue; saturation = Saturation; luminance = Luminance; }
    public String TypeName => "ColorHSL";
    public Array<String> FieldNames => (Array1<String>)new[] { (String)"Hue", (String)"Saturation", (String)"Luminance" };
    public Array<Dynamic> FieldValues => (Array1<Dynamic>)new[] { new Dynamic(Hue), new Dynamic(Saturation), new Dynamic(Luminance) };
    // Unimplemented concept functions
    public ColorHSL Zero => (Hue.Zero, Saturation.Zero, Luminance.Zero);
    public ColorHSL One => (Hue.One, Saturation.One, Luminance.One);
    public ColorHSL MinValue => (Hue.MinValue, Saturation.MinValue, Luminance.MinValue);
    public ColorHSL MaxValue => (Hue.MaxValue, Saturation.MaxValue, Luminance.MaxValue);
    public Integer Compare(ColorHSL y) => throw new NotImplementedException();
    public ColorHSL Lerp(ColorHSL b, Number amount) => throw new NotImplementedException();
    public Number Unlerp(ColorHSL a, ColorHSL b) => throw new NotImplementedException();
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
    public static implicit operator (Unit, Unit, Unit)(ColorYCbCr self) => (self.Y, self.Cb, self.Cr);
    public static implicit operator ColorYCbCr((Unit, Unit, Unit) value) => new ColorYCbCr(value.Item1, value.Item2, value.Item3);
    public void Deconstruct(out Unit y, out Unit cb, out Unit cr) { y = Y; cb = Cb; cr = Cr; }
    public String TypeName => "ColorYCbCr";
    public Array<String> FieldNames => (Array1<String>)new[] { (String)"Y", (String)"Cb", (String)"Cr" };
    public Array<Dynamic> FieldValues => (Array1<Dynamic>)new[] { new Dynamic(Y), new Dynamic(Cb), new Dynamic(Cr) };
    // Unimplemented concept functions
    public ColorYCbCr Zero => (Y.Zero, Cb.Zero, Cr.Zero);
    public ColorYCbCr One => (Y.One, Cb.One, Cr.One);
    public ColorYCbCr MinValue => (Y.MinValue, Cb.MinValue, Cr.MinValue);
    public ColorYCbCr MaxValue => (Y.MaxValue, Cb.MaxValue, Cr.MaxValue);
    public Integer Compare(ColorYCbCr y) => throw new NotImplementedException();
    public ColorYCbCr Lerp(ColorYCbCr b, Number amount) => throw new NotImplementedException();
    public Number Unlerp(ColorYCbCr a, ColorYCbCr b) => throw new NotImplementedException();
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
    public static implicit operator (Number, Angle, Angle)(SphericalCoordinate self) => (self.Radius, self.Azimuth, self.Polar);
    public static implicit operator SphericalCoordinate((Number, Angle, Angle) value) => new SphericalCoordinate(value.Item1, value.Item2, value.Item3);
    public void Deconstruct(out Number radius, out Angle azimuth, out Angle polar) { radius = Radius; azimuth = Azimuth; polar = Polar; }
    public String TypeName => "SphericalCoordinate";
    public Array<String> FieldNames => (Array1<String>)new[] { (String)"Radius", (String)"Azimuth", (String)"Polar" };
    public Array<Dynamic> FieldValues => (Array1<Dynamic>)new[] { new Dynamic(Radius), new Dynamic(Azimuth), new Dynamic(Polar) };
    // Unimplemented concept functions
    public SphericalCoordinate Zero => (Radius.Zero, Azimuth.Zero, Polar.Zero);
    public SphericalCoordinate One => (Radius.One, Azimuth.One, Polar.One);
    public SphericalCoordinate MinValue => (Radius.MinValue, Azimuth.MinValue, Polar.MinValue);
    public SphericalCoordinate MaxValue => (Radius.MaxValue, Azimuth.MaxValue, Polar.MaxValue);
    public Integer Compare(SphericalCoordinate y) => throw new NotImplementedException();
    public SphericalCoordinate Lerp(SphericalCoordinate b, Number amount) => throw new NotImplementedException();
    public Number Unlerp(SphericalCoordinate a, SphericalCoordinate b) => throw new NotImplementedException();
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
    public static implicit operator (Number, Angle)(PolarCoordinate self) => (self.Radius, self.Angle);
    public static implicit operator PolarCoordinate((Number, Angle) value) => new PolarCoordinate(value.Item1, value.Item2);
    public void Deconstruct(out Number radius, out Angle angle) { radius = Radius; angle = Angle; }
    public String TypeName => "PolarCoordinate";
    public Array<String> FieldNames => (Array1<String>)new[] { (String)"Radius", (String)"Angle" };
    public Array<Dynamic> FieldValues => (Array1<Dynamic>)new[] { new Dynamic(Radius), new Dynamic(Angle) };
    // Unimplemented concept functions
    public PolarCoordinate Zero => (Radius.Zero, Angle.Zero);
    public PolarCoordinate One => (Radius.One, Angle.One);
    public PolarCoordinate MinValue => (Radius.MinValue, Angle.MinValue);
    public PolarCoordinate MaxValue => (Radius.MaxValue, Angle.MaxValue);
    public Integer Compare(PolarCoordinate y) => throw new NotImplementedException();
    public PolarCoordinate Lerp(PolarCoordinate b, Number amount) => throw new NotImplementedException();
    public Number Unlerp(PolarCoordinate a, PolarCoordinate b) => throw new NotImplementedException();
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
    public static implicit operator (Number, Angle)(LogPolarCoordinate self) => (self.Rho, self.Azimuth);
    public static implicit operator LogPolarCoordinate((Number, Angle) value) => new LogPolarCoordinate(value.Item1, value.Item2);
    public void Deconstruct(out Number rho, out Angle azimuth) { rho = Rho; azimuth = Azimuth; }
    public String TypeName => "LogPolarCoordinate";
    public Array<String> FieldNames => (Array1<String>)new[] { (String)"Rho", (String)"Azimuth" };
    public Array<Dynamic> FieldValues => (Array1<Dynamic>)new[] { new Dynamic(Rho), new Dynamic(Azimuth) };
    // Unimplemented concept functions
    public LogPolarCoordinate Zero => (Rho.Zero, Azimuth.Zero);
    public LogPolarCoordinate One => (Rho.One, Azimuth.One);
    public LogPolarCoordinate MinValue => (Rho.MinValue, Azimuth.MinValue);
    public LogPolarCoordinate MaxValue => (Rho.MaxValue, Azimuth.MaxValue);
    public Integer Compare(LogPolarCoordinate y) => throw new NotImplementedException();
    public LogPolarCoordinate Lerp(LogPolarCoordinate b, Number amount) => throw new NotImplementedException();
    public Number Unlerp(LogPolarCoordinate a, LogPolarCoordinate b) => throw new NotImplementedException();
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
    public static implicit operator (Number, Angle, Number)(CylindricalCoordinate self) => (self.RadialDistance, self.Azimuth, self.Height);
    public static implicit operator CylindricalCoordinate((Number, Angle, Number) value) => new CylindricalCoordinate(value.Item1, value.Item2, value.Item3);
    public void Deconstruct(out Number radialDistance, out Angle azimuth, out Number height) { radialDistance = RadialDistance; azimuth = Azimuth; height = Height; }
    public String TypeName => "CylindricalCoordinate";
    public Array<String> FieldNames => (Array1<String>)new[] { (String)"RadialDistance", (String)"Azimuth", (String)"Height" };
    public Array<Dynamic> FieldValues => (Array1<Dynamic>)new[] { new Dynamic(RadialDistance), new Dynamic(Azimuth), new Dynamic(Height) };
    // Unimplemented concept functions
    public CylindricalCoordinate Zero => (RadialDistance.Zero, Azimuth.Zero, Height.Zero);
    public CylindricalCoordinate One => (RadialDistance.One, Azimuth.One, Height.One);
    public CylindricalCoordinate MinValue => (RadialDistance.MinValue, Azimuth.MinValue, Height.MinValue);
    public CylindricalCoordinate MaxValue => (RadialDistance.MaxValue, Azimuth.MaxValue, Height.MaxValue);
    public Integer Compare(CylindricalCoordinate y) => throw new NotImplementedException();
    public CylindricalCoordinate Lerp(CylindricalCoordinate b, Number amount) => throw new NotImplementedException();
    public Number Unlerp(CylindricalCoordinate a, CylindricalCoordinate b) => throw new NotImplementedException();
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
    public static implicit operator (Number, Angle, Number)(HorizontalCoordinate self) => (self.Radius, self.Azimuth, self.Height);
    public static implicit operator HorizontalCoordinate((Number, Angle, Number) value) => new HorizontalCoordinate(value.Item1, value.Item2, value.Item3);
    public void Deconstruct(out Number radius, out Angle azimuth, out Number height) { radius = Radius; azimuth = Azimuth; height = Height; }
    public String TypeName => "HorizontalCoordinate";
    public Array<String> FieldNames => (Array1<String>)new[] { (String)"Radius", (String)"Azimuth", (String)"Height" };
    public Array<Dynamic> FieldValues => (Array1<Dynamic>)new[] { new Dynamic(Radius), new Dynamic(Azimuth), new Dynamic(Height) };
    // Unimplemented concept functions
    public HorizontalCoordinate Zero => (Radius.Zero, Azimuth.Zero, Height.Zero);
    public HorizontalCoordinate One => (Radius.One, Azimuth.One, Height.One);
    public HorizontalCoordinate MinValue => (Radius.MinValue, Azimuth.MinValue, Height.MinValue);
    public HorizontalCoordinate MaxValue => (Radius.MaxValue, Azimuth.MaxValue, Height.MaxValue);
    public Integer Compare(HorizontalCoordinate y) => throw new NotImplementedException();
    public HorizontalCoordinate Lerp(HorizontalCoordinate b, Number amount) => throw new NotImplementedException();
    public Number Unlerp(HorizontalCoordinate a, HorizontalCoordinate b) => throw new NotImplementedException();
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
    public static implicit operator (Angle, Angle)(GeoCoordinate self) => (self.Latitude, self.Longitude);
    public static implicit operator GeoCoordinate((Angle, Angle) value) => new GeoCoordinate(value.Item1, value.Item2);
    public void Deconstruct(out Angle latitude, out Angle longitude) { latitude = Latitude; longitude = Longitude; }
    public String TypeName => "GeoCoordinate";
    public Array<String> FieldNames => (Array1<String>)new[] { (String)"Latitude", (String)"Longitude" };
    public Array<Dynamic> FieldValues => (Array1<Dynamic>)new[] { new Dynamic(Latitude), new Dynamic(Longitude) };
    // Unimplemented concept functions
    public GeoCoordinate Zero => (Latitude.Zero, Longitude.Zero);
    public GeoCoordinate One => (Latitude.One, Longitude.One);
    public GeoCoordinate MinValue => (Latitude.MinValue, Longitude.MinValue);
    public GeoCoordinate MaxValue => (Latitude.MaxValue, Longitude.MaxValue);
    public Integer Compare(GeoCoordinate y) => throw new NotImplementedException();
    public GeoCoordinate Lerp(GeoCoordinate b, Number amount) => throw new NotImplementedException();
    public Number Unlerp(GeoCoordinate a, GeoCoordinate b) => throw new NotImplementedException();
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
    public static implicit operator (GeoCoordinate, Number)(GeoCoordinateWithAltitude self) => (self.Coordinate, self.Altitude);
    public static implicit operator GeoCoordinateWithAltitude((GeoCoordinate, Number) value) => new GeoCoordinateWithAltitude(value.Item1, value.Item2);
    public void Deconstruct(out GeoCoordinate coordinate, out Number altitude) { coordinate = Coordinate; altitude = Altitude; }
    public String TypeName => "GeoCoordinateWithAltitude";
    public Array<String> FieldNames => (Array1<String>)new[] { (String)"Coordinate", (String)"Altitude" };
    public Array<Dynamic> FieldValues => (Array1<Dynamic>)new[] { new Dynamic(Coordinate), new Dynamic(Altitude) };
    // Unimplemented concept functions
    public GeoCoordinateWithAltitude Zero => (Coordinate.Zero, Altitude.Zero);
    public GeoCoordinateWithAltitude One => (Coordinate.One, Altitude.One);
    public GeoCoordinateWithAltitude MinValue => (Coordinate.MinValue, Altitude.MinValue);
    public GeoCoordinateWithAltitude MaxValue => (Coordinate.MaxValue, Altitude.MaxValue);
    public Integer Compare(GeoCoordinateWithAltitude y) => throw new NotImplementedException();
    public GeoCoordinateWithAltitude Lerp(GeoCoordinateWithAltitude b, Number amount) => throw new NotImplementedException();
    public Number Unlerp(GeoCoordinateWithAltitude a, GeoCoordinateWithAltitude b) => throw new NotImplementedException();
}
public readonly partial struct Circle: Value<Circle>
{
    public readonly Point2D Center;
    public readonly Number Radius;
    public Circle WithCenter(Point2D center) => (center, Radius);
    public Circle WithRadius(Number radius) => (Center, radius);
    public Circle(Point2D center, Number radius) => (Center, Radius) = (center, radius);
    public static Circle Default = new Circle();
    public static Circle New(Point2D center, Number radius) => new Circle(center, radius);
    public static implicit operator (Point2D, Number)(Circle self) => (self.Center, self.Radius);
    public static implicit operator Circle((Point2D, Number) value) => new Circle(value.Item1, value.Item2);
    public void Deconstruct(out Point2D center, out Number radius) { center = Center; radius = Radius; }
    public String TypeName => "Circle";
    public Array<String> FieldNames => (Array1<String>)new[] { (String)"Center", (String)"Radius" };
    public Array<Dynamic> FieldValues => (Array1<Dynamic>)new[] { new Dynamic(Center), new Dynamic(Radius) };
    // Unimplemented concept functions
    public Circle Zero => (Center.Zero, Radius.Zero);
    public Circle One => (Center.One, Radius.One);
    public Circle MinValue => (Center.MinValue, Radius.MinValue);
    public Circle MaxValue => (Center.MaxValue, Radius.MaxValue);
    public Boolean Equals(Circle b) => (Center.Equals(b.Center) & Radius.Equals(b.Radius));
    public static Boolean operator ==(Circle a, Circle b) => a.Equals(b);
    public Boolean NotEquals(Circle b) => (Center.NotEquals(b.Center) & Radius.NotEquals(b.Radius));
    public static Boolean operator !=(Circle a, Circle b) => a.NotEquals(b);
}
public readonly partial struct Chord: Value<Chord>
{
    public readonly Circle Circle;
    public readonly Arc Arc;
    public Chord WithCircle(Circle circle) => (circle, Arc);
    public Chord WithArc(Arc arc) => (Circle, arc);
    public Chord(Circle circle, Arc arc) => (Circle, Arc) = (circle, arc);
    public static Chord Default = new Chord();
    public static Chord New(Circle circle, Arc arc) => new Chord(circle, arc);
    public static implicit operator (Circle, Arc)(Chord self) => (self.Circle, self.Arc);
    public static implicit operator Chord((Circle, Arc) value) => new Chord(value.Item1, value.Item2);
    public void Deconstruct(out Circle circle, out Arc arc) { circle = Circle; arc = Arc; }
    public String TypeName => "Chord";
    public Array<String> FieldNames => (Array1<String>)new[] { (String)"Circle", (String)"Arc" };
    public Array<Dynamic> FieldValues => (Array1<Dynamic>)new[] { new Dynamic(Circle), new Dynamic(Arc) };
    // Unimplemented concept functions
    public Chord Zero => (Circle.Zero, Arc.Zero);
    public Chord One => (Circle.One, Arc.One);
    public Chord MinValue => (Circle.MinValue, Arc.MinValue);
    public Chord MaxValue => (Circle.MaxValue, Arc.MaxValue);
    public Boolean Equals(Chord b) => (Circle.Equals(b.Circle) & Arc.Equals(b.Arc));
    public static Boolean operator ==(Chord a, Chord b) => a.Equals(b);
    public Boolean NotEquals(Chord b) => (Circle.NotEquals(b.Circle) & Arc.NotEquals(b.Arc));
    public static Boolean operator !=(Chord a, Chord b) => a.NotEquals(b);
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
    public static implicit operator (Number, Number)(Size2D self) => (self.Width, self.Height);
    public static implicit operator Size2D((Number, Number) value) => new Size2D(value.Item1, value.Item2);
    public void Deconstruct(out Number width, out Number height) { width = Width; height = Height; }
    public String TypeName => "Size2D";
    public Array<String> FieldNames => (Array1<String>)new[] { (String)"Width", (String)"Height" };
    public Array<Dynamic> FieldValues => (Array1<Dynamic>)new[] { new Dynamic(Width), new Dynamic(Height) };
    // Unimplemented concept functions
    public Size2D Zero => (Width.Zero, Height.Zero);
    public Size2D One => (Width.One, Height.One);
    public Size2D MinValue => (Width.MinValue, Height.MinValue);
    public Size2D MaxValue => (Width.MaxValue, Height.MaxValue);
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
    public static implicit operator (Number, Number, Number)(Size3D self) => (self.Width, self.Height, self.Depth);
    public static implicit operator Size3D((Number, Number, Number) value) => new Size3D(value.Item1, value.Item2, value.Item3);
    public void Deconstruct(out Number width, out Number height, out Number depth) { width = Width; height = Height; depth = Depth; }
    public String TypeName => "Size3D";
    public Array<String> FieldNames => (Array1<String>)new[] { (String)"Width", (String)"Height", (String)"Depth" };
    public Array<Dynamic> FieldValues => (Array1<Dynamic>)new[] { new Dynamic(Width), new Dynamic(Height), new Dynamic(Depth) };
    // Unimplemented concept functions
    public Size3D Zero => (Width.Zero, Height.Zero, Depth.Zero);
    public Size3D One => (Width.One, Height.One, Depth.One);
    public Size3D MinValue => (Width.MinValue, Height.MinValue, Depth.MinValue);
    public Size3D MaxValue => (Width.MaxValue, Height.MaxValue, Depth.MaxValue);
    public Boolean Equals(Size3D b) => (Width.Equals(b.Width) & Height.Equals(b.Height) & Depth.Equals(b.Depth));
    public static Boolean operator ==(Size3D a, Size3D b) => a.Equals(b);
    public Boolean NotEquals(Size3D b) => (Width.NotEquals(b.Width) & Height.NotEquals(b.Height) & Depth.NotEquals(b.Depth));
    public static Boolean operator !=(Size3D a, Size3D b) => a.NotEquals(b);
}
public readonly partial struct Rectangle2D: Value<Rectangle2D>
{
    public readonly Point2D Center;
    public readonly Size2D Size;
    public Rectangle2D WithCenter(Point2D center) => (center, Size);
    public Rectangle2D WithSize(Size2D size) => (Center, size);
    public Rectangle2D(Point2D center, Size2D size) => (Center, Size) = (center, size);
    public static Rectangle2D Default = new Rectangle2D();
    public static Rectangle2D New(Point2D center, Size2D size) => new Rectangle2D(center, size);
    public static implicit operator (Point2D, Size2D)(Rectangle2D self) => (self.Center, self.Size);
    public static implicit operator Rectangle2D((Point2D, Size2D) value) => new Rectangle2D(value.Item1, value.Item2);
    public void Deconstruct(out Point2D center, out Size2D size) { center = Center; size = Size; }
    public String TypeName => "Rectangle2D";
    public Array<String> FieldNames => (Array1<String>)new[] { (String)"Center", (String)"Size" };
    public Array<Dynamic> FieldValues => (Array1<Dynamic>)new[] { new Dynamic(Center), new Dynamic(Size) };
    // Unimplemented concept functions
    public Rectangle2D Zero => (Center.Zero, Size.Zero);
    public Rectangle2D One => (Center.One, Size.One);
    public Rectangle2D MinValue => (Center.MinValue, Size.MinValue);
    public Rectangle2D MaxValue => (Center.MaxValue, Size.MaxValue);
    public Boolean Equals(Rectangle2D b) => (Center.Equals(b.Center) & Size.Equals(b.Size));
    public static Boolean operator ==(Rectangle2D a, Rectangle2D b) => a.Equals(b);
    public Boolean NotEquals(Rectangle2D b) => (Center.NotEquals(b.Center) & Size.NotEquals(b.Size));
    public static Boolean operator !=(Rectangle2D a, Rectangle2D b) => a.NotEquals(b);
}
public readonly partial struct Proportion: Numerical<Proportion>
{
    public readonly Number Value;
    public Proportion WithValue(Number value) => (value);
    public Proportion(Number value) => (Value) = (value);
    public static Proportion Default = new Proportion();
    public static Proportion New(Number value) => new Proportion(value);
    public static implicit operator Number(Proportion self) => self.Value;
    public static implicit operator Proportion(Number value) => new Proportion(value);
    public String TypeName => "Proportion";
    public Array<String> FieldNames => (Array1<String>)new[] { (String)"Value" };
    public Array<Dynamic> FieldValues => (Array1<Dynamic>)new[] { new Dynamic(Value) };
    // Unimplemented concept functions
    public Number Unlerp(Proportion a, Proportion b) => throw new NotImplementedException();
    public Number Magnitude => throw new NotImplementedException();
    public Integer Compare(Proportion y) => throw new NotImplementedException();
    public Proportion Reciprocal => (Value.Reciprocal);
    public Proportion Negative => (Value.Negative);
    public static Proportion operator -(Proportion self) => self.Negative;
    public Proportion Multiply(Proportion other) => (Value.Multiply(other.Value));
    public static Proportion operator *(Proportion self, Proportion other) => self.Multiply(other);
    public Proportion Divide(Proportion other) => (Value.Divide(other.Value));
    public static Proportion operator /(Proportion self, Proportion other) => self.Divide(other);
    public Proportion Modulo(Proportion other) => (Value.Modulo(other.Value));
    public static Proportion operator %(Proportion self, Proportion other) => self.Modulo(other);
    public Proportion Add(Proportion other) => (Value.Add(other.Value));
    public static Proportion operator +(Proportion self, Proportion other) => self.Add(other);
    public Proportion Subtract(Proportion other) => (Value.Subtract(other.Value));
    public static Proportion operator -(Proportion self, Proportion other) => self.Subtract(other);
    public Proportion Zero => (Value.Zero);
    public Proportion One => (Value.One);
    public Proportion MinValue => (Value.MinValue);
    public Proportion MaxValue => (Value.MaxValue);
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
    public static implicit operator (Number, Number)(Fraction self) => (self.Numerator, self.Denominator);
    public static implicit operator Fraction((Number, Number) value) => new Fraction(value.Item1, value.Item2);
    public void Deconstruct(out Number numerator, out Number denominator) { numerator = Numerator; denominator = Denominator; }
    public String TypeName => "Fraction";
    public Array<String> FieldNames => (Array1<String>)new[] { (String)"Numerator", (String)"Denominator" };
    public Array<Dynamic> FieldValues => (Array1<Dynamic>)new[] { new Dynamic(Numerator), new Dynamic(Denominator) };
    // Unimplemented concept functions
    public Fraction Zero => (Numerator.Zero, Denominator.Zero);
    public Fraction One => (Numerator.One, Denominator.One);
    public Fraction MinValue => (Numerator.MinValue, Denominator.MinValue);
    public Fraction MaxValue => (Numerator.MaxValue, Denominator.MaxValue);
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
    public static implicit operator Number(Angle self) => self.Radians;
    public static implicit operator Angle(Number value) => new Angle(value);
    public String TypeName => "Angle";
    public Array<String> FieldNames => (Array1<String>)new[] { (String)"Radians" };
    public Array<Dynamic> FieldValues => (Array1<Dynamic>)new[] { new Dynamic(Radians) };
    // Unimplemented concept functions
    public Number Value => throw new NotImplementedException();
    public Angle Lerp(Angle b, Number amount) => throw new NotImplementedException();
    public Number Unlerp(Angle a, Angle b) => throw new NotImplementedException();
    public Number Magnitude => throw new NotImplementedException();
    public Integer Compare(Angle y) => throw new NotImplementedException();
    public Angle Multiply(Number other) => throw new NotImplementedException();
    public static Angle operator *(Angle self, Number other) => self.Multiply(other);
    public Angle Divide(Number other) => throw new NotImplementedException();
    public static Angle operator /(Angle self, Number other) => self.Divide(other);
    public Angle Modulo(Number other) => throw new NotImplementedException();
    public static Angle operator %(Angle self, Number other) => self.Modulo(other);
    public Angle Add(Number other) => throw new NotImplementedException();
    public static Angle operator +(Angle self, Number other) => self.Add(other);
    public Angle Subtract(Number other) => throw new NotImplementedException();
    public static Angle operator -(Angle self, Number other) => self.Subtract(other);
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
    public static implicit operator Number(Length self) => self.Meters;
    public static implicit operator Length(Number value) => new Length(value);
    public String TypeName => "Length";
    public Array<String> FieldNames => (Array1<String>)new[] { (String)"Meters" };
    public Array<Dynamic> FieldValues => (Array1<Dynamic>)new[] { new Dynamic(Meters) };
    // Unimplemented concept functions
    public Number Value => throw new NotImplementedException();
    public Length Lerp(Length b, Number amount) => throw new NotImplementedException();
    public Number Unlerp(Length a, Length b) => throw new NotImplementedException();
    public Number Magnitude => throw new NotImplementedException();
    public Integer Compare(Length y) => throw new NotImplementedException();
    public Length Multiply(Number other) => throw new NotImplementedException();
    public static Length operator *(Length self, Number other) => self.Multiply(other);
    public Length Divide(Number other) => throw new NotImplementedException();
    public static Length operator /(Length self, Number other) => self.Divide(other);
    public Length Modulo(Number other) => throw new NotImplementedException();
    public static Length operator %(Length self, Number other) => self.Modulo(other);
    public Length Add(Number other) => throw new NotImplementedException();
    public static Length operator +(Length self, Number other) => self.Add(other);
    public Length Subtract(Number other) => throw new NotImplementedException();
    public static Length operator -(Length self, Number other) => self.Subtract(other);
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
    public static implicit operator Number(Mass self) => self.Kilograms;
    public static implicit operator Mass(Number value) => new Mass(value);
    public String TypeName => "Mass";
    public Array<String> FieldNames => (Array1<String>)new[] { (String)"Kilograms" };
    public Array<Dynamic> FieldValues => (Array1<Dynamic>)new[] { new Dynamic(Kilograms) };
    // Unimplemented concept functions
    public Number Value => throw new NotImplementedException();
    public Mass Lerp(Mass b, Number amount) => throw new NotImplementedException();
    public Number Unlerp(Mass a, Mass b) => throw new NotImplementedException();
    public Number Magnitude => throw new NotImplementedException();
    public Integer Compare(Mass y) => throw new NotImplementedException();
    public Mass Multiply(Number other) => throw new NotImplementedException();
    public static Mass operator *(Mass self, Number other) => self.Multiply(other);
    public Mass Divide(Number other) => throw new NotImplementedException();
    public static Mass operator /(Mass self, Number other) => self.Divide(other);
    public Mass Modulo(Number other) => throw new NotImplementedException();
    public static Mass operator %(Mass self, Number other) => self.Modulo(other);
    public Mass Add(Number other) => throw new NotImplementedException();
    public static Mass operator +(Mass self, Number other) => self.Add(other);
    public Mass Subtract(Number other) => throw new NotImplementedException();
    public static Mass operator -(Mass self, Number other) => self.Subtract(other);
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
    public static implicit operator Number(Temperature self) => self.Celsius;
    public static implicit operator Temperature(Number value) => new Temperature(value);
    public String TypeName => "Temperature";
    public Array<String> FieldNames => (Array1<String>)new[] { (String)"Celsius" };
    public Array<Dynamic> FieldValues => (Array1<Dynamic>)new[] { new Dynamic(Celsius) };
    // Unimplemented concept functions
    public Number Value => throw new NotImplementedException();
    public Temperature Lerp(Temperature b, Number amount) => throw new NotImplementedException();
    public Number Unlerp(Temperature a, Temperature b) => throw new NotImplementedException();
    public Number Magnitude => throw new NotImplementedException();
    public Integer Compare(Temperature y) => throw new NotImplementedException();
    public Temperature Multiply(Number other) => throw new NotImplementedException();
    public static Temperature operator *(Temperature self, Number other) => self.Multiply(other);
    public Temperature Divide(Number other) => throw new NotImplementedException();
    public static Temperature operator /(Temperature self, Number other) => self.Divide(other);
    public Temperature Modulo(Number other) => throw new NotImplementedException();
    public static Temperature operator %(Temperature self, Number other) => self.Modulo(other);
    public Temperature Add(Number other) => throw new NotImplementedException();
    public static Temperature operator +(Temperature self, Number other) => self.Add(other);
    public Temperature Subtract(Number other) => throw new NotImplementedException();
    public static Temperature operator -(Temperature self, Number other) => self.Subtract(other);
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
    public static implicit operator Number(Time self) => self.Seconds;
    public static implicit operator Time(Number value) => new Time(value);
    public String TypeName => "Time";
    public Array<String> FieldNames => (Array1<String>)new[] { (String)"Seconds" };
    public Array<Dynamic> FieldValues => (Array1<Dynamic>)new[] { new Dynamic(Seconds) };
    // Unimplemented concept functions
    public Number Value => throw new NotImplementedException();
    public Time Lerp(Time b, Number amount) => throw new NotImplementedException();
    public Number Unlerp(Time a, Time b) => throw new NotImplementedException();
    public Number Magnitude => throw new NotImplementedException();
    public Integer Compare(Time y) => throw new NotImplementedException();
    public Time Multiply(Number other) => throw new NotImplementedException();
    public static Time operator *(Time self, Number other) => self.Multiply(other);
    public Time Divide(Number other) => throw new NotImplementedException();
    public static Time operator /(Time self, Number other) => self.Divide(other);
    public Time Modulo(Number other) => throw new NotImplementedException();
    public static Time operator %(Time self, Number other) => self.Modulo(other);
    public Time Add(Number other) => throw new NotImplementedException();
    public static Time operator +(Time self, Number other) => self.Add(other);
    public Time Subtract(Number other) => throw new NotImplementedException();
    public static Time operator -(Time self, Number other) => self.Subtract(other);
    public Time Zero => (Seconds.Zero);
    public Time One => (Seconds.One);
    public Time MinValue => (Seconds.MinValue);
    public Time MaxValue => (Seconds.MaxValue);
}
public readonly partial struct TimeRange: Interval<TimeRange, DateTime, Time>
{
    public readonly DateTime Begin;
    public readonly DateTime End;
    public TimeRange WithBegin(DateTime begin) => (begin, End);
    public TimeRange WithEnd(DateTime end) => (Begin, end);
    public TimeRange(DateTime begin, DateTime end) => (Begin, End) = (begin, end);
    public static TimeRange Default = new TimeRange();
    public static TimeRange New(DateTime begin, DateTime end) => new TimeRange(begin, end);
    public static implicit operator (DateTime, DateTime)(TimeRange self) => (self.Begin, self.End);
    public static implicit operator TimeRange((DateTime, DateTime) value) => new TimeRange(value.Item1, value.Item2);
    public void Deconstruct(out DateTime begin, out DateTime end) { begin = Begin; end = End; }
    public String TypeName => "TimeRange";
    public Array<String> FieldNames => (Array1<String>)new[] { (String)"Begin", (String)"End" };
    public Array<Dynamic> FieldValues => (Array1<Dynamic>)new[] { new Dynamic(Begin), new Dynamic(End) };
    // Unimplemented concept functions
    public DateTime Min => throw new NotImplementedException();
    public DateTime Max => throw new NotImplementedException();
    public Time Size => throw new NotImplementedException();
    public TimeRange Zero => (Begin.Zero, End.Zero);
    public TimeRange One => (Begin.One, End.One);
    public TimeRange MinValue => (Begin.MinValue, End.MinValue);
    public TimeRange MaxValue => (Begin.MaxValue, End.MaxValue);
    public Boolean Equals(TimeRange b) => (Begin.Equals(b.Begin) & End.Equals(b.End));
    public static Boolean operator ==(TimeRange a, TimeRange b) => a.Equals(b);
    public Boolean NotEquals(TimeRange b) => (Begin.NotEquals(b.Begin) & End.NotEquals(b.End));
    public static Boolean operator !=(TimeRange a, TimeRange b) => a.NotEquals(b);
}
public readonly partial struct DateTime: Coordinate<DateTime>, AdditiveArithmetic<DateTime, Time>
{
    public readonly Number Value;
    public DateTime WithValue(Number value) => (value);
    public DateTime(Number value) => (Value) = (value);
    public static DateTime Default = new DateTime();
    public static DateTime New(Number value) => new DateTime(value);
    public static implicit operator Number(DateTime self) => self.Value;
    public static implicit operator DateTime(Number value) => new DateTime(value);
    public String TypeName => "DateTime";
    public Array<String> FieldNames => (Array1<String>)new[] { (String)"Value" };
    public Array<Dynamic> FieldValues => (Array1<Dynamic>)new[] { new Dynamic(Value) };
    // Unimplemented concept functions
    public DateTime Zero => (Value.Zero);
    public DateTime One => (Value.One);
    public DateTime MinValue => (Value.MinValue);
    public DateTime MaxValue => (Value.MaxValue);
    public Integer Compare(DateTime y) => throw new NotImplementedException();
    public DateTime Lerp(DateTime b, Number amount) => throw new NotImplementedException();
    public Number Unlerp(DateTime a, DateTime b) => throw new NotImplementedException();
    public DateTime Add(Time other) => throw new NotImplementedException();
    public static DateTime operator +(DateTime self, Time other) => self.Add(other);
    public DateTime Subtract(Time other) => throw new NotImplementedException();
    public static DateTime operator -(DateTime self, Time other) => self.Subtract(other);
}
public readonly partial struct AnglePair: Interval<AnglePair, Angle, Angle>
{
    public readonly Angle Start;
    public readonly Angle End;
    public AnglePair WithStart(Angle start) => (start, End);
    public AnglePair WithEnd(Angle end) => (Start, end);
    public AnglePair(Angle start, Angle end) => (Start, End) = (start, end);
    public static AnglePair Default = new AnglePair();
    public static AnglePair New(Angle start, Angle end) => new AnglePair(start, end);
    public static implicit operator (Angle, Angle)(AnglePair self) => (self.Start, self.End);
    public static implicit operator AnglePair((Angle, Angle) value) => new AnglePair(value.Item1, value.Item2);
    public void Deconstruct(out Angle start, out Angle end) { start = Start; end = End; }
    public String TypeName => "AnglePair";
    public Array<String> FieldNames => (Array1<String>)new[] { (String)"Start", (String)"End" };
    public Array<Dynamic> FieldValues => (Array1<Dynamic>)new[] { new Dynamic(Start), new Dynamic(End) };
    // Unimplemented concept functions
    public Angle Min => throw new NotImplementedException();
    public Angle Max => throw new NotImplementedException();
    public Angle Size => throw new NotImplementedException();
    public AnglePair Zero => (Start.Zero, End.Zero);
    public AnglePair One => (Start.One, End.One);
    public AnglePair MinValue => (Start.MinValue, End.MinValue);
    public AnglePair MaxValue => (Start.MaxValue, End.MaxValue);
    public Boolean Equals(AnglePair b) => (Start.Equals(b.Start) & End.Equals(b.End));
    public static Boolean operator ==(AnglePair a, AnglePair b) => a.Equals(b);
    public Boolean NotEquals(AnglePair b) => (Start.NotEquals(b.Start) & End.NotEquals(b.End));
    public static Boolean operator !=(AnglePair a, AnglePair b) => a.NotEquals(b);
}
public readonly partial struct Ring: Value<Ring>
{
    public readonly Circle Circle;
    public readonly Number InnerRadius;
    public Ring WithCircle(Circle circle) => (circle, InnerRadius);
    public Ring WithInnerRadius(Number innerRadius) => (Circle, innerRadius);
    public Ring(Circle circle, Number innerRadius) => (Circle, InnerRadius) = (circle, innerRadius);
    public static Ring Default = new Ring();
    public static Ring New(Circle circle, Number innerRadius) => new Ring(circle, innerRadius);
    public static implicit operator (Circle, Number)(Ring self) => (self.Circle, self.InnerRadius);
    public static implicit operator Ring((Circle, Number) value) => new Ring(value.Item1, value.Item2);
    public void Deconstruct(out Circle circle, out Number innerRadius) { circle = Circle; innerRadius = InnerRadius; }
    public String TypeName => "Ring";
    public Array<String> FieldNames => (Array1<String>)new[] { (String)"Circle", (String)"InnerRadius" };
    public Array<Dynamic> FieldValues => (Array1<Dynamic>)new[] { new Dynamic(Circle), new Dynamic(InnerRadius) };
    // Unimplemented concept functions
    public Ring Zero => (Circle.Zero, InnerRadius.Zero);
    public Ring One => (Circle.One, InnerRadius.One);
    public Ring MinValue => (Circle.MinValue, InnerRadius.MinValue);
    public Ring MaxValue => (Circle.MaxValue, InnerRadius.MaxValue);
    public Boolean Equals(Ring b) => (Circle.Equals(b.Circle) & InnerRadius.Equals(b.InnerRadius));
    public static Boolean operator ==(Ring a, Ring b) => a.Equals(b);
    public Boolean NotEquals(Ring b) => (Circle.NotEquals(b.Circle) & InnerRadius.NotEquals(b.InnerRadius));
    public static Boolean operator !=(Ring a, Ring b) => a.NotEquals(b);
}
public readonly partial struct Arc: Value<Arc>
{
    public readonly AnglePair Angles;
    public readonly Circle Cirlce;
    public Arc WithAngles(AnglePair angles) => (angles, Cirlce);
    public Arc WithCirlce(Circle cirlce) => (Angles, cirlce);
    public Arc(AnglePair angles, Circle cirlce) => (Angles, Cirlce) = (angles, cirlce);
    public static Arc Default = new Arc();
    public static Arc New(AnglePair angles, Circle cirlce) => new Arc(angles, cirlce);
    public static implicit operator (AnglePair, Circle)(Arc self) => (self.Angles, self.Cirlce);
    public static implicit operator Arc((AnglePair, Circle) value) => new Arc(value.Item1, value.Item2);
    public void Deconstruct(out AnglePair angles, out Circle cirlce) { angles = Angles; cirlce = Cirlce; }
    public String TypeName => "Arc";
    public Array<String> FieldNames => (Array1<String>)new[] { (String)"Angles", (String)"Cirlce" };
    public Array<Dynamic> FieldValues => (Array1<Dynamic>)new[] { new Dynamic(Angles), new Dynamic(Cirlce) };
    // Unimplemented concept functions
    public Arc Zero => (Angles.Zero, Cirlce.Zero);
    public Arc One => (Angles.One, Cirlce.One);
    public Arc MinValue => (Angles.MinValue, Cirlce.MinValue);
    public Arc MaxValue => (Angles.MaxValue, Cirlce.MaxValue);
    public Boolean Equals(Arc b) => (Angles.Equals(b.Angles) & Cirlce.Equals(b.Cirlce));
    public static Boolean operator ==(Arc a, Arc b) => a.Equals(b);
    public Boolean NotEquals(Arc b) => (Angles.NotEquals(b.Angles) & Cirlce.NotEquals(b.Cirlce));
    public static Boolean operator !=(Arc a, Arc b) => a.NotEquals(b);
}
public readonly partial struct RealInterval: Interval<RealInterval, Number, Number>
{
    public readonly Number A;
    public readonly Number B;
    public RealInterval WithA(Number a) => (a, B);
    public RealInterval WithB(Number b) => (A, b);
    public RealInterval(Number a, Number b) => (A, B) = (a, b);
    public static RealInterval Default = new RealInterval();
    public static RealInterval New(Number a, Number b) => new RealInterval(a, b);
    public static implicit operator (Number, Number)(RealInterval self) => (self.A, self.B);
    public static implicit operator RealInterval((Number, Number) value) => new RealInterval(value.Item1, value.Item2);
    public void Deconstruct(out Number a, out Number b) { a = A; b = B; }
    public String TypeName => "RealInterval";
    public Array<String> FieldNames => (Array1<String>)new[] { (String)"A", (String)"B" };
    public Array<Dynamic> FieldValues => (Array1<Dynamic>)new[] { new Dynamic(A), new Dynamic(B) };
    // Unimplemented concept functions
    public Number Min => throw new NotImplementedException();
    public Number Max => throw new NotImplementedException();
    public Number Size => throw new NotImplementedException();
    public RealInterval Zero => (A.Zero, B.Zero);
    public RealInterval One => (A.One, B.One);
    public RealInterval MinValue => (A.MinValue, B.MinValue);
    public RealInterval MaxValue => (A.MaxValue, B.MaxValue);
    public Boolean Equals(RealInterval b) => (A.Equals(b.A) & B.Equals(b.B));
    public static Boolean operator ==(RealInterval a, RealInterval b) => a.Equals(b);
    public Boolean NotEquals(RealInterval b) => (A.NotEquals(b.A) & B.NotEquals(b.B));
    public static Boolean operator !=(RealInterval a, RealInterval b) => a.NotEquals(b);
}
public readonly partial struct Capsule: Value<Capsule>
{
    public readonly Line3D Line;
    public readonly Number Radius;
    public Capsule WithLine(Line3D line) => (line, Radius);
    public Capsule WithRadius(Number radius) => (Line, radius);
    public Capsule(Line3D line, Number radius) => (Line, Radius) = (line, radius);
    public static Capsule Default = new Capsule();
    public static Capsule New(Line3D line, Number radius) => new Capsule(line, radius);
    public static implicit operator (Line3D, Number)(Capsule self) => (self.Line, self.Radius);
    public static implicit operator Capsule((Line3D, Number) value) => new Capsule(value.Item1, value.Item2);
    public void Deconstruct(out Line3D line, out Number radius) { line = Line; radius = Radius; }
    public String TypeName => "Capsule";
    public Array<String> FieldNames => (Array1<String>)new[] { (String)"Line", (String)"Radius" };
    public Array<Dynamic> FieldValues => (Array1<Dynamic>)new[] { new Dynamic(Line), new Dynamic(Radius) };
    // Unimplemented concept functions
    public Capsule Zero => (Line.Zero, Radius.Zero);
    public Capsule One => (Line.One, Radius.One);
    public Capsule MinValue => (Line.MinValue, Radius.MinValue);
    public Capsule MaxValue => (Line.MaxValue, Radius.MaxValue);
    public Boolean Equals(Capsule b) => (Line.Equals(b.Line) & Radius.Equals(b.Radius));
    public static Boolean operator ==(Capsule a, Capsule b) => a.Equals(b);
    public Boolean NotEquals(Capsule b) => (Line.NotEquals(b.Line) & Radius.NotEquals(b.Radius));
    public static Boolean operator !=(Capsule a, Capsule b) => a.NotEquals(b);
}
public readonly partial struct Matrix3D: Value<Matrix3D>
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
    public static implicit operator (Vector4D, Vector4D, Vector4D, Vector4D)(Matrix3D self) => (self.Column1, self.Column2, self.Column3, self.Column4);
    public static implicit operator Matrix3D((Vector4D, Vector4D, Vector4D, Vector4D) value) => new Matrix3D(value.Item1, value.Item2, value.Item3, value.Item4);
    public void Deconstruct(out Vector4D column1, out Vector4D column2, out Vector4D column3, out Vector4D column4) { column1 = Column1; column2 = Column2; column3 = Column3; column4 = Column4; }
    public String TypeName => "Matrix3D";
    public Array<String> FieldNames => (Array1<String>)new[] { (String)"Column1", (String)"Column2", (String)"Column3", (String)"Column4" };
    public Array<Dynamic> FieldValues => (Array1<Dynamic>)new[] { new Dynamic(Column1), new Dynamic(Column2), new Dynamic(Column3), new Dynamic(Column4) };
    // Unimplemented concept functions
    public Matrix3D Zero => (Column1.Zero, Column2.Zero, Column3.Zero, Column4.Zero);
    public Matrix3D One => (Column1.One, Column2.One, Column3.One, Column4.One);
    public Matrix3D MinValue => (Column1.MinValue, Column2.MinValue, Column3.MinValue, Column4.MinValue);
    public Matrix3D MaxValue => (Column1.MaxValue, Column2.MaxValue, Column3.MaxValue, Column4.MaxValue);
    public Boolean Equals(Matrix3D b) => (Column1.Equals(b.Column1) & Column2.Equals(b.Column2) & Column3.Equals(b.Column3) & Column4.Equals(b.Column4));
    public static Boolean operator ==(Matrix3D a, Matrix3D b) => a.Equals(b);
    public Boolean NotEquals(Matrix3D b) => (Column1.NotEquals(b.Column1) & Column2.NotEquals(b.Column2) & Column3.NotEquals(b.Column3) & Column4.NotEquals(b.Column4));
    public static Boolean operator !=(Matrix3D a, Matrix3D b) => a.NotEquals(b);
}
public readonly partial struct Cylinder: Value<Cylinder>
{
    public readonly Line3D Line;
    public readonly Number Radius;
    public Cylinder WithLine(Line3D line) => (line, Radius);
    public Cylinder WithRadius(Number radius) => (Line, radius);
    public Cylinder(Line3D line, Number radius) => (Line, Radius) = (line, radius);
    public static Cylinder Default = new Cylinder();
    public static Cylinder New(Line3D line, Number radius) => new Cylinder(line, radius);
    public static implicit operator (Line3D, Number)(Cylinder self) => (self.Line, self.Radius);
    public static implicit operator Cylinder((Line3D, Number) value) => new Cylinder(value.Item1, value.Item2);
    public void Deconstruct(out Line3D line, out Number radius) { line = Line; radius = Radius; }
    public String TypeName => "Cylinder";
    public Array<String> FieldNames => (Array1<String>)new[] { (String)"Line", (String)"Radius" };
    public Array<Dynamic> FieldValues => (Array1<Dynamic>)new[] { new Dynamic(Line), new Dynamic(Radius) };
    // Unimplemented concept functions
    public Cylinder Zero => (Line.Zero, Radius.Zero);
    public Cylinder One => (Line.One, Radius.One);
    public Cylinder MinValue => (Line.MinValue, Radius.MinValue);
    public Cylinder MaxValue => (Line.MaxValue, Radius.MaxValue);
    public Boolean Equals(Cylinder b) => (Line.Equals(b.Line) & Radius.Equals(b.Radius));
    public static Boolean operator ==(Cylinder a, Cylinder b) => a.Equals(b);
    public Boolean NotEquals(Cylinder b) => (Line.NotEquals(b.Line) & Radius.NotEquals(b.Radius));
    public static Boolean operator !=(Cylinder a, Cylinder b) => a.NotEquals(b);
}
public readonly partial struct Cone: Value<Cone>
{
    public readonly Line3D Line;
    public readonly Number Radius;
    public Cone WithLine(Line3D line) => (line, Radius);
    public Cone WithRadius(Number radius) => (Line, radius);
    public Cone(Line3D line, Number radius) => (Line, Radius) = (line, radius);
    public static Cone Default = new Cone();
    public static Cone New(Line3D line, Number radius) => new Cone(line, radius);
    public static implicit operator (Line3D, Number)(Cone self) => (self.Line, self.Radius);
    public static implicit operator Cone((Line3D, Number) value) => new Cone(value.Item1, value.Item2);
    public void Deconstruct(out Line3D line, out Number radius) { line = Line; radius = Radius; }
    public String TypeName => "Cone";
    public Array<String> FieldNames => (Array1<String>)new[] { (String)"Line", (String)"Radius" };
    public Array<Dynamic> FieldValues => (Array1<Dynamic>)new[] { new Dynamic(Line), new Dynamic(Radius) };
    // Unimplemented concept functions
    public Cone Zero => (Line.Zero, Radius.Zero);
    public Cone One => (Line.One, Radius.One);
    public Cone MinValue => (Line.MinValue, Radius.MinValue);
    public Cone MaxValue => (Line.MaxValue, Radius.MaxValue);
    public Boolean Equals(Cone b) => (Line.Equals(b.Line) & Radius.Equals(b.Radius));
    public static Boolean operator ==(Cone a, Cone b) => a.Equals(b);
    public Boolean NotEquals(Cone b) => (Line.NotEquals(b.Line) & Radius.NotEquals(b.Radius));
    public static Boolean operator !=(Cone a, Cone b) => a.NotEquals(b);
}
public readonly partial struct Tube: Value<Tube>
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
    public static implicit operator (Line3D, Number, Number)(Tube self) => (self.Line, self.InnerRadius, self.OuterRadius);
    public static implicit operator Tube((Line3D, Number, Number) value) => new Tube(value.Item1, value.Item2, value.Item3);
    public void Deconstruct(out Line3D line, out Number innerRadius, out Number outerRadius) { line = Line; innerRadius = InnerRadius; outerRadius = OuterRadius; }
    public String TypeName => "Tube";
    public Array<String> FieldNames => (Array1<String>)new[] { (String)"Line", (String)"InnerRadius", (String)"OuterRadius" };
    public Array<Dynamic> FieldValues => (Array1<Dynamic>)new[] { new Dynamic(Line), new Dynamic(InnerRadius), new Dynamic(OuterRadius) };
    // Unimplemented concept functions
    public Tube Zero => (Line.Zero, InnerRadius.Zero, OuterRadius.Zero);
    public Tube One => (Line.One, InnerRadius.One, OuterRadius.One);
    public Tube MinValue => (Line.MinValue, InnerRadius.MinValue, OuterRadius.MinValue);
    public Tube MaxValue => (Line.MaxValue, InnerRadius.MaxValue, OuterRadius.MaxValue);
    public Boolean Equals(Tube b) => (Line.Equals(b.Line) & InnerRadius.Equals(b.InnerRadius) & OuterRadius.Equals(b.OuterRadius));
    public static Boolean operator ==(Tube a, Tube b) => a.Equals(b);
    public Boolean NotEquals(Tube b) => (Line.NotEquals(b.Line) & InnerRadius.NotEquals(b.InnerRadius) & OuterRadius.NotEquals(b.OuterRadius));
    public static Boolean operator !=(Tube a, Tube b) => a.NotEquals(b);
}
public readonly partial struct ConeSegment: Value<ConeSegment>
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
    public static implicit operator (Line3D, Number, Number)(ConeSegment self) => (self.Line, self.Radius1, self.Radius2);
    public static implicit operator ConeSegment((Line3D, Number, Number) value) => new ConeSegment(value.Item1, value.Item2, value.Item3);
    public void Deconstruct(out Line3D line, out Number radius1, out Number radius2) { line = Line; radius1 = Radius1; radius2 = Radius2; }
    public String TypeName => "ConeSegment";
    public Array<String> FieldNames => (Array1<String>)new[] { (String)"Line", (String)"Radius1", (String)"Radius2" };
    public Array<Dynamic> FieldValues => (Array1<Dynamic>)new[] { new Dynamic(Line), new Dynamic(Radius1), new Dynamic(Radius2) };
    // Unimplemented concept functions
    public ConeSegment Zero => (Line.Zero, Radius1.Zero, Radius2.Zero);
    public ConeSegment One => (Line.One, Radius1.One, Radius2.One);
    public ConeSegment MinValue => (Line.MinValue, Radius1.MinValue, Radius2.MinValue);
    public ConeSegment MaxValue => (Line.MaxValue, Radius1.MaxValue, Radius2.MaxValue);
    public Boolean Equals(ConeSegment b) => (Line.Equals(b.Line) & Radius1.Equals(b.Radius1) & Radius2.Equals(b.Radius2));
    public static Boolean operator ==(ConeSegment a, ConeSegment b) => a.Equals(b);
    public Boolean NotEquals(ConeSegment b) => (Line.NotEquals(b.Line) & Radius1.NotEquals(b.Radius1) & Radius2.NotEquals(b.Radius2));
    public static Boolean operator !=(ConeSegment a, ConeSegment b) => a.NotEquals(b);
}
public readonly partial struct Box2D: Value<Box2D>
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
    public static implicit operator (Point2D, Angle, Size2D)(Box2D self) => (self.Center, self.Rotation, self.Extent);
    public static implicit operator Box2D((Point2D, Angle, Size2D) value) => new Box2D(value.Item1, value.Item2, value.Item3);
    public void Deconstruct(out Point2D center, out Angle rotation, out Size2D extent) { center = Center; rotation = Rotation; extent = Extent; }
    public String TypeName => "Box2D";
    public Array<String> FieldNames => (Array1<String>)new[] { (String)"Center", (String)"Rotation", (String)"Extent" };
    public Array<Dynamic> FieldValues => (Array1<Dynamic>)new[] { new Dynamic(Center), new Dynamic(Rotation), new Dynamic(Extent) };
    // Unimplemented concept functions
    public Box2D Zero => (Center.Zero, Rotation.Zero, Extent.Zero);
    public Box2D One => (Center.One, Rotation.One, Extent.One);
    public Box2D MinValue => (Center.MinValue, Rotation.MinValue, Extent.MinValue);
    public Box2D MaxValue => (Center.MaxValue, Rotation.MaxValue, Extent.MaxValue);
    public Boolean Equals(Box2D b) => (Center.Equals(b.Center) & Rotation.Equals(b.Rotation) & Extent.Equals(b.Extent));
    public static Boolean operator ==(Box2D a, Box2D b) => a.Equals(b);
    public Boolean NotEquals(Box2D b) => (Center.NotEquals(b.Center) & Rotation.NotEquals(b.Rotation) & Extent.NotEquals(b.Extent));
    public static Boolean operator !=(Box2D a, Box2D b) => a.NotEquals(b);
}
public readonly partial struct Box3D: Value<Box3D>
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
    public static implicit operator (Point3D, Rotation3D, Size3D)(Box3D self) => (self.Center, self.Rotation, self.Extent);
    public static implicit operator Box3D((Point3D, Rotation3D, Size3D) value) => new Box3D(value.Item1, value.Item2, value.Item3);
    public void Deconstruct(out Point3D center, out Rotation3D rotation, out Size3D extent) { center = Center; rotation = Rotation; extent = Extent; }
    public String TypeName => "Box3D";
    public Array<String> FieldNames => (Array1<String>)new[] { (String)"Center", (String)"Rotation", (String)"Extent" };
    public Array<Dynamic> FieldValues => (Array1<Dynamic>)new[] { new Dynamic(Center), new Dynamic(Rotation), new Dynamic(Extent) };
    // Unimplemented concept functions
    public Box3D Zero => (Center.Zero, Rotation.Zero, Extent.Zero);
    public Box3D One => (Center.One, Rotation.One, Extent.One);
    public Box3D MinValue => (Center.MinValue, Rotation.MinValue, Extent.MinValue);
    public Box3D MaxValue => (Center.MaxValue, Rotation.MaxValue, Extent.MaxValue);
    public Boolean Equals(Box3D b) => (Center.Equals(b.Center) & Rotation.Equals(b.Rotation) & Extent.Equals(b.Extent));
    public static Boolean operator ==(Box3D a, Box3D b) => a.Equals(b);
    public Boolean NotEquals(Box3D b) => (Center.NotEquals(b.Center) & Rotation.NotEquals(b.Rotation) & Extent.NotEquals(b.Extent));
    public static Boolean operator !=(Box3D a, Box3D b) => a.NotEquals(b);
}
public readonly partial struct UV: Vector<UV>
{
    public readonly Unit U;
    public readonly Unit V;
    public UV WithU(Unit u) => (u, V);
    public UV WithV(Unit v) => (U, v);
    public UV(Unit u, Unit v) => (U, V) = (u, v);
    public static UV Default = new UV();
    public static UV New(Unit u, Unit v) => new UV(u, v);
    public static implicit operator (Unit, Unit)(UV self) => (self.U, self.V);
    public static implicit operator UV((Unit, Unit) value) => new UV(value.Item1, value.Item2);
    public void Deconstruct(out Unit u, out Unit v) { u = U; v = V; }
    public String TypeName => "UV";
    public Array<String> FieldNames => (Array1<String>)new[] { (String)"U", (String)"V" };
    public Array<Dynamic> FieldValues => (Array1<Dynamic>)new[] { new Dynamic(U), new Dynamic(V) };
    // Unimplemented concept functions
    public UV Multiply(Number other) => throw new NotImplementedException();
    public static UV operator *(UV self, Number other) => self.Multiply(other);
    public UV Divide(Number other) => throw new NotImplementedException();
    public static UV operator /(UV self, Number other) => self.Divide(other);
    public UV Modulo(Number other) => throw new NotImplementedException();
    public static UV operator %(UV self, Number other) => self.Modulo(other);
    public UV Add(Number other) => throw new NotImplementedException();
    public static UV operator +(UV self, Number other) => self.Add(other);
    public UV Subtract(Number other) => throw new NotImplementedException();
    public static UV operator -(UV self, Number other) => self.Subtract(other);
    public Number Unlerp(UV a, UV b) => throw new NotImplementedException();
    public Integer Compare(UV y) => throw new NotImplementedException();
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
    public Integer Count => throw new NotImplementedException();
    public Number At(Integer n) => throw new NotImplementedException();
    public Number this[Integer n] => At(n);
}
public readonly partial struct UVW: Vector<UVW>
{
    public readonly Unit U;
    public readonly Unit V;
    public readonly Unit W;
    public UVW WithU(Unit u) => (u, V, W);
    public UVW WithV(Unit v) => (U, v, W);
    public UVW WithW(Unit w) => (U, V, w);
    public UVW(Unit u, Unit v, Unit w) => (U, V, W) = (u, v, w);
    public static UVW Default = new UVW();
    public static UVW New(Unit u, Unit v, Unit w) => new UVW(u, v, w);
    public static implicit operator (Unit, Unit, Unit)(UVW self) => (self.U, self.V, self.W);
    public static implicit operator UVW((Unit, Unit, Unit) value) => new UVW(value.Item1, value.Item2, value.Item3);
    public void Deconstruct(out Unit u, out Unit v, out Unit w) { u = U; v = V; w = W; }
    public String TypeName => "UVW";
    public Array<String> FieldNames => (Array1<String>)new[] { (String)"U", (String)"V", (String)"W" };
    public Array<Dynamic> FieldValues => (Array1<Dynamic>)new[] { new Dynamic(U), new Dynamic(V), new Dynamic(W) };
    // Unimplemented concept functions
    public UVW Multiply(Number other) => throw new NotImplementedException();
    public static UVW operator *(UVW self, Number other) => self.Multiply(other);
    public UVW Divide(Number other) => throw new NotImplementedException();
    public static UVW operator /(UVW self, Number other) => self.Divide(other);
    public UVW Modulo(Number other) => throw new NotImplementedException();
    public static UVW operator %(UVW self, Number other) => self.Modulo(other);
    public UVW Add(Number other) => throw new NotImplementedException();
    public static UVW operator +(UVW self, Number other) => self.Add(other);
    public UVW Subtract(Number other) => throw new NotImplementedException();
    public static UVW operator -(UVW self, Number other) => self.Subtract(other);
    public Number Unlerp(UVW a, UVW b) => throw new NotImplementedException();
    public Integer Compare(UVW y) => throw new NotImplementedException();
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
    public Integer Count => throw new NotImplementedException();
    public Number At(Integer n) => throw new NotImplementedException();
    public Number this[Integer n] => At(n);
}
public readonly partial struct CubicBezier2D: Value<CubicBezier2D>
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
    public static implicit operator (Point2D, Point2D, Point2D, Point2D)(CubicBezier2D self) => (self.A, self.B, self.C, self.D);
    public static implicit operator CubicBezier2D((Point2D, Point2D, Point2D, Point2D) value) => new CubicBezier2D(value.Item1, value.Item2, value.Item3, value.Item4);
    public void Deconstruct(out Point2D a, out Point2D b, out Point2D c, out Point2D d) { a = A; b = B; c = C; d = D; }
    public String TypeName => "CubicBezier2D";
    public Array<String> FieldNames => (Array1<String>)new[] { (String)"A", (String)"B", (String)"C", (String)"D" };
    public Array<Dynamic> FieldValues => (Array1<Dynamic>)new[] { new Dynamic(A), new Dynamic(B), new Dynamic(C), new Dynamic(D) };
    // Unimplemented concept functions
    public CubicBezier2D Zero => (A.Zero, B.Zero, C.Zero, D.Zero);
    public CubicBezier2D One => (A.One, B.One, C.One, D.One);
    public CubicBezier2D MinValue => (A.MinValue, B.MinValue, C.MinValue, D.MinValue);
    public CubicBezier2D MaxValue => (A.MaxValue, B.MaxValue, C.MaxValue, D.MaxValue);
    public Boolean Equals(CubicBezier2D b) => (A.Equals(b.A) & B.Equals(b.B) & C.Equals(b.C) & D.Equals(b.D));
    public static Boolean operator ==(CubicBezier2D a, CubicBezier2D b) => a.Equals(b);
    public Boolean NotEquals(CubicBezier2D b) => (A.NotEquals(b.A) & B.NotEquals(b.B) & C.NotEquals(b.C) & D.NotEquals(b.D));
    public static Boolean operator !=(CubicBezier2D a, CubicBezier2D b) => a.NotEquals(b);
}
public readonly partial struct CubicBezier3D: Value<CubicBezier3D>
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
    public static implicit operator (Point3D, Point3D, Point3D, Point3D)(CubicBezier3D self) => (self.A, self.B, self.C, self.D);
    public static implicit operator CubicBezier3D((Point3D, Point3D, Point3D, Point3D) value) => new CubicBezier3D(value.Item1, value.Item2, value.Item3, value.Item4);
    public void Deconstruct(out Point3D a, out Point3D b, out Point3D c, out Point3D d) { a = A; b = B; c = C; d = D; }
    public String TypeName => "CubicBezier3D";
    public Array<String> FieldNames => (Array1<String>)new[] { (String)"A", (String)"B", (String)"C", (String)"D" };
    public Array<Dynamic> FieldValues => (Array1<Dynamic>)new[] { new Dynamic(A), new Dynamic(B), new Dynamic(C), new Dynamic(D) };
    // Unimplemented concept functions
    public CubicBezier3D Zero => (A.Zero, B.Zero, C.Zero, D.Zero);
    public CubicBezier3D One => (A.One, B.One, C.One, D.One);
    public CubicBezier3D MinValue => (A.MinValue, B.MinValue, C.MinValue, D.MinValue);
    public CubicBezier3D MaxValue => (A.MaxValue, B.MaxValue, C.MaxValue, D.MaxValue);
    public Boolean Equals(CubicBezier3D b) => (A.Equals(b.A) & B.Equals(b.B) & C.Equals(b.C) & D.Equals(b.D));
    public static Boolean operator ==(CubicBezier3D a, CubicBezier3D b) => a.Equals(b);
    public Boolean NotEquals(CubicBezier3D b) => (A.NotEquals(b.A) & B.NotEquals(b.B) & C.NotEquals(b.C) & D.NotEquals(b.D));
    public static Boolean operator !=(CubicBezier3D a, CubicBezier3D b) => a.NotEquals(b);
}
public readonly partial struct QuadraticBezier2D: Value<QuadraticBezier2D>
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
    public static implicit operator (Point2D, Point2D, Point2D)(QuadraticBezier2D self) => (self.A, self.B, self.C);
    public static implicit operator QuadraticBezier2D((Point2D, Point2D, Point2D) value) => new QuadraticBezier2D(value.Item1, value.Item2, value.Item3);
    public void Deconstruct(out Point2D a, out Point2D b, out Point2D c) { a = A; b = B; c = C; }
    public String TypeName => "QuadraticBezier2D";
    public Array<String> FieldNames => (Array1<String>)new[] { (String)"A", (String)"B", (String)"C" };
    public Array<Dynamic> FieldValues => (Array1<Dynamic>)new[] { new Dynamic(A), new Dynamic(B), new Dynamic(C) };
    // Unimplemented concept functions
    public QuadraticBezier2D Zero => (A.Zero, B.Zero, C.Zero);
    public QuadraticBezier2D One => (A.One, B.One, C.One);
    public QuadraticBezier2D MinValue => (A.MinValue, B.MinValue, C.MinValue);
    public QuadraticBezier2D MaxValue => (A.MaxValue, B.MaxValue, C.MaxValue);
    public Boolean Equals(QuadraticBezier2D b) => (A.Equals(b.A) & B.Equals(b.B) & C.Equals(b.C));
    public static Boolean operator ==(QuadraticBezier2D a, QuadraticBezier2D b) => a.Equals(b);
    public Boolean NotEquals(QuadraticBezier2D b) => (A.NotEquals(b.A) & B.NotEquals(b.B) & C.NotEquals(b.C));
    public static Boolean operator !=(QuadraticBezier2D a, QuadraticBezier2D b) => a.NotEquals(b);
}
public readonly partial struct QuadraticBezier3D: Value<QuadraticBezier3D>
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
    public static implicit operator (Point3D, Point3D, Point3D)(QuadraticBezier3D self) => (self.A, self.B, self.C);
    public static implicit operator QuadraticBezier3D((Point3D, Point3D, Point3D) value) => new QuadraticBezier3D(value.Item1, value.Item2, value.Item3);
    public void Deconstruct(out Point3D a, out Point3D b, out Point3D c) { a = A; b = B; c = C; }
    public String TypeName => "QuadraticBezier3D";
    public Array<String> FieldNames => (Array1<String>)new[] { (String)"A", (String)"B", (String)"C" };
    public Array<Dynamic> FieldValues => (Array1<Dynamic>)new[] { new Dynamic(A), new Dynamic(B), new Dynamic(C) };
    // Unimplemented concept functions
    public QuadraticBezier3D Zero => (A.Zero, B.Zero, C.Zero);
    public QuadraticBezier3D One => (A.One, B.One, C.One);
    public QuadraticBezier3D MinValue => (A.MinValue, B.MinValue, C.MinValue);
    public QuadraticBezier3D MaxValue => (A.MaxValue, B.MaxValue, C.MaxValue);
    public Boolean Equals(QuadraticBezier3D b) => (A.Equals(b.A) & B.Equals(b.B) & C.Equals(b.C));
    public static Boolean operator ==(QuadraticBezier3D a, QuadraticBezier3D b) => a.Equals(b);
    public Boolean NotEquals(QuadraticBezier3D b) => (A.NotEquals(b.A) & B.NotEquals(b.B) & C.NotEquals(b.C));
    public static Boolean operator !=(QuadraticBezier3D a, QuadraticBezier3D b) => a.NotEquals(b);
}
public readonly partial struct Area: Measure<Area>
{
    public readonly Number MetersSquared;
    public Area WithMetersSquared(Number metersSquared) => (metersSquared);
    public Area(Number metersSquared) => (MetersSquared) = (metersSquared);
    public static Area Default = new Area();
    public static Area New(Number metersSquared) => new Area(metersSquared);
    public static implicit operator Number(Area self) => self.MetersSquared;
    public static implicit operator Area(Number value) => new Area(value);
    public String TypeName => "Area";
    public Array<String> FieldNames => (Array1<String>)new[] { (String)"MetersSquared" };
    public Array<Dynamic> FieldValues => (Array1<Dynamic>)new[] { new Dynamic(MetersSquared) };
    // Unimplemented concept functions
    public Number Value => throw new NotImplementedException();
    public Area Lerp(Area b, Number amount) => throw new NotImplementedException();
    public Number Unlerp(Area a, Area b) => throw new NotImplementedException();
    public Number Magnitude => throw new NotImplementedException();
    public Integer Compare(Area y) => throw new NotImplementedException();
    public Area Multiply(Number other) => throw new NotImplementedException();
    public static Area operator *(Area self, Number other) => self.Multiply(other);
    public Area Divide(Number other) => throw new NotImplementedException();
    public static Area operator /(Area self, Number other) => self.Divide(other);
    public Area Modulo(Number other) => throw new NotImplementedException();
    public static Area operator %(Area self, Number other) => self.Modulo(other);
    public Area Add(Number other) => throw new NotImplementedException();
    public static Area operator +(Area self, Number other) => self.Add(other);
    public Area Subtract(Number other) => throw new NotImplementedException();
    public static Area operator -(Area self, Number other) => self.Subtract(other);
    public Area Zero => (MetersSquared.Zero);
    public Area One => (MetersSquared.One);
    public Area MinValue => (MetersSquared.MinValue);
    public Area MaxValue => (MetersSquared.MaxValue);
}
public readonly partial struct Volume: Measure<Volume>
{
    public readonly Number MetersCubed;
    public Volume WithMetersCubed(Number metersCubed) => (metersCubed);
    public Volume(Number metersCubed) => (MetersCubed) = (metersCubed);
    public static Volume Default = new Volume();
    public static Volume New(Number metersCubed) => new Volume(metersCubed);
    public static implicit operator Number(Volume self) => self.MetersCubed;
    public static implicit operator Volume(Number value) => new Volume(value);
    public String TypeName => "Volume";
    public Array<String> FieldNames => (Array1<String>)new[] { (String)"MetersCubed" };
    public Array<Dynamic> FieldValues => (Array1<Dynamic>)new[] { new Dynamic(MetersCubed) };
    // Unimplemented concept functions
    public Number Value => throw new NotImplementedException();
    public Volume Lerp(Volume b, Number amount) => throw new NotImplementedException();
    public Number Unlerp(Volume a, Volume b) => throw new NotImplementedException();
    public Number Magnitude => throw new NotImplementedException();
    public Integer Compare(Volume y) => throw new NotImplementedException();
    public Volume Multiply(Number other) => throw new NotImplementedException();
    public static Volume operator *(Volume self, Number other) => self.Multiply(other);
    public Volume Divide(Number other) => throw new NotImplementedException();
    public static Volume operator /(Volume self, Number other) => self.Divide(other);
    public Volume Modulo(Number other) => throw new NotImplementedException();
    public static Volume operator %(Volume self, Number other) => self.Modulo(other);
    public Volume Add(Number other) => throw new NotImplementedException();
    public static Volume operator +(Volume self, Number other) => self.Add(other);
    public Volume Subtract(Number other) => throw new NotImplementedException();
    public static Volume operator -(Volume self, Number other) => self.Subtract(other);
    public Volume Zero => (MetersCubed.Zero);
    public Volume One => (MetersCubed.One);
    public Volume MinValue => (MetersCubed.MinValue);
    public Volume MaxValue => (MetersCubed.MaxValue);
}
public readonly partial struct Velocity: Measure<Velocity>
{
    public readonly Number MetersPerSecond;
    public Velocity WithMetersPerSecond(Number metersPerSecond) => (metersPerSecond);
    public Velocity(Number metersPerSecond) => (MetersPerSecond) = (metersPerSecond);
    public static Velocity Default = new Velocity();
    public static Velocity New(Number metersPerSecond) => new Velocity(metersPerSecond);
    public static implicit operator Number(Velocity self) => self.MetersPerSecond;
    public static implicit operator Velocity(Number value) => new Velocity(value);
    public String TypeName => "Velocity";
    public Array<String> FieldNames => (Array1<String>)new[] { (String)"MetersPerSecond" };
    public Array<Dynamic> FieldValues => (Array1<Dynamic>)new[] { new Dynamic(MetersPerSecond) };
    // Unimplemented concept functions
    public Number Value => throw new NotImplementedException();
    public Velocity Lerp(Velocity b, Number amount) => throw new NotImplementedException();
    public Number Unlerp(Velocity a, Velocity b) => throw new NotImplementedException();
    public Number Magnitude => throw new NotImplementedException();
    public Integer Compare(Velocity y) => throw new NotImplementedException();
    public Velocity Multiply(Number other) => throw new NotImplementedException();
    public static Velocity operator *(Velocity self, Number other) => self.Multiply(other);
    public Velocity Divide(Number other) => throw new NotImplementedException();
    public static Velocity operator /(Velocity self, Number other) => self.Divide(other);
    public Velocity Modulo(Number other) => throw new NotImplementedException();
    public static Velocity operator %(Velocity self, Number other) => self.Modulo(other);
    public Velocity Add(Number other) => throw new NotImplementedException();
    public static Velocity operator +(Velocity self, Number other) => self.Add(other);
    public Velocity Subtract(Number other) => throw new NotImplementedException();
    public static Velocity operator -(Velocity self, Number other) => self.Subtract(other);
    public Velocity Zero => (MetersPerSecond.Zero);
    public Velocity One => (MetersPerSecond.One);
    public Velocity MinValue => (MetersPerSecond.MinValue);
    public Velocity MaxValue => (MetersPerSecond.MaxValue);
}
public readonly partial struct Acceleration: Measure<Acceleration>
{
    public readonly Number MetersPerSecondSquared;
    public Acceleration WithMetersPerSecondSquared(Number metersPerSecondSquared) => (metersPerSecondSquared);
    public Acceleration(Number metersPerSecondSquared) => (MetersPerSecondSquared) = (metersPerSecondSquared);
    public static Acceleration Default = new Acceleration();
    public static Acceleration New(Number metersPerSecondSquared) => new Acceleration(metersPerSecondSquared);
    public static implicit operator Number(Acceleration self) => self.MetersPerSecondSquared;
    public static implicit operator Acceleration(Number value) => new Acceleration(value);
    public String TypeName => "Acceleration";
    public Array<String> FieldNames => (Array1<String>)new[] { (String)"MetersPerSecondSquared" };
    public Array<Dynamic> FieldValues => (Array1<Dynamic>)new[] { new Dynamic(MetersPerSecondSquared) };
    // Unimplemented concept functions
    public Number Value => throw new NotImplementedException();
    public Acceleration Lerp(Acceleration b, Number amount) => throw new NotImplementedException();
    public Number Unlerp(Acceleration a, Acceleration b) => throw new NotImplementedException();
    public Number Magnitude => throw new NotImplementedException();
    public Integer Compare(Acceleration y) => throw new NotImplementedException();
    public Acceleration Multiply(Number other) => throw new NotImplementedException();
    public static Acceleration operator *(Acceleration self, Number other) => self.Multiply(other);
    public Acceleration Divide(Number other) => throw new NotImplementedException();
    public static Acceleration operator /(Acceleration self, Number other) => self.Divide(other);
    public Acceleration Modulo(Number other) => throw new NotImplementedException();
    public static Acceleration operator %(Acceleration self, Number other) => self.Modulo(other);
    public Acceleration Add(Number other) => throw new NotImplementedException();
    public static Acceleration operator +(Acceleration self, Number other) => self.Add(other);
    public Acceleration Subtract(Number other) => throw new NotImplementedException();
    public static Acceleration operator -(Acceleration self, Number other) => self.Subtract(other);
    public Acceleration Zero => (MetersPerSecondSquared.Zero);
    public Acceleration One => (MetersPerSecondSquared.One);
    public Acceleration MinValue => (MetersPerSecondSquared.MinValue);
    public Acceleration MaxValue => (MetersPerSecondSquared.MaxValue);
}
public readonly partial struct Force: Measure<Force>
{
    public readonly Number Newtons;
    public Force WithNewtons(Number newtons) => (newtons);
    public Force(Number newtons) => (Newtons) = (newtons);
    public static Force Default = new Force();
    public static Force New(Number newtons) => new Force(newtons);
    public static implicit operator Number(Force self) => self.Newtons;
    public static implicit operator Force(Number value) => new Force(value);
    public String TypeName => "Force";
    public Array<String> FieldNames => (Array1<String>)new[] { (String)"Newtons" };
    public Array<Dynamic> FieldValues => (Array1<Dynamic>)new[] { new Dynamic(Newtons) };
    // Unimplemented concept functions
    public Number Value => throw new NotImplementedException();
    public Force Lerp(Force b, Number amount) => throw new NotImplementedException();
    public Number Unlerp(Force a, Force b) => throw new NotImplementedException();
    public Number Magnitude => throw new NotImplementedException();
    public Integer Compare(Force y) => throw new NotImplementedException();
    public Force Multiply(Number other) => throw new NotImplementedException();
    public static Force operator *(Force self, Number other) => self.Multiply(other);
    public Force Divide(Number other) => throw new NotImplementedException();
    public static Force operator /(Force self, Number other) => self.Divide(other);
    public Force Modulo(Number other) => throw new NotImplementedException();
    public static Force operator %(Force self, Number other) => self.Modulo(other);
    public Force Add(Number other) => throw new NotImplementedException();
    public static Force operator +(Force self, Number other) => self.Add(other);
    public Force Subtract(Number other) => throw new NotImplementedException();
    public static Force operator -(Force self, Number other) => self.Subtract(other);
    public Force Zero => (Newtons.Zero);
    public Force One => (Newtons.One);
    public Force MinValue => (Newtons.MinValue);
    public Force MaxValue => (Newtons.MaxValue);
}
public readonly partial struct Pressure: Measure<Pressure>
{
    public readonly Number Pascals;
    public Pressure WithPascals(Number pascals) => (pascals);
    public Pressure(Number pascals) => (Pascals) = (pascals);
    public static Pressure Default = new Pressure();
    public static Pressure New(Number pascals) => new Pressure(pascals);
    public static implicit operator Number(Pressure self) => self.Pascals;
    public static implicit operator Pressure(Number value) => new Pressure(value);
    public String TypeName => "Pressure";
    public Array<String> FieldNames => (Array1<String>)new[] { (String)"Pascals" };
    public Array<Dynamic> FieldValues => (Array1<Dynamic>)new[] { new Dynamic(Pascals) };
    // Unimplemented concept functions
    public Number Value => throw new NotImplementedException();
    public Pressure Lerp(Pressure b, Number amount) => throw new NotImplementedException();
    public Number Unlerp(Pressure a, Pressure b) => throw new NotImplementedException();
    public Number Magnitude => throw new NotImplementedException();
    public Integer Compare(Pressure y) => throw new NotImplementedException();
    public Pressure Multiply(Number other) => throw new NotImplementedException();
    public static Pressure operator *(Pressure self, Number other) => self.Multiply(other);
    public Pressure Divide(Number other) => throw new NotImplementedException();
    public static Pressure operator /(Pressure self, Number other) => self.Divide(other);
    public Pressure Modulo(Number other) => throw new NotImplementedException();
    public static Pressure operator %(Pressure self, Number other) => self.Modulo(other);
    public Pressure Add(Number other) => throw new NotImplementedException();
    public static Pressure operator +(Pressure self, Number other) => self.Add(other);
    public Pressure Subtract(Number other) => throw new NotImplementedException();
    public static Pressure operator -(Pressure self, Number other) => self.Subtract(other);
    public Pressure Zero => (Pascals.Zero);
    public Pressure One => (Pascals.One);
    public Pressure MinValue => (Pascals.MinValue);
    public Pressure MaxValue => (Pascals.MaxValue);
}
public readonly partial struct Energy: Measure<Energy>
{
    public readonly Number Joules;
    public Energy WithJoules(Number joules) => (joules);
    public Energy(Number joules) => (Joules) = (joules);
    public static Energy Default = new Energy();
    public static Energy New(Number joules) => new Energy(joules);
    public static implicit operator Number(Energy self) => self.Joules;
    public static implicit operator Energy(Number value) => new Energy(value);
    public String TypeName => "Energy";
    public Array<String> FieldNames => (Array1<String>)new[] { (String)"Joules" };
    public Array<Dynamic> FieldValues => (Array1<Dynamic>)new[] { new Dynamic(Joules) };
    // Unimplemented concept functions
    public Number Value => throw new NotImplementedException();
    public Energy Lerp(Energy b, Number amount) => throw new NotImplementedException();
    public Number Unlerp(Energy a, Energy b) => throw new NotImplementedException();
    public Number Magnitude => throw new NotImplementedException();
    public Integer Compare(Energy y) => throw new NotImplementedException();
    public Energy Multiply(Number other) => throw new NotImplementedException();
    public static Energy operator *(Energy self, Number other) => self.Multiply(other);
    public Energy Divide(Number other) => throw new NotImplementedException();
    public static Energy operator /(Energy self, Number other) => self.Divide(other);
    public Energy Modulo(Number other) => throw new NotImplementedException();
    public static Energy operator %(Energy self, Number other) => self.Modulo(other);
    public Energy Add(Number other) => throw new NotImplementedException();
    public static Energy operator +(Energy self, Number other) => self.Add(other);
    public Energy Subtract(Number other) => throw new NotImplementedException();
    public static Energy operator -(Energy self, Number other) => self.Subtract(other);
    public Energy Zero => (Joules.Zero);
    public Energy One => (Joules.One);
    public Energy MinValue => (Joules.MinValue);
    public Energy MaxValue => (Joules.MaxValue);
}
public readonly partial struct Memory: Measure<Memory>
{
    public readonly Integer Bytes;
    public Memory WithBytes(Integer bytes) => (bytes);
    public Memory(Integer bytes) => (Bytes) = (bytes);
    public static Memory Default = new Memory();
    public static Memory New(Integer bytes) => new Memory(bytes);
    public static implicit operator Integer(Memory self) => self.Bytes;
    public static implicit operator Memory(Integer value) => new Memory(value);
    public String TypeName => "Memory";
    public Array<String> FieldNames => (Array1<String>)new[] { (String)"Bytes" };
    public Array<Dynamic> FieldValues => (Array1<Dynamic>)new[] { new Dynamic(Bytes) };
    // Unimplemented concept functions
    public Number Value => throw new NotImplementedException();
    public Memory Lerp(Memory b, Number amount) => throw new NotImplementedException();
    public Number Unlerp(Memory a, Memory b) => throw new NotImplementedException();
    public Number Magnitude => throw new NotImplementedException();
    public Integer Compare(Memory y) => throw new NotImplementedException();
    public Memory Multiply(Number other) => throw new NotImplementedException();
    public static Memory operator *(Memory self, Number other) => self.Multiply(other);
    public Memory Divide(Number other) => throw new NotImplementedException();
    public static Memory operator /(Memory self, Number other) => self.Divide(other);
    public Memory Modulo(Number other) => throw new NotImplementedException();
    public static Memory operator %(Memory self, Number other) => self.Modulo(other);
    public Memory Add(Number other) => throw new NotImplementedException();
    public static Memory operator +(Memory self, Number other) => self.Add(other);
    public Memory Subtract(Number other) => throw new NotImplementedException();
    public static Memory operator -(Memory self, Number other) => self.Subtract(other);
    public Memory Zero => (Bytes.Zero);
    public Memory One => (Bytes.One);
    public Memory MinValue => (Bytes.MinValue);
    public Memory MaxValue => (Bytes.MaxValue);
}
public readonly partial struct Frequency: Measure<Frequency>
{
    public readonly Number Hertz;
    public Frequency WithHertz(Number hertz) => (hertz);
    public Frequency(Number hertz) => (Hertz) = (hertz);
    public static Frequency Default = new Frequency();
    public static Frequency New(Number hertz) => new Frequency(hertz);
    public static implicit operator Number(Frequency self) => self.Hertz;
    public static implicit operator Frequency(Number value) => new Frequency(value);
    public String TypeName => "Frequency";
    public Array<String> FieldNames => (Array1<String>)new[] { (String)"Hertz" };
    public Array<Dynamic> FieldValues => (Array1<Dynamic>)new[] { new Dynamic(Hertz) };
    // Unimplemented concept functions
    public Number Value => throw new NotImplementedException();
    public Frequency Lerp(Frequency b, Number amount) => throw new NotImplementedException();
    public Number Unlerp(Frequency a, Frequency b) => throw new NotImplementedException();
    public Number Magnitude => throw new NotImplementedException();
    public Integer Compare(Frequency y) => throw new NotImplementedException();
    public Frequency Multiply(Number other) => throw new NotImplementedException();
    public static Frequency operator *(Frequency self, Number other) => self.Multiply(other);
    public Frequency Divide(Number other) => throw new NotImplementedException();
    public static Frequency operator /(Frequency self, Number other) => self.Divide(other);
    public Frequency Modulo(Number other) => throw new NotImplementedException();
    public static Frequency operator %(Frequency self, Number other) => self.Modulo(other);
    public Frequency Add(Number other) => throw new NotImplementedException();
    public static Frequency operator +(Frequency self, Number other) => self.Add(other);
    public Frequency Subtract(Number other) => throw new NotImplementedException();
    public static Frequency operator -(Frequency self, Number other) => self.Subtract(other);
    public Frequency Zero => (Hertz.Zero);
    public Frequency One => (Hertz.One);
    public Frequency MinValue => (Hertz.MinValue);
    public Frequency MaxValue => (Hertz.MaxValue);
}
public readonly partial struct Loudness: Measure<Loudness>
{
    public readonly Number Decibels;
    public Loudness WithDecibels(Number decibels) => (decibels);
    public Loudness(Number decibels) => (Decibels) = (decibels);
    public static Loudness Default = new Loudness();
    public static Loudness New(Number decibels) => new Loudness(decibels);
    public static implicit operator Number(Loudness self) => self.Decibels;
    public static implicit operator Loudness(Number value) => new Loudness(value);
    public String TypeName => "Loudness";
    public Array<String> FieldNames => (Array1<String>)new[] { (String)"Decibels" };
    public Array<Dynamic> FieldValues => (Array1<Dynamic>)new[] { new Dynamic(Decibels) };
    // Unimplemented concept functions
    public Number Value => throw new NotImplementedException();
    public Loudness Lerp(Loudness b, Number amount) => throw new NotImplementedException();
    public Number Unlerp(Loudness a, Loudness b) => throw new NotImplementedException();
    public Number Magnitude => throw new NotImplementedException();
    public Integer Compare(Loudness y) => throw new NotImplementedException();
    public Loudness Multiply(Number other) => throw new NotImplementedException();
    public static Loudness operator *(Loudness self, Number other) => self.Multiply(other);
    public Loudness Divide(Number other) => throw new NotImplementedException();
    public static Loudness operator /(Loudness self, Number other) => self.Divide(other);
    public Loudness Modulo(Number other) => throw new NotImplementedException();
    public static Loudness operator %(Loudness self, Number other) => self.Modulo(other);
    public Loudness Add(Number other) => throw new NotImplementedException();
    public static Loudness operator +(Loudness self, Number other) => self.Add(other);
    public Loudness Subtract(Number other) => throw new NotImplementedException();
    public static Loudness operator -(Loudness self, Number other) => self.Subtract(other);
    public Loudness Zero => (Decibels.Zero);
    public Loudness One => (Decibels.One);
    public Loudness MinValue => (Decibels.MinValue);
    public Loudness MaxValue => (Decibels.MaxValue);
}
public readonly partial struct LuminousIntensity: Measure<LuminousIntensity>
{
    public readonly Number Candelas;
    public LuminousIntensity WithCandelas(Number candelas) => (candelas);
    public LuminousIntensity(Number candelas) => (Candelas) = (candelas);
    public static LuminousIntensity Default = new LuminousIntensity();
    public static LuminousIntensity New(Number candelas) => new LuminousIntensity(candelas);
    public static implicit operator Number(LuminousIntensity self) => self.Candelas;
    public static implicit operator LuminousIntensity(Number value) => new LuminousIntensity(value);
    public String TypeName => "LuminousIntensity";
    public Array<String> FieldNames => (Array1<String>)new[] { (String)"Candelas" };
    public Array<Dynamic> FieldValues => (Array1<Dynamic>)new[] { new Dynamic(Candelas) };
    // Unimplemented concept functions
    public Number Value => throw new NotImplementedException();
    public LuminousIntensity Lerp(LuminousIntensity b, Number amount) => throw new NotImplementedException();
    public Number Unlerp(LuminousIntensity a, LuminousIntensity b) => throw new NotImplementedException();
    public Number Magnitude => throw new NotImplementedException();
    public Integer Compare(LuminousIntensity y) => throw new NotImplementedException();
    public LuminousIntensity Multiply(Number other) => throw new NotImplementedException();
    public static LuminousIntensity operator *(LuminousIntensity self, Number other) => self.Multiply(other);
    public LuminousIntensity Divide(Number other) => throw new NotImplementedException();
    public static LuminousIntensity operator /(LuminousIntensity self, Number other) => self.Divide(other);
    public LuminousIntensity Modulo(Number other) => throw new NotImplementedException();
    public static LuminousIntensity operator %(LuminousIntensity self, Number other) => self.Modulo(other);
    public LuminousIntensity Add(Number other) => throw new NotImplementedException();
    public static LuminousIntensity operator +(LuminousIntensity self, Number other) => self.Add(other);
    public LuminousIntensity Subtract(Number other) => throw new NotImplementedException();
    public static LuminousIntensity operator -(LuminousIntensity self, Number other) => self.Subtract(other);
    public LuminousIntensity Zero => (Candelas.Zero);
    public LuminousIntensity One => (Candelas.One);
    public LuminousIntensity MinValue => (Candelas.MinValue);
    public LuminousIntensity MaxValue => (Candelas.MaxValue);
}
public readonly partial struct ElectricPotential: Measure<ElectricPotential>
{
    public readonly Number Volts;
    public ElectricPotential WithVolts(Number volts) => (volts);
    public ElectricPotential(Number volts) => (Volts) = (volts);
    public static ElectricPotential Default = new ElectricPotential();
    public static ElectricPotential New(Number volts) => new ElectricPotential(volts);
    public static implicit operator Number(ElectricPotential self) => self.Volts;
    public static implicit operator ElectricPotential(Number value) => new ElectricPotential(value);
    public String TypeName => "ElectricPotential";
    public Array<String> FieldNames => (Array1<String>)new[] { (String)"Volts" };
    public Array<Dynamic> FieldValues => (Array1<Dynamic>)new[] { new Dynamic(Volts) };
    // Unimplemented concept functions
    public Number Value => throw new NotImplementedException();
    public ElectricPotential Lerp(ElectricPotential b, Number amount) => throw new NotImplementedException();
    public Number Unlerp(ElectricPotential a, ElectricPotential b) => throw new NotImplementedException();
    public Number Magnitude => throw new NotImplementedException();
    public Integer Compare(ElectricPotential y) => throw new NotImplementedException();
    public ElectricPotential Multiply(Number other) => throw new NotImplementedException();
    public static ElectricPotential operator *(ElectricPotential self, Number other) => self.Multiply(other);
    public ElectricPotential Divide(Number other) => throw new NotImplementedException();
    public static ElectricPotential operator /(ElectricPotential self, Number other) => self.Divide(other);
    public ElectricPotential Modulo(Number other) => throw new NotImplementedException();
    public static ElectricPotential operator %(ElectricPotential self, Number other) => self.Modulo(other);
    public ElectricPotential Add(Number other) => throw new NotImplementedException();
    public static ElectricPotential operator +(ElectricPotential self, Number other) => self.Add(other);
    public ElectricPotential Subtract(Number other) => throw new NotImplementedException();
    public static ElectricPotential operator -(ElectricPotential self, Number other) => self.Subtract(other);
    public ElectricPotential Zero => (Volts.Zero);
    public ElectricPotential One => (Volts.One);
    public ElectricPotential MinValue => (Volts.MinValue);
    public ElectricPotential MaxValue => (Volts.MaxValue);
}
public readonly partial struct ElectricCharge: Measure<ElectricCharge>
{
    public readonly Number Columbs;
    public ElectricCharge WithColumbs(Number columbs) => (columbs);
    public ElectricCharge(Number columbs) => (Columbs) = (columbs);
    public static ElectricCharge Default = new ElectricCharge();
    public static ElectricCharge New(Number columbs) => new ElectricCharge(columbs);
    public static implicit operator Number(ElectricCharge self) => self.Columbs;
    public static implicit operator ElectricCharge(Number value) => new ElectricCharge(value);
    public String TypeName => "ElectricCharge";
    public Array<String> FieldNames => (Array1<String>)new[] { (String)"Columbs" };
    public Array<Dynamic> FieldValues => (Array1<Dynamic>)new[] { new Dynamic(Columbs) };
    // Unimplemented concept functions
    public Number Value => throw new NotImplementedException();
    public ElectricCharge Lerp(ElectricCharge b, Number amount) => throw new NotImplementedException();
    public Number Unlerp(ElectricCharge a, ElectricCharge b) => throw new NotImplementedException();
    public Number Magnitude => throw new NotImplementedException();
    public Integer Compare(ElectricCharge y) => throw new NotImplementedException();
    public ElectricCharge Multiply(Number other) => throw new NotImplementedException();
    public static ElectricCharge operator *(ElectricCharge self, Number other) => self.Multiply(other);
    public ElectricCharge Divide(Number other) => throw new NotImplementedException();
    public static ElectricCharge operator /(ElectricCharge self, Number other) => self.Divide(other);
    public ElectricCharge Modulo(Number other) => throw new NotImplementedException();
    public static ElectricCharge operator %(ElectricCharge self, Number other) => self.Modulo(other);
    public ElectricCharge Add(Number other) => throw new NotImplementedException();
    public static ElectricCharge operator +(ElectricCharge self, Number other) => self.Add(other);
    public ElectricCharge Subtract(Number other) => throw new NotImplementedException();
    public static ElectricCharge operator -(ElectricCharge self, Number other) => self.Subtract(other);
    public ElectricCharge Zero => (Columbs.Zero);
    public ElectricCharge One => (Columbs.One);
    public ElectricCharge MinValue => (Columbs.MinValue);
    public ElectricCharge MaxValue => (Columbs.MaxValue);
}
public readonly partial struct ElectricCurrent: Measure<ElectricCurrent>
{
    public readonly Number Amperes;
    public ElectricCurrent WithAmperes(Number amperes) => (amperes);
    public ElectricCurrent(Number amperes) => (Amperes) = (amperes);
    public static ElectricCurrent Default = new ElectricCurrent();
    public static ElectricCurrent New(Number amperes) => new ElectricCurrent(amperes);
    public static implicit operator Number(ElectricCurrent self) => self.Amperes;
    public static implicit operator ElectricCurrent(Number value) => new ElectricCurrent(value);
    public String TypeName => "ElectricCurrent";
    public Array<String> FieldNames => (Array1<String>)new[] { (String)"Amperes" };
    public Array<Dynamic> FieldValues => (Array1<Dynamic>)new[] { new Dynamic(Amperes) };
    // Unimplemented concept functions
    public Number Value => throw new NotImplementedException();
    public ElectricCurrent Lerp(ElectricCurrent b, Number amount) => throw new NotImplementedException();
    public Number Unlerp(ElectricCurrent a, ElectricCurrent b) => throw new NotImplementedException();
    public Number Magnitude => throw new NotImplementedException();
    public Integer Compare(ElectricCurrent y) => throw new NotImplementedException();
    public ElectricCurrent Multiply(Number other) => throw new NotImplementedException();
    public static ElectricCurrent operator *(ElectricCurrent self, Number other) => self.Multiply(other);
    public ElectricCurrent Divide(Number other) => throw new NotImplementedException();
    public static ElectricCurrent operator /(ElectricCurrent self, Number other) => self.Divide(other);
    public ElectricCurrent Modulo(Number other) => throw new NotImplementedException();
    public static ElectricCurrent operator %(ElectricCurrent self, Number other) => self.Modulo(other);
    public ElectricCurrent Add(Number other) => throw new NotImplementedException();
    public static ElectricCurrent operator +(ElectricCurrent self, Number other) => self.Add(other);
    public ElectricCurrent Subtract(Number other) => throw new NotImplementedException();
    public static ElectricCurrent operator -(ElectricCurrent self, Number other) => self.Subtract(other);
    public ElectricCurrent Zero => (Amperes.Zero);
    public ElectricCurrent One => (Amperes.One);
    public ElectricCurrent MinValue => (Amperes.MinValue);
    public ElectricCurrent MaxValue => (Amperes.MaxValue);
}
public readonly partial struct ElectricResistance: Measure<ElectricResistance>
{
    public readonly Number Ohms;
    public ElectricResistance WithOhms(Number ohms) => (ohms);
    public ElectricResistance(Number ohms) => (Ohms) = (ohms);
    public static ElectricResistance Default = new ElectricResistance();
    public static ElectricResistance New(Number ohms) => new ElectricResistance(ohms);
    public static implicit operator Number(ElectricResistance self) => self.Ohms;
    public static implicit operator ElectricResistance(Number value) => new ElectricResistance(value);
    public String TypeName => "ElectricResistance";
    public Array<String> FieldNames => (Array1<String>)new[] { (String)"Ohms" };
    public Array<Dynamic> FieldValues => (Array1<Dynamic>)new[] { new Dynamic(Ohms) };
    // Unimplemented concept functions
    public Number Value => throw new NotImplementedException();
    public ElectricResistance Lerp(ElectricResistance b, Number amount) => throw new NotImplementedException();
    public Number Unlerp(ElectricResistance a, ElectricResistance b) => throw new NotImplementedException();
    public Number Magnitude => throw new NotImplementedException();
    public Integer Compare(ElectricResistance y) => throw new NotImplementedException();
    public ElectricResistance Multiply(Number other) => throw new NotImplementedException();
    public static ElectricResistance operator *(ElectricResistance self, Number other) => self.Multiply(other);
    public ElectricResistance Divide(Number other) => throw new NotImplementedException();
    public static ElectricResistance operator /(ElectricResistance self, Number other) => self.Divide(other);
    public ElectricResistance Modulo(Number other) => throw new NotImplementedException();
    public static ElectricResistance operator %(ElectricResistance self, Number other) => self.Modulo(other);
    public ElectricResistance Add(Number other) => throw new NotImplementedException();
    public static ElectricResistance operator +(ElectricResistance self, Number other) => self.Add(other);
    public ElectricResistance Subtract(Number other) => throw new NotImplementedException();
    public static ElectricResistance operator -(ElectricResistance self, Number other) => self.Subtract(other);
    public ElectricResistance Zero => (Ohms.Zero);
    public ElectricResistance One => (Ohms.One);
    public ElectricResistance MinValue => (Ohms.MinValue);
    public ElectricResistance MaxValue => (Ohms.MaxValue);
}
public readonly partial struct Power: Measure<Power>
{
    public readonly Number Watts;
    public Power WithWatts(Number watts) => (watts);
    public Power(Number watts) => (Watts) = (watts);
    public static Power Default = new Power();
    public static Power New(Number watts) => new Power(watts);
    public static implicit operator Number(Power self) => self.Watts;
    public static implicit operator Power(Number value) => new Power(value);
    public String TypeName => "Power";
    public Array<String> FieldNames => (Array1<String>)new[] { (String)"Watts" };
    public Array<Dynamic> FieldValues => (Array1<Dynamic>)new[] { new Dynamic(Watts) };
    // Unimplemented concept functions
    public Number Value => throw new NotImplementedException();
    public Power Lerp(Power b, Number amount) => throw new NotImplementedException();
    public Number Unlerp(Power a, Power b) => throw new NotImplementedException();
    public Number Magnitude => throw new NotImplementedException();
    public Integer Compare(Power y) => throw new NotImplementedException();
    public Power Multiply(Number other) => throw new NotImplementedException();
    public static Power operator *(Power self, Number other) => self.Multiply(other);
    public Power Divide(Number other) => throw new NotImplementedException();
    public static Power operator /(Power self, Number other) => self.Divide(other);
    public Power Modulo(Number other) => throw new NotImplementedException();
    public static Power operator %(Power self, Number other) => self.Modulo(other);
    public Power Add(Number other) => throw new NotImplementedException();
    public static Power operator +(Power self, Number other) => self.Add(other);
    public Power Subtract(Number other) => throw new NotImplementedException();
    public static Power operator -(Power self, Number other) => self.Subtract(other);
    public Power Zero => (Watts.Zero);
    public Power One => (Watts.One);
    public Power MinValue => (Watts.MinValue);
    public Power MaxValue => (Watts.MaxValue);
}
public readonly partial struct Density: Measure<Density>
{
    public readonly Number KilogramsPerMeterCubed;
    public Density WithKilogramsPerMeterCubed(Number kilogramsPerMeterCubed) => (kilogramsPerMeterCubed);
    public Density(Number kilogramsPerMeterCubed) => (KilogramsPerMeterCubed) = (kilogramsPerMeterCubed);
    public static Density Default = new Density();
    public static Density New(Number kilogramsPerMeterCubed) => new Density(kilogramsPerMeterCubed);
    public static implicit operator Number(Density self) => self.KilogramsPerMeterCubed;
    public static implicit operator Density(Number value) => new Density(value);
    public String TypeName => "Density";
    public Array<String> FieldNames => (Array1<String>)new[] { (String)"KilogramsPerMeterCubed" };
    public Array<Dynamic> FieldValues => (Array1<Dynamic>)new[] { new Dynamic(KilogramsPerMeterCubed) };
    // Unimplemented concept functions
    public Number Value => throw new NotImplementedException();
    public Density Lerp(Density b, Number amount) => throw new NotImplementedException();
    public Number Unlerp(Density a, Density b) => throw new NotImplementedException();
    public Number Magnitude => throw new NotImplementedException();
    public Integer Compare(Density y) => throw new NotImplementedException();
    public Density Multiply(Number other) => throw new NotImplementedException();
    public static Density operator *(Density self, Number other) => self.Multiply(other);
    public Density Divide(Number other) => throw new NotImplementedException();
    public static Density operator /(Density self, Number other) => self.Divide(other);
    public Density Modulo(Number other) => throw new NotImplementedException();
    public static Density operator %(Density self, Number other) => self.Modulo(other);
    public Density Add(Number other) => throw new NotImplementedException();
    public static Density operator +(Density self, Number other) => self.Add(other);
    public Density Subtract(Number other) => throw new NotImplementedException();
    public static Density operator -(Density self, Number other) => self.Subtract(other);
    public Density Zero => (KilogramsPerMeterCubed.Zero);
    public Density One => (KilogramsPerMeterCubed.One);
    public Density MinValue => (KilogramsPerMeterCubed.MinValue);
    public Density MaxValue => (KilogramsPerMeterCubed.MaxValue);
}
public readonly partial struct NormalDistribution: Value<NormalDistribution>
{
    public readonly Number Mean;
    public readonly Number StandardDeviation;
    public NormalDistribution WithMean(Number mean) => (mean, StandardDeviation);
    public NormalDistribution WithStandardDeviation(Number standardDeviation) => (Mean, standardDeviation);
    public NormalDistribution(Number mean, Number standardDeviation) => (Mean, StandardDeviation) = (mean, standardDeviation);
    public static NormalDistribution Default = new NormalDistribution();
    public static NormalDistribution New(Number mean, Number standardDeviation) => new NormalDistribution(mean, standardDeviation);
    public static implicit operator (Number, Number)(NormalDistribution self) => (self.Mean, self.StandardDeviation);
    public static implicit operator NormalDistribution((Number, Number) value) => new NormalDistribution(value.Item1, value.Item2);
    public void Deconstruct(out Number mean, out Number standardDeviation) { mean = Mean; standardDeviation = StandardDeviation; }
    public String TypeName => "NormalDistribution";
    public Array<String> FieldNames => (Array1<String>)new[] { (String)"Mean", (String)"StandardDeviation" };
    public Array<Dynamic> FieldValues => (Array1<Dynamic>)new[] { new Dynamic(Mean), new Dynamic(StandardDeviation) };
    // Unimplemented concept functions
    public NormalDistribution Zero => (Mean.Zero, StandardDeviation.Zero);
    public NormalDistribution One => (Mean.One, StandardDeviation.One);
    public NormalDistribution MinValue => (Mean.MinValue, StandardDeviation.MinValue);
    public NormalDistribution MaxValue => (Mean.MaxValue, StandardDeviation.MaxValue);
    public Boolean Equals(NormalDistribution b) => (Mean.Equals(b.Mean) & StandardDeviation.Equals(b.StandardDeviation));
    public static Boolean operator ==(NormalDistribution a, NormalDistribution b) => a.Equals(b);
    public Boolean NotEquals(NormalDistribution b) => (Mean.NotEquals(b.Mean) & StandardDeviation.NotEquals(b.StandardDeviation));
    public static Boolean operator !=(NormalDistribution a, NormalDistribution b) => a.NotEquals(b);
}
public readonly partial struct PoissonDistribution: Value<PoissonDistribution>
{
    public readonly Number Expected;
    public readonly Integer Occurrences;
    public PoissonDistribution WithExpected(Number expected) => (expected, Occurrences);
    public PoissonDistribution WithOccurrences(Integer occurrences) => (Expected, occurrences);
    public PoissonDistribution(Number expected, Integer occurrences) => (Expected, Occurrences) = (expected, occurrences);
    public static PoissonDistribution Default = new PoissonDistribution();
    public static PoissonDistribution New(Number expected, Integer occurrences) => new PoissonDistribution(expected, occurrences);
    public static implicit operator (Number, Integer)(PoissonDistribution self) => (self.Expected, self.Occurrences);
    public static implicit operator PoissonDistribution((Number, Integer) value) => new PoissonDistribution(value.Item1, value.Item2);
    public void Deconstruct(out Number expected, out Integer occurrences) { expected = Expected; occurrences = Occurrences; }
    public String TypeName => "PoissonDistribution";
    public Array<String> FieldNames => (Array1<String>)new[] { (String)"Expected", (String)"Occurrences" };
    public Array<Dynamic> FieldValues => (Array1<Dynamic>)new[] { new Dynamic(Expected), new Dynamic(Occurrences) };
    // Unimplemented concept functions
    public PoissonDistribution Zero => (Expected.Zero, Occurrences.Zero);
    public PoissonDistribution One => (Expected.One, Occurrences.One);
    public PoissonDistribution MinValue => (Expected.MinValue, Occurrences.MinValue);
    public PoissonDistribution MaxValue => (Expected.MaxValue, Occurrences.MaxValue);
    public Boolean Equals(PoissonDistribution b) => (Expected.Equals(b.Expected) & Occurrences.Equals(b.Occurrences));
    public static Boolean operator ==(PoissonDistribution a, PoissonDistribution b) => a.Equals(b);
    public Boolean NotEquals(PoissonDistribution b) => (Expected.NotEquals(b.Expected) & Occurrences.NotEquals(b.Occurrences));
    public static Boolean operator !=(PoissonDistribution a, PoissonDistribution b) => a.NotEquals(b);
}
public readonly partial struct BernoulliDistribution: Value<BernoulliDistribution>
{
    public readonly Probability P;
    public BernoulliDistribution WithP(Probability p) => (p);
    public BernoulliDistribution(Probability p) => (P) = (p);
    public static BernoulliDistribution Default = new BernoulliDistribution();
    public static BernoulliDistribution New(Probability p) => new BernoulliDistribution(p);
    public static implicit operator Probability(BernoulliDistribution self) => self.P;
    public static implicit operator BernoulliDistribution(Probability value) => new BernoulliDistribution(value);
    public String TypeName => "BernoulliDistribution";
    public Array<String> FieldNames => (Array1<String>)new[] { (String)"P" };
    public Array<Dynamic> FieldValues => (Array1<Dynamic>)new[] { new Dynamic(P) };
    // Unimplemented concept functions
    public BernoulliDistribution Zero => (P.Zero);
    public BernoulliDistribution One => (P.One);
    public BernoulliDistribution MinValue => (P.MinValue);
    public BernoulliDistribution MaxValue => (P.MaxValue);
    public Boolean Equals(BernoulliDistribution b) => (P.Equals(b.P));
    public static Boolean operator ==(BernoulliDistribution a, BernoulliDistribution b) => a.Equals(b);
    public Boolean NotEquals(BernoulliDistribution b) => (P.NotEquals(b.P));
    public static Boolean operator !=(BernoulliDistribution a, BernoulliDistribution b) => a.NotEquals(b);
}
public readonly partial struct Probability: Numerical<Probability>
{
    public readonly Number Value;
    public Probability WithValue(Number value) => (value);
    public Probability(Number value) => (Value) = (value);
    public static Probability Default = new Probability();
    public static Probability New(Number value) => new Probability(value);
    public static implicit operator Number(Probability self) => self.Value;
    public static implicit operator Probability(Number value) => new Probability(value);
    public String TypeName => "Probability";
    public Array<String> FieldNames => (Array1<String>)new[] { (String)"Value" };
    public Array<Dynamic> FieldValues => (Array1<Dynamic>)new[] { new Dynamic(Value) };
    // Unimplemented concept functions
    public Number Unlerp(Probability a, Probability b) => throw new NotImplementedException();
    public Number Magnitude => throw new NotImplementedException();
    public Integer Compare(Probability y) => throw new NotImplementedException();
    public Probability Reciprocal => (Value.Reciprocal);
    public Probability Negative => (Value.Negative);
    public static Probability operator -(Probability self) => self.Negative;
    public Probability Multiply(Probability other) => (Value.Multiply(other.Value));
    public static Probability operator *(Probability self, Probability other) => self.Multiply(other);
    public Probability Divide(Probability other) => (Value.Divide(other.Value));
    public static Probability operator /(Probability self, Probability other) => self.Divide(other);
    public Probability Modulo(Probability other) => (Value.Modulo(other.Value));
    public static Probability operator %(Probability self, Probability other) => self.Modulo(other);
    public Probability Add(Probability other) => (Value.Add(other.Value));
    public static Probability operator +(Probability self, Probability other) => self.Add(other);
    public Probability Subtract(Probability other) => (Value.Subtract(other.Value));
    public static Probability operator -(Probability self, Probability other) => self.Subtract(other);
    public Probability Zero => (Value.Zero);
    public Probability One => (Value.One);
    public Probability MinValue => (Value.MinValue);
    public Probability MaxValue => (Value.MaxValue);
}
public readonly partial struct BinomialDistribution: Value<BinomialDistribution>
{
    public readonly Integer Trials;
    public readonly Probability P;
    public BinomialDistribution WithTrials(Integer trials) => (trials, P);
    public BinomialDistribution WithP(Probability p) => (Trials, p);
    public BinomialDistribution(Integer trials, Probability p) => (Trials, P) = (trials, p);
    public static BinomialDistribution Default = new BinomialDistribution();
    public static BinomialDistribution New(Integer trials, Probability p) => new BinomialDistribution(trials, p);
    public static implicit operator (Integer, Probability)(BinomialDistribution self) => (self.Trials, self.P);
    public static implicit operator BinomialDistribution((Integer, Probability) value) => new BinomialDistribution(value.Item1, value.Item2);
    public void Deconstruct(out Integer trials, out Probability p) { trials = Trials; p = P; }
    public String TypeName => "BinomialDistribution";
    public Array<String> FieldNames => (Array1<String>)new[] { (String)"Trials", (String)"P" };
    public Array<Dynamic> FieldValues => (Array1<Dynamic>)new[] { new Dynamic(Trials), new Dynamic(P) };
    // Unimplemented concept functions
    public BinomialDistribution Zero => (Trials.Zero, P.Zero);
    public BinomialDistribution One => (Trials.One, P.One);
    public BinomialDistribution MinValue => (Trials.MinValue, P.MinValue);
    public BinomialDistribution MaxValue => (Trials.MaxValue, P.MaxValue);
    public Boolean Equals(BinomialDistribution b) => (Trials.Equals(b.Trials) & P.Equals(b.P));
    public static Boolean operator ==(BinomialDistribution a, BinomialDistribution b) => a.Equals(b);
    public Boolean NotEquals(BinomialDistribution b) => (Trials.NotEquals(b.Trials) & P.NotEquals(b.P));
    public static Boolean operator !=(BinomialDistribution a, BinomialDistribution b) => a.NotEquals(b);
}
public readonly partial struct Tuple2<T0, T1>
{
    public readonly T0 Item0;
    public readonly T1 Item1;
    public Tuple2<T0, T1> WithItem0(T0 item0) => (item0, Item1);
    public Tuple2<T0, T1> WithItem1(T1 item1) => (Item0, item1);
    public Tuple2(T0 item0, T1 item1) => (Item0, Item1) = (item0, item1);
    public static Tuple2<T0, T1> Default = new Tuple2<T0, T1>();
    public static Tuple2<T0, T1> New(T0 item0, T1 item1) => new Tuple2<T0, T1>(item0, item1);
    public static implicit operator (T0, T1)(Tuple2<T0, T1> self) => (self.Item0, self.Item1);
    public static implicit operator Tuple2<T0, T1>((T0, T1) value) => new Tuple2<T0, T1>(value.Item1, value.Item2);
    public void Deconstruct(out T0 item0, out T1 item1) { item0 = Item0; item1 = Item1; }
    public String TypeName => "Tuple2<T0, T1>";
    public Array<String> FieldNames => (Array1<String>)new[] { (String)"Item0", (String)"Item1" };
    public Array<Dynamic> FieldValues => (Array1<Dynamic>)new[] { new Dynamic(Item0), new Dynamic(Item1) };
    // Unimplemented concept functions
}
public readonly partial struct Tuple3<T0, T1, T2>
{
    public readonly T0 Item0;
    public readonly T1 Item1;
    public readonly T2 Item2;
    public Tuple3<T0, T1, T2> WithItem0(T0 item0) => (item0, Item1, Item2);
    public Tuple3<T0, T1, T2> WithItem1(T1 item1) => (Item0, item1, Item2);
    public Tuple3<T0, T1, T2> WithItem2(T2 item2) => (Item0, Item1, item2);
    public Tuple3(T0 item0, T1 item1, T2 item2) => (Item0, Item1, Item2) = (item0, item1, item2);
    public static Tuple3<T0, T1, T2> Default = new Tuple3<T0, T1, T2>();
    public static Tuple3<T0, T1, T2> New(T0 item0, T1 item1, T2 item2) => new Tuple3<T0, T1, T2>(item0, item1, item2);
    public static implicit operator (T0, T1, T2)(Tuple3<T0, T1, T2> self) => (self.Item0, self.Item1, self.Item2);
    public static implicit operator Tuple3<T0, T1, T2>((T0, T1, T2) value) => new Tuple3<T0, T1, T2>(value.Item1, value.Item2, value.Item3);
    public void Deconstruct(out T0 item0, out T1 item1, out T2 item2) { item0 = Item0; item1 = Item1; item2 = Item2; }
    public String TypeName => "Tuple3<T0, T1, T2>";
    public Array<String> FieldNames => (Array1<String>)new[] { (String)"Item0", (String)"Item1", (String)"Item2" };
    public Array<Dynamic> FieldValues => (Array1<Dynamic>)new[] { new Dynamic(Item0), new Dynamic(Item1), new Dynamic(Item2) };
    // Unimplemented concept functions
}
