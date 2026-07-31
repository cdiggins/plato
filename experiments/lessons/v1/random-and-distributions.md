---
lesson: random-and-distributions
title: Random Numbers and Distributions
domain: Math, statistics & signals
v3-files: [59-random.plato]
audience: Basic probability (mean, variance, "bell curve") and general programming background
status: draft-v1
---

# Random Numbers and Distributions

Roll a die in a game, jitter a particle's lifetime, or bootstrap a Monte Carlo integral —
you need randomness that is **repeatable** when you replay a bug, and **statistically
shaped** when "uniform between 0 and 1" is the wrong shape. The first need is a pure
generator state. The second is a library of probability distributions: parameter records
that know their density, cumulative probability, mean, and variance.

## The idea

### Pure functional RNG

Classic APIs mutate a global seed: call `rand()`, get a float, world state changes behind
your back. That fights replay, parallelism, and testing. A **value-typed** generator
carries its state in a record. Drawing a number returns the sample **and** a new state.
Same inputs → same outputs. Streams (independent sequences) let systems share one algorithm
without correlating their draws.

```
  (state₀, stream) ──draw──► (u₀, state₁)
  (state₁, stream) ──draw──► (u₁, state₂)
         ...
```

Pseudo-random generators are deterministic algorithms. "Random" means statistically
hard to distinguish from ideal noise for the application's purposes — not metaphysical
unpredictability.

### Uniform vs shaped noise

A raw generator typically yields uniforms on $[0,1]$ or bit patterns. Almost every
interesting use **transforms** uniforms into another law:

| Need | Typical law |
|------|-------------|
| "Any direction equally likely" | uniform on an interval / sphere |
| "Cluster near a mean" | normal (Gaussian) |
| "Waiting time for the next event" | exponential |
| "Success / fail" | Bernoulli |
| "Count of rare events" | Poisson |

The **probability density** $f(x)$ (continuous) or **mass** $p(k)$ (discrete) says how
likely outcomes are. The **CDF** $F(x) = P(X \le x)$ is the integral/sum of that density.
Means and variances summarize location and spread — when they exist (Cauchy has neither in
the usual sense).

### Normal intuition

The Gaussian

$$
f(x) = \frac{1}{\sigma\sqrt{2\pi}}
\exp\bigl(-(x-\mu)^2/(2\sigma^2)\bigr)
$$

is the "bell curve": $\mu$ centers it, $\sigma$ widens it. Central-limit folklore explains
why measurement noise and sums of small effects look normal. Log-normal is what you get
when the **logarithm** is normal — good for positive skewed quantities (sizes, incomes).

### Seeding and reproducibility

A seed chooses the initial state. Fix the seed → identical sequences (demos, unit tests,
bug repro). Change the stream index → an independent lane of randomness without changing
the algorithm. Never seed from "current time" in tests if you want assertions to hold
tomorrow.

## In Plato

File `59-random.plato` separates generator state from distribution parameters.

### RandomState

```plato
type RandomState
    implements Value, Hashable
{
    State: Integer;
    Stream: Integer;
}
```

Doc contract: PCG-style; same `(State, Stream)` always yields the same next value; each
draw returns a new `RandomState`. Sampling operations are deferred to a later pass — the
vocabulary today is the state record plus distributions.

### Univariate concept

```plato
concept ProbabilityDistribution
    inherits Value
{
    Mean(x: Self): Number;
    Variance(x: Self): Number;
    Pdf(x: Self, value: Number): Number;
    Cdf(x: Self, value: Number): Number;
}
```

Continuous examples:

```plato
type UniformDistribution
    implements ProbabilityDistribution
{
    Min: Number;
    Max: Number;
}

type NormalDistribution
    implements ProbabilityDistribution
{
    Mean: Number;
    StandardDeviation: Number;
}

type ExponentialDistribution
    implements ProbabilityDistribution
{
    Rate: Number;   // mean = 1 / Rate
}

type BetaDistribution
    implements ProbabilityDistribution
{
    Alpha: Number;
    Beta: Number;
}

type VonMisesDistribution
    implements ProbabilityDistribution
{
    MeanDirection: Angle;
    Concentration: Number;   // 0 → uniform on the circle
}
```

Discrete examples (`Pdf` is mass at the value rounded down):

