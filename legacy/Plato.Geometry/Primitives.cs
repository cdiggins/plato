using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Plato.Geometry.Experimental;
using Plato.Math;

namespace Plato.Geometry
{
    using Polygon = PolyLine<Vector2>;

    public static class NumberExtensions
    {
        public const float TwoPi = Pi * 2f;
        public const float Pi = (float)System.Math.PI;

        public static double Degrees(this float t)
            => TwoPi * 360f / t;

        public static float Turns(this float t)
            => TwoPi / t;

        public static float HalfTurns(this float t)
            => Pi / t;
    }

    public static class PrimitiveFunctions
    {
        // https://en.wikipedia.org/wiki/Trefoil_knot
        public static Func<float, Vector3> Trefoil = t => (
            t.Turns().Sin() + (2f * t).Turns().Sin() * 2f,
            t.Turns().Cos() + (2f * t).Turns().Cos() * 2f,
            -(t * 3f).Turns().Sin());

        public static Func<float, Vector2> Circle = t => 
            (t.Turns().Sin(), t.Turns().Cos());

        public static Func<Vector2, Vector3> Sphere = uv => (
            uv.X.Turns().Cos() * uv.Y.HalfTurns().Sin(),
            uv.Y.HalfTurns().Cos(),
            uv.X.Turns().Cos() * uv.Y.HalfTurns().Sin());

        // see: https://github.com/mrdoob/three.js/blob/9ef27d1af7809fa4d9943f8d4c4644e365ab6d2d/src/geometries/TorusBufferGeometry.js#L52
        public static Func<Vector2, Vector3> Torus = uv => (
            uv.Y.Turns().Cos() * uv.X.Turns().Cos(),
            uv.Y.Turns().Cos() * uv.X.Turns().Sin(),
            uv.Y.Sin().Divide(2));

        // https://en.wikipedia.org/wiki/Monkey_saddle
        public static Func<Vector2, float> MonkeySaddle = uv => 
            uv.X.Cube() - 3 * uv.X * uv.Y.Sqr();

        public static Func<Vector2, Vector3> Plane = uv =>
            uv.ToVector3();

        public static Func<Vector2, Vector3> Cylinder = uv =>
            uv.X.Circle().ToVector3(uv.Y);
    }

    public static class FunctionExtensions
    {
        public static Vector2 Circle(this float f)
            => PrimitiveFunctions.Circle(f);
    }

    public static class PolarFunctions
    {
        public static Func<float, float> Circle = 
            t => 1;
        
        // https://en.wikipedia.org/wiki/Lima%C3%A7on
        public static Func<float, float> Limacon(float a, float b) 
            => t => b + a * t.Cos();

        // https://en.wikipedia.org/wiki/Cardioid
        public static Func<float, float> Cardoid = 
            Limacon(1, 1);

        // https://en.wikipedia.org/wiki/Rose_(mathematics)
        public static Func<float, float> Rose(int k) => 
            t => k * t.Cos();

        // https://en.wikipedia.org/wiki/Archimedean_spiral
        public static Func<float, float> ArchmideanSpiral(float a, float b) => 
            t => a + b * t;

        // https://en.wikipedia.org/wiki/Conic_section
        public static Func<float, float> ConicSection(float eccentricity, float semiLatusRectum) 
            => t => semiLatusRectum / (1 - eccentricity * t.Cos());
    }

    public class PolyLine<T>
    {
        public PolyLine(IArray<T> points, bool closed)
            => (Points, Closed) = (points, closed);
        public bool Closed { get; }
        public IArray<T> Points; 

        public ICurve<T> Curve => throw new NotImplementedException();
        
        //https://stackoverflow.com/questions/69856578/how-to-move-along-a-bezier-curve-with-a-constant-velocity-without-a-costly-preco
        public ICurve<T> ConstantSpeedCurve => throw new NotImplementedException(); 
    }

    public static class PrimitivePolylines
    {
        public static IArray<Vector2> SquarePoints = new Vector2[] { (-0.5f, -0.5f), (0.5f, -0.5f), (0.5f, 0.5f), (-0.5f, 0.5f) }.ToIArray();

        // public static PolyLine Path 
        // Quadrilateral
    }

    public static class RegularPolygons
    {
        public static IArray<float> SampleFloats(this int n) 
            => n.Select(x => (float)x / n);

