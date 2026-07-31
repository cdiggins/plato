---
id: plato-078
title: Revive and productionize the Plato TypeScript writer
type: feature
status: idea
priority: "?"
effort: "?"
risk: "?"
area: plato
sprint: 
created: 2026-07-19
closed:
links: [submodules/Plato/Plato.TypeScriptWriter, submodules/Plato/CLAUDE.md, tracker/issues/plato-076.md, labs/platoflow]
---

## Idea

Promote `Plato.TypeScriptWriter/` from "exists, out of scope" (Plato CLAUDE.md)
to a supported target: conformance-style test suite for TS output, CLI flag
hardening, CI gate, and fixing any rot accumulated since the TIR became the
sole body writer (C4). A TIR body writer for TS already exists
(`TirTypeScriptBodyWriter.cs`, ~320 lines), so this is revival, not greenfield.

Two consumers exist without Gratify: PlatoFlow (labs Plato⇄graph demo, TS side)
and the founding one-source-many-targets pitch (overview names C# and TS as the
primary targets; only C# is real today). Also de-risks the [[plato-076]]
motion.plato spike — if the writer is badly rotted, better to learn that in a
dedicated effort than mid-spike.

## Related

- [plato-076](plato-076.md) — spin-off origin; the kernel-under-both-ports play
  requires TS emission.
- [labs/platoflow] — existing TS-side consumer.
