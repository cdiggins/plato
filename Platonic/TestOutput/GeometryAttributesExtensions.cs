using System;

using System.Collections.Generic;

using System.Linq;

using Vim.LinqArray;

using Vim.Math3d;

namespace Vim.G3d
{
    // Type has fields False
    // Type has writable fields False
    // Type has public setters False
    public static class GeometryAttributesExtensions
    {
        // A public static method named ExpectedElementCount with a type int
        // operation kind is Block and type 
        // member references = Association, assoc_all, assoc_none, assoc_vertex, NumVertices, assoc_face, NumFaces, assoc_corner, NumCorners, assoc_edge, NumCorners, assoc_mesh, NumMeshes, assoc_instance, NumInstances, assoc_shapevertex, NumShapeVertices, assoc_shape, NumShapes
        // assignments = 
        // Written symbols are 
        // Read symbols are (Name=self Kind=Parameter), (Name=desc Kind=Parameter)
        // Captured symbols are 
        // Variables declared are 
                public static int ExpectedElementCount(this IGeometryAttributes self, AttributeDescriptor desc)
        {
            switch (desc.Association)
            {
                case Association.assoc_all:
                    return 1;
                case Association.assoc_none:
                    return 0;
                case Association.assoc_vertex:
                    return self.NumVertices;
                case Association.assoc_face:
                    return self.NumFaces;
                case Association.assoc_corner:
                    return self.NumCorners;
                case Association.assoc_edge:
                    return self.NumCorners;
                case Association.assoc_mesh:
                    return self.NumMeshes;
                case Association.assoc_instance:
                    return self.NumInstances;
                case Association.assoc_shapevertex:
                    return self.NumShapeVertices;
                case Association.assoc_shape:
                    return self.NumShapes;
            }

            return -1;
        }

        // A public static method named AttributeNames with a type Vim.LinqArray.IArray<string>
        // operation kind is Block and type 
        // member references = Attributes, Name
        // assignments = 
        // Written symbols are (Name=attr Kind=Parameter)
        // Read symbols are (Name=g Kind=Parameter), (Name=attr Kind=Parameter)
        // Captured symbols are 
        // Variables declared are (Name=attr Kind=Parameter)
        
        public static IArray<string> AttributeNames(this IGeometryAttributes g)
        {
            return g.Attributes.Select(attr => attr.Name);
        }

        // A public static method named GetAttribute with a type Vim.G3d.GeometryAttribute<T>
        // operation kind is Block and type 
        // member references = 
        // assignments = 
        // Written symbols are 
        // Read symbols are (Name=g Kind=Parameter), (Name=attributeName Kind=Parameter)
        // Captured symbols are 
        // Variables declared are 
        
        public static GeometryAttribute<T> GetAttribute<T>(this IGeometryAttributes g, string attributeName)
            
        {
            return g.GetAttribute(attributeName)?.AsType<T>();
        }

        // A public static method named DefaultAttribute with a type Vim.G3d.GeometryAttribute
        // operation kind is Block and type 
        // member references = 
        // assignments = 
        // Written symbols are 
        // Read symbols are (Name=self Kind=Parameter), (Name=name Kind=Parameter)
        // Captured symbols are 
        // Variables declared are 
        
        public static GeometryAttribute DefaultAttribute(this IGeometryAttributes self, string name)
        {
            return self.DefaultAttribute(AttributeDescriptor.Parse(name));
        }

        // A public static method named DefaultAttribute with a type Vim.G3d.GeometryAttribute
        // operation kind is Block and type 
        // member references = 
        // assignments = 
        // Written symbols are 
        // Read symbols are (Name=self Kind=Parameter), (Name=desc Kind=Parameter)
        // Captured symbols are 
        // Variables declared are 
        
        public static GeometryAttribute DefaultAttribute(this IGeometryAttributes self, AttributeDescriptor desc)
        {
            return desc.ToDefaultAttribute(self.ExpectedElementCount(desc));
        }

        // A public static method named GetOrDefaultAttribute with a type Vim.G3d.GeometryAttribute
        // operation kind is Block and type 
        // member references = 
        // assignments = 
        // Written symbols are 
        // Read symbols are (Name=self Kind=Parameter), (Name=desc Kind=Parameter)
        // Captured symbols are 
        // Variables declared are 
        
        public static GeometryAttribute GetOrDefaultAttribute(this IGeometryAttributes self, AttributeDescriptor desc)
        {
            return self.GetAttribute(desc.ToString()) ?? desc.ToDefaultAttribute(self.ExpectedElementCount(desc));
        }

        // A public static method named NoneAttributes with a type System.Collections.Generic.IEnumerable<Vim.G3d.GeometryAttribute>
        // operation kind is Block and type 
        // member references = Attributes, Association, Descriptor, assoc_none
        // assignments = 
        // Written symbols are (Name=a Kind=Parameter)
        // Read symbols are (Name=g Kind=Parameter), (Name=a Kind=Parameter)
        // Captured symbols are 
        // Variables declared are (Name=a Kind=Parameter)
        
        public static IEnumerable<GeometryAttribute> NoneAttributes(this IGeometryAttributes g)
        {
            return g.Attributes.Where(a => a.Descriptor.Association == Association.assoc_none);
        }

        // A public static method named CornerAttributes with a type System.Collections.Generic.IEnumerable<Vim.G3d.GeometryAttribute>
        // operation kind is Block and type 
        // member references = Attributes, Association, Descriptor, assoc_corner
        // assignments = 
        // Written symbols are (Name=a Kind=Parameter)
        // Read symbols are (Name=g Kind=Parameter), (Name=a Kind=Parameter)
        // Captured symbols are 
        // Variables declared are (Name=a Kind=Parameter)
        
        public static IEnumerable<GeometryAttribute> CornerAttributes(this IGeometryAttributes g)
        {
            return g.Attributes.Where(a => a.Descriptor.Association == Association.assoc_corner);
        }

        // A public static method named EdgeAttributes with a type System.Collections.Generic.IEnumerable<Vim.G3d.GeometryAttribute>
        // operation kind is Block and type 
        // member references = Attributes, Association, Descriptor, assoc_edge
        // assignments = 
        // Written symbols are (Name=a Kind=Parameter)
        // Read symbols are (Name=g Kind=Parameter), (Name=a Kind=Parameter)
        // Captured symbols are 
        // Variables declared are (Name=a Kind=Parameter)
        
        public static IEnumerable<GeometryAttribute> EdgeAttributes(this IGeometryAttributes g)
        {
            return g.Attributes.Where(a => a.Descriptor.Association == Association.assoc_edge);
        }

        // A public static method named FaceAttributes with a type System.Collections.Generic.IEnumerable<Vim.G3d.GeometryAttribute>
        // operation kind is Block and type 
        // member references = Attributes, Association, Descriptor, assoc_face
        // assignments = 
        // Written symbols are (Name=a Kind=Parameter)
        // Read symbols are (Name=g Kind=Parameter), (Name=a Kind=Parameter)
        // Captured symbols are 
        // Variables declared are (Name=a Kind=Parameter)
        
        public static IEnumerable<GeometryAttribute> FaceAttributes(this IGeometryAttributes g)
        {
            return g.Attributes.Where(a => a.Descriptor.Association == Association.assoc_face);
        }

        // A public static method named VertexAttributes with a type System.Collections.Generic.IEnumerable<Vim.G3d.GeometryAttribute>
        // operation kind is Block and type 
        // member references = Attributes, Association, Descriptor, assoc_vertex
        // assignments = 
        // Written symbols are (Name=a Kind=Parameter)
        // Read symbols are (Name=g Kind=Parameter), (Name=a Kind=Parameter)
        // Captured symbols are 
        // Variables declared are (Name=a Kind=Parameter)
        
