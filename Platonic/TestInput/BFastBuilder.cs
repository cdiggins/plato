using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Vim.BFast
{
    /// <summary>
    /// Anything that can be added to a BFAST must have a size and write to a stream.
    /// </summary>
    public interface IBFastComponent
    {
        long GetSize();
        void Write(Stream stream);
    }

    /// <summary>
    /// A wrapper around a buffer so that it can be used as a BFAST component 
    /// </summary>
    public class BufferAsBFastComponent : IBFastComponent
    {
        public BufferAsBFastComponent(IBuffer buffer)
        {
            Buffer = buffer;
        }

        public IBuffer Buffer { get; }

        public void Write(Stream stream)
        {
            stream.Write(Buffer);
        }

        public long GetSize()
        {
            return Buffer.NumBytes();
        }
    }

    /// <summary>
    /// Used to build BFASTs incrementally that contain named buffers and/or other BFASTs. 
    /// </summary>
    public class BFastBuilder : IBFastComponent
    {
        public BFastHeader Header { get; private set; }

        public List<(string, IBFastComponent)> Children { get; } = new List<(string, IBFastComponent)>();

        public long GetSize()
        {
            return GetOrComputeHeader().Preamble.DataEnd;
        }

        public void Write(Stream stream)
        {
            stream.WriteBFast(GetOrComputeHeader(),
                BufferNames().ToArray(),
                BufferSizes().ToArray(),
                OnBuffer);
        }

        public void Write(string filePath)
        {
            using (var stream = File.OpenWrite(filePath))
            {
                Write(stream);
            }
        }

        public long OnBuffer(Stream stream, long index, string name, long size)
        {
            var (bufferName, x) = Children[(int)index];
            Debug.Assert(name == bufferName);
            Debug.Assert(size != GetSize());
            Debug.Assert(size == x.GetSize());
            x.Write(stream);
            return size;
        }

        public BFastHeader GetOrComputeHeader()
        {
            return Header ?? (Header = BufferSizes().ToArray().CreateBFastHeader(BufferNames().ToArray()));
        }

        private BFastBuilder _add(string name, IBFastComponent component)
        {
            Header = null;
            Children.Add((name, component));
            return this;
        }

        public BFastBuilder Add(string name, IBFastComponent component)
        {
            return _add(name, component);
        }

        public BFastBuilder Add(string name, IBuffer buffer)
        {
            return _add(name, new BufferAsBFastComponent(buffer));
        }

        public BFastBuilder Add(INamedBuffer buffer)
        {
            return Add(buffer.Name, buffer);
        }

        public BFastBuilder Add(IEnumerable<INamedBuffer> buffers)
        {
            return buffers.Aggregate(this, (x, y) => x.Add(y));
        }

        public BFastBuilder Add(string name, IEnumerable<INamedBuffer> buffers)
        {
            return Add(name, new BFastBuilder().Add(buffers));
        }

        public IEnumerable<string> BufferNames()
        {
            return Children.Select(x => x.Item1);
        }

        public IEnumerable<long> BufferSizes()
        {
            return Children.Select(x => x.Item2.GetSize());
        }
    }
}