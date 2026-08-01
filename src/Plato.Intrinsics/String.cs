using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using static System.Runtime.CompilerServices.MethodImplOptions;

namespace Ara3D.Geometry
{
    /// <summary>
    /// A simple wrapper around the built-in <c>string</c> type.
    /// </summary>
    [DataContract]
    public partial struct String
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

        // Property, not method: `Count` is the `Countable` concept member, and an interface
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
}