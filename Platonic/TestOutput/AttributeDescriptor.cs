using System;

namespace Vim.G3d
{
    // Type has fields False
    // Type has writable fields False
    // Type has public setters False
    public class AttributeDescriptor
    {
        // A public instance method named .ctor with a type void
        // operation kind is Block and type 
        // member references = Association, Semantic, DataType, DataArity, Index, DataTypeSize, DataType, DataElementSize, DataTypeSize, DataArity, Name, AssociationString, Semantic, Index, DataTypeString, DataArity
        // assignments = ParameterReference, ParameterReference, ParameterReference, ParameterReference, ParameterReference, Invocation, Binary, InterpolatedString
        // Written symbols are 
        // Read symbols are (Name=this Kind=Parameter), (Name=association Kind=Parameter), (Name=semantic Kind=Parameter), (Name=dataType Kind=Parameter), (Name=dataArity Kind=Parameter), (Name=index Kind=Parameter)
        // Captured symbols are 
        // Variables declared are 
                public AttributeDescriptor(Association association, string semantic, DataType dataType, int dataArity,
            int index = 0)
        {
            Association = association;
            if (semantic.Contains(":"))
                throw new Exception("The semantic must not contain a semicolon");
            Semantic = semantic;
            DataType = dataType;
            DataArity = dataArity;
            Index = index;
            DataTypeSize = GetDataTypeSize(DataType);
            DataElementSize = DataTypeSize * DataArity;
            Name = $"g3d:{AssociationString}:{Semantic}:{Index}:{DataTypeString}:{DataArity}";
        }

        // A public instance method named ToString with a type string
        // operation kind is Block and type 
        // member references = Name
        // assignments = 
        // Written symbols are 
        // Read symbols are (Name=this Kind=Parameter)
        // Captured symbols are 
        // Variables declared are 
        
        /// <summary>
        /// Generates a URN representation of the attribute descriptor
        /// </summary>
        public override string ToString()
        {
            return Name;
        }

        // A public static method named TryParse with a type bool
        // operation kind is Block and type 
        // member references = 
        // assignments = Conversion, Invocation
        // Written symbols are (Name=attributeDescriptor Kind=Parameter)
        // Read symbols are (Name=urn Kind=Parameter), (Name=attributeDescriptor Kind=Parameter)
        // Captured symbols are 
        // Variables declared are 
        
        /// <summary>
        /// Returns true if the attribute descriptor has been successfully parsed.
        /// </summary>
        public static bool TryParse(string urn, out AttributeDescriptor attributeDescriptor)
        {
            attributeDescriptor = null;
            try
            {
                attributeDescriptor = Parse(urn);
            }
            catch
            {
                // do nothing.
            }

            return attributeDescriptor != null;
        }

        // A public static method named Parse with a type Vim.G3d.AttributeDescriptor
        // operation kind is Block and type 
        // member references = Length
        // assignments = 
        // Written symbols are (Name=vals Kind=Local)
        // Read symbols are (Name=urn Kind=Parameter), (Name=vals Kind=Local)
        // Captured symbols are 
        // Variables declared are (Name=vals Kind=Local)
        
        /// <summary>
        /// Parses a URN representation of the attribute descriptor to generate an actual attribute descriptor 
        /// </summary>
        public static AttributeDescriptor Parse(string urn)
        {
            var vals = urn.Split(':');
            if (vals.Length != 6) throw new Exception("Expected 6 parts to the attribute descriptor URN");
            if (vals[0] != "g3d") throw new Exception("First part of URN must be g3d");
            return new AttributeDescriptor(
                ParseAssociation(vals[1]),
                vals[2],
                ParseDataType(vals[4]),
                int.Parse(vals[5]),
                int.Parse(vals[3])
            );
        }

        // A public instance method named Validate with a type bool
        // operation kind is Block and type 
        // member references = 
        // assignments = 
        // Written symbols are (Name=urn Kind=Local), (Name=tmp Kind=Local)
        // Read symbols are (Name=this Kind=Parameter), (Name=urn Kind=Local), (Name=tmp Kind=Local)
        // Captured symbols are 
        // Variables declared are (Name=urn Kind=Local), (Name=tmp Kind=Local)
        
        public bool Validate()
        {
            var urn = ToString();
            var tmp = Parse(urn);
            if (!Equals(tmp))
                throw new Exception("Invalid attribute descriptor (or internal error in the parsing/string conversion");
            return true;
        }

        // A public instance method named Equals with a type bool
        // operation kind is Block and type 
        // member references = 
        // assignments = 
        // Written symbols are 
        // Read symbols are (Name=this Kind=Parameter), (Name=other Kind=Parameter)
        // Captured symbols are 
        // Variables declared are 
        
        public bool Equals(AttributeDescriptor other)
        {
            return ToString() == other.ToString();
        }

