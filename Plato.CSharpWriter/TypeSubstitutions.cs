using System;
using System.Collections.Generic;
using Plato.Compiler.Symbols;

namespace Plato.CSharpWriter
{
    public class TypeSubstitutions
    {
        public TypeDefinition Self { get; }
        public TypeExpression Type { get; }

        public Dictionary<TypeParameterDefinition, TypeExpression> Lookup { get; } 

        public void AddSubstitutions(TypeExpression expr)
            => AddSubstitutions(expr.TypeArgs, expr.Definition);

        public void AddSubstitutions(IReadOnlyList<TypeExpression> args, TypeDefinition def)
        {
            if (args.Count != def.TypeParameters.Count)
                throw new Exception($"Number of type arguments does not match number of type parameters");
            for (var i = 0; i < args.Count; i++)
            {
                var tp = def.TypeParameters[i];
                var arg = args[i];
                if (!Lookup.ContainsKey(tp))
                    Lookup.Add(tp, arg);
                AddSubstitutions(arg);
            }
        }

        public TypeSubstitutions(TypeDefinition self, TypeExpression typeExpression, Dictionary<TypeParameterDefinition, TypeExpression> lookup = null)
        {
            Lookup = lookup ?? new Dictionary<TypeParameterDefinition, TypeExpression>();
            Self = self;
            Type = typeExpression;
            AddSubstitutions(typeExpression);
        }
    }
}