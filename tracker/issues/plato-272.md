---
id: plato-272
title: Add Constants library to stdlib and use it
type: feature
status: done
priority: p1
effort: M
risk: low
area: plato
sprint: 
created: 2026-07-28
closed: 2026-07-28
links: [plato-230, docs/plato-v3-vocabulary-report.md, submodules/Plato/stdlib-legacy/constants.plato, docs/plato-execution-plan-2026-07-09.md, submodules/Plato/stdlib]
---

## Idea
Ship a `library Constants` (and color constants as needed) in the forward stdlib (`stdlib`), then rewrite call sites that currently hardcode `3.14159…`, `1e-7`, unit axes, unit intervals, etc. to use named zero-arg constants. Port the good surface from shipping `stdlib-legacy/constants.plato` / `colors.constants.plato`; adapt names to v3 vocabulary (`Vector2D` not `Vector2`, `Angle` where angles belong).

## Assumptions
- v3 may host `library` bodies (already true for `transforms.plato` and the `*.library.plato` files in `stdlib`); constants are pure zero-arg functions, so they fit.
- Identity transforms stay on types via `_: Type` statics (`Quaternion.Identity`) — Constants owns scalar/math/axis/unit-shape values, not every identity.
- Shipping A.2 (execution plan) remains a separate burn-down for `plato-src` correctness; this item is the v3 surface + adoption.

## Design decisions
- **Where the file lives** — a `library` adjacent to `numbers.plato` vs a dedicated `constants.plato` vs a `*.library.plato`-style sibling. Prefer a dedicated file near foundation (e.g. after numbers) so discoverability matches shipping `constants.plato`.
- **Angles** — expose `Pi` as `Number` vs `HalfTurn`/`FullTurn` as `Angle` (or both). Prefer both: numeric irrationals as `Number`, geometric half/full turns as `Angle`.
- **Axes** — **superseded by plato-288 (2026-07-28):** do NOT introduce an independent `XAxis3D(): Vector3D` spelling. The canonical unit axes now derive from the `Axis3D`/`Axis2D`/`SignedAxis3D` sum types in `stdlib/axes.plato` — unit X is `Axis3D.X.Vector3D` (or `.Direction3D` for the unit direction), so "unit X" has exactly one source. Constants should reference/derive from the axes surface rather than restating the basis vectors. (Note: `stdlib/constants.library.plato` still carries `UnitX/UnitY/UnitZ(_: Vector3D)` from an earlier pass; fold those onto `Axis3D.X.Vector3D` when this issue is executed so there is one canonical spelling.)
- **Color constants** — same PR vs follow-up. Split: math/geometry constants first; colors second (large surface).

## Related
- [plato-230](plato-230.md) — created declaration-only v3; constants deferred.
- [docs/plato-v3-vocabulary-report.md](../../docs/plato-v3-vocabulary-report.md) — explicitly flags "No constants."
- [stdlib-legacy/constants.plato](../../submodules/Plato/stdlib-legacy/constants.plato) — port source (Pi, Epsilon, axes, UnitInterval, …).
- [docs/plato-execution-plan-2026-07-09.md](../../docs/plato-execution-plan-2026-07-09.md) A.2 — shipping-library constant *bugs*; do not conflate.
- [transforms.plato](../../submodules/Plato/stdlib/transforms.plato) — already has `Identity` statics; Constants must not duplicate those.

## Approaches
Short term: add `library Constants` with the shipping math set (Zero/One/Epsilon/Pi/Sqrt*/Ln*/axes/UnitInterval), lint green, replace literal Pi/Epsilon in `transforms.plato` and the concept libraries where obvious.
Long term: `colors.constants`, measure conversion factors, named frames/gizmo axes as Constants or domain libraries.
Adjacent: file numbering / type-vs-library split for transforms (separate design questions).

## Bedrock
Strengthens the **stdlib discoverability seam**: one place for named numeric/geometric literals, so algorithms stop embedding magic numbers and backends can specialize constant folding. **Verdict: simplest-along-the-grain** — must NOT invent new language `const` syntax or move Identity off type statics; just a library of zero-arg functions used at call sites.

## Done means
- [ ] `library Constants` exists under `stdlib` with at least: Zero, One, Epsilon, Pi, TwoPi/HalfPi (or Angle equivalents), SqrtTwo, GoldenRatio, 2D/3D unit axes, UnitInterval
- [ ] `lint stdlib` still 0 parse / 0 symbol errors
- [ ] At least one existing library body (`transforms` or a concept library) uses Constants instead of raw literals for Pi or Epsilon (or documents why none apply yet)
- [ ] README / file-map notes the Constants file

## Simplest possible implementation
Copy shipping `constants.plato`, rename types to v3 (`Vector2D`/`Point2D`/`NumberInterval`), drop anything that doesn't resolve, add the file, lint.
- Pros: fast, proven surface, unblocks lessons and bodies.
- Cons: may carry unused measure factors; angle-as-Number from v1 needs a deliberate Angle decision before wide adoption.
