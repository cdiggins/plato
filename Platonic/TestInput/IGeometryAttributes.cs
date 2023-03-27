using Vim.LinqArray;

namespace Vim.G3d
{
    /// <summary>
    /// This is a read-only collection of G3D attributes. 
    /// </summary>
    public interface IGeometryAttributes
    {
        int NumCornersPerFace { get; }
        int NumVertices { get; }
        int NumCorners { get; }
        int NumFaces { get; }
        int NumInstances { get; }
        int NumMeshes { get; }
        int NumShapeVertices { get; }
        int NumShapes { get; }

        IArray<GeometryAttribute> Attributes { get; }
        GeometryAttribute GetAttribute(string name);
    }
}