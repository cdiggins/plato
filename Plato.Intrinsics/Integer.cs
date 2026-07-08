using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
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

        // -------------------------------------------------------------------------------
        // Integer-specific Helper Properties/Methods
        // -------------------------------------------------------------------------------

        /// <summary>
        /// The absolute value of the integer.
        /// </summary>
        public Integer Abs
        {
            [MethodImpl(AggressiveInlining)]
            get => Math.Abs(Value);
        }

        /// <summary>
        /// An integer that indicates the sign of this integer (-1, 0, or 1).
        /// </summary>
        public Integer Sign
        {
            [MethodImpl(AggressiveInlining)]
            get => Math.Sign(Value);
        }

        /// <summary>
        /// Linear interpolation, using default integer rounding.
        /// </summary>
        public Integer Lerp(Integer other, Number t)
        {
            var delta = other - this;
            return (int)(Value + (delta.Value * t));
        }

        /// <summary>
        /// Creates a range of integers from 0 to this integer. 
        /// </summary>
        public IReadOnlyList<Integer> Range
            => ArrayExtensions.Range(this);

        /// <summary>
        /// Implicit conversion to <see cref="Number"/> type.
        /// </summary>
        public static implicit operator Number(Integer n)
            => n.Number;

        /// <summary>
        /// Conversion function 
        /// </summary>
        public Number Number
        {
            [MethodImpl(AggressiveInlining)]
            get => Value;
        }
    }
}
