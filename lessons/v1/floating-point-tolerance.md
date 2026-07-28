---
lesson: floating-point-tolerance
title: Floating-Point Tolerance
domain: Math, statistics & signals
v3-files: [63-uncertainty.plato]
audience: Programmers who have seen IEEE floats; high-school error bars helpful.
status: draft-v1
---

# Floating-Point Tolerance

`0.1 + 0.2 == 0.3` is false in IEEE-754 binary floating point. The decimal values are not
exactly representable; the sum rounds to a neighbor of $0.3$, and bitwise equality fails. Geometry
code hits the same wall constantly: a point "on" a plane is rarely exactly zero distance away; two
unit vectors that should be parallel have a dot product of $0.9999997$; a matrix that should be
orthogonal drifts after a hundred multiplies.

Treating `==` as the definition of "same" for `Number` is a lie you can get away with in toy
examples and nowhere else. Robust code compares with an explicit **tolerance** — and knows whether
that tolerance is absolute, relative, or an engineering allowance around a nominal value.

## The idea

### Absolute epsilon

Say two values are equal if $|a - b| \le \varepsilon$ for a fixed $\varepsilon$:

$$
\text{almostEqual}(a,b;\varepsilon_{\mathrm{abs}}) \iff |a-b| \le \varepsilon_{\mathrm{abs}}
$$

Works when $a$ and $b$ live near a known scale (coordinates in meters within a building; parameters
in $[0,1]$). Fails when magnitudes span orders: $\varepsilon = 10^{-6}$ is tight near $1$ and
meaningless near $10^{9}$.

### Relative epsilon

Scale the allowance by the size of the values:

$$
|a-b| \le \varepsilon_{\mathrm{rel}} \cdot \max(|a|,|b|)
$$

(or a similar form). Good for comparing large physical quantities. Weak near zero: everything small
looks "equal" unless you also keep an absolute floor.

### Combined test (practical default)

Many libraries use both:

$$
|a-b| \le \max(\varepsilon_{\mathrm{abs}},\ \varepsilon_{\mathrm{rel}} \cdot \max(|a|,|b|))
$$

Pick $\varepsilon_{\mathrm{abs}}$ for the noise floor (ulps near zero, sensor quantization) and
$\varepsilon_{\mathrm{rel}}$ for scale-invariant agreement.

### Units and meaning

A tolerance is not just a float. "±0.5 mm on a 100 mm shaft" is an **engineering tolerance** about
a nominal. "±0.02 at 1-sigma" is a **statistical uncertainty**. "95% confidence interval $[L,U]$"
is yet another shape. Collapsing all three to `eps = 1e-6` loses information you need for metrology
and for honest predicate design.

### Robust predicates (preview)

Geometry predicates (orient2d, incircle) ask qualitative questions: left or right of a line?
Exact arithmetic or adaptive precision filters exist so the **sign** is correct even when the
magnitude is near floating-point underflow. Tolerance-based comparisons approximate that for
non-critical work; CAD kernels escalate to exact predicates when a wrong sign flips a mesh.

```
true value --------+--------
                   |
        measured   •  ± uncertainty band
                   |
        nominal ----N---- [N-minus, N+plus] tolerance window
```

## In Plato

`63-uncertainty.plato` names the vocabularies of uncertain and tolerated quantities — without yet
declaring comparison helpers. That split is useful: it forces you to say *which* notion of
sloppiness you mean.

### Standard uncertainty

```plato
// A measured or estimated number with its standard (1-sigma) uncertainty,
// in the same implicit unit as the value.
type UncertainNumber
    implements Value
{
    Value: Number;
    StandardUncertainty: Number;
}
```

One-sigma unless a coverage factor says otherwise (see `ExpandedUncertainty`). Propagation through
formulas is a separate concern (`ErrorPropagation` = `Linear | MonteCarlo | Unscented`).

```
length = UncertainNumber(Value = 2.500, StandardUncertainty = 0.002)
// "2.500 ± 0.002 (1σ)" — not the same as an acceptance window
```

### Engineering tolerance

```plato
// An asymmetric engineering tolerance about a nominal value. The acceptable
// range is [Nominal - Minus, Nominal + Plus]; Plus and Minus are
// non-negative magnitudes.
type Tolerance
    implements Value
{
    Nominal: Number;
    Plus: Number;
    Minus: Number;
}
```

```
fit = Tolerance(Nominal = 10.0, Plus = 0.1, Minus = 0.05)
// accept x in [9.95, 10.1]
inSpec = (x >= fit.Nominal - fit.Minus) && (x <= fit.Nominal + fit.Plus)
```

This is the right type for "drawing says 10 +0.1/−0.05," not for float-equality in a shader.

### Confidence intervals and bounds

```plato
type ConfidenceInterval
{
    Lower: Number;
    Upper: Number;
    Level: Probability;
}

type IntervalBound
{
    Value: Number;
    Inclusive: Boolean;
}
```

