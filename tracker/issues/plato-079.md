---
id: plato-079
title: Option/Result partiality cleanup of Plato stdlib
type: debt
status: idea
priority: "?"
effort: "?"
risk: "?"
area: plato
sprint: 
created: 2026-07-19
closed:
links: [submodules/Plato/plato-src, submodules/Plato/docs/plato-overview.md, tracker/issues/plato-076.md]
---

## Idea

Once sum types land ([[plato-077]] implemented), sweep the plato-src partiality
conventions: `CanInvert` + `Invert` pairs become `Invert: Option<Matrix>`,
ray-intersection misses become `Option<Hit>`, and the C#-side
`Tuple2<Matrix3x2, Boolean>` shapes retire. Update the conformance suite in the
same change. Pure library-quality work, no UI involvement — and a good first
real consumer to prove the sum-type feature on shipping code before anything
else (e.g. the Gratify kernel) depends on it.

**Hard-blocked on [[plato-077]]** — file-and-wait, not parallel work.

## Related

- [plato-077](plato-077.md) — blocking dependency (the language feature).
- [plato-076](plato-076.md) — spin-off origin.
