---
lesson: histogram-binning
title: Histogram Binning
domain: Math, statistics & signals
v3-files: [12-intervals-bounds.plato, 58-statistics.plato]
audience: High-school math and general programming background
status: draft-v1
---

# Histogram Binning

You have a thousand measurements and want to *see* their shape — not just mean and
variance, but where mass piles up, whether there are two peaks, whether outliers live
alone at the edge. A **histogram** answers by counting how many values fall into each
of a sequence of adjacent bins. It is the simplest density estimate: a bar chart of
counts over a chosen range.

Every image histogram, every audio level meter, every "how tall are the players"
plot is this idea. The subtlety is not the count — it is *which bin* a value lands in,
and what happens at the edges.

## The idea

Fix an interval $[L, R]$ on the real line and a bin count $n \ge 1$. The bin width is

$$
w = \frac{R - L}{n}
$$

Bin $i$ (zero-based) covers the half-open span

$$
[L + i w,\; L + (i+1)w)
$$

except the **last** bin, which also includes the right endpoint $R$ so that values
exactly equal to $R$ are not silently dropped. Values outside $[L, R]$ are ignored.

```
 L                         R
 |----|----|----|----|----|
   0    1    2    3    4     bin indices
 [)   [)   [)   [)   [ ]    half-open; last closed at R
```

Given a sample $x_0,\ldots,x_{m-1}$, the count array $c[0..n)$ satisfies

$$
c[i] = \#\{\, j : x_j \text{ falls in bin } i \,\}
$$

and $\sum_i c[i]$ equals the number of in-range samples (not necessarily $m$).

**Choosing $n$.** Too few bins smear structure; too many turn every sample into its own
spike. Rules of thumb (Sturges, Freedman–Diaconis, Scott) trade bias and variance;
they are heuristics, not laws. Always look at the plot.

**Density vs count.** Dividing counts by $(m_{\mathrm{in}} \cdot w)$ approximates a
PDF so the bar areas sum to one. Raw counts are better for discrete event tallies.

A **2D histogram** applies the same rule independently on $x$ and $y$, storing an
`Array2D` of joint counts — the discrete cousin of a bivariate density.

## In Plato

`58-statistics.plato` encodes a 1D histogram as a range plus a count array. The range
is a `NumberInterval` from `12-intervals-bounds.plato`.

```plato
type NumberInterval
    implements IntervalLike<Number>
{
    Start: Number;
    End: Number;
}

type Histogram
    implements Value
{
    Range: NumberInterval;
    Counts: Array<Integer>;
}
```

Doc convention (normative for the type): bin $i$ is half-open
`[Start + i*w, Start + (i+1)*w)` with $w = (\mathrm{End}-\mathrm{Start}) / n$, and
the final bin also includes `End`. Outside values are not counted.

```plato
// Five bins over [0, 10]
let h = Histogram(
    NumberInterval(0, 10),
    // Counts length = 5, initially zeros, then filled by a library pass
    counts);

// After inserting samples {0.0, 1.5, 9.9, 10.0, 11.0}:
//   bin 0 [0,2)  -> 0.0, 1.5
//   bin 4 [8,10] -> 9.9, 10.0  (10.0 included in last bin)
//   11.0 discarded
```

The joint form:

```plato
type Histogram2D
    implements Value
{
    RangeX: NumberInterval;
    RangeY: NumberInterval;
    Counts: Array2D<Integer>;
}
```

Column index selects the $x$ bin; row index selects the $y$ bin — same half-open
edge rule on each axis.

Nearby vocabulary that often rides with histograms:

```plato
type SummaryStatistics { Count; Sum; Mean; Variance; ... }
type Quantiles { Levels: Array<Probability>; Values: Array<Number>; }
type FiveNumberSummary { Minimum; LowerQuartile; Median; UpperQuartile; Maximum; }
```

A histogram is a *coarse* picture of the same sample that `SummaryStatistics` and
`Quantiles` summarize with scalars. Use scalars for tables; use bins when shape
matters.

## Pitfalls / fine print

**Off-by-one at the right edge.** If every bin were half-open and the last excluded
`End`, a value exactly equal to `R` would vanish. Plato's convention closes the last
bin at `End` on purpose. Implementations that use `floor((x-L)/w)` must clamp the
index into `[0, n)`.

**Empty and reversed intervals.** `NumberInterval` allows `Start > End` (a reversed
directed span). A histogram over a reversed range is not defined by the current doc
comment — treat `Start < End` as required until the library says otherwise.

**Unequal bins.** `Histogram` is *uniform* bins only. Variable-width bins (common in
adaptive density estimation) need a different type; do not overload `Counts` with
implied unequal edges.

**Integer data.** For whole-number categories, prefer explicit category counts or a
`DiscreteDistribution` weight array rather than forcing continuous bin edges through
integer midpoints.

**Normalization confusion.** `Counts` are integers. Plotting "probability" requires
dividing by total in-range count (and optionally by width). The type does not store
normalized densities — keep that as a view.

**Streaming.** `RunningStatistics` accumulates mean/variance online; there is no
declared running histogram. Rebuilding from a buffer, or maintaining fixed bins with
an external increment, is left to the caller for now.

## Try it

1. Range $[0, 4]$, four bins. Which bin does $x = 2.0$ enter? Which does $x = 4.0$?
2. Width $w$ for range $[−1, 1]$ with 8 bins?
3. Samples $\{−0.5, 0.0, 0.5, 2.0\}$ over range $[0, 1]$ with 2 bins: what are the
   counts (approximate reasoning)?

<details>
<summary>Answers</summary>

1. $w = 1$. Bin edges $[0,1),[1,2),[2,3),[3,4]$. So $2.0$ → bin 2; $4.0$ → bin 3
   (last bin includes the end).
2. $w = 2/8 = 0.25$.
3. Only $0.0$ and $0.5$ are in range. With $w = 0.5$: bin 0 $[0, 0.5)$ gets $0.0$;
   bin 1 $[0.5, 1]$ gets $0.5$. Counts $[1, 1]$. $-0.5$ and $2.0$ are discarded.

</details>

## Library recommendations

- **missing-function** — `58-statistics.plato`: `Histogram` has no `BinIndex(h, x)`,
  `Insert(h, x)`, or `AddSample` declared. Teaching binning requires stating the index
  formula in prose; the library should own
  `BinIndex(h: Histogram, value: Number): Integer` (−1 if outside range).

- **missing-function** — `58-statistics.plato`: no `Density(h: Histogram): Array<Number>`
  that divides counts by `(sum * binWidth)`. Almost every plotting path wants this view;
  leaving it unnamed invites inconsistent normalization.

- **doc-comment** — `58-statistics.plato`: state explicitly that `Range.Start < Range.End`
  is required and that `Counts.Count >= 1`. Reversed `NumberInterval` is legal elsewhere
  but poisonous here.

- **wrong-shape** — `58-statistics.plato`: consider a `HistogramBinning` parameter
  record `{ Range: NumberInterval; BinCount: Integer }` separate from filled `Counts`,
  so empty templates and accumulated histograms are distinct types rather than
  "zeros means empty" convention.
