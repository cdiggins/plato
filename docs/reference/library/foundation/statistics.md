# Statistics, covariance and fitted models

[Statistics records](../../api/foundation/statistics.types.md) define the stored summaries; [statistics bodies](../../api/foundation/statistics.library.md) derive readings from them. A stored summary is not an estimator that computes it from raw observations.

## Read the statistical convention

`SummaryStatistics.Variance` uses sample variance, dividing squared deviations by `n-1`; kurtosis is excess kurtosis, so the normal reference is zero. `RunningStatistics` stores count, mean and the accumulated squared-deviation quantity `M2` used by Welford-style algorithms. Population variance is `M2/n`; sample variance is `M2/(n-1)`.

These divisions require sufficient observations. Sample variance needs `n>1`; a mean or population variance needs `n>0`. A standard error also assumes an appropriate sampling model, not just valid arithmetic. A zero mean makes a coefficient-of-variation formula singular or inappropriate.

`WeightedValue<T>` pairs a value with a nonnegative weight. A weighted mean needs a positive total weight. Weight semantics matter: replication counts, probability weights and inverse-variance weights do not necessarily share an unbiased variance formula.

## Histograms and quantiles

Histogram bins are half-open except that the final bin includes the maximum range endpoint. Out-of-range values are not counted. With range `[0,10]` and two equal bins, 5 belongs to the second bin, 10 also belongs to the second, and 11 contributes nothing. A frequency is a count fraction; a density additionally divides by bin width.

`Quantiles` aligns ascending probability levels with value entries. The representation does not choose the interpolation convention for estimating a quantile from a finite sample. `FiveNumberSummary` and box-plot records similarly store outcomes rather than proving how the quartiles were computed.

## Covariance and regression

`Covariance2D` stores two variances and one cross-covariance. Pearson correlation divides covariance by the product of standard deviations, so constant variables make the denominator zero. Paired observations must be index-aligned and equal in count. A `LinearFit` stores `Slope`, `Intercept`, and `RSquared`; `Predict(x)` evaluates the stored line. A polynomial fit uses ascending powers for coefficients.

**Explicit gap:** the source notes that raw-sample estimators, quantile interpolation, Spearman/Kendall estimation, smoothing and outlier algorithms are outside these derived bodies. Their selector and result types do not imply executable implementations. Verify a concrete function before planning an analysis pipeline around it.

## Related work

[SciPy's statistics reference](https://docs.scipy.org/doc/scipy/reference/stats.html) includes distributions, estimators, correlations and tests. Plato's stored summaries and derived readings are a smaller capability surface: a familiar result name does not supply SciPy's estimator. Sample/population choice and quantile definition must be read from the concrete Plato API.
