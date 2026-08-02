# Plato for VS Code / Cursor

Syntax highlighting and compiler-backed navigation (Go to Definition, Find All References,
Hover) for [Plato](https://github.com/ara3d/plato) `.plato` source files.

Highlighting is derived from the Plato Parakeet grammar (`type` / `interface` / `concept` /
`library`, `implements` / `inherits` / `where`, expression bodies with `=>`, C++-style comments,
etc.). Navigation talks to `Plato.Navigation.CLI serve` over stdio NDJSON — the same index the
plato-navigation MCP uses.

For the full Visual Studio IDE, use `../visualstudio-plato/` (MEF classifier). That package is
unrelated to this VS Code / Cursor extension.

## Install (development)

From this folder (requires `npm` once, then a TypeScript compile):

```bat
npm install
npm run compile
code --install-extension .
```

Or in Cursor / VS Code: **Extensions → ⋯ → Install from VSIX…** after packaging, or
**Developer: Install Extension from Location…** pointed at this folder.

After a source change, re-run `npm run compile` and reload the window
(`Developer: Reload Window`).

### Package a `.vsix`

```bat
npx --yes @vscode/vsce package --no-dependencies
```

That writes `plato-0.2.4.vsix` in this directory.

## Navigation

On first Go to Definition / Find References / Hover in a `.plato` file the extension:

1. Locates `src/Plato.Navigation.CLI/Plato.Navigation.CLI.csproj` by walking up from the
   workspace (and from this extension folder). Studio checkouts that keep Plato under
   `submodules/Plato/` are also recognized.
2. Builds the CLI in Release if the DLL is missing.
3. Starts `serve` with a root scoped to the open file’s corpus (`stdlib`, `stdlib-tests`,
   `stdlib-legacy`, `stdlib-legacy-tests`) so sibling libraries are never mixed.

| Setting | Purpose |
|---------|---------|
| `plato.navigation.roots` | Explicit folders to index (overrides discovery) |
| `plato.navigation.cliProject` | Override path to `Plato.Navigation.CLI.csproj` |
| `plato.navigation.dotnetPath` | `dotnet` executable (default `dotnet`) |

Command palette: **Plato: Reload Navigation Index**.

Diagnostics land in the **Plato Navigation** output channel. If activation fails with
“Could not find Plato.Navigation.CLI.csproj”, open the Plato repo (or studio) as a workspace
folder, or set `plato.navigation.cliProject`.

### Known limits

- Overloads are not disambiguated — Go to Definition may offer every overload in the group.
- Match-expression binders and some compiler-built-ins have no definition site.
- Full detail: `../src/Plato.Navigation/README.md`.

## What it colors

| Scope | Examples |
|-------|----------|
| Declarations | `type`, `interface`, `concept`, `library`, `unique` |
| Clauses | `implements`, `inherits`, `where` |
| Control | `if`, `else`, `for`, `foreach`, `return`, … |
| Literals | numbers, strings, `true` / `false` / `null` |
| Special | `Self`, `=>`, operators, comments |

## Related

- Visual Studio (full IDE): `../visualstudio-plato/`
- Language reference: `../docs/plato-for-agents.md`
- Navigation library: `../src/Plato.Navigation/`
