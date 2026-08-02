---
id: plato-367
title: "`primitive` keyword: declare compiler-assumed types with `primitive` instead of `type`"
type: feature
status: done
priority: p2
effort: M
risk: low
area: plato
sprint:
created: 2026-07-30
closed: 2026-07-30
links: [plato-365, submodules/Plato/parakeet/Parakeet.Grammars/PlatoGrammar.cs, submodules/Plato/stdlib/foundation/primitives.types.plato, submodules/Plato/stdlib/foundation/primitives-arrays.types.plato]
---

## Task
Primitive types — those that must be defined outside the Plato runtime — should be
declared with a dedicated `primitive` keyword instead of `type`, so the language is
honest about which types the compiler assumes to exist by name.

The primitive set (per Christopher, 2026-07-30):
**Number, Integer, Boolean, Character, String, Array, Buffer, List, Function0–Function9,
Dynamic, Type.**

Scope for this increment (explicitly per user):
1. **Grammar** — accept `primitive` as a declaration keyword (parallel to `type`).
2. **Stdlib** — declare all of the above with `primitive` in a single file
   **`primitives.plato`** (consolidating/replacing the current declarations).
3. **Parses** — the stdlib parses cleanly after the change.

Explicitly OUT of scope for now: linters, gates, ratchets, and C# code generation.

## Current state (measured 2026-07-30)
- Grammar: `parakeet/Parakeet.Grammars/PlatoGrammar.cs:187` — `Type` rule is
  `UniqueKeyword.Optional() + Keyword("type") + ...`. CSTs are generated into
  `parakeet/Parakeet.Cst/PlatoGrammarCst*.cs` (and mirrored in `parakeet/output/`).
  Parakeet grammar is NOT frozen, but parakeet is its own submodule — push it manually.
