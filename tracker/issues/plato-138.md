---
id: plato-138
title: random.plato: deterministic hash/noise stdlib library
type: idea
status: idea
priority: "?"
effort: "?"
risk: "?"
area: plato
sprint: 
created: 2026-07-21
closed:
links: [tracker/issues/plato-134.md, docs/design/plato-kernel-libraries-sketch-2026-07-21.md, submodules/Plato/plato-src/procedurals.plato]
---

## Idea

Deterministic, stateless random/noise library for plato-src: pure functions of
their inputs (`Hash(seed): Number`, `Hash2(x, y): Number`, `Jitter(i, seed):
Vector2`, value/gradient noise later). No RNG state — hash-of-inputs form fits
Plato's purity exactly and emits identically to C#/TS/GLSL. Consumers: particle
jitter and shake (plato-134 effects), scatter layouts, procedurals.plato
(currently has no noise source). Named the biggest true stdlib gap during the
plato-134 elaboration. Capture only — elaborate at promotion.