        public static IEnumerable<GeometryAttribute> VertexAttributes(this IGeometryAttributes g)
        {
            return g.Attributes.Where(a => a.Descriptor.Association == Association.assoc_vertex);
        }

        // A public static method named InstanceAttributes with a type System.Collections.Generic.IEnumerable<Vim.G3d.GeometryAttribute>
        // operation kind is Block and type 
        // member references = Attributes, Association, Descriptor, assoc_instance
        // assignments = 
        // Written symbols are (Name=a Kind=Parameter)
        // Read symbols are (Name=g Kind=Parameter), (Name=a Kind=Parameter)
        // Captured symbols are 
        // Variables declared are (Name=a Kind=Parameter)
        
        public static IEnumerable<GeometryAttribute> InstanceAttributes(this IGeometryAttributes g)
        {
            return g.Attributes.Where(a => a.Descriptor.Association == Association.assoc_instance);
        }

        // A public static method named MeshAttributes with a type System.Collections.Generic.IEnumerable<Vim.G3d.GeometryAttribute>
        // operation kind is Block and type 
        // member references = Attributes, Association, Descriptor, assoc_mesh
        // assignments = 
        // Written symbols are (Name=a Kind=Parameter)
        // Read symbols are (Name=g Kind=Parameter), (Name=a Kind=Parameter)
        // Captured symbols are 
        // Variables declared are (Name=a Kind=Parameter)
        
        public static IEnumerable<GeometryAttribute> MeshAttributes(this IGeometryAttributes g)
        {
            return g.Attributes.Where(a => a.Descriptor.Association == Association.assoc_mesh);
        }

        // A public static method named SubMeshAttributes with a type System.Collections.Generic.IEnumerable<Vim.G3d.GeometryAttribute>
        // operation kind is Block and type 
        // member references = Attributes, Association, Descriptor, assoc_submesh
        // assignments = 
        // Written symbols are (Name=a Kind=Parameter)
        // Read symbols are (Name=g Kind=Parameter), (Name=a Kind=Parameter)
        // Captured symbols are 
        // Variables declared are (Name=a Kind=Parameter)
        
        public static IEnumerable<GeometryAttribute> SubMeshAttributes(this IGeometryAttributes g)
        {
            return g.Attributes.Where(a => a.Descriptor.Association == Association.assoc_submesh);
        }

        // A public static method named WholeGeometryAttributes with a type System.Collections.Generic.IEnumerable<Vim.G3d.GeometryAttribute>
        // operation kind is Block and type 
        // member references = Attributes, Association, Descriptor, assoc_all
        // assignments = 
        // Written symbols are (Name=a Kind=Parameter)
        // Read symbols are (Name=g Kind=Parameter), (Name=a Kind=Parameter)
        // Captured symbols are 
        // Variables declared are (Name=a Kind=Parameter)
        
        public static IEnumerable<GeometryAttribute> WholeGeometryAttributes(this IGeometryAttributes g)
        {
            return g.Attributes.Where(a => a.Descriptor.Association == Association.assoc_all);
        }

        // A public static method named ShapeVertexAttributes with a type System.Collections.Generic.IEnumerable<Vim.G3d.GeometryAttribute>
        // operation kind is Block and type 
        // member references = Attributes, Association, Descriptor, assoc_shapevertex
        // assignments = 
        // Written symbols are (Name=a Kind=Parameter)
        // Read symbols are (Name=g Kind=Parameter), (Name=a Kind=Parameter)
        // Captured symbols are 
        // Variables declared are (Name=a Kind=Parameter)
        
        public static IEnumerable<GeometryAttribute> ShapeVertexAttributes(this IGeometryAttributes g)
        {
            return g.Attributes.Where(a => a.Descriptor.Association == Association.assoc_shapevertex);
        }

        // A public static method named ShapeAttributes with a type System.Collections.Generic.IEnumerable<Vim.G3d.GeometryAttribute>
        // operation kind is Block and type 
        // member references = Attributes, Association, Descriptor, assoc_shape
        // assignments = 
        // Written symbols are (Name=a Kind=Parameter)
        // Read symbols are (Name=g Kind=Parameter), (Name=a Kind=Parameter)
        // Captured symbols are 
        // Variables declared are (Name=a Kind=Parameter)
        
        public static IEnumerable<GeometryAttribute> ShapeAttributes(this IGeometryAttributes g)
        {
            return g.Attributes.Where(a => a.Descriptor.Association == Association.assoc_shape);
        }

        // A public static method named HasSameAttributes with a type bool
        // operation kind is Block and type 
        // member references = Count, Attributes, Count, Attributes, Attributes, Name, this[], Attributes, Name, this[], Attributes
        // assignments = 
        // Written symbols are (Name=i Kind=Parameter)
        // Read symbols are (Name=g1 Kind=Parameter), (Name=g2 Kind=Parameter), (Name=i Kind=Parameter)
        // Captured symbols are (Name=g1 Kind=Parameter), (Name=g2 Kind=Parameter)
        // Variables declared are (Name=i Kind=Parameter)
        
        public static bool HasSameAttributes(this IGeometryAttributes g1, IGeometryAttributes g2)
        {
            return g1.Attributes.Count == g2.Attributes.Count &&
                   g1.Attributes.Indices().All(i => g1.Attributes[i].Name == g2.Attributes[i].Name);
        }

        // A public static method named FaceToCorner with a type int
        // operation kind is Block and type 
        // member references = NumCornersPerFace
        // assignments = 
        // Written symbols are 
        // Read symbols are (Name=g Kind=Parameter), (Name=f Kind=Parameter)
        // Captured symbols are 
        // Variables declared are 
        
        public static int FaceToCorner(this IGeometryAttributes g, int f)
        {
            return f * g.NumCornersPerFace;
        }

        // A public static method named FaceIndicesToCornerIndices with a type Vim.LinqArray.IArray<int>
        // operation kind is Block and type 
        // member references = Count, NumCornersPerFace, this[], NumCornersPerFace, NumCornersPerFace
        // assignments = 
        // Written symbols are (Name=i Kind=Parameter)
        // Read symbols are (Name=g Kind=Parameter), (Name=faceIndices Kind=Parameter), (Name=i Kind=Parameter)
        // Captured symbols are (Name=g Kind=Parameter), (Name=faceIndices Kind=Parameter)
        // Variables declared are (Name=i Kind=Parameter)
        
        /// <summary>
        /// Given a set of face indices, creates an array of corner indices
        /// </summary>
        public static IArray<int> FaceIndicesToCornerIndices(this IGeometryAttributes g, IArray<int> faceIndices)
        {
            return (faceIndices.Count * g.NumCornersPerFace)
                .Select(i => g.FaceToCorner(faceIndices[i / g.NumCornersPerFace]) + i % g.NumCornersPerFace);
        }

        // A public static method named FaceIndicesToFirstCornerIndices with a type Vim.LinqArray.IArray<int>
        // operation kind is Block and type 
        // member references = NumCornersPerFace
        // assignments = 
        // Written symbols are (Name=f Kind=Parameter)
        // Read symbols are (Name=g Kind=Parameter), (Name=faceIndices Kind=Parameter), (Name=f Kind=Parameter)
        // Captured symbols are (Name=g Kind=Parameter)
        // Variables declared are (Name=f Kind=Parameter)
        
        /// <summary>
        /// Given a set of face indices, creates an array of indices of the first corner in each face
        /// </summary>
        public static IArray<int> FaceIndicesToFirstCornerIndices(this IGeometryAttributes g, IArray<int> faceIndices)
        {
            return faceIndices.Select(f => f * g.NumCornersPerFace);
        }

