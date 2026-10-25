# Dynamics, collision, particles and cloth

Read [kinematics records](../../api/future/kinematics.types.md), [interfaces](../../api/future/kinematics.concepts.md), [kinematics bodies](../../api/future/kinematics.library.md), [rigid records](../../api/future/rigid-dynamics.types.md), [force interfaces](../../api/future/rigid-dynamics.concepts.md), [rigid bodies](../../api/future/rigid-dynamics.library.md), [collision types](../../api/future/collision.types.md) and [collision algorithms](../../api/future/collision.library.md).

## Motion and force have different meanings

Kinematic sampling supplies position and velocity at an elapsed `Duration`. Rigid dynamics adds mass, inertia, forces and impulses. The documented physical convention uses meters, kilograms, seconds and radians. The world frame stays Z-up even when a local example uses a differently directed acceleration.

**Naming conflict:** `AverageSpeedBetween` computes endpoint chord length divided by elapsed seconds. That is not path length divided by elapsed time for a curved trajectory: a full closed orbit can return zero. `AverageVelocityBetween` does use the correct endpoint displacement definition. Read the body rather than treating these names as equivalent physics definitions.

## Contacts and joints

The narrow-phase collision normal points from the first shape A toward the second B. Penetration is positive for overlap; zero is touching. Empty contact arrays mean no contact. The contact position uses the B-side surface convention. Swapping A and B therefore requires changing direction and interpreting the contact consistently.

The sequential-impulse solver threads an immutable world through rows and iterations. Later contacts read velocities changed by earlier contacts. Reordering or parallelizing those visits changes the numerical algorithm. Replacing array elements can be costly; this reference formulation is not a claim of a high-throughput mutable engine.

[Joint records](../../api/future/joints.types.md) and [joint bodies](../../api/future/joints.library.md) differ on static-world encoding: declarations allow `-1`, while solver bodies require valid distinct body indices and an explicit zero-inverse-mass world body. Follow the solver precondition before indexing. Distance and ball/socket constraints have bodies; hinge, slider, orientation limits, motors and other declared families remain unsolved there.

## Particles and cloth

[Particle records](../../api/future/particles.types.md) and [bodies](../../api/future/particles.library.md) combine acceleration fields; those fields do not divide a force by each particle's mass. Drag uses velocity relative to wind. [Cloth records](../../api/future/cloth.types.md) and [bodies](../../api/future/cloth.library.md) use position-Verlet state with distance/bend constraints and either positional projection or spring forces. Step size, pinning, degeneracy and solver iteration assumptions remain essential. Neither stability nor energy conservation follows from pure functional formulation.

## Related work

[Box2D](https://box2d.org/documentation/md_simulation.html) provides a concrete contrast for worlds, contacts and joints. Plato shares sequential constraint-solving ideas but returns new values and has a narrower implemented joint set; do not assume Box2D's runtime, collision coverage or tuning behavior transfers.
