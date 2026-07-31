using System;
using System.Collections.Generic;
using System.Linq;
using Ara3D.Geometry.AST;
using Ara3D.Geometry.Compiler.Utilities;

namespace Ara3D.Geometry.Compiler.Symbols
{
    public class TypeExpression : Expression
    {
        public override string Name => Def?.Name ?? "_untyped_";
        public TypeDef Def { get; }
        public IReadOnlyList<TypeExpression> TypeArgs { get; }
            
        public TypeExpression(TypeDef def, params TypeExpression[] args)
        {
            if (def == null)
                throw new ArgumentNullException(nameof(def));
            Def = def;
            TypeArgs = args;
        }

        public override string ToString()
        {
            return TypeArgs.Count > 0 
                ? $"{Name}<{string.Join(",", TypeArgs)}>" 
                : Name;
        }

        public static TypeExpression CreateTypeVar(Scope scope, string name)
            => name.StartsWith("$")
                ? new TypeExpression(new TypeVariable(scope, name))
                : throw new Exception("Type variable names must start with $ character");

        public override IEnumerable<Symbol> GetChildSymbols()
            => TypeArgs;
            
        public override int GetHashCode()
            => Hasher.Combine(TypeArgs.Select(ta => ta.GetHashCode()).Append(Def.GetHashCode()));

        public override bool Equals(object obj)
            => obj is TypeExpression tes
                   && tes.Def.Name == Def.Name
                   && tes.Def.Id == Def.Id
                   && tes.TypeArgs.SequenceEqual(TypeArgs);

        public bool IsSelfType()
            => Name == "Self";

        public bool UsesSelfType()
            => IsSelfType() || TypeArgs.Any(ta => ta.UsesSelfType());

        public TypeExpression GetReplacement(TypeExpression te)
        {
            for (var i=0; i < Def.TypeParameters.Count; i++)
            {
                if (Def.TypeParameters[i].Name == te.Name)
                    return TypeArgs[i];
            }

            return te;
        }

        public override Symbol Rewrite(Func<Symbol, Symbol> f)
            => f(this);

        public bool IsTypeVariable
            => Def.Kind == TypeKind.TypeVariable;

        public bool IsTypeParameter
            => Def.Kind == TypeKind.TypeParameter;
    }
}