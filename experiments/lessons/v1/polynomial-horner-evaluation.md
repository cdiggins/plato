---
lesson: polynomial-horner-evaluation
title: Polynomial Evaluation with Horner's Method
domain: Math, statistics & signals
v3-files: [61-polynomials.plato]
audience: High-school algebra and general programming background
status: draft-v1
---

# Polynomial Evaluation with Horner's Method

A polynomial is the friendliest nontrivial function: add scaled powers of $x$. Evaluating
it naively — compute $x^2$, $x^3$, … then multiply by coefficients — wastes work and
invites rounding error. **Horner's method** nests the same arithmetic so each power
reuses the last multiply, cutting operations roughly in half and usually improving
numerical behavior.

Whenever a ray hits a quadratic surface, a cubic spline basis is sampled, or a color
response curve is applied, something is evaluating a polynomial. How you nest the
multiplies matters.

## The idea

Write a degree-$n$ polynomial in the **ascending** power basis:

$$
p(x) = c_0 + c_1 x + c_2 x^2 + \cdots + c_n x^n
$$

Horner rewrites it as nested multiplication:

$$
p(x) = c_0 + x\bigl(c_1 + x\bigl(c_2 + \cdots + x\, c_n\bigr)\cdots\bigr)
$$

Algorithm (high coefficient first):

```
acc ← cₙ
for k = n-1 down to 0:
    acc ← acc * x + cₖ
return acc
```

Example: $p(x) = 2 + 3x + 5x^2$ at $x = 4$.

```
acc = 5
acc = 5*4 + 3 = 23
acc = 23*4 + 2 = 94
```

Check: $2 + 3\cdot4 + 5\cdot16 = 2+12+80 = 94$.

Cost: $n$ multiplies and $n$ adds for degree $n$, versus roughly $2n$ multiplies if
you form each power separately. For evaluation-heavy loops (shaders, root polishers,
series), that constant factor is real money.

**Why ascending storage?** Graphics and CAD libraries often store $c_k$ as the
coefficient of $x^k$ so degree is `count - 1` and the zero polynomial is an empty
array. School notation $ax^2+bx+c$ is the opposite order — both are fine if you are
explicit.

**Derivatives.** Differentiating termwise is easy in the monomial basis. Horner has a
companion that evaluates $p$ and $p'$ together (useful for Newton iteration) by
carrying a second accumulator — same nesting idea.

## In Plato

`61-polynomials.plato` declares dense and sparse forms, plus fixed-degree convenience
types. Convention for dense arrays: **ascending powers**.

```plato
// Dense univariate polynomial. Coefficients[k] multiplies x^k.
type Polynomial
    implements Value
{
    Coefficients: Array<Number>;
}

// Sparse: term i is Coefficients[i] * x^Powers[i]
type SparsePolynomial
{
    Powers: Array<Integer>;
    Coefficients: Array<Number>;
}

// School form: A*x^2 + B*x + C  (descending)
type QuadraticPolynomial { A: Number; B: Number; C: Number; }

type CubicPolynomial { A: Number; B: Number; C: Number; D: Number; }
```

Usage-shaped construction (illustrative):

```plato
// p(x) = 2 + 3x + 5x^2   →  Coefficients = [2, 3, 5]
let p = Polynomial([2, 3, 5]);

// Same quadratic in school fields
let q = QuadraticPolynomial(5, 3, 2);   // A=5, B=3, C=2
```

v3 does **not** yet declare `Evaluate` or `Horner`. The mathematics still lives on
these types; a future library pass would expose something like:

```plato
// Desired shape (not declared yet — see recommendations)
// Evaluate(p: Polynomial, x: Number): Number
// using the Horner nest over Coefficients
```

Related forms that also need evaluation strategies:

```plato
type PowerSeries { Center: Number; Coefficients: Array<Number>; }
// sum Coefficients[k] * (x - Center)^k  — Horner in (x - Center)

type BernsteinPolynomial { Coefficients: Array<Number>; }
// Bézier-style basis on [0,1]; de Casteljau is the structured eval, not Horner

type PiecewisePolynomial
{
    Breakpoints: Array<Number>;
    Pieces: Array<Polynomial>;
}
// choose piece i on [Breakpoints[i], Breakpoints[i+1]), eval in local x
```

