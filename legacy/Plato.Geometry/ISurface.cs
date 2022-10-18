using System;
using System.Runtime.CompilerServices;
using Plato.Math;

namespace Plato.Geometry
{
    /// <summary>
    /// https://en.wikipedia.org/wiki/Surface_(topology)
    /// Represents continuous parametrized surfaces.
    /// </summary>
    public interface ISurface : ITransformable<ISurface>
    {
        bool ClosedX { get; }
        bool ClosedY { get; }
        Vector3 Sample(Vector2 uv);
    }

    public class Surface : ISurface
    {
        public bool ClosedX { get; }
        public bool ClosedY { get; }

        public Func<Vector2, Vector3> Func { get; }

        public Surface(Func<Vector2, Vector3> func, bool closedX, bool closedY)
            => (Func, ClosedX, ClosedY) = (func, closedX, closedY);

        public ISurface Transform(Matrix4x4 mat)
            => new Surface(x => Func(x).Transform(mat), ClosedX, ClosedY);

        public Vector3 Sample(Vector2 uv)
            => Func(uv);
    }

    public static class SurfaceOperations
    {
        public static Vector3 RotateAround(Vector3 point, Vector3 axis, float angleInRad)
            => point.Transform(Quaternion.CreateFromAxisAngle(axis, angleInRad));

        public static ISurface Create(this Func<Vector2, Vector3> func, bool closedOnX, bool closedOnY)
            => new Surface(func, closedOnX, closedOnY);

        public static ISurface Create(this Func<float, Func<float, Vector3>> func)
            => new Surface(uv => func(uv.X)(uv.Y), false, false);

        public static ISurface Sweep(this ICurve<Vector3> profile, ICurve<Vector3> path)
            => Create(uv => profile.Sample(uv.X) + path.Sample(uv.Y), profile.Closed, path.Closed);

        // TODO: I need chord and arc functions

        // TODO: should this actually be defined in terms of sweep, where the curve is a chord or arc. 
        public static ISurface Revolve(this ICurve<Vector3> profile, Vector3 axis, float angleInRad, bool closed)
            => Create((Vector2 uv) => profile.Sample(uv.X).RotateAround(axis, angleInRad / uv.Y), profile.Closed, closed);

        public static Vector3 LerpAlongVector(this Vector3 v, Vector3 direction, float t)
            => v.Lerp(v + direction, t);

        public static ISurface Extrude(this ICurve<Vector3> profile, Vector3 vector)
            => Create((Vector2 uv) => profile.Sample(uv.X).LerpAlongVector(vector, uv.Y), profile.Closed, false);

        public static ISurface Loft(this ICurve<Vector3> path, IArray<ICurve<Vector3>> profiles)
            => throw new NotImplementedException();

        public static Vertex GetVertex(this ISurface surface, Vector2 uv)
            => new Vertex(surface.Sample(uv), surface.GetNormal(uv), ColorRGBA.Zero, uv);

        public static Vector3 GetNormal(this ISurface surface, Vector2 uv)
        {
            var p0 = surface.Sample(uv);
            const float eps = 1f / (1000 * 1000);
            var p1 = surface.Sample(uv + (eps, 0));
            var p2 = surface.Sample(uv + (0, eps));
            return p2.Cross(p1).Normalize();
        }
    }
}