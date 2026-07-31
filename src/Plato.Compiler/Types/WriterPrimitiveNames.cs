using System.Collections.Generic;

namespace Ara3D.Geometry.Compiler.Types
{
    /// <summary>
    /// The type names a backend writer treats as PRIMITIVE: names whose representation comes from
    /// the runtime rather than from the Plato declaration. When a name is on this list the C# writer
    /// emits its mapped runtime type (<c>Number -&gt; float</c>) and NEVER emits the declared fields
    /// — a `type X { A: Number }` whose name is here has its shape silently discarded.
    ///
    /// This list is the SOLE copy of the name set. It is held in the compiler assembly so the
    /// language-level rule (<c>Linter.CheckPrimitiveNameShadowing</c>, LINT015) can be enforced
    /// without the compiler depending on any writer; <c>CSharpWriter.PrimitiveTypes</c> builds its
    /// keys from it and only attaches the per-name runtime mapping (a name with no mapping is a
    /// startup failure there). <c>PlatoTests/LinterPrimitiveShadowingTests</c> pins the two
    /// together.
    ///
    /// plato-365 shrank this set to the SCALAR MAP and nothing else, which is also the answer for
    /// any new backend: C++, GLSL and Rust need `number/integer/boolean/character/string/type/
    /// function` and no per-type name list. Nine names left in 2026-07-30 — Vector2/3/4/8
    /// (never declared in the forward stdlib at all), then Angle, Plane, Matrix3x2, Matrix4x4 and
    /// Quaternion, each of which now generates an ordinary struct from its declaration with the
    /// handwritten Plato.Intrinsics runtime supplying only its behaviour
    /// (<c>CSharpWriter.IntrinsicBackedTypes</c>).
    ///
    /// ADDING a name here is how a type stops being real. Do not, unless the type genuinely has no
    /// declarable shape.
    /// </summary>
    public static class WriterPrimitiveNames
    {
        /// <summary>
        /// Every name a writer treats as primitive: scalars, the type token, and the function
        /// types. Equals <c>CSharpWriter.PrimitiveTypes.Keys</c> by construction.
        ///
        /// <c>Dynamic</c> left in 2026-07-31: nothing in the stdlib ever used it, and its presence
        /// obliged every new backend to answer "what is my dynamic?" for a name no declaration
        /// mentions. Re-add it here and in the writers' maps if interop ever needs the escape hatch.
        /// </summary>
        public static readonly IReadOnlyCollection<string> All = new HashSet<string>
        {
            "Number", "Boolean", "Integer", "Character", "String", "Type",
            "Function0", "Function1", "Function2", "Function3", "Function4",
            "Function5", "Function6", "Function7", "Function8", "Function9",
        };

        /// <summary>
        /// Shadowing declarations grandfathered when LINT015 landed: reported as Warnings rather
        /// than Errors while plato-365 retired them. EMPTY since 2026-07-30 — plato-365 removed
        /// every name it covered, so LINT015 is now an unconditional Error and no future type can
        /// shadow a primitive silently.
        ///
        /// This list only ever shrinks. It should stay empty: a new entry means someone re-added a
        /// non-scalar primitive rather than fixing the declaration.
        /// </summary>
        public static readonly IReadOnlyCollection<string> KnownShadowedByStdlib = new HashSet<string>();
    }
}