```plato
type BernoulliDistribution
    implements ProbabilityDistribution
{
    SuccessProbability: Probability;
}

type BinomialDistribution
    implements ProbabilityDistribution
{
    TrialCount: Integer;
    SuccessProbability: Probability;
}

type PoissonDistribution
    implements ProbabilityDistribution
{
    Rate: Number;
}

type DiscreteDistribution
    implements ProbabilityDistribution
{
    Weights: Array<Number>;  // need not be normalized
}
```

Multivariate normals are separate — they are not univariate `ProbabilityDistribution`:

```plato
type NormalDistribution2D
    implements Value
{
    Mean: Vector2D;
    Covariance: Covariance2D;
}

type NormalDistribution3D
    implements Value
{
    Mean: Vector3D;
    Covariance: Covariance3D;
}
```

Usage-shaped sketches (illustrative):

```plato
let rng0 = RandomState { State: 12345, Stream: 0 };
// let (u, rng1) = NextUniform(rng0);   // not declared yet — see recommendations

let lifetimeLaw = UniformDistribution { Min: 0.5, Max: 2.0 };
let speedLaw = NormalDistribution { Mean: 10, StandardDeviation: 1.5 };
let sparkChance = BernoulliDistribution {
    SuccessProbability: ...
};

let Mean(speedLaw);       // 10
let Variance(speedLaw);   // 1.5²
let Pdf(speedLaw, 10);    // peak density
let Cdf(speedLaw, 10);    // ≈ 0.5
```

Also declared: `LogNormalDistribution`, `GammaDistribution`, `ChiSquaredDistribution`,
`StudentTDistribution`, `CauchyDistribution`, `LaplaceDistribution`,
`WeibullDistribution`, `ParetoDistribution`, `TriangularDistribution`,
`GeometricDistribution`.

## Pitfalls / fine print

**Uniform on [Min, Max] with Max ≤ Min.** Degenerate or inverted ranges need a policy
(swap, empty, error). The type does not encode `Max > Min` in the type system.

**Normal with σ ≤ 0.** Standard deviation must be positive; zero is a Dirac spike, negative
is nonsense. Same story for many scale parameters.

**Cauchy moments.** `ProbabilityDistribution` always exposes `Mean` and `Variance`; for
Cauchy the docs say they conventionally report location and $+\infty$. Callers that assume
finite variance (e.g. standardization) break.

**Von Mises Pdf/Cdf take radians.** `MeanDirection` is `Angle`, but the concept's `Pdf`
value parameter is `Number`. Teaching circular stats must track that unit bridge carefully.

**Discrete Pdf.** Mass is at `floor(value)`. Passing a continuous sample index without
flooring intent misreads the mass function.

**Weights vs probabilities.** `DiscreteDistribution.Weights` need not sum to 1 — forgetting
to normalize when interpreting raw weights as probabilities double-counts.

**Missing sample API.** Until draw functions exist, holding a `NormalDistribution` does not
by itself produce numbers; you need the future pairing with `RandomState`.

## Try it

1. `UniformDistribution { Min: 2, Max: 6 }`: what are `Mean` and `Variance`?
2. Why does fixing `RandomState` make a flaky particle test become deterministic?
3. Name one reason to use `ExponentialDistribution` instead of `UniformDistribution` for
   "time until something happens."

<details>
<summary>Answers</summary>

1. Mean $(2+6)/2 = 4$; variance $(6-2)^2/12 = 16/12 = 4/3$.
2. The same state/stream replay the same draws; failures become reproducible.
3. Memoryless waiting times and heavier near-zero probability mass match Poisson processes;
   a uniform wait treats every delay in a window as equally likely.

</details>

## Library recommendations

- **missing-function** — `59-random.plato`: `RandomState` documents draws that return a new
  state, but no `NextUnitInterval`, `NextInteger`, or `Sample(distribution, rng)` pair is
  declared. The entire teaching punchline ("pure draw") has no verb on the surface.

- **missing-concept** — `59-random.plato`: `NormalDistribution2D` / `3D` cannot implement
  `ProbabilityDistribution` (univariate Pdf). A `MultivariateDistribution` concept with
  `Pdf(Self, VectorND)` would give the Gaussians a home and clarify why they are split out.

- **naming** — `59-random.plato`: `NormalDistribution` has a field also named `Mean`, while
  the concept function is `Mean(x: Self)`. Teaching "the mean parameter vs the Mean
  operation" is fine but easy to confuse in prose and in generated APIs.

- **doc-comment** — `59-random.plato`: `VonMisesDistribution` says Pdf/Cdf take radians and
  `Mean` reports radians, yet `MeanDirection` is `Angle`. Spell the conversion expectation
  next to the concept mismatch so implementors and lessons agree.
