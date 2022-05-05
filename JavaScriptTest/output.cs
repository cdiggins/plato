//==begin==//
public interface IArray<T> : var
{
  /*
  int Count { get; }
  */
  public int/* unresolved */ Count
  {
  get;
    }
  ;
  /*
  T this[int index] { get; }
  */
  public T this [int/* unresolved */ index]
  
  {
  get;
    }
  }
//==end==//
//==begin==//
public class Array<T> : var
{
  public Func<int/* unresolved */, T>/* unresolved */ _Func_ ;
  public int/* unresolved */ _Count_ ;
  /*
  public Array(int count, Func<int, T> func) => (Count, Func) = (count, func);
  */
  public Array(int/* unresolved */ count, Func<int/* unresolved */, T>/* unresolved */ func)
  {
      // Let declaration
      var _var120  = (count, func);
      Count = _var120.Item1/* unresolved */;
      Func = _var120.Item2/* unresolved */;
      }
    /*
  public Func<int, T> Func { get; }
  */
  public Func<int/* unresolved */, T>/* unresolved */ Func
  {
  get
      {
        return _Func_;
        }
      }
  ;
  /*
  public int Count { get; }
  */
  public int/* unresolved */ Count
  {
  get
      {
        return _Count_;
        }
      }
  ;
  /*
  public T this[int index] => Func(index);
  */
  public T this [int/* unresolved */ index]
  
  {
  get
      {
        return Func/* unresolved */(index/* unresolved */);
        }
      }
  }
//==end==//
//==begin==//
public class Vector2 : var
{
  /*
  public Vector2(float x = 0, float y = 0) => (X, Y) = (x, y);
  */
  public Vector2(float/* unresolved */ x = 0, float/* unresolved */ y = 0)
  {
      // Let declaration
      var _var126  = (x, y);
      X = _var126.Item1/* unresolved */;
      Y = _var126.Item2/* unresolved */;
      }
    /*
  public static Vector2 Zero => new();
  */
  public static Vector2 Zero
  {
  get
      {
        return new Vector2();
        }
      }
  ;
  /*
  public float MagnitudeSquared => Dot(this);
  */
  public float/* unresolved */ MagnitudeSquared
  {
  get
      {
        return Dot(this);
        }
      }
  ;
  /*
  public float Magnitude => MathF.Sqrt(MagnitudeSquared);
  */
  public float/* unresolved */ Magnitude
  {
  get
      {
        return Sqrt/* unresolved */(MagnitudeSquared);
        }
      }
  ;
  /*
  public Vector2 Normal => this / Magnitude;
  */
  public Vector2 Normal
  {
  get
      {
        return this/Magnitude;
        }
      }
  ;
  /*
  public Vector2 WithX(float x) => new(x, Y);
  */
  public Vector2 WithX(float/* unresolved */ x)
  {
    return new Vector2(x, Y);
    }
  /*
  public Vector2 WithY(float y) => new(X, y);
  */
  public Vector2 WithY(float/* unresolved */ y)
  {
    return new Vector2(X, y);
    }
  /*
  public float Dot(Vector2 v) => X * v.X + v.Y * Y;
  */
  public float/* unresolved */ Dot(Vector2 v)
  {
    return X*v.X+v.Y*Y;
    }
  /*
  public override string ToString() => "Vector2(" + X  + "," + Y + ")";
  */
  public string/* unresolved */ ToString()
  {
    return "Vector2("+X+","+Y+")";
    }
  /*
  public static implicit operator Vector2(float v) => new(v, v);
  */
  public static Vector2 operator implicit(float/* unresolved */ v)
  {
    return new Vector2(v, v);
    }
  /*
  public static Vector2 operator +(Vector2 q, Vector2 r) => new(q.X + r.X, q.Y + r.Y);
  */
  public static Vector2 operator +(Vector2 q, Vector2 r)
  {
    return new Vector2(q.X+r.X, q.Y+r.Y);
    }
  /*
  public static Vector2 operator *(Vector2 q, Vector2 r) => new(q.X * r.X, q.Y * r.Y);
  */
  public static Vector2 operator *(Vector2 q, Vector2 r)
  {
    return new Vector2(q.X*r.X, q.Y*r.Y);
    }
  /*
  public static Vector2 operator /(Vector2 q, Vector2 r) => new(q.X / r.X, q.Y / r.Y);
  */
  public static Vector2 operator /(Vector2 q, Vector2 r)
  {
    return new Vector2(q.X/r.X, q.Y/r.Y);
    }
  }
