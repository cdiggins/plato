# Plato for VS Code / Cursor

Syntax highlighting for [Plato](https://github.com/ara3d/plato) `.plato` source files.

Derived from the Plato Parakeet grammar (`type` / `interface` / `concept` / `library`, `implements` / `inherits` / `where`, expression bodies with `=>`, C++-style comments, etc.). The older Visual Studio package (`PlatoVSIX`) targeted the full Visual Studio IDE and is unrelated to this extension.

## Install (development)

From this folder:

```bat
code --install-extension .
```

Or in Cursor / VS Code: **Extensions → ⋯ → Install from VSIX…** after packaging, or open this folder and use **Developer: Install Extension from Location…**.

### Package a `.vsix`

```bat
npx --yes @vscode/vsce package --no-dependencies
```

That writes `plato-0.1.0.vsix` in this directory.

## What it colors

| Scope | Examples |
|-------|----------|
| Declarations | `type`, `interface`, `concept`, `library`, `unique` |
| Clauses | `implements`, `inherits`, `where` |
| Control | `if`, `else`, `for`, `foreach`, `return`, … |
| Literals | numbers, strings, `true` / `false` / `null` |
| Special | `Self`, `=>`, operators, comments |

## Related

- Visual Studio (full IDE) classifier sources historically lived under `ara3d-sdk/toolchain/Plato/PlatoVSIX` (built `.vsix` may still be present under `bin/Release/`).
- Language reference: `../docs/plato-for-agents.md`
