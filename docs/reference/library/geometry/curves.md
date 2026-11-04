# Curves, arcs and splines

A curve maps a scalar parameter to a point. Unless a concrete type says otherwise, the preferred domain is [0, 1], and a closed curve meets itself at the endpoints. Equal steps in that parameter are not equal distances along the curve. Arc-length parameterization is a separate capability.

## A parameter is not a distance

An angular curve takes an `Angle`. A bridge maps the numeric unit parameter onto that angle. A circular or elliptical arc stores a sweep, so a full circle, a short arc, and a reversed sweep can share one analytic circle. A curve with no placement fields sits where its declaration puts it. A transform places it.

`Eval`, a secant, a finite-difference tangent, and an analytic derivative are different contracts. The general helpers can estimate a tangent from samples. Only a differentiable capability returns derivative evidence. A finite-difference step is in parameter units, and it can cross a seam or leave the valid domain.

## Control points and basis choices

Interior Bézier control points are not points on the curve. The endpoints are. A rational Bézier or a NURBS also has weights, and the denominator has to stay meaningful. A B-spline knot vector has to match the declared order and multiplicity. A Hermite curve stores endpoint tangents. Changing the parameter scale changes what those tangents mean.

The [polynomial guide](../foundation/polynomials.md) explains the basis arithmetic. Turning a continuous curve into a polyline chooses a sample count or a tolerance. The polyline is an approximation, even when every sample lies on the original curve.

## What the bodies actually evaluate

The Catmull-Rom evaluator uses the uniform basis and ignores `Alpha`. A centripetal or chordal alpha does not select those algorithms. The source marks this as a cross-package TODO.

The comment above the knot accessors still says `NaturalCubicSpline1D` keeps no `Eval`, and it describes the tridiagonal solve as unfinished. The section under that comment does evaluate. `Eval` takes a unit parameter, maps it onto the knot domain, and clamps outside [0, 1]. Fewer than two knots returns the first knot value. The sweep recomputes earlier rows, and the source describes that evaluation as correct and not fast. The earlier comment is not the contract.

Comments in the analytic-curve bodies still say an angular curve interface is missing. The curve interface file now has those capabilities, and the curve helpers bridge a numeric parameter to an angle of `t` turns. Read the interface file before treating those comments as open gaps.

## Where to look

- [Curve interfaces](../../api/geometry/curves.concepts.md) and [curve helpers](../../api/geometry/curves.library.md)
- [Analytic curve types](../../api/geometry/curves-shapes.types.md) and [bodies](../../api/geometry/curves-shapes.library.md)
- [Spline types](../../api/geometry/splines.types.md) and [spline bodies](../../api/geometry/splines.library.md)
- [Polynomials, series and basis functions](../foundation/polynomials.md)

[DLMF's approximation and polynomial chapters](https://dlmf.nist.gov/) cover the basis mathematics, and [CGAL's package overview](https://doc.cgal.org/latest/Manual/packages.html) separates representations from interpolation algorithms. A shared `Eval` does not make parameterization, smoothness, or approximation accuracy the same from one spline family to another.
