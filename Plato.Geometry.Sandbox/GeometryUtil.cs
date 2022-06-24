using System;
using System.Collections.Generic;
using System.Linq;
using Vim.LinqArray;
using Vim.Math3d;

namespace Vim.Geometry
{
    // TODO: many of these functions should live in other places
    public static class GeometryUtil
    {
        public static IArray<Vector3> Normalize(this IArray<Vector3> vectors)
            => vectors.Select(v => v.Normalize());

        public static bool SequenceAlmostEquals(this IArray<Vector3> vs1, IArray<Vector3> vs2, float tolerance = Constants.Tolerance)
            => vs1.Count == vs2.Count && vs1.Indices().All(i => vs1[i].AlmostEquals(vs2[i], tolerance));

        public static bool AreColinear(this IEnumerable<Vector3> vectors, Vector3 reference, float tolerance = (float)Constants.OneTenthOfADegree)
            => !reference.IsNaN() && vectors.All(v => v.Colinear(reference, tolerance));

        public static bool AreColinear(this IEnumerable<Vector3> vectors, float tolerance = (float)Constants.OneTenthOfADegree)
            => vectors.ToList().AreColinear(tolerance);

        public static bool AreColinear(this IList<Vector3> vectors, float tolerance = (float)Constants.OneTenthOfADegree)
            => vectors.Count <= 1 || vectors.Skip(1).AreColinear(vectors[0], tolerance);

        public static AABox BoundingBox(this IArray<Vector3> vertices)
            => AABox.Create(vertices.ToEnumerable());

        public static IArray<float> SampleZeroToOneInclusive(this int count)
            => (count + 1).Select(i => i / (float)count);

        public static IArray<float> SampleZeroToOneExclusive(this int count)
            => count.Select(i => i / (float)count);

        public static IArray<Vector3> InterpolateInclusive(this int count, Func<float, Vector3> function)
            => count.SampleZeroToOneInclusive().Select(function);

        public static IArray<Vector3> Interpolate(this Line self, int count)
            => count.InterpolateInclusive(self.Lerp);

        public static IArray<Vector3> Rotate(this IArray<Vector3> self, Vector3 axis, float angle)
            => self.Transform(Matrix4x4.CreateFromAxisAngle(axis, angle));

        public static IArray<Vector3> Transform(this IArray<Vector3> self, Matrix4x4 matrix)
            => self.Select(x => x.Transform(matrix));

        public static Int3 Sort(this Int3 v)
        {
            if (v.X < v.Y)
            {
                return (v.Y < v.Z)
                    ? new Int3(v.X, v.Y, v.Z)
                    : (v.X < v.Z)
                        ? new Int3(v.X, v.Z, v.Y)
                        : new Int3(v.Z, v.X, v.Y);
            }
            else
            {
                return (v.X < v.Z)
                     ? new Int3(v.Y, v.X, v.Z)
                     : (v.Y < v.Z)
                        ? new Int3(v.Y, v.Z, v.X)
                        : new Int3(v.Z, v.Y, v.X);
            }
        }
    }
}
