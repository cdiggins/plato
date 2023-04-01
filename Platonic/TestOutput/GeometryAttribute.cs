using System;

using System.Collections.Generic;

using System.IO;

using System.Linq;

using Vim.BFast;

using Vim.LinqArray;

using Vim.Math3d;

namespace Vim.G3d
{
    // Type has fields False
    // Type has writable fields False
    // Type has public setters False
    public abstract class GeometryAttribute
    {
        // A private instance method named .ctor with a type void
        // operation kind is Block and type 
        // member references = Descriptor, ElementCount
        // assignments = Conversion
        // Written symbols are 
        // Read symbols are (Name=this Kind=Parameter), (Name=descriptor Kind=Parameter), (Name=count Kind=Parameter)
        // Captured symbols are 
        // Variables declared are 
                /// <summary>
        /// Constructor.
        /// </summary>
        protected GeometryAttribute(AttributeDescriptor descriptor, int count)
        {
            (Descriptor, ElementCount) = (descriptor, count);
        }

        // A public instance method named Merge with a type Vim.G3d.GeometryAttribute
        // No associated operation
        // No data-flow analysis could be created
        
        /// <summary>
        /// Multiple mesh attributes can be merged together if they have the same
        /// underlying descriptor and data type. 
        /// </summary>
        public abstract GeometryAttribute Merge(IEnumerable<GeometryAttribute> others);

        // A public instance method named Remap with a type Vim.G3d.GeometryAttribute
        // No associated operation
        // No data-flow analysis could be created
        
        /// <summary>
        /// A mesh attribute can be remapped, using the given indices. 
        /// </summary>
        public abstract GeometryAttribute Remap(IArray<int> indices);

        // A public instance method named ToBuffer with a type Vim.BFast.INamedBuffer
        // No associated operation
        // No data-flow analysis could be created
        
        /// <summary>
        /// Converted to an INamedBuffer which consists of a name and an array of unmanaged types. 
        /// </summary>
        public abstract INamedBuffer ToBuffer();

        // A public instance method named IsType with a type bool
        // operation kind is Block and type 
        // member references = 
        // assignments = 
        // Written symbols are 
        // Read symbols are (Name=this Kind=Parameter)
        // Captured symbols are 
        // Variables declared are 
        
        /// <summary>
        /// Convenience function to check if this object is a mesh attribute of the given type.
        /// </summary>
        public bool IsType<T>() 
        {
            return this is GeometryAttribute<T>;
        }

        // A public instance method named IsTypeAndAssociation with a type bool
        // operation kind is Block and type 
        // member references = Association, Descriptor
        // assignments = 
        // Written symbols are 
        // Read symbols are (Name=this Kind=Parameter), (Name=assoc Kind=Parameter)
        // Captured symbols are 
        // Variables declared are 
        
        /// <summary>
        /// Convenience function to check if this object is a mesh attribute of the given type, and the association matches.
        /// </summary>
        public bool IsTypeAndAssociation<T>(Association assoc) 
        {
            return Descriptor.Association == assoc && this is GeometryAttribute<T>;
        }

        // A public instance method named AsType with a type Vim.G3d.GeometryAttribute<T>
        // operation kind is Block and type 
        // member references = 
        // assignments = 
        // Written symbols are 
        // Read symbols are (Name=this Kind=Parameter)
        // Captured symbols are 
        // Variables declared are 
        
        /// <summary>
        /// Convenience function to cast this object into a mesh attribute of the given type, throwing an exception if not possible, 
        /// </summary>
        public GeometryAttribute<T> AsType<T>() 
        {
            return this as GeometryAttribute<T> ??
                   throw new Exception($"The type of the attribute is {GetType()} not MeshAttribute<{typeof(T)}>");
        }

        // A public instance method named Read with a type Vim.G3d.GeometryAttribute
        // No associated operation
        // No data-flow analysis could be created
        
