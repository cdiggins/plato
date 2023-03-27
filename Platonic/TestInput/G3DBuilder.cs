using System.Collections.Generic;
using Vim.LinqArray;
using Vim.Math3d;

namespace Vim.G3d
{
    /// <summary>
    /// This is a helper class for constructing a G3D from individual attributes
    /// </summary>
    public class G3DBuilder
    {
        public readonly List<GeometryAttribute> Attributes = new List<GeometryAttribute>();

        public G3D ToG3D(G3dHeader? header = null)
        {
            return new G3D(Attributes, header ?? G3dHeader.Default);
        }

        public G3DBuilder Add(GeometryAttribute attr)
        {
            Attributes.Add(attr);
            return this;
        }

        public G3DBuilder AddIndices(int[] indices)
        {
            return Add(indices.ToIndexAttribute());
        }

        public G3DBuilder AddIndices(IArray<int> indices)
        {
            return Add(indices.ToIndexAttribute());
        }

        public G3DBuilder SetObjectFaceSize(int objectFaceSize)
        {
            return Add(new[] { objectFaceSize }.ToIArray().ToObjectFaceSizeAttribute());
        }

        public G3DBuilder AddVertices(IArray<Vector3> vertices)
        {
            return Add(vertices.ToPositionAttribute());
        }

        public IGeometryAttributes ToIGeometryAttributes()
        {
            return new GeometryAttributes(Attributes);
        }
    }
}