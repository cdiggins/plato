---
id: plato-380
title: Integer2/Integer3 key types: re-express Indexable2D/3D as key instances
type: idea
status: idea
priority: "?"
effort: "?"
risk: "?"
area: plato
sprint: 
created: 2026-08-01
closed:
links: [stdlib/foundation/collections-indexable.concepts.plato, stdlib/foundation/primitives-arrays.types.plato, tracker/issues/plato-379.md]
---

## Idea

Spin-off from [plato-379](plato-379.md), deliberately kept separate. If the lookup interface
becomes parameterised on its key, then `Indexable2D<T>` and `Indexable3D<T>` are just that
interface at `Integer2` and `Integer3` keys, rather than separate interfaces carrying their own
`ColumnCount` / `RowCount` / `LayerCount` plus a multi-argument `At` overload.

Today (`collections-indexable.concepts.plato:32,41`) both inherit `Indexable<T>` for flattened
row-major access and add `At(x, column, row)` / `At(x, column, row, layer)` on top. `Array2D<T>`
(`primitives-arrays.types.plato:28`) implements `Indexable2D<T>` over a flat `Elements: Array<T>`
plus explicit counts, so the flattening is already explicit in the one concrete implementer.

Requires introducing `Integer2` / `Integer3` key types, which do not exist in the corpus today.

## Why it is not part of plato-379

The multi-argument `At` overloads are what the compiler's indexing syntax maps to for
`xs[col, row]`. Changing them entangles a pure vocabulary refactor with the indexing-syntax
mapping and the writer's `At` synthesis. plato-379 explicitly forbids folding this in; its
recommendation is to re-point `Indexable2D`/`3D` at the Integer-keyed interface and change nothing
else.

## Open questions

- Does `xs[col, row]` still work if the key is a single `Integer2`, or does the compiler need a
  tupling rule? This gates the whole idea.
- Do `ColumnCount` / `RowCount` survive as interface members, or become properties of the key
  space? Row-major flattening needs them either way.
- Is `Integer2` a new primitive, or a `type` over two `Integer` fields? Vectors already have a
  fixed-size story worth mirroring.

## Depends on

[plato-379](plato-379.md) landing first — without a key-parameterised interface there is nothing
for `Integer2` to be a key of.
