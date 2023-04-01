using System;

using System.Collections.Generic;

using System.Diagnostics;

using System.Linq;

using Vim.LinqArray;

using Vim.Math3d;

namespace Vim.G3d
{
    // Type has fields False
    // Type has writable fields False
    // Type has public setters False
    public static class AttributeExtensions
    {
        // A public static method named CheckArity with a type Vim.G3d.GeometryAttribute<T>
        // operation kind is Block and type 
        // member references = Descriptor, DataArity
        // assignments = 
        // Written symbols are 
        // Read symbols are (Name=self Kind=Parameter), (Name=arity Kind=Parameter)
        // Captured symbols are 
        // Variables declared are 
                public static GeometryAttribute<T> CheckArity<T>(this GeometryAttribute<T> self, int arity) 
        {
            return self?.Descriptor?.DataArity == arity ? self : null;
        }

        // A public static method named CheckAssociation with a type Vim.G3d.GeometryAttribute<T>
        // operation kind is Block and type 
        // member references = Descriptor, Association
        // assignments = 
        // Written symbols are 
        // Read symbols are (Name=self Kind=Parameter), (Name=assoc Kind=Parameter)
        // Captured symbols are 
        // Variables declared are 
        
        public static GeometryAttribute<T> CheckAssociation<T>(this GeometryAttribute<T> self, Association assoc)
            
        {
            return self?.Descriptor?.Association == assoc ? self : null;
        }

        // A public static method named CheckArityAndAssociation with a type Vim.G3d.GeometryAttribute<T>
        // operation kind is Block and type 
        // member references = 
        // assignments = 
        // Written symbols are 
        // Read symbols are (Name=self Kind=Parameter), (Name=arity Kind=Parameter), (Name=assoc Kind=Parameter)
        // Captured symbols are 
        // Variables declared are 
        
        public static GeometryAttribute<T> CheckArityAndAssociation<T>(this GeometryAttribute<T> self, int arity,
            Association assoc) 
        {
            return self?.CheckArity(arity)?.CheckAssociation(assoc);
        }

        // A public static method named ToAttribute with a type Vim.G3d.GeometryAttribute<T>
        // operation kind is Block and type 
        // member references = 
        // assignments = 
        // Written symbols are 
        // Read symbols are (Name=self Kind=Parameter), (Name=desc Kind=Parameter)
        // Captured symbols are 
        // Variables declared are 
        
        public static GeometryAttribute<T> ToAttribute<T>(this IList<T> self, string desc) 
        {
            return self.ToIArray().ToAttribute(desc);
        }

        // A public static method named ToAttribute with a type Vim.G3d.GeometryAttribute<T>
        // operation kind is Block and type 
        // member references = 
        // assignments = 
        // Written symbols are 
        // Read symbols are (Name=self Kind=Parameter), (Name=desc Kind=Parameter)
        // Captured symbols are 
        // Variables declared are 
        
        public static GeometryAttribute<T> ToAttribute<T>(this IList<T> self, AttributeDescriptor desc)
            
        {
            return self.ToIArray().ToAttribute(desc);
        }

        // A public static method named ToAttribute with a type Vim.G3d.GeometryAttribute<T>
        // operation kind is Block and type 
        // member references = 
        // assignments = 
        // Written symbols are 
        // Read symbols are (Name=self Kind=Parameter), (Name=desc Kind=Parameter)
        // Captured symbols are 
        // Variables declared are 
        
        public static GeometryAttribute<T> ToAttribute<T>(this IArray<T> self, AttributeDescriptor desc)
            
        {
            return new GeometryAttribute<T>(self, desc);
        }

        // A public static method named ToAttribute with a type Vim.G3d.GeometryAttribute<T>
        // operation kind is Block and type 
        // member references = 
        // assignments = 
        // Written symbols are 
        // Read symbols are (Name=self Kind=Parameter), (Name=desc Kind=Parameter)
        // Captured symbols are 
        // Variables declared are 
        
        public static GeometryAttribute<T> ToAttribute<T>(this IArray<T> self, string desc) 
        {
            return self.ToAttribute(AttributeDescriptor.Parse(desc));
        }

        // A public static method named ToAttribute with a type Vim.G3d.GeometryAttribute<T>
        // operation kind is Block and type 
        // member references = 
        // assignments = 
        // Written symbols are 
        // Read symbols are (Name=self Kind=Parameter), (Name=desc Kind=Parameter), (Name=index Kind=Parameter)
        // Captured symbols are 
        // Variables declared are 
        
