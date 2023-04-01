using System;

using System.Collections.Generic;

using System.IO;

using System.Linq;

using System.Runtime.InteropServices;

namespace Vim.BFast
{
    // Type has fields False
    // Type has writable fields False
    // Type has public setters False
    public static class BufferExtensions
    {
        // A public static method named ToBytes with a type byte[]
        // operation kind is Block and type 
        // member references = Length, Length
        // assignments = 
        // Written symbols are (Name=bytes Kind=Local)
        // Read symbols are (Name=xs Kind=Parameter), (Name=bytes Kind=Local)
        // Captured symbols are 
        // Variables declared are (Name=bytes Kind=Local)
                public static byte[] ToBytes<T>(this T[] xs) 
        {
            var bytes = new byte[xs.Length * Marshal.SizeOf<T>()];
            Array.Copy(xs, bytes, xs.Length);
            return bytes;
        }

        // A public static method named ToBuffer with a type Vim.BFast.Buffer<T>
        // operation kind is Block and type 
        // member references = 
        // assignments = 
        // Written symbols are 
        // Read symbols are (Name=xs Kind=Parameter)
        // Captured symbols are 
        // Variables declared are 
        
        public static Buffer<T> ToBuffer<T>(this T[] xs) 
        {
            return new Buffer<T>(xs.ToBytes());
        }

        // A public static method named ToNamedBuffer with a type Vim.BFast.NamedBuffer<T>
        // operation kind is Block and type 
        // member references = 
        // assignments = 
        // Written symbols are 
        // Read symbols are (Name=xs Kind=Parameter), (Name=name Kind=Parameter)
        // Captured symbols are 
        // Variables declared are 
        
        public static NamedBuffer<T> ToNamedBuffer<T>(this T[] xs, string name = "") 
        {
            return new NamedBuffer<T>(xs, name);
        }

        // A public static method named ToNamedBuffer with a type Vim.BFast.NamedBuffer
        // operation kind is Block and type 
        // member references = 
        // assignments = 
        // Written symbols are 
        // Read symbols are (Name=buffer Kind=Parameter), (Name=name Kind=Parameter)
        // Captured symbols are 
        // Variables declared are 
        
        public static NamedBuffer ToNamedBuffer(this IBuffer buffer, string name = "")
        {
            return new NamedBuffer(buffer, name);
        }

        // A public static method named ToNamedBuffers with a type System.Collections.Generic.IEnumerable<Vim.BFast.INamedBuffer>
        // operation kind is Block and type 
        // member references = ToNamedBuffer
        // assignments = 
        // Written symbols are (Name=b Kind=Parameter)
        // Read symbols are (Name=buffers Kind=Parameter), (Name=names Kind=Parameter), (Name=b Kind=Parameter)
        // Captured symbols are 
        // Variables declared are (Name=b Kind=Parameter)
        
        public static IEnumerable<INamedBuffer> ToNamedBuffers(this IEnumerable<IBuffer> buffers,
            IEnumerable<string> names = null)
        {
            return names == null ? buffers.Select(b => b.ToNamedBuffer()) : buffers.Zip(names, ToNamedBuffer);
        }

        // A public static method named ToDictionary with a type System.Collections.Generic.IDictionary<string, Vim.BFast.INamedBuffer>
        // operation kind is Block and type 
        // member references = Name
        // assignments = 
        // Written symbols are (Name=b Kind=Parameter), (Name=b Kind=Parameter)
        // Read symbols are (Name=buffers Kind=Parameter), (Name=b Kind=Parameter), (Name=b Kind=Parameter)
        // Captured symbols are 
        // Variables declared are (Name=b Kind=Parameter), (Name=b Kind=Parameter)
        
        public static IDictionary<string, INamedBuffer> ToDictionary(this IEnumerable<INamedBuffer> buffers)
        {
            return buffers.ToDictionary(b => b.Name, b => b);
        }

