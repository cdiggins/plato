# Curves, arcs and splines

Read [curve interfaces](../../api/geometry/curves.concepts.md), [curve helpers](../../api/geometry/curves.library.md), [analytic curve types](../../api/geometry/curves-shapes.types.md) and [bodies](../../api/geometry/curves-shapes.library.md), [spline types](../../api/geometry/splines.types.md) and [spline bodies](../../api/geometry/splines.library.md).

## Parameter is not distance

A curve maps a scalar parameter to a point. Unless a concrete type says otherwise, its preferred parameter domain is `[0,1]`; a closed curve agrees at the endpoints. Equal increments in the parameter need not produce equal world-space distances. Arc-length parameterization is an extra capability.

An angular curve's natural input is `Angle`; a bridge maps its numeric unit parameter to that angle. Circular or elliptical arc records carry a sweep; a full circle, a short arc and a reversed sweep can share the same underlying analytic circle. A curve without placement fields is in the canonical position described by its declaration; use a transform to place it.

`Eval`, a secant, a finite-difference tangent and an analytic derivative are distinct contracts. The general helpers can estimate from evaluations; only a differentiable capability supplies derivative evidence. A finite-difference step is in parameter units and can cross a seam or an invalid domain endpoint.

## Control points and basis choices

Bézier control points determine a polynomial curve; they are generally not interpolation points except for endpoints. Rational Bézier and NURBS additionally use weights and a denominator that must remain meaningful. B-spline knot vectors must satisfy their declared order and multiplicity conditions. Hermite curves carry endpoint tangents; changing a parameter scale changes the meaning of those tangents.

The [polynomial guide](../foundation/polynomials.md) explains the basis arithmetic. Converting a continuous curve to a polyline chooses a sample count or tolerance and is explicit approximation, even if the sampled points lie exactly on the original curve.

## Explicit implementation gaps

The Catmull-Rom evaluator uses the uniform basis and **ignores the declared `Alpha` parameter**. Setting centripetal or chordal alpha does not currently select those algorithms. The source records this as a cross-package TODO.

`NaturalCubicSpline1D` has knot accessors but no evaluator in the spline body file; the documented missing step is the tridiagonal solve for second derivatives. Neither a declaration nor accessors make it usable as an evaluated natural spline. Other comments describing missing angular interfaces must be checked against the actual curve interface file, which now contains angular capabilities.

## Related work

[DLMF's approximation and polynomial chapters](https://dlmf.nist.gov/) supply basis mathematics. [CGAL's package overview](https://doc.cgal.org/latest/Manual/packages.html) separates representations from interpolation algorithms. Plato's shared evaluation interface does not make parameterization, smoothness, or approximation accuracy interchangeable across spline families.
