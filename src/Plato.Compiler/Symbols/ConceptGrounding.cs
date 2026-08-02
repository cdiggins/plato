using System.Linq;
using Ara3D.Geometry.AST;
using Ara3D.Geometry.Compiler.Types;

namespace Ara3D.Geometry.Compiler.Symbols
{
    /// <summary>
    /// Shared "is this interface reference grounded, or existential?" test (plato-311). A
    /// self-constrained interface <c>C</c> can appear in a type expression two ways:
    ///   - GROUNDED: the enclosing type IS the interface itself (self-reference, e.g. a return type
    ///     of <c>Self</c> inside <c>C</c>'s own declaration), or the enclosing type genuinely
    ///     IMPLEMENTS/INHERITS <c>C</c> (an `implements`/`inherits` clause, where Self legitimately
    ///     grounds to the enclosing concrete/interface type). This is "some C" resolved at a known
    ///     site — the F-bounded generic form `C&lt;Self&gt;` lowers cleanly.
    ///   - EXISTENTIAL: the enclosing type merely REFERENCES C — a field type, return type, or
    ///     parameter type unrelated to what the enclosing type implements (e.g. a field
    ///     `Path: Curve3D` on `SweptSurface`, which does not implement `Curve3D`). This is "any C"
    ///     — an existential that only the non-generic object-safe view interface can express.
    ///
    /// Both <c>Plato.CSharpWriter</c> (to pick the F-bounded vs. non-generic view spelling) and the
    /// checker (to diagnose an existential reference to an interface with an empty object-safe
    /// surface) need this exact same test, so it lives once here rather than being re-derived.
    /// </summary>
    public static class ConceptGrounding
    {
        /// <summary>True when <paramref name="conceptDef"/> is grounded from the point of view of
        /// <paramref name="owner"/>: <paramref name="owner"/> IS <paramref name="conceptDef"/>, or
        /// <paramref name="owner"/> implements/inherits it (directly or transitively).</summary>
        public static bool GroundsSelf(TypeDef owner, TypeDef conceptDef)
        {
            if (owner == null || conceptDef == null)
                return false;
            if (ReferenceEquals(owner, conceptDef))
                return true;
            return owner.GetAllImplementedConcepts().Any(c => c?.Def == conceptDef);
        }

        /// <summary>True when <paramref name="te"/> is a bare (implicit-Self) reference to a
        /// self-constrained interface, used from <paramref name="owner"/>'s point of view, that is
        /// NOT grounded — i.e. an existential ("any C") type-position use.</summary>
        public static bool IsExistentialConceptReference(TypeDef owner, TypeExpression te)
            => te?.Def != null
               && te.Def.Kind != TypeKind.SelfType
               && te.Def.IsInterface()
               && te.Def.IsSelfConstrained()
               && !GroundsSelf(owner, te.Def);
    }
}
