// The Number profile of this build: the one place the C# runtime decides which machine type
// Number wraps and which math library its intrinsics call. Everything else in Plato.Intrinsics,
// and every generated file, names the aliases below and never `float`, `double` or `MathF`.
//
// The profile is the MSBuild property PlatoNumberProfile (Plato.Intrinsics.projitems turns it
// into a compilation symbol):
//   cpu64 (the default)    Number is IEEE 754 binary64 (System.Double), math through System.Math;
//                          Integer is 64-bit (System.Int64), so every Integer within ±(2^53 - 1)
//                          converts to Number exactly.
//   gpu32                  Number is binary32 (System.Single), math through System.MathF;
//                          Integer is 32-bit (System.Int32).
// The compiler's half is src/Plato.Compiler/Types/NumberProfile.cs. Code generated with
// `Plato.CLI --profile=<name>` carries NumberProfile.g.cs, which stops the build when the two
// disagree. docs/SEMANTICS.md section 1 states what each profile means.
//
// Later steps attach here: the profile constants a library precision requirement reads
// (Number.Precision, Number.RoundingUnit, ...), and Plato-owned correctly rounded elementary
// functions, which would replace NumberMath for Sin/Cos/Exp/Log/Pow/Atan2 under cpu64.

#if PLATO_NUMBER_GPU32
global using NumberValue = System.Single;
global using NumberMath = System.MathF;
global using IntegerValue = System.Int32;
#else
global using NumberValue = System.Double;
global using NumberMath = System.Math;
global using IntegerValue = System.Int64;
#endif

namespace Ara3D.Geometry
{
    /// <summary>The Number profile this assembly was built under, readable at run time so a run
    /// record can name the arithmetic that produced a result.</summary>
    public static class PlatoNumberProfile
    {
#if PLATO_NUMBER_GPU32
        public const string Name = "gpu32";
#else
        public const string Name = "cpu64";
#endif

        /// <summary>Significand bits including the hidden bit: 53 under cpu64, 24 under gpu32.</summary>
        public const int Precision = sizeof(NumberValue) == 8 ? 53 : 24;

        /// <summary>Storage bits of Integer: 64 under cpu64, 32 under gpu32.</summary>
        public const int IntegerBits = sizeof(IntegerValue) * 8;

        /// <summary>Reads a JSON number straight into the profile's type, so a binary32 value is
        /// not rounded twice through binary64.</summary>
        internal static NumberValue ReadJson(ref System.Text.Json.Utf8JsonReader reader)
#if PLATO_NUMBER_GPU32
            => reader.GetSingle();
#else
            => reader.GetDouble();
#endif

        /// <summary>Reads a JSON integer at the profile's Integer width.</summary>
        internal static IntegerValue ReadJsonInteger(ref System.Text.Json.Utf8JsonReader reader)
#if PLATO_NUMBER_GPU32
            => reader.GetInt32();
#else
            => reader.GetInt64();
#endif
    }
}