Roots and rational functions sit beside evaluation but are separate problems:

```plato
type PolynomialRoots
{
    RealRoots: Array<Number>;
    ComplexRoots: Array<Complex>;
}

type RationalFunction
{
    Numerator: Polynomial;
    Denominator: Polynomial;
}
```

Evaluating a rational is two Horners and a divide — watching for poles where the
denominator vanishes.

## Pitfalls / fine print

**Coefficient order.** Mixing ascending `Polynomial` with descending
`QuadraticPolynomial` without converting is the number-one silent bug. $A$ on a
quadratic is the $x^2$ weight; `Coefficients[2]` on a dense cubic is also $x^2$, but
`Coefficients[0]` is the constant, not $A$.

**Empty array.** Doc comment: empty `Coefficients` is the zero polynomial. Horner on
empty should return `0`, not crash.

**Leading zeros.** Degree is "count − 1" only if the last coefficient is nonzero.
Trailing zeros inflate degree and waste Horner steps; strip them when comparing
polynomials.

**Numerical cancellation.** Horner helps but does not cure ill-conditioning near roots
of high-degree polynomials. For Chebyshev economization or minimax fits, prefer
`ChebyshevSeries` evaluation (Clenshaw's algorithm) over converting to monomials then
using Horner.

**Integer powers in sparse form.** `SparsePolynomial` must not assume a dense Horner
sweep; jump by multiplying by $x^{\Delta p}$ between terms, or convert to dense when
degree is modest.

**Piecewise local coordinate.** `PiecewisePolynomial` evaluates piece $i$ in
$x - \mathrm{Breakpoints}[i]$, not global $x$. Forgetting the shift evaluates the wrong
polynomial on the right interval.

## Try it

1. Write Horner steps for $c = [1, 0, −2, 1]$ (that is $1 - 2x^2 + x^3$) at $x = 2$.
2. Convert `QuadraticPolynomial(A=1, B=−3, C=2)` to ascending `Polynomial` coefficients.
3. Why is evaluating $((((c_n)x + c_{n-1})x + \cdots)$ better than summing $c_k * x^k$
   with a running `xPow *= x`? Give two reasons.

<details>
<summary>Answers</summary>

1. $\mathrm{acc}=1$; $\mathrm{acc}=1\cdot2 + (-2)=0$; $\mathrm{acc}=0\cdot2 + 0=0$;
   $\mathrm{acc}=0\cdot2 + 1=1$. Check: $1 - 2\cdot4 + 8 = 1$.
2. Ascending $[C, B, A] = [2, -3, 1]$.
3. Fewer multiplies ($n$ vs ~$2n$), and typically better rounding because you avoid
   forming large intermediate powers before scaling by small coefficients.

</details>

## Library recommendations

- **missing-function** — `61-polynomials.plato`: `Polynomial` has no `Evaluate(Self, Number)`
  (Horner) and no `EvaluateDerivative` pair. The entire file is evaluation-shaped
  vocabulary without an evaluation entry point; this lesson cannot show a legal call.

- **missing-function** — `61-polynomials.plato`: no conversion helpers
  `ToPolynomial(QuadraticPolynomial)` / `ToQuadratic(Polynomial)` documenting the
  ascending ↔ descending map. Pedagogy and CAD interop both need them.

- **naming** — `61-polynomials.plato`: fixed-degree types use school letters `A..E`
  while dense uses ascending arrays. A short banner comment at the fixed-degree
  section ("A is highest power; convert with ToPolynomial") would prevent silent
  swaps.

- **pedagogy** — `61-polynomials.plato`: `BernsteinPolynomial` should note that Horner
  in the monomial basis after conversion is numerically inferior to de Casteljau on
  the Bernstein coefficients — otherwise callers "optimize" the wrong way.
