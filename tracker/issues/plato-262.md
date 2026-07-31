---
id: plato-262
title: Plato C++/CUDA: dynamic Array + rich ToString
type: feature
status: done
priority: p1
effort: L
risk: med
area: plato
sprint: 
created: 2026-07-28
closed: 2026-07-28
links: [plato-239, submodules/Plato/Plato.CppWriter, submodules/Plato/Plato.CppWriter/README.md]
---

## Issue
Follow-on to closed [plato-239](plato-239.md): ship a device-friendly dynamic `Array<T>`
(concrete type; interface is `IArray`) and rich `ToString` for the C++/CUDA writer.
Binding constraints from plato-239 still hold — identical C++/CUDA bodies, no
`std::vector` / `std::string`, Plato-simple POD data types.

## Impact
Dynamic-array callees (`Range`, `MapRange`, `Map`/`Zip` on `IArray`, mesh builders,
`FieldNames`/`FieldValues`/`GetType`) dominate remaining stdlib skips after M3/M4.
Rich ToString unblocks debugging / logging without iostream.

## Affected code
- `submodules/Plato/Plato.CppWriter/CppPrelude.cs` — String POD; needs Array + arena/cap
- `submodules/Plato/Plato.CppWriter/CppWriter.cs` — Array ignored; ToString is TypeName-based
- `submodules/Plato/Plato.CppWriter/TirCppBodyWriter.cs` — array literals / Range lowering
- `submodules/Plato/Plato.CppWriter/README.md` — status / skip table

## Priority
p1: unblocks the largest remaining skip bucket and is the natural next slice after
plato-239 M3/M4.

## Dependencies
- Blocked by: none (plato-239 closed)
- Blocks: mesh-builder / FieldNames coverage once Array exists
- Touches: Plato.CppWriter only (do not disturb dialect identity)

## Fix approaches
1. **Fixed-cap growable Array** (like String) — `T data[CAP]; int count` — simplest, identical
   host/device, but CAP must be large enough for Range/mesh or we still skip big arrays.
2. **Bump / arena allocator in preamble** — `{T* data; int count}` + shared arena — truly
   dynamic within a frame; needs careful CUDA host/device story (static arena?).
3. **Hybrid** — fixed-cap for small N + skip oversized literals; ship Range/Map within cap.

**Chosen:** (1) fixed-cap POD (`PLATO_ARRAY_CAP` default 64), matching String. Arena deferred.

## Bedrock
Keep `Dialects_Differ_Only_In_The_Preamble` as the tripwire. Array and String stay
first-principles PODs — never `std::vector`/`std::string`. Verdict:
**simplest-along-the-grain** — do NOT introduce a host-only heap path that breaks
identical bodies.

## Done means
- [x] Device-friendly dynamic `Array<T>` POD in preamble (not `std::vector`); identical C++/CUDA bodies
- [x] Lower enough Array surface to unblock measurable skips (`Range` and/or Map/Zip / literals as feasible)
- [x] Rich `ToString` formats primitives/vectors/structs into the String buffer (not just TypeName)
- [x] `dotnet test submodules/Plato/Plato.CppWriter.Tests -c Release` green (8/8 or honest new gates)
- [x] README updated with representation choice + emitted/skipped delta
- [x] Remaining gaps documented (mesh builders, FieldNames, etc. if deferred)

## Simplest next slice
If full dynamic Array + mesh builders is too large: ship Array POD + Range + rich ToString
for primitives/structs that compiles; document remaining; tick Done-means honestly —
do not half-break the dialect identity test.

## Outcome (milestone landed)
- Representation: fixed-cap `Array<T> { T data[PLATO_ARRAY_CAP]; int count; }` (default 64)
- Stdlib: **1638→1716** emitted, **1367→1364** skipped; demos **147→153** / 0
- Preamble HOFs: Range, MapRange, Map, Zip×2/3, Reduce, All/Any, Reverse, FlatMap, Concat/Append/Prepend
- Remaining: Array2D/3D, FieldNames/GetType, WithNext, large-cap / arena for mesh builders
