using System;
using System.Collections.Generic;
using System.Linq;
using Ara3D.Parakeet;

namespace Ara3D.Geometry.Compiler.Symbols
{
    /// <summary>
    /// Recovers the `//` comment block written directly above a declaration, so the C# writer can
    /// carry it into the emitted struct as an XML doc comment.
    ///
    /// It is recovered from the SOURCE TEXT rather than from the parse tree because the Plato
    /// grammar folds comments into whitespace (`WS => (SpaceChars | Comment).ZeroOrMore()`), which
    /// leaves no CST node to hang them on. Every AST node carries its ParserRange, and the range
    /// knows its own input text, so the block above a declaration is recoverable exactly.
    ///
    /// What counts as the doc block: the run of `//` lines immediately above the declaration, with
    /// no blank line between them and it. A blank line ends the run — which is what separates a
    /// type's own comment from the `//==` section banners the stdlib puts above groups of types.
    /// A `//=` line ends it too, so a banner touching a declaration is not mistaken for its doc.
    /// </summary>
    public static class DocComment
    {
        public static string Extract(ILocation location)
        {
            var range = location?.GetRange();
            if (range?.End?.Input == null)
                return null;
            var text = range.InputText;
            if (string.IsNullOrEmpty(text))
                return null;
            var declStart = SkipTrivia(text, Math.Min(range.BeginPosition, text.Length));
            return BlockAbove(text, declStart);
        }

        /// <summary>The first position at or after <paramref name="pos"/> that is neither
        /// whitespace nor a comment. A node's range may begin before its leading trivia, so the
        /// backward scan has to start from the declaration keyword itself, not from the range.</summary>
        private static int SkipTrivia(string text, int pos)
        {
            while (pos < text.Length)
            {
                if (char.IsWhiteSpace(text[pos]))
                {
                    pos++;
                }
                else if (text[pos] == '/' && pos + 1 < text.Length && text[pos + 1] == '/')
                {
                    while (pos < text.Length && text[pos] != '\n')
                        pos++;
                }
                else if (text[pos] == '/' && pos + 1 < text.Length && text[pos + 1] == '*')
                {
                    var end = text.IndexOf("*/", pos + 2, StringComparison.Ordinal);
                    pos = end < 0 ? text.Length : end + 2;
                }
                else
                {
                    break;
                }
            }
            return pos;
        }

        private static string BlockAbove(string text, int declStart)
        {
            var lines = new List<string>();
            var lineStart = StartOfLine(text, declStart);
            while (lineStart > 0)
            {
                var prevEnd = lineStart - 1;
                if (prevEnd > 0 && text[prevEnd - 1] == '\r')
                    prevEnd--;
                var prevStart = StartOfLine(text, prevEnd);
                var line = text.Substring(prevStart, Math.Max(0, prevEnd - prevStart)).Trim();
                if (!line.StartsWith("//") || line.StartsWith("//="))
                    break;
                lines.Add(line.Substring(2).Trim());
                lineStart = prevStart;
            }
            lines.Reverse();
            var doc = string.Join(Environment.NewLine, lines).Trim();
            return doc.Length == 0 ? null : doc;
        }

        /// <summary>The index just past the last newline STRICTLY BEFORE <paramref name="pos"/> —
        /// i.e. the start of the line <paramref name="pos"/> sits on. Searching from pos itself
        /// would find the newline that ENDS an empty line and report a start past its end.</summary>
        private static int StartOfLine(string text, int pos)
        {
            if (pos <= 0)
                return 0;
            var i = text.LastIndexOf('\n', Math.Min(pos, text.Length) - 1);
            return i < 0 ? 0 : i + 1;
        }
    }
}
