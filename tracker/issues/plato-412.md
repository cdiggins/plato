---
id: plato-412
title: ToSdf: concrete geometry shapes as signed distance fields
type: feature
status: in-progress
priority: p2
effort: M
risk: low
area: plato
sprint: 
created: 2026-08-02
closed:
links: [stdlib/geometry/fields-implicits.library.plato, stdlib/geometry/implicit-sdf.library.plato, stdlib/tests/implicit-sdf.laws.plato, tracker/issues/plato-409.md]
---

## Problem

The implicit corner of the stdlib and the boundary-representation corner never met. There
were closed-form distance primitives over raw points and vectors, and there were concrete
shape types (`Sphere`, `Circle`, `Plane`, `Triangle2D/3D`, `Quad2D/3D`, `Line2D/3D`,
`Capsule2D/3D`, …), and nothing turned one into the other. A caller holding a `Sphere` had
to know the primitive's name, its centred frame, and how to write the lambda — so the
implicit machinery (CSG, smoothing, offsetting, ray marching, lattice sampling) was
unreachable from ordinary geometry without hand-written glue at every call site.

Two of the primitives that a bridge most obviously wants did not exist either: the triangle
and the quadrilateral, in both dimensions. plato-409's operator survey recorded them as
deliberately-not-ported catalog work.

## Design decisions

- **`ToSdf`, explicit, never an implicit conversion.** The field cannot recover the shape it
  came from (`CONVENTIONS.md` - Conversions).
- **Where the lambda lives.** The primitive bodies in `implicit-sdf.library.plato` stay
  array-free and lambda-free so they port to GLSL/C++/CUDA unchanged; every `ToSdf` lives in
  `fields-implicits.library.plato`, which is already the file that holds lambda-valued
  fields. The conversion closes over its shape and defers to the primitive.
- **Three sign grades, documented per shape.** Solids give a true signed field. Half-regions
  (`Plane`, `HalfSpace`, `HalfPlane2D`) give the signed field of the region behind the
  boundary. Zero-thickness shapes — a segment, and a triangle or quad patch in SPACE — have
  no interior, so their field is non-negative and `IsInside` is false everywhere. Those carry
  a second overload taking a radius or a thickness, which is the shape's Minkowski sum with a
  ball and has an exact signed distance.
- **The 3D patch bodies are named `UnsignedDistanceTo*`.** `DistanceTo*` in that section
  means signed; a patch has no sign to give, and a name that hid the difference would be the
  easiest wrong assumption in the library to make.
- **A degenerate planar triangle is NOT inside-everywhere.** `Contains(Triangle2D, Point2D)`
  is a closed-set membership test whose three-way sign agreement reports a collinear triangle
  as containing every point. That is defensible for containment and useless for a field, so
  `DistanceToTriangle` compares each edge test against the triangle's own orientation and
  excludes the zero-orientation case. `Law_CollinearTriangleIsSegmentDistance` pins it.
- **The planar quad's sign is the even-odd crossing rule**, reusing `EdgeCrossesRay`, so
  concave quads work; the spatial quad patch uses IQ's four edge-vote test, which assumes the
  corners are coplanar.

## Related

- [plato-409](plato-409.md) — the SDF umbrella. Its survey named triangle and quad as the
  unported primitives; this issue ports them and adds the bridge that motivated them.
- `docs/plato-library-map.md` — where these files sit in the artifact map.

## Done means

- [x] A `Triangle2D` / `Quad2D` has an exact signed distance, and a `Triangle3D` / `Quad3D`
      an exact unsigned distance plus a thickened signed form
- [x] `Line2D/3D`, `Plane`, `HalfSpace`, `HalfPlane2D`, `Sphere`, `Circle`, `Capsule2D/3D`
      and `Annulus` convert to a field with `ToSdf`
- [x] Laws cross-check each new primitive against an independently written one
- [x] `plato_check` clean across the touched files (no new lint, type, or style diagnostics)
- [ ] The remaining analytic shapes convert too (see below)

## Follow-up: the shapes still without a `ToSdf`

Left out deliberately, because each needs a frame change (a centre plus an orientation) that
the current conversions do not, and the placement question is plato-409's `PlacedSdf*` work
rather than this one's:

`Box3D` and `RoundedBox3D` (centre + `Quaternion`), `Cylinder` and `ConicalFrustum` and
`Cone` (centre + axis direction), `Torus`, `Disk3D`, `Ellipsoid` and `Ellipse` (bound, not
exact), `RoundedRect2D`, `OrientedBox2D`, `Bounds2D/3D`, `SphericalShell`, `Parallelogram2D`.

Separately array-valued and therefore a different shape of body: `Polygon2D`,
`Polyline2D/3D`, `ConvexPolytope3D` / `ConvexVolume` (max over the bounding planes — a lower
bound, exact only outside the face regions), and `Tetrahedron`. A triangle-mesh conversion is
the big one and belongs in its own issue: the minimum over per-face unsigned fields is itself
unsigned, and recovering a sign for a closed mesh needs an angle-weighted pseudonormal or a
generalized winding number, plus a BVH to make it affordable.
