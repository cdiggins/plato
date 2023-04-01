using System.Collections.Generic;

using System.Diagnostics;

using System.IO;

using System.Linq;

namespace Vim.BFast
{
    // Type has fields False
    // Type has writable fields False
    // Type has public setters False
    public interface IBFastComponent
    {
        // A private instance method named GetSize with a type long
        // No associated operation
        // No data-flow analysis could be created
                long GetSize();

        // A private instance method named Write with a type void
        // No associated operation
        // No data-flow analysis could be created
                void Write(Stream stream);

    } // type
} // namespace
namespace Vim.BFast
{
    // Type has fields False
    // Type has writable fields False
    // Type has public setters False
    public class BufferAsBFastComponent
    : IBFastComponent

    {
        // A public instance method named .ctor with a type void
        // operation kind is Block and type 
        // member references = Buffer
        // assignments = ParameterReference
        // Written symbols are 
        // Read symbols are (Name=this Kind=Parameter), (Name=buffer Kind=Parameter)
        // Captured symbols are 
        // Variables declared are 
                public BufferAsBFastComponent(IBuffer buffer)
        {
            Buffer = buffer;
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
            stream.Write(Buffer);
        }

        // A public instance method named GetSize with a type long
        // operation kind is Block and type 
        // member references = Buffer
        // assignments = 
        // Written symbols are 
        // Read symbols are (Name=this Kind=Parameter)
        // Captured symbols are 
        // Variables declared are 
        
        public long GetSize()
        {
            return Buffer.NumBytes();
        }

        // A public instance property named Buffer with a type Vim.BFast.IBuffer
        // No associated operation
        // No data-flow analysis could be created
        
        public IBuffer Buffer { get; }

    } // type
} // namespace
namespace Vim.BFast
{
    // Type has fields False
    // Type has writable fields False
    // Type has public setters False
    public class BFastBuilder
    : IBFastComponent

