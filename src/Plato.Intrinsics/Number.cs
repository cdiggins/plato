using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using static System.Runtime.CompilerServices.MethodImplOptions;

namespace Ara3D.Geometry
{
    /// <summary>
    /// A simple wrapper around the built-in <c>float</c> type,
    /// forwarding all arithmetic and common methods to <c>float</c>.
    /// The Plato math intrinsics live in <see cref="NumberIntrinsics"/> as extension methods
    /// (all-extension-methods runtime: struct = field + constructor + operators + conversions).
    /// </summary>
    [DataContract]
    public partial struct Number
    {
        // -------------------------------------------------------------------------------
        // Field (the wrapped float)
        // -------------------------------------------------------------------------------

        [DataMember] public readonly float Value;

        // -------------------------------------------------------------------------------
        // Constructors
        // -------------------------------------------------------------------------------

        [MethodImpl(AggressiveInlining)]
        public Number(float value) => Value = value;

        // -------------------------------------------------------------------------------
        // Convert to/from float
        // -------------------------------------------------------------------------------

        [MethodImpl(AggressiveInlining)]
        public static Number FromSystem(float f) => new(f);

        [MethodImpl(AggressiveInlining)]
        public static implicit operator Number(float f) => FromSystem(f);

        [MethodImpl(AggressiveInlining)]
        public static implicit operator float(Number n) => n.Value;

        // -------------------------------------------------------------------------------
        // Operators (forward to float)
        // -------------------------------------------------------------------------------
        [MethodImpl(AggressiveInlining)]
        public static Number operator +(Number a, Number b)
            => a.Value + b.Value;

        [MethodImpl(AggressiveInlining)]
        public static Number operator -(Number a, Number b)
            => a.Value - b.Value;

        [MethodImpl(AggressiveInlining)]
        public static Number operator *(Number a, Number b)
            => a.Value * b.Value;

        [MethodImpl(AggressiveInlining)]
        public static Number operator %(Number a, Number b)
            => a.Value % b.Value;

        [MethodImpl(AggressiveInlining)]
        public static Number operator /(Number a, Number b)
            => a.Value / b.Value;

        [MethodImpl(AggressiveInlining)]
        public static Number operator -(Number n)
            => -n.Value;

        [MethodImpl(AggressiveInlining)]
        public static Boolean operator <(Number a, Number b)
            => a.Value < b.Value;

        [MethodImpl(AggressiveInlining)]
        public static Boolean operator <=(Number a, Number b)
            => a.Value <= b.Value;

        [MethodImpl(AggressiveInlining)]
        public static Boolean operator >(Number a, Number b)
            => a.Value > b.Value;

        [MethodImpl(AggressiveInlining)]
        public static Boolean operator >=(Number a, Number b)
            => a.Value >= b.Value;

        [MethodImpl(AggressiveInlining)]
        public Integer CompareTo(Number other)
            => Value.CompareTo(other.Value);

        // ------------------------------------------------------------------------------
        // The `_`-receiver constants of the intrinsic contract
        // (stdlib/foundation/intrinsics.library.plato). A `_` receiver is Plato's
        // type-level idiom, so a moved extension body re-qualifies the bare name to a
        // STATIC read on the wrapper (`Ara3D.Geometry.Number.Pi`).
        // Zero / One / Tau / E are NOT here: the forward stdlib gives them Plato bodies
        // (primitives-number.library.plato, constants.library.plato) and the generated
        // partial struct declares them (plato-378 constant migration).
        // ------------------------------------------------------------------------------

        public static Number MinValue = float.MinValue;
        public static Number MaxValue = float.MaxValue;
        public static Number Pi = MathF.PI;
        public static Number Epsilon = float.Epsilon;

        // ------------------------------------------------------------------------------
        // The rest of the Number half of the intrinsic contract. These are INSTANCE
        // members of the struct, not extension methods: several of them discharge a
        // concept obligation (which an extension method cannot do), and the generated
        // extension forwarders call every one of them on the wrapper receiver
        // (`self.Sqrt`, `self.Pow(power)`). No-arg members are PROPERTIES because the
        // concepts declare them as properties in the wrapper-scalar recipe.
        //
        // Everything derivable from these has a Plato reference body in
        // primitives-number.library.plato (Abs, Sign, Square, Reciprocal, Min, Max,
        // Ceiling, Truncate, Cbrt, Log/Log2/Log10, Tan/Sinh/Cosh/Tanh, the inverse-trig
        // family, IsFinite) and MUST NOT be duplicated here - a duplicate makes every
        // generated call site ambiguous (CS0121).
        // ------------------------------------------------------------------------------

        public Integer Hash { [MethodImpl(AggressiveInlining)] get => Value.GetHashCode(); }
        public Number Floor { [MethodImpl(AggressiveInlining)] get => MathF.Floor(Value); }
        public Number Sqrt { [MethodImpl(AggressiveInlining)] get => MathF.Sqrt(Value); }
        public Number Exp { [MethodImpl(AggressiveInlining)] get => MathF.Exp(Value); }
        public Number NaturalLog { [MethodImpl(AggressiveInlining)] get => MathF.Log(Value); }
        public Number Sin { [MethodImpl(AggressiveInlining)] get => MathF.Sin(Value); }
        public Number Cos { [MethodImpl(AggressiveInlining)] get => MathF.Cos(Value); }
        public Boolean IsNaN { [MethodImpl(AggressiveInlining)] get => float.IsNaN(Value); }
        public Boolean IsInfinite { [MethodImpl(AggressiveInlining)] get => float.IsInfinity(Value); }
        public Integer ToInteger { [MethodImpl(AggressiveInlining)] get => (int)Value; }

        [MethodImpl(AggressiveInlining)] public Number Round(Integer digits) => MathF.Round(Value, digits.Value);
        [MethodImpl(AggressiveInlining)] public Number Pow(Number power) => MathF.Pow(Value, power.Value);
        [MethodImpl(AggressiveInlining)] public Number FusedMultiplyAdd(Number y, Number z) => MathF.FusedMultiplyAdd(Value, y.Value, z.Value);
        [MethodImpl(AggressiveInlining)] public Number Atan2Radians(Number x) => MathF.Atan2(Value, x.Value);

#if PLATO_FORWARD_CONCEPTS
        // `NumericalLimits<Self>` declares MinValue / MaxValue as INSTANCE members, and the
        // wrapper already spends those two names on statics, so the obligation is discharged
        // explicitly. They are METHODS because the generated interfaces are property-free.
        // Guarded: the interface only exists in a project that compiles the forward stdlib's
        // Interfaces.g.cs, and this shared project is also imported by the legacy generated
        // projects and the tests, which have no such type.
        [MethodImpl(AggressiveInlining)] Number NumericalLimits<Number>.MinValue() => MinValue;
        [MethodImpl(AggressiveInlining)] Number NumericalLimits<Number>.MaxValue() => MaxValue;
#endif
    }
}
