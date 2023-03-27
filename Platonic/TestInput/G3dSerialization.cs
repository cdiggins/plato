using System;
using System.IO;
using System.Linq;
using Vim.BFast;

namespace Vim.G3d
{
    public static partial class G3DExtension
    {
        public static void WriteAttribute(Stream stream, GeometryAttribute attribute, string name, long size)
        {
            var buffer = attribute.ToBuffer();
            if (buffer.NumBytes() != size)
                throw new Exception(
                    $"Internal error while writing attribute, expected number of bytes was {size} but instead was {buffer.NumBytes()}");
            if (buffer.Name != name)
                throw new Exception(
                    $"Internal error while writing attribute, expected name was {name} but instead was {buffer.Name}");
            stream.Write(buffer);
        }

        public static G3dWriter ToG3DWriter(this IGeometryAttributes self, G3dHeader? header = null)
        {
            return new G3dWriter(self, header);
        }

        public static void Write(this IGeometryAttributes self, Stream stream, G3dHeader? header = null)
        {
            self.ToG3DWriter(header).Write(stream);
        }

        public static void Write(this IGeometryAttributes self, string filePath, G3dHeader? header = null)
        {
            using (var stream = File.OpenWrite(filePath))
            {
                self.Write(stream, header);
            }
        }

        public static byte[] WriteToBytes(this IGeometryAttributes self)
        {
            using (var memoryStream = new MemoryStream())
            {
                self.Write(memoryStream);
                return memoryStream.ToArray();
            }
        }

        public static bool TryReadHeader(Stream stream, long size, out G3dHeader outHeader)
        {
            var buffer = stream.ReadArray<byte>((int)size);

            if (buffer[0] == G3dHeader.MagicA && buffer[1] == G3dHeader.MagicB)
            {
                outHeader = G3dHeader.FromBytes(buffer);
                return true;
            }

            outHeader = default;
            return false;
        }

        public static bool TryReadGeometryAttribute(Stream stream, string name, long size,
            out GeometryAttribute geometryAttribute)
        {
            geometryAttribute = null;

            bool ReadFailure()
            {
                // Update the seek head to consume the stream and return false.
                stream.Seek((int)size, SeekOrigin.Current);
                return false;
            }

            if (!AttributeDescriptor.TryParse(name, out var attributeDescriptor))
                // Skip unknown attribute descriptors.
                return ReadFailure();

            // Populate a default attribute with the parsed attribute descriptor.
            GeometryAttribute defaultAttribute;
            try
            {
                defaultAttribute = attributeDescriptor.ToDefaultAttribute(0);
            }
            catch
            {
                // Eat the exception and return.
                return ReadFailure();
            }

            // Success; consume the stream.
            geometryAttribute = defaultAttribute.Read(stream, size);
            return true;
        }

        public static G3D ReadG3d(this Stream stream, Func<string, string> renameFunc = null)
        {
            var header = G3dHeader.Default;

            GeometryAttribute ReadG3dSegment(Stream s2, string name, long size)
            {
                name = renameFunc?.Invoke(name) ?? name;

                // Check for the G3dHeader 
                if (name == "meta" && size == 8)
                {
                    if (TryReadHeader(s2, size, out var outHeader))
                        // Assign to the header variable in the closure.
                        header = outHeader;

                    return null;
                }

                return TryReadGeometryAttribute(s2, name, size, out var geometryAttribute)
                    ? geometryAttribute
                    : null;
            }


            // TODO: Need to implement the BFast reader
            //var results = stream.ReadBFast(ReadG3dSegment).Select(r => r.Item2);
            //return new G3D(results.Where(x => x != null), header);
            throw new NotImplementedException();
        }
    }
}