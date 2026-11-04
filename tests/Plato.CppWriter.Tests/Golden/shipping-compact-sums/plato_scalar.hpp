// scalar: Number and Integer intrinsics.
// A fragment of plato_cpu.hpp: included by it, in order, inside namespace plato. Not self-contained.

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
    inline bool LessThanOrEquals(char16_t a, char16_t b) { return a <= b; }   // C# compares UTF-16 code units; ordinal here
    inline bool LessThanOrEquals(const String& a, const String& b) { return a.compare(b) <= 0; }   // ordinal UTF-16 code units

    inline double Floor(double x) { return std::floor(x); }
    inline double Sqrt(double x) { return std::sqrt(x); }
    inline double Cbrt(double x) { return std::cbrt(x); }   // Math.Cbrt: correctly rounded
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