        // A public static method named CornerToFace with a type int
        // operation kind is Block and type 
        // member references = NumCornersPerFace
        // assignments = 
        // Written symbols are 
        // Read symbols are (Name=g Kind=Parameter), (Name=c Kind=Parameter)
        // Captured symbols are 
        // Variables declared are 
        
        public static int CornerToFace(this IGeometryAttributes g, int c)
        {
            return c / g.NumCornersPerFace;
        }

        // A public static method named CornersToFaces with a type Vim.LinqArray.IArray<int>
        // operation kind is Block and type 
        // member references = NumCorners, CornerToFace
        // assignments = 
        // Written symbols are 
        // Read symbols are (Name=g Kind=Parameter)
        // Captured symbols are 
        // Variables declared are 
        
        public static IArray<int> CornersToFaces(this IGeometryAttributes g)
        {
            return g.NumCorners.Select(g.CornerToFace);
        }

        // A public static method named CornerNumber with a type int
        // operation kind is Block and type 
        // member references = NumCornersPerFace
        // assignments = 
        // Written symbols are 
        // Read symbols are (Name=g Kind=Parameter), (Name=c Kind=Parameter)
        // Captured symbols are 
        // Variables declared are 
        
        public static int CornerNumber(this IGeometryAttributes g, int c)
        {
            return c % g.NumCornersPerFace;
        }

        // A public static method named ToGeometryAttributes with a type Vim.G3d.IGeometryAttributes
        // operation kind is Block and type 
        // member references = 
        // assignments = 
        // Written symbols are 
        // Read symbols are (Name=attributes Kind=Parameter)
        // Captured symbols are 
        // Variables declared are 
        
        public static IGeometryAttributes ToGeometryAttributes(this IEnumerable<GeometryAttribute> attributes)
        {
            return new GeometryAttributes(attributes);
        }

        // A public static method named ToGeometryAttributes with a type Vim.G3d.IGeometryAttributes
        // operation kind is Block and type 
        // member references = 
        // assignments = 
        // Written symbols are 
        // Read symbols are (Name=attributes Kind=Parameter)
        // Captured symbols are 
        // Variables declared are 
        
        public static IGeometryAttributes ToGeometryAttributes(this IArray<GeometryAttribute> attributes)
        {
            return attributes.ToEnumerable().ToGeometryAttributes();
        }

        // A public static method named AddAttributes with a type Vim.G3d.IGeometryAttributes
        // operation kind is Block and type 
        // member references = Attributes
        // assignments = 
        // Written symbols are 
        // Read symbols are (Name=attributes Kind=Parameter), (Name=newAttributes Kind=Parameter)
        // Captured symbols are 
        // Variables declared are 
        
        public static IGeometryAttributes AddAttributes(this IGeometryAttributes attributes,
            params GeometryAttribute[] newAttributes)
        {
            return attributes.Attributes.ToEnumerable().Concat(newAttributes).ToGeometryAttributes();
        }

        // A public static method named GetAttributeOrDefault with a type Vim.G3d.GeometryAttribute
        // operation kind is Block and type 
        // member references = 
        // assignments = 
        // Written symbols are 
        // Read symbols are (Name=g Kind=Parameter), (Name=name Kind=Parameter)
        // Captured symbols are 
        // Variables declared are 
        
        public static GeometryAttribute GetAttributeOrDefault(this IGeometryAttributes g, string name)
        {
            return g.GetAttribute(name) ?? g.DefaultAttribute(name);
        }

        // A public static method named Merge with a type Vim.G3d.IGeometryAttributes
        // operation kind is Block and type 
        // member references = 
        // assignments = 
        // Written symbols are (Name=x Kind=Parameter)
        // Read symbols are (Name=gs Kind=Parameter), (Name=x Kind=Parameter)
        // Captured symbols are 
        // Variables declared are (Name=x Kind=Parameter)
        
        public static IGeometryAttributes Merge(this IArray<G3D> gs)
        {
            return gs.Select(x => (IGeometryAttributes)x).Merge();
        }

        // A public static method named Merge with a type Vim.G3d.IGeometryAttributes
        // operation kind is Block and type 
        // member references = 
        // assignments = 
        // Written symbols are 
        // Read symbols are (Name=self Kind=Parameter), (Name=gs Kind=Parameter)
        // Captured symbols are 
        // Variables declared are 
        
        public static IGeometryAttributes Merge(this IGeometryAttributes self, params IGeometryAttributes[] gs)
        {
            return gs.ToIArray().Prepend(self).Merge();
        }

        // A public static method named Merge with a type Vim.G3d.IGeometryAttributes
        // operation kind is Block and type 
        // member references = Count, Empty, this[], Count, NumCornersPerFace, NumCornersPerFace, Semantic, Descriptor, Index, Name, NumVertices, NumCorners
        // assignments = 
        // Written symbols are (Name=first Kind=Local), (Name=corners Kind=Local), (Name=g Kind=Parameter), (Name=attributes Kind=Local), (Name=attr Kind=Parameter), (Name=others Kind=Local), (Name=attributeList Kind=Local), (Name=attr Kind=Parameter), (Name=g Kind=Parameter), (Name=mergedIndexAttribute Kind=Local), (Name=ga Kind=Parameter), (Name=ga Kind=Parameter), (Name=mergedSubmeshIndexOffsetAttribute Kind=Local), (Name=ga Kind=Parameter), (Name=ga Kind=Parameter)
        // Read symbols are (Name=geometryAttributesArray Kind=Parameter), (Name=first Kind=Local), (Name=corners Kind=Local), (Name=g Kind=Parameter), (Name=attributes Kind=Local), (Name=attr Kind=Parameter), (Name=others Kind=Local), (Name=attributeList Kind=Local), (Name=attr Kind=Parameter), (Name=g Kind=Parameter), (Name=mergedIndexAttribute Kind=Local), (Name=ga Kind=Parameter), (Name=ga Kind=Parameter), (Name=mergedSubmeshIndexOffsetAttribute Kind=Local), (Name=ga Kind=Parameter), (Name=ga Kind=Parameter)
        // Captured symbols are (Name=corners Kind=Local), (Name=others Kind=Local), (Name=attr Kind=Parameter)
        // Variables declared are (Name=first Kind=Local), (Name=corners Kind=Local), (Name=g Kind=Parameter), (Name=attributes Kind=Local), (Name=attr Kind=Parameter), (Name=others Kind=Local), (Name=attributeList Kind=Local), (Name=attr Kind=Parameter), (Name=g Kind=Parameter), (Name=mergedIndexAttribute Kind=Local), (Name=ga Kind=Parameter), (Name=ga Kind=Parameter), (Name=mergedSubmeshIndexOffsetAttribute Kind=Local), (Name=ga Kind=Parameter), (Name=ga Kind=Parameter)
        
