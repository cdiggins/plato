using System;

namespace Ara3D.Geometry
{
    /// <summary>Position-based JSON scanning over a span. Shared by the object and array readers;
    /// every method advances <paramref name="pos"/> only on success.</summary>
    public static class JsonScan
    {
        public static void Whitespace(ReadOnlySpan<char> text, scoped ref int pos)
        {
            while (pos < text.Length && char.IsWhiteSpace(text[pos]))
                pos++;
        }

        public static bool Take(ReadOnlySpan<char> text, scoped ref int pos, char c)
        {
            if (pos >= text.Length || text[pos] != c)
                return false;
            pos++;
            return true;
        }

        /// <summary>A quoted string; <paramref name="body"/> is the RAW span between the quotes
        /// (escapes intact — <see cref="Unquote"/> decodes them).</summary>
        public static bool String(ReadOnlySpan<char> text, scoped ref int pos, out ReadOnlySpan<char> body)
        {
            body = default;
            if (pos >= text.Length || text[pos] != '"')
                return false;
            var start = ++pos;
            while (pos < text.Length)
            {
                var c = text[pos];
                if (c == '\\') { pos += 2; continue; }
                if (c == '"')
                {
                    body = text.Slice(start, pos - start);
                    pos++;
                    return true;
                }
                pos++;
            }
            return false;
        }

        /// <summary>One complete JSON value; <paramref name="raw"/> is its verbatim text
        /// (quotes and all, so the value parser can tell `1` from `"1"`).</summary>
        public static bool Value(ReadOnlySpan<char> text, scoped ref int pos, out ReadOnlySpan<char> raw)
        {
            raw = default;
            var start = pos;
            if (!SkipValue(text, ref pos))
                return false;
            raw = text.Slice(start, pos - start);
            return true;
        }

        private const string ValueTerminators = ",}] \t\r\n";

        private static bool SkipValue(ReadOnlySpan<char> text, scoped ref int pos)
        {
            if (pos >= text.Length)
                return false;
            switch (text[pos])
            {
                case '"': return String(text, ref pos, out _);
                case '{': return SkipNested(text, ref pos, '{', '}');
                case '[': return SkipNested(text, ref pos, '[', ']');
                default:
                    var start = pos;
                    while (pos < text.Length && ValueTerminators.IndexOf(text[pos]) < 0)
                        pos++;
                    return pos > start;
            }
        }

        private static bool SkipNested(ReadOnlySpan<char> text, scoped ref int pos, char open, char close)
        {
            var depth = 0;
            while (pos < text.Length)
            {
                var c = text[pos];
                if (c == '"')
                {
                    if (!String(text, ref pos, out _))
                        return false;
                    continue;
                }
                pos++;
                if (c == open)
                    depth++;
                else if (c == close && --depth == 0)
                    return true;
            }
            return false;
        }

        /// <summary>Decode a JSON string token (with its quotes) to its text.</summary>
        public static bool Unquote(ReadOnlySpan<char> raw, out string result)
        {
            result = null;
            var pos = 0;
            if (!String(raw, ref pos, out var body) || pos != raw.Length)
                return false;
            if (body.IndexOf('\\') < 0)
            {
                result = body.ToString();
                return true;
            }
            var sb = new System.Text.StringBuilder(body.Length);
            for (var i = 0; i < body.Length; i++)
            {
                if (body[i] != '\\')
                {
                    sb.Append(body[i]);
                    continue;
                }
                if (++i >= body.Length)
                    return false;
                switch (body[i])
                {
                    case '"': sb.Append('"'); break;
                    case '\\': sb.Append('\\'); break;
                    case '/': sb.Append('/'); break;
                    case 'b': sb.Append('\b'); break;
                    case 'f': sb.Append('\f'); break;
                    case 'n': sb.Append('\n'); break;
                    case 'r': sb.Append('\r'); break;
                    case 't': sb.Append('\t'); break;
                    case 'u':
                        if (i + 4 >= body.Length
                            || !ushort.TryParse(body.Slice(i + 1, 4), System.Globalization.NumberStyles.HexNumber,
                                PlatoJson.Invariant, out var code))
                            return false;
                        sb.Append((char)code);
                        i += 4;
                        break;
                    default: return false;
                }
            }
            result = sb.ToString();
            return true;
        }
    }

