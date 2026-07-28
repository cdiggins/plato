---
lesson: coordinate-frames
title: Coordinate Frames
domain: Matrices & transforms
v3-files: [13-transforms.plato]
audience: High-school vectors and general programming background
status: draft-v1
---

# Coordinate Frames

The same thumbtack on your desk has many correct coordinates: centimeters from
the desk corner, inches from the room door, millimeters from a CAD origin.
Nothing about the thumbtack changed — only the **frame** you measured in.

A coordinate frame is an origin plus a set of axes. Change the frame, change
the numbers; the geometric point is one. Graphics pipelines, robotics, and CAD
spend half their lives converting "the same point described twice."

## The idea

In 3D, an **orthonormal frame** is:

- an **origin** point $O$
- three mutually perpendicular **unit** axes $\hat{x}, \hat{y}, \hat{z}$
  (usually right-handed: $\hat{z} = \hat{x} \times \hat{y}$)

A point's **local** coordinates $(u, v, w)$ mean

$$
P = O + u\,\hat{x} + v\,\hat{y} + w\,\hat{z}.
$$

Those $(u,v,w)$ are numbers in the frame; $P$ is the world-space point (once
you agree what "world" is — itself just another frame).

```
        ẑ
        |
        |
        O----ŷ
       /
      x̂

  Local (1,0,0) is the point one unit along x̂ from O —
  not the world point (1,0,0) unless O is the world origin
  and x̂ is world +X.
```

A **basis** without an origin is just three vectors (axes of a linear map).
Bases need not be orthonormal — they can be skewed or scaled. Frames in Plato
insist on orthonormal unit axes; `Basis3D` allows general vectors.

Change of coordinates is a rigid pose: the frame's axes are a rotation, the
origin a translation. Moving data from local to world applies that pose;
world to local applies the inverse.

## In Plato

From `13-transforms.plato`:

```plato
// An orthonormal coordinate frame in space: an origin and three mutually
// perpendicular unit axes.
type Frame3D
    implements Value
{
    Origin: Point3D;
    XAxis: Direction3D;
    YAxis: Direction3D;
    ZAxis: Direction3D;
}

// Three basis vectors in 3D; not necessarily orthonormal.
type Basis3D
    implements Value
{
    X: Vector3D;
    Y: Vector3D;
    Z: Vector3D;
}

// An orthonormal coordinate frame in the plane.
type Frame2D
{
    Origin: Point2D;
    XAxis: Direction2D;
    YAxis: Direction2D;
}
```

Axes on `Frame3D` are `Direction3D` — normalized by construction — so the type
enforces unit length. `Basis3D` uses `Vector3D`, so length and orthogonality
are the caller's problem.

Rigid poses are the compact twin of frames:

```plato
type Pose3D
{
    Position: Point3D;
    Orientation: Quaternion;
}
```

Library conversions (frames as "poses in matrix-friendly clothing"):

```plato
// Local → world matrix: axes as linear rows, origin as translation row.
Matrix4x4(f: Frame3D): Matrix4x4 { ... }

// Frame from a pose: rotate world unit axes by the orientation at Position.
Frame3D(pose: Pose3D): Frame3D {
    var q = pose.Orientation;
    var x = Vector3D(1.0, 0.0, 0.0).Transform(q);
    var y = Vector3D(0.0, 1.0, 0.0).Transform(q);
    var z = Vector3D(0.0, 0.0, 1.0).Transform(q);
    return (pose.Position, Direction3D(x), Direction3D(y), Direction3D(z));
}

// Pose from a frame (precondition: orthonormal, right-handed).
Pose3D(f: Frame3D): Pose3D
    => f.Matrix4x4.Pose3D;

// Basis from a pure rotation (no origin).
Basis3D(q: Quaternion): Basis3D
    => (Vector3D(1,0,0).Transform(q),
        Vector3D(0,1,0).Transform(q),
        Vector3D(0,0,1).Transform(q));
```

