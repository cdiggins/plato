using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vim.DotNetUtilities;
using Vim.G3d;
using Vim.LinqArray;
using Vim.Math3d;

namespace Vim.Geometry
{
    public static class MeshExtensions
    {
        #region constructors
        public static IMesh ToIMesh(this IArray<GeometryAttribute> self)
            => self.ToEnumerable().ToIMesh();

        public static IMesh ToIMesh(this IEnumerable<GeometryAttribute> self)
        {
            var attrs = self.Where(x => x != null).ToArray();
            var tmp = new GeometryAttributes(self);
            switch (tmp.NumCornersPerFace)
            {
                case 3:
                    return new TriMesh(tmp.Attributes.ToEnumerable());
                case 4:
                    return new QuadMesh(tmp.Attributes.ToEnumerable()).ToTriMesh();
                default:
                    throw new Exception($"Can not convert a geometry with {tmp.NumCornersPerFace} to a triangle mesh: only quad meshes");
            }
        }

        public static IMesh ToIMesh(this IGeometryAttributes g)
            => g is IMesh m ? m : g is QuadMesh q ? q.ToIMesh() : g.Attributes.ToIMesh();
        #endregion

        // Computes the topology: this is a slow O(N) operation
        public static Topology ComputeTopology(this IMesh mesh)
            => new Topology(mesh);

        public static double Area(this IMesh mesh)
            => mesh.Triangles().Sum(t => t.Area);

        #region validation
        public static bool IsDegenerateVertexIndices(this Int3 vertexIndices)
            => vertexIndices.X == vertexIndices.Y || vertexIndices.X == vertexIndices.Z || vertexIndices.Y == vertexIndices.Z;

        public static bool HasDegenerateFaceVertexIndices(this IMesh self)
            => self.AllFaceVertexIndices().Any(IsDegenerateVertexIndices);
        #endregion

        // TODO: find a better location for this function. DotNetUtilties doesn't know about IArray unfortunately, so maybe this project needs its own Utility class.
        public static DictionaryOfLists<U, T> GroupBy<T, U>(this IArray<T> xs, Func<int, U> groupingFunc)
        {
            var r = new DictionaryOfLists<U, T>();
            for (var i = 0; i < xs.Count; ++i)
                r.Add(groupingFunc(i), xs[i]);
            return r;
        }

        public static IArray<int> GetFaceMaterials(this IMesh mesh)
        {
            // SubmeshIndexOffsets: [0, A, B]
            // SubmeshIndexCount:   [X, Y, Z]
            // SubmeshMaterials:    [L, M, N]
            // ---
            // FaceMaterials:       [...Repeat(L, X / 3), ...Repeat(M, Y / 3), ...Repeat(N, Z / 3)] <-- divide by 3 for the number of corners per Triangular face
            var numCornersPerFace = mesh.NumCornersPerFace;
            return mesh.SubmeshIndexCount
                .ToEnumerable()
                .SelectMany((indexCount, i) => Enumerable.Repeat(mesh.SubmeshMaterials[i], indexCount / numCornersPerFace))
                .ToIArray();
        }

        public static IEnumerable<int> DisctinctMaterials(this IMesh mesh)
            => mesh.GetFaceMaterials().ToEnumerable().Distinct();

        public static DictionaryOfLists<int, int> IndicesByMaterial(this IMesh mesh)
        {
            var faceMaterials = mesh.GetFaceMaterials();
            return mesh.Indices.GroupBy(i => faceMaterials[i / 3]);
        }

        public static IMesh Merge(this IArray<IMesh> meshes)
            => meshes.Select(m => (IGeometryAttributes)m).Merge().ToIMesh();

        public static IMesh Merge(this IEnumerable<IMesh> meshes)
            => meshes.ToIArray().Merge();

        public static IMesh Merge(this IMesh mesh, params IMesh[] others)
        {
            var gs = others.ToList();
            gs.Insert(0, mesh);
            return gs.Merge();
        }

