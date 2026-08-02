# Plato for Visual Studio

MEF classifier that maps `.plato` to a `plato` content type and colors keywords, comments,
strings, numbers, and operators. This is the full Visual Studio IDE extension (VSSDK) — it is
**not** the Cursor / VS Code package in `../vscode-plato/`.

| Feature | Visual Studio (`visualstudio-plato`) | Cursor / VS Code (`vscode-plato`) |
|---------|--------------------------------------|-----------------------------------|
| Syntax highlighting | Yes | Yes |
| Go to Definition / Find References / Hover | No | Yes (`Plato.Navigation.CLI`) |

## Build

Requires Visual Studio 2022+ with the Visual Studio extension development workload.

```bat
"C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe" PlatoVSIX.csproj -p:Configuration=Release
```

Output: `bin\Release\PlatoVSIX.vsix`.

## Install

```bat
"C:\Program Files\Microsoft Visual Studio\2022\Community\Common7\IDE\VSIXInstaller.exe" /quiet bin\Release\PlatoVSIX.vsix
```

Or double-click the `.vsix`. **Close all Visual Studio windows first** — quiet install fails with
exit code 2004 (`BlockingProcessesException`) while `devenv` is running. Restart Visual Studio
afterward so it rescans extensions.

Supported: VS 2022 and VS 2026 (Community / Pro / Enterprise).

## Keywords

Kept in sync with `vscode-plato/syntaxes/plato.tmLanguage.json` (`unique`, `type`, `concept`,
`library`, `primitive`, `implements`, `inherits`, …).
