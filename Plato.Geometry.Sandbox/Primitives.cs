using System;
using System.Collections.Generic;
using System.Linq;
using Vim.G3d;
using Vim.LinqArray;
using Vim.Math3d;

namespace Plato.Geometry
{
    // TODO: plane, cylinder, cone, ruled face, 
    public static class Primitives
    {
        public static IMesh TriMesh(IEnumerable<GeometryAttribute> attributes)
            => attributes.Where(x => x != null).ToIMesh();

        public static IMesh TriMesh(params GeometryAttribute[] attributes)
            => TriMesh(attributes.AsEnumerable());

        public static IMesh TriMesh(
            this IArray<Vector3> vertices,
            IArray<int> indices = null,
            IArray<Vector2> uvs = null,
            IArray<Vector4> colors = null,
            IArray<int> materials = null,
            IArray<int> submeshMaterials = null)
            => TriMesh(
                vertices?.ToPositionAttribute(),
                indices?.ToIndexAttribute(),
                uvs?.ToVertexUvAttribute(),
                materials?.ToFaceMaterialAttribute(),
                colors?.ToVertexColorAttribute(),
                submeshMaterials?.ToSubmeshMaterialAttribute()
            );

        public static IMesh TriMesh(this IArray<Vector3> vertices, IArray<int> indices = null, params GeometryAttribute[] attributes)
            => new GeometryAttribute[] {
                vertices?.ToPositionAttribute(),
                indices?.ToIndexAttribute(),
            }.Concat(attributes).ToIMesh();

        public static IMesh QuadMesh(params GeometryAttribute[] attributes)
            => QuadMesh(attributes.AsEnumerable());

        public static IMesh QuadMesh(this IEnumerable<GeometryAttribute> attributes)
            => new QuadMesh(attributes.Where(x => x != null)).ToTriMesh();

        public static IMesh QuadMesh(this IArray<Vector3> vertices, IArray<int> indices = null, IArray<Vector2> uvs = null, IArray<int> materials = null, IArray<int> objectIds = null)
            => QuadMesh(
                vertices.ToPositionAttribute(),
                indices?.ToIndexAttribute(),
                uvs?.ToVertexUvAttribute(),
                materials?.ToFaceMaterialAttribute()
            );

        public static IMesh QuadMesh(Vector3 a, Vector3 b, Vector3 c, Vector3 d)
            => QuadMesh(new[] { a, b, c, d }.ToIArray());

        public static IMesh Cube
        {
            get
            {
                var vertices = new[] {
                    // front
                    new Vector3(-0.5f, -0.5f,  0.5f),
                    new Vector3(0.5f, -0.5f,  0.5f),
                    new Vector3(0.5f,  0.5f,  0.5f),
                    new Vector3(-0.5f,  0.5f,  0.5f),
                    // back
                    new Vector3(-0.5f, -0.5f, -0.5f),
                    new Vector3(0.5f, -0.5f, -0.5f),
                    new Vector3(0.5f,  0.5f, -0.5f),
                    new Vector3(-0.5f,  0.5f, -0.5f)
                }.ToIArray();

                var indices = new[] {
                    // front
                    0, 1, 2,
                    2, 3, 0,
                    // right
                    1, 5, 6,
                    6, 2, 1,
                    // back
                    7, 6, 5,
                    5, 4, 7,
                    // left
                    4, 0, 3,
                    3, 7, 4,
                    // bottom
                    4, 5, 1,
                    1, 0, 4,
                    // top
                    3, 2, 6,
                    6, 7, 3
                }.ToIArray();

                return vertices.TriMesh(indices);
            }
        }

        public static IMesh ToIMesh(this AABox box)
            => Cube.Scale(box.Extent).Translate(box.Center);

        public static float Sqrt2 = 2.0f.Sqrt();

        public static readonly IMesh Tetrahedron
            = TriMesh(LinqArray.LinqArray.Create(
                new Vector3(1f, 0.0f, -1f / Sqrt2),
                new Vector3(-1f, 0.0f, -1f / Sqrt2),
                new Vector3(0.0f, 1f, 1f / Sqrt2),
                new Vector3(0.0f, -1f, 1f / Sqrt2)),
            LinqArray.LinqArray.Create(0, 1, 2, 1, 0, 3, 0, 2, 3, 1, 3, 2));

        public static readonly IMesh Square
            = LinqArray.LinqArray.Create(
                new Vector2(-0.5f, -0.5f),
                new Vector2(-0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, -0.5f)).Select(x => x.ToVector3()).QuadMesh();

