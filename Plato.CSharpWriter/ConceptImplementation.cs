using System.Collections.Generic;
using System.Linq;
using Ara3D.Utils;
using Plato.Compiler.Symbols;
using Plato.Compiler.Types;

namespace Plato.CSharpWriter
{
    /// <summary>
    /// Represents a specific concept implementation in a type. 
    /// </summary>
    public class ConceptImplementation
    {
        public LibraryAnalysis Libraries { get; }
        public ConceptImplementation InheritedFrom { get; }
        public IReadOnlyList<ConceptImplementation> Inherited { get; }
        public TypeExpression Expression { get; }
        public TypeDefinition Concept => Expression.Definition;
        public TypeDefinition ConcreteType { get;}
        public TypeSubstitutions Substitutions { get; }
        public IReadOnlyList<FunctionAnalysis> Functions { get; }

        public ConceptImplementation(
            LibraryAnalysis libraries,
            TypeDefinition concreteType, 
            TypeSubstitutions substitutions, 
            TypeExpression expression,
            ConceptImplementation inheritedFrom = null)
        {
            Verifier.AssertNotNull(libraries, nameof(libraries));
            Verifier.AssertNotNull(concreteType, nameof(concreteType));
            Verifier.AssertNotNull(substitutions, nameof(substitutions));
            Verifier.AssertNotNull(substitutions, nameof(expression));

            Verifier.Assert(concreteType.IsConcrete());
            Verifier.Assert(expression.Definition.IsConcept());

            Libraries = libraries;
            InheritedFrom = inheritedFrom;
            ConcreteType = concreteType;
            Substitutions = substitutions;
            Expression = expression;

            Verifier.Assert(Concept.IsConcept());

            Inherited = Concept.Inherits.Select(CreateInheritedConceptImplementation).ToList();

            var implementedFunctions = libraries.GetFunctionsForType(Concept.Name);
            Functions = implementedFunctions.Select(AnalyzeConceptFunction).ToList();
        }

        public ConceptImplementation CreateInheritedConceptImplementation(TypeExpression inheritsType)
            => new ConceptImplementation(Libraries, ConcreteType, Substitutions.Add(inheritsType), inheritsType, this);

        public FunctionAnalysis AnalyzeConceptFunction(FunctionDefinition function)
            => new FunctionAnalysis(ConcreteType, function, Substitutions);
    }
}