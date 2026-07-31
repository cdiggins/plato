---
id: plato-241
title: Rename Vector2/Vector3/Vector4 to Vector2D/Vector3D/Vector4D (etc.)
type: idea
status: idea
priority: "?"
effort: "?"
risk: "?"
area: plato
sprint: 
created: 2026-07-27
closed:
links: []
---

User idea, captured 2026-07-27.

Rename the geometric vector types to carry an explicit dimension suffix: `Vector2` -> `Vector2D`,
`Vector3` -> `Vector3D`, `Vector4` -> `Vector4D`, and the same treatment for related families
(points, matrices, bounds). Motivation: "2D" reads as the dimension of the space, and frees the
plain `VectorN` names for the literal SIMD-width numeric vectors (see [[plato-247]]).

Breaking change across the generated surface — needs a coordinated stdlib rename + regen of
`ara3d-sdk/src/Plato.Generated/` + API baseline refresh.
