// json: the DOM, text reader and writer, from_json and to_json.
// A fragment of plato_cpu.hpp: included by it, in order, inside namespace plato. Not self-contained.

    // ---- Json: the DOM and the text PlatoJson.cs writes (invariant "R" numbers; non-finite as the
    // quoted names "NaN", "Infinity", "-Infinity"; a Character as a one-character string) ----

    struct Json
    {
        enum class Kind : uint8_t { Null, Bool, Number, String, Array, Object };
        Kind kind = Kind::Null;
        bool b = false;
        double n = 0.0;
        std::string s;
        std::vector<Json> items;        // array elements, or object values
        std::vector<std::string> keys;  // object keys, parallel to items

        static Json Null() { return Json(); }
        static Json Bool(bool v) { Json j; j.kind = Kind::Bool; j.b = v; return j; }
        static Json Number(double v) { Json j; j.kind = Kind::Number; j.n = v; return j; }
        static Json String(std::string v) { Json j; j.kind = Kind::String; j.s = std::move(v); return j; }
        static Json Array() { Json j; j.kind = Kind::Array; return j; }
        static Json Object() { Json j; j.kind = Kind::Object; return j; }

        bool is_null() const { return kind == Kind::Null; }
        bool is_bool() const { return kind == Kind::Bool; }
        bool is_number() const { return kind == Kind::Number; }
        bool is_string() const { return kind == Kind::String; }
        bool is_array() const { return kind == Kind::Array; }
        bool is_object() const { return kind == Kind::Object; }

        bool as_bool() const { return b; }
        // A string holding a non-finite name reads as that number (the writer's spelling).
        double as_number() const
        {
            if (kind == Kind::Number) return n;
            if (kind == Kind::String)
            {
                if (s == "NaN") return std::nan("");
                if (s == "Infinity") return HUGE_VAL;
                if (s == "-Infinity") return -HUGE_VAL;
            }
            return 0.0;
        }
        const std::string& as_string() const { return s; }
        int32_t size() const { return static_cast<int32_t>(items.size()); }

        static const Json& null_ref() { static const Json j; return j; }
        const Json& operator[](int32_t i) const
        {
            return i >= 0 && static_cast<size_t>(i) < items.size() ? items[static_cast<size_t>(i)] : null_ref();
        }
        // Exact member name first, then case-insensitively (the reader's PropertyNameCaseInsensitive).
        const Json* find(const std::string& name) const
        {
            if (kind != Kind::Object) return nullptr;
            for (size_t i = 0; i < keys.size(); i++)
                if (keys[i] == name) return &items[i];
            for (size_t i = 0; i < keys.size(); i++)
            {
                if (keys[i].size() != name.size()) continue;
                bool same = true;
                for (size_t k = 0; k < name.size() && same; k++)
                {
                    char x = keys[i][k], y = name[k];
                    if (x >= 'A' && x <= 'Z') x = static_cast<char>(x + 32);
                    if (y >= 'A' && y <= 'Z') y = static_cast<char>(y + 32);
                    same = x == y;
                }
                if (same) return &items[i];
            }
            return nullptr;
        }
        const Json& operator[](const std::string& name) const { const Json* j = find(name); return j ? *j : null_ref(); }
        const Json& operator[](const char* name) const { return (*this)[std::string(name)]; }

        void push(Json v) { items.push_back(std::move(v)); }
        void set(const std::string& name, Json v) { keys.push_back(name); items.push_back(std::move(v)); }
    };

    inline void json_write_string(std::string& out, const std::string& s)
    {
        out.push_back('"');
        for (char c : s)
        {
            switch (c)
            {
            case '"': out += "\\\""; break;
            case '\\': out += "\\\\"; break;
            case '\b': out += "\\b"; break;
            case '\f': out += "\\f"; break;
            case '\n': out += "\\n"; break;
            case '\r': out += "\\r"; break;
            case '\t': out += "\\t"; break;
            default:
                if (static_cast<unsigned char>(c) < 0x20)
                {
                    static const char hex[] = "0123456789abcdef";
                    out += "\\u00";
                    out.push_back(hex[(c >> 4) & 15]);
                    out.push_back(hex[c & 15]);
                }
                else
                    out.push_back(c);
            }
        }
        out.push_back('"');
    }

    inline void json_write(std::string& out, const Json& j)
    {
        switch (j.kind)
        {
        case Json::Kind::Null: out += "null"; break;
        case Json::Kind::Bool: out += j.b ? "true" : "false"; break;
        case Json::Kind::Number:
            if (std::isfinite(j.n)) out += NumberTextUtf8(j.n);
            else json_write_string(out, NumberTextUtf8(j.n));
            break;
        case Json::Kind::String: json_write_string(out, j.s); break;
        case Json::Kind::Array:
            out.push_back('[');
            for (size_t i = 0; i < j.items.size(); i++)
            {
                if (i) out.push_back(',');
                json_write(out, j.items[i]);
            }
            out.push_back(']');
            break;
        case Json::Kind::Object:
            out.push_back('{');
            for (size_t i = 0; i < j.items.size(); i++)
            {
                if (i) out.push_back(',');
                json_write_string(out, j.keys[i]);
                out.push_back(':');
                json_write(out, j.items[i]);
            }
            out.push_back('}');
            break;
        }
    }

    inline std::string json_dump(const Json& j) { std::string s; json_write(s, j); return s; }

    struct JsonParser
    {
        const char* p;
        const char* end;
        bool ok = true;

        void ws() { while (p < end && (*p == ' ' || *p == '\t' || *p == '\n' || *p == '\r')) ++p; }
        bool fail() { ok = false; return false; }
        bool lit(const char* word, size_t n)
        {
            if (static_cast<size_t>(end - p) < n || std::memcmp(p, word, n) != 0) return fail();
            p += n;
            return true;
        }
        static void utf8(std::string& s, uint32_t cp)
        {
            if (cp < 0x80) s.push_back(static_cast<char>(cp));
            else if (cp < 0x800) { s.push_back(static_cast<char>(0xC0 | (cp >> 6))); s.push_back(static_cast<char>(0x80 | (cp & 63))); }
            else if (cp < 0x10000) { s.push_back(static_cast<char>(0xE0 | (cp >> 12))); s.push_back(static_cast<char>(0x80 | ((cp >> 6) & 63))); s.push_back(static_cast<char>(0x80 | (cp & 63))); }
            else { s.push_back(static_cast<char>(0xF0 | (cp >> 18))); s.push_back(static_cast<char>(0x80 | ((cp >> 12) & 63))); s.push_back(static_cast<char>(0x80 | ((cp >> 6) & 63))); s.push_back(static_cast<char>(0x80 | (cp & 63))); }
        }
        bool hex4(uint32_t& v)
        {
            if (end - p < 4) return fail();
            v = 0;
            for (int i = 0; i < 4; i++)
            {
                const char c = *p++;
                v <<= 4;
                if (c >= '0' && c <= '9') v |= static_cast<uint32_t>(c - '0');
                else if (c >= 'a' && c <= 'f') v |= static_cast<uint32_t>(c - 'a' + 10);
                else if (c >= 'A' && c <= 'F') v |= static_cast<uint32_t>(c - 'A' + 10);
                else return fail();
            }
            return true;
        }
        bool str(std::string& out)
        {
            if (p >= end || *p != '"') return fail();
            ++p;
            while (p < end && *p != '"')
            {
                char c = *p++;
                if (c != '\\') { out.push_back(c); continue; }
                if (p >= end) return fail();
                c = *p++;
                switch (c)
                {
                case '"': out.push_back('"'); break;
                case '\\': out.push_back('\\'); break;
                case '/': out.push_back('/'); break;
                case 'b': out.push_back('\b'); break;
                case 'f': out.push_back('\f'); break;
                case 'n': out.push_back('\n'); break;
                case 'r': out.push_back('\r'); break;
                case 't': out.push_back('\t'); break;
                case 'u':
                {
                    uint32_t cp;
                    if (!hex4(cp)) return false;
                    if (cp >= 0xD800 && cp < 0xDC00 && end - p >= 6 && p[0] == '\\' && p[1] == 'u')
                    {
                        p += 2;
                        uint32_t lo;
                        if (!hex4(lo)) return false;
                        cp = 0x10000 + ((cp - 0xD800) << 10) + (lo - 0xDC00);
                    }
                    utf8(out, cp);
                    break;
                }
                default: return fail();
                }
            }
            if (p >= end) return fail();
            ++p;
            return true;
        }
        bool value(Json& j, int depth)
        {
            if (depth > 512) return fail();
            ws();
            if (p >= end) return fail();
            const char c = *p;
            if (c == '{')
            {
                ++p;
                j = Json::Object();
                ws();
                if (p < end && *p == '}') { ++p; return true; }
                for (;;)
                {
                    ws();
                    std::string key;
                    if (!str(key)) return false;
                    ws();
                    if (p >= end || *p != ':') return fail();
                    ++p;
                    Json v;
                    if (!value(v, depth + 1)) return false;
                    j.set(key, std::move(v));
                    ws();
                    if (p < end && *p == ',') { ++p; continue; }
                    if (p < end && *p == '}') { ++p; return true; }
                    return fail();
                }
            }
            if (c == '[')
            {
                ++p;
                j = Json::Array();
                ws();
                if (p < end && *p == ']') { ++p; return true; }
                for (;;)
                {
                    Json v;
                    if (!value(v, depth + 1)) return false;
                    j.push(std::move(v));
                    ws();
                    if (p < end && *p == ',') { ++p; continue; }
                    if (p < end && *p == ']') { ++p; return true; }
                    return fail();
                }
            }
            if (c == '"') { std::string s; if (!str(s)) return false; j = Json::String(std::move(s)); return true; }
            if (c == 't') { j = Json::Bool(true); return lit("true", 4); }
            if (c == 'f') { j = Json::Bool(false); return lit("false", 5); }
            if (c == 'n') { j = Json::Null(); return lit("null", 4); }
            const char* q = p;
            while (q < end && ((*q >= '0' && *q <= '9') || *q == '-' || *q == '+' || *q == '.' || *q == 'e' || *q == 'E')) ++q;
            double d = 0.0;
            auto r = std::from_chars(p, q, d);
            if (q == p || r.ec != std::errc() || r.ptr != q) return fail();
            p = q;
            j = Json::Number(d);
            return true;
        }
    };

    // Parses a whole document; false (and *error set) on malformed text.
    inline bool json_parse(const std::string& text, Json& out, std::string* error = nullptr)
    {
        JsonParser ps{ text.data(), text.data() + text.size() };
        Json j;
        bool good = ps.value(j, 0);
        if (good)
        {
            ps.ws();
            good = ps.p == ps.end;
        }
        if (!good)
        {
            if (error) *error = "invalid JSON at offset " + NumberTextUtf8(static_cast<int32_t>(ps.p - text.data()));
            return false;
        }
        out = std::move(j);
        return true;
    }

    // from_json / to_json: scalars, std::string and Array<T> here; the generated structs add theirs.
    inline void from_json(const Json& j, double& v) { v = j.as_number(); }
    inline void from_json(const Json& j, int32_t& v) { v = ToInteger(j.as_number()); }
    inline void from_json(const Json& j, bool& v) { v = j.as_bool(); }
    inline void from_json(const Json& j, char16_t& v)
    {
        if (!j.is_string()) std::abort();
        const auto text = StringFromUtf8(j.s);
        if (text.size() != 1) std::abort();
        v = text[0];
    }
    inline void from_json(const Json& j, String& v) { v = j.is_string() ? StringFromUtf8(j.s) : String(); }
    // Bytes use the managed Latin-1 JSON string representation; Json strings store UTF-8.
    inline void from_json(const Json& j, Bytes& value)
    {
        if (!j.is_string() || j.s.size() > static_cast<size_t>(INT32_MAX)) std::abort();
        auto* bytes = arena_alloc<uint8_t>(static_cast<int32_t>(j.s.size()));
        int32_t count = 0;
        for (size_t i = 0; i < j.s.size(); ++i)
        {
            const auto c = static_cast<uint8_t>(j.s[i]);
            if (c < 128) bytes[count++] = c;
            else
            {
                if ((c != 0xc2 && c != 0xc3) || ++i >= j.s.size()) std::abort();
                const auto next = static_cast<uint8_t>(j.s[i]);
                if (next < 0x80 || next > 0xbf) std::abort();
                bytes[count++] = static_cast<uint8_t>(((c & 3) << 6) | (next & 0x3f));
            }
        }
        value = { bytes, count };
    }

    template <class T>
    inline void from_json(const Json& j, Array<T>& a)
    {
        if (j.kind == Json::Kind::Null) { a = Array<T>{}; return; }
        if (!j.is_array()) std::abort();
        const int32_t n = j.size();
        T* p = arena_alloc<T>(n);
        for (int32_t i = 0; i < n; i++)
            from_json(j[i], p[i]);
        a = Array<T>{ p, n };
    }

    inline Json to_json(double v) { return Json::Number(v); }
    inline Json to_json(int32_t v) { return Json::Number(static_cast<double>(v)); }
    inline Json to_json(bool v) { return Json::Bool(v); }
    inline Json to_json(char16_t v) { return Json::String(StringToUtf8(String(1, v))); }
    inline Json to_json(const String& v) { return Json::String(StringToUtf8(v)); }
    inline Json to_json(const Bytes& value)
    {
        std::string utf8;
        for (int32_t i = 0; i < value.count; ++i)
        {
            const auto c = value.data[i];
            if (c >= 128) utf8.push_back(static_cast<char>(0xc0 | (c >> 6)));
            utf8.push_back(static_cast<char>(c < 128 ? c : 0x80 | (c & 0x3f)));
        }
        return Json::String(std::move(utf8));
    }
    template <class T>
    inline Json to_json(const Array<T>& a)
    {
        if (!a.has_value) return Json{};
        Json j = Json::Array();
        for (int32_t i = 0; i < a.count; i++)
            j.push(to_json(a.data[i]));
        return j;
    }
    // Missing record fields receive CLR-style typed defaults; explicit null still
    // goes through the scalar/array converter and its normal validation.
    template <class T>
    inline void from_json_member(const Json& object, const char* name, T& value)
    {
        if (const auto* field = object.find(name)) from_json(*field, value);
        else value = T{};
    }
