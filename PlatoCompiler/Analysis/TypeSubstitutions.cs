using System;
using System.Collections.Generic;
using Plato.Compiler.Symbols;
using Plato.Compiler.Types;

namespace Plato.Compiler.Analysis
{
    public class TypeSubstitutions
    {
        public string Name { get; }
        public TypeExpression Replacement { get; }
        public TypeSubstitutions Previous { get; }
    
        public TypeSubstitutions Add(TypeExpression expr)
            => Add(expr.TypeArgs, expr.Def);

        public TypeSubstitutions Add(IReadOnlyList<TypeExpression> args, TypeDef def)
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

        public TypeSubstitutions Add(TypeParameterDef parameter, TypeExpression replace)
            => Add(parameter.Name, replace);

        public TypeSubstitutions Add(string name, TypeExpression replace)
            => new TypeSubstitutions(name, replace, this);

        public override string ToString()
        {
            var s = $"{Name}={Replacement};";
            return Previous != null ? s + Previous : s;
        }

        public TypeExpression Replace(TypeExpression expr)
        {
            if (expr.Name == Name)
            {
                var r = Replacement;
                if (r.Def.IsTypeVariable() && Previous != null)
                    return Previous.Replace(r);
                return r;
            }

            if (Previous != null)
                return Previous.Replace(expr);
            return expr;
        }
        
        public TypeSubstitutions(string name, TypeExpression replace, TypeSubstitutions subs = null)
        {
            Name = name;
            Replacement = replace;
            Previous = subs;
        }
    }
}