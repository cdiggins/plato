namespace Ara3D.Geometry;

using Ara3D.Collections;

/// <summary>
/// Array intrinsics that plato-src declares but Ara3D.Collections does not implement yet;
/// earcut.plato is their first consumer. See ../FINDINGS.md.
/// </summary>
public static class EarcutSupport
{
    public static IReadOnlyList<T> Concatenate<T>(this IReadOnlyList<T> xs, IReadOnlyList<T> ys)
        => xs.Concat(ys);
}
