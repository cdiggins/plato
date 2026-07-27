using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using static System.Runtime.CompilerServices.MethodImplOptions;

namespace Ara3D.Geometry
{
    /// <summary>
    /// A simple wrapper around the built-in <c>char</c> type.
    /// </summary>
    [DataContract]
    public partial struct Character
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
    }
}