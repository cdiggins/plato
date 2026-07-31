---
id: plato-271
title: Decide Plato numeric precision and fixed-size types
type: idea
status: idea
priority: "?"
effort: "?"
risk: "?"
area: plato
sprint: 
created: 2026-07-28
closed:
links: [ara3d-014, docs/plato-execution-plan-2026-07-09.md, plato-239, plato-261]
---

## Idea

Make an explicit product/language decision on **numeric precision** for the Plato stdlib (float32 vs float64 vs dual / scalar-polymorphic) and on whether the library should expose **fixed-size numeric/vector types** as first-class (e.g. `float2`/`float4`-style or fixed `Array` tiers) vs keeping a single abstract `Number` + length-generic vectors. Related C# work already filed as [ara3d-014](ara3d-014.md) (double-precision geometry library); this issue is the Plato-side policy that should lead or constrain that.

## Assumptions

- Today Plato/`Number` maps primarily to single-precision in shipping codegen; semantics docs note doubles as non-feature in places ([plato-261](plato-261.md)).
- GPU/GLSL paths want float32 and fixed-width vectors; CAD/BIM analysis often wants float64.
- Fixed-size arrays already appear in C++/CUDA writer work ([plato-239](plato-239.md) Components / floatN).
- Scalar polymorphism ([plato-252](plato-252.md) territory) may dissolve "pick one precision" if it lands.

## Design decisions

- **Precision model** — single default (`float` or `double`) vs `--scalar=` dual codegen vs `Real`/`Float`/`Double` types in source.
- **Default for stdlib** — what `Number` means in docs and in default emit.
- **Fixed-size vectors** — only as codegen lowering of `Vector2`… vs source-level `Float32x4` / SIMD-facing types.
- **Fixed-size arrays** — library types (`Array2`…`ArrayN`) vs only backend representation.
- **Cross-precision** — whether differential conformance (float vs double) is required ([docs/plato-execution-plan-2026-07-09.md](../../docs/plato-execution-plan-2026-07-09.md) already sketched this).

## Related

- [ara3d-014](ara3d-014.md) — double-precision *Ara3D.Geometry* feature idea (enrich/align after this ADR).
- [plato-239](plato-239.md) — fixed-size Components in C++ emit.
- [plato-252](plato-252.md) — scalar polymorphism / `Real` (if pursued, changes this decision).
- [plato-261](plato-261.md) — current "no doubles" language-doc stance to update if reversed.
- Execution-plan notes on `Plato.Geometry.Double` / cross-precision differentials.

## Approaches

Short term: ADR answering (1) what `Number` is, (2) whether dual emit exists, (3) fixed-size story for vectors/arrays.
Long term: implement the chosen model (codegen flags, stdlib types, tests).
Adjacent: document GPU vs CAD precision profiles as "targets," not library forks.

## Case against

- **Scalar polymorphism first.** Picking float XOR double hard-codes a fork that generics over `Real` would avoid ([plato-252](plato-252.md)).
- **Status quo works.** Studio ships; reopening precision without a concrete bug is speculative.
- **Fixed-size types clutter.** Exploding `Vector2f`/`Vector2d`/`Float4` mirrors C++ pain Plato was meant to escape.
- **ara3d-014 may be enough** if only C# consumers need double and Plato stays float.

**Verdict: pursue** as an **ADR before** implementing ara3d-014 or dual stdlibs. Prefer a decision that keeps source polymorphic or single-`Number` with emit-time scalar, and treats fixed-size as a **backend/packing** concern unless a concrete SIMD/GPU API needs source types. Park implementation until the ADR exists.

## Bedrock

Strengthens the **`Number` / scalar emit contract** between stdlib source and writers (C#/GLSL/C++) — one policy instead of per-backend accidents. **Verdict: simplest-along-the-grain** — write the ADR; must NOT fork the stdlib into `*.float` / `*.double` trees in the decision change itself.

## Done means

- [ ] ADR: precision model + what `Number` means + fixed-size policy
- [ ] ara3d-014 and codegen plans updated to match (or explicitly diverge with reason)
- [ ] Language/agent docs reflect the decision

## Simplest possible implementation

One ADR page with the three decisions and rejected alternatives; link from plato-261 / ara3d-014; no code until approved.

Pros: cheap; prevents divergent float/double forks  
Cons: doesn't ship capability; may need revisit when scalar polymorphism lands
