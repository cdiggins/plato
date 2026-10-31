# Easing and time-varying graphics

`ITimeVarying<T>` requires `Sample(time: Duration)`. That time is elapsed time from the owning object's start, not a civil timestamp. An `Instant` and a `Duration` have different roles even when both carry seconds.

## Sample time is an offset

An easing maps normalized time to progress. The intended endpoint values are zero and one. Intermediate progress may leave the unit interval. An easing is not a probability, and it is not a clamp.

For a tween, let `e = time.Seconds - Delay.Seconds` and `d = Duration.Seconds`. When `d` is positive, `u = clamp(e/d, 0, 1)`, and the sample is `Lerp(From, To, Easing.Eval(u))`. Before the delay the sample is the start value. After the duration it is the end value, subject to the easing's endpoint behavior. A nonpositive duration switches at the delay. The easing input is 0 before the delay and 1 at or after it. An easing that overshoots can leave the interval from `From` to `To` even though `u` was clamped.

## Frequency, phase, and the Change helper

An oscillator has a bias, an amplitude, a frequency, and a phase. Frequency counts cycles per second. Phase is an angle. The waveform bodies share a phase convention, so changing the waveform keeps that phase reference. A time remap changes when another animation is sampled. An easing changes the normalized progress of a blend.

Layer, timeline, repeater, echo, stagger, and frame-sequence records describe a composition. The motion body file samples tweens and oscillators. It does not interpret every timeline description. Keyframes, tracks, and skeletons are in the future tier, which is not part of the default library.

The generic `Change` helper subtracts two samples. `ITimeVarying<T>` does not declare an additive bound on `T`. Sampling a value and subtracting two values are different capabilities. Check compilation for the concrete `T` before assuming every instantiation supports `Change`.

## Where to look

- [Easing interface](../../api/graphics/easing.concepts.md), [easing records](../../api/graphics/easing.types.md), and [easing bodies](../../api/graphics/easing.library.md)
- [Time-varying interface](../../api/graphics/time-varying.concepts.md) and [time helpers](../../api/graphics/time-varying.library.md)
- [Motion records](../../api/graphics/motion-graphics.types.md) and [motion bodies](../../api/graphics/motion-graphics.library.md)
- [Animation and signals](../future/animation-signals.md)

[SVG 2](https://www.w3.org/TR/SVG2/paths.html) also uses geometry for animation paths. Elapsed-time sampling, easing, and path data stay separate here, and none of them is a browser animation scheduler. The sampler returns a value when called. A host chooses the clock and the frame cadence.
