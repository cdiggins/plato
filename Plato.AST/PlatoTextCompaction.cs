using System.Text;

namespace Ara3D.Geometry.AST
{
    public static class PlatoTextCompaction
    {
        static bool IsDelimiter(char c)
            => c is '{' or '}' or ',' or ';' or '<' or '>' or ':';

        public static string Apply(string text, PlatoFormatOptions options)
        {
            if (options.Pretty)
                return text;

            var result = text;
            if (options.Compressed)
                result = CollapseWhitespace(result);
            if (options.TightDelimiters)
                result = TightenDelimiters(result);
            return result;
        }

        static string CollapseWhitespace(string text)
        {
            var sb = new StringBuilder(text.Length);
            var inWs = false;
            foreach (var ch in text)
            {
                if (char.IsWhiteSpace(ch))
                {
                    if (!inWs)
                    {
                        sb.Append(' ');
                        inWs = true;
                    }
                }
                else
                {
                    sb.Append(ch);
                    inWs = false;
                }
            }
            return sb.ToString().Trim();
        }

        static string TightenDelimiters(string text)
        {
            var sb = new StringBuilder(text.Length);
            for (var i = 0; i < text.Length; i++)
            {
                var ch = text[i];
                if (ch != ' ')
                {
                    sb.Append(ch);
                    continue;
                }

                var prev = i > 0 ? text[i - 1] : '\0';
                var next = i + 1 < text.Length ? text[i + 1] : '\0';
                if (IsDelimiter(prev) || IsDelimiter(next))
                    continue;
                sb.Append(' ');
            }
            return sb.ToString();
        }
    }
}
