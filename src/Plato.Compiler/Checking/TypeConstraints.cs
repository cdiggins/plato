using System;
using System.Collections.Generic;
using System.Linq;
using Ara3D.Geometry.AST;
using Ara3D.Geometry.Compiler.Symbols;
using Ara3D.Geometry.Compiler.Types;

namespace Ara3D.Geometry.Compiler.Checking
{
    /// <summary>
    /// THE reading of declared type-parameter bounds (`interface C&lt;T&gt; where T: Additive`,
    /// `type Tween&lt;T&gt; where T: Interpolatable`) for the checker. One place answers three
    /// questions, so the instantiation-site gate and the member-lookup licence can never disagree:
    ///
    ///   * what bounds does parameter <c>i</c> of a CONSTRUCTED type carry, with the construction's
    ///     own arguments substituted in (<c>BoundsLike&lt;Point2D, Vector2D&gt;</c> with
    ///     `where TPoint: Difference&lt;TDelta&gt;` yields <c>Difference&lt;Vector2D&gt;</c>);
    ///   * does a given type argument SATISFY a bound;
    ///   * which bounds does a library function's signature variable inherit from the constructed
    ///     types it mentions (<c>Sample(x: Tween&lt;$T&gt;, ...)</c> gives <c>$T: Interpolatable</c>).
    ///
    /// Satisfaction is interface membership as <see cref="ConceptClosure"/> defines it — the same
    /// transitive, per-level-substituted walk the solver already uses for interface parameters — so a
    /// bound is satisfied by exactly the types the solver would accept where the bound is written
    /// out as a parameter type. Nothing here is record- or sum-specific: a bound on a sum type's
    /// parameter resolves through this identical path.
    ///
    /// VOCABULARY. The clause is a CONSTRAINT (which is what the syntax layer calls it —
    /// <see cref="AstConstraint"/>, <see cref="Symbols.TypeParameterDef.Constraints"/>, LINT002);
    /// the interface it names is a BOUND, which is what this file and the checker call it. One clause
    /// may state several bounds, so the two words are not synonyms and neither name is redundant.
    /// </summary>
    public static class TypeConstraints
    {
        private static readonly IReadOnlyList<TypeExpression> None = new TypeExpression[0];

        /// <summary>
        /// Which declarations' bounds are carried into generated code (plato-382 phase C): the ones
        /// declared on a CONCRETE type — `type Tween&lt;T&gt; where T: Interpolatable`, the construct
        /// this issue added. An interface's own `where` clause is deliberately NOT emitted yet: those
        /// predate bound checking, the shipping vocabulary carries several, and putting them on the
        /// generated interfaces would propagate a constraint to every mention of the interface at
        /// once. Emitting them is a separate, library-wide change.
        ///
        /// One predicate so the two halves cannot disagree: a `where` clause is emitted on a
        /// function's signature exactly when it comes from a declaration whose own C# form carries
        /// the matching clause, and <see cref="TirEmitSource.IsOpenGenericEmittable"/> licenses a
        /// call from exactly the same set.
        /// </summary>
        public static bool EmittedToCSharp(TypeDef d) => d != null && !d.IsInterface();

        /// <summary>The bounds DECLARED on a type-parameter reference (`T` inside the body of the
        /// declaration that introduced it). Empty for a unification variable, a concrete type, or an
        /// unbounded parameter.</summary>
        public static IReadOnlyList<TypeExpression> DeclaredBounds(TypeExpression t)
            => (t?.Def as TypeParameterDef)?.Constraints ?? None;

        /// <summary>The bounds parameter <paramref name="i"/> of <paramref name="constructed"/>
        /// carries, with the construction's parameter-to-argument map applied so a bound naming a
        /// sibling parameter is reported in terms of the ACTUAL argument.</summary>
        public static IReadOnlyList<TypeExpression> BoundsAt(TypeExpression constructed, int i)
        {
            var tps = constructed?.Def?.TypeParameters;
            if (tps == null || i >= tps.Count)
                return None;
            var bounds = tps[i].Constraints;
            if (bounds.Count == 0)
                return None;
            var map = ArgumentMap(constructed);
            return bounds.Select(b => Substitute(b, map)).ToList();
        }

