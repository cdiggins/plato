using System.Collections.Generic;
using System.Linq;
using Ara3D.Geometry.Compiler.Symbols;
using Ara3D.Geometry.Compiler.Types;

namespace Ara3D.Geometry.Compiler.Checking
{
    /// <summary>
    /// Well-formedness checker for concepts used in TYPE POSITION (plato-311, Option A). A
    /// self-constrained concept referenced as a field type, return type, or parameter type from a
    /// type that does not implement/inherit it is an EXISTENTIAL reference ("any C") — Plato.CSharpWriter
    /// lowers it to the concept's non-generic object-safe view interface (`interface C`). A concept
    /// with NO object-safe member (every method uses Self somewhere other than the receiver) has no
    /// such view, so an existential reference to it has no defined C# lowering. This checker catches
    /// that case with a diagnostic instead of letting the writer improvise.
    ///
    /// Diagnostics:
    ///   CHK308 existential (type-position) reference to a concept with an empty object-safe surface
    /// </summary>
    public class ExistentialConceptChecker
    {
        public Compilation Compilation { get; }

        public List<CheckDiagnostic> Diagnostics { get; } = new List<CheckDiagnostic>();

        public ExistentialConceptChecker(Compilation compilation)
        {
            Compilation = compilation;
        }

        public IReadOnlyList<CheckDiagnostic> Check()
        {
            // FIELDS ONLY, deliberately. A bare concept in a library-function parameter or return
            // is CONSTRAINT-position by Plato convention — the checker unifies all of them to one
            // implementing type and the monomorphizer grounds it (e.g. AlgebraMetric's
            // `IsNear(a: MetricSpace, b: MetricSpace, ...)`), so the writer never sees it as an
            // existential. Only a concept-typed FIELD on a concrete type is genuine storage of
            // "any C" (e.g. `Path: Curve3D` on SweptSurface) — that is the one position whose
            // lowering is the non-generic view, and therefore the one position where a view-less
            // concept is an error.
            foreach (var t in Compilation.AllTypeAndLibraryDefinitions ?? Enumerable.Empty<TypeDef>())
            {
                if (t == null || t.IsInterface())
                    continue;
                foreach (var field in t.Fields ?? Enumerable.Empty<FieldDef>())
                    CheckReference(t, field.Type, field);
            }

            return Diagnostics;
        }

        private void CheckReference(TypeDef owner, TypeExpression te, Symbol origin)
        {
            if (te == null)
                return;

            if (ConceptGrounding.IsExistentialConceptReference(owner, te) && !te.Def.HasObjectSafeSurface())
            {
                Error("CHK308",
                    $"'{te.Def.Name}' is used in type position (an existential 'any {te.Def.Name}') " +
                    $"but has no object-safe member (every member of '{te.Def.Name}' uses Self " +
                    "somewhere other than the receiver), so it has no non-generic view — declare at " +
                    "least one object-safe member on the concept, or avoid using it in type position",
                    origin);
            }

            // Recurse into generic type arguments (e.g. Array<Curve3D>).
            foreach (var arg in te.TypeArgs)
                CheckReference(owner, arg, origin);
        }

        private void Error(string code, string message, Symbol origin)
            => Diagnostics.Add(new CheckDiagnostic(DiagnosticSeverity.Error, code, message, origin));
    }
}
