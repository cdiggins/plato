using System.Collections.Generic;

using Vim.LinqArray;

using Vim.Math3d;

namespace Vim.G3d
{
    // Type has fields True
    // Type has writable fields False
    // Type has public setters False
    public class G3DBuilder
    {
        // A public instance method named ToG3D with a type Vim.G3d.G3D
        // operation kind is Block and type 
        // member references = Attributes, Default
        // assignments = 
        // Written symbols are 
        // Read symbols are (Name=this Kind=Parameter), (Name=header Kind=Parameter)
        // Captured symbols are 
        // Variables declared are 
        
        public G3D ToG3D(G3dHeader? header = null)
        {
            return new G3D(Attributes, header ?? G3dHeader.Default);
        }

        // A public instance method named Add with a type Vim.G3d.G3DBuilder
        // operation kind is Block and type 
        // member references = Attributes
        // assignments = 
        // Written symbols are 
        // Read symbols are (Name=this Kind=Parameter), (Name=attr Kind=Parameter)
        // Captured symbols are 
        // Variables declared are 
        
        public G3DBuilder Add(GeometryAttribute attr)
        {
            Attributes.Add(attr);
            return this;
        }

        // A public instance method named AddIndices with a type Vim.G3d.G3DBuilder
        // operation kind is Block and type 
        // member references = 
        // assignments = 
        // Written symbols are 
        // Read symbols are (Name=this Kind=Parameter), (Name=indices Kind=Parameter)
        // Captured symbols are 
        // Variables declared are 
        
        public G3DBuilder AddIndices(int[] indices)
        {
            return Add(indices.ToIndexAttribute());
        }

        // A public instance method named AddIndices with a type Vim.G3d.G3DBuilder
        // operation kind is Block and type 
        // member references = 
        // assignments = 
        // Written symbols are 
        // Read symbols are (Name=this Kind=Parameter), (Name=indices Kind=Parameter)
        // Captured symbols are 
        // Variables declared are 
        
        public G3DBuilder AddIndices(IArray<int> indices)
        {
            return Add(indices.ToIndexAttribute());
        }

        // A public instance method named SetObjectFaceSize with a type Vim.G3d.G3DBuilder
        // operation kind is Block and type 
        // member references = 
        // assignments = 
        // Written symbols are 
        // Read symbols are (Name=this Kind=Parameter), (Name=objectFaceSize Kind=Parameter)
        // Captured symbols are 
        // Variables declared are 
        
        public G3DBuilder SetObjectFaceSize(int objectFaceSize)
        {
            return Add(new[] { objectFaceSize }.ToIArray().ToObjectFaceSizeAttribute());
        }

        // A public instance method named AddVertices with a type Vim.G3d.G3DBuilder
        // operation kind is Block and type 
        // member references = 
        // assignments = 
        // Written symbols are 
        // Read symbols are (Name=this Kind=Parameter), (Name=vertices Kind=Parameter)
        // Captured symbols are 
        // Variables declared are 
        
        public G3DBuilder AddVertices(IArray<Vector3> vertices)
        {
            return Add(vertices.ToPositionAttribute());
        }

        // A public instance method named ToIGeometryAttributes with a type Vim.G3d.IGeometryAttributes
        // operation kind is Block and type 
        // member references = Attributes
        // assignments = 
        // Written symbols are 
        // Read symbols are (Name=this Kind=Parameter)
        // Captured symbols are 
        // Variables declared are 
        
        public IGeometryAttributes ToIGeometryAttributes()
        {
            return new GeometryAttributes(Attributes);
        }

        // A public instance field named Attributes with a type System.Collections.Generic.List<Vim.G3d.GeometryAttribute>
        // No associated operation
        // No data-flow analysis could be created
                public readonly List<GeometryAttribute> Attributes = new List<GeometryAttribute>();

    } // type
} // namespace
