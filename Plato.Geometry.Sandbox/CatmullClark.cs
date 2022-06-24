using System;
using System.Collections.Generic;
using Vim.LinqArray;
using Vim.Math3d;

namespace Vim.Geometry
{
    // This is used to index an edge from any two vertices.
    // Vertex order doesnt matter.
    public class GeometryEdgeKey : IEquatable<GeometryEdgeKey>
    {
        public int Vertex0 { get; }
        public int Vertex1 { get; }

        public GeometryEdgeKey(int v0, int v1)
        {
            Vertex0 = Math.Min(v0, v1);
            Vertex1 = Math.Max(v0, v1);
        }

        public override int GetHashCode()
            => Hash.Combine(Vertex0.GetHashCode(), Vertex1.GetHashCode());

        public override bool Equals(object obj)
            => obj is GeometryEdgeKey && Equals((GeometryEdgeKey)obj);

        public bool Equals(GeometryEdgeKey p)
            => Vertex0 == p.Vertex0 && Vertex1 == p.Vertex1;
    }

    // Stores information about an edge in a mesh.
    public class GeometryEdge
    {
        public Vector3 EdgePoint { get; set; }
        public int EdgePointIndex { get; set; } = -1;
        public int Face0 { get; set; } = -1;
        public int Face1 { get; set; } = -1;
    }

    public static class CatmullClarkSmoothing
    {
        public static Dictionary<GeometryEdgeKey, GeometryEdge> CreateEdgeMap(this IMesh geometry)
        {
            var globalEdgeIndex = 0;
            var edgeMap = new Dictionary<GeometryEdgeKey, GeometryEdge>();
            var currentIndex = 0;
            for (var faceIndex = 0; faceIndex < geometry.NumFaces; faceIndex++)
            {
                var faceSize = 3;
                for (var edgeIndex = 0; edgeIndex < faceSize; edgeIndex++)
                {
                    var geometryEdgeKey = new GeometryEdgeKey(geometry.Indices[currentIndex + edgeIndex], geometry.Indices[currentIndex + (edgeIndex + 1) % faceSize]);
                    if (edgeMap.ContainsKey(geometryEdgeKey))
                    {
                        edgeMap[geometryEdgeKey].Face1 = faceIndex;
                    }
                    else
                    {
                        var geometryEdge = new GeometryEdge();
                        geometryEdge.EdgePointIndex = globalEdgeIndex++;
                        geometryEdge.Face0 = faceIndex;
                        edgeMap[geometryEdgeKey] = geometryEdge;
                    }
                }
                currentIndex += faceSize;
            }

            return edgeMap;
        }

        public static IMesh CatmullClark(this IMesh geometry, float smoothing = 0.0f)
        {
            var edgeMap = geometry.CreateEdgeMap();

            var numQuads = geometry.NumFaces * 3;
            var numVertices = geometry.Vertices.Count + edgeMap.Count + geometry.NumFaces;

            var facePoints = new Vector3[geometry.NumFaces];
            var vertexFPoints = new Vector3[geometry.Vertices.Count];
            var vertexRPoints = new Vector3[geometry.Vertices.Count];
            var vertexNumFaces = new float[geometry.Vertices.Count];
            var vertexNumEdges = new float[geometry.Vertices.Count];
            var newVertices = new Vector3[numVertices];
            var newIndices = new int[numQuads * 4];
            var edgeVertices = new bool[geometry.Vertices.Count];

            foreach (var edge in edgeMap)
            {
                if (edge.Value.Face0 == -1 || edge.Value.Face1 == -1)
                {
                    edgeVertices[edge.Key.Vertex0] = true;
                    edgeVertices[edge.Key.Vertex1] = true;
                }
            }

            // For each face, add a face point
            //     Set each face point to be the average of all original points for the respective face.
            // For each edge, add an edge point.
            //     Set each edge point to be the average of the two neighbouring face points and its two original endpoints.
            // For each face point, add an edge for every edge of the face, connecting the face point to each edge point for the face.
            // For each original point P, take the average F of all n (recently created) face points for faces touching P, and take 
            // the average R of all n edge midpoints for (original) edges touching P, where each edge midpoint is the average of its
            // two endpoint vertices (not to be confused with new "edge points" above). Move each original point to the point
            // 
            //     (F + 2 R + ( n − 3 ) P) / n
            // 
            // This is the barycenter of P, R and F with respective weights (n − 3), 2 and 1.
            // 
            // Connect each new vertex point to the new edge points of all original edges incident on the original vertex.
            // Define new faces as enclosed by edges.

            CalculateFaceCentroidPoints(geometry, facePoints, vertexNumFaces, vertexFPoints);
            CalculateEdgePoints(geometry, edgeMap, facePoints, vertexNumEdges, vertexRPoints, smoothing);
            CalculateVertexPoints(geometry, vertexNumFaces, vertexFPoints, vertexNumEdges, vertexRPoints, newVertices, edgeVertices, smoothing);

            var facePointStartIndex = geometry.Vertices.Count;
            var edgePointStartIndex = facePointStartIndex + facePoints.Length;

            // Copy all points into a single buffer
            for (var i = 0; i < facePoints.Length; i++)
            {
                newVertices[facePointStartIndex + i] = facePoints[i];
            }
            foreach (var edge in edgeMap)
            {
                newVertices[edgePointStartIndex + edge.Value.EdgePointIndex] = edge.Value.EdgePoint;
            }

            return GenerateMesh(geometry, newVertices, newIndices, edgeMap, facePointStartIndex, edgePointStartIndex, numQuads);
        }

