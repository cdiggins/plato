using System;

namespace Plato
{

    public enum EaseType
    {
        Linear,
        QuadraticIn,
        QuadraticOut,
        QuadraticInOut,
        CubicIn,
        CubicOut,
        CubicInOut,
        QuarticIn,
        QuarticOut,
        QuarticInOut,
        QuinticIn,
        QuinticOut,
        QuinticInOut,
        BounceIn,
        BounceOut,
        BounceInOut,
        ElasticIn,
        ElasticOut,
        ElasticInOut,
        CircularIn,
        CircularOut,
        CircularInOut,
        SineIn,
        SineOut,
        SineInOut,
        ExponentialIn,
        ExponentialOut,
        ExponentialInOut
    }

    /// <summary>
    /// This is a wrapper around a interpolation function
    /// such as Lerp or Slerp (Spherical lerp).
    /// When composed with an ease curve function (double -> double)
    /// it can produce interesting animated effects.  
    /// </summary>
    public interface IInterpolator<T>
    {
        T Evaluate(Amount t);
    }

    /// <summary>
    /// An animation curve represents a function over time,
    /// and a duration (potentially infinite).
    /// </summary>
    public interface IAnimationCurve
    {
        Amount this[Time t] { get; }
        Time Duration { get; }
    }

    public class Interpolator<T> : IInterpolator<T>
    {
        public Interpolator(Func<Amount, T> func)
            => Func = func;
        public Func<Amount, T> Func { get; }
        public T Evaluate(Amount t)
            => Func(t);
    }

    public class AnimatedValue<T>
    {
        public IAnimationCurve Curve { get; }
        public IInterpolator<T> Interpolator { get; }

        public T Evaluate(AnimationState state)
            => Interpolator.Evaluate(Curve.Evaluate(state.ElapsedTime));

        public AnimatedValue(IAnimationCurve curve, IInterpolator<T> interpolator)
            => (Curve, Interpolator) = (curve, interpolator);
    }

    /// <summary>
    /// Represents the status of an animation. Supports starting, stopping, and resuming of an animation,
    /// and returns how much of the animation has been evaluated. 
    /// </summary>
    public class AnimationState
    {
        public AnimationState(bool started, bool stopped, Time startTime, Time stopTime, Time currentTime, Time pauseTime)
            => (Started, Stopped, StartTime, StopTime, CurrentTime, PauseTime) = (started, stopped, startTime, stopTime, currentTime, pauseTime);

        public bool Started { get; }
        public bool Stopped { get; }
        public Time StartTime { get; }
        public Time StopTime { get; }
        public Time CurrentTime { get; }
        public Time PauseTime { get; }
        public bool Active => Started && !Stopped;

        public Time ElapsedTime
            => Started ? (Stopped ? StopTime : CurrentTime) - StartTime - PauseTime : 0;

        public AnimationState Start(Time t)
            => new(true, false, t, Time.MaxValue, t, PauseTime);

        public AnimationState Resume(Time t)
            => new(true, false, t, Time.MaxValue, t, PauseTime + (t - StopTime));

        public AnimationState Stop(Time t)
            => new(true, true, StartTime, t, t, PauseTime);

        public AnimationState Update(Time t)
            => new(true, false, StartTime, StopTime, t, PauseTime);
    }

    public class AnimationCurve : IAnimationCurve
    {
        public Func<Time, Amount> Func { get; }
        public Time Duration { get; }
        public Amount this[Time t] => Func(t);
        public AnimationCurve(Func<Time, Amount> f, Time duration)
            => (Func, Duration) = (f, duration);
    }

