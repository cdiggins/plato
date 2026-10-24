// Defines numeric precision profiles and their primitive representation choices.
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ara3D.Geometry.Compiler.Types
{
    /// <summary>
    /// The arithmetic that <c>Number</c> and <c>Integer</c> mean in one compilation. <c>Number</c>
    /// is an abstract real; a profile binds it to one machine arithmetic for the whole program, so a
    /// Plato program means one thing per profile (docs/SEMANTICS.md section 1). It is chosen per
    /// compilation, not per declaration: there is no second scalar type.
    ///
    /// This is the compiler's single description of a profile. A writer derives its scalar type
    /// names, literal spelling and intrinsic library from it; the C# runtime half
    /// (src/Plato.Intrinsics/NumberProfile.cs) binds the same names from the MSBuild property
    /// <c>PlatoNumberProfile</c>, and the generated <c>NumberProfile.g.cs</c> stops the build if the
    /// two disagree.
    ///
    /// Later steps attach here: library precision requirements
    /// (<c>requires Number.ResolutionAt(1.0e8) &lt;= 1.0e-6</c>) evaluate against
    /// <see cref="Precision"/> and <see cref="RoundingUnit"/> at compile time, and the Plato-owned,
    /// correctly rounded elementary functions replace the host's under <see cref="Cpu64"/>.
    /// </summary>
    public sealed class NumberProfile
    {
        /// <summary>The name a build passes: <c>--profile=cpu64</c>, <c>-p:PlatoNumberProfile=cpu64</c>.</summary>
        public string Name { get; }

        /// <summary>Storage width of <c>Number</c>: 64 (IEEE 754 binary64) or 32 (binary32).</summary>
        public int NumberBits { get; }

        /// <summary>Significand bits including the hidden bit: 53 for binary64, 24 for binary32.</summary>
        public int Precision { get; }

        /// <summary>Storage width of <c>Integer</c>.</summary>
        public int IntegerBits { get; }

        /// <summary>Half the gap between 1 and the next larger <c>Number</c>: 2^-53 or 2^-24.</summary>
        public double RoundingUnit => Math.Pow(2, -Precision);

        private NumberProfile(string name, int numberBits, int precision, int integerBits)
            => (Name, NumberBits, Precision, IntegerBits) = (name, numberBits, precision, integerBits);

        /// <summary>
        /// The default on CPU backends. <c>Number</c> is binary64; <c>Integer</c> is stored in 64
        /// bits and is exact within ±(2^53 − 1), so every <c>Integer</c> converts to <c>Number</c>
        /// exactly.
        /// </summary>
        public static readonly NumberProfile Cpu64 = new NumberProfile("cpu64", 64, 53, 64);

        /// <summary>
        /// On request, for code that must match a GPU: <c>Number</c> is binary32 and <c>Integer</c>
        /// is 32-bit.
        /// </summary>
        public static readonly NumberProfile Gpu32 = new NumberProfile("gpu32", 32, 24, 32);

        public static NumberProfile Default => Cpu64;

        public static IReadOnlyList<NumberProfile> All { get; } = new[] { Cpu64, Gpu32 };

        /// <summary>The profile of that name, or null.</summary>
        public static NumberProfile TryParse(string name)
            => All.FirstOrDefault(p => string.Equals(p.Name, name, StringComparison.Ordinal));

        public static string Names => string.Join(", ", All.Select(p => p.Name));

        public override string ToString() => Name;
    }
}