        /// <summary>
        /// Loads the correct typed data from a Stream.
        /// </summary>
        public abstract GeometryAttribute Read(Stream stream, long byteCount);

        // A public instance method named SetIndex with a type Vim.G3d.GeometryAttribute
        // No associated operation
        // No data-flow analysis could be created
        
        /// <summary>
        /// Creates a new GeometryAttribute with the same data, but with a different index. Useful when constructing attributes 
        /// </summary>
        public abstract GeometryAttribute SetIndex(int index);

        // A public instance property named Descriptor with a type Vim.G3d.AttributeDescriptor
        // No associated operation
        // No data-flow analysis could be created
        
        /// <summary>
        /// The descriptor contains information about the data contained in the attribute:
        /// * the primitive data type
        /// * the arity
        /// * the association
        /// * the semantic 
        /// </summary>
        public AttributeDescriptor Descriptor { get; }

        // A public instance property named Name with a type string
        // operation kind is PropertyReference and type string
        // member references = Name, Descriptor
        // assignments = 
        // Written symbols are 
        // Read symbols are (Name=this Kind=Parameter)
        // Captured symbols are 
        // Variables declared are 
        
        /// <summary>
        /// A "name" is a string encoding of the attribute descriptor. 
        /// </summary>
        public string Name
            => Descriptor.Name;

        // A public instance property named ElementCount with a type int
        // No associated operation
        // No data-flow analysis could be created
        
        /// <summary>
        /// This is the number of data elements in the attribute. This is equal to
        /// the number of primitives times the arity. All mesh attributes associated
        /// with the same mesh component (e.g. vertices) must have the same element count.
        /// </summary>
        public int ElementCount { get; }

    } // type
} // namespace
namespace Vim.G3d
{
    // Type has fields True
    // Type has writable fields True
    // Type has public setters False
    public class GeometryAttribute
    <T>
    : GeometryAttribute 

