# Plato — agent guide

Plato is a small, pure, statically typed language for geometry and numeric libraries. In the [Ara 3D studio](https://github.com/ara3d/studio) monorepo it lives at `submodules/Plato/`. Write algorithms once in `.plato` files; the compiler emits idiomatic libraries for multiple targets.

**Why agents care:** the whole language plus its standard-library vocabulary fits in one context
window — the compressed index is [`stdlib/types-and-concepts.txt`](../stdlib/types-and-concepts.txt),
one line per declaration. One declaration fans out into many generated members across types and
targets, so global consistency is a single edit, not a multi-file refactor.

---

## Language

**Normative reference: [`SEMANTICS.md`](SEMANTICS.md)** — what every
construct means, resolution/coercion rules, and the explicit non-features. The 5-line version:

- **`type`** — immutable data: fields only, or a sum type (`type X = A(f: T) | B;`) consumed by exhaustive `match`.
- **`primitive`** — a type the compiler assumes **by name**, with no declarable shape; the whole set is `stdlib/foundation/primitives.plato`. Only these may appear in an intrinsic signature.
- **`interface`** (alias `concept`) — type classes with a `Self` type; constrained generics, not OO dispatch; monomorphized. Names are `I*`.
- **`library`** — pure free functions; first argument is the receiver (`v.Length`, `a.Lerp(b, t)`); nullary calls need no parens. A bodiless signature here is an **intrinsic**: the host runtime supplies it ([`plato-intrinsics-surface.md`](plato-intrinsics-surface.md)).
- Conveniences: tuples construct types (`(x, y)` → `Point2D`), type-named functions are implicit conversions, operators come from well-known names (`Add` → `+`), `_: Type` first params are statics.
- Restrictions are intentional: no mutation, no `this`, no I/O, no exceptions, no null.

Example (from the stdlib):

```plato
type Circle { Center: Point2D; Radius: Number; }

library AngularCurves2D {
    Eval(curve: Circle, t: Angle): Point2D
        => t.Circle(curve.Center, curve.Radius);
}
```

---

## Source layout

Paths below are relative to this repo's root (also checked out as `submodules/Plato/` in studio).

**Stdlib mapping:** `stdlib` = forward vocabulary, and the only library this repo generates C# from;
`legacy/stdlib-legacy` = the older generation whose emitted C# now lives, checked in and frozen, in
the `ara3d-sdk` repo. Do not confuse the two.

In `stdlib/`, one file holds exactly one **kind** of declaration, with no cap on how many:
`<stem>.plato` = types, `<stem>.concepts.plato` = interface declarations (filename stem kept),
`<stem>.library.plato` = exactly one `library` block. Files sit directly in the tier folders
(`foundation`, `geometry`, `graphics`, `future`) described by [`stdlib/README.md`](../stdlib/README.md).

| Path | Purpose |
|------|---------|
| `stdlib/` | Forward stdlib vocabulary — types, interfaces, **and** library bodies. New *vocabulary* goes here. Read its [`README`](../stdlib/README.md), [`CONVENTIONS`](../stdlib/CONVENTIONS.md), [`STYLE_GUIDE`](../stdlib/STYLE_GUIDE.md), and [`LIBRARIES`](../stdlib/LIBRARIES.md) before editing. |
| `stdlib/tests/` | Forward law packet (`Law_*`) for `stdlib/`. Inside the folder, but **not a tier** — the library's gates name the tiers, so they never see it. |
| `legacy/stdlib-legacy/*.plato` | Shipping standard library. Writable; no longer a codegen source in this repo. |
| `demos/plato-src/geometry.plato` | Curated demo subset for TS/Rust browsers (not the full stdlib). |
| `src/Plato.CLI/` | Compiler entry point. |
| `src/Plato.ContextExport/` | Compact export of types + interfaces for agent context (`tools/export-types-context.bat`). |
| `stdlib/types-and-concepts.txt` | Generated index of every type + interface in the shipping `stdlib/` tiers (`future` excluded), one compressed declaration per line, every interface first and then every type, each group sorted by name. Regeneration is mandatory when `stdlib/` changes — see [`stdlib/AGENTS.md`](../stdlib/AGENTS.md). |
| `docs/types-and-concepts-context.txt` | Generated stdlib-legacy context (types + interfaces only); same regen script. |
| `writers/Plato.TypeScriptWriter/` | TypeScript backend (POC). |
| `writers/Plato.RustWriter/` | Rust backend (POC). |
| `writers/Plato.GlslWriter/` | GLSL ES 3.00 / WebGL2 backend (POC). |
| `writers/Plato.CppWriter/` | C++17 / CUDA backend (POC; one emitter, two dialects). |
| `src/Plato.Intrinsics/` | The **live** handwritten C# runtime that discharges the intrinsic contract. |

---

## How codegen works

All backends share the same front end: parse `.plato` → build AST → compile (resolve symbols) →
type-check into the Typed IR (Normalize → Constrain → Solve → Elaborate → Monomorphize) → walk the
compilation with a language-specific writer. Every C# body is rendered from the monomorphized TIR
by `TirCSharpBodyWriter`; there is no second body writer.

```
stdlib/<tier>/*.plato  →  Plato.CLI  →  Plato.CSharpWriter     →  one .g.cs per type
                                     →  Plato.TypeScriptWriter →  plato.g.ts
                                     →  Plato.RustWriter       →  plato.rs
                                     →  Plato.GlslWriter       →  plato.glsl
                                     →  Plato.CppWriter        →  plato.hpp / plato.cu
```

### C# (the live recipe)

- **Consumer:** [`generated/Plato.Generated.Foundation.Unoptimized`](../generated/README.md) — a
  buildable project whose `.g.cs` files are ordinary cached output. Not a golden: there is no
  byte-identity gate and staleness is acceptable. Regenerating is the only way to change it.
- **Command** (from this repo's root): `.\tools\regen-foundation.ps1` — clears stale `*.g.cs`, runs
  the recipe, then builds the result. `-WhatIf` previews the diff without writing; `-Test` adds the
  generated-project tests. The invocation it wraps:

```bat
dotnet run --project src\Plato.CLI -c Release -- ^
  stdlib\foundation generated\Plato.Generated.Foundation.Unoptimized ^
  --csharp-style=extensions
```

- **Output:** one `.g.cs` per type, `partial` for hand extensions, extension methods (one static
  class per Plato library), and **no properties or indexers** — every no-arg member is a method,
  unconditionally
  ([decision](../tracker/decisions/2026-08-01-property-free-emission-is-unconditional.md)).
- **Flags.** `--csharp-style=extensions` is the only accepted value (the default style went at C4).
  `--scalar=…` is a **hard error**: scalars are always wrapper structs
  ([decision](../tracker/decisions/2026-08-01-wrapper-scalars-are-the-only-representation.md)).
  Optional: `--optimize` (component unrolling), `--optimize-arrays`, `--inline`, `--loops`,
  `--static-abstract`, and the diagnostics-only `--dump-tir=<dir>` / `--inline-report`.
  `--out=<folder>` frees every positional argument to be an input root, which is how several tiers
  are compiled as one program.
- **`ara3d-sdk/src/Plato.Generated/` is not generated from here any more.** It is a checked-in copy
  of the old V1 shape owned by the `ara3d-sdk` repo and frozen by studio's
  `tools/check-frozen-v1.ps1`. The `regen-plato.ps1` script that used to re-derive it is gone; see
  [`plato-library-map.md`](plato-library-map.md).
- **Intrinsics:** the runtime is `src/Plato.Intrinsics/`; it must supply every bodiless signature in
  `stdlib/foundation/intrinsics.library.plato` (gate: `IntrinsicObligationTests`).

### TypeScript (proof of concept)

- **Demo:** `demos/typescript/geometry-samples/` — browser samples via Three.js.
- **Command:**

```bat
dotnet run --project src\Plato.CLI -c Release -- ^
  <inputFolder> <outputFolder> --typescript
```

- **Output:** single module `plato.g.ts`. Primitives map to native `number`/`boolean`/`string` with prototype extensions for fluent calls: `(0.5).Turns().Cos()`. Concrete types become `export class`es.

### Rust (proof of concept)

- **Demo:** `demos/rust/geometry-samples/` — WASM browser demo; same algorithms as TS.
- **Command:**

```bat
dotnet run --project src\Plato.CLI -c Release -- ^
  <inputFolder> <outputFolder> --rust
```

- **Output:** single module `plato.rs`. Primitives get extension traits (`NumberExt` on `f64`, etc.). Structs are `Copy` with PascalCase API for parity with C#/TS.

### GLSL (proof of concept)

- **Demo:** `demos/glsl/` — eight WebGL2 shaders generated from Plato.
- **Command:**

```bat
dotnet run --project src\Plato.CLI -c Release -- ^
  <inputFolder> <outputFolder> --glsl
```

- **Output:** single `plato.glsl` (GLSL ES 3.00). Free functions only; `Number`→`float`; vectors→`vecN`. No lambdas, no dynamic arrays, no strings. Details: [`writers/Plato.GlslWriter/README.md`](../writers/Plato.GlslWriter/README.md).

### C++ / CUDA (proof of concept)

- **Tests:** `tests/Plato.CppWriter.Tests` — generate then compile-gate with MSVC (`/std:c++17`); CUDA uses nvcc when present, else an MSVC + `cuda_runtime.h` shim.
- **Command:**

```bat
dotnet run --project src\Plato.CLI -c Release -- ^
  <inputFolder> <outputFolder> --cpp
dotnet run --project src\Plato.CLI -c Release -- ^
  <inputFolder> <outputFolder> --cuda
```

- **Output:** `plato.hpp` (C++17) or `plato.cu` (nvcc). **Bodies are identical; only the preamble differs** (`Dialects_Differ_Only_In_The_Preamble`). Free functions; `Number`→`float`; vectors→`float2/3/4`. Same representability gaps as GLSL for lambdas / dynamic arrays / strings in V1. Details: [`writers/Plato.CppWriter/README.md`](../writers/Plato.CppWriter/README.md). Not in `Ara3D.Studio.sln` yet.

TS and Rust backends compile a curated demo library and pass a shared conformance suite; they have not yet consumed the full `stdlib-legacy` stdlib. Sum types are C#-only in v1 — the TS and Rust writers reject a sum declaration with a `CHK320` comment. Live demos: [cdiggins.github.io/plato](https://cdiggins.github.io/plato/). GLSL has eight live WebGL2 demos under `demos/glsl/`. C++/CUDA is compile-verified only (no runtime demos yet).

---

## Commands agents use

Run from **this repo's** root. Only `check-all.ps1` and `regen-forward-conformance.ps1` still live
in the studio checkout.

```bat
:: Inner loop after every edit batch: lint + checker ratchet + index freshness
.\tools\check-stdlib-fast.ps1

:: Codegen rung: regenerate the foundation tier and build the emitted C#
.\tools\regen-foundation.ps1          :: -WhatIf previews, -Test runs the generated tests

:: Faster still when the Plato Navigation MCP server is up: plato_check runs the same
:: gates warm, inside the server, against cached ASTs (see the `plato-mcp` skill).

:: Lint plato source directly (parse + resolve, no output). Each root is enumerated
:: TOP-DIRECTORY-ONLY, so tiers are named explicitly — `lint stdlib` alone finds nothing.
dotnet run --project src\Plato.CLI -c Release -- ^
  lint stdlib\foundation stdlib\geometry stdlib\graphics --strict

:: Export the stdlib index for agent context (tracked docs + gitignored stats)
tools\export-types-context.bat
::   -> stdlib\types-and-concepts.txt          (forward tiers; regeneration is MANDATORY
::                                              in any commit that changes stdlib/)
::   -> docs\types-and-concepts-context.txt    (stdlib-legacy)
::   -> .temp\types-and-concepts-context-stats.txt
```

**Exit codes.** Generate mode returns 1 on a parse failure, an unresolvable input folder, an
incomplete compilation, or a retired flag (`--scalar=`, `--csharp-style` other than `extensions`).
It still returns **0** when individual bodies fail to lower: those are emitted as throwing stubs and
logged as `DEGRADED bodies` on the console. Read that line — a green exit is not a clean library.

---

## Rules when editing Plato

1. **Nothing in this repo is frozen.** `legacy/stdlib-legacy/` is writable (2026-07-09), and
   `generated/` is ordinary cached output — the golden diff-gate and the V1 runtime freeze were
   retired 2026-07-30 / 2026-07-31. The pre-refactor snapshot and the legacy law packet were
   deleted; recover them from git history if you need them.
2. **Do not hand-edit any `.g.cs`** — a change comes from rerunning the recipe. That includes
   `ara3d-sdk/src/Plato.Generated/`, which is owned by the `ara3d-sdk` repo and is not regenerated
   from here at all.
3. **Known bugs are being fixed**, tracked in
   `tests/conformance/Plato.ForwardConformanceTests/KnownFailures.json` — fixing a bug means
   removing its entry in the same change; see [`docs/reports/plato-library-review.md`](reports/plato-library-review.md).
4. **Do not touch** `parakeet/` (nested submodule).
5. Prefer `+`/`*`/`-`/`/` for ordinary arithmetic in library bodies; keep named
   `Add`/`Multiply`/… at definition sites — see [`stdlib/STYLE_GUIDE.md`](../stdlib/STYLE_GUIDE.md)
   (Arithmetic spelling).

---

## Further reading

| Doc | Contents |
|-----|----------|
| [`SEMANTICS.md`](SEMANTICS.md) | **Normative language semantics** — construct meaning, resolution, coercions, non-features |
| [`../README.md`](../README.md) | Full language pitch, examples, demos |
| [`../CLAUDE.md`](../CLAUDE.md) | Repo layout, hard rules, mission protocol |
| [`archive/plato-roadmap.md`](archive/plato-roadmap.md) | Compiler and library roadmap (historical) |
| [`reports/plato-library-review.md`](reports/plato-library-review.md) | Verified stdlib bug catalog |
| [`../tracker/BACKLOG.md`](../tracker/BACKLOG.md) | Open Plato work items |
| [`../writers/Plato.TypeScriptWriter/README.md`](../writers/Plato.TypeScriptWriter/README.md) | TS output model |
| [`../writers/Plato.RustWriter/README.md`](../writers/Plato.RustWriter/README.md) | Rust output model |
| [`../writers/Plato.GlslWriter/README.md`](../writers/Plato.GlslWriter/README.md) | GLSL output model |
| [`../writers/Plato.CppWriter/README.md`](../writers/Plato.CppWriter/README.md) | C++ / CUDA output model |
