using System.Collections.Immutable;
using System.Diagnostics;
using System.Xml.Schema;

namespace PlatoTestJavaScript
{
    public interface IArray<T>
    {
        int Count { get; }
        T this[int index] { get; }
    }

    public class Array<T> : IArray<T>
    {
        public Array(int count, Func<int, T> func) => (Count, Func) = (count, func);
        public Func<int, T> Func { get; }
        public int Count { get; }
        public T this[int index] => Func(index);
    }

    public class Vector2
    {
        public readonly float X, Y;
        public Vector2(float x = 0, float y = 0) => (X, Y) = (x, y);
        public static Vector2 Zero => new();
        public Vector2 WithX(float x) => new(x, Y);
        public Vector2 WithY(float y) => new(X, y);
        public static implicit operator Vector2(float v) => new(v, v);
        public static Vector2 operator +(Vector2 q, Vector2 r) => new(q.X + r.X, q.Y + r.Y);
        public static Vector2 operator *(Vector2 q, Vector2 r) => new(q.X * r.X, q.Y * r.Y);
        public static Vector2 operator /(Vector2 q, Vector2 r) => new(q.X / r.X, q.Y / r.Y);
        public float Dot(Vector2 v) => X * v.X + v.Y * Y;
        public float MagnitudeSquared => Dot(this);
        public float Magnitude => MathF.Sqrt(MagnitudeSquared);
        public Vector2 Normal => this / Magnitude;
        public override string ToString() => "Vector2(" + X  + "," + Y + ")";
    }

    public class Vector3 : IArray<float>
    {
        public readonly float X, Y, Z;
        public Vector3(float x = 0, float y = 0, float z = 0) => (X, Y, Z) = (x, y, z);
        public static Vector3 Zero => new();
        public Vector3 WithX(float x) => new(x, Y, Z);
        public Vector3 WithY(float y) => new(X, y, Z);
        public Vector3 WithZ(float z) => new(X, Y, z);
        public static implicit operator Vector3(float v) => new(v, v, v);
        public static Vector3 operator +(Vector3 q, Vector3 r) => new(q.X + r.X, q.Y + r.Y, q.Z + r.Z);
        public static Vector3 operator -(Vector3 q, Vector3 r) => new(q.X - r.X, q.Y - r.Y, q.Z - r.Z);
        public static Vector3 operator *(Vector3 q, Vector3 r) => new(q.X * r.X, q.Y * r.Y, q.Z * r.Z);
        public static Vector3 operator /(Vector3 q, Vector3 r) => new(q.X / r.X, q.Y / r.Y, q.Z / r.Z);
        public float Dot(Vector3 v) => X * v.X + v.Y * Y + Z * v.Z;
        public float MagnitudeSquared => Dot(this);
        public float Magnitude => MathF.Sqrt(MagnitudeSquared);
        public Vector3 Normal => this / Magnitude;
        public override string ToString() => "Vector3(" + X + "," + Y + "," + Z + ")";
        public int Count => 3;
        public float this[int i] => i switch
        {
            0 => X,
            1 => Y,
            2 => Z,
            _ => throw new ArgumentOutOfRangeException()
        };
        public Vector3 Cross(Vector3 v)
            => new(Y* v.Z - Z* v.Y, Z* v.X - X* v.Z, X* v.Y - Y* v.X);
    }