    {
        // A public instance method named GetSize with a type long
        // operation kind is Block and type 
        // member references = DataEnd, Preamble
        // assignments = 
        // Written symbols are 
        // Read symbols are (Name=this Kind=Parameter)
        // Captured symbols are 
        // Variables declared are 
        
        public long GetSize()
        {
            return GetOrComputeHeader().Preamble.DataEnd;
        }

        // A public instance method named Write with a type void
        // operation kind is Block and type 
        // member references = OnBuffer
        // assignments = 
        // Written symbols are 
        // Read symbols are (Name=this Kind=Parameter), (Name=stream Kind=Parameter)
        // Captured symbols are 
        // Variables declared are 
        
        public void Write(Stream stream)
        {
            stream.WriteBFast(GetOrComputeHeader(),
                BufferNames().ToArray(),
                BufferSizes().ToArray(),
                OnBuffer);
        }

        // A public instance method named Write with a type void
        // operation kind is Block and type 
        // member references = 
        // assignments = 
        // Written symbols are (Name=stream Kind=Local)
        // Read symbols are (Name=this Kind=Parameter), (Name=filePath Kind=Parameter), (Name=stream Kind=Local)
        // Captured symbols are 
        // Variables declared are (Name=stream Kind=Local)
        
        public void Write(string filePath)
        {
            using (var stream = File.OpenWrite(filePath))
            {
                Write(stream);
            }
        }

        // A public instance method named OnBuffer with a type long
        // operation kind is Block and type 
        // member references = this[], Children
        // assignments = PropertyReference
        // Written symbols are (Name=bufferName Kind=Local), (Name=x Kind=Local)
        // Read symbols are (Name=this Kind=Parameter), (Name=stream Kind=Parameter), (Name=index Kind=Parameter), (Name=name Kind=Parameter), (Name=size Kind=Parameter), (Name=bufferName Kind=Local), (Name=x Kind=Local)
        // Captured symbols are 
        // Variables declared are (Name=bufferName Kind=Local), (Name=x Kind=Local)
        
        public long OnBuffer(Stream stream, long index, string name, long size)
        {
            var (bufferName, x) = Children[(int)index];
            Debug.Assert(name == bufferName);
            Debug.Assert(size != GetSize());
            Debug.Assert(size == x.GetSize());
            x.Write(stream);
            return size;
        }

        // A public instance method named GetOrComputeHeader with a type Vim.BFast.BFastHeader
        // operation kind is Block and type 
        // member references = Header, Header
        // assignments = Invocation
        // Written symbols are 
        // Read symbols are (Name=this Kind=Parameter)
        // Captured symbols are 
        // Variables declared are 
        
        public BFastHeader GetOrComputeHeader()
        {
            return Header ?? (Header = BufferSizes().ToArray().CreateBFastHeader(BufferNames().ToArray()));
        }

        // A private instance method named _add with a type Vim.BFast.BFastBuilder
        // operation kind is Block and type 
        // member references = Header, Children
        // assignments = Conversion
        // Written symbols are 
        // Read symbols are (Name=this Kind=Parameter), (Name=name Kind=Parameter), (Name=component Kind=Parameter)
        // Captured symbols are 
        // Variables declared are 
        
        private BFastBuilder _add(string name, IBFastComponent component)
        {
            Header = null;
            Children.Add((name, component));
            return this;
        }

        // A public instance method named Add with a type Vim.BFast.BFastBuilder
        // operation kind is Block and type 
        // member references = 
        // assignments = 
        // Written symbols are 
        // Read symbols are (Name=this Kind=Parameter), (Name=name Kind=Parameter), (Name=component Kind=Parameter)
        // Captured symbols are 
        // Variables declared are 
        
        public BFastBuilder Add(string name, IBFastComponent component)
        {
            return _add(name, component);
        }

        // A public instance method named Add with a type Vim.BFast.BFastBuilder
        // operation kind is Block and type 
        // member references = 
        // assignments = 
        // Written symbols are 
        // Read symbols are (Name=this Kind=Parameter), (Name=name Kind=Parameter), (Name=buffer Kind=Parameter)
        // Captured symbols are 
        // Variables declared are 
        
        public BFastBuilder Add(string name, IBuffer buffer)
        {
            return _add(name, new BufferAsBFastComponent(buffer));
        }

        // A public instance method named Add with a type Vim.BFast.BFastBuilder
        // operation kind is Block and type 
        // member references = Name
        // assignments = 
        // Written symbols are 
        // Read symbols are (Name=this Kind=Parameter), (Name=buffer Kind=Parameter)
        // Captured symbols are 
        // Variables declared are 
        
        public BFastBuilder Add(INamedBuffer buffer)
        {
            return Add(buffer.Name, buffer);
        }

        // A public instance method named Add with a type Vim.BFast.BFastBuilder
        // operation kind is Block and type 
        // member references = 
        // assignments = 
        // Written symbols are (Name=x Kind=Parameter), (Name=y Kind=Parameter)
        // Read symbols are (Name=this Kind=Parameter), (Name=buffers Kind=Parameter), (Name=x Kind=Parameter), (Name=y Kind=Parameter)
        // Captured symbols are 
        // Variables declared are (Name=x Kind=Parameter), (Name=y Kind=Parameter)
        
        public BFastBuilder Add(IEnumerable<INamedBuffer> buffers)
        {
            return buffers.Aggregate(this, (x, y) => x.Add(y));
        }

        // A public instance method named Add with a type Vim.BFast.BFastBuilder
        // operation kind is Block and type 
        // member references = 
        // assignments = 
        // Written symbols are 
        // Read symbols are (Name=this Kind=Parameter), (Name=name Kind=Parameter), (Name=buffers Kind=Parameter)
        // Captured symbols are 
        // Variables declared are 
        
        public BFastBuilder Add(string name, IEnumerable<INamedBuffer> buffers)
        {
            return Add(name, new BFastBuilder().Add(buffers));
        }

        // A public instance method named BufferNames with a type System.Collections.Generic.IEnumerable<string>
        // operation kind is Block and type 
        // member references = Children, Item1
        // assignments = 
        // Written symbols are (Name=x Kind=Parameter)
        // Read symbols are (Name=this Kind=Parameter), (Name=x Kind=Parameter)
        // Captured symbols are 
        // Variables declared are (Name=x Kind=Parameter)
        
        public IEnumerable<string> BufferNames()
        {
            return Children.Select(x => x.Item1);
        }

        // A public instance method named BufferSizes with a type System.Collections.Generic.IEnumerable<long>
        // operation kind is Block and type 
        // member references = Children, Item2
        // assignments = 
        // Written symbols are (Name=x Kind=Parameter)
        // Read symbols are (Name=this Kind=Parameter), (Name=x Kind=Parameter)
        // Captured symbols are 
        // Variables declared are (Name=x Kind=Parameter)
        
        public IEnumerable<long> BufferSizes()
        {
            return Children.Select(x => x.Item2.GetSize());
        }

        // A public instance property named Header with a type Vim.BFast.BFastHeader
        // No associated operation
        // No data-flow analysis could be created
                public BFastHeader Header { get; private set; }

        // A public instance property named Children with a type System.Collections.Generic.List<(string, Vim.BFast.IBFastComponent)>
        // No associated operation
        // No data-flow analysis could be created
        
        public List<(string, IBFastComponent)> Children { get; } = new List<(string, IBFastComponent)>();

    } // type
} // namespace
