using System.Collections.Generic;
using Plato.Compiler.Symbols;

namespace Plato.CSharpWriter
{
    public class TypeFunctions
    {
        public TypeDefinition Type { get; }
        public IReadOnlyList<FunctionAnalysis> Functions { get;  }
        public TypeConverter Converter { get; }

        public TypeFunctions(TypeDefinition type, LibraryAnalysis libFuncs)
        {

        }
    }
}