// Resolves implicit coercions, tuple construction conversions, and conversion lookup for the solver.
using System.Collections.Generic;
using System.Linq;
using Ara3D.Geometry.AST;
using Ara3D.Geometry.Compiler.Symbols;
using Ara3D.Geometry.Compiler.Types;

namespace Ara3D.Geometry.Compiler.Checking
{
    public partial class Solver
    {
        // --- return-position coercion ---------------------------------------------

        /// <summary>
        /// Resolve a return-position coercion: the body's type must unify with — or implicitly
        /// convert to — the declared return type. Accepted conversions mirror what the generated C#
        /// compiles implicitly: a cast relation (<c>Vector3</c> → <c>Point3D</c>), a same-shape
        /// tuple for a struct (<c>(a, b)</c> → <c>Line2D</c>, binding the tuple's element holes
        /// from the struct's fields), and a value for an interface it implements. When not forced,
        /// defers while either side still has holes a later resolution could fill.
        /// </summary>
        private bool ResolveCoercion(CoercionConstraint cc, bool force)
        {
            var from = Zonk(cc.From, Substitution);
            var to = Zonk(cc.To, Substitution);

            // Grounding the DECLARED side (`to`) from the body is always safe and informative, so do
            // it eagerly. But grounding the BODY side (`from`) — when it is still a bare result var of
            // a not-yet-resolved call — must wait: a coercion is not an equality, and hard-binding the
            // body var to the declared type defeats the very conversion this constraint exists for.
            // The classic victim is a tuple return `(a, b): Point2D`, lowered to a `Tuple2(a, b)` call
            // whose result var is unbound while its element overloads (`SumOf(..)/count`) are pending:
            // binding that var to `Point2D` here makes the later `Tuple2` commit fail CHK101. Defer
            // (non-forced) so the overload grounds `from` to `Tuple2<..>` first, then TryCoerce runs.
            if (from == null || to == null || IsVar(to))
            {
                Unify(cc.From, cc.To, Substitution, cc.Origin, record: true);
                return true;
            }
            if (IsVar(from))
            {
                if (!force)
                    return false; // let a pending overload define the body type before we bind it
                Unify(cc.From, cc.To, Substitution, cc.Origin, record: true);
                return true;
            }

            if (!force && (HasVar(from) || HasVar(to)))
                return false; // inner holes may still be filled — defer

            if (TryCoerce(from, to, Substitution))
                return true;

            // Nothing fits: report as the unification failure it is (located).
            Unify(from, to, Substitution, cc.Origin, record: true);
            return true;
        }

        /// <summary>
        /// Whether <paramref name="from"/> coerces to <paramref name="to"/> under the same rules the
        /// generated C# compiles implicitly, binding any variables the match determines (committed to
        /// <paramref name="sub"/> only on full success):
        ///   * identity / unification;
        ///   * a declared cast relation (<c>Vector3</c> → <c>Point3D</c>);
        ///   * a same-shape <c>Tuple{N}</c> into an N-field struct, each element coercing to its field
        ///     (the generated tuple-constructor implicit operator applies per-argument conversions,
        ///     so <c>(Vector3, Number)</c> → <c>AxisAngle { Axis: Vector3, Angle: Angle }</c> works via
        ///     <c>Number</c> → <c>Angle</c>);
        ///   * a value into an interface it implements.
        /// Mirrors <see cref="CSharpConcreteTypeWriter"/>'s emitted implicit operators, so a coercion
        /// accepted here is one the runtime actually performs. A bare payload never wraps into a
        /// one-field type (<c>Number</c> → <c>ModelLength { Value: Number }</c>): the writer emits only
        /// the unwrap, so wrapping is a constructor call
        /// (<c>tracker/decisions/2026-08-02-single-field-mirror-unwraps-only.md</c>, TKT-445).
        /// </summary>
        private bool TryCoerce(TypeExpression from, TypeExpression to, Dictionary<string, TypeExpression> sub, int depth = 0)
        {
            if (from == null || to == null || depth > MaxDepth)
                return true;
            if (UnifyAllOrNothing(from, to, sub))
                return true;
            if (HasConversion(from, to))
                return true;
            if (TupleConvertsToStruct(from, to, sub, depth))
                return true;
            if (to.Def != null && to.Def.IsInterface() && SatisfiesConcept(from, to, sub))
                return true;
            return false;
        }