        /// <summary>Whether <paramref name="arg"/> satisfies <paramref name="bound"/>.
        ///
        /// A concrete type satisfies a bound when its interface closure carries the bound's interface.
        /// A type PARAMETER or unification variable satisfies it when one of the bounds IT carries
        /// implies the required one — and, when it carries no bounds at all, permissively: Plato
        /// does not require bounds, so an unbounded parameter is "not yet known to fail", exactly as
        /// the solver has always treated it. <c>Self</c> satisfies anything (the reifier decides
        /// what Self is), and a non-interface bound is not this function's error to report (CHK310).
        /// </summary>
        public static bool Satisfies(TypeExpression arg, TypeExpression bound,
            IReadOnlyList<TypeExpression> extraArgBounds = null)
        {
            if (arg?.Def == null || bound?.Def == null || !bound.Def.IsInterface())
                return true;
            if (arg.Def.Kind == TypeKind.SelfType)
                return true;

            if (arg.Def.Kind == TypeKind.TypeParameter || arg.Def.Kind == TypeKind.TypeVariable)
            {
                var carried = KnownBounds(arg, extraArgBounds);
                return carried.Count == 0 || carried.Any(b => Implies(b, bound));
            }

            var instance = ConceptClosure.FindInstance(arg, bound.Def.Name);
            return instance != null && ArgumentsAgree(instance, bound);
        }

        /// <summary>Whether the instance a type carries agrees with the bound's own type arguments.
        /// Compared only where BOTH sides are ground: `Difference&lt;Vector2D&gt;` does not satisfy
        /// `Difference&lt;Duration&gt;`, but anything still holding a variable is left to the solver
        /// rather than pre-judged here.</summary>
        private static bool ArgumentsAgree(TypeExpression instance, TypeExpression bound)
        {
            if (instance.TypeArgs.Count != bound.TypeArgs.Count)
                return true; // arity is not this check's business (CHK102 territory)
            for (var i = 0; i < instance.TypeArgs.Count; i++)
            {
                var a = instance.TypeArgs[i];
                var b = bound.TypeArgs[i];
                if (IsGround(a) && IsGround(b) && a.ToString() != b.ToString())
                    return false;
            }
            return true;
        }

        /// <summary>No unification variable, type parameter or Self anywhere in the type.</summary>
        private static bool IsGround(TypeExpression t)
            => t?.Def != null
               && t.Def.Kind != TypeKind.TypeVariable
               && t.Def.Kind != TypeKind.TypeParameter
               && t.Def.Kind != TypeKind.SelfType
               && t.TypeArgs.All(IsGround);

        /// <summary>Every bound a parameter/variable is known to carry, at a site that also holds a
        /// signature's inherited bounds keyed by variable name (the shape
        /// <see cref="InheritedBounds"/> returns). THE reading used by the solver's licence, the
        /// construction-site check and the emit-time licence alike.</summary>
        public static IReadOnlyList<TypeExpression> KnownBounds(TypeExpression t,
            IReadOnlyDictionary<string, IReadOnlyList<TypeExpression>> inherited)
            => KnownBounds(t, Inherited(t, inherited));

        /// <summary>What a signature's inherited bounds say about one variable, or null.</summary>
        public static IReadOnlyList<TypeExpression> Inherited(TypeExpression t,
            IReadOnlyDictionary<string, IReadOnlyList<TypeExpression>> inherited)
            => t?.Name != null && inherited != null && inherited.TryGetValue(t.Name, out var b) ? b : null;

        /// <summary>Every bound a parameter/variable is known to carry: the ones declared on its own
        /// declaration, plus any inherited through a signature (see <see cref="InheritedBounds"/>).</summary>
        public static IReadOnlyList<TypeExpression> KnownBounds(TypeExpression t,
            IReadOnlyList<TypeExpression> extra = null)
        {
            var declared = DeclaredBounds(t);
            if (extra == null || extra.Count == 0)
                return declared;
            if (declared.Count == 0)
                return extra;
            return declared.Concat(extra).ToList();
        }

        /// <summary>Whether holding <paramref name="held"/> is enough to satisfy
        /// <paramref name="required"/> — i.e. the required interface is in the held bound's own
        /// closure (`Difference&lt;TDelta&gt;` implies `Value` when Difference inherits it).</summary>
        public static bool Implies(TypeExpression held, TypeExpression required)
        {
            if (held?.Def == null || required?.Def == null)
                return false;
            var instance = ConceptClosure.FindInstance(held, required.Def.Name);
            return instance != null && ArgumentsAgree(instance, required);
        }

