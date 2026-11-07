// helpers: names the body writer emits, and structural hashing.
// A fragment of plato_cpu.hpp: included by it, in order, inside namespace plato. Not self-contained.

    // ---- Helpers the body writer emits (names as in the GPU prelude, on double) ----

    inline double min_(double a, double b) { return a < b ? a : b; }
    inline double max_(double a, double b) { return a > b ? a : b; }
    inline int32_t min_(int32_t a, int32_t b) { return a < b ? a : b; }
    inline int32_t max_(int32_t a, int32_t b) { return a > b ? a : b; }
    template <class T> inline T clamp_(T x, T lo, T hi) { return min_(max_(x, lo), hi); }
    template <class T> inline T mix_(T a, T b, double t) { return a * (1.0 - t) + b * t; }
    inline double saturate_(double x) { return clamp_(x, 0.0, 1.0); }
    inline double dot_(double a, double b) { return a * b; }
    inline double length_(double x) { return std::fabs(x); }
    inline double sign_(double x) { return x > 0.0 ? 1.0 : (x < 0.0 ? -1.0 : 0.0); }
    inline double fract_(double x) { return x - std::floor(x); }
    inline double step_(double edge, double x) { return x < edge ? 0.0 : 1.0; }
    inline double smoothstep_(double e0, double e1, double x)
    {
        const double t = clamp_((x - e0) / (e1 - e0), 0.0, 1.0);
        return t * t * (3.0 - 2.0 * t);
    }

    // ---- Hashing (structural; not .NET's randomised HashCode, so values differ from C# by design) ----

    inline int32_t mix_hash(int32_t a, int32_t b)
    {
        return static_cast<int32_t>((static_cast<uint32_t>(a) * 16777619u) ^ static_cast<uint32_t>(b));
    }
    inline int32_t hash_float(double x) { return Hash(x); }
    inline int32_t GetHashCode(int32_t x) { return x; }
    inline int32_t GetHashCode(bool x) { return x ? 1 : 0; }
    inline int32_t GetHashCode(char16_t x) { return static_cast<int32_t>(x); }
    inline int32_t GetHashCode(double x) { return Hash(x); }
    inline int32_t HashString(const String& s)
    {
        int32_t h = static_cast<int32_t>(s.size());
        for (char16_t c : s)
            h = mix_hash(h, static_cast<int32_t>(c));
        return h;
    }
    inline int32_t GetHashCode(const String& s) { return HashString(s); }
    template <class T>
    inline int32_t GetHashCode(Array<T> xs)
    {
        int32_t h = xs.count;
        for (int32_t i = 0; i < xs.count; i++)
            h = mix_hash(h, GetHashCode(xs.data[i]));
        return h;
    }
    inline int32_t CombineHashCodes() { return 17; }
    template <class T, class... Rest>
    inline int32_t CombineHashCodes(const T& first, const Rest&... rest)
    {
        return mix_hash(CombineHashCodes(rest...), GetHashCode(first));
    }