        public static void CalculateFaceCentroidPoints(IMesh geometry, Vector3[] outFacePoints, float[] outVertexNumFaces, Vector3[] outVertexFPoints)
        {
            var currentVertexIndex = 0;
            for (var faceIndex = 0; faceIndex < geometry.NumFaces; faceIndex++)
            {
                var faceSize = geometry.NumCornersPerFace;

                var facePoint = new Vector3();

                for (var edgeIndex = 0; edgeIndex < faceSize; edgeIndex++)
                {
                    var vertexIndex = geometry.Indices[currentVertexIndex + edgeIndex];
                    facePoint += geometry.Vertices[vertexIndex];
                }

                facePoint /= faceSize;

                outFacePoints[faceIndex] = facePoint;

                for (var edgeIndex = 0; edgeIndex < faceSize; edgeIndex++)
                {
                    var vertexIndex = geometry.Indices[currentVertexIndex + edgeIndex];
                    outVertexNumFaces[vertexIndex]++;
                    outVertexFPoints[vertexIndex] += facePoint;
                }

                currentVertexIndex += faceSize;
            }
        }

        public static void CalculateEdgePoints(IMesh geometry, Dictionary<GeometryEdgeKey, GeometryEdge> edgeMap, Vector3[] facePoints, float[] outVertexNumEdges, Vector3[] outVertexRPoints, float smoothing)
        {
            foreach (var edge in edgeMap)
            {
                var n = 2.0f;
                var middlePoint = geometry.Vertices[edge.Key.Vertex0] + geometry.Vertices[edge.Key.Vertex1];
                var edgePoint = middlePoint;

                if (edge.Value.Face0 >= 0 && edge.Value.Face1 >= 0)
                {
                    edgePoint += facePoints[edge.Value.Face0];
                    edgePoint += facePoints[edge.Value.Face1];
                    n += 2.0f;
                }

                middlePoint /= 2.0f;
                edgePoint /= n;

                edge.Value.EdgePoint = edgePoint * smoothing + middlePoint * (1.0f - smoothing);

                outVertexNumEdges[edge.Key.Vertex0]++;
                outVertexNumEdges[edge.Key.Vertex1]++;
                outVertexRPoints[edge.Key.Vertex0] += edge.Value.EdgePoint;
                outVertexRPoints[edge.Key.Vertex1] += edge.Value.EdgePoint;
            }
        }

        public static void CalculateVertexPoints(IMesh geometry, float[] vertexNumFaces, Vector3[] vertexFPoints, float[] vertexNumEdges, Vector3[] vertexRPoints, Vector3[] outNewVertices, bool[] edgeVertices, float smoothing)
        {
            for (var index = 0; index < geometry.Vertices.Count; index++)
            {
                var numFaces = vertexNumFaces[index];
                var numEdges = vertexNumEdges[index];
                var newVertex = (vertexFPoints[index] / numFaces + 2.0f * vertexRPoints[index] / numEdges + (numFaces - 3.0f) * geometry.Vertices[index]) / numFaces;

                outNewVertices[index] = edgeVertices[index] ? geometry.Vertices[index] : smoothing * newVertex + (1.0f - smoothing) * geometry.Vertices[index];
            }
        }

        public static IMesh GenerateMesh(IMesh geometry, Vector3[] newVertices, int[] newIndices, Dictionary<GeometryEdgeKey, GeometryEdge> edgeMap, int facePointStartIndex, int edgePointStartIndex, int numQuads)
        {
            var currentNewVertexIndex = 0;
            var currentOldVertexIndex = 0;
            for (var faceIndex = 0; faceIndex < geometry.NumFaces; faceIndex++)
            {
                var faceSize = geometry.NumCornersPerFace;

                for (var i = 0; i < faceSize; i++)
                {
                    var prevFaceVertexIndex = geometry.Indices[currentOldVertexIndex + i];
                    var faceVertexIndex = geometry.Indices[currentOldVertexIndex + (i + 1) % faceSize];
                    var nextFaceVertexIndex = geometry.Indices[currentOldVertexIndex + (i + 2) % faceSize];

                    var edgeKey0 = new GeometryEdgeKey(prevFaceVertexIndex, faceVertexIndex);
                    var edgeKey1 = new GeometryEdgeKey(faceVertexIndex, nextFaceVertexIndex);

                    newIndices[currentNewVertexIndex++] = faceIndex + facePointStartIndex;
                    newIndices[currentNewVertexIndex++] = edgeMap[edgeKey0].EdgePointIndex + edgePointStartIndex;
                    newIndices[currentNewVertexIndex++] = faceVertexIndex;
                    newIndices[currentNewVertexIndex++] = edgeMap[edgeKey1].EdgePointIndex + edgePointStartIndex;
                }

                currentOldVertexIndex += faceSize;
            }

            return Primitives.QuadMesh(newVertices.ToIArray(), newIndices.ToIArray());
        }
    }
}