        public static IGeometryAttributes Merge(this IArray<IGeometryAttributes> geometryAttributesArray)
        {
            if (geometryAttributesArray.Count == 0)
                return GeometryAttributes.Empty;

            var first = geometryAttributesArray[0];

            if (geometryAttributesArray.Count == 1)
                return first;
            var corners = first.NumCornersPerFace;
            if (!geometryAttributesArray.All(g => g.NumCornersPerFace == corners))
                throw new Exception("Cannot merge meshes with different numbers of corners per faces");

            // Merge all of the attributes of the different geometries
            // Except: indices, group indexes, subgeo, and instance attributes
            var attributes = first.VertexAttributes()
                .Concat(first.CornerAttributes())
                .Concat(first.EdgeAttributes())
                .Concat(first.NoneAttributes())
                .Concat(first.FaceAttributes())
                .Append(first.GetAttributeSubmeshMaterial())
                // Skip the index semantic because things get re-ordered
                .Where(attr => attr != null && attr.Descriptor.Semantic != Semantic.Index)
                .ToArray();

            // Merge the non-indexed attributes
            var others = geometryAttributesArray.Skip().ToEnumerable();
            var attributeList = attributes.Select(
                attr => attr.Merge(others.Select(g => g.GetAttributeOrDefault(attr.Name)))).ToList();

            // Merge the index attribute
            // numVertices:               [X],       [Y],             [Z],                   ...
            // valueOffsets:              [0],       [X],             [X+Y],                 ...
            // indices:                   [A, B, C], [D,     E,   F], [G,         H,     I], ...
            // mergedIndices:             [A, B, C], [X+D, X+E, X+F], [X+Y+G, X+Y+H, X+Y+I], ...
            var mergedIndexAttribute = geometryAttributesArray.MergeIndexedAttribute(
                    ga => ga.GetAttributeIndex(),
                    ga => ga.NumVertices)
                ?.ToIndexAttribute();

            if (mergedIndexAttribute != null)
                attributeList.Add(mergedIndexAttribute);

            // Merge the submesh index offset attribute
            // numCorners:                [X],       [Y],           [Z],                 ...
            // valueOffsets:              [0]        [X],           [X+Y],               ...
            // submeshIndexOffsets:       [0, A, B], [0,   C,   D], [0,       E,     F], ...
            // mergedSubmeshIndexOffsets: [0, A, B], [X, X+C, X+D], [X+Y, X+Y+E, X+Y+F], ...
            var mergedSubmeshIndexOffsetAttribute = geometryAttributesArray.MergeIndexedAttribute(
                    ga => ga.GetAttributeSubmeshIndexOffset(),
                    ga => ga.NumCorners)
                ?.ToSubmeshIndexOffsetAttribute();

            if (mergedSubmeshIndexOffsetAttribute != null)
                attributeList.Add(mergedSubmeshIndexOffsetAttribute);

            return attributeList.ToGeometryAttributes();
        }

        // A public static method named MergeIndexedAttribute with a type int[]
        // operation kind is Block and type 
        // member references = Count, Data, Attribute, Data, Count, Data, this[]
        // assignments = Binary, LocalReference, Invocation
        // Written symbols are (Name=first Kind=Local), (Name=firstAttribute Kind=Local), (Name=tuples Kind=Parameter), (Name=valueOffset Kind=Local), (Name=mergedCount Kind=Local), (Name=merged Kind=Local), (Name=t Kind=Parameter), (Name=parent Kind=Local), (Name=attr Kind=Local), (Name=attrData Kind=Local), (Name=attrDataCount Kind=Local), (Name=i Kind=Local)
        // Read symbols are (Name=geometryAttributesArray Kind=Parameter), (Name=getIndexedAttributeFunc Kind=Parameter), (Name=getValueOffsetFunc Kind=Parameter), (Name=initialValueOffset Kind=Parameter), (Name=first Kind=Local), (Name=firstAttribute Kind=Local), (Name=tuples Kind=Parameter), (Name=valueOffset Kind=Local), (Name=mergedCount Kind=Local), (Name=merged Kind=Local), (Name=t Kind=Parameter), (Name=parent Kind=Local), (Name=attr Kind=Local), (Name=attrData Kind=Local), (Name=attrDataCount Kind=Local), (Name=i Kind=Local)
        // Captured symbols are (Name=getValueOffsetFunc Kind=Parameter), (Name=initialValueOffset Kind=Parameter)
        // Variables declared are (Name=first Kind=Local), (Name=firstAttribute Kind=Local), (Name=tuples Kind=Parameter), (Name=valueOffset Kind=Local), (Name=mergedCount Kind=Local), (Name=merged Kind=Local), (Name=t Kind=Parameter), (Name=parent Kind=Local), (Name=attr Kind=Local), (Name=attrData Kind=Local), (Name=attrDataCount Kind=Local), (Name=i Kind=Local)
        
        /// <summary>
        /// Merges the indexed attributes based on the given transformations and returns an array of integers
        /// representing the merged and offset values.
        /// </summary>
        public static int[] MergeIndexedAttribute(
            this IArray<IGeometryAttributes> geometryAttributesArray,
            System.Func<IGeometryAttributes, GeometryAttribute<int>> getIndexedAttributeFunc,
            Func<IGeometryAttributes, int> getValueOffsetFunc,
            int initialValueOffset = 0)
        {
            var first = geometryAttributesArray.FirstOrDefault();
            if (first == null)
                return null;

            var firstAttribute = getIndexedAttributeFunc(first);
            if (firstAttribute == null)
                return null;

            return geometryAttributesArray.MergeAttributes(
                getIndexedAttributeFunc,
                tuples =>
                {
                    var valueOffset = initialValueOffset;
                    var mergedCount = 0;

                    var merged = new int[tuples.Sum(t => t.Attribute.Data.Count)];

                    foreach (var (parent, attr) in tuples)
                    {
                        var attrData = attr.Data;
                        var attrDataCount = attr.Data.Count;

                        for (var i = 0; i < attrDataCount; ++i)
                            merged[mergedCount + i] = attrData[i] + valueOffset;

                        mergedCount += attrDataCount;
                        valueOffset += getValueOffsetFunc(parent);
                    }

                    return merged;
                });
        }

        // A public static method named MergeAttributes with a type T[]
        // operation kind is Block and type 
        // member references = GeometryAttribute, Length, Count
        // assignments = 
        // Written symbols are (Name=tuples Kind=Local), (Name=ga Kind=Parameter), (Name=tuple Kind=Parameter)
        // Read symbols are (Name=geometryAttributesArray Kind=Parameter), (Name=getAttributeFunc Kind=Parameter), (Name=mergeFunc Kind=Parameter), (Name=tuples Kind=Local), (Name=ga Kind=Parameter), (Name=tuple Kind=Parameter)
        // Captured symbols are (Name=getAttributeFunc Kind=Parameter)
        // Variables declared are (Name=tuples Kind=Local), (Name=ga Kind=Parameter), (Name=tuple Kind=Parameter)
        
        /// <summary>
        /// Merges the attributes based on the given transformations and returns an array of merged values.
        /// </summary>
        public static T[] MergeAttributes<T>(
            this IArray<IGeometryAttributes> geometryAttributesArray,
            Func<IGeometryAttributes, GeometryAttribute<T>> getAttributeFunc,
            Func<(IGeometryAttributes Parent, GeometryAttribute<T> Attribute)[], T[]> mergeFunc) 
        {
            var tuples = geometryAttributesArray
                .Select(ga => (Parent: ga, GeometryAttribute: getAttributeFunc(ga)))
                .Where(tuple => tuple.GeometryAttribute != null)
                .ToArray();

            if (tuples.Length != geometryAttributesArray.Count)
                throw new Exception("The geometry attributes array do not all contain the same attribute");

            return mergeFunc(tuples);
        }

        // A public static method named Deform with a type Vim.G3d.IGeometryAttributes
        // operation kind is Block and type 
        // member references = Attributes, Semantic, Descriptor, Position, Data, Descriptor, Semantic, Descriptor, Normal, Data, Descriptor
        // assignments = 
        // Written symbols are (Name=a Kind=Parameter), (Name=p Kind=Local), (Name=n Kind=Local)
        // Read symbols are (Name=g Kind=Parameter), (Name=positionTransform Kind=Parameter), (Name=normalTransform Kind=Parameter), (Name=a Kind=Parameter), (Name=p Kind=Local), (Name=n Kind=Local)
        // Captured symbols are (Name=positionTransform Kind=Parameter), (Name=normalTransform Kind=Parameter)
        // Variables declared are (Name=a Kind=Parameter), (Name=p Kind=Local), (Name=n Kind=Local)
        
