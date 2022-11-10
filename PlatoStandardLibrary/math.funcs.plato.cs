using System;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace Plato.__FUNCS__
{
    class Constants
    {
        public const double Tolerance = double.Epsilon * 10;
        public const double Pi = Math.PI;
        public const double TwoPi = Pi * 2;
        public const double HalfPi = Pi / 2;
    }

    class Funcs
    {
        Angle Acos(double x) => Math.Acos(x);
        Angle Asin(double x) => Math.Asin(x);
        Angle Atan(double x) => Math.Atan(x);
        
        double Cos(Angle x) => Math.Cos(x);
        double Cosh(Angle x) => Math.Cosh(x);
        double Sin(Angle x) => Math.Sin(x);
        double Sinh(Angle x) => Math.Sinh(x);
        double Tan(Angle x) => Math.Tan(x);
        double Tanh(Angle x) => Math.Tanh(x);

        double Abs(double x) => Math.Abs(x);
        double Exp(double x) => Math.Exp(x);
        double Log(double x) => Math.Log(x);
        double Log10(double x) => Math.Log10(x);
        double Sqrt(double x) => Math.Sqrt(x);
        double Pow2(double x) => x * x;
        double Pow3(double x) => x * x * x;
        double Pow4(double x) => x * x * x * x;
        double Pow5(double x) => x * x * x * x * x;
        double Pow(double x, double y) => Math.Pow(x, y);

        int Sign(double x) => x > 0 ? 1 : x < 0 ? -1 : 0;
        double Magnitude(double x) => x;
        double MagnitudeSquared(double x) => x * x;
        double Inverse(double x) => 1 / x;
        double Truncate(double x) => Math.Truncate(x);
        double Ceiling(double x) => Math.Ceiling(x);
        double Floor(double x) => Math.Floor(x);
        double Round(double x) => System.Math.Round(x);
        double Distance(double v1, double v2) => (v1 - v2).Abs();
        
        bool IsInfinity(double v) => double.IsInfinity(v);
        bool IsNaN(double v) => double.IsNaN(v);
        bool AlmostEquals(double v1, double v2, double tolerance = Constants.Tolerance) => (v2 - v1).AlmostZero(tolerance);
        bool AlmostZero(double v, double tolerance = Constants.Tolerance) => v.Abs() < tolerance;
        bool AlmostOne(double v, double tolerance = Constants.Tolerance) => (1 - v.Abs()) < tolerance;
        double Smoothstep(double v) => v * v * (3 - 2 * v);

        double Lerp(double v1, double v2, double t) => v1 + (v2 - v1) * t;
        double Mix(double v1, double v2, double t) => Lerp(v1, v2, t);
        double InverseLerp(double v, double a, double b) => (v - a) / (b - a);
        double Unmix(double v, double a, double b) => InverseLerp(v, a, b);
        double LerpPrecise(double v1, double v2, double t) => ((1 - t) * v1) + (v2 * t);
        double ClampLower(double v, double min) => Max(v, min);
        double ClampUpper(double v, double max) => Min(v, max);
        double Clamp(double v, double min, double max) => v.ClampUpper(max).ClampLower(min);
        double Average(double v1, double v2) => v1.Lerp(v2, 0.5);
        double Barycentric(double v1, double v2, double v3, double u, double v) => v1 + (v2 - v1) * u + (v3 - v1) * v;

        bool Within(double v, double min, double max) => v >= min && v < max;
        double Min(double v1, double v2) => Math.Min(v1, v2);
        double Max(double v1, double v2) => Math.Max(v1, v2);

        //===
        // Signal or Shaping functions
        // https://iquilezles.org/articles/functions/
        // https://thebookofshaders.com/05/

        double ExponentialImpulse(double x, double k) => k * x * (1.0 - k * x).Exp();
        double QuadraticImpulse(double x, double k) => 2.0 * k.Sqrt() * x / (1.0 + k * x * x);
        double PolynomalialImpulse(double x, double k, double n) => (n/(n-1.0))*Pow((n-1.0)*k,1.0/n)*x/(1.0+k*Pow(x, n));
        double NormalizedPowerCurve(double x, double a, double b)  => Pow(a + b, a + b) / (Pow(a, a) * Pow(b, b)) * UnnormalizedPowerCurve(x, a, b);
        double UnnormalizedPowerCurve(double x, double a, double b) => Pow(x, a) * Pow(1.0 - x, b);
        double Parabola(double x, double k) => Pow(4.0 * x * (1.0 - x), k);
        double Sinc(double x, double k) { var a = Constants.Pi * k * (x - 1.0); return Sin(a) / a; }
        double Gain(double x, double k) { var a = 0.5 * Pow(2.0 * ((x < 0.5) ? x : 1.0 - x), k); return (x < 0.5) ? a : 1.0 - a; }
        double ExponentialStep(double x, double k, double n) => Exp(-k * Pow(x, n));
        double AlmostIdentityCubic(double x, double m, double n) { if (x > m) return x; var a = 2.0 * n - m; var b = 2.0 * m - 3.0 * n; var t = x / m; return (a * t + b) * t * t + n; }
        double AlmostIdentitySqrt(double x, double m, double n) => Sqrt(x * x+n);
        double AlmostUnityIdentity(double x) => x * x * (2.0-x);
        double IntegralSmoothstep(double x, double t) => (x > t) ? x - t / 2.0 : Pow3(x) * (1.0 - x * 0.5 / t) / t / t;
        
        //===
        // http://www.flong.com/archive/texts/code/shapers_circ/

        double CircleSigmoid(double x, double a = 0.5)=> x <= a ? a - Sqrt(Pow2(a) - Pow2(x)) : a + Sqrt(Pow2(1 - a) - Pow2(x - 1));
        double CircularSeat(double x, double a = 0.5) => x<=a ? Sqrt(Pow2(a) - Pow2(x-a)) : Sqrt(Pow2(1-a) - Pow2(x-a));
        double EllipticalSeat(double x, double a, double b) => (AlmostZero(a) || AlmostOne(a)) ? a : (x <= a) ? (b / a) * Sqrt(Pow2(a) - Pow2(x - a)) : 1 - ((1 - b) / (1 - a)) * Sqrt(Pow2(1 - a) - Pow2(x - a));
        double EllipticalSigmoid(double x, double a, double b) => (AlmostZero(a) || AlmostOne(a)) ? a : (x<=a) ? b* (1 - (Sqrt(Pow2(a) - Pow2(x))/a)) : b + ((1-b)/(1-a))* Sqrt(Pow2(1-a) - Pow2(x-1));

        //===
        // Easings.cs
        // https://easings.net/
        // https://github.com/acron0/Easings/blob/master/Easings.cs

        Func<double, double> EaseInOutFunc(Func<double, double> easeIn, Func<double, double> easeOut) => p => p < 0.5 ? 0.5 * easeIn(p * 2) : 0.5 * easeOut(p * 2 - 1) + 0.5;
        Func<double, double> EaseInOutFunc(Func<double, double> easeIn) => EaseInOutFunc(easeIn, InvertEaseFunc(easeIn));
        Func<double, double> InvertEaseFunc(Func<double, double> easeIn) => p => 1 - easeIn(1 - p);
        
        double Linear(double p) => p;
        double QuadraticEaseIn(double p) => Pow2(p);
        double QuadraticEaseOut(double p) => InvertEaseFunc(QuadraticEaseIn)(p);
        double QuadraticEaseInOut(double p) => EaseInOutFunc(QuadraticEaseIn)(p);
        double CubicEaseIn(double p) => Pow3(p);
        double CubicEaseOut(double p) => InvertEaseFunc(CubicEaseIn)(p);
        double CubicEaseInOut(double p) => EaseInOutFunc(CubicEaseIn)(p);
        double QuarticEaseIn(double p) => Pow4(p);
        double QuarticEaseOut(double p) => InvertEaseFunc(QuarticEaseIn)(p);
        double QuarticEaseInOut(double p) => EaseInOutFunc(QuarticEaseIn)(p);
        double QuinticEaseIn(double p) => Pow5(p);
        double QuinticEaseOut(double p) => InvertEaseFunc(QuinticEaseIn)(p);
        double QuinticEaseInOut(double p) => EaseInOutFunc(QuinticEaseIn)(p);
        double SineEaseIn(double p) => InvertEaseFunc(SineEaseOut)(p);
        double SineEaseOut(double p) => Sin(p * Constants.HalfPi);
        double SineEaseInOut(double p) => EaseInOutFunc(SineEaseIn, SineEaseOut)(p);
        double CircularEaseIn(double p) => 1 - Sqrt(1 - Pow2(p));
        double CircularEaseOut(double p) => InvertEaseFunc(CircularEaseIn)(p);
        double CircularEaseInOut(double p) => EaseInOutFunc(CircularEaseIn, CircularEaseOut)(p);
        double ExponentialEaseIn(double p) => AlmostZero(p) ? p : Pow(2, 10 * (p - 1));
        double ExponentialEaseOut(double p) => InvertEaseFunc(ExponentialEaseIn)(p);
        double ExponentialEaseInOut(double p) => EaseInOutFunc(ExponentialEaseIn)(p);
        double ElasticEaseIn(double p) => Sin(13 * Constants.HalfPi * p) * Pow(2, 10 * (p - 1));
        double ElasticEaseOut(double p) => InvertEaseFunc(ElasticEaseIn)(p);
        double ElasticEaseInOut(double p) => EaseInOutFunc(ElasticEaseIn)(p);
        double BackEaseIn(double p) => Pow3(p) - p * Sin(p * Constants.Pi);
        double BackEaseOut(double p) => InvertEaseFunc(BackEaseIn)(p);
        double BackEaseInOut(double p) => EaseInOutFunc(BackEaseIn)(p);
        double BounceEaseIn(double p) => InvertEaseFunc(BounceEaseIn)(p);
        double BounceEaseOut(double p)
        {
            if (p < 4 / 11.0) return 121.0 * Pow2(p) / 16.0;
            if (p < 8 / 11.0) return 363.0 / 40.0 * Pow2(p) - 99.0 / 10.0 * p + 17.0 / 5.0;
            if (p < 9 / 10.0) return 4356.0 / 361.0 * Pow2(p) - 35442.0 / 1805.0 * p + 16061.0 / 1805.0;
            return 54.0 / 5.0 * Pow2(p) - 513.0 / 25.0 * p + 268.0 / 25.0;
        }
        double BounceEaseInOut(double p) => EaseInOutFunc(BounceEaseIn, BounceEaseOut)(p);

        // https://en.wikipedia.org/wiki/Catenary
        double Caternay(double x, double a = 1.0) => Cosh(x / a);

        //== 
        // Some parametric curves 
        // https://en.wikipedia.org/wiki/Epitrochoid
        // https://en.wikipedia.org/wiki/Hypotrochoid 
        // https://en.wikipedia.org/wiki/Hypocycloid
        // Cyclocycloid

        Double2 Circle(double t) 
            => (Cos(t), Sin(t));

        Double2 Lissajous(Angle t, double a, double b, double kx, double ky) 
            => (a * Cos(kx * t), 
                b * Sin(ky * t));
        
        Double2 Hypotrochoid(Angle t, double r, double d) 
            => ((1 - r) * Cos(t) + d * Cos(t - 1), 
                (1 - r) * Sin(t) + d * Sin(t - 1));

        Double2 Epicycloid(Angle t, double r, double k)
            => (r * (k + 1) * Cos(t) - r * Cos((k + 1) * t),
                r * (k + 1) * Sin(t) - r * Sin((k + 1) *t));

        Double2 ClosedEpicycloid(Angle t, int k) => Epicycloid(t, 1.0 / k, 1);
        Double2 Cardoid(Angle t) => ClosedEpicycloid(t, 1);
        Double2 Nephroid(Angle t) => ClosedEpicycloid(t, 2);
        Double2 Trefoiloid(Angle t) => ClosedEpicycloid(t, 3);
        Double2 Quatrefoiloid(Angle t) => ClosedEpicycloid(t, 4);

        Double2 Hypocycloid(Angle t, double r, double k)
            => (r * (k - 1) * Cos(t) + r * Cos((k - 1) * t),
                r * (k - 1) * Sin(t) + r * Sin((k - 1) * t));

        Double2 ClosedHypocycloid(Angle t, int k) => Hypocycloid(t, 1.0 / k, 1);
        Double2 Deltoid(Angle t) => ClosedHypocycloid(t, 3);
        Double2 Astroid(Angle t) => ClosedHypocycloid(t, 4);

        Double2 Epitrochoid(Angle t, double r, double d) 
            => ((1 + r) * Cos(t) - d * Cos((1 + r) / r * t), 
                (1 + r) * Sin(t) - d * Sin((1 + r) / r * t));

        Double2 GeneralizedParametricCurve(Angle t, double a, double b, double c, double d, double j, double k) 
            => (Cos(a * t) - Pow(Cos(b * t), j), 
                Sin(c * t) - Pow(Sin(d * t), k));
    }
}