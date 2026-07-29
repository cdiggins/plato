using System.Linq;
using Ara3D.Geometry.Compiler.Symbols;

namespace Ara3D.Geometry.Compiler.Checking
{
    /// <summary>
    /// Recognizes a qualified reference to a NULLARY case of a sum type — <c>Axis3D.Y</c>,
    /// <c>SignedAxis3D.NegX</c>, <c>RotationOrder.ZXY</c> (wave-2, plato-232).
    ///
    /// Member access desugars receiver-first, so <c>Axis3D.Y</c> normalizes to the argument-list-less
    /// call <c>Y(Axis3D)</c>, and ordinary resolution mishandles it in one of two ways: it binds some
    /// unrelated member of the same name (<c>Y</c> exists, so <c>Axis3D.Y</c> types as
    /// <c>Number</c> — CHK101), or it finds nothing at all (<c>NegX</c> — CHK201). A last-resort
    /// fallback therefore does not suffice; the rule has to be tried BEFORE ordinary member
    /// resolution.
    ///
    /// What keeps that safe is the gating: the receiver must NAME a type (a syntactic type
    /// reference) rather than be a value of one, and the member must name a nullary case of that
    /// type. No member or library function reachable on a VALUE is ever shadowed. This is also why
    /// the rule lives in the constrain/elaborate passes rather than the solver — by the time the
    /// solver sees the argument it is just the type expression <c>Axis3D</c>, indistinguishable
    /// from a value of it.
    /// </summary>
    public static class SumCaseAccess
    {
        /// <summary>The sum type a call of the form <c>Sum.Case</c> denotes; null if the call is not
        /// that shape. A case WITH fields is excluded: it is constructed by calling it
        /// (<c>PathSegment2D.Line(p)</c>), which is an ordinary call and resolves the usual way.</summary>
        public static TypeDef Resolve(FunctionCall fc, string memberName)
        {
            if (fc == null || fc.HasArgList || fc.Args.Count != 1)
                return null;
            var td = ReceiverTypeName(fc.Args[0]);
            if (td == null || !td.IsSum)
                return null;
            var c = td.Cases.FirstOrDefault(x => x.Name == memberName);
            return c != null && c.Fields.Count == 0 ? td : null;
        }

        /// <summary>The type a receiver expression NAMES (as opposed to the type it HAS): a bare type
        /// reference, or the type expression the binder substitutes when the name is a type AND a
        /// function group (<c>Axis3D</c> is both a type and a <c>SignedAxes</c> member).</summary>
        private static TypeDef ReceiverTypeName(Expression e)
            => (e as TypeRefSymbol)?.Def ?? (e as TypeExpression)?.Def;
    }
}