        /// <summary>
        /// Applies a transformation function to position attributes and another to normal attributes. When deforming, we may want to
        /// apply a similar deformation to the normals. For example a matrix can change the position, rotation, and scale of a geometry,
        /// but the only changes should be to the direction of the normal, not the length.
        /// </summary>
        public static IGeometryAttributes Deform(this IGeometryAttributes g, Func<Vector3, Vector3> positionTransform,
            Func<Vector3, Vector3> normalTransform)
        {
            return g.Attributes.Select(
                    a =>
                        a.Descriptor.Semantic == Semantic.Position && a is GeometryAttribute<Vector3> p
                            ? p.Data.Select(positionTransform).ToAttribute(a.Descriptor)
                            : a.Descriptor.Semantic == Semantic.Normal && a is GeometryAttribute<Vector3> n
                                ? n.Data.Select(normalTransform).ToAttribute(a.Descriptor)
                                : a)
                .ToGeometryAttributes();
        }

        // A public static method named Deform with a type Vim.G3d.IGeometryAttributes
        // operation kind is Block and type 
        // member references = Attributes, Semantic, Descriptor, Position, Data, Descriptor
        // assignments = 
        // Written symbols are (Name=a Kind=Parameter), (Name=p Kind=Local)
        // Read symbols are (Name=g Kind=Parameter), (Name=positionTransform Kind=Parameter), (Name=a Kind=Parameter), (Name=p Kind=Local)
        // Captured symbols are (Name=positionTransform Kind=Parameter)
        // Variables declared are (Name=a Kind=Parameter), (Name=p Kind=Local)
        
        /// <summary>
        /// Applies a deformation to points, without changing the normals. For some transformation functions this can result in incorrect normals.
        /// </summary>
        public static IGeometryAttributes Deform(this IGeometryAttributes g, Func<Vector3, Vector3> positionTransform)
        {
            return g.Attributes.Select(
                    a =>
                        a.Descriptor.Semantic == Semantic.Position && a is GeometryAttribute<Vector3> p
                            ? p.Data.Select(positionTransform).ToAttribute(a.Descriptor)
                            : a)
                .ToGeometryAttributes();
        }

        // A public static method named SetPosition with a type Vim.G3d.IGeometryAttributes
        // operation kind is Block and type 
        // member references = 
        // assignments = 
        // Written symbols are 
        // Read symbols are (Name=g Kind=Parameter), (Name=points Kind=Parameter)
        // Captured symbols are 
        // Variables declared are 
        
        public static IGeometryAttributes SetPosition(this IGeometryAttributes g, IArray<Vector3> points)
        {
            return g.SetAttribute(points.ToPositionAttribute());
        }

        // A public static method named SetAttribute with a type Vim.G3d.IGeometryAttributes
        // operation kind is Block and type 
        // member references = Attributes, Descriptor, Descriptor
        // assignments = 
        // Written symbols are (Name=a Kind=Parameter)
        // Read symbols are (Name=self Kind=Parameter), (Name=attr Kind=Parameter), (Name=a Kind=Parameter)
        // Captured symbols are (Name=attr Kind=Parameter)
        // Variables declared are (Name=a Kind=Parameter)
        
        public static IGeometryAttributes SetAttribute(this IGeometryAttributes self, GeometryAttribute attr)
        {
            return self.Attributes.Where(a => !a.Descriptor.Equals(attr.Descriptor)).Append(attr)
                .ToGeometryAttributes();
        }

        // A public static method named SetAttribute with a type Vim.G3d.IGeometryAttributes
        // operation kind is Block and type 
        // member references = 
        // assignments = 
        // Written symbols are 
        // Read symbols are (Name=self Kind=Parameter), (Name=values Kind=Parameter), (Name=desc Kind=Parameter)
        // Captured symbols are 
        // Variables declared are 
        
        public static IGeometryAttributes SetAttribute<ValueT>(this IGeometryAttributes self, IArray<ValueT> values,
            AttributeDescriptor desc) where ValueT : unmanaged
        {
            return self.SetAttribute(values.ToAttribute(desc));
        }

        // A public static method named RemapFaces with a type Vim.G3d.IGeometryAttributes
        // operation kind is Block and type 
        // member references = 
        // assignments = 
        // Written symbols are 
        // Read symbols are (Name=g Kind=Parameter), (Name=faceRemap Kind=Parameter)
        // Captured symbols are 
        // Variables declared are 
        
        /// <summary>
        /// Leaves the vertex buffer intact and creates a new geometry that remaps all of the group, corner, and face data.
        /// The newFaces array is a list of indices into the old face array.
        /// Note: meshes are lost.
        /// </summary>
        public static IGeometryAttributes RemapFaces(this IGeometryAttributes g, IArray<int> faceRemap)
        {
            return g.RemapFacesAndCorners(faceRemap, g.FaceIndicesToCornerIndices(faceRemap));
        }

        // A public static method named SetFaceSizeAttribute with a type System.Collections.Generic.IEnumerable<Vim.G3d.GeometryAttribute>
        // operation kind is Block and type 
        // member references = Semantic, Descriptor, FaceSize
        // assignments = 
        // Written symbols are (Name=attr Kind=Parameter)
        // Read symbols are (Name=attributes Kind=Parameter), (Name=numCornersPerFaces Kind=Parameter), (Name=attr Kind=Parameter)
        // Captured symbols are 
        // Variables declared are (Name=attr Kind=Parameter)
        
        public static IEnumerable<GeometryAttribute> SetFaceSizeAttribute(
            this IEnumerable<GeometryAttribute> attributes, int numCornersPerFaces)
        {
            return numCornersPerFaces <= 0
                ? attributes
                : attributes
                    .Where(attr => attr.Descriptor.Semantic != Semantic.FaceSize)
                    .Append(new[] { numCornersPerFaces }.ToObjectFaceSizeAttribute());
        }

        // A public static method named RemapFacesAndCorners with a type Vim.G3d.IGeometryAttributes
        // operation kind is Block and type 
        // member references = 
        // assignments = 
        // Written symbols are (Name=attr Kind=Parameter), (Name=attr Kind=Parameter), (Name=attr Kind=Parameter)
        // Read symbols are (Name=g Kind=Parameter), (Name=faceRemap Kind=Parameter), (Name=cornerRemap Kind=Parameter), (Name=numCornersPerFace Kind=Parameter), (Name=attr Kind=Parameter), (Name=attr Kind=Parameter), (Name=attr Kind=Parameter)
        // Captured symbols are (Name=faceRemap Kind=Parameter), (Name=cornerRemap Kind=Parameter)
        // Variables declared are (Name=attr Kind=Parameter), (Name=attr Kind=Parameter), (Name=attr Kind=Parameter)
        
        /// <summary>
        /// Low-level remap function. Maps faces and corners at the same time.
        /// In some cases, this is important (e.g. triangulating quads).
        /// Note: meshes are lost.
        /// </summary>
        public static IGeometryAttributes RemapFacesAndCorners(this IGeometryAttributes g, IArray<int> faceRemap,
            IArray<int> cornerRemap, int numCornersPerFace = -1)
        {
            return g.VertexAttributes()
                .Concat(g.NoneAttributes())
                .Concat(g.FaceAttributes().Select(attr => attr.Remap(faceRemap)))
                .Concat(g.EdgeAttributes().Select(attr => attr.Remap(cornerRemap)))
                .Concat(g.CornerAttributes().Select(attr => attr.Remap(cornerRemap)))
                .Concat(g.WholeGeometryAttributes())
                .SetFaceSizeAttribute(numCornersPerFace)
                .ToGeometryAttributes();
        }

