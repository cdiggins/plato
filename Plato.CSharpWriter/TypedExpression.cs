using System;
using System.Collections.Generic;
using Plato.Compiler.Symbols;
using Plato.Compiler.Types;

namespace Plato.CSharpWriter
{
    public class TypedExpression : Expression
    {
        public IType DeterminedType;
        public IType UsageType;
        public Expression Expression;
        public FunctionGroupCallAnalysis Analysis;

        public TypedExpression(Expression expr, IType determined, IType usage)
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