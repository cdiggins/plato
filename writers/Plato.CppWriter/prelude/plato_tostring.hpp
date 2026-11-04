// tostring: the invariant-culture text C# writes, and string append helpers.
// A fragment of plato_cpu.hpp: included by it, in order, inside namespace plato. Not self-contained.

    // ---- ToString: the invariant-culture text C# writes (round-trip "R" for numbers) ----

    inline std::string NumberTextUtf8(int32_t v)
    {
        char buf[16];
        auto r = std::to_chars(buf, buf + sizeof buf, v);
        return std::string(buf, r.ptr);
    }

    // Shortest round-trip digits, laid out as .NET writes "R" (checked on .NET 8, 9 and 10): plain from 1e-4 up to below 1e17, otherwise d.dddE+XX.
    inline std::string NumberTextUtf8(double v)
    {
        if (std::isnan(v)) return "NaN";
        if (std::isinf(v)) return v > 0 ? "Infinity" : "-Infinity";
        if (v == 0.0) return std::signbit(v) ? "-0" : "0";
        char buf[48];
        auto r = std::to_chars(buf, buf + sizeof buf, v, std::chars_format::scientific);
        const char* p = buf;
        const char* end = r.ptr;
        std::string out;
        if (*p == '-') { out.push_back('-'); ++p; }
        std::string digits;
        while (p < end && *p != 'e')
        {
            if (*p != '.')
                digits.push_back(*p);
            ++p;
        }
        ++p;
        bool eneg = false;
        if (p < end && (*p == '+' || *p == '-')) { eneg = (*p == '-'); ++p; }
        int e = 0;
        while (p < end) { e = e * 10 + (*p - '0'); ++p; }
        if (eneg) e = -e;
        if (e >= 17 || e < -4)
        {
            out.push_back(digits[0]);
            if (digits.size() > 1)
            {
                out.push_back('.');
                out.append(digits, 1, std::string::npos);
            }
            out.push_back('E');
            out.push_back(e < 0 ? '-' : '+');
            const int a = e < 0 ? -e : e;
            if (a < 10) out.push_back('0');
            out += NumberTextUtf8(static_cast<int32_t>(a));
        }
        else if (e >= 0)
        {
            const size_t intLen = static_cast<size_t>(e) + 1;
            if (digits.size() <= intLen)
            {
                out += digits;
                out.append(intLen - digits.size(), '0');
            }
            else
            {
                out.append(digits, 0, intLen);
                out.push_back('.');
                out.append(digits, intLen, std::string::npos);
            }
        }
        else
        {
            out += "0.";
            out.append(static_cast<size_t>(-e - 1), '0');
            out += digits;
        }
        return out;
    }

    inline String ToString(int32_t v) { return StringFromUtf8(NumberTextUtf8(v)); }
    inline String ToString(double v) { return StringFromUtf8(NumberTextUtf8(v)); }
    inline String ToString(bool v) { return v ? u"True" : u"False"; }
    inline String ToString(char16_t v) { return String(1, v); }
    inline String ToString(const String& v) { return v; }
    inline String NumberToShortestString(double v) { return ToString(v); }

    // Generated UTF-8 literals carry their byte length, including embedded NULs.
#ifdef PLATO_STRING_PROBE
    // Probe build (-DPLATO_STRING_PROBE): make_string_n evaluations, read by a single-threaded host (TKT-1159); build with /FIcstdio so fprintf is declared.
    inline long long plato_make_string_count = 0;
    struct PlatoMakeStringReport { ~PlatoMakeStringReport() { std::fprintf(stderr, "PLATO_STRING_PROBE make_string_n=%lld
", plato_make_string_count); } };
    inline PlatoMakeStringReport plato_make_string_report;
#define PLATO_STRING_PROBE_COUNT() (++::plato::plato_make_string_count)
#else
#define PLATO_STRING_PROBE_COUNT() ((void)0)
#endif
    inline String make_string_n(const char* s, int32_t n) { PLATO_STRING_PROBE_COUNT(); return StringFromUtf8(std::string(s, static_cast<size_t>(n < 0 ? 0 : n))); }
    inline void string_clear(String* s) { s->clear(); }
    inline void string_append_char(String* s, char16_t c) { s->push_back(c); }
    inline void string_append_cstr(String* s, const char* p) { if (p) s->append(StringFromUtf8(p)); }
    inline void string_append_int(String* s, int32_t v) { s->append(ToString(v)); }
    inline void string_append_float(String* s, double v) { s->append(ToString(v)); }
