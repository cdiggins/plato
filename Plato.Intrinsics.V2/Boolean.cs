using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
    using static System.Runtime.CompilerServices.MethodImplOptions;

    namespace Ara3D.Geometry
    {
        /// <summary>
        /// A simple wrapper around the built-in <c>Boolean</c> type,
        /// forwarding logical operations and common methods to <c>Boolean</c>.
        /// </summary>
        [DataContract]
        public partial struct Boolean
        {
            // -------------------------------------------------------------------------------
            // Field (the wrapped Boolean)
            // -------------------------------------------------------------------------------

            [DataMember] public readonly bool Value;

            // -------------------------------------------------------------------------------
            // Constructors
            // -------------------------------------------------------------------------------

            [MethodImpl(AggressiveInlining)]
            public Boolean(bool value) => Value = value;

            // -------------------------------------------------------------------------------
            // Convert to/from Boolean
            // -------------------------------------------------------------------------------

            [MethodImpl(AggressiveInlining)]
            public bool ToSystem() => Value;

            [MethodImpl(AggressiveInlining)]
            public static Boolean FromSystem(bool b) => new(b);

            [MethodImpl(AggressiveInlining)]
            public static implicit operator Boolean(bool b) => FromSystem(b);

            [MethodImpl(AggressiveInlining)]
            public static implicit operator bool(Boolean b) => b.ToSystem();

            [MethodImpl(AggressiveInlining)]
            public static bool operator true(Boolean b) => b.Value;

            [MethodImpl(AggressiveInlining)]
            public static bool operator false(Boolean b) => !b.Value;

            // -------------------------------------------------------------------------------
            // Operators (logical, equality, etc.)
            // -------------------------------------------------------------------------------

            /// <summary>
            /// Logical AND operator (bitwise AND for booleans).
            /// </summary>
            [MethodImpl(AggressiveInlining)]
            public static Boolean operator &(Boolean a, Boolean b)
                => a.Value & b.Value;

            /// <summary>
            /// Logical OR operator (bitwise OR for booleans).
            /// </summary>
            [MethodImpl(AggressiveInlining)]
            public static Boolean operator |(Boolean a, Boolean b)
                => a.Value | b.Value;

            /// <summary>
            /// Logical XOR operator (bitwise exclusive OR for booleans).
            /// </summary>
            [MethodImpl(AggressiveInlining)]
            public static Boolean operator ^(Boolean a, Boolean b)
                => a.Value ^ b.Value;

            /// <summary>
            /// Logical NOT operator (negates the value).
            /// </summary>
            [MethodImpl(AggressiveInlining)]
            public static Boolean operator !(Boolean b)
                => !b.Value;

            // -------------------------------------------------------------------------------
            // Comparison operators
            // -------------------------------------------------------------------------------

            public static bool operator <=(Boolean a, Boolean b)
                => !a.Value || a.Value == b.Value;

            public static bool operator >=(Boolean a, Boolean b)
                => a.Value || a.Value == b.Value;

            public static bool operator <(Boolean a, Boolean b)
                => !a && b;

            public static bool operator >(Boolean a, Boolean b)
                => a && !b;

            // -------------------------------------------------------------------------------
            // IComparable / IComparable<Boolean> Implementation
            // -------------------------------------------------------------------------------

            [MethodImpl(AggressiveInlining)]
            public int CompareTo(Boolean other)
                => Value.CompareTo(other);


            public Boolean ExclusiveOr(Boolean other)
                => Value ^ other;
        }
    }