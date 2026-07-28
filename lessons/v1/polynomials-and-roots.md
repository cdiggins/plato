---
lesson: polynomials-and-roots
title: Polynomials and Roots
domain: Math, statistics & signals
v3-files: [61-polynomials.plato]
audience: High-school algebra (quadratics, factoring) and general programming background
status: draft-v1
---

# Polynomials and Roots

Ray hits sphere. Cubic Bezier self-intersection. Smoothstep derivatives. Again and again,
geometry reduces to "find $x$ where a polynomial is zero" or "evaluate a polynomial
cheaply and stably." Polynomials are the algebra of intersections: low degree enough to
solve, expressive enough to model curves and distances.

## The idea

### What a polynomial is

A univariate polynomial is

$$
p(x) = c_0 + c_1 x + c_2 x^2 + \cdots + c_n x^n
$$

Degree $n$ (if $c_n \ne 0$). The **roots** are solutions of $p(x) = 0$. Over the complex
numbers there are exactly $n$ roots counting multiplicity (fundamental theorem of algebra).
Over the reals, some may come in conjugate pairs — a cubic always has at least one real
root; a quadratic may have zero, one, or two.

### Horner's method

Naive evaluation does $x^2$, $x^3$, … separately. **Horner** nests multiply-adds:

$$
c_0 + x\bigl(c_1 + x\bigl(c_2 + x(\cdots)\bigr)\bigr)
$$

$n$ multiplies, $n$ adds, better rounded error behavior. This is the default evaluation
algorithm in serious libraries.

### Quadratics and cubics in geometry

**Quadratic** $ax^2 + bx + c = 0$:

$$
x = \frac{-b \pm \sqrt{b^2 - 4ac}}{2a}
$$

(with a numerically stabler variant when $b^2 \approx 4ac$). Ray–sphere and ray–cylinder
reduce to quadratics in the ray parameter $t$.

**Cubics** appear in ray–torus (after depressing the degree), in cubic spline inversion, and
in Fresnel-adjacent approximations. Closed forms exist (Cardano) but are finicky; often one
uses real-root isolation plus Newton polish.

### Sparse vs dense; bases beyond monomials

Most coefficients zero → **sparse** storage (powers + coeffs). Truncated **power series**
about a center approximate analytic functions locally. **Chebyshev** and **Legendre** bases
are better conditioned on intervals. **Bernstein** polynomials on $[0,1]$ are the Bezier
basis: coefficients are control values, and the curve stays in their convex hull —
geometry's favorite positivity property.

### Rational functions and root counting

A ratio of polynomials is a **rational function** — poles where the denominator vanishes
(perspective projections, Padé approximants). A **Sturm chain** counts distinct real roots
in an interval by watching sign changes — the classical tool before you spend money on
refining each root.

## In Plato

File `61-polynomials.plato` fixes the dense convention: `Coefficients[k]` multiplies $x^k$
(**ascending** powers). Fixed-degree convenience types use school **descending** form
instead.

### Dense, sparse, fixed degree

```plato
type Polynomial
    implements Value
{
    Coefficients: Array<Number>;  // ascending; empty = zero polynomial
}

type Monomial
    implements Value
{
    Coefficient: Number;
    Power: Integer;
}

type SparsePolynomial
    implements Value
{
    Powers: Array<Integer>;       // distinct, ascending
    Coefficients: Array<Number>;
}

type QuadraticPolynomial
    implements Value
{
    A: Number;  // A*x^2 + B*x + C
    B: Number;
    C: Number;
}

type CubicPolynomial
    implements Value
{
    A: Number;
    B: Number;
    C: Number;
    D: Number;
}

type QuarticPolynomial
    implements Value
{
    A: Number;
    B: Number;
    C: Number;
    D: Number;
    E: Number;
}
```

### Roots and rationals

```plato
type PolynomialRoots
    implements Value
{
    RealRoots: Array<Number>;      // ascending, with multiplicity
    ComplexRoots: Array<Complex>;  // one per conjugate pair (Im > 0)
}

type RationalFunction
    implements Value
{
    Numerator: Polynomial;
    Denominator: Polynomial;       // not the zero polynomial
}
```

### Classical series and pieces