        // A public static method named ToNamedBuffers with a type System.Collections.Generic.IEnumerable<Vim.BFast.INamedBuffer>
        // operation kind is Block and type 
        // member references = Value, Key
        // assignments = 
        // Written symbols are (Name=kv Kind=Parameter)
        // Read symbols are (Name=d Kind=Parameter), (Name=kv Kind=Parameter)
        // Captured symbols are 
        // Variables declared are (Name=kv Kind=Parameter)
        
        public static IEnumerable<INamedBuffer> ToNamedBuffers(this IDictionary<string, IBuffer> d)
        {
            return d.Select(kv => kv.Value.ToNamedBuffer(kv.Key));
        }

        // A public static method named ToNamedBuffers with a type System.Collections.Generic.IEnumerable<Vim.BFast.INamedBuffer>
        // operation kind is Block and type 
        // member references = Value, Key
        // assignments = 
        // Written symbols are (Name=kv Kind=Parameter)
        // Read symbols are (Name=d Kind=Parameter), (Name=kv Kind=Parameter)
        // Captured symbols are 
        // Variables declared are (Name=kv Kind=Parameter)
        
        public static IEnumerable<INamedBuffer> ToNamedBuffers(this IDictionary<string, byte[]> d)
        {
            return d.Select(kv => kv.Value.ToNamedBuffer(kv.Key));
        }

        // A public static method named CopyBytes with a type System.Array
        // operation kind is Block and type 
        // member references = Data
        // assignments = 
        // Written symbols are 
        // Read symbols are (Name=src Kind=Parameter), (Name=dst Kind=Parameter), (Name=srcOffset Kind=Parameter), (Name=destOffset Kind=Parameter)
        // Captured symbols are 
        // Variables declared are 
        
        public static Array CopyBytes(this IBuffer src, Array dst, int srcOffset = 0, int destOffset = 0)
        {
            Buffer.BlockCopy(src.Data, srcOffset, dst, destOffset, (int)src.NumBytes());
            return dst;
        }

        // A public static method named ToBytes with a type byte[]
        // operation kind is Block and type 
        // member references = 
        // assignments = 
        // Written symbols are 
        // Read symbols are (Name=src Kind=Parameter), (Name=dest Kind=Parameter)
        // Captured symbols are 
        // Variables declared are 
        
        public static byte[] ToBytes(this IBuffer src, byte[] dest = null)
        {
            return src.ToArray(dest);
        }

        // A public static method named ToBytes with a type byte[]
        // operation kind is Block and type 
        // member references = 
        // assignments = 
        // Written symbols are 
        // Read symbols are (Name=xs Kind=Parameter), (Name=dest Kind=Parameter)
        // Captured symbols are 
        // Variables declared are 
        
        public static byte[] ToBytes<T>(this T[] xs, byte[] dest = null) 
        {
            return xs.RecastArray(dest);
        }

        // A public static method named ToArray with a type T[]
        // operation kind is Block and type 
        // member references = 
        // assignments = 
        // Written symbols are 
        // Read symbols are (Name=buffer Kind=Parameter), (Name=dest Kind=Parameter)
        // Captured symbols are 
        // Variables declared are 
        
        /// <summary>
        /// Accepts an array of the given type, or creates one if necessary, copy the buffer data into it 
        /// </summary>
        public static T[] ToArray<T>(this IBuffer buffer, T[] dest = null) 
        {
            return (T[])buffer.CopyBytes(dest ?? new T[buffer.NumBytes() / Marshal.SizeOf<T>()]);
        }

        // A public static method named AsArray with a type T[]
        // operation kind is Block and type 
        // member references = Data
        // assignments = 
        // Written symbols are (Name=r Kind=Local)
        // Read symbols are (Name=buffer Kind=Parameter), (Name=r Kind=Local)
        // Captured symbols are 
        // Variables declared are (Name=r Kind=Local)
        
