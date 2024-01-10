using System;
using Ara3D.Utils;

namespace Plato.CSharpWriter
{
    public static class CSharpWriterExtensions
    {

        public static SymbolWriterCSharp ToCSharp(this Compiler.Compiler compiler, DirectoryPath outputFolder)
        {
            var writer = new SymbolWriterCSharp(compiler, outputFolder);
            return writer.WriteAll();
        }
    }
}