        /// <summary>A <c>Tuple{N}</c> converts to a concrete struct with N fields, each element
        /// COERCING (not merely unifying) to its field type — the generated tuple-constructor
        /// implicit operator applies an implicit conversion per constructor argument.</summary>
        private bool TupleConvertsToStruct(TypeExpression from, TypeExpression to, Dictionary<string, TypeExpression> sub, int depth)
        {
            if (from?.Def?.Name == null || !from.Def.Name.StartsWith("Tuple"))
                return false;
            if (to?.Def == null || !to.Def.IsConcrete() || to.Def.Fields.Count != from.TypeArgs.Count)
                return false;
            var map = BuildParamSubstitution(to);
            var scratch = new Dictionary<string, TypeExpression>(sub);
            for (var i = 0; i < from.TypeArgs.Count; i++)
            {
                var fieldType = Substitute(to.Def.Fields[i].Type, map);
                if (!TryCoerce(Zonk(from.TypeArgs[i], scratch), Zonk(fieldType, scratch), scratch, depth + 1))
                    return false;
            }
            foreach (var kv in scratch)
                sub[kv.Key] = kv.Value;
            return true;
        }

        /// <summary>
        /// The instance of <paramref name="conceptDef"/> the argument implements *directly* (as itself,
        /// or via a one-level Implements/Inherits clause), with the argument's type arguments
        /// substituted in — e.g. List&lt;Number&gt; : IArray&lt;T&gt; yields IArray&lt;Number&gt;. Returns null
        /// for purely transitive implementations, where a one-level substitution would be incomplete.
        /// </summary>
        private TypeExpression DirectInstance(TypeExpression argType, TypeDef conceptDef)
        {
            if (argType.Def.Name == conceptDef.Name)
                return argType;
            var map = BuildParamSubstitution(argType);
            foreach (var impl in argType.Def.Implements.Concat(argType.Def.Inherits))
                if (impl?.Def != null && impl.Def.Name == conceptDef.Name)
                    return Substitute(impl, map);
            return null;
        }

        /// <summary>Unify on a scratch copy and adopt the bindings only if it fully succeeds, so a
        /// partial/failed match never leaves spurious bindings or rejects the candidate.</summary>
        private void BindBestEffort(TypeExpression a, TypeExpression b, Dictionary<string, TypeExpression> sub)
        {
            var scratch = new Dictionary<string, TypeExpression>(sub);
            if (Unify(a, b, scratch, null, record: false))
                foreach (var kv in scratch)
                    sub[kv.Key] = kv.Value;
        }

        /// <summary>Map a type's declared parameters to its actual arguments (List's T -&gt; Number).</summary>
        private static Dictionary<string, TypeExpression> BuildParamSubstitution(TypeExpression t)
        {
            var map = new Dictionary<string, TypeExpression>();
            var tps = t.Def.TypeParameters;
            for (var i = 0; i < tps.Count && i < t.TypeArgs.Count; i++)
                map[tps[i].Name] = t.TypeArgs[i];
            return map;
        }

        private bool HasConversion(TypeExpression from, TypeExpression to)
            => FindConversion(from, to) != null;

        /// <summary>The cast function witnessing an implicit conversion from <paramref name="from"/>
        /// to <paramref name="to"/>, or null if none. Looks up cast relations directly rather than via
        /// Compilation.GetRelation, which throws when a source has multiple relations to the same
        /// destination — the solver must stay total. The returned function is recorded on a
        /// <see cref="ResolvedCall"/> so the elaborator's TirCoerce node names the exact conversion.</summary>
        private IFunction FindConversion(TypeExpression from, TypeExpression to)
        {
            if (Compilation?.TypeRelations == null || from?.Def == null || to?.Def == null)
                return null;
            if (!Compilation.TypeRelations.RelationLookup.TryGetValue(from.Def, out var list))
                return null;
            return list.FirstOrDefault(rel => rel.Cast != null && rel.Dest.Equals(to.Def))?.Cast;
        }

    }
}
