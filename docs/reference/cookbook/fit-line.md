# Fit a line by least squares

Five measurements were taken at times 0 to 4. Find the straight line that fits them best in the least-squares sense, how well it fits, and what it predicts for time 5.

Needs: `stdlib/foundation`. Status: Supported.

## The program

Source: [fit-line.plato](../implementation/examples/cookbook-fit-line/fit-line.plato).

```plato
library CookbookFitLine {
    // Ordinary least squares for y = Slope * x + Intercept.
    // Precondition: at least two points and not all x equal.
    FitLine(xs: Array<Number>, ys: Array<Number>): LinearFit {
        var n = xs.Count.ToNumber;
        var meanX = xs.SumOf / n;
        var meanY = ys.SumOf / n;
        var sxx = xs.Map(x => (x - meanX).Square).SumOf;
        var syy = ys.Map(y => (y - meanY).Square).SumOf;
        var sxy = xs.Count.MapRange(i => (xs[i] - meanX) * (ys[i] - meanY)).SumOf;
        var slope = sxy / sxx;
        return new LinearFit(slope, meanY - slope * meanX, sxy * sxy / (sxx * syy));
    }

    Measurements(): Array<Number> => [1.1, 2.9, 5.2, 6.8, 9.0];
    Times(): Array<Number> => [0.0, 1.0, 2.0, 3.0, 4.0];

    Law_ReferenceFitLineSlope(): Boolean
        => FitLine(Times(), Measurements()).Slope.AlmostEqual(1.97, 1e-9);
    Law_ReferenceFitLineIntercept(): Boolean
        => FitLine(Times(), Measurements()).Intercept.AlmostEqual(1.06, 1e-9);
    Law_ReferenceFitLineRSquared(): Boolean
        => FitLine(Times(), Measurements()).RSquared.AlmostEqual(0.99766, 1e-5);
    Law_ReferenceFitLinePredict(): Boolean
        => FitLine(Times(), Measurements()).Predict(5.0).AlmostEqual(10.91, 1e-9);
}
```

## Result

| Quantity | Value |
|---|---|
| `Slope` | 1.97 |
| `Intercept` | 1.06 |
| `RSquared` | 0.99766 (to five places) |
| `Predict(5.0)` | 10.91 |

The four laws state these values. The slope, intercept and prediction are checked to `1e-9`, and `RSquared` to `1e-5`.

## Walk through

1. The means of `x` and `y` are the centre of the data. The fitted line passes through that centre.
2. `sxx` and `syy` are the sums of squared deviations. `sxy` is the sum of the products of the paired deviations; it pairs the arrays by index with `Count.MapRange`.
3. The slope is `sxy / sxx`, and the intercept places the line through the means.
4. `RSquared` is `sxy^2 / (sxx * syy)`, the coefficient of determination of a simple linear fit.
5. The result is a `LinearFit`, whose `Predict(x)` evaluates `Slope * x + Intercept`. `Residual(x, y)` gives the observed value minus that prediction.

## Library pages used

- Guide: [Statistics, covariance and fitted models](../library/foundation/statistics.md), and [Collections](../library/foundation/collections.md) for `Map`, `MapRange` and `SumOf`.
- API: [statistics.types](../api/foundation/statistics.types.md) (`LinearFit`), [statistics.library](../api/foundation/statistics.library.md) (`Predict`, `Residual`), [collections.library](../api/foundation/collections.library.md) (`SumOf`, `Map`).

## Limits

- The library stores a fit but has no estimator for it, so this program computes the fit itself. The statistics guide names that gap.
- The times must not all be equal: `sxx` would be zero and the slope undefined. `RSquared` divides by `syy`, so constant measurements also divide by zero.
- The two arrays must be index-aligned and have the same count; the program does not check this.
- Ordinary least squares treats `x` as exact and minimises vertical error only. It is not a fit for noise in both variables.