        // A public static method named GetDataTypeSize with a type int
        // operation kind is Block and type 
        // member references = dt_uint8, dt_int8, dt_uint16, dt_int16, dt_uint32, dt_int32, dt_uint64, dt_int64, dt_float32, dt_float64
        // assignments = 
        // Written symbols are 
        // Read symbols are (Name=dt Kind=Parameter)
        // Captured symbols are 
        // Variables declared are 
        
        public static int GetDataTypeSize(DataType dt)
        {
            switch (dt)
            {
                case DataType.dt_uint8:
                case DataType.dt_int8:
                    return 1;
                case DataType.dt_uint16:
                case DataType.dt_int16:
                    return 2;
                case DataType.dt_uint32:
                case DataType.dt_int32:
                    return 4;
                case DataType.dt_uint64:
                case DataType.dt_int64:
                    return 8;
                case DataType.dt_float32:
                    return 4;
                case DataType.dt_float64:
                    return 8;
                default:
                    throw new ArgumentOutOfRangeException(nameof(dt), dt, null);
            }
        }

        // A public static method named ParseAssociation with a type Vim.G3d.Association
        // operation kind is Block and type 
        // member references = assoc_all, assoc_corner, assoc_edge, assoc_face, assoc_instance, assoc_vertex, assoc_shapevertex, assoc_shape, assoc_material, assoc_mesh, assoc_submesh, assoc_none
        // assignments = 
        // Written symbols are 
        // Read symbols are (Name=s Kind=Parameter)
        // Captured symbols are 
        // Variables declared are 
        
        public static Association ParseAssociation(string s)
        {
            switch (s)
            {
                case "all":
                    return Association.assoc_all;
                case "corner":
                    return Association.assoc_corner;
                case "edge":
                    return Association.assoc_edge;
                case "face":
                    return Association.assoc_face;
                case "instance":
                    return Association.assoc_instance;
                case "vertex":
                    return Association.assoc_vertex;
                case "shapevertex":
                    return Association.assoc_shapevertex;
                case "shape":
                    return Association.assoc_shape;
                case "material":
                    return Association.assoc_material;
                case "mesh":
                    return Association.assoc_mesh;
                case "submesh":
                    return Association.assoc_submesh;

                // Anything else we just treat as unknown 
                default:
                    return Association.assoc_none;
            }
        }

        // A public static method named ParseDataType with a type Vim.G3d.DataType
        // operation kind is Block and type 
        // member references = 
        // assignments = 
        // Written symbols are 
        // Read symbols are (Name=s Kind=Parameter)
        // Captured symbols are 
        // Variables declared are 
        
        public static DataType ParseDataType(string s)
        {
            return (DataType)Enum.Parse(typeof(DataType), "dt_" + s);
        }

        // A public instance method named SetIndex with a type Vim.G3d.AttributeDescriptor
        // operation kind is Block and type 
        // member references = Association, Semantic, DataType, DataArity
        // assignments = 
        // Written symbols are 
        // Read symbols are (Name=this Kind=Parameter), (Name=index Kind=Parameter)
        // Captured symbols are 
        // Variables declared are 
        
        public AttributeDescriptor SetIndex(int index)
        {
            return new AttributeDescriptor(Association, Semantic, DataType, DataArity, index);
        }

        // A public instance property named Association with a type Vim.G3d.Association
        // No associated operation
        // No data-flow analysis could be created
        
        public Association Association { get; }

        // A public instance property named Semantic with a type string
        // No associated operation
        // No data-flow analysis could be created
                public string Semantic { get; }

        // A public instance property named DataType with a type Vim.G3d.DataType
        // No associated operation
        // No data-flow analysis could be created
                public DataType DataType { get; }

        // A public instance property named DataArity with a type int
        // No associated operation
        // No data-flow analysis could be created
                public int DataArity { get; }

        // A public instance property named Index with a type int
        // No associated operation
        // No data-flow analysis could be created
                public int Index { get; }

        // A public instance property named DataElementSize with a type int
        // No associated operation
        // No data-flow analysis could be created
        
        public int DataElementSize { get; }

        // A public instance property named DataTypeSize with a type int
        // No associated operation
        // No data-flow analysis could be created
                public int DataTypeSize { get; }

        // A public instance property named Name with a type string
        // No associated operation
        // No data-flow analysis could be created
                public string Name { get; }

        // A public instance property named AssociationString with a type string
        // operation kind is Invocation and type string
        // member references = Association, Length
        // assignments = 
        // Written symbols are 
        // Read symbols are (Name=this Kind=Parameter)
        // Captured symbols are 
        // Variables declared are 
        
        public string AssociationString
            => Association.ToString().Substring("assoc_".Length);

        // A public instance property named DataTypeString with a type string
        // operation kind is Coalesce and type string
        // member references = DataType, Length
        // assignments = 
        // Written symbols are 
        // Read symbols are (Name=this Kind=Parameter)
        // Captured symbols are 
        // Variables declared are 
        
        public string DataTypeString
            => DataType.ToString()?.Substring("dt_".Length) ?? null;

    } // type
} // namespace
