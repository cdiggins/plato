using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using Ara3D.Utils;
using Plato.Compiler.Symbols;

namespace Plato.Compiler.Analysis
{
    // TODO:
    // 1) I think this needs to take into account substitutions.
    // 2) It may need the context of the ConcreteType within which it is being used. 
    // 3) What about the context of the declaring function (if present) and its substitutions. 
    // 
    public class TypeInstance
    {
        public string Name => Def.Name;
        public TypeExpression Expr { get; }
        public TypeDef Def => Expr.Def;

        public TypeInstance(TypeExpression expr, IEnumerable<TypeInstance> args)
        {
            Expr = expr;
            var tmp = args?.ToList() ?? new List<TypeInstance>();
            if (Def.IsSelfConstrained())
            {
                var self = Create(Def.Self);
                tmp.Insert(0, self);
            }
            Args = tmp;
        }

        public IReadOnlyList<TypeInstance> Args { get; }

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
    }
}