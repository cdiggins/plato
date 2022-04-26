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

    public class Vector3
    {
        public readonly float X, Y, Z;
        public Vector3(float x = 0, float y = 0, float z = 0) => (X, Y, Z) = (x, y, z);
        public static Vector3 Zero => new();
        public Vector3 WithX(float x) => new(x, Y, Z);
        public Vector3 WithY(float y) => new(X, y, Z);
        public Vector3 WithZ(float z) => new(X, Y, z);
        public static implicit operator Vector3(float v) => new(v, v, v);
        public static Vector3 operator +(Vector3 q, Vector3 r) => new(q.X + r.X, q.Y + r.Y, q.Z + r.Z);
        public static Vector3 operator *(Vector3 q, Vector3 r) => new(q.X * r.X, q.Y * r.Y, q.Z * r.Z);
        public static Vector3 operator /(Vector3 q, Vector3 r) => new(q.X / r.X, q.Y / r.Y, q.Z / r.Z);
        public float Dot(Vector3 v) => X * v.X + v.Y * Y + Z * v.Z;
        public float MagnitudeSquared => Dot(this);
        public float Magnitude => MathF.Sqrt(MagnitudeSquared);
        public Vector3 Normal => this / Magnitude;
        public override string ToString() => "Vector3(" + X + "," + Y + "," + Z + ")";
    }

    public class Int3
    {
        public readonly int X, Y, Z;
        public Int3(int x = 0, int y = 0, int z = 0) => (X, Y, Z) = (x, y, z);
        public static Int3 Zero => new();
        public Int3 WithX(int x) => new(x, Y, Z);
        public Int3 WithY(int y) => new(X, y, Z);
        public Int3 WithZ(int z) => new(X, Y, z);
        public override string ToString() => "Int3(" + X + "," + Y + "," + Z + ")";
    }

    public class Int4
    {
        public readonly int X, Y, Z, W;
        public Int4(int x = 0, int y = 0, int z = 0, int w = 0) => (X, Y, Z, W) = (x, y, z, w);
        public static Int4 Zero => new();
        public Int4 WithX(int x) => new(x, Y, Z, W);
        public Int4 WithY(int y) => new(X, y, Z, W);
        public Int4 WithZ(int z) => new(X, Y, z, W);
        public Int4 WithW(int w) => new(X, Y, Z, w);
        public override string ToString() => "Int4(" + X + "," + Y + "," + Z + "," + W + ")";
    }

    public class TriMesh
    {
        public TriMesh(IArray<Vector3> points, IArray<Int3> indices)
            => (Points, Indices) = (points, indices);
        public IArray<Int3> Indices { get; }
        public IArray<Vector3> Points { get; }
    }

    public class QuadMesh
    {
        public QuadMesh(IArray<Vector3> points, IArray<Int4> indices)
            => (Points, Indices) = (points, indices);
        public IArray<Vector3> Points { get; }
        public IArray<Int4> Indices { get; }
    }

    public static class Extensions
    {
        public static IArray<T> Select<T>(this int count, Func<int, T> func)
            => new Array<T>(count, func);

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

        public static QuadMesh ToQuadMesh(this Func<Vector2, Vector3> func, int rows, int cols, bool wrapRows = false, bool wrapCols = false)
            => new (
                ComputeQuadStripUVs(rows, cols).Select(func),
                ComputeQuadStripIndices(rows, cols, wrapRows, wrapCols));

        public static IArray<Vector2> ComputeQuadStripUVs(int usegs, int vsegs)
            => new Array<Vector2>(usegs * vsegs, 
                i => new((float)i / (usegs - 1), (float)i % (vsegs - 1)));

        public static IArray<Int4> ComputeQuadStripIndices(int usegs, int vsegs, bool wrapUSegs = false, bool wrapVSegs = false)
        {
            var maxUSegs = wrapUSegs ? usegs : usegs + 1;
            var maxVSegs = wrapVSegs ? vsegs : vsegs + 1;

            return new Array<Int4>(usegs * vsegs, k =>
            {
                var i = k / vsegs;
                var rowA = i * maxUSegs;
                var rowB = ((i + 1) % maxVSegs) * maxUSegs;

                var j = k % usegs;
                var colA = j;
                var colB = (j + 1) % maxUSegs;

                return new(rowA + colA, rowA + colB, rowB + colB, rowB + colA);
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

        public static void TestOperator()
        {
            var x = new Vector3(1, 2, 3);
            var y = x + x;
        }
    }
}