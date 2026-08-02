---
id: plato-304
title: Port Ara3D.Collections IArray capabilities into Plato (rename and improve)
type: idea
status: idea
priority: "?"
effort: "?"
risk: "?"
area: plato
sprint: 
created: 2026-07-29
closed:
links: [ara3d-sdk/src/Ara3D.Collections/LinqArray.cs, ara3d-sdk/src/Ara3D.Collections/ReadOnlyList.cs, ara3d-sdk/src/Ara3D.Collections/IntegerRange.cs, ara3d-sdk/src/Ara3D.Collections/CompressedSparseRow.cs, submodules/Plato/stdlib/intrinsics-arrays.library.plato, submodules/Plato/stdlib/collections-indexable.concepts.plato, submodules/Plato/stdlib/collections-indexable.library.plato, submodules/Plato/stdlib/collections-containers.library.plato, submodules/Plato/stdlib/STYLE_GUIDE.md, tracker/issues/plato-303.md, tracker/issues/plato-295.md]
---

## Idea

Port the useful surface of Ara3D’s historical **IArray** toolkit — today primarily `LinqArray` extensions over `IReadOnlyList<T>` plus `ReadOnlyList` / `IntegerRange` (comments still say “IArray”) — into the **forward Plato stdlib**, renamed and improved for Plato: interface-first (`Indexable` / `Array`), small pure functions, Plato naming (`Map` not `Select`, `FlatMap` not `SelectMany`, `Concatenate` not `Concat`), and no LINQ/`IEnumerable` leakage. Much of the core map/reduce/slice family already exists as **intrinsics** (`intrinsics-arrays.library.plato`) and thin interface libraries (`collections-indexable.library.plato`); this issue is the deliberate gap analysis + fill of the *rest* of the C# library that geometry/polyhedra/CSR work still wants (pairs/triplets, scans/prefix sums, indices-where, strides, zip-with-next, counts↔offsets, etc.).

## Assumptions

- Plato’s noun is `Array` / interface `Indexable<$T>`, not a revived `IArray` type name — rename on port unless an ADR says otherwise.
- Host intrinsics stay the implementation home for anything that needs allocation/backends; derived libraries add Plato-expressible bodies on top (STYLE_GUIDE / LIBRARIES.md).
- Lazy `ReadOnlyList(count, f)` functional arrays in C# may map to `MapRange` or stay intrinsic; Plato need not preserve C# laziness semantics.
- Impure or host-only APIs (`ToArrayInParallel`, `AsSpan`, `AddTo` mutating collections, `BinarySearch` on unsorted assumptions) are drop or park — not automatic ports.
- [plato-303](plato-303.md) owns CSR as a type/interface; this issue may supply `CountsToOffsets` / pack helpers that CSR consumes, without duplicating CSR itself.

## Design decisions

- **Inventory first** — spreadsheet/gap table: LinqArray member → Plato intrinsic / interface library / drop / rename. Do not bulk-port the ~100 public methods.
- **Where bodies live** — extend `collections-indexable.library.plato` / new `collections-arrays.library.plato` vs grow `IntrinsicsArrays`. Prefer derived libraries when expressible; intrinsics only when every backend must supply it.
- **Naming map** — document Select→Map, SelectMany→FlatMap, Where→Filter (or KeepIf), Aggregate→Reduce, Concat→Concatenate, ElementAt→At, InRange→IsValidIndex (already), etc. Prefer existing Plato names when present.
- **Eager vs lazy** — Plato arrays are values; C# `Select` often returns lazy `ReadOnlyList`. Port as eager `Map`/`MapRange` unless an explicit lazy view type is justified.
- **Numeric reductions** — `Sum`/`Min`/`Max`/`PrefixSums` on `Indexable<Number>` vs leave in statistics libraries. Prefer Number-constrained helpers, not untyped Aggregate clones.

## Related

