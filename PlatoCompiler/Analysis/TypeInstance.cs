using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Ara3D.Utils;
using Plato.Compiler.Symbols;

namespace Plato.Compiler.Analysis
{
    public class TypeInstance
    {
        public TypeDefinition Definition { get; }

        public TypeInstance(TypeDefinition definition, IEnumerable<TypeInstance> args)
        {
            Debug.Assert(!(definition is TypeVariable));
            Definition = definition;
            Args = args?.ToList() ?? new List<TypeInstance>();
        }

        public IReadOnlyList<TypeInstance> Args { get; }

        public override string ToString()
        {
            var r = Definition.Name;
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
    }
}