using System.Collections.Generic;
using System.Linq;
using Ara3D.Geometry.Compiler.Symbols;
using Ara3D.Geometry.Compiler.Types;
using Ara3D.Utils;

namespace Ara3D.Geometry.CSharpWriter
{
    /// <summary>
    /// THE rendering of a DECLARED type-parameter bound (`type Tween&lt;T&gt; where T: Interpolatable`,
    /// plato-382) as a C# `where` clause. One place answers it for both surfaces that need it —
    /// the generated struct's own parameters (<see cref="CSharpConcreteTypeWriter"/>) and a generic
    /// library function's signature variables (<see cref="CSharpFunctionInfo"/>) — so the clause a
    /// caller must discharge and the clause a callee declares are spelled identically.
    ///
    /// The shape is the F-bounded one the writer already produces for function type variables
    /// (<see cref="CSharpFunctionInfo.ConstraintString"/>): an interface emits as
    /// `interface C&lt;Self, ...&gt; where Self : C&lt;Self, ...&gt;`, so a bound `T: C&lt;A&gt;`
    /// reads `where T : C&lt;T, A&gt;` — the bounded parameter itself occupies Self.
    /// </summary>
    public static class CSharpBoundWriter
    {
        /// <summary>The full `where` clause list for a declaration's own type parameters, or ""
        /// when none of them is bounded. Rendered in declaration order, one clause per parameter,
        /// each carrying every bound that parameter declares.</summary>
        public static string WhereClauses(IReadOnlyList<TypeParameterDef> typeParameters,
            ITypeToCSharp typeToCSharp)
            => typeParameters == null
                ? ""
                : typeParameters
                    .Select(tp => WhereClause(tp.Name, tp.Constraints, typeToCSharp))
                    .JoinStrings("");

        /// <summary>The `where` clause for one parameter given the bounds it carries, or "" when it
        /// carries none that render (a bound that does not name an interface is CHK310's error to
        /// report, not something to emit as invalid C#).</summary>
        public static string WhereClause(string parameterName, IReadOnlyList<TypeExpression> bounds,
            ITypeToCSharp typeToCSharp)
        {
            if (parameterName == null || bounds == null || bounds.Count == 0)
                return "";
            var rendered = bounds
                .Select(b => BoundTypeName(parameterName, b, typeToCSharp))
                .Where(s => s != null)
                .Distinct()
                .ToList();
            return rendered.Count == 0
                ? ""
                : $" where {parameterName} : {rendered.JoinStringsWithComma()}";
        }

        /// <summary>The C# interface a bound denotes, with the bounded parameter substituted into
        /// the interface's implicit Self slot. Null when the bound does not name an interface.</summary>
        private static string BoundTypeName(string parameterName, TypeExpression bound,
            ITypeToCSharp typeToCSharp)
        {
            if (bound?.Def == null || !bound.Def.IsInterface())
                return null;
            var args = bound.TypeArgs.Select(a => typeToCSharp.ToCSharpType(a)).ToList();
            if (bound.Def.IsSelfConstrained())
                args.Insert(0, parameterName);
            return args.Count > 0 ? $"{bound.Def.Name}<{args.JoinStringsWithComma()}>" : bound.Def.Name;
        }
    }
}
