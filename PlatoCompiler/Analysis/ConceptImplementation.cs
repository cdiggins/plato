using System.Collections.Generic;
using System.Linq;
using Ara3D.Utils;
using Plato.Compiler.Symbols;
using Plato.Compiler.Types;

namespace Plato.Compiler.Analysis
{
    /// <summary>
    /// Represents a specific concept implementation in a type. 
    /// </summary>
    public class ConceptImplementation : ITree<ConceptImplementation>
    {
        public string Name => Concept.Name;
        public LibraryFunctions Libraries { get; }
        public ConceptImplementation InheritedFrom { get; }
        public IEnumerable<ConceptImplementation> Children { get; }
        public TypeExpression Expression { get; }
        public TypeDef Concept => Expression.Def;
        public ConcreteType ConcreteType { get;}
        public TypeSubstitutions Substitutions { get; }
        public IReadOnlyList<FunctionInstance> ImplementedFunctions { get; }
        public IReadOnlyList<FunctionInstance> DeclaredFunctions { get; }

        public ConceptImplementation(
            LibraryFunctions libraries,
            ConcreteType concreteType, 
            TypeSubstitutions substitutions, 
            TypeExpression expression,
            ConceptImplementation inheritedFrom = null)
        {
            Verifier.AssertNotNull(libraries, nameof(libraries));
            Verifier.AssertNotNull(concreteType, nameof(concreteType));
            Verifier.AssertNotNull(substitutions, nameof(substitutions));
            Verifier.AssertNotNull(substitutions, nameof(expression));

            Verifier.Assert(expression.Def.IsConcept());

            Libraries = libraries;
            InheritedFrom = inheritedFrom;
            ConcreteType = concreteType;
            Expression = expression;
            Substitutions = substitutions.Add(Concept.Name, ConcreteType.Type.ToTypeExpression());

            Verifier.Assert(Concept.IsConcept());

            Children = Concept.Inherits.Select(CreateInheritedConceptImplementation).ToList();

            var implementedFunctions = libraries.GetFunctionsForType(Expression);
            ImplementedFunctions = implementedFunctions.Select(f => AnalyzeConceptFunction(f, FunctionInstanceKind.ConceptImpemented)).ToList();

            DeclaredFunctions = Concept.Functions.Select(f => AnalyzeConceptFunction(f, FunctionInstanceKind.ConceptDeclared)).ToList();
        }

        public ConceptImplementation CreateInheritedConceptImplementation(TypeExpression inheritsType)
            => new ConceptImplementation(Libraries, ConcreteType, Substitutions.Add(inheritsType), inheritsType, this);

        public FunctionInstance AnalyzeConceptFunction(FunctionDef function, FunctionInstanceKind kind)
            => new FunctionInstance(function, ConcreteType, this, kind, Substitutions);

        public IEnumerable<FunctionInstance> AllFunctions()
            => ImplementedFunctions.Concat(Children.SelectMany(i => i.AllFunctions()));
    }
}