using System.Collections.Generic;
using System.Linq;
using Ara3D.Geometry.Compiler.Symbols;

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
            // FunctionDefinitions already covers every method AND every field (a field is a
            // one-parameter FunctionDef under the hood — see FieldDef), plus library/free
            // functions, so this single pass is exhaustive; no separate field/method walk needed.
            foreach (var f in Compilation.FunctionDefinitions ?? Enumerable.Empty<FunctionDef>())
                CheckFunction(f.OwnerType, f);

            return Diagnostics;
        }

        private void CheckFunction(TypeDef owner, FunctionDef f)
        {
            if (f == null)
                return;

            // Parameters[0] is the receiver for member/library-extension functions; it legitimately
            // carries the literal Self type and is excluded from the existential test the same way
            // TypeDef.IsObjectSafeMethod excludes it.
            var toCheck = f.Parameters.Count > 0
                ? f.Parameters.Skip(1).Select(p => p.Type).Append(f.ReturnType)
                : new[] { f.ReturnType };

            foreach (var te in toCheck)
                CheckReference(owner, te, f);
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