//==end==//
//==begin==//
public class Vector3 : var
{
  /*
  public Vector3(float x = 0, float y = 0, float z = 0) => (X, Y, Z) = (x, y, z);
  */
  public Vector3(float/* unresolved */ x = 0, float/* unresolved */ y = 0, float/* unresolved */ z = 0)
  {
      // Let declaration
      var _var140  = (x, y, z);
      X = _var140.Item1/* unresolved */;
      Y = _var140.Item2/* unresolved */;
      Z = _var140.Item3/* unresolved */;
      }
    /*
  public static Vector3 Zero => new();
  */
  public static Vector3 Zero
  {
  get
      {
        return new Vector3();
        }
      }
  ;
  /*
  public float MagnitudeSquared => Dot(this);
  */
  public float/* unresolved */ MagnitudeSquared
  {
  get
      {
        return Dot(this);
        }
      }
  ;
  /*
  public float Magnitude => MathF.Sqrt(MagnitudeSquared);
  */
  public float/* unresolved */ Magnitude
  {
  get
      {
        return Sqrt/* unresolved */(MagnitudeSquared);
        }
      }
  ;
  /*
  public Vector3 Normal => this / Magnitude;
  */
  public Vector3 Normal
  {
  get
      {
        return this/Magnitude;
        }
      }
  ;
  /*
  public int Count => 3;
  */
  public int/* unresolved */ Count
  {
  get
      {
        return 3;
        }
      }
  ;
  /*
  public Vector3 WithX(float x) => new(x, Y, Z);
  */
  public Vector3 WithX(float/* unresolved */ x)
  {
    return new Vector3(x, Y, Z);
    }
  /*
  public Vector3 WithY(float y) => new(X, y, Z);
  */
  public Vector3 WithY(float/* unresolved */ y)
  {
    return new Vector3(X, y, Z);
    }
  /*
  public Vector3 WithZ(float z) => new(X, Y, z);
  */
  public Vector3 WithZ(float/* unresolved */ z)
  {
    return new Vector3(X, Y, z);
    }
  /*
  public float Dot(Vector3 v) => X * v.X + v.Y * Y + Z * v.Z;
  */
  public float/* unresolved */ Dot(Vector3 v)
  {
    return X*v.X+v.Y*Y+Z*v.Z;
    }
  /*
  public override string ToString() => "Vector3(" + X + "," + Y + "," + Z + ")";
  */
  public string/* unresolved */ ToString()
  {
    return "Vector3("+X+","+Y+","+Z+")";
    }
  /*
  public Vector3 Cross(Vector3 v)
            => new(Y* v.Z - Z* v.Y, Z* v.X - X* v.Z, X* v.Y - Y* v.X);
  */
  public Vector3 Cross(Vector3 v)
  {
    return new Vector3(Y*v.Z-Z*v.Y, Z*v.X-X*v.Z, X*v.Y-Y*v.X);
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
  public float/* unresolved */ this [int/* unresolved */ i]
  
  {
  get
      {
        return i/* unresolved */ switch 
        {
          0 => X;
          1 => Y;
          2 => Z;
          _ => throw new ArgumentOutOfRangeException/* unresolved */();
          }
        ;
        }
      }
  /*
  public static implicit operator Vector3(float v) => new(v, v, v);
  */
  public static Vector3 operator implicit(float/* unresolved */ v)
  {
    return new Vector3(v, v, v);
    }
  /*
  public static Vector3 operator +(Vector3 q, Vector3 r) => new(q.X + r.X, q.Y + r.Y, q.Z + r.Z);
  */
  public static Vector3 operator +(Vector3 q, Vector3 r)
  {
    return new Vector3(q.X+r.X, q.Y+r.Y, q.Z+r.Z);
    }
  /*
  public static Vector3 operator -(Vector3 q, Vector3 r) => new(q.X - r.X, q.Y - r.Y, q.Z - r.Z);
  */
  public static Vector3 operator -(Vector3 q, Vector3 r)
  {
    return new Vector3(q.X-r.X, q.Y-r.Y, q.Z-r.Z);
    }
  /*
  public static Vector3 operator *(Vector3 q, Vector3 r) => new(q.X * r.X, q.Y * r.Y, q.Z * r.Z);
  */
  public static Vector3 operator *(Vector3 q, Vector3 r)
  {
    return new Vector3(q.X*r.X, q.Y*r.Y, q.Z*r.Z);
    }
  /*
  public static Vector3 operator /(Vector3 q, Vector3 r) => new(q.X / r.X, q.Y / r.Y, q.Z / r.Z);
  */
  public static Vector3 operator /(Vector3 q, Vector3 r)
  {
    return new Vector3(q.X/r.X, q.Y/r.Y, q.Z/r.Z);
    }
  }
//==end==//
//==begin==//
public class Int3 : var
{
  /*
  public Int3(int x = 0, int y = 0, int z = 0) => (X, Y, Z) = (x, y, z);
  */
  public Int3(int/* unresolved */ x = 0, int/* unresolved */ y = 0, int/* unresolved */ z = 0)
  {
      // Let declaration
      var _var159  = (x, y, z);
      X = _var159.Item1/* unresolved */;
      Y = _var159.Item2/* unresolved */;
      Z = _var159.Item3/* unresolved */;
      }
    /*
  public static Int3 Zero => new();
  */
  public static Int3 Zero
  {
  get
      {
        return new Int3();
        }
      }
  ;
  /*
  public int Count => 3;
  */
  public int/* unresolved */ Count
  {
  get
      {
        return 3;
        }
      }
  ;
  /*
  public Int3 WithX(int x) => new(x, Y, Z);
  */
  public Int3 WithX(int/* unresolved */ x)
  {
    return new Int3(x, Y, Z);
    }
  /*
  public Int3 WithY(int y) => new(X, y, Z);
  */
  public Int3 WithY(int/* unresolved */ y)
  {
    return new Int3(X, y, Z);
    }
  /*
  public Int3 WithZ(int z) => new(X, Y, z);
  */
  public Int3 WithZ(int/* unresolved */ z)
  {
    return new Int3(X, Y, z);
    }
  /*
  public override string ToString() => "Int3(" + X + "," + Y + "," + Z + ")";
  */
  public string/* unresolved */ ToString()
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
  public int/* unresolved */ this [int/* unresolved */ i]
  
