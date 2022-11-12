using System;
using System.Security.Cryptography;
using static Plato.Operations;

namespace Plato
{
    [Operations]
    class VectorOperations
    {
        Double2 Normal(Double2 v) => v / v.Length();
        Double3 Normal(Double3 v) => v / v.Length();
        Double4 Normal(Double4 v) => v / v.Length();
        Float2 Normal(Float2 v) => v / (float)v.Length();
        Float3 Normal(Float3 v) => v / (float)v.Length();
        Float4 Normal(Float4 v) => v / (float)v.Length();
        double Length(Line2D line) => Distance(line.A, line.B);
        double Distance(Double2 a, Double2 b) => (b - a).Length();
        double Distance(Double3 a, Double3 b) => (b - a).Length();
        double Distance(Double4 a, Double4 b) => (b - a).Length();
        double Distance(Float2 a, Float2 b) => (b - a).Length();
        double Distance(Float3 a, Float3 b) => (b - a).Length();
        double Distance(Float4 a, Float4 b) => (b - a).Length();
    }

    [VectorizedOperations]
    class VectorizedOperations
    {
        double SafeDivide(double x, double y) => y.AlmostZero() ? x : x / y;
        double Half(double x) => x * 0.5;
        double Quarter(double x) => x * 0.25;
        double Twice(double x) => x * 2;
        double Thrice(double x) => x * 3;
        double MinusOne(double x) => x - 1;
        double PlusOne(double x) => x + 1;
        double FromOne(double x) => 1 - x;
        double Abs(double x) => Math.Abs(x);
        double Exp(double x) => Math.Exp(x);
        double Log(double x) => Math.Log(x);
        double Log10(double x) => Math.Log10(x);
        double Sqrt(double x) => Math.Sqrt(x);
        double Sign(double x) => x > 0 ? 1 : x < 0 ? -1 : 0;
        double Inverse(double x) => 1 / x;
        double Truncate(double x) => Math.Truncate(x);
        double Ceiling(double x) => Math.Ceiling(x);
        double Floor(double x) => Math.Floor(x);
        double Round(double x) => Math.Round(x);
        double Smoothstep(double v) => v.Pow2() * (3 - 2 * v);
        double Lerp(double v1, double v2, double t) => v1 * (1 - t) + v2 * t;
        double Mix(double v1, double v2, double t) => Lerp(v1, v2, t);
        double InverseLerp(double v, double a, double b) => (v - a) / (b - a);
        double Unmix(double v, double a, double b) => InverseLerp(v, a, b);
        double Clamp(double v, double min, double max) => Max(Min(v, max), min);
        double ClampZeroToOne(double v) => Clamp(v, 0, 1);
        double Average(double v1, double v2) => Lerp(v1, v2, 0.5);
        double Barycentric(double v1, double v2, double v3, Double2 uv) => v1 + (v2 - v1) * uv.X + (v3 - v1) * uv.Y;
        double Min(double v1, double v2) => Math.Min(v1, v2);
        double Max(double v1, double v2) => Math.Max(v1, v2);
        double ClampPositive(double v) => Math.Max(v, 0);
        double ClampNegative(double v) => Math.Min(v, 0);
        double Pow2(double x) => x * x;
        double Pow3(double x) => x * x * x;
        double Pow4(double x) => x * x * x * x;
        double Pow5(double x) => x * x * x * x * x;
        double Pow(double x, double y) => Math.Pow(x, y);
    }

    [Operations]
    class ConversionOperations
    {
        Angle Revs(double x) => x * Constants.TwoPi;
        Angle Rads(double x) => x;
        Angle Degrees(double x) => (x / 360).Revs();
        double Revs(Angle x) => x.Radians / Constants.TwoPi;
        double Rads(Angle x) => x.Radians;
        double Degrees(Angle x) => x.Revs() * 360;
    }

    [Operations]
    class TrigOperations
    {
        Angle Acos(double x) => Math.Acos(x);
        Angle Asin(double x) => Math.Asin(x);
        Angle Atan(double x) => Math.Atan(x);

