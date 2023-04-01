using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace Vim.BFast
{
    /// <summary>
    /// Helper functions for working with buffers 
    /// </summary>
    public static class BufferExtensions
    {
        public static byte[] ToBytes<T>(this T[] xs) 
        {
            var bytes = new byte[xs.Length * Marshal.SizeOf<T>()];
            Array.Copy(xs, bytes, xs.Length);
            return bytes;
        }

        public static Buffer<T> ToBuffer<T>(this T[] xs) 
        {
            return new Buffer<T>(xs.ToBytes());
        }

        public static NamedBuffer<T> ToNamedBuffer<T>(this T[] xs, string name = "") 
        {
            return new NamedBuffer<T>(xs, name);
        }

        public static NamedBuffer ToNamedBuffer(this IBuffer buffer, string name = "")
        {
            return new NamedBuffer(buffer, name);
        }

        public static IEnumerable<INamedBuffer> ToNamedBuffers(this IEnumerable<IBuffer> buffers,
            IEnumerable<string> names = null)
        {
            return names == null ? buffers.Select(b => b.ToNamedBuffer()) : buffers.Zip(names, ToNamedBuffer);
        }

        public static IDictionary<string, INamedBuffer> ToDictionary(this IEnumerable<INamedBuffer> buffers)
        {
            return buffers.ToDictionary(b => b.Name, b => b);
        }

        public static IEnumerable<INamedBuffer> ToNamedBuffers(this IDictionary<string, IBuffer> d)
        {
            return d.Select(kv => kv.Value.ToNamedBuffer(kv.Key));
        }

        public static IEnumerable<INamedBuffer> ToNamedBuffers(this IDictionary<string, byte[]> d)
        {
            return d.Select(kv => kv.Value.ToNamedBuffer(kv.Key));
        }

        public static Array CopyBytes(this IBuffer src, Array dst, int srcOffset = 0, int destOffset = 0)
        {
            Buffer.BlockCopy(src.Data, srcOffset, dst, destOffset, (int)src.NumBytes());
            return dst;
        }

        public static byte[] ToBytes(this IBuffer src, byte[] dest = null)
        {
            return src.ToArray(dest);
        }

        public static byte[] ToBytes<T>(this T[] xs, byte[] dest = null) 
        {
            return xs.RecastArray(dest);
        }

        /// <summary>
        /// Accepts an array of the given type, or creates one if necessary, copy the buffer data into it 
        /// </summary>
        public static T[] ToArray<T>(this IBuffer buffer, T[] dest = null) 
        {
            return (T[])buffer.CopyBytes(dest ?? new T[buffer.NumBytes() / Marshal.SizeOf<T>()]);
        }

        /// <summary>
        /// Returns the array in the buffer, if it is of the correct type, or creates a new array of the create type and copies
        /// bytes into it, as necessary. 
        /// </summary>
        public static T[] AsArray<T>(this IBuffer buffer) 
        {
            return buffer.Data is T[] r ? r : buffer.ToArray<T>();
        }

        /// <summary>
        /// Copies an array of unmanaged types into another array of unmanaged types
        /// </summary>
        public static U[] RecastArray<T, U>(this T[] src, U[] r = null)  where U : unmanaged
        {
            return src.ToBuffer().ToArray(r);
        }

        public static int NumElements(this IBuffer buffer)
        {
            return buffer.Data.Length;
        }

        public static long NumBytes(this IBuffer buffer)
        {
            return (long)buffer.NumElements() * buffer.ElementSize;
        }

        public static Buffer<T> ReadBuffer<T>(this Stream stream, int numElements, int elementSize) 
        {
            return new Buffer<T>(stream.ReadBytes(numElements * elementSize));
        }

        public static byte[] ReadBytes(this Stream stream, int numBytes)
        {
            var buffer = new byte[numBytes];
            stream.Read(buffer, 0, numBytes);
            return buffer;
        }

        public static void Write(this Stream stream, IBuffer buffer)
        {
            buffer.Write(stream);
        }

        public static T[] ReadArray<T>(this Stream stream, int count) 
        {
            var n = count * Marshal.SizeOf<T>();
            var buffer = stream.ReadBytes(n);
            var r = new T[count];
            Array.Copy(buffer, r, buffer.Length);
            return r;
        }
    }
}