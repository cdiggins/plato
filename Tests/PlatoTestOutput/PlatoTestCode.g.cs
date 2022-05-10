using System.Runtime.CompilerServices;
namespace PlatoTest {
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
public readonly struct Array<T> : PlatoTest.IArray<T>
{
  public readonly System.Func<System.Int32, T> _Func_ ;
  public readonly System.Int32 _Count_ ;
  /*
  public Array(int count, Func<int, T> func) => (Count, Func) = (count, func);
  */
  public Array(System.Int32 count, System.Func<System.Int32, T> func)
  {
      // Let declaration
      var _var140  = (count, func);
      _Count_ = _var140.Item1;
      _Func_ = _var140.Item2;
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
public readonly struct Vector2
{
  /*
  X
  */
  public readonly System.Single X ;
  /*
  Y
  */
  public readonly System.Single Y ;
  /*
  public Vector2(float x = 0, float y = 0) => (X, Y) = (x, y);
  */
  public Vector2(System.Single x = 0, System.Single y = 0)
  {
      // Let declaration
      var _var146  = (x, y);
      X = _var146.Item1;
      Y = _var146.Item2;
      }
    /*
  public static Vector2 Zero => new();
  */
  public static PlatoTest.Vector2 Zero
  {
  get
      {
        return new PlatoTest.Vector2();
        }
      }
  
  /*
  public float MagnitudeSquared => Dot(this);
  */
  public System.Single MagnitudeSquared
  {
  get
      {
        /*
        <generated>
        */
        System.Single result_0  = default(System.Single);
        {
          {
            result_0 = X*this.X+this.Y*Y;
            }
          }
        return result_0;
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
  public PlatoTest.Vector2 Normal
  {
  get
      {
        /*
        <generated>
        */
        PlatoTest.Vector2 result_1  = default(PlatoTest.Vector2);
        {
          {
            result_1 = new PlatoTest.Vector2(this.X/Magnitude.X, this.Y/Magnitude.Y);
            }
          }
        return result_1;
        }
      }
  
  /*
  public Vector2 WithX(float x) => new(x, Y);
  */
  [MethodImpl(MethodImplOptions.AggressiveInlining)]public PlatoTest.Vector2 _inlined_WithX(System.Single x)
  {
    return new PlatoTest.Vector2(x, Y);
    }
  /*
  public Vector2 WithY(float y) => new(X, y);
  */
  [MethodImpl(MethodImplOptions.AggressiveInlining)]public PlatoTest.Vector2 _inlined_WithY(System.Single y)
  {
    return new PlatoTest.Vector2(X, y);
    }
  /*
  public float Dot(Vector2 v) => X * v.X + v.Y * Y;
  */
  [MethodImpl(MethodImplOptions.AggressiveInlining)]public System.Single _inlined_Dot(PlatoTest.Vector2 v)
  {
    return X*this.X+this.Y*Y;
    }
  /*
  public override string ToString() => "Vector2(" + X  + "," + Y + ")";
  */
  [MethodImpl(MethodImplOptions.AggressiveInlining)]public System.String _inlined_ToString()
  {
    return "Vector2("+X+","+Y+")";
    }
  /*
  public static implicit operator Vector2(float v) => new(v, v);
  */
  [MethodImpl(MethodImplOptions.AggressiveInlining)]public static implicit operator PlatoTest.Vector2(System.Single v)
  {
    return new PlatoTest.Vector2(v, v);
    }
  /*
  public static Vector2 operator +(Vector2 q, Vector2 r) => new(q.X + r.X, q.Y + r.Y);
  */
  [MethodImpl(MethodImplOptions.AggressiveInlining)]public static PlatoTest.Vector2 operator +(PlatoTest.Vector2 q, PlatoTest.Vector2 r)
  {
    return new PlatoTest.Vector2(q.X+r.X, q.Y+r.Y);
    }
  /*
  public static Vector2 operator *(Vector2 q, Vector2 r) => new(q.X * r.X, q.Y * r.Y);
  */
  [MethodImpl(MethodImplOptions.AggressiveInlining)]public static PlatoTest.Vector2 operator *(PlatoTest.Vector2 q, PlatoTest.Vector2 r)
  {
    return new PlatoTest.Vector2(q.X*r.X, q.Y*r.Y);
    }
  /*
  public static Vector2 operator /(Vector2 q, Vector2 r) => new(q.X / r.X, q.Y / r.Y);
  */
  [MethodImpl(MethodImplOptions.AggressiveInlining)]public static PlatoTest.Vector2 operator /(PlatoTest.Vector2 q, PlatoTest.Vector2 r)
  {
    return new PlatoTest.Vector2(this.X/Magnitude.X, this.Y/Magnitude.Y);
    }
  }
//==end==//
//==begin==//
public readonly struct Vector3 : PlatoTest.IArray<System.Single>
{
  /*
  X
  */
  public readonly System.Single X ;
  /*
  Y
  */
  public readonly System.Single Y ;
  /*
  Z
  */
  public readonly System.Single Z ;
  /*
  public Vector3(float x = 0, float y = 0, float z = 0) => (X, Y, Z) = (x, y, z);
  */
  public Vector3(System.Single x = 0, System.Single y = 0, System.Single z = 0)
  {
      // Let declaration
      var _var160  = (x, y, z);
      X = _var160.Item1;
      Y = _var160.Item2;
      Z = _var160.Item3;
      }
    /*
  public static Vector3 Zero => new();
  */
  public static PlatoTest.Vector3 Zero
  {
  get
      {
        return new PlatoTest.Vector3();
        }
      }
  
  /*
  public float MagnitudeSquared => Dot(this);
  */
  public System.Single MagnitudeSquared
  {
  get
      {
        /*
        <generated>
        */
        System.Single result_2  = default(System.Single);
        {
          {
            result_2 = X*this.X+this.Y*Y+Z*this.Z;
            }
          }
        return result_2;
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
  public PlatoTest.Vector3 Normal
  {
  get
      {
        /*
        <generated>
        */
        PlatoTest.Vector3 result_3  = default(PlatoTest.Vector3);
        {
          {
            result_3 = new PlatoTest.Vector3(this.X/Magnitude.X, this.Y/Magnitude.Y, this.Z/Magnitude.Z);
            }
          }
        return result_3;
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
  [MethodImpl(MethodImplOptions.AggressiveInlining)]public PlatoTest.Vector3 _inlined_WithX(System.Single x)
  {
    return new PlatoTest.Vector3(x, Y, Z);
    }
  /*
  public Vector3 WithY(float y) => new(X, y, Z);
  */
  [MethodImpl(MethodImplOptions.AggressiveInlining)]public PlatoTest.Vector3 _inlined_WithY(System.Single y)
  {
    return new PlatoTest.Vector3(X, y, Z);
    }
  /*
  public Vector3 WithZ(float z) => new(X, Y, z);
  */
  [MethodImpl(MethodImplOptions.AggressiveInlining)]public PlatoTest.Vector3 _inlined_WithZ(System.Single z)
  {
    return new PlatoTest.Vector3(X, Y, z);
    }
  /*
  public float Dot(Vector3 v) => X * v.X + v.Y * Y + Z * v.Z;
  */
  [MethodImpl(MethodImplOptions.AggressiveInlining)]public System.Single _inlined_Dot(PlatoTest.Vector3 v)
  {
    return X*this.X+this.Y*Y+Z*this.Z;
    }
  /*
  public override string ToString() => "Vector3(" + X + "," + Y + "," + Z + ")";
  */
  [MethodImpl(MethodImplOptions.AggressiveInlining)]public System.String _inlined_ToString()
  {
    return "Vector3("+X+","+Y+","+Z+")";
    }
  /*
  public Vector3 Cross(Vector3 v)
            => new(Y* v.Z - Z* v.Y, Z* v.X - X* v.Z, X* v.Y - Y* v.X);
  */
  [MethodImpl(MethodImplOptions.AggressiveInlining)]public PlatoTest.Vector3 _inlined_Cross(PlatoTest.Vector3 v)
  {
    return new PlatoTest.Vector3(Y*v2.Z-Z*v2.Y, Z*v2.X-X*v2.Z, X*v2.Y-Y*v2.X);
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
  [MethodImpl(MethodImplOptions.AggressiveInlining)]public static implicit operator PlatoTest.Vector3(System.Single v)
  {
    return new PlatoTest.Vector3(v, v, v);
    }
  /*
  public static Vector3 operator +(Vector3 q, Vector3 r) => new(q.X + r.X, q.Y + r.Y, q.Z + r.Z);
  */
  [MethodImpl(MethodImplOptions.AggressiveInlining)]public static PlatoTest.Vector3 operator +(PlatoTest.Vector3 q, PlatoTest.Vector3 r)
  {
    return new PlatoTest.Vector3(x.X+x.X, x.Y+x.Y, x.Z+x.Z);
    }
  /*
  public static Vector3 operator -(Vector3 q, Vector3 r) => new(q.X - r.X, q.Y - r.Y, q.Z - r.Z);
  */
  [MethodImpl(MethodImplOptions.AggressiveInlining)]public static PlatoTest.Vector3 operator -(PlatoTest.Vector3 q, PlatoTest.Vector3 r)
  {
    return new PlatoTest.Vector3(b.X-a.X, b.Y-a.Y, b.Z-a.Z);
    }
  /*
  public static Vector3 operator *(Vector3 q, Vector3 r) => new(q.X * r.X, q.Y * r.Y, q.Z * r.Z);
  */
  [MethodImpl(MethodImplOptions.AggressiveInlining)]public static PlatoTest.Vector3 operator *(PlatoTest.Vector3 q, PlatoTest.Vector3 r)
  {
    return new PlatoTest.Vector3(q.X*r.X, q.Y*r.Y, q.Z*r.Z);
    }
  /*
  public static Vector3 operator /(Vector3 q, Vector3 r) => new(q.X / r.X, q.Y / r.Y, q.Z / r.Z);
  */
  [MethodImpl(MethodImplOptions.AggressiveInlining)]public static PlatoTest.Vector3 operator /(PlatoTest.Vector3 q, PlatoTest.Vector3 r)
  {
    return new PlatoTest.Vector3(this.X/Magnitude.X, this.Y/Magnitude.Y, this.Z/Magnitude.Z);
    }
  }
//==end==//
//==begin==//
public readonly struct Int3 : PlatoTest.IArray<System.Int32>
{
  /*
  X
  */
  public readonly System.Int32 X ;
  /*
  Y
  */
  public readonly System.Int32 Y ;
  /*
  Z
  */
  public readonly System.Int32 Z ;
  /*
  public Int3(int x = 0, int y = 0, int z = 0) => (X, Y, Z) = (x, y, z);
  */
  public Int3(System.Int32 x = 0, System.Int32 y = 0, System.Int32 z = 0)
  {
      // Let declaration
      var _var179  = (x, y, z);
      X = _var179.Item1;
      Y = _var179.Item2;
      Z = _var179.Item3;
      }
    /*
  public static Int3 Zero => new();
  */
  public static PlatoTest.Int3 Zero
  {
  get
      {
        return new PlatoTest.Int3();
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
  [MethodImpl(MethodImplOptions.AggressiveInlining)]public PlatoTest.Int3 _inlined_WithX(System.Int32 x)
  {
    return new PlatoTest.Int3(x, Y, Z);
    }
  /*
  public Int3 WithY(int y) => new(X, y, Z);
  */
  [MethodImpl(MethodImplOptions.AggressiveInlining)]public PlatoTest.Int3 _inlined_WithY(System.Int32 y)
  {
    return new PlatoTest.Int3(X, y, Z);
    }
  /*
  public Int3 WithZ(int z) => new(X, Y, z);
  */
  [MethodImpl(MethodImplOptions.AggressiveInlining)]public PlatoTest.Int3 _inlined_WithZ(System.Int32 z)
  {
    return new PlatoTest.Int3(X, Y, z);
    }
  /*
  public override string ToString() => "Int3(" + X + "," + Y + "," + Z + ")";
  */
  [MethodImpl(MethodImplOptions.AggressiveInlining)]public System.String _inlined_ToString()
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
public readonly struct Int4 : PlatoTest.IArray<System.Int32>
{
  /*
  X
  */
  public readonly System.Int32 X ;
  /*
  Y
  */
  public readonly System.Int32 Y ;
  /*
  Z
  */
  public readonly System.Int32 Z ;
  /*
  W
  */
  public readonly System.Int32 W ;
  /*
  public Int4(int x = 0, int y = 0, int z = 0, int w = 0) => (X, Y, Z, W) = (x, y, z, w);
  */
  public Int4(System.Int32 x = 0, System.Int32 y = 0, System.Int32 z = 0, System.Int32 w = 0)
  {
      // Let declaration
      var _var188  = (x, y, z, w);
      X = _var188.Item1;
      Y = _var188.Item2;
      Z = _var188.Item3;
      W = _var188.Item4;
      }
    /*
  public static Int4 Zero => new();
  */
  public static PlatoTest.Int4 Zero
  {
  get
      {
        return new PlatoTest.Int4();
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
  [MethodImpl(MethodImplOptions.AggressiveInlining)]public PlatoTest.Int4 _inlined_WithX(System.Int32 x)
  {
    return new PlatoTest.Int4(x, Y, Z, W);
    }
  /*
  public Int4 WithY(int y) => new(X, y, Z, W);
  */
  [MethodImpl(MethodImplOptions.AggressiveInlining)]public PlatoTest.Int4 _inlined_WithY(System.Int32 y)
  {
    return new PlatoTest.Int4(X, y, Z, W);
    }
  /*
  public Int4 WithZ(int z) => new(X, Y, z, W);
  */
  [MethodImpl(MethodImplOptions.AggressiveInlining)]public PlatoTest.Int4 _inlined_WithZ(System.Int32 z)
  {
    return new PlatoTest.Int4(X, Y, z, W);
    }
  /*
  public Int4 WithW(int w) => new(X, Y, Z, w);
  */
  [MethodImpl(MethodImplOptions.AggressiveInlining)]public PlatoTest.Int4 _inlined_WithW(System.Int32 w)
  {
    return new PlatoTest.Int4(X, Y, Z, w);
    }
  /*
  public override string ToString() => "Int4(" + X + "," + Y + "," + Z + "," + W + ")";
  */
  [MethodImpl(MethodImplOptions.AggressiveInlining)]public System.String _inlined_ToString()
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
public readonly struct Points
{
  public readonly PlatoTest.IArray<PlatoTest.Vector3> _Positions_ ;
  public readonly PlatoTest.IArray<PlatoTest.Vector2> _UVs_ ;
  public readonly PlatoTest.IArray<PlatoTest.Vector3> _Normals_ ;
  /*
  public Points(IArray<Vector3> positions, IArray<Vector2> uvs, IArray<Vector3> normals)
            => (Positions, UVs, Normals) = (positions, uvs, normals);
  */
  public Points(PlatoTest.IArray<PlatoTest.Vector3> positions, PlatoTest.IArray<PlatoTest.Vector2> uvs, PlatoTest.IArray<PlatoTest.Vector3> normals)
  {
      // Let declaration
      var _var197  = (positions, uvs, normals);
      _Positions_ = _var197.Item1;
      _UVs_ = _var197.Item2;
      _Normals_ = _var197.Item3;
      }
    /*
  public IArray<Vector3> Positions { get; }
  */
  public PlatoTest.IArray<PlatoTest.Vector3> Positions
  {
  get
      {
        return _Positions_;
        }
      }
  
  /*
  public IArray<Vector2> UVs { get; }
  */
  public PlatoTest.IArray<PlatoTest.Vector2> UVs
  {
  get
      {
        return _UVs_;
        }
      }
  
  /*
  public IArray<Vector3> Normals { get; }
  */
  public PlatoTest.IArray<PlatoTest.Vector3> Normals
  {
  get
      {
        return _Normals_;
        }
      }
  
  }
//==end==//
//==begin==//
public readonly struct TriMesh
{
  public readonly PlatoTest.Points _Points_ ;
  public readonly PlatoTest.IArray<PlatoTest.Int3> _Faces_ ;
  /*
  public TriMesh(Points points, IArray<Int3> faces)
            => (Points, Faces) = (points, faces);
  */
  public TriMesh(PlatoTest.Points points, PlatoTest.IArray<PlatoTest.Int3> faces)
  {
      // Let declaration
      var _var203  = (points, faces);
      _Points_ = _var203.Item1;
      _Faces_ = _var203.Item2;
      }
    /*
  public Points Points { get; }
  */
  public PlatoTest.Points Points
  {
  get
      {
        return _Points_;
        }
      }
  
  /*
  public IArray<Int3> Faces { get; }
  */
  public PlatoTest.IArray<PlatoTest.Int3> Faces
  {
  get
      {
        return _Faces_;
        }
      }
  
  }
//==end==//
//==begin==//
public readonly struct QuadMesh
{
  public readonly PlatoTest.Points _Points_ ;
  public readonly PlatoTest.IArray<PlatoTest.Int4> _Faces_ ;
  /*
  public QuadMesh(Points points, IArray<Int4> faces)
            => (Points, Faces) = (points, faces);
  */
  public QuadMesh(PlatoTest.Points points, PlatoTest.IArray<PlatoTest.Int4> faces)
  {
      // Let declaration
      var _var208  = (points, faces);
      _Points_ = _var208.Item1;
      _Faces_ = _var208.Item2;
      }
    /*
  public Points Points { get; }
  */
  public PlatoTest.Points Points
  {
  get
      {
        return _Points_;
        }
      }
  
  /*
  public IArray<Int4> Faces { get; }
  */
  public PlatoTest.IArray<PlatoTest.Int4> Faces
  {
  get
      {
        return _Faces_;
        }
      }
  
  }
//==end==//
//==begin==//
public readonly struct Triangle
{
  public readonly PlatoTest.Vector3 _A_ ;
  public readonly PlatoTest.Vector3 _B_ ;
  public readonly PlatoTest.Vector3 _C_ ;
  /*
  public Triangle(Vector3 a, Vector3 b, Vector3 c)
            => (A, B, C) = (a, b, c);
  */
  public Triangle(PlatoTest.Vector3 a, PlatoTest.Vector3 b, PlatoTest.Vector3 c)
  {
      // Let declaration
      var _var214  = (a, b, c);
      _A_ = _var214.Item1;
      _B_ = _var214.Item2;
      _C_ = _var214.Item3;
      }
    /*
  public Vector3 A { get; }
  */
  public PlatoTest.Vector3 A
  {
  get
      {
        return _A_;
        }
      }
  
  /*
  public Vector3 B { get; }
  */
  public PlatoTest.Vector3 B
  {
  get
      {
        return _B_;
        }
      }
  
  /*
  public Vector3 C { get; }
  */
  public PlatoTest.Vector3 C
  {
  get
      {
        return _C_;
        }
      }
  
  /*
  public Vector3 Normal => (B - A).Cross(C - A).Normal;
  */
  public PlatoTest.Vector3 Normal
  {
  get
      {
        return (_B_-_A_)._inlined_Cross(_C_-_A_).Normal;
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
  [MethodImpl(MethodImplOptions.AggressiveInlining)]public static PlatoTest.IArray<T> _inlined_ToIArray<T>(this System.Collections.Generic.IReadOnlyList<T> self)
  {
    /*
    <generated>
    */
    PlatoTest.IArray<T> result_4  = default(PlatoTest.IArray<T>);
    {
      {
        result_4 = new PlatoTest.Array<T>(self.Count, /* Captured: self*/( i)
         => {
            return self[i];
            }
          );
        }
      }
    return result_4;
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
  [MethodImpl(MethodImplOptions.AggressiveInlining)]public static T[] _inlined_ToArray<T>(this PlatoTest.IArray<System.Single> self)
  {
    System.Single[] r  = new System.Single[self.Count];
    {
      System.Int32 i  = 0;
      while(i<self_7.Count)
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
  [MethodImpl(MethodImplOptions.AggressiveInlining)]public static System.Single[] _inlined_ToFloatArray(this PlatoTest.IArray<PlatoTest.Vector3> self)
  {
    /*
    <generated>
    */
    System.Single[] result_6  = default(System.Single[]);
    {
      PlatoTest.IArray<System.Single> self_7  = self._inlined_SelectMany<PlatoTest.Vector3, System.Single>(/* Captured: */( x)
       => {
          return x;
          }
        );
      {
        System.Single[] r  = new System.Single[self.Count];
        {
          System.Int32 i  = 0;
          while(i<self_7.Count)
          {
              r[i] = self_7[i];
              ++i;
              }
            }
        result_6 = r;
        }
      }
    return result_6;
    }
  /*
  public static IArray<T> Select<T>(this int count, Func<int, T> func)
            => new Array<T>(count, func);
  */
  [MethodImpl(MethodImplOptions.AggressiveInlining)]public static PlatoTest.IArray<T> _inlined_Select<T>(this System.Int32 count, System.Func<System.Int32, T> func)
  {
    return new PlatoTest.Array<T>(count, func);
    }
  /*
  public static IArray<U> SelectMany<T, U>(this IArray<T> self, Func<T, IArray<U>> func)
        {
            var r = new List<U>();
            for (var i = 0; i < self.Count; ++i)
            {
                var tmp = func(self[i]);
                for (var j = 0; j < tmp.Count; ++j)
                    r.Add(tmp[j]);
            }

            return r.Count.Select(i => r[i]);
        }
  */
  [MethodImpl(MethodImplOptions.AggressiveInlining)]public static PlatoTest.IArray<U> _inlined_SelectMany<T, U>(this PlatoTest.IArray<T> self, System.Func<T, PlatoTest.IArray<U>> func)
  {
    System.Collections.Generic.List<PlatoTest.Int3> r  = new System.Collections.Generic.List<PlatoTest.Int3>();
    {
      System.Int32 i  = 0;
      while(i<self.Count)
      {
          {
            PlatoTest.IArray<PlatoTest.Int3> tmp  = /* Captured: */( f)
             => {
                return new PlatoTest.Int3[2]{new PlatoTest.Int3(f.X, f.Y, f.Z), new PlatoTest.Int3(f.Z, f.W, f.X)}._inlined_ToIArray<PlatoTest.Int3>();
                }
              (self[i]);
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
    /*
    <generated>
    */
    PlatoTest.IArray<PlatoTest.Int3> result_5  = default(PlatoTest.IArray<PlatoTest.Int3>);
    {
      {
        result_5 = new PlatoTest.Array<T>(r.Count, /* Captured: ri*/( i)
         => {
            return r[i];
            }
          );
        }
      }
    return result_5;
    }
  /*
  public static IArray<U> Select<T, U>(this IArray<T> self, Func<T, U> func)
            => self.Count.Select(i => func(self[i]));
  */
  [MethodImpl(MethodImplOptions.AggressiveInlining)]public static PlatoTest.IArray<U> _inlined_Select<T, U>(this PlatoTest.IArray<PlatoTest.Triangle> self, System.Func<T, U> func)
  {
    /*
    <generated>
    */
    PlatoTest.IArray<PlatoTest.Triangle> result_8  = default(PlatoTest.IArray<PlatoTest.Triangle>);
    {
      {
        result_8 = new PlatoTest.Array<T>(mesh._Faces_.Count, /* Captured: selffunc*/( i)
         => {
            return /* Captured: mesh*/( f)
             => {
                return new PlatoTest.Triangle(mesh._Points_._Positions_[f.X], mesh._Points_._Positions_[f.Y], mesh._Points_._Positions_[f.Z]);
                }
              (mesh._Faces_[i]);
            }
          );
        }
      }
    return result_8;
    }
  /*
  public static float Cos(this float self)
            => MathF.Cos(self);
  */
  [MethodImpl(MethodImplOptions.AggressiveInlining)]public static System.Single _inlined_Cos(this System.Single self)
  {
    return System.MathF.Cos(self);
    }
  /*
  public static float Sin(this float self)
            => MathF.Sin(self);
  */
  [MethodImpl(MethodImplOptions.AggressiveInlining)]public static System.Single _inlined_Sin(this System.Single self)
  {
    return System.MathF.Sin(self);
    }
  /*
  public static float UnitToRad(this float self)
            => self * MathF.PI;
  */
  [MethodImpl(MethodImplOptions.AggressiveInlining)]public static System.Single _inlined_UnitToRad(this System.Single self)
  {
    return self*System.MathF.PI;
    }
  /*
  public static IArray<float> SampleFloats(int count, float max = 1.0f)
            => count.Select(i => max * count);
  */
  [MethodImpl(MethodImplOptions.AggressiveInlining)]public static PlatoTest.IArray<System.Single> _inlined_SampleFloats(System.Int32 count, System.Single max = 1.0f)
  {
    /*
    <generated>
    */
    PlatoTest.IArray<System.Single> result_9  = default(PlatoTest.IArray<System.Single>);
    {
      {
        result_9 = new PlatoTest.Array<T>(count, /* Captured: countmax*/( i)
         => {
            return max*count;
            }
          );
        }
      }
    return result_9;
    }
  /*
  public static IArray<Int3> ToTriangleIndices(this IArray<Int4> self)
            => self.SelectMany(f => new[] { new Int3(f.X, f.Y, f.Z), new Int3(f.Z, f.W, f.X) }.ToIArray());
  */
  [MethodImpl(MethodImplOptions.AggressiveInlining)]public static PlatoTest.IArray<PlatoTest.Int3> _inlined_ToTriangleIndices(this PlatoTest.IArray<PlatoTest.Int4> self)
  {
    /*
    <generated>
    */
    PlatoTest.IArray<PlatoTest.Int3> result_10  = default(PlatoTest.IArray<PlatoTest.Int3>);
    {
      {
        System.Collections.Generic.List<PlatoTest.Int3> r  = new System.Collections.Generic.List<PlatoTest.Int3>();
        {
          System.Int32 i  = 0;
          while(i<self.Count)
          {
              {
                PlatoTest.IArray<PlatoTest.Int3> tmp  = /* Captured: */( f)
                 => {
                    return new PlatoTest.Int3[2]{new PlatoTest.Int3(f.X, f.Y, f.Z), new PlatoTest.Int3(f.Z, f.W, f.X)}._inlined_ToIArray<PlatoTest.Int3>();
                    }
                  (self[i]);
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
        /*
        <generated>
        */
        PlatoTest.IArray<PlatoTest.Int3> result_5  = default(PlatoTest.IArray<PlatoTest.Int3>);
        {
          {
            result_5 = new PlatoTest.Array<T>(r.Count, /* Captured: ri*/( i)
             => {
                return r[i];
                }
              );
            }
          }
        result_10 = result_5;
        }
      }
    return result_10;
    }
  /*
  public static QuadMesh ToQuadMesh(this Func<Vector2, Vector3> func, int rows, int cols)
            => new(ComputeQuadStripPoints(func, rows, cols),
                ComputeQuadStripIndices(rows, cols));
  */
  [MethodImpl(MethodImplOptions.AggressiveInlining)]public static PlatoTest.QuadMesh _inlined_ToQuadMesh(this System.Func<PlatoTest.Vector2, PlatoTest.Vector3> func, System.Int32 rows, System.Int32 cols)
  {
    return new PlatoTest.QuadMesh(_inlined_ComputeQuadStripPoints(func, rows, cols), _inlined_ComputeQuadStripIndices(rows, cols));
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
  [MethodImpl(MethodImplOptions.AggressiveInlining)]public static PlatoTest.Vector3 _inlined_UVToNormal(this PlatoTest.Vector2 uv, System.Func<PlatoTest.Vector2, PlatoTest.Vector3> func, System.Single epsilon = 0.00001f)
  {
    PlatoTest.Vector3 a  = func(new PlatoTest.Vector2(uv.X+0.00001f, uv.Y));
    PlatoTest.Vector3 b  = func(new PlatoTest.Vector2(uv.X-0.00001f, uv.Y));
    PlatoTest.Vector3 c  = func(new PlatoTest.Vector2(uv.X, uv.Y+0.00001f));
    PlatoTest.Vector3 d  = func(new PlatoTest.Vector2(uv.X, uv.Y-0.00001f));
    /*
    <generated>
    */
    PlatoTest.Vector3 result_11  = default(PlatoTest.Vector3);
    {
      {
        result_11 = new PlatoTest.Vector3(b.X-a.X, b.Y-a.Y, b.Z-a.Z);
        }
      }
    PlatoTest.Vector3 v1  = result_11;
    /*
    <generated>
    */
    PlatoTest.Vector3 result_12  = default(PlatoTest.Vector3);
    {
      {
        result_12 = new PlatoTest.Vector3(b.X-a.X, b.Y-a.Y, b.Z-a.Z);
        }
      }
    PlatoTest.Vector3 v2  = result_12;
    /*
    <generated>
    */
    PlatoTest.Vector3 result_13  = default(PlatoTest.Vector3);
    {
      {
        result_13 = new PlatoTest.Vector3(Y*v2.Z-Z*v2.Y, Z*v2.X-X*v2.Z, X*v2.Y-Y*v2.X);
        }
      }
    PlatoTest.Vector3 r  = result_13;
    return r.Normal;
    }
  /*
  public static Points UVsToPoints(this IArray<Vector2> uvs, Func<Vector2, Vector3> func)
            => new(uvs.Select(func), uvs, uvs.Select(uv => uv.UVToNormal(func)));
  */
  [MethodImpl(MethodImplOptions.AggressiveInlining)]public static PlatoTest.Points _inlined_UVsToPoints(this PlatoTest.IArray<PlatoTest.Vector2> uvs, System.Func<PlatoTest.Vector2, PlatoTest.Vector3> func)
  {
    return new PlatoTest.Points(uvs_16._inlined_Select<PlatoTest.Vector2, PlatoTest.Vector3>(func), uvs, uvs_16._inlined_Select<PlatoTest.Vector2, PlatoTest.Vector3>(/* Captured: func*/( uv)
     => {
        /*
        <generated>
        */
        PlatoTest.Vector3 result_14  = default(PlatoTest.Vector3);
        {
          {
            PlatoTest.Vector3 a  = func(new PlatoTest.Vector2(uv.X+0.00001f, uv.Y));
            PlatoTest.Vector3 b  = func(new PlatoTest.Vector2(uv.X-0.00001f, uv.Y));
            PlatoTest.Vector3 c  = func(new PlatoTest.Vector2(uv.X, uv.Y+0.00001f));
            PlatoTest.Vector3 d  = func(new PlatoTest.Vector2(uv.X, uv.Y-0.00001f));
            /*
            <generated>
            */
            PlatoTest.Vector3 result_11  = default(PlatoTest.Vector3);
            {
              {
                result_11 = new PlatoTest.Vector3(b.X-a.X, b.Y-a.Y, b.Z-a.Z);
                }
              }
            PlatoTest.Vector3 v1  = result_11;
            /*
            <generated>
            */
            PlatoTest.Vector3 result_12  = default(PlatoTest.Vector3);
            {
              {
                result_12 = new PlatoTest.Vector3(b.X-a.X, b.Y-a.Y, b.Z-a.Z);
                }
              }
            PlatoTest.Vector3 v2  = result_12;
            /*
            <generated>
            */
            PlatoTest.Vector3 result_13  = default(PlatoTest.Vector3);
            {
              {
                result_13 = new PlatoTest.Vector3(Y*v2.Z-Z*v2.Y, Z*v2.X-X*v2.Z, X*v2.Y-Y*v2.X);
                }
              }
            PlatoTest.Vector3 r  = result_13;
            result_14 = r.Normal;
            }
          }
        return result_14;
        }
      ));
    }
  /*
  public static Points ComputeQuadStripPoints(this Func<Vector2, Vector3> func, int usegs, int vsegs)
            => ComputeQuadStripUVs(usegs, vsegs).UVsToPoints(func);
  */
  [MethodImpl(MethodImplOptions.AggressiveInlining)]public static PlatoTest.Points _inlined_ComputeQuadStripPoints(this System.Func<PlatoTest.Vector2, PlatoTest.Vector3> func, System.Int32 usegs, System.Int32 vsegs)
  {
    /*
    <generated>
    */
    PlatoTest.Points result_15  = default(PlatoTest.Points);
    {
      PlatoTest.IArray<PlatoTest.Vector2> uvs_16  = _inlined_ComputeQuadStripUVs(usegs, vsegs);
      {
        result_15 = new PlatoTest.Points(uvs_16._inlined_Select<PlatoTest.Vector2, PlatoTest.Vector3>(func), uvs_16, uvs_16._inlined_Select<PlatoTest.Vector2, PlatoTest.Vector3>(/* Captured: func*/( uv)
         => {
            /*
            <generated>
            */
            PlatoTest.Vector3 result_14  = default(PlatoTest.Vector3);
            {
              {
                PlatoTest.Vector3 a  = func(new PlatoTest.Vector2(uv.X+0.00001f, uv.Y));
                PlatoTest.Vector3 b  = func(new PlatoTest.Vector2(uv.X-0.00001f, uv.Y));
                PlatoTest.Vector3 c  = func(new PlatoTest.Vector2(uv.X, uv.Y+0.00001f));
                PlatoTest.Vector3 d  = func(new PlatoTest.Vector2(uv.X, uv.Y-0.00001f));
                /*
                <generated>
                */
                PlatoTest.Vector3 result_11  = default(PlatoTest.Vector3);
                {
                  {
                    result_11 = new PlatoTest.Vector3(b.X-a.X, b.Y-a.Y, b.Z-a.Z);
                    }
                  }
                PlatoTest.Vector3 v1  = result_11;
                /*
                <generated>
                */
                PlatoTest.Vector3 result_12  = default(PlatoTest.Vector3);
                {
                  {
                    result_12 = new PlatoTest.Vector3(b.X-a.X, b.Y-a.Y, b.Z-a.Z);
                    }
                  }
                PlatoTest.Vector3 v2  = result_12;
                /*
                <generated>
                */
                PlatoTest.Vector3 result_13  = default(PlatoTest.Vector3);
                {
                  {
                    result_13 = new PlatoTest.Vector3(Y*v2.Z-Z*v2.Y, Z*v2.X-X*v2.Z, X*v2.Y-Y*v2.X);
                    }
                  }
                PlatoTest.Vector3 r  = result_13;
                result_14 = r.Normal;
                }
              }
            return result_14;
            }
          ));
        }
      }
    return result_15;
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
  [MethodImpl(MethodImplOptions.AggressiveInlining)]public static PlatoTest.IArray<PlatoTest.Vector2> _inlined_ComputeQuadStripUVs(System.Int32 usegs, System.Int32 vsegs)
  {
    return new PlatoTest.Array<PlatoTest.Vector2>(usegs*vsegs, /* Captured: usegsvsegs*/( i)
     => {
        System.Int32 row  = i/vsegs;
        System.Int32 col  = i%usegs;
        return new PlatoTest.Vector2((System.Single)col/(usegs-1), (System.Single)row/(vsegs-1));
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
  [MethodImpl(MethodImplOptions.AggressiveInlining)]public static PlatoTest.IArray<PlatoTest.Int4> _inlined_ComputeQuadStripIndices(System.Int32 usegs, System.Int32 vsegs)
  {
    return new PlatoTest.Array<PlatoTest.Int4>((usegs-1)*(vsegs-1), /* Captured: usegsvsegs*/( i)
     => {
        System.Int32 row  = i/(vsegs-1);
        System.Int32 col  = i%(usegs-1);
        System.Int32 nextCol  = (col+1);
        System.Int32 nextRow  = (row+1);
        System.Int32 a  = (row*usegs)+col;
        System.Int32 b  = (row*usegs)+nextCol;
        System.Int32 c  = (nextRow*usegs)+nextCol;
        System.Int32 d  = (nextRow*usegs)+col;
        return new PlatoTest.Int4(a, b, c, d);
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
  [MethodImpl(MethodImplOptions.AggressiveInlining)]public static PlatoTest.Vector3 _inlined_UvToSphere(PlatoTest.Vector2 uv, System.Single radius)
  {
    return new PlatoTest.Vector3(-radius*uv.X._inlined_UnitToRad()._inlined_Cos()*(uv.Y*System.MathF.PI)._inlined_Sin(), radius*(uv.Y*System.MathF.PI)._inlined_Cos(), radius*uv.X._inlined_UnitToRad()._inlined_Cos()*(uv.Y*System.MathF.PI)._inlined_Sin());
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
  [MethodImpl(MethodImplOptions.AggressiveInlining)]public static PlatoTest.Vector3 _inlined_UvToTorus(PlatoTest.Vector2 uv, System.Single radius, System.Single tube)
  {
    uv = uv*System.MathF.PI*2;
    return new PlatoTest.Vector3((radius+tube*uv.Y._inlined_Cos())*uv.X._inlined_Cos(), (radius+tube*uv.Y._inlined_Cos())*uv.X._inlined_Sin(), tube*uv.Y._inlined_Sin());
    }
  /*
  public static TriMesh ToTriMesh(this QuadMesh mesh)
            => new(mesh.Points, mesh.Faces.ToTriangleIndices());
  */
  [MethodImpl(MethodImplOptions.AggressiveInlining)]public static PlatoTest.TriMesh _inlined_ToTriMesh(this PlatoTest.QuadMesh mesh)
  {
    return new PlatoTest.TriMesh(mesh_26._Points_, mesh_26._Faces_._inlined_ToTriangleIndices());
    }
  /*
  public static void TestOperator()
        {
            var x = new Vector3(1, 2, 3);
            var y = x + x;
        }
  */
  [MethodImpl(MethodImplOptions.AggressiveInlining)]public static void _inlined_TestOperator()
  {
    PlatoTest.Vector3 x  = new PlatoTest.Vector3(1, 2, 3);
    /*
    <generated>
    */
    PlatoTest.Vector3 result_17  = default(PlatoTest.Vector3);
    {
      {
        result_17 = new PlatoTest.Vector3(x.X+x.X, x.Y+x.Y, x.Z+x.Z);
        }
      }
    PlatoTest.Vector3 y  = result_17;
    }
  /*
  public static QuadMesh Torus(int rows, int cols, float radius, float tube)
            => ToQuadMesh(uv => UvToTorus(uv, radius, tube), rows, cols);
  */
  [MethodImpl(MethodImplOptions.AggressiveInlining)]public static PlatoTest.QuadMesh _inlined_Torus(System.Int32 rows, System.Int32 cols, System.Single radius, System.Single tube)
  {
    /*
    <generated>
    */
    PlatoTest.QuadMesh result_18  = default(PlatoTest.QuadMesh);
    {
      {
        result_18 = new PlatoTest.QuadMesh(_inlined_ComputeQuadStripPoints(/* Captured: radiustube*/( uv)
         => {
            return _inlined_UvToTorus(uv, radius, tube);
            }
          , rows, cols), _inlined_ComputeQuadStripIndices(rows, cols));
        }
      }
    return result_18;
    }
  /*
  public static int[] ToIntArray(this IArray<Int3> faces)
            => faces.SelectMany(f => f).ToArray();
  */
  [MethodImpl(MethodImplOptions.AggressiveInlining)]public static System.Int32[] _inlined_ToIntArray(this PlatoTest.IArray<PlatoTest.Int3> faces)
  {
    /*
    <generated>
    */
    System.Int32[] result_19  = default(System.Int32[]);
    {
      PlatoTest.IArray<System.Single> self_20  = faces._inlined_SelectMany<PlatoTest.Int3, System.Int32>(/* Captured: */( f)
       => {
          return f;
          }
        );
      {
        System.Single[] r  = new System.Single[self.Count];
        {
          System.Int32 i  = 0;
          while(i<self_7.Count)
          {
              r[i] = self_20[i];
              ++i;
              }
            }
        result_19 = r;
        }
      }
    return result_19;
    }
  /*
  public static IArray<Triangle> Triangles(this TriMesh mesh)
            => mesh.Faces.Select(f => new Triangle(mesh.Points.Positions[f.X], mesh.Points.Positions[f.Y], mesh.Points.Positions[f.Z]));
  */
  [MethodImpl(MethodImplOptions.AggressiveInlining)]public static PlatoTest.IArray<PlatoTest.Triangle> _inlined_Triangles(this PlatoTest.TriMesh mesh)
  {
    /*
    <generated>
    */
    PlatoTest.IArray<PlatoTest.Triangle> result_21  = default(PlatoTest.IArray<PlatoTest.Triangle>);
    {
      {
        /*
        <generated>
        */
        PlatoTest.IArray<PlatoTest.Triangle> result_8  = default(PlatoTest.IArray<PlatoTest.Triangle>);
        {
          {
            result_8 = new PlatoTest.Array<T>(mesh._Faces_.Count, /* Captured: selffunc*/( i)
             => {
                return /* Captured: mesh*/( f)
                 => {
                    return new PlatoTest.Triangle(mesh._Points_._Positions_[f.X], mesh._Points_._Positions_[f.Y], mesh._Points_._Positions_[f.Z]);
                    }
                  (mesh._Faces_[i]);
                }
              );
            }
          }
        result_21 = result_8;
        }
      }
    return result_21;
    }
  /*
  public static IArray<Vector3> FaceNormals(this TriMesh mesh)
            => mesh.Triangles().Select(tri => tri.Normal);
  */
  [MethodImpl(MethodImplOptions.AggressiveInlining)]public static PlatoTest.IArray<PlatoTest.Vector3> _inlined_FaceNormals(this PlatoTest.TriMesh mesh)
  {
    /*
    <generated>
    */
    PlatoTest.IArray<PlatoTest.Vector3> result_22  = default(PlatoTest.IArray<PlatoTest.Vector3>);
    {
      PlatoTest.IArray<PlatoTest.Triangle> self_23  = mesh._inlined_Triangles();
      {
        /*
        <generated>
        */
        PlatoTest.IArray<PlatoTest.Triangle> result_8  = default(PlatoTest.IArray<PlatoTest.Triangle>);
        {
          {
            result_8 = new PlatoTest.Array<T>(mesh._Faces_.Count, /* Captured: selffunc*/( i)
             => {
                return /* Captured: mesh*/( f)
                 => {
                    return new PlatoTest.Triangle(mesh._Points_._Positions_[f.X], mesh._Points_._Positions_[f.Y], mesh._Points_._Positions_[f.Z]);
                    }
                  (mesh._Faces_[i]);
                }
              );
            }
          }
        result_22 = result_8;
        }
      }
    return result_22;
    }
  /*
  public static (T, TimeSpan) TimeIt<T>(Func<T> func)
        {
            var sw = Stopwatch.StartNew();
            return (func(), sw.Elapsed);
        }
  */
  [MethodImpl(MethodImplOptions.AggressiveInlining)]public static System.ValueTuple<T, System.TimeSpan> _inlined_TimeIt<T>(System.Func<T> func)
  {
    System.Diagnostics.Stopwatch sw  = System.Diagnostics.Stopwatch.StartNew();
    return (func(), sw.Elapsed);
    }
  /*
  public static void Log(string s)
            => Console.WriteLine(s);
  */
  [MethodImpl(MethodImplOptions.AggressiveInlining)]public static void _inlined_Log(System.String s)
  {
    System.Console.WriteLine(s);
    }
  /*
  public static T LogTiming<T>(Func<T> func)
        {
            var r = TimeIt(func);
            Console.WriteLine("msec elapsed: " + r.Item2.Milliseconds);
            return r.Item1;
        }
  */
  [MethodImpl(MethodImplOptions.AggressiveInlining)]public static T _inlined_LogTiming<T>(System.Func<T> func)
  {
    /*
    <generated>
    */
    System.ValueTuple<System.Single[], System.TimeSpan> result_24  = default(System.ValueTuple<System.Single[], System.TimeSpan>);
    {
      {
        System.Diagnostics.Stopwatch sw  = System.Diagnostics.Stopwatch.StartNew();
        result_24 = (func(), sw.Elapsed);
        }
      }
    System.ValueTuple<System.Single[], System.TimeSpan> r  = result_24;
    System.Console.WriteLine("msec elapsed: "+r.Item2.Milliseconds);
    return r.Item1;
    }
  /*
  public static void ManualBenchmark()
        {
            var torus = Torus(5000, 1000, 1, 0.2f).ToTriMesh();
            var floats = LogTiming(torus.FaceNormals().ToFloatArray);
            var filePath = Path.Combine(Path.GetTempPath(), "profiling.txt");
            File.WriteAllLines(filePath, floats.Select(f => f.ToString()));
        }
  */
  [MethodImpl(MethodImplOptions.AggressiveInlining)]public static void _inlined_ManualBenchmark()
  {
    /*
    <generated>
    */
    PlatoTest.TriMesh result_25  = default(PlatoTest.TriMesh);
    {
      PlatoTest.QuadMesh mesh_26  = _inlined_Torus(5000, 1000, 1, 0.2f);
      {
        result_25 = new PlatoTest.TriMesh(mesh_26._Points_, mesh_26._Faces_._inlined_ToTriangleIndices());
        }
      }
    PlatoTest.TriMesh torus  = result_25;
    /*
    <generated>
    */
    System.Single[] result_27  = default(System.Single[]);
    {
      {
        /*
        <generated>
        */
        System.ValueTuple<System.Single[], System.TimeSpan> result_24  = default(System.ValueTuple<System.Single[], System.TimeSpan>);
        {
          {
            System.Diagnostics.Stopwatch sw  = System.Diagnostics.Stopwatch.StartNew();
            result_24 = (torus._inlined_FaceNormals()._inlined_ToFloatArray(), sw.Elapsed);
            }
          }
        System.ValueTuple<System.Single[], System.TimeSpan> r  = result_24;
        System.Console.WriteLine("msec elapsed: "+r.Item2.Milliseconds);
        result_27 = r.Item1;
        }
      }
    System.Single[] floats  = result_27;
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
