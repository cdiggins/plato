using System.Collections.Generic;
using System.Linq;
using Plato.Compiler.Symbols;
using Plato.Compiler.Types;

namespace Plato.CSharpWriter
{
    public class LibraryFunctions
    {
        public Dictionary<string, List<FuncInfo>> Lookup { get; } =
            new Dictionary<string, List<FuncInfo>>();


        public IEnumerable<FuncInfo> AllFunctions()
            => Lookup.Values.SelectMany(g => g);

        public LibraryFunctions(Compiler.Compiler c)
        {
            foreach (var t in c.AllTypeAndLibraryDefinitions)
            {
                if (!t.IsLibrary())
                    continue;

                foreach (var m in t.Methods)
                {
                    var fi = new FuncInfo(m.Function)
                    
                }
            }
        }
    }

    public class TypeFunctions
    {
        public TypeDefinition Type { get; }
        public IReadOnlyList<FuncInfo> Functions { get;  }
        public TypeConverter Converter { get; }

        public TypeFunctions(TypeDefinition type, LibraryFunctions libFuncs)
        {

        }
    }
}