Confidence intervals carry a probability `Level` (e.g. 0.95). `IntervalBound` records whether an
endpoint is closed — critical when translating continuous tests into discrete acceptance.

### Uncertain geometry

```plato
type UncertainPoint3D
{
    Value: Point3D;
    Covariance: Covariance3D;
}

type UncertainVector3D
{
    Value: Vector3D;
    Covariance: Covariance3D;
}
```

Gaussian uncertainty on positions and displacements: the covariance is the multi-dimensional
cousin of `StandardUncertainty`. Nearest-neighbor and registration code should consume these when
inputs come from sensors, not pretend every `Point3D` is exact.

### What the file does *not* give you

There is no `AlmostEqual(a, b, eps)`, no ulp helper, and no predicate tying `Tolerance` to
`Number` comparison. Illustrative checks stay explicit:

```
absDiff = Abs(a - b)
okAbs = absDiff <= epsAbs
okRel = absDiff <= epsRel * Max(Abs(a), Abs(b))
ok = okAbs || okRel
```

Use `Tolerance` when the allowance is part of the product definition; use raw epsilons when the
allowance is numerical noise; use `UncertainNumber` when the allowance is estimated error.

## Pitfalls / fine print

**One global `EPS`.** A constant that works for unit-cube meshes fails for planetary coordinates.
Scale epsilons to the feature size, or prefer relative tests.

**Comparing normalized vectors with absolute eps on components.** Prefer angular tolerance:
$\mathbf{u}\cdot\mathbf{v} \ge \cos\theta_{\max}$, with a clamp for numerical overshoot above 1.

**Symmetric vs asymmetric.** `Tolerance` allows different plus/minus. Forcing a single `±eps`
loses press-fit intent.

**Confusing uncertainty with tolerance.** A part can be measured as $10.02 \pm 0.01$ (uncertain)
and still be out of spec relative to $10 +0.01/-0.00$ (tolerance).

**Inclusive endpoints.** Hash sets and boundary tests disagree when one side uses `<` and the other
`<=`. `IntervalBound.Inclusive` exists so you can state the choice.

**Exact zeros.** "Distance to plane == 0" almost never holds. Classify with a band: below
$-\varepsilon$ inside, above $+\varepsilon$ outside, else on-boundary / uncertain.

## Try it

<details>
<summary>Exercise 1 — Pick the tool</summary>

A drawing specifies hole diameter $8.0^{+0.05}_{-0.00}$ mm. Which Plato type models that?

**Answer.** `Tolerance(Nominal = 8.0, Plus = 0.05, Minus = 0.0)`.
</details>

<details>
<summary>Exercise 2 — Why == fails</summary>

After `Normalize`, two independently computed unit vectors print as `(1,0,0)` but their dot product
is `0.9999998`. Are they "equal"? What test would you use?

**Answer.** Equal enough for facing tests if $\mathbf{u}\cdot\mathbf{v} \ge 1 - \varepsilon$ (or
$\ge \cos\theta$). Component-wise `==` is the wrong tool.
</details>

<details>
<summary>Exercise 3 — Absolute vs relative</summary>

Is $|a-b| < 10^{-9}$ a good test that two Earth-centered positions (meters, ~$6\times 10^6$) are
the same survey point?

**Answer.** No — that absolute epsilon is far below coordinate magnitude and may still be wrong for
survey accuracy. Prefer a tolerance in meters that matches the survey (e.g. 0.01) or a relative
test tied to ECEF scale plus an absolute floor.
</details>

## Library recommendations

- **missing-function** — `63-uncertainty.plato`: no `Contains(Tolerance, Number): Boolean`,
  `AlmostEqual(Number, Number, absolute, relative)`, or `WithinUncertainty(UncertainNumber, Number)`.
  The lesson's acceptance checks are handwritten every time; the types cry out for these predicates.

- **missing-type** — no `FloatingPointTolerance` / `Epsilon` record with `{Absolute, Relative}`
  fields for numerical (as opposed to engineering) comparison. `Tolerance` is already claimed for
  manufacturing semantics; overloading it for ulps-style epsilons would muddle the file's story.

- **doc-comment** — `Tolerance`: emphasize that Plus/Minus are **acceptance allowances**, not
  1-sigma uncertainties, and point to `UncertainNumber` for the latter. Authors arriving from
  float-equality blog posts will otherwise stuff epsilons into `Tolerance`.

- **missing-function** — geometric companions: `AlmostEqual(Point3D, Point3D, …)` and
  `AlmostParallel(Vector3D, Vector3D, Angle)` are absent. Uncertainty geometry has covariances but
  no simple deterministic epsilon API for the exact `Point3D` / `Vector3D` world most call sites use.

- **pedagogy** — `ErrorPropagation` declares *how* to push uncertainty through code but nothing
  connects it to `UncertainNumber` arithmetic. A minimal `Add(UncertainNumber, UncertainNumber)`
  under linear propagation would make the file teachable end-to-end instead of vocabulary-only.
