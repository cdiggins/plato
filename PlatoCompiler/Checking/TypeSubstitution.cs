using System.Collections.Generic;
using System.Linq;
using Ara3D.Geometry.AST;
using Ara3D.Geometry.Compiler.Symbols;
using Ara3D.Geometry.Compiler.Types;

namespace Ara3D.Geometry.Compiler.Checking
{
    /// <summary>
    /// A monomorphization substitution: a map from the <em>structural form</em> of an abstract type
    /// (its <see cref="TypeExpression.ToString"/> — <c>"Self"</c>, a type parameter <c>"T"</c>, a
    /// unification variable <c>"$V0"</c>, or a whole first-parameter interface <c>"IArray&lt;T&gt;"</c>)
    /// to a concrete <see cref="TypeExpression"/>, grounding the abstract types in a
    /// <see cref="TirFunction"/> (see <see cref="Monomorphizer"/>). Keying by structural form (rather
    /// than bare name) matches the reifier exactly: <see cref="ReifiedType"/>'s concept map replaces
    /// <c>Self</c> and each concept type-parameter by name (no type-args, so name == structural form),
    /// while the library-over-interface map replaces the whole first parameter by <em>equality</em>
    /// (<c>te.Equals(pt)</c>) — so <c>IArray&lt;T&gt;</c> maps, but a different <c>IArray&lt;Number&gt;</c>
    /// in the body does not. It is applied with the <em>same</em> structural helper the reifier uses
    /// (<see cref="ReificationExtensions.Replace"/>), so a specialized type is the type the reifier
    /// would have produced.
    ///
    /// Application is total and null-safe: an unbound type is left alone, so a partial substitution
    /// simply leaves the uncovered types abstract (measured, never thrown on).
    /// </summary>
    public class TypeSubstitution
    {
        // Keyed by TypeExpression.ToString() (structural form), not bare Def.Name.
        private readonly IReadOnlyDictionary<string, TypeExpression> _map;

        public TypeSubstitution(IReadOnlyDictionary<string, TypeExpression> map)
            => _map = map ?? new Dictionary<string, TypeExpression>();

        public static readonly TypeSubstitution Empty =
            new TypeSubstitution(new Dictionary<string, TypeExpression>());

        public bool IsEmpty => _map.Count == 0;

        /// <summary>The bound structural keys (e.g. <c>"Self"</c>, <c>"T"</c>, <c>"IVector"</c>).</summary>
        public IReadOnlyCollection<string> Keys => _map.Keys.ToList();

        /// <summary>Look up a single structural key's replacement (null if unbound).</summary>
        public TypeExpression Lookup(string key)
            => key != null && _map.TryGetValue(key, out var t) ? t : null;

        /// <summary>Return this substitution extended with <c>Self → <paramref name="selfType"/></c>
        /// (only if <c>Self</c> is not already bound). Library-over-interface functions reference
        /// <c>Self</c> in their body (e.g. <c>Reverse</c>'s <c>Self.CreateFromComponents(…)</c>)
        /// though it is absent from their signature; <c>Self</c> always denotes the type the function
        /// is reified onto, so binding it is universally sound.</summary>
        public TypeSubstitution WithSelf(TypeExpression selfType)
        {
            if (selfType == null || _map.ContainsKey("Self"))
                return this;
            var m = new Dictionary<string, TypeExpression>(_map) { ["Self"] = selfType };
            return new TypeSubstitution(m);
        }

        /// <summary>Ground a type by replacing every bound (sub)type (recursively into type arguments),
        /// reusing the reifier's <see cref="ReificationExtensions.Replace"/>. Null-safe.</summary>
        public TypeExpression Apply(TypeExpression t)
            => t?.Replace(MapNode);

        public IReadOnlyList<TypeExpression> ApplyAll(IReadOnlyList<TypeExpression> ts)
            => ts?.Select(Apply).ToList();

        private TypeExpression MapNode(TypeExpression t)
            => t != null && _map.TryGetValue(t.ToString(), out var r) ? r : t;

        // --- construction --------------------------------------------------------

        /// <summary>
        /// Derive the substitution that turns <paramref name="original"/>'s declared signature into
        /// the given ground signature (a <see cref="Types.ReifiedFunction"/>'s parameter/return
        /// types). Structural pairing: where an abstract head differs from the ground head, bind that
        /// name (<c>Self</c>→T, first-parameter <c>IVector</c>→T); where heads agree, recurse into
        /// type-arguments (<c>IArray&lt;T&gt;</c> vs <c>IArray&lt;Number&gt;</c> binds <c>T</c>→
        /// <c>Number</c>). This anchors the substitution to the reifier oracle by construction.
        /// </summary>
        public static TypeSubstitution FromSignature(FunctionDef original,
            IReadOnlyList<TypeExpression> groundParams, TypeExpression groundReturn)
        {
            var map = new Dictionary<string, TypeExpression>();
            if (original != null)
            {
                var ps = original.Parameters;
                if (groundParams != null)
                    for (var i = 0; i < ps.Count && i < groundParams.Count; i++)
                        Pair(ps[i].Type, groundParams[i], map);
                Pair(original.ReturnType, groundReturn, map);
            }
            return new TypeSubstitution(map);
        }

