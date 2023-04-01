using Vim.LinqArray;

using Vim.Math3d;

namespace Vim.G3d
{
    // Type has fields True
    // Type has writable fields False
    // Type has public setters False
    public class G3dShape
    {
        // A public instance method named .ctor with a type void
        // operation kind is Block and type 
        // member references = G3D, Index, Vertices, ShapeVertices, G3D, ShapeVertexOffset, NumVertices
        // assignments = Conversion, ConditionalAccess
        // Written symbols are 
        // Read symbols are (Name=this Kind=Parameter), (Name=parent Kind=Parameter), (Name=index Kind=Parameter)
        // Captured symbols are 
        // Variables declared are 
        
        public G3dShape(G3D parent, int index)
        {
            (G3D, Index) = (parent, index);
            Vertices = G3D.ShapeVertices?.SubArray(ShapeVertexOffset, NumVertices);
        }

        // A public instance field named G3D with a type Vim.G3d.G3D
        // No associated operation
        // No data-flow analysis could be created
                public readonly G3D G3D;

        // A public instance field named Index with a type int
        // No associated operation
        // No data-flow analysis could be created
                public readonly int Index;

        // A public instance field named Vertices with a type Vim.LinqArray.IArray<Vim.Math3d.Vector3>
        // No associated operation
        // No data-flow analysis could be created
                public readonly IArray<Vector3> Vertices;

        // A public instance property named ShapeVertexOffset with a type int
        // operation kind is PropertyReference and type int
        // member references = this[], ShapeVertexOffsets, G3D, Index
        // assignments = 
        // Written symbols are 
        // Read symbols are (Name=this Kind=Parameter)
        // Captured symbols are 
        // Variables declared are 
        
        public int ShapeVertexOffset => G3D.ShapeVertexOffsets[Index];

        // A public instance property named NumVertices with a type int
        // operation kind is PropertyReference and type int
        // member references = this[], ShapeVertexCounts, G3D, Index
        // assignments = 
        // Written symbols are 
        // Read symbols are (Name=this Kind=Parameter)
        // Captured symbols are 
        // Variables declared are 
                public int NumVertices => G3D.ShapeVertexCounts[Index];

        // A public instance property named Color with a type Vim.Math3d.Vector4
        // operation kind is PropertyReference and type Vim.Math3d.Vector4
        // member references = this[], ShapeColors, G3D, Index
        // assignments = 
        // Written symbols are 
        // Read symbols are (Name=this Kind=Parameter)
        // Captured symbols are 
        // Variables declared are 
                public Vector4 Color => G3D.ShapeColors[Index];

        // A public instance property named Width with a type float
        // operation kind is PropertyReference and type float
        // member references = this[], ShapeWidths, G3D, Index
        // assignments = 
        // Written symbols are 
        // Read symbols are (Name=this Kind=Parameter)
        // Captured symbols are 
        // Variables declared are 
                public float Width => G3D.ShapeWidths[Index];

    } // type
} // namespace
