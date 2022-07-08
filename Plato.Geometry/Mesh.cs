using System;
using System.Reflection.Emit;
using Plato.Math;

namespace Plato.Geometry
{
    public interface IMesh : ITransformable<IMesh>
    {
        IArray<Vertex> Vertices { get; }
        IArray<Int3> FaceVertices { get; }
    }

    public class Face
    {
        public Face(IMesh mesh, int index)
            => (Mesh, Index) = (mesh, index);

        public int Index { get; }
        public IMesh Mesh { get; }
        public Int3 VertexIndices => Mesh.FaceVertices[Index];
        public Triangle Triangle => Mesh.Triangles()[Index];
    }

    public class Vertex : ITransformable<Vertex>
    {
        public Vertex(Vector3 position, Vector3 normal, ColorRGBA color, Vector2 uv)
            => (Position, Normal, Color, UV) = (position, normal, color, uv);

        public Vector3 Position { get; }
        public Vector3 Normal { get; }
        public ColorRGBA Color { get; }
        public Vector2 UV { get; }

        public Vertex Transform(Matrix4x4 mat)
            => new Vertex(Position.Transform(mat), Normal.TransformNormal(mat), Color, UV);
    }

    public class Mesh : IMesh
    {
        public Mesh(IArray<Vertex> vertices, IArray<Int3> faceVertices)
            => (Vertices, FaceVertices) = (vertices, faceVertices);

        public IArray<Vertex> Vertices { get; }
        public IArray<Int3> FaceVertices { get; }

        public IMesh Transform(Matrix4x4 mat)
            => new Mesh(Vertices.Select(v => v.Transform(mat)), FaceVertices);
    }

    public static class MeshExtensions
    {
        public static IArray<int> Indices(this IMesh mesh)
            => mesh.FaceVertices.SelectMany(f => f);

        public static IArray<Vector3> Points(this IMesh mesh)
            => mesh.Vertices.Select(v => v.Position);

        public static IArray<Face> Faces(this IMesh mesh)
            => mesh.FaceVertices.Count.Select(fi => new Face(mesh, fi));

        public static IArray<Triangle> Triangles(this IMesh mesh)
            => mesh.FaceVertices.Select(fv => new Triangle(mesh.Points()[fv.X], mesh.Points()[fv.Y], mesh.Points()[fv.Z]));

        public static IMesh ToIMesh(this QuadMesh mesh)
            => new Mesh(mesh.Vertices,
                mesh.FaceVertices.SelectMany(fv => (new Int3(fv.X, fv.Y, fv.Z), new Int3(fv.Z, fv.W, fv.X))));

        public static IArray<float> SampleFloats(this int n, float max = 1f)
            => n == 1 ? 0f.Unit() : n.Select(i => i * max / (n - 1));

        public static IArray<V> CartesianProduct<T, U, V>(this IArray<U> self, IArray<T> other, Func<T, U, V> func)
            => other.SelectMany(x => self.Select(y => func(x, y)));

        public static QuadMesh ToMesh(this ISurface surface, int cols, int rows)
        {
            if (cols <= 0) throw new ArgumentOutOfRangeException(nameof(cols));
            if (rows <= 0) throw new ArgumentOutOfRangeException(nameof(rows));
            var nx = surface.ClosedX ? cols - 1 : cols;
            var ny = surface.ClosedY ? rows - 1 : rows;
            var us = nx.SampleFloats();
            var vs = ny.SampleFloats();
            var uvs = us.CartesianProduct(vs, (u, v) => new Vector2(u, v));
            var vertices = uvs.Select(surface.GetVertex);

            Int4 QuadMeshFaceVertices(int row, int col)
            {
                var a = row * cols + col;
                var b = row * cols + (col + 1) % cols;
                var c = (row + 1) % rows * cols + (col + 1) % cols;
                var d = (row + 1) % rows * cols + col;
                return (a, b, c, d);
            }

            var faceVertices = (cols - 1).Range().CartesianProduct((rows - 1).Range(), QuadMeshFaceVertices);
            return new QuadMesh(vertices, faceVertices);
        }
    }

    public class QuadMesh : ITransformable<QuadMesh>
    {
        public QuadMesh(IArray<Vertex> vertices, IArray<Int4> faceVertices)
            => (Vertices, FaceVertices) = (vertices, faceVertices);

        public IArray<Vertex> Vertices { get; }
        public IArray<Int4> FaceVertices { get; }

        public QuadMesh Transform(Matrix4x4 mat)
            => new QuadMesh(Vertices.Select(v => v.Transform(mat)), FaceVertices);
    }
}