  {
  get
      {
        return i/* unresolved */ switch 
        {
          0 => X;
          1 => Y;
          2 => Z;
          _ => throw new ArgumentOutOfRangeException/* unresolved */();
          }
        ;
        }
      }
  }
//==end==//
//==begin==//
public class Int4 : var
{
  /*
  public Int4(int x = 0, int y = 0, int z = 0, int w = 0) => (X, Y, Z, W) = (x, y, z, w);
  */
  public Int4(int/* unresolved */ x = 0, int/* unresolved */ y = 0, int/* unresolved */ z = 0, int/* unresolved */ w = 0)
  {
      // Let declaration
      var _var168  = (x, y, z, w);
      X = _var168.Item1/* unresolved */;
      Y = _var168.Item2/* unresolved */;
      Z = _var168.Item3/* unresolved */;
      W = _var168.Item4/* unresolved */;
      }
    /*
  public static Int4 Zero => new();
  */
  public static Int4 Zero
  {
  get
      {
        return new Int4();
        }
      }
  ;
  /*
  public int Count => 4;
  */
  public int/* unresolved */ Count
  {
  get
      {
        return 4;
        }
      }
  ;
  /*
  public Int4 WithX(int x) => new(x, Y, Z, W);
  */
  public Int4 WithX(int/* unresolved */ x)
  {
    return new Int4(x, Y, Z, W);
    }
  /*
  public Int4 WithY(int y) => new(X, y, Z, W);
  */
  public Int4 WithY(int/* unresolved */ y)
  {
    return new Int4(X, y, Z, W);
    }
  /*
  public Int4 WithZ(int z) => new(X, Y, z, W);
  */
  public Int4 WithZ(int/* unresolved */ z)
  {
    return new Int4(X, Y, z, W);
    }
  /*
  public Int4 WithW(int w) => new(X, Y, Z, w);
  */
  public Int4 WithW(int/* unresolved */ w)
  {
    return new Int4(X, Y, Z, w);
    }
  /*
  public override string ToString() => "Int4(" + X + "," + Y + "," + Z + "," + W + ")";
  */
  public string/* unresolved */ ToString()
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
  public int/* unresolved */ this [int/* unresolved */ i]
  
