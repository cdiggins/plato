/*
    G3D Geometry Format Library
    Copyright 2019, VIMaec LLC.
    Copyright 2018, Ara 3D Inc.
    Usage licensed under terms of MIT License
*/

using System;

using System.Collections.Generic;

using System.IO;

using System.Linq;

using Vim.LinqArray;

using Vim.Math3d;

namespace Vim.G3d
{
    // Type has fields True
    // Type has writable fields True
    // Type has public setters False
    public class G3D
    : GeometryAttributes

    {
        // A public instance method named .ctor with a type void
        // operation kind is Block and type 
        // member references = Header, Attributes, Descriptor, Semantic, Index, assoc_corner, Indices, Indices, Data, assoc_corner, Indices, Indices, Data, Position, assoc_vertex, Vertices, Vertices, Data, assoc_corner, Vertices, Vertices, Data, assoc_shapevertex, ShapeVertices, ShapeVertices, Data, Tangent, assoc_vertex, VertexTangents, VertexTangents, Data, X, Y, Z, assoc_vertex, VertexTangents, VertexTangents, Data, Uv, assoc_vertex, AllVertexUvs, Data, X, Y, assoc_vertex, AllVertexUvs, Data, Color, Association, assoc_vertex, AllVertexColors, Association, assoc_shape, ShapeColors, ShapeColors, Association, assoc_material, MaterialColors, MaterialColors, VertexOffset, assoc_mesh, MeshVertexOffsets, MeshVertexOffsets, Data, assoc_shape, ShapeVertexOffsets, ShapeVertexOffsets, Data, IndexOffset, assoc_mesh, MeshIndexOffsets, MeshIndexOffsets, Data, assoc_submesh, SubmeshIndexOffsets, SubmeshIndexOffsets, Data, Normal, assoc_face, FaceNormals, FaceNormals, Data, assoc_vertex, VertexNormals, VertexNormals, Data, Material, assoc_face, FaceMaterials, FaceMaterials, Data, assoc_submesh, SubmeshMaterials, SubmeshMaterials, Data, Parent, assoc_instance, InstanceParents, InstanceParents, Data, Mesh, assoc_instance, InstanceMeshes, InstanceMeshes, Data, Transform, assoc_instance, InstanceTransforms, InstanceTransforms, Data, Width, assoc_shape, ShapeWidths, ShapeWidths, Data, Glossiness, assoc_material, MaterialGlossiness, Data, Smoothness, assoc_material, MaterialSmoothness, Data, SubMeshOffset, assoc_mesh, MeshSubmeshOffset, Data, Flags, assoc_instance, InstanceFlags, Data, Vertices, Vertices, Indices, Indices, Vertices, FaceNormals, VertexNormals, FaceNormals, NumFaces, ComputeFaceNormal, NumMeshes, MeshSubmeshOffset, MeshIndexOffsets, MeshSubmeshOffset, this[], SubmeshIndexOffsets, MeshSubmeshCount, Count, MeshSubmeshOffset, MeshSubmeshOffset, NumSubmeshes, MeshIndexOffsets, MeshIndexCounts, NumMeshes, MeshIndexOffsets, NumCorners, MeshVertexOffsets, MeshIndexOffsets, MeshIndexCounts, Indices, start, count, MeshVertexOffsets, MeshVertexCounts, NumMeshes, MeshVertexOffsets, NumVertices, MeshSubmeshCount, SubmeshIndexOffsets, SubmeshIndexCount, Count, SubmeshIndexOffsets, SubmeshIndexOffsets, NumCorners, Meshes, NumMeshes, MaterialColors, Materials, Count, MaterialColors, ShapeVertices, ShapeVertices, ShapeVertexOffsets, ShapeVertexOffsets, ShapeColors, ShapeColors, ShapeWidths, ShapeWidths, InstanceFlags, InstanceFlags, NumInstances, ShapeVertexCounts, NumShapes, ShapeVertexOffsets, Count, ShapeVertices, ShapeVertexCounts, ShapeVertexCounts, Shapes, NumShapes
        // assignments = Coalesce, Coalesce, Coalesce, Coalesce, Coalesce, Coalesce, Coalesce, Coalesce, Coalesce, Coalesce, Coalesce, Coalesce, Coalesce, Coalesce, Coalesce, Coalesce, Coalesce, Coalesce, Coalesce, Coalesce, Coalesce, Coalesce, FieldReference, FieldReference, FieldReference, FieldReference, Invocation, Invocation, Invocation, Invocation, Invocation, Invocation, Invocation, Invocation, Invocation, Invocation, Invocation, Invocation, Invocation, Invocation, Invocation, Invocation, Invocation, Invocation, Invocation
        // Written symbols are (Name=attr Kind=Local), (Name=desc Kind=Local), (Name=x Kind=Parameter), (Name=v Kind=Parameter), (Name=uv Kind=Parameter), (Name=submesh Kind=Parameter), (Name=start Kind=Parameter), (Name=count Kind=Parameter), (Name=range Kind=Parameter), (Name=i Kind=Parameter), (Name=i Kind=Parameter), (Name=i Kind=Parameter)
        // Read symbols are (Name=this Kind=Parameter), (Name=header Kind=Parameter), (Name=attr Kind=Local), (Name=desc Kind=Local), (Name=x Kind=Parameter), (Name=v Kind=Parameter), (Name=uv Kind=Parameter), (Name=submesh Kind=Parameter), (Name=start Kind=Parameter), (Name=count Kind=Parameter), (Name=range Kind=Parameter), (Name=i Kind=Parameter), (Name=i Kind=Parameter), (Name=i Kind=Parameter)
        // Captured symbols are (Name=this Kind=Parameter)
        // Variables declared are (Name=attr Kind=Local), (Name=desc Kind=Local), (Name=x Kind=Parameter), (Name=v Kind=Parameter), (Name=uv Kind=Parameter), (Name=submesh Kind=Parameter), (Name=start Kind=Parameter), (Name=count Kind=Parameter), (Name=range Kind=Parameter), (Name=i Kind=Parameter), (Name=i Kind=Parameter), (Name=i Kind=Parameter)
        
        public G3D(IEnumerable<GeometryAttribute> attributes, G3dHeader? header = null,
            int numCornersPerFaceOverride = -1)
            : base(attributes, numCornersPerFaceOverride)
        {
            Header = header ?? new G3dHeader();

            foreach (var attr in Attributes.ToEnumerable())
            {
                var desc = attr.Descriptor;
                switch (desc.Semantic)
                {
                    case Semantic.Index:
                        if (attr.IsTypeAndAssociation<int>(Association.assoc_corner))
                            Indices = Indices ?? attr.AsType<int>().Data;
                        if (attr.IsTypeAndAssociation<short>(Association.assoc_corner))
                            Indices = Indices ?? attr.AsType<short>().Data.Select(x => (int)x);
                        break;

                    case Semantic.Position:
                        if (attr.IsTypeAndAssociation<Vector3>(Association.assoc_vertex))
                            Vertices = Vertices ?? attr.AsType<Vector3>().Data;
                        if (attr.IsTypeAndAssociation<Vector3>(Association.assoc_corner))
                            Vertices = Vertices ?? attr.AsType<Vector3>().Data; // TODO: is this used?
                        if (attr.IsTypeAndAssociation<Vector3>(Association.assoc_shapevertex))
                            ShapeVertices = ShapeVertices ?? attr.AsType<Vector3>().Data;
                        break;

                    case Semantic.Tangent:
                        if (attr.IsTypeAndAssociation<Vector3>(Association.assoc_vertex))
                            VertexTangents = VertexTangents ?? attr.AsType<Vector3>().Data.Select(v => new Vector4(v.X, v.Y, v.Z, 0));
                        if (attr.IsTypeAndAssociation<Vector4>(Association.assoc_vertex))
                            VertexTangents = VertexTangents ?? attr.AsType<Vector4>().Data;
                        break;

                    case Semantic.Uv:
                        if (attr.IsTypeAndAssociation<Vector3>(Association.assoc_vertex))
                            AllVertexUvs.Add(attr.AsType<Vector3>().Data.Select(uv => new Vector2(uv.X, uv.Y)));
                        if (attr.IsTypeAndAssociation<Vector2>(Association.assoc_vertex))
                            AllVertexUvs.Add(attr.AsType<Vector2>().Data);
                        break;

                    case Semantic.Color:
                        if (desc.Association == Association.assoc_vertex)
                            AllVertexColors.Add(attr.AttributeToColors());
                        if (desc.Association == Association.assoc_shape)
                            ShapeColors = ShapeColors ?? attr.AttributeToColors();
                        if (desc.Association == Association.assoc_material)
                            MaterialColors = MaterialColors ?? attr.AttributeToColors();
                        break;

                    case Semantic.VertexOffset:
                        if (attr.IsTypeAndAssociation<int>(Association.assoc_mesh))
                            MeshVertexOffsets = MeshVertexOffsets ?? attr.AsType<int>().Data;
                        if (attr.IsTypeAndAssociation<int>(Association.assoc_shape))
                            ShapeVertexOffsets = ShapeVertexOffsets ?? attr.AsType<int>().Data;
                        break;

                    case Semantic.IndexOffset:
                        if (attr.IsTypeAndAssociation<int>(Association.assoc_mesh))
                            MeshIndexOffsets = MeshIndexOffsets ?? attr.AsType<int>().Data;
                        if (attr.IsTypeAndAssociation<int>(Association.assoc_submesh))
                            SubmeshIndexOffsets = SubmeshIndexOffsets ?? attr.AsType<int>().Data;
                        break;

                    case Semantic.Normal:
                        if (attr.IsTypeAndAssociation<Vector3>(Association.assoc_face))
                            FaceNormals = FaceNormals ?? attr.AsType<Vector3>().Data;
                        if (attr.IsTypeAndAssociation<Vector3>(Association.assoc_vertex))
                            VertexNormals = VertexNormals ?? attr.AsType<Vector3>().Data;
                        break;

                    case Semantic.Material:
                        if (attr.IsTypeAndAssociation<int>(Association.assoc_face))
                            FaceMaterials = FaceMaterials ?? attr.AsType<int>().Data;
                        if (attr.IsTypeAndAssociation<int>(Association.assoc_submesh))
                            SubmeshMaterials = SubmeshMaterials ?? attr.AsType<int>().Data;
                        break;

                    case Semantic.Parent:
                        if (attr.IsTypeAndAssociation<int>(Association.assoc_instance))
                            InstanceParents = InstanceParents ?? attr.AsType<int>().Data;
                        break;

                    case Semantic.Mesh:
                        if (attr.IsTypeAndAssociation<int>(Association.assoc_instance))
                            InstanceMeshes = InstanceMeshes ?? attr.AsType<int>().Data;
                        break;

                    case Semantic.Transform:
                        if (attr.IsTypeAndAssociation<Matrix4x4>(Association.assoc_instance))
                            InstanceTransforms = InstanceTransforms ?? attr.AsType<Matrix4x4>().Data;
                        break;

                    case Semantic.Width:
                        if (attr.IsTypeAndAssociation<float>(Association.assoc_shape))
                            ShapeWidths = ShapeWidths ?? attr.AsType<float>().Data;
                        break;

                    case Semantic.Glossiness:
                        if (attr.IsTypeAndAssociation<float>(Association.assoc_material))
                            MaterialGlossiness = attr.AsType<float>().Data;
                        break;

                    case Semantic.Smoothness:
                        if (attr.IsTypeAndAssociation<float>(Association.assoc_material))
                            MaterialSmoothness = attr.AsType<float>().Data;
                        break;

                    case Semantic.SubMeshOffset:
                        if (attr.IsTypeAndAssociation<int>(Association.assoc_mesh))
                            MeshSubmeshOffset = attr.AsType<int>().Data;
                        break;

                    case Semantic.Flags:
                        if (attr.IsTypeAndAssociation<ushort>(Association.assoc_instance))
                            InstanceFlags = attr.AsType<ushort>().Data;
                        break;
                }
            }

            // If no vertices are provided, we are going to generate a list of zero vertices.
            if (Vertices == null)
                Vertices = (new Vector3(0,0,0)).Repeat(0);

            // If no indices are provided then we are going to have to treat the index buffer as indices
            if (Indices == null)
                Indices = Vertices.Indices();

            // Compute face normals if possible
            if (FaceNormals == null && VertexNormals != null)
                FaceNormals = NumFaces.Select(ComputeFaceNormal);

            if (NumMeshes > 0)
            {
                // Mesh offset is the same as the offset of its first submesh.
                if (MeshSubmeshOffset != null)
                {
                    MeshIndexOffsets = MeshSubmeshOffset.Select(submesh => SubmeshIndexOffsets[submesh]);
                    MeshSubmeshCount = GetSubArrayCounts(MeshSubmeshOffset.Count, MeshSubmeshOffset, NumSubmeshes)
                        .Evaluate();
                }

                if (MeshIndexOffsets != null)
                {
                    MeshIndexCounts = GetSubArrayCounts(NumMeshes, MeshIndexOffsets, NumCorners);
                    MeshVertexOffsets = MeshIndexOffsets
                        .Zip(MeshIndexCounts, (start, count) => (start, count))
                        .Select(range => Indices.SubArray(range.start, range.count).Min());
                }

                if (MeshVertexOffsets != null)
                    MeshVertexCounts = GetSubArrayCounts(NumMeshes, MeshVertexOffsets, NumVertices);
            }
            else
            {
                MeshSubmeshCount = Array.Empty<int>().ToIArray();
            }

            if (SubmeshIndexOffsets != null)
                SubmeshIndexCount = GetSubArrayCounts(SubmeshIndexOffsets.Count, SubmeshIndexOffsets, NumCorners)
                    .Evaluate();

            // Compute all meshes
            Meshes = NumMeshes.Select(i => new G3dMesh(this, i));

            if (MaterialColors != null)
                Materials = MaterialColors.Count.Select(i => new G3dMaterial(this, i));

            // Process the shape data
            if (ShapeVertices == null)
                ShapeVertices = (new Vector3(0,0,0)).Repeat(0);

            if (ShapeVertexOffsets == null)
                ShapeVertexOffsets = Array.Empty<int>().ToIArray();

            if (ShapeColors == null)
                ShapeColors = (new Vector4(0,0,0,0)).Repeat(0);

            if (ShapeWidths == null)
                ShapeWidths = Array.Empty<float>().ToIArray();

            // Update the instance options
            if (InstanceFlags == null)
                InstanceFlags = ((ushort)0).Repeat(NumInstances);

            ShapeVertexCounts = GetSubArrayCounts(NumShapes, ShapeVertexOffsets, ShapeVertices.Count);
            ValidateSubArrayCounts(ShapeVertexCounts, nameof(ShapeVertexCounts));

            Shapes = NumShapes.Select(i => new G3dShape(this, i));
        }

        // A private static method named GetSubArrayCounts with a type Vim.LinqArray.IArray<int>
        // operation kind is Block and type 
        // member references = this[], this[], this[]
        // assignments = 
        // Written symbols are (Name=i Kind=Parameter)
        // Read symbols are (Name=numItems Kind=Parameter), (Name=offsets Kind=Parameter), (Name=totalCount Kind=Parameter), (Name=i Kind=Parameter)
        // Captured symbols are (Name=numItems Kind=Parameter), (Name=offsets Kind=Parameter), (Name=totalCount Kind=Parameter)
        // Variables declared are (Name=i Kind=Parameter)
        
        private static IArray<int> GetSubArrayCounts(int numItems, IArray<int> offsets, int totalCount)
        {
            return numItems.Select(i => i < numItems - 1
                ? offsets[i + 1] - offsets[i]
                : totalCount - offsets[i]);
        }

        // A private static method named ValidateSubArrayCounts with a type void
        // operation kind is Block and type 
        // member references = Count, this[]
        // assignments = 
        // Written symbols are (Name=i Kind=Local)
        // Read symbols are (Name=subArrayCounts Kind=Parameter), (Name=memberName Kind=Parameter), (Name=i Kind=Local)
        // Captured symbols are 
        // Variables declared are (Name=i Kind=Local)
        
        private static void ValidateSubArrayCounts(IArray<int> subArrayCounts, string memberName)
        {
            for (var i = 0; i < subArrayCounts.Count; ++i)
                if (subArrayCounts[i] < 0)
                    throw new Exception($"{memberName}[{i}] is a negative sub array count.");
        }

        // A public static method named Average with a type Vim.Math3d.Vector3
        // operation kind is Block and type 
        // member references = X, X, Y, Y, Z, Z, X, Count, Y, Count, Z, Count
        // assignments = 
        // Written symbols are (Name=sum Kind=Local), (Name=a Kind=Parameter), (Name=b Kind=Parameter)
        // Read symbols are (Name=xs Kind=Parameter), (Name=sum Kind=Local), (Name=a Kind=Parameter), (Name=b Kind=Parameter)
        // Captured symbols are 
        // Variables declared are (Name=sum Kind=Local), (Name=a Kind=Parameter), (Name=b Kind=Parameter)
        
        public static Vector3 Average(IArray<Vector3> xs)
        {
            var sum = xs.Aggregate(new Vector3(0,0,0), (a, b) => new Vector3(a.X + b.X, a.Y + b.Y, a.Z + b.Z));
            return new Vector3(sum.X / xs.Count, sum.Y / xs.Count, sum.Z / xs.Count);
        }

        // A public instance method named ComputeFaceNormal with a type Vim.Math3d.Vector3
        // operation kind is Block and type 
        // member references = NumCornersPerFace, this[], VertexNormals, NumCornersPerFace
        // assignments = 
        // Written symbols are (Name=c Kind=Parameter)
        // Read symbols are (Name=this Kind=Parameter), (Name=nFace Kind=Parameter), (Name=c Kind=Parameter)
        // Captured symbols are (Name=this Kind=Parameter), (Name=nFace Kind=Parameter)
        // Variables declared are (Name=c Kind=Parameter)
        
        public Vector3 ComputeFaceNormal(int nFace)
        {
            return Average(NumCornersPerFace.Select(c => VertexNormals[nFace * NumCornersPerFace + c]));
        }

        // A public static method named Read with a type Vim.G3d.G3D
        // operation kind is Block and type 
        // member references = 
        // assignments = 
        // Written symbols are (Name=stream Kind=Local)
        // Read symbols are (Name=filePath Kind=Parameter), (Name=stream Kind=Local)
        // Captured symbols are 
        // Variables declared are (Name=stream Kind=Local)
        
        public static G3D Read(string filePath)
        {
            using (var stream = File.OpenRead(filePath))
            {
                return stream.ReadG3d();
            }
        }

        // A public static method named Read with a type Vim.G3d.G3D
        // operation kind is Block and type 
        // member references = 
        // assignments = 
        // Written symbols are 
        // Read symbols are (Name=stream Kind=Parameter)
        // Captured symbols are 
        // Variables declared are 
        
        public static G3D Read(Stream stream)
        {
            return stream.ReadG3d();
        }

        // A public static method named Read with a type Vim.G3d.G3D
        // operation kind is Block and type 
        // member references = 
        // assignments = 
        // Written symbols are (Name=stream Kind=Local)
        // Read symbols are (Name=bytes Kind=Parameter), (Name=stream Kind=Local)
        // Captured symbols are 
        // Variables declared are (Name=stream Kind=Local)
        
        public static G3D Read(byte[] bytes)
        {
            using (var stream = new MemoryStream(bytes))
            {
                return stream.ReadG3d();
            }
        }

        // A public static method named Create with a type Vim.G3d.G3D
        // operation kind is Block and type 
        // member references = 
        // assignments = 
        // Written symbols are 
        // Read symbols are (Name=attributes Kind=Parameter)
        // Captured symbols are 
        // Variables declared are 
        
        public static G3D Create(params GeometryAttribute[] attributes)
        {
            return new G3D(attributes);
        }

        // A public static method named Create with a type Vim.G3d.G3D
        // operation kind is Block and type 
        // member references = 
        // assignments = 
        // Written symbols are 
        // Read symbols are (Name=header Kind=Parameter), (Name=attributes Kind=Parameter)
        // Captured symbols are 
        // Variables declared are 
        
        public static G3D Create(G3dHeader header, params GeometryAttribute[] attributes)
        {
            return new G3D(attributes, header);
        }

        // A public static field named Empty with a type Vim.G3d.G3D
        // No associated operation
        // No data-flow analysis could be created
                public new static G3D Empty = Create();

        // A public instance property named Header with a type Vim.G3d.G3dHeader
        // No associated operation
        // No data-flow analysis could be created
        
        public G3dHeader Header { get; }

        // A public instance property named Vertices with a type Vim.LinqArray.IArray<Vim.Math3d.Vector3>
        // No associated operation
        // No data-flow analysis could be created
        
        // These are the values of the most common attributes. Some are retrieved directly from data, others are computed on demand, or coerced. 

        // Vertex buffer. Usually present.
        public IArray<Vector3> Vertices { get; }

        // A public instance property named Indices with a type Vim.LinqArray.IArray<int>
        // No associated operation
        // No data-flow analysis could be created
        
        // Index buffer (one index per corner, and per half-edge). Computed if absent. 
        public IArray<int> Indices { get; }

        // A public instance property named AllVertexUvs with a type System.Collections.Generic.List<Vim.LinqArray.IArray<Vim.Math3d.Vector2>>
        // No associated operation
        // No data-flow analysis could be created
        
        // Vertex associated data, provided or null
        public List<IArray<Vector2>> AllVertexUvs { get; } = new List<IArray<Vector2>>();

        // A public instance property named AllVertexColors with a type System.Collections.Generic.List<Vim.LinqArray.IArray<Vim.Math3d.Vector4>>
        // No associated operation
        // No data-flow analysis could be created
                public List<IArray<Vector4>> AllVertexColors { get; } = new List<IArray<Vector4>>();

        // A public instance property named VertexUvs with a type Vim.LinqArray.IArray<Vim.Math3d.Vector2>
        // operation kind is ConditionalAccess and type Vim.LinqArray.IArray<Vim.Math3d.Vector2>
        // member references = AllVertexUvs
        // assignments = 
        // Written symbols are 
        // Read symbols are (Name=this Kind=Parameter)
        // Captured symbols are 
        // Variables declared are 
                public IArray<Vector2> VertexUvs => AllVertexUvs?.ElementAtOrDefault(0);

        // A public instance property named VertexColors with a type Vim.LinqArray.IArray<Vim.Math3d.Vector4>
        // operation kind is ConditionalAccess and type Vim.LinqArray.IArray<Vim.Math3d.Vector4>
        // member references = AllVertexColors
        // assignments = 
        // Written symbols are 
        // Read symbols are (Name=this Kind=Parameter)
        // Captured symbols are 
        // Variables declared are 
                public IArray<Vector4> VertexColors => AllVertexColors?.ElementAtOrDefault(0);

        // A public instance property named VertexNormals with a type Vim.LinqArray.IArray<Vim.Math3d.Vector3>
        // No associated operation
        // No data-flow analysis could be created
                public IArray<Vector3> VertexNormals { get; }

        // A public instance property named VertexTangents with a type Vim.LinqArray.IArray<Vim.Math3d.Vector4>
        // No associated operation
        // No data-flow analysis could be created
                public IArray<Vector4> VertexTangents { get; }

        // A public instance property named FaceMaterials with a type Vim.LinqArray.IArray<int>
        // No associated operation
        // No data-flow analysis could be created
        
        // Faces
        public IArray<int> FaceMaterials { get; } // Material indices per face, 

        // A public instance property named FaceNormals with a type Vim.LinqArray.IArray<Vim.Math3d.Vector3>
        // No associated operation
        // No data-flow analysis could be created
        
        public IArray<Vector3>
            FaceNormals { get; } // If not provided, are computed dynamically as the average of all vertex normals,

        // A public instance property named MeshIndexOffsets with a type Vim.LinqArray.IArray<int>
        // No associated operation
        // No data-flow analysis could be created
        
        // Meshes
        public IArray<int> MeshIndexOffsets { get; } // Offset into the index buffer for each Mesh

        // A public instance property named MeshVertexOffsets with a type Vim.LinqArray.IArray<int>
        // No associated operation
        // No data-flow analysis could be created
                public IArray<int> MeshVertexOffsets { get; } // Offset into the vertex buffer for each Mesh

        // A public instance property named MeshIndexCounts with a type Vim.LinqArray.IArray<int>
        // No associated operation
        // No data-flow analysis could be created
                public IArray<int> MeshIndexCounts { get; } // Computed

        // A public instance property named MeshVertexCounts with a type Vim.LinqArray.IArray<int>
        // No associated operation
        // No data-flow analysis could be created
                public IArray<int> MeshVertexCounts { get; } // Computed

        // A public instance property named MeshSubmeshOffset with a type Vim.LinqArray.IArray<int>
        // No associated operation
        // No data-flow analysis could be created
                public IArray<int> MeshSubmeshOffset { get; }

        // A public instance property named MeshSubmeshCount with a type Vim.LinqArray.IArray<int>
        // No associated operation
        // No data-flow analysis could be created
                public IArray<int> MeshSubmeshCount { get; } // Computed

        // A public instance property named Meshes with a type Vim.LinqArray.IArray<Vim.G3d.G3dMesh>
        // No associated operation
        // No data-flow analysis could be created
                public IArray<G3dMesh> Meshes { get; }

        // A public instance property named InstanceParents with a type Vim.LinqArray.IArray<int>
        // No associated operation
        // No data-flow analysis could be created
        
        // Instances
        public IArray<int> InstanceParents { get; } // Index of the parent transform 

        // A public instance property named InstanceTransforms with a type Vim.LinqArray.IArray<Vim.Math3d.Matrix4x4>
        // No associated operation
        // No data-flow analysis could be created
                public IArray<Matrix4x4> InstanceTransforms { get; } // A 4x4 matrix in row-column order defining the transormed

        // A public instance property named InstanceMeshes with a type Vim.LinqArray.IArray<int>
        // No associated operation
        // No data-flow analysis could be created
                public IArray<int> InstanceMeshes { get; } // The SubGeometry associated with the index

        // A public instance property named InstanceFlags with a type Vim.LinqArray.IArray<ushort>
        // No associated operation
        // No data-flow analysis could be created
                public IArray<ushort> InstanceFlags { get; } // The instance flags associated with the index.

        // A public instance property named ShapeVertices with a type Vim.LinqArray.IArray<Vim.Math3d.Vector3>
        // No associated operation
        // No data-flow analysis could be created
        
        // Shapes
        public IArray<Vector3> ShapeVertices { get; }

        // A public instance property named ShapeVertexOffsets with a type Vim.LinqArray.IArray<int>
        // No associated operation
        // No data-flow analysis could be created
                public IArray<int> ShapeVertexOffsets { get; }

        // A public instance property named ShapeColors with a type Vim.LinqArray.IArray<Vim.Math3d.Vector4>
        // No associated operation
        // No data-flow analysis could be created
                public IArray<Vector4> ShapeColors { get; }

        // A public instance property named ShapeWidths with a type Vim.LinqArray.IArray<float>
        // No associated operation
        // No data-flow analysis could be created
                public IArray<float> ShapeWidths { get; }

        // A public instance property named ShapeVertexCounts with a type Vim.LinqArray.IArray<int>
        // No associated operation
        // No data-flow analysis could be created
                public IArray<int> ShapeVertexCounts { get; } // Computed

        // A public instance property named Shapes with a type Vim.LinqArray.IArray<Vim.G3d.G3dShape>
        // No associated operation
        // No data-flow analysis could be created
                public IArray<G3dShape> Shapes { get; } // Computed

        // A public instance property named MaterialColors with a type Vim.LinqArray.IArray<Vim.Math3d.Vector4>
        // No associated operation
        // No data-flow analysis could be created
        
        // Materials
        public IArray<Vector4> MaterialColors { get; } // RGBA with transparency.

        // A public instance property named MaterialGlossiness with a type Vim.LinqArray.IArray<float>
        // No associated operation
        // No data-flow analysis could be created
                public IArray<float> MaterialGlossiness { get; }

        // A public instance property named MaterialSmoothness with a type Vim.LinqArray.IArray<float>
        // No associated operation
        // No data-flow analysis could be created
                public IArray<float> MaterialSmoothness { get; }

        // A public instance property named Materials with a type Vim.LinqArray.IArray<Vim.G3d.G3dMaterial>
        // No associated operation
        // No data-flow analysis could be created
                public IArray<G3dMaterial> Materials { get; }

        // A public instance property named SubmeshIndexOffsets with a type Vim.LinqArray.IArray<int>
        // No associated operation
        // No data-flow analysis could be created
        

        // Submeshes
        public IArray<int> SubmeshIndexOffsets { get; }

        // A public instance property named SubmeshIndexCount with a type Vim.LinqArray.IArray<int>
        // No associated operation
        // No data-flow analysis could be created
                public IArray<int> SubmeshIndexCount { get; }

        // A public instance property named SubmeshMaterials with a type Vim.LinqArray.IArray<int>
        // No associated operation
        // No data-flow analysis could be created
                public IArray<int> SubmeshMaterials { get; }

    } // type
} // namespace
