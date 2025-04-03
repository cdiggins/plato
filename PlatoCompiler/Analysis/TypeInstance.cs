using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using Ara3D.Utils;
using Plato.Compiler.Symbols;
using Plato.Compiler.Types;

namespace Plato.Compiler.Analysis
{
    public class TypeInstance
    {
        public string Name => Def.Name;
        public TypeExpression Expr { get; }
        public TypeDef Def => Expr.Def;
        public bool IsSelfConstrained => Def.IsSelfConstrained();

        public TypeInstance(TypeExpression expr, IEnumerable<TypeInstance> args)
        {
            Expr = expr;
            Args = args?.ToList() ?? new List<TypeInstance>();
        }

        public IReadOnlyList<TypeInstance> Args { get; }

        public IEnumerable<TypeInstance> ArgsWithSelf
            => IsSelfConstrained ? Args.Prepend(Self) : Args;

        public override string ToString()
        {
            var r = Def.Name;
            if (Args.Count == 0) return r;
            return $"{r}<{Args.JoinStringsWithComma()}>";
        }

        public IEnumerable<TypeInstance> SelfAndDescendants()
        {
            yield return this;
            foreach (var arg in Args)
            foreach (var arg2 in arg.SelfAndDescendants())
                yield return arg2;
        }

        public static TypeInstance Create(TypeExpression expr)
            => new TypeInstance(expr, expr.TypeArgs.Select(Create));

        public static TypeInstance Create(TypeDef def)
            => Create(def.ToTypeExpression());

        public static TypeInstance Self = 
            SelfType.Instance.ToTypeInstance();
    }

    public static class TypeInstanceExtensions
    {
        public static TypeInstance ToTypeInstance(this TypeDef self)
            => TypeInstance.Create(self);

        public static bool ContainsRawTypeVariable(this TypeInstance self)
            => self.ToString().Contains("$");
    }
}