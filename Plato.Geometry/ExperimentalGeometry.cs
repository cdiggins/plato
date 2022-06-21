using System.Numerics;

namespace Plato.Geometry
{
    public class Int3
    {
        public int X { get; }
        public int Y { get; }
        public int Z { get; }
    }

    public class Vector3x3
    {
        public Vector3 A { get; }
        public Vector3 B { get; }
        public Vector3 C { get; }
    }

    public interface IArray<T> { }

    public enum Association
    {
        Vertex,
        Face, 
        Mesh,
        Instance,
    }

    public enum AttributeType
    {
        Float32,
        Float64,
        Int8,
        Int16,
        Int32,
        Int64,
        UInt8,
        IUnt16,
        UInt32,
        UInt64,
        String,
        Boolean,
    }

    public class Bytes { }
    
    public interface IAttribute
    {
        Bytes Bytes { get; }
        int Arity { get; }
        Association Association { get; }
        int TypeSize { get; }
        AttributeType AttributeType { get; }
    }

    public interface IMeshData
    {
        IArray<Vector3> Vertices { get; }
        IArray<Int3> Faces { get; }
        IArray<Vector3>? Colors { get; }
        IArray<IArray<Vector2>>? UVs { get; }
        IArray<Vector3>? VertexNormals { get; }
        IArray<Vector3>? FaceNormals { get; }
    }

    public interface ISubMeshData
    { 
        IArray<int> Materials { get; }
        IArray<int> VertexOffsets { get; }
        IArray<int> IndexOffset { get;  }
    }

    public interface IVertexData
    {
        Vector3 Position { get; }
        Vector3 Color { get; }
        int Index { get; }
        float Transparency { get; }
        Vector3 Normal { get; }
        Vector3 Tangent { get; }
        Vector3 Binormal { get; }
        Vector2 Uv { get; }
        IArray<Vector2> Uvs { get; }
    }

    public interface IInstanceData
    {
        Matrix4x4 Transform { get; }
        int Parent { get; }
        int Index { get; }
        int MeshIndex { get; }
    }

    public interface IFaceData
    {
        Int3 Indices { get; }
        int Index { get; }
        Vector3 Normal { get; }
        Vector3x3 Vertices { get; }
    }



    /*
    public interface IAttribute
    {
        IArra
    }*/

    public interface IScene
    {
        IArray<IMeshData> Meshes { get; }
        IArray<Matrix4x4> Transforms { get; }
        IArray<int> ParentNodes { get; }
    }


    public class UvParameterization
    {
        Func<Vector2, Vector3> UvToPos { get; }
    }

    public class SignedDistanceField
    {
        Func<Vector3, double> Distance { get; }
    }
}