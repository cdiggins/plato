# Plato — agent guide

Plato is a small, pure, statically typed language for geometry and numeric libraries. In the [Ara 3D studio](https://github.com/ara3d/studio) monorepo it lives at `submodules/Plato/`. Write algorithms once in `.plato` files; the compiler emits idiomatic libraries for multiple targets.

**Why agents care:** the full language plus standard library is ~34K tokens — small enough to hold in context. One declaration fans out into hundreds of generated members across types and targets, so global consistency is a single edit, not a multi-file refactor.

---

## Language (three constructs)

| Construct | Role |
|-----------|------|
| **`type`** | Immutable data (fields only). Tuples construct types: `(x, y)` → `Point2D`. |
| **`interface`** | Type classes with a `Self` type — constrained generics, not OO virtual dispatch. |
| **`library`** | Pure free functions; first argument is the receiver (`v.Length`, `a.Lerp(b, t)`). |

Restrictions are intentional: no mutation, no `this`, no I/O, no exceptions. Distinct types (`Angle` vs `Number`, `Point` vs `Vector`) catch common geometry bugs at compile time. Interface-generic code is **monomorphized** into direct calls on each concrete type.

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

| Path | Purpose |
|------|---------|
| `plato-src/*.plato` | Production standard library (~3,500 lines → 11,000+ lines of C#). |
| `plato-test-src/` | Law/witness tests only — **never merge into `plato-src`**. |
| `demos/plato-src/geometry.plato` | Curated demo subset for TS/Rust browsers (not the full stdlib). |
| `Plato.CLI/` | Compiler entry point. |
| `Plato.ContextExport/` | Compact export of types + concepts for agent context (`tools/export-types-context.bat`). |
| `docs/types-and-concepts-context.txt` | Generated stdlib context (types + concepts only); regen via `tools/export-types-context.bat`. |
| `Plato.TypeScriptWriter/` | TypeScript backend (POC). |
| `Plato.RustWriter/` | Rust backend (POC). |
| `Plato.Intrinsics/` | Handwritten C# runtime wrappers (source of truth; synced to SDK). |

---

## How codegen works

All backends share the same pipeline: parse `.plato` → build AST → compile (resolve symbols) → walk compilation with a language-specific writer.

```
plato-src/*.plato  →  Plato.CLI  →  Plato.CSharpWriter   →  ara3d-sdk/src/Plato.Generated/
                                 →  Plato.TypeScriptWriter →  plato.g.ts
                                 →  Plato.RustWriter       →  plato.rs
```

### C# (production)

- **Consumer:** `ara3d-sdk/src/Plato.Generated/` — checked-in output that backs `Ara3D.Geometry`.
- **Command** (from studio repo root): `.\tools\regen-plato.ps1` (diff-gates byte identity; `-Apply` writes changes). Or directly:

```bat
dotnet run --project submodules\Plato\Plato.CLI -c Release -- ^
  submodules\Plato\plato-src ara3d-sdk\src\Plato.Generated
```

- **Output:** one `.g.cs` per type, packed structs, aggressive inlining, `partial` for hand extensions. Flags: `--csharp-style=default|extensions`, `--optimize`, `--scalar=wrapper|float`.
- **Intrinsics:** `Plato.Intrinsics/` (Number, Vector, Matrix, Angle wrappers) is edited here, synced to `ara3d-sdk/src/Plato.Intrinsics/` via `regen-plato.ps1 -Apply`. Never edit the SDK copy.

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

TS and Rust backends compile a curated demo library and pass a shared conformance suite; they have not yet consumed the full `plato-src` stdlib. Live demos: [cdiggins.github.io/plato](https://cdiggins.github.io/plato/).

---

## Commands agents use

Run from the studio repo root unless noted.

```bat
:: Regenerate production C#
.\tools\regen-plato.ps1              :: check drift (exit 1 if changed)
.\tools\regen-plato.ps1 -Apply       :: write + sync intrinsics

:: Lint plato source (parse + resolve, no output)
dotnet run --project submodules\Plato\Plato.CLI -c Release -- lint submodules\Plato\plato-src

:: Export full stdlib context for agents (tracked doc + gitignored stats)
submodules\Plato\tools\export-types-context.bat
::   -> submodules\Plato\docs\types-and-concepts-context.txt
::   -> submodules\Plato\.temp\types-and-concepts-context-stats.txt
```

**Caveat:** `Plato.CLI` in generate mode **exits 0 even on compile errors** — always verify output file count or build the result.

---

## Rules when editing Plato

1. **`plato-src/` is WRITABLE** as of 2026-07-09 (content-leads refactor; the Phase-4 freeze is retired).
   Edit freely; the frozen pre-refactor snapshot lives in `plato-src-legacy/` (reference only). Plan:
   [`docs/plato-execution-plan-2026-07-09.md`](../../../docs/plato-execution-plan-2026-07-09.md).
2. **Do not hand-edit** `ara3d-sdk/src/Plato.Generated/` — regenerate via `regen-plato.ps1`.
3. **Known bugs are being fixed**, tracked in `conformance/.../KnownFailures.json`; see [`docs/plato-library-review.md`](../../../docs/plato-library-review.md).
4. **Do not touch** `parakeet/` (nested submodule).
5. Conformance expected result: **142 pass / 36 ignored-known / 0 fail** (`.\tools\check-all.ps1` from studio root).

---

## Further reading

| Doc | Contents |
|-----|----------|
| [`../README.md`](../README.md) | Full language pitch, examples, demos |
| [`../CLAUDE.md`](../CLAUDE.md) | Repo layout, hard rules, mission protocol |
| [`../../../docs/plato-roadmap.md`](../../../docs/plato-roadmap.md) | Compiler and library roadmap (studio repo) |
| [`../../../docs/plato-library-review.md`](../../../docs/plato-library-review.md) | Verified stdlib bug catalog (studio repo) |
| [`../Plato.TypeScriptWriter/README.md`](../Plato.TypeScriptWriter/README.md) | TS output model |
| [`../Plato.RustWriter/README.md`](../Plato.RustWriter/README.md) | Rust output model |