        // Extremely esoteric functions.
        double Versin(Angle x) => 1 - Cos(x);
        double Vercosin(Angle x) => 1 + Cos(x);
        double Coversin(Angle x) => 1 - Sin(x);
        double Covercosin(Angle x) => 1 + Sin(x);
        double Haversin(Angle x) => Versin(x) / 2;
        double Havercosin(Angle x) => Vercosin(x) / 2;
        double Hacoversin(Angle x) => Coversin(x) / 2;
        double Hacovercosin(Angle x) => Coversin(x) / 2;
        double Exsec(Angle x) => Sec(x) - 1;
        double Excsc(Angle x) => Csc(x) - 1;

        double Cos(Angle x) => Math.Cos(x);
        double Cosh(Angle x) => Math.Cosh(x);
        double Sin(Angle x) => Math.Sin(x);
        double Sinh(Angle x) => Math.Sinh(x);
        double Tan(Angle x) => Math.Tan(x);
        double Tanh(Angle x) => Math.Tanh(x);

        double Sec(Angle x) => 1.0 / Cos(x);
        double Csc(Angle x) => 1.0 / Sin(x);
        double Cot(Angle x) => 1.0 / Tan(x);
    }

    [Operations]
    class ComparisonOperations
    {
        bool GtZ(double x) => x > 0;
        bool LtZ(double x) => x < 0;
        bool GtEqZ(double x) => x >= 0;
        bool LtEqZ(double x) => x <= 0;
        bool Gt(double x, double y) => x > y;
        bool Lt(double x, double y) => x > y;
        bool GtEq(double x, double y) => x >= y;
        bool LtEq(double x, double y) => x <= y;
        bool IsInfinity(double v) => double.IsInfinity(v);
        bool IsNaN(double v) => double.IsNaN(v);
        bool AlmostZero(double v) => v.AlmostEquals(0);
        bool AlmostOne(double v) => v.AlmostEquals(1);
        bool AlmostZeroOrOne(double v) => v.AlmostZero() && v.AlmostOne();
        bool Within(double v, double min, double max) => v >= min && v < max;
    }

    [Operations]
    class EasingOperations
    {
        //===
        // Easings.cs
        // https://easings.net/
        // https://github.com/acron0/Easings/blob/master/Easings.cs

        Func<double, double> EaseInOutFunc(Func<double, double> easeIn, Func<double, double> easeOut) => p => p < 0.5 ? 0.5 * easeIn(p * 2) : 0.5 * easeOut(p * 2 - 1) + 0.5;
        Func<double, double> EaseInOutFunc(Func<double, double> easeIn) => EaseInOutFunc(easeIn, InvertEaseFunc(easeIn));
        Func<double, double> InvertEaseFunc(Func<double, double> easeIn) => p => 1 - easeIn(1 - p);

        double Linear(double p) => p;
        double QuadraticEaseIn(double p) => p.Pow2();
        double QuadraticEaseOut(double p) => InvertEaseFunc(QuadraticEaseIn)(p);
        double QuadraticEaseInOut(double p) => EaseInOutFunc(QuadraticEaseIn)(p);
        double CubicEaseIn(double p) => p.Pow3();
        double CubicEaseOut(double p) => InvertEaseFunc(CubicEaseIn)(p);
        double CubicEaseInOut(double p) => EaseInOutFunc(CubicEaseIn)(p);
        double QuarticEaseIn(double p) => p.Pow4();
        double QuarticEaseOut(double p) => InvertEaseFunc(QuarticEaseIn)(p);
        double QuarticEaseInOut(double p) => EaseInOutFunc(QuarticEaseIn)(p);
        double QuinticEaseIn(double p) => p.Pow5();
        double QuinticEaseOut(double p) => InvertEaseFunc(QuinticEaseIn)(p);
        double QuinticEaseInOut(double p) => EaseInOutFunc(QuinticEaseIn)(p);
        double SineEaseIn(double p) => InvertEaseFunc(SineEaseOut)(p);
        double SineEaseOut(double p) => p.Quarter().Revs().Sin();
        double SineEaseInOut(double p) => EaseInOutFunc(SineEaseIn, SineEaseOut)(p);
        double CircularEaseIn(double p) => 1 - (1 - p.Pow2()).Sqrt();
        double CircularEaseOut(double p) => InvertEaseFunc(CircularEaseIn)(p);
        double CircularEaseInOut(double p) => EaseInOutFunc(CircularEaseIn, CircularEaseOut)(p);
        double ExponentialEaseIn(double p) => p.AlmostZero() ? p : 2.0.Pow(10 * (p - 1));
        double ExponentialEaseOut(double p) => InvertEaseFunc(ExponentialEaseIn)(p);
        double ExponentialEaseInOut(double p) => EaseInOutFunc(ExponentialEaseIn)(p);
        double ElasticEaseIn(double p) => (13 * p.Quarter().Revs()) * 2.0.Pow(10 * (p - 1)).Rads().Sin();
        double ElasticEaseOut(double p) => InvertEaseFunc(ElasticEaseIn)(p);
        double ElasticEaseInOut(double p) => EaseInOutFunc(ElasticEaseIn)(p);
        double BackEaseIn(double p) => p.Pow3() - p * p.Half().Revs().Sin();
        double BackEaseOut(double p) => InvertEaseFunc(BackEaseIn)(p);
        double BackEaseInOut(double p) => EaseInOutFunc(BackEaseIn)(p);
        double BounceEaseIn(double p) => InvertEaseFunc(BounceEaseIn)(p);
        double BounceEaseOut(double p)
        {
            if (p < 4 / 11.0) return 121.0 * p.Pow2() / 16.0;
            if (p < 8 / 11.0) return 363.0 / 40.0 * p.Pow2() - 99.0 / 10.0 * p + 17.0 / 5.0;
            if (p < 9 / 10.0) return 4356.0 / 361.0 * p.Pow2() - 35442.0 / 1805.0 * p + 16061.0 / 1805.0;
            return 54.0 / 5.0 * p.Pow2() - 513.0 / 25.0 * p + 268.0 / 25.0;
        }
        double BounceEaseInOut(double p) => EaseInOutFunc(BounceEaseIn, BounceEaseOut)(p);
    }

