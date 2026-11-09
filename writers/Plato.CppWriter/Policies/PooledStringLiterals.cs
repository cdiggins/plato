// A string-literal policy that pools the program's constants: one table, one initializer, an index per use site (TKT-1223).
using System.Collections.Generic;
using System.Text;

namespace Ara3D.Geometry.CppWriter.Policies
{
    /// <summary>
    /// Each use of a constant is <c>PLATO_SP(i)</c>: a read of entry <c>i</c> of <c>plato::string_pool</c>, a namespace-scope
    /// array of the program's distinct constants that one initializer fills before <c>main</c> (the writer emits it as
    /// plato_strings.cpp, see <c>CppWriter.EmitStringPool</c>). Constants are deduplicated by text, so a text the program
    /// spells at many sites is one String, and the per-site function-local static of <see cref="StaticStringLiterals"/>, with its
    /// guard, initializer and atexit destructor, is gone. The empty constant is <c>String()</c>, which allocates nothing.
    /// The indices this policy hands out are provisional: the writer renumbers them over the emitted text, so the table holds
    /// only the constants of the functions that survive pruning, in order of first appearance.
    /// </summary>
    public sealed class PooledStringLiterals : IStringLiteralPolicy
    {
        private readonly Dictionary<string, int> _index = new Dictionary<string, int>(System.StringComparer.Ordinal);
        private readonly List<string> _texts = new List<string>();

        /// <summary>The distinct constants in the order they were first spelled, by provisional index.</summary>
        public IReadOnlyList<string> Texts => _texts;

        public string Literal(string text, int length)
        {
            if (length == 0)
                return "String()";
            var utf16 = Normalize(text);
            if (!_index.TryGetValue(utf16, out var i))
            {
                i = _texts.Count;
                _index.Add(utf16, i);
                _texts.Add(utf16);
            }
            return $"PLATO_SP({i})";
        }

        /// <summary>The String a <c>make_string_n</c> site builds from the UTF-8 spelling of the text: the text through UTF-8 and back,
        /// so a lone surrogate (which the encoder replaces) is the same character under every policy.</summary>
        private static string Normalize(string text)
            => Encoding.UTF8.GetString(Encoding.UTF8.GetBytes(text));
    }
}
