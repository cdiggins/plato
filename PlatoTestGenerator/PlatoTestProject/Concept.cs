using Newtonsoft.Json.Linq;
using NUnit.Framework.Internal;
using Plato;
using System.Net;
using System.Runtime.InteropServices;

namespace Plato
{
    [Concept]
    interface IValue<T> 
        : IEqualityComparer<T> 
        where T: IValue<T>
    {
        T Default { get; }
        T Zero { get; }
        T One {get; }
        T MinValue { get; }
        T MaxValue { get; }
    }

    // https://en.wikipedia.org/wiki/Quantity
    [Concept]
    interface IQuantity<T> 
        : IAdditiveGroup<T>, IValue<T>, IScalable<T, double> 
        where T: IQuantity<T>
    {
        double Magnitude();
        int CompareTo(T x);
        T Negate();
    }
    
    // https://en.wikipedia.org/wiki/Physical_quantity
    [Concept]
    interface IMeasure<T>
        : IQuantity<T>
        where T : IMeasure<T>
    {
        string Dimension();
    }

    interface ITotalOrdered<T>
    {
        bool LessThan(T x);
    }

    [Concept]
    class Number
    { }

    [Concept]
    interface IIntrinsicMathOperations<T>
    {
        // Math operations
        T Abs();
        T Acos();
        T Acosh();
        T Asin();
        T Asinh();
        T Atan();
        T Atan2(T other);
        T Atanh();
        T BitDecrement();
        T BitIncrement();
        T Cbrt();
        T Ceiling();
        T CopySign(T other);
        T Cos();
        T Cosh();
        T Exp();
        T Floor();
        T FusedMultiplyAdd(T y, T z);
        T IEEERemainder(T other);
        T ILogB();
        T Log();
        T Log(T other);
        T Log10();
        T Log2();
        T Max(T other);
        T MaxMagnitude(T other);
        T Min(T other);
        T MinMagnitude();
        T Pow(T other);
        T ReciprocalEstimate();
        T ReciprocalSqrtEstimate();
        T Round();
        T ScaleB(int other);
        int Sign();
        T Sin();
        (T, T) SinCos();
        T Sinh();
        T Sqrt();
        T Tan();
        T Tanh();
        T Truncate();
    }

    // TO INVESTIGATE:
    // https://en.wikipedia.org/wiki/Transseries
    // https://en.wikipedia.org/wiki/Real_closed_field
    // https://en.wikipedia.org/wiki/Superreal_number
    // https://en.wikipedia.org/wiki/Hyperreal_number
    // https://en.wikipedia.org/wiki/Surreal_number

    public class Test
    {
    }

    [Concept]
    interface IInterval<T, TElement> 
        where T : IInterval<T, TElement> 
        where TElement : IRing<TElement>
    {
        TElement A { get; }
        TElement B { get; } 
        Interval WithA(TElement value);
        Interval WithB(TElement value);
    }

    // Intrinsic
    class Intrinsic
    {
        bool Between(Number v, Number a, Number b) => throw new NotImplementedException();
        bool Between(IArray<Number> v, IArray<Number> a, IArray<Number> b) => v.Zip(a, b, Between).All(x => x);
    }

    [Operations]
    class ConceptOperations
    {
        // Intrinsics: do I really have to define all of these? 
        // Basically everything in the "Vectorized". Also this applies to other things
        Number Max(Number a, Number b) => throw new NotImplementedException();
        Number Min(Number a, Number b) => throw new NotImplementedException();
        Number Sqrt(Number x) => throw new NotImplementedException();
        // TODO: Where are those "Intrinsics" going to be defined? I would imagine in the concept class.
        // Is the concept going to be an interfaces or a class?

        Number Center(Interval interval) => Lerp(interval, 0.5);
        Interval Unit(Interval _) => new(_.A.Zero(), _.A.One()));

            // To:
        Number Dot(Number n) => n;
        double Magnitude(Number x) => throw new NotImplementedException();
        Number Normal(Number v) => v / Magnitude(v);
        double Distance(Number a, Number b) => Magnitude(b - a);

        double Magnitude(Interval i) => Distance(i.A, i.B);
        int CompareTo(Number a, Number b) => Magnitude(b) - Magnitude(a) < 0 ? -1 : +1;
        bool IsEmpty(Interval interval) => LessThanOrEqualTo(interval.B, interval.A);
        bool LessThan(Number a, Number b) => Magnitude(a) < Magnitude(b);
        bool LessThanOrEqualTo(Number a, Number b) => Magnitude(a) <= Magnitude(b);
        bool GreaterThan(Number a, Number b) => Magnitude(a) > Magnitude(b);
        bool GreaterThanOrEqualTo(Number a, Number b) => Magnitude(a) >= Magnitude(b);

        // TODO: Interval would need to be renamed
        // TODO: Interval A should be Lower, and B should be Upper

        Interval Empty(Interval _) => Interval.Zero;
        Interval With(Interval i, Number a, Number b) => i.WithA(a).WithB(b);
        Interval Union(Interval a, Interval b) => With(a, Lesser(a.A, b.A), Greater(a.B, b.B));