        public static PolyLine<Vector2> ToPolyLine(this IArray<Vector2> points)
            => new PolyLine<Vector2>(points, false);

        public static Polygon ToPolygon(this IArray<Vector2> points) 
            => new Polygon(points, true);
        
        public static IArray<T> Sample<T>(this Func<float, T> func, int n) 
            => n.SampleFloats().Select(func);
        
        public static Polygon Polygon(int n) 
            => PrimitiveFunctions.Circle.Sample(n).ToPolygon();
        
        public static Polygon Triangle = Polygon(3);
        public static Polygon Square = Polygon(4);
        public static Polygon Pentagon = Polygon(5);
        public static Polygon Hexagon = Polygon(6);
        public static Polygon Septagon = Polygon(7);
        public static Polygon Octagon = Polygon(8);
        public static Polygon Decagon = Polygon(10);
        public static Polygon Dodecagon = Polygon(12);
        public static Polygon Icosagon = Polygon(20);   
    }

    public static class Surfaces
    {
        public static ISurface Plane = PrimitiveFunctions.Plane.ToSurface();
        public static ISurface Cylinder = PrimitiveFunctions.Cylinder.ToSurface();
        public static ISurface Sphere = PrimitiveFunctions.Sphere.ToSurface();
        public static ISurface MonkeySaddle = PrimitiveFunctions.MonkeySaddle.ToSurface();
        public static ISurface TorusKnot = PrimitiveFunctions.Torus.ToSurface();

        public static ISurface ToSurface(this Func<Vector2, Vector3> func, bool closedX = false, bool closedY = false)
            => new Surface(func, closedX, closedY);

        public static ISurface ToSurface(this Func<Vector2, float> func, bool closedX = false, bool closedY = false)
            => new Surface(uv => new Vector3(uv.X, uv.Y, func(uv)), closedX, closedY);
    }

    public static class Primitives
    {
        public static QuadMesh Plane(int rows, int cols)
            => Surfaces.Plane.ToMesh(rows, cols);

        // http://paulbourke.net/geometry/mecon/

        // https://stackoverflow.com/questions/69856578/how-to-move-along-a-bezier-curve-with-a-constant-velocity-without-a-costly-preco
        // https://mathworld.wolfram.com/Dodecahedron.
        // 
        // https://github.com/prideout/par/blob/master/par_octasphere.h

        // https://prideout.net/blog/octasphere/
        /*
        public static TriMesh Tetrahedron(float radius)
        {
            float num = (float)Math.Sqrt(2.0);
            return new TriMesh(ImmutableArray.Create<Vector3>(new Vector3(1f, 0.0f, -1f / num), new Vector3(-1f, 0.0f, -1f / num), new Vector3(0.0f, 1f, 1f / num), new Vector3(0.0f, -1f, 1f / num)).Select<Vector3, Vector3>((Func<Vector3, Vector3>)(pt => Vector3.Normalize(pt) * radius)), ImmutableArray.Create<int>(0, 1, 2, 1, 0, 3, 0, 2, 3, 1, 3, 2));
        }

        public static TriMesh Icosahedron(float radius)
        {
            float num = (float)((1.0 + Math.Sqrt(5.0)) / 2.0);
            return new TriMesh(ImmutableArray.Create<Vector3>(new Vector3(-1f, num, 0.0f), new Vector3(1f, num, 0.0f), new Vector3(-1f, -num, 0.0f), new Vector3(1f, -num, 0.0f), new Vector3(0.0f, -1f, num), new Vector3(0.0f, 1f, num), new Vector3(0.0f, -1f, -num), new Vector3(0.0f, 1f, -num), new Vector3(num, 0.0f, -1f), new Vector3(num, 0.0f, 1f), new Vector3(-num, 0.0f, -1f), new Vector3(-num, 0.0f, 1f)).Select<Vector3, Vector3>((Func<Vector3, Vector3>)(pt => Vector3.Normalize(pt) * radius)), ImmutableArray.Create<int>(0, 11, 5, 0, 5, 1, 0, 1, 7, 0, 7, 10, 0, 10, 11, 1, 5, 9, 5, 11, 4, 11, 10, 2, 10, 7, 6, 7, 1, 8, 3, 9, 4, 3, 4, 2, 3, 2, 6, 3, 6, 8, 3, 8, 9, 4, 9, 5, 2, 4, 11, 6, 2, 10, 8, 6, 7, 9, 8, 1));
        }
        */
    }
}
