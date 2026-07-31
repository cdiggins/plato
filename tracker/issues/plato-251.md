---
id: plato-251
title: Simple LLM demo written in Plato
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

A small language model (inference only — e.g. a GPT-2-class or tiny transformer with pre-trained
weights loaded from a file) implemented in Plato. Compelling as a demo because it exercises the
numeric stack end to end: tensor shapes, matmul, softmax, and SIMD throughput ([[plato-247]]).
Success is "it emits coherent text at a tolerable speed", not competitive performance.
