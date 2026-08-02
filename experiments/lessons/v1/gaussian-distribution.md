---
lesson: gaussian-distribution
title: The Gaussian Distribution
domain: Math, statistics & signals
v3-files: [05-numbers.plato, 59-random.plato]
audience: High-school probability and general programming background
status: draft-v1
---

# The Gaussian Distribution

Roll a fair die once and the outcomes are flat. Roll it a thousand times and add the
results, and a smooth bell appears. That shape — tall in the middle, thin in the tails —
is the **Gaussian**, or **normal**, distribution. It is the default noise model in
science, graphics, and simulation because so many independent small effects add up to
something that looks like this curve, even when each effect is not itself Gaussian.

If you have ever written `mean + sigma * randn()` to jitter a particle, sample a soft
shadow, or perturb a camera, you were drawing from a normal distribution. The
parameters are few; the consequences are everywhere.

## The idea

A continuous distribution on the real line is described by a **probability density
function** (PDF). For a value $x$, the PDF height is not a probability — it is density.
Probability lives in *areas* under the curve:

$$
P(a \le X \le b) = \int_a^b \mathrm{pdf}(x)\,dx
$$

The **cumulative distribution function** (CDF) is that integral from $-\infty$ to $x$:

$$
\mathrm{cdf}(x) = P(X \le x)
$$

The Gaussian with mean $\mu$ and standard deviation $\sigma > 0$ has PDF

$$
\mathrm{pdf}(x) = \frac{1}{\sigma\sqrt{2\pi}}
\exp\left(-\frac{(x-\mu)^2}{2\sigma^2}\right)
$$

```
 pdf
  ^
  |        .-.
  |       /   \
  |      /     \
  |     /       \
  |----/---------\----> x
           μ
```

Properties worth memorizing:

- **Location:** $\mu$ shifts the peak; it is also the mean, median, and mode.
- **Scale:** $\sigma$ stretches the curve horizontally and lowers it so the area stays 1.
- **Variance** is $\sigma^2$.
- Roughly **68%** of mass lies in $[\mu-\sigma,\,\mu+\sigma]$, **95%** in
  $[\mu-2\sigma,\,\mu+2\sigma]$, **99.7%** in $[\mu-3\sigma,\,\mu+3\sigma]$ (the
  "68–95–99.7 rule").
- The **standard normal** is $\mu = 0$, $\sigma = 1$. Any other Gaussian is an
  affine transform of that: $X = \mu + \sigma Z$ with $Z$ standard normal.

The **central limit theorem** is why the shape keeps showing up: sums of many
independent random contributions with finite variance converge toward a Gaussian,
regardless of the individual shapes (under mild conditions). That is the mathematical
excuse for treating measurement error, Brownian motion increments, and diffuse
scattering as "normal noise."

Related cousins in the same family:

- **Log-normal:** a positive variable whose *logarithm* is normal (multiplicative effects).
- **Student's $t$:** heavier tails; approaches the normal as degrees of freedom grow.
- **Chi-squared:** sum of squared standard normals.
- **Bivariate / trivariate normals:** jointly Gaussian vectors with a covariance matrix.

## In Plato

File `59-random.plato` separates **generator state** from **distribution parameters**.
A `RandomState` is a pure PCG-style value: each draw conceptually returns a new state.
Distributions are parameter records; they implement `ProbabilityDistribution` for the
univariate line.

```plato
type RandomState
    implements Value, Hashable
{
    State: Integer;
    Stream: Integer;
}

interface ProbabilityDistribution
    inherits Value
{
    Mean(x: Self): Number;
    Variance(x: Self): Number;
    Pdf(x: Self, value: Number): Number;
    Cdf(x: Self, value: Number): Number;
}

type NormalDistribution
    implements ProbabilityDistribution
{
    Mean: Number;
    StandardDeviation: Number;
}
```

Usage-shaped expressions (illustrative — bodies are not in v3 yet):

```plato
// Standard normal: mean 0, unit variance
let z = NormalDistribution(0, 1);

// Measurement noise around a sensor reading of 20 with σ = 0.5
let noise = NormalDistribution(20, 0.5);

// Density and cumulative probability at a point
Pdf(noise, 20.0)          // peak height ≈ 0.798
Cdf(noise, 20.0)          // 0.5 — half the mass is below the mean
Cdf(noise, 21.0)          // ≈ 0.977 — about two σ above the mean
Variance(noise)           // 0.25
Mean(noise)               // 20
```

