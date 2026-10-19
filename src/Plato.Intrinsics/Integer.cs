using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Ara3D.Collections;
using static System.Runtime.CompilerServices.MethodImplOptions;

namespace Ara3D.Geometry
{
    /// <summary>
    /// A wrapper around the profile's machine integer, <c>IntegerValue</c> (NumberProfile.cs):
    /// <c>long</c> under cpu64, <c>int</c> under gpu32, forwarding all arithmetic to it. The
    /// conversion to <c>int</c> stays implicit, because indexers and counts of the .NET
    /// collections the runtime wraps are <c>int</c>; under cpu64 it is checked, so an Integer
    /// outside the <c>int</c> range used as an index throws instead of wrapping.
    /// </summary>
    [DataContract, JsonConverter(typeof(IntegerJsonConverter))]
    public readonly partial struct Integer 
    {
        // -------------------------------------------------------------------------------
        // Field (the wrapped machine integer)
        // -------------------------------------------------------------------------------
        [DataMember] public readonly IntegerValue Value;

        // -------------------------------------------------------------------------------
        // Constructors
        // -------------------------------------------------------------------------------
        [MethodImpl(AggressiveInlining)]
        public Integer(IntegerValue value) => Value = value;

        // -------------------------------------------------------------------------------
        // Convert to/from int
        // -------------------------------------------------------------------------------
       
        [MethodImpl(AggressiveInlining)]
        public IntegerValue ToSystem() => Value;

        [MethodImpl(AggressiveInlining)]
        public static Integer FromSystem(IntegerValue i) => new(i);

#if PLATO_NUMBER_GPU32
        [MethodImpl(AggressiveInlining)]
        public static implicit operator Integer(int i) => FromSystem(i);

        [MethodImpl(AggressiveInlining)]
        public static implicit operator int(Integer n) => n.Value;
#else
        [MethodImpl(AggressiveInlining)]
        public static implicit operator Integer(int i) => FromSystem(i);

        [MethodImpl(AggressiveInlining)]
        public static implicit operator Integer(long i) => FromSystem(i);

        [MethodImpl(AggressiveInlining)]
        public static implicit operator long(Integer n) => n.Value;

        [MethodImpl(AggressiveInlining)]
        public static implicit operator int(Integer n) => checked((int)n.Value);
#endif

        // -------------------------------------------------------------------------------
        // Type-token constants (the `_`-receiver members in intrinsics-scalars)
        // -------------------------------------------------------------------------------
        // Zero / One are NOT here: the forward stdlib gives them Plato bodies
        // (primitives.library.plato) and the generated partial struct declares them
        // (plato-378 constant migration).
        public static Integer MinValue = IntegerValue.MinValue;
        public static Integer MaxValue = IntegerValue.MaxValue;

        // ------------------------------------------------------------------------------
        // Interface obligations that no Plato declaration can carry for a primitive. These
        // must be INSTANCE members (an extension method cannot implement an interface
        // member). Bitwise's no-arg member is a property; its argument-taking members are
        // methods, matching the Bitwise<Self> interface.
        // ------------------------------------------------------------------------------

        public Integer Hash { [MethodImpl(AggressiveInlining)] get => Value.GetHashCode(); }
        public Integer BitwiseNot { [MethodImpl(AggressiveInlining)] get => ~Value; }
        public Number ToNumber { [MethodImpl(AggressiveInlining)] get => Value; }

        [MethodImpl(AggressiveInlining)] public Integer BitwiseXor(Integer b) => Value ^ b.Value;
        [MethodImpl(AggressiveInlining)] public Integer ShiftLeft(Integer bits) => Value << (int)bits.Value;
        [MethodImpl(AggressiveInlining)] public Integer ShiftRight(Integer bits) => Value >> (int)bits.Value;

        /// <summary>The array kernel's only constructor (intrinsics.library.plato); see
        /// <see cref="ArrayIntrinsics.MapRange{T}"/>, which computes each element once.</summary>
        [MethodImpl(AggressiveInlining)]
        public IReadOnlyList<T> MapRange<T>(Func<Integer, T> f) => checked((int)Value).MapRange(f);

#if PLATO_FORWARD_CONCEPTS
        // See the note on Number: explicit because the two names are already statics, methods
        // because the generated interfaces are property-free, and guarded because
        // INumericalLimits<Self> only exists where the forward stdlib's Interfaces.g.cs is compiled.
        [MethodImpl(AggressiveInlining)] Integer INumericalLimits<Integer>.MinValue() => MinValue;
        [MethodImpl(AggressiveInlining)] Integer INumericalLimits<Integer>.MaxValue() => MaxValue;
#endif

        // -------------------------------------------------------------------------------
        // Operators (forward to the machine integer)
        // -------------------------------------------------------------------------------
        
        [MethodImpl(AggressiveInlining)]
        public static Integer operator +(Integer a, Integer b)
            => a.Value + b.Value;

        [MethodImpl(AggressiveInlining)]
        public static Integer operator -(Integer a, Integer b)
            => a.Value - b.Value;

        [MethodImpl(AggressiveInlining)]
        public static Integer operator *(Integer a, Integer b)
            => a.Value * b.Value;

        [MethodImpl(AggressiveInlining)]
        public static Integer operator /(Integer a, Integer b)
            => a.Value / b.Value;

        [MethodImpl(AggressiveInlining)]
        public static Integer operator %(Integer a, Integer b)
            => a.Value % b.Value;

        [MethodImpl(AggressiveInlining)]
        public static Integer operator -(Integer n)
            => -n.Value;

        // The Bitwise interface's two operator-named members; the generated struct renders
        // BitwiseAnd / BitwiseOr from these.
        [MethodImpl(AggressiveInlining)]
        public static Integer operator &(Integer a, Integer b)
            => a.Value & b.Value;

        [MethodImpl(AggressiveInlining)]
        public static Integer operator |(Integer a, Integer b)
            => a.Value | b.Value;

        [MethodImpl(AggressiveInlining)]
        public static Integer operator ^(Integer a, Integer b)
            => a.Value ^ b.Value;

        [MethodImpl(AggressiveInlining)]
        public static Integer operator ~(Integer a)
            => ~a.Value;

        [MethodImpl(AggressiveInlining)]
        public static Boolean operator <(Integer a, Integer b)
            => a.Value < b.Value;

        [MethodImpl(AggressiveInlining)]
        public static Boolean operator <=(Integer a, Integer b)
            => a.Value <= b.Value;

        [MethodImpl(AggressiveInlining)]
        public static Boolean operator >(Integer a, Integer b)
            => a.Value > b.Value;

        [MethodImpl(AggressiveInlining)]
        public static Boolean operator >=(Integer a, Integer b)
            => a.Value >= b.Value;

        // -------------------------------------------------------------------------------
        // IComparable / IComparable<Integer> Implementation
        // -------------------------------------------------------------------------------

        [MethodImpl(AggressiveInlining)]
        public int CompareTo(Integer other)
            => Value.CompareTo(other.Value);

        /// <summary>
        /// Implicit conversion to <see cref="Number"/> type.
        /// </summary>
        public static implicit operator Number(Integer n)
            => n.ToNumber;
    }

}
