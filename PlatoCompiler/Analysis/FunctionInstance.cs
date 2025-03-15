using System.Collections.Generic;
using System.Diagnostics;
using Ara3D.Utils;
using System.Linq;
using Plato.AST;
using Plato.Compiler.Symbols;
using Plato.Compiler.Types;

namespace Plato.Compiler.Analysis
{
    public enum FunctionInstanceKind
    {
        ConceptDeclared,
        ConceptImpemented,
        ConcreteType,
        FieldType,
        Constant,
        InterfaceExtension,
        ConceptInterface,
        Lambda,
    }

    /// <summary>
    /// Represents an instance of a function. Used for analyzing library functions and generating code.
    /// </summary>
    public class FunctionInstance
    {
        public FunctionDef Implementation { get; }
        public TypeSubstitutions Substitutions { get; private set; }
        public TypeDef Interface { get; }
        public ConcreteType ConcreteType { get; }
        public string ConceptName => Interface?.Name ?? "";
        public IReadOnlyList<string> ParameterNames { get; }
        public string Name => Implementation.Name;
        public TypeInstance ReturnType { get; }
        public string SignatureId => $"{Name}({ParameterTypes.JoinStringsWithComma()}):{ReturnType}";
        public IReadOnlyList<TypeInstance> ParameterTypes { get; }
        public override string ToString() => $"{SignatureId}:{ReturnType}";
        public bool IsImplicitCast => Name == ReturnType.Name && ParameterNames.Count == 1 && !ReturnType.Def.IsInterface();
        public InterfaceImplementation InterfaceImplementation { get; }
        public FunctionInstanceKind Kind { get; }
        public FunctionTypeVariableAnalysis TypeVariableAnalysis { get; }
        public IReadOnlyList<string> TypeVariables => TypeVariableAnalysis.GetTypeVariableNames();

        public FunctionInstance(FunctionDef implementation, ConcreteType type, InterfaceImplementation ci, FunctionInstanceKind kind, TypeSubstitutions substitutions = null)
        {
            Implementation = implementation;
            ConcreteType = type;
            InterfaceImplementation = ci;
            Kind = kind;

            Substitutions = substitutions;
            ParameterNames = Implementation.Parameters.Select(p => p.Name).ToList();
            
            // The first type in the list, if it is an interface, we are going to replace it with the concrete type 
            var first = Implementation.Parameters.FirstOrDefault();
            if (first?.Type.Def.IsInterface() == true && ci != null) 
            {
                Interface = first.Type.Def;
                ReplaceTypeVariables(first.Type, ci.TypeExpression);
            }

            // Create an initial version of the return type, and parameter types
            ReturnType = ToInstance(Implementation.ReturnType);
            ParameterTypes = Implementation.Parameters.Select(p => ToInstance(p.Type)).ToList();

            // Generate and gather the type variables 
            TypeVariableAnalysis = new FunctionTypeVariableAnalysis(ParameterTypes, ReturnType);

            // Create the final version of the return type, and parameter types
            ReturnType = TypeVariableAnalysis.ReturnType;
            ParameterTypes = TypeVariableAnalysis.ParameterTypes;
        }

        public void ReplaceTypeVariables(TypeExpression original, TypeExpression replace)
        {
            if (original.Name.StartsWith("$"))
            {
                Substitutions = new TypeSubstitutions(original.Name, replace, Substitutions);
                Debug.Assert(original.TypeArgs.Count == 0);
                return;
            }
            Debug.Assert(original.TypeArgs.Count == replace.TypeArgs.Count);
            for (var i = 0; i < original.TypeArgs.Count; ++i)
            {
                ReplaceTypeVariables(original.TypeArgs[i], replace.TypeArgs[i]);
            }
        }

        public TypeInstance ToInstance(TypeExpression expr)
        {
            // Repeat until we don't change anymore. 
            while (true)
            {
                var r = Substitutions?.Replace(expr) ?? expr;

                if (ReferenceEquals(r, expr)) 
                    return new TypeInstance(r, r.TypeArgs.Select(ToInstance));

                expr = r;
            }
        }
    }
}