        /// <summary>
        /// Returns the array in the buffer, if it is of the correct type, or creates a new array of the create type and copies
        /// bytes into it, as necessary. 
        /// </summary>
        public static T[] AsArray<T>(this IBuffer buffer) 
        {
            return buffer.Data is T[] r ? r : buffer.ToArray<T>();
        }

        // A public static method named RecastArray with a type U[]
        // operation kind is Block and type 
        // member references = 
        // assignments = 
        // Written symbols are 
        // Read symbols are (Name=src Kind=Parameter), (Name=r Kind=Parameter)
        // Captured symbols are 
        // Variables declared are 
        
        /// <summary>
        /// Copies an array of unmanaged types into another array of unmanaged types
        /// </summary>
        public static U[] RecastArray<T, U>(this T[] src, U[] r = null)  where U : unmanaged
        {
            return src.ToBuffer().ToArray(r);
        }

        // A public static method named NumElements with a type int
        // operation kind is Block and type 
        // member references = Length, Data
        // assignments = 
        // Written symbols are 
        // Read symbols are (Name=buffer Kind=Parameter)
        // Captured symbols are 
        // Variables declared are 
        
        public static int NumElements(this IBuffer buffer)
        {
            return buffer.Data.Length;
        }

        // A public static method named NumBytes with a type long
        // operation kind is Block and type 
        // member references = ElementSize
        // assignments = 
        // Written symbols are 
        // Read symbols are (Name=buffer Kind=Parameter)
        // Captured symbols are 
        // Variables declared are 
        
        public static long NumBytes(this IBuffer buffer)
        {
            return (long)buffer.NumElements() * buffer.ElementSize;
        }

        // A public static method named ReadBuffer with a type Vim.BFast.Buffer<T>
        // operation kind is Block and type 
        // member references = 
        // assignments = 
        // Written symbols are 
        // Read symbols are (Name=stream Kind=Parameter), (Name=numElements Kind=Parameter), (Name=elementSize Kind=Parameter)
        // Captured symbols are 
        // Variables declared are 
        
        public static Buffer<T> ReadBuffer<T>(this Stream stream, int numElements, int elementSize) 
        {
            return new Buffer<T>(stream.ReadBytes(numElements * elementSize));
        }

        // A public static method named ReadBytes with a type byte[]
        // operation kind is Block and type 
        // member references = 
        // assignments = 
        // Written symbols are (Name=buffer Kind=Local)
        // Read symbols are (Name=stream Kind=Parameter), (Name=numBytes Kind=Parameter), (Name=buffer Kind=Local)
        // Captured symbols are 
        // Variables declared are (Name=buffer Kind=Local)
        
        public static byte[] ReadBytes(this Stream stream, int numBytes)
        {
            var buffer = new byte[numBytes];
            stream.Read(buffer, 0, numBytes);
            return buffer;
        }

        // A public static method named Write with a type void
        // operation kind is Block and type 
        // member references = 
        // assignments = 
        // Written symbols are 
        // Read symbols are (Name=stream Kind=Parameter), (Name=buffer Kind=Parameter)
        // Captured symbols are 
        // Variables declared are 
        
        public static void Write(this Stream stream, IBuffer buffer)
        {
            buffer.Write(stream);
        }

        // A public static method named ReadArray with a type T[]
        // operation kind is Block and type 
        // member references = Length
        // assignments = 
        // Written symbols are (Name=n Kind=Local), (Name=buffer Kind=Local), (Name=r Kind=Local)
        // Read symbols are (Name=stream Kind=Parameter), (Name=count Kind=Parameter), (Name=n Kind=Local), (Name=buffer Kind=Local), (Name=r Kind=Local)
        // Captured symbols are 
        // Variables declared are (Name=n Kind=Local), (Name=buffer Kind=Local), (Name=r Kind=Local)
        
        public static T[] ReadArray<T>(this Stream stream, int count) 
        {
            var n = count * Marshal.SizeOf<T>();
            var buffer = stream.ReadBytes(n);
            var r = new T[count];
            Array.Copy(buffer, r, buffer.Length);
            return r;
        }

    } // type
} // namespace