        public static IEnumerable<(int Material, IMesh Mesh)> SplitByMaterial(this IMesh mesh)
        {
            var submeshMaterials = mesh.SubmeshMaterials;
            if (submeshMaterials == null || submeshMaterials.Count == 0)
            {
                // Base case: no submesh materials are defined on the mesh.
                return new[] { (-1, mesh) };
            }

            var submeshIndexOffets = mesh.SubmeshIndexOffsets;
            var submeshIndexCounts = mesh.SubmeshIndexCount;
            if (submeshIndexOffets == null || submeshIndexCounts == null ||
                submeshMaterials.Count <= 1 || submeshIndexOffets.Count <= 1 || submeshIndexCounts.Count <= 1)
            {
                // Base case: only one submesh material.
                return new [] { (submeshMaterials[0], mesh) };
            }

            // Example:
            //
            // ------------
            // INPUT MESH:
            // ------------
            // Vertices            [Va, Vb, Vc, Vd, Ve, Vf, Vg] <-- 7 vertices
            // Indices             [0 (Va), 1 (Vb), 2 (Vc), 1 (Vb), 2 (Vc), 3 (Vd), 4 (Ve), 5 (Vf), 6 (Vg)] <-- 3 triangles referencing the 7 vertices
            // SubmeshIndexOffsets [0, 3, 6]
            // SubmeshIndexCount   [3, 3, 3] (computed)
            // SubmeshMaterials    [Ma, Mb, Mc]
            //
            // ------------
            // OUTPUT MESHES
            // ------------
            // - MESH FOR MATERIAL Ma
            //   Vertices:             [Va, Vb, Vc]
            //   Indices:              [0, 1, 2]
            //   SubmeshIndexOffsets:  [0]
            //   SubmeshMaterials:     [Ma]
            //
            //- MESH FOR MATERIAL Mb
            //   Vertices:             [Vb, Vc, Vd]
            //   Indices:              [0, 1, 2]
            //   SubmeshIndexOffsets:  [0]
            //   SubmeshMaterials:     [Mb]
            //
            //- MESH FOR MATERIAL Mc
            //   Vertices:             [Ve, Vf, Vg]
            //   Indices:              [0, 1, 2]
            //   SubmeshIndexOffsets:  [0]
            //   SubmeshMaterials:     [Mc]

            return mesh.SubmeshMaterials
                .Select((submeshMaterial, submeshIndex) => (submeshMaterial, submeshIndex))
                .GroupBy(t => t.submeshMaterial)
                .SelectMany(g =>
                {
                    var material = g.Key;
                    var meshes = g.Select((t, _) =>
                    {
                        var submeshMaterial = t.submeshMaterial;
                        var submeshStartIndex = submeshIndexOffets[t.submeshIndex];
                        var submeshIndexCount = submeshIndexCounts[t.submeshIndex];

                        var indexSlice = mesh.Indices.Slice(submeshStartIndex, submeshStartIndex + submeshIndexCount);

                        var newVertexAttributes = mesh.VertexAttributes().Select(attr => attr.Remap(indexSlice));
                        var newIndexAttribute = indexSlice.Count.Select(i => i).ToIndexAttribute();

                        var newSubmeshIndexOffsets = 0.Repeat(1).ToSubmeshIndexOffsetAttribute();
                        var newSubmeshMaterials = submeshMaterial.Repeat(1).ToSubmeshMaterialAttribute();
                        
                        return newVertexAttributes
                            .Concat(mesh.NoneAttributes())
                            .Concat(mesh.WholeGeometryAttributes())
                            // TODO: TECH DEBT - face, edge, and corner attributes are ignored for now.
                            .Append(newIndexAttribute)
                            .Append(newSubmeshIndexOffsets)
                            .Append(newSubmeshMaterials)
                            .ToGeometryAttributes()
                            .ToIMesh();
                    });

                    return meshes.Select(m => (material, m));
                });
        }