Usage-shaped snippets:

```plato
let worldOrigin = Point3D { X: 0.0, Y: 0.0, Z: 0.0 };
let pose = Pose3D {
    Position: Point3D { X: 10.0, Y: 0.0, Z: 0.0 },
    Orientation: Quaternion.Identity
};
let frame = pose.Frame3D;

// Local point (1,0,0) in this frame → world (11,0,0)
let local = Point3D { X: 1.0, Y: 0.0, Z: 0.0 };
let world = local.Transform(pose);

// Same change of coordinates via the frame matrix
let M = frame.Matrix4x4;
let world2 = local.Transform(M.AffineTransform3D);  // when applicable

// General (possibly skewed) axes
let basis = Basis3D {
    X: Vector3D { X: 2.0, Y: 0.0, Z: 0.0 },
    Y: Vector3D { X: 0.5, Y: 1.0, Z: 0.0 },
    Z: Vector3D { X: 0.0, Y: 0.0, Z: 1.0 }
};
```

World-to-local is the inverse pose: undo translation, then undo rotation —
`pose.Inverse`, then `Transform`.

## Pitfalls / fine print

**Local vs world confusion.** Drawing with local numbers in a world shader, or
the reverse, is the classic bug. Name variables `localPos` / `worldPos` and
convert explicitly through `Pose3D` / `Frame3D`.

**Left-handed vs right-handed.** `Pose3D(f: Frame3D)` requires a right-handed
orthonormal frame. A mirrored axis set fails the precondition or flips
winding.

**Directions vs vectors.** Frame axes are `Direction3D`. Feeding non-unit
vectors into a hand-built `Frame3D` violates the type's invariant even if the
record syntax allows the fields at construction time in loose sketches.

**Basis ≠ Frame.** `Basis3D` has no origin. Using it to place objects forgets
translation. Using `Frame3D` where a linear change-of-basis (normals, inertia)
is needed may over-specify.

**Parent chains.** A child's frame expressed in the parent, times the parent's
frame in world, yields the child's world frame — compose poses/matrices, do not
add origins as if they were vectors in the same space without rotating.

**Orthonormal drift.** Reconstructing frames from noisy data needs
re-orthogonalization; floating-point pose integration drifts off the manifold.

## Try it

1. Frame with origin $(5,0,0)$ and identity axes. What world point is local
   $(0,2,0)$?
2. Why are `Frame3D` axes `Direction3D` while `Basis3D` axes are `Vector3D`?
3. If `pose` takes local to world, which transform takes world to local?

<details>
<summary>Answers</summary>

1. $(5, 2, 0)$ — origin plus $2\,\hat{y}$.
2. Frames are orthonormal by definition (unit axes). Bases may be scaled or
   skewed, so they need general vectors.
3. `pose.Inverse` (then `Transform`).

</details>

## Library recommendations

- **missing-function** — `13-transforms.plato`: no
  `ToWorld(local: Point3D, frame: Frame3D): Point3D` /
  `ToLocal(world: Point3D, frame: Frame3D): Point3D` sugar. Everything goes
  through `Pose3D` or `Matrix4x4`, which is correct but heavier than the
  teaching vocabulary "express this point in that frame."

- **missing-function** — `13-transforms.plato`: `Basis3D` has a constructor from
  `Quaternion` but no `Matrix3x3(basis)`, `Orthonormalize`, or
  `Frame3D(origin, basis)` that checks/consumes a basis. The split is clear;
  the bridges are thin.

- **missing-function** — no `Compose(parent: Frame3D, childLocal: Frame3D)` or
  equivalent parent-child helper, even though pose composition exists. Scene
  graphs are frames of frames; the API stops at poses.

- **doc-comment** — `Frame3D`: state right-handed orthonormal invariant and
  "matrix maps local coordinates to the parent/world space of the axes" on the
  type itself so `Matrix4x4(f)`'s meaning is unambiguous.
