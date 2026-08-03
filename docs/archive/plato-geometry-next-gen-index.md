# Plato Geometry — Next Generation: Document Index

*Index of every document, discussion, and tracker item related to the next
generation of the Plato geometry library. Compiled 2026-07-26. Update this file
when new documents land.*

## 1. Where we are (assessment)

| Document | What it says |
|---|---|
| [reports/plato-library-review.md](../reports/plato-library-review.md) | Honest review of the current stdlib + `Ara3D.Geometry`. Verdict: architecture is right (pure value types, type-class interfaces, monomorphized output), but the library content does not yet live up to the README's pitch. The "what is wrong now" doc. |
| [reports/plato-codebase-assessment-2026-07-10.md](../reports/plato-codebase-assessment-2026-07-10.md) | Post-increment-3 compiler/codebase report with simplification ranking and options table. |
| [archive/plato-reassessment-2026-07-09.md](plato-reassessment-2026-07-09.md) | The July reassessment that reset priorities toward stdlib content + output performance. |
| [why-functional-programming-matters-in-geometry.md](why-functional-programming-matters-in-geometry.md) | Public-facing article on why geometry benefits from pure functions and immutable values, how Plato compiles those abstractions efficiently, and where the current performance boundary remains. |

## 2. What to build next (ideas & roadmaps)

| Document | What it says |
|---|---|
| [reports/plato-library-roadmap-ideas.md](../reports/plato-library-roadmap-ideas.md) | Companion brainstorm to the review. Two cross-cutting multipliers: scalar fields as universal modulators ("effectors"), plus a catalog of new-addition ideas grounded in the SDK examples (deformers, clones, sweeps, voxels, surface generators). The "what to build" doc. |
| [design/plato-kernel-libraries-sketch-2026-07-21.md](../design/plato-kernel-libraries-sketch-2026-07-21.md) | Design sketch for a family of small, pure Plato kernel libraries (motion, effects, style, layout) sitting beneath Gratify and any 2D/3D consumer, authored once and emitted to both TypeScript and C#. Tracker: plato-134 (parked); gate is the plato-076 motion spike. |
| [archive/plato-execution-plan-2026-07-09.md](plato-execution-plan-2026-07-09.md) | Execution plan following the reassessment. |

## 3. Type-system directions that shape the library

| Document | What it says |
|---|---|
| [affine-types-overview.md](../affine-types-overview.md) | Affine types (use-at-most-once values) in Plato — motivation and design. |
| [plato-pure-functional-programming.md](plato-pure-functional-programming.md) | Why Plato is built purely functional, written for working developers without a PL background. |
| [discussions/plato-uniqueness-types-july-7-2026-14h42.md](../discussions/plato-uniqueness-types-july-7-2026-14h42.md) | Discussion transcript on uniqueness types. |

A cleaned-up combined write-up also lives in the Plato repo:
`submodules/Plato/docs/Pure Functional Programming and Affine Types.md`.

## 4. Packaging, naming, and targets

| Document | What it says |
|---|---|
| [discussions/plato-geometry-july-7-2026-12h44.md](../discussions/plato-geometry-july-7-2026-12h44.md) | Discussion: should Plato geometry move to its own repo with multiple targets? `Plato.Geometry` vs `Ara3D.Geometry` naming? Where should the C# library live? |
| [discussions/plato-analysis-july-7-2026-14h18.md](../discussions/plato-analysis-july-7-2026-14h18.md) | Broader Plato positioning/analysis discussion. |
| [design/plato-rust-writer-plan.md](../design/plato-rust-writer-plan.md) | Plan for a Rust backend — part of the multi-target story. |
| [archive/plato-tir-scalar-lowering-plan-2026-07-12.md](plato-tir-scalar-lowering-plan-2026-07-12.md) | TIR endgame plan (TIR is the sole C# body writer). |

Shipped multi-target proof: the GLSL backend (`--glsl`, Plato.GlslWriter, 2026-07-19)
with the WebGL2-verified demo gallery in `submodules/Plato/demos/glsl`.

## 5. Tracker items

| Item | What it is |
|---|---|
| [ara3d-014](../tracker/issues/ara3d-014.md) | Double-precision version of the geometry library (p2, effort L). |
| [ara3d-017](../tracker/issues/ara3d-017.md) | Geometry library applied to TypeScript (idea, p3). |
| [plato-218](../../tracker/issues/plato-218.md) | Glyph-as-geometry library; stdlib gains distance-to-Bezier; text shaping stays outside the geometry library. |
| plato-134 | Kernel libraries (motion/effects/style/layout) — parked pending plato-076. |
| plato-076 | Motion spike — the gate for the kernel-library split. |
