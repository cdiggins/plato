// Provides the String scalar wrapper and its host string operations.
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using static System.Runtime.CompilerServices.MethodImplOptions;

namespace Ara3D.Geometry
{
    /// <summary>
    /// A simple wrapper around the built-in <c>string</c> type.
    /// </summary>
    [DataContract, JsonConverter(typeof(StringJsonConverter))]
    public readonly partial struct String
    {
        // -------------------------------------------------------------------------------
        // Field
        // -------------------------------------------------------------------------------

        // Storage is private and the accessor normalizes (plato-383). This wrapper is a struct
        // over a REFERENCE, so `default(String)` holds null and no constructor can intercept
        // it; routing every Plato-visible member through `Value` makes the default behave as
        // the empty string, the way every other wrapper's default is its zero value.
        [DataMember(Name = nameof(Value))] private readonly string _value;

        public string Value { [MethodImpl(AggressiveInlining)] get => _value ?? string.Empty; }

        // -------------------------------------------------------------------------------
        // Constructor
        // -------------------------------------------------------------------------------

        [MethodImpl(AggressiveInlining)]
        public String(string value) => _value = value;

        // -------------------------------------------------------------------------------
        // Methods 
        // -------------------------------------------------------------------------------

        [MethodImpl(AggressiveInlining)]
        public Character At(Integer n) => Value[n];

        public Character this[Integer n] { [MethodImpl(AggressiveInlining)] get => At(n); }

        // Property, not method: `Count` is the `Countable` interface member, and an interface
        // obligation can only be discharged by a member of the matching shape.
        public Integer Count { [MethodImpl(AggressiveInlining)] get => Value.Length; }
        
        // -------------------------------------------------------------------------------
        // Conversions
        // -------------------------------------------------------------------------------

        [MethodImpl(AggressiveInlining)]
        public string ToSystem() => Value;

        [MethodImpl(AggressiveInlining)]
        public static String FromSystem(string s) => new(s);

        [MethodImpl(AggressiveInlining)]
        public static implicit operator String(string s) => FromSystem(s);

        [MethodImpl(AggressiveInlining)]
        public static implicit operator string(String s) => s.ToSystem();

        [MethodImpl(AggressiveInlining)]
        public static bool operator <= (String a, String b) => a.Value.CompareTo(b.Value) <= 0;

        [MethodImpl(AggressiveInlining)]
        public static bool operator >=(String a, String b) => a.Value.CompareTo(b.Value) >= 0;

        [MethodImpl(AggressiveInlining)]
        public static bool operator <(String a, String b) => a.Value.CompareTo(b.Value) < 0;

        [MethodImpl(AggressiveInlining)]
        public static bool operator >(String a, String b) => a.Value.CompareTo(b.Value) > 0;

    }

    /// <summary>
    /// String members that take or produce an Array of Characters: the host half of
    /// <c>FromCharacters</c> (<c>stdlib/foundation/intrinsics.library.plato</c>). Array is
    /// <c>IReadOnlyList</c> in the C# runtime, so the receiver is the list, as for MapRange.
    /// </summary>
    public static class StringIntrinsics
    {
        /// <summary>The String whose characters are <c>cs</c> in order; the empty array gives the empty String.</summary>
        public static String FromCharacters(this IReadOnlyList<Character> cs)
        {
            var chars = new char[cs.Count];
            for (var i = 0; i < chars.Length; i++)
                chars[i] = cs[i].Value;
            return new String(new string(chars));
        }
    }
}