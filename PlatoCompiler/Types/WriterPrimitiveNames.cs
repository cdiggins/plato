using System.Collections.Generic;

namespace Ara3D.Geometry.Compiler.Types
{
    /// <summary>
    /// The type names a backend writer treats as PRIMITIVE: names whose representation comes from
    /// the runtime rather than from the Plato declaration. When a name is on this list the C# writer
    /// emits its mapped runtime type (<c>Number -&gt; float</c>, <c>Quaternion -&gt;
    /// System.Numerics.Quaternion</c>) and NEVER emits the declared fields — a `type X { A: Number }`
    /// whose name is here has its shape silently discarded.
    ///
    /// This list is the canonical copy, held in the compiler assembly so the language-level rule
    /// (<c>Linter.CheckPrimitiveNameShadowing</c>, LINT015) can be enforced without the compiler
    /// depending on any writer. The C# writer's <c>CSharpWriter.PrimitiveTypes</c> dictionary is the
    /// SAME set with the runtime mappings attached; the two are held together by
    /// <c>PlatoTests/LinterPrimitiveShadowingTests.WriterPrimitiveTableMatchesTheCompilerCopy</c>,
    /// which fails if either side gains or loses a name. Editing one without the other is a test
    /// failure, not a silent divergence — that duplication-with-an-enforcing-test is the deliberate
    /// shape here, and the eventual cleanup (plato-365) is for the writer's dictionary to build its
    /// keys from this list.
    ///
    /// plato-365 shrinks this set to scalars only. Removing a name here (and from the writer's
    /// dictionary) is exactly what makes the corresponding stdlib declaration authoritative.
    /// </summary>
    public static class WriterPrimitiveNames
    {
        /// <summary>
        /// Every name the C# writer currently treats as primitive. Must equal
        /// <c>CSharpWriter.PrimitiveTypes.Keys</c>.
        /// </summary>
        public static readonly IReadOnlyCollection<string> All = new HashSet<string>
        {
            // Scalars and function types — the intended end state (plato-365).
            "Number", "Boolean", "Integer", "Character", "String", "Dynamic", "Type",
            "Function0", "Function1", "Function2", "Function3", "Function4",
            "Function5", "Function6", "Function7", "Function8", "Function9",

            // Non-scalar entries, being retired by plato-365. Each one is a name whose stdlib
            // declaration is currently overridden by a System.Numerics (or Vector256) type.
            // (Vector2/3/4/8 left the list on 2026-07-30 with plato-365 increment (a); they were
            // never declared in the forward stdlib, so their departure changed no finding. Angle
            // and Plane left the same day: both now generate as ordinary structs from their
            // declarations, with the handwritten runtime supplying only their behaviour.)
            "Matrix3x2", "Matrix4x4", "Quaternion",
        };

        /// <summary>
        /// The shadowing declarations that already exist in the forward stdlib when LINT015 landed.
        /// They are the defect the rule exists to describe, so they are reported — but as Warnings,
        /// because their removal is scheduled work (plato-365 increments a and c) rather than
        /// something the author of the declaration can fix, and an Error would red the `lint
        /// --strict` gate for unrelated missions.
        ///
        /// This list only ever shrinks. Delete a name from it when the matching entry leaves
        /// <see cref="All"/>, at which point the declaration stops shadowing anything and the
        /// finding disappears on its own. A name that is NOT here shadowing a primitive is an
        /// Error — which is the whole point: no FUTURE type can do this silently.
        /// </summary>
        public static readonly IReadOnlyCollection<string> KnownShadowedByStdlib = new HashSet<string>
        {
            "Matrix3x2",    // plato-365 increment (c)
            "Matrix4x4",    // plato-365 increment (c)
            "Quaternion",   // plato-365 increment (c)
        };
    }
}