        // A public static method named TriangulateQuadMesh with a type Vim.G3d.IGeometryAttributes
        // operation kind is Block and type 
        // member references = NumCornersPerFace, NumFaces, NumFaces, NumFaces
        // assignments = Binary, Binary, Binary, Binary, Binary, Binary, LocalReference, LocalReference
        // Written symbols are (Name=cornerRemap Kind=Local), (Name=faceRemap Kind=Local), (Name=cur Kind=Local), (Name=i Kind=Local)
        // Read symbols are (Name=g Kind=Parameter), (Name=cornerRemap Kind=Local), (Name=faceRemap Kind=Local), (Name=cur Kind=Local), (Name=i Kind=Local)
        // Captured symbols are 
        // Variables declared are (Name=cornerRemap Kind=Local), (Name=faceRemap Kind=Local), (Name=cur Kind=Local), (Name=i Kind=Local)
        
        /// <summary>
        /// Converts a quadrilateral mesh into a triangular mesh carrying over all attributes.
        /// </summary>
        public static IGeometryAttributes TriangulateQuadMesh(this IGeometryAttributes g)
        {
            if (g.NumCornersPerFace != 4) throw new Exception("Not a quad mesh");

            var cornerRemap = new int[g.NumFaces * 6];
            var faceRemap = new int[g.NumFaces * 2];
            var cur = 0;
            for (var i = 0; i < g.NumFaces; ++i)
            {
                cornerRemap[cur++] = i * 4 + 0;
                cornerRemap[cur++] = i * 4 + 1;
                cornerRemap[cur++] = i * 4 + 2;
                cornerRemap[cur++] = i * 4 + 0;
                cornerRemap[cur++] = i * 4 + 2;
                cornerRemap[cur++] = i * 4 + 3;

                faceRemap[i * 2 + 0] = i;
                faceRemap[i * 2 + 1] = i;
            }

            return g.RemapFacesAndCorners(faceRemap.ToIArray(), cornerRemap.ToIArray(), 3);
        }

        // A public static method named CopyFaces with a type Vim.G3d.IGeometryAttributes
        // operation kind is Block and type 
        // member references = this[]
        // assignments = 
        // Written symbols are (Name=i Kind=Parameter)
        // Read symbols are (Name=g Kind=Parameter), (Name=keep Kind=Parameter), (Name=i Kind=Parameter)
        // Captured symbols are (Name=keep Kind=Parameter)
        // Variables declared are (Name=i Kind=Parameter)
        
        public static IGeometryAttributes CopyFaces(this IGeometryAttributes g, IArray<bool> keep)
        {
            return g.CopyFaces(i => keep[i]);
        }

        // A public static method named CopyFaces with a type Vim.G3d.IGeometryAttributes
        // operation kind is Block and type 
        // member references = 
        // assignments = 
        // Written symbols are 
        // Read symbols are (Name=g Kind=Parameter), (Name=keep Kind=Parameter)
        // Captured symbols are 
        // Variables declared are 
        
        public static IGeometryAttributes CopyFaces(this IGeometryAttributes g, IArray<int> keep)
        {
            return g.RemapFaces(keep);
        }

        // A public static method named CopyFaces with a type Vim.G3d.IGeometryAttributes
        // operation kind is Block and type 
        // member references = NumFaces
        // assignments = 
        // Written symbols are (Name=i Kind=Parameter)
        // Read symbols are (Name=self Kind=Parameter), (Name=predicate Kind=Parameter), (Name=i Kind=Parameter)
        // Captured symbols are 
        // Variables declared are (Name=i Kind=Parameter)
        
        public static IGeometryAttributes CopyFaces(this IGeometryAttributes self, Func<int, bool> predicate)
        {
            return self.RemapFaces(self.NumFaces.Select(i => i).IndicesWhere(predicate).ToIArray());
        }

        // A public static method named DeleteFaces with a type Vim.G3d.IGeometryAttributes
        // operation kind is Block and type 
        // member references = 
        // assignments = 
        // Written symbols are (Name=i Kind=Parameter)
        // Read symbols are (Name=g Kind=Parameter), (Name=predicate Kind=Parameter), (Name=i Kind=Parameter)
        // Captured symbols are (Name=predicate Kind=Parameter)
        // Variables declared are (Name=i Kind=Parameter)
        
        public static IGeometryAttributes DeleteFaces(this IGeometryAttributes g, Func<int, bool> predicate)
        {
            return g.CopyFaces(i => !predicate(i));
        }

        // A public static method named CopyFaces with a type Vim.G3d.IGeometryAttributes
        // operation kind is Block and type 
        // member references = 
        // assignments = 
        // Written symbols are (Name=i Kind=Parameter)
        // Read symbols are (Name=g Kind=Parameter), (Name=from Kind=Parameter), (Name=count Kind=Parameter), (Name=i Kind=Parameter)
        // Captured symbols are (Name=from Kind=Parameter), (Name=count Kind=Parameter)
        // Variables declared are (Name=i Kind=Parameter)
        
        public static IGeometryAttributes CopyFaces(this IGeometryAttributes g, int from, int count)
        {
            return g.CopyFaces(i => i >= from && i < from + count);
        }

        // A public static method named RemapVertices with a type Vim.G3d.IGeometryAttributes
        // operation kind is Block and type 
        // member references = 
        // assignments = 
        // Written symbols are (Name=attr Kind=Parameter)
        // Read symbols are (Name=g Kind=Parameter), (Name=newVertices Kind=Parameter), (Name=newIndices Kind=Parameter), (Name=attr Kind=Parameter)
        // Captured symbols are (Name=newVertices Kind=Parameter)
        // Variables declared are (Name=attr Kind=Parameter)
        
        /// <summary>
        /// Updates the vertex buffer (e.g. after identifying unwanted faces) and the index
        /// buffer. Vertices are either re-ordered, removed, or deleted. Does not affect any other
        /// </summary>
        public static IGeometryAttributes RemapVertices(this IGeometryAttributes g, IArray<int> newVertices,
            IArray<int> newIndices)
        {
            return new[] { newIndices.ToIndexAttribute() }
                .Concat(
                    g.VertexAttributes()
                        .Select(attr => attr.Remap(newVertices)))
                .Concat(g.NoneAttributes())
                .Concat(g.FaceAttributes())
                .Concat(g.EdgeAttributes())
                .Concat(g.CornerAttributes())
                .Concat(g.WholeGeometryAttributes())
                .ToGeometryAttributes();
        }

        // A public static method named RemapVertices with a type Vim.G3d.IGeometryAttributes
        // operation kind is Block and type 
        // member references = NumVertices, Count, this[], Data, NumVertices
        // assignments = LocalReference
        // Written symbols are (Name=vertLookup Kind=Local), (Name=i Kind=Local), (Name=oldVert Kind=Local), (Name=oldIndices Kind=Local), (Name=newIndices Kind=Local), (Name=i Kind=Parameter), (Name=x Kind=Parameter)
        // Read symbols are (Name=g Kind=Parameter), (Name=vertRemap Kind=Parameter), (Name=vertLookup Kind=Local), (Name=i Kind=Local), (Name=oldVert Kind=Local), (Name=oldIndices Kind=Local), (Name=newIndices Kind=Local), (Name=i Kind=Parameter), (Name=x Kind=Parameter)
        // Captured symbols are (Name=vertLookup Kind=Local)
        // Variables declared are (Name=vertLookup Kind=Local), (Name=i Kind=Local), (Name=oldVert Kind=Local), (Name=oldIndices Kind=Local), (Name=newIndices Kind=Local), (Name=i Kind=Parameter), (Name=x Kind=Parameter)
        
