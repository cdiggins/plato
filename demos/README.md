# Plato geometry demos

Multi-language proof that Plato geometry code can be written once and compiled
to fast, idiomatic libraries on several targets.

**Live on GitHub Pages: <https://cdiggins.github.io/plato/>**
([TypeScript](https://cdiggins.github.io/plato/typescript/) ·
[SDF](https://cdiggins.github.io/plato/sdf/) ·
[Rust + WASM](https://cdiggins.github.io/plato/rust/))

## Layout

```
demos/
  plato-src/
    geometry.plato              Shared Plato source (TypeScript + Rust demos)
  typescript/
    geometry-samples/           Vite + Three.js sample browser; Node tests
    sdf/                        Forward-stdlib SDF catalog (2D fields + 3D raymarch)
  rust/
    geometry-samples/           Cargo crate; WASM browser demo; conformance tests
  glsl/                         Plato → GLSL PoC studio
  site/
    index.html                  Landing page for the GitHub Pages site
```

The curated `geometry.plato` here is a **demo subset** — not the full standard
library in [`../stdlib-legacy/`](../legacy/stdlib-legacy/) at the repo root (which feeds C#
codegen for `Ara3D.Geometry`).

## Quick start

**TypeScript geometry sample browser** (from `demos/typescript/geometry-samples/`):

```bat
npm install
npm run dev        # http://localhost:5173
```

**TypeScript SDF demo** (from `demos/typescript/sdf/`):

```bat
npm install
npm run gen:plato  # TypeScript from stdlib/ via Plato.CLI --typescript
npm run dev        # http://localhost:5174
```

**Rust browser demo** (from `demos/rust/geometry-samples/`; the WASM module is
checked in, so this needs only Node — no Rust toolchain):

```bat
node web/serve.mjs   # http://localhost:8873
```

**Tests:**

```bat
cd demos/typescript/geometry-samples && npm test
cd demos/rust/geometry-samples && cargo test
```

**Regenerate from Plato** after editing `demos/plato-src/geometry.plato`:

```bat
cd demos/typescript/geometry-samples
npm run gen:plato

cd demos/rust/geometry-samples
./gen-plato.ps1
```

Both call [`Plato.CLI`](../src/Plato.CLI) with `--typescript` or `--rust`.

## GitHub Pages deployment

[`.github/workflows/pages.yml`](../.github/workflows/pages.yml) builds and
deploys the site on every push to `main`: the landing page from `site/`, the
Vite build of the TypeScript demo under `/typescript/`, and the static Rust
demo (`web/` plus the `.rs` sources shown in its code panel) under `/rust/`.
Enable it once in the repo settings: **Settings → Pages → Source: GitHub
Actions**.

When cloned inside the [Ara 3D studio](https://github.com/ara3d/studio) monorepo,
these demos live at `submodules/Plato/demos/`. `Plato.CLI` still references
`ara3d-sdk` for `Ara3D.Utils` and `Ara3D.Logging` when built from studio.

## See also

- [TypeScript demo README](typescript/geometry-samples/README.md)
- [TypeScript SDF demo README](typescript/sdf/README.md)
- [Rust demo README](rust/geometry-samples/README.md)
- [Plato.TypeScriptWriter](../writers/Plato.TypeScriptWriter/README.md)
- [Plato.RustWriter](../writers/Plato.RustWriter/README.md)
