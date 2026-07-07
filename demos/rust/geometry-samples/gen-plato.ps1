# Regenerates src/plato.rs from the shared Plato source (demos/plato-src/geometry.plato).
$root = Split-Path -Parent $MyInvocation.MyCommand.Path
dotnet run --project "$root\..\..\..\Plato.CLI" -- `
    "$root\..\..\plato-src" "$root\src" --rust