        public static readonly IMesh Octahedron
            = Square.Vertices.Append(Vector3.UnitZ, -Vector3.UnitZ).Normalize().TriMesh(
                LinqArray.LinqArray.Create(
                    0, 1, 4, 1, 2, 4, 2, 3, 4,
                    3, 2, 5, 2, 1, 5, 1, 0, 5));

        // see: https://github.com/mrdoob/three.js/blob/9ef27d1af7809fa4d9943f8d4c4644e365ab6d2d/src/geometries/TorusBufferGeometry.js#L52
        public static Vector3 TorusFunction(Vector2 uv, float radius, float tube)
        {
            uv = uv * Constants.TwoPi;
            return new Vector3(
                (radius + tube * uv.Y.Cos()) * uv.X.Cos(),
                (radius + tube * uv.Y.Cos()) * uv.X.Sin(),
                tube * uv.Y.Sin());
        }

        public static IMesh Torus(float radius, float tubeRadius, int uSegs, int vSegs)
            => QuadMesh(uv => TorusFunction(uv, radius, tubeRadius), uSegs, vSegs);

        // see: https://github.com/mrdoob/three.js/blob/9ef27d1af7809fa4d9943f8d4c4644e365ab6d2d/src/geometries/SphereBufferGeometry.js#L76
        public static Vector3 SphereFunction(Vector2 uv, float radius)
            => new Vector3(
                (float)(-radius * Math.Cos(uv.X * Constants.TwoPi) * Math.Sin(uv.Y * Constants.Pi)),
                (float)(radius * Math.Cos(uv.Y * Constants.Pi)),
                (float)(radius * Math.Sin(uv.X * Constants.TwoPi) * Math.Sin(uv.Y * Constants.Pi)));

        public static IMesh Sphere(float radius, int uSegs, int vSegs)
            => QuadMesh(uv => SphereFunction(uv, radius), uSegs, vSegs);

        /// <summary>
        /// Creates a TriMesh from four points. 
        /// </summary>
        public static IMesh TriMeshFromQuad(Vector3 a, Vector3 b, Vector3 c, Vector3 d)
            => TriMesh(new[] { a, b, c, c, d, a }.ToIArray());

        // Icosahedron, Dodecahedron,

        /// <summary>
        /// Returns a collection of circular points.
        /// </summary>
        public static IArray<Vector2> CirclePoints(float radius, int numPoints)
            => CirclePoints(numPoints).Select(x => x * radius);

        public static IArray<Vector2> CirclePoints(int numPoints)
            => numPoints.Select(i => CirclePoint(i, numPoints));

        public static Vector2 CirclePoint(int i, int numPoints)
            => new Vector2((i * (Constants.TwoPi / numPoints)).Cos(), (i * (Constants.TwoPi / numPoints)).Sin());

        /// <summary>
        /// Computes the indices of a quad mesh astrip.
        /// </summary>
        public static IArray<int> ComputeQuadMeshStripIndices(int usegs, int vsegs, bool wrapUSegs = false, bool wrapVSegs = false)
        {
            var indices = new List<int>();

            var maxUSegs = wrapUSegs ? usegs : usegs + 1;
            var maxVSegs = wrapVSegs ? vsegs : vsegs + 1;

            for (var i = 0; i < vsegs; ++i)
            {
                var rowA = i * maxUSegs;
                var rowB = ((i + 1) % maxVSegs) * maxUSegs;

                for (var j = 0; j < usegs; ++j)
                {
                    var colA = j;
                    var colB = (j + 1) % maxUSegs;

                    indices.Add(rowA + colA);
                    indices.Add(rowA + colB);
                    indices.Add(rowB + colB);
                    indices.Add(rowB + colA);
                }
            }

            return indices.ToIArray();
        }

