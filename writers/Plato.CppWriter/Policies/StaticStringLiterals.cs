// A string-literal policy that builds each constant once per use site (TKT-1159, idea B3).
namespace Ara3D.Geometry.CppWriter.Policies
{
    /// <summary>
    /// Each use of a constant is <c>PLATO_STR("text", length)</c>: a function-local static String built on the first
    /// evaluation (thread-safe since C++11) and then read as a <c>const String&amp;</c>, so later evaluations pay no
    /// heap std::string and no UTF-8 decode. The empty constant is <c>String()</c>, which allocates nothing.
    /// </summary>
    public sealed class StaticStringLiterals : IStringLiteralPolicy
    {
        public string Literal(string text, int length)
            => length == 0
                ? "String()"
                : $"PLATO_STR(\"{CppWriter.EscapeCppStringPublic(text)}\", {length})";
    }
}
