// Unifies type expressions and resolves substitution chains with occurs and depth checks.
using System.Collections.Generic;
using System.Linq;
using Ara3D.Geometry.AST;
using Ara3D.Geometry.Compiler.Symbols;
using Ara3D.Geometry.Compiler.Types;

namespace Ara3D.Geometry.Compiler.Checking
{
    public partial class Solver
    {
        // --- unification ---------------------------------------------------------

        private static bool IsVar(TypeExpression t)
            => t.Def != null && t.Def.Kind == TypeKind.TypeVariable;

        public bool Unify(TypeExpression a, TypeExpression b, Dictionary<string, TypeExpression> sub, Symbol origin, bool record, int depth = 0)
        {
            if (a == null || b == null || depth > MaxDepth)
                return true;

            a = ResolveTop(a, sub);
            b = ResolveTop(b, sub);

            if (IsVar(a) && IsVar(b) && a.Name == b.Name)
                return true;

            // A signature type variable ($T from the generic function under check) is RIGID during
            // trial / best-effort matching (record == false): it is universally quantified in the
            // body and must never absorb a concrete type there. Otherwise a call whose argument is
            // that bare $T (`value.Between(x.Min, x.Max)` with value: $T) commits to the scalar
            // Between(Number,…) overload by binding $T := Number, which then poisons the whole body's
            // view of $T — the emitted C# casts a Point2D to (float). Left free, the call binds the
            // interface overload instead and monomorphization grounds $T to the concrete type.
            // Rigidity is lifted for record == true (Phase-1 equalities and the committed
            // result-unify), and flexible variables still bind toward a rigid one.
            // Two variables, one of them the signature's own: the flexible one binds toward the
            // signature variable, whichever side it is on, so the body's view of `$T` stays `$T`.
            // Recorded the other way round (`$T := $Ret3`), every type mentioning `$T` zonked to
            // a hole, the diagnostics printed the hole, and the calls over it never grounded.
            if (IsVar(a) && IsVar(b) && IsRigid(a) && !IsRigid(b))
                return Bind(b, a, sub, origin, record);

            var aRigid = !record && IsRigid(a);
            var bRigid = !record && IsRigid(b);
            if (IsVar(a) && !aRigid)
                return Bind(a, b, sub, origin, record);
            if (IsVar(b) && !bRigid)
                return Bind(b, a, sub, origin, record);
            if (IsVar(a) || IsVar(b))
            {
                // A rigid variable remains on at least one side. It is still compatible with Self
                // (which the reifier substitutes), but not with a concrete type at trial time.
                if (a.Def?.Kind == TypeKind.SelfType || b.Def?.Kind == TypeKind.SelfType)
                    return true;
                return false;
            }

            // `Self` is compatible with anything: it is a placeholder the reifier replaces with
            // the concrete type a function is stamped onto, so at generic-check time both
            // `Self ~ Vector3` and `Self ~ IArrayLike<T>` are satisfiable-by-substitution. No
            // binding: Self stays Self in the solved view and monomorphization grounds it.
            if (a.Def?.Kind == TypeKind.SelfType || b.Def?.Kind == TypeKind.SelfType)
                return true;

            // Both are named (or rigid type parameters): match nominally.
            if (a.Def?.Name != b.Def?.Name)
            {
                if (record)
                    Report(DiagnosticSeverity.Error, "CHK101", $"Cannot unify '{a}' with '{b}'", origin);
                return false;
            }
            if (a.TypeArgs.Count != b.TypeArgs.Count)
            {
                if (record)
                    Report(DiagnosticSeverity.Error, "CHK102", $"Type argument count mismatch between '{a}' and '{b}'", origin);
                return false;
            }

            var ok = true;
            for (var i = 0; i < a.TypeArgs.Count; i++)
                ok &= Unify(a.TypeArgs[i], b.TypeArgs[i], sub, origin, record, depth + 1);
            return ok;
        }

        private bool Bind(TypeExpression var, TypeExpression t, Dictionary<string, TypeExpression> sub, Symbol origin, bool record)
        {
            if (IsVar(t) && t.Name == var.Name)
                return true;
            if (Occurs(var.Name, t, sub))
            {
                if (record)
                    Report(DiagnosticSeverity.Error, "CHK103", $"Recursive type: '{var.Name}' occurs in '{Zonk(t, sub)}'", origin);
                return false;
            }
            sub[var.Name] = t;
            return true;
        }

        private bool Occurs(string name, TypeExpression t, Dictionary<string, TypeExpression> sub, int depth = 0)
        {
            if (t == null || depth > MaxDepth) return false;
            t = ResolveTop(t, sub);
            if (IsVar(t))
                return t.Name == name;
            return t.TypeArgs.Any(a => Occurs(name, a, sub, depth + 1));
        }

        private static TypeExpression ResolveTop(TypeExpression t, Dictionary<string, TypeExpression> sub)
        {
            var depth = 0;
            while (t != null && IsVar(t) && sub.TryGetValue(t.Name, out var u) && depth++ < MaxDepth)
                t = u;
            return t;
        }

        /// <summary>Fully apply the substitution (deep), yielding a variable-free type where solved.</summary>
        public TypeExpression Zonk(TypeExpression t, Dictionary<string, TypeExpression> sub = null, int depth = 0)
        {
            sub ??= Substitution;
            if (t == null || depth > MaxDepth) return t;
            t = ResolveTop(t, sub);
            if (t == null || t.TypeArgs.Count == 0)
                return t;
            return new TypeExpression(t.Def, t.TypeArgs.Select(a => Zonk(a, sub, depth + 1)).ToArray());
        }

        private static bool HasVar(TypeExpression t)
            => t != null && (IsVar(t) || t.TypeArgs.Any(HasVar));

        /// <summary>Like <see cref="HasVar"/>, but a rigid signature variable counts as ground.</summary>
        private bool HasFlexibleVar(TypeExpression t)
            => t != null && ((IsVar(t) && !IsRigid(t)) || t.TypeArgs.Any(HasFlexibleVar));

    }
}
