using Ara3D.Geometry.AST;
using Ara3D.Geometry.Compiler.Symbols;

namespace Ara3D.Geometry.Compiler.Checking
{
    /// <summary>
    /// Boolean <c>a &amp;&amp; b</c> and <c>a || b</c> in the TIR (docs/SEMANTICS.md section 6). The
    /// elaborator builds them as conditionals, <c>a ? b : false</c> and <c>a ? true : b</c>, so every
    /// backend evaluates the right operand only when the left does not decide the result. A writer
    /// whose target language has short-circuit operators on its native boolean recognizes the same
    /// shapes with <see cref="TryMatch"/> and emits <c>&amp;&amp;</c> or <c>||</c>.
    /// </summary>
    public static class TirShortCircuit
    {
        /// <summary><c>left &amp;&amp; right</c> when <paramref name="isAnd"/>, else <c>left || right</c>.</summary>
        public static TirConditional Build(bool isAnd, TirNode left, TirNode right, TypeExpression boolType, Symbol origin)
        {
            var constant = new TirLiteral(!isAnd, LiteralTypesEnum.Boolean, boolType, origin);
            return isAnd
                ? new TirConditional(left, right, constant, boolType, origin)
                : new TirConditional(left, constant, right, boolType, origin);
        }

        /// <summary>The operator, <c>&amp;&amp;</c> or <c>||</c>, and its operands when
        /// <paramref name="cond"/> has the shape <see cref="Build"/> makes; null otherwise.</summary>
        public static string TryMatch(TirConditional cond, out TirNode left, out TirNode right)
        {
            left = cond.Condition;
            right = null;
            if (IsLiteral(cond.IfFalse, false))
            {
                right = cond.IfTrue;
                return "&&";
            }
            if (IsLiteral(cond.IfTrue, true))
            {
                right = cond.IfFalse;
                return "||";
            }
            return null;
        }

        private static bool IsLiteral(TirNode node, bool value)
            => node is TirLiteral lit && lit.LiteralType == LiteralTypesEnum.Boolean && lit.Value is bool b && b == value;
    }
}
