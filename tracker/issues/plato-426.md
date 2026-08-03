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

_(fill in — integrator, solver, constraint set, what you rejected)_

## Done means

- [ ] Particle state and a pure step function with a stated integrator
- [ ] The declared `Particle*` force types actually apply
- [ ] Cloth constraint graph built from a grid and from a `PolygonMesh3D`
- [ ] A mass-spring solver and a PBD solver over the same constraint graph
- [ ] Pinning, bending, and collision against at least a sphere and a plane
- [ ] Simulated state rebuilt into a `PolygonMesh3D` with the original topology
- [ ] All four tiers parse and type-check; no shipping-tier file references `future`
- [ ] Design decisions recorded above
