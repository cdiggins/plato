using System;

using System.IO;

using System.Runtime.InteropServices;

namespace Vim.BFast
{
    // Type has fields False
    // Type has writable fields False
    // Type has public setters False
    public interface IBuffer
    {
        // A private instance method named Write with a type void
        // No associated operation
        // No data-flow analysis could be created
                void Write(Stream stream);

        // A private instance property named Data with a type System.Array
        // No associated operation
        // No data-flow analysis could be created
                Array Data { get; }

        // A private instance property named ElementSize with a type int
        // No associated operation
        // No data-flow analysis could be created
                int ElementSize { get; }

    } // type
} // namespace
namespace Vim.BFast
{
    // Type has fields False
    // Type has writable fields False
    // Type has public setters False
    public interface IBuffer
    <T>
    : IBuffer

    {
        // A private instance method named GetTypedData with a type T[]
        // No associated operation
        // No data-flow analysis could be created
                T[] GetTypedData();

    } // type
} // namespace
namespace Vim.BFast
{
    // Type has fields False
    // Type has writable fields False
    // Type has public setters False
    public interface INamedBuffer
    : IBuffer

    {
        // A private instance property named Name with a type string
        // No associated operation
        // No data-flow analysis could be created
                string Name { get; }

    } // type
} // namespace
namespace Vim.BFast
{
    // Type has fields False
    // Type has writable fields False
    // Type has public setters False
    public interface INamedBuffer
    <T>
    : INamedBuffer, IBuffer<T>

    {
    } // type
} // namespace
namespace Vim.BFast
{
    // Type has fields False
    // Type has writable fields False
    // Type has public setters False
    public class Buffer
    <T>
    : IBuffer<T> 

    {
        // A public instance method named .ctor with a type void
        // operation kind is Block and type 
        // member references = Data
        // assignments = Conversion
        // Written symbols are 
        // Read symbols are (Name=this Kind=Parameter), (Name=data Kind=Parameter)
        // Captured symbols are 
        // Variables declared are 
                public Buffer(byte[] data)
        {
            Data = data;
        }

        // A public instance method named GetTypedData with a type T[]
        // operation kind is Block and type 
        // member references = Data
        // assignments = 
        // Written symbols are 
        // Read symbols are (Name=this Kind=Parameter)
        // Captured symbols are 
        // Variables declared are 
        
        // TODO: BUG: Needs to be copied or cast safely. 
        public T[] GetTypedData()
        {
            return Data as T[];
        }

        // A public instance method named Write with a type void
        // operation kind is Block and type 
        // member references = Data, Length, Data
        // assignments = 
        // Written symbols are 
        // Read symbols are (Name=this Kind=Parameter), (Name=stream Kind=Parameter)
        // Captured symbols are 
        // Variables declared are 
        
        public void Write(Stream stream)
        {
            stream.Write((byte[])Data, 0, Data.Length);
        }

        // A public instance property named Data with a type System.Array
        // No associated operation
        // No data-flow analysis could be created
        
        public Array Data { get; }

        // A public instance property named ElementSize with a type int
        // operation kind is Invocation and type int
        // member references = 
        // assignments = 
        // Written symbols are 
        // Read symbols are 
        // Captured symbols are 
        // Variables declared are 
                public int ElementSize => Marshal.SizeOf<T>();

    } // type
} // namespace
namespace Vim.BFast
{
    // Type has fields False
    // Type has writable fields False
    // Type has public setters False
    public class NamedBuffer
    : INamedBuffer

    {
        // A public instance method named .ctor with a type void
        // operation kind is Block and type 
        // member references = Buffer, Name
        // assignments = Conversion
        // Written symbols are 
        // Read symbols are (Name=this Kind=Parameter), (Name=buffer Kind=Parameter), (Name=name Kind=Parameter)
        // Captured symbols are 
        // Variables declared are 
                public NamedBuffer(IBuffer buffer, string name)
        {
            (Buffer, Name) = (buffer, name);
        }

        // A public instance method named Write with a type void
        // operation kind is Block and type 
        // member references = Buffer
        // assignments = 
        // Written symbols are 
        // Read symbols are (Name=this Kind=Parameter), (Name=stream Kind=Parameter)
        // Captured symbols are 
        // Variables declared are 
        
        public void Write(Stream stream)
        {
            Buffer.Write(stream);
        }

        // A public instance property named Buffer with a type Vim.BFast.IBuffer
        // No associated operation
        // No data-flow analysis could be created
        
        public IBuffer Buffer { get; }

        // A public instance property named Name with a type string
        // No associated operation
        // No data-flow analysis could be created
                public string Name { get; }

        // A public instance property named ElementSize with a type int
        // operation kind is PropertyReference and type int
        // member references = ElementSize, Buffer
        // assignments = 
        // Written symbols are 
        // Read symbols are (Name=this Kind=Parameter)
        // Captured symbols are 
        // Variables declared are 
                public int ElementSize => Buffer.ElementSize;

        // A public instance property named Data with a type System.Array
        // operation kind is PropertyReference and type System.Array
        // member references = Data, Buffer
        // assignments = 
        // Written symbols are 
        // Read symbols are (Name=this Kind=Parameter)
        // Captured symbols are 
        // Variables declared are 
                public Array Data => Buffer.Data;

    } // type
} // namespace
namespace Vim.BFast
{
    // Type has fields True
    // Type has writable fields False
    // Type has public setters False
    public class NamedBuffer
    <T>
    : INamedBuffer<T> 

    {
        // A public instance method named .ctor with a type void
        // operation kind is Block and type 
        // member references = Array, Name
        // assignments = Conversion
        // Written symbols are 
        // Read symbols are (Name=this Kind=Parameter), (Name=data Kind=Parameter), (Name=name Kind=Parameter)
        // Captured symbols are 
        // Variables declared are 
        
        public NamedBuffer(T[] data, string name)
        {
            (Array, Name) = (data, name);
        }

        // A public instance method named GetTypedData with a type T[]
        // operation kind is Block and type 
        // member references = Array
        // assignments = 
        // Written symbols are 
        // Read symbols are (Name=this Kind=Parameter)
        // Captured symbols are 
        // Variables declared are 
        
        public T[] GetTypedData()
        {
            return Array;
        }

        // A public instance method named Write with a type void
        // operation kind is Block and type 
        // member references = Data, Length, Data
        // assignments = 
        // Written symbols are 
        // Read symbols are (Name=this Kind=Parameter), (Name=stream Kind=Parameter)
        // Captured symbols are 
        // Variables declared are 
        
        public void Write(Stream stream)
        {
            stream.Write((byte[])Data, 0, Data.Length);
        }

        // A public instance field named Array with a type T[]
        // No associated operation
        // No data-flow analysis could be created
                public readonly T[] Array;

        // A public instance property named Name with a type string
        // No associated operation
        // No data-flow analysis could be created
        
        public string Name { get; }

        // A public instance property named ElementSize with a type int
        // operation kind is Invocation and type int
        // member references = 
        // assignments = 
        // Written symbols are 
        // Read symbols are 
        // Captured symbols are 
        // Variables declared are 
                public int ElementSize => Marshal.SizeOf<T>();

        // A public instance property named Data with a type System.Array
        // operation kind is FieldReference and type T[]
        // member references = Array
        // assignments = 
        // Written symbols are 
        // Read symbols are (Name=this Kind=Parameter)
        // Captured symbols are 
        // Variables declared are 
                public Array Data => Array;

    } // type
} // namespace
