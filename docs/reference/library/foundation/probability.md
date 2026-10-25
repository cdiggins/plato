# Probability distributions and random state

Read [distribution types](../../api/foundation/random.types.md), [the distribution interface](../../api/foundation/random.concepts.md), [distribution bodies](../../api/foundation/random.library.md), and their [special-function dependencies](../../api/foundation/numbers.library.md).

## Parameters are part of the definition

`IProbabilityDistribution` requires mean, variance, `Pdf` and `Cdf`. For a continuous law, `Pdf(x)` is a density, not the probability of drawing exactly `x`. For a discrete law the same name denotes probability mass. `Cdf(x)` is the probability of drawing at most `x`. For continuous distributions, probability over an interval is a CDF difference; for discrete distributions endpoint inclusion changes the subtraction needed.

Check the declaration and body for parameterization. An exponential distribution stores a positive rate, whose reciprocal is the mean. A normal distribution stores standard deviation, not variance. A log-normal distribution's parameters describe the logarithm. A Pareto mean requires shape greater than one; its variance requires shape greater than two. Algebraic formulas outside their preconditions need not return an honest representation of an undefined or infinite moment.

Example: a uniform law over `[2,6]` has density `1/4` within its support and mean 4. Its probability on `[3,5]` is `1/2`. This is a mathematical worked example; implementation accuracy is a separate question.

## Numerical approximations are visible

The normal CDF body uses a named rational approximation. Gamma, beta and related distributions call finite numerical special-function implementations. A formula labelled “exact” in source means exact mathematical form; floating-point evaluation still rounds and may overflow or lose precision in a tail.

The von Mises circular distribution is a special case: its location is an `Angle`, but the common interface accepts raw numeric radians. Its variance is circular variance, not ordinary squared-radian variance. The CDF body wraps inputs at an antipodal cut and evaluates a truncated Fourier/Bessel series. **Source conflict:** the file's opening commentary still says that CDF is omitted; the later body exists. Use the body and its accuracy qualifications. Do not infer a globally monotone real-line CDF after repeated wrapping around the circle.

## State and sampling are different capabilities

`RandomState` describes deterministic generator state. A state record and distribution density functions do not establish a complete draw API for every declared distribution. Multivariate parameter records need their own operations; they do not implement the univariate interface merely because they are distributions.

## Related work

[DLMF](https://dlmf.nist.gov/) defines the gamma, beta, error and Bessel functions used in the formulas. [PBRT's Monte Carlo chapters](https://pbr-book.org/4ed/contents) distinguish density evaluation from drawing and transforming samples. Plato's distribution records should be read with that same distinction; neither source proves the accuracy of a particular Plato approximation.

[SciPy's distribution API](https://docs.scipy.org/doc/scipy/reference/stats.html) distinguishes continuous densities from discrete mass functions and supplies sampling methods. Plato uses the common name `Pdf` for both density and mass, so callers must identify the distribution family explicitly; do not assume a sampling method exists merely because SciPy has one.
