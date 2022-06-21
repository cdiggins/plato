using g3;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Vim.DotNetUtilities;
using Vim.LinqArray;
using Vim.Math3d;
    
namespace Vim.Geometry
{
    public class RayMeshIntersection
    {
        public int TriangleIndex;
        public float Distance;
    }

    public class MeshMeshIntersection
    {
        public MeshMeshIntersection(int triIndexA, int triIndexB, Vector3 a, Vector3 b)
            => (TriangleA, TriangleB, A, B, IsLine) = (triIndexA, triIndexB, a, b, true);

        public MeshMeshIntersection(int triIndexA, int triIndexB, Vector3 v)
            => (TriangleA, TriangleB, A, B, IsLine) = (triIndexA, triIndexB, v, v, false);

        public Vector3 A { get; }
        public Vector3 B { get; }
        public bool IsLine { get; }
        public int TriangleA { get; }
        public int TriangleB { get; }
    }

    public class AABBTree
    {
        public AABBTree(IMesh source)
        {
            if (source.NumCornersPerFace != 3)
                throw new Exception("Requires a triangulated mesh to work");
            Source = source;
        }

        private DMeshAABBTree3 _tree;
        public DMeshAABBTree3 Tree => _tree ?? (_tree = Mesh?.BuildDMeshAABBTree()); 
        private DMesh3 _mesh;
        public DMesh3 Mesh => _mesh ?? (_mesh = Source?.ToG3Sharp());
        public IMesh Source { get; }
        public AABox Box => Tree?.Bounds.ToMath3d() ?? AABox.Zero;

        public int NearestTriangleIndex(Vector3 v)
            => Tree?.FindNearestTriangle(v.ToG3Sharp()) ?? -1;

        public int NearestHitTriangle(Ray r)
            => Tree?.FindNearestHitTriangle(r.ToG3Sharp()) ?? -1;

        public Vector3 NearestPointOnTriangle(Vector3 v, int id)
            => MeshQueries.TriangleDistance(Mesh, id, v.ToG3Sharp()).TriangleClosest.ToMath3d();

        public Vector3 NearestPoint(Vector3 v)
            => NearestPointOnTriangle(v, NearestTriangleIndex(v));

        public IArray<int> AllHitTriangles(Ray ray)
        {
            var r = new List<int>();
            Tree?.FindAllHitTriangles(ray.ToG3Sharp(), r);
            return r.ToIArray();
        }

        public bool IsInside(Vector3 v)
            => Tree?.IsInside(v.ToG3Sharp()) ?? false;

        public bool Intersects(AABBTree tree)
            => tree == null || Tree == null ? false : Tree.TestIntersection(tree.Tree);

        public bool Intersects(AABBTree tree, Matrix4x4 selfWorldTransform, Matrix4x4 otherWorldTransform)
        {
            if (tree == null || Tree == null || tree.Tree == null)
                return false;

            var matrix = otherWorldTransform * selfWorldTransform.Inverse();
            
            Vector3d ConvertToLocalSpace(Vector3d v)
                => v.ToMath3d().Transform(matrix).ToG3Sharp();

            try
            {
                return Tree.TestIntersection(tree.Tree, ConvertToLocalSpace);
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Error occured during intersection test: {e}");
                return false;
            }
        }

        public float Distance(AABBTree tree, Matrix4x4 selfWorldTransform, Matrix4x4 otherWorldTransform, float maxDistance)
        {
            if (tree == null || Tree == null || tree.Tree == null)
                return float.MaxValue;

            var matrix = otherWorldTransform * selfWorldTransform.Inverse();
            
            Vector3d ConvertToLocalSpace(Vector3d v)
                => v.ToMath3d().Transform(matrix).ToG3Sharp();

            try
            {
                var result = Tree.FindNearestTriangles(tree.Tree, ConvertToLocalSpace, out var distance, maxDistance);
                if (result != Index2i.Max)
                {
                    return (float)distance;
                }
                else
                {
                    return float.MaxValue;
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Error occured during intersection test: {e}");
                return float.MaxValue;
            }
        }

        public IArray<MeshMeshIntersection> FindAllIntersections(AABBTree tree)
        {
            var a = Tree;
            var b = tree.Tree;
            if (a == null || b == null)
                return LinqArray.LinqArray.Empty<MeshMeshIntersection>();

            var tmp = a.FindAllIntersections(b);
            return tmp.Segments.Select(s => new MeshMeshIntersection(s.t0, s.t1, s.point0.ToMath3d(), s.point1.ToMath3d()))
                .Concat(tmp.Points.Select(p => new MeshMeshIntersection(p.t0, p.t1, p.point.ToMath3d())))
                .ToIArray();
        }

        public IArray<MeshMeshIntersection> FindAllSegmentIntersections(AABBTree tree)
        {
            var a = Tree;
            var b = tree.Tree;
            if (a == null || b == null)
                return LinqArray.LinqArray.Empty<MeshMeshIntersection>();
            var tmp = a.FindAllIntersections(b);
            return tmp.Segments.Select(s => new MeshMeshIntersection(s.t0, s.t1, s.point0.ToMath3d(), s.point1.ToMath3d()))
                .ToIArray();
        }

        public IArray<MeshMeshIntersection> FindAllPointIntersections(AABBTree tree)
        {
            var a = Tree;
            var b = tree.Tree;
            if (a == null || b == null)
                return LinqArray.LinqArray.Empty<MeshMeshIntersection>();
            var tmp = a.FindAllIntersections(b);
            return tmp.Points.Select(p => new MeshMeshIntersection(p.t0, p.t1, p.point.ToMath3d()))
                .ToIArray();
        }

        public IEnumerable<RayMeshIntersection> FindAllIntersections(Ray ray)
            => AllHitTriangles(ray)
                .Select(i => new RayMeshIntersection
                {
                    TriangleIndex = i,
                    Distance = ray.Intersects(Source.Triangle(i)) ?? -1
                })
            .Where(x => x.Distance >= 0);

        public RayMeshIntersection FindClosestsIntersections(Ray ray)
            => FindAllIntersections(ray).Maximize((i1, i2) => i1.Distance < i2.Distance);
    }
}