    {
        // A public instance method named .ctor with a type void
        // operation kind is Block and type 
        // member references = Data, dt_uint8, dt_int8, dt_int8, dt_int8, dt_int8, dt_uint16, dt_int16, dt_uint32, dt_int32, dt_int32, dt_int32, dt_int32, dt_uint64, dt_int64, dt_float32, dt_float32, dt_float32, dt_float32, dt_float32, dt_float64, dt_float64, dt_float64, dt_float64, DataType, Descriptor, DataType, Descriptor, DataArity, Descriptor, DataArity, Descriptor
        // assignments = ParameterReference, Conversion, Conversion, Conversion, Conversion, Conversion, Conversion, Conversion, Conversion, Conversion, Conversion, Conversion, Conversion, Conversion, Conversion, Conversion, Conversion, Conversion, Conversion, Conversion, Conversion, Conversion, Conversion, Conversion
        // Written symbols are (Name=arity Kind=Local), (Name=dataType Kind=Local)
        // Read symbols are (Name=this Kind=Parameter), (Name=data Kind=Parameter), (Name=arity Kind=Local), (Name=dataType Kind=Local)
        // Captured symbols are 
        // Variables declared are (Name=arity Kind=Local), (Name=dataType Kind=Local)
        
        public GeometryAttribute(IArray<T> data, AttributeDescriptor descriptor)
            : base(descriptor, data.Count)
        {
            Data = data;
            int arity;
            DataType dataType;
            // TODO: TECH DEBT - Support unsigned tuples in Math3d
            if (typeof(T) == typeof(byte))
                (arity, dataType) = (1, DataType.dt_uint8);
            else if (typeof(T) == typeof(sbyte))
                (arity, dataType) = (1, DataType.dt_int8);
            else if (typeof(T) == typeof(Byte2))
                (arity, dataType) = (2, DataType.dt_int8);
            else if (typeof(T) == typeof(Byte3))
                (arity, dataType) = (3, DataType.dt_int8);
            else if (typeof(T) == typeof(Byte4))
                (arity, dataType) = (4, DataType.dt_int8);
            else if (typeof(T) == typeof(ushort))
                (arity, dataType) = (1, DataType.dt_uint16);
            else if (typeof(T) == typeof(short))
                (arity, dataType) = (1, DataType.dt_int16);
            else if (typeof(T) == typeof(uint))
                (arity, dataType) = (1, DataType.dt_uint32);
            else if (typeof(T) == typeof(int))
                (arity, dataType) = (1, DataType.dt_int32);
            else if (typeof(T) == typeof(Int2))
                (arity, dataType) = (2, DataType.dt_int32);
            else if (typeof(T) == typeof(Int3))
                (arity, dataType) = (3, DataType.dt_int32);
            else if (typeof(T) == typeof(Int4))
                (arity, dataType) = (4, DataType.dt_int32);
            else if (typeof(T) == typeof(ulong))
                (arity, dataType) = (1, DataType.dt_uint64);
            else if (typeof(T) == typeof(long))
                (arity, dataType) = (1, DataType.dt_int64);
            else if (typeof(T) == typeof(float))
                (arity, dataType) = (1, DataType.dt_float32);
            else if (typeof(T) == typeof(Vector2))
                (arity, dataType) = (2, DataType.dt_float32);
            else if (typeof(T) == typeof(Vector3))
                (arity, dataType) = (3, DataType.dt_float32);
            else if (typeof(T) == typeof(Vector4))
                (arity, dataType) = (4, DataType.dt_float32);
            else if (typeof(T) == typeof(Matrix4x4))
                (arity, dataType) = (16, DataType.dt_float32);
            else if (typeof(T) == typeof(double))
                (arity, dataType) = (1, DataType.dt_float64);
            else if (typeof(T) == typeof(DVector2))
                (arity, dataType) = (2, DataType.dt_float64);
            else if (typeof(T) == typeof(DVector3))
                (arity, dataType) = (3, DataType.dt_float64);
            else if (typeof(T) == typeof(DVector4))
                (arity, dataType) = (4, DataType.dt_float64);
            else
                throw new Exception($"Unsupported data type {typeof(T)}");

            // Check that the computed data type is consistent with the descriptor
            if (dataType != Descriptor.DataType)
                throw new Exception($"DataType was {dataType} but expected {Descriptor.DataType}");

            // Check that the computed data arity is consistent with the descriptor
            if (arity != Descriptor.DataArity)
                throw new Exception($"DatArity was {arity} but expected {Descriptor.DataArity}");
        }

        // A public instance method named Merge with a type Vim.G3d.GeometryAttribute
        // operation kind is Block and type 
        // member references = Descriptor, Descriptor, Descriptor, Association, Descriptor, assoc_all, Association, Descriptor, assoc_none, Association, Descriptor, assoc_mesh, Association, Descriptor, assoc_instance, Semantic, Descriptor, Index, Data, Descriptor
        // assignments = 
        // Written symbols are (Name=ma Kind=Parameter), (Name=ma Kind=Parameter), (Name=ma Kind=Parameter), (Name=attr Kind=Parameter)
        // Read symbols are (Name=this Kind=Parameter), (Name=others Kind=Parameter), (Name=ma Kind=Parameter), (Name=ma Kind=Parameter), (Name=ma Kind=Parameter), (Name=attr Kind=Parameter)
        // Captured symbols are (Name=this Kind=Parameter)
        // Variables declared are (Name=ma Kind=Parameter), (Name=ma Kind=Parameter), (Name=ma Kind=Parameter), (Name=attr Kind=Parameter)
        
        public override GeometryAttribute Merge(IEnumerable<GeometryAttribute> others)
        {
            if (!others.Any())
                return this;

            // Check that all attributes have the same descriptor 
            if (!others.All(ma => ma.Descriptor.Equals(Descriptor)))
                throw new Exception($"All attributes have to have same descriptor {Descriptor} to be concatenated");

            // Check that all attributes have the same type 
            if (!others.All(ma => ma is GeometryAttribute<T>))
                throw new Exception($"All attributes have to have the same type {typeof(T)} to be concatenated");

            // Given multiple attributes associated with "all" or with "nothing", the first one takes precedence	
            if (Descriptor.Association == Association.assoc_all || Descriptor.Association == Association.assoc_none)
                return this;

            // Sub-geometry attributes can't be merged 
            if (Descriptor.Association == Association.assoc_mesh)
                throw new Exception("Can't merge sub-geometry attributes");

            // Instance attributes can't be merged 
            if (Descriptor.Association == Association.assoc_instance)
                throw new Exception("Can't merge instance attributes");

            // Index attributes can't be merged 
            if (Descriptor.Semantic == Semantic.Index)
                throw new Exception("Can't merge index attributes");

            return others
                .Select(ma => ma as GeometryAttribute<T>)
                .Prepend(this)
                .ToIArray()
                .Select(attr => attr.Data)
                .Flatten()
                .ToAttribute(Descriptor);
        }

        // A public instance method named Remap with a type Vim.G3d.GeometryAttribute
        // operation kind is Block and type 
        // member references = Data, Descriptor
        // assignments = 
        // Written symbols are 
        // Read symbols are (Name=this Kind=Parameter), (Name=indices Kind=Parameter)
        // Captured symbols are 
        // Variables declared are 
        
        public override GeometryAttribute Remap(IArray<int> indices)
        {
            return Data.SelectByIndex(indices).ToAttribute(Descriptor);
        }

        // A public instance method named ToBuffer with a type Vim.BFast.INamedBuffer
        // operation kind is Block and type 
        // member references = Data, Name
        // assignments = 
        // Written symbols are 
        // Read symbols are (Name=this Kind=Parameter)
        // Captured symbols are 
        // Variables declared are 
        
        public override INamedBuffer ToBuffer()
        {
            return Data.ToArray().ToNamedBuffer(Name);
        }

        // A public instance method named Read with a type Vim.G3d.GeometryAttribute
        // operation kind is Block and type 
        // member references = DataElementSize, Descriptor, DataElementSize, Descriptor, DataElementSize, Descriptor, MaxValue, Descriptor
        // assignments = 
        // Written symbols are (Name=nElements Kind=Local), (Name=data Kind=Local)
        // Read symbols are (Name=this Kind=Parameter), (Name=stream Kind=Parameter), (Name=byteCount Kind=Parameter), (Name=nElements Kind=Local), (Name=data Kind=Local)
        // Captured symbols are 
        // Variables declared are (Name=nElements Kind=Local), (Name=data Kind=Local)
        
        public override GeometryAttribute Read(Stream stream, long byteCount)
        {
            if (byteCount % Descriptor.DataElementSize != 0)
                throw new Exception(
                    $"The number of bytes to read {byteCount} does not divide cleanly by the size of the elements {Descriptor.DataElementSize}");
            var nElements = byteCount / Descriptor.DataElementSize;
            if (nElements > int.MaxValue)
                throw new Exception(
                    $"Trying to read {nElements} which is more than the maximum number of elements in a C# array");
            var data = stream.ReadArray<T>((int)nElements);
            return new GeometryAttribute<T>(data.ToIArray(), Descriptor);
        }

        // A public instance method named SetIndex with a type Vim.G3d.GeometryAttribute
        // operation kind is Block and type 
        // member references = Index, Descriptor, Data, Descriptor
        // assignments = 
        // Written symbols are 
        // Read symbols are (Name=this Kind=Parameter), (Name=index Kind=Parameter)
        // Captured symbols are 
        // Variables declared are 
        
        public override GeometryAttribute SetIndex(int index)
        {
            return index == Descriptor.Index ? this : new GeometryAttribute<T>(Data, Descriptor.SetIndex(index));
        }

        // A public instance field named Data with a type Vim.LinqArray.IArray<T>
        // No associated operation
        // No data-flow analysis could be created
                public IArray<T> Data;

    } // type
} // namespace
