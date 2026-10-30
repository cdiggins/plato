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

        /// <summary>The characters at the indices <c>start</c> to <c>start + count - 1</c> that exist:
        /// the range is clamped to the String at both ends, and a count of zero or less, or a range
        /// wholly outside, gives the empty String. Never fails. (<c>Substring</c> in the contract.)</summary>
        public String Substring(Integer start, Integer count)
        {
            long first = start.Value;
            long n = count.Value;
            var length = (long)Value.Length;
            if (first < 0)
            {
                n += first; // first is negative and n may be any value: no overflow when n > 0
                first = 0;
            }
            if (n <= 0 || first >= length)
                return new String(string.Empty);
            if (n > length - first)
                n = length - first;
            return new String(Value.Substring((int)first, (int)n));
        }
        
        /// <summary>True when the text is a number in the P21 real forms and its value is finite:
        /// an optional sign, digits with an optional point and more digits (at least one digit
        /// in all), then an optional exponent <c>E</c> or <c>e</c> with an optional sign and digits.
        /// <c>1.</c>, <c>.5</c> and <c>1.E-06</c> are numbers; <c>1e999</c> is not (it rounds to
        /// infinity), and neither are <c>NaN</c>, <c>Infinity</c>, <c>0x10</c>, <c>1,5</c>, an empty
        /// text or any text with a space. (<c>IsNumberText</c> in the contract.)</summary>
        public Boolean IsNumberText() => TextForms.TryParseNumber(Value, out _);

        /// <summary>The correctly rounded value of the number the text spells, parsed by the
        /// host's own parser in the invariant culture after the form is checked; 0 when
        /// <see cref="IsNumberText"/> is false. (<c>NumberFromText</c> in the contract.)</summary>
        public Number NumberFromText() => TextForms.TryParseNumber(Value, out var v) ? v : 0;

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
    /// The text forms the parse intrinsics accept, checked by hand so the three runtimes agree
    /// on what is a number, and then parsed by the host's correctly rounded parser.
    /// </summary>
    internal static class TextForms
    {
        private static bool IsDigit(char c) => c >= '0' && c <= '9';

        /// <summary>Sign, digits with an optional point, optional exponent; nothing else.</summary>
        internal static bool IsRealForm(string t)
        {
            var i = 0;
            var n = t.Length;
            if (i < n && (t[i] == '+' || t[i] == '-'))
                i++;
            var digits = 0;
            while (i < n && IsDigit(t[i])) { i++; digits++; }
            if (i < n && t[i] == '.')
            {
                i++;
                while (i < n && IsDigit(t[i])) { i++; digits++; }
            }
            if (digits == 0)
                return false;
            if (i < n && (t[i] == 'e' || t[i] == 'E'))
            {
                i++;
                if (i < n && (t[i] == '+' || t[i] == '-'))
                    i++;
                var exponentDigits = 0;
                while (i < n && IsDigit(t[i])) { i++; exponentDigits++; }
                if (exponentDigits == 0)
                    return false;
            }
            return i == n;
        }

        /// <summary>The value of a real-form text, false when the form is wrong or the value overflows to infinity.</summary>
        internal static bool TryParseNumber(string t, out NumberValue value)
        {
            value = 0;
            return IsRealForm(t)
                && NumberValue.TryParse(t, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out value)
                && NumberValue.IsFinite(value);
        }
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