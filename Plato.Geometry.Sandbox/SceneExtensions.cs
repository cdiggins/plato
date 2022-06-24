using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Vim.DotNetUtilities;
using Vim.LinqArray;
using Vim.Math3d;

namespace Vim.Geometry
{
    public static class SceneExtensions
    {
        public static IMesh TransformedMesh(this ISceneNode node)
            => node.GetMesh()?.Transform(node.Transform);

        public static IEnumerable<IMesh> TransformedMeshes(this IScene scene)
            => scene.Nodes.Where(n => n.GetMesh() != null).Select(TransformedMesh);

        public static IMesh ToIMesh(this IScene scene)
            => scene.TransformedMeshes().Merge();

        public static bool HasLoop(this ISceneNode n)
        {
            if (n == null) return false;
            var visited = new HashSet<ISceneNode>();
            for (; n != null; n = n.Parent)
            {
                if (visited.Contains(n))
                    return true;
                visited.Add(n);
            }

            return false;
        }

        public static IMesh MergedGeometry(this IScene scene)
            => scene.Nodes.ToEnumerable().MergedGeometry();

        public static IMesh MergedGeometry(this IEnumerable<ISceneNode> nodes)
            => nodes.Where(n => n.GetMesh() != null).Select(TransformedMesh).Merge();

        public static Matrix4x4 LocalTransform(this ISceneNode node)
            => node.Parent != null
                ? node.Transform * node.Parent.Transform.Inverse()
                : node.Transform;

        public static IEnumerable<IMesh> AllDistinctMeshes(this IScene scene)
            => scene.UntransformedMeshes().Where(x => x != null).Distinct();

        public static IndexedSet<IMesh> GeometryLookup(this IScene scene)
            => scene.Meshes.ToEnumerable().ToIndexedSet();

        public static IArray<IMesh> UntransformedMeshes(this IScene scene)
            => scene.Nodes.Select(n => n.GetMesh());

        public static bool HasGeometry(this IScene scene)
            => scene.Nodes.Any(n => n.GetMesh() != null);

        public static Dictionary<IMesh, int> GeometryCounts(this IScene scene)
            => scene.Nodes.ToEnumerable().CountInstances(x => x.GetMesh());

        public static IEnumerable<Vector3> AllVertices(this IScene scene)
            => scene.TransformedMeshes().SelectMany(g => g.Vertices.ToEnumerable());

        public static AABox BoundingBox(this IScene scene)
            => AABox.Create(scene.AllVertices());

        public static IArray<Matrix4x4> Transforms(this IScene scene)
            => scene.Nodes.Select(n => n.Transform);

        public static IArray<Vector3> NodePositions(this IScene scene)
            => scene.Transforms().Select(m => m.Translation);

        public static AABox NodePositionBoundingBox(this IScene scene)
            => AABox.Create(scene.NodePositions().ToEnumerable());

        public static Sphere BoundingSphere(this IScene scene)
            => scene.BoundingBox().ToSphere();

        public static float BoundingRadius(this IScene scene)
            => scene.BoundingSphere().Radius;

        public static IArray<Vector3> TransformedVertices(this ISceneNode node)
            => node.TransformedMesh()?.Vertices;

        public static AABox TransformedBoundingBox(this ISceneNode node)
            => AABox.Create(node.TransformedVertices()?.ToEnumerable());
    }
}
