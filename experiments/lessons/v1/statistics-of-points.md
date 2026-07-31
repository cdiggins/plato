---
lesson: statistics-of-points
title: Statistics of Points
domain: Math, statistics & signals
v3-files: [58-statistics.plato]
audience: High-school mean/variance and general programming background
status: draft-v1
---

# Statistics of Points

You scan a room with a depth camera and get a cloud of 3D points. Where is the "middle"?
How spread out is the cloud? Are two coordinates correlated? Is this blob roughly Gaussian
or heavy-tailed? Descriptive statistics answer those questions without yet fitting a fancy
model — and the same summaries apply to one-dimensional samples (temperatures, scores)
and to coordinates pulled from geometry.

## The idea

### Center: mean vs median vs medoid

For numbers $x_1,\ldots,x_n$:

- **Mean** $\bar{x} = (\sum x_i)/n$ — balances signed deviations; sensitive to outliers
- **Median** — middle value after sorting; robust to extremes
- **Mode** — most common bin; needs discretization

For a point set in space, the **centroid** is the coordinate-wise mean — the unique point
minimizing sum of squared distances. The **geometric median** minimizes sum of distances
(harder; not declared here). A **medoid** is an actual sample point closest to the center
in some sense — useful when you must pick an existing vertex, not a fractional average.

```
   ·  ·
 ·  ·  ★  ·     ★ = centroid (mean of coordinates)
  ·   ·  ·
```

### Spread: variance and covariance

Sample variance (Bessel-corrected) uses $n-1$ in the denominator:

$$
s^2 = \frac{1}{n-1}\sum_i (x_i - \bar{x})^2
$$

so an unbiased estimate of population variance for i.i.d. samples. In 2D/3D, **covariance
matrices** generalize that idea: diagonal entries are per-axis variances; off-diagonals
say whether axes grow together.

Principal axes of a point cloud are eigenvectors of the covariance — the seed of PCA and
of oriented bounding boxes.

### Histograms and quantiles

A **histogram** counts how many values fall in uniform bins over a range. Shape of the
counts is a nonparametric look at the distribution. **Quantiles** mark values at cumulative
probabilities (median = 0.5 quantile). The **five-number summary** (min, Q1, median, Q3,
max) and box plots add whiskers and outlier lists for quick visual hygiene checks.

### Correlation and fits

**Pearson** correlation measures linear association in $[-1,1]$. Spearman/Kendall use ranks
for monotonic association. When you want a prediction line, ordinary least squares yields
slope, intercept, and $R^2$ (fraction of variance explained). Polynomial fits extend the
same idea to higher powers.

### Streaming summaries

If $n$ is huge or online, Welford's algorithm keeps a running mean and $M_2$ (sum of
squared deviations) in one pass — numerically stabler than naive sum / sum-of-squares.

## In Plato

File `58-statistics.plato` states conventions up front: sample (n−1) variance; excess
kurtosis (normal → 0); paired arrays index-aligned and equal length.

### Univariate summaries

```plato
type SummaryStatistics
    implements Value
{
    Count: Integer;
    Sum: Number;
    Mean: Number;
    Variance: Number;
    StandardDeviation: Number;
    Minimum: Number;
    Maximum: Number;
    Skewness: Number;
    Kurtosis: Number;
}

type RunningStatistics
    implements Value
{
    Count: Integer;
    Mean: Number;
    M2: Number;   // sample variance = M2 / (Count - 1)
}
```

```plato
type WeightedValue<T>
{
    Value: T;
    Weight: Number;
}
```

### Histograms and quantiles

```plato
type Histogram
    implements Value
{
    Range: NumberInterval;
    Counts: Array<Integer>;
}

type Histogram2D
    implements Value
{
    RangeX: NumberInterval;
    RangeY: NumberInterval;
    Counts: Array2D<Integer>;
}

type Quantiles
    implements Value
{
    Levels: Array<Probability>;
    Values: Array<Number>;
}

type FiveNumberSummary
    implements Value
{
    Minimum: Number;
    LowerQuartile: Number;
    Median: Number;
    UpperQuartile: Number;
    Maximum: Number;
}

type BoxPlotStatistics
    implements Value
{
    Summary: FiveNumberSummary;
    LowerWhisker: Number;
    UpperWhisker: Number;
    Outliers: Array<Number>;
}
```

Histogram bins are half-open $[Start + i w,\; Start + (i+1)w)$ with $w$ the range length
over bin count; the final bin also includes the range end. Values outside `Range` are
dropped.

### Covariance, correlation, fits

