---
id: plato-426
title: Cloth simulation: mass-spring and PBD cloth over the particle vocabulary
type: feature
status: in-progress
priority: p2
effort: M
risk: low
area: plato
sprint: 
created: 2026-08-03
closed:
links: []
---

## What and why

`stdlib/future/particles.types.plato` declares `Particle2D` / `Particle3D`,
`ParticleSystem2D/3D`, emitters and the force types (`ParticleGravity`,
`ParticleDrag`, `ParticleVortex`, `ParticleAttractor`, `ParticleTurbulence`), and
`SpringParameters` names stiffness/damping/mass — but nothing steps a particle,
and there is no cloth. Cloth is the smallest simulation that shows the whole
pipeline: a mesh becomes a constraint graph, the graph is solved, the solution
becomes a mesh again.

Scope: **`stdlib/future`** — not linted, not converted to C#, but it **must parse
and type-check** (`ForwardStdLib*` reads all four tiers), and nothing in a shipping
tier may reference it. Coordinate with plato-425 (rigid dynamics): if both want a
shared force or integrator vocabulary, one of you declares it and the other reuses
it — say in your report which way it went.

Subject matter, roughly:

- **Particle state and a step**: position, previous position or velocity, inverse
  mass, pinned flag. Verlet and semi-implicit Euler both work; pick one, say why.
- **The force set**: gravity, drag, wind, and the declared `Particle*` force types,
  applied as pure accumulations.
- **Cloth from a mesh**: structural (edge), shear (diagonal) and bend (two-away)
  springs derived from a grid or from any `PolygonMesh3D`'s edges, with rest lengths
  taken from the initial configuration.
- **Two solvers, because they illustrate different things**: mass-spring with
  Hookean forces and explicit integration (simple, and visibly unstable at high
  stiffness — that is a feature for a demo), and **position-based dynamics** with
  distance-constraint projection over Gauss-Seidel iterations plus a stiffness
  exponent, which is stable and is what people actually ship.
- **Constraints beyond distance**: pinning, bending as a dihedral-angle constraint,
  and self-distance or collision against a sphere/plane/SDF.
- **Reading the result back**: the simulated positions rebuilt into a
  `PolygonMesh3D` with the original topology, so the existing mesh and normal paths
  render it with no new plumbing.

Purity again is the shaping constraint: Gauss-Seidel projection is normally in-place.
Say how you expressed the iteration and what it costs.

## Design decisions

**Files.** Three new files under `stdlib/future`: `cloth.types.plato` (4 types),
`cloth.library.plato` (`library Cloth`), `particles.library.plato`
(`library Particles`); one type added to `particles.types.plato`. No
`cloth.concepts.plato` — see "no collider interface" below. Nothing in a
shipping tier was touched, so `types-and-concepts.txt` (which excludes `future`)
does not move.

**Gauss-Seidel expressed as a left fold, and what it costs.** Constraint
projection is defined by its sequencing: constraint *k* must see the corrections
of constraints 0..*k*-1, which is why textbook implementations write one array in
place. `Reduce` has exactly that sequencing, so a sweep is
`constraints.Reduce(vertices, (current, c) => ProjectDistance(current, c, ...))`
— the whole vertex array is the accumulator, and each projection returns a new
array instead of mutating a shared one. Iteration order is the constraint
array's own order, as in the in-place version, and the arithmetic and
convergence are identical.

The cost is one array rebuild per constraint: a projection is O(vertices)
instead of O(1), so a sweep is O(vertices x constraints) against the in-place
O(constraints). Repetition is the same trick one level up —
`iterations.MapRange(i => i).Reduce(vertices, ...)`.

*Rejected: `Buffer<ClothVertex>`.* An affine builder would restore the in-place
cost and stay pure by uniqueness, but a builder may not be captured in a lambda
(LINT006 / LINT007 ban storing one in a field or passing it as a generic
argument), and Plato has no loop form — so nothing can drive a per-constraint
sweep over one. This is the real reason the fold is the answer, not a
preference.

*Rejected: a Jacobi sweep.* Evaluating every constraint against the same input
and averaging the per-vertex corrections needs only one rebuild per sweep, but
doing that in O(constraints) rather than O(vertices x constraints) needs a
per-vertex constraint adjacency table this vocabulary does not build, and Jacobi
converges more slowly and needs under-relaxation — a different solver, not a
faster spelling of this one. Noted as a follow-up.

**Integrator: position Verlet for cloth, semi-implicit Euler for particles.**
`ClothVertex` already stores `{Position, PreviousPosition}`, and that pair is
exactly what PBD wants: its post-solve velocity update is
`(corrected - pre-step) / dt`, which *is* the Verlet difference quotient, so
storing a velocity as well would be two answers to one question. It also means
every response in the file — pinning, projection, contact — is expressed by
moving a position and gets the right velocity for free; with a stored velocity
each would need a second edit, and a missed one is the classic cause of cloth
that sticks to a collider. `Particle3D` stores a velocity instead, so
`particles.library.plato` uses semi-implicit (symplectic) Euler: same cost as
explicit Euler, but it does not spiral particles out of an attractor orbit.

