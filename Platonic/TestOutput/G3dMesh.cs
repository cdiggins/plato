using System;

using System.Collections.Generic;

using Vim.LinqArray;

using Vim.Math3d;

namespace Vim.G3d
{
    // Type has fields False
    // Type has writable fields False
    // Type has public setters False
    public class G3dMesh
    {
        // A public instance method named .ctor with a type void
        // operation kind is Block and type 
        // member references = G3D, Index, Vertices, Vertices, G3D, VertexOffset, NumVertices, VertexOffset, Indices, Indices, G3D, IndexOffset, NumCorners, VertexUvs, VertexUvs, G3D, VertexOffset, NumVertices, VertexNormals, VertexNormals, G3D, VertexOffset, NumVertices, VertexColors, VertexColors, G3D, VertexOffset, NumVertices, VertexTangents, VertexTangents, G3D, VertexOffset, NumVertices, FaceNormals, FaceNormals, G3D, FaceOffset, NumFaces, Array, SubmeshIndexOffsets, G3D, IndexOffset, Length, IndexOffset, NumCorners, SubmeshMaterials, SubmeshMaterials, G3D, SubmeshIndexOffsets, SubmeshIndexOffsets, G3D, IndexOffset, MeshSubmeshOffset
        // assignments = Conversion, ConditionalAccess, ConditionalAccess, ConditionalAccess, ConditionalAccess, ConditionalAccess, ConditionalAccess, ConditionalAccess, ConditionalAccess, ConditionalAccess, Invocation
        // Written symbols are (Name=offset Kind=Local), (Name=i Kind=Parameter), (Name=submeshArray Kind=Local), (Name=submeshIndex Kind=Local), (Name=submeshCount Kind=Local), (Name=i Kind=Local), (Name=indexOffset Kind=Local), (Name=i Kind=Parameter)
        // Read symbols are (Name=this Kind=Parameter), (Name=parent Kind=Parameter), (Name=index Kind=Parameter), (Name=offset Kind=Local), (Name=i Kind=Parameter), (Name=submeshArray Kind=Local), (Name=submeshIndex Kind=Local), (Name=submeshCount Kind=Local), (Name=i Kind=Local), (Name=indexOffset Kind=Local), (Name=i Kind=Parameter)
        // Captured symbols are (Name=this Kind=Parameter), (Name=offset Kind=Local)
        // Variables declared are (Name=offset Kind=Local), (Name=i Kind=Parameter), (Name=submeshArray Kind=Local), (Name=submeshIndex Kind=Local), (Name=submeshCount Kind=Local), (Name=i Kind=Local), (Name=indexOffset Kind=Local), (Name=i Kind=Parameter)
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

        // A public instance property named G3D with a type Vim.G3d.G3D
        // No associated operation
        // No data-flow analysis could be created
        
        public G3D G3D { get; }

        // A public instance property named Index with a type int
        // No associated operation
        // No data-flow analysis could be created
                public int Index { get; }

        // A public instance property named VertexOffset with a type int
        // operation kind is PropertyReference and type int
        // member references = this[], MeshVertexOffsets, G3D, Index
        // assignments = 
        // Written symbols are 
        // Read symbols are (Name=this Kind=Parameter)
        // Captured symbols are 
        // Variables declared are 
        
        public int VertexOffset => G3D.MeshVertexOffsets[Index];

        // A public instance property named NumVertices with a type int
        // operation kind is PropertyReference and type int
        // member references = this[], MeshVertexCounts, G3D, Index
        // assignments = 
        // Written symbols are 
        // Read symbols are (Name=this Kind=Parameter)
        // Captured symbols are 
        // Variables declared are 
                public int NumVertices => G3D.MeshVertexCounts[Index];

        // A public instance property named IndexOffset with a type int
        // operation kind is PropertyReference and type int
        // member references = this[], MeshIndexOffsets, G3D, Index
        // assignments = 
        // Written symbols are 
        // Read symbols are (Name=this Kind=Parameter)
        // Captured symbols are 
        // Variables declared are 
                public int IndexOffset => G3D.MeshIndexOffsets[Index];