    /// <summary>
    /// Reads the members of a JSON object one at a time. The generated <c>TryParse</c> loops
    /// <c>while (r.Read())</c>, matches <see cref="Name"/> against its field names, and accepts the
    /// result only when <see cref="Completed"/> — so unknown members are skipped but malformed or
    /// truncated input is rejected.
    /// </summary>
    public ref struct JsonObjectReader
    {
        private readonly ReadOnlySpan<char> _text;
        private int _pos;
        private bool _failed;
        private bool _closed;
        private bool _first;

        public ReadOnlySpan<char> Name { get; private set; }
        public ReadOnlySpan<char> Value { get; private set; }

        public JsonObjectReader(ReadOnlySpan<char> text)
        {
            _text = text;
            _pos = 0;
            _closed = false;
            _first = true;
            Name = default;
            Value = default;
            JsonScan.Whitespace(_text, ref _pos);
            _failed = !JsonScan.Take(_text, ref _pos, '{');
        }

        /// <summary>The closing brace and all trailing whitespace were consumed with nothing
        /// malformed on the way. The ONLY state in which a parse may be accepted.</summary>
        public bool Completed => _closed && !_failed;

        public bool NameIs(string name)
            => Name.Equals(name.AsSpan(), StringComparison.Ordinal);

        public bool Read()
        {
            if (_failed || _closed)
                return false;
            JsonScan.Whitespace(_text, ref _pos);
            if (JsonScan.Take(_text, ref _pos, '}'))
            {
                JsonScan.Whitespace(_text, ref _pos);
                _closed = true;
                _failed = _pos != _text.Length;
                return false;
            }
            if (!_first && !JsonScan.Take(_text, ref _pos, ','))
                return Fail();
            _first = false;
            JsonScan.Whitespace(_text, ref _pos);
            if (!JsonScan.String(_text, ref _pos, out var name))
                return Fail();
            Name = name;
            JsonScan.Whitespace(_text, ref _pos);
            if (!JsonScan.Take(_text, ref _pos, ':'))
                return Fail();
            JsonScan.Whitespace(_text, ref _pos);
            if (!JsonScan.Value(_text, ref _pos, out var value))
                return Fail();
            Value = value;
            return true;
        }

        private bool Fail()
        {
            _failed = true;
            return false;
        }
    }

    /// <summary>The array counterpart of <see cref="JsonObjectReader"/>, used by the
    /// collection-valued branch of <see cref="JsonValueParser{T}"/>.</summary>
    public ref struct JsonArrayReader
    {
        private readonly ReadOnlySpan<char> _text;
        private int _pos;
        private bool _failed;
        private bool _closed;
        private bool _first;

        public ReadOnlySpan<char> Value { get; private set; }

        public JsonArrayReader(ReadOnlySpan<char> text)
        {
            _text = text;
            _pos = 0;
            _closed = false;
            _first = true;
            Value = default;
            JsonScan.Whitespace(_text, ref _pos);
            _failed = !JsonScan.Take(_text, ref _pos, '[');
        }

        public bool Completed => _closed && !_failed;

        public bool Read()
        {
            if (_failed || _closed)
                return false;
            JsonScan.Whitespace(_text, ref _pos);
            if (JsonScan.Take(_text, ref _pos, ']'))
            {
                JsonScan.Whitespace(_text, ref _pos);
                _closed = true;
                _failed = _pos != _text.Length;
                return false;
            }
            if (!_first && !JsonScan.Take(_text, ref _pos, ','))
                return Fail();
            _first = false;
            JsonScan.Whitespace(_text, ref _pos);
            if (!JsonScan.Value(_text, ref _pos, out var value))
                return Fail();
            Value = value;
            return true;
        }

        private bool Fail()
        {
            _failed = true;
            return false;
        }
    }
}
