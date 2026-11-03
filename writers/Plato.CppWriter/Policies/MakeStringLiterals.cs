// The current string-literal policy: every constant builds its String with make_string_n.
namespace Ara3D.Geometry.CppWriter.Policies
{
    /// <summary>Each use of a constant calls <c>make_string_n("text", length)</c>.</summary>
    public sealed class MakeStringLiterals : IStringLiteralPolicy
    {
        public string Literal(string text, int length)
            => $"make_string_n(\"{CppWriter.EscapeCppStringPublic(text)}\", {length})";
    }
}
