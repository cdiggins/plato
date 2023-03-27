using System;

namespace Vim.G3d
{
    /// <summary>
    /// Provides information about identifying the role and parsing the data within an attribute data buffer.
    /// This is encoded using a string in a particular URN form. 
    /// </summary>
    public class AttributeDescriptor
    {
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

        public Association Association { get; }
        public string Semantic { get; }
        public DataType DataType { get; }
        public int DataArity { get; }
        public int Index { get; }

        public int DataElementSize { get; }
        public int DataTypeSize { get; }
        public string Name { get; }

        public string AssociationString
            => Association.ToString().Substring("assoc_".Length);

        public string DataTypeString
            => DataType.ToString()?.Substring("dt_".Length) ?? null;

        /// <summary>
        /// Generates a URN representation of the attribute descriptor
        /// </summary>
        public override string ToString()
        {
            return Name;
        }

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

        public bool Validate()
        {
            var urn = ToString();
            var tmp = Parse(urn);
            if (!Equals(tmp))
                throw new Exception("Invalid attribute descriptor (or internal error in the parsing/string conversion");
            return true;
        }

        public bool Equals(AttributeDescriptor other)
        {
            return ToString() == other.ToString();
        }

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

        public static DataType ParseDataType(string s)
        {
            return (DataType)Enum.Parse(typeof(DataType), "dt_" + s);
        }

        public AttributeDescriptor SetIndex(int index)
        {
            return new AttributeDescriptor(Association, Semantic, DataType, DataArity, index);
        }
    }
}