        public static GeometryAttribute<T> ToAttribute<T>(this IArray<T> self, string desc, int index)
            
        {
            return self.ToAttribute(AttributeDescriptor.Parse(desc).SetIndex(index));
        }

        // A public static method named AttributeToColors with a type Vim.LinqArray.IArray<Vim.Math3d.Vector4>
        // operation kind is Block and type 
        // member references = Descriptor, DataType, dt_float32, DataArity, Data, DataArity, Data, X, Y, Z, DataArity, Data, X, Y, DataArity, Data, DataType, dt_int8, DataArity, Data, X, Y, Z, W, DataArity, Data, X, Y, Z, DataArity, Data, X, Y, DataArity, Data, Descriptor
        // assignments = 
        // Written symbols are (Name=desc Kind=Local), (Name=vc Kind=Parameter), (Name=vc Kind=Parameter), (Name=vc Kind=Parameter), (Name=b Kind=Parameter), (Name=b Kind=Parameter), (Name=b Kind=Parameter), (Name=b Kind=Parameter)
        // Read symbols are (Name=attr Kind=Parameter), (Name=desc Kind=Local), (Name=vc Kind=Parameter), (Name=vc Kind=Parameter), (Name=vc Kind=Parameter), (Name=b Kind=Parameter), (Name=b Kind=Parameter), (Name=b Kind=Parameter), (Name=b Kind=Parameter)
        // Captured symbols are 
        // Variables declared are (Name=desc Kind=Local), (Name=vc Kind=Parameter), (Name=vc Kind=Parameter), (Name=vc Kind=Parameter), (Name=b Kind=Parameter), (Name=b Kind=Parameter), (Name=b Kind=Parameter), (Name=b Kind=Parameter)
        
        public static IArray<Vector4> AttributeToColors(this GeometryAttribute attr)
        {
            var desc = attr.Descriptor;
            if (desc.DataType == DataType.dt_float32)
            {
                if (desc.DataArity == 4)
                    return attr.AsType<Vector4>().Data;
                if (desc.DataArity == 3)
                    return attr.AsType<Vector3>().Data.Select(vc => new Vector4(vc.X, vc.Y, vc.Z, 1f));
                if (desc.DataArity == 2)
                    return attr.AsType<Vector2>().Data.Select(vc => new Vector4(vc.X, vc.Y, 0, 1f));
                if (desc.DataArity == 1)
                    return attr.AsType<float>().Data.Select(vc => new Vector4(vc, vc, vc, 1f));
            }

            if (desc.DataType == DataType.dt_int8)
            {
                if (desc.DataArity == 4)
                    return attr.AsType<Byte4>().Data
                        .Select(b => new Vector4(b.X / 255f, b.Y / 255f, b.Z / 255f, b.W / 255f));
                if (desc.DataArity == 3)
                    return attr.AsType<Byte3>().Data.Select(b => new Vector4(b.X / 255f, b.Y / 255f, b.Z / 255f, 1f));
                if (desc.DataArity == 2)
                    return attr.AsType<Byte2>().Data.Select(b => new Vector4(b.X / 255f, b.Y / 255f, 0f, 1f));
                if (desc.DataArity == 1)
                    return attr.AsType<byte>().Data.Select(b => new Vector4(b / 255f, b / 255f, b / 255f, 1f));
            }

            Debug.WriteLine($"Failed to recongize color format {attr.Descriptor}");
            return null;
        }

        // A public static method named ToDefaultAttribute with a type Vim.G3d.GeometryAttribute
        // operation kind is Block and type 
        // member references = DataType, dt_uint8, DataArity, dt_int8, DataArity, DataArity, DataArity, DataArity, dt_uint16, DataArity, dt_int16, DataArity, dt_uint32, DataArity, dt_int32, DataArity, DataArity, DataArity, DataArity, dt_uint64, DataArity, dt_int64, DataArity, dt_float32, DataArity, DataArity, DataArity, DataArity, DataArity, dt_float64, DataArity, DataArity, DataArity, DataArity
        // assignments = 
        // Written symbols are 
        // Read symbols are (Name=desc Kind=Parameter), (Name=count Kind=Parameter)
        // Captured symbols are 
        // Variables declared are 
        
