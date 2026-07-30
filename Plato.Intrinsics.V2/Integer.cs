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
            => n.Number();

        /// <summary>
        /// Conversion function 
        /// </summary>
        [MethodImpl(AggressiveInlining)] public Number Number() => Value;
    }

    /// <summary>
    /// Plato math intrinsics for <see cref="Integer"/>, as extension methods (the
    /// all-extension-methods runtime).
    /// </summary>
    public static class IntegerIntrinsics
    {
        /// <summary>
        /// The absolute value of the integer.
        /// </summary>
        [MethodImpl(AggressiveInlining)] public static Integer Abs(this Integer self) => Math.Abs(self.Value);

        /// <summary>
        /// An integer that indicates the sign of this integer (-1, 0, or 1).
        /// </summary>
        [MethodImpl(AggressiveInlining)] public static Integer Sign(this Integer self) => Math.Sign(self.Value);

        /// <summary>
        /// Linear interpolation, using default integer rounding.
        /// </summary>
        public static Integer Lerp(this Integer self, Integer other, Number t)
        {
            var delta = other - self;
            return (int)(self.Value + (delta.Value * t));
        }

        /// <summary>
        /// Creates a range of integers from 0 to this integer. The ELEMENT type is the erased
        /// primitive, matching ArrayExtensions.Range — see the note on that class (plato-323).
        /// </summary>
        [MethodImpl(AggressiveInlining)]
        public static IReadOnlyList<int> Range(this Integer self) => ArrayExtensions.Range(self);

        // Forward-stdlib scalar obligations (intrinsics-scalars.library.plato). Exact `int`
        // returns for Compare/Hash: the scalar-erased Comparable/Hashable obligations demand the
        // erased type (see the same note on Angle.Compare/Hash). ShiftLeft/ShiftRight and the
        // Bitwise pair have no operator-name mapping in Operators, so the writer cannot
        // synthesize them from the struct's operator overloads.
        [MethodImpl(AggressiveInlining)] public static int Compare(this Integer a, Integer b) => a.Value.CompareTo(b.Value);
        [MethodImpl(AggressiveInlining)] public static int Hash(this Integer self) => self.Value.GetHashCode();
        [MethodImpl(AggressiveInlining)] public static Integer Min(this Integer a, Integer b) => Math.Min(a.Value, b.Value);
        [MethodImpl(AggressiveInlining)] public static Integer Max(this Integer a, Integer b) => Math.Max(a.Value, b.Value);
        [MethodImpl(AggressiveInlining)] public static Integer BitwiseNot(this Integer x) => ~x.Value;
        [MethodImpl(AggressiveInlining)] public static Integer BitwiseXor(this Integer a, Integer b) => a.Value ^ b.Value;
        [MethodImpl(AggressiveInlining)] public static Integer ShiftLeft(this Integer x, Integer bits) => x.Value << bits.Value;
        [MethodImpl(AggressiveInlining)] public static Integer ShiftRight(this Integer x, Integer bits) => x.Value >> bits.Value;
        [MethodImpl(AggressiveInlining)] public static Number ToNumber(this Integer self) => self.Value;
        [MethodImpl(AggressiveInlining)] public static ReadOnlyList<T> Repeat<T>(this Integer n, T x) => n.MapRange(_ => x);
    }
}
