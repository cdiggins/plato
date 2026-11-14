// The current string-literal policy: every constant builds its String with make_string_n.
namespace Ara3D.Geometry.CppWriter.Policies
{
    /// <summary>Each use of a constant calls <c>make_string_n("text", length)</c>, which decodes the UTF-8 text into the
    /// calling thread's arena at every evaluation; the String lasts until that arena is reset or rolled back.</summary>
    public sealed class MakeStringLiterals : IStringLiteralPolicy
    {
        public string Literal(string text, int length)
            => $"make_string_n(\"{CppWriter.EscapeCppStringPublic(text)}\", {length})";
    }
}
