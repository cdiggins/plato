using System;
using System.Collections.Generic;
using Ara3D.Geometry.Compiler.Symbols;
using Ara3D.Geometry.Compiler.Types;

namespace Ara3D.Geometry.CSharpWriter
{
    public class TypedExpression : Expression
    {
        public TypeExpression DeterminedType;
        public TypeExpression UsageType;
        public Expression Expression;
        public FunctionGroupCallAnalysis Analysis;

        public TypedExpression(Expression expr, TypeExpression determined, TypeExpression usage)
        {
            DeterminedType = determined;
            UsageType = usage;
            Expression = expr;
        }

        public override IEnumerable<Symbol> GetChildSymbols()
        {
            yield return Expression;
        }

        public override string Name => $"TypedExpression:{Expression.Name}";

        public override string ToString()
        {
            var fid = Analysis?.ViableFunctions.Count;
            return $"{Expression}#{fid}";// :{DeterminedType}:{UsageType}";
        }

        public override Symbol Rewrite(Func<Symbol, Symbol> f)
            => throw new NotImplementedException();

        public TypedExpression With(FunctionGroupCallAnalysis fcr)
        {
            Analysis = fcr;
            return this;
        }
    }
}