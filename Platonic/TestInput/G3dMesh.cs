using System;
using System.Collections.Generic;
using Vim.LinqArray;
using Vim.Math3d;

namespace Vim.G3d
{
    /// <summary>
    /// A G3dMesh is a section of the G3D data that defines a mesh.
    /// This does not implement IGeometryAttributes for performance reasons. 
    /// </summary>
    public class G3dMesh
    {
        public G3dMesh(G3D parent, int index)
        {
            (G3D, Index) = (parent, index);
            Vertices = G3D.Vertices?.SubArray(VertexOffset, NumVertices);
            var offset = VertexOffset;
            Indices = G3D.Indices?.SubArray(IndexOffset, NumCorners).Select(i => i - offset);
            VertexUvs = G3D.VertexUvs?.SubArray(VertexOffset, NumVertices);
            VertexNormals = G3D.VertexNormals?.SubArray(VertexOffset, NumVertices);
            VertexColors = G3D.VertexColors?.SubArray(VertexOffset, NumVertices);
            VertexTangents = G3D.VertexTangents?.SubArray(VertexOffset, NumVertices);
            FaceNormals = G3D.FaceNormals?.SubArray(FaceOffset, NumFaces);

            // TODO: Remove need for this.
            var submeshArray = (G3D.SubmeshIndexOffsets as ArrayAdapter<int>).Array;
            var submeshIndex = Array.BinarySearch(submeshArray, IndexOffset);
            var submeshCount = 0;
            for (var i = submeshIndex; i < submeshArray.Length; i++)
            {
                var indexOffset = submeshArray[i];
                if (indexOffset - IndexOffset >= NumCorners)
                    break;
                submeshCount++;
            }

            SubmeshMaterials = G3D.SubmeshMaterials?.SubArray(submeshIndex, submeshCount);
            SubmeshIndexOffsets = G3D.SubmeshIndexOffsets?.SubArray(submeshIndex, submeshCount)
                .Select(i => i - IndexOffset);
            MeshSubmeshOffset = new List<int> { 0 }.ToIArray();
        }

        public G3D G3D { get; }
        public int Index { get; }

        public int VertexOffset => G3D.MeshVertexOffsets[Index];
        public int NumVertices => G3D.MeshVertexCounts[Index];
        public int IndexOffset => G3D.MeshIndexOffsets[Index];
        public int NumCorners => G3D.MeshIndexCounts[Index];
        public int FaceOffset => IndexOffset / NumCornersPerFace;
        public int NumFaces => NumCorners / NumCornersPerFace;
        public int NumCornersPerFace => G3D.NumCornersPerFace;

        // Vertex buffer. Usually present.
        public IArray<Vector3> Vertices { get; }

        // Index buffer (one index per corner, and per half-edge)
        public IArray<int> Indices { get; }

        // Vertex associated data
        public IArray<Vector2> VertexUvs { get; }
        public IArray<Vector3> VertexNormals { get; }
        public IArray<Vector4> VertexColors { get; }
        public IArray<Vector4> VertexTangents { get; }

        // Face associated data.
        public IArray<Vector3> FaceNormals { get; }

        public IArray<int> SubmeshMaterials { get; }
        public IArray<int> SubmeshIndexOffsets { get; }
        public IArray<int> MeshSubmeshOffset { get; }
    }
}