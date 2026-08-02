using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Utilities;

namespace PlatoVSIX
{
    public static class PlatoContentTypeDefinition
    {
        [Export(typeof(ContentTypeDefinition))] [Name("plato")] [BaseDefinition("code")]
        public static ContentTypeDefinition PlatoContentType;

        [Export(typeof(FileExtensionToContentTypeDefinition))] [ContentType("plato")] [FileExtension(".plato")]
        public static FileExtensionToContentTypeDefinition PlatoFileExtension;
    }
}