# Polynomials, series and basis functions

A coefficient index names a basis term. It is not universally a power of the raw input.

## Representation determines evaluation

A dense `Polynomial` stores ascending powers: `[2,3,4]` denotes `2 + 3*x + 4*x*x`, not `2*x*x + 3*x + 4`. Fixed-degree records use the school convention: `QuadraticPolynomial(A,B,C)` denotes `A*x*x+B*x+C`. Conversion between these records must reverse the corresponding order.

For coefficients `a[k]`, Horner's recurrence evaluates from the highest power down: begin with zero and repeatedly replace `r` by `r*x+a[k]`. It reduces multiplication work but may round differently from explicitly computing every power. A trailing zero coefficient can affect a representation-derived degree. Inspect normalization rather than assuming arrays are trimmed.

`SparsePolynomial` uses matching coefficient and power arrays with distinct, ascending nonnegative powers. `RationalFunction` requires a nonzero denominator polynomial. Evaluation at its roots is undefined. `PolynomialRoots` stores real roots with multiplicity and only the positive-imaginary representative of each strictly complex conjugate pair. Do not count that array as every complex root individually.

## Local coordinates and other bases

`PiecewisePolynomial` selects half-open spans between ascending breakpoints and evaluates piece `i` at `x-Breakpoints[i]`. Its final endpoint requires the concrete evaluator's policy. Do not infer it from inclusive geometric intervals. `PowerSeries` also shifts by its center.

Chebyshev series map the declared interval to `[-1,1]`. Legendre and Hermite series use their named classical bases. Bernstein coefficients act as control values on the unit interval. They are not monomial coefficients. Fourier series pair equal-length sine and cosine arrays. The zero cosine coefficient is DC, and the zero sine coefficient is ignored. Frequency supplies the temporal scale. On interchange with [NumPy's polynomial package](https://numpy.org/doc/stable/reference/routines.polynomials.html), match the basis and the coefficient order: an array does not identify its basis, and Plato's fixed-degree school-form records use a different order from its dense ascending-power record.

## Claims and limits

Padé and Sturm records describe mathematical constructions. A result record does not establish that approximation fitting or root isolation is implemented for every form. Degenerate leading coefficients, repeated roots, poles, near-singular denominators, and ill-conditioned roots require separate preconditions and numerical evidence. Plato stores finite arrays in named bases. The [DLMF](https://dlmf.nist.gov/) documents the infinite expansions, and a finite stored series has no automatically proved convergence or truncation-error bound.

## Where to look

- [Polynomial and series types](../../api/foundation/polynomials.types.md)
- [Polynomial operations](../../api/foundation/polynomials.library.md)