        public static IGeometryAttributes DeleteUnusedVertices(this IMesh mesh)
        {
            var tmp = new bool[mesh.Vertices.Count];
            for (var i = 0; i < mesh.Indices.Count; ++i)
                tmp[mesh.Indices[i]] = true;

            var remap = new List<int>();
            for (var i = 0; i < tmp.Length; ++i)
            {
                if (tmp[i])
                    remap.Add(i);
            }

            return mesh.RemapVertices(remap.ToIArray());
        }

        public static bool GeometryEquals(this IMesh mesh, IMesh other, float tolerance = Constants.Tolerance)
        {
            if (mesh.NumFaces != other.NumFaces)
                return false;
            return mesh.Triangles().Zip(other.Triangles(), (t1, t2) => t1.AlmostEquals(t2, tolerance)).All(x => x);
        }

        public static IMesh SimplePolygonTessellate(this IEnumerable<Vector3> points)
        {
            var pts = points.ToList();
            var cnt = pts.Count;
            var sum = Vector3.Zero;
            var idxs = new List<int>(pts.Count * 3);
            for (var i = 0; i < pts.Count; ++i)
            {
                idxs.Add(i);
                idxs.Add(i + 1 % cnt);
                idxs.Add(cnt);
                sum += pts[i];
            }

            var midPoint = sum / pts.Count;
            pts.Add(midPoint);

            return Primitives.TriMesh(pts.ToIArray(), idxs.ToIArray());
        }

        public static IGeometryAttributes ReverseWindingOrder(this IMesh mesh)
        {
            var n = mesh.Indices.Count;
            var r = new int[n];
            for (var i = 0; i < n; i += 3)
            {
                r[i + 0] = mesh.Indices[i + 2];
                r[i + 1] = mesh.Indices[i + 1];
                r[i + 2] = mesh.Indices[i + 0];
            }
            return mesh.SetAttribute(r.ToIArray().ToIndexAttribute());
        }

        /// <summary>
        /// Returns the closest point in a sequence of points
        /// </summary>
        public static Vector3 NearestPoint(this IEnumerable<Vector3> points, Vector3 x)
            => points.Minimize(float.MaxValue, p => p.DistanceSquared(x));

        /// <summary>
        /// Returns the closest point in a sequence of points
        /// </summary>
        public static Vector3 NearestPoint(this IArray<Vector3> points, Vector3 x)
            => points.ToEnumerable().NearestPoint(x);

        /// <summary>
        /// Returns the closest point in a geometry
        /// </summary>
        public static Vector3 NearestPoint(this IMesh mesh, Vector3 x)
            => mesh.Vertices.NearestPoint(x);

        public static Vector3 FurthestPoint(this IMesh mesh, Vector3 x0, Vector3 x1)
            => mesh.Vertices.FurthestPoint(x0, x1);

        public static Vector3 FurthestPoint(this IArray<Vector3> points, Vector3 x0, Vector3 x1)
            => points.ToEnumerable().FurthestPoint(x0, x1);

        public static Vector3 FurthestPoint(this IEnumerable<Vector3> points, Vector3 x0, Vector3 x1)
            => points.Maximize(float.MinValue, v => v.Distance(x0).Min(v.Distance(x1)));

        public static Vector3 FurthestPoint(this IMesh mesh, Vector3 x)
            => mesh.Vertices.FurthestPoint(x);

        public static Vector3 FurthestPoint(this IArray<Vector3> points, Vector3 x)
            => points.ToEnumerable().FurthestPoint(x);

        public static Vector3 FurthestPoint(this IEnumerable<Vector3> points, Vector3 x)
            => points.Maximize(float.MinValue, v => v.Distance(x));

