# Intersect a ray with a triangle

Fire a ray straight down onto a triangle lying in the plane `z = 0`. Report whether it hits, how far away, where, and which way the surface faces.

Needs: `stdlib/foundation` and `stdlib/geometry`. Status: Supported.

## The program

Source: [ray-triangle.plato](../implementation/examples/cookbook-ray-triangle/ray-triangle.plato).

```plato
library CookbookRayTriangle {
    Triangle(): Triangle3D
        => new Triangle3D(new Point3D(0.0, 0.0, 0.0), new Point3D(4.0, 0.0, 0.0), new Point3D(0.0, 4.0, 0.0));

    // A ray fired straight down from (1, 1, 3).
    Down(x: Number, y: Number): Ray3D
        => new Ray3D(new Point3D(x, y, 3.0), new Direction3D(new Vector3D(0.0, 0.0, -1.0)));

    Law_ReferenceRayHits(): Boolean => Triangle().Raycast(Down(1.0, 1.0)).Hit;
    Law_ReferenceRayDistance(): Boolean => Triangle().Raycast(Down(1.0, 1.0)).Distance.AlmostEqual(3.0, 1e-9);
    Law_ReferenceRayPosition(): Boolean
        => Triangle().Raycast(Down(1.0, 1.0)).Position.Z.AlmostEqual(0.0, 1e-9);
    Law_ReferenceRayNormalOpposesRay(): Boolean
        => Triangle().Raycast(Down(1.0, 1.0)).Normal.Vector.Z.AlmostEqual(1.0, 1e-9);
    Law_ReferenceRayMissesOutside(): Boolean => Triangle().Raycast(Down(3.0, 3.0)).Hit.Not;
}
```

## Result

| Ray from | Outcome |
|---|---|
| (1, 1, 3) | hit at distance 3, position `z = 0`, normal `(0, 0, 1)` |
| (3, 3, 3) | miss: (3, 3) lies beyond the hypotenuse `x + y = 4` |

Distance, position and normal are checked to `1e-9`.

## Walk through

1. `Triangle3D` takes three corners. `Ray3D` takes an origin and a `Direction3D`.
2. `Raycast(triangle, ray)` solves the ray and triangle system directly (Moller-Trumbore) and returns a `RayHit3D`.
3. On a hit, `Distance` is the parameter along the ray, `Position` is the origin plus direction times distance, and `Normal` is the triangle normal flipped to oppose the ray, so it faces the viewer whichever side the ray arrives from.
4. On a miss, `Hit` is false and the other fields are placeholders; read them only after checking `Hit`.

## Library pages used

- Guide: [Capabilities and primitives](../library/geometry/primitives.md), [Points and vectors](../library/foundation/points-vectors.md).
- API: [lines.library](../api/geometry/lines.library.md) (`Raycast`, `MollerTrumbore`), [lines.types](../api/geometry/lines.types.md) (`Ray3D`), [spatial-structures.types](../api/geometry/spatial-structures.types.md) (`RayHit3D`).

## Limits

- A ray parallel to the triangle's plane misses, as does one that would cross behind its origin (negative distance).
- Backfaces are not culled: a ray arriving from behind also hits.
- The containment test is inclusive, so a ray through an edge or corner can hit; rounding decides on the boundary.
- The direction is taken as given. The program passes a unit vector; a non-unit direction would scale `Distance`.