        /// <summary>
        /// The vertRemap is a list of vertices in the new vertex buffer, and where they came from.
        /// This could be a reordering of the original vertex buffer, it could even be a repetition.
        /// It could also be some vertices were deleted, BUT if those vertices are still referenced
        /// then this will throw an exception.
        /// The values in the index buffer will change, but it will stay the same length.
        /// </summary>
        public static IGeometryAttributes RemapVertices(this IGeometryAttributes g, IArray<int> vertRemap)
        {
            var vertLookup = (-1).Repeat(g.NumVertices).ToArray();
            for (var i = 0; i < vertRemap.Count; ++i)
            {
                var oldVert = vertRemap[i];
                vertLookup[oldVert] = i;
            }

            var oldIndices = g.GetAttributeIndex()?.Data ?? g.NumVertices.Range();
            var newIndices = oldIndices.Select(i => vertLookup[i]).Evaluate();

            if (newIndices.Any(x => x == -1))
                throw new Exception("At least one of the indices references a vertex that no longer exists");

            return g.RemapVertices(vertRemap, newIndices);
        }

        // A public static method named SelectFaces with a type Vim.G3d.IGeometryAttributes
        // operation kind is Block and type 
        // member references = NumFaces, Count, Count, Count, NumCornersPerFace, Count, this[], Data, this[], NumCornersPerFace, Length, ElementCount, ElementCount, Length
        // assignments = PropertyReference, LocalReference, SimpleAssignment, Increment, LocalReference
        // Written symbols are (Name=oldIndices Kind=Local), (Name=oldSelIndices Kind=Local), (Name=i Kind=Local), (Name=t Kind=Local), (Name=newIndices Kind=Local), (Name=indexLookup Kind=Local), (Name=numUsedVertices Kind=Local), (Name=oldVertices Kind=Local), (Name=usedVertices Kind=Local), (Name=i Kind=Local), (Name=oldIndex Kind=Local), (Name=newIndex Kind=Local), (Name=vertRemapping Kind=Local), (Name=cornerRemapping Kind=Local), (Name=attr Kind=Parameter), (Name=attr Kind=Parameter), (Name=attr Kind=Parameter), (Name=attr Kind=Parameter)
        // Read symbols are (Name=g Kind=Parameter), (Name=faces Kind=Parameter), (Name=oldIndices Kind=Local), (Name=oldSelIndices Kind=Local), (Name=i Kind=Local), (Name=t Kind=Local), (Name=newIndices Kind=Local), (Name=indexLookup Kind=Local), (Name=numUsedVertices Kind=Local), (Name=oldVertices Kind=Local), (Name=usedVertices Kind=Local), (Name=i Kind=Local), (Name=oldIndex Kind=Local), (Name=newIndex Kind=Local), (Name=vertRemapping Kind=Local), (Name=cornerRemapping Kind=Local), (Name=attr Kind=Parameter), (Name=attr Kind=Parameter), (Name=attr Kind=Parameter), (Name=attr Kind=Parameter)
        // Captured symbols are (Name=faces Kind=Parameter), (Name=vertRemapping Kind=Local), (Name=cornerRemapping Kind=Local)
        // Variables declared are (Name=oldIndices Kind=Local), (Name=oldSelIndices Kind=Local), (Name=i Kind=Local), (Name=t Kind=Local), (Name=newIndices Kind=Local), (Name=indexLookup Kind=Local), (Name=numUsedVertices Kind=Local), (Name=oldVertices Kind=Local), (Name=usedVertices Kind=Local), (Name=i Kind=Local), (Name=oldIndex Kind=Local), (Name=newIndex Kind=Local), (Name=vertRemapping Kind=Local), (Name=cornerRemapping Kind=Local), (Name=attr Kind=Parameter), (Name=attr Kind=Parameter), (Name=attr Kind=Parameter), (Name=attr Kind=Parameter)
        
        /// <summary>
        /// For mesh g, create a new mesh from the passed selected faces,
        /// discarding un-referenced data and generating new index, vertex & face buffers.
        /// </summary>
        public static IGeometryAttributes SelectFaces(this IGeometryAttributes g, IArray<int> faces)
        {
            // Early exit, if all selected no need to do anything
            if (g.NumFaces == faces.Count)
                return g;

            // Early exit, if none selected no need to do anything
            if (faces.Count == 0)
                return null;

            // First, get all the indices for this array of faces
            var oldIndices = g.GetAttributeIndex();
            var oldSelIndices = new int[faces.Count * g.NumCornersPerFace];
            // var oldIndices = faces.SelectMany(f => f.Indices());
            for (var i = 0; i < faces.Count; i++)
            for (var t = 0; t < 3; t++)
                oldSelIndices[i * 3 + t] = oldIndices.Data[faces[i] * g.NumCornersPerFace + t];

            // We need to create list of newIndices, and remapping
            // of oldVertices to newVertices
            var newIndices = (-1).Repeat(oldSelIndices.Length).ToArray();
            // Each index could potentially be in the new mesh
            var indexLookup = (-1).Repeat(oldIndices.ElementCount).ToArray();

            // Build mapping.  For each index, if the vertex it has
            // already been referred to has been mapped, use the
            // mapped index.  Otherwise, remember that we need this vertex
            var numUsedVertices = 0;
            // remapping from old vert array => new vert array
            // Cache built of the old indices of vertices that are used
            // should be equivalent to indexLookup.Where(i => i != -1)
            var oldVertices = g.GetAttributePosition();
            var usedVertices = (-1).Repeat(oldVertices.ElementCount).ToArray();
            for (var i = 0; i < oldSelIndices.Length; ++i)
            {
                var oldIndex = oldSelIndices[i];
                var newIndex = indexLookup[oldIndex];
                if (newIndex < 0)
                {
                    // remapping from old vert array => new vert array
                    usedVertices[numUsedVertices] = oldIndex;
                    newIndex = indexLookup[oldIndex] = numUsedVertices++;
                }

                newIndices[i] = newIndex;
            }

            //var faceRemapping = faces.Select(f => f.Index);
            var vertRemapping = usedVertices.Take(numUsedVertices).ToIArray();
            var cornerRemapping = g.FaceIndicesToCornerIndices(faces);

            return g.VertexAttributes()
                .Select(attr => attr.Remap(vertRemapping))
                .Concat(g.NoneAttributes())
                .Concat(g.FaceAttributes().Select(attr => attr.Remap(faces)))
                .Concat(g.EdgeAttributes().Select(attr => attr.Remap(cornerRemapping)))
                .Concat(g.CornerAttributes().Select(attr => attr.Remap(cornerRemapping)))
                .Concat(g.WholeGeometryAttributes())
                .ToGeometryAttributes()
                .SetAttribute(newIndices.ToIndexAttribute());
        }

        // A public static method named ToG3d with a type Vim.G3d.G3D
        // operation kind is Block and type 
        // member references = 
        // assignments = 
        // Written symbols are 
        // Read symbols are (Name=attributes Kind=Parameter), (Name=header Kind=Parameter)
        // Captured symbols are 
        // Variables declared are 
        
        public static G3D ToG3d(this IEnumerable<GeometryAttribute> attributes, G3dHeader? header = null)
        {
            return new G3D(attributes, header);
        }