        // A public instance property named NumCorners with a type int
        // operation kind is PropertyReference and type int
        // member references = this[], MeshIndexCounts, G3D, Index
        // assignments = 
        // Written symbols are 
        // Read symbols are (Name=this Kind=Parameter)
        // Captured symbols are 
        // Variables declared are 
                public int NumCorners => G3D.MeshIndexCounts[Index];

        // A public instance property named FaceOffset with a type int
        // operation kind is Binary and type int
        // member references = IndexOffset, NumCornersPerFace
        // assignments = 
        // Written symbols are 
        // Read symbols are (Name=this Kind=Parameter)
        // Captured symbols are 
        // Variables declared are 
                public int FaceOffset => IndexOffset / NumCornersPerFace;

        // A public instance property named NumFaces with a type int
        // operation kind is Binary and type int
        // member references = NumCorners, NumCornersPerFace
        // assignments = 
        // Written symbols are 
        // Read symbols are (Name=this Kind=Parameter)
        // Captured symbols are 
        // Variables declared are 
                public int NumFaces => NumCorners / NumCornersPerFace;

        // A public instance property named NumCornersPerFace with a type int
        // operation kind is PropertyReference and type int
        // member references = NumCornersPerFace, G3D
        // assignments = 
        // Written symbols are 
        // Read symbols are (Name=this Kind=Parameter)
        // Captured symbols are 
        // Variables declared are 
                public int NumCornersPerFace => G3D.NumCornersPerFace;

        // A public instance property named Vertices with a type Vim.LinqArray.IArray<Vim.Math3d.Vector3>
        // No associated operation
        // No data-flow analysis could be created
        
        // Vertex buffer. Usually present.
        public IArray<Vector3> Vertices { get; }

        // A public instance property named Indices with a type Vim.LinqArray.IArray<int>
        // No associated operation
        // No data-flow analysis could be created
        
        // Index buffer (one index per corner, and per half-edge)
        public IArray<int> Indices { get; }

        // A public instance property named VertexUvs with a type Vim.LinqArray.IArray<Vim.Math3d.Vector2>
        // No associated operation
        // No data-flow analysis could be created
        
        // Vertex associated data
        public IArray<Vector2> VertexUvs { get; }

        // A public instance property named VertexNormals with a type Vim.LinqArray.IArray<Vim.Math3d.Vector3>
        // No associated operation
        // No data-flow analysis could be created
                public IArray<Vector3> VertexNormals { get; }

        // A public instance property named VertexColors with a type Vim.LinqArray.IArray<Vim.Math3d.Vector4>
        // No associated operation
        // No data-flow analysis could be created
                public IArray<Vector4> VertexColors { get; }

        // A public instance property named VertexTangents with a type Vim.LinqArray.IArray<Vim.Math3d.Vector4>
        // No associated operation
        // No data-flow analysis could be created
                public IArray<Vector4> VertexTangents { get; }

        // A public instance property named FaceNormals with a type Vim.LinqArray.IArray<Vim.Math3d.Vector3>
        // No associated operation
        // No data-flow analysis could be created
        
        // Face associated data.
        public IArray<Vector3> FaceNormals { get; }

        // A public instance property named SubmeshMaterials with a type Vim.LinqArray.IArray<int>
        // No associated operation
        // No data-flow analysis could be created
        
        public IArray<int> SubmeshMaterials { get; }

        // A public instance property named SubmeshIndexOffsets with a type Vim.LinqArray.IArray<int>
        // No associated operation
        // No data-flow analysis could be created
                public IArray<int> SubmeshIndexOffsets { get; }

        // A public instance property named MeshSubmeshOffset with a type Vim.LinqArray.IArray<int>
        // No associated operation
        // No data-flow analysis could be created
                public IArray<int> MeshSubmeshOffset { get; }

    } // type
} // namespace
