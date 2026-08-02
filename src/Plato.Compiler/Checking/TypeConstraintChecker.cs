using System.Collections.Generic;
using System.Linq;
using Ara3D.Geometry.AST;
using Ara3D.Geometry.Compiler.Symbols;
using Ara3D.Geometry.Compiler.Types;

namespace Ara3D.Geometry.Compiler.Checking
{
    /// <summary>
    /// Well-formedness checker for DECLARED TYPE-PARAMETER BOUNDS (plato-382) —
    /// `type Tween&lt;T&gt; where T: Interpolatable`, and the same clause on `interface`.
    ///
    /// It answers the question the bounds exist to answer: is every place that CONSTRUCTS a bounded
    /// type supplying an argument that satisfies the bound? Without it a bound is a comment with
    /// syntax; with it, `Tween&lt;String&gt;` is rejected at the declaration that writes it rather
    /// than surviving to C# emission as a throwing stub.
    ///
    /// Runs at the SYMBOL level, alongside <see cref="SumTypeChecker"/> and
    /// <see cref="ExistentialConceptChecker"/>, so every diagnostic carries the offending symbol
    /// (field, parameter, or the declaration itself) for source location.
    ///
    /// Diagnostics (CHK3xx):
    ///   CHK309 a type argument does not satisfy the bound declared on that parameter
    ///   CHK310 a declared bound does not name an interface — on a type/interface parameter, or on a
    ///          function's own signature variable (plato-393)
    ///
    /// UNIFORM OVER SUM TYPES BY CONSTRUCTION: the walk is over <see cref="TypeDef"/>, and a sum
    /// type's per-case field types are visited in the same loop as a record's fields. Nothing here
    /// tests <see cref="TypeDef.IsSum"/>.
    /// </summary>
    public class TypeConstraintChecker
    {
        public Compilation Compilation { get; }

        public List<CheckDiagnostic> Diagnostics { get; } = new List<CheckDiagnostic>();

        public TypeConstraintChecker(Compilation compilation)
            => Compilation = compilation;

        public IReadOnlyList<CheckDiagnostic> Check()
        {
            foreach (var t in Compilation.AllTypeAndLibraryDefinitions ?? Enumerable.Empty<TypeDef>())
            {
                if (t == null)
                    continue;
                CheckBoundsAreConcepts(t);
                CheckDeclaration(t);
            }
            return Diagnostics;
        }

        // --- the bounds themselves ------------------------------------------------

        /// <summary>CHK310: a bound must name an interface. `where T: Number` promises something the
        /// language cannot check and C# cannot express as a constraint. One reading for both places
        /// a bound may be written — a declaration's parameter, and a function's own signature
        /// variable (plato-393); <paramref name="target"/> names which, for the message.</summary>
        private void CheckBoundIsConcept(TypeExpression bound, string target, Symbol origin)
        {
            if (bound?.Def != null && !bound.Def.IsInterface())
                Error("CHK310",
                    $"the bound '{bound}' declared on {target} is not an interface — a `where` bound "
                    + "must name an interface", origin);
        }

        private void CheckBoundsAreConcepts(TypeDef t)
        {
            foreach (var tp in t.TypeParameters)
                foreach (var bound in tp.Constraints)
                    CheckBoundIsConcept(bound, $"parameter '{tp.Name}' of '{t.Name}'", t);
        }

        // --- the construction sites -----------------------------------------------

        /// <summary>Every type expression this declaration WRITES: what it implements/inherits, its
        /// field types (a sum type's case fields included), and the signature of every function it
        /// declares. Each is checked, with its own nested type arguments.</summary>
        private void CheckDeclaration(TypeDef t)
        {
            foreach (var te in t.Implements.Concat(t.Inherits))
                CheckType(te, t, null);

            foreach (var field in t.Fields ?? Enumerable.Empty<FieldDef>())
                CheckType(field.Type, t, field);

            foreach (var c in t.Cases)
                foreach (var field in c.Fields)
                    CheckType(field.Type, t, t);

            // DECLARED methods only. TypeDef.Functions also contains the synthesized field accessor
            // (`Animation(x: Holder): Tween<String>`) and the sum-case factories, whose signatures
            // are built from the field and case types already walked above — including them would
            // report one authored mistake three or four times.
            foreach (var f in t.Methods.Select(m => m.Function))
            {
                if (f == null)
                    continue;
                // A signature's own type variables acquire the bounds of the constructed types it
                // mentions (`Sample(x: Tween<$T>, ...)` gives `$T: Interpolatable`), so a variable
                // reused across parameters is judged against everything the signature knows about it.
                var inherited = TypeConstraints.InheritedBounds(f);
                foreach (var p in f.Parameters ?? Enumerable.Empty<ParameterDef>())
                    CheckType(p.Type, t, p, inherited);
                CheckType(f.ReturnType, t, f, inherited);

                foreach (var d in f.DeclaredBounds)
                    CheckBoundIsConcept(d?.Bound,
                        $"'{d?.VariableName}' of function '{f.Name}' (in '{t.Name}')", f);
            }
        }

        /// <summary>CHK309: for one constructed type, every argument must satisfy the bounds its
        /// parameter declares. Recurses into the arguments, so `Array&lt;Tween&lt;String&gt;&gt;`
        /// is caught at the inner construction.</summary>
        private void CheckType(TypeExpression te, TypeDef owner, Symbol origin,
            IReadOnlyDictionary<string, IReadOnlyList<TypeExpression>> inherited = null, int depth = 0)
        {
            if (te?.Def == null || depth > 8)
                return;

            for (var i = 0; i < te.TypeArgs.Count; i++)
            {
                var arg = te.TypeArgs[i];
                var extra = TypeConstraints.Inherited(arg, inherited);
                foreach (var bound in TypeConstraints.BoundsAt(te, i))
                {
                    if (TypeConstraints.Satisfies(arg, bound, extra))
                        continue;
                    var parameterName = te.Def.TypeParameters[i].Name;
                    Error("CHK309",
                        $"'{arg}' does not satisfy the bound '{bound}' declared on parameter "
                        + $"'{parameterName}' of '{te.Def.Name}' (in '{owner?.Name}'): "
                        + Reason(arg, bound, extra),
                        origin ?? owner);
                }
                CheckType(arg, owner, origin, inherited, depth + 1);
            }
        }

        /// <summary>Why the argument failed, in the terms the author has to act on: it does not
        /// implement the interface at all, it implements it at different type arguments, or — for a
        /// bare parameter — nothing it is known to carry supplies it.</summary>
        private static string Reason(TypeExpression arg, TypeExpression bound,
            IReadOnlyList<TypeExpression> extra)
        {
            var carried = TypeConstraints.KnownBounds(arg, extra);
            if (arg.Def.Kind == TypeKind.TypeParameter || arg.Def.Kind == TypeKind.TypeVariable)
                return $"its own bounds ({string.Join(", ", carried)}) do not imply '{bound}'";
            return ConceptClosure.FindInstance(arg, bound.Def.Name) != null
                ? $"'{arg}' implements '{bound.Def.Name}' at different type arguments"
                : $"'{arg}' does not implement '{bound.Def.Name}'";
        }

        private void Error(string code, string message, Symbol origin)
            => Diagnostics.Add(new CheckDiagnostic(DiagnosticSeverity.Error, code, message, origin));
    }
}
