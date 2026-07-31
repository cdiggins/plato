using System.Runtime.CompilerServices;
using static System.Runtime.CompilerServices.MethodImplOptions;

namespace Ara3D.Geometry
{
    /// <summary>
    /// Behaviour-only partial for the GENERATED <c>Angle</c> struct (plato-365).
    ///
    /// Angle used to be a handwritten wrapper around a <c>float Value</c> AND a
    /// <c>CSharpWriter.PrimitiveTypes</c> entry mapping the name to bare <c>float</c> AND a stdlib
    /// declaration (<c>type Angle implements Quantity { Radians: Number; }</c>) — three stories
    /// about one name, none of them checked against the others. The declaration is now the only
    /// shape authority: the writer generates the <c>Radians</c> field, the constructor, the
    /// conversions and the equality scaffolding, and this file supplies only what a generated
    /// struct cannot express — operators and the trigonometric kernel.
    ///
    /// Nothing here may declare a field, a constructor, a conversion or an <c>Equals</c>/
    /// <c>GetHashCode</c>/<c>ToString</c> override: the generated partial already has them, and a
    /// second copy is a duplicate-member error rather than a silent override.
    /// </summary>
    public partial struct Angle
    {
        // -------------------------------------------------------------------------------
        // Operators (forward to the radians payload)
        // -------------------------------------------------------------------------------

        [MethodImpl(AggressiveInlining)]
        public static Angle operator +(Angle a, Angle b)
            => new(a.Radians + b.Radians);

        [MethodImpl(AggressiveInlining)]
        public static Angle operator -(Angle a, Angle b)
            => new(a.Radians - b.Radians);

        [Obsolete("This method is illegal and should not be used.", true)]
        public static Angle operator *(Angle a, Angle b)
            => throw new Exception("Multiplying two angles is not well-defined");

        [Obsolete("This method is illegal and should not be used.", true)]
        public static Angle operator /(Angle a, Angle b)
            => throw new Exception("Dividing two angles is not well-defined");

        [MethodImpl(AggressiveInlining)]
        public static Angle operator *(Angle a, float x)
            => new(a.Radians * x);

        [MethodImpl(AggressiveInlining)]
        public static Angle operator *(float x, Angle a)
            => new(x * a.Radians);

        [MethodImpl(AggressiveInlining)]
        public static Angle operator /(Angle a, float x)
            => new(a.Radians / x);

        [MethodImpl(AggressiveInlining)]
        public static Angle operator %(Angle a, float x)
            => new(a.Radians % x);

        [MethodImpl(AggressiveInlining)]
        public static Angle operator -(Angle n)
            => new(-n.Radians);

        [MethodImpl(AggressiveInlining)]
        public static bool operator <(Angle a, Angle b)
            => a.Radians < b.Radians;

        [MethodImpl(AggressiveInlining)]
        public static bool operator <=(Angle a, Angle b)
            => a.Radians <= b.Radians;

        [MethodImpl(AggressiveInlining)]
        public static bool operator >(Angle a, Angle b)
            => a.Radians > b.Radians;

        [MethodImpl(AggressiveInlining)]
        public static bool operator >=(Angle a, Angle b)
            => a.Radians >= b.Radians;

        // -------------------------------------------------------------------------------
        // Comparison / hashing obligations
        // -------------------------------------------------------------------------------
        //
        // Exact `int` signatures: these discharge the (scalar-erased) Comparable / Hashable
        // obligations the forward-stdlib generation declares on the Angle partial, and interface
        // implementation demands the exact erased types.

        [MethodImpl(AggressiveInlining)]
        public int CompareTo(Angle other)
            => Radians.CompareTo(other.Radians);

        [MethodImpl(AggressiveInlining)]
        public int Compare(Angle other)
            => Radians.CompareTo(other.Radians);

        [MethodImpl(AggressiveInlining)]
        public int Hash()
            => Radians.GetHashCode();
    }

    /// <summary>
    /// Behavioural intrinsics for <see cref="Angle"/> as extension methods — the all-extension-methods
    /// runtime (M5 / consolidation plan C3). The trigonometric functions
    /// (https://en.wikipedia.org/wiki/Trigonometric_functions) live here; the radians payload is a
    /// generated field, so nothing in this class reaches for a wrapper.
    /// </summary>
    public static class AngleIntrinsics
    {
        /// <summary>Cosine.</summary>
        [MethodImpl(AggressiveInlining)] public static float Cos(this Angle self) => MathF.Cos(self.Radians);

        /// <summary>Hyperbolic cosine.</summary>
        [MethodImpl(AggressiveInlining)] public static float Cosh(this Angle self) => MathF.Cosh(self.Radians);

        /// <summary>Sine.</summary>
        [MethodImpl(AggressiveInlining)] public static float Sin(this Angle self) => MathF.Sin(self.Radians);

        /// <summary>Sine and cosine.</summary>
        [MethodImpl(AggressiveInlining)] public static (float Sin, float Cos) SinCos(this Angle self) => MathF.SinCos(self.Radians);

        /// <summary>Hyperbolic sine.</summary>
        [MethodImpl(AggressiveInlining)] public static float Sinh(this Angle self) => MathF.Sinh(self.Radians);

        /// <summary>The tangent.</summary>
        [MethodImpl(AggressiveInlining)] public static float Tan(this Angle self) => MathF.Tan(self.Radians);

        /// <summary>The hyperbolic tangent.</summary>
        [MethodImpl(AggressiveInlining)] public static float Tanh(this Angle self) => MathF.Tanh(self.Radians);
    }
}
