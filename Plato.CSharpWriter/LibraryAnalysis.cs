using System.Collections.Generic;
using System.Linq;
using Plato.Compiler.Symbols;
using Plato.Compiler.Types;

namespace Plato.CSharpWriter
{
    /// <summary>
    /// Used to aggregate all functions from all libraries. 
    /// </summary>
    public class LibraryAnalysis
    {
        public Dictionary<string, List<FunctionDefinition>> Lookup { get; } =
            new Dictionary<string, List<FunctionDefinition>>();

        public IEnumerable<FunctionDefinition> AllFunctions()
            => Lookup.Values.SelectMany(g => g);

        public IEnumerable<FunctionDefinition> GetFunctionsForType(string typeName)
            => AllFunctions().Where(f => f.Parameters.Count > 0 && f.Parameters[0].Type.Name == typeName);

        public LibraryAnalysis(Compiler.Compiler c)
        {
            foreach (var t in c.AllTypeAndLibraryDefinitions)
            {
                if (!t.IsLibrary())
                    continue;
                var list = new List<FunctionDefinition>();
                foreach (var m in t.Methods)
                {
                    list.Add(m.Function);
                }
                Lookup.Add(t.Name, list);
            }
        }
    }
}