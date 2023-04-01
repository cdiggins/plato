using System.IO;
using System.Linq;
using Vim.BFast;
using Vim.LinqArray;

namespace Vim.G3d
{
    /// <summary>
    /// This is a helper class for writing G3Ds
    /// </summary>
    public class G3dWriter : IBFastComponent
    {
        public G3dWriter(IGeometryAttributes g, G3dHeader? header = null)
        {
            Attributes = g;
            Meta = (header ?? G3dHeader.Default).ToBytes().ToNamedBuffer("meta");
            Names = new[] { Meta.Name }.Concat(g.Attributes.ToEnumerable().Select(attr => attr.Name)).ToArray();
            Sizes = new[] { Meta.NumBytes() }.Concat(g.Attributes.ToEnumerable().Select(attr => attr.GetByteSize()))
                .ToArray();
            Header = Sizes.CreateBFastHeader(Names);
        }

        public INamedBuffer Meta { get; }
        public string[] Names { get; }
        public long[] Sizes { get; }
        private BFastHeader Header { get; }
        private IGeometryAttributes Attributes { get; }

        public long GetSize()
        {
            return Header.Preamble.DataEnd;
        }

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
    }
}