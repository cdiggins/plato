# Plato — agent guide

Plato is a small, pure, statically typed language for geometry and numeric libraries. In the [Ara 3D studio](https://github.com/ara3d/studio) monorepo it lives at `submodules/Plato/`. Write algorithms once in `.plato` files; the compiler emits idiomatic libraries for multiple targets.

**Why agents care:** the full language plus standard library is ~34K tokens — small enough to hold in context. One declaration fans out into hundreds of generated members across types and targets, so global consistency is a single edit, not a multi-file refactor.

---

## Language

**Normative reference: [`plato-language-semantics.md`](plato-language-semantics.md)** — what every
construct means, resolution/coercion rules, and the explicit non-features. The 5-line version:

- **`type`** — immutable data: fields only, or a sum type (`type X = A(f: T) | B;`) consumed by exhaustive `match`.
- **`primitive`** — a type the compiler assumes **by name**, with no declarable shape; the whole set is `stdlib/foundation/primitives.plato`. Only these may appear in an intrinsic signature.
- **`concept`** (alias `interface`) — type classes with a `Self` type; constrained generics, not OO dispatch; monomorphized.
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

Paths below are relative to this submodule (`submodules/Plato/`).

**Stdlib mapping:** `stdlib` = forward vocabulary (the next-generation library); `stdlib-legacy` =
shipping generation (drives `Plato.Generated` / Studio).

In `stdlib/`, one file holds exactly one **kind** of declaration, with no cap on how many:
`<stem>.plato` = types, `<stem>.concepts.plato` = concepts, `<stem>.library.plato` =
exactly one `library` block. Files sit directly in the tier folders (`foundation`, `geometry`,
`graphics`, `future`) described by [`stdlib/README.md`](../stdlib/README.md).

| Path | Purpose |
|------|---------|
| `stdlib/` | Forward stdlib vocabulary — types, concepts, **and** library bodies. New *vocabulary* goes here. Read its [`README`](../stdlib/README.md), [`CONVENTIONS`](../stdlib/CONVENTIONS.md), [`STYLE_GUIDE`](../stdlib/STYLE_GUIDE.md), and [`LIBRARIES`](../stdlib/LIBRARIES.md) before editing. |
| `tests/stdlib-tests/` | Forward law packet (`Law_*`) for `stdlib/` — **never merge into `stdlib`**. |
| `legacy/stdlib-legacy/*.plato` | Shipping standard library (~3,500 lines → 11,000+ lines of C#). |
| `stdlib-legacy-tests/` | Law/witness tests only — **never merge into `stdlib-legacy`**. |
| `legacy/stdlib-snapshot-2026-07-09/` | Frozen pre-refactor snapshot — reference only. |
| `demos/plato-src/geometry.plato` | Curated demo subset for TS/Rust browsers (not the full stdlib). |
| `src/Plato.CLI/` | Compiler entry point. |
| `Plato.ContextExport/` | Compact export of types + concepts for agent context (`tools/export-types-context.bat`). |
| `stdlib/types-and-concepts.txt` | Generated index of every type + concept in the shipping `stdlib/` tiers (`future` excluded), one compressed declaration per line, every concept first and then every type, each group sorted by name. Regeneration is mandatory when `stdlib/` changes — see [`stdlib/AGENTS.md`](../stdlib/AGENTS.md). |
| `docs/types-and-concepts-context.txt` | Generated stdlib-legacy context (types + concepts only); same regen script. |
| `writers/Plato.TypeScriptWriter/` | TypeScript backend (POC). |
| `writers/Plato.RustWriter/` | Rust backend (POC). |
| `writers/Plato.GlslWriter/` | GLSL ES 3.00 / WebGL2 backend (POC). |
| `writers/Plato.CppWriter/` | C++17 / CUDA backend (POC; one emitter, two dialects). |
| `src/Plato.Intrinsics/` | The **live** handwritten C# runtime that discharges the intrinsic contract. |

---

## How codegen works

All backends share the same pipeline: parse `.plato` → build AST → compile (resolve symbols) → walk compilation with a language-specific writer.

```
stdlib-legacy/*.plato  →  Plato.CLI  →  Plato.CSharpWriter   →  ara3d-sdk/src/Plato.Generated/
                                 →  Plato.TypeScriptWriter →  plato.g.ts
                                 →  Plato.RustWriter       →  plato.rs
                                 →  Plato.GlslWriter       →  plato.glsl
                                 →  Plato.CppWriter        →  plato.hpp / plato.cu
```

### C# (production)

- **Consumer:** `ara3d-sdk/src/Plato.Generated/` — checked-in output that backs `Ara3D.Geometry`.
- **Command** (from studio repo root): `.\tools\regen-plato.ps1` (diff-gates byte identity; `-Apply` writes changes). Or directly:

```bat
dotnet run --project submodules\Plato\Plato.CLI -c Release -- ^
  submodules\Plato\stdlib-legacy ara3d-sdk\src\Plato.Generated
```

- **Output:** one `.g.cs` per type, packed structs, aggressive inlining, `partial` for hand extensions. Flags: `--csharp-style=default|extensions`, `--optimize`, `--scalar=wrapper|float`.
- **Intrinsics:** the runtime is `src/Plato.Intrinsics/`; it must supply every bodiless signature in `stdlib/foundation/intrinsics.library.plato` (gate: `IntrinsicObligationTests`). The old V1 runtime was deleted 2026-07-31; the copy in `ara3d-sdk` belongs to that repo.

### TypeScript (proof of concept)

- **Demo:** `demos/typescript/geometry-samples/` — browser samples via Three.js.
- **Command:**

```bat
dotnet run --project submodules\Plato\Plato.CLI -c Release -- ^
  <inputFolder> <outputFolder> --typescript
```

- **Output:** single module `plato.g.ts`. Primitives map to native `number`/`boolean`/`string` with prototype extensions for fluent calls: `(0.5).Turns().Cos()`. Concrete types become `export class`es.

### Rust (proof of concept)

- **Demo:** `demos/rust/geometry-samples/` — WASM browser demo; same algorithms as TS.
- **Command:**

```bat
dotnet run --project submodules\Plato\Plato.CLI -c Release -- ^
  <inputFolder> <outputFolder> --rust
```

- **Output:** single module `plato.rs`. Primitives get extension traits (`NumberExt` on `f64`, etc.). Structs are `Copy` with PascalCase API for parity with C#/TS.

### GLSL (proof of concept)

- **Demo:** `demos/glsl/` — eight WebGL2 shaders generated from Plato.
- **Command:**

```bat
dotnet run --project submodules\Plato\Plato.CLI -c Release -- ^
  <inputFolder> <outputFolder> --glsl
```

- **Output:** single `plato.glsl` (GLSL ES 3.00). Free functions only; `Number`→`float`; vectors→`vecN`. No lambdas, no dynamic arrays, no strings. Details: [`writers/Plato.GlslWriter/README.md`](../writers/Plato.GlslWriter/README.md).

### C++ / CUDA (proof of concept)

- **Tests:** `tests/Plato.CppWriter.Tests` — generate then compile-gate with MSVC (`/std:c++17`); CUDA uses nvcc when present, else an MSVC + `cuda_runtime.h` shim.
- **Command:**

```bat
dotnet run --project submodules\Plato\Plato.CLI -c Release -- ^
  <inputFolder> <outputFolder> --cpp
dotnet run --project submodules\Plato\Plato.CLI -c Release -- ^
  <inputFolder> <outputFolder> --cuda
```

- **Output:** `plato.hpp` (C++17) or `plato.cu` (nvcc). **Bodies are identical; only the preamble differs** (`Dialects_Differ_Only_In_The_Preamble`). Free functions; `Number`→`float`; vectors→`float2/3/4`. Same representability gaps as GLSL for lambdas / dynamic arrays / strings in V1. Details: [`writers/Plato.CppWriter/README.md`](../writers/Plato.CppWriter/README.md). Not in `Ara3D.Studio.sln` yet.

TS and Rust backends compile a curated demo library and pass a shared conformance suite; they have not yet consumed the full `stdlib-legacy` stdlib. Sum types are C#-only in v1 — the TS and Rust writers reject a sum declaration with a `CHK320` comment. Live demos: [cdiggins.github.io/plato](https://cdiggins.github.io/plato/). GLSL has eight live WebGL2 demos under `demos/glsl/`. C++/CUDA is compile-verified only (no runtime demos yet).

---

## Commands agents use

Run from the studio repo root unless noted.

```bat
:: Regenerate production C#
.\tools\regen-plato.ps1              :: check drift (exit 1 if changed)
.\tools\regen-plato.ps1 -Apply       :: write + sync intrinsics

:: Lint plato source (parse + resolve, no output)
dotnet run --project submodules\Plato\Plato.CLI -c Release -- lint submodules\Plato\stdlib-legacy

:: Export full stdlib context for agents (tracked doc + gitignored stats)
submodules\Plato\tools\export-types-context.bat
::   -> submodules\Plato\stdlib\types-and-concepts.txt
::   -> submodules\Plato\docs\types-and-concepts-context.txt
::   -> submodules\Plato\.temp\types-and-concepts-context-stats.txt
```

**Caveat:** `src/Plato.CLI` in generate mode **exits 0 even on compile errors** — always verify output file count or build the result.

---

## Rules when editing Plato

1. **`legacy/stdlib-legacy/` is WRITABLE** as of 2026-07-09 (content-leads refactor; the Phase-4 freeze is retired).
   Edit freely; the frozen pre-refactor snapshot lives in `legacy/stdlib-snapshot-2026-07-09/` (reference only). Plan:
   [`docs/plato-execution-plan-2026-07-09.md`](plato-execution-plan-2026-07-09.md).
2. **Do not hand-edit** `ara3d-sdk/src/Plato.Generated/` — regenerate via `regen-plato.ps1`.
3. **Known bugs are being fixed**, tracked in `tests/conformance/.../KnownFailures.json`; see [`docs/plato-library-review.md`](plato-library-review.md).
4. **Do not touch** `parakeet/` (nested submodule).
5. Conformance expected result: **142 pass / 36 ignored-known / 0 fail** (`.\tools\check-all.ps1` from studio root).
6. Prefer `+`/`*`/`-`/`/` for ordinary arithmetic in library bodies; keep named
   `Add`/`Multiply`/… at definition sites — see [`stdlib/STYLE_GUIDE.md`](../stdlib/STYLE_GUIDE.md)
   (Arithmetic spelling).

---

## Further reading

| Doc | Contents |
|-----|----------|
| [`plato-language-semantics.md`](plato-language-semantics.md) | **Normative language semantics** — construct meaning, resolution, coercions, non-features |
| [`../README.md`](../README.md) | Full language pitch, examples, demos |
| [`../CLAUDE.md`](../CLAUDE.md) | Repo layout, hard rules, mission protocol |
| [`archive/plato-roadmap.md`](archive/plato-roadmap.md) | Compiler and library roadmap (historical) |
| [`plato-library-review.md`](plato-library-review.md) | Verified stdlib bug catalog |
| [`../tracker/BACKLOG.md`](../tracker/BACKLOG.md) | Open Plato work items |
| [`../Plato.TypeScriptWriter/README.md`](../writers/Plato.TypeScriptWriter/README.md) | TS output model |
| [`../Plato.RustWriter/README.md`](../writers/Plato.RustWriter/README.md) | Rust output model |
| [`../Plato.GlslWriter/README.md`](../writers/Plato.GlslWriter/README.md) | GLSL output model |
| [`../Plato.CppWriter/README.md`](../writers/Plato.CppWriter/README.md) | C++ / CUDA output model |