        public static IGeometryAttributes SnapPoints(this IMesh mesh, float snapSize)
            => snapSize.Abs() >= Constants.Tolerance
                ? mesh.Deform(v => (v * snapSize.Inverse()).Truncate() * snapSize)
                : mesh.Deform(v => Vector3.Zero);

        /// <summary>
        /// Returns the vertices organized by face corner. 
        /// </summary>
        public static IArray<Vector3> VerticesByIndex(this IMesh mesh)
            => mesh.Vertices.SelectByIndex(mesh.Indices);

        /// <summary>
        /// Returns the vertices organized by face corner, normalized to the first position.
        /// This is useful for detecting if two meshes are the same except offset by 
        /// position.
        /// </summary>
        public static IArray<Vector3> NormalizedVerticesByCorner(this IMesh m)
        {
            if (m.NumCorners == 0)
                return Vector3.Zero.Repeat(0);
            var firstVertex = m.Vertices[m.Indices[0]];
            return m.VerticesByIndex().Select(v => v - firstVertex);
        }

        /// <summary>
        /// Compares the face positions of two meshes normalized by the vertex buffer, returning the maximum distance, or null
        /// if the meshes have different topology. 
        /// </summary>
        public static float? MaxNormalizedDistance(this IMesh mesh, IMesh other)
        {
            var xs = mesh.NormalizedVerticesByCorner();
            var ys = other.NormalizedVerticesByCorner();
            if (xs.Count != ys.Count)
                return null;
            return xs.Zip(ys, (x, y) => x.Distance(y)).Max();
        }

        public static AABox BoundingBox(this IMesh mesh)
            => AABox.Create(mesh.Vertices.ToEnumerable());

        public static Sphere BoundingSphere(this IMesh mesh)
            => mesh.BoundingBox().ToSphere();

        public static float BoundingRadius(this IMesh mesh)
            => mesh.BoundingSphere().Radius;

        public static Vector3 Center(this IMesh mesh)
            => mesh.BoundingBox().Center;

        public static Vector3 Centroid(this IMesh mesh)
            => mesh.Vertices.Aggregate(Vector3.Zero, (x, y) => x + y) / mesh.Vertices.Count;

        public static bool AreIndicesValid(this IMesh mesh)
            => mesh.Indices.All(i => i >= 0 && i < mesh.Vertices.Count);

        public static bool AreAllVerticesUsed(this IMesh mesh)
        {
            var used = new bool[mesh.Vertices.Count];
            mesh.Indices.ForEach(idx => used[idx] = true);
            return used.All(b => b);
        }

        public static IMesh ResetPivot(this IMesh mesh)
            => mesh.Translate(-mesh.BoundingBox().CenterBottom);

        #region Face operations

        /// <summary>
        /// Given an array of face data, creates an array of indexed data to match vertices
        /// </summary>
        public static IArray<T> FaceDataToVertexData<T>(this IMesh mesh, IArray<T> data)
        {
            if (data.Count != mesh.NumFaces)
                throw new Exception("Cannot match input Face data to existing faces");

            var vertexData = new T[mesh.NumVertices];
            for (var i = 0; i < mesh.Indices.Count; ++i)
                vertexData[mesh.Indices[i]] = data[i / 3];
            return vertexData.ToIArray();
        }

        public static IArray<Int3> AllFaceVertexIndices(this IMesh mesh)
            => mesh.NumFaces.Select(mesh.FaceVertexIndices);

        public static Int3 FaceVertexIndices(this IMesh mesh, int faceIndex)
            => new Int3(mesh.Indices[faceIndex * 3], mesh.Indices[faceIndex * 3 + 1], mesh.Indices[faceIndex * 3 + 2]);

        public static Triangle VertexIndicesToTriangle(this IMesh mesh, Int3 indices)
            => new Triangle(mesh.Vertices[indices.X], mesh.Vertices[indices.Y], mesh.Vertices[indices.Z]);

