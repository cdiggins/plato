namespace PlatoTestJavaScript {
//==begin==//
public interface IArray<T>
{
  /*
  int Count { get; }
  */
  public System.Int32 Count
  {
  get;
    }
  
  /*
  T this[int index] { get; }
  */
  public T this [System.Int32 index]
  
  {
  get;
    }
  }
//==end==//
//==begin==//
public class Array<T> : PlatoTestJavaScript.IArray<T>
{
  public System.Func<System.Int32, T> _Func_ ;
  public System.Int32 _Count_ ;
  /*
  public Array(int count, Func<int, T> func) => (Count, Func) = (count, func);
  */
  public Array(System.Int32 count, System.Func<System.Int32, T> func)
  {
      // Let declaration
      var _var135  = (count, func);
      _Count_ = _var135.Item1;
      _Func_ = _var135.Item2;
      }
    /*
  public Func<int, T> Func { get; }
  */
  public System.Func<System.Int32, T> Func
  {
  get
      {
        return _Func_;
        }
      }
  
  /*
  public int Count { get; }
  */
  public System.Int32 Count
  {
  get
      {
        return _Count_;
        }
      }
  
  /*
  public T this[int index] => Func(index);
  */
  public T this [System.Int32 index]
  
  {
  get
      {
        return _Func_(index);
        }
      }
  }
//==end==//
//==begin==//
public class Vector2
{
  /*
  X
  */
  public System.Single X ;
  /*
  Y
  */
  public System.Single Y ;
  /*
  public Vector2(float x = 0, float y = 0) => (X, Y) = (x, y);
  */
  public Vector2(System.Single x = 0, System.Single y = 0)
  {
      // Let declaration
      var _var141  = (x, y);
      X = _var141.Item1;
      Y = _var141.Item2;
      }
    /*
  public static Vector2 Zero => new();
  */
  public static PlatoTestJavaScript.Vector2 Zero
  {
  get
      {
        return new PlatoTestJavaScript.Vector2();
        }
      }
  
  /*
  public float MagnitudeSquared => Dot(this);
  */
  public System.Single MagnitudeSquared
  {
  get
      {
        return Dot(this);
        }
      }
  
  /*
  public float Magnitude => MathF.Sqrt(MagnitudeSquared);
  */
  public System.Single Magnitude
  {
  get
      {
        return System.MathF.Sqrt(MagnitudeSquared);
        }
      }
  
  /*
  public Vector2 Normal => this / Magnitude;
  */
  public PlatoTestJavaScript.Vector2 Normal
  {
  get
      {
        return this/Magnitude;
        }
      }
  
  /*
  public Vector2 WithX(float x) => new(x, Y);
  */
  public PlatoTestJavaScript.Vector2 WithX(System.Single x)
  {
    return new PlatoTestJavaScript.Vector2(x, Y);
    }
  /*
  public Vector2 WithY(float y) => new(X, y);
  */
  public PlatoTestJavaScript.Vector2 WithY(System.Single y)
  {
    return new PlatoTestJavaScript.Vector2(X, y);
    }
  /*
  public float Dot(Vector2 v) => X * v.X + v.Y * Y;
  */
  public System.Single Dot(PlatoTestJavaScript.Vector2 v)
  {
    return X*v.X+v.Y*Y;
    }
  /*
  public override string ToString() => "Vector2(" + X  + "," + Y + ")";
  */
  public System.String ToString()
  {
    return "Vector2("+X+","+Y+")";
    }
  /*
  public static implicit operator Vector2(float v) => new(v, v);
  */
  public static implicit operator PlatoTestJavaScript.Vector2(System.Single v)
  {
    return new PlatoTestJavaScript.Vector2(v, v);
    }
  /*
  public static Vector2 operator +(Vector2 q, Vector2 r) => new(q.X + r.X, q.Y + r.Y);
  */
  public static PlatoTestJavaScript.Vector2 operator +(PlatoTestJavaScript.Vector2 q, PlatoTestJavaScript.Vector2 r)
  {
    return new PlatoTestJavaScript.Vector2(q.X+r.X, q.Y+r.Y);
    }
  /*
  public static Vector2 operator *(Vector2 q, Vector2 r) => new(q.X * r.X, q.Y * r.Y);
  */
  public static PlatoTestJavaScript.Vector2 operator *(PlatoTestJavaScript.Vector2 q, PlatoTestJavaScript.Vector2 r)
  {
    return new PlatoTestJavaScript.Vector2(q.X*r.X, q.Y*r.Y);
    }
  /*
  public static Vector2 operator /(Vector2 q, Vector2 r) => new(q.X / r.X, q.Y / r.Y);
  */
  public static PlatoTestJavaScript.Vector2 operator /(PlatoTestJavaScript.Vector2 q, PlatoTestJavaScript.Vector2 r)
  {
    return new PlatoTestJavaScript.Vector2(q.X/r.X, q.Y/r.Y);
    }
  }
//==end==//
//==begin==//
public class Vector3 : PlatoTestJavaScript.IArray<System.Single>
{
  /*
  X
  */
  public System.Single X ;
  /*
  Y
  */
  public System.Single Y ;
  /*
  Z
  */
  public System.Single Z ;
  /*
  public Vector3(float x = 0, float y = 0, float z = 0) => (X, Y, Z) = (x, y, z);
  */
  public Vector3(System.Single x = 0, System.Single y = 0, System.Single z = 0)
  {
      // Let declaration
      var _var155  = (x, y, z);
      X = _var155.Item1;
      Y = _var155.Item2;
      Z = _var155.Item3;
      }
    /*
  public static Vector3 Zero => new();
  */
  public static PlatoTestJavaScript.Vector3 Zero
  {
  get
      {
        return new PlatoTestJavaScript.Vector3();
        }
      }
  
  /*
  public float MagnitudeSquared => Dot(this);
  */
  public System.Single MagnitudeSquared
  {
  get
      {
        return Dot(this);
        }
      }
  
  /*
  public float Magnitude => MathF.Sqrt(MagnitudeSquared);
  */
  public System.Single Magnitude
  {
  get
      {
        return System.MathF.Sqrt(MagnitudeSquared);
        }
      }
  
  /*
  public Vector3 Normal => this / Magnitude;
  */
  public PlatoTestJavaScript.Vector3 Normal
  {
  get
      {
        return this/Magnitude;
        }
      }
  
  /*
  public int Count => 3;
  */
  public System.Int32 Count
  {
  get
      {
        return 3;
        }
      }
  
  /*
  public Vector3 WithX(float x) => new(x, Y, Z);
  */
  public PlatoTestJavaScript.Vector3 WithX(System.Single x)
  {
    return new PlatoTestJavaScript.Vector3(x, Y, Z);
    }
  /*
  public Vector3 WithY(float y) => new(X, y, Z);
  */
  public PlatoTestJavaScript.Vector3 WithY(System.Single y)
  {
    return new PlatoTestJavaScript.Vector3(X, y, Z);
    }
  /*
  public Vector3 WithZ(float z) => new(X, Y, z);
  */
  public PlatoTestJavaScript.Vector3 WithZ(System.Single z)
  {
    return new PlatoTestJavaScript.Vector3(X, Y, z);
    }
  /*
  public float Dot(Vector3 v) => X * v.X + v.Y * Y + Z * v.Z;
  */
  public System.Single Dot(PlatoTestJavaScript.Vector3 v)
  {
    return X*v.X+v.Y*Y+Z*v.Z;
    }
  /*
  public override string ToString() => "Vector3(" + X + "," + Y + "," + Z + ")";
  */
  public System.String ToString()
  {
    return "Vector3("+X+","+Y+","+Z+")";
    }
  /*
  public Vector3 Cross(Vector3 v)
            => new(Y* v.Z - Z* v.Y, Z* v.X - X* v.Z, X* v.Y - Y* v.X);
  */
  public PlatoTestJavaScript.Vector3 Cross(PlatoTestJavaScript.Vector3 v)
  {
    return new PlatoTestJavaScript.Vector3(Y*v.Z-Z*v.Y, Z*v.X-X*v.Z, X*v.Y-Y*v.X);
    }
  /*
  public float this[int i] => i switch
        {
            0 => X,
            1 => Y,
            2 => Z,
            _ => throw new ArgumentOutOfRangeException()
        };
  */
  public System.Single this [System.Int32 i]
  
  {
  get
      {
        return i switch 
        {
          0 => X,
          1 => Y,
          2 => Z,
          _ => throw new System.ArgumentOutOfRangeException(),
          }
        ;
        }
      }
  /*
  public static implicit operator Vector3(float v) => new(v, v, v);
  */
  public static implicit operator PlatoTestJavaScript.Vector3(System.Single v)
  {
    return new PlatoTestJavaScript.Vector3(v, v, v);
    }
  /*
  public static Vector3 operator +(Vector3 q, Vector3 r) => new(q.X + r.X, q.Y + r.Y, q.Z + r.Z);
  */
  public static PlatoTestJavaScript.Vector3 operator +(PlatoTestJavaScript.Vector3 q, PlatoTestJavaScript.Vector3 r)
  {
    return new PlatoTestJavaScript.Vector3(q.X+r.X, q.Y+r.Y, q.Z+r.Z);
    }
  /*
  public static Vector3 operator -(Vector3 q, Vector3 r) => new(q.X - r.X, q.Y - r.Y, q.Z - r.Z);
  */
  public static PlatoTestJavaScript.Vector3 operator -(PlatoTestJavaScript.Vector3 q, PlatoTestJavaScript.Vector3 r)
  {
    return new PlatoTestJavaScript.Vector3(q.X-r.X, q.Y-r.Y, q.Z-r.Z);
    }
  /*
  public static Vector3 operator *(Vector3 q, Vector3 r) => new(q.X * r.X, q.Y * r.Y, q.Z * r.Z);
  */
  public static PlatoTestJavaScript.Vector3 operator *(PlatoTestJavaScript.Vector3 q, PlatoTestJavaScript.Vector3 r)
  {
    return new PlatoTestJavaScript.Vector3(q.X*r.X, q.Y*r.Y, q.Z*r.Z);
    }
  /*
  public static Vector3 operator /(Vector3 q, Vector3 r) => new(q.X / r.X, q.Y / r.Y, q.Z / r.Z);
  */
  public static PlatoTestJavaScript.Vector3 operator /(PlatoTestJavaScript.Vector3 q, PlatoTestJavaScript.Vector3 r)
  {
    return new PlatoTestJavaScript.Vector3(q.X/r.X, q.Y/r.Y, q.Z/r.Z);
    }
  }
//==end==//
//==begin==//
public class Int3 : PlatoTestJavaScript.IArray<System.Int32>
{
  /*
  X
  */
  public System.Int32 X ;
  /*
  Y
  */
  public System.Int32 Y ;
  /*
  Z
  */
  public System.Int32 Z ;
  /*
  public Int3(int x = 0, int y = 0, int z = 0) => (X, Y, Z) = (x, y, z);
  */
  public Int3(System.Int32 x = 0, System.Int32 y = 0, System.Int32 z = 0)
  {
      // Let declaration
      var _var174  = (x, y, z);
      X = _var174.Item1;
      Y = _var174.Item2;
      Z = _var174.Item3;
      }
    /*
  public static Int3 Zero => new();
  */
  public static PlatoTestJavaScript.Int3 Zero
  {
  get
      {
        return new PlatoTestJavaScript.Int3();
        }
      }
  
  /*
  public int Count => 3;
  */
  public System.Int32 Count
  {
  get
      {
        return 3;
        }
      }
  
  /*
  public Int3 WithX(int x) => new(x, Y, Z);
  */
  public PlatoTestJavaScript.Int3 WithX(System.Int32 x)
  {
    return new PlatoTestJavaScript.Int3(x, Y, Z);
    }
  /*
  public Int3 WithY(int y) => new(X, y, Z);
  */
  public PlatoTestJavaScript.Int3 WithY(System.Int32 y)
  {
    return new PlatoTestJavaScript.Int3(X, y, Z);
    }
  /*
  public Int3 WithZ(int z) => new(X, Y, z);
  */
  public PlatoTestJavaScript.Int3 WithZ(System.Int32 z)
  {
    return new PlatoTestJavaScript.Int3(X, Y, z);
    }
  /*
  public override string ToString() => "Int3(" + X + "," + Y + "," + Z + ")";
  */
  public System.String ToString()
  {
    return "Int3("+X+","+Y+","+Z+")";
    }
  /*
  public int this[int i] => i switch
        {
            0 => X,
            1 => Y,
            2 => Z,
            _ => throw new ArgumentOutOfRangeException()
        };
  */
  public System.Int32 this [System.Int32 i]
  
  {
  get
      {
        return i switch 
        {
          0 => X,
          1 => Y,
          2 => Z,
          _ => throw new System.ArgumentOutOfRangeException(),
          }
        ;
        }
      }
  }
//==end==//
//==begin==//
public class Int4 : PlatoTestJavaScript.IArray<System.Int32>
{
  /*
  X
  */
  public System.Int32 X ;
  /*
  Y
  */
  public System.Int32 Y ;
  /*
  Z
  */
  public System.Int32 Z ;
  /*
  W
  */
  public System.Int32 W ;
  /*
  public Int4(int x = 0, int y = 0, int z = 0, int w = 0) => (X, Y, Z, W) = (x, y, z, w);
  */
  public Int4(System.Int32 x = 0, System.Int32 y = 0, System.Int32 z = 0, System.Int32 w = 0)
  {
      // Let declaration
      var _var183  = (x, y, z, w);
      X = _var183.Item1;
      Y = _var183.Item2;
      Z = _var183.Item3;
      W = _var183.Item4;
      }
    /*
  public static Int4 Zero => new();
  */
  public static PlatoTestJavaScript.Int4 Zero
  {
  get
      {
        return new PlatoTestJavaScript.Int4();
        }
      }
  
  /*
  public int Count => 4;
  */
  public System.Int32 Count
  {
  get
      {
        return 4;
        }
      }
  
  /*
  public Int4 WithX(int x) => new(x, Y, Z, W);
  */
  public PlatoTestJavaScript.Int4 WithX(System.Int32 x)
  {
    return new PlatoTestJavaScript.Int4(x, Y, Z, W);
    }
  /*
  public Int4 WithY(int y) => new(X, y, Z, W);
  */
  public PlatoTestJavaScript.Int4 WithY(System.Int32 y)
  {
    return new PlatoTestJavaScript.Int4(X, y, Z, W);
    }
  /*
  public Int4 WithZ(int z) => new(X, Y, z, W);
  */
  public PlatoTestJavaScript.Int4 WithZ(System.Int32 z)
  {
    return new PlatoTestJavaScript.Int4(X, Y, z, W);
    }
  /*
  public Int4 WithW(int w) => new(X, Y, Z, w);
  */
  public PlatoTestJavaScript.Int4 WithW(System.Int32 w)
  {
    return new PlatoTestJavaScript.Int4(X, Y, Z, w);
    }
  /*
  public override string ToString() => "Int4(" + X + "," + Y + "," + Z + "," + W + ")";
  */
  public System.String ToString()
  {
    return "Int4("+X+","+Y+","+Z+","+W+")";
    }
  /*
  public int this[int i] => i switch
        {
            0 => X,
            1 => Y,
            2 => Z,
            3 => W,
            _ => throw new ArgumentOutOfRangeException()
        };
  */
  public System.Int32 this [System.Int32 i]
  
  {
  get
      {
        return i switch 
        {
          0 => X,
          1 => Y,
          2 => Z,
          3 => W,
          _ => throw new System.ArgumentOutOfRangeException(),
          }
        ;
        }
      }
  }
//==end==//
//==begin==//
public class Points
{
  public PlatoTestJavaScript.IArray<PlatoTestJavaScript.Vector3> _Positions_ ;
  public PlatoTestJavaScript.IArray<PlatoTestJavaScript.Vector2> _UVs_ ;
  public PlatoTestJavaScript.IArray<PlatoTestJavaScript.Vector3> _Normals_ ;
  /*
  public Points(IArray<Vector3> positions, IArray<Vector2> uvs, IArray<Vector3> normals)
            => (Positions, UVs, Normals) = (positions, uvs, normals);
  */
  public Points(PlatoTestJavaScript.IArray<PlatoTestJavaScript.Vector3> positions, PlatoTestJavaScript.IArray<PlatoTestJavaScript.Vector2> uvs, PlatoTestJavaScript.IArray<PlatoTestJavaScript.Vector3> normals)
  {
      // Let declaration
      var _var192  = (positions, uvs, normals);
      _Positions_ = _var192.Item1;
      _UVs_ = _var192.Item2;
      _Normals_ = _var192.Item3;
      }
    /*
  public IArray<Vector3> Positions { get; }
  */
  public PlatoTestJavaScript.IArray<PlatoTestJavaScript.Vector3> Positions
  {
  get
      {
        return _Positions_;
        }
      }
  
  /*
  public IArray<Vector2> UVs { get; }
  */
  public PlatoTestJavaScript.IArray<PlatoTestJavaScript.Vector2> UVs
  {
  get
      {
        return _UVs_;
        }
      }
  
  /*
  public IArray<Vector3> Normals { get; }
  */
  public PlatoTestJavaScript.IArray<PlatoTestJavaScript.Vector3> Normals
  {
  get
      {
        return _Normals_;
        }
      }
  
  }
//==end==//
//==begin==//
public class TriMesh
{
  public PlatoTestJavaScript.Points _Points_ ;
  public PlatoTestJavaScript.IArray<PlatoTestJavaScript.Int3> _Faces_ ;
  /*
  public TriMesh(Points points, IArray<Int3> faces)
            => (Points, Faces) = (points, faces);
  */
  public TriMesh(PlatoTestJavaScript.Points points, PlatoTestJavaScript.IArray<PlatoTestJavaScript.Int3> faces)
  {
      // Let declaration
      var _var198  = (points, faces);
      _Points_ = _var198.Item1;
      _Faces_ = _var198.Item2;
      }
    /*
  public Points Points { get; }
  */
  public PlatoTestJavaScript.Points Points
  {
  get
      {
        return _Points_;
        }
      }
  
  /*
  public IArray<Int3> Faces { get; }
  */
  public PlatoTestJavaScript.IArray<PlatoTestJavaScript.Int3> Faces
  {
  get
      {
        return _Faces_;
        }
      }
  
  }
//==end==//
//==begin==//
public class QuadMesh
{
  public PlatoTestJavaScript.Points _Points_ ;
  public PlatoTestJavaScript.IArray<PlatoTestJavaScript.Int4> _Faces_ ;
  /*
  public QuadMesh(Points points, IArray<Int4> faces)
            => (Points, Faces) = (points, faces);
  */
  public QuadMesh(PlatoTestJavaScript.Points points, PlatoTestJavaScript.IArray<PlatoTestJavaScript.Int4> faces)
  {
      // Let declaration
      var _var203  = (points, faces);
      _Points_ = _var203.Item1;
      _Faces_ = _var203.Item2;
      }
    /*
  public Points Points { get; }
  */
  public PlatoTestJavaScript.Points Points
  {
  get
      {
        return _Points_;
        }
      }
  
  /*
  public IArray<Int4> Faces { get; }
  */
  public PlatoTestJavaScript.IArray<PlatoTestJavaScript.Int4> Faces
  {
  get
      {
        return _Faces_;
        }
      }
  
  }
//==end==//
//==begin==//
public class Triangle
{
  public PlatoTestJavaScript.Vector3 _A_ ;
  public PlatoTestJavaScript.Vector3 _B_ ;
  public PlatoTestJavaScript.Vector3 _C_ ;
  /*
  public Triangle(Vector3 a, Vector3 b, Vector3 c)
            => (A, B, C) = (a, b, c);
  */
  public Triangle(PlatoTestJavaScript.Vector3 a, PlatoTestJavaScript.Vector3 b, PlatoTestJavaScript.Vector3 c)
  {
      // Let declaration
      var _var209  = (a, b, c);
      _A_ = _var209.Item1;
      _B_ = _var209.Item2;
      _C_ = _var209.Item3;
      }
    /*
  public Vector3 A { get; }
  */
  public PlatoTestJavaScript.Vector3 A
  {
  get
      {
        return _A_;
        }
      }
  
  /*
  public Vector3 B { get; }
  */
  public PlatoTestJavaScript.Vector3 B
  {
  get
      {
        return _B_;
        }
      }
  
  /*
  public Vector3 C { get; }
  */
  public PlatoTestJavaScript.Vector3 C
  {
  get
      {
        return _C_;
        }
      }
  
  /*
  public Vector3 Normal => (B - A).Cross(C - A).Normal;
  */
  public PlatoTestJavaScript.Vector3 Normal
  {
  get
      {
        return (_B_-_A_).Cross(_C_-_A_).Normal;
        }
      }
  
  }
//==end==//
//==begin==//
public static class Extensions
{
  /*
  public static IArray<T> ToIArray<T>(this IReadOnlyList<T> self)
            => self.Count.Select(i => self[i]);
  */
  public static PlatoTestJavaScript.IArray<T> ToIArray<T>(this System.Collections.Generic.IReadOnlyList<T> self)
  {
    return self.Count.Select<T>(/* Captured: self*/( i)
     => {
        return self[i];
        }
      );
    }
  /*
  public static T[] ToArray<T>(this IArray<T> self)
        {
            var r = new T[self.Count];
            for (var i = 0; i < self.Count; ++i)
                r[i] = self[i];
            return r;
        }
  */
  public static T[] ToArray<T>(this PlatoTestJavaScript.IArray<T> self)
  {
    T[] r  = new T[self.Count];
    {
      System.Int32 i  = 0;
      while(i<self.Count)
      {
          r[i] = self[i];
          ++i;
          }
        }
    return r;
    }
  /*
  public static float[] ToFloatArray(this IArray<Vector3> self)
            => self.SelectMany(x => x).ToArray();
  */
  public static System.Single[] ToFloatArray(this PlatoTestJavaScript.IArray<PlatoTestJavaScript.Vector3> self)
  {
    return self.SelectMany<PlatoTestJavaScript.Vector3, System.Single>(/* Captured: */( x)
     => {
        return x;
        }
      ).ToArray<System.Single>();
    }
  /*
  public static IArray<T> Select<T>(this int count, Func<int, T> func)
            => new Array<T>(count, func);
  */
  public static PlatoTestJavaScript.IArray<T> Select<T>(this System.Int32 count, System.Func<System.Int32, T> func)
  {
    return new PlatoTestJavaScript.Array<T>(count, func);
    }
  /*
  public static IArray<U> SelectMany<T, U>(this IArray<T> self, Func<T, IArray<U>> func)
        {
            var r = new List<U>();
            for (var i = 0; i < self.Count; ++i)
            {
                var tmp = func(self[i]);
                for (var j=0; j < tmp.Count; ++j)
                    r.Add(tmp[j]);
            }

            return r.Count.Select(i => r[i]);
        }
  */
  public static PlatoTestJavaScript.IArray<U> SelectMany<T, U>(this PlatoTestJavaScript.IArray<T> self, System.Func<T, PlatoTestJavaScript.IArray<U>> func)
  {
    System.Collections.Generic.List<U> r  = new System.Collections.Generic.List<U>();
    {
      System.Int32 i  = 0;
      while(i<self.Count)
      {
          {
            PlatoTestJavaScript.IArray<U> tmp  = func(self[i]);
            {
              System.Int32 j  = 0;
              while(j<tmp.Count)
              {
                  r.Add(tmp[j]);
                  ++j;
                  }
                }
            }
          ++i;
          }
        }
    return r.Count.Select<U>(/* Captured: ri*/( i)
     => {
        return r[i];
        }
      );
    }
  /*
  public static IArray<U> Select<T, U>(this IArray<T> self, Func<T, U> func)
            => self.Count.Select(i => func(self[i]));
  */
  public static PlatoTestJavaScript.IArray<U> Select<T, U>(this PlatoTestJavaScript.IArray<T> self, System.Func<T, U> func)
  {
    return self.Count.Select<U>(/* Captured: selffunc*/( i)
     => {
        return func(self[i]);
        }
      );
    }
  /*
  public static float Cos(this float self) 
            => MathF.Cos(self);
  */
  public static System.Single Cos(this System.Single self)
  {
    return System.MathF.Cos(self);
    }
  /*
  public static float Sin(this float self) 
            => MathF.Sin(self);
  */
  public static System.Single Sin(this System.Single self)
  {
    return System.MathF.Sin(self);
    }
  /*
  public static float UnitToRad(this float self)
            => self * MathF.PI;
  */
  public static System.Single UnitToRad(this System.Single self)
  {
    return self*System.MathF.PI;
    }
  /*
  public static IArray<float> SampleFloats(int count, float max = 1.0f)
            => count.Select(i => max * count);
  */
  public static PlatoTestJavaScript.IArray<System.Single> SampleFloats(System.Int32 count, System.Single max = 1.0f)
  {
    return count.Select<System.Single>(/* Captured: countmax*/( i)
     => {
        return max*count;
        }
      );
    }
  /*
  public static IArray<Int3> ToTriangleIndices(this IArray<Int4> self)
            => self.SelectMany(f => new[] { new Int3(f.X, f.Y, f.Z), new Int3(f.Z, f.W, f.X) }.ToIArray());
  */
  public static PlatoTestJavaScript.IArray<PlatoTestJavaScript.Int3> ToTriangleIndices(this PlatoTestJavaScript.IArray<PlatoTestJavaScript.Int4> self)
  {
    return self.SelectMany<PlatoTestJavaScript.Int4, PlatoTestJavaScript.Int3>(/* Captured: */( f)
     => {
        return new PlatoTestJavaScript.Int3[2]{new PlatoTestJavaScript.Int3(f.X, f.Y, f.Z), new PlatoTestJavaScript.Int3(f.Z, f.W, f.X)}.ToIArray<PlatoTestJavaScript.Int3>();
        }
      );
    }
  /*
  public static QuadMesh ToQuadMesh(this Func<Vector2, Vector3> func, int rows, int cols)
            => new (ComputeQuadStripPoints(func, rows, cols),
                ComputeQuadStripIndices(rows, cols));
  */
  public static PlatoTestJavaScript.QuadMesh ToQuadMesh(this System.Func<PlatoTestJavaScript.Vector2, PlatoTestJavaScript.Vector3> func, System.Int32 rows, System.Int32 cols)
  {
    return new PlatoTestJavaScript.QuadMesh(ComputeQuadStripPoints(func, rows, cols), ComputeQuadStripIndices(rows, cols));
    }
  /*
  public static Vector3 UVToNormal(this Vector2 uv, Func<Vector2, Vector3> func, float epsilon = 0.00001f)
        {
            var a = func(new(uv.X + epsilon, uv.Y));
            var b = func(new(uv.X - epsilon, uv.Y));
            var c = func(new(uv.X, uv.Y + epsilon));
            var d = func(new(uv.X, uv.Y - epsilon));
            var v1 = b - a;
            var v2 = d - c;
            var r = v1.Cross(v2);
            return r.Normal;
        }
  */
  public static PlatoTestJavaScript.Vector3 UVToNormal(this PlatoTestJavaScript.Vector2 uv, System.Func<PlatoTestJavaScript.Vector2, PlatoTestJavaScript.Vector3> func, System.Single epsilon = 0.00001f)
  {
    PlatoTestJavaScript.Vector3 a  = func(new PlatoTestJavaScript.Vector2(uv.X+epsilon, uv.Y));
    PlatoTestJavaScript.Vector3 b  = func(new PlatoTestJavaScript.Vector2(uv.X-epsilon, uv.Y));
    PlatoTestJavaScript.Vector3 c  = func(new PlatoTestJavaScript.Vector2(uv.X, uv.Y+epsilon));
    PlatoTestJavaScript.Vector3 d  = func(new PlatoTestJavaScript.Vector2(uv.X, uv.Y-epsilon));
    PlatoTestJavaScript.Vector3 v1  = b-a;
    PlatoTestJavaScript.Vector3 v2  = d-c;
    PlatoTestJavaScript.Vector3 r  = v1.Cross(v2);
    return r.Normal;
    }
  /*
  public static Points UVsToPoints(this IArray<Vector2> uvs, Func<Vector2, Vector3> func)
            => new(uvs.Select(func), uvs, uvs.Select(uv => uv.UVToNormal(func)));
  */
  public static PlatoTestJavaScript.Points UVsToPoints(this PlatoTestJavaScript.IArray<PlatoTestJavaScript.Vector2> uvs, System.Func<PlatoTestJavaScript.Vector2, PlatoTestJavaScript.Vector3> func)
  {
    return new PlatoTestJavaScript.Points(uvs.Select<PlatoTestJavaScript.Vector2, PlatoTestJavaScript.Vector3>(func), uvs, uvs.Select<PlatoTestJavaScript.Vector2, PlatoTestJavaScript.Vector3>(/* Captured: func*/( uv)
     => {
        return uv.UVToNormal(func);
        }
      ));
    }
  /*
  public static Points ComputeQuadStripPoints(this Func<Vector2, Vector3> func, int usegs, int vsegs)
            => ComputeQuadStripUVs(usegs, vsegs).UVsToPoints(func);
  */
  public static PlatoTestJavaScript.Points ComputeQuadStripPoints(this System.Func<PlatoTestJavaScript.Vector2, PlatoTestJavaScript.Vector3> func, System.Int32 usegs, System.Int32 vsegs)
  {
    return ComputeQuadStripUVs(usegs, vsegs).UVsToPoints(func);
    }
  /*
  public static IArray<Vector2> ComputeQuadStripUVs(int usegs, int vsegs)
            => new Array<Vector2>(usegs * vsegs, 
                i =>
                {
                    var row = i / vsegs;
                    var col = i % usegs;
                    return new((float)col / (usegs - 1), (float)row / (vsegs - 1));
                });
  */
  public static PlatoTestJavaScript.IArray<PlatoTestJavaScript.Vector2> ComputeQuadStripUVs(System.Int32 usegs, System.Int32 vsegs)
  {
    return new PlatoTestJavaScript.Array<PlatoTestJavaScript.Vector2>(usegs*vsegs, /* Captured: usegsvsegs*/( i)
     => {
        System.Int32 row  = i/vsegs;
        System.Int32 col  = i%usegs;
        return new PlatoTestJavaScript.Vector2((System.Single)col/(usegs-1), (System.Single)row/(vsegs-1));
        }
      );
    }
  /*
  public static IArray<Int4> ComputeQuadStripIndices(int usegs, int vsegs)
        {
            return new Array<Int4>((usegs - 1) * (vsegs - 1), i =>
            {
                var row = i / (vsegs - 1);
                var col = i % (usegs - 1);
                var nextCol = (col + 1);
                var nextRow = (row + 1);
                var a = (row * usegs) + col;
                var b = (row * usegs) + nextCol;
                var c = (nextRow * usegs) + nextCol;
                var d = (nextRow * usegs) + col;
                return new(a, b, c, d);
            });
        }
  */
  public static PlatoTestJavaScript.IArray<PlatoTestJavaScript.Int4> ComputeQuadStripIndices(System.Int32 usegs, System.Int32 vsegs)
  {
    return new PlatoTestJavaScript.Array<PlatoTestJavaScript.Int4>((usegs-1)*(vsegs-1), /* Captured: usegsvsegs*/( i)
     => {
        System.Int32 row  = i/(vsegs-1);
        System.Int32 col  = i%(usegs-1);
        System.Int32 nextCol  = (col+1);
        System.Int32 nextRow  = (row+1);
        System.Int32 a  = (row*usegs)+col;
        System.Int32 b  = (row*usegs)+nextCol;
        System.Int32 c  = (nextRow*usegs)+nextCol;
        System.Int32 d  = (nextRow*usegs)+col;
        return new PlatoTestJavaScript.Int4(a, b, c, d);
        }
      );
    }
  /*
  public static Vector3 UvToSphere(Vector2 uv, float radius)
            => new(
                -radius * uv.X.UnitToRad().Cos() * (uv.Y * MathF.PI).Sin(),
                radius * (uv.Y * MathF.PI).Cos(),
                radius * uv.X.UnitToRad().Cos() * (uv.Y * MathF.PI).Sin());
  */
  public static PlatoTestJavaScript.Vector3 UvToSphere(PlatoTestJavaScript.Vector2 uv, System.Single radius)
  {
    return new PlatoTestJavaScript.Vector3(-radius*uv.X.UnitToRad().Cos()*(uv.Y*System.MathF.PI).Sin(), radius*(uv.Y*System.MathF.PI).Cos(), radius*uv.X.UnitToRad().Cos()*(uv.Y*System.MathF.PI).Sin());
    }
  /*
  public static Vector3 UvToTorus(Vector2 uv, float radius, float tube)
        {
            uv = uv * MathF.PI * 2;
            return new Vector3(
                (radius + tube * uv.Y.Cos()) * uv.X.Cos(),
                (radius + tube * uv.Y.Cos()) * uv.X.Sin(),
                tube * uv.Y.Sin());
        }
  */
  public static PlatoTestJavaScript.Vector3 UvToTorus(PlatoTestJavaScript.Vector2 uv, System.Single radius, System.Single tube)
  {
    uv = uv*System.MathF.PI*2;
    return new PlatoTestJavaScript.Vector3((radius+tube*uv.Y.Cos())*uv.X.Cos(), (radius+tube*uv.Y.Cos())*uv.X.Sin(), tube*uv.Y.Sin());
    }
  /*
  public static TriMesh ToTriMesh(this QuadMesh mesh)
            => new(mesh.Points, mesh.Faces.ToTriangleIndices());
  */
  public static PlatoTestJavaScript.TriMesh ToTriMesh(this PlatoTestJavaScript.QuadMesh mesh)
  {
    return new PlatoTestJavaScript.TriMesh(mesh._Points_, mesh._Faces_.ToTriangleIndices());
    }
  /*
  public static void TestOperator()
        {
            var x = new Vector3(1, 2, 3);
            var y = x + x;
        }
  */
  public static void TestOperator()
  {
    PlatoTestJavaScript.Vector3 x  = new PlatoTestJavaScript.Vector3(1, 2, 3);
    PlatoTestJavaScript.Vector3 y  = x+x;
    }
  /*
  public static QuadMesh Torus(int rows, int cols, float radius, float tube)
            => ToQuadMesh(uv => UvToTorus(uv, radius, tube), rows, cols);
  */
  public static PlatoTestJavaScript.QuadMesh Torus(System.Int32 rows, System.Int32 cols, System.Single radius, System.Single tube)
  {
    return ToQuadMesh(/* Captured: radiustube*/( uv)
     => {
        return UvToTorus(uv, radius, tube);
        }
      , rows, cols);
    }
  /*
  public static int[] ToIntArray(this IArray<Int3> faces)
            => faces.SelectMany(f => f).ToArray();
  */
  public static System.Int32[] ToIntArray(this PlatoTestJavaScript.IArray<PlatoTestJavaScript.Int3> faces)
  {
    return faces.SelectMany<PlatoTestJavaScript.Int3, System.Int32>(/* Captured: */( f)
     => {
        return f;
        }
      ).ToArray<System.Int32>();
    }
  /*
  public static IArray<Triangle> Triangles(this TriMesh mesh)
            => mesh.Faces.Select(f => new Triangle(mesh.Points.Positions[f.X], mesh.Points.Positions[f.Y], mesh.Points.Positions[f.Z]));
  */
  public static PlatoTestJavaScript.IArray<PlatoTestJavaScript.Triangle> Triangles(this PlatoTestJavaScript.TriMesh mesh)
  {
    return mesh._Faces_.Select<PlatoTestJavaScript.Int3, PlatoTestJavaScript.Triangle>(/* Captured: mesh*/( f)
     => {
        return new PlatoTestJavaScript.Triangle(mesh._Points_._Positions_[f.X], mesh._Points_._Positions_[f.Y], mesh._Points_._Positions_[f.Z]);
        }
      );
    }
  /*
  public static IArray<Vector3> FaceNormals(this TriMesh mesh)
            => mesh.Triangles().Select(tri => tri.Normal);
  */
  public static PlatoTestJavaScript.IArray<PlatoTestJavaScript.Vector3> FaceNormals(this PlatoTestJavaScript.TriMesh mesh)
  {
    return mesh.Triangles().Select<PlatoTestJavaScript.Triangle, PlatoTestJavaScript.Vector3>(/* Captured: */( tri)
     => {
        return tri.Normal;
        }
      );
    }
  /*
  public static (T, TimeSpan) TimeIt<T>(Func<T> func)
        {
            var sw = Stopwatch.StartNew();
            return (func(), sw.Elapsed);
        }
  */
  public static System.ValueTuple<T, System.TimeSpan> TimeIt<T>(System.Func<T> func)
  {
    System.Diagnostics.Stopwatch sw  = System.Diagnostics.Stopwatch.StartNew();
    return (func(), sw.Elapsed);
    }
  /*
  public static void Log(string s)
            => Debug.WriteLine(s);
  */
  public static void Log(System.String s)
  {
    System.Diagnostics.Debug.WriteLine(s);
    }
  /*
  public static T LogTiming<T>(Func<T> func)
        {
            var r = TimeIt(func);
            Debug.WriteLine("msec elapsed: " + r.Item2.Milliseconds);
            return r.Item1;
        }
  */
  public static T LogTiming<T>(System.Func<T> func)
  {
    System.ValueTuple<T, System.TimeSpan> r  = TimeIt<T>(func);
    System.Console.WriteLine("msec elapsed: "+r.Item2.Milliseconds);
    return r.Item1;
    }
  /*
  public static void Main()
        {
            var torus = Torus(500, 100, 1, 0.2f).ToTriMesh();
            var floats = LogTiming(torus.FaceNormals().ToFloatArray);
            var filePath = Path.Combine(Path.GetTempPath(), "profiling.txt");
            File.WriteAllLines(filePath, floats.Select(f => f.ToString()));
        }
  */
  public static void Main()
  {
    PlatoTestJavaScript.TriMesh torus  = Torus(5000, 1000, 1, 0.2f).ToTriMesh();
    System.Single[] floats  = LogTiming<System.Single[]>(torus.FaceNormals().ToFloatArray);
    System.String filePath  = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "profiling.txt");
    System.IO.File.WriteAllLines(filePath, floats.Select<System.Single, System.String>(/* Captured: */( f)
     => {
        return f.ToString();
        }
      ));
    }
  }
//==end==//
}
