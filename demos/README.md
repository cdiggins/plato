# Plato geometry demos

Multi-language proof that Plato geometry code can be written once and compiled
to fast, idiomatic libraries on several targets.

## Layout

```
demos/
  plato-src/
    geometry.plato              Shared Plato source (TypeScript + Rust demos)
  typescript/
    geometry-samples/           Vite + Three.js browser demo; Node tests
  rust/
    geometry-samples/           Cargo crate; WASM browser demo; conformance tests
```

The curated `geometry.plato` here is a **demo subset** — not the full standard
library in [`../plato-src/`](../plato-src/) at the repo root (which feeds C#
codegen for `Ara3D.Geometry`).

## Quick start

**TypeScript browser demo** (from `demos/typescript/geometry-samples/`):

```bat
npm install
npm run dev
```

**Rust tests** (from `demos/rust/geometry-samples/`):

```bat
cargo test
```

**Regenerate from Plato** after editing `demos/plato-src/geometry.plato`:

```bat
cd demos/typescript/geometry-samples
npm run gen:plato

cd demos/rust/geometry-samples
./gen-plato.ps1
```

Both call [`Plato.CLI`](../Plato.CLI) with `--typescript` or `--rust`.

When cloned inside the [Ara 3D studio](https://github.com/ara3d/studio) monorepo,
these demos live at `submodules/Plato/demos/`. `Plato.CLI` still references
`ara3d-sdk` for `Ara3D.Utils` and `Ara3D.Logging` when built from studio.

## See also

- [TypeScript demo README](typescript/geometry-samples/README.md)
- [Rust demo README](rust/geometry-samples/README.md)
- [Plato.TypeScriptWriter](../Plato.TypeScriptWriter/README.md)
- [Plato.RustWriter](../Plato.RustWriter/README.md)
