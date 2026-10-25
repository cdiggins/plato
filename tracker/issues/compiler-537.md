---
id: compiler-537
title: Preserve captured fields in nested statement lambda lowering
type: bug
status: ready
priority: p2
effort: M
risk: med
area: compiler
sprint: 
created: 2026-10-25
closed:
links: []
---

While verifying the STEP scalar evaluator with compiler fix `6d64d604` and
`--loops --fuse`, valid field accesses captured by a nested statement lambda
were emitted as method calls. Generation and the stub gate pass, but C# build
fails with five CS1955 errors. Representative error:

> Non-invocable member 'BSplineCurveData.Basis' cannot be used like a method.

The generated outer body reads `d.Basis.Degree` correctly. Inside the nested
lambda, `_Integer.g.cs:5024` emits
`d.Basis().SplineSpan(d.ControlPoints().Count, _eval10)`; lines 5027–5028 also
emit `d.Basis().Knots()`. These are fields. This prevents valid geometry laws
from compiling and could affect similar geometry implementation callbacks.

Verified reproducer: `.temp/step-wave/optimizers/run-wrapper/inputs/candidate/bspline-local-scalar.laws.plato`,
`Law_OwnedScalar_KnotsAndShift`. Full logs, command and source hashes are under
the same run folder. CLI SHA256:
`9e2ea7d7e2c0e652b1a6288f4b0f6178b8007f08c7a66f3cc73802ee49fd3470`.
The relevant source structure is retained here (this excerpt alone is not a
standalone verified reproducer):

```text
var d = k.CurveFixture;
var p = d.Basis.Degree;
return d.Basis.Knots.Knots.All(t => [-0.000001, 0.0, 0.000001].All(delta =>
{
    var x = t + delta;
    var span = d.Basis.SplineSpan(d.ControlPoints.Count, x);
    var values = d.HomogeneousControls;
    var local = (p + 1).MapRange(i => values[span - p + i]);
    var padded = KnotVector((d.Basis.Knots.Count + 1).MapRange(i => i == 0 ? -7.0 : d.Basis.Knots.KnotAt(i - 1)));
    return (p + 5).Range.All(i => OwnedScalarAgreement(local, d.Basis.Knots, 0, p, span, i - 2, x)
        && OwnedScalarAgreement(local, padded, 1, p, span, i - 2, x));
}));
```

The bounded test workaround moves fixture setup to a typed helper and keeps
the nested callbacks as expressions, preserving the cases and assertions.
The compiler itself has not been fixed by that workaround.

## Done means

- [ ] Reduce to a checked compiler regression fixture retaining nested statement
      lambda capture and record-field projection.
- [ ] Correct the lowering/emission path without changing method-call spelling.
- [ ] Compile and execute generated C# with shipping loops/fusion and with
      optimizers disabled; retain the geometry-law reproducer as an integration check.
