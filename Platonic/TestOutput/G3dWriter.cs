using System.IO;

using System.Linq;

using Vim.BFast;

using Vim.LinqArray;

namespace Vim.G3d
{
    // Type has fields False
    // Type has writable fields False
    // Type has public setters False
    public class G3dWriter
    : IBFastComponent

    {
        // A public instance method named .ctor with a type void
        // operation kind is Block and type 
        // member references = Attributes, Meta, Default, Names, Name, Meta, Attributes, Name, Sizes, Meta, Attributes, Header, Sizes, Names
        // assignments = ParameterReference, Conversion, Invocation, Invocation, Invocation
        // Written symbols are (Name=attr Kind=Parameter), (Name=attr Kind=Parameter)
        // Read symbols are (Name=this Kind=Parameter), (Name=g Kind=Parameter), (Name=header Kind=Parameter), (Name=attr Kind=Parameter), (Name=attr Kind=Parameter)
        // Captured symbols are 
        // Variables declared are (Name=attr Kind=Parameter), (Name=attr Kind=Parameter)
                public G3dWriter(IGeometryAttributes g, G3dHeader? header = null)
        {
            Attributes = g;
            Meta = (header ?? G3dHeader.Default).ToBytes().ToNamedBuffer("meta");
            Names = new[] { Meta.Name }.Concat(g.Attributes.ToEnumerable().Select(attr => attr.Name)).ToArray();
            Sizes = new[] { Meta.NumBytes() }.Concat(g.Attributes.ToEnumerable().Select(attr => attr.GetByteSize()))
                .ToArray();
            Header = Sizes.CreateBFastHeader(Names);
        }

        // A public instance method named GetSize with a type long
        // operation kind is Block and type 
        // member references = DataEnd, Preamble, Header
        // assignments = 
        // Written symbols are 
        // Read symbols are (Name=this Kind=Parameter)
        // Captured symbols are 
        // Variables declared are 
        
        public long GetSize()
        {
            return Header.Preamble.DataEnd;
        }

        // A public instance method named Write with a type void
        // operation kind is Block and type 
        // member references = Header, Header, Names, Sizes, Meta, this[], Attributes, Attributes
        // assignments = 
        // Written symbols are (Name=_stream Kind=Parameter), (Name=index Kind=Parameter), (Name=name Kind=Parameter), (Name=size Kind=Parameter)
        // Read symbols are (Name=this Kind=Parameter), (Name=stream Kind=Parameter), (Name=_stream Kind=Parameter), (Name=index Kind=Parameter), (Name=name Kind=Parameter), (Name=size Kind=Parameter)
        // Captured symbols are (Name=this Kind=Parameter)
        // Variables declared are (Name=_stream Kind=Parameter), (Name=index Kind=Parameter), (Name=name Kind=Parameter), (Name=size Kind=Parameter)
        
        public void Write(Stream stream)
        {
            stream.WriteBFastHeader(Header);
            stream.WriteBFastBody(Header, Names, Sizes, (_stream, index, name, size) =>
            {
                if (index == 0)
                    _stream.Write(Meta);
                else
                    G3DExtension.WriteAttribute(_stream, Attributes.Attributes[(int)index - 1], name, size);
                return size;
            });
        }

        // A public instance property named Meta with a type Vim.BFast.INamedBuffer
        // No associated operation
        // No data-flow analysis could be created
        
        public INamedBuffer Meta { get; }

        // A public instance property named Names with a type string[]
        // No associated operation
        // No data-flow analysis could be created
                public string[] Names { get; }

        // A public instance property named Sizes with a type long[]
        // No associated operation
        // No data-flow analysis could be created
                public long[] Sizes { get; }

        // A private instance property named Header with a type Vim.BFast.BFastHeader
        // No associated operation
        // No data-flow analysis could be created
                private BFastHeader Header { get; }

        // A private instance property named Attributes with a type Vim.G3d.IGeometryAttributes
        // No associated operation
        // No data-flow analysis could be created
                private IGeometryAttributes Attributes { get; }

    } // type
} // namespace
