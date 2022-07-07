using System;
using Plato.Geometry;
using Plato.Math;

namespace Plato.Unity
{
    public static class ToPlatoExtensions
    {
        public static IArray<U> ToPlato<T, U>(this T[] self, Func<T, U> f)
            => self.ToIArray().Select(f);

        public static IMesh ToPlato(this UnityEngine.Mesh mesh)
        {

            //mesh.vertices.ToIArray().Select(ToPlato);
            //mesh.
            throw new NotImplementedException();
        }

        public static Matrix4x4 ToPlato(this UnityEngine.Matrix4x4 self)
            => Matrix4x4.CreateFromRows(self.GetRow(0).ToPlato(), self.GetRow(1).ToPlato(), self.GetRow(2).ToPlato(), self.GetRow(3).ToPlato());

        public static Matrix4x4 ToPlato(this UnityEngine.Transform transform)
            => transform.localToWorldMatrix.ToPlato();

        public static Box ToPlato(this UnityEngine.Bounds self)
            => (self.min.ToPlato(), self.max.ToPlato());

        public static Sphere ToPlato(this UnityEngine.BoundingSphere self)
            => (self.position.ToPlato(), self.radius);

        public static Quaternion ToPlato(this UnityEngine.Quaternion q)
            => (q.x, q.y, q.z, q.w);

        public static Vector4 ToPlato(this UnityEngine.Vector4 v)
            => (v.x, v.y, v.z, v.w);

        public static Vector3 ToPlato(this UnityEngine.Vector3 v)
            => (v.x, v.y, v.z);

        public static Vector2 ToPlato(this UnityEngine.Vector2 v)
            => (v.x, v.y);

        public static ColorHDR ToPlato(this UnityEngine.Color self)
            => (self.r, self.g, self.b, self.a);

        public static Ray ToPlato(this UnityEngine.Ray self)
            => (self.origin.ToPlato(), self.direction.ToPlato());

        //public static UnityEngine
        // Line
        // TriMesh
        // Points
        // Surface
        // Texture/Bitmap
        // 
    }
}