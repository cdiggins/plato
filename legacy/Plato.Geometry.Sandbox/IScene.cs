using System.Collections;
using System.Collections.Generic;
using Vim.LinqArray;
using Vim.Math3d;

namespace Vim.Geometry
{
    /// <summary>
    /// An IScene is a generic representation of a 3D scene graph.
    /// </summary>
    public interface IScene
    {
        IArray<ISceneNode> Nodes { get; }
        IArray<IMesh> Meshes { get; }
    }

    /// <summary>
    /// A node in a scene graph. 
    /// </summary>
    public interface ISceneNode
    {
        int Id { get; }
        IScene Scene { get; }
        Matrix4x4 Transform { get; }
        int MeshIndex { get; }
        IMesh GetMesh();
        ISceneNode Parent { get; }

        // TODO: DEPRECATE: this needs to be removed, currently only used in Vim.Max.Bridge.
        IArray<ISceneNode> Children { get; }
    }

    public class SceneNodeComparer : EqualityComparer<ISceneNode>, IComparer<ISceneNode>
    {
        public static readonly SceneNodeComparer Instance = new SceneNodeComparer();

        public int Compare(ISceneNode x, ISceneNode y)
            => x.Id - y.Id;
        public override bool Equals(ISceneNode x, ISceneNode y)
            => x.Id == y.Id;
        public override int GetHashCode(ISceneNode obj)
            => obj.Id;
    }
}