- Stdlib declarations today:
  - `stdlib/foundation/primitives.types.plato` — Number, Integer, Boolean, String,
    Character, Dynamic, **Object** (Object is NOT on the primitive list; decide whether
    it stays a plain `type` or joins the list — surface the call, don't silently drop it).
  - `stdlib/foundation/primitives-arrays.types.plato` — Array<T>, Array2D<T>, Array3D<T>
    (only Array<T> is on the list; Array2D/3D presumably stay `type`).
  - **Buffer, List, Function0–9, Type are not declared anywhere in stdlib today** —
    they are compiler-assumed names (see `PlatoCompiler/Types/WriterPrimitiveNames.cs`).
    This task makes them visible as `primitive` declarations in `primitives.plato`.
- Compiler: parser/symbol path must accept the new node kind —
  `PlatoCompiler/Symbols/SymbolFactory.cs` builds type definitions from the CST. The
  minimal path is: `primitive` produces the same TypeDefinition as `type` (perhaps with
  an IsPrimitive flag), so everything downstream is untouched.

## Done means
- [x] `PlatoGrammar.cs` accepts `primitive Name<Params> implements ... { }` declarations;
      CST regenerated; parakeet pushed manually.
- [x] `stdlib/foundation/primitives.plato` exists and declares Number, Integer, Boolean,
      Character, String, Array, Buffer, List, Function0–Function9, Dynamic, Type with the
      `primitive` keyword; the superseded declarations are removed (no duplicate decls).
- [x] Whole stdlib parses cleanly (plato_check MCP or compiler parse stage — parse errors
      zero; downstream lint/ratchet noise expressly ignored this increment).
- [x] Decision recorded (in this file) for Object, Array2D/3D, and what interfaces (if any)
      the newly-declared Buffer/List/FunctionN/Type implement.

## Outcome (2026-07-30)

### Grammar (parakeet `817e636`)
```csharp
public Rule PrimitiveKeyword => Node(Keyword("primitive"));
public Rule Type => Node(UniqueKeyword.Optional() + (PrimitiveKeyword | Keyword("type")) + ...);
```
One new CST leaf (`CstPrimitiveKeyword`) and one new filter on `CstType`; `type` is
untouched, so every existing declaration parses exactly as before. `unique primitive
List<T>` works because `UniqueKeyword` still leads. Regenerated
`Parakeet.Cst/PlatoGrammarCst{,Factory}.cs` + `output/PlatoGrammar/grammar.txt` via
`dotnet test Parakeet.Tests --filter GenerateCstCode` / `~OutputDefinitions`.

### Compiler (Plato `571f077`)
Modelled exactly on the existing `unique` modifier, one hop per stage:
`CstType.PrimitiveKeyword.Present` -> `AstTypeDeclaration.IsPrimitiveDeclaration`
(`Plato.AST/Ast.cs`, `AstNodeFactory.cs`) -> `TypeDef.IsPrimitiveDeclaration`
(`PlatoCompiler/Symbols/Definitions.cs`, `SymbolFactory.cs`). **`Kind` stays
`TypeKind.ConcreteType`**, deliberately: a `primitive` declaration is byte-for-byte the
same TypeDef a `type` declaration produced, so resolution, checking and every writer are
untouched by this increment. Note the name — `TypeDef.IsPrimitiveDeclaration` is NOT the
pre-existing `TypeExtensions.IsPrimitive()`, which tests `Kind == TypeKind.Primitive`.
Nothing reads the new flag yet; it is the hook for plato-365.

### Decisions
- **Object stays `type`** (in `primitives.types.plato`, now the only declaration there).
  The compiler does not assume `Object` by name and it is not in
  `WriterPrimitiveNames.All` — it is an ordinary interop type whose declaration is its own
  authority.
- **Array2D/Array3D stay `type`** (in `primitives-arrays.types.plato`). Same reason: only
  rank-1 `Array<T>` is a compiler-assumed name.
- **Interface surfaces are carried over verbatim, not invented.** `Number: Real`;
  `Integer: Whole, Bitwise`; `Boolean: Value, Orderable, Logical`;
  `Character: Value, Orderable`; `String: Value, Orderable, Countable`;
  `Array<T>: Indexable<T>`. **`Dynamic`, `Type`, `List<T>`, `Buffer<T>` and
  `Function0–9` implement nothing** — `Dynamic`/`Type` are escape hatches with no interface
  surface, the builders are affine and reachable only through `intrinsics.library.plato`,
  and the function types are structural. Adding interfaces to any of them is a separate,
  deliberate design call, not a side effect of this rename.
- **`unique` is retained on List/Buffer** (`unique primitive List<T>`): `UniqueTypes` and
  LINT006/007 key off `IsUnique`, and dropping it would change behaviour.
- Two files became empty and were **deleted**: `primitives-builders.types.plato` (its long
  affine-builder doc block moved into `primitives.plato`) and
  `primitives-functions.types.plato`.

### Known, accepted deviations
- `primitives.plato` has **no `.types`/`.interfaces`/`.library` suffix** — per the task spec
  the primitive set is its own kind of declaration and gets its own name. Any file-suffix
  lint rule that objects is accepted noise.
- It holds **20 top-level declarations**, over the stdlib's "at most twelve per file"
  convention. Deliberate: the whole point is that the primitive set is visible in one
  place. Revisit if/when plato-365 shrinks the set.

### Verification
`dotnet run --project submodules/Plato/Plato.CLI -c Release -- lint
submodules/Plato/stdlib/{foundation,geometry,graphics,future} --strict` -> **exit 0**;
786 files, **0 parse errors**, **0 symbol resolution errors**, **0 lint Errors**,
ratchet **229 (unchanged)**. Info findings 2577 -> 2583 (LINT010, Info severity, excluded
from the ratchet). Out of scope and not run: C# codegen, the forward-conformance suite,
`check-all.ps1`. `Generated/` does not compile, but that is pre-existing plato-365a Angle
work from a concurrent session, unrelated to this change.

## Notes
- Related: [plato-365](plato-365.md) (retire non-scalar primitives / PrimitiveTypes =
  scalars only). This task is the surface-syntax half; keep the two consistent — the
  `primitive` keyword marks exactly the set the compiler must know by name, which is the
  list plato-365 wants to shrink.
- FunctionN declarations are generic: `primitive Function1<T0, TR>` etc. (arity 0–9;
  Function0 has only a return type parameter).
