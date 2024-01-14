using System.Collections.Generic;
using Ara3D.Utils;
using System.Linq;
using Plato.Compiler.Symbols;
using System.Runtime.InteropServices.ComTypes;
using Plato.Compiler.Types;

namespace Plato.Compiler.Analysis
{
    /// <summary>
    /// Represents an instance of a function. Used for analyzing library functions and generating code.
    /// </summary>
    public class FunctionInstance
    {
        public TypeDefinition ConcreteType { get; }
        public FunctionDefinition Implementation { get; }
        public TypeSubstitutions Substitutions { get; }
        public string ConceptName { get; }

        public FunctionInstance(
            TypeDefinition concreteType,
            FunctionDefinition implementation,
            TypeSubstitutions substitutions)
        {
            ConcreteType = concreteType;
            Implementation = implementation;
            Substitutions = substitutions;
            ConceptName = Parameters.Count == 0
                ? ""
                : Parameters[0].Type.Definition.IsConcept()
                    ? Parameters[0].Type.Name
                    : "";
        }

        public string GetTypeString(TypeExpression te)
        {
            te = Substitutions.Replace(te);
            if (te.Name == ConceptName) 
                return ConcreteType.Name;
            return te.Name + JoinTypeParameters(te.TypeArgs.Select(GetTypeString));
        }

        public static string JoinTypeParameters(IEnumerable<string> parameters)
        {
            var r = parameters.JoinStrings(", ");
            return r.Length == 0 ? r : $"<{r}>";
        }

        public string Name 
            => Implementation.Name;

        public TypeExpression ReturnType
            => Implementation.ReturnType;

        public IReadOnlyList<ParameterDefinition> Parameters 
            => Implementation.Parameters;
    }
}