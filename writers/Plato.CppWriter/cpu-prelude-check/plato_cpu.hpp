// plato_cpu.hpp: the CPU prelude of the Plato C++ writer, dialect cpu64i32 (TKT-1097).
// Mirrors the C# runtime (Intrinsics/*.cs of Ara3D.StepBrep) name for name: Number is double,
// Integer is int32_t, Boolean is bool, Character is char, String is std::string, and an
// Array<T> is an immutable view into a thread-local bump arena. No exceptions, no RTTI, no iostream.
#pragma once

#include <cassert>
#include <charconv>
#include <cmath>
#include <cstddef>
#include <cstdint>
#include <cstdlib>
#include <cstring>
#include <limits>
#include <new>
#include <string>
#include <type_traits>
#include <vector>

#ifndef PLATO_ASSERT
#define PLATO_ASSERT(c) assert(c)
#endif

namespace plato
{
    using String = std::string;

    // ---- Arena: thread-local bump allocator, 16-byte aligned, freed only by arena_reset ----
    // Destructors never run: a non-trivial element type (a record holding a std::string) is
    // default-constructed on allocation and its heap memory, if any, is not returned on reset.

#ifdef _MSC_VER
#pragma warning(push)
#pragma warning(disable : 4324)  // padded by alignas(16): intended
#endif
    struct alignas(16) ArenaBlock
    {
        ArenaBlock* next;
        size_t cap;
        size_t used;
    };
#ifdef _MSC_VER
#pragma warning(pop)
#endif

    struct ArenaState
    {
        ArenaBlock* head = nullptr;
        ArenaBlock* tail = nullptr;
        ArenaBlock* cur = nullptr;
        size_t bytes = 0;
        size_t peak = 0;
        size_t next_block = size_t(1) << 20;
    };

    inline ArenaState& arena_state()
    {
        static thread_local ArenaState s;
        return s;
    }

    inline unsigned char* arena_alloc_bytes(size_t bytes)
    {
        bytes = (bytes + 15) & ~size_t(15);
        if (bytes == 0)
            return nullptr;
        ArenaState& s = arena_state();
        for (;;)
        {
            if (s.cur && s.cur->cap - s.cur->used >= bytes)
            {
                unsigned char* p = reinterpret_cast<unsigned char*>(s.cur) + sizeof(ArenaBlock) + s.cur->used;
                s.cur->used += bytes;
                s.bytes += bytes;
                if (s.bytes > s.peak)
                    s.peak = s.bytes;
                return p;
            }
            if (s.cur && s.cur->next)
            {
                s.cur = s.cur->next;
                continue;
            }
            size_t cap = s.next_block > bytes ? s.next_block : bytes;
            ArenaBlock* b = static_cast<ArenaBlock*>(std::malloc(sizeof(ArenaBlock) + cap));
            if (!b)
                std::abort();
            b->next = nullptr;
            b->cap = cap;
            b->used = 0;
            if (s.tail)
                s.tail->next = b;
            else
                s.head = b;
            s.tail = b;
            s.cur = b;
            if (s.next_block < (size_t(256) << 20))
                s.next_block *= 2;
        }
    }

    template <class T>
    inline T* arena_alloc(int32_t n)
    {
        static_assert(alignof(T) <= 16, "arena_alloc: over-aligned type");
        if (n <= 0)
            return nullptr;
        T* p = reinterpret_cast<T*>(arena_alloc_bytes(sizeof(T) * static_cast<size_t>(n)));
        if constexpr (!std::is_trivially_default_constructible<T>::value)
            for (int32_t i = 0; i < n; i++)
                new (p + i) T();
        return p;
    }

    inline void arena_reset()
    {
        ArenaState& s = arena_state();
        for (ArenaBlock* b = s.head; b; b = b->next)
            b->used = 0;
        s.cur = s.head;
        s.bytes = 0;
    }

    inline size_t arena_bytes() { return arena_state().bytes; }
    inline size_t arena_peak_bytes() { return arena_state().peak; }

    // ---- Array<T>: immutable view; At does not clamp (debug asserts, release trusts the index) ----

    template <class T>
    struct Array
    {
        const T* data;
        int32_t count;
        const T& operator[](int32_t i) const
        {
            PLATO_ASSERT(i >= 0 && i < count);
            return data[i];
        }
    };

