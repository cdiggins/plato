namespace Ara3D.Geometry.CppWriter
{
    /// <summary>
    /// The fixed preamble that every generated file starts with.
    ///
    /// It exists so that the EMITTED CODE IS DIALECT AGNOSTIC: the body writer always
    /// produces float2/float3/float4, make_floatN(...), native operators and plato::*
    /// math helpers, and the preamble is what makes those mean the right thing for the
    /// target. For CUDA the vector types come from cuda_runtime.h; for portable C++ the
    /// preamble declares layout-compatible ones itself.
    ///
    /// CUDA deliberately does NOT define arithmetic operators for its vector types
    /// (that is what the sample-only helper_math.h is for), so the shared core defines
    /// them here for both dialects.
    /// </summary>
    public static class CppPrelude
    {
        public static string Preamble(CppDialect dialect)
            => (dialect == CppDialect.Cuda ? CudaHead : CppHead)
               + SharedCore
               + StringAndCharacterCore
               + ArrayCore
               + StringFormatCore;

        private const string CppHead = @"#pragma once
#include <cmath>

// Every generated function is a free function; 'inline' keeps the header self-contained.
#define PLATO_FN inline

// Layout-compatible stand-ins for the CUDA vector types, so the generated code below is
// byte-for-byte the same as the CUDA output.
struct float2 { float x, y; };
struct float3 { float x, y, z; };
struct float4 { float x, y, z, w; };

PLATO_FN float2 make_float2(float x, float y) { return float2{ x, y }; }
PLATO_FN float3 make_float3(float x, float y, float z) { return float3{ x, y, z }; }
PLATO_FN float4 make_float4(float x, float y, float z, float w) { return float4{ x, y, z, w }; }
";

        private const string CudaHead = @"#pragma once
#include <cuda_runtime.h>
#include <cmath>

// Emitted functions are callable from host and device code alike.
#define PLATO_FN __host__ __device__ inline
";

        /// <summary>
        /// Vector operators + the math helpers the body writer lowers Plato intrinsics onto.
        /// Helpers live in namespace 'plato' with lowercase names so they can never collide
        /// with the emitted Plato functions (Dot, Length, Min, ... at global scope).
        /// </summary>
        private const string SharedCore = @"
// ---- Vector arithmetic (CUDA does not provide these for float2/3/4) ----
PLATO_FN float2 operator-(float2 a) { return make_float2(-a.x, -a.y); }
PLATO_FN float3 operator-(float3 a) { return make_float3(-a.x, -a.y, -a.z); }
PLATO_FN float4 operator-(float4 a) { return make_float4(-a.x, -a.y, -a.z, -a.w); }

PLATO_FN float2 operator+(float2 a, float2 b) { return make_float2(a.x + b.x, a.y + b.y); }
PLATO_FN float2 operator-(float2 a, float2 b) { return make_float2(a.x - b.x, a.y - b.y); }
PLATO_FN float2 operator*(float2 a, float2 b) { return make_float2(a.x * b.x, a.y * b.y); }
PLATO_FN float2 operator/(float2 a, float2 b) { return make_float2(a.x / b.x, a.y / b.y); }
PLATO_FN float2 operator*(float2 a, float s) { return make_float2(a.x * s, a.y * s); }
PLATO_FN float2 operator*(float s, float2 a) { return make_float2(a.x * s, a.y * s); }
PLATO_FN float2 operator/(float2 a, float s) { return make_float2(a.x / s, a.y / s); }
PLATO_FN float2 operator+(float2 a, float s) { return make_float2(a.x + s, a.y + s); }
PLATO_FN float2 operator+(float s, float2 a) { return make_float2(a.x + s, a.y + s); }
PLATO_FN float2 operator-(float2 a, float s) { return make_float2(a.x - s, a.y - s); }

PLATO_FN float3 operator+(float3 a, float3 b) { return make_float3(a.x + b.x, a.y + b.y, a.z + b.z); }
PLATO_FN float3 operator-(float3 a, float3 b) { return make_float3(a.x - b.x, a.y - b.y, a.z - b.z); }
PLATO_FN float3 operator*(float3 a, float3 b) { return make_float3(a.x * b.x, a.y * b.y, a.z * b.z); }
PLATO_FN float3 operator/(float3 a, float3 b) { return make_float3(a.x / b.x, a.y / b.y, a.z / b.z); }
PLATO_FN float3 operator*(float3 a, float s) { return make_float3(a.x * s, a.y * s, a.z * s); }
PLATO_FN float3 operator*(float s, float3 a) { return make_float3(a.x * s, a.y * s, a.z * s); }
PLATO_FN float3 operator/(float3 a, float s) { return make_float3(a.x / s, a.y / s, a.z / s); }
PLATO_FN float3 operator+(float3 a, float s) { return make_float3(a.x + s, a.y + s, a.z + s); }
PLATO_FN float3 operator+(float s, float3 a) { return make_float3(a.x + s, a.y + s, a.z + s); }
PLATO_FN float3 operator-(float3 a, float s) { return make_float3(a.x - s, a.y - s, a.z - s); }

PLATO_FN float4 operator+(float4 a, float4 b) { return make_float4(a.x + b.x, a.y + b.y, a.z + b.z, a.w + b.w); }
PLATO_FN float4 operator-(float4 a, float4 b) { return make_float4(a.x - b.x, a.y - b.y, a.z - b.z, a.w - b.w); }
PLATO_FN float4 operator*(float4 a, float4 b) { return make_float4(a.x * b.x, a.y * b.y, a.z * b.z, a.w * b.w); }
PLATO_FN float4 operator/(float4 a, float4 b) { return make_float4(a.x / b.x, a.y / b.y, a.z / b.z, a.w / b.w); }
PLATO_FN float4 operator*(float4 a, float s) { return make_float4(a.x * s, a.y * s, a.z * s, a.w * s); }
PLATO_FN float4 operator*(float s, float4 a) { return make_float4(a.x * s, a.y * s, a.z * s, a.w * s); }
PLATO_FN float4 operator/(float4 a, float s) { return make_float4(a.x / s, a.y / s, a.z / s, a.w / s); }
PLATO_FN float4 operator+(float4 a, float s) { return make_float4(a.x + s, a.y + s, a.z + s, a.w + s); }
PLATO_FN float4 operator+(float s, float4 a) { return make_float4(a.x + s, a.y + s, a.z + s, a.w + s); }
PLATO_FN float4 operator-(float4 a, float s) { return make_float4(a.x - s, a.y - s, a.z - s, a.w - s); }

PLATO_FN bool operator==(float2 a, float2 b) { return a.x == b.x && a.y == b.y; }
PLATO_FN bool operator!=(float2 a, float2 b) { return !(a == b); }
PLATO_FN bool operator==(float3 a, float3 b) { return a.x == b.x && a.y == b.y && a.z == b.z; }
PLATO_FN bool operator!=(float3 a, float3 b) { return !(a == b); }
PLATO_FN bool operator==(float4 a, float4 b) { return a.x == b.x && a.y == b.y && a.z == b.z && a.w == b.w; }
PLATO_FN bool operator!=(float4 a, float4 b) { return !(a == b); }

// ---- Math helpers the intrinsic lowering targets ----
namespace plato
{
    PLATO_FN float min_(float a, float b) { return a < b ? a : b; }
    PLATO_FN float max_(float a, float b) { return a > b ? a : b; }
    PLATO_FN int min_(int a, int b) { return a < b ? a : b; }
    PLATO_FN int max_(int a, int b) { return a > b ? a : b; }
    PLATO_FN float2 min_(float2 a, float2 b) { return make_float2(min_(a.x, b.x), min_(a.y, b.y)); }
    PLATO_FN float2 max_(float2 a, float2 b) { return make_float2(max_(a.x, b.x), max_(a.y, b.y)); }
    PLATO_FN float3 min_(float3 a, float3 b) { return make_float3(min_(a.x, b.x), min_(a.y, b.y), min_(a.z, b.z)); }
    PLATO_FN float3 max_(float3 a, float3 b) { return make_float3(max_(a.x, b.x), max_(a.y, b.y), max_(a.z, b.z)); }
    PLATO_FN float4 min_(float4 a, float4 b) { return make_float4(min_(a.x, b.x), min_(a.y, b.y), min_(a.z, b.z), min_(a.w, b.w)); }
    PLATO_FN float4 max_(float4 a, float4 b) { return make_float4(max_(a.x, b.x), max_(a.y, b.y), max_(a.z, b.z), max_(a.w, b.w)); }

    template <typename T> PLATO_FN T clamp_(T x, T lo, T hi) { return min_(max_(x, lo), hi); }
    template <typename T> PLATO_FN T mix_(T a, T b, float t) { return a * (1.0f - t) + b * t; }
    PLATO_FN float saturate_(float x) { return clamp_(x, 0.0f, 1.0f); }

    PLATO_FN float dot_(float2 a, float2 b) { return a.x * b.x + a.y * b.y; }
    PLATO_FN float dot_(float3 a, float3 b) { return a.x * b.x + a.y * b.y + a.z * b.z; }
    PLATO_FN float dot_(float4 a, float4 b) { return a.x * b.x + a.y * b.y + a.z * b.z + a.w * b.w; }

    PLATO_FN float3 cross_(float3 a, float3 b)
    { return make_float3(a.y * b.z - a.z * b.y, a.z * b.x - a.x * b.z, a.x * b.y - a.y * b.x); }

    PLATO_FN float length_(float x) { return fabsf(x); }
    PLATO_FN float length_(float2 v) { return sqrtf(dot_(v, v)); }
    PLATO_FN float length_(float3 v) { return sqrtf(dot_(v, v)); }
    PLATO_FN float length_(float4 v) { return sqrtf(dot_(v, v)); }

    template <typename T> PLATO_FN T normalize_(T v) { return v * (1.0f / length_(v)); }
    template <typename T> PLATO_FN float distance_(T a, T b) { return length_(b - a); }

    PLATO_FN float sign_(float x) { return x > 0.0f ? 1.0f : (x < 0.0f ? -1.0f : 0.0f); }
    PLATO_FN float fract_(float x) { return x - floorf(x); }
    PLATO_FN float step_(float edge, float x) { return x < edge ? 0.0f : 1.0f; }
    PLATO_FN float smoothstep_(float e0, float e1, float x)
    {
        const float t = clamp_((x - e0) / (e1 - e0), 0.0f, 1.0f);
        return t * t * (3.0f - 2.0f * t);
    }
    PLATO_FN float3 reflect_(float3 v, float3 n) { return v - n * (2.0f * dot_(v, n)); }
    PLATO_FN float2 reflect_(float2 v, float2 n) { return v - n * (2.0f * dot_(v, n)); }

    // Structural GetHashCode helpers (device-safe; not cryptographic).
    PLATO_FN int hash_float(float x) { return (int)(x * 73856093.0f) ^ (int)x; }
    PLATO_FN int mix_hash(int a, int b) { return a * 16777619 ^ b; }

    // Fixed-size value arrays for Map/Zip results whose element type is not floatN
    // (e.g. Zip→bool). Device-friendly: no heap, no std::function.
    template <typename T> struct Array1 { T e0; };
    template <typename T> struct Array2 { T e0, e1; };
    template <typename T> struct Array3 { T e0, e1, e2; };
    template <typename T> struct Array4 { T e0, e1, e2, e3; };
    template <typename T> struct Array5 { T e0, e1, e2, e3, e4; };
    template <typename T> struct Array6 { T e0, e1, e2, e3, e4, e5; };
    template <typename T> struct Array7 { T e0, e1, e2, e3, e4, e5, e6; };
    template <typename T> struct Array8 { T e0, e1, e2, e3, e4, e5, e6, e7; };

    // Identity functor for All/Any over Zip→bool results (AllZipComponents path).
    struct Id
    {
        template <typename T>
        PLATO_FN T operator()(T x) const { return x; }
    };
}
";

        /// <summary>
        /// Character + fixed-capacity String (no heap). Shared by C++ and CUDA so generated
        /// bodies stay identical. Capacity is small: enough for TypeName / short literals.
        /// </summary>
        public const string StringAndCharacterCore = @"
// ---- Character / String (Plato primitives; not std::string) ----
// Character erases to char. String is a fixed-capacity POD (device-safe, no heap).
#ifndef PLATO_STRING_CAP
#define PLATO_STRING_CAP 64
#endif
struct String
{
    char data[PLATO_STRING_CAP];
    int len;
};

PLATO_FN String make_string_n(const char* s, int n)
{
    String r;
    r.len = n < 0 ? 0 : (n >= PLATO_STRING_CAP ? PLATO_STRING_CAP - 1 : n);
    for (int i = 0; i < r.len; i++) r.data[i] = s[i];
    for (int i = r.len; i < PLATO_STRING_CAP; i++) r.data[i] = '\0';
    return r;
}

PLATO_FN bool operator==(String a, String b)
{
    if (a.len != b.len) return false;
    for (int i = 0; i < a.len; i++) if (a.data[i] != b.data[i]) return false;
    return true;
}
PLATO_FN bool operator!=(String a, String b) { return !(a == b); }
PLATO_FN bool operator<=(String a, String b)
{
    const int n = a.len < b.len ? a.len : b.len;
    for (int i = 0; i < n; i++)
    {
        if ((unsigned char)a.data[i] < (unsigned char)b.data[i]) return true;
        if ((unsigned char)a.data[i] > (unsigned char)b.data[i]) return false;
    }
    return a.len <= b.len;
}
PLATO_FN bool operator<(String a, String b) { return a <= b && !(a == b); }
PLATO_FN bool operator>=(String a, String b) { return b <= a; }
PLATO_FN bool operator>(String a, String b) { return b < a; }

PLATO_FN int HashString(String a)
{
    int h = a.len;
    for (int i = 0; i < a.len; i++) h = plato::mix_hash(h, (int)(unsigned char)a.data[i]);
    return h;
}
";

        /// <summary>
        /// Fixed-capacity dynamic Array&lt;T&gt; (Plato concrete type; interface is IArray).
        /// Same host/device story as String: inline buffer, no heap, no std::vector.
        /// Oversized Range/Map results clamp to PLATO_ARRAY_CAP.
        /// </summary>
        public const string ArrayCore = @"
// ---- Array<T> (Plato dynamic array; not std::vector) ----
// Fixed-capacity POD so C++ and CUDA bodies stay identical. Raise PLATO_ARRAY_CAP if needed.
#ifndef PLATO_ARRAY_CAP
#define PLATO_ARRAY_CAP 64
#endif
template <typename T>
struct Array
{
    T data[PLATO_ARRAY_CAP];
    int count;
};

template <typename T>
PLATO_FN Array<T> make_array_empty()
{
    Array<T> r;
    r.count = 0;
    return r;
}

template <typename T>
PLATO_FN int Count(Array<T> xs)
{
    return xs.count;
}

template <typename T>
PLATO_FN T At(Array<T> xs, int i)
{
    if (xs.count <= 0) return T{};
    if (i < 0) i = 0;
    if (i >= xs.count) i = xs.count - 1;
    return xs.data[i];
}

PLATO_FN Array<int> Range(int n)
{
    Array<int> r;
    r.count = n < 0 ? 0 : (n > PLATO_ARRAY_CAP ? PLATO_ARRAY_CAP : n);
    for (int i = 0; i < r.count; i++) r.data[i] = i;
    return r;
}

template <typename F>
PLATO_FN auto MapRange(int n, F f) -> Array<decltype(f(0))>
{
    Array<decltype(f(0))> r;
    r.count = n < 0 ? 0 : (n > PLATO_ARRAY_CAP ? PLATO_ARRAY_CAP : n);
    for (int i = 0; i < r.count; i++) r.data[i] = f(i);
    return r;
}

template <typename T, typename F>
PLATO_FN auto Map(Array<T> xs, F f) -> Array<decltype(f(xs.data[0]))>
{
    Array<decltype(f(xs.data[0]))> r;
    r.count = xs.count;
    for (int i = 0; i < r.count; i++) r.data[i] = f(xs.data[i]);
    return r;
}

template <typename T, typename... Rest>
PLATO_FN Array<T> make_array(T first, Rest... rest)
{
    const T vals[] = { first, static_cast<T>(rest)... };
    Array<T> r;
    r.count = (int)(1 + sizeof...(Rest));
    if (r.count > PLATO_ARRAY_CAP) r.count = PLATO_ARRAY_CAP;
    for (int i = 0; i < r.count; i++) r.data[i] = vals[i];
    return r;
}

template <typename T, typename U, typename F>
PLATO_FN auto Zip(Array<T> a, Array<U> b, F f) -> Array<decltype(f(a.data[0], b.data[0]))>
{
    Array<decltype(f(a.data[0], b.data[0]))> r;
    r.count = a.count < b.count ? a.count : b.count;
    for (int i = 0; i < r.count; i++) r.data[i] = f(a.data[i], b.data[i]);
    return r;
}

template <typename T, typename U, typename V, typename F>
PLATO_FN auto Zip(Array<T> a, Array<U> b, Array<V> c, F f) -> Array<decltype(f(a.data[0], b.data[0], c.data[0]))>
{
    Array<decltype(f(a.data[0], b.data[0], c.data[0]))> r;
    r.count = a.count < b.count ? a.count : b.count;
    if (c.count < r.count) r.count = c.count;
    for (int i = 0; i < r.count; i++) r.data[i] = f(a.data[i], b.data[i], c.data[i]);
    return r;
}

template <typename T, typename Acc, typename F>
PLATO_FN Acc Reduce(Array<T> xs, Acc acc, F f)
{
    for (int i = 0; i < xs.count; i++) acc = f(acc, xs.data[i]);
    return acc;
}

template <typename T, typename F>
PLATO_FN bool All(Array<T> xs, F f)
{
    for (int i = 0; i < xs.count; i++) if (!f(xs.data[i])) return false;
    return true;
}

template <typename T, typename F>
PLATO_FN bool Any(Array<T> xs, F f)
{
    for (int i = 0; i < xs.count; i++) if (f(xs.data[i])) return true;
    return false;
}

template <typename T>
PLATO_FN Array<T> Reverse(Array<T> xs)
{
    Array<T> r;
    r.count = xs.count;
    for (int i = 0; i < r.count; i++) r.data[i] = xs.data[r.count - 1 - i];
    return r;
}

template <typename T, typename F>
PLATO_FN auto FlatMap(Array<T> xs, F f) -> decltype(f(xs.data[0]))
{
    decltype(f(xs.data[0])) r;
    r.count = 0;
    for (int i = 0; i < xs.count; i++)
    {
        auto part = f(xs.data[i]);
        for (int j = 0; j < part.count && r.count < PLATO_ARRAY_CAP; j++)
            r.data[r.count++] = part.data[j];
    }
    return r;
}

template <typename T>
PLATO_FN Array<T> Concatenate(Array<T> xs, Array<T> ys)
{
    Array<T> r;
    r.count = 0;
    for (int i = 0; i < xs.count && r.count < PLATO_ARRAY_CAP; i++) r.data[r.count++] = xs.data[i];
    for (int i = 0; i < ys.count && r.count < PLATO_ARRAY_CAP; i++) r.data[r.count++] = ys.data[i];
    return r;
}

template <typename T>
PLATO_FN Array<T> Append(Array<T> xs, T value)
{
    Array<T> r = xs;
    if (r.count < PLATO_ARRAY_CAP) r.data[r.count++] = value;
    return r;
}

template <typename T>
PLATO_FN Array<T> Prepend(Array<T> xs, T value)
{
    Array<T> r;
    r.count = 0;
    if (r.count < PLATO_ARRAY_CAP) r.data[r.count++] = value;
    for (int i = 0; i < xs.count && r.count < PLATO_ARRAY_CAP; i++) r.data[r.count++] = xs.data[i];
    return r;
}

template <typename T>
PLATO_FN bool operator==(Array<T> a, Array<T> b)
{
    if (a.count != b.count) return false;
    for (int i = 0; i < a.count; i++) if (!(a.data[i] == b.data[i])) return false;
    return true;
}
template <typename T>
PLATO_FN bool operator!=(Array<T> a, Array<T> b) { return !(a == b); }

template <typename T>
PLATO_FN bool Equals(Array<T> a, Array<T> b) { return a == b; }
template <typename T>
PLATO_FN bool NotEquals(Array<T> a, Array<T> b) { return a != b; }
template <typename T>
PLATO_FN int GetHashCode(Array<T> xs)
{
    int h = xs.count;
    for (int i = 0; i < xs.count; i++) h = plato::mix_hash(h, GetHashCode(xs.data[i]));
    return h;
}
";

        /// <summary>
        /// Hand-rolled String append / number format (no iostream, no snprintf — device-safe).
        /// Used by rich ToString overloads generated in the writer.
        /// </summary>
        public const string StringFormatCore = @"
// ---- String formatting helpers (device-safe; no iostream / snprintf) ----
PLATO_FN void string_clear(String* s)
{
    s->len = 0;
    for (int i = 0; i < PLATO_STRING_CAP; i++) s->data[i] = '\0';
}

PLATO_FN void string_append_char(String* s, char c)
{
    if (s->len + 1 >= PLATO_STRING_CAP) return;
    s->data[s->len++] = c;
    s->data[s->len] = '\0';
}

PLATO_FN void string_append_cstr(String* s, const char* p)
{
    if (!p) return;
    while (*p) string_append_char(s, *p++);
}

PLATO_FN void string_append_int(String* s, int v)
{
    if (v == 0) { string_append_char(s, '0'); return; }
    if (v < 0) { string_append_char(s, '-'); v = -v; }
    char buf[12];
    int n = 0;
    while (v > 0 && n < 11) { buf[n++] = (char)('0' + (v % 10)); v /= 10; }
    while (n > 0) string_append_char(s, buf[--n]);
}

PLATO_FN void string_append_float(String* s, float v)
{
    if (v != v) { string_append_cstr(s, ""NaN""); return; }
    if (v < 0.0f) { string_append_char(s, '-'); v = -v; }
    // Cap magnitude so the fixed buffer cannot explode.
    if (v > 1.0e9f) { string_append_cstr(s, ""inf""); return; }
    int ip = (int)v;
    string_append_int(s, ip);
    string_append_char(s, '.');
    float frac = v - (float)ip;
    for (int i = 0; i < 4; i++)
    {
        frac *= 10.0f;
        int d = (int)frac;
        string_append_char(s, (char)('0' + d));
        frac -= (float)d;
    }
}
";
    }
}
