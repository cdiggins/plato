# Summarize a sample

Eight readings from one instrument. Report the mean, the sample variance, the spread, and the standard error of the mean.

Needs: `stdlib/foundation`. Status: Supported.

## The program

Source: [sample-statistics.plato](../implementation/examples/cookbook-sample-statistics/sample-statistics.plato).

```plato
library CookbookSampleStatistics {
    // Summary of a sample with at least two values: mean, sample variance
    // (divide by n - 1), and extremes. Skewness and kurtosis are not computed.
    Summarize(xs: Array<Number>): SummaryStatistics {
        var n = xs.Count.ToNumber;
        var mean = xs.SumOf / n;
        var variance = xs.Map(x => (x - mean).Square).SumOf / (n - 1.0);
        var low = xs.Reduce(xs[0], (a, x) => a.Lesser(x));
        var high = xs.Reduce(xs[0], (a, x) => a.Greater(x));
        return new SummaryStatistics(xs.Count, xs.SumOf, mean, variance, variance.Sqrt, low, high, 0.0, 0.0);
    }

    Sample(): Array<Number> => [2.0, 4.0, 4.0, 4.0, 5.0, 5.0, 7.0, 9.0];

    Law_ReferenceSampleMean(): Boolean => Summarize(Sample()).Mean.AlmostEqual(5.0, 1e-12);
    Law_ReferenceSampleVariance(): Boolean => Summarize(Sample()).Variance.AlmostEqual(32.0 / 7.0, 1e-12);
    Law_ReferenceSamplePopulationVariance(): Boolean
        => Summarize(Sample()).PopulationVariance.AlmostEqual(4.0, 1e-12);
    Law_ReferenceSampleRange(): Boolean => Summarize(Sample()).ValueRange.AlmostEqual(7.0, 1e-12);
    Law_ReferenceSampleStandardError(): Boolean
        => Summarize(Sample()).StandardErrorOfMean.AlmostEqual((32.0 / 7.0).Sqrt / 8.0.Sqrt, 1e-12);
}
```

## Result

| Quantity | Value |
|---|---|
| Mean | 5 |
| Sample variance (`n - 1`) | 32/7, about 4.5714 |
| Population variance | 4 |
| Value range | 7 |
| Standard error of the mean | `sqrt(32/7) / sqrt(8)`, about 0.7559 |

Each is checked to `1e-12`.

## Walk through

1. The library stores a `SummaryStatistics` record but has no estimator that fills one from a sample, so `Summarize` computes the fields.
2. The variance divides the squared deviations by `n - 1`, the convention `SummaryStatistics.Variance` documents.
3. The record's own bodies then give derived values: `PopulationVariance` rescales the sample variance by `(n - 1) / n`, `ValueRange` is maximum minus minimum, and `StandardErrorOfMean` is the standard deviation over `sqrt(n)`.

## Library pages used

- Guide: [Statistics, covariance and fitted models](../library/foundation/statistics.md), [Collections](../library/foundation/collections.md).
- API: [statistics.types](../api/foundation/statistics.types.md) (`SummaryStatistics`), [statistics.library](../api/foundation/statistics.library.md) (`PopulationVariance`, `ValueRange`, `StandardErrorOfMean`).

## Limits

- Needs at least two values: the variance divides by `n - 1`. A standard error also assumes an appropriate sampling model, not only valid arithmetic.
- Skewness and kurtosis are set to 0 here because the program does not compute them. Do not read them from this record. The stored kurtosis convention is excess kurtosis.
- Quantiles, medians and outlier rules are not in the library; each has several defensible conventions.
- The sum is accumulated in order, so a very long or badly scaled sample loses precision. The library's `RunningStatistics` record is the stored form of a streaming (Welford-style) accumulator.
