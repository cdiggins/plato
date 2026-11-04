# Dynamics, collision, particles and cloth

This vocabulary is in the `future` tier, which is not part of the default library. Kinematic sampling returns position and velocity at an elapsed `Duration`. Rigid dynamics adds mass, inertia, forces, and impulses. The documented physical convention uses meters, kilograms, seconds, and radians. The world frame stays Z-up even when a local example uses acceleration in another direction.

## Average speed is a chord

`AverageSpeedBetween` returns the chord between the endpoints, divided by elapsed seconds. That is not path length over time. A full closed orbit can return zero. `AverageVelocityBetween` returns endpoint displacement over elapsed seconds, which is the usual definition of average velocity. Read the body before treating the two names as the same physics.

## Contacts and joints

The narrow-phase collision normal points from the first shape, A, toward the second shape, B. Penetration is positive for overlap, and zero means touching. An empty contact array means no contact. The contact position uses the B-side surface. Swapping A and B changes the direction, and the contact has to be read the same way.

The sequential-impulse solver threads an immutable world through rows and iterations. A later contact reads velocities that earlier contacts already changed. Reordering or parallelizing those visits changes the numerical algorithm. Replacing array elements can be costly. This formulation is not a high-throughput mutable engine.

Joint declarations allow `-1` for a static world. The solver bodies require valid, distinct body indices and an explicit world body whose inverse mass is zero. Follow the solver precondition before indexing. Distance constraints and ball-and-socket constraints have bodies. Hinge, slider, orientation limits, motors, and the other declared families are unsolved there.

## Particles and cloth

Particle bodies combine acceleration fields. Those fields do not divide a force by each particle's mass. Drag uses velocity relative to the wind. Cloth bodies use position-Verlet state, distance and bend constraints, and either positional projection or spring forces. Step size, pinning, degeneracy, and the solver's iteration assumptions stay part of the call. Stability and energy conservation do not follow from the functions being pure.

## Where to look

- [Kinematics records](../../api/future/kinematics.types.md), [interfaces](../../api/future/kinematics.concepts.md), and [bodies](../../api/future/kinematics.library.md)
- [Rigid records](../../api/future/rigid-dynamics.types.md), [force interfaces](../../api/future/rigid-dynamics.concepts.md), and [rigid bodies](../../api/future/rigid-dynamics.library.md)
- [Collision types](../../api/future/collision.types.md) and [collision algorithms](../../api/future/collision.library.md)
- [Joint records](../../api/future/joints.types.md) and [joint bodies](../../api/future/joints.library.md)
- [Particle records](../../api/future/particles.types.md) and [bodies](../../api/future/particles.library.md)
- [Cloth records](../../api/future/cloth.types.md) and [bodies](../../api/future/cloth.library.md)
- [Aspirational library domains](domains.md)

[Box2D](https://box2d.org/documentation/md_simulation.html) documents worlds, contacts, and joints for a mutable engine. Sequential constraint solving is the shared idea. Box2D's runtime, collision coverage, and tuning do not transfer, and the implemented joint set here is the distance and ball-and-socket bodies.