        // A public static method named ToG3d with a type Vim.G3d.G3D
        // operation kind is Block and type 
        // member references = 
        // assignments = 
        // Written symbols are 
        // Read symbols are (Name=attributes Kind=Parameter), (Name=header Kind=Parameter)
        // Captured symbols are 
        // Variables declared are 
        
        public static G3D ToG3d(this IArray<GeometryAttribute> attributes, G3dHeader? header = null)
        {
            return attributes.ToEnumerable().ToG3d(header);
        }

        // A public static method named IndexFlippedRemapping with a type Vim.LinqArray.IArray<int>
        // operation kind is Block and type 
        // member references = NumCorners, NumCornersPerFace, NumCornersPerFace, NumCornersPerFace
        // assignments = 
        // Written symbols are (Name=c Kind=Parameter)
        // Read symbols are (Name=g Kind=Parameter), (Name=c Kind=Parameter)
        // Captured symbols are (Name=g Kind=Parameter)
        // Variables declared are (Name=c Kind=Parameter)
        
        public static IArray<int> IndexFlippedRemapping(this IGeometryAttributes g)
        {
            return g.NumCorners.Select(c =>
                (c / g.NumCornersPerFace + 1) * g.NumCornersPerFace - 1 - c % g.NumCornersPerFace);
        }

        // A public static method named IsNormalAttribute with a type bool
        // operation kind is Block and type 
        // member references = Semantic, Descriptor
        // assignments = 
        // Written symbols are 
        // Read symbols are (Name=attr Kind=Parameter)
        // Captured symbols are 
        // Variables declared are 
        
        public static bool IsNormalAttribute(this GeometryAttribute attr)
        {
            return attr.IsType<Vector3>() && attr.Descriptor.Semantic == "normal";
        }

        // A public static method named FlipNormalAttributes with a type System.Collections.Generic.IEnumerable<Vim.G3d.GeometryAttribute>
        // operation kind is Block and type 
        // member references = Data, X, Y, Z, Descriptor
        // assignments = 
        // Written symbols are (Name=attr Kind=Parameter), (Name=v Kind=Parameter)
        // Read symbols are (Name=self Kind=Parameter), (Name=attr Kind=Parameter), (Name=v Kind=Parameter)
        // Captured symbols are 
        // Variables declared are (Name=attr Kind=Parameter), (Name=v Kind=Parameter)
        
        public static IEnumerable<GeometryAttribute> FlipNormalAttributes(this IEnumerable<GeometryAttribute> self)
        {
            return self.Select(attr => attr.IsNormalAttribute()
                ? attr.AsType<Vector3>().Data.Select(v => new Vector3(1f / v.X, 1f / v.Y, 1f / v.Z)).ToAttribute(attr.Descriptor)
                : attr);
        }

        // A public static method named FlipWindingOrder with a type Vim.G3d.IGeometryAttributes
        // operation kind is Block and type 
        // member references = 
        // assignments = 
        // Written symbols are (Name=attr Kind=Parameter), (Name=attr Kind=Parameter)
        // Read symbols are (Name=g Kind=Parameter), (Name=attr Kind=Parameter), (Name=attr Kind=Parameter)
        // Captured symbols are (Name=g Kind=Parameter)
        // Variables declared are (Name=attr Kind=Parameter), (Name=attr Kind=Parameter)
        
        public static IGeometryAttributes FlipWindingOrder(this IGeometryAttributes g)
        {
            return g.VertexAttributes()
                .Concat(g.NoneAttributes())
                .Concat(g.FaceAttributes())
                .Concat(g.EdgeAttributes().Select(attr => attr.Remap(g.IndexFlippedRemapping())))
                .Concat(g.CornerAttributes().Select(attr => attr.Remap(g.IndexFlippedRemapping())))
                .Concat(g.WholeGeometryAttributes())
                .FlipNormalAttributes()
                .ToGeometryAttributes();
        }

        // A public static method named DoubleSided with a type Vim.G3d.IGeometryAttributes
        // operation kind is Block and type 
        // member references = 
        // assignments = 
        // Written symbols are 
        // Read symbols are (Name=g Kind=Parameter)
        // Captured symbols are 
        // Variables declared are 
        
        public static IGeometryAttributes DoubleSided(this IGeometryAttributes g)
        {
            return g.Merge(g.FlipWindingOrder());
        }

        // A public static method named DefaultMaterials with a type Vim.LinqArray.IArray<int>
        // operation kind is Block and type 
        // member references = NumFaces
        // assignments = 
        // Written symbols are 
        // Read symbols are (Name=self Kind=Parameter)
        // Captured symbols are 
        // Variables declared are 
        
        public static IArray<int> DefaultMaterials(this IGeometryAttributes self)
        {
            return (-1).Repeat(self.NumFaces);
        }

        // A public static method named DefaultColors with a type Vim.LinqArray.IArray<Vim.Math3d.Vector4>
        // operation kind is Block and type 
        // member references = NumVertices
        // assignments = 
        // Written symbols are 
        // Read symbols are (Name=self Kind=Parameter)
        // Captured symbols are 
        // Variables declared are 
        
        public static IArray<Vector4> DefaultColors(this IGeometryAttributes self)
        {
            return (new Vector4(0,0,0,0)).Repeat(self.NumVertices);
        }

        // A public static method named DefaultUvs with a type Vim.LinqArray.IArray<Vim.Math3d.Vector2>
        // operation kind is Block and type 
        // member references = NumVertices
        // assignments = 
        // Written symbols are 
        // Read symbols are (Name=self Kind=Parameter)
        // Captured symbols are 
        // Variables declared are 
        
        public static IArray<Vector2> DefaultUvs(this IGeometryAttributes self)
        {
            return (new Vector2(0, 0)).Repeat(self.NumVertices);
        }

        // A public static method named Replace with a type Vim.G3d.IGeometryAttributes
        // operation kind is Block and type 
        // member references = Attributes, Descriptor
        // assignments = 
        // Written symbols are (Name=a Kind=Parameter)
        // Read symbols are (Name=self Kind=Parameter), (Name=selector Kind=Parameter), (Name=attribute Kind=Parameter), (Name=a Kind=Parameter)
        // Captured symbols are (Name=selector Kind=Parameter)
        // Variables declared are (Name=a Kind=Parameter)
        
        public static IGeometryAttributes Replace(this IGeometryAttributes self,
            Func<AttributeDescriptor, bool> selector, GeometryAttribute attribute)
        {
            return self.Attributes.Where(a => !selector(a.Descriptor)).Append(attribute).ToGeometryAttributes();
        }

        // A public static method named Remove with a type Vim.G3d.IGeometryAttributes
        // operation kind is Block and type 
        // member references = Attributes, Descriptor
        // assignments = 
        // Written symbols are (Name=a Kind=Parameter)
        // Read symbols are (Name=self Kind=Parameter), (Name=selector Kind=Parameter), (Name=a Kind=Parameter)
        // Captured symbols are (Name=selector Kind=Parameter)
        // Variables declared are (Name=a Kind=Parameter)
        
        public static IGeometryAttributes Remove(this IGeometryAttributes self,
            Func<AttributeDescriptor, bool> selector)
        {
            return self.Attributes.Where(a => !selector(a.Descriptor)).ToGeometryAttributes();
        }

        // A public static method named ToG3d with a type Vim.G3d.G3D
        // operation kind is Block and type 
        // member references = Attributes
        // assignments = 
        // Written symbols are 
        // Read symbols are (Name=self Kind=Parameter)
        // Captured symbols are 
        // Variables declared are 
        
        public static G3D ToG3d(this IGeometryAttributes self)
        {
            return G3D.Create(self.Attributes.ToArray());
        }

    } // type
} // namespace
