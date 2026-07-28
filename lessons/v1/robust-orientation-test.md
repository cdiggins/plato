---
lesson: robust-orientation-test
title: Robust Orientation Tests in the Plane
domain: Planar geometry & uncertainty
v3-files: [17-planar-shapes.plato, 63-uncertainty.plato]
audience: Comfortable with 2D points and cross products; has seen a floating-point "left or right?" bug.
status: draft-v1
---

# Robust Orientation Tests in the Plane

Given three points $A$, $B$, $C$ in the plane, is $C$ to the left of the directed line
$AB$, to the right, or on it? That single **orientation** predicate drives convex hulls,
point-in-polygon, constrained triangulation, and boolean clipping. In exact arithmetic it
is the sign of a $2\times2$ determinant. In floating point, near-collinear inputs make the
sign flicker — and one flicker flips a hull edge.

Plato's planar shapes assume a winding convention; its uncertainty types name the
tolerances and covariances you need when the predicate must be *robust*, not merely
algebraic. This lesson ties those together.

## The signed-area predicate

For points $A$, $B$, $C$:

$$
\begin{aligned}
\mathbf{u} &= B - A,\qquad \mathbf{v} = C - A \\
\Delta &= u_x v_y - u_y v_x
\end{aligned}
$$

$\Delta$ is twice the signed area of triangle $ABC$.

| Sign of $\Delta$ | Geometric meaning (CCW-positive) |
|------------------|----------------------------------|
| $\Delta > 0$ | $C$ is left of directed line $AB$ |
| $\Delta < 0$ | $C$ is right of $AB$ |
| $\Delta = 0$ | $A$, $B$, $C$ collinear |

```
          C (left, Δ > 0)
         /
        /
   A ------→ B


   A ------→ B
        \
         \
          C (right, Δ < 0)
```

Plato's planar file banner states the convention up front: **counter-clockwise is
positive**. Triangles follow it:

```plato
// Three vertices, counter-clockwise for positive area. Degenerate when the
// vertices are collinear.
type Triangle2D
    implements Geometry2D, ClosedShape, ConvexShape, Connected,
               PlanarMeasurable, Bounded2D, PointSet2D, Centroid2D,
               ContainsPoint2D, NearestPoint2D, Deformable2D
{
    A: Point2D;
    B: Point2D;
    C: Point2D;
}
```

`PlanarMeasurable.Area` for a CCW triangle is positive; clockwise input yields negative
signed area if the implementation preserves orientation (callers often take absolute
value for "how much ink").

### Worked example

$A = (0,0)$, $B = (2,0)$, $C = (1,1)$:

$$
\Delta = 2\cdot 1 - 0\cdot 1 = 2 > 0 \quad\text{(left / CCW)}
$$

$C' = (1,-1)$ gives $\Delta = -2$ (right / CW). $C'' = (3,0)$ gives $\Delta = 0$
(collinear).

### Why floating point fails

When $C$ is almost on $AB$, $\Delta$ is a tiny cancellation of large products. Rounding
can push a mathematically positive value negative. Algorithms that branch on `sign(Δ)`
then take inconsistent paths — classic Shewchuk / adaptive-precision motivation.

Naive "epsilon" fixes:

```
if Abs(Δ) < eps then Collinear else ...
```

help only if `eps` matches the scale of your coordinates. A fixed $10^{-9}$ is
meaningless when points are in millimeters vs kilometers.

## In Plato — shapes and winding

`17-planar-shapes.plato` encodes orientation in type docs and concepts, not as a free
function named `Orient2D` (that function is exactly what a library should add — see
recommendations).

```plato
type Parallelogram2D
    implements Geometry2D, ClosedShape, ConvexShape, Connected,
               PlanarMeasurable, Bounded2D, Centroid2D, ContainsPoint2D, Deformable2D
{
    Origin: Point2D;
    SideA: Vector2D;
    SideB: Vector2D;
}
```

Signed area of the parallelogram is the 2D cross $A_x B_y - A_y B_x$ — the same
determinant as orientation of `Origin`, `Origin+SideA`, `Origin+SideB`.

`RegularPolygon` and `OrientedBox2D` carry an explicit `Rotation: Angle` for placement;
vertex order for measures still obeys the file-wide CCW-positive rule.

```plato
concept Orientable
{ }
```

`Orientable` in the geometry concepts file marks shapes that admit a consistent global
orientation (two-sided surfaces). In 2D closed shapes, that pairs with the winding
convention rather than replacing the orientation predicate.

### Uncertainty vocabulary for robustness

From `63-uncertainty.plato`:

```plato
type Tolerance
    implements Value
{
    Nominal: Number;
    Plus: Number;
    Minus: Number;
}

type UncertainPoint2D
    implements Value
{
    Value: Point2D;
    Covariance: Covariance2D;
}

type UncertainNumber
    implements Value
{
    Value: Number;
    StandardUncertainty: Number;
}
```

