---
id: plato-434
title: Collision pair tests are unusable in TypeScript: sphere-box silently reports no contact, sphere-capsule throws, box corners duplicate
type: bug
status: ready
priority: p1
effort: M
risk: med
area: plato
sprint: 
created: 2026-08-03
closed:
links: []
---

## What and why

The rigid-body demo (`plato-425`'s demo box) is the first thing to execute
`stdlib/future/collision.library.plato` and `rigid-dynamics.library.plato`. **The
solver itself is sound** — stacks settle, energy decays to 1e-32 J, warm starting
and split impulse both do what the source claims, and restitution and friction
match theory. This issue is about everything around it that does not survive the
TypeScript writer.

Every item below is an instance of a defect already catalogued in `plato-419`.
They are collected here because together they mean **only ball-vs-ball simulation
is trustworthy on this backend**, which is a much bigger statement than any one of
them, and because two of them fail silently.

## The defects, worst first

**1. The 2^n lazy fold. This one is a hang, not a slow path.** `Step` ends in
`IntegratePoses`, whose `Map` reads *both* `Bodies[i]` and `corrections[i]` —
itself a `Map` over `Bodies`. `Arr.Map` is lazy, so each step doubles the
traversal depth. Repeated `world = world.StepBallScene(...)` with no eager
rebuild, cost to read **one** `Center`:

| Frame | 0 | 10 | 15 | 20 | 25 |
|---|---|---|---|---|---|
| ms | 0.87 | 5.9 | 63.5 | 1984 | **75 003** |

No throw, no NaN — the page simply stops. The demo works around it by
materializing `Bodies` and `Constraints` after every step. This is the same trap
`demos/webgl/README.md` warns demo authors about and that `plato-430` records
biting a shipped `sampling` member; here it bites the central loop of a
simulation library, where every user will hit it. **The fix belongs in the
library or the writer, not in each caller.**

**2. `Sphere.Collide(Box3D)` silently reports "not touching" at any depth.** A
dropped overload (plato-419 defect 3) means the ball-vs-ball body runs; `b.Radius`
is `undefined`, penetration is `NaN`, and `NaN > 0` is false — so the manifold
comes back **empty**, which reads as "apart" rather than as an error. Verified: a
sphere of radius 1 at (0, 0, 0.5) against
`new Box3D(new Point3D(0,0,0.5), new Size3D(1,1,1), Quaternion.Identity())`
returns 0 points. This is the worst failure shape in the catalogue — no
exception, no NaN reaching the caller, just objects passing through each other.

**3. `Box3D.BoxCorner(index)` returns duplicate corners.** `index / 2 % 2` and
`index / 4` are emitted as float division (plato-419 defect 4), so only corners 0,
1, 2 and 4 are distinct — 3, 5, 6 and 7 duplicate earlier ones. Consequence:
`Box3D.Collide(Plane)` returns **7 contact points for a box lying flat** where the
source says 4. Failing call:
`new Box3D(new Point3D(0,0,0.2), new Size3D(1,1,1), Quaternion.Identity()).Collide(new Plane(new Direction3D(new Vector3D(0,0,1)), 0))`.

**4. `Sphere.Collide(Capsule3D)` throws** — `Cannot read properties of undefined
(reading 'Offset')`. Loud, so the least dangerous of the four.

**5. `SolverBody3D.ApplyImpulse(AppliedImpulse3D)` and `(RadialImpulse3D)` are
dropped** — `impulse.Multiply is not a function`. The `(Vector3D, Point3D)` form
works, which is enough for a demo.

**6. Two typing gaps that work at runtime but not in TypeScript.**
`MaterialCombine` is installed on `globalThis` by the demo prelude but is not
exported from `plato.g.ts`, so a caller can run it but cannot import or type it.
`Vector3D.Zero` is called statically throughout `plato.g.ts` and declared only as
an instance method; callers must write `new Vector3D(0, 0, 0)`.

## Consequence, stated plainly

`StepBallScene` is ball-only, and defects 2 to 4 mean no box or capsule pair test
can be trusted, so **spheres are the only shape this library can simulate on the
TypeScript backend today**. That is worth knowing before anyone builds on it, and
it is not what the library's own vocabulary implies.

## Done means

- [ ] Stepping a world repeatedly does not compound traversal depth — fixed in
      the library or the writer rather than in each caller
- [ ] `Sphere.Collide(Box3D)` reports contact, or fails loudly
- [ ] `Box3D.BoxCorner` returns eight distinct corners, and `Box3D.Collide(Plane)`
      returns four points for a box lying flat
- [ ] `Sphere.Collide(Capsule3D)` runs
- [ ] `ApplyImpulse`'s record overloads emit
- [ ] `MaterialCombine` is importable and `Vector3D.Zero` is callable as declared
