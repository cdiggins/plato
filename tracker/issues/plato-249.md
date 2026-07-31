---
id: plato-249
title: Physics simulation capabilities (rigid and soft body)
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

Physics simulation in Plato covering both rigid-body (collision detection + response, constraints)
and soft-body (mass-spring, position-based dynamics, cloth). Depends on efficient broad-phase
spatial queries ([[plato-248]]) and on integrators ([[plato-250]]). Note: `studio-008` tracks
*integrating* a physics engine into Studio; this is about Plato-native capability.
