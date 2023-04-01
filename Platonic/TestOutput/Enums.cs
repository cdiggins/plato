using System;

namespace Vim.G3d
{
        /// <summary>
    /// The different types of data types that can be used as elements.
    /// </summary> 
    public enum DataType
    {
        dt_uint8,
        dt_int8,
        dt_uint16,
        dt_int16,
        dt_uint32,
        dt_int32,
        dt_uint64,
        dt_int64,
        dt_float32,
        dt_float64,
        dt_string
    }

}
namespace Vim.G3d
{
    
    /// <summary>
    /// What element or component of a mesh each attribute is associated with.  
    /// </summary>  
    public enum Association
    {
        assoc_all, // associated with all data in G3d
        assoc_none, // no association
        assoc_vertex, // vertex or point data 
        assoc_face, // face associated data
        assoc_corner, // corner (aka face-vertex) data. A corner is associated with one vertex, but a vertex may be shared between multiple corners  
        assoc_edge, // half-edge data. Each face consists of n half-edges, one per corner. A half-edge, is a directed edge
        assoc_instance, // instance information 
        assoc_shapevertex, // flattened shape vertex collection.
        assoc_shape, // shape instance
        assoc_material, // material properties
        assoc_mesh,
        assoc_submesh
    }

}
namespace Vim.G3d
{
    
    [Flags]
    public enum InstanceFlags
    {
        /// <summary>
        /// Default - no instance options defined.
        /// </summary>
        None = 0,

        /// <summary>
        /// When enabled, indicates that the renderer (or the consuming application) should hide
        /// the instance by default.
        /// </summary>
        Hidden = 1
    }

}
namespace Vim.G3d
{
    // Type has fields True
    // Type has writable fields False
    // Type has public setters False
    public static class Semantic
    {
        // A public instance field named Position with a type string
        // No associated operation
        // No data-flow analysis could be created
                public const string Position = "position";

        // A public instance field named Index with a type string
        // No associated operation
        // No data-flow analysis could be created
                public const string Index = "index";

        // A public instance field named FaceSize with a type string
        // No associated operation
        // No data-flow analysis could be created
                public const string FaceSize = "facesize";

        // A public instance field named Uv with a type string
        // No associated operation
        // No data-flow analysis could be created
                public const string Uv = "uv";

        // A public instance field named Normal with a type string
        // No associated operation
        // No data-flow analysis could be created
                public const string Normal = "normal";

        // A public instance field named Color with a type string
        // No associated operation
        // No data-flow analysis could be created
                public const string Color = "color";

        // A public instance field named Bitangent with a type string
        // No associated operation
        // No data-flow analysis could be created
                public const string Bitangent = "bitangent";

        // A public instance field named Tangent with a type string
        // No associated operation
        // No data-flow analysis could be created
                public const string Tangent = "tangent";

        // A public instance field named Weight with a type string
        // No associated operation
        // No data-flow analysis could be created
                public const string Weight = "weight";

        // A public instance field named Width with a type string
        // No associated operation
        // No data-flow analysis could be created
                public const string Width = "width";

        // A public instance field named Material with a type string
        // No associated operation
        // No data-flow analysis could be created
        
        // Usually associated with face.
        public const string Material = "material";

        // A public instance field named Glossiness with a type string
        // No associated operation
        // No data-flow analysis could be created
        
        // Usually associated with material.
        public const string Glossiness = "glossiness";

        // A public instance field named Smoothness with a type string
        // No associated operation
        // No data-flow analysis could be created
        
        public const string Smoothness = "smoothness";

        // A public instance field named IndexOffset with a type string
        // No associated operation
        // No data-flow analysis could be created
        
        // Usually associated with meshes and submeshes
        public const string IndexOffset = "indexoffset";

        // A public instance field named VertexOffset with a type string
        // No associated operation
        // No data-flow analysis could be created
                public const string VertexOffset = "vertexoffset";

        // A public instance field named Subgeometry with a type string
        // No associated operation
        // No data-flow analysis could be created
        
        // Usually associated with instances
        public const string Subgeometry = "subgeometry";

        // A public instance field named Mesh with a type string
        // No associated operation
        // No data-flow analysis could be created
                public const string Mesh = "mesh";

        // A public instance field named Parent with a type string
        // No associated operation
        // No data-flow analysis could be created
                public const string Parent = "parent";

        // A public instance field named Transform with a type string
        // No associated operation
        // No data-flow analysis could be created
                public const string Transform = "transform";

        // A public instance field named Flags with a type string
        // No associated operation
        // No data-flow analysis could be created
                public const string Flags = "flags";

        // A public instance field named TangentInt with a type string
        // No associated operation
        // No data-flow analysis could be created
        
        public const string TangentInt = "tangentin";

        // A public instance field named TangentOut with a type string
        // No associated operation
        // No data-flow analysis could be created
                public const string TangentOut = "tangentout";

        // A public instance field named SubMeshOffset with a type string
        // No associated operation
        // No data-flow analysis could be created
        
        public const string SubMeshOffset = "submeshoffset";

    } // type
} // namespace
