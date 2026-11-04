# Transform points by a rotation and a translation

A part has three corner points in its own plane. Place it by turning it a quarter turn about the origin and then shifting it by (10, 5).

Needs: `stdlib/foundation`. Status: Supported.

## The program

Source: [transform-points.plato](../implementation/examples/cookbook-transform-points/transform-points.plato).

```plato
library CookbookTransformPoints {
    // Rotate every point a quarter turn about the origin, then shift it by (10, 5).
    // The pose becomes one affine map, which rotates and then translates.
    Move(points: Array<Point2D>): Array<Point2D> {
        var pose = new Pose2D(new Point2D(10.0, 5.0), new Rotation2D(90.0.Degrees));
        var map = pose.AffineTransform2D;
        return points.Map(p => p.Transform(map));
    }

    Corners(): Array<Point2D>
        => [new Point2D(0.0, 0.0), new Point2D(1.0, 0.0), new Point2D(1.0, 2.0)];

    Law_ReferenceMoveOrigin(): Boolean
        => Move(Corners())[0].X.AlmostEqual(10.0, 1e-9) && Move(Corners())[0].Y.AlmostEqual(5.0, 1e-9);
    Law_ReferenceMoveFirstAxisPoint(): Boolean
        => Move(Corners())[1].X.AlmostEqual(10.0, 1e-9) && Move(Corners())[1].Y.AlmostEqual(6.0, 1e-9);
    Law_ReferenceMoveThirdPoint(): Boolean
        => Move(Corners())[2].X.AlmostEqual(8.0, 1e-9) && Move(Corners())[2].Y.AlmostEqual(6.0, 1e-9);
}
```

## Result

| Input | Output |
|---|---|
| (0, 0) | (10, 5) |
| (1, 0) | (10, 6) |
| (1, 2) | (8, 6) |

Each coordinate is checked to `1e-9`; the rotation goes through `cos` and `sin`, so a result is not bit-exact.

## Walk through

1. A `Pose2D` pairs a position with a `Rotation2D`. It is a rigid motion: the rotation is about the origin, then the position is added.
2. The angle is built with `90.0.Degrees`. An angle is always the `Angle` type, stored in radians, never a bare number.
3. `AffineTransform2D` widens the pose to the common affine form, so one map serves every point.
4. `p.Transform(map)` applies the map to a point, translation included. A `Vector2D` with the same map would skip the translation.

## Library pages used

- Guide: [Matrices and transforms](../library/foundation/matrices-transforms.md), [Angles](../library/foundation/angles.md), [Points and vectors](../library/foundation/points-vectors.md).
- API: [transforms.types](../api/foundation/transforms.types.md) (`Pose2D`), [transforms.library](../api/foundation/transforms.library.md) (`Transform`, `AffineTransform2D`), [angles.library](../api/foundation/angles.library.md) (`Degrees`).

## Limits

- The order is fixed by the pose: rotate about the origin, then translate. To rotate about another centre, use a `RotationAbout2D` or compose maps.
- Angles follow the library's convention (radians stored, canonical interval `(-pi, pi]`); see [stdlib/CONVENTIONS.md](../../../stdlib/CONVENTIONS.md).
- Results carry floating-point rounding; compare with a tolerance, as the laws do.
