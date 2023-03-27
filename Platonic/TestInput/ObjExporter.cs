using System.Collections.Generic;
using System.IO;
using System.Text;
using Vim.LinqArray;

namespace Vim.G3d
{
    /// <summary>
    /// This is a simple ObjExporter for the purposes of testing.
    /// </summary>
    public static class ObjExporter
    {
        public static IEnumerable<string> ObjLines(G3D g3d)
        {
            // Write the vertices 
            var vertices = g3d.Vertices;
            var uvs = g3d.VertexUvs;
            foreach (var v in vertices.ToEnumerable())
                yield return $"v {v.X} {v.Y} {v.Z}";
            if (uvs != null)
                for (var v = 0; v < uvs.Count; v++)
                    yield return $"vt {uvs[v].X} {uvs[v].Y}";

            var indices = g3d.Indices;
            var sb = new StringBuilder();
            var i = 0;
            var faceSize = g3d.NumCornersPerFace;
            while (i < indices.Count)
            {
                sb.Append("f");

                if (uvs == null)
                    for (var j = 0; j < faceSize; ++j)
                    {
                        var index = g3d.Indices[i++] + 1;
                        sb.Append(" ").Append(index);
                    }
                else
                    for (var j = 0; j < faceSize; ++j)
                    {
                        var index = g3d.Indices[i++] + 1;
                        sb.Append(" ").Append(index).Append("/").Append(index);
                    }

                yield return sb.ToString();
                sb.Clear();
            }
        }

        public static void WriteObj(this G3D g3d, string filePath)
        {
            File.WriteAllLines(filePath, ObjLines(g3d));
        }
    }
}