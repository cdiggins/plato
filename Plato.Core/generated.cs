using System;
namespace Plato {
public static partial class Intrinsics {
public static float Add(this float a, float b) => a + b;
public static float Subtract(this float a, float b) => a - b;
public static float Multiply(this float a, float b) => a * b;
public static float Divide(this float a, float b) => a / b;
public static float Negate(this float a) => - a;
public static bool Equals(this float a, float b) => a == b;
public static bool NotEquals(this float a, float b) => a != b;
public static float Default(this float _) => default(float);
public static float Zero(this float _) => (float)0;
public static float One(this float _) => (float)1;
public static float MinValue(this float _) => float.MinValue;
public static float MaxValue(this float _) => float.MaxValue;
}
public static partial class Intrinsics {
public static double Add(this double a, double b) => a + b;
public static double Subtract(this double a, double b) => a - b;
public static double Multiply(this double a, double b) => a * b;
public static double Divide(this double a, double b) => a / b;
public static double Negate(this double a) => - a;
public static bool Equals(this double a, double b) => a == b;
public static bool NotEquals(this double a, double b) => a != b;
public static double Default(this double _) => default(double);
public static double Zero(this double _) => (double)0;
public static double One(this double _) => (double)1;
public static double MinValue(this double _) => double.MinValue;
public static double MaxValue(this double _) => double.MaxValue;
}
public static partial class Intrinsics {
public static int Add(this int a, int b) => a + b;
public static int Subtract(this int a, int b) => a - b;
public static int Multiply(this int a, int b) => a * b;
public static int Divide(this int a, int b) => a / b;
public static int Negate(this int a) => - a;
public static bool Equals(this int a, int b) => a == b;
public static bool NotEquals(this int a, int b) => a != b;
public static int Default(this int _) => default(int);
public static int Zero(this int _) => (int)0;
public static int One(this int _) => (int)1;
public static int MinValue(this int _) => int.MinValue;
public static int MaxValue(this int _) => int.MaxValue;
}
public static partial class Intrinsics {
public static long Add(this long a, long b) => a + b;
public static long Subtract(this long a, long b) => a - b;
public static long Multiply(this long a, long b) => a * b;
public static long Divide(this long a, long b) => a / b;
public static long Negate(this long a) => - a;
public static bool Equals(this long a, long b) => a == b;
public static bool NotEquals(this long a, long b) => a != b;
public static long Default(this long _) => default(long);
public static long Zero(this long _) => (long)0;
public static long One(this long _) => (long)1;
public static long MinValue(this long _) => long.MinValue;
public static long MaxValue(this long _) => long.MaxValue;
}
public static partial class Intrinsics {
public static decimal Add(this decimal a, decimal b) => a + b;
public static decimal Subtract(this decimal a, decimal b) => a - b;
public static decimal Multiply(this decimal a, decimal b) => a * b;
public static decimal Divide(this decimal a, decimal b) => a / b;
public static decimal Negate(this decimal a) => - a;
public static bool Equals(this decimal a, decimal b) => a == b;
public static bool NotEquals(this decimal a, decimal b) => a != b;
public static decimal Default(this decimal _) => default(decimal);
public static decimal Zero(this decimal _) => (decimal)0;
public static decimal One(this decimal _) => (decimal)1;
public static decimal MinValue(this decimal _) => decimal.MinValue;
public static decimal MaxValue(this decimal _) => decimal.MaxValue;
}
public static partial class Intrinsics {
public static byte Default(this byte _) => default(byte);
public static byte Zero(this byte _) => (byte)0;
public static byte One(this byte _) => (byte)1;
public static byte MinValue(this byte _) => byte.MinValue;
public static byte MaxValue(this byte _) => byte.MaxValue;
}
public partial struct Vector2
{
public Vector2(Single x, Single y) => (X, Y) = (x, y);
public static implicit operator Vector2((Single X, Single Y) tuple) => new Vector2(tuple.X, tuple.Y);
public static implicit operator (Single X, Single Y)(Vector2 self) => (self.X, self.Y);
public void Deconstruct(out Single x, out Single y) => (x, y) = (X, Y);
public override string ToString() => $"{{ \"X\" : { X }, \"Y\" : { Y } }}";
public override bool Equals(object other) => other is Vector2 typedOther && this == typedOther;
public override int GetHashCode() => (X, Y).GetHashCode();
public static readonly Vector2 Default = default;
public static Vector2 Zero = new Vector2(Default.X.Zero(),Default.Y.Zero());
public static Vector2 One = new Vector2(Default.X.One(),Default.Y.One());
public static Vector2 MinValue = new Vector2(Default.X.MinValue(),Default.Y.MinValue());
public static Vector2 MaxValue = new Vector2(Default.X.MaxValue(),Default.Y.MaxValue());
public static bool operator ==(Vector2 a, Vector2 b) => (a.X == b.X) && (a.Y == b.Y);
public static bool operator !=(Vector2 a, Vector2 b) => (a.X != b.X) || (a.Y != b.Y);
public Vector2 WithX(Single value) => new Vector2(value, Y);
public Vector2 WithY(Single value) => new Vector2(X, value);
public static Vector2 operator +(Vector2 a, Vector2 b) => new Vector2(a.X + b.X, a.Y + b.Y);
public static Vector2 operator -(Vector2 a, Vector2 b) => new Vector2(a.X - b.X, a.Y - b.Y);
public static Vector2 operator *(Vector2 a, Vector2 b) => new Vector2(a.X * b.X, a.Y * b.Y);
public static Vector2 operator /(Vector2 a, Vector2 b) => new Vector2(a.X / b.X, a.Y / b.Y);
public static Vector2 operator -(Vector2 a) => new Vector2(- a.X, - a.Y);
public static Vector2 operator *(Vector2 self, Single scalar) => new Vector2(self.X * scalar, self.Y * scalar);
public static Vector2 operator /(Vector2 self, Single scalar) => new Vector2(self.X / scalar, self.Y / scalar);
public int Count => 2;
public Single this[int index] { get { switch (index) {
case 0: return X;
case 1: return Y;
default: throw new System.ArgumentOutOfRangeException(nameof(index));
} } }
}
public static partial class Intrinsics {
public static Vector2 Add(this Vector2 a, Vector2 b) => a + b;
public static Vector2 Subtract(this Vector2 a, Vector2 b) => a - b;
public static Vector2 Multiply(this Vector2 a, Vector2 b) => a * b;
public static Vector2 Divide(this Vector2 a, Vector2 b) => a / b;
public static Vector2 Negate(this Vector2 a) => - a;
public static bool Equals(this Vector2 a, Vector2 b) => a == b;
public static bool NotEquals(this Vector2 a, Vector2 b) => a != b;
public static Vector2 Default(this Vector2 _) => default(Vector2);
public static Vector2 Zero(this Vector2 _) => Vector2.Zero;
public static Vector2 One(this Vector2 _) => Vector2.One;
public static Vector2 MinValue(this Vector2 _) => Vector2.MinValue;
public static Vector2 MaxValue(this Vector2 _) => Vector2.MaxValue;
}
public partial struct Vector3
{
public Vector3(Single x, Single y, Single z) => (X, Y, Z) = (x, y, z);
public static implicit operator Vector3((Single X, Single Y, Single Z) tuple) => new Vector3(tuple.X, tuple.Y, tuple.Z);
public static implicit operator (Single X, Single Y, Single Z)(Vector3 self) => (self.X, self.Y, self.Z);
public void Deconstruct(out Single x, out Single y, out Single z) => (x, y, z) = (X, Y, Z);
public override string ToString() => $"{{ \"X\" : { X }, \"Y\" : { Y }, \"Z\" : { Z } }}";
public override bool Equals(object other) => other is Vector3 typedOther && this == typedOther;
public override int GetHashCode() => (X, Y, Z).GetHashCode();
public static readonly Vector3 Default = default;
public static Vector3 Zero = new Vector3(Default.X.Zero(),Default.Y.Zero(),Default.Z.Zero());
public static Vector3 One = new Vector3(Default.X.One(),Default.Y.One(),Default.Z.One());
public static Vector3 MinValue = new Vector3(Default.X.MinValue(),Default.Y.MinValue(),Default.Z.MinValue());
public static Vector3 MaxValue = new Vector3(Default.X.MaxValue(),Default.Y.MaxValue(),Default.Z.MaxValue());
public static bool operator ==(Vector3 a, Vector3 b) => (a.X == b.X) && (a.Y == b.Y) && (a.Z == b.Z);
public static bool operator !=(Vector3 a, Vector3 b) => (a.X != b.X) || (a.Y != b.Y) || (a.Z != b.Z);
public Vector3 WithX(Single value) => new Vector3(value, Y, Z);
public Vector3 WithY(Single value) => new Vector3(X, value, Z);
public Vector3 WithZ(Single value) => new Vector3(X, Y, value);
public static Vector3 operator +(Vector3 a, Vector3 b) => new Vector3(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
public static Vector3 operator -(Vector3 a, Vector3 b) => new Vector3(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
public static Vector3 operator *(Vector3 a, Vector3 b) => new Vector3(a.X * b.X, a.Y * b.Y, a.Z * b.Z);
public static Vector3 operator /(Vector3 a, Vector3 b) => new Vector3(a.X / b.X, a.Y / b.Y, a.Z / b.Z);
public static Vector3 operator -(Vector3 a) => new Vector3(- a.X, - a.Y, - a.Z);
public static Vector3 operator *(Vector3 self, Single scalar) => new Vector3(self.X * scalar, self.Y * scalar, self.Z * scalar);
public static Vector3 operator /(Vector3 self, Single scalar) => new Vector3(self.X / scalar, self.Y / scalar, self.Z / scalar);
public int Count => 3;
public Single this[int index] { get { switch (index) {
case 0: return X;
case 1: return Y;
case 2: return Z;
default: throw new System.ArgumentOutOfRangeException(nameof(index));
} } }
}
public static partial class Intrinsics {
public static Vector3 Add(this Vector3 a, Vector3 b) => a + b;
public static Vector3 Subtract(this Vector3 a, Vector3 b) => a - b;
public static Vector3 Multiply(this Vector3 a, Vector3 b) => a * b;
public static Vector3 Divide(this Vector3 a, Vector3 b) => a / b;
public static Vector3 Negate(this Vector3 a) => - a;
public static bool Equals(this Vector3 a, Vector3 b) => a == b;
public static bool NotEquals(this Vector3 a, Vector3 b) => a != b;
public static Vector3 Default(this Vector3 _) => default(Vector3);
public static Vector3 Zero(this Vector3 _) => Vector3.Zero;
public static Vector3 One(this Vector3 _) => Vector3.One;
public static Vector3 MinValue(this Vector3 _) => Vector3.MinValue;
public static Vector3 MaxValue(this Vector3 _) => Vector3.MaxValue;
}
public partial struct Vector4
{
public Vector4(Single x, Single y, Single z, Single w) => (X, Y, Z, W) = (x, y, z, w);
public static implicit operator Vector4((Single X, Single Y, Single Z, Single W) tuple) => new Vector4(tuple.X, tuple.Y, tuple.Z, tuple.W);
public static implicit operator (Single X, Single Y, Single Z, Single W)(Vector4 self) => (self.X, self.Y, self.Z, self.W);
public void Deconstruct(out Single x, out Single y, out Single z, out Single w) => (x, y, z, w) = (X, Y, Z, W);
public override string ToString() => $"{{ \"X\" : { X }, \"Y\" : { Y }, \"Z\" : { Z }, \"W\" : { W } }}";
public override bool Equals(object other) => other is Vector4 typedOther && this == typedOther;
public override int GetHashCode() => (X, Y, Z, W).GetHashCode();
public static readonly Vector4 Default = default;
public static Vector4 Zero = new Vector4(Default.X.Zero(),Default.Y.Zero(),Default.Z.Zero(),Default.W.Zero());
public static Vector4 One = new Vector4(Default.X.One(),Default.Y.One(),Default.Z.One(),Default.W.One());
public static Vector4 MinValue = new Vector4(Default.X.MinValue(),Default.Y.MinValue(),Default.Z.MinValue(),Default.W.MinValue());
public static Vector4 MaxValue = new Vector4(Default.X.MaxValue(),Default.Y.MaxValue(),Default.Z.MaxValue(),Default.W.MaxValue());
public static bool operator ==(Vector4 a, Vector4 b) => (a.X == b.X) && (a.Y == b.Y) && (a.Z == b.Z) && (a.W == b.W);
public static bool operator !=(Vector4 a, Vector4 b) => (a.X != b.X) || (a.Y != b.Y) || (a.Z != b.Z) || (a.W != b.W);
public Vector4 WithX(Single value) => new Vector4(value, Y, Z, W);
public Vector4 WithY(Single value) => new Vector4(X, value, Z, W);
public Vector4 WithZ(Single value) => new Vector4(X, Y, value, W);
public Vector4 WithW(Single value) => new Vector4(X, Y, Z, value);
public static Vector4 operator +(Vector4 a, Vector4 b) => new Vector4(a.X + b.X, a.Y + b.Y, a.Z + b.Z, a.W + b.W);
public static Vector4 operator -(Vector4 a, Vector4 b) => new Vector4(a.X - b.X, a.Y - b.Y, a.Z - b.Z, a.W - b.W);
public static Vector4 operator *(Vector4 a, Vector4 b) => new Vector4(a.X * b.X, a.Y * b.Y, a.Z * b.Z, a.W * b.W);
public static Vector4 operator /(Vector4 a, Vector4 b) => new Vector4(a.X / b.X, a.Y / b.Y, a.Z / b.Z, a.W / b.W);
public static Vector4 operator -(Vector4 a) => new Vector4(- a.X, - a.Y, - a.Z, - a.W);
public static Vector4 operator *(Vector4 self, Single scalar) => new Vector4(self.X * scalar, self.Y * scalar, self.Z * scalar, self.W * scalar);
public static Vector4 operator /(Vector4 self, Single scalar) => new Vector4(self.X / scalar, self.Y / scalar, self.Z / scalar, self.W / scalar);
public int Count => 4;
public Single this[int index] { get { switch (index) {
case 0: return X;
case 1: return Y;
case 2: return Z;
case 3: return W;
default: throw new System.ArgumentOutOfRangeException(nameof(index));
} } }
}
public static partial class Intrinsics {
public static Vector4 Add(this Vector4 a, Vector4 b) => a + b;
public static Vector4 Subtract(this Vector4 a, Vector4 b) => a - b;
public static Vector4 Multiply(this Vector4 a, Vector4 b) => a * b;
public static Vector4 Divide(this Vector4 a, Vector4 b) => a / b;
public static Vector4 Negate(this Vector4 a) => - a;
public static bool Equals(this Vector4 a, Vector4 b) => a == b;
public static bool NotEquals(this Vector4 a, Vector4 b) => a != b;
public static Vector4 Default(this Vector4 _) => default(Vector4);
public static Vector4 Zero(this Vector4 _) => Vector4.Zero;
public static Vector4 One(this Vector4 _) => Vector4.One;
public static Vector4 MinValue(this Vector4 _) => Vector4.MinValue;
public static Vector4 MaxValue(this Vector4 _) => Vector4.MaxValue;
}
public partial struct Quaternion
{
public Quaternion(Single x, Single y, Single z, Single w) => (X, Y, Z, W) = (x, y, z, w);
public static implicit operator Quaternion((Single X, Single Y, Single Z, Single W) tuple) => new Quaternion(tuple.X, tuple.Y, tuple.Z, tuple.W);
public static implicit operator (Single X, Single Y, Single Z, Single W)(Quaternion self) => (self.X, self.Y, self.Z, self.W);
public void Deconstruct(out Single x, out Single y, out Single z, out Single w) => (x, y, z, w) = (X, Y, Z, W);
public override string ToString() => $"{{ \"X\" : { X }, \"Y\" : { Y }, \"Z\" : { Z }, \"W\" : { W } }}";
public override bool Equals(object other) => other is Quaternion typedOther && this == typedOther;
public override int GetHashCode() => (X, Y, Z, W).GetHashCode();
public static readonly Quaternion Default = default;
public static Quaternion Zero = new Quaternion(Default.X.Zero(),Default.Y.Zero(),Default.Z.Zero(),Default.W.Zero());
public static Quaternion One = new Quaternion(Default.X.One(),Default.Y.One(),Default.Z.One(),Default.W.One());
public static Quaternion MinValue = new Quaternion(Default.X.MinValue(),Default.Y.MinValue(),Default.Z.MinValue(),Default.W.MinValue());
public static Quaternion MaxValue = new Quaternion(Default.X.MaxValue(),Default.Y.MaxValue(),Default.Z.MaxValue(),Default.W.MaxValue());
public static bool operator ==(Quaternion a, Quaternion b) => (a.X == b.X) && (a.Y == b.Y) && (a.Z == b.Z) && (a.W == b.W);
public static bool operator !=(Quaternion a, Quaternion b) => (a.X != b.X) || (a.Y != b.Y) || (a.Z != b.Z) || (a.W != b.W);
public Quaternion WithX(Single value) => new Quaternion(value, Y, Z, W);
public Quaternion WithY(Single value) => new Quaternion(X, value, Z, W);
public Quaternion WithZ(Single value) => new Quaternion(X, Y, value, W);
public Quaternion WithW(Single value) => new Quaternion(X, Y, Z, value);
public static Quaternion operator +(Quaternion a, Quaternion b) => new Quaternion(a.X + b.X, a.Y + b.Y, a.Z + b.Z, a.W + b.W);
public static Quaternion operator -(Quaternion a, Quaternion b) => new Quaternion(a.X - b.X, a.Y - b.Y, a.Z - b.Z, a.W - b.W);
public static Quaternion operator *(Quaternion a, Quaternion b) => new Quaternion(a.X * b.X, a.Y * b.Y, a.Z * b.Z, a.W * b.W);
public static Quaternion operator /(Quaternion a, Quaternion b) => new Quaternion(a.X / b.X, a.Y / b.Y, a.Z / b.Z, a.W / b.W);
public static Quaternion operator -(Quaternion a) => new Quaternion(- a.X, - a.Y, - a.Z, - a.W);
}
public static partial class Intrinsics {
public static Quaternion Add(this Quaternion a, Quaternion b) => a + b;
public static Quaternion Subtract(this Quaternion a, Quaternion b) => a - b;
public static Quaternion Multiply(this Quaternion a, Quaternion b) => a * b;
public static Quaternion Divide(this Quaternion a, Quaternion b) => a / b;
public static Quaternion Negate(this Quaternion a) => - a;
public static bool Equals(this Quaternion a, Quaternion b) => a == b;
public static bool NotEquals(this Quaternion a, Quaternion b) => a != b;
public static Quaternion Default(this Quaternion _) => default(Quaternion);
public static Quaternion Zero(this Quaternion _) => Quaternion.Zero;
public static Quaternion One(this Quaternion _) => Quaternion.One;
public static Quaternion MinValue(this Quaternion _) => Quaternion.MinValue;
public static Quaternion MaxValue(this Quaternion _) => Quaternion.MaxValue;
}
public partial struct DVector2
{
public DVector2(Double x, Double y) => (X, Y) = (x, y);
public static implicit operator DVector2((Double X, Double Y) tuple) => new DVector2(tuple.X, tuple.Y);
public static implicit operator (Double X, Double Y)(DVector2 self) => (self.X, self.Y);
public void Deconstruct(out Double x, out Double y) => (x, y) = (X, Y);
public override string ToString() => $"{{ \"X\" : { X }, \"Y\" : { Y } }}";
public override bool Equals(object other) => other is DVector2 typedOther && this == typedOther;
public override int GetHashCode() => (X, Y).GetHashCode();
public static readonly DVector2 Default = default;
public static DVector2 Zero = new DVector2(Default.X.Zero(),Default.Y.Zero());
public static DVector2 One = new DVector2(Default.X.One(),Default.Y.One());
public static DVector2 MinValue = new DVector2(Default.X.MinValue(),Default.Y.MinValue());
public static DVector2 MaxValue = new DVector2(Default.X.MaxValue(),Default.Y.MaxValue());
public static bool operator ==(DVector2 a, DVector2 b) => (a.X == b.X) && (a.Y == b.Y);
public static bool operator !=(DVector2 a, DVector2 b) => (a.X != b.X) || (a.Y != b.Y);
public DVector2 WithX(Double value) => new DVector2(value, Y);
public DVector2 WithY(Double value) => new DVector2(X, value);
public static DVector2 operator +(DVector2 a, DVector2 b) => new DVector2(a.X + b.X, a.Y + b.Y);
public static DVector2 operator -(DVector2 a, DVector2 b) => new DVector2(a.X - b.X, a.Y - b.Y);
public static DVector2 operator *(DVector2 a, DVector2 b) => new DVector2(a.X * b.X, a.Y * b.Y);
public static DVector2 operator /(DVector2 a, DVector2 b) => new DVector2(a.X / b.X, a.Y / b.Y);
public static DVector2 operator -(DVector2 a) => new DVector2(- a.X, - a.Y);
public static DVector2 operator *(DVector2 self, Double scalar) => new DVector2(self.X * scalar, self.Y * scalar);
public static DVector2 operator /(DVector2 self, Double scalar) => new DVector2(self.X / scalar, self.Y / scalar);
public int Count => 2;
public Double this[int index] { get { switch (index) {
case 0: return X;
case 1: return Y;
default: throw new System.ArgumentOutOfRangeException(nameof(index));
} } }
}
public static partial class Intrinsics {
public static DVector2 Add(this DVector2 a, DVector2 b) => a + b;
public static DVector2 Subtract(this DVector2 a, DVector2 b) => a - b;
public static DVector2 Multiply(this DVector2 a, DVector2 b) => a * b;
public static DVector2 Divide(this DVector2 a, DVector2 b) => a / b;
public static DVector2 Negate(this DVector2 a) => - a;
public static bool Equals(this DVector2 a, DVector2 b) => a == b;
public static bool NotEquals(this DVector2 a, DVector2 b) => a != b;
public static DVector2 Default(this DVector2 _) => default(DVector2);
public static DVector2 Zero(this DVector2 _) => DVector2.Zero;
public static DVector2 One(this DVector2 _) => DVector2.One;
public static DVector2 MinValue(this DVector2 _) => DVector2.MinValue;
public static DVector2 MaxValue(this DVector2 _) => DVector2.MaxValue;
}
public partial struct DVector3
{
public DVector3(Double x, Double y, Double z) => (X, Y, Z) = (x, y, z);
public static implicit operator DVector3((Double X, Double Y, Double Z) tuple) => new DVector3(tuple.X, tuple.Y, tuple.Z);
public static implicit operator (Double X, Double Y, Double Z)(DVector3 self) => (self.X, self.Y, self.Z);
public void Deconstruct(out Double x, out Double y, out Double z) => (x, y, z) = (X, Y, Z);
public override string ToString() => $"{{ \"X\" : { X }, \"Y\" : { Y }, \"Z\" : { Z } }}";
public override bool Equals(object other) => other is DVector3 typedOther && this == typedOther;
public override int GetHashCode() => (X, Y, Z).GetHashCode();
public static readonly DVector3 Default = default;
public static DVector3 Zero = new DVector3(Default.X.Zero(),Default.Y.Zero(),Default.Z.Zero());
public static DVector3 One = new DVector3(Default.X.One(),Default.Y.One(),Default.Z.One());
public static DVector3 MinValue = new DVector3(Default.X.MinValue(),Default.Y.MinValue(),Default.Z.MinValue());
public static DVector3 MaxValue = new DVector3(Default.X.MaxValue(),Default.Y.MaxValue(),Default.Z.MaxValue());
public static bool operator ==(DVector3 a, DVector3 b) => (a.X == b.X) && (a.Y == b.Y) && (a.Z == b.Z);
public static bool operator !=(DVector3 a, DVector3 b) => (a.X != b.X) || (a.Y != b.Y) || (a.Z != b.Z);
public DVector3 WithX(Double value) => new DVector3(value, Y, Z);
public DVector3 WithY(Double value) => new DVector3(X, value, Z);
public DVector3 WithZ(Double value) => new DVector3(X, Y, value);
public static DVector3 operator +(DVector3 a, DVector3 b) => new DVector3(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
public static DVector3 operator -(DVector3 a, DVector3 b) => new DVector3(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
public static DVector3 operator *(DVector3 a, DVector3 b) => new DVector3(a.X * b.X, a.Y * b.Y, a.Z * b.Z);
public static DVector3 operator /(DVector3 a, DVector3 b) => new DVector3(a.X / b.X, a.Y / b.Y, a.Z / b.Z);
public static DVector3 operator -(DVector3 a) => new DVector3(- a.X, - a.Y, - a.Z);
public static DVector3 operator *(DVector3 self, Double scalar) => new DVector3(self.X * scalar, self.Y * scalar, self.Z * scalar);
public static DVector3 operator /(DVector3 self, Double scalar) => new DVector3(self.X / scalar, self.Y / scalar, self.Z / scalar);
public int Count => 3;
public Double this[int index] { get { switch (index) {
case 0: return X;
case 1: return Y;
case 2: return Z;
default: throw new System.ArgumentOutOfRangeException(nameof(index));
} } }
}
public static partial class Intrinsics {
public static DVector3 Add(this DVector3 a, DVector3 b) => a + b;
public static DVector3 Subtract(this DVector3 a, DVector3 b) => a - b;
public static DVector3 Multiply(this DVector3 a, DVector3 b) => a * b;
public static DVector3 Divide(this DVector3 a, DVector3 b) => a / b;
public static DVector3 Negate(this DVector3 a) => - a;
public static bool Equals(this DVector3 a, DVector3 b) => a == b;
public static bool NotEquals(this DVector3 a, DVector3 b) => a != b;
public static DVector3 Default(this DVector3 _) => default(DVector3);
public static DVector3 Zero(this DVector3 _) => DVector3.Zero;
public static DVector3 One(this DVector3 _) => DVector3.One;
public static DVector3 MinValue(this DVector3 _) => DVector3.MinValue;
public static DVector3 MaxValue(this DVector3 _) => DVector3.MaxValue;
}
public partial struct DVector4
{
public DVector4(Double x, Double y, Double z, Double w) => (X, Y, Z, W) = (x, y, z, w);
public static implicit operator DVector4((Double X, Double Y, Double Z, Double W) tuple) => new DVector4(tuple.X, tuple.Y, tuple.Z, tuple.W);
public static implicit operator (Double X, Double Y, Double Z, Double W)(DVector4 self) => (self.X, self.Y, self.Z, self.W);
public void Deconstruct(out Double x, out Double y, out Double z, out Double w) => (x, y, z, w) = (X, Y, Z, W);
public override string ToString() => $"{{ \"X\" : { X }, \"Y\" : { Y }, \"Z\" : { Z }, \"W\" : { W } }}";
public override bool Equals(object other) => other is DVector4 typedOther && this == typedOther;
public override int GetHashCode() => (X, Y, Z, W).GetHashCode();
public static readonly DVector4 Default = default;
public static DVector4 Zero = new DVector4(Default.X.Zero(),Default.Y.Zero(),Default.Z.Zero(),Default.W.Zero());
public static DVector4 One = new DVector4(Default.X.One(),Default.Y.One(),Default.Z.One(),Default.W.One());
public static DVector4 MinValue = new DVector4(Default.X.MinValue(),Default.Y.MinValue(),Default.Z.MinValue(),Default.W.MinValue());
public static DVector4 MaxValue = new DVector4(Default.X.MaxValue(),Default.Y.MaxValue(),Default.Z.MaxValue(),Default.W.MaxValue());
public static bool operator ==(DVector4 a, DVector4 b) => (a.X == b.X) && (a.Y == b.Y) && (a.Z == b.Z) && (a.W == b.W);
public static bool operator !=(DVector4 a, DVector4 b) => (a.X != b.X) || (a.Y != b.Y) || (a.Z != b.Z) || (a.W != b.W);
public DVector4 WithX(Double value) => new DVector4(value, Y, Z, W);
public DVector4 WithY(Double value) => new DVector4(X, value, Z, W);
public DVector4 WithZ(Double value) => new DVector4(X, Y, value, W);
public DVector4 WithW(Double value) => new DVector4(X, Y, Z, value);
public static DVector4 operator +(DVector4 a, DVector4 b) => new DVector4(a.X + b.X, a.Y + b.Y, a.Z + b.Z, a.W + b.W);
public static DVector4 operator -(DVector4 a, DVector4 b) => new DVector4(a.X - b.X, a.Y - b.Y, a.Z - b.Z, a.W - b.W);
public static DVector4 operator *(DVector4 a, DVector4 b) => new DVector4(a.X * b.X, a.Y * b.Y, a.Z * b.Z, a.W * b.W);
public static DVector4 operator /(DVector4 a, DVector4 b) => new DVector4(a.X / b.X, a.Y / b.Y, a.Z / b.Z, a.W / b.W);
public static DVector4 operator -(DVector4 a) => new DVector4(- a.X, - a.Y, - a.Z, - a.W);
public static DVector4 operator *(DVector4 self, Double scalar) => new DVector4(self.X * scalar, self.Y * scalar, self.Z * scalar, self.W * scalar);
public static DVector4 operator /(DVector4 self, Double scalar) => new DVector4(self.X / scalar, self.Y / scalar, self.Z / scalar, self.W / scalar);
public int Count => 4;
public Double this[int index] { get { switch (index) {
case 0: return X;
case 1: return Y;
case 2: return Z;
case 3: return W;
default: throw new System.ArgumentOutOfRangeException(nameof(index));
} } }
}
public static partial class Intrinsics {
public static DVector4 Add(this DVector4 a, DVector4 b) => a + b;
public static DVector4 Subtract(this DVector4 a, DVector4 b) => a - b;
public static DVector4 Multiply(this DVector4 a, DVector4 b) => a * b;
public static DVector4 Divide(this DVector4 a, DVector4 b) => a / b;
public static DVector4 Negate(this DVector4 a) => - a;
public static bool Equals(this DVector4 a, DVector4 b) => a == b;
public static bool NotEquals(this DVector4 a, DVector4 b) => a != b;
public static DVector4 Default(this DVector4 _) => default(DVector4);
public static DVector4 Zero(this DVector4 _) => DVector4.Zero;
public static DVector4 One(this DVector4 _) => DVector4.One;
public static DVector4 MinValue(this DVector4 _) => DVector4.MinValue;
public static DVector4 MaxValue(this DVector4 _) => DVector4.MaxValue;
}
public partial struct DQuaternion
{
public DQuaternion(Double x, Double y, Double z, Double w) => (X, Y, Z, W) = (x, y, z, w);
public static implicit operator DQuaternion((Double X, Double Y, Double Z, Double W) tuple) => new DQuaternion(tuple.X, tuple.Y, tuple.Z, tuple.W);
public static implicit operator (Double X, Double Y, Double Z, Double W)(DQuaternion self) => (self.X, self.Y, self.Z, self.W);
public void Deconstruct(out Double x, out Double y, out Double z, out Double w) => (x, y, z, w) = (X, Y, Z, W);
public override string ToString() => $"{{ \"X\" : { X }, \"Y\" : { Y }, \"Z\" : { Z }, \"W\" : { W } }}";
public override bool Equals(object other) => other is DQuaternion typedOther && this == typedOther;
public override int GetHashCode() => (X, Y, Z, W).GetHashCode();
public static readonly DQuaternion Default = default;
public static DQuaternion Zero = new DQuaternion(Default.X.Zero(),Default.Y.Zero(),Default.Z.Zero(),Default.W.Zero());
public static DQuaternion One = new DQuaternion(Default.X.One(),Default.Y.One(),Default.Z.One(),Default.W.One());
public static DQuaternion MinValue = new DQuaternion(Default.X.MinValue(),Default.Y.MinValue(),Default.Z.MinValue(),Default.W.MinValue());
public static DQuaternion MaxValue = new DQuaternion(Default.X.MaxValue(),Default.Y.MaxValue(),Default.Z.MaxValue(),Default.W.MaxValue());
public static bool operator ==(DQuaternion a, DQuaternion b) => (a.X == b.X) && (a.Y == b.Y) && (a.Z == b.Z) && (a.W == b.W);
public static bool operator !=(DQuaternion a, DQuaternion b) => (a.X != b.X) || (a.Y != b.Y) || (a.Z != b.Z) || (a.W != b.W);
public DQuaternion WithX(Double value) => new DQuaternion(value, Y, Z, W);
public DQuaternion WithY(Double value) => new DQuaternion(X, value, Z, W);
public DQuaternion WithZ(Double value) => new DQuaternion(X, Y, value, W);
public DQuaternion WithW(Double value) => new DQuaternion(X, Y, Z, value);
public static DQuaternion operator +(DQuaternion a, DQuaternion b) => new DQuaternion(a.X + b.X, a.Y + b.Y, a.Z + b.Z, a.W + b.W);
public static DQuaternion operator -(DQuaternion a, DQuaternion b) => new DQuaternion(a.X - b.X, a.Y - b.Y, a.Z - b.Z, a.W - b.W);
public static DQuaternion operator *(DQuaternion a, DQuaternion b) => new DQuaternion(a.X * b.X, a.Y * b.Y, a.Z * b.Z, a.W * b.W);
public static DQuaternion operator /(DQuaternion a, DQuaternion b) => new DQuaternion(a.X / b.X, a.Y / b.Y, a.Z / b.Z, a.W / b.W);
public static DQuaternion operator -(DQuaternion a) => new DQuaternion(- a.X, - a.Y, - a.Z, - a.W);
}
public static partial class Intrinsics {
public static DQuaternion Add(this DQuaternion a, DQuaternion b) => a + b;
public static DQuaternion Subtract(this DQuaternion a, DQuaternion b) => a - b;
public static DQuaternion Multiply(this DQuaternion a, DQuaternion b) => a * b;
public static DQuaternion Divide(this DQuaternion a, DQuaternion b) => a / b;
public static DQuaternion Negate(this DQuaternion a) => - a;
public static bool Equals(this DQuaternion a, DQuaternion b) => a == b;
public static bool NotEquals(this DQuaternion a, DQuaternion b) => a != b;
public static DQuaternion Default(this DQuaternion _) => default(DQuaternion);
public static DQuaternion Zero(this DQuaternion _) => DQuaternion.Zero;
public static DQuaternion One(this DQuaternion _) => DQuaternion.One;
public static DQuaternion MinValue(this DQuaternion _) => DQuaternion.MinValue;
public static DQuaternion MaxValue(this DQuaternion _) => DQuaternion.MaxValue;
}
public partial struct Byte2
{
public Byte2(Byte a, Byte b) => (A, B) = (a, b);
public static implicit operator Byte2((Byte A, Byte B) tuple) => new Byte2(tuple.A, tuple.B);
public static implicit operator (Byte A, Byte B)(Byte2 self) => (self.A, self.B);
public void Deconstruct(out Byte a, out Byte b) => (a, b) = (A, B);
public override string ToString() => $"{{ \"A\" : { A }, \"B\" : { B } }}";
public override bool Equals(object other) => other is Byte2 typedOther && this == typedOther;
public override int GetHashCode() => (A, B).GetHashCode();
public static readonly Byte2 Default = default;
public static Byte2 Zero = new Byte2(Default.A.Zero(),Default.B.Zero());
public static Byte2 One = new Byte2(Default.A.One(),Default.B.One());
public static Byte2 MinValue = new Byte2(Default.A.MinValue(),Default.B.MinValue());
public static Byte2 MaxValue = new Byte2(Default.A.MaxValue(),Default.B.MaxValue());
public static bool operator ==(Byte2 a, Byte2 b) => (a.A == b.A) && (a.B == b.B);
public static bool operator !=(Byte2 a, Byte2 b) => (a.A != b.A) || (a.B != b.B);
public Byte2 WithA(Byte value) => new Byte2(value, B);
public Byte2 WithB(Byte value) => new Byte2(A, value);
}
public static partial class Intrinsics {
public static Byte2 Default(this Byte2 _) => default(Byte2);
public static Byte2 Zero(this Byte2 _) => Byte2.Zero;
public static Byte2 One(this Byte2 _) => Byte2.One;
public static Byte2 MinValue(this Byte2 _) => Byte2.MinValue;
public static Byte2 MaxValue(this Byte2 _) => Byte2.MaxValue;
}
public partial struct Byte3
{
public Byte3(Byte a, Byte b, Byte c) => (A, B, C) = (a, b, c);
public static implicit operator Byte3((Byte A, Byte B, Byte C) tuple) => new Byte3(tuple.A, tuple.B, tuple.C);
public static implicit operator (Byte A, Byte B, Byte C)(Byte3 self) => (self.A, self.B, self.C);
public void Deconstruct(out Byte a, out Byte b, out Byte c) => (a, b, c) = (A, B, C);
public override string ToString() => $"{{ \"A\" : { A }, \"B\" : { B }, \"C\" : { C } }}";
public override bool Equals(object other) => other is Byte3 typedOther && this == typedOther;
public override int GetHashCode() => (A, B, C).GetHashCode();
public static readonly Byte3 Default = default;
public static Byte3 Zero = new Byte3(Default.A.Zero(),Default.B.Zero(),Default.C.Zero());
public static Byte3 One = new Byte3(Default.A.One(),Default.B.One(),Default.C.One());
public static Byte3 MinValue = new Byte3(Default.A.MinValue(),Default.B.MinValue(),Default.C.MinValue());
public static Byte3 MaxValue = new Byte3(Default.A.MaxValue(),Default.B.MaxValue(),Default.C.MaxValue());
public static bool operator ==(Byte3 a, Byte3 b) => (a.A == b.A) && (a.B == b.B) && (a.C == b.C);
public static bool operator !=(Byte3 a, Byte3 b) => (a.A != b.A) || (a.B != b.B) || (a.C != b.C);
public Byte3 WithA(Byte value) => new Byte3(value, B, C);
public Byte3 WithB(Byte value) => new Byte3(A, value, C);
public Byte3 WithC(Byte value) => new Byte3(A, B, value);
}
public static partial class Intrinsics {
public static Byte3 Default(this Byte3 _) => default(Byte3);
public static Byte3 Zero(this Byte3 _) => Byte3.Zero;
public static Byte3 One(this Byte3 _) => Byte3.One;
public static Byte3 MinValue(this Byte3 _) => Byte3.MinValue;
public static Byte3 MaxValue(this Byte3 _) => Byte3.MaxValue;
}
public partial struct Byte4
{
public Byte4(Byte a, Byte b, Byte c, Byte d) => (A, B, C, D) = (a, b, c, d);
public static implicit operator Byte4((Byte A, Byte B, Byte C, Byte D) tuple) => new Byte4(tuple.A, tuple.B, tuple.C, tuple.D);
public static implicit operator (Byte A, Byte B, Byte C, Byte D)(Byte4 self) => (self.A, self.B, self.C, self.D);
public void Deconstruct(out Byte a, out Byte b, out Byte c, out Byte d) => (a, b, c, d) = (A, B, C, D);
public override string ToString() => $"{{ \"A\" : { A }, \"B\" : { B }, \"C\" : { C }, \"D\" : { D } }}";
public override bool Equals(object other) => other is Byte4 typedOther && this == typedOther;
public override int GetHashCode() => (A, B, C, D).GetHashCode();
public static readonly Byte4 Default = default;
public static Byte4 Zero = new Byte4(Default.A.Zero(),Default.B.Zero(),Default.C.Zero(),Default.D.Zero());
public static Byte4 One = new Byte4(Default.A.One(),Default.B.One(),Default.C.One(),Default.D.One());
public static Byte4 MinValue = new Byte4(Default.A.MinValue(),Default.B.MinValue(),Default.C.MinValue(),Default.D.MinValue());
public static Byte4 MaxValue = new Byte4(Default.A.MaxValue(),Default.B.MaxValue(),Default.C.MaxValue(),Default.D.MaxValue());
public static bool operator ==(Byte4 a, Byte4 b) => (a.A == b.A) && (a.B == b.B) && (a.C == b.C) && (a.D == b.D);
public static bool operator !=(Byte4 a, Byte4 b) => (a.A != b.A) || (a.B != b.B) || (a.C != b.C) || (a.D != b.D);
public Byte4 WithA(Byte value) => new Byte4(value, B, C, D);
public Byte4 WithB(Byte value) => new Byte4(A, value, C, D);
public Byte4 WithC(Byte value) => new Byte4(A, B, value, D);
public Byte4 WithD(Byte value) => new Byte4(A, B, C, value);
}
public static partial class Intrinsics {
public static Byte4 Default(this Byte4 _) => default(Byte4);
public static Byte4 Zero(this Byte4 _) => Byte4.Zero;
public static Byte4 One(this Byte4 _) => Byte4.One;
public static Byte4 MinValue(this Byte4 _) => Byte4.MinValue;
public static Byte4 MaxValue(this Byte4 _) => Byte4.MaxValue;
}
public partial struct Pose
{
public Pose(Vector3 position, Quaternion orientation) => (Position, Orientation) = (position, orientation);
public static implicit operator Pose((Vector3 Position, Quaternion Orientation) tuple) => new Pose(tuple.Position, tuple.Orientation);
public static implicit operator (Vector3 Position, Quaternion Orientation)(Pose self) => (self.Position, self.Orientation);
public void Deconstruct(out Vector3 position, out Quaternion orientation) => (position, orientation) = (Position, Orientation);
public override string ToString() => $"{{ \"Position\" : { Position }, \"Orientation\" : { Orientation } }}";
public override bool Equals(object other) => other is Pose typedOther && this == typedOther;
public override int GetHashCode() => (Position, Orientation).GetHashCode();
public static readonly Pose Default = default;
public static Pose Zero = new Pose(Default.Position.Zero(),Default.Orientation.Zero());
public static Pose One = new Pose(Default.Position.One(),Default.Orientation.One());
public static Pose MinValue = new Pose(Default.Position.MinValue(),Default.Orientation.MinValue());
public static Pose MaxValue = new Pose(Default.Position.MaxValue(),Default.Orientation.MaxValue());
public static bool operator ==(Pose a, Pose b) => (a.Position == b.Position) && (a.Orientation == b.Orientation);
public static bool operator !=(Pose a, Pose b) => (a.Position != b.Position) || (a.Orientation != b.Orientation);
public Pose WithPosition(Vector3 value) => new Pose(value, Orientation);
public Pose WithOrientation(Quaternion value) => new Pose(Position, value);
public static Pose operator +(Pose a, Pose b) => new Pose(a.Position + b.Position, a.Orientation + b.Orientation);
public static Pose operator -(Pose a, Pose b) => new Pose(a.Position - b.Position, a.Orientation - b.Orientation);
public static Pose operator *(Pose a, Pose b) => new Pose(a.Position * b.Position, a.Orientation * b.Orientation);
public static Pose operator /(Pose a, Pose b) => new Pose(a.Position / b.Position, a.Orientation / b.Orientation);
public static Pose operator -(Pose a) => new Pose(- a.Position, - a.Orientation);
}
public static partial class Intrinsics {
public static Pose Add(this Pose a, Pose b) => a + b;
public static Pose Subtract(this Pose a, Pose b) => a - b;
public static Pose Multiply(this Pose a, Pose b) => a * b;
public static Pose Divide(this Pose a, Pose b) => a / b;
public static Pose Negate(this Pose a) => - a;
public static bool Equals(this Pose a, Pose b) => a == b;
public static bool NotEquals(this Pose a, Pose b) => a != b;
public static Pose Default(this Pose _) => default(Pose);
public static Pose Zero(this Pose _) => Pose.Zero;
public static Pose One(this Pose _) => Pose.One;
public static Pose MinValue(this Pose _) => Pose.MinValue;
public static Pose MaxValue(this Pose _) => Pose.MaxValue;
}
public partial struct DPose
{
public DPose(DVector3 position, DQuaternion orientation) => (Position, Orientation) = (position, orientation);
public static implicit operator DPose((DVector3 Position, DQuaternion Orientation) tuple) => new DPose(tuple.Position, tuple.Orientation);
public static implicit operator (DVector3 Position, DQuaternion Orientation)(DPose self) => (self.Position, self.Orientation);
public void Deconstruct(out DVector3 position, out DQuaternion orientation) => (position, orientation) = (Position, Orientation);
public override string ToString() => $"{{ \"Position\" : { Position }, \"Orientation\" : { Orientation } }}";
public override bool Equals(object other) => other is DPose typedOther && this == typedOther;
public override int GetHashCode() => (Position, Orientation).GetHashCode();
public static readonly DPose Default = default;
public static DPose Zero = new DPose(Default.Position.Zero(),Default.Orientation.Zero());
public static DPose One = new DPose(Default.Position.One(),Default.Orientation.One());
public static DPose MinValue = new DPose(Default.Position.MinValue(),Default.Orientation.MinValue());
public static DPose MaxValue = new DPose(Default.Position.MaxValue(),Default.Orientation.MaxValue());
public static bool operator ==(DPose a, DPose b) => (a.Position == b.Position) && (a.Orientation == b.Orientation);
public static bool operator !=(DPose a, DPose b) => (a.Position != b.Position) || (a.Orientation != b.Orientation);
public DPose WithPosition(DVector3 value) => new DPose(value, Orientation);
public DPose WithOrientation(DQuaternion value) => new DPose(Position, value);
public static DPose operator +(DPose a, DPose b) => new DPose(a.Position + b.Position, a.Orientation + b.Orientation);
public static DPose operator -(DPose a, DPose b) => new DPose(a.Position - b.Position, a.Orientation - b.Orientation);
public static DPose operator *(DPose a, DPose b) => new DPose(a.Position * b.Position, a.Orientation * b.Orientation);
public static DPose operator /(DPose a, DPose b) => new DPose(a.Position / b.Position, a.Orientation / b.Orientation);
public static DPose operator -(DPose a) => new DPose(- a.Position, - a.Orientation);
}
public static partial class Intrinsics {
public static DPose Add(this DPose a, DPose b) => a + b;
public static DPose Subtract(this DPose a, DPose b) => a - b;
public static DPose Multiply(this DPose a, DPose b) => a * b;
public static DPose Divide(this DPose a, DPose b) => a / b;
public static DPose Negate(this DPose a) => - a;
public static bool Equals(this DPose a, DPose b) => a == b;
public static bool NotEquals(this DPose a, DPose b) => a != b;
public static DPose Default(this DPose _) => default(DPose);
public static DPose Zero(this DPose _) => DPose.Zero;
public static DPose One(this DPose _) => DPose.One;
public static DPose MinValue(this DPose _) => DPose.MinValue;
public static DPose MaxValue(this DPose _) => DPose.MaxValue;
}
public partial struct Transform
{
public Transform(Vector3 translation, Quaternion rotation, Vector3 scale) => (Translation, Rotation, Scale) = (translation, rotation, scale);
public static implicit operator Transform((Vector3 Translation, Quaternion Rotation, Vector3 Scale) tuple) => new Transform(tuple.Translation, tuple.Rotation, tuple.Scale);
public static implicit operator (Vector3 Translation, Quaternion Rotation, Vector3 Scale)(Transform self) => (self.Translation, self.Rotation, self.Scale);
public void Deconstruct(out Vector3 translation, out Quaternion rotation, out Vector3 scale) => (translation, rotation, scale) = (Translation, Rotation, Scale);
public override string ToString() => $"{{ \"Translation\" : { Translation }, \"Rotation\" : { Rotation }, \"Scale\" : { Scale } }}";
public override bool Equals(object other) => other is Transform typedOther && this == typedOther;
public override int GetHashCode() => (Translation, Rotation, Scale).GetHashCode();
public static readonly Transform Default = default;
public static Transform Zero = new Transform(Default.Translation.Zero(),Default.Rotation.Zero(),Default.Scale.Zero());
public static Transform One = new Transform(Default.Translation.One(),Default.Rotation.One(),Default.Scale.One());
public static Transform MinValue = new Transform(Default.Translation.MinValue(),Default.Rotation.MinValue(),Default.Scale.MinValue());
public static Transform MaxValue = new Transform(Default.Translation.MaxValue(),Default.Rotation.MaxValue(),Default.Scale.MaxValue());
public static bool operator ==(Transform a, Transform b) => (a.Translation == b.Translation) && (a.Rotation == b.Rotation) && (a.Scale == b.Scale);
public static bool operator !=(Transform a, Transform b) => (a.Translation != b.Translation) || (a.Rotation != b.Rotation) || (a.Scale != b.Scale);
public Transform WithTranslation(Vector3 value) => new Transform(value, Rotation, Scale);
public Transform WithRotation(Quaternion value) => new Transform(Translation, value, Scale);
public Transform WithScale(Vector3 value) => new Transform(Translation, Rotation, value);
public static Transform operator +(Transform a, Transform b) => new Transform(a.Translation + b.Translation, a.Rotation + b.Rotation, a.Scale + b.Scale);
public static Transform operator -(Transform a, Transform b) => new Transform(a.Translation - b.Translation, a.Rotation - b.Rotation, a.Scale - b.Scale);
public static Transform operator *(Transform a, Transform b) => new Transform(a.Translation * b.Translation, a.Rotation * b.Rotation, a.Scale * b.Scale);
public static Transform operator /(Transform a, Transform b) => new Transform(a.Translation / b.Translation, a.Rotation / b.Rotation, a.Scale / b.Scale);
public static Transform operator -(Transform a) => new Transform(- a.Translation, - a.Rotation, - a.Scale);
}
public static partial class Intrinsics {
public static Transform Add(this Transform a, Transform b) => a + b;
public static Transform Subtract(this Transform a, Transform b) => a - b;
public static Transform Multiply(this Transform a, Transform b) => a * b;
public static Transform Divide(this Transform a, Transform b) => a / b;
public static Transform Negate(this Transform a) => - a;
public static bool Equals(this Transform a, Transform b) => a == b;
public static bool NotEquals(this Transform a, Transform b) => a != b;
public static Transform Default(this Transform _) => default(Transform);
public static Transform Zero(this Transform _) => Transform.Zero;
public static Transform One(this Transform _) => Transform.One;
public static Transform MinValue(this Transform _) => Transform.MinValue;
public static Transform MaxValue(this Transform _) => Transform.MaxValue;
}
public partial struct BoundingBox2D
{
public BoundingBox2D(Vector2 lower, Vector2 upper) => (Lower, Upper) = (lower, upper);
public static implicit operator BoundingBox2D((Vector2 Lower, Vector2 Upper) tuple) => new BoundingBox2D(tuple.Lower, tuple.Upper);
public static implicit operator (Vector2 Lower, Vector2 Upper)(BoundingBox2D self) => (self.Lower, self.Upper);
public void Deconstruct(out Vector2 lower, out Vector2 upper) => (lower, upper) = (Lower, Upper);
public override string ToString() => $"{{ \"Lower\" : { Lower }, \"Upper\" : { Upper } }}";
public override bool Equals(object other) => other is BoundingBox2D typedOther && this == typedOther;
public override int GetHashCode() => (Lower, Upper).GetHashCode();
public static readonly BoundingBox2D Default = default;
public static BoundingBox2D Zero = new BoundingBox2D(Default.Lower.Zero(),Default.Upper.Zero());
public static BoundingBox2D One = new BoundingBox2D(Default.Lower.One(),Default.Upper.One());
public static BoundingBox2D MinValue = new BoundingBox2D(Default.Lower.MinValue(),Default.Upper.MinValue());
public static BoundingBox2D MaxValue = new BoundingBox2D(Default.Lower.MaxValue(),Default.Upper.MaxValue());
public static bool operator ==(BoundingBox2D a, BoundingBox2D b) => (a.Lower == b.Lower) && (a.Upper == b.Upper);
public static bool operator !=(BoundingBox2D a, BoundingBox2D b) => (a.Lower != b.Lower) || (a.Upper != b.Upper);
public BoundingBox2D WithLower(Vector2 value) => new BoundingBox2D(value, Upper);
public BoundingBox2D WithUpper(Vector2 value) => new BoundingBox2D(Lower, value);
}
public static partial class Intrinsics {
public static BoundingBox2D Default(this BoundingBox2D _) => default(BoundingBox2D);
public static BoundingBox2D Zero(this BoundingBox2D _) => BoundingBox2D.Zero;
public static BoundingBox2D One(this BoundingBox2D _) => BoundingBox2D.One;
public static BoundingBox2D MinValue(this BoundingBox2D _) => BoundingBox2D.MinValue;
public static BoundingBox2D MaxValue(this BoundingBox2D _) => BoundingBox2D.MaxValue;
}
public partial struct BoundingBox3D
{
public BoundingBox3D(Vector3 lower, Vector3 upper) => (Lower, Upper) = (lower, upper);
public static implicit operator BoundingBox3D((Vector3 Lower, Vector3 Upper) tuple) => new BoundingBox3D(tuple.Lower, tuple.Upper);
public static implicit operator (Vector3 Lower, Vector3 Upper)(BoundingBox3D self) => (self.Lower, self.Upper);
public void Deconstruct(out Vector3 lower, out Vector3 upper) => (lower, upper) = (Lower, Upper);
public override string ToString() => $"{{ \"Lower\" : { Lower }, \"Upper\" : { Upper } }}";
public override bool Equals(object other) => other is BoundingBox3D typedOther && this == typedOther;
public override int GetHashCode() => (Lower, Upper).GetHashCode();
public static readonly BoundingBox3D Default = default;
public static BoundingBox3D Zero = new BoundingBox3D(Default.Lower.Zero(),Default.Upper.Zero());
public static BoundingBox3D One = new BoundingBox3D(Default.Lower.One(),Default.Upper.One());
public static BoundingBox3D MinValue = new BoundingBox3D(Default.Lower.MinValue(),Default.Upper.MinValue());
public static BoundingBox3D MaxValue = new BoundingBox3D(Default.Lower.MaxValue(),Default.Upper.MaxValue());
public static bool operator ==(BoundingBox3D a, BoundingBox3D b) => (a.Lower == b.Lower) && (a.Upper == b.Upper);
public static bool operator !=(BoundingBox3D a, BoundingBox3D b) => (a.Lower != b.Lower) || (a.Upper != b.Upper);
public BoundingBox3D WithLower(Vector3 value) => new BoundingBox3D(value, Upper);
public BoundingBox3D WithUpper(Vector3 value) => new BoundingBox3D(Lower, value);
}
public static partial class Intrinsics {
public static BoundingBox3D Default(this BoundingBox3D _) => default(BoundingBox3D);
public static BoundingBox3D Zero(this BoundingBox3D _) => BoundingBox3D.Zero;
public static BoundingBox3D One(this BoundingBox3D _) => BoundingBox3D.One;
public static BoundingBox3D MinValue(this BoundingBox3D _) => BoundingBox3D.MinValue;
public static BoundingBox3D MaxValue(this BoundingBox3D _) => BoundingBox3D.MaxValue;
}
public partial struct Interval
{
public Interval(Single lower, Single upper) => (Lower, Upper) = (lower, upper);
public static implicit operator Interval((Single Lower, Single Upper) tuple) => new Interval(tuple.Lower, tuple.Upper);
public static implicit operator (Single Lower, Single Upper)(Interval self) => (self.Lower, self.Upper);
public void Deconstruct(out Single lower, out Single upper) => (lower, upper) = (Lower, Upper);
public override string ToString() => $"{{ \"Lower\" : { Lower }, \"Upper\" : { Upper } }}";
public override bool Equals(object other) => other is Interval typedOther && this == typedOther;
public override int GetHashCode() => (Lower, Upper).GetHashCode();
public static readonly Interval Default = default;
public static Interval Zero = new Interval(Default.Lower.Zero(),Default.Upper.Zero());
public static Interval One = new Interval(Default.Lower.One(),Default.Upper.One());
public static Interval MinValue = new Interval(Default.Lower.MinValue(),Default.Upper.MinValue());
public static Interval MaxValue = new Interval(Default.Lower.MaxValue(),Default.Upper.MaxValue());
public static bool operator ==(Interval a, Interval b) => (a.Lower == b.Lower) && (a.Upper == b.Upper);
public static bool operator !=(Interval a, Interval b) => (a.Lower != b.Lower) || (a.Upper != b.Upper);
public Interval WithLower(Single value) => new Interval(value, Upper);
public Interval WithUpper(Single value) => new Interval(Lower, value);
}
public static partial class Intrinsics {
public static Interval Default(this Interval _) => default(Interval);
public static Interval Zero(this Interval _) => Interval.Zero;
public static Interval One(this Interval _) => Interval.One;
public static Interval MinValue(this Interval _) => Interval.MinValue;
public static Interval MaxValue(this Interval _) => Interval.MaxValue;
}
public partial struct DInterval
{
public DInterval(Double lower, Double upper) => (Lower, Upper) = (lower, upper);
public static implicit operator DInterval((Double Lower, Double Upper) tuple) => new DInterval(tuple.Lower, tuple.Upper);
public static implicit operator (Double Lower, Double Upper)(DInterval self) => (self.Lower, self.Upper);
public void Deconstruct(out Double lower, out Double upper) => (lower, upper) = (Lower, Upper);
public override string ToString() => $"{{ \"Lower\" : { Lower }, \"Upper\" : { Upper } }}";
public override bool Equals(object other) => other is DInterval typedOther && this == typedOther;
public override int GetHashCode() => (Lower, Upper).GetHashCode();
public static readonly DInterval Default = default;
public static DInterval Zero = new DInterval(Default.Lower.Zero(),Default.Upper.Zero());
public static DInterval One = new DInterval(Default.Lower.One(),Default.Upper.One());
public static DInterval MinValue = new DInterval(Default.Lower.MinValue(),Default.Upper.MinValue());
public static DInterval MaxValue = new DInterval(Default.Lower.MaxValue(),Default.Upper.MaxValue());
public static bool operator ==(DInterval a, DInterval b) => (a.Lower == b.Lower) && (a.Upper == b.Upper);
public static bool operator !=(DInterval a, DInterval b) => (a.Lower != b.Lower) || (a.Upper != b.Upper);
public DInterval WithLower(Double value) => new DInterval(value, Upper);
public DInterval WithUpper(Double value) => new DInterval(Lower, value);
}
public static partial class Intrinsics {
public static DInterval Default(this DInterval _) => default(DInterval);
public static DInterval Zero(this DInterval _) => DInterval.Zero;
public static DInterval One(this DInterval _) => DInterval.One;
public static DInterval MinValue(this DInterval _) => DInterval.MinValue;
public static DInterval MaxValue(this DInterval _) => DInterval.MaxValue;
}
public partial struct Complex
{
public Complex(Double real, Double imaginary) => (Real, Imaginary) = (real, imaginary);
public static implicit operator Complex((Double Real, Double Imaginary) tuple) => new Complex(tuple.Real, tuple.Imaginary);
public static implicit operator (Double Real, Double Imaginary)(Complex self) => (self.Real, self.Imaginary);
public void Deconstruct(out Double real, out Double imaginary) => (real, imaginary) = (Real, Imaginary);
public override string ToString() => $"{{ \"Real\" : { Real }, \"Imaginary\" : { Imaginary } }}";
public override bool Equals(object other) => other is Complex typedOther && this == typedOther;
public override int GetHashCode() => (Real, Imaginary).GetHashCode();
public static readonly Complex Default = default;
public static Complex Zero = new Complex(Default.Real.Zero(),Default.Imaginary.Zero());
public static Complex One = new Complex(Default.Real.One(),Default.Imaginary.One());
public static Complex MinValue = new Complex(Default.Real.MinValue(),Default.Imaginary.MinValue());
public static Complex MaxValue = new Complex(Default.Real.MaxValue(),Default.Imaginary.MaxValue());
public static bool operator ==(Complex a, Complex b) => (a.Real == b.Real) && (a.Imaginary == b.Imaginary);
public static bool operator !=(Complex a, Complex b) => (a.Real != b.Real) || (a.Imaginary != b.Imaginary);
public Complex WithReal(Double value) => new Complex(value, Imaginary);
public Complex WithImaginary(Double value) => new Complex(Real, value);
public static Complex operator +(Complex a, Complex b) => new Complex(a.Real + b.Real, a.Imaginary + b.Imaginary);
public static Complex operator -(Complex a, Complex b) => new Complex(a.Real - b.Real, a.Imaginary - b.Imaginary);
public static Complex operator *(Complex a, Complex b) => new Complex(a.Real * b.Real, a.Imaginary * b.Imaginary);
public static Complex operator /(Complex a, Complex b) => new Complex(a.Real / b.Real, a.Imaginary / b.Imaginary);
public static Complex operator -(Complex a) => new Complex(- a.Real, - a.Imaginary);
public static Complex operator *(Complex self, Double scalar) => new Complex(self.Real * scalar, self.Imaginary * scalar);
public static Complex operator /(Complex self, Double scalar) => new Complex(self.Real / scalar, self.Imaginary / scalar);
public int Count => 2;
public Double this[int index] { get { switch (index) {
case 0: return Real;
case 1: return Imaginary;
default: throw new System.ArgumentOutOfRangeException(nameof(index));
} } }
}
public static partial class Intrinsics {
public static Complex Add(this Complex a, Complex b) => a + b;
public static Complex Subtract(this Complex a, Complex b) => a - b;
public static Complex Multiply(this Complex a, Complex b) => a * b;
public static Complex Divide(this Complex a, Complex b) => a / b;
public static Complex Negate(this Complex a) => - a;
public static bool Equals(this Complex a, Complex b) => a == b;
public static bool NotEquals(this Complex a, Complex b) => a != b;
public static Complex Default(this Complex _) => default(Complex);
public static Complex Zero(this Complex _) => Complex.Zero;
public static Complex One(this Complex _) => Complex.One;
public static Complex MinValue(this Complex _) => Complex.MinValue;
public static Complex MaxValue(this Complex _) => Complex.MaxValue;
}
public partial struct Ray
{
public Ray(Vector3 direction, Vector3 position) => (Direction, Position) = (direction, position);
public static implicit operator Ray((Vector3 Direction, Vector3 Position) tuple) => new Ray(tuple.Direction, tuple.Position);
public static implicit operator (Vector3 Direction, Vector3 Position)(Ray self) => (self.Direction, self.Position);
public void Deconstruct(out Vector3 direction, out Vector3 position) => (direction, position) = (Direction, Position);
public override string ToString() => $"{{ \"Direction\" : { Direction }, \"Position\" : { Position } }}";
public override bool Equals(object other) => other is Ray typedOther && this == typedOther;
public override int GetHashCode() => (Direction, Position).GetHashCode();
public static readonly Ray Default = default;
public static Ray Zero = new Ray(Default.Direction.Zero(),Default.Position.Zero());
public static Ray One = new Ray(Default.Direction.One(),Default.Position.One());
public static Ray MinValue = new Ray(Default.Direction.MinValue(),Default.Position.MinValue());
public static Ray MaxValue = new Ray(Default.Direction.MaxValue(),Default.Position.MaxValue());
public static bool operator ==(Ray a, Ray b) => (a.Direction == b.Direction) && (a.Position == b.Position);
public static bool operator !=(Ray a, Ray b) => (a.Direction != b.Direction) || (a.Position != b.Position);
public Ray WithDirection(Vector3 value) => new Ray(value, Position);
public Ray WithPosition(Vector3 value) => new Ray(Direction, value);
}
public static partial class Intrinsics {
public static Ray Default(this Ray _) => default(Ray);
public static Ray Zero(this Ray _) => Ray.Zero;
public static Ray One(this Ray _) => Ray.One;
public static Ray MinValue(this Ray _) => Ray.MinValue;
public static Ray MaxValue(this Ray _) => Ray.MaxValue;
}
public partial struct Sphere
{
public Sphere(Vector3 center, Single radius) => (Center, Radius) = (center, radius);
public static implicit operator Sphere((Vector3 Center, Single Radius) tuple) => new Sphere(tuple.Center, tuple.Radius);
public static implicit operator (Vector3 Center, Single Radius)(Sphere self) => (self.Center, self.Radius);
public void Deconstruct(out Vector3 center, out Single radius) => (center, radius) = (Center, Radius);
public override string ToString() => $"{{ \"Center\" : { Center }, \"Radius\" : { Radius } }}";
public override bool Equals(object other) => other is Sphere typedOther && this == typedOther;
public override int GetHashCode() => (Center, Radius).GetHashCode();
public static readonly Sphere Default = default;
public static Sphere Zero = new Sphere(Default.Center.Zero(),Default.Radius.Zero());
public static Sphere One = new Sphere(Default.Center.One(),Default.Radius.One());
public static Sphere MinValue = new Sphere(Default.Center.MinValue(),Default.Radius.MinValue());
public static Sphere MaxValue = new Sphere(Default.Center.MaxValue(),Default.Radius.MaxValue());
public static bool operator ==(Sphere a, Sphere b) => (a.Center == b.Center) && (a.Radius == b.Radius);
public static bool operator !=(Sphere a, Sphere b) => (a.Center != b.Center) || (a.Radius != b.Radius);
public Sphere WithCenter(Vector3 value) => new Sphere(value, Radius);
public Sphere WithRadius(Single value) => new Sphere(Center, value);
}
public static partial class Intrinsics {
public static Sphere Default(this Sphere _) => default(Sphere);
public static Sphere Zero(this Sphere _) => Sphere.Zero;
public static Sphere One(this Sphere _) => Sphere.One;
public static Sphere MinValue(this Sphere _) => Sphere.MinValue;
public static Sphere MaxValue(this Sphere _) => Sphere.MaxValue;
}
public partial struct Plane
{
public Plane(Vector3 normal, Single d) => (Normal, D) = (normal, d);
public static implicit operator Plane((Vector3 Normal, Single D) tuple) => new Plane(tuple.Normal, tuple.D);
public static implicit operator (Vector3 Normal, Single D)(Plane self) => (self.Normal, self.D);
public void Deconstruct(out Vector3 normal, out Single d) => (normal, d) = (Normal, D);
public override string ToString() => $"{{ \"Normal\" : { Normal }, \"D\" : { D } }}";
public override bool Equals(object other) => other is Plane typedOther && this == typedOther;
public override int GetHashCode() => (Normal, D).GetHashCode();
public static readonly Plane Default = default;
public static Plane Zero = new Plane(Default.Normal.Zero(),Default.D.Zero());
public static Plane One = new Plane(Default.Normal.One(),Default.D.One());
public static Plane MinValue = new Plane(Default.Normal.MinValue(),Default.D.MinValue());
public static Plane MaxValue = new Plane(Default.Normal.MaxValue(),Default.D.MaxValue());
public static bool operator ==(Plane a, Plane b) => (a.Normal == b.Normal) && (a.D == b.D);
public static bool operator !=(Plane a, Plane b) => (a.Normal != b.Normal) || (a.D != b.D);
public Plane WithNormal(Vector3 value) => new Plane(value, D);
public Plane WithD(Single value) => new Plane(Normal, value);
}
public static partial class Intrinsics {
public static Plane Default(this Plane _) => default(Plane);
public static Plane Zero(this Plane _) => Plane.Zero;
public static Plane One(this Plane _) => Plane.One;
public static Plane MinValue(this Plane _) => Plane.MinValue;
public static Plane MaxValue(this Plane _) => Plane.MaxValue;
}
public partial struct Triangle
{
public Triangle(Vector3 a, Vector3 b, Vector3 c) => (A, B, C) = (a, b, c);
public static implicit operator Triangle((Vector3 A, Vector3 B, Vector3 C) tuple) => new Triangle(tuple.A, tuple.B, tuple.C);
public static implicit operator (Vector3 A, Vector3 B, Vector3 C)(Triangle self) => (self.A, self.B, self.C);
public void Deconstruct(out Vector3 a, out Vector3 b, out Vector3 c) => (a, b, c) = (A, B, C);
public override string ToString() => $"{{ \"A\" : { A }, \"B\" : { B }, \"C\" : { C } }}";
public override bool Equals(object other) => other is Triangle typedOther && this == typedOther;
public override int GetHashCode() => (A, B, C).GetHashCode();
public static readonly Triangle Default = default;
public static Triangle Zero = new Triangle(Default.A.Zero(),Default.B.Zero(),Default.C.Zero());
public static Triangle One = new Triangle(Default.A.One(),Default.B.One(),Default.C.One());
public static Triangle MinValue = new Triangle(Default.A.MinValue(),Default.B.MinValue(),Default.C.MinValue());
public static Triangle MaxValue = new Triangle(Default.A.MaxValue(),Default.B.MaxValue(),Default.C.MaxValue());
public static bool operator ==(Triangle a, Triangle b) => (a.A == b.A) && (a.B == b.B) && (a.C == b.C);
public static bool operator !=(Triangle a, Triangle b) => (a.A != b.A) || (a.B != b.B) || (a.C != b.C);
public Triangle WithA(Vector3 value) => new Triangle(value, B, C);
public Triangle WithB(Vector3 value) => new Triangle(A, value, C);
public Triangle WithC(Vector3 value) => new Triangle(A, B, value);
public static Triangle operator +(Triangle a, Triangle b) => new Triangle(a.A + b.A, a.B + b.B, a.C + b.C);
public static Triangle operator -(Triangle a, Triangle b) => new Triangle(a.A - b.A, a.B - b.B, a.C - b.C);
public static Triangle operator *(Triangle a, Triangle b) => new Triangle(a.A * b.A, a.B * b.B, a.C * b.C);
public static Triangle operator /(Triangle a, Triangle b) => new Triangle(a.A / b.A, a.B / b.B, a.C / b.C);
public static Triangle operator -(Triangle a) => new Triangle(- a.A, - a.B, - a.C);
public static Triangle operator *(Triangle self, Vector3 scalar) => new Triangle(self.A * scalar, self.B * scalar, self.C * scalar);
public static Triangle operator /(Triangle self, Vector3 scalar) => new Triangle(self.A / scalar, self.B / scalar, self.C / scalar);
public int Count => 3;
public Vector3 this[int index] { get { switch (index) {
case 0: return A;
case 1: return B;
case 2: return C;
default: throw new System.ArgumentOutOfRangeException(nameof(index));
} } }
}
public static partial class Intrinsics {
public static Triangle Add(this Triangle a, Triangle b) => a + b;
public static Triangle Subtract(this Triangle a, Triangle b) => a - b;
public static Triangle Multiply(this Triangle a, Triangle b) => a * b;
public static Triangle Divide(this Triangle a, Triangle b) => a / b;
public static Triangle Negate(this Triangle a) => - a;
public static bool Equals(this Triangle a, Triangle b) => a == b;
public static bool NotEquals(this Triangle a, Triangle b) => a != b;
public static Triangle Default(this Triangle _) => default(Triangle);
public static Triangle Zero(this Triangle _) => Triangle.Zero;
public static Triangle One(this Triangle _) => Triangle.One;
public static Triangle MinValue(this Triangle _) => Triangle.MinValue;
public static Triangle MaxValue(this Triangle _) => Triangle.MaxValue;
}
public partial struct Triangle2D
{
public Triangle2D(Vector2 a, Vector2 b, Vector2 c) => (A, B, C) = (a, b, c);
public static implicit operator Triangle2D((Vector2 A, Vector2 B, Vector2 C) tuple) => new Triangle2D(tuple.A, tuple.B, tuple.C);
public static implicit operator (Vector2 A, Vector2 B, Vector2 C)(Triangle2D self) => (self.A, self.B, self.C);
public void Deconstruct(out Vector2 a, out Vector2 b, out Vector2 c) => (a, b, c) = (A, B, C);
public override string ToString() => $"{{ \"A\" : { A }, \"B\" : { B }, \"C\" : { C } }}";
public override bool Equals(object other) => other is Triangle2D typedOther && this == typedOther;
public override int GetHashCode() => (A, B, C).GetHashCode();
public static readonly Triangle2D Default = default;
public static Triangle2D Zero = new Triangle2D(Default.A.Zero(),Default.B.Zero(),Default.C.Zero());
public static Triangle2D One = new Triangle2D(Default.A.One(),Default.B.One(),Default.C.One());
public static Triangle2D MinValue = new Triangle2D(Default.A.MinValue(),Default.B.MinValue(),Default.C.MinValue());
public static Triangle2D MaxValue = new Triangle2D(Default.A.MaxValue(),Default.B.MaxValue(),Default.C.MaxValue());
public static bool operator ==(Triangle2D a, Triangle2D b) => (a.A == b.A) && (a.B == b.B) && (a.C == b.C);
public static bool operator !=(Triangle2D a, Triangle2D b) => (a.A != b.A) || (a.B != b.B) || (a.C != b.C);
public Triangle2D WithA(Vector2 value) => new Triangle2D(value, B, C);
public Triangle2D WithB(Vector2 value) => new Triangle2D(A, value, C);
public Triangle2D WithC(Vector2 value) => new Triangle2D(A, B, value);
public static Triangle2D operator +(Triangle2D a, Triangle2D b) => new Triangle2D(a.A + b.A, a.B + b.B, a.C + b.C);
public static Triangle2D operator -(Triangle2D a, Triangle2D b) => new Triangle2D(a.A - b.A, a.B - b.B, a.C - b.C);
public static Triangle2D operator *(Triangle2D a, Triangle2D b) => new Triangle2D(a.A * b.A, a.B * b.B, a.C * b.C);
public static Triangle2D operator /(Triangle2D a, Triangle2D b) => new Triangle2D(a.A / b.A, a.B / b.B, a.C / b.C);
public static Triangle2D operator -(Triangle2D a) => new Triangle2D(- a.A, - a.B, - a.C);
public static Triangle2D operator *(Triangle2D self, Vector2 scalar) => new Triangle2D(self.A * scalar, self.B * scalar, self.C * scalar);
public static Triangle2D operator /(Triangle2D self, Vector2 scalar) => new Triangle2D(self.A / scalar, self.B / scalar, self.C / scalar);
public int Count => 3;
public Vector2 this[int index] { get { switch (index) {
case 0: return A;
case 1: return B;
case 2: return C;
default: throw new System.ArgumentOutOfRangeException(nameof(index));
} } }
}
public static partial class Intrinsics {
public static Triangle2D Add(this Triangle2D a, Triangle2D b) => a + b;
public static Triangle2D Subtract(this Triangle2D a, Triangle2D b) => a - b;
public static Triangle2D Multiply(this Triangle2D a, Triangle2D b) => a * b;
public static Triangle2D Divide(this Triangle2D a, Triangle2D b) => a / b;
public static Triangle2D Negate(this Triangle2D a) => - a;
public static bool Equals(this Triangle2D a, Triangle2D b) => a == b;
public static bool NotEquals(this Triangle2D a, Triangle2D b) => a != b;
public static Triangle2D Default(this Triangle2D _) => default(Triangle2D);
public static Triangle2D Zero(this Triangle2D _) => Triangle2D.Zero;
public static Triangle2D One(this Triangle2D _) => Triangle2D.One;
public static Triangle2D MinValue(this Triangle2D _) => Triangle2D.MinValue;
public static Triangle2D MaxValue(this Triangle2D _) => Triangle2D.MaxValue;
}
public partial struct Quad
{
public Quad(Vector3 a, Vector3 b, Vector3 c, Vector3 d) => (A, B, C, D) = (a, b, c, d);
public static implicit operator Quad((Vector3 A, Vector3 B, Vector3 C, Vector3 D) tuple) => new Quad(tuple.A, tuple.B, tuple.C, tuple.D);
public static implicit operator (Vector3 A, Vector3 B, Vector3 C, Vector3 D)(Quad self) => (self.A, self.B, self.C, self.D);
public void Deconstruct(out Vector3 a, out Vector3 b, out Vector3 c, out Vector3 d) => (a, b, c, d) = (A, B, C, D);
public override string ToString() => $"{{ \"A\" : { A }, \"B\" : { B }, \"C\" : { C }, \"D\" : { D } }}";
public override bool Equals(object other) => other is Quad typedOther && this == typedOther;
public override int GetHashCode() => (A, B, C, D).GetHashCode();
public static readonly Quad Default = default;
public static Quad Zero = new Quad(Default.A.Zero(),Default.B.Zero(),Default.C.Zero(),Default.D.Zero());
public static Quad One = new Quad(Default.A.One(),Default.B.One(),Default.C.One(),Default.D.One());
public static Quad MinValue = new Quad(Default.A.MinValue(),Default.B.MinValue(),Default.C.MinValue(),Default.D.MinValue());
public static Quad MaxValue = new Quad(Default.A.MaxValue(),Default.B.MaxValue(),Default.C.MaxValue(),Default.D.MaxValue());
public static bool operator ==(Quad a, Quad b) => (a.A == b.A) && (a.B == b.B) && (a.C == b.C) && (a.D == b.D);
public static bool operator !=(Quad a, Quad b) => (a.A != b.A) || (a.B != b.B) || (a.C != b.C) || (a.D != b.D);
public Quad WithA(Vector3 value) => new Quad(value, B, C, D);
public Quad WithB(Vector3 value) => new Quad(A, value, C, D);
public Quad WithC(Vector3 value) => new Quad(A, B, value, D);
public Quad WithD(Vector3 value) => new Quad(A, B, C, value);
public static Quad operator +(Quad a, Quad b) => new Quad(a.A + b.A, a.B + b.B, a.C + b.C, a.D + b.D);
public static Quad operator -(Quad a, Quad b) => new Quad(a.A - b.A, a.B - b.B, a.C - b.C, a.D - b.D);
public static Quad operator *(Quad a, Quad b) => new Quad(a.A * b.A, a.B * b.B, a.C * b.C, a.D * b.D);
public static Quad operator /(Quad a, Quad b) => new Quad(a.A / b.A, a.B / b.B, a.C / b.C, a.D / b.D);
public static Quad operator -(Quad a) => new Quad(- a.A, - a.B, - a.C, - a.D);
public static Quad operator *(Quad self, Vector3 scalar) => new Quad(self.A * scalar, self.B * scalar, self.C * scalar, self.D * scalar);
public static Quad operator /(Quad self, Vector3 scalar) => new Quad(self.A / scalar, self.B / scalar, self.C / scalar, self.D / scalar);
public int Count => 4;
public Vector3 this[int index] { get { switch (index) {
case 0: return A;
case 1: return B;
case 2: return C;
case 3: return D;
default: throw new System.ArgumentOutOfRangeException(nameof(index));
} } }
}
public static partial class Intrinsics {
public static Quad Add(this Quad a, Quad b) => a + b;
public static Quad Subtract(this Quad a, Quad b) => a - b;
public static Quad Multiply(this Quad a, Quad b) => a * b;
public static Quad Divide(this Quad a, Quad b) => a / b;
public static Quad Negate(this Quad a) => - a;
public static bool Equals(this Quad a, Quad b) => a == b;
public static bool NotEquals(this Quad a, Quad b) => a != b;
public static Quad Default(this Quad _) => default(Quad);
public static Quad Zero(this Quad _) => Quad.Zero;
public static Quad One(this Quad _) => Quad.One;
public static Quad MinValue(this Quad _) => Quad.MinValue;
public static Quad MaxValue(this Quad _) => Quad.MaxValue;
}
public partial struct Line
{
public Line(Vector3 a, Vector3 b) => (A, B) = (a, b);
public static implicit operator Line((Vector3 A, Vector3 B) tuple) => new Line(tuple.A, tuple.B);
public static implicit operator (Vector3 A, Vector3 B)(Line self) => (self.A, self.B);
public void Deconstruct(out Vector3 a, out Vector3 b) => (a, b) = (A, B);
public override string ToString() => $"{{ \"A\" : { A }, \"B\" : { B } }}";
public override bool Equals(object other) => other is Line typedOther && this == typedOther;
public override int GetHashCode() => (A, B).GetHashCode();
public static readonly Line Default = default;
public static Line Zero = new Line(Default.A.Zero(),Default.B.Zero());
public static Line One = new Line(Default.A.One(),Default.B.One());
public static Line MinValue = new Line(Default.A.MinValue(),Default.B.MinValue());
public static Line MaxValue = new Line(Default.A.MaxValue(),Default.B.MaxValue());
public static bool operator ==(Line a, Line b) => (a.A == b.A) && (a.B == b.B);
public static bool operator !=(Line a, Line b) => (a.A != b.A) || (a.B != b.B);
public Line WithA(Vector3 value) => new Line(value, B);
public Line WithB(Vector3 value) => new Line(A, value);
public static Line operator +(Line a, Line b) => new Line(a.A + b.A, a.B + b.B);
public static Line operator -(Line a, Line b) => new Line(a.A - b.A, a.B - b.B);
public static Line operator *(Line a, Line b) => new Line(a.A * b.A, a.B * b.B);
public static Line operator /(Line a, Line b) => new Line(a.A / b.A, a.B / b.B);
public static Line operator -(Line a) => new Line(- a.A, - a.B);
public static Line operator *(Line self, Vector3 scalar) => new Line(self.A * scalar, self.B * scalar);
public static Line operator /(Line self, Vector3 scalar) => new Line(self.A / scalar, self.B / scalar);
public int Count => 2;
public Vector3 this[int index] { get { switch (index) {
case 0: return A;
case 1: return B;
default: throw new System.ArgumentOutOfRangeException(nameof(index));
} } }
}
public static partial class Intrinsics {
public static Line Add(this Line a, Line b) => a + b;
public static Line Subtract(this Line a, Line b) => a - b;
public static Line Multiply(this Line a, Line b) => a * b;
public static Line Divide(this Line a, Line b) => a / b;
public static Line Negate(this Line a) => - a;
public static bool Equals(this Line a, Line b) => a == b;
public static bool NotEquals(this Line a, Line b) => a != b;
public static Line Default(this Line _) => default(Line);
public static Line Zero(this Line _) => Line.Zero;
public static Line One(this Line _) => Line.One;
public static Line MinValue(this Line _) => Line.MinValue;
public static Line MaxValue(this Line _) => Line.MaxValue;
}
public partial struct Line2D
{
public Line2D(Vector2 a, Vector2 b) => (A, B) = (a, b);
public static implicit operator Line2D((Vector2 A, Vector2 B) tuple) => new Line2D(tuple.A, tuple.B);
public static implicit operator (Vector2 A, Vector2 B)(Line2D self) => (self.A, self.B);
public void Deconstruct(out Vector2 a, out Vector2 b) => (a, b) = (A, B);
public override string ToString() => $"{{ \"A\" : { A }, \"B\" : { B } }}";
public override bool Equals(object other) => other is Line2D typedOther && this == typedOther;
public override int GetHashCode() => (A, B).GetHashCode();
public static readonly Line2D Default = default;
public static Line2D Zero = new Line2D(Default.A.Zero(),Default.B.Zero());
public static Line2D One = new Line2D(Default.A.One(),Default.B.One());
public static Line2D MinValue = new Line2D(Default.A.MinValue(),Default.B.MinValue());
public static Line2D MaxValue = new Line2D(Default.A.MaxValue(),Default.B.MaxValue());
public static bool operator ==(Line2D a, Line2D b) => (a.A == b.A) && (a.B == b.B);
public static bool operator !=(Line2D a, Line2D b) => (a.A != b.A) || (a.B != b.B);
public Line2D WithA(Vector2 value) => new Line2D(value, B);
public Line2D WithB(Vector2 value) => new Line2D(A, value);
public static Line2D operator +(Line2D a, Line2D b) => new Line2D(a.A + b.A, a.B + b.B);
public static Line2D operator -(Line2D a, Line2D b) => new Line2D(a.A - b.A, a.B - b.B);
public static Line2D operator *(Line2D a, Line2D b) => new Line2D(a.A * b.A, a.B * b.B);
public static Line2D operator /(Line2D a, Line2D b) => new Line2D(a.A / b.A, a.B / b.B);
public static Line2D operator -(Line2D a) => new Line2D(- a.A, - a.B);
public static Line2D operator *(Line2D self, Vector2 scalar) => new Line2D(self.A * scalar, self.B * scalar);
public static Line2D operator /(Line2D self, Vector2 scalar) => new Line2D(self.A / scalar, self.B / scalar);
public int Count => 2;
public Vector2 this[int index] { get { switch (index) {
case 0: return A;
case 1: return B;
default: throw new System.ArgumentOutOfRangeException(nameof(index));
} } }
}
public static partial class Intrinsics {
public static Line2D Add(this Line2D a, Line2D b) => a + b;
public static Line2D Subtract(this Line2D a, Line2D b) => a - b;
public static Line2D Multiply(this Line2D a, Line2D b) => a * b;
public static Line2D Divide(this Line2D a, Line2D b) => a / b;
public static Line2D Negate(this Line2D a) => - a;
public static bool Equals(this Line2D a, Line2D b) => a == b;
public static bool NotEquals(this Line2D a, Line2D b) => a != b;
public static Line2D Default(this Line2D _) => default(Line2D);
public static Line2D Zero(this Line2D _) => Line2D.Zero;
public static Line2D One(this Line2D _) => Line2D.One;
public static Line2D MinValue(this Line2D _) => Line2D.MinValue;
public static Line2D MaxValue(this Line2D _) => Line2D.MaxValue;
}
public partial struct Color
{
public Color(Single r, Single g, Single b, Single a) => (R, G, B, A) = (r, g, b, a);
public static implicit operator Color((Single R, Single G, Single B, Single A) tuple) => new Color(tuple.R, tuple.G, tuple.B, tuple.A);
public static implicit operator (Single R, Single G, Single B, Single A)(Color self) => (self.R, self.G, self.B, self.A);
public void Deconstruct(out Single r, out Single g, out Single b, out Single a) => (r, g, b, a) = (R, G, B, A);
public override string ToString() => $"{{ \"R\" : { R }, \"G\" : { G }, \"B\" : { B }, \"A\" : { A } }}";
public override bool Equals(object other) => other is Color typedOther && this == typedOther;
public override int GetHashCode() => (R, G, B, A).GetHashCode();
public static readonly Color Default = default;
public static Color Zero = new Color(Default.R.Zero(),Default.G.Zero(),Default.B.Zero(),Default.A.Zero());
public static Color One = new Color(Default.R.One(),Default.G.One(),Default.B.One(),Default.A.One());
public static Color MinValue = new Color(Default.R.MinValue(),Default.G.MinValue(),Default.B.MinValue(),Default.A.MinValue());
public static Color MaxValue = new Color(Default.R.MaxValue(),Default.G.MaxValue(),Default.B.MaxValue(),Default.A.MaxValue());
public static bool operator ==(Color a, Color b) => (a.R == b.R) && (a.G == b.G) && (a.B == b.B) && (a.A == b.A);
public static bool operator !=(Color a, Color b) => (a.R != b.R) || (a.G != b.G) || (a.B != b.B) || (a.A != b.A);
public Color WithR(Single value) => new Color(value, G, B, A);
public Color WithG(Single value) => new Color(R, value, B, A);
public Color WithB(Single value) => new Color(R, G, value, A);
public Color WithA(Single value) => new Color(R, G, B, value);
}
public static partial class Intrinsics {
public static Color Default(this Color _) => default(Color);
public static Color Zero(this Color _) => Color.Zero;
public static Color One(this Color _) => Color.One;
public static Color MinValue(this Color _) => Color.MinValue;
public static Color MaxValue(this Color _) => Color.MaxValue;
}
public partial struct ColorHSV
{
public ColorHSV(Single h, Single s, Single v) => (H, S, V) = (h, s, v);
public static implicit operator ColorHSV((Single H, Single S, Single V) tuple) => new ColorHSV(tuple.H, tuple.S, tuple.V);
public static implicit operator (Single H, Single S, Single V)(ColorHSV self) => (self.H, self.S, self.V);
public void Deconstruct(out Single h, out Single s, out Single v) => (h, s, v) = (H, S, V);
public override string ToString() => $"{{ \"H\" : { H }, \"S\" : { S }, \"V\" : { V } }}";
public override bool Equals(object other) => other is ColorHSV typedOther && this == typedOther;
public override int GetHashCode() => (H, S, V).GetHashCode();
public static readonly ColorHSV Default = default;
public static ColorHSV Zero = new ColorHSV(Default.H.Zero(),Default.S.Zero(),Default.V.Zero());
public static ColorHSV One = new ColorHSV(Default.H.One(),Default.S.One(),Default.V.One());
public static ColorHSV MinValue = new ColorHSV(Default.H.MinValue(),Default.S.MinValue(),Default.V.MinValue());
public static ColorHSV MaxValue = new ColorHSV(Default.H.MaxValue(),Default.S.MaxValue(),Default.V.MaxValue());
public static bool operator ==(ColorHSV a, ColorHSV b) => (a.H == b.H) && (a.S == b.S) && (a.V == b.V);
public static bool operator !=(ColorHSV a, ColorHSV b) => (a.H != b.H) || (a.S != b.S) || (a.V != b.V);
public ColorHSV WithH(Single value) => new ColorHSV(value, S, V);
public ColorHSV WithS(Single value) => new ColorHSV(H, value, V);
public ColorHSV WithV(Single value) => new ColorHSV(H, S, value);
}
public static partial class Intrinsics {
public static ColorHSV Default(this ColorHSV _) => default(ColorHSV);
public static ColorHSV Zero(this ColorHSV _) => ColorHSV.Zero;
public static ColorHSV One(this ColorHSV _) => ColorHSV.One;
public static ColorHSV MinValue(this ColorHSV _) => ColorHSV.MinValue;
public static ColorHSV MaxValue(this ColorHSV _) => ColorHSV.MaxValue;
}
public partial struct ColorHSL
{
public ColorHSL(Single hue, Single saturation, Single luminance) => (Hue, Saturation, Luminance) = (hue, saturation, luminance);
public static implicit operator ColorHSL((Single Hue, Single Saturation, Single Luminance) tuple) => new ColorHSL(tuple.Hue, tuple.Saturation, tuple.Luminance);
public static implicit operator (Single Hue, Single Saturation, Single Luminance)(ColorHSL self) => (self.Hue, self.Saturation, self.Luminance);
public void Deconstruct(out Single hue, out Single saturation, out Single luminance) => (hue, saturation, luminance) = (Hue, Saturation, Luminance);
public override string ToString() => $"{{ \"Hue\" : { Hue }, \"Saturation\" : { Saturation }, \"Luminance\" : { Luminance } }}";
public override bool Equals(object other) => other is ColorHSL typedOther && this == typedOther;
public override int GetHashCode() => (Hue, Saturation, Luminance).GetHashCode();
public static readonly ColorHSL Default = default;
public static ColorHSL Zero = new ColorHSL(Default.Hue.Zero(),Default.Saturation.Zero(),Default.Luminance.Zero());
public static ColorHSL One = new ColorHSL(Default.Hue.One(),Default.Saturation.One(),Default.Luminance.One());
public static ColorHSL MinValue = new ColorHSL(Default.Hue.MinValue(),Default.Saturation.MinValue(),Default.Luminance.MinValue());
public static ColorHSL MaxValue = new ColorHSL(Default.Hue.MaxValue(),Default.Saturation.MaxValue(),Default.Luminance.MaxValue());
public static bool operator ==(ColorHSL a, ColorHSL b) => (a.Hue == b.Hue) && (a.Saturation == b.Saturation) && (a.Luminance == b.Luminance);
public static bool operator !=(ColorHSL a, ColorHSL b) => (a.Hue != b.Hue) || (a.Saturation != b.Saturation) || (a.Luminance != b.Luminance);
public ColorHSL WithHue(Single value) => new ColorHSL(value, Saturation, Luminance);
public ColorHSL WithSaturation(Single value) => new ColorHSL(Hue, value, Luminance);
public ColorHSL WithLuminance(Single value) => new ColorHSL(Hue, Saturation, value);
}
public static partial class Intrinsics {
public static ColorHSL Default(this ColorHSL _) => default(ColorHSL);
public static ColorHSL Zero(this ColorHSL _) => ColorHSL.Zero;
public static ColorHSL One(this ColorHSL _) => ColorHSL.One;
public static ColorHSL MinValue(this ColorHSL _) => ColorHSL.MinValue;
public static ColorHSL MaxValue(this ColorHSL _) => ColorHSL.MaxValue;
}
public partial struct ColorYCbCr
{
public ColorYCbCr(Single y, Single cb, Single cr) => (Y, Cb, Cr) = (y, cb, cr);
public static implicit operator ColorYCbCr((Single Y, Single Cb, Single Cr) tuple) => new ColorYCbCr(tuple.Y, tuple.Cb, tuple.Cr);
public static implicit operator (Single Y, Single Cb, Single Cr)(ColorYCbCr self) => (self.Y, self.Cb, self.Cr);
public void Deconstruct(out Single y, out Single cb, out Single cr) => (y, cb, cr) = (Y, Cb, Cr);
public override string ToString() => $"{{ \"Y\" : { Y }, \"Cb\" : { Cb }, \"Cr\" : { Cr } }}";
public override bool Equals(object other) => other is ColorYCbCr typedOther && this == typedOther;
public override int GetHashCode() => (Y, Cb, Cr).GetHashCode();
public static readonly ColorYCbCr Default = default;
public static ColorYCbCr Zero = new ColorYCbCr(Default.Y.Zero(),Default.Cb.Zero(),Default.Cr.Zero());
public static ColorYCbCr One = new ColorYCbCr(Default.Y.One(),Default.Cb.One(),Default.Cr.One());
public static ColorYCbCr MinValue = new ColorYCbCr(Default.Y.MinValue(),Default.Cb.MinValue(),Default.Cr.MinValue());
public static ColorYCbCr MaxValue = new ColorYCbCr(Default.Y.MaxValue(),Default.Cb.MaxValue(),Default.Cr.MaxValue());
public static bool operator ==(ColorYCbCr a, ColorYCbCr b) => (a.Y == b.Y) && (a.Cb == b.Cb) && (a.Cr == b.Cr);
public static bool operator !=(ColorYCbCr a, ColorYCbCr b) => (a.Y != b.Y) || (a.Cb != b.Cb) || (a.Cr != b.Cr);
public ColorYCbCr WithY(Single value) => new ColorYCbCr(value, Cb, Cr);
public ColorYCbCr WithCb(Single value) => new ColorYCbCr(Y, value, Cr);
public ColorYCbCr WithCr(Single value) => new ColorYCbCr(Y, Cb, value);
}
public static partial class Intrinsics {
public static ColorYCbCr Default(this ColorYCbCr _) => default(ColorYCbCr);
public static ColorYCbCr Zero(this ColorYCbCr _) => ColorYCbCr.Zero;
public static ColorYCbCr One(this ColorYCbCr _) => ColorYCbCr.One;
public static ColorYCbCr MinValue(this ColorYCbCr _) => ColorYCbCr.MinValue;
public static ColorYCbCr MaxValue(this ColorYCbCr _) => ColorYCbCr.MaxValue;
}
public partial struct SphericalCoordinate
{
public SphericalCoordinate(Double radius, Angle azimuth, Angle inclination) => (Radius, Azimuth, Inclination) = (radius, azimuth, inclination);
public static implicit operator SphericalCoordinate((Double Radius, Angle Azimuth, Angle Inclination) tuple) => new SphericalCoordinate(tuple.Radius, tuple.Azimuth, tuple.Inclination);
public static implicit operator (Double Radius, Angle Azimuth, Angle Inclination)(SphericalCoordinate self) => (self.Radius, self.Azimuth, self.Inclination);
public void Deconstruct(out Double radius, out Angle azimuth, out Angle inclination) => (radius, azimuth, inclination) = (Radius, Azimuth, Inclination);
public override string ToString() => $"{{ \"Radius\" : { Radius }, \"Azimuth\" : { Azimuth }, \"Inclination\" : { Inclination } }}";
public override bool Equals(object other) => other is SphericalCoordinate typedOther && this == typedOther;
public override int GetHashCode() => (Radius, Azimuth, Inclination).GetHashCode();
public static readonly SphericalCoordinate Default = default;
public static SphericalCoordinate Zero = new SphericalCoordinate(Default.Radius.Zero(),Default.Azimuth.Zero(),Default.Inclination.Zero());
public static SphericalCoordinate One = new SphericalCoordinate(Default.Radius.One(),Default.Azimuth.One(),Default.Inclination.One());
public static SphericalCoordinate MinValue = new SphericalCoordinate(Default.Radius.MinValue(),Default.Azimuth.MinValue(),Default.Inclination.MinValue());
public static SphericalCoordinate MaxValue = new SphericalCoordinate(Default.Radius.MaxValue(),Default.Azimuth.MaxValue(),Default.Inclination.MaxValue());
public static bool operator ==(SphericalCoordinate a, SphericalCoordinate b) => (a.Radius == b.Radius) && (a.Azimuth == b.Azimuth) && (a.Inclination == b.Inclination);
public static bool operator !=(SphericalCoordinate a, SphericalCoordinate b) => (a.Radius != b.Radius) || (a.Azimuth != b.Azimuth) || (a.Inclination != b.Inclination);
public SphericalCoordinate WithRadius(Double value) => new SphericalCoordinate(value, Azimuth, Inclination);
public SphericalCoordinate WithAzimuth(Angle value) => new SphericalCoordinate(Radius, value, Inclination);
public SphericalCoordinate WithInclination(Angle value) => new SphericalCoordinate(Radius, Azimuth, value);
}
public static partial class Intrinsics {
public static SphericalCoordinate Default(this SphericalCoordinate _) => default(SphericalCoordinate);
public static SphericalCoordinate Zero(this SphericalCoordinate _) => SphericalCoordinate.Zero;
public static SphericalCoordinate One(this SphericalCoordinate _) => SphericalCoordinate.One;
public static SphericalCoordinate MinValue(this SphericalCoordinate _) => SphericalCoordinate.MinValue;
public static SphericalCoordinate MaxValue(this SphericalCoordinate _) => SphericalCoordinate.MaxValue;
}
public partial struct PolarCoordinate
{
public PolarCoordinate(Double radius, Angle azimuth) => (Radius, Azimuth) = (radius, azimuth);
public static implicit operator PolarCoordinate((Double Radius, Angle Azimuth) tuple) => new PolarCoordinate(tuple.Radius, tuple.Azimuth);
public static implicit operator (Double Radius, Angle Azimuth)(PolarCoordinate self) => (self.Radius, self.Azimuth);
public void Deconstruct(out Double radius, out Angle azimuth) => (radius, azimuth) = (Radius, Azimuth);
public override string ToString() => $"{{ \"Radius\" : { Radius }, \"Azimuth\" : { Azimuth } }}";
public override bool Equals(object other) => other is PolarCoordinate typedOther && this == typedOther;
public override int GetHashCode() => (Radius, Azimuth).GetHashCode();
public static readonly PolarCoordinate Default = default;
public static PolarCoordinate Zero = new PolarCoordinate(Default.Radius.Zero(),Default.Azimuth.Zero());
public static PolarCoordinate One = new PolarCoordinate(Default.Radius.One(),Default.Azimuth.One());
public static PolarCoordinate MinValue = new PolarCoordinate(Default.Radius.MinValue(),Default.Azimuth.MinValue());
public static PolarCoordinate MaxValue = new PolarCoordinate(Default.Radius.MaxValue(),Default.Azimuth.MaxValue());
public static bool operator ==(PolarCoordinate a, PolarCoordinate b) => (a.Radius == b.Radius) && (a.Azimuth == b.Azimuth);
public static bool operator !=(PolarCoordinate a, PolarCoordinate b) => (a.Radius != b.Radius) || (a.Azimuth != b.Azimuth);
public PolarCoordinate WithRadius(Double value) => new PolarCoordinate(value, Azimuth);
public PolarCoordinate WithAzimuth(Angle value) => new PolarCoordinate(Radius, value);
}
public static partial class Intrinsics {
public static PolarCoordinate Default(this PolarCoordinate _) => default(PolarCoordinate);
public static PolarCoordinate Zero(this PolarCoordinate _) => PolarCoordinate.Zero;
public static PolarCoordinate One(this PolarCoordinate _) => PolarCoordinate.One;
public static PolarCoordinate MinValue(this PolarCoordinate _) => PolarCoordinate.MinValue;
public static PolarCoordinate MaxValue(this PolarCoordinate _) => PolarCoordinate.MaxValue;
}
public partial struct LogPolarCoordinate
{
public LogPolarCoordinate(Double rho, Angle azimuth) => (Rho, Azimuth) = (rho, azimuth);
public static implicit operator LogPolarCoordinate((Double Rho, Angle Azimuth) tuple) => new LogPolarCoordinate(tuple.Rho, tuple.Azimuth);
public static implicit operator (Double Rho, Angle Azimuth)(LogPolarCoordinate self) => (self.Rho, self.Azimuth);
public void Deconstruct(out Double rho, out Angle azimuth) => (rho, azimuth) = (Rho, Azimuth);
public override string ToString() => $"{{ \"Rho\" : { Rho }, \"Azimuth\" : { Azimuth } }}";
public override bool Equals(object other) => other is LogPolarCoordinate typedOther && this == typedOther;
public override int GetHashCode() => (Rho, Azimuth).GetHashCode();
public static readonly LogPolarCoordinate Default = default;
public static LogPolarCoordinate Zero = new LogPolarCoordinate(Default.Rho.Zero(),Default.Azimuth.Zero());
public static LogPolarCoordinate One = new LogPolarCoordinate(Default.Rho.One(),Default.Azimuth.One());
public static LogPolarCoordinate MinValue = new LogPolarCoordinate(Default.Rho.MinValue(),Default.Azimuth.MinValue());
public static LogPolarCoordinate MaxValue = new LogPolarCoordinate(Default.Rho.MaxValue(),Default.Azimuth.MaxValue());
public static bool operator ==(LogPolarCoordinate a, LogPolarCoordinate b) => (a.Rho == b.Rho) && (a.Azimuth == b.Azimuth);
public static bool operator !=(LogPolarCoordinate a, LogPolarCoordinate b) => (a.Rho != b.Rho) || (a.Azimuth != b.Azimuth);
public LogPolarCoordinate WithRho(Double value) => new LogPolarCoordinate(value, Azimuth);
public LogPolarCoordinate WithAzimuth(Angle value) => new LogPolarCoordinate(Rho, value);
}
public static partial class Intrinsics {
public static LogPolarCoordinate Default(this LogPolarCoordinate _) => default(LogPolarCoordinate);
public static LogPolarCoordinate Zero(this LogPolarCoordinate _) => LogPolarCoordinate.Zero;
public static LogPolarCoordinate One(this LogPolarCoordinate _) => LogPolarCoordinate.One;
public static LogPolarCoordinate MinValue(this LogPolarCoordinate _) => LogPolarCoordinate.MinValue;
public static LogPolarCoordinate MaxValue(this LogPolarCoordinate _) => LogPolarCoordinate.MaxValue;
}
public partial struct HorizontalCoordinate
{
public HorizontalCoordinate(Double radius, Angle azimuth, Double height) => (Radius, Azimuth, Height) = (radius, azimuth, height);
public static implicit operator HorizontalCoordinate((Double Radius, Angle Azimuth, Double Height) tuple) => new HorizontalCoordinate(tuple.Radius, tuple.Azimuth, tuple.Height);
public static implicit operator (Double Radius, Angle Azimuth, Double Height)(HorizontalCoordinate self) => (self.Radius, self.Azimuth, self.Height);
public void Deconstruct(out Double radius, out Angle azimuth, out Double height) => (radius, azimuth, height) = (Radius, Azimuth, Height);
public override string ToString() => $"{{ \"Radius\" : { Radius }, \"Azimuth\" : { Azimuth }, \"Height\" : { Height } }}";
public override bool Equals(object other) => other is HorizontalCoordinate typedOther && this == typedOther;
public override int GetHashCode() => (Radius, Azimuth, Height).GetHashCode();
public static readonly HorizontalCoordinate Default = default;
public static HorizontalCoordinate Zero = new HorizontalCoordinate(Default.Radius.Zero(),Default.Azimuth.Zero(),Default.Height.Zero());
public static HorizontalCoordinate One = new HorizontalCoordinate(Default.Radius.One(),Default.Azimuth.One(),Default.Height.One());
public static HorizontalCoordinate MinValue = new HorizontalCoordinate(Default.Radius.MinValue(),Default.Azimuth.MinValue(),Default.Height.MinValue());
public static HorizontalCoordinate MaxValue = new HorizontalCoordinate(Default.Radius.MaxValue(),Default.Azimuth.MaxValue(),Default.Height.MaxValue());
public static bool operator ==(HorizontalCoordinate a, HorizontalCoordinate b) => (a.Radius == b.Radius) && (a.Azimuth == b.Azimuth) && (a.Height == b.Height);
public static bool operator !=(HorizontalCoordinate a, HorizontalCoordinate b) => (a.Radius != b.Radius) || (a.Azimuth != b.Azimuth) || (a.Height != b.Height);
public HorizontalCoordinate WithRadius(Double value) => new HorizontalCoordinate(value, Azimuth, Height);
public HorizontalCoordinate WithAzimuth(Angle value) => new HorizontalCoordinate(Radius, value, Height);
public HorizontalCoordinate WithHeight(Double value) => new HorizontalCoordinate(Radius, Azimuth, value);
}
public static partial class Intrinsics {
public static HorizontalCoordinate Default(this HorizontalCoordinate _) => default(HorizontalCoordinate);
public static HorizontalCoordinate Zero(this HorizontalCoordinate _) => HorizontalCoordinate.Zero;
public static HorizontalCoordinate One(this HorizontalCoordinate _) => HorizontalCoordinate.One;
public static HorizontalCoordinate MinValue(this HorizontalCoordinate _) => HorizontalCoordinate.MinValue;
public static HorizontalCoordinate MaxValue(this HorizontalCoordinate _) => HorizontalCoordinate.MaxValue;
}
public partial struct GeoCoordinate
{
public GeoCoordinate(Double latitude, Double longitude, Double altitude) => (Latitude, Longitude, Altitude) = (latitude, longitude, altitude);
public static implicit operator GeoCoordinate((Double Latitude, Double Longitude, Double Altitude) tuple) => new GeoCoordinate(tuple.Latitude, tuple.Longitude, tuple.Altitude);
public static implicit operator (Double Latitude, Double Longitude, Double Altitude)(GeoCoordinate self) => (self.Latitude, self.Longitude, self.Altitude);
public void Deconstruct(out Double latitude, out Double longitude, out Double altitude) => (latitude, longitude, altitude) = (Latitude, Longitude, Altitude);
public override string ToString() => $"{{ \"Latitude\" : { Latitude }, \"Longitude\" : { Longitude }, \"Altitude\" : { Altitude } }}";
public override bool Equals(object other) => other is GeoCoordinate typedOther && this == typedOther;
public override int GetHashCode() => (Latitude, Longitude, Altitude).GetHashCode();
public static readonly GeoCoordinate Default = default;
public static GeoCoordinate Zero = new GeoCoordinate(Default.Latitude.Zero(),Default.Longitude.Zero(),Default.Altitude.Zero());
public static GeoCoordinate One = new GeoCoordinate(Default.Latitude.One(),Default.Longitude.One(),Default.Altitude.One());
public static GeoCoordinate MinValue = new GeoCoordinate(Default.Latitude.MinValue(),Default.Longitude.MinValue(),Default.Altitude.MinValue());
public static GeoCoordinate MaxValue = new GeoCoordinate(Default.Latitude.MaxValue(),Default.Longitude.MaxValue(),Default.Altitude.MaxValue());
public static bool operator ==(GeoCoordinate a, GeoCoordinate b) => (a.Latitude == b.Latitude) && (a.Longitude == b.Longitude) && (a.Altitude == b.Altitude);
public static bool operator !=(GeoCoordinate a, GeoCoordinate b) => (a.Latitude != b.Latitude) || (a.Longitude != b.Longitude) || (a.Altitude != b.Altitude);
public GeoCoordinate WithLatitude(Double value) => new GeoCoordinate(value, Longitude, Altitude);
public GeoCoordinate WithLongitude(Double value) => new GeoCoordinate(Latitude, value, Altitude);
public GeoCoordinate WithAltitude(Double value) => new GeoCoordinate(Latitude, Longitude, value);
}
public static partial class Intrinsics {
public static GeoCoordinate Default(this GeoCoordinate _) => default(GeoCoordinate);
public static GeoCoordinate Zero(this GeoCoordinate _) => GeoCoordinate.Zero;
public static GeoCoordinate One(this GeoCoordinate _) => GeoCoordinate.One;
public static GeoCoordinate MinValue(this GeoCoordinate _) => GeoCoordinate.MinValue;
public static GeoCoordinate MaxValue(this GeoCoordinate _) => GeoCoordinate.MaxValue;
}
public partial struct AxisAngle
{
public AxisAngle(DVector3 axis, Angle angle) => (Axis, Angle) = (axis, angle);
public static implicit operator AxisAngle((DVector3 Axis, Angle Angle) tuple) => new AxisAngle(tuple.Axis, tuple.Angle);
public static implicit operator (DVector3 Axis, Angle Angle)(AxisAngle self) => (self.Axis, self.Angle);
public void Deconstruct(out DVector3 axis, out Angle angle) => (axis, angle) = (Axis, Angle);
public override string ToString() => $"{{ \"Axis\" : { Axis }, \"Angle\" : { Angle } }}";
public override bool Equals(object other) => other is AxisAngle typedOther && this == typedOther;
public override int GetHashCode() => (Axis, Angle).GetHashCode();
public static readonly AxisAngle Default = default;
public static AxisAngle Zero = new AxisAngle(Default.Axis.Zero(),Default.Angle.Zero());
public static AxisAngle One = new AxisAngle(Default.Axis.One(),Default.Angle.One());
public static AxisAngle MinValue = new AxisAngle(Default.Axis.MinValue(),Default.Angle.MinValue());
public static AxisAngle MaxValue = new AxisAngle(Default.Axis.MaxValue(),Default.Angle.MaxValue());
public static bool operator ==(AxisAngle a, AxisAngle b) => (a.Axis == b.Axis) && (a.Angle == b.Angle);
public static bool operator !=(AxisAngle a, AxisAngle b) => (a.Axis != b.Axis) || (a.Angle != b.Angle);
public AxisAngle WithAxis(DVector3 value) => new AxisAngle(value, Angle);
public AxisAngle WithAngle(Angle value) => new AxisAngle(Axis, value);
}
public static partial class Intrinsics {
public static AxisAngle Default(this AxisAngle _) => default(AxisAngle);
public static AxisAngle Zero(this AxisAngle _) => AxisAngle.Zero;
public static AxisAngle One(this AxisAngle _) => AxisAngle.One;
public static AxisAngle MinValue(this AxisAngle _) => AxisAngle.MinValue;
public static AxisAngle MaxValue(this AxisAngle _) => AxisAngle.MaxValue;
}
public partial struct EulerAngles
{
public EulerAngles(Angle yaw, Angle pitch, Angle roll) => (Yaw, Pitch, Roll) = (yaw, pitch, roll);
public static implicit operator EulerAngles((Angle Yaw, Angle Pitch, Angle Roll) tuple) => new EulerAngles(tuple.Yaw, tuple.Pitch, tuple.Roll);
public static implicit operator (Angle Yaw, Angle Pitch, Angle Roll)(EulerAngles self) => (self.Yaw, self.Pitch, self.Roll);
public void Deconstruct(out Angle yaw, out Angle pitch, out Angle roll) => (yaw, pitch, roll) = (Yaw, Pitch, Roll);
public override string ToString() => $"{{ \"Yaw\" : { Yaw }, \"Pitch\" : { Pitch }, \"Roll\" : { Roll } }}";
public override bool Equals(object other) => other is EulerAngles typedOther && this == typedOther;
public override int GetHashCode() => (Yaw, Pitch, Roll).GetHashCode();
public static readonly EulerAngles Default = default;
public static EulerAngles Zero = new EulerAngles(Default.Yaw.Zero(),Default.Pitch.Zero(),Default.Roll.Zero());
public static EulerAngles One = new EulerAngles(Default.Yaw.One(),Default.Pitch.One(),Default.Roll.One());
public static EulerAngles MinValue = new EulerAngles(Default.Yaw.MinValue(),Default.Pitch.MinValue(),Default.Roll.MinValue());
public static EulerAngles MaxValue = new EulerAngles(Default.Yaw.MaxValue(),Default.Pitch.MaxValue(),Default.Roll.MaxValue());
public static bool operator ==(EulerAngles a, EulerAngles b) => (a.Yaw == b.Yaw) && (a.Pitch == b.Pitch) && (a.Roll == b.Roll);
public static bool operator !=(EulerAngles a, EulerAngles b) => (a.Yaw != b.Yaw) || (a.Pitch != b.Pitch) || (a.Roll != b.Roll);
public EulerAngles WithYaw(Angle value) => new EulerAngles(value, Pitch, Roll);
public EulerAngles WithPitch(Angle value) => new EulerAngles(Yaw, value, Roll);
public EulerAngles WithRoll(Angle value) => new EulerAngles(Yaw, Pitch, value);
}
public static partial class Intrinsics {
public static EulerAngles Default(this EulerAngles _) => default(EulerAngles);
public static EulerAngles Zero(this EulerAngles _) => EulerAngles.Zero;
public static EulerAngles One(this EulerAngles _) => EulerAngles.One;
public static EulerAngles MinValue(this EulerAngles _) => EulerAngles.MinValue;
public static EulerAngles MaxValue(this EulerAngles _) => EulerAngles.MaxValue;
}
public partial struct Circle
{
public Circle(DVector2 position, Double radius) => (Position, Radius) = (position, radius);
public static implicit operator Circle((DVector2 Position, Double Radius) tuple) => new Circle(tuple.Position, tuple.Radius);
public static implicit operator (DVector2 Position, Double Radius)(Circle self) => (self.Position, self.Radius);
public void Deconstruct(out DVector2 position, out Double radius) => (position, radius) = (Position, Radius);
public override string ToString() => $"{{ \"Position\" : { Position }, \"Radius\" : { Radius } }}";
public override bool Equals(object other) => other is Circle typedOther && this == typedOther;
public override int GetHashCode() => (Position, Radius).GetHashCode();
public static readonly Circle Default = default;
public static Circle Zero = new Circle(Default.Position.Zero(),Default.Radius.Zero());
public static Circle One = new Circle(Default.Position.One(),Default.Radius.One());
public static Circle MinValue = new Circle(Default.Position.MinValue(),Default.Radius.MinValue());
public static Circle MaxValue = new Circle(Default.Position.MaxValue(),Default.Radius.MaxValue());
public static bool operator ==(Circle a, Circle b) => (a.Position == b.Position) && (a.Radius == b.Radius);
public static bool operator !=(Circle a, Circle b) => (a.Position != b.Position) || (a.Radius != b.Radius);
public Circle WithPosition(DVector2 value) => new Circle(value, Radius);
public Circle WithRadius(Double value) => new Circle(Position, value);
}
public static partial class Intrinsics {
public static Circle Default(this Circle _) => default(Circle);
public static Circle Zero(this Circle _) => Circle.Zero;
public static Circle One(this Circle _) => Circle.One;
public static Circle MinValue(this Circle _) => Circle.MinValue;
public static Circle MaxValue(this Circle _) => Circle.MaxValue;
}
public partial struct Size
{
public Size(Double width, Double height) => (Width, Height) = (width, height);
public static implicit operator Size((Double Width, Double Height) tuple) => new Size(tuple.Width, tuple.Height);
public static implicit operator (Double Width, Double Height)(Size self) => (self.Width, self.Height);
public void Deconstruct(out Double width, out Double height) => (width, height) = (Width, Height);
public override string ToString() => $"{{ \"Width\" : { Width }, \"Height\" : { Height } }}";
public override bool Equals(object other) => other is Size typedOther && this == typedOther;
public override int GetHashCode() => (Width, Height).GetHashCode();
public static readonly Size Default = default;
public static Size Zero = new Size(Default.Width.Zero(),Default.Height.Zero());
public static Size One = new Size(Default.Width.One(),Default.Height.One());
public static Size MinValue = new Size(Default.Width.MinValue(),Default.Height.MinValue());
public static Size MaxValue = new Size(Default.Width.MaxValue(),Default.Height.MaxValue());
public static bool operator ==(Size a, Size b) => (a.Width == b.Width) && (a.Height == b.Height);
public static bool operator !=(Size a, Size b) => (a.Width != b.Width) || (a.Height != b.Height);
public Size WithWidth(Double value) => new Size(value, Height);
public Size WithHeight(Double value) => new Size(Width, value);
}
public static partial class Intrinsics {
public static Size Default(this Size _) => default(Size);
public static Size Zero(this Size _) => Size.Zero;
public static Size One(this Size _) => Size.One;
public static Size MinValue(this Size _) => Size.MinValue;
public static Size MaxValue(this Size _) => Size.MaxValue;
}
public partial struct Rectangle
{
public Rectangle(DVector2 topleft, Size size) => (TopLeft, Size) = (topleft, size);
public static implicit operator Rectangle((DVector2 TopLeft, Size Size) tuple) => new Rectangle(tuple.TopLeft, tuple.Size);
public static implicit operator (DVector2 TopLeft, Size Size)(Rectangle self) => (self.TopLeft, self.Size);
public void Deconstruct(out DVector2 topleft, out Size size) => (topleft, size) = (TopLeft, Size);
public override string ToString() => $"{{ \"TopLeft\" : { TopLeft }, \"Size\" : { Size } }}";
public override bool Equals(object other) => other is Rectangle typedOther && this == typedOther;
public override int GetHashCode() => (TopLeft, Size).GetHashCode();
public static readonly Rectangle Default = default;
public static Rectangle Zero = new Rectangle(Default.TopLeft.Zero(),Default.Size.Zero());
public static Rectangle One = new Rectangle(Default.TopLeft.One(),Default.Size.One());
public static Rectangle MinValue = new Rectangle(Default.TopLeft.MinValue(),Default.Size.MinValue());
public static Rectangle MaxValue = new Rectangle(Default.TopLeft.MaxValue(),Default.Size.MaxValue());
public static bool operator ==(Rectangle a, Rectangle b) => (a.TopLeft == b.TopLeft) && (a.Size == b.Size);
public static bool operator !=(Rectangle a, Rectangle b) => (a.TopLeft != b.TopLeft) || (a.Size != b.Size);
public Rectangle WithTopLeft(DVector2 value) => new Rectangle(value, Size);
public Rectangle WithSize(Size value) => new Rectangle(TopLeft, value);
}
public static partial class Intrinsics {
public static Rectangle Default(this Rectangle _) => default(Rectangle);
public static Rectangle Zero(this Rectangle _) => Rectangle.Zero;
public static Rectangle One(this Rectangle _) => Rectangle.One;
public static Rectangle MinValue(this Rectangle _) => Rectangle.MinValue;
public static Rectangle MaxValue(this Rectangle _) => Rectangle.MaxValue;
}
public partial struct Angle
{
public Angle(Double radians) => (Radians) = (radians);
public override string ToString() => $"{{ \"Radians\" : { Radians } }}";
public override bool Equals(object other) => other is Angle typedOther && this == typedOther;
public override int GetHashCode() => (Radians).GetHashCode();
public static readonly Angle Default = default;
public static Angle Zero = new Angle(Default.Radians.Zero());
public static Angle One = new Angle(Default.Radians.One());
public static Angle MinValue = new Angle(Default.Radians.MinValue());
public static Angle MaxValue = new Angle(Default.Radians.MaxValue());
public static bool operator ==(Angle a, Angle b) => (a.Radians == b.Radians);
public static bool operator !=(Angle a, Angle b) => (a.Radians != b.Radians);
public Angle WithRadians(Double value) => new Angle(value);
public static Angle operator +(Angle a, Angle b) => new Angle(a.Radians + b.Radians);
public static Angle operator -(Angle a, Angle b) => new Angle(a.Radians - b.Radians);
public static Angle operator *(Angle a, Angle b) => new Angle(a.Radians * b.Radians);
public static Angle operator /(Angle a, Angle b) => new Angle(a.Radians / b.Radians);
public static Angle operator -(Angle a) => new Angle(- a.Radians);
}
public static partial class Intrinsics {
public static Angle Add(this Angle a, Angle b) => a + b;
public static Angle Subtract(this Angle a, Angle b) => a - b;
public static Angle Multiply(this Angle a, Angle b) => a * b;
public static Angle Divide(this Angle a, Angle b) => a / b;
public static Angle Negate(this Angle a) => - a;
public static bool Equals(this Angle a, Angle b) => a == b;
public static bool NotEquals(this Angle a, Angle b) => a != b;
public static Angle Default(this Angle _) => default(Angle);
public static Angle Zero(this Angle _) => Angle.Zero;
public static Angle One(this Angle _) => Angle.One;
public static Angle MinValue(this Angle _) => Angle.MinValue;
public static Angle MaxValue(this Angle _) => Angle.MaxValue;
}
public partial struct Distance
{
public Distance(Double meters) => (Meters) = (meters);
public override string ToString() => $"{{ \"Meters\" : { Meters } }}";
public override bool Equals(object other) => other is Distance typedOther && this == typedOther;
public override int GetHashCode() => (Meters).GetHashCode();
public static readonly Distance Default = default;
public static Distance Zero = new Distance(Default.Meters.Zero());
public static Distance One = new Distance(Default.Meters.One());
public static Distance MinValue = new Distance(Default.Meters.MinValue());
public static Distance MaxValue = new Distance(Default.Meters.MaxValue());
public static bool operator ==(Distance a, Distance b) => (a.Meters == b.Meters);
public static bool operator !=(Distance a, Distance b) => (a.Meters != b.Meters);
public Distance WithMeters(Double value) => new Distance(value);
public static Distance operator +(Distance a, Distance b) => new Distance(a.Meters + b.Meters);
public static Distance operator -(Distance a, Distance b) => new Distance(a.Meters - b.Meters);
public static Distance operator *(Distance a, Distance b) => new Distance(a.Meters * b.Meters);
public static Distance operator /(Distance a, Distance b) => new Distance(a.Meters / b.Meters);
public static Distance operator -(Distance a) => new Distance(- a.Meters);
}
public static partial class Intrinsics {
public static Distance Add(this Distance a, Distance b) => a + b;
public static Distance Subtract(this Distance a, Distance b) => a - b;
public static Distance Multiply(this Distance a, Distance b) => a * b;
public static Distance Divide(this Distance a, Distance b) => a / b;
public static Distance Negate(this Distance a) => - a;
public static bool Equals(this Distance a, Distance b) => a == b;
public static bool NotEquals(this Distance a, Distance b) => a != b;
public static Distance Default(this Distance _) => default(Distance);
public static Distance Zero(this Distance _) => Distance.Zero;
public static Distance One(this Distance _) => Distance.One;
public static Distance MinValue(this Distance _) => Distance.MinValue;
public static Distance MaxValue(this Distance _) => Distance.MaxValue;
}
public partial struct Mass
{
public Mass(Double kilograms) => (Kilograms) = (kilograms);
public override string ToString() => $"{{ \"Kilograms\" : { Kilograms } }}";
public override bool Equals(object other) => other is Mass typedOther && this == typedOther;
public override int GetHashCode() => (Kilograms).GetHashCode();
public static readonly Mass Default = default;
public static Mass Zero = new Mass(Default.Kilograms.Zero());
public static Mass One = new Mass(Default.Kilograms.One());
public static Mass MinValue = new Mass(Default.Kilograms.MinValue());
public static Mass MaxValue = new Mass(Default.Kilograms.MaxValue());
public static bool operator ==(Mass a, Mass b) => (a.Kilograms == b.Kilograms);
public static bool operator !=(Mass a, Mass b) => (a.Kilograms != b.Kilograms);
public Mass WithKilograms(Double value) => new Mass(value);
public static Mass operator +(Mass a, Mass b) => new Mass(a.Kilograms + b.Kilograms);
public static Mass operator -(Mass a, Mass b) => new Mass(a.Kilograms - b.Kilograms);
public static Mass operator *(Mass a, Mass b) => new Mass(a.Kilograms * b.Kilograms);
public static Mass operator /(Mass a, Mass b) => new Mass(a.Kilograms / b.Kilograms);
public static Mass operator -(Mass a) => new Mass(- a.Kilograms);
}
public static partial class Intrinsics {
public static Mass Add(this Mass a, Mass b) => a + b;
public static Mass Subtract(this Mass a, Mass b) => a - b;
public static Mass Multiply(this Mass a, Mass b) => a * b;
public static Mass Divide(this Mass a, Mass b) => a / b;
public static Mass Negate(this Mass a) => - a;
public static bool Equals(this Mass a, Mass b) => a == b;
public static bool NotEquals(this Mass a, Mass b) => a != b;
public static Mass Default(this Mass _) => default(Mass);
public static Mass Zero(this Mass _) => Mass.Zero;
public static Mass One(this Mass _) => Mass.One;
public static Mass MinValue(this Mass _) => Mass.MinValue;
public static Mass MaxValue(this Mass _) => Mass.MaxValue;
}
public partial struct Duration
{
public Duration(Double value) => (Value) = (value);
public override string ToString() => $"{{ \"Value\" : { Value } }}";
public override bool Equals(object other) => other is Duration typedOther && this == typedOther;
public override int GetHashCode() => (Value).GetHashCode();
public static readonly Duration Default = default;
public static Duration Zero = new Duration(Default.Value.Zero());
public static Duration One = new Duration(Default.Value.One());
public static Duration MinValue = new Duration(Default.Value.MinValue());
public static Duration MaxValue = new Duration(Default.Value.MaxValue());
public static bool operator ==(Duration a, Duration b) => (a.Value == b.Value);
public static bool operator !=(Duration a, Duration b) => (a.Value != b.Value);
public Duration WithValue(Double value) => new Duration(value);
public static Duration operator +(Duration a, Duration b) => new Duration(a.Value + b.Value);
public static Duration operator -(Duration a, Duration b) => new Duration(a.Value - b.Value);
public static Duration operator *(Duration a, Duration b) => new Duration(a.Value * b.Value);
public static Duration operator /(Duration a, Duration b) => new Duration(a.Value / b.Value);
public static Duration operator -(Duration a) => new Duration(- a.Value);
}
public static partial class Intrinsics {
public static Duration Add(this Duration a, Duration b) => a + b;
public static Duration Subtract(this Duration a, Duration b) => a - b;
public static Duration Multiply(this Duration a, Duration b) => a * b;
public static Duration Divide(this Duration a, Duration b) => a / b;
public static Duration Negate(this Duration a) => - a;
public static bool Equals(this Duration a, Duration b) => a == b;
public static bool NotEquals(this Duration a, Duration b) => a != b;
public static Duration Default(this Duration _) => default(Duration);
public static Duration Zero(this Duration _) => Duration.Zero;
public static Duration One(this Duration _) => Duration.One;
public static Duration MinValue(this Duration _) => Duration.MinValue;
public static Duration MaxValue(this Duration _) => Duration.MaxValue;
}
} // End namespace
