using System;
using Unity;
using Plato.Math;
using Plato.Geometry;
using UnityEngine;
using Matrix4x4 = Plato.Math.Matrix4x4;
using Quaternion = Plato.Math.Quaternion;
using Ray = Plato.Math.Ray;
using Vector2 = Plato.Math.Vector2;
using Vector3 = Plato.Math.Vector3;
using Vector4 = Plato.Math.Vector4;

namespace Plato.Unity
{
    public static class ToUnityExtensions
    {
        public static void CopyTo<T, U>(this IArray<T> self, U[] other, Func<T, U> f)
        {
            if (other.Length != self.Count)
                Array.Resize(ref other, self.Count);
            for (var i = 0; i < self.Count; ++i)
                other[i] = f(self[i]);
        }

        public static UnityEngine.Mesh ToUnity(this IMesh self, UnityEngine.Mesh mesh)
        {
            //mesh.uv;
            //mesh.colors;
            //mesh.bounds;
            mesh.Clear();
            mesh.vertices = self.Vertices.Select(v => v.Position.ToUnity()).ToArray(); 
            mesh.colors = self.Vertices.Select(v => v.Color.ToUnity()).ToArray();
            mesh.uv = self.Vertices.Select(v => v.UV.ToUnity()).ToArray();
            mesh.normals = self.Vertices.Select(v => v.Normal.ToUnity()).ToArray();
            mesh.triangles = self.Indices().ToArray();
            return mesh;
        }

        public static UnityEngine.Matrix4x4 ToUnity(this Matrix4x4 self)
            => new UnityEngine.Matrix4x4(self.GetColumn(0).ToUnity(), self.GetColumn(1).ToUnity(), self.GetColumn(2).ToUnity(),
                self.GetColumn(3).ToUnity());

        public static UnityEngine.Transform ToUnity(this Matrix4x4 self, UnityEngine.Transform transform)
        {
            var mat = self.ToUnity();
            var scl = mat.lossyScale;
            var rot = mat.rotation;
            var pos = new UnityEngine.Vector3(mat[0, 3], mat[1, 3], mat[2, 3]);
            transform.position = pos;
            transform.rotation = rot;
            scl.x /= transform.localScale.x;
            scl.y /= transform.localScale.y;
            scl.z /= transform.localScale.z;
            transform.localScale = scl;
            return transform;
        }

        public static UnityEngine.Bounds ToUnity(this Box self)
            => new Bounds(self.Center.ToUnity(), self.Extent.ToUnity());

        public static UnityEngine.BoundingSphere ToUnity(this Sphere self)
            => new BoundingSphere(self.Center.ToUnity(), self.Radius);

        public static UnityEngine.Quaternion ToUnity(this Quaternion q)
            => new UnityEngine.Quaternion(q.X, q.Y, q.Z, q.W);

        public static UnityEngine.Vector4 ToUnity(this Vector4 v)
            => new UnityEngine.Vector4(v.X, v.Y, v.Z, v.W);

        public static UnityEngine.Vector3 ToUnity(this Vector3 v)
            => new UnityEngine.Vector3(v.X, v.Y, v.Z);

        public static UnityEngine.Vector2 ToUnity(this Vector2 v)
            => new UnityEngine.Vector2(v.X, v.Y);

        public static UnityEngine.Color ToUnity(this ColorHDR self)
            => new Color(self.R, self.G, self.B, self.A);

        public static UnityEngine.Color ToUnity(this ColorRGB self)
            => self.ToHdr().ToUnity();

        public static UnityEngine.Color ToUnity(this ColorRGBA self)
            => self.ToHdr().ToUnity();

        public static UnityEngine.Ray ToUnity(this Ray self)
            => new UnityEngine.Ray(self.Position.ToUnity(), self.Direction.ToUnity());

        //public static UnityEngine
        // Line
        // TriMesh
        // Points
        // Surface
        // Texture/Bitmap
        // 
    }
}