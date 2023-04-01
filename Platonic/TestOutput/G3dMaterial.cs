using Vim.Math3d;

namespace Vim.G3d
{
    // Type has fields True
    // Type has writable fields False
    // Type has public setters False
    public class G3dMaterial
    {
        // A public instance method named .ctor with a type void
        // operation kind is Block and type 
        // member references = G3d, Index
        // assignments = ParameterReference, ParameterReference
        // Written symbols are 
        // Read symbols are (Name=this Kind=Parameter), (Name=g3D Kind=Parameter), (Name=index Kind=Parameter)
        // Captured symbols are 
        // Variables declared are 
        
        public G3dMaterial(G3D g3D, int index)
        {
            G3d = g3D;
            Index = index;
        }

        // A public instance field named G3d with a type Vim.G3d.G3D
        // No associated operation
        // No data-flow analysis could be created
                public readonly G3D G3d;

        // A public instance field named Index with a type int
        // No associated operation
        // No data-flow analysis could be created
                public readonly int Index;

        // A public instance property named Color with a type Vim.Math3d.Vector4
        // operation kind is PropertyReference and type Vim.Math3d.Vector4
        // member references = this[], MaterialColors, G3d, Index
        // assignments = 
        // Written symbols are 
        // Read symbols are (Name=this Kind=Parameter)
        // Captured symbols are 
        // Variables declared are 
        
        public Vector4 Color => G3d.MaterialColors[Index];

        // A public instance property named Glossiness with a type float
        // operation kind is Coalesce and type float
        // member references = G3d, this[], MaterialGlossiness, Index
        // assignments = 
        // Written symbols are 
        // Read symbols are (Name=this Kind=Parameter)
        // Captured symbols are 
        // Variables declared are 
                public float Glossiness => G3d?.MaterialGlossiness[Index] ?? 0f;

        // A public instance property named Smoothness with a type float
        // operation kind is Coalesce and type float
        // member references = G3d, this[], MaterialSmoothness, Index
        // assignments = 
        // Written symbols are 
        // Read symbols are (Name=this Kind=Parameter)
        // Captured symbols are 
        // Variables declared are 
                public float Smoothness => G3d?.MaterialSmoothness[Index] ?? 0f;

    } // type
} // namespace