        public static GeometryAttribute ToDefaultAttribute(this AttributeDescriptor desc, int count)
        {
            switch (desc.DataType)
            {
                // TODO: TECH DEBT - Add unsigned tuple objects to Math3d
                case DataType.dt_uint8:
                    if (desc.DataArity == 1)
                        return default(byte).Repeat(count).ToAttribute(desc);
                    break;
                case DataType.dt_int8:
                    if (desc.DataArity == 1)
                        return default(byte).Repeat(count).ToAttribute(desc);
                    if (desc.DataArity == 2)
                        return default(Byte2).Repeat(count).ToAttribute(desc);
                    if (desc.DataArity == 3)
                        return default(Byte3).Repeat(count).ToAttribute(desc);
                    if (desc.DataArity == 4)
                        return default(Byte4).Repeat(count).ToAttribute(desc);
                    break;
                case DataType.dt_uint16:
                    if (desc.DataArity == 1)
                        return default(ushort).Repeat(count).ToAttribute(desc);
                    break;
                case DataType.dt_int16:
                    if (desc.DataArity == 1)
                        return default(short).Repeat(count).ToAttribute(desc);
                    break;
                case DataType.dt_uint32:
                    if (desc.DataArity == 1)
                        return default(uint).Repeat(count).ToAttribute(desc);
                    break;
                case DataType.dt_int32:
                    if (desc.DataArity == 1)
                        return default(int).Repeat(count).ToAttribute(desc);
                    if (desc.DataArity == 2)
                        return default(Int2).Repeat(count).ToAttribute(desc);
                    if (desc.DataArity == 3)
                        return default(Int3).Repeat(count).ToAttribute(desc);
                    if (desc.DataArity == 4)
                        return default(Int4).Repeat(count).ToAttribute(desc);
                    break;
                case DataType.dt_uint64:
                    if (desc.DataArity == 1)
                        return default(ulong).Repeat(count).ToAttribute(desc);
                    break;
                case DataType.dt_int64:
                    if (desc.DataArity == 1)
                        return default(long).Repeat(count).ToAttribute(desc);
                    break;
                case DataType.dt_float32:
                    if (desc.DataArity == 1)
                        return default(float).Repeat(count).ToAttribute(desc);
                    if (desc.DataArity == 2)
                        return default(Vector2).Repeat(count).ToAttribute(desc);
                    if (desc.DataArity == 3)
                        return default(Vector3).Repeat(count).ToAttribute(desc);
                    if (desc.DataArity == 4)
                        return default(Vector4).Repeat(count).ToAttribute(desc);
                    if (desc.DataArity == 16)
                        return default(Matrix4x4).Repeat(count).ToAttribute(desc);
                    break;
                case DataType.dt_float64:
                    if (desc.DataArity == 1)
                        return default(double).Repeat(count).ToAttribute(desc);
                    if (desc.DataArity == 2)
                        return default(DVector2).Repeat(count).ToAttribute(desc);
                    if (desc.DataArity == 3)
                        return default(DVector3).Repeat(count).ToAttribute(desc);
                    if (desc.DataArity == 4)
                        return default(DVector4).Repeat(count).ToAttribute(desc);
                    break;
            }

            throw new Exception($"Could not create a default attribute for {desc}");
        }

        // A public static method named GetByteSize with a type long
        // operation kind is Block and type 
        // member references = ElementCount, DataElementSize, Descriptor
        // assignments = 
        // Written symbols are 
        // Read symbols are (Name=attribute Kind=Parameter)
        // Captured symbols are 
        // Variables declared are 
        
        public static long GetByteSize(this GeometryAttribute attribute)
        {
            return (long)attribute.ElementCount * attribute.Descriptor.DataElementSize;
        }

        // A public static method named Merge with a type Vim.G3d.GeometryAttribute
        // operation kind is Block and type 
        // member references = 
        // assignments = 
        // Written symbols are 
        // Read symbols are (Name=attributes Kind=Parameter)
        // Captured symbols are 
        // Variables declared are 
        
        public static GeometryAttribute Merge(this IEnumerable<GeometryAttribute> attributes)
        {
            return attributes.FirstOrDefault()?.Merge(attributes.Skip(1));
        }

        // A public static method named Merge with a type Vim.G3d.GeometryAttribute
        // operation kind is Block and type 
        // member references = 
        // assignments = 
        // Written symbols are 
        // Read symbols are (Name=attributes Kind=Parameter)
        // Captured symbols are 
        // Variables declared are 
        
        public static GeometryAttribute Merge(this IArray<GeometryAttribute> attributes)
        {
            return attributes.ToEnumerable().Merge();
        }

    } // type
} // namespace
