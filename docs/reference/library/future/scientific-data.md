# Scientific data, provenance and uncertainty

Read [scientific-data records](../../api/future/scientific-data.types.md), [time-sampled interface](../../api/future/scientific-data.concepts.md), [data helpers](../../api/future/scientific-data.library.md), [uncertainty records](../../api/future/uncertainty.types.md) and [measurement helpers](../../api/future/uncertainty-measurement.library.md).

## Separate value, metadata and trust

A typed table column chooses one element type. Tables require aligned column lengths; a dataset descriptor, sensor channel or calibration record carries metadata rather than numerically modifying all values by itself. `UnitOfMeasure` records label units explicitly; they do not provide the same compile-time distinction as a dedicated `Length` type unless a concrete conversion uses that information.

`Measurement<T>` retains a value with provenance, time and quality information. Quality is distinct from uncertainty: a trustworthy instrument can measure a noisy quantity. `Reading` intentionally strips provenance. A calibration's applicable interval must be checked before assuming a reading has been calibrated.

`ITimeSampled` exposes count and timestamp by index. Endpoint helpers need a nonempty series; an endpoint span is not proof of uniform sample spacing, monotone time order or absence of gaps. Uniform and irregular series have different representations. Missing-value and resampling policies require an implementation that interprets them.

## Three different tolerances

`UncertainNumber` stores a nominal value and a standard uncertainty, conventionally one standard deviation. Expanded uncertainty adds a coverage interpretation. A confidence interval additionally depends on an inferential procedure; it is not interchangeable with a fixed probability that an unknown constant lies in that observed interval under every statistical interpretation.

Engineering `Tolerance` stores asymmetric acceptance allowances around a nominal value. `ComparisonTolerance` in foundation stores absolute/relative numerical comparison allowances. None substitutes for the others. A unit mismatch can survive a generic numeric operation unless a semantic type or explicit check prevents it.

Weighted ensembles require nonnegative weights with a positive total for a mean. Covariance matters when propagating correlated uncertainties; independent-error formulas cannot be applied merely because individual standard deviations are available. Kalman/filter/propagation records describe inputs or states, not a universal estimation implementation.

## Related work

[JCGM's Guide to the Expression of Uncertainty in Measurement and supplements](https://www.bipm.org/en/committees/jc/jcgm/publications) distinguish standard uncertainty, coverage and Monte Carlo propagation. Plato's records preserve related concepts; a helper returning a weighted average does not implement the entire metrological procedure or certify the uncertainty model.
