using System.Collections.Generic;
using System.Linq;
using Plato.Compiler.Symbols;
using Plato.Compiler.Types;

namespace Plato.Compiler.Analysis
{
    /// <summary>
    /// Used to aggregate all functions from all libraries. 
    /// </summary>
    public class LibraryFunctions
    {
        public Dictionary<string, List<FunctionDef>> Lookup { get; } =
            new Dictionary<string, List<FunctionDef>>();

        public IEnumerable<FunctionDef> AllFunctions()
            => Lookup.Values.SelectMany(g => g);

        public IEnumerable<FunctionDef> GetFunctionsForType(string typeName)
            => AllFunctions().Where(f => f.Parameters.Count > 0 && f.Parameters[0].Type.Name == typeName);

        public IEnumerable<FunctionDef> AllConstants()
            => AllFunctions().Where(f => f.Parameters.Count == 0);

        public LibraryFunctions(Plato.Compiler.Compilation c)
        {
            foreach (var t in c.AllTypeAndLibraryDefinitions)
            {
                if (!t.IsLibrary())
                    continue;
                var list = new List<FunctionDef>();
                foreach (var m in t.Methods)
                {
                    list.Add(m.Function);
                }
                Lookup.Add(t.Name, list);
            }
        }
    }
}