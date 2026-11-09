// bytes: the Bytes view and its intrinsics.
// A fragment of plato_cpu.hpp: included by it, in order, inside namespace plato. Not self-contained.

    // ---- Bytes: a view of bytes, the span of the Bytes primitive (TKT-1143) ----
    // A pointer and a count. The host owns the bytes a Bytes views and must keep them alive while it does (a file
    // read into a buffer, wrapped with make_bytes); FromIntegers allocates in the arena, as Array does. SubBytes
    // makes another view of the same bytes and never copies. The order is ordinal on unsigned bytes, as in C#.
    struct Bytes
    {
        const uint8_t* data = nullptr;
        int32_t count = 0;
    };

    inline Bytes make_bytes(const void* data, int32_t count) { return Bytes{ static_cast<const uint8_t*>(data), count < 0 ? 0 : count }; }
    inline int32_t ToCount(Bytes b) { return b.count; }
    inline int32_t At(Bytes b, int32_t i) { PLATO_ASSERT(i >= 0 && i < b.count); return static_cast<int32_t>(b.data[i]); }

    // SubBytes: the bytes at start .. start + count - 1 that exist; clamped at both ends, never fails, never copies.
    inline Bytes SubBytes(Bytes b, int32_t start, int32_t count)
    {
        int64_t first = start;
        int64_t n = count;
        if (first < 0) { n += first; first = 0; }
        if (n <= 0 || first >= b.count) return Bytes{};
        if (n > b.count - first) n = b.count - first;
        return Bytes{ b.data + first, static_cast<int32_t>(n) };
    }

    // FromIntegers: the integers in order, each reduced to its low 8 bits, in the arena.
    inline Bytes FromIntegers(Array<int32_t> xs)
    {
        if (xs.count <= 0) return Bytes{};
        uint8_t* p = arena_alloc<uint8_t>(xs.count);
        for (int32_t i = 0; i < xs.count; i++) p[i] = static_cast<uint8_t>(xs.data[i]);
        return Bytes{ p, xs.count };
    }

    inline int32_t CompareBytes_(Bytes a, Bytes b)
    {
        const int32_t n = a.count < b.count ? a.count : b.count;
        const int c = n > 0 ? std::memcmp(a.data, b.data, static_cast<size_t>(n)) : 0;   // memcmp compares unsigned bytes
        return c != 0 ? (c < 0 ? -1 : 1) : (a.count < b.count ? -1 : (a.count > b.count ? 1 : 0));
    }
    inline bool operator==(Bytes a, Bytes b) { return a.count == b.count && (a.count == 0 || std::memcmp(a.data, b.data, static_cast<size_t>(a.count)) == 0); }
    inline bool operator!=(Bytes a, Bytes b) { return !(a == b); }
    inline bool LessThanOrEquals(Bytes a, Bytes b) { return CompareBytes_(a, b) <= 0; }

    // Hash: 32-bit FNV-1a; the bits are read as the signed Integer of this dialect, as C# does under cpu64i32.
    inline int32_t Hash(Bytes b)
    {
        uint32_t h = 2166136261u;
        for (int32_t i = 0; i < b.count; i++) h = (h ^ b.data[i]) * 16777619u;
        return static_cast<int32_t>(h);
    }

    // NumberFromBytes: the correctly rounded Number the bytes spell, or NaN when they are not in the P21 real forms
    // or the value overflows to infinity. The form is the one IsRealForm checks, over bytes.
    inline bool TryParseNumberBytes(Bytes t, double& value) { return TryParseNumber(reinterpret_cast<const char*>(t.data), static_cast<size_t>(t.count), value); }
    inline double NumberFromBytes(Bytes b) { double v; return TryParseNumberBytes(b, v) ? v : std::numeric_limits<double>::quiet_NaN(); }

    // Each byte maps to the same-valued Latin-1 UTF-16 code unit (never UTF-8 decoding).
    inline String ToText(Bytes b)
    {
        String text;
        text.reserve(static_cast<size_t>(b.count));
        for (int32_t i = 0; i < b.count; ++i) text.push_back(static_cast<char16_t>(b.data[i]));
        return text;
    }
