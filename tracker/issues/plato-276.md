---
id: plato-276
title: "Rust/TypeScript writers: At/Count synthesis ignores single-collection-field types"
type: bug
status: idea
priority: p2
effort: S
risk: low
area: plato
sprint: 
created: 2026-07-28
closed:
links: [Plato.RustWriter/RustTypeWriter.cs:219, Plato.TypeScriptWriter/TypeScriptConcreteTypeWriter.cs:346, Plato.CSharpWriter/CSharpTypeWriter.cs]
---

Port the C# writer's At/Count fix (commit `9bacd3f`, branch `claude/elated-bardeen-56e9b8`)
to the Rust and TypeScript writers.

## Problem

All three writers synthesize the `At`/`Count` interface obligations from a type's FIELDS.
Correct for fixed-arity types (`Point3D`: X/Y/Z, fields ARE the components); wrong for
runtime-arity types (`VectorN`: one `Array<Number>` field, where the FIELD is the component
surface). Enumerating fields there gives `Count == 1` and `At(0) == the whole collection`.

`Plato.RustWriter/RustTypeWriter.cs:219` has no collection case at all:

```csharp
if (f.Name == "At") { ... for (var i = 0; i < fs.Count; i++) s += $"if {p} == {i} {{ self.{fs[i]} }} else "; ... }
if (f.Name == "Count") return WriteLine($"pub fn Count(self) -> i64 {{ {fs.Count} }}");
```

`Plato.TypeScriptWriter/TypeScriptConcreteTypeWriter.cs:346` calls the same shape of helper.

Neither is reported by anything: `At`/`Count` are exempt from LINT001 via
`Linter.MembersImplementedByWriter`, so the type lints perfectly clean.

## Fix

Mirror `CSharpTypeWriter.SingleIndexableFieldName`: delegate when the type has exactly one
field whose OWN type provides both a linear `At` and `Count` (via `GetSelfAndAllInheritedTypes`
+ `GetAllImplementedConcepts`). Key off the concept, not the type NAME — the library spells
these fields both `Array<T>` (stdlib, implements `Indexable<T>`) and `IArray<T>` (stdlib-legacy).
A name-based check is what was wrong in the C# writer before `9bacd3f`. Requiring both members
also correctly excludes `Array2D`/`Array3D` (they implement `Indexable2D`/`3D`, no linear Count).

Consider hoisting the helper to a shared location so a fourth backend cannot re-acquire the bug.
GLSL/C++/CUDA writers not yet checked for the same shape.

## Verify

`PlatoTests/CollectionFieldAtCountTests.cs` is the C# model: builds a self-contained corpus
(fixed-arity control + `Array`-typed field + `IArray`-typed field) and runs the writer in-proc.
Add the Rust/TS equivalents alongside `RustEmitFlagOnTests` / `TypeScriptEmitFlagOnTests`.

## Notes

Latent, not live — no type in the shipping generation reaches this synthesis, so no output
should change. Both are POC backends. An explicit `At`/`Count` library body always wins over
synthesis (lands in `ConcreteType.ImplementedFunctions`), so declaring bodies stays the escape
hatch for shapes neither rule covers.