  {
  get
      {
        return i/* unresolved */ switch 
        {
          0 => X;
          1 => Y;
          2 => Z;
          3 => W;
          _ => throw new ArgumentOutOfRangeException/* unresolved */();
          }
        ;
        }
      }
  }
//==end==//
//==begin==//
public class Points : var
{
  public IArray<Vector3> _Positions_ ;
  public IArray<Vector2> _UVs_ ;
  public IArray<Vector3> _Normals_ ;
  /*
  public Points(IArray<Vector3> positions, IArray<Vector2> uvs, IArray<Vector3> normals)
            => (Positions, UVs, Normals) = (positions, uvs, normals);
  */
  public Points(IArray<Vector3> positions, IArray<Vector2> uvs, IArray<Vector3> normals)
  {
      // Let declaration
      var _var177  = (positions, uvs, normals);
      Positions = _var177.Item1/* unresolved */;
      UVs = _var177.Item2/* unresolved */;
      Normals = _var177.Item3/* unresolved */;
      }
    /*
  public IArray<Vector3> Positions { get; }
  */
  public IArray<Vector3> Positions
  {
  get
      {
        return _Positions_;
        }
      }
  ;
  /*
  public IArray<Vector2> UVs { get; }
  */
  public IArray<Vector2> UVs
  {
  get
      {
        return _UVs_;
        }
      }
  ;
  /*
  public IArray<Vector3> Normals { get; }
  */
  public IArray<Vector3> Normals
  {
  get
      {
        return _Normals_;
        }
      }
  ;
  }
//==end==//
//==begin==//
public class TriMesh : var
{
  public Points _Points_ ;
  public IArray<Int3> _Faces_ ;
  /*
  public TriMesh(Points points, IArray<Int3> faces)
            => (Points, Faces) = (points, faces);
  */
  public TriMesh(Points points, IArray<Int3> faces)
  {
      // Let declaration
      var _var183  = (points, faces);
      Points = _var183.Item1/* unresolved */;
      Faces = _var183.Item2/* unresolved */;
      }
    /*
  public Points Points { get; }
  */
  public Points Points
  {
  get
      {
        return _Points_;
        }
      }
  ;
  /*
  public IArray<Int3> Faces { get; }
  */
  public IArray<Int3> Faces
  {
  get
      {
        return _Faces_;
        }
      }
  ;
  }
//==end==//
//==begin==//
public class QuadMesh : var
{
  public Points _Points_ ;
  public IArray<Int4> _Faces_ ;
  /*
  public QuadMesh(Points points, IArray<Int4> faces)
            => (Points, Faces) = (points, faces);
  */
  public QuadMesh(Points points, IArray<Int4> faces)
  {
      // Let declaration
      var _var188  = (points, faces);
      Points = _var188.Item1/* unresolved */;
      Faces = _var188.Item2/* unresolved */;
      }
    /*
  public Points Points { get; }
  */
  public Points Points
  {
  get
      {
        return _Points_;
        }
      }
  ;
  /*
  public IArray<Int4> Faces { get; }
  */
  public IArray<Int4> Faces
  {
  get
      {
        return _Faces_;
        }
      }
  ;
  }
//==end==//
//==begin==//
public class Extensions : var
{
  /*
  public static IArray<T> ToIArray<T>(this IReadOnlyList<T> self)
            => self.Count.Select(i => self[i]);
  */
  public static IArray<T> ToIArray<T>(IReadOnlyList<T>/* unresolved */ self)
  {
    return Select(/* Captured: IReadOnlyList<T>/* unresolved */ self*/(var i)
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
  public static /* unresolved */ ToArray<T>(IArray<T> self)
  {
    /* unresolved */ r  = new var[]{};
    {
      int/* unresolved */ i  = 0;
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
  public static /* unresolved */ ToFloatArray(IArray<Vector3> self)
  {
    return ToArray();
    }
  /*
  public static IArray<T> Select<T>(this int count, Func<int, T> func)
            => new Array<T>(count, func);
  */
  public static IArray<T> Select<T>(int/* unresolved */ count, Func<int/* unresolved */, T>/* unresolved */ func)
  {
    return new Array<T>(count, func);
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
  public static IArray<U> SelectMany<T, U>(IArray<T> self, Func<T, IArray<U>>/* unresolved */ func)
  {
    List<U>/* unresolved */ r  = new List<U>/* unresolved */();
    {
      int/* unresolved */ i  = 0;
      while(i<self.Count)
      {
          {
            IArray<U> tmp  = func/* unresolved */(self[i]);
            {
              int/* unresolved */ j  = 0;
              while(j<tmp.Count)
              {
                  Add/* unresolved */(tmp[j]);
                  ++j;
                  }
                }
            }
          ++i;
          }
        }
    return Select(/* Captured: List<U>/* unresolved */ r  = new List<U>/* unresolved */()var i*/(var i)
     => {
        return r[i];
        }
      );
    }
  /*
  public static IArray<U> Select<T, U>(this IArray<T> self, Func<T, U> func)
            => self.Count.Select(i => func(self[i]));
  */
  public static IArray<U> Select<T, U>(IArray<T> self, Func<T, U>/* unresolved */ func)
  {
    return Select(/* Captured: IArray<T> selfFunc<T, U>/* unresolved */ func*/(var i)
     => {
        return func/* unresolved */(self[i]);
        }
      );
    }
  /*
  public static float Cos(this float self) 
            => MathF.Cos(self);
  */
  public static float/* unresolved */ Cos(float/* unresolved */ self)
  {
    return Cos/* unresolved */(self);
    }
  /*
  public static float Sin(this float self) 
            => MathF.Sin(self);
  */
  public static float/* unresolved */ Sin(float/* unresolved */ self)
  {
    return Sin/* unresolved */(self);
    }
  /*
  public static float UnitToRad(this float self)
            => self * MathF.PI;
  */
  public static float/* unresolved */ UnitToRad(float/* unresolved */ self)
  {
    return self*MathF/* unresolved */.PI/* unresolved */;
    }
  /*
  public static IArray<float> SampleFloats(int count, float max = 1.0f)
            => count.Select(i => max * count);
  */
  public static IArray<float/* unresolved */> SampleFloats(int/* unresolved */ count, float/* unresolved */ max = 1.0f)
  {
    return Select(/* Captured: int/* unresolved */ countfloat/* unresolved */ max = 1.0f*/(var i)
     => {
        return max*count;
        }
      );
    }
  /*
  public static IArray<Int3> ToTriangles(this IArray<Int4> self)
            => self.SelectMany(f => new[] { new Int3(f.X, f.Y, f.Z), new Int3(f.Z, f.W, f.X) }.ToIArray());
  */
  public static IArray<Int3> ToTriangles(IArray<Int4> self)
  {
    return SelectMany(/* Captured: */(var f)
     => {
        return ToIArray();
        }
      );
    }
  /*
  public static QuadMesh ToQuadMesh(this Func<Vector2, Vector3> func, int rows, int cols)
            => new (ComputeQuadStripPoints(func, rows, cols),
                ComputeQuadStripIndices(rows, cols));
  */
  public static QuadMesh ToQuadMesh(Func<Vector2, Vector3>/* unresolved */ func, int/* unresolved */ rows, int/* unresolved */ cols)
  {
    return new QuadMesh(ComputeQuadStripPoints(func, rows, cols), ComputeQuadStripIndices(rows, cols));
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
  public static Vector3 UVToNormal(Vector2 uv, Func<Vector2, Vector3>/* unresolved */ func, float/* unresolved */ epsilon = 0.00001f)
  {
    Vector3 a  = func/* unresolved */(new Vector2(uv.X+epsilon, uv.Y));
    Vector3 b  = func/* unresolved */(new Vector2(uv.X-epsilon, uv.Y));
    Vector3 c  = func/* unresolved */(new Vector2(uv.X, uv.Y+epsilon));
    Vector3 d  = func/* unresolved */(new Vector2(uv.X, uv.Y-epsilon));
    Vector3 v1  = b-a;
    Vector3 v2  = d-c;
    Vector3 r  = Cross(v2);
    return r.Normal;
    }
  /*
  public static Points UVsToPoints(this IArray<Vector2> uvs, Func<Vector2, Vector3> func)
            => new(uvs.Select(func), uvs, uvs.Select(uv => uv.UVToNormal(func)));
  */
  public static Points UVsToPoints(IArray<Vector2> uvs, Func<Vector2, Vector3>/* unresolved */ func)
  {
    return new Points(Select(func), uvs, Select(/* Captured: Func<Vector2, Vector3>/* unresolved */ func*/(var uv)
     => {
        return UVToNormal(func);
        }
      ));
    }
  /*
  public static Points ComputeQuadStripPoints(this Func<Vector2, Vector3> func, int usegs, int vsegs)
            => ComputeQuadStripUVs(usegs, vsegs).UVsToPoints(func);
  */
  public static Points ComputeQuadStripPoints(Func<Vector2, Vector3>/* unresolved */ func, int/* unresolved */ usegs, int/* unresolved */ vsegs)
  {
    return UVsToPoints(func);
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
  public static IArray<Vector2> ComputeQuadStripUVs(int/* unresolved */ usegs, int/* unresolved */ vsegs)
  {
    return new Array<Vector2>(usegs*vsegs, /* Captured: int/* unresolved */ usegsint/* unresolved */ vsegs*/(var i)
     => {
        int/* unresolved */ row  = i/vsegs;
        int/* unresolved */ col  = i%usegs;
        return new Vector2((float/* unresolved */)col/(usegs-1), (float/* unresolved */)row/(vsegs-1));
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
  public static IArray<Int4> ComputeQuadStripIndices(int/* unresolved */ usegs, int/* unresolved */ vsegs)
  {
    return new Array<Int4>((usegs-1)*(vsegs-1), /* Captured: int/* unresolved */ usegsint/* unresolved */ vsegs*/(var i)
     => {
        int/* unresolved */ row  = i/(vsegs-1);
        int/* unresolved */ col  = i%(usegs-1);
        int/* unresolved */ nextCol  = (col+1);
        int/* unresolved */ nextRow  = (row+1);
        int/* unresolved */ a  = (row*usegs)+col;
        int/* unresolved */ b  = (row*usegs)+nextCol;
        int/* unresolved */ c  = (nextRow*usegs)+nextCol;
        int/* unresolved */ d  = (nextRow*usegs)+col;
        return new Int4(a, b, c, d);
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
  public static Vector3 UvToSphere(Vector2 uv, float/* unresolved */ radius)
  {
    return new Vector3(-radius*Cos()*Sin(), radius*Cos(), radius*Cos()*Sin());
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
  public static Vector3 UvToTorus(Vector2 uv, float/* unresolved */ radius, float/* unresolved */ tube)
  {
    uv = uv*MathF/* unresolved */.PI/* unresolved */*2;
    return new Vector3((radius+tube*Cos())*Cos(), (radius+tube*Cos())*Sin(), tube*Sin());
    }
  /*
  public static void TestOperator()
        {
            var x = new Vector3(1, 2, 3);
            var y = x + x;
        }
  */
  public static void/* unresolved */ TestOperator()
  {
    Vector3 x  = new Vector3(1, 2, 3);
    Vector3 y  = x+x;
    }
  /*
  public static QuadMesh Torus(int rows, int cols, float radius, float tube)
            => ToQuadMesh(uv => UvToTorus(uv, radius, tube), rows, cols);
  */
  public static QuadMesh Torus(int/* unresolved */ rows, int/* unresolved */ cols, float/* unresolved */ radius, float/* unresolved */ tube)
  {
    return ToQuadMesh(/* Captured: float/* unresolved */ radiusfloat/* unresolved */ tube*/(var uv)
     => {
        return UvToTorus(uv, radius, tube);
        }
      , rows, cols);
    }
  /*
  public static int[] ToIntArray(this IArray<Int3> faces)
            => faces.SelectMany(f => f).ToArray();
  */
  public static /* unresolved */ ToIntArray(IArray<Int3> faces)
  {
    return ToArray();
    }
  }
//==end==//
