// Indexes library functions by receiver and declaration for analysis.
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

        /// <summary>The functions whose first parameter is a CLOSED instance of a generic type
        /// (<c>F(m: Maybe&lt;Number&gt;)</c> for <c>Maybe&lt;T&gt;</c>): a type argument names a type,
        /// not a <c>$</c> variable, so <see cref="MatchesType"/> pairs them with no declaration and
        /// they cannot be members of the generic struct (TKT-443).</summary>
        public IEnumerable<FunctionDef> GetFunctionsForClosedInstancesOf(TypeExpression genericType)
            => genericType.TypeArgs.Count == 0
                ? Enumerable.Empty<FunctionDef>()
                : AllFunctions().Where(f => f.Parameters.Count > 0
                    && f.Parameters[0].Type.Name == genericType.Name
                    && f.Parameters[0].Type.TypeArgs.Count == genericType.TypeArgs.Count
                    && !MatchesType(f.Parameters[0].Type, genericType));

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