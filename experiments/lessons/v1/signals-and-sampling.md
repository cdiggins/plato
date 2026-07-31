---
lesson: signals-and-sampling
title: Signals and Sampling
domain: Math, statistics & signals
v3-files: [60-signals.plato]
audience: Basic trig (sine waves) and general programming background
status: draft-v1
---

# Signals and Sampling

A microphone voltage wiggles continuously. A computer stores a list of numbers. Between
those worlds sits **sampling**: measure the signal at regular times, then pretend the list
*is* the signal. Do it carefully and you can reconstruct what mattered. Do it carelessly
and you get aliasing — fake frequencies that were never in the room, or the shimmering
moiré on a striped shirt in a digital photo. Audio and images are the same mathematics in
different costumes.

## The idea

### Discrete time

A uniformly sampled scalar signal stores samples $s[i]$ at times

$$
t_i = i / f_s
$$

where $f_s$ is the **sample rate** (hertz). Higher $f_s$ means finer time resolution and
more data.

```
  continuous s(t)     samples at 1/fs
       ╱╲╱╲╱            •  •  •  •  •
      ╱    ╲           •           •
```

### Frequency domain and the spectrum

Any reasonable signal can be seen as a sum of sinusoids. A **spectrum** reports how much
energy (magnitude) and at what phase lives in each frequency bin. Bin $k$ sits at
frequency $k \cdot \Delta f$ with $\Delta f$ the bin width; bin $0$ is DC (the average).

A **spectrogram** is spectrum-vs-time: short windows hopped along the signal, each
producing a column of magnitudes — the familiar audio "waterfall."

### Nyquist and aliasing

The **Nyquist–Shannon** story (in slogan form): to capture frequencies up to $f_{\max}$,
you need

$$
f_s > 2\, f_{\max}
$$

Frequencies above the Nyquist frequency $f_s/2$ fold back into lower bands — **aliases**.
A $5\,\mathrm{kHz}$ tone sampled at $6\,\mathrm{kHz}$ can appear as a $1\,\mathrm{kHz}$
ghost. Anti-alias filters (low-pass before sampling or before downsampling) are how studios
and renderers stay honest.

```
  true high frequency          after undersampling
  ∿∿∿∿∿∿∿∿∿∿∿∿∿∿∿              ∿     ∿     ∿
                               looks like a slow wave
```

Image moiré is spatial aliasing: the camera grid samples a fine pattern at too low a
spatial rate.

### Windows, filters, envelopes

Chopping a signal into blocks without tapering leaks energy across bins. **Window
functions** (Hann, Hamming, Kaiser, …) taper edges. **Biquad** / FIR / IIR filters shape
which frequencies pass. **ADSR** envelopes shape amplitude over note lifetimes. Waveform
generators (sine, square, saw) and noise colors (white, pink, brown, blue) are pure
descriptions of sources you can render into a `SampledSignal`.

### Resampling and crossfades

Changing rate needs interpolation: hold, linear, cubic Hermite, or windowed sinc (higher
quality, heavier). Crossfades mix two signals with gain laws (linear, equal-power,
S-curve) so transitions do not click or dip.

## In Plato

File `60-signals.plato` declares sampled signals, spectra, windows, filters, and
generators.

### Core signals and spectra

```plato
type SampledSignal
    implements Value
{
    SampleRate: Frequency;
    Samples: Array<Number>;
}

type MultichannelSignal
    implements Value
{
    SampleRate: Frequency;
    ChannelCount: Integer;
    Samples: Array<Number>;   // frame-interleaved
}

type Spectrum
    implements Value
{
    BinWidth: Frequency;
    Magnitudes: Array<Number>;
    Phases: Array<Angle>;
}

type ComplexSpectrum
    implements Value
{
    BinWidth: Frequency;
    Bins: Array<Complex>;
}

type Spectrogram
    implements Value
{
    TimeStep: Duration;
    BinWidth: Frequency;
    Magnitudes: Array2D<Number>;  // column = time, row = frequency
}
```

### Windows and filters

```plato
type WindowFunction
    implements Value
    = Rectangular
    | Hann
    | Hamming
    | Blackman
    | BlackmanHarris
    | Kaiser(Beta: Number)
    | Gaussian(StandardDeviation: Number)
    | Tukey(TaperFraction: Proportion);

type AnalysisWindow
    implements Value
{
    Function: WindowFunction;
    Size: Integer;
    HopSize: Integer;
}

type BiquadResponse
    = LowPass | HighPass | BandPass | Notch | AllPass
    | PeakingEq(GainDecibels: Number)
    | LowShelf(GainDecibels: Number)
    | HighShelf(GainDecibels: Number);

type BiquadFilter
    implements Value
{
    Response: BiquadResponse;
    Frequency: Frequency;
    Q: Number;
}

type FirFilter
    implements Value
{
    Taps: Array<Number>;
}

type IirFilter
    implements Value
{
    FeedforwardCoefficients: Array<Number>;
    FeedbackCoefficients: Array<Number>;  // [0] normalized to 1
}
```

