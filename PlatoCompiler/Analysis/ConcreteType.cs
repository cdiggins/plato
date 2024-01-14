using System.Collections.Generic;
using System.Linq;
using Ara3D.Utils;
using Plato.Compiler.Symbols;
using Plato.Compiler.Types;

namespace Plato.Compiler.Analysis
{
    /// <summary>
    /// Holds a list of all the functions associated with the type,
    /// and the implemented concepts.
    /// </summary>
    public class ConcreteType
    {
        public TypeDefinition Type { get; }
        public LibrarySet Libraries { get; }
        public IReadOnlyList<ConceptImplementation> Concepts { get; }
        public IReadOnlyList<FunctionInstance> ConcreteFunctions { get; }

        public ConcreteType(TypeDefinition type, LibrarySet libraries)
        {
            Verifier.AssertNotNull(type, nameof(type));
            Verifier.AssertNotNull(libraries, nameof(libraries));
            Verifier.Assert(type.IsConcrete());
            Type = type;
            Libraries = libraries;
            Concepts = type.Implements.Select(CreateConceptImplementation).ToList();
            ConcreteFunctions = libraries.GetFunctionsForType(Type.Name).Select(AnalyzeConcreteFunction).ToList();
        }

        public ConceptImplementation CreateConceptImplementation(TypeExpression type)
            => new ConceptImplementation(Libraries, Type, TypeSubstitutions.Create(type), type);

        public FunctionInstance AnalyzeConcreteFunction(FunctionDefinition function)
            => new FunctionInstance(Type, function, new TypeSubstitutions());

        public IEnumerable<FunctionInstance> GetConceptFunctions()
            => Concepts.SelectMany(c => c.AllFunctions());
    }
}