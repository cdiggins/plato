using System.Linq;
using Ara3D.Geometry.Compiler.Symbols;

namespace Ara3D.Geometry.Compiler.Types
{
    public class TypeParamLookup
    {
        public TypeExpression Expr { get; }
        public string Name { get; }
        public TypeParamLookup Prev { get; }

        public TypeParamLookup(string name, TypeExpression expr, TypeParamLookup prev)
        {
            Name = name;
            Expr = expr;
            Prev = prev;
        }

        public TypeExpression Find(string name)
            => Name == name
                ? Expr
                : Prev?.Find(name);

        public TypeExpression ReplaceVars(TypeExpression expr)
        {
            if (expr.Def is TypeVariable var)
            {
                var r = Find(expr.Def.Name);
                return r ?? expr;
            }

            if (expr.TypeArgs.Count == 0)
                return expr;

            var args = expr.TypeArgs.Select(ReplaceVars).ToArray();
            return new TypeExpression(expr.Def, args);
        }
    }

    public static class TypeParamExtensions
    {
        public static TypeParamLookup ReplaceParameters(this TypeParamLookup self, TypeExpression expr)
        {
            if (expr.TypeArgs.Count == 0)
                return self; ;
            for (var i = 0; i < expr.TypeArgs.Count; ++i)
            {
                var tp = expr.Def.TypeParameters[i];
                self = self.Add(tp.Name, expr.TypeArgs[i]);
            }

            return self;
        }

        public static TypeParamLookup Add(this TypeParamLookup self, string name, TypeExpression expr)
                => new TypeParamLookup(name, expr, self);
    }
}