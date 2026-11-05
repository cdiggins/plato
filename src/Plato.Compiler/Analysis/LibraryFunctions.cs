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

        // The library functions by the name of their first parameter's type, each list in AllFunctions
        // order: ConcreteType asks for every type and interface, so a scan of all functions costs seconds.
        private readonly Dictionary<string, List<FunctionDef>> _byReceiverName = new();

        private IEnumerable<FunctionDef> WithReceiver(string typeName)
            => _byReceiverName.TryGetValue(typeName, out var functions) ? functions : Enumerable.Empty<FunctionDef>();

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
            return WithReceiver(typeExpr.Name).Where(f => MatchesType(f.Parameters[0].Type, typeExpr));
        }

        /// <summary>The functions whose first parameter is a CLOSED instance of a generic type
        /// (<c>F(m: Maybe&lt;Number&gt;)</c> for <c>Maybe&lt;T&gt;</c>): a type argument names a type,
        /// not a <c>$</c> variable, so <see cref="MatchesType"/> pairs them with no declaration and
        /// they cannot be members of the generic struct (TKT-443).</summary>
        public IEnumerable<FunctionDef> GetFunctionsForClosedInstancesOf(TypeExpression genericType)
            => genericType.TypeArgs.Count == 0
                ? Enumerable.Empty<FunctionDef>()
                : WithReceiver(genericType.Name).Where(f =>
                    f.Parameters[0].Type.TypeArgs.Count == genericType.TypeArgs.Count
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
            foreach (var f in AllFunctions().Where(f => f.Parameters.Count > 0))
            {
                var name = f.Parameters[0].Type.Name;
                if (!_byReceiverName.TryGetValue(name, out var functions))
                    _byReceiverName.Add(name, functions = new List<FunctionDef>());
                functions.Add(f);
            }
        }
    }
}