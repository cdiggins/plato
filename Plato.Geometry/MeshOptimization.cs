using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using Vim.DotNetUtilities;
using Vim.G3d;
using Vim.LinqArray;
using Vim.Math3d;

namespace Vim.Geometry
{
    /// <summary>
    /// This class is used to compare quickly two meshes within a lookup table (e.g. Dictionary, HashTable).
    /// it looks at the positions of each corner, and the number of faces, and assures that the object ID 
    /// and material IDs are the same. 
    /// When using a class within a dictionary or hash table, the equals operator is called frequently.
    /// By converting an IMesh to a MeshHash we minimize the amount of comparisons done. It becomes 
    /// possible, but highly unlikely that two different meshes would have the same hash.
    /// </summary>
    public class MeshHash
    {
        public IMesh Mesh;
        public float Tolerance;
        public int NumFaces;
        public int NumVertices;
        public int TopologyHash;
        public Int3 BoxExtents;
        public Int3 BoxMin;

        public int Round(float f)
            => (int)(f / Tolerance);

        public Int3 Round(Vector3 v)
            => new Int3(Round(v.X), Round(v.Y), Round(v.Z));

        public MeshHash(IMesh mesh, float tolerance)
        {
            Mesh = mesh;
            Tolerance = tolerance;
            NumFaces = mesh.NumFaces;
            NumVertices = mesh.NumVertices;
            TopologyHash = Hash.Combine(mesh.Indices.ToArray());
            var box = mesh.BoundingBox();
            BoxMin = Round(box.Min);
            BoxExtents = Round(box.Extent);
        }

        public override bool Equals(object obj)
            => obj is MeshHash other && Equals(other);

        public bool Equals(MeshHash other)
            => NumFaces == other.NumFaces
            && NumVertices == other.NumVertices
            && BoxMin.Equals(other.BoxMin)
            && BoxExtents.Equals(other.BoxExtents)
            && Mesh.GeometryEquals(other.Mesh);

        public override int GetHashCode()
            => Hash.Combine(NumFaces, NumVertices, TopologyHash, BoxMin.GetHashCode(), BoxExtents.GetHashCode());
    }

    public class IntegerPositionColorNormal
    {
        public Int3 Position;
        public Int3 Color;
        public Int3 Normal;

        public IntegerPositionColorNormal(Int3 pos, Int3 color, Int3 normal)
            => (Position, Color, Normal) = (pos, color, normal);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode()
            => Hash.Combine(Position.GetHashCode(), Color.GetHashCode(), Normal.GetHashCode());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object obj)
            => Equals(obj as IntegerPositionColorNormal);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(IntegerPositionColorNormal other)
            => other != null && Position.Equals(other.Position) && Color.Equals(other.Color) && Normal.Equals(other.Normal);
    }

    public static class Optimization
    {
        public static Dictionary<MeshHash, List<IMesh>> GroupMeshesByHash(this IArray<IMesh> meshes, float tolerance)
            => meshes.ToEnumerable().GroupMeshesByHash(tolerance);

        public static Dictionary<MeshHash, List<IMesh>> GroupMeshesByHash(this IEnumerable<IMesh> meshes, float tolerance)
            => meshes.AsParallel().GroupBy(m => new MeshHash(m, tolerance)).ToDictionary(grp => grp.Key, grp => grp.ToList());

        /// <summary>
        /// Merges vertices that are within a certain distance and have similar normals and colors.        
        /// </summary>
        public static IMesh WeldVertices(this IMesh g, float threshold = (float)Constants.MmToFeet)
        {
            var positions = g.Vertices;
            var normals = g.GetOrComputeVertexNormals().ToArray();
            var colors = g.VertexColors ?? Vector4.Zero.Repeat(positions.Count);

            // Vertex indices by color, and then by normal 
            var d = new Dictionary<IntegerPositionColorNormal, int>();

            // The mapping of old indices to new ones
            var indexRemap = new int[g.Vertices.Count];

            // This is a list of vertex indices that we are keeping 
            var vertRemap = new List<int>();

            // Local helper function 
            Int3 ToInt3(Vector3 v)
                => new Int3((int)v.X, (int)v.Y, (int)v.Z);

            for (var i = 0; i < g.NumVertices; ++i)
            {
                var p = ToInt3(positions[i] * (1 / threshold));
                var c = ToInt3(colors[i].ToVector3() * 10000);
                var n = ToInt3(normals[i] * 10000);

                var pcn = new IntegerPositionColorNormal(p, c, n);

                if (d.TryGetValue(pcn, out var index))
                {
                    indexRemap[i] = index;
                    continue;
                }

                var newVertIndex = vertRemap.Count;
                indexRemap[i] = newVertIndex;
                vertRemap.Add(i);

                d.Add(pcn, newVertIndex);
            }

            Debug.Assert(vertRemap.Count <= g.NumVertices);
            for (var i = 1; i < vertRemap.Count; ++i)
                Debug.Assert(vertRemap[i - 1] < vertRemap[i]);

            return g.RemapVertices(
                vertRemap.ToIArray(),
                g.Indices.Select(i => indexRemap[i]))
                .ToIMesh();
        }
    }
}