    public class Int3 : IArray<int>
    {
        public readonly int X, Y, Z;
        public Int3(int x = 0, int y = 0, int z = 0) => (X, Y, Z) = (x, y, z);
        public static Int3 Zero => new();
        public Int3 WithX(int x) => new(x, Y, Z);
        public Int3 WithY(int y) => new(X, y, Z);
        public Int3 WithZ(int z) => new(X, Y, z);
        public override string ToString() => "Int3(" + X + "," + Y + "," + Z + ")";
        public int Count => 3;
        public int this[int i] => i switch
        {
            0 => X,
            1 => Y,
            2 => Z,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public class Int4 : IArray<int>
    {
        public readonly int X, Y, Z, W;
        public Int4(int x = 0, int y = 0, int z = 0, int w = 0) => (X, Y, Z, W) = (x, y, z, w);
        public static Int4 Zero => new();
        public Int4 WithX(int x) => new(x, Y, Z, W);
        public Int4 WithY(int y) => new(X, y, Z, W);
        public Int4 WithZ(int z) => new(X, Y, z, W);
        public Int4 WithW(int w) => new(X, Y, Z, w);
        public override string ToString() => "Int4(" + X + "," + Y + "," + Z + "," + W + ")";
        public int Count => 4;
        public int this[int i] => i switch
        {
            0 => X,
            1 => Y,
            2 => Z,
            3 => W,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public class Points
    {
        public Points(IArray<Vector3> positions, IArray<Vector2> uvs, IArray<Vector3> normals)
            => (Positions, UVs, Normals) = (positions, uvs, normals);
        public IArray<Vector3> Positions { get; }
        public IArray<Vector2> UVs { get; }
        public IArray<Vector3> Normals { get; }
    }

    public class TriMesh 
    {
        public TriMesh(Points points, IArray<Int3> faces)
            => (Points, Faces) = (points, faces);
        public Points Points { get; }
        public IArray<Int3> Faces { get; }
    }

    public class QuadMesh
    {
        public QuadMesh(Points points, IArray<Int4> faces)
            => (Points, Faces) = (points, faces);
        public Points Points { get; }
        public IArray<Int4> Faces { get; }
    }

    public class Triangle
    {
        public Vector3 A { get; }
        public Vector3 B { get; }
        public Vector3 C { get; }

        public Triangle(Vector3 a, Vector3 b, Vector3 c)
            => (A, B, C) = (a, b, c);

        public Vector3 Normal => (B - A).Cross(C - A).Normal;
    }

    public static class Extensions
    {
        public static IArray<T> ToIArray<T>(this IReadOnlyList<T> self)
            => self.Count.Select(i => self[i]);

        // TODO: stuff like this, should probably be moved to an adapter layer. A special project for interop 
        // with legacy code. A Plato project might not need this. Though technically this is like FFI / Interop / Marshalling
        public static T[] ToArray<T>(this IArray<T> self)
        {
            var r = new T[self.Count];
            for (var i = 0; i < self.Count; ++i)
                r[i] = self[i];
            return r;
        }

        public static float[] ToFloatArray(this IArray<Vector3> self)
            => self.SelectMany(x => x).ToArray();

        public static IArray<T> Select<T>(this int count, Func<int, T> func)
            => new Array<T>(count, func);

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

        public static IArray<U> Select<T, U>(this IArray<T> self, Func<T, U> func)
            => self.Count.Select(i => func(self[i]));

        public static float Cos(this float self) 
            => MathF.Cos(self);

        public static float Sin(this float self) 
            => MathF.Sin(self);

        public static float UnitToRad(this float self)
            => self * MathF.PI;

        public static IArray<float> SampleFloats(int count, float max = 1.0f)
            => count.Select(i => max * count);

        public static IArray<Int3> ToTriangleIndices(this IArray<Int4> self)
            => self.SelectMany(f => new[] { new Int3(f.X, f.Y, f.Z), new Int3(f.Z, f.W, f.X) }.ToIArray());

        public static QuadMesh ToQuadMesh(this Func<Vector2, Vector3> func, int rows, int cols)
            => new (ComputeQuadStripPoints(func, rows, cols),
                ComputeQuadStripIndices(rows, cols));

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

        public static Points UVsToPoints(this IArray<Vector2> uvs, Func<Vector2, Vector3> func)
            => new(uvs.Select(func), uvs, uvs.Select(uv => uv.UVToNormal(func)));

        public static Points ComputeQuadStripPoints(this Func<Vector2, Vector3> func, int usegs, int vsegs)
            => ComputeQuadStripUVs(usegs, vsegs).UVsToPoints(func);

        public static IArray<Vector2> ComputeQuadStripUVs(int usegs, int vsegs)
            => new Array<Vector2>(usegs * vsegs, 
                i =>
                {
                    var row = i / vsegs;
                    var col = i % usegs;
                    return new((float)col / (usegs - 1), (float)row / (vsegs - 1));
                });

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

        // see: https://github.com/mrdoob/three.js/blob/9ef27d1af7809fa4d9943f8d4c4644e365ab6d2d/src/geometries/SphereBufferGeometry.js#L76
        public static Vector3 UvToSphere(Vector2 uv, float radius)
            => new(
                -radius * uv.X.UnitToRad().Cos() * (uv.Y * MathF.PI).Sin(),
                radius * (uv.Y * MathF.PI).Cos(),
                radius * uv.X.UnitToRad().Cos() * (uv.Y * MathF.PI).Sin());

        // see: https://github.com/mrdoob/three.js/blob/9ef27d1af7809fa4d9943f8d4c4644e365ab6d2d/src/geometries/TorusBufferGeometry.js#L52
        public static Vector3 UvToTorus(Vector2 uv, float radius, float tube)
        {
            uv = uv * MathF.PI * 2;
            return new Vector3(
                (radius + tube * uv.Y.Cos()) * uv.X.Cos(),
                (radius + tube * uv.Y.Cos()) * uv.X.Sin(),
                tube * uv.Y.Sin());
        }

        public static TriMesh ToTriMesh(this QuadMesh mesh)
            => new(mesh.Points, mesh.Faces.ToTriangleIndices());

        public static void TestOperator()
        {
            var x = new Vector3(1, 2, 3);
            var y = x + x;
        }

        public static QuadMesh Torus(int rows, int cols, float radius, float tube)
            => ToQuadMesh(uv => UvToTorus(uv, radius, tube), rows, cols);

        public static int[] ToIntArray(this IArray<Int3> faces)
            => faces.SelectMany(f => f).ToArray();

        public static IArray<Triangle> Triangles(this TriMesh mesh)
            => mesh.Faces.Select(f => new Triangle(mesh.Points.Positions[f.X], mesh.Points.Positions[f.Y], mesh.Points.Positions[f.Z]));

        public static IArray<Vector3> FaceNormals(this TriMesh mesh)
            => mesh.Triangles().Select(tri => tri.Normal);

        public static (T, TimeSpan) TimeIt<T>(Func<T> func)
        {
            var sw = Stopwatch.StartNew();
            return (func(), sw.Elapsed);
        }

        public static void Log(string s)
            => Debug.WriteLine(s);

        public static T LogTiming<T>(Func<T> func)
        {
            var r = TimeIt(func);
            Debug.WriteLine("msec elapsed: " + r.Item2.Milliseconds);
            return r.Item1;
        }

        public static void Main()
        {
            var torus = Torus(500, 100, 1, 0.2f).ToTriMesh();
            var floats = LogTiming(torus.FaceNormals().ToFloatArray);
            var filePath = Path.Combine(Path.GetTempPath(), "profiling.txt");
            File.WriteAllLines(filePath, floats.Select(f => f.ToString()));
        }
    }
}