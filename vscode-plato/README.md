# Plato for VS Code / Cursor

Syntax highlighting plus Go to Definition / Find All References / Hover for
[Plato](https://github.com/ara3d/plato) `.plato` files. Navigation talks to
`Plato.Navigation.CLI serve` (same index as the plato-navigation MCP).

## Install

```bat
npm install
npm run compile
npx --yes @vscode/vsce package --no-dependencies
code --install-extension plato-0.2.7.vsix --force
cursor --install-extension plato-0.2.7.vsix --force
```

Then **Developer: Reload Window**.

Open a `.plato` file. Language mode (bottom-right) should be **Plato**.

| Action | How |
|--------|-----|
| Go to Definition | F12 / right-click / Ctrl+click |
| Find All References | Shift+F12 / right-click |
| Reload index | Command Palette → **Plato: Reload Navigation Index** |

Diagnostics: **Output** panel → **Plato Navigation**.

If Go to Definition fails with “Could not find Plato.Navigation.CLI”, open the Plato
repo as a workspace folder, or set `plato.navigation.cliProject` to
`src/Plato.Navigation.CLI/Plato.Navigation.CLI.csproj`.

## Related

- Visual Studio (full IDE highlighter): `../visualstudio-plato/`
- Language reference: `../docs/plato-for-agents.md`
- Navigation library: `../src/Plato.Navigation/`
