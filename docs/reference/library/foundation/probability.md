# Probability distributions and random state

`IProbabilityDistribution` requires mean, variance, `Pdf`, and `Cdf`.

## Parameters are part of the definition

For a continuous law, `Pdf(x)` is a density, not the probability of drawing exactly `x`. For a discrete law the same name denotes probability mass. `Cdf(x)` is the probability of drawing at most `x`. For continuous distributions, probability over an interval is a CDF difference. For discrete distributions, endpoint inclusion changes the subtraction needed.

Check the declaration and body for parameterization. An exponential distribution stores a positive rate, whose reciprocal is the mean. A normal distribution stores standard deviation, not variance. A log-normal distribution's parameters describe the logarithm. A Pareto mean requires shape greater than one, and its variance requires shape greater than two. Algebraic formulas outside their preconditions need not return an honest representation of an undefined or infinite moment.

A uniform law over `[2,6]` has density `1/4` within its support and mean 4. Its probability on `[3,5]` is `1/2`. This is a mathematical worked example. Implementation accuracy is a separate question. Plato uses the common name `Pdf` for both density and mass, so identify the family explicitly. [SciPy's distribution API](https://docs.scipy.org/doc/scipy/reference/stats.html) separates continuous densities from discrete mass functions and also supplies sampling methods, which a Plato `Pdf` does not imply.

## Numerical approximations are visible

The normal CDF body uses a named rational approximation. Gamma, beta, and related distributions call finite numerical special-function implementations. A formula labelled "exact" in source means exact mathematical form. Floating-point evaluation still rounds, and it may overflow or lose precision in a tail. The [DLMF](https://dlmf.nist.gov/) defines the gamma, beta, error, and Bessel functions used in the formulas, and that definition does not prove the accuracy of a particular Plato approximation.

The von Mises circular distribution is a special case. Its location is an `Angle`, but the common interface accepts raw numeric radians. Its variance is circular variance, not ordinary squared-radian variance. The CDF body wraps inputs at an antipodal cut and evaluates a truncated Fourier/Bessel series. The file's opening commentary still says that the CDF is omitted, and the later body exists. Use the body and its accuracy qualifications. Do not infer a globally monotone real-line CDF after repeated wrapping around the circle.

## Sampling and generator state

`RandomState` describes deterministic generator state. A state record and distribution density functions do not establish a complete draw API for every declared distribution. [PBRT's Monte Carlo chapters](https://pbr-book.org/4ed/contents) separate density evaluation from drawing samples, and neither that discussion nor the DLMF definitions above prove the accuracy of a Plato approximation. Multivariate parameter records need their own operations. They do not implement the univariate interface merely because they are distributions.

## Where to look

- [Distribution types](../../api/foundation/random.types.md) and [the distribution interface](../../api/foundation/random.concepts.md)
- [Distribution bodies](../../api/foundation/random.library.md)
- [Special-function dependencies](../../api/foundation/numbers.library.md)
