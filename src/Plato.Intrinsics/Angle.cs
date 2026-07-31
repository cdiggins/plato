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
        // Comparison obligation
        // -------------------------------------------------------------------------------
        //
        // `Compare` and `Hash` are NOT here: the forward stdlib gives Angle reference bodies for
        // both (Comparable / Hashable over the Radians field), so the generated partial declares
        // them and a handwritten twin is a duplicate-member error. Only CompareTo — which no
        // Plato declaration spells — stays.

        [MethodImpl(AggressiveInlining)]
        public int CompareTo(Angle other)
            => Radians.CompareTo(other.Radians);
    }

    // The trigonometric extension class that used to live here (AngleIntrinsics: Cos, Cosh, Sin,
    // SinCos, Sinh, Tan, Tanh over an Angle receiver) is gone. The forward stdlib declares that
    // whole surface with bodies over the Radians field, so the generation emits it as AngleTrig —
    // and two extension classes offering `angle.Cos()` are not a duplicate the compiler can pick
    // between, they are CS0121 at every call site. The scalar kernel it forwarded to
    // (MathF.Cos etc.) is still an intrinsic, reached as `float.CosRadians()`.
}
