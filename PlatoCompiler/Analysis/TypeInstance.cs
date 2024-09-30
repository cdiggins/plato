using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Ara3D.Utils;
using Plato.Compiler.Symbols;

namespace Plato.Compiler.Analysis
{
    // TODO: it's been a while, and I'm really struggling to understand how this is different from a TypeExpression. 
    // TODO: this has TypeSubstitutions as well. We need to know what they are. 
    // This way when we query functions, inherited types, and implemented types
    // We can figure it all out. 
    public class TypeInstance
    {
        public string Name => Def.Name;
        public TypeDef Def { get; }

        public TypeInstance(TypeDef def, IEnumerable<TypeInstance> args)
        {
            Debug.Assert(!(def is TypeVariable));
            Def = def;
            Args = args?.ToList() ?? new List<TypeInstance>();
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

        public static TypeInstance Create(TypeDef def)
            => new TypeInstance(def, def.TypeParameters.Select(Create));

        public static TypeInstance Create(TypeExpression expr)
            => new TypeInstance(expr.Def, expr.TypeArgs.Select(Create));
    }
}