    [Operations]
    class ShapingOperations
    {
        //===
        // Signal or Shaping functions
        // https://iquilezles.org/articles/functions/
        // https://thebookofshaders.com/05/

        double ExponentialImpulse(double x, double k) => k * x * (1.0 - k * x).Exp();
        double QuadraticImpulse(double x, double k) => 2.0 * k.Sqrt() * x / (1.0 + k * x * x);
        double PolynomalialImpulse(double x, double k, double n) => (n / (n - 1.0)) * ((n - 1.0) * k).Pow(1.0 / n) * x / (1.0 + k * x.Pow(n));
        double NormalizedPowerCurve(double x, double a, double b) => (a + b.Pow(a + b) / (a.Pow(a) * (b.Pow(b)) * UnnormalizedPowerCurve(x, a, b)));
        double UnnormalizedPowerCurve(double x, double a, double b) => x.Pow(a) * (1.0 - x).Pow(b);
        double Parabola(double x, double k) => 4.0 * x * (1.0 - x).Pow(k);
        double Sinc(double x, double k) { var a = k * (x - 1.0).Half().Revs(); return a.Sin() / a; }
        double Gain(double x, double k) { var a = 0.5 * (2.0 * ((x < 0.5) ? x : 1.0 - x).Pow(k)); return (x < 0.5) ? a : 1.0 - a; }
        double ExponentialStep(double x, double k, double n) => (-k * x.Pow(n)).Exp();
        //double AlmostIdentityCubic(double x, double m, double n) { if (x > m) return x; var a = 2.0 * n - m; var b = 2.0 * m - 3.0 * n; var t = x / m; return (a * t + b) * t * t + n; }
        //double AlmostIdentitySqrt(double x, double m, double n) => (x * x + n).Sqrt();
        //double AlmostUnityIdentity(double x) => x * x * (2.0 - x);
        double IntegralSmoothstep(double x, double t) => (x > t) ? x - t / 2.0 : (x).Pow3() * (1.0 - x * 0.5 / t) / t / t;
    }

    [Operations]
    class CurveOperations
    {
        //===
        // http://www.flong.com/archive/texts/code/shapers_circ/

        double CircleSigmoid(double x, double a = 0.5) => x <= a
            ? a - (a.Pow2() - x.Pow2()).Sqrt()
            : a + ((1 - a).Pow2() - (x - 1).Pow2().Pow2()).Sqrt();

        double CircularSeat(double x, double a = 0.5) =>
            x <= a ? (a.Pow2() - (x - a).Pow2()).Sqrt() : ((1 - a).Pow2() - (x - a).Pow2()).Sqrt();

