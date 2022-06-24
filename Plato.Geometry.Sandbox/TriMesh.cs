using System.Collections.Generic;
using System.Diagnostics;
using Vim.G3d;
using Vim.Math3d;

namespace Vim.Geometry
{
    /// <summary>
    /// A triangular mesh data structure.
    /// </summary>
    public class TriMesh : G3D, IMesh
    {
        public TriMesh(IEnumerable<GeometryAttribute> attributes)
            : base(attributes)
            => Debug.Assert(NumCornersPerFace == 3);

        public IMesh Transform(Matrix4x4 mat)
            => ((IGeometryAttributes)this).Transform(mat).ToIMesh();
    }
}
