using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Vim.G3d;
using Vim.LinqArray;
using Vim.Math3d;

namespace Vim.Geometry
{
    /// <summary>
    /// The base class of topology face, topology edge, topology vertex, and topology element
    /// </summary>
    public class TopoElement
    {
        public TopoElement(Topology topology, int index)
            => (Topology, Index) = (topology, index);
        public Topology Topology { get; }
        public int Index { get; }
    }

    /// <summary>
    /// A topological face. 
    /// </summary>
    public class TopoFace : TopoElement
    {
        public TopoFace(Topology topology, int index)
            : base(topology, index)
            => Debug.Assert(index >= 0 && index < topology.Mesh.NumFaces);
    }

    /// <summary>
    /// A directed edge around a polygon (aka a half-edge). There is exactly one half-edge per "corner" in a mesh.
    /// A non-border edge in a manifold mesh has exactly two half-edges, and a border edge has one edge.  
    /// </summary>
    public class TopoEdge : TopoElement
    {
        public TopoEdge(Topology topology, int index)
            : base(topology, index)
            => Debug.Assert(index >= 0 && index < topology.Mesh.NumCorners);
    }

    /// <summary>
    /// A vertex in the mesh. 
    /// </summary>
    public class TopoVertex : TopoElement
    {
        public TopoVertex(Topology topology, int index)
            : base(topology, index)
            => Debug.Assert(index >= 0 && index < topology.Mesh.NumVertices);
    }

    /// <summary>
    /// Also called a "face-corner". Associated with exactly one face, and one vertex.
    /// A vertex may be associated with multiple corners 
    /// </summary>
    public class TopoCorner : TopoElement
    {
        public TopoCorner(Topology topology, int index)
            : base(topology, index)
            => Debug.Assert(index >= 0 && index < topology.Mesh.NumCorners);
    }

    /// <summary>
    /// This class is used to make efficient topological queries for an IGeometry.
    /// Construction is a O(N) operation, so it is not created automatically. 
    /// </summary>
    public class Topology
    {
        public TopoFace Face(int f)
            => new TopoFace(this, f);

        public TopoEdge Edge(int e)
            => new TopoEdge(this, e);

        public Topology(IMesh m)
        {
            Mesh = m;
            Corners = Mesh.Indices.Indices();
            Faces = Mesh.NumFaces.Range();
            Vertices = Mesh.Vertices.Indices();

            // Compute the mapping from vertex indices to faces that reference them 
            VerticesToFaces = new List<int>[m.Vertices.Count];
            for (var c = 0; c < m.Indices.Count; ++c)
            {
                var v = m.Indices[c];
                var f = m.CornerToFace(c);

                Debug.Assert(f.Within(0, m.NumFaces));
                Debug.Assert(v.Within(0, m.NumVertices));
                Debug.Assert(c.Within(0, m.NumCorners));

                if (VerticesToFaces[v] == null)
                    VerticesToFaces[v] = new List<int> { f };
                else
                    VerticesToFaces[v].Add(f);
            }

            // NOTE: the same edge can occur in more than two faces, only in non-manifold meshes

            // Compute the face on the other side of an edge 
            EdgeToOtherFace = (-1).Repeat(Mesh.NumCorners).ToArray();
            for (var c = 0; c < Mesh.NumCorners; ++c)
            {
                var c2 = NextCorner(c);
                var f0 = CornerToFace(c);
                foreach (var f1 in FacesFromCorner(c).ToEnumerable())
                {
                    if (f1 != f0)
                    {
                        foreach (var f2 in FacesFromCorner(c2).ToEnumerable())
                        {
                            if (f2 == f1)
                            {
                                if (EdgeToOtherFace[c] != -1)
                                    NonManifold = true;
                                EdgeToOtherFace[c] = f2;
                            }
                        }
                    }
                }
            }

            // TODO: there is some serious validation I coudl be doing doing here.
        }

        public IMesh Mesh { get; }