    public static class AnimationCurves
    {
        public static Func<double, double> ToFunc(this EaseType easeType)
        {
            return easeType switch
            {
                EaseType.Linear => EasingOperations.Linear,
                EaseType.QuadraticIn => EasingOperations.QuadraticEaseIn,
                EaseType.QuadraticOut => EasingOperations.QuadraticEaseOut,
                EaseType.QuadraticInOut => EasingOperations.QuadraticEaseInOut,
                EaseType.CubicIn => EasingOperations.CubicEaseIn,
                EaseType.CubicOut => EasingOperations.CubicEaseOut,
                EaseType.CubicInOut => EasingOperations.CubicEaseInOut,
                EaseType.QuarticIn => EasingOperations.QuarticEaseIn,
                EaseType.QuarticOut => EasingOperations.QuarticEaseOut,
                EaseType.QuarticInOut => EasingOperations.QuarticEaseInOut,
                EaseType.QuinticIn => EasingOperations.QuinticEaseIn,
                EaseType.QuinticOut => EasingOperations.QuinticEaseOut,
                EaseType.QuinticInOut => EasingOperations.QuinticEaseInOut,
                EaseType.BounceIn => EasingOperations.BounceEaseIn,
                EaseType.BounceOut => EasingOperations.BounceEaseOut,
                EaseType.BounceInOut => EasingOperations.BounceEaseInOut,
                EaseType.ElasticIn => EasingOperations.ElasticEaseIn,
                EaseType.ElasticOut => EasingOperations.ElasticEaseOut,
                EaseType.ElasticInOut => EasingOperations.ElasticEaseInOut,
                EaseType.CircularIn => EasingOperations.CircularEaseIn,
                EaseType.CircularOut => EasingOperations.CircularEaseOut,
                EaseType.CircularInOut => EasingOperations.CircularEaseInOut,
                EaseType.SineIn => EasingOperations.SineEaseIn,
                EaseType.SineOut => EasingOperations.SineEaseOut,
                EaseType.SineInOut => EasingOperations.SineEaseInOut,
                EaseType.ExponentialIn => EasingOperations.ExponentialEaseIn,
                EaseType.ExponentialOut => EasingOperations.ExponentialEaseOut,
                EaseType.ExponentialInOut => EasingOperations.ExponentialEaseInOut,
                _ => throw new ArgumentOutOfRangeException(nameof(easeType), easeType, null)
            };
        }

        public static IAnimationCurve ToAnimationCurve(this Func<double, double> func) => UnscaledCurve(func, 1);
        public static IAnimationCurve ToAnimationCurve(this Func<double, double> func, Time duration) => ScaledCurve(func, duration);
        public static IAnimationCurve ToAnimationCurve(this EaseType easeType) => easeType.ToFunc().ToAnimationCurve();
        public static IAnimationCurve ToAnimationCurve(this EaseType easeType, Time duration) => easeType.ToFunc().ToAnimationCurve(duration);
        public static IAnimationCurve SetEaseType(this IAnimationCurve curve, EaseType easeType) => easeType.ToAnimationCurve(curve.Duration);
        public static IAnimationCurve ScaledCurve(Func<double, double> f, Time duration) => UnscaledCurve(t => f(t / duration), duration);
        public static IAnimationCurve UnscaledCurve(Func<double, double> f, Time duration) => new AnimationCurve(x => f(x), duration);
        public static double Evaluate(this IAnimationCurve curve, Time t) => curve[t];
        public static IAnimationCurve SetDurationAndRescale(this IAnimationCurve curve, double amount) => UnscaledCurve(t => curve[t / amount], curve.Duration * amount);
        public static IAnimationCurve SetDuration(this IAnimationCurve curve, Time duration) => UnscaledCurve(t => curve[t], duration);
        public static IAnimationCurve Cut(this IAnimationCurve curve, double duration) => curve.SetDuration(duration);
        public static IAnimationCurve Skip(this IAnimationCurve curve, double duration) => UnscaledCurve(t => curve[t + duration], curve.Duration - duration);
        public static IAnimationCurve MultiplyDuration(this IAnimationCurve curve, double amount) => curve.SetDuration(curve.Duration * amount);
        public static IAnimationCurve MapInput(this IAnimationCurve curve, Func<double, double> f) => new AnimationCurve(t => curve.Evaluate(f(t)), curve.Duration);
        public static IAnimationCurve MapOutput(this IAnimationCurve curve, Func<double, double> f) => new AnimationCurve(t => f(curve.Evaluate(t)), curve.Duration);
        public static IAnimationCurve Clamp(this IAnimationCurve curve, double min, double max) => curve.MapOutput(x => x.Clamp(min, max));
        public static IAnimationCurve Invert(this IAnimationCurve curve) => curve.MapOutput(x => 1 - x);
        public static IAnimationCurve Mirror(this IAnimationCurve curve) => curve.Sequence(curve.Reverse());
        public static IAnimationCurve Reverse(this IAnimationCurve curve) => curve.MapInput(x => 1 - x);
        public static IAnimationCurve Repeat(this IAnimationCurve curve, double count) => UnscaledCurve(x => curve[x % curve.Duration], curve.Duration * count);
        public static IAnimationCurve YoyoLoop(this IAnimationCurve curve, double count) => curve.Mirror().Repeat(count);
        public static IAnimationCurve Sequence(this IAnimationCurve curve, IAnimationCurve other) => UnscaledCurve(t => t < curve.Duration ? curve[t] : other[t - curve.Duration], curve.Duration + other.Duration);
        public static IAnimationCurve Blend(this IAnimationCurve curve, IAnimationCurve other, Func<double, double, double> func) => UnscaledCurve(t => func(curve[t], other[t]), curve.Duration.Seconds.Max(other.Duration));
        public static IAnimationCurve Max(this IAnimationCurve curve, IAnimationCurve other) => curve.Blend(other, (x, y) => x.Max(y));
    }

