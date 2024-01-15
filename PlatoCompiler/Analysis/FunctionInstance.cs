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
        public TypeSubstitutions Substitutions { get; }
        public TypeDefinition Concept { get; }
        public string ConceptName => Concept?.Name ?? "";
        public IReadOnlyList<string> ParameterNames { get; }
        public string Name => Implementation.Name;
        public TypeInstance ReturnType { get; }
        public string SignatureId => $"{Name}({ParameterTypes.JoinStringsWithComma()})";
        public IReadOnlyList<TypeInstance> ParameterTypes { get; }
        public Dictionary<string, List<TypeParameterDefinition>> TypeVarLookup { get; } 
            = new Dictionary<string, List<TypeParameterDefinition>>();
        public IReadOnlyList<TypeParameterDefinition> TypeParameters { get; }
        // TODO: 
        public Dictionary<TypeParameterDefinition, TypeParameterDefinition> Unifiers { get; } =
            new Dictionary<TypeParameterDefinition, TypeParameterDefinition>();

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

            TypeParameters = ParameterTypes.SelectMany(t => t.SelfAndDescendants())
                .Select(t => t.Definition).OfType<TypeParameterDefinition>().Distinct().
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
                    if (TypeVarLookup.ContainsKey(ta.Name))
                    {
                        TypeVarLookup[ta.Name].Add(tp);
                    }
                    else
                    {
                        TypeVarLookup.Add(ta.Name, new List<TypeParameterDefinition>() { tp });
                    }
                }

                GatherTypeVariables(ta);
            }
        }

        public TypeInstance ToInstance(TypeExpression expr)
        {
            // TODO: right now I just pick one type at random.
            // However, what if I choose the wrong one? 
            if (TypeVarLookup.ContainsKey(expr.Name))
                return ToInstance(TypeVarLookup[expr.Name].First().ToTypeExpression());
            if (expr.Name == ConceptName)
                return ToInstance(ConcreteType.ToTypeExpression());
            var r = Substitutions.Replace(expr);
            return new TypeInstance(r.Definition, r.TypeArgs.Select(ToInstance));
        }
    }
}