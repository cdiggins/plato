using Plato.Math;

namespace Plato.Geometry
{
    public interface IScene
    {
        IArray<INode> Nodes { get; }
        IArray<IMesh> Meshes { get; }
        IArray<ColorRGBA> MaterialColors { get; }
    }

    public interface IMesh
    {
        int Id { get; }
        IArray<IVertex> Vertices { get; }
        IArray<IFace> Faces { get; }
    }

    public interface IMeshTopology
    { 
        // TODO: this is extra work to create 
        IMap<int, IArray<IFace>> VertexToFace { get;}
    }

    public interface IFace
    {
        int Id { get; }
        IArray<IVertex> Vertices { get; }
        int MaterialID { get; }
    }

    public interface IVertex
    {
        int Id { get; }
        Vector3 Normal { get; }
        Vector3 Position { get; }
        ColorRGBA Color { get; }
        Vector2 UV { get; }        
    }

    public interface INode
    {
        public int Id { get; }
        public int ParentId { get; }
        public Matrix4x4 Transform { get; }
        public Pose Pose { get; }
        public IMesh? Mesh { get; }
    }
}
