/*
    G3D Geometry Format Library
    Copyright 2019, VIMaec LLC.
    Copyright 2018, Ara 3D Inc.
    Usage licensed under terms of MIT License
*/

using System;
using System.Collections.Generic;
using System.Linq;
using Vim.LinqArray;

namespace Vim.G3d
{
    /// <summary>
    /// This is the most generic representation of a G3D, which is a bag of attributes, and the observed size of each component. 
    /// </summary>
    public class GeometryAttributes : IGeometryAttributes
    {
        public Dictionary<string, GeometryAttribute> Lookup
            = new Dictionary<string, GeometryAttribute>();

        public GeometryAttributes(IEnumerable<GeometryAttribute> attributes, int numCornersPerFaceOverride = -1)
        {
            foreach (var attr in attributes)
                if (attr != null && !Lookup.ContainsKey(attr.Name))
                    Lookup.Add(attr.Name, attr);

            foreach (var attr in Lookup.Values)
            {
                var desc = attr.Descriptor;

                switch (desc.Association)
                {
                    case Association.assoc_none:
                        break;
                    case Association.assoc_vertex:
                        NumVertices = ValidateAttribute(attr, NumVertices);
                        break;
                    case Association.assoc_edge:
                    case Association.assoc_corner:
                        NumCorners = ValidateAttribute(attr, NumCorners);
                        break;
                    case Association.assoc_face:
                        NumFaces = ValidateAttribute(attr, NumFaces);
                        break;
                    case Association.assoc_instance:
                        NumInstances = ValidateAttribute(attr, NumInstances);
                        break;
                    case Association.assoc_submesh:
                        NumSubmeshes = ValidateAttribute(attr, NumSubmeshes);
                        break;
                    case Association.assoc_material:
                        NumMaterials = ValidateAttribute(attr, NumMaterials);
                        break;
                    case Association.assoc_mesh:
                        NumMeshes = ValidateAttribute(attr, NumMeshes);
                        break;
                    case Association.assoc_shapevertex:
                        NumShapeVertices = ValidateAttribute(attr, NumShapeVertices);
                        break;
                    case Association.assoc_shape:
                        NumShapes = ValidateAttribute(attr, NumShapes);
                        break;
                }

                if (desc.Semantic == Semantic.FaceSize)
                {
                    if (desc.Association != Association.assoc_all)
                        throw new Exception(
                            $"The face size semantic has to be associated with entire geometry set, not {desc.Association}");
                    if (desc.DataArity != 1)
                        throw new Exception($"The face size semantic has to have arity of 1, not {desc.DataArity}");

                    if (desc.DataType == DataType.dt_int8)
                        NumCornersPerFace = attr.AsType<byte>().Data[0];
                    else if (desc.DataType == DataType.dt_int16)
                        NumCornersPerFace = attr.AsType<short>().Data[0];
                    else if (desc.DataType == DataType.dt_int32)
                        NumCornersPerFace = attr.AsType<int>().Data[0];
                    else
                        throw new Exception("The face size semantic has to be an int8, int16, or int32");
                }
            }

            if (NumVertices < 0) NumVertices = 0;

            // If the index attribute is missing we will have to add it. 
            if (!Lookup.ContainsKey(CommonAttributes.Index))
            {
                if (NumCorners < 0) NumCorners = NumVertices;
                Lookup.Add(CommonAttributes.Index, NumCorners.Range().ToIndexAttribute());
            }

            // Now we create the public ordered list of attributes 
            Attributes = Lookup.Values.OrderBy(attr => attr.Name).ToIArray();


            // If the number of corner and faces are observed, one has to be a multiple of the other
            if (NumCorners > 0 && NumFaces > 0)
                if (NumCorners % NumFaces != 0)
                    throw new Exception(
                        $"The number of corners {NumCorners} to be divisible by the number of faces {NumFaces}");

            // Try to compute the number of corners per face
            if (NumCornersPerFace < 0)
            {
                if (NumCorners > 0 && NumFaces > 0)
                    // We compute the number of corners per face by dividing the observed number of faces by the observed number of corners 
                    NumCornersPerFace = NumCorners / NumFaces;
                else
                    // By default we assume a triangular mesh
                    NumCornersPerFace = 3;
            }

            if (numCornersPerFaceOverride >= 0) NumCornersPerFace = numCornersPerFaceOverride;

            if (NumCorners < 0) NumCorners = NumVertices;
            if (NumInstances < 0) NumInstances = 0;
            if (NumMeshes < 0) NumMeshes = 0;
            if (NumFaces < 0) NumFaces = NumCorners / NumCornersPerFace;

            if (NumShapeVertices < 0) NumShapeVertices = 0;
            if (NumShapes < 0) NumShapes = 0;
        }

        public int NumMaterials { get; } = -1;
        public int NumSubmeshes { get; } = -1;

        public static GeometryAttributes Empty
            => new GeometryAttributes(new GeometryAttribute[] { });

        public int NumCornersPerFace { get; } = -1;

        public int NumVertices { get; } = -1;
        public int NumFaces { get; } = -1;
        public int NumCorners { get; } = -1;
        public int NumMeshes { get; } = -1;
        public int NumInstances { get; } = -1;

        public int NumShapeVertices { get; } = -1;
        public int NumShapes { get; } = -1;

        public IArray<GeometryAttribute> Attributes { get; }

        public GeometryAttribute GetAttribute(string name)
        {
            return Lookup.TryGetValue(name, out var val) ? val : null;
        }

        public int ValidateAttribute(GeometryAttribute attr, int expectedCount)
        {
            if (expectedCount >= 0 && attr.ElementCount != expectedCount)
                throw new Exception(
                    $"Attribute {attr.Descriptor.Name} size {attr.ElementCount} not match the expected size {expectedCount}");
            return attr.ElementCount;
        }
    }
}