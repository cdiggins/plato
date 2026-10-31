# Scientific data, provenance and uncertainty

This vocabulary is in the `future` tier, which is not part of the default library. A stored number, the record that describes it, and the reason to trust it are different objects.

## Value, provenance, and quality

A typed table column has one element type. A table needs aligned column lengths. A dataset descriptor, a sensor channel, or a calibration record carries metadata. It does not, by itself, modify every numeric value. `UnitOfMeasure` labels units. It does not give the compile-time distinction of a dedicated `Length` type unless a concrete conversion uses that information.

`Measurement<T>` keeps a value together with provenance, time, and quality. Quality is distinct from uncertainty: a trustworthy instrument can measure a noisy quantity. `Reading` strips provenance on purpose. Check a calibration's applicable interval before treating a reading as calibrated.

`ITimeSampled` returns a count and a timestamp by index. Endpoint helpers need a nonempty series. An endpoint span does not mean the samples are uniformly spaced, ordered in time, or free of gaps. Uniform and irregular series are different representations. A missing-value policy or a resampling policy needs an implementation that interprets it.

## Three different tolerances

`UncertainNumber` stores a nominal value and a standard uncertainty, conventionally one standard deviation. Expanded uncertainty adds a coverage interpretation. A confidence interval also depends on an inferential procedure. It is not, under every statistical reading, a fixed probability that an unknown constant lies in the interval you observed.

Engineering `Tolerance` stores asymmetric acceptance allowances around a nominal value. `ComparisonTolerance` in foundation stores absolute and relative allowances for numerical comparison. None of the three substitutes for the others. A unit mismatch can survive a generic numeric operation unless a semantic type or an explicit check stops it.

A weighted ensemble needs nonnegative weights and a positive total before a mean is defined. Covariance matters when correlated uncertainties propagate. Independent-error formulas do not apply just because each standard deviation is on hand. Kalman, filter, and propagation records describe inputs or states. They are not a universal estimator.

## Where to look

- [Scientific-data records](../../api/future/scientific-data.types.md), [time-sampled interface](../../api/future/scientific-data.concepts.md), and [data helpers](../../api/future/scientific-data.library.md)
- [Uncertainty records](../../api/future/uncertainty.types.md) and [measurement helpers](../../api/future/uncertainty-measurement.library.md)
- [Aspirational library domains](domains.md)

[JCGM's Guide to the Expression of Uncertainty in Measurement and its supplements](https://www.bipm.org/en/committees/jc/jcgm/publications) separate standard uncertainty, coverage, and Monte Carlo propagation. A helper that returns a weighted average does not implement that procedure, and it does not certify the uncertainty model.