        double EllipticalSeat(double x, double a, double b) => (a.AlmostZeroOrOne()) ? a :
            (x <= a) ? (b / a) * (a.Pow2() - (x - a).Pow2()).Sqrt() :
            1 - ((1 - b) / (1 - a)) * ((1 - a).Pow2() - (x - a).Pow2()).Sqrt();

        double EllipticalSigmoid(double x, double a, double b) => a.AlmostZeroOrOne() ? a :
            (x <= a) ? b * (1 - ((a.Pow2() - x.Pow2()) / a).Sqrt()) :
            b + ((1 - b) / (1 - a)) * ((1 - a).Pow2() - (x - 1).Pow2()).Sqrt();

        // https://en.wikipedia.org/wiki/Catenary
        double Caternay(double x, double a = 1.0) => (x / a).Rads().Cosh();

        //== 
        // Some parametric curves 
        // https://en.wikipedia.org/wiki/Cycloid
        // https://en.wikipedia.org/wiki/Trochoid
        // https://en.wikipedia.org/wiki/Epitrochoid
        // https://en.wikipedia.org/wiki/Hypotrochoid 
        // https://en.wikipedia.org/wiki/Hypocycloid
        // https://en.wikipedia.org/wiki/Cyclocycloid
        // https://en.wikipedia.org/wiki/Epicycloid
        // https://en.wikipedia.org/wiki/Lissajous_curve
        // https://en.wikipedia.org/wiki/Lemniscate_of_Gerono
        // https://en.wikipedia.org/wiki/Parametric_equation
        // https://en.wikipedia.org/wiki/Lima%C3%A7on

        Double2 Parabola(double x)
            => (x, x.Pow2());

        Double2 Hyperbola(Angle t)
            => (t.Sec(), t.Tan());

        Double2 LeminscateOfGerono(Angle t)
            => (t.Cos(), t.Sin() * t.Cos());

        Double2 Circle(Angle t)
            => (t.Cos(), t.Sin());

        Double2 Lissajous(Angle t, double a, double b, double kx, double ky)
            => (a * (kx * t).Cos(),
                b * (ky * t).Sin());

        Double2 Hypotrochoid(Angle t, double r, double d)
            => ((1 - r) * t.Cos() + d * (t - 1.0).Cos(),
                (1 - r) * t.Sin() + d * (t - 1.0).Sin());

        Double2 Epicycloid(Angle t, double r, double k)
            => (r * (k + 1) * t.Cos() - r * ((k + 1) * t).Cos(),
                r * (k + 1) * t.Sin() - r * ((k + 1) * t).Sin());

        Double2 ClosedEpicycloid(Angle t, int k) => Epicycloid(t, 1.0 / k, 1);
        Double2 Cardoid(Angle t) => ClosedEpicycloid(t, 1);
        Double2 Nephroid(Angle t) => ClosedEpicycloid(t, 2);
        Double2 Trefoiloid(Angle t) => ClosedEpicycloid(t, 3);
        Double2 Quatrefoiloid(Angle t) => ClosedEpicycloid(t, 4);

        Double2 Hypocycloid(Angle t, double r, double k)
            => (r * (k - 1) * t.Cos() + r * ((k - 1) * t).Cos(),
                r * (k - 1) * t.Sin() + r * ((k - 1) * t).Cos());

        Double2 ClosedHypocycloid(Angle t, int k) => Hypocycloid(t, 1.0 / k, 1);
        Double2 Deltoid(Angle t) => ClosedHypocycloid(t, 3);
        Double2 Astroid(Angle t) => ClosedHypocycloid(t, 4);

        Double2 Epitrochoid(Angle t, double r, double d)
            => ((1 + r) * t.Cos() - d * ((1 + r) / r * t).Cos(),
                (1 + r) * t.Sin() - d * ((1 + r) / r * t).Sin());

        // TODO: there are some cute examples in Wikipedia at https://en.wikipedia.org/wiki/Parametric_equation, but no names
        Double2 GeneralizedParametricCurve(Angle t, double a, double b, double c, double d, double j, double k)
            => ((a * t).Cos() - (b * t).Cos().Pow(j),
                (c * t).Sin() - (d * t).Sin().Pow(k));

        double PolarLimacon(Angle t, double a, double b)
            => b + a * t.Cos();