        public List<int>[] VerticesToFaces { get; }
        public int[] EdgeToOtherFace { get; } // Assumes manifold meshes
        public bool NonManifold { get; }
        public IArray<int> Corners { get; }
        public IArray<int> Vertices { get; }
        public IArray<int> Edges => Corners;
        public IArray<int> Faces { get; }

        public int CornerToFace(int i)
            => Mesh.CornerToFace(i);

        public IArray<int> FacesFromVertexIndex(int v)
            => VerticesToFaces[v]?.ToIArray() ?? 0.Repeat(0);

        public IArray<int> FacesFromCorner(int c)
            => FacesFromVertexIndex(Mesh.Indices[c]);

        public int VertexIndexFromCorner(int c)
            => Mesh.Indices[c];

        /// <summary>
        /// Differs from neighbour faces in that the faces have to share an edge, not just a vertex.
        /// An alternative construction would have been to getNeighbourFaces and filter out those that don't share
        /// </summary>
        public IEnumerable<int> BorderingFacesFromFace(int f)
            => EdgesFromFace(f).Select(BorderFace).Where(bf => bf >= 0);

        public int BorderFace(int e)
            => EdgeToOtherFace[e];

        public bool IsBorderEdge(int e)
            => EdgeToOtherFace[e] < 0;

        public bool IsBorderFace(int f)
            => EdgesFromFace(f).Any(IsBorderEdge);

        public IArray<int> CornersFromFace(int f)
            => Mesh.NumCornersPerFace.Range().Add(FirstCornerInFace(f));

        public IArray<int> EdgesFromFace(int f)
            => CornersFromFace(f);

        public int FirstCornerInFace(int f)
            => f * Mesh.NumCornersPerFace;

        public bool FaceHasCorner(int f, int c)
            => CornersFromFace(f).Contains(c);

        public int NextCorner(int c)
        {
            var f = CornerToFace(c);
            var begin = FirstCornerInFace(f);
            var end = begin + Mesh.NumCornersPerFace;
            Debug.Assert(c >= begin);
            Debug.Assert(c < end);
            var c2 = c + 1;
            if (c2 < end)
                return c2;
            Debug.Assert(c2 == end);
            return begin;
        }

        public IArray<int> CornersFromEdge(int e)
            => LinqArray.LinqArray.Create(e, NextCorner(e));

        public IArray<int> VertexIndicesFromEdge(int e)
            => CornersFromEdge(e).Select(VertexIndexFromCorner);

        public IArray<int> VertexIndicesFromFace(int f)
            => Mesh.Indices.SelectByIndex(LinqArray.LinqArray.Create(f * 3, f * 3 + 1, f * 3 + 2));

        public IEnumerable<int> NeighbourVertices(int v)
            => FacesFromVertexIndex(v).SelectMany(f => VertexIndicesFromFace(f)).Where(v2 => v2 != v).Distinct();

        public IEnumerable<int> BorderEdges
            => Edges.Where(IsBorderEdge);

        public IEnumerable<int> BorderFaces
            => Faces.Where(IsBorderFace);

        public int EdgeFirstCorner(int e)
            => e;

        public int EdgeNextCorner(int e)
            => NextCorner(e);

        public int EdgeFirstVertex(int e)
            => VertexIndexFromCorner(EdgeFirstCorner(e));

        public int EdgeNextVertex(int e)
            => VertexIndexFromCorner(EdgeFirstCorner(e));

        public IArray<int> EdgeVertices(int e)
            => LinqArray.LinqArray.Create(EdgeFirstVertex(e), EdgeNextVertex(e));

        public Vector3 PointFromVertex(int v)
            => Mesh.Vertices[v];

        public IArray<Vector3> EdgePoints(int e)
            => EdgeVertices(e).Select(PointFromVertex);
    }

    public static class TopologyExtensions
    {
        public static IMesh Mesh(this TopoElement self)
            => self.Topology.Mesh;
    }
}
