using System.Collections.Generic;
using System.Threading.Tasks;
using Vim.Math3d;
using Vim.LinqArray;
using Vim.G3d;
using System;
using System.Diagnostics;

namespace Vim.Geometry
{
    public static class G3dSharpGeometryExtensions
    {
        /// <summary>
        /// Returns an array of boolean values the length of the number of faces.
        /// Each array indicates whether a ray drawn from one of the origins can hit a face. 
        /// </summary>
        public static bool[] RayIntersects(this IMesh g, AABBTree tree, IEnumerable<Vector3> origins)
        {
            // Shoot rays at it from one side
            var hit = new bool[g.NumFaces];

            Parallel.For(0, g.NumFaces, i =>
            {
                // We don't need to continue, if we already determined the value is true. 
                if (!hit[i])
                {
                    var v = g.Triangle(i).MidPoint;
                    foreach (var p in origins)
                    {
                        var r = new Ray(p, v - p);
                        var id = tree.NearestHitTriangle(r);
                        if (id >= 0)
                            hit[id] = true;
                    }
                }
            });

            return hit;
        }

        /*
        public static bool[] ShootRaysAllSides(this IGeometry g, AABBTree tree)
            => RayIntersects(g, tree, g.BoundingBox().Scale(2.0f).GetCornersAndFaceCenters());
            */

        public static bool[] ShootRaysAllSides(this IMesh g, AABBTree tree)
            => RayIntersects(g, tree, g.BoundingBox().Scale(2.0f).FaceCenters());

        public static IMesh GetExterior(this IMesh g)
        {
            var tree = new AABBTree(g);
            var keep = ShootRaysAllSides(g, tree);
            return g.CopyFaces(keep.ToIArray().ToPredicate()).ToIMesh();
        }

        public static AABBTree ToAABBTree(this IMesh m)
        {
            if (m == null)
                return null;
            try
            {
                return new AABBTree(m);
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Failed to create AABBTree {e.Message}");
                return null;
            }
        }
    }
}
