// Provides the profile-selected Number wrapper and host mathematical operations.
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using static System.Runtime.CompilerServices.MethodImplOptions;

namespace Ara3D.Geometry
{
    /// <summary>
    /// A simple wrapper around the machine type the Number profile selects (<c>NumberValue</c>:
    /// <c>double</c> under cpu64 and cpu64i32, <c>float</c> under gpu32; see NumberProfile.cs), forwarding all arithmetic to it.
    /// The Plato math intrinsics live in <see cref="NumberIntrinsics"/> as extension methods
    /// (all-extension-methods runtime: struct = field + constructor + operators + conversions).
    /// </summary>
    [DataContract, JsonConverter(typeof(NumberJsonConverter))]
    public readonly partial struct Number
    {
        // -------------------------------------------------------------------------------
        // Field (the wrapped machine number)
        // -------------------------------------------------------------------------------

        [DataMember] public readonly NumberValue Value;

        // -------------------------------------------------------------------------------
        // Constructors
        // -------------------------------------------------------------------------------

        [MethodImpl(AggressiveInlining)]
        public Number(NumberValue value) => Value = value;

        // -------------------------------------------------------------------------------
        // Convert to/from the machine number
        // -------------------------------------------------------------------------------

        [MethodImpl(AggressiveInlining)]
        public static Number FromSystem(NumberValue f) => new(f);

        [MethodImpl(AggressiveInlining)]
        public static implicit operator Number(NumberValue f) => FromSystem(f);

        [MethodImpl(AggressiveInlining)]
        public static implicit operator NumberValue(Number n) => n.Value;

        // -------------------------------------------------------------------------------
        // Operators (forward to the machine number)
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
        // (primitives.library.plato, constants.library.plato) and the generated
        // partial struct declares them (plato-378 constant migration).
        // ------------------------------------------------------------------------------

        public static Number MinValue = NumberValue.MinValue;
        public static Number MaxValue = NumberValue.MaxValue;
        public static Number Pi = NumberMath.PI;
        public static Number Epsilon = NumberValue.Epsilon;

        // ------------------------------------------------------------------------------
        // The rest of the Number half of the intrinsic contract. These are INSTANCE
        // members of the struct, not extension methods: several of them discharge a
        // interface obligation (which an extension method cannot do), and the generated
        // extension forwarders call every one of them on the wrapper receiver
        // (`self.Sqrt`, `self.Pow(power)`). No-arg members are PROPERTIES because the
        // interfaces declare them as properties in the wrapper-scalar recipe.
        //
        // Everything derivable from these has a Plato reference body in
        // primitives.library.plato (Abs, Sign, Square, Reciprocal, Min, Max,
        // Ceiling, Truncate, Cbrt, Log/Log2/Log10, Tan/Sinh/Cosh/Tanh, the inverse-trig
        // family, IsFinite) and MUST NOT be duplicated here - a duplicate makes every
        // generated call site ambiguous (CS0121).
        // ------------------------------------------------------------------------------

        public Integer Hash { [MethodImpl(AggressiveInlining)] get => Value.GetHashCode(); }
        public Number Floor { [MethodImpl(AggressiveInlining)] get => NumberMath.Floor(Value); }
        public Number Sqrt { [MethodImpl(AggressiveInlining)] get => NumberMath.Sqrt(Value); }
        public Number Exp { [MethodImpl(AggressiveInlining)] get => NumberMath.Exp(Value); }
        public Number NaturalLog { [MethodImpl(AggressiveInlining)] get => NumberMath.Log(Value); }
        public Number Sin { [MethodImpl(AggressiveInlining)] get => NumberMath.Sin(Value); }
        public Number Cos { [MethodImpl(AggressiveInlining)] get => NumberMath.Cos(Value); }
        public Boolean IsNaN { [MethodImpl(AggressiveInlining)] get => NumberValue.IsNaN(Value); }
        public Boolean IsInfinite { [MethodImpl(AggressiveInlining)] get => NumberValue.IsInfinity(Value); }
        public Integer ToInteger { [MethodImpl(AggressiveInlining)] get => (IntegerValue)Value; }

        [MethodImpl(AggressiveInlining)] public Number Round(Integer digits) => NumberMath.Round(Value, (int)digits.Value);
        [MethodImpl(AggressiveInlining)] public Number Pow(Number power) => NumberMath.Pow(Value, power.Value);
        [MethodImpl(AggressiveInlining)] public Number FusedMultiplyAdd(Number y, Number z) => NumberMath.FusedMultiplyAdd(Value, y.Value, z.Value);
        [MethodImpl(AggressiveInlining)] public Number Atan2Radians(Number x) => NumberMath.Atan2(Value, x.Value);

#if PLATO_FORWARD_CONCEPTS
        // `INumericalLimits<Self>` declares MinValue / MaxValue as INSTANCE members, and the
        // wrapper already spends those two names on statics, so the obligation is discharged
        // explicitly. They are METHODS because the generated interfaces are property-free.
        // Guarded: the interface only exists in a project that compiles the forward stdlib's
        // Interfaces.g.cs, and this shared project is also imported by the legacy generated
        // projects and the tests, which have no such type.
        [MethodImpl(AggressiveInlining)] Number INumericalLimits<Number>.MinValue() => MinValue;
        [MethodImpl(AggressiveInlining)] Number INumericalLimits<Number>.MaxValue() => MaxValue;
#endif
    }
}