**One integrator, two solvers.** Mass-spring and PBD run over the same graph and
the same `Integrate`; the only difference is where the constraint response
enters — as a Hookean force before integration, or as a position correction
after it. That is what makes the instability of the explicit solver at high
stiffness a controlled comparison rather than an anecdote.

**Constraint set.** Distance (`ClothDistanceConstraint`) carries structural,
shear and two-away bend springs; rest lengths always come from the initial
configuration. Projection is the XPBD form — the declared `Compliance` enters as
`alpha = Compliance / dt^2` — with a per-sweep stiffness from
`IterationStiffness(k, n) = 1 - (1-k)^(1/n)` (the PBD stiffness exponent), so an
iteration-count slider does not silently also change stiffness. Bending has both
readings: the grid builder uses two-away distance springs (one projection, not
four gradients, and the pattern is regular enough), and the mesh builder emits
real dihedral `ClothBendConstraint`s solved with the Müller 2007 four-gradient
projection, with rest angles read off the source mesh. Pinning is not a special
case: `SolverInverseMass` reports zero for a pinned vertex and every projection
divides by inverse mass already.

**Wind is a velocity field, not a force.** `WindModel` names a speed and a
direction and no coupling coefficient — because air pushes through *drag*, whose
coefficients belong to the particle. So `VelocityAt` gives the wind's velocity
(with gusts as a sinusoid travelling downwind), and the drag term is evaluated
on the velocity relative to it. Nothing is invented, and a scene with wind and
no drag correctly moves nothing.

**No collider interface.** Collision is three overloads of `ProjectOutOf` on
`Sphere`, `Plane` and `ISignedDistanceField3D` (the last with a
central-difference gradient, since the bare SDF interface reports no gradient).
An interface would earn its place only if colliders had to be stored
heterogeneously in one array; they share no state and no derived surface, each
body is two lines, and an interface-typed field would risk a viewless
existential (CHK308 is a hard zero). Contacts are composed after a step —
`cloth.Step(...).CollideWith(sphere).CollideWith(ground)` — which is also the
right order.

**Reading back.** `ClothMesh3D` pairs a `Cloth3D` with the `JaggedArray` face
table it was built from, because `Cloth3D` stores triangles and a polygon source
may have quads. The solver never touches the table, so `ToPolygonMesh` is a
position substitution and the existing mesh and normal paths render the result
with no new plumbing.

**Shared with plato-425.** `ParticleForces3D` (the six force arrays bundled
without particles) is declared in *this* track's `particles.types.plato`, and
`particles.library.plato` owns the particle-shaped force vocabulary
`AccelerationAt(force, position, velocity, time)`. Rigid dynamics' existing
`IForceModel2D/3D` is body-shaped (`ForceOn(x, body)`) and does not overlap.

**Deliberately out of scope.** Emission from `ParticleEmitter*` (needs a random
stream threaded through and returned from the step — its own state design);
self-collision (needs a broad phase); `Rope3D`, `SoftBodySettings` and the SPH
types (declared, still unstepped).

**Verification honesty.** No gate in this repo executes these bodies
(`plato-308`), so every behavioural claim — the sign of a bend gradient, the
winding of the grid quads — is backed by inspection against the cited sources,
not by a run.

## Done means

- [x] Particle state and a pure step function with a stated integrator
- [x] The declared `Particle*` force types actually apply
- [x] Cloth constraint graph built from a grid and from a `PolygonMesh3D`
- [x] A mass-spring solver and a PBD solver over the same constraint graph
- [x] Pinning, bending, and collision against at least a sphere and a plane
- [x] Simulated state rebuilt into a `PolygonMesh3D` with the original topology
- [x] All four tiers parse and type-check; no shipping-tier file references `future`
- [x] Design decisions recorded above
- [ ] A browser demo drives it: `demos/webgl/cloth.html` + `src/demos/cloth.ts`,
      green under `npm run typecheck` and `npm run scenes` (the latter steps every
      ticking scene and fails on a non-finite position, which is the check that
      matters for a solver)

## Follow-ups worth their own issue

- **A per-vertex constraint adjacency table for cloth.** It turns a Jacobi sweep
  and the mass-spring force gather from O(vertices x constraints) into
  O(constraints), and it is the prerequisite for graph-colored parallel
  projection. Today both are gathers that scan every constraint per vertex.
- **Particle emission.** `ParticleEmitter2D/3D` are declared and unused: spawning
  needs a random stream threaded through the step and returned from it, which is
  a state-design decision the whole `future` tier will want (SPH and the
  optimization types have the same need).
- **Cloth self-collision.** Needs a broad phase; `Cloth3D.Thickness` is already
  the shell radius it would use.
