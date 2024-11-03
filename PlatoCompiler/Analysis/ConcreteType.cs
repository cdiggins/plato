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
        public string Name => Type.Name;
        public TypeDef Type { get; }
        public LibraryFunctions Libraries { get; }
        public TypeSubstitutions Substitutions { get; }
        public IReadOnlyList<ConceptImplementation> Concepts { get; }
        public IReadOnlyList<ConceptImplementation> AllConcepts { get; }
        public IReadOnlyList<FunctionInstance> ConcreteFunctions { get; }
        public IReadOnlyList<FunctionInstance> FieldFunctions { get; }
        public IReadOnlyList<FunctionInstance> DeclaredFunctions { get; }
        public IReadOnlyList<FunctionInstance> ImplementedFunctions { get; }
        public IReadOnlyList<FunctionInstance> UnimplementedFunctions { get; }

        public ConcreteType(TypeDef type, LibraryFunctions libraries)
        {
            Verifier.AssertNotNull(type, nameof(type));
            Verifier.AssertNotNull(libraries, nameof(libraries));
            Verifier.Assert(type.IsConcrete());
            Type = type;
            Libraries = libraries;
            Substitutions = new TypeSubstitutions("Self", Type.ToTypeExpression());
            Concepts = type.Implements.Select(CreateConceptImplementation).ToList();
            AllConcepts = Concepts.AllDescendants().ToList();
            ConcreteFunctions = libraries.GetFunctionsForType(Type.ToTypeExpression()).Select(AnalyzeFunction).ToList();
            FieldFunctions = Type.Fields.Select(f => AnalyzeFunction(f.Function)).ToList();

            ImplementedFunctions = AllConcepts.SelectMany(c => c.ImplementedFunctions).Concat(ConcreteFunctions).Concat(FieldFunctions).ToList();
            DeclaredFunctions = AllConcepts.SelectMany(c => c.DeclaredFunctions).Distinct(d => d.SignatureId).ToList();

            var implementedSigs = new HashSet<string>(ImplementedFunctions.Select(f => f.SignatureId));
            UnimplementedFunctions = DeclaredFunctions.Where(f => !implementedSigs.Contains(f.SignatureId)).ToList();
        }

        public ConceptImplementation CreateConceptImplementation(TypeExpression type)
            => new ConceptImplementation(Libraries, this, Substitutions.Add(type), type);

        public FunctionInstance AnalyzeFunction(FunctionDef function)
            => new FunctionInstance(function, this, Substitutions);

        public IEnumerable<FunctionInstance> GetConceptFunctions()
        {
            var r = Concepts.SelectMany(c => c.AllFunctions()).ToList();
            // NOTE: the problem here, is that I only want Concept functions that exactly match this concrete type.
            // For example, there might be a function to defined specifically for IArray<Vector3D> and that would not 
            // work here. 
            // So there are "AllFunctions", and it depends on whether we are setting stuff  
            return r;
        }

        public IReadOnlyList<TypeExpression> DistinctFieldTypes
            => Type.Fields.Select(f => f.Type).Distinct().ToList();

        public override string ToString()
            => $"Concrete:{Type}";
    }
}