    template <class T>
    inline Array<T> make_array_empty() { return Array<T>{ nullptr, 0 }; }

    template <class T>
    inline Array<T> make_array(const T* data, int32_t count) { return Array<T>{ data, count < 0 ? 0 : count }; }

    // make_array(a, b, c): a literal array, copied into the arena. Not for pointer element types
    // (that would be ambiguous with the (data, count) form above).
    template <class T, class... Rest, class = typename std::enable_if<!std::is_pointer<T>::value>::type>
    inline Array<T> make_array(T first, Rest... rest)
    {
        const int32_t n = static_cast<int32_t>(1 + sizeof...(Rest));
        T* p = arena_alloc<T>(n);
        const T vals[] = { first, static_cast<T>(rest)... };
        for (int32_t i = 0; i < n; i++)
            p[i] = vals[i];
        return Array<T>{ p, n };
    }

    template <class T> inline int32_t ToCount(Array<T> xs) { return xs.count; }
    template <class T> inline T At(Array<T> xs, int32_t i) { PLATO_ASSERT(i >= 0 && i < xs.count); return xs.data[i]; }

    // MapRange: f(0) to f(n - 1), each once, in index order; n <= 0 gives the empty array.
    template <class F>
    inline auto MapRange(int32_t n, F f) -> Array<std::decay_t<decltype(f(int32_t(0)))>>
    {
        using R = std::decay_t<decltype(f(int32_t(0)))>;
        if (n <= 0)
            return Array<R>{ nullptr, 0 };
        R* p = arena_alloc<R>(n);
        for (int32_t i = 0; i < n; i++)
            p[i] = f(i);
        return Array<R>{ p, n };
    }

    inline Array<int32_t> Range(int32_t n)
    {
        return MapRange(n, [](int32_t i) { return i; });
    }

    template <class T, class F>
    inline auto Map(Array<T> xs, F f) -> Array<std::decay_t<decltype(f(xs.data[0]))>>
    {
        using R = std::decay_t<decltype(f(xs.data[0]))>;
        R* p = arena_alloc<R>(xs.count);
        for (int32_t i = 0; i < xs.count; i++)
            p[i] = f(xs.data[i]);
        return Array<R>{ p, xs.count };
    }

    template <class T, class U, class F>
    inline auto Zip(Array<T> a, Array<U> b, F f) -> Array<std::decay_t<decltype(f(a.data[0], b.data[0]))>>
    {
        using R = std::decay_t<decltype(f(a.data[0], b.data[0]))>;
        const int32_t n = a.count < b.count ? a.count : b.count;
        R* p = arena_alloc<R>(n);
        for (int32_t i = 0; i < n; i++)
            p[i] = f(a.data[i], b.data[i]);
        return Array<R>{ p, n < 0 ? 0 : n };
    }

    template <class T, class U, class V, class F>
    inline auto Zip(Array<T> a, Array<U> b, Array<V> c, F f) -> Array<std::decay_t<decltype(f(a.data[0], b.data[0], c.data[0]))>>
    {
        using R = std::decay_t<decltype(f(a.data[0], b.data[0], c.data[0]))>;
        int32_t n = a.count < b.count ? a.count : b.count;
        if (c.count < n)
            n = c.count;
        R* p = arena_alloc<R>(n);
        for (int32_t i = 0; i < n; i++)
            p[i] = f(a.data[i], b.data[i], c.data[i]);
        return Array<R>{ p, n < 0 ? 0 : n };
    }

    template <class T, class Acc, class F>
    inline Acc Reduce(Array<T> xs, Acc acc, F f)
    {
        for (int32_t i = 0; i < xs.count; i++)
            acc = f(acc, xs.data[i]);
        return acc;
    }

    // FlatMap: f runs once per element, in order; the parts are concatenated.
    template <class T, class F>
    inline auto FlatMap(Array<T> xs, F f) -> decltype(f(xs.data[0]))
    {
        using Part = decltype(f(xs.data[0]));
        using R = std::decay_t<decltype(*f(xs.data[0]).data)>;
        Part* parts = arena_alloc<Part>(xs.count);
        int32_t total = 0;
        for (int32_t i = 0; i < xs.count; i++)
        {
            parts[i] = f(xs.data[i]);
            total += parts[i].count;
        }
        R* p = arena_alloc<R>(total);
        int32_t k = 0;
        for (int32_t i = 0; i < xs.count; i++)
            for (int32_t j = 0; j < parts[i].count; j++)
                p[k++] = parts[i].data[j];
        return Part{ p, total };
    }

