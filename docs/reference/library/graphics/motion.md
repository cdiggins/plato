# Easing and time-varying graphics

Read [easing interface](../../api/graphics/easing.concepts.md), [easing records](../../api/graphics/easing.types.md), [easing bodies](../../api/graphics/easing.library.md), [time-varying interface](../../api/graphics/time-varying.concepts.md), [time helpers](../../api/graphics/time-varying.library.md), [motion records](../../api/graphics/motion-graphics.types.md) and [motion bodies](../../api/graphics/motion-graphics.library.md).

## Sample an offset from an owner's start

`ITimeVarying<T>` requires `Sample(time: Duration)`. This is elapsed time relative to the owning object, not an absolute civil timestamp. An `Instant` and a `Duration` have different roles even if both carry seconds.

An easing maps normalized time to progress. Its intended endpoint values are zero and one, but intermediate progress may overshoot the unit interval. An easing is therefore not generally a probability or a clamping function.

For a tween, let `e = time.Seconds - Delay.Seconds` and `d = Duration.Seconds`. With positive `d`, compute `u = clamp(e/d,0,1)`; return `Lerp(From,To,Easing.Eval(u))`. Before the delay it samples the start endpoint; after the duration it samples the end endpoint, subject to easing endpoint behavior. With nonpositive duration it switches instantaneously at the delay. Overshooting easing can exceed the endpoint values even though `u` was clamped.

## Parameter names carry scale

Oscillators combine a bias, amplitude, frequency and phase. Frequency counts cycles per second; phase is angular. The waveform bodies align their phase conventions, so a waveform change preserves the intended phase reference. A time remap changes when another animation is sampled, whereas an easing changes a blend's normalized progress.

Layer, timeline, repeater, echo, stagger and frame-sequence records describe composition. The motion body file contains concrete tween and oscillator sampling; it does not automatically interpret every timeline description. Keyframes, tracks and skeletons are in the [future tier](../future/animation-signals.md).

**Interface gap:** the generic `Change` helper subtracts two samples even though `ITimeVarying<T>` does not declare an additive bound. Sampling a value and subtracting two values require different capabilities. Inspect compilation for a concrete instantiation rather than assuming every `T` supports change.

## Related work

[SVG 2](https://www.w3.org/TR/SVG2/paths.html) also uses geometry for animation paths. Plato separates elapsed-time sampling, easing and path data; it does not inherit a browser animation scheduler merely by using corresponding concepts. The pure sampler produces a value when called; an external host chooses the clock and frame cadence.
