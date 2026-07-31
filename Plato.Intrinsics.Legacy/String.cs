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
        
        [DataMember] public readonly string Value;

        // -------------------------------------------------------------------------------
        // Constructor
        // -------------------------------------------------------------------------------

        [MethodImpl(AggressiveInlining)]
        public String(string value) => Value = value;

        // -------------------------------------------------------------------------------
        // Methods 
        // -------------------------------------------------------------------------------

        [MethodImpl(AggressiveInlining)]
        public Character At(Integer n) => Value[n];

        public Character this[Integer n] { [MethodImpl(AggressiveInlining)] get => At(n); }

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