using System;
using System.Collections.Generic;
using System.Text;
using Vim.LinqArray;

namespace Vim.Geometry
{
    public static class Validation
    {
        public static void ValidateIndices(this IMesh mesh)
        {
            foreach (var index in mesh.Indices.ToEnumerable())
            {
                if (index < 0 || index >= mesh.NumVertices)
                    throw new Exception($"Invalid mesh index: {index}. Expected a value greater or equal to 0 and less than {mesh.NumVertices}");
            }
        }

        public static void Validate(this IMesh mesh)
        {
            mesh.ValidateIndices();
        }
    }
}
