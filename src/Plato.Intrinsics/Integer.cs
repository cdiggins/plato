using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using Ara3D.Collections;
using static System.Runtime.CompilerServices.MethodImplOptions;

namespace Ara3D.Geometry
{
    /// <summary>
    /// A simple wrapper around the built-in <c>int</c> type,
    /// forwarding all arithmetic and common methods to <c>int</c>.
    /// </summary>
    [DataContract]
    public partial struct Integer 
    {
        // -------------------------------------------------------------------------------
        // Field (the wrapped int)
        // -------------------------------------------------------------------------------
        [DataMember] public readonly int Value;

        // -------------------------------------------------------------------------------
        // Constructors
        // -------------------------------------------------------------------------------
        [MethodImpl(AggressiveInlining)]
        public Integer(int value) => Value = value;

        // -------------------------------------------------------------------------------
        // Convert to/from int
        // -------------------------------------------------------------------------------
       
        [MethodImpl(AggressiveInlining)]
        public int ToSystem() => Value;

        [MethodImpl(AggressiveInlining)]
        public static Integer FromSystem(int i) => new(i);

        [MethodImpl(AggressiveInlining)]
        public static implicit operator Integer(int i) => FromSystem(i);

        [MethodImpl(AggressiveInlining)]
        public static implicit operator int(Integer n) => n.ToSystem();

        // -------------------------------------------------------------------------------
        // Type-token constants (the `_`-receiver members in intrinsics-scalars)
        // -------------------------------------------------------------------------------
        // Zero / One are NOT here: the forward stdlib gives them Plato bodies
        // (primitives-integer.library.plato) and the generated partial struct declares them
        // (plato-378 constant migration).
        public static Integer MinValue = int.MinValue;
        public static Integer MaxValue = int.MaxValue;

        // ------------------------------------------------------------------------------
        // Concept obligations that no Plato declaration can carry for a primitive. These
        // must be INSTANCE members (an extension method cannot implement an interface
        // member). Bitwise's no-arg member is a property; its argument-taking members are
        // methods, matching the Bitwise<Self> concept.
        // ------------------------------------------------------------------------------

        public Integer Hash { [MethodImpl(AggressiveInlining)] get => Value.GetHashCode(); }
        public Integer BitwiseNot { [MethodImpl(AggressiveInlining)] get => ~Value; }
        public Number ToNumber { [MethodImpl(AggressiveInlining)] get => Value; }

        [MethodImpl(AggressiveInlining)] public Integer BitwiseXor(Integer b) => Value ^ b.Value;
        [MethodImpl(AggressiveInlining)] public Integer ShiftLeft(Integer bits) => Value << bits.Value;
        [MethodImpl(AggressiveInlining)] public Integer ShiftRight(Integer bits) => Value >> bits.Value;

        /// <summary>The array kernel's only constructor (intrinsics.library.plato).</summary>
        [MethodImpl(AggressiveInlining)]
        public IReadOnlyList<T> MapRange<T>(Func<Integer, T> f) => new ReadOnlyList<T>(Value, i => f(i));

#if PLATO_FORWARD_CONCEPTS
        // See the note on Number: explicit because the two names are already statics, and
        // guarded because NumericalLimits<Self> only exists where the forward stdlib's
        // Interfaces.g.cs is compiled.
        Integer NumericalLimits<Integer>.MinValue { [MethodImpl(AggressiveInlining)] get => MinValue; }
        Integer NumericalLimits<Integer>.MaxValue { [MethodImpl(AggressiveInlining)] get => MaxValue; }
#endif

        // -------------------------------------------------------------------------------
        // Operators (forward to int)
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

        // The Bitwise concept's two operator-named members; the generated struct renders
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
