// Provides the Character scalar wrapper and its host character operations.
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using static System.Runtime.CompilerServices.MethodImplOptions;

namespace Ara3D.Geometry
{
    /// <summary>
    /// A simple wrapper around the built-in <c>char</c> type.
    /// </summary>
    [DataContract, JsonConverter(typeof(CharacterJsonConverter))]
    public readonly partial struct Character
    {
        // -------------------------------------------------------------------------------
        // Field
        // -------------------------------------------------------------------------------
     
        [DataMember] public readonly char Value;

        // -------------------------------------------------------------------------------
        // Constructor
        // -------------------------------------------------------------------------------

        [MethodImpl(AggressiveInlining)]
        public Character(char value) => Value = value;

        // -------------------------------------------------------------------------------
        // Conversions
        // -------------------------------------------------------------------------------

        /// <summary>The UTF-16 code unit value, 0 to 65535. (<c>ToInteger</c> on Character in the contract.)</summary>
        [MethodImpl(AggressiveInlining)]
        public Integer ToInteger() => (IntegerValue)Value;

        [MethodImpl(AggressiveInlining)]
        public char ToSystem() => Value;

        [MethodImpl(AggressiveInlining)]
        public static Character FromSystem(char c) => new(c);

        [MethodImpl(AggressiveInlining)]
        public static implicit operator Character(char c) => FromSystem(c);

        [MethodImpl(AggressiveInlining)]
        public static implicit operator char(Character c) => c.ToSystem();

        [MethodImpl(AggressiveInlining)]
        public static implicit operator Character(Integer n) => FromSystem((char)n.Value);

        [MethodImpl(AggressiveInlining)]
        public static implicit operator Integer(Character c) => c.ToSystem();

        // -------------------------------------------------------------------------------
        // Ordering
        // -------------------------------------------------------------------------------
        //
        // Character declares Orderable in the forward stdlib (primitives.plato), whose
        // LessThanOrEquals is an intrinsic: the writer discharges an operator-named
        // intrinsic by emitting a wrapper over the operator, so the operators have to
        // exist. Ordinal (code-unit) order, matching String's comparison operators.

        [MethodImpl(AggressiveInlining)]
        public static bool operator <=(Character a, Character b) => a.Value <= b.Value;

        [MethodImpl(AggressiveInlining)]
        public static bool operator >=(Character a, Character b) => a.Value >= b.Value;

        [MethodImpl(AggressiveInlining)]
        public static bool operator <(Character a, Character b) => a.Value < b.Value;

        [MethodImpl(AggressiveInlining)]
        public static bool operator >(Character a, Character b) => a.Value > b.Value;
    }
}