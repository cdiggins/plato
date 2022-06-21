using Vim.LinqArray;
using Vim.Math3d;

namespace Vim.Geometry
{
    public class MeshDebugView
    {
        IMesh Interface { get; }

        public int NumCorners => Interface.NumCorners;
        public int NumFaces => Interface.NumFaces;
        public int NumMeshes => Interface.NumMeshes;
        public int NumInstances => Interface.NumInstances;

        public Vector3[] Vertices => Interface.Vertices.ToArray();
        public int[] Indices => Interface.Indices.ToArray();

        public MeshDebugView(IMesh g)
            => Interface = g;
    }
}
