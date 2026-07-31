using System.Collections.Generic;
using System.Linq;
using Ara3D.Geometry.Compiler.Symbols;

namespace Ara3D.Geometry.CSharpWriter
{
    public class ParameterReplacements
    {
        public readonly ParameterReplacements Previous;
        public readonly ParameterDef Parameter;
        public readonly TypedExpression Expression;

        public ParameterReplacements(ParameterDef pd, TypedExpression expr, ParameterReplacements previous)
        {
            Previous = previous;
            Parameter = pd;
            Expression = expr;
        }

        public ParameterReplacements Add(ParameterDef pd, TypedExpression expr)
            => new ParameterReplacements(pd, expr, this);

        public ParameterReplacements AddRange(IEnumerable<ParameterDef> pds, IEnumerable<TypedExpression> exprs)
            => pds.Zip(exprs, (x, y) => (pd: x, expr: y))
                .Aggregate(this,
                    (current, pair) => current.Add(pair.pd, pair.expr));

        public TypedExpression Replace(ParameterDef pd)
        {
            if (pd.Equals(Parameter))
                return Expression;
            if (Previous != null)   
                return Previous.Replace(pd);
            return null;
        }
    }
}