- [LinqArray.cs](../../ara3d-sdk/src/Ara3D.Collections/LinqArray.cs) — main C# capability surface (comments: IArray).
- [ReadOnlyList.cs](../../ara3d-sdk/src/Ara3D.Collections/ReadOnlyList.cs) / [IntegerRange.cs](../../ara3d-sdk/src/Ara3D.Collections/IntegerRange.cs) — functional range / indexed views.
- [CompressedSparseRow.cs](../../ara3d-sdk/src/Ara3D.Collections/CompressedSparseRow.cs) — C# CSR; pairs with [plato-303](plato-303.md).
- [intrinsics-arrays.library.plato](../../submodules/Plato/stdlib/intrinsics-arrays.library.plato) — Map/FlatMap/Zip/Slice/MapRange/… already declared.
- [collections-indexable.*](../../submodules/Plato/stdlib/collections-indexable.concepts.plato) — Countable/Indexable + IsEmpty/First/Map/Reduce/All/Any.
- [STYLE_GUIDE.md](../../submodules/Plato/stdlib/STYLE_GUIDE.md) — literals vs MapRange; small functions.
- [plato-295](plato-295.md) — fixed-arity literals (related naming/style, not a substitute for this port).
- [plato-303](plato-303.md) — CSR abstraction; consumer of counts/offsets helpers.

## Approaches

Short term: (1) gap inventory against LinqArray; (2) port high-value missing ops as small Plato functions — `MapPairs`/`ZipEachWithNext`, `IndicesWhere`, `Stride`, `Scan`/`PrefixSums`, `CountsToOffsets`/`OffsetsToCounts`, `RepeatElements`; (3) rename table in STYLE_GUIDE or LIBRARIES.

Long term: 2D/3D list helpers aligned with `ReadOnlyList2D`/`3D`; retire duplicate C# usage in new geometry as Plato codegen catches up.

Adjacent: Filter/KeepIf consistency; lazy array view type (probably park); full LINQ parity (drop).

## Case against

- **Most of it is already there.** Intrinsics + Indexable cover the daily path; a “port IArray” project invites dumping the whole LinqArray kitchen sink into stdlib.
- **Backend cost.** Every new intrinsic multiplies C#/C++/TS/CUDA shims; derived-only ports are safer but slower.
- **Name churn.** Reviving `IArray` as an interface alias splits vocabulary again (legacy already used `IArray`).

**Verdict: pursue** as a **curated gap fill + rename guide**, not a wholesale LinqArray dump. Park parallel/span/mutation APIs. Prefer Plato names (`Indexable`/`Array`) over resurrecting `IArray`.

## Bedrock

Strengthens the **indexable-array programming model** shared by geometry, CSR, and polyhedra: one documented map of capabilities from the battle-tested C# library into Plato intrinsics/interface libraries, with small composable functions instead of LINQ-shaped megamethods. **Verdict: simplest-along-the-grain** — inventory + port the missing high-value pure ops into existing library files; must NOT add `IArray` as a second name for `Indexable`, must NOT port impure/parallel APIs, must NOT block [plato-303](plato-303.md)/[plato-301](plato-301.md) on full parity.

## Done means

- [ ] Written gap inventory: LinqArray (and closely related ReadOnlyList/IntegerRange APIs) → present / port / rename / drop
- [ ] Rename conventions recorded (Select/Map, etc.) in STYLE_GUIDE or adjacent stdlib doc
- [ ] Priority missing pure ops landed in forward stdlib (intrinsics and/or collections libraries) with lint 0 parse / 0 resolution
- [ ] Explicit drop list for impure/host-only members
- [ ] Cross-links from [plato-303](plato-303.md) where counts↔offsets / pack helpers overlap

## Simplest possible implementation

Produce the inventory markdown under `.temp/` or a short section in the issue; port only `ZipEachWithNext` / `IndicesWhere` / `CountsToOffsets` (three small functions) as a proof that the pipeline is “gap → Plato name → library”; expand in follow-ups.

Pros: low risk; immediately useful for mesh/CSR; forces rename discipline.
Cons: incomplete vs full LinqArray; inventory work is upfront cost.