    template <class T, class F>
    inline bool All(Array<T> xs, F f)
    {
        for (int32_t i = 0; i < xs.count; i++)
            if (!f(xs.data[i]))
                return false;
        return true;
    }

    template <class T, class F>
    inline bool Any(Array<T> xs, F f)
    {
        for (int32_t i = 0; i < xs.count; i++)
            if (f(xs.data[i]))
                return true;
        return false;
    }

    template <class T>
    inline Array<T> Reverse(Array<T> xs)
    {
        T* p = arena_alloc<T>(xs.count);
        for (int32_t i = 0; i < xs.count; i++)
            p[i] = xs.data[xs.count - 1 - i];
        return Array<T>{ p, xs.count };
    }

    template <class T>
    inline Array<T> Concatenate(Array<T> xs, Array<T> ys)
    {
        const int32_t n = xs.count + ys.count;
        T* p = arena_alloc<T>(n);
        for (int32_t i = 0; i < xs.count; i++)
            p[i] = xs.data[i];
        for (int32_t i = 0; i < ys.count; i++)
            p[xs.count + i] = ys.data[i];
        return Array<T>{ p, n };
    }

    template <class T>
    inline Array<T> Append(Array<T> xs, typename std::common_type<T>::type x)
    {
        T* p = arena_alloc<T>(xs.count + 1);
        for (int32_t i = 0; i < xs.count; i++)
            p[i] = xs.data[i];
        p[xs.count] = x;
        return Array<T>{ p, xs.count + 1 };
    }

    template <class T>
    inline Array<T> Prepend(Array<T> xs, typename std::common_type<T>::type x)
    {
        T* p = arena_alloc<T>(xs.count + 1);
        p[0] = x;
        for (int32_t i = 0; i < xs.count; i++)
            p[i + 1] = xs.data[i];
        return Array<T>{ p, xs.count + 1 };
    }

    // Slice, Take and Skip share the storage (the view is immutable); out-of-range bounds clamp.
    template <class T>
    inline Array<T> Slice(Array<T> xs, int32_t start, int32_t count)
    {
        if (start < 0) start = 0;
        if (start > xs.count) start = xs.count;
        if (count < 0) count = 0;
        if (count > xs.count - start) count = xs.count - start;
        return Array<T>{ xs.data + start, count };
    }
    template <class T> inline Array<T> Take(Array<T> xs, int32_t n) { return Slice(xs, 0, n); }
    template <class T> inline Array<T> Skip(Array<T> xs, int32_t n) { return Slice(xs, n, xs.count); }

    template <class T>
    inline bool operator==(Array<T> a, Array<T> b)
    {
        if (a.count != b.count)
            return false;
        for (int32_t i = 0; i < a.count; i++)
            if (!(a.data[i] == b.data[i]))
                return false;
        return true;
    }
    template <class T> inline bool operator!=(Array<T> a, Array<T> b) { return !(a == b); }

    template <class T>
    inline int32_t IndexOf(Array<T> xs, typename std::common_type<T>::type x)
    {
        for (int32_t i = 0; i < xs.count; i++)
            if (xs.data[i] == x)
                return i;
        return -1;
    }
    template <class T>
    inline bool Contains(Array<T> xs, typename std::common_type<T>::type x) { return IndexOf(xs, x) >= 0; }

    // ---- Builders (unique List<T> and Buffer<T>): arena-backed; the old value is dead after Add/Set ----

    template <class T>
    struct PlatoList
    {
        T* items;
        int32_t count;
        int32_t cap;
    };
    template <class T> using List = PlatoList<T>;

    template <class T> inline PlatoList<T> EmptyList(Array<T>) { return PlatoList<T>{ nullptr, 0, 0 }; }
    template <class T> inline int32_t ToCount(PlatoList<T> l) { return l.count; }
    template <class T> inline T At(PlatoList<T> l, int32_t i) { PLATO_ASSERT(i >= 0 && i < l.count); return l.items[i]; }

