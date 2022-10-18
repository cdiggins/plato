using Plato.Math;

// https://math.stackexchange.com/questions/192055/what-is-a-geometry
// TODO:
// do I want to make the whole thing contingent on some constants? Like unit of measure, tolerance, precision of float
// do I want the id to be a special type, instead of just "int"?
// maybe I want some code generation for types that have just one value? For casting operators and things like that.
// make it easier. 

// Big queries should have some special questions: "HasTopologicalInfo" : "ComputeTopologicalInfo" (which gives us a new version, with that information cached)
// When it is an interface, then we can rely on it. 
// If it is an interface ... what do we do? We would like it to be made available to others. 
// We would like it to sit on the object, so that it is cleaned up with the object. Otherwise, it gets tricky. 
// Updating the structure doesn't always help ... other functions won't see it. Might as well make it explicit to create, and work from 
// there. However ... the funky version with it computed, can also implement the ISurface interface .... etc. 


namespace Plato.Geometry
{
    public interface IScene3D
    {
        IArray<IObject3D> Objects { get; }
        IArray<IMesh> Meshes { get; }
    }

    public interface IMaterial
    {
        int Id { get; }
        ColorRGBA Color { get; }
        string Name { get; }
    }

    public interface IArrayElement<T>
    {
        IArray<T> Array { get; }
        int Index { get; }
    }

    public interface IMeshInstance
    {
        IArray<IMesh> Meshes { get; }
        int MeshIndex { get; }
    }

    public interface ITransformedMesh
    {
        IMesh Mesh { get; }
        Matrix4x4 Transform { get; }
    }

    public interface IObject3D : ITransformedMesh, IMeshInstance
    {
    }

    public class Scene3D : IScene3D
    {
        public Scene3D(IArray<IMesh> meshes, IArray<(int MeshIndex, Matrix4x4 Transform)> nodeData)
            => (Meshes, _nodeData) = (meshes, nodeData);

        private readonly IArray<(int MeshIndex, Matrix4x4 Transform)> _nodeData;
        public IArray<IObject3D> Objects => _nodeData.Select(nd => (IObject3D)new Object3D(Meshes, nd.MeshIndex, nd.Transform));
        public IArray<IMesh> Meshes { get; }
    }

    public class Object3D : IObject3D
    {
        public IMesh Mesh => Meshes[MeshIndex];
        public int MeshIndex { get; }
        public Matrix4x4 Transform { get; }
        public IArray<IMesh> Meshes { get; }

        public Object3D(IArray<IMesh> meshes, int index, Matrix4x4 matrix)
            => (Meshes, MeshIndex, Transform) = (meshes, index, matrix);
    }

    public interface IVolume
    {
        float Sample(Vector3 point);
    }

    public interface IQuadMesh : ISurface
    {
        IArray<Vertex> Vertices { get; }
        IArray<Int4> Faces { get; }
    }

    public interface IPoints : IArray<Vector3>, ICurve<Vector3>
    { }

}
