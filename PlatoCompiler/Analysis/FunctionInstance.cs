using System;
using System.Collections.Generic;
using Ara3D.Utils;
using System.Linq;
using Plato.Compiler.Symbols;
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
        public TypeSubstitutions Substitutions { get; private set; }
        public TypeDefinition Concept { get; }
        public string ConceptName => Concept?.Name ?? "";
        public IReadOnlyList<string> ParameterNames { get; }
        public string Name => Implementation.Name;
        public TypeInstance ReturnType { get; }
        public string SignatureId => $"{Name}({ParameterTypes.JoinStringsWithComma()})";
        public IReadOnlyList<TypeInstance> ParameterTypes { get; }
        public IReadOnlyList<TypeParameterDefinition> UsedTypeParameters { get; }
        public override string ToString() => $"{SignatureId}:{ReturnType}";

        // TODO: this might be necessary in the future, when choosing the correct type parameter is difficult.
        //public Dictionary<TypeParameterDefinition, TypeParameterDefinition> Unifiers { get; } =
        //    new Dictionary<TypeParameterDefinition, TypeParameterDefinition>();

        public FunctionInstance(
            TypeDefinition concreteType,
            FunctionDefinition implementation,
            TypeSubstitutions substitutions)
        {
            ConcreteType = concreteType;
            Implementation = implementation;
            Substitutions = substitutions;
            ParameterNames = Implementation.Parameters.Select(p => p.Name).ToList();
            
            var first = Implementation.Parameters.FirstOrDefault();
            if (first?.Type.Definition.IsConcept() == true)
                Concept = first.Type.Definition;

            foreach (var p in Implementation.Parameters)
                GatherTypeVariables(p.Type);

            ReturnType = ToInstance(Implementation.ReturnType);
            ParameterTypes = Implementation.Parameters.Select(p => ToInstance(p.Type)).ToList();

            UsedTypeParameters = ParameterTypes.SelectMany(t => t.SelfAndDescendants())
                .Select(t => t.Definition)
                .OfType<TypeParameterDefinition>().Distinct().
                ToList();
        }

        public void GatherTypeVariables(TypeExpression expr)
        {
            for (var i = 0; i < expr.TypeArgs.Count; ++i)
            {
                var ta = expr.TypeArgs[i];
                var tp = expr.Definition.TypeParameters[i];

                if (ta.Name.StartsWith("$"))
                {
                    Substitutions = Substitutions.Add(ta.Name, tp.ToTypeExpression());
                }

                GatherTypeVariables(ta);
            }
        }

        public TypeInstance ToInstance(TypeExpression expr)
        {
            // Repeat until we don't change anymore. 
            while (true)
            {
                var r = Substitutions.Replace(expr);

                if (ReferenceEquals(r, expr)) 
                    return new TypeInstance(r.Definition, r.TypeArgs.Select(ToInstance));

                expr = r;
            }
        }
    }
}