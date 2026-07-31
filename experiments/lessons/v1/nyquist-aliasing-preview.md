---
lesson: nyquist-aliasing-preview
title: Nyquist Rate and Aliasing
domain: Math, statistics & signals
v3-files: [06-quantities.plato, 60-signals.plato]
audience: High-school trig and general programming background
status: draft-v1
---

# Nyquist Rate and Aliasing

Film a spinning wheel with a camera and sometimes the spokes crawl backward. Sample a
high musical pitch too slowly and it reappears as a lower tone. Both are **aliasing**:
distinct continuous signals that become indistinguishable once you only keep a grid of
samples. The **Nyquist rate** is the sampling-speed threshold that separates faithful
capture from this collapse.

The same mathematics governs audio sample rates, image moiré on fine grids, and
temporal shimmer on thin geometry. If you sample, you inherit Nyquist.

## The idea

A pure tone $\sin(2\pi f t)$ oscillates at frequency $f$ (hertz). Sample it at rate
$f_s$ — one number every $1/f_s$ seconds:

$$
s[n] = \sin\!\left(2\pi f \cdot \frac{n}{f_s}\right), \quad n = 0,1,2,\ldots
$$

Two frequencies $f$ and $f'$ produce the **same** sample sequence when they differ by
an integer multiple of $f_s$, or when one is the mirror of the other across a multiple
of $f_s/2$. The classic statement:

> To reconstruct a signal that contains no energy above $f_{\max}$, you must sample
> strictly faster than $2 f_{\max}$.

That threshold $2 f_{\max}$ is the **Nyquist rate**. Equivalently, with a fixed $f_s$,
the **Nyquist frequency** $f_s/2$ is the highest frequency you can uniquely represent.

```
 frequency axis
 0        fs/2       fs       3fs/2
 |---------|---------|---------|
   usable     aliases fold back
   band       into the usable band
```

**Folding picture.** Content above $f_s/2$ mirrors down into $[0, f_s/2]$. A tone at
$0.6 f_s$ appears as a tone at $0.4 f_s$. A tone at $0.9 f_s$ appears near $0.1 f_s$.
The wheel that "spins backward" is a temporal alias of a rotation faster than half the
frame rate.

**Anti-aliasing before sampling.** Once aliased, the damage is baked into the discrete
sequence — no clever interpolator recovers the true high frequency. The fix is to
*low-pass filter* (band-limit) before downsampling, then sample. Upsampling needs
interpolation; downsampling needs filtering first.

**Images.** Spatial frequency is cycles per pixel. A checkerboard finer than one cycle
per two pixels is above Nyquist for the pixel grid; under a box filter it becomes gray
moiré or crawl. Mipmaps and area sampling are anti-alias filters in disguise.

## In Plato

`60-signals.plato` models uniformly sampled signals with an explicit `SampleRate` of
type `Frequency` (hertz, from `06-quantities.plato`).

```plato
type Frequency implements Quantity { Hertz: Number; }

type SampledSignal
    implements Value
{
    SampleRate: Frequency;
    Samples: Array<Number>;
}

// Sample i occurs at time i / SampleRate   (file convention)
```

A multichannel buffer is the same idea with interleaved frames:

```plato
type MultichannelSignal
{
    SampleRate: Frequency;
    ChannelCount: Integer;
    Samples: Array<Number>;
}
```

Spectral views make the Nyquist bin explicit: bin $k$ sits at $k \cdot \mathrm{BinWidth}$,
with bin 0 at DC. For a length-$N$ real FFT of a signal sampled at $f_s$, the last
unique positive-frequency bin is near $f_s/2$.

```plato
type Spectrum
{
    BinWidth: Frequency;
    Magnitudes: Array<Number>;
    Phases: Array<Angle>;
}

type ComplexSpectrum
{
    BinWidth: Frequency;
    Bins: Array<Complex>;
}
```

Resampling requests name the interpolation — but they do not by themselves guarantee
anti-alias filtering when `TargetRate < SourceRate`:

```plato
type SignalResampling = ZeroOrderHold | Linear | CubicHermite | WindowedSinc;

type SampleRateConversion
{
    SourceRate: Frequency;
    TargetRate: Frequency;
    Interpolation: SignalResampling;
}
```

`WindowedSinc` is the interpolator associated with band-limited reconstruction ideals;
cheap modes (`ZeroOrderHold`, `Linear`) are not anti-alias filters when decimating.

Waveform generators describe continuous-time intent; sampling them into a
`SampledSignal` at too low a rate is exactly how you manufacture aliases in a synth:

```plato
type WaveformGenerator
{
    Waveform: Waveform;
    Frequency: Frequency;
    Amplitude: Number;
    Phase: Angle;
}
```

Analysis windows (`WindowFunction`, `AnalysisWindow`) taper blocks before FFTs to
control spectral leakage — a different artifact from aliasing, often confused with it.

## Pitfalls / fine print

**Equals Nyquist is not enough.** The theorem needs content *strictly below* $f_s/2$.
Energy exactly at $f_s/2$ is a razor-edge case; practical systems leave headroom and
filter earlier.

**Nyquist rate vs Nyquist frequency.** Rate $= 2 f_{\max}$ (samples per second needed).
Frequency $= f_s/2$ (highest representable Hertz at a given $f_s$). Mixing the names
in APIs causes off-by-two bugs.

**Aliasing ≠ quantization.** Quantization is amplitude rounding (bit depth). Aliasing
is frequency folding from an insufficient *rate*. You can have pristine 24-bit samples
of a completely wrong pitch.

**Decimation without a low-pass.** Taking every $k$-th sample of a `SampledSignal` drops
the rate by $k$ and aliases unless you filter first. `SampleRateConversion` should be
read as a *request*; safe decimation is a pipeline, not a single lerp.

**Spectrogram time-frequency trade.** `Spectrogram` uses `TimeStep` and `BinWidth`.
Short windows give poor frequency resolution; that blur is not aliasing, though both
can make plots look "wrong."

**No named Nyquist helper.** v3 has no `NyquistFrequency(signal)` field or function.
Compute `SampleRate.Hertz / 2` yourself and keep units on `Frequency`.

## Try it

1. Audio at $f_s = 48\,\mathrm{kHz}$. What is the Nyquist frequency?
2. A $5\,\mathrm{kHz}$ tone sampled at $6\,\mathrm{kHz}$. What aliased frequency in
   $[0, 3\,\mathrm{kHz}]$ do you hear?
3. You downsample $48\,\mathrm{kHz}$ material to $16\,\mathrm{kHz}$ with linear
   interpolation only. What did you skip?

<details>
<summary>Answers</summary>

1. $24\,\mathrm{kHz}$.
2. Fold across $3\,\mathrm{kHz}$: distance above Nyquist is $2\,\mathrm{kHz}$, so the
   alias sits at $3 - 2 = 1\,\mathrm{kHz}$.
3. A low-pass (anti-alias) filter cutting content above $8\,\mathrm{kHz}$ before
   decimation. Linear interpolation does not remove those frequencies.

</details>

## Library recommendations

- **missing-function** — `60-signals.plato`: no `NyquistFrequency(s: SampledSignal): Frequency`
  (or on `Frequency` alone: `Nyquist(fs)`). Every sampling lesson and every safe
  resampler needs this one-liner as a named operation.

- **missing-type** — `60-signals.plato`: `SampleRateConversion` names interpolation but
  not an anti-alias policy for decimation. A field such as
  `AntiAlias: BiquadFilter | FirFilter | None` (or a dedicated sum) would make the
  Nyquist requirement visible in the type.

- **doc-comment** — `60-signals.plato`: `SampledSignal` should mention that
  representable content lies in $[0, \mathrm{SampleRate}/2)$ and that constructing
  samples from `WaveformGenerator` above that band aliases.

- **pedagogy** — `60-signals.plato`: `SignalResampling.WindowedSinc` doc should state
  whether implementations are expected to low-pass on downsample or only interpolate
  on upsample — the aliasing preview cannot be taught honestly without that contract.
