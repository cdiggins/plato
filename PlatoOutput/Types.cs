using System;
public readonly partial struct Number
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
public readonly partial struct Integer
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
public readonly partial struct String
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
public readonly partial struct Boolean
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
public readonly partial struct Character
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
public readonly partial struct Count
{
    public readonly Integer Value;
    public Count WithValue(Integer value) => (value);
    public Count(Integer value) => (Value) = (value);
    public static Count Default = new Count();
    public static Count New(Integer value) => new Count(value);
    public static implicit operator Integer(Count self) => self.Value;
    public static implicit operator Count(Integer value) => new Count(value);
    public String TypeName => "Count";
    public Array<String> FieldNames => (Array1<String>)new[] { (String)"Value" };
    public Array<Dynamic> FieldValues => (Array1<Dynamic>)new[] { new Dynamic(Value) };
    // Unimplemented concept functions
    public Count Zero => (Value.Zero);
    public Count One => (Value.One);
    public Count MinValue => (Value.MinValue);
    public Count MaxValue => (Value.MaxValue);
    public Number Unlerp(Count a, Count b) => throw new NotImplementedException();
    public Number Magnitude => throw new NotImplementedException();
    public Integer Compare(Count y) => throw new NotImplementedException();
    public Count Reciprocal => (Value.Reciprocal);
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
}
public readonly partial struct Index
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
    public Boolean Equals(Index b) => throw new NotImplementedException();
    public static Boolean operator ==(Index a, Index b) => a.Equals(b);
    public Boolean NotEquals(Index b) => throw new NotImplementedException();
    public static Boolean operator !=(Index a, Index b) => a.NotEquals(b);
}
public readonly partial struct Unit
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
    public Unit Zero => (Value.Zero);
    public Unit One => (Value.One);
    public Unit MinValue => (Value.MinValue);
    public Unit MaxValue => (Value.MaxValue);
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
}
public readonly partial struct Percent
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
    public Percent Zero => (Value.Zero);
    public Percent One => (Value.One);
    public Percent MinValue => (Value.MinValue);
    public Percent MaxValue => (Value.MaxValue);
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
}
public readonly partial struct Quaternion
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
    public Boolean Equals(Quaternion b) => throw new NotImplementedException();
    public static Boolean operator ==(Quaternion a, Quaternion b) => a.Equals(b);
    public Boolean NotEquals(Quaternion b) => throw new NotImplementedException();
    public static Boolean operator !=(Quaternion a, Quaternion b) => a.NotEquals(b);
}
public readonly partial struct Unit2D
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
    public Boolean Equals(Unit2D b) => throw new NotImplementedException();
    public static Boolean operator ==(Unit2D a, Unit2D b) => a.Equals(b);
    public Boolean NotEquals(Unit2D b) => throw new NotImplementedException();
    public static Boolean operator !=(Unit2D a, Unit2D b) => a.NotEquals(b);
}
public readonly partial struct Unit3D
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
    public Boolean Equals(Unit3D b) => throw new NotImplementedException();
    public static Boolean operator ==(Unit3D a, Unit3D b) => a.Equals(b);
    public Boolean NotEquals(Unit3D b) => throw new NotImplementedException();
    public static Boolean operator !=(Unit3D a, Unit3D b) => a.NotEquals(b);
}
public readonly partial struct Direction3D
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
    public Boolean Equals(Direction3D b) => throw new NotImplementedException();
    public static Boolean operator ==(Direction3D a, Direction3D b) => a.Equals(b);
    public Boolean NotEquals(Direction3D b) => throw new NotImplementedException();
    public static Boolean operator !=(Direction3D a, Direction3D b) => a.NotEquals(b);
}
public readonly partial struct AxisAngle
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
    public Boolean Equals(AxisAngle b) => throw new NotImplementedException();
    public static Boolean operator ==(AxisAngle a, AxisAngle b) => a.Equals(b);
    public Boolean NotEquals(AxisAngle b) => throw new NotImplementedException();
    public static Boolean operator !=(AxisAngle a, AxisAngle b) => a.NotEquals(b);
}
public readonly partial struct EulerAngles
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
    public Boolean Equals(EulerAngles b) => throw new NotImplementedException();
    public static Boolean operator ==(EulerAngles a, EulerAngles b) => a.Equals(b);
    public Boolean NotEquals(EulerAngles b) => throw new NotImplementedException();
    public static Boolean operator !=(EulerAngles a, EulerAngles b) => a.NotEquals(b);
}
public readonly partial struct Rotation3D
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
    public Boolean Equals(Rotation3D b) => throw new NotImplementedException();
    public static Boolean operator ==(Rotation3D a, Rotation3D b) => a.Equals(b);
    public Boolean NotEquals(Rotation3D b) => throw new NotImplementedException();
    public static Boolean operator !=(Rotation3D a, Rotation3D b) => a.NotEquals(b);
}
public readonly partial struct Vector2D
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
    public Number Unlerp(Vector2D a, Vector2D b) => throw new NotImplementedException();
    public Integer Compare(Vector2D y) => throw new NotImplementedException();
    public Vector2D Zero => (X.Zero, Y.Zero);
    public Vector2D One => (X.One, Y.One);
    public Vector2D MinValue => (X.MinValue, Y.MinValue);
    public Vector2D MaxValue => (X.MaxValue, Y.MaxValue);
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
    public Integer Count => throw new NotImplementedException();
    public Number At(Integer n) => throw new NotImplementedException();
    public Number this[Integer n] => At(n);
}
public readonly partial struct Vector3D
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
    public Number Unlerp(Vector3D a, Vector3D b) => throw new NotImplementedException();
    public Integer Compare(Vector3D y) => throw new NotImplementedException();
    public Vector3D Zero => (X.Zero, Y.Zero, Z.Zero);
    public Vector3D One => (X.One, Y.One, Z.One);
    public Vector3D MinValue => (X.MinValue, Y.MinValue, Z.MinValue);
    public Vector3D MaxValue => (X.MaxValue, Y.MaxValue, Z.MaxValue);
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
    public Integer Count => throw new NotImplementedException();
    public Number At(Integer n) => throw new NotImplementedException();
    public Number this[Integer n] => At(n);
}
public readonly partial struct Vector4D
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
    public Number Unlerp(Vector4D a, Vector4D b) => throw new NotImplementedException();
    public Integer Compare(Vector4D y) => throw new NotImplementedException();
    public Vector4D Zero => (X.Zero, Y.Zero, Z.Zero, W.Zero);
    public Vector4D One => (X.One, Y.One, Z.One, W.One);
    public Vector4D MinValue => (X.MinValue, Y.MinValue, Z.MinValue, W.MinValue);
    public Vector4D MaxValue => (X.MaxValue, Y.MaxValue, Z.MaxValue, W.MaxValue);
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
    public Integer Count => throw new NotImplementedException();
    public Number At(Integer n) => throw new NotImplementedException();
    public Number this[Integer n] => At(n);
}
public readonly partial struct Orientation3D
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
    public Boolean Equals(Orientation3D b) => throw new NotImplementedException();
    public static Boolean operator ==(Orientation3D a, Orientation3D b) => a.Equals(b);
    public Boolean NotEquals(Orientation3D b) => throw new NotImplementedException();
    public static Boolean operator !=(Orientation3D a, Orientation3D b) => a.NotEquals(b);
}
public readonly partial struct Pose2D
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
    public Boolean Equals(Pose2D b) => throw new NotImplementedException();
    public static Boolean operator ==(Pose2D a, Pose2D b) => a.Equals(b);
    public Boolean NotEquals(Pose2D b) => throw new NotImplementedException();
    public static Boolean operator !=(Pose2D a, Pose2D b) => a.NotEquals(b);
}
public readonly partial struct Pose3D
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
    public Boolean Equals(Pose3D b) => throw new NotImplementedException();
    public static Boolean operator ==(Pose3D a, Pose3D b) => a.Equals(b);
    public Boolean NotEquals(Pose3D b) => throw new NotImplementedException();
    public static Boolean operator !=(Pose3D a, Pose3D b) => a.NotEquals(b);
}
public readonly partial struct Transform3D
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
    public Boolean Equals(Transform3D b) => throw new NotImplementedException();
    public static Boolean operator ==(Transform3D a, Transform3D b) => a.Equals(b);
    public Boolean NotEquals(Transform3D b) => throw new NotImplementedException();
    public static Boolean operator !=(Transform3D a, Transform3D b) => a.NotEquals(b);
}
public readonly partial struct Transform2D
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
    public Boolean Equals(Transform2D b) => throw new NotImplementedException();
    public static Boolean operator ==(Transform2D a, Transform2D b) => a.Equals(b);
    public Boolean NotEquals(Transform2D b) => throw new NotImplementedException();
    public static Boolean operator !=(Transform2D a, Transform2D b) => a.NotEquals(b);
}
public readonly partial struct AlignedBox2D
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
}
public readonly partial struct AlignedBox3D
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
}
public readonly partial struct Complex
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
    public Number Unlerp(Complex a, Complex b) => throw new NotImplementedException();
    public Integer Compare(Complex y) => throw new NotImplementedException();
    public Complex Zero => (Real.Zero, Imaginary.Zero);
    public Complex One => (Real.One, Imaginary.One);
    public Complex MinValue => (Real.MinValue, Imaginary.MinValue);
    public Complex MaxValue => (Real.MaxValue, Imaginary.MaxValue);
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
    public Integer Count => throw new NotImplementedException();
    public Number At(Integer n) => throw new NotImplementedException();
    public Number this[Integer n] => At(n);
}
public readonly partial struct Ray3D
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
    public Boolean Equals(Ray3D b) => throw new NotImplementedException();
    public static Boolean operator ==(Ray3D a, Ray3D b) => a.Equals(b);
    public Boolean NotEquals(Ray3D b) => throw new NotImplementedException();
    public static Boolean operator !=(Ray3D a, Ray3D b) => a.NotEquals(b);
}
public readonly partial struct Ray2D
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
    public Boolean Equals(Ray2D b) => throw new NotImplementedException();
    public static Boolean operator ==(Ray2D a, Ray2D b) => a.Equals(b);
    public Boolean NotEquals(Ray2D b) => throw new NotImplementedException();
    public static Boolean operator !=(Ray2D a, Ray2D b) => a.NotEquals(b);
}
public readonly partial struct Sphere
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
    public Boolean Equals(Sphere b) => throw new NotImplementedException();
    public static Boolean operator ==(Sphere a, Sphere b) => a.Equals(b);
    public Boolean NotEquals(Sphere b) => throw new NotImplementedException();
    public static Boolean operator !=(Sphere a, Sphere b) => a.NotEquals(b);
}
public readonly partial struct Plane
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
    public Boolean Equals(Plane b) => throw new NotImplementedException();
    public static Boolean operator ==(Plane a, Plane b) => a.Equals(b);
    public Boolean NotEquals(Plane b) => throw new NotImplementedException();
    public static Boolean operator !=(Plane a, Plane b) => a.NotEquals(b);
}
public readonly partial struct Triangle2D
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
    public Boolean Equals(Triangle2D b) => throw new NotImplementedException();
    public static Boolean operator ==(Triangle2D a, Triangle2D b) => a.Equals(b);
    public Boolean NotEquals(Triangle2D b) => throw new NotImplementedException();
    public static Boolean operator !=(Triangle2D a, Triangle2D b) => a.NotEquals(b);
}
public readonly partial struct Triangle3D
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
    public Boolean Equals(Triangle3D b) => throw new NotImplementedException();
    public static Boolean operator ==(Triangle3D a, Triangle3D b) => a.Equals(b);
    public Boolean NotEquals(Triangle3D b) => throw new NotImplementedException();
    public static Boolean operator !=(Triangle3D a, Triangle3D b) => a.NotEquals(b);
}
public readonly partial struct Quad2D
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
    public Boolean Equals(Quad2D b) => throw new NotImplementedException();
    public static Boolean operator ==(Quad2D a, Quad2D b) => a.Equals(b);
    public Boolean NotEquals(Quad2D b) => throw new NotImplementedException();
    public static Boolean operator !=(Quad2D a, Quad2D b) => a.NotEquals(b);
}
public readonly partial struct Quad3D
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
    public Boolean Equals(Quad3D b) => throw new NotImplementedException();
    public static Boolean operator ==(Quad3D a, Quad3D b) => a.Equals(b);
    public Boolean NotEquals(Quad3D b) => throw new NotImplementedException();
    public static Boolean operator !=(Quad3D a, Quad3D b) => a.NotEquals(b);
}
public readonly partial struct Point2D
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
    public Point2D Lerp(Point2D b, Number amount) => throw new NotImplementedException();
    public Number Unlerp(Point2D a, Point2D b) => throw new NotImplementedException();
}
public readonly partial struct Point3D
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
    public Point3D Lerp(Point3D b, Number amount) => throw new NotImplementedException();
    public Number Unlerp(Point3D a, Point3D b) => throw new NotImplementedException();
}
public readonly partial struct Line2D
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
    public Vector3D Size => throw new NotImplementedException();
}
public readonly partial struct Line3D
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
}
public readonly partial struct Color
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
    public Boolean Equals(Color b) => throw new NotImplementedException();
    public static Boolean operator ==(Color a, Color b) => a.Equals(b);
    public Boolean NotEquals(Color b) => throw new NotImplementedException();
    public static Boolean operator !=(Color a, Color b) => a.NotEquals(b);
}
public readonly partial struct ColorLUV
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
    public Boolean Equals(ColorLUV b) => throw new NotImplementedException();
    public static Boolean operator ==(ColorLUV a, ColorLUV b) => a.Equals(b);
    public Boolean NotEquals(ColorLUV b) => throw new NotImplementedException();
    public static Boolean operator !=(ColorLUV a, ColorLUV b) => a.NotEquals(b);
}
public readonly partial struct ColorLAB
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
    public Boolean Equals(ColorLAB b) => throw new NotImplementedException();
    public static Boolean operator ==(ColorLAB a, ColorLAB b) => a.Equals(b);
    public Boolean NotEquals(ColorLAB b) => throw new NotImplementedException();
    public static Boolean operator !=(ColorLAB a, ColorLAB b) => a.NotEquals(b);
}
public readonly partial struct ColorLCh
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
    public Boolean Equals(ColorLCh b) => throw new NotImplementedException();
    public static Boolean operator ==(ColorLCh a, ColorLCh b) => a.Equals(b);
    public Boolean NotEquals(ColorLCh b) => throw new NotImplementedException();
    public static Boolean operator !=(ColorLCh a, ColorLCh b) => a.NotEquals(b);
}
public readonly partial struct ColorHSV
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
    public Boolean Equals(ColorHSV b) => throw new NotImplementedException();
    public static Boolean operator ==(ColorHSV a, ColorHSV b) => a.Equals(b);
    public Boolean NotEquals(ColorHSV b) => throw new NotImplementedException();
    public static Boolean operator !=(ColorHSV a, ColorHSV b) => a.NotEquals(b);
}
public readonly partial struct ColorHSL
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
    public Boolean Equals(ColorHSL b) => throw new NotImplementedException();
    public static Boolean operator ==(ColorHSL a, ColorHSL b) => a.Equals(b);
    public Boolean NotEquals(ColorHSL b) => throw new NotImplementedException();
    public static Boolean operator !=(ColorHSL a, ColorHSL b) => a.NotEquals(b);
}
public readonly partial struct ColorYCbCr
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
    public Boolean Equals(ColorYCbCr b) => throw new NotImplementedException();
    public static Boolean operator ==(ColorYCbCr a, ColorYCbCr b) => a.Equals(b);
    public Boolean NotEquals(ColorYCbCr b) => throw new NotImplementedException();
    public static Boolean operator !=(ColorYCbCr a, ColorYCbCr b) => a.NotEquals(b);
}
public readonly partial struct SphericalCoordinate
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
    public Boolean Equals(SphericalCoordinate b) => throw new NotImplementedException();
    public static Boolean operator ==(SphericalCoordinate a, SphericalCoordinate b) => a.Equals(b);
    public Boolean NotEquals(SphericalCoordinate b) => throw new NotImplementedException();
    public static Boolean operator !=(SphericalCoordinate a, SphericalCoordinate b) => a.NotEquals(b);
}
public readonly partial struct PolarCoordinate
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
    public Boolean Equals(PolarCoordinate b) => throw new NotImplementedException();
    public static Boolean operator ==(PolarCoordinate a, PolarCoordinate b) => a.Equals(b);
    public Boolean NotEquals(PolarCoordinate b) => throw new NotImplementedException();
    public static Boolean operator !=(PolarCoordinate a, PolarCoordinate b) => a.NotEquals(b);
}
public readonly partial struct LogPolarCoordinate
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
    public Boolean Equals(LogPolarCoordinate b) => throw new NotImplementedException();
    public static Boolean operator ==(LogPolarCoordinate a, LogPolarCoordinate b) => a.Equals(b);
    public Boolean NotEquals(LogPolarCoordinate b) => throw new NotImplementedException();
    public static Boolean operator !=(LogPolarCoordinate a, LogPolarCoordinate b) => a.NotEquals(b);
}
public readonly partial struct CylindricalCoordinate
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
    public Boolean Equals(CylindricalCoordinate b) => throw new NotImplementedException();
    public static Boolean operator ==(CylindricalCoordinate a, CylindricalCoordinate b) => a.Equals(b);
    public Boolean NotEquals(CylindricalCoordinate b) => throw new NotImplementedException();
    public static Boolean operator !=(CylindricalCoordinate a, CylindricalCoordinate b) => a.NotEquals(b);
}
public readonly partial struct HorizontalCoordinate
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
    public Boolean Equals(HorizontalCoordinate b) => throw new NotImplementedException();
    public static Boolean operator ==(HorizontalCoordinate a, HorizontalCoordinate b) => a.Equals(b);
    public Boolean NotEquals(HorizontalCoordinate b) => throw new NotImplementedException();
    public static Boolean operator !=(HorizontalCoordinate a, HorizontalCoordinate b) => a.NotEquals(b);
}
public readonly partial struct GeoCoordinate
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
    public Boolean Equals(GeoCoordinate b) => throw new NotImplementedException();
    public static Boolean operator ==(GeoCoordinate a, GeoCoordinate b) => a.Equals(b);
    public Boolean NotEquals(GeoCoordinate b) => throw new NotImplementedException();
    public static Boolean operator !=(GeoCoordinate a, GeoCoordinate b) => a.NotEquals(b);
}
public readonly partial struct GeoCoordinateWithAltitude
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
    public Boolean Equals(GeoCoordinateWithAltitude b) => throw new NotImplementedException();
    public static Boolean operator ==(GeoCoordinateWithAltitude a, GeoCoordinateWithAltitude b) => a.Equals(b);
    public Boolean NotEquals(GeoCoordinateWithAltitude b) => throw new NotImplementedException();
    public static Boolean operator !=(GeoCoordinateWithAltitude a, GeoCoordinateWithAltitude b) => a.NotEquals(b);
}
public readonly partial struct Circle
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
    public Boolean Equals(Circle b) => throw new NotImplementedException();
    public static Boolean operator ==(Circle a, Circle b) => a.Equals(b);
    public Boolean NotEquals(Circle b) => throw new NotImplementedException();
    public static Boolean operator !=(Circle a, Circle b) => a.NotEquals(b);
}
public readonly partial struct Chord
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
    public Boolean Equals(Chord b) => throw new NotImplementedException();
    public static Boolean operator ==(Chord a, Chord b) => a.Equals(b);
    public Boolean NotEquals(Chord b) => throw new NotImplementedException();
    public static Boolean operator !=(Chord a, Chord b) => a.NotEquals(b);
}
public readonly partial struct Size2D
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
    public Boolean Equals(Size2D b) => throw new NotImplementedException();
    public static Boolean operator ==(Size2D a, Size2D b) => a.Equals(b);
    public Boolean NotEquals(Size2D b) => throw new NotImplementedException();
    public static Boolean operator !=(Size2D a, Size2D b) => a.NotEquals(b);
}
public readonly partial struct Size3D
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
    public Boolean Equals(Size3D b) => throw new NotImplementedException();
    public static Boolean operator ==(Size3D a, Size3D b) => a.Equals(b);
    public Boolean NotEquals(Size3D b) => throw new NotImplementedException();
    public static Boolean operator !=(Size3D a, Size3D b) => a.NotEquals(b);
}
public readonly partial struct Rectangle2D
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
    public Boolean Equals(Rectangle2D b) => throw new NotImplementedException();
    public static Boolean operator ==(Rectangle2D a, Rectangle2D b) => a.Equals(b);
    public Boolean NotEquals(Rectangle2D b) => throw new NotImplementedException();
    public static Boolean operator !=(Rectangle2D a, Rectangle2D b) => a.NotEquals(b);
}
public readonly partial struct Proportion
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
    public Proportion Zero => (Value.Zero);
    public Proportion One => (Value.One);
    public Proportion MinValue => (Value.MinValue);
    public Proportion MaxValue => (Value.MaxValue);
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
}
public readonly partial struct Fraction
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
    public Boolean Equals(Fraction b) => throw new NotImplementedException();
    public static Boolean operator ==(Fraction a, Fraction b) => a.Equals(b);
    public Boolean NotEquals(Fraction b) => throw new NotImplementedException();
    public static Boolean operator !=(Fraction a, Fraction b) => a.NotEquals(b);
}
public readonly partial struct Angle
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
    public Angle Lerp(Angle b, Number amount) => throw new NotImplementedException();
    public Number Unlerp(Angle a, Angle b) => throw new NotImplementedException();
}
public readonly partial struct Length
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
    public Length Multiply(Length other) => (Meters.Multiply(other.Meters));
    public static Length operator *(Length self, Length other) => self.Multiply(other);
    public Length Divide(Length other) => (Meters.Divide(other.Meters));
    public static Length operator /(Length self, Length other) => self.Divide(other);
    public Length Modulo(Length other) => (Meters.Modulo(other.Meters));
    public static Length operator %(Length self, Length other) => self.Modulo(other);
    public Length Add(Number other) => throw new NotImplementedException();
    public static Length operator +(Length self, Number other) => self.Add(other);
    public Length Subtract(Number other) => throw new NotImplementedException();
    public static Length operator -(Length self, Number other) => self.Subtract(other);
}
public readonly partial struct Mass
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
    public Mass Multiply(Mass other) => (Kilograms.Multiply(other.Kilograms));
    public static Mass operator *(Mass self, Mass other) => self.Multiply(other);
    public Mass Divide(Mass other) => (Kilograms.Divide(other.Kilograms));
    public static Mass operator /(Mass self, Mass other) => self.Divide(other);
    public Mass Modulo(Mass other) => (Kilograms.Modulo(other.Kilograms));
    public static Mass operator %(Mass self, Mass other) => self.Modulo(other);
    public Mass Add(Number other) => throw new NotImplementedException();
    public static Mass operator +(Mass self, Number other) => self.Add(other);
    public Mass Subtract(Number other) => throw new NotImplementedException();
    public static Mass operator -(Mass self, Number other) => self.Subtract(other);
}
public readonly partial struct Temperature
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
    public Temperature Multiply(Temperature other) => (Celsius.Multiply(other.Celsius));
    public static Temperature operator *(Temperature self, Temperature other) => self.Multiply(other);
    public Temperature Divide(Temperature other) => (Celsius.Divide(other.Celsius));
    public static Temperature operator /(Temperature self, Temperature other) => self.Divide(other);
    public Temperature Modulo(Temperature other) => (Celsius.Modulo(other.Celsius));
    public static Temperature operator %(Temperature self, Temperature other) => self.Modulo(other);
    public Temperature Add(Number other) => throw new NotImplementedException();
    public static Temperature operator +(Temperature self, Number other) => self.Add(other);
    public Temperature Subtract(Number other) => throw new NotImplementedException();
    public static Temperature operator -(Temperature self, Number other) => self.Subtract(other);
}
public readonly partial struct Time
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
    public Time Multiply(Time other) => (Seconds.Multiply(other.Seconds));
    public static Time operator *(Time self, Time other) => self.Multiply(other);
    public Time Divide(Time other) => (Seconds.Divide(other.Seconds));
    public static Time operator /(Time self, Time other) => self.Divide(other);
    public Time Modulo(Time other) => (Seconds.Modulo(other.Seconds));
    public static Time operator %(Time self, Time other) => self.Modulo(other);
    public Time Add(Number other) => throw new NotImplementedException();
    public static Time operator +(Time self, Number other) => self.Add(other);
    public Time Subtract(Number other) => throw new NotImplementedException();
    public static Time operator -(Time self, Number other) => self.Subtract(other);
}
public readonly partial struct TimeRange
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
}
public readonly partial struct DateTime
{
    public readonly Integer Year;
    public readonly Integer Month;
    public readonly Integer TimeZoneOffset;
    public readonly Integer Day;
    public readonly Integer Minute;
    public readonly Integer Second;
    public readonly Number Milliseconds;
    public DateTime WithYear(Integer year) => (year, Month, TimeZoneOffset, Day, Minute, Second, Milliseconds);
    public DateTime WithMonth(Integer month) => (Year, month, TimeZoneOffset, Day, Minute, Second, Milliseconds);
    public DateTime WithTimeZoneOffset(Integer timeZoneOffset) => (Year, Month, timeZoneOffset, Day, Minute, Second, Milliseconds);
    public DateTime WithDay(Integer day) => (Year, Month, TimeZoneOffset, day, Minute, Second, Milliseconds);
    public DateTime WithMinute(Integer minute) => (Year, Month, TimeZoneOffset, Day, minute, Second, Milliseconds);
    public DateTime WithSecond(Integer second) => (Year, Month, TimeZoneOffset, Day, Minute, second, Milliseconds);
    public DateTime WithMilliseconds(Number milliseconds) => (Year, Month, TimeZoneOffset, Day, Minute, Second, milliseconds);
    public DateTime(Integer year, Integer month, Integer timeZoneOffset, Integer day, Integer minute, Integer second, Number milliseconds) => (Year, Month, TimeZoneOffset, Day, Minute, Second, Milliseconds) = (year, month, timeZoneOffset, day, minute, second, milliseconds);
    public static DateTime Default = new DateTime();
    public static DateTime New(Integer year, Integer month, Integer timeZoneOffset, Integer day, Integer minute, Integer second, Number milliseconds) => new DateTime(year, month, timeZoneOffset, day, minute, second, milliseconds);
    public static implicit operator (Integer, Integer, Integer, Integer, Integer, Integer, Number)(DateTime self) => (self.Year, self.Month, self.TimeZoneOffset, self.Day, self.Minute, self.Second, self.Milliseconds);
    public static implicit operator DateTime((Integer, Integer, Integer, Integer, Integer, Integer, Number) value) => new DateTime(value.Item1, value.Item2, value.Item3, value.Item4, value.Item5, value.Item6, value.Item7);
    public void Deconstruct(out Integer year, out Integer month, out Integer timeZoneOffset, out Integer day, out Integer minute, out Integer second, out Number milliseconds) { year = Year; month = Month; timeZoneOffset = TimeZoneOffset; day = Day; minute = Minute; second = Second; milliseconds = Milliseconds; }
    public String TypeName => "DateTime";
    public Array<String> FieldNames => (Array1<String>)new[] { (String)"Year", (String)"Month", (String)"TimeZoneOffset", (String)"Day", (String)"Minute", (String)"Second", (String)"Milliseconds" };
    public Array<Dynamic> FieldValues => (Array1<Dynamic>)new[] { new Dynamic(Year), new Dynamic(Month), new Dynamic(TimeZoneOffset), new Dynamic(Day), new Dynamic(Minute), new Dynamic(Second), new Dynamic(Milliseconds) };
    // Unimplemented concept functions
    public DateTime Lerp(DateTime b, Number amount) => throw new NotImplementedException();
    public Number Unlerp(DateTime a, DateTime b) => throw new NotImplementedException();
}
public readonly partial struct AnglePair
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
}
public readonly partial struct Ring
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
    public Boolean Equals(Ring b) => throw new NotImplementedException();
    public static Boolean operator ==(Ring a, Ring b) => a.Equals(b);
    public Boolean NotEquals(Ring b) => throw new NotImplementedException();
    public static Boolean operator !=(Ring a, Ring b) => a.NotEquals(b);
}
public readonly partial struct Arc
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
    public Boolean Equals(Arc b) => throw new NotImplementedException();
    public static Boolean operator ==(Arc a, Arc b) => a.Equals(b);
    public Boolean NotEquals(Arc b) => throw new NotImplementedException();
    public static Boolean operator !=(Arc a, Arc b) => a.NotEquals(b);
}
public readonly partial struct RealInterval
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
}
public readonly partial struct Capsule
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
    public Boolean Equals(Capsule b) => throw new NotImplementedException();
    public static Boolean operator ==(Capsule a, Capsule b) => a.Equals(b);
    public Boolean NotEquals(Capsule b) => throw new NotImplementedException();
    public static Boolean operator !=(Capsule a, Capsule b) => a.NotEquals(b);
}
public readonly partial struct Matrix3D
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
    public Boolean Equals(Matrix3D b) => throw new NotImplementedException();
    public static Boolean operator ==(Matrix3D a, Matrix3D b) => a.Equals(b);
    public Boolean NotEquals(Matrix3D b) => throw new NotImplementedException();
    public static Boolean operator !=(Matrix3D a, Matrix3D b) => a.NotEquals(b);
}
public readonly partial struct Cylinder
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
    public Boolean Equals(Cylinder b) => throw new NotImplementedException();
    public static Boolean operator ==(Cylinder a, Cylinder b) => a.Equals(b);
    public Boolean NotEquals(Cylinder b) => throw new NotImplementedException();
    public static Boolean operator !=(Cylinder a, Cylinder b) => a.NotEquals(b);
}
public readonly partial struct Cone
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
    public Boolean Equals(Cone b) => throw new NotImplementedException();
    public static Boolean operator ==(Cone a, Cone b) => a.Equals(b);
    public Boolean NotEquals(Cone b) => throw new NotImplementedException();
    public static Boolean operator !=(Cone a, Cone b) => a.NotEquals(b);
}
public readonly partial struct Tube
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
    public Boolean Equals(Tube b) => throw new NotImplementedException();
    public static Boolean operator ==(Tube a, Tube b) => a.Equals(b);
    public Boolean NotEquals(Tube b) => throw new NotImplementedException();
    public static Boolean operator !=(Tube a, Tube b) => a.NotEquals(b);
}
public readonly partial struct ConeSegment
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
    public Boolean Equals(ConeSegment b) => throw new NotImplementedException();
    public static Boolean operator ==(ConeSegment a, ConeSegment b) => a.Equals(b);
    public Boolean NotEquals(ConeSegment b) => throw new NotImplementedException();
    public static Boolean operator !=(ConeSegment a, ConeSegment b) => a.NotEquals(b);
}
public readonly partial struct Box2D
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
    public Boolean Equals(Box2D b) => throw new NotImplementedException();
    public static Boolean operator ==(Box2D a, Box2D b) => a.Equals(b);
    public Boolean NotEquals(Box2D b) => throw new NotImplementedException();
    public static Boolean operator !=(Box2D a, Box2D b) => a.NotEquals(b);
}
public readonly partial struct Box3D
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
    public Boolean Equals(Box3D b) => throw new NotImplementedException();
    public static Boolean operator ==(Box3D a, Box3D b) => a.Equals(b);
    public Boolean NotEquals(Box3D b) => throw new NotImplementedException();
    public static Boolean operator !=(Box3D a, Box3D b) => a.NotEquals(b);
}
public readonly partial struct UV
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
    public Number Unlerp(UV a, UV b) => throw new NotImplementedException();
    public Integer Compare(UV y) => throw new NotImplementedException();
    public UV Zero => (U.Zero, V.Zero);
    public UV One => (U.One, V.One);
    public UV MinValue => (U.MinValue, V.MinValue);
    public UV MaxValue => (U.MaxValue, V.MaxValue);
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
    public Integer Count => throw new NotImplementedException();
    public Number At(Integer n) => throw new NotImplementedException();
    public Number this[Integer n] => At(n);
}
public readonly partial struct UVW
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
    public Number Unlerp(UVW a, UVW b) => throw new NotImplementedException();
    public Integer Compare(UVW y) => throw new NotImplementedException();
    public UVW Zero => (U.Zero, V.Zero, W.Zero);
    public UVW One => (U.One, V.One, W.One);
    public UVW MinValue => (U.MinValue, V.MinValue, W.MinValue);
    public UVW MaxValue => (U.MaxValue, V.MaxValue, W.MaxValue);
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
    public Integer Count => throw new NotImplementedException();
    public Number At(Integer n) => throw new NotImplementedException();
    public Number this[Integer n] => At(n);
}
public readonly partial struct CubicBezier2D
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
    public Boolean Equals(CubicBezier2D b) => throw new NotImplementedException();
    public static Boolean operator ==(CubicBezier2D a, CubicBezier2D b) => a.Equals(b);
    public Boolean NotEquals(CubicBezier2D b) => throw new NotImplementedException();
    public static Boolean operator !=(CubicBezier2D a, CubicBezier2D b) => a.NotEquals(b);
}
public readonly partial struct CubicBezier3D
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
    public Boolean Equals(CubicBezier3D b) => throw new NotImplementedException();
    public static Boolean operator ==(CubicBezier3D a, CubicBezier3D b) => a.Equals(b);
    public Boolean NotEquals(CubicBezier3D b) => throw new NotImplementedException();
    public static Boolean operator !=(CubicBezier3D a, CubicBezier3D b) => a.NotEquals(b);
}
public readonly partial struct QuadraticBezier2D
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
    public Boolean Equals(QuadraticBezier2D b) => throw new NotImplementedException();
    public static Boolean operator ==(QuadraticBezier2D a, QuadraticBezier2D b) => a.Equals(b);
    public Boolean NotEquals(QuadraticBezier2D b) => throw new NotImplementedException();
    public static Boolean operator !=(QuadraticBezier2D a, QuadraticBezier2D b) => a.NotEquals(b);
}
public readonly partial struct QuadraticBezier3D
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
    public Boolean Equals(QuadraticBezier3D b) => throw new NotImplementedException();
    public static Boolean operator ==(QuadraticBezier3D a, QuadraticBezier3D b) => a.Equals(b);
    public Boolean NotEquals(QuadraticBezier3D b) => throw new NotImplementedException();
    public static Boolean operator !=(QuadraticBezier3D a, QuadraticBezier3D b) => a.NotEquals(b);
}
public readonly partial struct Area
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
    public Area Multiply(Area other) => (MetersSquared.Multiply(other.MetersSquared));
    public static Area operator *(Area self, Area other) => self.Multiply(other);
    public Area Divide(Area other) => (MetersSquared.Divide(other.MetersSquared));
    public static Area operator /(Area self, Area other) => self.Divide(other);
    public Area Modulo(Area other) => (MetersSquared.Modulo(other.MetersSquared));
    public static Area operator %(Area self, Area other) => self.Modulo(other);
    public Area Add(Number other) => throw new NotImplementedException();
    public static Area operator +(Area self, Number other) => self.Add(other);
    public Area Subtract(Number other) => throw new NotImplementedException();
    public static Area operator -(Area self, Number other) => self.Subtract(other);
}
public readonly partial struct Volume
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
    public Volume Multiply(Volume other) => (MetersCubed.Multiply(other.MetersCubed));
    public static Volume operator *(Volume self, Volume other) => self.Multiply(other);
    public Volume Divide(Volume other) => (MetersCubed.Divide(other.MetersCubed));
    public static Volume operator /(Volume self, Volume other) => self.Divide(other);
    public Volume Modulo(Volume other) => (MetersCubed.Modulo(other.MetersCubed));
    public static Volume operator %(Volume self, Volume other) => self.Modulo(other);
    public Volume Add(Number other) => throw new NotImplementedException();
    public static Volume operator +(Volume self, Number other) => self.Add(other);
    public Volume Subtract(Number other) => throw new NotImplementedException();
    public static Volume operator -(Volume self, Number other) => self.Subtract(other);
}
public readonly partial struct Velocity
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
    public Velocity Multiply(Velocity other) => (MetersPerSecond.Multiply(other.MetersPerSecond));
    public static Velocity operator *(Velocity self, Velocity other) => self.Multiply(other);
    public Velocity Divide(Velocity other) => (MetersPerSecond.Divide(other.MetersPerSecond));
    public static Velocity operator /(Velocity self, Velocity other) => self.Divide(other);
    public Velocity Modulo(Velocity other) => (MetersPerSecond.Modulo(other.MetersPerSecond));
    public static Velocity operator %(Velocity self, Velocity other) => self.Modulo(other);
    public Velocity Add(Number other) => throw new NotImplementedException();
    public static Velocity operator +(Velocity self, Number other) => self.Add(other);
    public Velocity Subtract(Number other) => throw new NotImplementedException();
    public static Velocity operator -(Velocity self, Number other) => self.Subtract(other);
}
public readonly partial struct Acceleration
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
    public Acceleration Multiply(Acceleration other) => (MetersPerSecondSquared.Multiply(other.MetersPerSecondSquared));
    public static Acceleration operator *(Acceleration self, Acceleration other) => self.Multiply(other);
    public Acceleration Divide(Acceleration other) => (MetersPerSecondSquared.Divide(other.MetersPerSecondSquared));
    public static Acceleration operator /(Acceleration self, Acceleration other) => self.Divide(other);
    public Acceleration Modulo(Acceleration other) => (MetersPerSecondSquared.Modulo(other.MetersPerSecondSquared));
    public static Acceleration operator %(Acceleration self, Acceleration other) => self.Modulo(other);
    public Acceleration Add(Number other) => throw new NotImplementedException();
    public static Acceleration operator +(Acceleration self, Number other) => self.Add(other);
    public Acceleration Subtract(Number other) => throw new NotImplementedException();
    public static Acceleration operator -(Acceleration self, Number other) => self.Subtract(other);
}
public readonly partial struct Force
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
    public Force Multiply(Force other) => (Newtons.Multiply(other.Newtons));
    public static Force operator *(Force self, Force other) => self.Multiply(other);
    public Force Divide(Force other) => (Newtons.Divide(other.Newtons));
    public static Force operator /(Force self, Force other) => self.Divide(other);
    public Force Modulo(Force other) => (Newtons.Modulo(other.Newtons));
    public static Force operator %(Force self, Force other) => self.Modulo(other);
    public Force Add(Number other) => throw new NotImplementedException();
    public static Force operator +(Force self, Number other) => self.Add(other);
    public Force Subtract(Number other) => throw new NotImplementedException();
    public static Force operator -(Force self, Number other) => self.Subtract(other);
}
public readonly partial struct Pressure
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
    public Pressure Multiply(Pressure other) => (Pascals.Multiply(other.Pascals));
    public static Pressure operator *(Pressure self, Pressure other) => self.Multiply(other);
    public Pressure Divide(Pressure other) => (Pascals.Divide(other.Pascals));
    public static Pressure operator /(Pressure self, Pressure other) => self.Divide(other);
    public Pressure Modulo(Pressure other) => (Pascals.Modulo(other.Pascals));
    public static Pressure operator %(Pressure self, Pressure other) => self.Modulo(other);
    public Pressure Add(Number other) => throw new NotImplementedException();
    public static Pressure operator +(Pressure self, Number other) => self.Add(other);
    public Pressure Subtract(Number other) => throw new NotImplementedException();
    public static Pressure operator -(Pressure self, Number other) => self.Subtract(other);
}
public readonly partial struct Energy
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
    public Energy Multiply(Energy other) => (Joules.Multiply(other.Joules));
    public static Energy operator *(Energy self, Energy other) => self.Multiply(other);
    public Energy Divide(Energy other) => (Joules.Divide(other.Joules));
    public static Energy operator /(Energy self, Energy other) => self.Divide(other);
    public Energy Modulo(Energy other) => (Joules.Modulo(other.Joules));
    public static Energy operator %(Energy self, Energy other) => self.Modulo(other);
    public Energy Add(Number other) => throw new NotImplementedException();
    public static Energy operator +(Energy self, Number other) => self.Add(other);
    public Energy Subtract(Number other) => throw new NotImplementedException();
    public static Energy operator -(Energy self, Number other) => self.Subtract(other);
}
public readonly partial struct Memory
{
    public readonly Count Bytes;
    public Memory WithBytes(Count bytes) => (bytes);
    public Memory(Count bytes) => (Bytes) = (bytes);
    public static Memory Default = new Memory();
    public static Memory New(Count bytes) => new Memory(bytes);
    public static implicit operator Count(Memory self) => self.Bytes;
    public static implicit operator Memory(Count value) => new Memory(value);
    public String TypeName => "Memory";
    public Array<String> FieldNames => (Array1<String>)new[] { (String)"Bytes" };
    public Array<Dynamic> FieldValues => (Array1<Dynamic>)new[] { new Dynamic(Bytes) };
    // Unimplemented concept functions
    public Number Value => throw new NotImplementedException();
    public Memory Lerp(Memory b, Number amount) => throw new NotImplementedException();
    public Number Unlerp(Memory a, Memory b) => throw new NotImplementedException();
    public Number Magnitude => throw new NotImplementedException();
    public Integer Compare(Memory y) => throw new NotImplementedException();
    public Memory Multiply(Memory other) => (Bytes.Multiply(other.Bytes));
    public static Memory operator *(Memory self, Memory other) => self.Multiply(other);
    public Memory Divide(Memory other) => (Bytes.Divide(other.Bytes));
    public static Memory operator /(Memory self, Memory other) => self.Divide(other);
    public Memory Modulo(Memory other) => (Bytes.Modulo(other.Bytes));
    public static Memory operator %(Memory self, Memory other) => self.Modulo(other);
    public Memory Add(Number other) => throw new NotImplementedException();
    public static Memory operator +(Memory self, Number other) => self.Add(other);
    public Memory Subtract(Number other) => throw new NotImplementedException();
    public static Memory operator -(Memory self, Number other) => self.Subtract(other);
}
public readonly partial struct Frequency
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
    public Frequency Multiply(Frequency other) => (Hertz.Multiply(other.Hertz));
    public static Frequency operator *(Frequency self, Frequency other) => self.Multiply(other);
    public Frequency Divide(Frequency other) => (Hertz.Divide(other.Hertz));
    public static Frequency operator /(Frequency self, Frequency other) => self.Divide(other);
    public Frequency Modulo(Frequency other) => (Hertz.Modulo(other.Hertz));
    public static Frequency operator %(Frequency self, Frequency other) => self.Modulo(other);
    public Frequency Add(Number other) => throw new NotImplementedException();
    public static Frequency operator +(Frequency self, Number other) => self.Add(other);
    public Frequency Subtract(Number other) => throw new NotImplementedException();
    public static Frequency operator -(Frequency self, Number other) => self.Subtract(other);
}
public readonly partial struct Loudness
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
    public Loudness Multiply(Loudness other) => (Decibels.Multiply(other.Decibels));
    public static Loudness operator *(Loudness self, Loudness other) => self.Multiply(other);
    public Loudness Divide(Loudness other) => (Decibels.Divide(other.Decibels));
    public static Loudness operator /(Loudness self, Loudness other) => self.Divide(other);
    public Loudness Modulo(Loudness other) => (Decibels.Modulo(other.Decibels));
    public static Loudness operator %(Loudness self, Loudness other) => self.Modulo(other);
    public Loudness Add(Number other) => throw new NotImplementedException();
    public static Loudness operator +(Loudness self, Number other) => self.Add(other);
    public Loudness Subtract(Number other) => throw new NotImplementedException();
    public static Loudness operator -(Loudness self, Number other) => self.Subtract(other);
}
public readonly partial struct LuminousIntensity
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
    public LuminousIntensity Multiply(LuminousIntensity other) => (Candelas.Multiply(other.Candelas));
    public static LuminousIntensity operator *(LuminousIntensity self, LuminousIntensity other) => self.Multiply(other);
    public LuminousIntensity Divide(LuminousIntensity other) => (Candelas.Divide(other.Candelas));
    public static LuminousIntensity operator /(LuminousIntensity self, LuminousIntensity other) => self.Divide(other);
    public LuminousIntensity Modulo(LuminousIntensity other) => (Candelas.Modulo(other.Candelas));
    public static LuminousIntensity operator %(LuminousIntensity self, LuminousIntensity other) => self.Modulo(other);
    public LuminousIntensity Add(Number other) => throw new NotImplementedException();
    public static LuminousIntensity operator +(LuminousIntensity self, Number other) => self.Add(other);
    public LuminousIntensity Subtract(Number other) => throw new NotImplementedException();
    public static LuminousIntensity operator -(LuminousIntensity self, Number other) => self.Subtract(other);
}
public readonly partial struct ElectricPotential
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
    public ElectricPotential Multiply(ElectricPotential other) => (Volts.Multiply(other.Volts));
    public static ElectricPotential operator *(ElectricPotential self, ElectricPotential other) => self.Multiply(other);
    public ElectricPotential Divide(ElectricPotential other) => (Volts.Divide(other.Volts));
    public static ElectricPotential operator /(ElectricPotential self, ElectricPotential other) => self.Divide(other);
    public ElectricPotential Modulo(ElectricPotential other) => (Volts.Modulo(other.Volts));
    public static ElectricPotential operator %(ElectricPotential self, ElectricPotential other) => self.Modulo(other);
    public ElectricPotential Add(Number other) => throw new NotImplementedException();
    public static ElectricPotential operator +(ElectricPotential self, Number other) => self.Add(other);
    public ElectricPotential Subtract(Number other) => throw new NotImplementedException();
    public static ElectricPotential operator -(ElectricPotential self, Number other) => self.Subtract(other);
}
public readonly partial struct ElectricCharge
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
    public ElectricCharge Multiply(ElectricCharge other) => (Columbs.Multiply(other.Columbs));
    public static ElectricCharge operator *(ElectricCharge self, ElectricCharge other) => self.Multiply(other);
    public ElectricCharge Divide(ElectricCharge other) => (Columbs.Divide(other.Columbs));
    public static ElectricCharge operator /(ElectricCharge self, ElectricCharge other) => self.Divide(other);
    public ElectricCharge Modulo(ElectricCharge other) => (Columbs.Modulo(other.Columbs));
    public static ElectricCharge operator %(ElectricCharge self, ElectricCharge other) => self.Modulo(other);
    public ElectricCharge Add(Number other) => throw new NotImplementedException();
    public static ElectricCharge operator +(ElectricCharge self, Number other) => self.Add(other);
    public ElectricCharge Subtract(Number other) => throw new NotImplementedException();
    public static ElectricCharge operator -(ElectricCharge self, Number other) => self.Subtract(other);
}
public readonly partial struct ElectricCurrent
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
    public ElectricCurrent Multiply(ElectricCurrent other) => (Amperes.Multiply(other.Amperes));
    public static ElectricCurrent operator *(ElectricCurrent self, ElectricCurrent other) => self.Multiply(other);
    public ElectricCurrent Divide(ElectricCurrent other) => (Amperes.Divide(other.Amperes));
    public static ElectricCurrent operator /(ElectricCurrent self, ElectricCurrent other) => self.Divide(other);
    public ElectricCurrent Modulo(ElectricCurrent other) => (Amperes.Modulo(other.Amperes));
    public static ElectricCurrent operator %(ElectricCurrent self, ElectricCurrent other) => self.Modulo(other);
    public ElectricCurrent Add(Number other) => throw new NotImplementedException();
    public static ElectricCurrent operator +(ElectricCurrent self, Number other) => self.Add(other);
    public ElectricCurrent Subtract(Number other) => throw new NotImplementedException();
    public static ElectricCurrent operator -(ElectricCurrent self, Number other) => self.Subtract(other);
}
public readonly partial struct ElectricResistance
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
    public ElectricResistance Multiply(ElectricResistance other) => (Ohms.Multiply(other.Ohms));
    public static ElectricResistance operator *(ElectricResistance self, ElectricResistance other) => self.Multiply(other);
    public ElectricResistance Divide(ElectricResistance other) => (Ohms.Divide(other.Ohms));
    public static ElectricResistance operator /(ElectricResistance self, ElectricResistance other) => self.Divide(other);
    public ElectricResistance Modulo(ElectricResistance other) => (Ohms.Modulo(other.Ohms));
    public static ElectricResistance operator %(ElectricResistance self, ElectricResistance other) => self.Modulo(other);
    public ElectricResistance Add(Number other) => throw new NotImplementedException();
    public static ElectricResistance operator +(ElectricResistance self, Number other) => self.Add(other);
    public ElectricResistance Subtract(Number other) => throw new NotImplementedException();
    public static ElectricResistance operator -(ElectricResistance self, Number other) => self.Subtract(other);
}
public readonly partial struct Power
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
    public Power Multiply(Power other) => (Watts.Multiply(other.Watts));
    public static Power operator *(Power self, Power other) => self.Multiply(other);
    public Power Divide(Power other) => (Watts.Divide(other.Watts));
    public static Power operator /(Power self, Power other) => self.Divide(other);
    public Power Modulo(Power other) => (Watts.Modulo(other.Watts));
    public static Power operator %(Power self, Power other) => self.Modulo(other);
    public Power Add(Number other) => throw new NotImplementedException();
    public static Power operator +(Power self, Number other) => self.Add(other);
    public Power Subtract(Number other) => throw new NotImplementedException();
    public static Power operator -(Power self, Number other) => self.Subtract(other);
}
public readonly partial struct Density
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
    public Density Multiply(Density other) => (KilogramsPerMeterCubed.Multiply(other.KilogramsPerMeterCubed));
    public static Density operator *(Density self, Density other) => self.Multiply(other);
    public Density Divide(Density other) => (KilogramsPerMeterCubed.Divide(other.KilogramsPerMeterCubed));
    public static Density operator /(Density self, Density other) => self.Divide(other);
    public Density Modulo(Density other) => (KilogramsPerMeterCubed.Modulo(other.KilogramsPerMeterCubed));
    public static Density operator %(Density self, Density other) => self.Modulo(other);
    public Density Add(Number other) => throw new NotImplementedException();
    public static Density operator +(Density self, Number other) => self.Add(other);
    public Density Subtract(Number other) => throw new NotImplementedException();
    public static Density operator -(Density self, Number other) => self.Subtract(other);
}
public readonly partial struct NormalDistribution
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
    public Boolean Equals(NormalDistribution b) => throw new NotImplementedException();
    public static Boolean operator ==(NormalDistribution a, NormalDistribution b) => a.Equals(b);
    public Boolean NotEquals(NormalDistribution b) => throw new NotImplementedException();
    public static Boolean operator !=(NormalDistribution a, NormalDistribution b) => a.NotEquals(b);
}
public readonly partial struct PoissonDistribution
{
    public readonly Number Expected;
    public readonly Count Occurrences;
    public PoissonDistribution WithExpected(Number expected) => (expected, Occurrences);
    public PoissonDistribution WithOccurrences(Count occurrences) => (Expected, occurrences);
    public PoissonDistribution(Number expected, Count occurrences) => (Expected, Occurrences) = (expected, occurrences);
    public static PoissonDistribution Default = new PoissonDistribution();
    public static PoissonDistribution New(Number expected, Count occurrences) => new PoissonDistribution(expected, occurrences);
    public static implicit operator (Number, Count)(PoissonDistribution self) => (self.Expected, self.Occurrences);
    public static implicit operator PoissonDistribution((Number, Count) value) => new PoissonDistribution(value.Item1, value.Item2);
    public void Deconstruct(out Number expected, out Count occurrences) { expected = Expected; occurrences = Occurrences; }
    public String TypeName => "PoissonDistribution";
    public Array<String> FieldNames => (Array1<String>)new[] { (String)"Expected", (String)"Occurrences" };
    public Array<Dynamic> FieldValues => (Array1<Dynamic>)new[] { new Dynamic(Expected), new Dynamic(Occurrences) };
    // Unimplemented concept functions
    public Boolean Equals(PoissonDistribution b) => throw new NotImplementedException();
    public static Boolean operator ==(PoissonDistribution a, PoissonDistribution b) => a.Equals(b);
    public Boolean NotEquals(PoissonDistribution b) => throw new NotImplementedException();
    public static Boolean operator !=(PoissonDistribution a, PoissonDistribution b) => a.NotEquals(b);
}
public readonly partial struct BernoulliDistribution
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
    public Boolean Equals(BernoulliDistribution b) => throw new NotImplementedException();
    public static Boolean operator ==(BernoulliDistribution a, BernoulliDistribution b) => a.Equals(b);
    public Boolean NotEquals(BernoulliDistribution b) => throw new NotImplementedException();
    public static Boolean operator !=(BernoulliDistribution a, BernoulliDistribution b) => a.NotEquals(b);
}
public readonly partial struct Probability
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
    public Probability Zero => (Value.Zero);
    public Probability One => (Value.One);
    public Probability MinValue => (Value.MinValue);
    public Probability MaxValue => (Value.MaxValue);
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
}
public readonly partial struct BinomialDistribution
{
    public readonly Count Trials;
    public readonly Probability P;
    public BinomialDistribution WithTrials(Count trials) => (trials, P);
    public BinomialDistribution WithP(Probability p) => (Trials, p);
    public BinomialDistribution(Count trials, Probability p) => (Trials, P) = (trials, p);
    public static BinomialDistribution Default = new BinomialDistribution();
    public static BinomialDistribution New(Count trials, Probability p) => new BinomialDistribution(trials, p);
    public static implicit operator (Count, Probability)(BinomialDistribution self) => (self.Trials, self.P);
    public static implicit operator BinomialDistribution((Count, Probability) value) => new BinomialDistribution(value.Item1, value.Item2);
    public void Deconstruct(out Count trials, out Probability p) { trials = Trials; p = P; }
    public String TypeName => "BinomialDistribution";
    public Array<String> FieldNames => (Array1<String>)new[] { (String)"Trials", (String)"P" };
    public Array<Dynamic> FieldValues => (Array1<Dynamic>)new[] { new Dynamic(Trials), new Dynamic(P) };
    // Unimplemented concept functions
    public Boolean Equals(BinomialDistribution b) => throw new NotImplementedException();
    public static Boolean operator ==(BinomialDistribution a, BinomialDistribution b) => a.Equals(b);
    public Boolean NotEquals(BinomialDistribution b) => throw new NotImplementedException();
    public static Boolean operator !=(BinomialDistribution a, BinomialDistribution b) => a.NotEquals(b);
}
