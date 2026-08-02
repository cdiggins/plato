---
id: plato-381
title: "Concrete Dictionary type: decide whether keyed-lookup vocabulary earns its keep"
type: idea
status: idea
priority: "?"
effort: "?"
risk: "?"
area: plato
sprint: 
created: 2026-08-01
closed:
links: [stdlib/foundation/collections-containers.concepts.plato, stdlib/foundation/collections-containers.library.plato, tracker/issues/plato-379.md, tracker/issues/plato-325.md]
---

## Idea

Spin-off from [plato-379](plato-379.md). `MapLike<TKey, TValue>`
(`collections-containers.concepts.plato:30`) has **no implementer anywhere in the corpus** — it
is one of the 36 LINT013 findings, and [plato-325](plato-325.md) files it under "vocabulary
genuinely ahead of its types → retire", alongside `SetLike` / `StackLike` / `QueueLike`.

The question this issue holds: **is a concrete `Dictionary` type actually coming?** Everything
else about the keyed-lookup vocabulary is downstream of that answer.

- If yes — the interface surface is fine and merely waiting, `ContainsKey` has a real home, and
  plato-379's decision to defer `ContainsKey` to a `DictionaryLike` is the right shape.
- If no — the honest move is deletion, and plato-379 shrinks to a pure rename of `ValueAt` with
  nothing to unify against.

## Open questions

- What would back it? Every collection in this vocabulary is immutable-by-value; a hash map
  wants either a persistent structure or the affine builder discipline already used by
  `List<T>` / `Buffer<T>` (`primitives.plato:63`).
- Is there a real consumer in the geometry or graphics stdlib, or would this be vocabulary
  built for its own sake? `meshes-volumetric.library.plato:53` carries an inline comment citing
  the absence of `MapLike` implementers as the reason for a slower path — that is one concrete
  data point in favour.
- If a dictionary lands, does `ContainsKey` sit on the type, on a `DictionaryLike` interface, or
  derive from a `Keys` traversal?

## Relationship to other issues

- [plato-379](plato-379.md) — parameterises the lookup interface on its key; assumes a dictionary
  may arrive and leaves a slot for it.
- [plato-325](plato-325.md) — currently plans to retire `MapLike`. If this issue answers "yes,
  a dictionary is coming", that classification changes.

Answering this issue should settle the disposition in both.
