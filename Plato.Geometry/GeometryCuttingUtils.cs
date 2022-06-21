using Vim;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vim.Math3d;

namespace Vim.Geometry.Algorithms
{
    public static class GeometryCuttingUtils
    {
        // Fins the intersection between two lines.
        // Returns true if they intersect
        // t and u are the distances of the intersection point along the two line
        public static bool LineLineIntersection(Vector2 line1Origin, Vector2 line1Direction, Vector2 line2Origin, Vector2 line2Direction, out float t, out float u, float epsilon = 0.01f)
        {
            var line1P2 = line1Origin + line1Direction;
            var line2P2 = line2Origin + line2Direction;

            var x1 = line1Origin.X;
            var y1 = line1Origin.Y;
            var x2 = line1P2.X;
            var y2 = line1P2.Y;
            var x3 = line2Origin.X;
            var y3 = line2Origin.Y;
            var x4 = line2P2.X;
            var y4 = line2P2.Y;

            var denominator = (x1 - x2) * (y3 - y4) - (y1 - y2) * (x3 - x4);

            if (denominator.Abs() < epsilon)
            {
                t = 0.0f;
                u = 0.0f;
                return false;
            }

            var tNeumerator = (x1 - x3) * (y3 - y4) - (y1 - y3) * (x3 - x4);
            var uNeumerator = (x1 - x2) * (y1 - y3) - (y1 - y2) * (x1 - x3);

            t = tNeumerator / denominator;
            u = -uNeumerator / denominator;

            return true;
        }

        // Returns the distance between two lines
        // t and u are the distances if the intersection points along the two lines 
        public static float LineLineDistance(Vector2 line1A, Vector2 line1B, Vector2 line2A, Vector2 line2B, out float t, out float u, float epsilon = 0.0000001f)
        {
            var x1 = line1A.X;
            var y1 = line1A.Y;
            var x2 = line1B.X;
            var y2 = line1B.Y;
            var x3 = line2A.X;
            var y3 = line2A.Y;
            var x4 = line2B.X;
            var y4 = line2B.Y;

            var denominator = (x1 - x2) * (y3 - y4) - (y1 - y2) * (x3 - x4);

            if (denominator.Abs() >= epsilon)
            {
                // Lines are not parallel, they should intersect nicely
                var tNeumerator = (x1 - x3) * (y3 - y4) - (y1 - y3) * (x3 - x4);
                var uNeumerator = (x1 - x2) * (y1 - y3) - (y1 - y2) * (x1 - x3);

                t = tNeumerator / denominator;
                u = -uNeumerator / denominator;

                var e = 0.0;
                if (t >= -e && t <= 1.0 + e && u >= -e && u <= 1.0 + e)
                {
                    t = t.Clamp(0.0f, 1.0f);
                    u = u.Clamp(0.0f, 1.0f);
                    return 0;
                }
            }

            // Parallel or non intersecting lines - default to point to line checks

            u = 0.0f;
            var minDistance = LinePointDistance(line1A, line1B, line2A, out t);
            var distance = LinePointDistance(line1A, line1B, line2B, out var closestPoint);
            if (distance < minDistance)
            {
                minDistance = distance;
                t = closestPoint;
                u = 1.0f;
            }

            distance = LinePointDistance(line2A, line2B, line1A, out closestPoint);
            if (distance < minDistance)
            {
                minDistance = distance;
                u = closestPoint;
                t = 0.0f;
            }

            distance = LinePointDistance(line2A, line2B, line1B, out closestPoint);
            if (distance < minDistance)
            {
                minDistance = distance;
                u = closestPoint;
                t = 1.0f;
            }

            return minDistance;
        }

        // Returns the distance between a line and a point.
        // t is the distance along the line of the closest point
        public static float LinePointDistance(Vector2 v, Vector2 w, Vector2 p, out float t)
        {
            // Return minimum distance between line segment vw and point p
            var l2 = (v - w).LengthSquared();  // i.e. |w-v|^2 -  avoid a sqrt
            if (l2 == 0.0f)  // v == w case
            {
                t = 0.5f;
                return (p - v).Length();
            }

            // Consider the line extending the segment, parameterized as v + t (w - v).
            // We find projection of point p onto the line. 
            // It falls where t = [(p-v) . (w-v)] / |w-v|^2
            // We clamp t from [0,1] to handle points outside the segment vw.
            t = ((p - v).Dot(w - v) / l2).Clamp(0.0f, 1.0f);
            var closestPoint = v + t * (w - v);  // Projection falls on the segment
            return (p - closestPoint).Length();
        }



        // Apply a surface function that projects a 2d triangulation in UV space into a 3d triangulation
        /*      public static IGeometry ApplySurfaceFunction(this List<EarClipTriangulation> triangulations, Func<Vector2, Vector3> surfaceFunction)
              {
                  var allIndices = new List<int>();
                  var allVertices = new List<Vector3>();

                  foreach (var triangulation in triangulations)
                  {
                      var vertices = triangulation.Points.ToList();
                      var indices = triangulation.TriangleIndices;

                      while (true)
                      {
                          List<int> outIndices;
                          List<Vector2> outVertices;
                          bool done = SubdivideLargeTriangles(vertices, indices, out outVertices, out outIndices, 0.3f);

                          vertices = outVertices;
                          indices = outIndices;

                          if (done)
                          {
                              break;
                          }
                      }

                      // Apply surface function
                      var vertices3d = vertices.Select(x => surfaceFunction(x.ToVector()));

                      // Merge all triangulations
                      int startIndex = allVertices.Count;
                      allVertices.AddRange(vertices3d);
                      allIndices.AddRange(indices.Select(x => x + startIndex));
                  }

                  var cutGeometry = new G3D(
                      allVertices.ToIArray().ToVertexAttribute(),
                      allIndices.ToIArray().ToIndexAttribute(),
                      new FunctionalArray<int>(allIndices.Count / 3, x => 3).ToFaceSizeAttribute()
                      ).ToIMesh();

                  return cutGeometry;
              }*/
    }
}
