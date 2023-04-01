using System.Collections.Generic;

using System.IO;

using System.Text;

using Vim.LinqArray;

namespace Vim.G3d
{
    // Type has fields False
    // Type has writable fields False
    // Type has public setters False
    public static class ObjExporter
    {
        // A public static method named ObjLines with a type System.Collections.Generic.IEnumerable<string>
        // operation kind is Block and type 
        // member references = Vertices, VertexUvs, X, Y, Z, Count, X, this[], Y, this[], Indices, NumCornersPerFace, Count, this[], Indices, this[], Indices
        // assignments = 
        // Written symbols are (Name=vertices Kind=Local), (Name=uvs Kind=Local), (Name=v Kind=Local), (Name=v Kind=Local), (Name=indices Kind=Local), (Name=sb Kind=Local), (Name=i Kind=Local), (Name=faceSize Kind=Local), (Name=j Kind=Local), (Name=index Kind=Local), (Name=j Kind=Local), (Name=index Kind=Local)
        // Read symbols are (Name=g3d Kind=Parameter), (Name=vertices Kind=Local), (Name=uvs Kind=Local), (Name=v Kind=Local), (Name=v Kind=Local), (Name=indices Kind=Local), (Name=sb Kind=Local), (Name=i Kind=Local), (Name=faceSize Kind=Local), (Name=j Kind=Local), (Name=index Kind=Local), (Name=j Kind=Local), (Name=index Kind=Local)
        // Captured symbols are 
        // Variables declared are (Name=vertices Kind=Local), (Name=uvs Kind=Local), (Name=v Kind=Local), (Name=v Kind=Local), (Name=indices Kind=Local), (Name=sb Kind=Local), (Name=i Kind=Local), (Name=faceSize Kind=Local), (Name=j Kind=Local), (Name=index Kind=Local), (Name=j Kind=Local), (Name=index Kind=Local)
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

        // A public static method named WriteObj with a type void
        // operation kind is Block and type 
        // member references = 
        // assignments = 
        // Written symbols are 
        // Read symbols are (Name=g3d Kind=Parameter), (Name=filePath Kind=Parameter)
        // Captured symbols are 
        // Variables declared are 
        
        public static void WriteObj(this G3D g3d, string filePath)
        {
            File.WriteAllLines(filePath, ObjLines(g3d));
        }

    } // type
} // namespace
