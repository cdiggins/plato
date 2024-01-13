using System;
using System.Collections.Generic;
using Plato.Compiler.Symbols;

namespace Plato.CSharpWriter
{
    public class TypeSubstitutions
    {
        public TypeParameterDefinition Parameter { get; }
        public TypeExpression Replacement { get; }
        public TypeSubstitutions Previous { get; }
    
        public TypeSubstitutions Add(TypeExpression expr)
            => Add(expr.TypeArgs, expr.Definition);

        public TypeSubstitutions Add(IReadOnlyList<TypeExpression> args, TypeDefinition def)
        {
            if (args.Count != def.TypeParameters.Count)
                throw new Exception($"Number of type arguments does not match number of type parameters");
            var r = this;
            for (var i = 0; i < args.Count; i++)
            {
                var tp = def.TypeParameters[i];
                var arg = args[i];
                r = r.Add(tp, arg);
            }
            return r; 
        }

        public TypeSubstitutions Add(TypeParameterDefinition parameter, TypeExpression replace)
            => new TypeSubstitutions(parameter, replace, this);

        public static TypeSubstitutions Create(TypeExpression expr)
            => new TypeSubstitutions().Add(expr);

        public TypeSubstitutions(TypeParameterDefinition parameter = null, TypeExpression replace = null, TypeSubstitutions subs = null)
        {
            Parameter = parameter;
            Replacement = replace;
            Previous = subs;
        }
    }
}