        public static Triangle Triangle(this IMesh mesh, int face)
            => mesh.VertexIndicesToTriangle(mesh.FaceVertexIndices(face));

        public static IArray<Triangle> Triangles(this IMesh mesh)
            => mesh.NumFaces.Select(mesh.Triangle);

        public static IArray<Line> GetAllEdgesAsLines(this IMesh mesh)
            => mesh.Triangles().SelectMany(tri => Tuple.Create(tri.AB, tri.BC, tri.CA));

        public static IArray<Vector3> ComputedNormals(this IMesh mesh)
            => mesh.Triangles().Select(t => t.Normal);

        public static bool Planar(this IMesh mesh, float tolerance = Constants.Tolerance)
        {
            if (mesh.NumFaces <= 1) return true;
            var normal = mesh.Triangle(0).Normal;
            return mesh.ComputedNormals().All(n => n.AlmostEquals(normal, tolerance));
        }

        public static IArray<Vector3> MidPoints(this IMesh mesh)
            => mesh.Triangles().Select(t => t.MidPoint);

        public static IArray<int> FacesToCorners(this IMesh mesh)
            => mesh.NumFaces.Select(i => i * 3);

        public static IArray<T> FaceDataToCornerData<T>(this IMesh mesh, IArray<T> data)
            => mesh.NumCorners.Select(i => data[i / 3]);

        public static IArray<Vector3> GetOrComputeFaceNormals(this IMesh mesh)
            => mesh.GetAttributeFaceNormal()?.Data ?? mesh.ComputedNormals();

        public static IArray<Vector3> GetOrComputeVertexNormals(this IMesh mesh)
            => mesh.VertexNormals ?? mesh.ComputeTopology().GetOrComputeVertexNormals();

        /// <summary>
        /// Returns vertex normals if present, otherwise computes vertex normals naively by averaging them.
        /// Given a pre-computed topology, will-leverage that.
        /// A more sophisticated algorithm would compute the weighted normal 
        /// based on an angle.
        /// </summary>
        public static IArray<Vector3> GetOrComputeVertexNormals(this Topology topo)
        {
            var mesh = topo.Mesh;
            var r = mesh.VertexNormals;
            if (r != null) return r;
            var faceNormals = mesh.GetOrComputeFaceNormals().ToArray();
            return mesh
                .NumVertices
                .Select(vi =>
                {
                    var tmp = topo
                        .FacesFromVertexIndex(vi)
                        .Select(fi => faceNormals[fi])
                        .Average();
                    if (tmp.IsNaN())
                        return Vector3.Zero;
                    return tmp.SafeNormalize();
                });
        }

        public static IMesh CopyFaces(this IMesh mesh, Func<int, bool> predicate)
            => (mesh as IGeometryAttributes).CopyFaces(predicate).ToIMesh();

        public static IMesh CopyFaces(this IMesh mesh, IArray<bool> keep)
            => mesh.CopyFaces(i => keep[i]);

        public static IMesh CopyFaces(this IMesh mesh, IArray<int> keep)
            => mesh.RemapFaces(keep).ToIMesh();

        public static IMesh DeleteFaces(this IMesh mesh, Func<int, bool> predicate)
            => mesh.CopyFaces(f => !predicate(f));
        #endregion

        #region Corner extensions
        /// <summary>
        /// Given an array of data associated with corners, return an array of data associated with
        /// vertices. If a vertex is not referenced, no data is returned. If a vertex is referenced
        /// multiple times, the last reference is used.
        /// TODO: supplement with a proper interpolation system.
        /// </summary>
        public static IArray<T> CornerDataToVertexData<T>(this IMesh mesh, IArray<T> data)
        {
            var vertexData = new T[mesh.NumVertices];
            for (var i = 0; i < data.Count; ++i)
                vertexData[mesh.Indices[i]] = data[i];
            return vertexData.ToIArray();
        }
        #endregion


    }
}