Use these when inputs are measurements, not ideal CAD vertices:

| Tool | Role in orientation |
|------|---------------------|
| `Tolerance` | Engineering band around a nominal clearance or snap distance |
| `UncertainPoint2D` | Point with Gaussian covariance — propagate into $\mathrm{Var}(\Delta)$ |
| `UncertainNumber` | Treat $\Delta$ itself as uncertain; decide Left/Right only if separated from zero by enough sigmas |
| `ConfidenceInterval` | Report a range for $\Delta$ instead of a brittle sign |

A **robust decision** might be:

1. Compute $\Delta$ from nominal coordinates.
2. Estimate $\sigma_\Delta$ from point covariances (or a scale-aware epsilon).
3. If $|\Delta| < k\,\sigma_\Delta$, return `Collinear` / `Uncertain`; else return the sign.

Exact arithmetic (adaptive predicates) is another path; Plato's uncertainty types do not
replace that machinery, but they name the *measurement* side of the problem.

### Scale-aware epsilon without covariance

When points are exact but floating, a common heuristic uses the magnitude of the inputs:

$$
\varepsilon \sim \eta \cdot \max(|u_x|,|u_y|,|v_x|,|v_y|) \cdot \max(|A|,|B|,|C|)
$$

with machine epsilon $\eta$. The constant is empirical; the point is **relative**
thresholds, not a global magic number stored as a bare `Number` with no unit of meaning.

`Tolerance` can document an authoring snap: "collinear if within $\pm 0.01$ model
units of the line," which is a product decision, not a substitute for adaptive
predicates in a kernel.

## Pitfalls and fine print

**Absolute epsilon.** `Abs(Δ) < 1e-9` without knowing coordinate scale is a bug farm.

**Winding vs predicate.** File convention is CCW-positive. Importing clockwise SVG paths
without reversing will invert "left" and "inside" consistently — until you mix sources.

**Degenerate triangles.** Collinear `Triangle2D` vertices are documented as degenerate;
`Area` near zero should be treated like $\Delta \approx 0$, not as a valid face.

**Covariance misuse.** Feeding `UncertainPoint2D` into a naïve sign test that ignores
`Covariance` is theater — you stored uncertainty and threw it away.

**3D confusion.** Orientation of three points in a plane embedded in 3D needs a plane
normal; this lesson's $\Delta$ is strictly 2D. Do not apply the $2\times2$ formula to
`Point3D` coordinates without projecting.

## Try it

<details>
<summary>Exercise 1 — Sign reading</summary>

$A=(0,0)$, $B=(0,2)$, $C=(-1,1)$. Is $C$ left or right of $AB$?

**Answer.** $\mathbf{u}=(0,2)$, $\mathbf{v}=(-1,1)$, $\Delta = 0\cdot1 - 2\cdot(-1) = 2 > 0$
→ left (CCW from $AB$ to $AC$).
</details>

<details>
<summary>Exercise 2 — Tolerance band</summary>

You snap collinearity with `Tolerance(Nominal: 0, Plus: 0.01, Minus: 0.01)` on the
*distance* from $C$ to line $AB$, not on raw $\Delta$. Why might that be clearer for
artists than thresholding $\Delta$?

**Answer.** Distance is in model units people understand; $\Delta$ scales with edge
length, so the same geometric offset yields larger $|\Delta|$ on longer segments.
</details>

<details>
<summary>Exercise 3 — Uncertain decision</summary>

Nominal $\Delta = 1\mathrm{e}{-6}$ and a propagation gives $\sigma_\Delta = 1\mathrm{e}{-5}$.
Using a 2-sigma rule, what should a robust classifier return?

**Answer.** $|\Delta| < 2\sigma$ → treat as uncertain/collinear, not a confident Left.
</details>

## Library recommendations

- **missing-function** — `17-planar-shapes.plato` / vectors: no declared
  `Orient2D(a, b, c): Integer` (or a sum type `Left | Right | Collinear`). The winding
  docs assume the predicate; the API never names it. This is the highest-value add for
  computational geometry consumers.

- **missing-function** — no `SignedArea(Triangle2D)` distinct from absolute
  `PlanarMeasurable.Area`. Teaching robustness needs the signed quantity explicitly.

- **doc-comment** — `63-uncertainty.plato`: `Tolerance` is framed as engineering
  plus/minus about a nominal, not as a geometric epsilon for predicates. A short note on
  (non-)use for orientation tests would prevent misuse as a global `eps`.

- **missing-function** — no bridge from `UncertainPoint2D` triples to an uncertain
  $\Delta$ (`UncertainNumber`). Without propagation helpers, covariance fields stay
  decorative in geometry kernels.

> Resolved 2026-07-28: planar-shapes.plato added Orient2D(a, b, c): Integer (+1 left / -1 right / 0 collinear) (343) and SignedArea(Triangle2D) distinct from the absolute Area (344).