        bool Contains(Interval i, Number v) => v >= i.A && v <= i.B;
        bool Contains(Interval i, Interval other) => Contains(i, other.A) && Contains(i, other.B);
        bool Overlaps(Interval i, Interval other) => Contains(i, other.A) || Contains(i, other.B) || Contains(other, i.A) || Contains(other, i.B);
        Interval Intersection(Interval a, Interval b) => a.WithA(Greater(a.A, b.A)).WithB(Lesser(a.B, b.B));
        (Interval, Interval) Split(Interval i, Number x) => (i.WithB(x), i.WithA(x));
        Number Range(Interval i) => i.B - i.A;
        Interval SubInterval(Interval i, Proportion start, Proportion length) => With(i, Lerp(i, start), Lerp(i, start) + Lerp(i, length));

        Number Lerp(Interval range, Proportion t) => Lerp(range.A, range.B, t);
        Number Lerp(Number a, Number b, Proportion t) => a * (1.0 - t) + b * t;
        Number InverseLerp(Number v, Number a, Number b) => (v - a) / (b - a);
        Number ClampLower(Number v, Number a) => v <= Max(v, a);
        Number ClampUpper(Number v, Number a) => v >= Min(v, a);
        Number Clamp(Number v, Number a, Number b) => ClampUpper(ClampLower(v, a), b);
        Number Clamp(Number v, Interval interval) => Clamp(v, interval.A, interval.B);
        Number Clamp(Interval interval, Number v) => Clamp(v, interval.A, interval.B);
        Number ClampedLerp(Interval interval, double t) => Clamp(Lerp(interval, t), interval); 

        Number Sum(IArray<Number> v) => v.Aggregate((a, b) => a + b);
        Number Product(IArray<Number> v) => v.Aggregate((a, b) => a * b);
        Number MinValue(Number _) => Number.MinValue;
        Number MaxValue(Number _) => Number.MaxValue;
        Number DefaultValue(IArray<Number> _) => default;
        Number MinValueElement(IArray<Number> v) => MinValue(DefaultValue(v));
        Number MaxValueElement(IArray<Number> v) => MaxValue(DefaultValue(v));
        Number Lesser(Number n1, Number n2) => n1 <= n2 ? n1 : n2;
        Number Greater(Number n1, Number n2) => n1 >= n2 ? n1 : n2;
        Number MaxComponent(IArray<Number> v) => v.Aggregate(MinValueElement(v), Greater);
        Number MinComponent(IArray<Number> v) => v.Aggregate(MaxValueElement(v), Lesser);
        Number Dot(Vector a, Vector b) => Sum(a * b);
        Number Average(IArray<Number> v) => Sum(v) / v.Count;

        Number WeightedAverage(IArray<Number> v, IArray<double> weights) => Average(v.Zip(weights, (a, b) => a * b));
        IArray<Number> Magnitude(IArra
        IArray<Number> Differences(IArray<Number> v) => (v.Count - 1).Select(i => v[i+1] - v[i]);
        IArray<Number> Differences(IArray<Number> v1, IArray<Number> v2) => v1.Zip(v2, (a,b) => a - b);
        IArray<Number> Lesser(IArray<Number> v1, IArray<Number> v2) => v1.Zip(v2, Lesser);
        IArray<Number> Greater(IArray<Number> v1, IArray<Number> v2) => v1.Zip(v2, Greater);
        IArray<Number> Multiply(IArray<Number> v1, IArray<Number> v2) => v1.Zip(v2, (a, b) => a * b);
        IArray<Number> Add(IArray<Number> v1, IArray<Number> v2) => v1.Zip(v2, (a, b) => a + b);
        IArray<Number> Divide(IArray<Number> v1, IArray<Number> v2) => v1.Zip(v2, (a, b) => a / b);
        IArray<Number> Subtract(IArray<Number> v1, IArray<Number> v2) => v1.Zip(v2, (a, b) => a - b);
        IArray<Number> Modulo(IArray<Number> v1, IArray<Number> v2) => v1.Zip(v2, (a, b) => a % b);
        IArray<Number> Mix(IArray<Number> v1, IArray<Number> v2, IArray<double> weights) => v1.Zip(v2, weights, (a,b,w) => Mix(a, b, w));
        IArray<Number> Clamp(IArray<Number> v, IArray<Number> lower, IArray<Number> upper) => v.Zip(lower, upper, (a, b, c) => Clamp(a, b, c));
        IArray<Number> SquaredDifferences(IArray<Number> v, Number value) => Squares(v.Select(x => x - value));
        IArray<Number> SquaredDifferences(IArray<Number> v1, IArray<Number> v2) => Squares(Differences(v1, v2));
        IArray<Number> Squares(IArray<Number> v) => v.Select(x => x * x);
        Number Variance(IArray<Number> v) => Sum(SquaredDifferences(v, Average(v))) / v.Count;
        Number StandardDeviation(IArray<Number> v) => Sqrt(Variance(v));

        // TODO: More stats 

        //Number PrefixSums(IArray<Number> v, IArray<double> weights) => Average(v.Zip(weights, (a, b) => a * b));
    }
}