```plato
type Covariance2D
    implements Value
{
    Xx: Number;
    Xy: Number;
    Yy: Number;
}

type Covariance3D
    implements Value
{
    Matrix: SymmetricMatrix3x3;
}

type CorrelationStatistic = Pearson | Spearman | Kendall;

type CorrelationCoefficient
    implements Value
{
    Statistic: CorrelationStatistic;
    Value: Number;
    SampleCount: Integer;
}

type LinearFit
    implements Value
{
    Slope: Number;
    Intercept: Number;
    RSquared: Number;
}

type PolynomialFit
    implements Value
{
    Coefficients: Array<Number>;  // ascending powers: coeff[k] * x^k
    RSquared: Number;
}
```

### Smoothing and outliers

```plato
type MovingAverage
    = Simple(WindowSize: Integer)
    | Exponential(Alpha: Proportion)
    | Gaussian(WindowSize: Integer, StandardDeviation: Number);

type OutlierDetection
    = ZScore(Threshold: Number)
    | Iqr(Threshold: Number)
    | Mad(Threshold: Number);
```

Usage-shaped sketches for a point cloud's X coordinates (illustrative):

```plato
// xs : Array<Number> extracted from Point3D.X
let stats = SummaryStatistics { ... };  // from sample
// stats.Mean is the centroid's X if xs are point X-coordinates

let hist = Histogram {
    Range: NumberInterval { ... },
    Counts: ...
};

let cov = Covariance3D {
    Matrix: ...   // 3×3 of XYZ sample covariances
};

let fit = LinearFit {
    Slope: 2,
    Intercept: 1,
    RSquared: 0.95
};  // y ≈ 2x + 1
```

## Pitfalls / fine print

**Population vs sample variance.** Using $/n$ vs $/(n-1)$ changes results for small $n$.
Plato commits to sample variance — match that when comparing to textbooks that use $/n$.

**Kurtosis flavor.** Excess kurtosis is zero for a normal; some libraries report raw
kurtosis (three for a normal). Mixing conventions invents "fat tails" that are not there.

**Centroid ≠ medoid.** Averaging coordinates can produce a point outside a non-convex
object or off the mesh. Picking a representative vertex needs a different criterion.

**Histogram range.** Out-of-range samples are silently uncounted — a "missing mass" bug
when the range was guessed too tight.

**Empty or singleton samples.** Variance with $n < 2$ is undefined under Bessel correction;
`RunningStatistics` with `Count: 1` must not divide by zero.

**Correlation ≠ causation; $R^2$ ≠ accuracy on new data.** Fits summarize the sample you
fed them. Overfitted high-degree `PolynomialFit` can still report pretty $R^2$.

**WeightedValue.** Weights must be non-negative; negative weights break "importance" stories
and weighted means.

## Try it

1. Data $\{1,2,3\}$. Sample variance? Population variance with $/n$?
2. Why might the median of building heights beat the mean for "typical height"?
3. A `Covariance2D` has `Xy = 0`. What does that say about the two coordinates (for
   Pearson / linear association in the sample)?

<details>
<summary>Answers</summary>

1. Mean $2$; squared deviations $1+0+1=2$; sample variance $2/(3-1)=1$; population $/n$
   gives $2/3$.
2. A few skyscrapers pull the mean up; the median stays with the bulk of buildings.
3. Sample covariance is zero — no linear co-variation detected (they may still be
   nonlinearly related).

</details>

## Library recommendations

- **missing-function** — `58-statistics.plato`: rich result types (`SummaryStatistics`,
  `Histogram`, `Covariance3D`) exist, but no `Summarize(Array<Number>)`,
  `Centroid(Array<Point3D>)`, or `SampleCovariance(Array<Point3D>)` constructors. Teaching
  point-set statistics has to invent the verbs that build these records.

- **missing-type** — `58-statistics.plato`: there is no `Medoid` / `GeometricMedian` result,
  and no point-cloud summary bundling centroid + `Covariance3D`. The lesson's "statistics
  of points" framing outruns the univariate-first vocabulary.

- **doc-comment** — `58-statistics.plato`: `PolynomialFit.Coefficients` ascending-power
  convention matches `Polynomial` elsewhere, but the banner does not cross-cite that type.
  A one-liner would prevent silent disagreement with descending school-form polynomials.

- **pedagogy** — `58-statistics.plato`: `OutlierDetection` and `MovingAverage` are parameter
  sums without an associated "apply to sample" result type. They read as intent records;
  lessons cannot show outputs without inventing parallel result shapes.
