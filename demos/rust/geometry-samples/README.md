# geometry-samples (Rust)

The Rust port of the geometry-samples proof of concept: **one Plato source,
many languages**. The same `geometry.plato` file that generates the TypeScript
library for the browser demo also generates the Rust library used by this crate.

```
../../plato-src/geometry.plato          (shared source of truth)
        │
        │  Plato.CLI --rust             (../../../Plato.CLI)
        ▼
src/plato.rs                            (GENERATED — do not edit)
        │
        │  used by
        ▼
src/samples/*.rs      12 sample algorithm drivers  (hand-written)
tests/*.rs            conformance + invariant tests
```

## Layout

- `src/plato.rs` — generated Plato library: `Vector2D`/`Vector3D`/`Angle`
  structs, `NumberExt`/`IntegerExt`/`BooleanExt` extension traits on the
  native types (fluent `(0.5).Turns().Cos()`), `Constants`, `Arr<T>`.
- `src/core/` — hand-written plumbing: `Drawable`/`MeshData` types,
  mesh-builder helpers (`grid_mesh`, `compute_vertex_normals`, ...), and the
  mulberry32 RNG (exact port; same seeds → same values as TypeScript).
- `src/samples/` — the 12 algorithm drivers, ported ~1:1 from
  `../../typescript/geometry-samples/src/samples/`. The Plato calls read
  identically to the TypeScript and C# versions; only the plumbing (loops,
  `Vec` handling) is language-specific.
- `tests/plato_conformance.rs` — the algebra identities from the TypeScript
  `plato.test.ts`, run against the generated Rust library.
- `tests/sample_invariants.rs` — the 11 sample invariants from the TypeScript
  suite (Euler characteristic, empty circumcircle, hull convexity, ray hit
  distances, ...), same seeds as TypeScript.

## Commands

```
cargo test           # conformance + sample invariants
cargo clippy         # clean (generated file has its own allow list)
./gen-plato.ps1      # regenerate src/plato.rs from geometry.plato
./build-wasm.ps1     # rebuild web/geometry_samples.wasm (needs the wasm32 target)
node web/serve.mjs   # demo browser at http://localhost:8873/
```

## Browser demo (Rust → WebAssembly)

`web/` contains a sample browser like the TypeScript demo's: a sidebar of the
12 samples, a Three.js viewport, and a code panel showing the Rust driver
source (plus the generated `plato.rs`). The geometry is computed **in the
browser by this crate compiled to WebAssembly** — every click calls into the
wasm module and reads the resulting positions/indices/normals straight out of
wasm linear memory (`src/wasm_api.rs`, a hand-rolled C ABI: no wasm-bindgen,
no npm build step; Three.js comes from a CDN import map).

```
rustup target add wasm32-unknown-unknown   # once
./build-wasm.ps1                           # after changing Rust code
node web/serve.mjs                         # then open http://localhost:8873/
```

The compiled module (~120 KB) is checked in as `web/geometry_samples.wasm` so
the demo runs without a Rust toolchain.

The generated Plato API keeps PascalCase (`v.Length()`, `v.X`) for parity with
C#/TypeScript — this is deliberate; `#![allow(non_snake_case)]` covers it.
Hand-written driver plumbing is ordinary snake_case Rust.