        /// <summary>
        /// Returns the index buffer of a quad mesh strip.
        /// Returns an empty array if either numRowPoints or numPointsPerRow is less than 2.
        /// </summary>
        public static IArray<int> QuadMeshStripIndicesFromPointRows(
            int numPointRows,
            int numPointsPerRow,
            bool clockwise = false)
        {
            // A quad(ABCD) is defined as 4 indices, counter clock-wise:
            //
            //     col    col
            // row  D------C     quad(ABCD) = (counter-clockwise) { A, B, C, D }
            //      |t1 /  |   triangle(t0) = (counter-clockwise) { A, B, C }
            //      |  / t0|   triangle(t1) = (counter-clockwise) { A, C, D }
            // row  A------B

            var indices = new List<int>(); // 4 indices per quad.
            for (var rowIndex = 0; rowIndex < numPointRows - 1; ++rowIndex)
            {
                for (var colIndex = 0; colIndex < numPointsPerRow - 1; ++colIndex)
                {
                    // The vertices will all be inserted in a flat list in the vertex buffer
                    // [ ...row0, ...row1, ...row2, ..., ...rowN]
                    //
                    //                   colIndex
                    //                      ...    ...
                    //                       |      |
                    // rowIndex + 1: [... ---D------C--- ...]
                    //                       |t1 /  |
                    //                       |  / t0|
                    // rowIndex:     [... ---A------B--- ...]
                    //                       |      |
                    //                      ...    ...
                    //
                    // rowSize:      |<-----numColumns----->|
                    //

                    var A = colIndex + rowIndex * numPointsPerRow;
                    var B = A + 1;
                    var D = colIndex + (rowIndex + 1) * numPointsPerRow;
                    var C = D + 1;

                    if (clockwise)
                    {
                        indices.Add(D);
                        indices.Add(C);
                        indices.Add(B);
                        indices.Add(A);
                    }
                    else
                    {
                        indices.Add(A);
                        indices.Add(B);
                        indices.Add(C);
                        indices.Add(D);
                    }
                }
            }

            return indices.ToIArray();
        }

        public static IArray<int> TriMeshCylinderCapIndices(int numEdgeVertices)
        {
            // Example cap where numEdgeVertices is 6:
            //
            // (!) It is assumed that vertex 0 is at the center of the cap
            // and that this center vertex is omitted from the numEdgeVertices count.
            //
            //      3<------2
            //     /  \t1 /  ^                t0 = (O, 1, 2)
            //    v t2 \ / t0 \               t1 = (O, 2, 3)
            //   4------0------1 <---- start  t2 = (O, 3, 4)
            //    \ t3 / \ t5 ^               t3 = (O, 4, 5)
            //     v  /t4 \  /                t4 = (O, 5, 6)
            //      5------>6 <---- end       t5 = (O, 6, 1) <-- special case.

            var center = 0;
            var indices = new List<int>();

            var numTriangles = numEdgeVertices;

            // Do all the triangles except the last one.
            for (var triangle = 0; triangle < numTriangles - 1; ++triangle)
            {
                var index0 = center; // 0
                var index1 = triangle + 1;
                var index2 = index1 + 1;

                indices.Add(index0);
                indices.Add(index1);
                indices.Add(index2);
            }

            // The last triangle loops back onto the first edge vertex.
            var lastTriangleIndex0 = center;
            var lastTriangleIndex1 = numEdgeVertices;
            var lastTriangleIndex2 = 1;
            indices.Add(lastTriangleIndex0);
            indices.Add(lastTriangleIndex1);
            indices.Add(lastTriangleIndex2);

            return indices.ToIArray();
        }

        /// <summary>
        /// Creates a quad mesh given a mapping from 2 space to 3 space 
        /// </summary>
        public static IMesh QuadMesh(this Func<Vector2, Vector3> f, int segs)
            => QuadMesh(f, segs, segs);

        /// <summary>
        /// Creates a quad mesh given a mapping from 2 space to 3 space 
        /// </summary>
        public static IMesh QuadMesh(this Func<Vector2, Vector3> f, int usegs, int vsegs, bool wrapUSegs = false, bool wrapVSegs = false)
        {
            var verts = new List<Vector3>();
            var maxUSegs = wrapUSegs ? usegs : usegs + 1;
            var maxVSegs = wrapVSegs ? vsegs : vsegs + 1;

            for (var i = 0; i < maxVSegs; ++i)
            {
                var v = (float)i / vsegs;
                for (var j = 0; j < maxUSegs; ++j)
                {
                    var u = (float)j / usegs;
                    verts.Add(f(new Vector2(u, v)));
                }
            }

            return QuadMesh(verts.ToIArray(), ComputeQuadMeshStripIndices(usegs, vsegs, wrapUSegs, wrapVSegs));
        }

        /// <summary>
        /// Creates a revolved face ... note that the last points are on top of the original 
        /// </summary>
        public static IMesh RevolveAroundAxis(this IArray<Vector3> points, Vector3 axis, int segments = 4)
        {
            var verts = new List<Vector3>();
            for (var i = 0; i < segments; ++i)
            {
                var angle = Constants.TwoPi / segments;
                points.Rotate(axis, angle).AddTo(verts);
            }

            return QuadMesh(verts.ToIArray(), ComputeQuadMeshStripIndices(segments - 1, points.Count - 1));
        }
    }
}