        /// <summary>Derive a substitution by pairing an abstract signature given as type lists —
        /// used with a solver-ZONKED signature, whose variables are in the same terminal form as
        /// the TIR body's residual variables (see <see cref="Checking.TirFunction"/>).</summary>
        public static TypeSubstitution FromSignature(IReadOnlyList<TypeExpression> abstractParams,
            TypeExpression abstractReturn, IReadOnlyList<TypeExpression> groundParams, TypeExpression groundReturn)
        {
            var map = new Dictionary<string, TypeExpression>();
            if (abstractParams != null && groundParams != null)
                for (var i = 0; i < abstractParams.Count && i < groundParams.Count; i++)
                    Pair(abstractParams[i], groundParams[i], map);
            Pair(abstractReturn, groundReturn, map);
            return new TypeSubstitution(map);
        }

        /// <summary>This substitution extended with <paramref name="other"/>'s bindings (existing
        /// keys win).</summary>
        public TypeSubstitution MergedWith(TypeSubstitution other)
        {
            if (other == null || other.IsEmpty)
                return this;
            var m = new Dictionary<string, TypeExpression>(_map);
            foreach (var key in other.Keys)
                if (!m.ContainsKey(key))
                    m[key] = other.Lookup(key);
            return new TypeSubstitution(m);
        }

        private static void Pair(TypeExpression abstractT, TypeExpression groundT,
            Dictionary<string, TypeExpression> map)
        {
            if (abstractT == null || groundT == null)
                return;

            if (abstractT.Name != groundT.Name)
            {
                // The heads differ: the reifier rewrote this node. Record the binding — keyed by the
                // whole structural form, so a first-parameter IArray<T> maps as a unit (matching the
                // reifier's te.Equals(pt)) — only for the kinds the reifier actually substitutes
                // (Self / type-parameter / type-variable / interface); never fabricate one for a
                // concrete head.
                var key = abstractT.ToString();
                if (IsSubstitutable(abstractT) && !map.ContainsKey(key))
                    map[key] = groundT;

                // A generic-interface head replaced by a concrete type ALSO determines the
                // interface's element types: IArrayLike<$T> ↔ Vector3 binds $T → Number through
                // Vector3's IArrayLike<Number> instance. Without this, body types built from the
                // elements (IArray<$T>, Function2<$T,$T,$T>) stay abstract — the residual
                // "$-element types the checker didn't ground" class.
                if (abstractT.TypeArgs.Count > 0 && abstractT.Def.IsInterface() && groundT.Def != null)
                {
                    var instance = ConceptClosure.FindInstance(groundT, abstractT.Def.Name);
                    if (instance != null && instance.TypeArgs.Count == abstractT.TypeArgs.Count)
                        for (var i = 0; i < abstractT.TypeArgs.Count; i++)
                            Pair(abstractT.TypeArgs[i], instance.TypeArgs[i], map);
                }
                return;
            }

            if (abstractT.TypeArgs.Count == groundT.TypeArgs.Count)
                for (var i = 0; i < abstractT.TypeArgs.Count; i++)
                    Pair(abstractT.TypeArgs[i], groundT.TypeArgs[i], map);
        }

        private static bool IsSubstitutable(TypeExpression t)
            => t.Def != null
               && (t.Def.Kind == TypeKind.SelfType
                   || t.Def.Kind == TypeKind.TypeParameter
                   || t.Def.Kind == TypeKind.TypeVariable
                   || t.Def.Kind == TypeKind.Interface);

        // --- groundness ----------------------------------------------------------

        /// <summary>A type is "ground" for monomorphization when it contains no type variable, type
        /// parameter, or Self node. Interfaces with ground arguments count as ground nominal types —
        /// consistent with the reifier, which leaves e.g. <c>IArray&lt;Number&gt;</c> as a return
        /// type. Null (a statement node's absent type) is ground.</summary>
        public static bool IsGround(TypeExpression t)
        {
            if (t?.Def == null)
                return true;
            if (t.Def.Kind == TypeKind.SelfType
                || t.Def.Kind == TypeKind.TypeParameter
                || t.Def.Kind == TypeKind.TypeVariable)
                return false;
            return t.TypeArgs.All(IsGround);
        }

        public override string ToString()
            => IsEmpty ? "{}" : "{" + string.Join(", ", _map.Select(kv => $"{kv.Key}->{kv.Value}")) + "}";
    }
}