        /// <summary>
        /// Every bound a function's signature type variables carry, from BOTH sources: the ones
        /// INHERITED from the constructed types its signature mentions, and the ones the function
        /// DECLARES itself in a `where` clause (plato-393). `Sample(x: Tween&lt;$T&gt;, t: Duration): $T` sees
        /// `Tween&lt;T&gt; where T: Interpolatable` and so learns `$T: Interpolatable` — which is what
        /// licenses a `Lerp` on a bare `$T` in the body, and what a later emission phase needs in
        /// order to put the `where` clause on the generated C# method.
        ///
        /// Keyed by variable NAME, matching the solver's substitution. Deliberately covers the
        /// declared signature only (parameters and return type): a bound must be visible in the
        /// signature to be honest about what a caller has to supply.
        ///
        /// <paramref name="source"/> restricts WHICH declarations' bounds are collected; null (the
        /// checker's reading) collects them all. See <see cref="EmittedToCSharp"/> for the emitter's.
        /// </summary>
        public static IReadOnlyDictionary<string, IReadOnlyList<TypeExpression>> InheritedBounds(
            FunctionDef f, Func<TypeDef, bool> source = null)
        {
            var result = new Dictionary<string, List<TypeExpression>>();

            // A variable mentioned twice in one signature (`(x: Gauge<$T>, y: Gauge<$T>)`) inherits
            // the same bound twice, and a declared bound may restate an inherited one; the SET is
            // what every consumer reads, so identical spellings collapse.
            void Add(string name, TypeExpression bound)
            {
                if (!result.TryGetValue(name, out var list))
                    result[name] = list = new List<TypeExpression>();
                if (!list.Any(x => x.ToString() == bound.ToString()))
                    list.Add(bound);
            }

            void Walk(TypeExpression t, int depth)
            {
                if (t?.Def == null || depth > 8)
                    return;
                var collect = source == null || source(t.Def);
                for (var i = 0; i < t.TypeArgs.Count; i++)
                {
                    var arg = t.TypeArgs[i];
                    if (collect && arg?.Def != null
                        && (arg.Def.Kind == TypeKind.TypeVariable || arg.Def.Kind == TypeKind.TypeParameter))
                    {
                        foreach (var b in BoundsAt(t, i))
                            Add(arg.Name, b);
                    }
                    Walk(arg, depth + 1);
                }
            }

            foreach (var t in (f?.Parameters ?? Enumerable.Empty<ParameterDef>()).Select(p => p.Type).Append(f?.ReturnType))
                Walk(t, 0);

            // Bounds the function DECLARES on its own variables (plato-393) join the inherited ones
            // as a second source of the same shape. They are collected whatever `source` says: a
            // declared bound's "declaration" is this very function, whose emitted C# always carries
            // the matching `where` clause, so it licenses a body under both readings.
            foreach (var d in f?.DeclaredBounds ?? Enumerable.Empty<DeclaredFunctionBound>())
                if (d?.Bound != null)
                    Add(d.VariableName, d.Bound);

            return result.ToDictionary(kv => kv.Key, kv => (IReadOnlyList<TypeExpression>)kv.Value);
        }

        // --- substitution --------------------------------------------------------

        /// <summary>A constructed type's declared parameters mapped to its actual arguments.</summary>
        public static IReadOnlyDictionary<string, TypeExpression> ArgumentMap(TypeExpression t)
        {
            var map = new Dictionary<string, TypeExpression>();
            var tps = t?.Def?.TypeParameters;
            if (tps == null)
                return map;
            for (var i = 0; i < tps.Count && i < t.TypeArgs.Count; i++)
                map[tps[i].Name] = t.TypeArgs[i];
            return map;
        }

        private static TypeExpression Substitute(TypeExpression t, IReadOnlyDictionary<string, TypeExpression> map)
        {
            if (t == null)
                return null;
            if (map.TryGetValue(t.Name, out var replacement))
                return replacement;
            return t.TypeArgs.Count == 0
                ? t
                : new TypeExpression(t.Def, t.TypeArgs.Select(a => Substitute(a, map)).ToArray());
        }
    }
}
