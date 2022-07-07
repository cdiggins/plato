using System;
using System.Linq;
using Plato.Geometry;
using UnityEngine;

namespace Plato.Unity
{
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    [DisallowMultipleComponent]
    [ExecuteInEditMode]
    public abstract class ProceduralMesh : MonoBehaviour
    {
        public abstract IMesh GetMesh();

        public virtual void Update()
        {
            var filter = GetComponent<MeshFilter>();
            if (filter == null)
                return;

            var mesh = GetMesh();
            var result = EvaluateAllModifiers(mesh);

            // At edit time we need to use the sharedMesh
            if (filter.sharedMesh == null)
                filter.sharedMesh = new UnityEngine.Mesh();
            result.ToUnity(filter.sharedMesh);

            //filter.mesh.RecalculateNormals();
            //filter.mesh.RecalculateTangents();
            filter.sharedMesh.RecalculateBounds();
        }

        public IMesh EvaluateAllModifiers(IMesh mesh)
            => GetComponents<Modifier>()
                .Where(mod => mod != null && mod.isActiveAndEnabled && mod.Function != null)
                .Aggregate(mesh, (mesh2, mod) => mod.Function.Invoke(mesh2));
    }

    public class PlaneMesh : ProceduralMesh
    {
        public int x_segments = 3;
        public int y_segments = 3;

        public override IMesh GetMesh()
            => Primitives.Plane(x_segments, y_segments).ToIMesh();
    }
}