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
            => (dialect == CppDialect.Cuda ? CudaHead : CppHead) + SharedCore;

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
}
";
    }
}