### Generators, resampling, derived forms

```plato
type AdsrEnvelope
    implements Value
{
    Attack: Duration;
    Decay: Duration;
    Sustain: Proportion;
    Release: Duration;
}

type NoiseColor = White | Pink | Brown | Blue;

type NoiseSignal
    implements Value
{
    Color: NoiseColor;
    Amplitude: Number;
    Seed: Integer;
}

type Waveform
    = Sine | Square | Triangle | Sawtooth
    | Pulse(DutyCycle: Proportion);

type WaveformGenerator
    implements Value
{
    Waveform: Waveform;
    Frequency: Frequency;
    Amplitude: Number;
    Phase: Angle;
}

type SignalResampling = ZeroOrderHold | Linear | CubicHermite | WindowedSinc;

type SampleRateConversion
    implements Value
{
    SourceRate: Frequency;
    TargetRate: Frequency;
    Interpolation: SignalResampling;
}

type CrossfadeLaw = Linear | EqualPower | SCurve;

type CrossfadeParameters
    implements Value
{
    Law: CrossfadeLaw;
    Duration: Duration;
}

type AnalyticSignal
    implements Value
{
    SampleRate: Frequency;
    InPhase: Array<Number>;
    Quadrature: Array<Number>;
}

type FrequencyBand
    implements Value
{
    Low: Frequency;
    High: Frequency;
}
```

Usage-shaped sketches:

```plato
let tone = WaveformGenerator {
    Waveform: Sine,
    Frequency: ...,
    Amplitude: 0.5,
    Phase: Angle { Radians: 0 }
};

// After rendering one second at 48 kHz:
let signal = SampledSignal {
    SampleRate: ...,
    Samples: ...
};

let analysis = AnalysisWindow {
    Function: Hann,
    Size: 1024,
    HopSize: 512
};

let antiAlias = BiquadFilter {
    Response: LowPass,
    Frequency: ...,   // below new Nyquist before downsample
    Q: 0.707
};

let convert = SampleRateConversion {
    SourceRate: ...,
    TargetRate: ...,
    Interpolation: WindowedSinc
};
```

## Pitfalls / fine print

**Nyquist is a sharp slogan, soft in practice.** Real filters are not bricks; leave
headroom below $f_s/2$. Ignoring that is how aliases sneak in near the top of the band.

**Window size vs hop.** Huge hops with tiny windows smash time resolution; tiny hops with
huge windows cost CPU and smear transients. `HopSize` greater than `Size` skips audio.

**Interleaving.** `MultichannelSignal` is frame-interleaved; treating it as planar channels
swaps ears and ruins FFT framing.

**IIR feedback.** `FeedbackCoefficients[0]` must stay 1; unstable coefficient sets scream
(literally).

**Decibels on shelves.** Peaking/shelf gains are in dB — passing linear gain ratios will
overshoot wildly.

**Noise seed.** `NoiseSignal.Seed` makes noise deterministic; forgetting to vary seeds
correlates "random" layers.

**Equal-power vs linear crossfade.** Linear gain crossfades dip in perceived loudness at
the midpoint for uncorrelated signals; equal-power is often preferred for audio.

## Try it

1. Sample rate $48\,\mathrm{kHz}$. Roughly what is the Nyquist frequency?
2. You downsample to $24\,\mathrm{kHz}$ without low-passing. What risk appears?
3. Spectrum bin width is $46.875\,\mathrm{Hz}$ and you look at bin $k = 10$. What center
   frequency is that (DC = bin 0)?

<details>
<summary>Answers</summary>

1. About $24\,\mathrm{kHz}$ ($f_s/2$).
2. Content between $12$–$24\,\mathrm{kHz}$ (relative to the old rate) can alias into the
   new audible band.
3. $10 \times 46.875 = 468.75\,\mathrm{Hz}$.

</details>

## Library recommendations

- **missing-function** — `60-signals.plato`: `SampledSignal`, `Spectrum`, and
  `WaveformGenerator` have no `Render`, `Fft`, `Apply(BiquadFilter)`, or `Resample`
  operations. Teaching Nyquist needs those verbs; only parameter records exist.

- **missing-type** — `60-signals.plato`: no explicit `NyquistLimit` helper or
  `AliasingRisk` doc-tied type linking `Frequency` sample rates to valid tone frequencies.
  Pedagogy invents the inequality $f_s > 2 f_{\max}$ with nowhere to hang it.

- **wrong-shape** — `60-signals.plato`: `Spectrogram.Magnitudes` uses column = time, row =
  frequency — the opposite of some image conventions (row-major time). A louder banner
  comment would prevent transposed visualizations.

- **doc-comment** — `60-signals.plato`: `SampledSignal` states sample $i$ at $i/f_s$ but
  does not state whether the first sample is at $t=0$ inclusive for duration
  `Count/SampleRate` vs `(Count-1)/SampleRate`. Fencepost ambiguity shows up immediately
  when teaching duration.
