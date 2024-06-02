﻿using System;
using System.Collections.Generic;
using System.Linq;
using Plato.AST;
using Plato.Compiler.Utilities;

namespace Plato.Compiler.Symbols
{
    public class TypeExpression : Symbol
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
            var kind = Def?.Kind.ToString() ?? "";
            if (TypeArgs.Count > 0)
                return kind + ":" + Name + $"<{string.Join(",", TypeArgs)}>";
            return kind + ":" + Name;
        }

        public static TypeExpression CreateTypeVar(string name)
            => name.StartsWith("$")
                ? new TypeExpression(new TypeDef(TypeKind.TypeVariable, name))
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
    }
}