    public static class FunctionalExtensions
    {
        public static Func<TR> PartialApply<T0, TR>(this Func<T0, TR> f, T0 v)
            => () => f(v);

        public static Func<T1, TR> PartialApply<T0, T1, TR>(this Func<T0, T1, TR> f, T0 v)
            => x => f(v, x);

        public static Func<TR> PartialApply<T0, T1, TR>(this Func<T0, T1, TR> f, T0 v0, T1 v1)
            => () => f(v0, v1);

        public static Func<T1, T2, TR> PartialApply<T0, T1, T2, TR>(this Func<T0, T1, T2, TR> f, T0 v)
            => (x, y) => f(v, x, y);

        public static Func<T2, TR> PartialApply<T0, T1, T2, TR>(this Func<T0, T1, T2, TR> f, T0 v0, T1 v1)
            => x => f(v0, v1, x);

        public static Func<TR> PartialApply<T0, T1, T2, TR>(this Func<T0, T1, T2, TR> f, T0 v0, T1 v1, T2 v2) => ()
            => f(v0, v1, v2);

        public static Func<T0, T2> Compose<T0, T1, T2>(this Func<T0, T1> f, Func<T1, T2> g)
            => x => g(f(x));
    }

    public static class Interpolations
    {
        public static IInterpolator<T> ToInterpolator<T>(this Func<double, T> f)
            => new Interpolator<T>(x => f(x));

        public static IInterpolator<T> ToInterpolator<T>(this Func<T, T, double, T> f, T from, T to)
            => f.PartialApply(from, to).ToInterpolator();

        public static IInterpolator<T> ToInterpolator<T>(this Func<T, T, float, T> f, T from, T to)
            => new Interpolator<T>(x => f(from, to, (float)x));

        public static IInterpolator<T> ToInterpolator<T>(this T from, T to, Func<T, T, double, T> f)
            => f.PartialApply(from, to).ToInterpolator();

        public static IInterpolator<T> ApplyEasing<T>(this IInterpolator<T> self, Func<double, double> f)
            => f.Compose(x => self.Evaluate(x)).ToInterpolator();

        public static IInterpolator<T> ApplyEasing<T>(this IInterpolator<T> self, EaseType easeType)
            => easeType.ToFunc().Compose(x => self.Evaluate(x)).ToInterpolator();
    }

    public static class AnimationExtensions
    {
        public static AnimatedValue<T> Animate<T>(this IInterpolator<T> interpolator)
            => new(EaseType.Linear.ToAnimationCurve(), interpolator);

        public static AnimatedValue<T> With<T>(this AnimatedValue<T> self, IInterpolator<T> interpolator)
            => new(self.Curve, interpolator);

        public static AnimatedValue<T> With<T>(this AnimatedValue<T> self, Func<IInterpolator<T>, IInterpolator<T>> f)
            => new(self.Curve, f(self.Interpolator));

        public static AnimatedValue<T> With<T>(this AnimatedValue<T> self, IAnimationCurve curve)
            => new(curve, self.Interpolator);

        // TODO: functions like this need to be added to Plato types
        public static AnimatedValue<T> With<T>(this AnimatedValue<T> self, Func<IAnimationCurve, IAnimationCurve> f)
            => self.With(f(self.Curve));

        public static AnimatedValue<T> SetDuration<T>(this AnimatedValue<T> self, double duration)
            => self.With(self.Curve.SetDuration(duration));

        public static AnimatedValue<T> SetCurveEaseType<T>(this AnimatedValue<T> self, EaseType easeType)
            => self.With(self.Curve.SetEaseType(easeType));

        public static AnimatedValue<T> ApplyEasingToInterpolator<T>(this AnimatedValue<T> self, EaseType easeType)
            => self.With(self.Interpolator.ApplyEasing(easeType));

        public static AnimatedValue<T> Animate<T>(this T from, T to, Func<T, T, double, T> lerp, EaseType type = EaseType.Linear, double duration = 1)
            => new(type.ToAnimationCurve().SetDurationAndRescale(duration), lerp.ToInterpolator(from, to));

    }
}
