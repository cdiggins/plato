using Vim.LinqArray;

namespace Vim.G3d
{
    // Type has fields False
    // Type has writable fields False
    // Type has public setters False
    public interface IGeometryAttributes
    {
        // A private instance method named GetAttribute with a type Vim.G3d.GeometryAttribute
        // No associated operation
        // No data-flow analysis could be created
                GeometryAttribute GetAttribute(string name);

        // A private instance property named NumCornersPerFace with a type int
        // No associated operation
        // No data-flow analysis could be created
                int NumCornersPerFace { get; }

        // A private instance property named NumVertices with a type int
        // No associated operation
        // No data-flow analysis could be created
                int NumVertices { get; }

        // A private instance property named NumCorners with a type int
        // No associated operation
        // No data-flow analysis could be created
                int NumCorners { get; }

        // A private instance property named NumFaces with a type int
        // No associated operation
        // No data-flow analysis could be created
                int NumFaces { get; }

        // A private instance property named NumInstances with a type int
        // No associated operation
        // No data-flow analysis could be created
                int NumInstances { get; }

        // A private instance property named NumMeshes with a type int
        // No associated operation
        // No data-flow analysis could be created
                int NumMeshes { get; }

        // A private instance property named NumShapeVertices with a type int
        // No associated operation
        // No data-flow analysis could be created
                int NumShapeVertices { get; }

        // A private instance property named NumShapes with a type int
        // No associated operation
        // No data-flow analysis could be created
                int NumShapes { get; }

        // A private instance property named Attributes with a type Vim.LinqArray.IArray<Vim.G3d.GeometryAttribute>
        // No associated operation
        // No data-flow analysis could be created
        
        IArray<GeometryAttribute> Attributes { get; }

    } // type
} // namespace
