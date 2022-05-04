//==begin==//
public interface IArray<T> : 
{
  /*
  int Count { get; }
  */
  public Int32/* unresolved */ Count
  {
  get;
    }
  ;
  /*
  T this[int index] { get; }
  */
  public T this[Int32/* unresolved */index]
  
  {
  get;
    }
  }
//==end==//
//==begin==//
public class Array<T> : 
{
  public Func<Int32/* unresolved */, T>/* unresolved */ _Func_ ;
  public Int32/* unresolved */ _Count_ ;
  /*
  public Array(int count, Func<int, T> func) => (Count, Func) = (count, func);
  */
  public Array(Int32/* unresolved */count, Func<Int32/* unresolved */, T>/* unresolved */func)
  {
      return (Count, Func) = (count, func);
      }
    /*
  public Func<int, T> Func { get; }
  */
  public Func<Int32/* unresolved */, T>/* unresolved */ Func
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
  public Int32/* unresolved */ Count
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
  public T this[Int32/* unresolved */index]
  
  {
  get;
    }
  }
//==end==//
//==begin==//
public class Vector2 : 
{
  /*
  public Vector2(float x = 0, float y = 0) => (X, Y) = (x, y);
  */
  public Vector2(Single/* unresolved */x = 0, Single/* unresolved */y = 0)
  {
      return (X, Y) = (x, y);
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
  public Single/* unresolved */ MagnitudeSquared
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
  public Single/* unresolved */ Magnitude
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
  public Vector2 WithX(Single/* unresolved */x)
  {
    return new Vector2(x, Y);
    }
  /*
  public Vector2 WithY(float y) => new(X, y);
  */
  public Vector2 WithY(Single/* unresolved */y)
  {
    return new Vector2(X, y);
    }
  /*
  public float Dot(Vector2 v) => X * v.X + v.Y * Y;
  */
  public Single/* unresolved */ Dot(Vector2v)
  {
    return X*v.X+v.Y*Y;
    }
  /*
  public override string ToString() => "Vector2(" + X  + "," + Y + ")";
  */
  public String/* unresolved */ ToString()
  {
    return "Vector2("+X+","+Y+")";
    }
  /*
  public static implicit operator Vector2(float v) => new(v, v);
  */
  public static Vector2 operator implicit(Single/* unresolved */v)
  {
    return new Vector2(v, v);
    }
  /*
  public static Vector2 operator +(Vector2 q, Vector2 r) => new(q.X + r.X, q.Y + r.Y);
  */
  public static Vector2 operator +(Vector2q, Vector2r)
  {
    return new Vector2(q.X+r.X, q.Y+r.Y);
    }
  /*
  public static Vector2 operator *(Vector2 q, Vector2 r) => new(q.X * r.X, q.Y * r.Y);
  */
  public static Vector2 operator *(Vector2q, Vector2r)
  {
    return new Vector2(q.X*r.X, q.Y*r.Y);
    }
  /*
  public static Vector2 operator /(Vector2 q, Vector2 r) => new(q.X / r.X, q.Y / r.Y);
  */
  public static Vector2 operator /(Vector2q, Vector2r)
  {
    return new Vector2(q.X/r.X, q.Y/r.Y);
    }
  }
//==end==//
//==begin==//
public class Vector3 : 
{
  /*
  public Vector3(float x = 0, float y = 0, float z = 0) => (X, Y, Z) = (x, y, z);
  */
  public Vector3(Single/* unresolved */x = 0, Single/* unresolved */y = 0, Single/* unresolved */z = 0)
  {
      return (X, Y, Z) = (x, y, z);
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
  public Single/* unresolved */ MagnitudeSquared
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
  public Single/* unresolved */ Magnitude
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
  public Int32/* unresolved */ Count
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
  public Vector3 WithX(Single/* unresolved */x)
  {
    return new Vector3(x, Y, Z);
    }
  /*
  public Vector3 WithY(float y) => new(X, y, Z);
  */
  public Vector3 WithY(Single/* unresolved */y)
  {
    return new Vector3(X, y, Z);
    }
  /*
  public Vector3 WithZ(float z) => new(X, Y, z);
  */
  public Vector3 WithZ(Single/* unresolved */z)
  {
    return new Vector3(X, Y, z);
    }
  /*
  public float Dot(Vector3 v) => X * v.X + v.Y * Y + Z * v.Z;
  */
  public Single/* unresolved */ Dot(Vector3v)
  {
    return X*v.X+v.Y*Y+Z*v.Z;
    }
  /*
  public override string ToString() => "Vector3(" + X + "," + Y + "," + Z + ")";
  */
  public String/* unresolved */ ToString()
  {
    return "Vector3("+X+","+Y+","+Z+")";
    }
  /*
  public Vector3 Cross(Vector3 v)
            => new(Y* v.Z - Z* v.Y, Z* v.X - X* v.Z, X* v.Y - Y* v.X);
  */
  public Vector3 Cross(Vector3v)
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
  public Single/* unresolved */ this[Int32/* unresolved */i]
  
  {
  get;
    }
  /*
  public static implicit operator Vector3(float v) => new(v, v, v);
  */
  public static Vector3 operator implicit(Single/* unresolved */v)
  {
    return new Vector3(v, v, v);
    }
  /*
  public static Vector3 operator +(Vector3 q, Vector3 r) => new(q.X + r.X, q.Y + r.Y, q.Z + r.Z);
  */
  public static Vector3 operator +(Vector3q, Vector3r)
  {
    return new Vector3(q.X+r.X, q.Y+r.Y, q.Z+r.Z);
    }
  /*
  public static Vector3 operator -(Vector3 q, Vector3 r) => new(q.X - r.X, q.Y - r.Y, q.Z - r.Z);
  */
  public static Vector3 operator -(Vector3q, Vector3r)
  {
    return new Vector3(q.X-r.X, q.Y-r.Y, q.Z-r.Z);
    }
  /*
  public static Vector3 operator *(Vector3 q, Vector3 r) => new(q.X * r.X, q.Y * r.Y, q.Z * r.Z);
  */
  public static Vector3 operator *(Vector3q, Vector3r)
  {
    return new Vector3(q.X*r.X, q.Y*r.Y, q.Z*r.Z);
    }
    /*
  public static Vector3 operator /(Vector3 q, Vector3 r) => new(q.X / r.X, q.Y / r.Y, q.Z / r.Z);
  */
  public static Vector3 operator /(Vector3q, Vector3r)
  {
    return new Vector3(q.X/r.X, q.Y/r.Y, q.Z/r.Z);
    }
  }
//==end==//
//==begin==//
public class Int3 : 
{
  /*
  public Int3(int x = 0, int y = 0, int z = 0) => (X, Y, Z) = (x, y, z);
  */
  public Int3(Int32/* unresolved */x = 0, Int32/* unresolved */y = 0, Int32/* unresolved */z = 0)
  {
      return (X, Y, Z) = (x, y, z);
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
  public Int32/* unresolved */ Count
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
  public Int3 WithX(Int32/* unresolved */x)
  {
    return new Int3(x, Y, Z);
    }
  /*
  public Int3 WithY(int y) => new(X, y, Z);
  */
  public Int3 WithY(Int32/* unresolved */y)
  {
    return new Int3(X, y, Z);
    }
  /*
  public Int3 WithZ(int z) => new(X, Y, z);
  */
  public Int3 WithZ(Int32/* unresolved */z)
  {
    return new Int3(X, Y, z);
    }
  /*
  public override string ToString() => "Int3(" + X + "," + Y + "," + Z + ")";
  */
  public String/* unresolved */ ToString()
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
  public Int32/* unresolved */ this[Int32/* unresolved */i]
  
  {
  get;
    }
  }
//==end==//
//==begin==//
public class Int4 : 
{
  /*
  public Int4(int x = 0, int y = 0, int z = 0, int w = 0) => (X, Y, Z, W) = (x, y, z, w);
  */
  public Int4(Int32/* unresolved */x = 0, Int32/* unresolved */y = 0, Int32/* unresolved */z = 0, Int32/* unresolved */w = 0)
  {
      return (X, Y, Z, W) = (x, y, z, w);
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
  public Int32/* unresolved */ Count
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
  public Int4 WithX(Int32/* unresolved */x)
  {
    return new Int4(x, Y, Z, W);
    }
  /*
  public Int4 WithY(int y) => new(X, y, Z, W);
  */
  public Int4 WithY(Int32/* unresolved */y)
  {
    return new Int4(X, y, Z, W);
    }
  /*
  public Int4 WithZ(int z) => new(X, Y, z, W);
  */
  public Int4 WithZ(Int32/* unresolved */z)
  {
    return new Int4(X, Y, z, W);
    }
  /*
  public Int4 WithW(int w) => new(X, Y, Z, w);
  */
  public Int4 WithW(Int32/* unresolved */w)
  {
    return new Int4(X, Y, Z, w);
    }
  /*
  public override string ToString() => "Int4(" + X + "," + Y + "," + Z + "," + W + ")";
  */
  public String/* unresolved */ ToString()
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
  public Int32/* unresolved */ this[Int32/* unresolved */i]
  
  {
  get;
    }
  }
//==end==//
//==begin==//
public class Points : 
{
  public IArray<Vector3> _Positions_ ;
  public IArray<Vector2> _UVs_ ;
  public IArray<Vector3> _Normals_ ;
  /*
  public Points(IArray<Vector3> positions, IArray<Vector2> uvs, IArray<Vector3> normals)
            => (Positions, UVs, Normals) = (positions, uvs, normals);
  */
  public Points(IArray<Vector3>positions, IArray<Vector2>uvs, IArray<Vector3>normals)
  {
      return (Positions, UVs, Normals) = (positions, uvs, normals);
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
public class TriMesh : 
{
  public Points _Points_ ;
  public IArray<Int3> _Faces_ ;
  /*
  public TriMesh(Points points, IArray<Int3> faces)
            => (Points, Faces) = (points, faces);
  */
  public TriMesh(Pointspoints, IArray<Int3>faces)
  {
      return (Points, Faces) = (points, faces);
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
public class QuadMesh : 
{
  public Points _Points_ ;
  public IArray<Int4> _Faces_ ;
  /*
  public QuadMesh(Points points, IArray<Int4> faces)
            => (Points, Faces) = (points, faces);
  */
  public QuadMesh(Pointspoints, IArray<Int4>faces)
  {
      return (Points, Faces) = (points, faces);
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
public class Extensions : 
{
  /*
  public static IArray<T> ToIArray<T>(this IReadOnlyList<T> self)
            => self.Count.Select(i => self[i]);
  */
  public static IArray<T> ToIArray<T>(IReadOnlyList<T>/* unresolved */self)
  {
    return Select(/* Captured: IReadOnlyList<T>/* unresolved */self*/(i)
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
  public static /* unresolved */ ToArray<T>(IArray<T>self)
  {
    /* unresolved */ r  = new []{};
    {
      Int32/* unresolved */ i  = 0;
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
  public static /* unresolved */ ToFloatArray(IArray<Vector3>self)
  {
    return ToArray();
    }
  /*
  public static IArray<T> Select<T>(this int count, Func<int, T> func)
            => new Array<T>(count, func);
  */
  public static IArray<T> Select<T>(Int32/* unresolved */count, Func<Int32/* unresolved */, T>/* unresolved */func)
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
  public static IArray<U> SelectMany<T, U>(IArray<T>self, Func<T, IArray<U>>/* unresolved */func)
  {
    List<U>/* unresolved */ r  = new List<U>/* unresolved */();
    {
      Int32/* unresolved */ i  = 0;
      while(i<self.Count)
      {
          {
            IArray<U> tmp  = func/* unresolved */(self[i]);
            {
              Int32/* unresolved */ j  = 0;
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
    return Select(/* Captured: List<U>/* unresolved */ r  = new List<U>/* unresolved */()i*/(i)
     => {
        return r[i];
        }
      );
    }
  /*
  public static IArray<U> Select<T, U>(this IArray<T> self, Func<T, U> func)
            => self.Count.Select(i => func(self[i]));
  */
  public static IArray<U> Select<T, U>(IArray<T>self, Func<T, U>/* unresolved */func)
  {
    return Select(/* Captured: IArray<T>selfFunc<T, U>/* unresolved */func*/(i)
     => {
        return func/* unresolved */(self[i]);
        }
      );
    }
  /*
  public static float Cos(this float self) 
            => MathF.Cos(self);
  */
  public static Single/* unresolved */ Cos(Single/* unresolved */self)
  {
    return Cos/* unresolved */(self);
    }
  /*
  public static float Sin(this float self) 
            => MathF.Sin(self);
  */
  public static Single/* unresolved */ Sin(Single/* unresolved */self)
  {
    return Sin/* unresolved */(self);
    }
  /*
  public static float UnitToRad(this float self)
            => self * MathF.PI;
  */
  public static Single/* unresolved */ UnitToRad(Single/* unresolved */self)
  {
    return self*MathF/* unresolved */.PI/* unresolved */;
    }
  /*
  public static IArray<float> SampleFloats(int count, float max = 1.0f)
            => count.Select(i => max * count);
  */
  public static IArray<Single/* unresolved */> SampleFloats(Int32/* unresolved */count, Single/* unresolved */max = 1.0f)
  {
    return Select(/* Captured: Int32/* unresolved */countSingle/* unresolved */max = 1.0f*/(i)
     => {
        return max*count;
        }
      );
    }
  /*
  public static IArray<Int3> ToTriangles(this IArray<Int4> self)
            => self.SelectMany(f => new[] { new Int3(f.X, f.Y, f.Z), new Int3(f.Z, f.W, f.X) }.ToIArray());
  */
  public static IArray<Int3> ToTriangles(IArray<Int4>self)
  {
    return SelectMany(/* Captured: */(f)
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
  public static QuadMesh ToQuadMesh(Func<Vector2, Vector3>/* unresolved */func, Int32/* unresolved */rows, Int32/* unresolved */cols)
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
  public static Vector3 UVToNormal(Vector2uv, Func<Vector2, Vector3>/* unresolved */func, Single/* unresolved */epsilon = 0.00001f)
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
  public static Points UVsToPoints(IArray<Vector2>uvs, Func<Vector2, Vector3>/* unresolved */func)
  {
    return new Points(Select(func), uvs, Select(/* Captured: Func<Vector2, Vector3>/* unresolved */func*/(uv)
     => {
        return UVToNormal(func);
        }
      ));
    }
  /*
  public static Points ComputeQuadStripPoints(this Func<Vector2, Vector3> func, int usegs, int vsegs)
            => ComputeQuadStripUVs(usegs, vsegs).UVsToPoints(func);
  */
  public static Points ComputeQuadStripPoints(Func<Vector2, Vector3>/* unresolved */func, Int32/* unresolved */usegs, Int32/* unresolved */vsegs)
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
  public static IArray<Vector2> ComputeQuadStripUVs(Int32/* unresolved */usegs, Int32/* unresolved */vsegs)
  {
    return new Array<Vector2>(usegs*vsegs, /* Captured: Int32/* unresolved */usegsInt32/* unresolved */vsegs*/(i)
     => {
        Int32/* unresolved */ row  = i/vsegs;
        Int32/* unresolved */ col  = i%usegs;
        return new Vector2((Single/* unresolved */)col/(usegs-1), (Single/* unresolved */)row/(vsegs-1));
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
  public static IArray<Int4> ComputeQuadStripIndices(Int32/* unresolved */usegs, Int32/* unresolved */vsegs)
  {
    return new Array<Int4>((usegs-1)*(vsegs-1), /* Captured: Int32/* unresolved */usegsInt32/* unresolved */vsegs*/(i)
     => {
        Int32/* unresolved */ row  = i/(vsegs-1);
        Int32/* unresolved */ col  = i%(usegs-1);
        Int32/* unresolved */ nextCol  = (col+1);
        Int32/* unresolved */ nextRow  = (row+1);
        Int32/* unresolved */ a  = (row*usegs)+col;
        Int32/* unresolved */ b  = (row*usegs)+nextCol;
        Int32/* unresolved */ c  = (nextRow*usegs)+nextCol;
        Int32/* unresolved */ d  = (nextRow*usegs)+col;
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
  public static Vector3 UvToSphere(Vector2uv, Single/* unresolved */radius)
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
  public static Vector3 UvToTorus(Vector2uv, Single/* unresolved */radius, Single/* unresolved */tube)
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
  public static Void/* unresolved */ TestOperator()
  {
    Vector3 x  = new Vector3(1, 2, 3);
    Vector3 y  = x+x;
    }
  /*
  public static QuadMesh Torus(int rows, int cols, float radius, float tube)
            => ToQuadMesh(uv => UvToTorus(uv, radius, tube), rows, cols);
  */
  public static QuadMesh Torus(Int32/* unresolved */rows, Int32/* unresolved */cols, Single/* unresolved */radius, Single/* unresolved */tube)
  {
    return ToQuadMesh(/* Captured: Single/* unresolved */radiusSingle/* unresolved */tube*/(uv)
     => {
        return UvToTorus(uv, radius, tube);
        }
      , rows, cols);
    }
  /*
  public static int[] ToIntArray(this IArray<Int3> faces)
            => faces.SelectMany(f => f).ToArray();
  */
  public static /* unresolved */ ToIntArray(IArray<Int3>faces)
  {
    return ToArray();
    }
  }
//==end==//