        Double2 CartesianLimacon(Angle t, double a = 1, double b = 1)
            => ToCartesian((PolarLimacon(t, a, b), t));

        Double2 ToCartesian(PolarCoordinate polar)
            => Circle(polar.Angle) * polar.Radius;

        // TODO: wave / signal functions 
        // https://en.wikipedia.org/wiki/Sawtooth_wave
        // https://en.wikipedia.org/wiki/Square_wave 

        // TODO: more polar functions
        // Lemniscates
    }

    [Operations]
    class DistanceField2DOperations
    {    
        // https://iquilezles.org/articles/distfunctions2d/
        
        double CircleDistance(Double2 p) => p.Length();
        double CircleDistance(Double2 p, double r) => p.Length() - r;

        Double2 XY(Double4 d) => (d.X, d.Y);
        Double2 ZW(Double4 d) => (d.Z, d.W);

        double RoundedBoxSDF(Double2 p, Double2 size, Double4 r)
        {
            var xy = p.X > 0.0 ? r.XY() : r.ZW();
            var x = p.Y > 0.0 ? xy.X : xy.Y;
            var q = p.Abs() - size + x;
            return q.X.Max(q.Y).Min(0.0) + q.Max(Double2.Zero).Length() - x;
        }

        double BoxSDF(Double2 p, Double2 size)
        {
            var d = p.Abs() - size;
            return d.ClampPositive().Length() + d.X.Max(d.Y).ClampPositive();
        }

        // TODO: I need a "Matrix2"
        /*
        double Line(Double2 p, Line2D line, double thickness)
        {
            var d = (line.B - line.A) / line.Length();
            var q = (p - (line.A + line.B) * 0.5);
            q = Mat2(d.x, -d.y, d.y, d.x) * q;
            q = Abs(q) - new Double2(line.Length(), thickness) * 0.5;
            return q.ClampPositive().Length() + Max(q.x, q.y).ClampNegative();
        }*/

        double LineSDF(Double2 p, Line2D line)
        {
            var pa = p - line.A;
            // TODO: Point2D - Point2D == Vector2D
            var ba = line.B.Value - line.A.Value;
            var h = (pa.Dot(ba) / ba.Dot(ba)).Clamp(0.0, 1.0);
            return (pa - ba * h).Length();
        }

        double PolygonSDF(Double2 p, IArray<Double2> v)
        {
            var d = (p - v[0]).Dot(p - v[0]);
            var s = 1.0;
            for (int i = 0, j = v.Count - 1; i < v.Count; j = i, i++)
            {
                // distance
                var e = v[j] - v[i];
                var w = p - v[i];
                var b = w - e * (w.Dot(e) / e.Dot(e)).ClampZeroToOne();
                d = d.Min(b.Dot(b));

                // winding number from http://geomalgorithms.com/a03-_inclusion.html
                var cond1 = p.Y >= v[i].Y;
                var cond2 = p.Y < v[j].Y;
                var cond3 = e.X * w.Y > e.Y * w.X;
                if (cond1 == cond2 && cond1 == cond3) s = -s;
            }

            return s * d.Sqrt();
        }

        double EquilateralTriangleSDF(Double2 p)
        {
            var k = 3.0.Sqrt();
            var x = p.X.Abs() - 1.0;
            var y = p.Y + 1.0 / k;
            var r = p;
            if (x + k * y > 0.0) r = new Double2(x - k * y, -k * x - y) / 2.0;
            r = r.WithX(r.X - p.X.Clamp(-2.0, 0.0));
            return -r.Length() * r.Y.Sign();
        }

        // Distance to a rounded "X" shape, given its width and thickness. It is exact in the exterior, and a bound in the interior
        double RoundXSDF(Double2 p, double w, double r)
        {
            p = p.Abs();
            return (p - (p.X + p.Y.Min(w) * 0.5)).Length() - r;
        }

        Func<Double2, double> RoundedSDF(Func<Double2, double> func, double r)
            => p => func(p) - r;

        Func<Double2, double> AnnularSDF(Func<Double2, double> func, double r)
            => p => func(p).Abs() - r;

        Func<Double2, double> RepeatSDF(Func<Double2, double> func, Double2 period)
            => p => func((p + new Double2(0.5, 0.5) * period).Modulo(period) - period * 0.5);
    }
}