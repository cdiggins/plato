namespace Ara3D.Geometry
{
    /// <summary>
    /// The Plato.Intrinsics.V2 shared sources call a handful of extension methods that normally
    /// come from the GENERATED library (which PlatoTests deliberately does not compile).
    /// IntrinsicsV2SurfaceTests only reflects over the V2 surface, never executes these, so
    /// minimal stand-ins are enough to compile. If a future V2 change calls a new generated
    /// extension, the build error lands here: add another one-liner.
    /// </summary>
    internal static class IntrinsicsV2TestShims
    {
        public static bool AlmostZero(this Vector3 v) => v.Value.LengthSquared() < 1e-12f;
    }
}
