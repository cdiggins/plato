using System.Collections.Generic;
using System.Linq;
using Ara3D.Geometry.Compiler.Symbols;

namespace Ara3D.Geometry.Compiler.Checking
{
    /// <summary>
    /// Enforces "sealed by F, G" (TKT-444): only the named functions may construct a sealed type.
    /// Plato has no visibility, so this is how a proof type (a checked mesh, a passed check) says
    /// which functions can build it. A function may construct a sealed type when its name is in the
    /// type's list and it returns the type; every overload of that name returning the type counts.
    ///
    /// It runs per function, after the solver, over the decisions the solver committed, so it sees
    /// exactly what the elaborator will emit:
    ///   * a call to the type's constructor (its fields, or one tuple of them) or to one of its
    ///     sum cases;
    ///   * a tuple converted to the type, as an argument or at a return;
    ///   * <c>new</c> or <c>default</c> of the type.
    /// Field reads, <c>match</c>, and calls to the named functions stay allowed everywhere.
    ///
    /// Diagnostics:
    ///   CHK311 a sealed type is constructed outside the functions its declaration names
    /// </summary>
    public static class SealedConstruction
    {
        public const string Code = "CHK311";

        /// <summary>Whether <paramref name="f"/> is one of the functions that may construct
        /// <paramref name="type"/>.</summary>
        public static bool Permits(TypeDef type, FunctionDef f)
            => type.SealedBy.Contains(f.Name) && f.ReturnType?.Def == type;

        /// <summary>The constructions of a sealed type in one checked function that its declaration
        /// does not permit.</summary>
        public static IEnumerable<CheckDiagnostic> Check(FunctionDef f, ConstraintSystem system, Solver solver)
        {
            TypeDef SealedAgainst(TypeExpression t)
            {
                var def = t == null ? null : solver.Zonk(t)?.Def;
                return def != null && def.IsSealed && !Permits(def, f) ? def : null;
            }

            bool IsTuple(TypeExpression t)
                => t != null && solver.Zonk(t)?.Def?.Name?.StartsWith("Tuple") == true;

            CheckDiagnostic Error(TypeDef type, string how, Symbol origin)
                => new CheckDiagnostic(DiagnosticSeverity.Error, Code,
                    $"'{type.Name}' is sealed by {string.Join(", ", type.SealedBy)}; '{f.Name}' may not construct it ({how})",
                    origin);

            foreach (var rc in solver.ResolvedCalls.Values)
            {
                var callee = rc.Callee;
                if ((callee.FunctionType == FunctionType.Constructor || callee.FunctionType == FunctionType.SumFactory)
                    && SealedAgainst(rc.ReturnType ?? callee.ReturnType) is TypeDef built)
                    yield return Error(built,
                        callee.FunctionType == FunctionType.SumFactory ? $"case {callee.Name}" : "constructor call",
                        rc.Call);

                for (var i = 0; i < rc.ArgKinds.Count && i < rc.Call.Args.Count; i++)
                    if (rc.ArgKinds[i] == ArgMatchKind.Conversion
                        && system.ExprTypes.TryGetValue(rc.Call.Args[i], out var argType) && IsTuple(argType)
                        && SealedAgainst(rc.ParameterTypes[i]) is TypeDef converted)
                        yield return Error(converted, "tuple conversion", rc.Call.Args[i]);
            }

            foreach (var cc in system.Coercions)
                if (IsTuple(cc.From) && SealedAgainst(cc.To) is TypeDef converted)
                    yield return Error(converted, "tuple conversion", cc.Origin);

            foreach (var kv in system.ExprTypes)
            {
                var fc = kv.Key as FunctionCall;
                var how = kv.Key is NewExpression ? "new"
                    : fc?.Function is KeywordRefSymbol k && k.Keyword == "default" ? "default"
                    // A nullary case (`Verdict.Pass`) is typed by the constrain pass, not resolved as a call.
                    : fc?.Function is FunctionGroupRefSymbol g && SumCaseAccess.Resolve(fc, g.Name) != null ? $"case {g.Name}"
                    : null;
                if (how != null && SealedAgainst(kv.Value) is TypeDef made)
                    yield return Error(made, how, kv.Key);
            }
        }
    }
}