    template <class T>
    inline PlatoList<T> list_reserve(PlatoList<T> l, int32_t need)
    {
        if (need <= l.cap)
            return l;
        int32_t cap = l.cap == 0 ? 4 : l.cap * 2;
        if (cap < need)
            cap = need;
        T* p = arena_alloc<T>(cap);
        for (int32_t i = 0; i < l.count; i++)
            p[i] = l.items[i];
        return PlatoList<T>{ p, l.count, cap };
    }

    template <class T>
    inline PlatoList<T> Add(PlatoList<T> l, typename std::common_type<T>::type x)
    {
        l = list_reserve(l, l.count + 1);
        l.items[l.count++] = x;
        return l;
    }

    template <class T>
    inline PlatoList<T> AddRange(PlatoList<T> l, Array<T> xs)
    {
        l = list_reserve(l, l.count + xs.count);
        for (int32_t i = 0; i < xs.count; i++)
            l.items[l.count + i] = xs.data[i];
        l.count += xs.count;
        return l;
    }

    template <class T>
    inline PlatoList<T> Set(PlatoList<T> l, int32_t i, typename std::common_type<T>::type x)
    {
        PLATO_ASSERT(i >= 0 && i < l.count);
        l.items[i] = x;
        return l;
    }

    template <class T> inline Array<T> Freeze(PlatoList<T> l) { return Array<T>{ l.items, l.count }; }

    template <class T>
    struct PlatoBuffer
    {
        T* items;
        int32_t count;
    };
    template <class T> using Buffer = PlatoBuffer<T>;

    // new Buffer<T>(n): n slots, value-initialised (zero for scalars) until written.
    template <class T>
    inline PlatoBuffer<T> make_buffer(int32_t n)
    {
        if (n < 0)
            n = 0;
        T* p = arena_alloc<T>(n);
        for (int32_t i = 0; i < n; i++)
            new (p + i) T();
        return PlatoBuffer<T>{ p, n };
    }
    template <class T> inline int32_t ToCount(PlatoBuffer<T> b) { return b.count; }
    template <class T> inline T At(PlatoBuffer<T> b, int32_t i) { PLATO_ASSERT(i >= 0 && i < b.count); return b.items[i]; }
    template <class T>
    inline PlatoBuffer<T> Set(PlatoBuffer<T> b, int32_t i, typename std::common_type<T>::type x)
    {
        PLATO_ASSERT(i >= 0 && i < b.count);
        b.items[i] = x;
        return b;
    }
    template <class T> inline Array<T> Freeze(PlatoBuffer<T> b) { return Array<T>{ b.items, b.count }; }

    // ---- Scalar intrinsics (stdlib/foundation/intrinsics.library.plato) ----

    inline double Add(double a, double b) { return a + b; }
    inline double Subtract(double a, double b) { return a - b; }
    inline double Multiply(double a, double b) { return a * b; }
    inline double Divide(double a, double b) { return a / b; }
    inline double Modulo(double a, double b) { return std::fmod(a, b); }  // C# %: sign of the dividend, same as fmod
    inline double Negative(double a) { return -a; }
    inline bool LessThanOrEquals(double a, double b) { return a <= b; }
    inline bool Equals(double a, double b) { return a == b; }

    inline int32_t Add(int32_t a, int32_t b) { return static_cast<int32_t>(static_cast<uint32_t>(a) + static_cast<uint32_t>(b)); }
    inline int32_t Subtract(int32_t a, int32_t b) { return static_cast<int32_t>(static_cast<uint32_t>(a) - static_cast<uint32_t>(b)); }
    inline int32_t Multiply(int32_t a, int32_t b) { return static_cast<int32_t>(static_cast<uint32_t>(a) * static_cast<uint32_t>(b)); }
    inline int32_t Divide(int32_t a, int32_t b) { return a / b; }   // truncates like C#; b == 0 is the caller's bug
    inline int32_t Modulo(int32_t a, int32_t b) { return a % b; }   // sign of the dividend like C#
    inline int32_t Negative(int32_t a) { return static_cast<int32_t>(0u - static_cast<uint32_t>(a)); }
    inline bool LessThanOrEquals(int32_t a, int32_t b) { return a <= b; }
    inline bool Equals(int32_t a, int32_t b) { return a == b; }
    inline int32_t BitwiseAnd(int32_t a, int32_t b) { return a & b; }
    inline int32_t BitwiseOr(int32_t a, int32_t b) { return a | b; }
    inline int32_t BitwiseXor(int32_t a, int32_t b) { return a ^ b; }
    inline int32_t BitwiseNot(int32_t a) { return ~a; }
    // C# masks the shift count to 5 bits; >> on int is arithmetic.
    inline int32_t ShiftLeft(int32_t x, int32_t bits) { return static_cast<int32_t>(static_cast<uint32_t>(x) << (bits & 31)); }
    inline int32_t ShiftRight(int32_t x, int32_t bits) { return x >> (bits & 31); }
    inline double ToNumber(int32_t x) { return static_cast<double>(x); }
    inline int32_t Hash(int32_t x) { return x; }

