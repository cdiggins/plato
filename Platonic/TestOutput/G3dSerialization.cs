using System;

using System.IO;

using System.Linq;

using Vim.BFast;

namespace Vim.G3d
{
    // Type has fields False
    // Type has writable fields False
    // Type has public setters False
    public static partial class G3DExtension
    {
        // A public static method named WriteAttribute with a type void
        // operation kind is Block and type 
        // member references = Name, Name
        // assignments = 
        // Written symbols are (Name=buffer Kind=Local)
        // Read symbols are (Name=stream Kind=Parameter), (Name=attribute Kind=Parameter), (Name=name Kind=Parameter), (Name=size Kind=Parameter), (Name=buffer Kind=Local)
        // Captured symbols are 
        // Variables declared are (Name=buffer Kind=Local)
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

        // A public static method named ToG3DWriter with a type Vim.G3d.G3dWriter
        // operation kind is Block and type 
        // member references = 
        // assignments = 
        // Written symbols are 
        // Read symbols are (Name=self Kind=Parameter), (Name=header Kind=Parameter)
        // Captured symbols are 
        // Variables declared are 
        
        public static G3dWriter ToG3DWriter(this IGeometryAttributes self, G3dHeader? header = null)
        {
            return new G3dWriter(self, header);
        }

        // A public static method named Write with a type void
        // operation kind is Block and type 
        // member references = 
        // assignments = 
        // Written symbols are 
        // Read symbols are (Name=self Kind=Parameter), (Name=stream Kind=Parameter), (Name=header Kind=Parameter)
        // Captured symbols are 
        // Variables declared are 
        
        public static void Write(this IGeometryAttributes self, Stream stream, G3dHeader? header = null)
        {
            self.ToG3DWriter(header).Write(stream);
        }

        // A public static method named Write with a type void
        // operation kind is Block and type 
        // member references = 
        // assignments = 
        // Written symbols are (Name=stream Kind=Local)
        // Read symbols are (Name=self Kind=Parameter), (Name=filePath Kind=Parameter), (Name=header Kind=Parameter), (Name=stream Kind=Local)
        // Captured symbols are 
        // Variables declared are (Name=stream Kind=Local)
        
        public static void Write(this IGeometryAttributes self, string filePath, G3dHeader? header = null)
        {
            using (var stream = File.OpenWrite(filePath))
            {
                self.Write(stream, header);
            }
        }

        // A public static method named WriteToBytes with a type byte[]
        // operation kind is Block and type 
        // member references = 
        // assignments = 
        // Written symbols are (Name=memoryStream Kind=Local)
        // Read symbols are (Name=self Kind=Parameter), (Name=memoryStream Kind=Local)
        // Captured symbols are 
        // Variables declared are (Name=memoryStream Kind=Local)
        
        public static byte[] WriteToBytes(this IGeometryAttributes self)
        {
            using (var memoryStream = new MemoryStream())
            {
                self.Write(memoryStream);
                return memoryStream.ToArray();
            }
        }

        // A public static method named TryReadHeader with a type bool
        // operation kind is Block and type 
        // member references = MagicA, MagicB
        // assignments = Invocation, Conversion
        // Written symbols are (Name=outHeader Kind=Parameter), (Name=buffer Kind=Local)
        // Read symbols are (Name=stream Kind=Parameter), (Name=size Kind=Parameter), (Name=buffer Kind=Local)
        // Captured symbols are 
        // Variables declared are (Name=buffer Kind=Local)
        
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

        // A public static method named TryReadGeometryAttribute with a type bool
        // operation kind is Block and type 
        // member references = Current
        // assignments = Conversion, Invocation, Invocation
        // Written symbols are (Name=geometryAttribute Kind=Parameter), (Name=attributeDescriptor Kind=Local), (Name=defaultAttribute Kind=Local)
        // Read symbols are (Name=stream Kind=Parameter), (Name=name Kind=Parameter), (Name=size Kind=Parameter), (Name=attributeDescriptor Kind=Local), (Name=defaultAttribute Kind=Local)
        // Captured symbols are (Name=stream Kind=Parameter), (Name=size Kind=Parameter)
        // Variables declared are (Name=attributeDescriptor Kind=Local), (Name=defaultAttribute Kind=Local)
        
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

        // A public static method named ReadG3d with a type Vim.G3d.G3D
        // operation kind is Block and type 
        // member references = Default
        // assignments = Coalesce, LocalReference
        // Written symbols are (Name=header Kind=Local), (Name=s2 Kind=Parameter), (Name=name Kind=Parameter), (Name=size Kind=Parameter), (Name=outHeader Kind=Local), (Name=geometryAttribute Kind=Local)
        // Read symbols are (Name=renameFunc Kind=Parameter), (Name=s2 Kind=Parameter), (Name=name Kind=Parameter), (Name=size Kind=Parameter), (Name=outHeader Kind=Local), (Name=geometryAttribute Kind=Local)
        // Captured symbols are (Name=renameFunc Kind=Parameter), (Name=header Kind=Local)
        // Variables declared are (Name=header Kind=Local), (Name=s2 Kind=Parameter), (Name=name Kind=Parameter), (Name=size Kind=Parameter), (Name=outHeader Kind=Local), (Name=geometryAttribute Kind=Local)
        
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

    } // type
} // namespace
