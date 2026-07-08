<#
.SYNOPSIS
    Export plato-src types and concepts for agent context.

.DESCRIPTION
    Runs Plato.ContextExport on plato-src and writes:
      docs/types-and-concepts-context.txt   (tracked — commit when stdlib changes)
      .temp/types-and-concepts-context-stats.txt   (gitignored diagnostics)

.EXAMPLE
    .\tools\export-types-context.ps1
    From studio root: .\submodules\Plato\tools\export-types-context.bat
#>
$ErrorActionPreference = 'Stop'

$PlatoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$Project = Join-Path $PlatoRoot 'Plato.ContextExport\Plato.ContextExport.csproj'
$Source = Join-Path $PlatoRoot 'plato-src'
$OutputFile = Join-Path $PlatoRoot 'docs\types-and-concepts-context.txt'
$StatsFile = Join-Path $PlatoRoot '.temp\types-and-concepts-context-stats.txt'

New-Item -ItemType Directory -Force (Join-Path $PlatoRoot '.temp') | Out-Null
New-Item -ItemType Directory -Force (Split-Path $OutputFile) | Out-Null

if (-not (Test-Path $Source)) {
    Write-Error "Source folder not found: $Source"
}

Write-Host "Building Plato.ContextExport..."
dotnet build $Project -c Release -v q
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

Write-Host "Exporting plato-src..."
$lines = & dotnet run --project $Project -c Release --no-build -- `
    $Source `
    --diagnostics-file $StatsFile
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

$utf8 = New-Object System.Text.UTF8Encoding $false
[System.IO.File]::WriteAllLines($OutputFile, $lines, $utf8)

Write-Host "Wrote $OutputFile"
if (Test-Path $StatsFile) {
    Write-Host "Stats: $StatsFile"
    Get-Content $StatsFile | ForEach-Object { Write-Host "  $_" }
}
