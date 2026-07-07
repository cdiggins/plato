using System.Collections.Generic;
using System.Linq;
using Ara3D.Geometry.Compiler.Symbols;
using Ara3D.Utils;
using Ara3D.Geometry.Compiler.Types;

namespace Ara3D.Geometry.Compiler.Analysis
{
    /// <summary>
    /// Represents a specific concept implementation in a type. 
    /// </summary>
    public class InterfaceImplementation : ITree<InterfaceImplementation>
    {
        public string Name => Interface.Name;
        public LibraryFunctions Libraries { get; }
        public InterfaceImplementation InheritedFrom { get; }
        public IEnumerable<InterfaceImplementation> Children { get; }
        public TypeExpression TypeExpression { get; }
        public TypeDef Interface => TypeExpression.Def;
        public ConcreteType ConcreteType { get;}
        public TypeSubstitutions Substitutions { get; }
        public IReadOnlyList<FunctionInstance> ImplementedFunctions { get; }
        public IReadOnlyList<FunctionInstance> DeclaredFunctions { get; }

        public InterfaceImplementation(
            LibraryFunctions libraries,
            ConcreteType concreteType, 
            TypeSubstitutions substitutions, 
            TypeExpression typeExpression,
            InterfaceImplementation inheritedFrom = null)
        {
            Verifier.AssertNotNull(libraries, nameof(libraries));
            Verifier.AssertNotNull(concreteType, nameof(concreteType));
            Verifier.AssertNotNull(substitutions, nameof(substitutions));
            Verifier.AssertNotNull(substitutions, nameof(typeExpression));

            Verifier.Assert(typeExpression.Def.IsInterface());

            Libraries = libraries;
            InheritedFrom = inheritedFrom;
            ConcreteType = concreteType;
            TypeExpression = typeExpression;
            Substitutions = substitutions.Add(Interface.Name, ConcreteType.TypeDef.ToTypeExpression());

            Verifier.Assert(Interface.IsInterface());

            Children = Interface.Inherits.Select(CreateInheritedConceptImplementation).ToList();

            var implementedFunctions = libraries.GetFunctionsForType(TypeExpression);
            ImplementedFunctions = implementedFunctions.Select(f => AnalyzeConceptFunction(f, FunctionInstanceKind.InterfaceImplemented)).ToList();

            DeclaredFunctions = Interface.Functions.Select(f => AnalyzeConceptFunction(f, FunctionInstanceKind.InterfaceDeclared)).ToList();
        }

        public InterfaceImplementation CreateInheritedConceptImplementation(TypeExpression inheritsType)
            => new InterfaceImplementation(Libraries, ConcreteType, Substitutions.Add(inheritsType), inheritsType, this);

        public FunctionInstance AnalyzeConceptFunction(FunctionDef function, FunctionInstanceKind kind)
            => new FunctionInstance(function, ConcreteType.TypeDef, this, kind, Substitutions);

        public IEnumerable<FunctionInstance> AllFunctions()
            => ImplementedFunctions.Concat(Children.SelectMany(i => i.AllFunctions()));

        public override string ToString()
            => TypeExpression.ToString();
    }
}