    inline bool And(bool a, bool b) { return a && b; }
    inline bool Or(bool a, bool b) { return a || b; }
    inline bool Not(bool a) { return !a; }
    inline bool LessThanOrEquals(char a, char b) { return a <= b; }   // C# compares UTF-16 code units; ordinal here
    inline bool LessThanOrEquals(const std::string& a, const std::string& b) { return a.compare(b) <= 0; }   // C# is culture-sensitive; ordinal here

    inline double Floor(double x) { return std::floor(x); }
    inline double Sqrt(double x) { return std::sqrt(x); }
    inline double Exp(double x) { return std::exp(x); }
    inline double NaturalLog(double x) { return std::log(x); }
    inline double Sin(double x) { return std::sin(x); }
    inline double Cos(double x) { return std::cos(x); }
    inline double Pow(double x, double p) { return std::pow(x, p); }
    inline double FusedMultiplyAdd(double x, double y, double z) { return std::fma(x, y, z); }
    inline double Atan2Radians(double y, double x) { return std::atan2(y, x); }   // Math.Atan2(self, x)
    // The `_`-receiver constants of Number.cs and Integer.cs: Type.Pi reads Pi(tag), the tag only selects the type.
    // Epsilon is System.Double.Epsilon (the smallest denormal), not DBL_EPSILON.
    inline double Pi(double) { return 3.14159265358979323846; }
    inline double Epsilon(double) { return std::numeric_limits<double>::denorm_min(); }
    inline double MaxValue(double) { return std::numeric_limits<double>::max(); }
    inline double MinValue(double) { return std::numeric_limits<double>::lowest(); }
    inline int32_t MaxValue(int32_t) { return std::numeric_limits<int32_t>::max(); }
    inline int32_t MinValue(int32_t) { return std::numeric_limits<int32_t>::lowest(); }
    inline bool IsNaN(double x) { return std::isnan(x); }
    inline bool IsInfinite(double x) { return std::isinf(x); }

    // Math.Round(x, digits): ToEven (nearbyint under the default rounding mode, not C's round);
    // digits is clamped to 0..15, and as in .NET a magnitude of 1e16 or more is returned unchanged.
    inline double ToRound(double x, int32_t digits)
    {
        if (digits < 0) digits = 0;
        if (digits > 15) digits = 15;
        if (std::fabs(x) < 1e16)
        {
            static const double p10[16] = { 1e0, 1e1, 1e2, 1e3, 1e4, 1e5, 1e6, 1e7, 1e8, 1e9, 1e10, 1e11, 1e12, 1e13, 1e14, 1e15 };
            const double p = p10[digits];
            x *= p;
            x = std::nearbyint(x);
            x /= p;
        }
        return x;
    }

    // (int)double in C# (.NET 9 and later): saturating, NaN is 0.
    inline int32_t ToInteger(double x)
    {
        if (x != x) return 0;
        if (x >= 2147483647.0) return INT32_MAX;
        if (x <= -2147483648.0) return INT32_MIN;
        return static_cast<int32_t>(x);
    }

    // Double.GetHashCode: the 64 bits folded to 32, with NaN and both zeros normalised.
    inline int32_t Hash(double x)
    {
        int64_t bits;
        std::memcpy(&bits, &x, sizeof bits);
        const int64_t mag = bits & INT64_C(0x7FFFFFFFFFFFFFFF);
        if (mag == 0 || mag > INT64_C(0x7FF0000000000000))
            bits &= INT64_C(0x7FF0000000000000);
        return static_cast<int32_t>(bits) ^ static_cast<int32_t>(bits >> 32);
    }