```plato
type PowerSeries
    implements Value
{
    Center: Number;
    Coefficients: Array<Number>;
}

type ChebyshevSeries
    implements Value
{
    Interval: NumberInterval;
    Coefficients: Array<Number>;
}

type BernsteinPolynomial
    implements Value
{
    Coefficients: Array<Number>;   // Bezier-like controls on [0, 1]
}

type FourierSeries
    implements Value
{
    Fundamental: Frequency;
    CosineCoefficients: Array<Number>;
    SineCoefficients: Array<Number>;
}

type PiecewisePolynomial
    implements Value
{
    Breakpoints: Array<Number>;    // ascending; one more than Pieces
    Pieces: Array<Polynomial>;     // local variable x - Breakpoints[i]
}

type SturmChain
    implements Value
{
    Polynomials: Array<Polynomial>;
}

type PadeApproximant
    implements Value
{
    Center: Number;
    Numerator: Polynomial;
    Denominator: Polynomial;       // constant term normalized to 1
}

type BivariatePolynomial
    implements Value
{
    Coefficients: Array2D<Number>; // (i,j) → x^i * y^j
}
```

Also: `LegendreSeries`, `HermiteSeries`.

Usage-shaped sketches:

```plato
// p(x) = 1 - 3x + 2x^2   (roots at x = 1 and x = 1/2)
let p = Polynomial {
    Coefficients: [1, -3, 2]
};

let q = QuadraticPolynomial { A: 2, B: -3, C: 1 };
// same polynomial in school form

let roots = PolynomialRoots {
    RealRoots: [0.5, 1],
    ComplexRoots: []
};

// Ray-sphere style: A t^2 + B t + C = 0 with A = d·d, etc.
let hitPoly = QuadraticPolynomial { A: a, B: b, C: c };
```

Horner nesting for $c_0 + c_1 x + c_2 x^2$:

```
((c2)*x + c1)*x + c0
```

## Pitfalls / fine print

**Ascending vs descending.** `Polynomial.Coefficients[0]` is the constant term;
`QuadraticPolynomial.A` is the $x^2$ coefficient. Mixing the layouts when converting is
the classic off-by-degree bug.

**Multiple roots.** Listing multiplicity in `RealRoots` matters for intersection codes that
must not double-count tangencies — or must specially handle them.

**Complex pairs.** Only one representative per conjugate pair (positive imaginary part) is
stored — reconstructing the conjugate is on you.

**Empty polynomial.** Empty coefficient array is zero, not "degree −1" in a casual sense;
degree conventions for the zero polynomial are historically messy.

**Rational poles.** Evaluating near denominator roots blows up; geometry code must reject
or clamp parameters that land on poles.

**Bernstein vs monomial.** Converting Bezier control points to monomial coefficients is
ill-conditioned at high degree — stay in Bernstein form when the algorithm allows.

**Piecewise local coordinate.** `PiecewisePolynomial` evaluates piece $i$ in
$x - \mathrm{Breakpoints}[i]$, not in global $x$. Feeding global $x$ into the piece
polynomial double-applies the shift.

## Try it

1. Write $x^2 - 5x + 6$ as a `Polynomial` coefficient array (ascending).
2. What are its real roots?
3. Why is Horner preferred over computing `pow(x,k)` per term for evaluation?

<details>
<summary>Answers</summary>

1. `[6, -5, 1]` because $6 - 5x + x^2$.
2. $x = 2$ and $x = 3$ (factor $(x-2)(x-3)$).
3. Fewer operations and usually better floating-point behavior; it also streams coefficients
   naturally from ascending storage.

</details>

## Library recommendations

- **missing-function** — `61-polynomials.plato`: no `Evaluate`, `Horner`, `Derivative`,
  `Add`, `Multiply`, or `Roots` declarations on `Polynomial` / `QuadraticPolynomial`.
  The lesson's core verbs are absent; only data shapes exist.

- **naming** — `61-polynomials.plato`: dual layouts (ascending dense vs descending
  `QuadraticPolynomial`) are documented but easy to miss. Aliases like
  `AscendingPolynomial` or conversion functions `ToPolynomial(QuadraticPolynomial)` would
  make the bridge explicit.

- **missing-type** — `61-polynomials.plato`: no `RootMultiplicity` pairing or interval
  isolation result (`IsolatedRoot` with bracket). `PolynomialRoots` is a flat list;
  Sturm-based pedagogy wants brackets before refinement.

- **doc-comment** — `61-polynomials.plato`: `BernsteinPolynomial` mentions Bezier control
  values but does not point at curve types elsewhere. A note that degree is
  `count(Coefficients) - 1` would match how Bezier degrees are taught.
