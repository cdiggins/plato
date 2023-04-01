using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Vim.BFast
{
    /// <summary>
    /// Provides an interface to an object that manages a potentially large array of elements all of the same unmanaged type.
    /// </summary>
    public interface IBuffer
    {
        Array Data { get; }
        int ElementSize { get; }
        void Write(Stream stream);
    }

    /// <summary>
    /// A version of the IBuffer interface when the element types are known
    /// </summary>
    public interface IBuffer<T> : IBuffer
    {
        T[] GetTypedData();
    }

    /// <summary>
    /// Represents a buffer associated with a string name. 
    /// </summary>
    public interface INamedBuffer : IBuffer
    {
        string Name { get; }
    }

    /// <summary>
    /// A version of the INamedBuffer interface when the element types are known
    /// </summary>
    public interface INamedBuffer<T> : INamedBuffer, IBuffer<T>
    {
    }

    /// <summary>
    /// A concrete implementation of IBuffer
    /// </summary>
    public class Buffer<T> : IBuffer<T> 
    {
        public Buffer(byte[] data)
        {
            Data = data;
        }

        public Array Data { get; }
        public int ElementSize => Marshal.SizeOf<T>();

        // TODO: BUG: Needs to be copied or cast safely. 
        public T[] GetTypedData()
        {
            return Data as T[];
        }

        public void Write(Stream stream)
        {
            stream.Write((byte[])Data, 0, Data.Length);
        }
    }

    /// <summary>
    /// A concrete implementation of INamedBuffer
    /// </summary>
    public class NamedBuffer : INamedBuffer
    {
        public NamedBuffer(IBuffer buffer, string name)
        {
            (Buffer, Name) = (buffer, name);
        }

        public IBuffer Buffer { get; }
        public string Name { get; }
        public int ElementSize => Buffer.ElementSize;
        public Array Data => Buffer.Data;

        public void Write(Stream stream)
        {
            Buffer.Write(stream);
        }
    }

    /// <summary>
    /// A concrete implementation of INamedBuffer with a specific type.
    /// </summary>
    public class NamedBuffer<T> : INamedBuffer<T> 
    {
        public readonly T[] Array;

        public NamedBuffer(T[] data, string name)
        {
            (Array, Name) = (data, name);
        }

        public string Name { get; }
        public int ElementSize => Marshal.SizeOf<T>();
        public Array Data => Array;

        public T[] GetTypedData()
        {
            return Array;
        }

        public void Write(Stream stream)
        {
            stream.Write((byte[])Data, 0, Data.Length);
        }
    }
}