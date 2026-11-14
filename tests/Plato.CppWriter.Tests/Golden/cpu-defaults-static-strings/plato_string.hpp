// string: String indexing, text forms, number and integer parsing.
// A fragment of plato_cpu.hpp: included by it, in order, inside namespace plato. Not self-contained.

    // PLATO_STR(u"text", n): a String constant of n UTF-16 code units (the writer's StaticStringLiterals policy). The
    // String views the literal itself, which has static storage: nothing is built or allocated when the site is
    // evaluated, the constant outlives every arena_reset(), and every thread may read it. The string pool's entries
    // (plato_strings.cpp) are the same kind of String. make_string_n is the form that allocates, in the arena.
#define PLATO_STR(text, n) (::plato::String(text, static_cast<size_t>(n)))

    inline char16_t* string_alloc(size_t n)
    {
        if (n > static_cast<size_t>(INT32_MAX)) std::abort();
        return arena_alloc<char16_t>(static_cast<int32_t>(n));
    }
    inline String string_copy(const char16_t* p, size_t n)
    {
        if (n == 0) return String();
        char16_t* units = string_alloc(n);
        std::memcpy(units, p, n * sizeof(char16_t));
        return String(units, n);
    }
    inline String string_copy(const String& s) { return string_copy(s.chars, s.size()); }
    inline String operator+(const String& a, const String& b)
    {
        if (a.length == 0) return b;
        if (b.length == 0) return a;
        const size_t n = a.size() + b.size();
        char16_t* units = string_alloc(n);
        std::memcpy(units, a.chars, a.size() * sizeof(char16_t));
        std::memcpy(units + a.length, b.chars, b.size() * sizeof(char16_t));
        return String(units, n);
    }

    // String.Count / At index UTF-16 code units, including surrogate halves.
    inline int32_t ToCount(const String& s) { return static_cast<int32_t>(s.size()); }
    inline char16_t At(const String& s, int32_t i) { PLATO_ASSERT(i >= 0 && static_cast<size_t>(i) < s.size()); return s[static_cast<size_t>(i)]; }

    // Character <-> Integer: the unsigned UTF-16 code unit.
    inline int32_t ToInteger(char16_t c) { return static_cast<int32_t>(c); }
    inline int32_t ToInteger(int32_t x) { return x; }
    inline char16_t ToCharacter(int32_t n) { return static_cast<char16_t>(n); }

    // FromCharacters: the String of the array's characters, in order. It views the array's elements (both are immutable
    // and live as long as each other); nothing is copied.
    inline String FromCharacters(Array<char16_t> cs) { return cs.count > 0 ? String(cs.data, static_cast<size_t>(cs.count)) : String(); }

    // Substring: the characters at start .. start + count - 1 that exist; clamped at both ends, never fails. It views
    // the units of s; nothing is copied.
    inline String Substring(const String& s, int32_t start, int32_t count)
    {
        int64_t first = start;
        int64_t n = count;
        const int64_t length = static_cast<int64_t>(s.size());
        if (first < 0) { n += first; first = 0; }
        if (n <= 0 || first >= length) return String();
        if (n > length - first) n = length - first;
        return String(s.chars + first, static_cast<size_t>(n));
    }

    // IsNumberText / NumberFromText: the P21 real forms, checked by hand (sign, digits with an optional point,
    // optional exponent; at least one digit before the exponent) and read by std::from_chars, which is correctly
    // rounded and locale-free. A value that overflows to infinity is not a number; an underflow is read by strtod,
    // which rounds a subnormal or zero correctly. The same grammar and results as the C# runtime's TextForms.
    inline bool IsRealForm(const char* t, size_t n)
    {
        size_t i = 0;
        auto digit = [&](size_t k) { return k < n && t[k] >= '0' && t[k] <= '9'; };
        if (i < n && (t[i] == '+' || t[i] == '-')) ++i;
        int digits = 0;
        while (digit(i)) { ++i; ++digits; }
        if (i < n && t[i] == '.')
        {
            ++i;
            while (digit(i)) { ++i; ++digits; }
        }
        if (digits == 0) return false;
        if (i < n && (t[i] == 'e' || t[i] == 'E'))
        {
            ++i;
            if (i < n && (t[i] == '+' || t[i] == '-')) ++i;
            int exponentDigits = 0;
            while (digit(i)) { ++i; ++exponentDigits; }
            if (exponentDigits == 0) return false;
        }
        return i == n;
    }
    inline bool IsRealForm(const std::string& t) { return IsRealForm(t.data(), t.size()); }

    inline bool TryParseNumber(const char* t, size_t n, double& value)
    {
        value = 0.0;
        if (!IsRealForm(t, n)) return false;
        const char* first = t;
        const char* last = t + n;
        const bool negative = *first == '-';
        if (*first == '+' || *first == '-') ++first;
        double v = 0.0;
        auto r = std::from_chars(first, last, v);
        if (r.ec == std::errc::result_out_of_range)
        {
            v = std::strtod(std::string(first, last).c_str(), nullptr);   // C locale: the program never calls setlocale
            if (std::isinf(v)) return false;
        }
        else if (r.ec != std::errc() || r.ptr != last)
            return false;
        value = negative ? -v : v;
        return true;
    }
    inline bool TryParseNumber(const std::string& t, double& value) { return TryParseNumber(t.data(), t.size(), value); }

    inline bool TryParseNumber(const String& t, double& value) { return TryParseNumber(StringToUtf8(t), value); }

    inline bool IsNumberText(const String& t) { double v; return TryParseNumber(t, v); }
    inline double NumberFromText(const String& t) { double v; return TryParseNumber(t, v) ? v : 0.0; }

    // IsIntegerText / IntegerFromText: an optional sign and one or more digits, within int32_t (this dialect's
    // Integer); overflow is reported, never wrapped. The same results as the C# runtime's TextForms under cpu64i32.
    inline bool TryParseInteger(const String& t, int32_t& value)
    {
        value = 0;
        const char16_t* first = t.data();
        const char16_t* last = first + t.size();
        const bool negative = first != last && *first == '-';
        if (first != last && (*first == '+' || *first == '-')) ++first;
        if (first == last) return false;
        for (const char16_t* p = first; p != last; ++p)
            if (*p < '0' || *p > '9') return false;
        int64_t v = 0;
        for (const char16_t* p = first; p != last; ++p)
        {
            v = v * 10 + (*p - '0');
            if (v > INT64_C(2147483648)) return false;
        }
        if (negative) v = -v;
        if (v < INT32_MIN || v > INT32_MAX) return false;
        value = static_cast<int32_t>(v);
        return true;
    }

    inline bool IsIntegerText(const String& t) { int32_t v; return TryParseInteger(t, v); }
    inline int32_t IntegerFromText(const String& t) { int32_t v; return TryParseInteger(t, v) ? v : 0; }
