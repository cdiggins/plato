using System.Collections.Generic;
using System.Linq;
using Ara3D.Geometry.Compiler.Symbols;
using Ara3D.Geometry.Compiler.Types;

namespace Ara3D.Geometry.Compiler.Analysis
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

        public static bool MatchesType(TypeExpression parameter, TypeExpression target)
        {
            if (parameter.Name != target.Name)
                return false;
            
            if (parameter.TypeArgs.Count != target.TypeArgs.Count)
                return false;

            for (var i = 0; i < parameter.TypeArgs.Count; i++)
            {
                var srcArg = parameter.TypeArgs[i];
                var targetArg = target.TypeArgs[i];

                if (srcArg.Name.StartsWith("$"))
                    continue;

                if (srcArg.Name != targetArg.Name)
                    return false;
            }

            return true;
        }

        public IEnumerable<FunctionDef> GetFunctionsForType(TypeExpression typeExpr)
        {
            //var typeName = typeExpr.Name;
            var r = AllFunctions()
                .Where(f => f.Parameters.Count > 0 
                    && MatchesType(f.Parameters[0].Type, typeExpr));
            return r;
        }

        public IEnumerable<FunctionDef> AllConstants()
            => AllFunctions().Where(f => f.Parameters.Count == 0);

        public LibraryFunctions(Compilation c)
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