using g3;
using System;
using System.Collections.Generic;
using System.Linq;
using Vim.Math3d;
using Vim.LinqArray;
using System.Diagnostics;

namespace Plato.Geometry
{
    public static class G3Sharp
    {
        // https://github.com/gradientspace/geometry3Sharp/issues/3
        public static DMesh3 Compact(this DMesh3 mesh, bool compactFlag = true)
            => compactFlag ? new DMesh3(mesh, true) : mesh;

        public static DMesh3 Slice(this DMesh3 self, Plane plane, bool compact = true)
        {
            var normal = plane.Normal;
            var origin = normal * -plane.D;
            var cutter = new MeshPlaneCut(self, origin.ToVector3D(), normal.ToVector3D());
            var result = cutter.Cut();
            Console.WriteLine($"Cutting result = {result}");
            Console.WriteLine($"Cut loops = {cutter.CutLoops.Count}");
            Console.WriteLine($"Cut spans = {cutter.CutSpans.Count}");
            Console.WriteLine($"Cut faces = {cutter.CutFaceSet?.Count ?? 0}");
            return cutter.Mesh.Compact(compact);
        }

        public static DMesh3 ReduceWithProjection(this DMesh3 mesh, float percent, bool compactResult = true)
            => mesh.Reduce(percent, compactResult, mesh.BuildDMeshAABBTree());

        public static DMesh3 Reduce(this DMesh3 mesh, float percent, bool compactResult = true, ISpatial target = null)
        {
            // TODO: not sure what triggers this
            //if (!mesh.CheckValidity(eFailMode: FailMode.ReturnOnly)) return mesh;

            var r = new Reducer(mesh);

            if (target != null)
            {
                r.SetProjectionTarget(new MeshProjectionTarget(mesh, target));

                // http://www.gradientspace.com/tutorials/2017/8/30/mesh-simplification
                // r.ProjectionMode = Reducer.TargetProjectionMode.Inline;
            }

            return r.Reduce((int)(mesh.VertexCount * percent / 100.0), compactResult);
        }

        public static DMesh3 Reduce(this Reducer reducer, int newVertexCount, bool compactResult = true)
        {
            reducer.ReduceToVertexCount(newVertexCount);
            return reducer.Mesh.Compact(compactResult);
        }

        public static DMeshAABBTree3 BuildDMeshAABBTree(this DMesh3 mesh)
        {
            var tree = new DMeshAABBTree3(mesh);
            tree.Build();
            return tree;
        }

        public static double? DistanceToTree(this DMeshAABBTree3 tree, Ray3d ray)
        {
            var hit_tid = tree.FindNearestHitTriangle(ray);
            if (hit_tid == DMesh3.InvalidID) return null;
            var intr = MeshQueries.TriangleIntersection(tree.Mesh, hit_tid, ray);
            return ray.Origin.Distance(ray.PointAt(intr.RayParameter));
        }

        public static Vector3d NearestPoint(this DMeshAABBTree3 tree, Vector3d point)
        {
            var tid = tree.FindNearestTriangle(point);
            if (tid == DMesh3.InvalidID)
                return new Vector3d();

            var dist = MeshQueries.TriangleDistance(tree.Mesh, tid, point);
            return dist.TriangleClosest;
        }

        public static IArray<Vector3d> NearestPoints(this IMesh g, IArray<Vector3d> points)
        {
            var tree = g.ToG3Sharp().BuildDMeshAABBTree();
            return points.Select(tree.NearestPoint);
        }

        public static IMesh ToIGeometry(this List<DMesh3> meshes)
            => meshes.Select(ToIGeometry).Merge();

        public static IMesh LoadGeometry(string path)
            => LoadMeshes(path).ToIGeometry();

        public static List<DMesh3> LoadMeshes(string path)
        {
            var builder = new DMesh3Builder();
            var reader = new StandardMeshReader { MeshBuilder = builder };
            var result = reader.Read(path, ReadOptions.Defaults);
            if (result.code == IOCode.Ok)
                return builder.Meshes;
            return null;
        }

        public static void WriteFile(this DMesh3 mesh, string filePath)
            => mesh.WriteFile(filePath, WriteOptions.Defaults);

        public static void WriteFileBinary(this DMesh3 mesh, string filePath)
            => mesh.WriteFile(filePath, new WriteOptions { bWriteBinary = true });

        public static void WriteFileAscii(this DMesh3 mesh, string filePath)
            => mesh.WriteFile(filePath, new WriteOptions { bWriteBinary = false });

        public static void WriteFile(this DMesh3 mesh, string filePath, WriteOptions opts)
        {
            var writer = new StandardMeshWriter();
            var m = new WriteMesh(mesh);
            var result = writer.Write(filePath, new List<WriteMesh> { m }, opts);
            if (!result.Equals(IOWriteResult.Ok))
                throw new Exception($"Failed to write file to {filePath} with result {result.ToString()}");
        }
        public static IArray<Vector3> ToVectors(this DVector<double> self)
            => (self.Length / 3).Select(i => new Vector3((float)self[i * 3], (float)self[i * 3 + 1], (float)self[i * 3 + 2]));

        public static Vector3 ToMath3d(this Vector3d self)
            => new Vector3((float)self.x, (float)self.y, (float)self.z);

        public static AABox ToMath3d(this AxisAlignedBox3d self)
            => new AABox(self.Min.ToMath3d(), self.Max.ToMath3d());

        public static IMesh ToIGeometry(this DMesh3 self)
        {
            // TODO: this can be optimized. 
            var verts = self.Vertices().Select(ToMath3d).ToIArray();
            var indices = self.TrianglesBuffer.ToIArray();
            return verts.TriMesh(indices);
        }

        public static Vector3d ToVector3D(this Vector3 self)
            => new Vector3d(self.X, self.Y, self.Z);

        public static Vector3d ToG3Sharp(this Vector3 self)
            => new Vector3d(self.X, self.Y, self.Z);

        // TODO: Ray.Position should be Ray.Origin
        public static Ray3d ToG3Sharp(this Ray self)
            => new Ray3d(self.Position.ToG3Sharp(), self.Direction.ToG3Sharp());

        public static Ray ToNumerics(this Ray3d ray)
            => new Ray(ray.Origin.ToMath3d(), ray.Direction.ToMath3d());

        public static DMesh3 ToG3Sharp(this IMesh self)
        {
            var r = new DMesh3();
            foreach (var v in self.Vertices.ToEnumerable())
                r.AppendVertex(v.ToVector3D());
            var indices = self.Indices;
            for (var i = 0; i < indices.Count; i += 3)
            {
                var result = r.AppendTriangle(indices[i], indices[i + 1], indices[i + 2]);
                if (result < 0)
                {
                    /*
                    if (result == DMesh3.NonManifoldID)
                    {
                        Debug.WriteLine("Couldn't create non-manifold mesh");
                        return null;
                    }
                    */
                    if (result == DMesh3.InvalidID)
                        throw new Exception("Invalid vertex ID");
                }
            }
            //Debug.Assert(r.CheckValidity(false, FailMode.DebugAssert));
            return r;
        }

        public static IMesh Reduce(this IMesh self, float percent)
            => self.ToG3Sharp().Reduce(percent).ToIGeometry();
    }
}
