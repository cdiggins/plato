// A string-literal policy that spells each constant as a view of a UTF-16 literal at its use site (TKT-1159, idea B3).
using System.Text;

namespace Ara3D.Geometry.CppWriter.Policies
{
    /// <summary>
    /// Each use of a constant is <c>PLATO_STR(u"text", n)</c>: a String that views the <c>u"..."</c> literal itself, n UTF-16
    /// code units in static storage. Nothing is built or allocated when the site is evaluated, and the constant outlives
    /// every arena reset. The text is the one a <c>make_string_n</c> site builds: through UTF-8 and back, so a lone
    /// surrogate (which the encoder replaces) is the same character under every policy. The empty constant is
    /// <c>String()</c>.
    /// </summary>
    public sealed class StaticStringLiterals : IStringLiteralPolicy
    {
        public string Literal(string text, int length)
        {
            if (length == 0)
                return "String()";
            var utf16 = Encoding.UTF8.GetString(Encoding.UTF8.GetBytes(text));
            return $"PLATO_STR(u\"{CppWriter.EscapeCppU16String(utf16)}\", {utf16.Length})";
        }
    }
}