A log-normal for multiplicative jitter uses the underlying normal's parameters on the
log scale:

```plato
let sizes = LogNormalDistribution(0, 0.25);
// LogMean = 0 → median of the positive variable is exp(0) = 1
```

For planar and spatial Gaussians (not univariate, so they do **not** implement
`ProbabilityDistribution`):

```plato
type NormalDistribution2D
{
    Mean: Vector2D;
    Covariance: Covariance2D;
}

type NormalDistribution3D
{
    Mean: Vector3D;
    Covariance: Covariance3D;
}
```

`Probability` from `05-numbers.plato` wraps a number in $[0,1]$ for discrete success
probabilities; continuous PDF values are raw `Number` densities and need not lie in
$[0,1]$.

## Pitfalls / fine print

**PDF vs probability.** `Pdf(dist, x)` can be greater than 1 when $\sigma$ is small.
That is legal: density, not mass. Only integrals (or CDFs) are probabilities.

**Standard deviation vs variance.** `NormalDistribution` stores `StandardDeviation`.
The interface reports `Variance` as $\sigma^2$. Confusing the two when seeding from a
variance estimate is a classic off-by-square bug.

**Field name `Mean` vs interface function `Mean`.** The type has a field `Mean` and the
interface declares `Mean(x: Self)`. For a normal they coincide, but the dual naming is
easy to trip over when writing libraries or bindings.

**Zero or negative σ.** The Gaussian is only defined for $\sigma > 0$. v3 does not yet
encode that invariant in the type; callers must guard.

**Tails are thin but not zero.** Extreme outliers are rare under a normal model, but
real data often has heavier tails. Prefer `StudentTDistribution` or robust estimators
when "almost never" events actually happen.

**Sampling is not declared.** v3 says draws pair a distribution with `RandomState` in a
later pass. Do not invent a `Sample` name in client code until the library surface
exists — parameterize the distribution and keep the RNG state separate.

**Multivariate normals are different.** `NormalDistribution2D`/`3D` use covariance
types from statistics; they intentionally skip `ProbabilityDistribution` because that
interface is univariate (`Pdf` takes a single `Number`).

## Try it

1. For `NormalDistribution(10, 2)`, which interval contains about 95% of the mass?
2. If $Z \sim N(0,1)$ and $X = 5 + 3Z$, what are $\mu$ and $\sigma$ of $X$?
3. Why can `Pdf(NormalDistribution(0, 0.1), 0)` exceed 1?

<details>
<summary>Answers</summary>

1. Roughly $[10 - 4,\, 10 + 4] = [6, 14]$ (the $\mu \pm 2\sigma$ rule).
2. $\mu = 5$, $\sigma = 3$ — affine transform of the standard normal.
3. PDF is density. A narrow spike packs probability into a small width, so height grows
   as $1/\sigma$; the area under the curve remains 1.

</details>

## Library recommendations

- **missing-function** — `59-random.plato`: `ProbabilityDistribution` has `Pdf`/`Cdf`/
  `Mean`/`Variance` but no declared `Sample(dist, rng: RandomState) -> (Number, RandomState)`
  (or equivalent). The file's own header says sampling pairs distribution with
  `RandomState` later; teaching the Gaussian immediately needs that pairing signature.

- **naming** — `59-random.plato`: `NormalDistribution.Mean` (field) collides conceptually
  with `ProbabilityDistribution.Mean(Self)`. Prefer renaming the field to `Location` or
  documenting that interface `Mean` must equal the field for this type, to reduce binder
  and pedagogy confusion.

- **missing-function** — `59-random.plato`: no `Standardize(x, dist) -> Number` or
  `FromStandard(z, dist) -> Number` helpers for the $z = (x-\mu)/\sigma$ transform that
  every Gaussian lesson (and every Box–Muller / inverse-CDF sampler) uses.

- **doc-comment** — `59-random.plato`: `NormalDistribution` should state the invariant
  `StandardDeviation > 0` and that `Variance` returns its square, matching the file's
  sample-statistics conventions elsewhere.