    // String.Count / At (Character is char; a String is UTF-8 here, UTF-16 in C#, identical for ASCII).
    inline int32_t ToCount(const std::string& s) { return static_cast<int32_t>(s.size()); }
    inline char At(const std::string& s, int32_t i) { PLATO_ASSERT(i >= 0 && static_cast<size_t>(i) < s.size()); return s[static_cast<size_t>(i)]; }

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
    inline int32_t GetHashCode(char x) { return static_cast<int32_t>(static_cast<unsigned char>(x)); }
    inline int32_t GetHashCode(double x) { return Hash(x); }
    inline int32_t HashString(const std::string& s)
    {
        int32_t h = static_cast<int32_t>(s.size());
        for (char c : s)
            h = mix_hash(h, static_cast<int32_t>(static_cast<unsigned char>(c)));
        return h;
    }
    inline int32_t GetHashCode(const std::string& s) { return HashString(s); }
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

    // ---- ToString: the invariant-culture text C# writes (round-trip "R" for numbers) ----

    inline std::string ToString(int32_t v)
    {
        char buf[16];
        auto r = std::to_chars(buf, buf + sizeof buf, v);
        return std::string(buf, r.ptr);
    }

    // Shortest round-trip digits, laid out as .NET does: plain below 1e15 and from 1e-4, otherwise d.dddE+XX.
    inline std::string ToString(double v)
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
        if (e >= 15 || e < -4)
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
            out += ToString(static_cast<int32_t>(a));
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

    inline std::string ToString(bool v) { return v ? "True" : "False"; }   // C# bool.ToString
    inline std::string ToString(char v) { return std::string(1, v); }
    inline std::string ToString(const std::string& v) { return v; }

    // ---- String helpers (std::string replaces the fixed-capacity GPU String) ----

    inline std::string make_string_n(const char* s, int32_t n) { return std::string(s, static_cast<size_t>(n < 0 ? 0 : n)); }
    inline void string_clear(std::string* s) { s->clear(); }
    inline void string_append_char(std::string* s, char c) { s->push_back(c); }
    inline void string_append_cstr(std::string* s, const char* p) { if (p) s->append(p); }
    inline void string_append_int(std::string* s, int32_t v) { s->append(ToString(v)); }
    inline void string_append_float(std::string* s, double v) { s->append(ToString(v)); }

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
            if (std::isfinite(j.n)) out += ToString(j.n);
            else json_write_string(out, ToString(j.n));
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
            if (error) *error = "invalid JSON at offset " + ToString(static_cast<int32_t>(ps.p - text.data()));
            return false;
        }
        out = std::move(j);
        return true;
    }

    // from_json / to_json: scalars, std::string and Array<T> here; the generated structs add theirs.
    inline void from_json(const Json& j, double& v) { v = j.as_number(); }
    inline void from_json(const Json& j, int32_t& v) { v = ToInteger(j.as_number()); }
    inline void from_json(const Json& j, bool& v) { v = j.as_bool(); }
    inline void from_json(const Json& j, char& v) { v = j.is_string() && !j.s.empty() ? j.s[0] : '\0'; }
    inline void from_json(const Json& j, std::string& v) { v = j.is_string() ? j.s : std::string(); }
    template <class T>
    inline void from_json(const Json& j, Array<T>& a)
    {
        const int32_t n = j.is_array() ? j.size() : 0;
        T* p = arena_alloc<T>(n);
        for (int32_t i = 0; i < n; i++)
            from_json(j[i], p[i]);
        a = Array<T>{ p, n };
    }

    inline Json to_json(double v) { return Json::Number(v); }
    inline Json to_json(int32_t v) { return Json::Number(static_cast<double>(v)); }
    inline Json to_json(bool v) { return Json::Bool(v); }
    inline Json to_json(char v) { return Json::String(std::string(1, v)); }
    inline Json to_json(const std::string& v) { return Json::String(v); }
    template <class T>
    inline Json to_json(const Array<T>& a)
    {
        Json j = Json::Array();
        for (int32_t i = 0; i < a.count; i++)
            j.push(to_json(a.data[i]));
        return j;
    }
}
