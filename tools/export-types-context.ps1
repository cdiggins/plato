<#
.SYNOPSIS
    Export types/concepts context and the concept hierarchy for agent review.

.DESCRIPTION
    Runs Plato.ContextExport twice:
      1. Flat declaration dump of legacy/stdlib-legacy →
         docs/types-and-concepts-context.txt  (tracked)
         .temp/types-and-concepts-context-stats.txt  (gitignored)
      2. ASCII concept inherits forest of stdlib/ →
         docs/concept-hierarchy.txt  (tracked)

.EXAMPLE
    .\tools\export-types-context.ps1
    From studio root: .\submodules\Plato\tools\export-types-context.bat
#>
$ErrorActionPreference = 'Stop'

$PlatoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$Project = Join-Path $PlatoRoot 'src\Plato.ContextExport\Plato.ContextExport.csproj'
$LegacySource = Join-Path $PlatoRoot 'legacy\stdlib-legacy'
$ForwardSource = Join-Path $PlatoRoot 'stdlib'
$FlatOutput = Join-Path $PlatoRoot 'docs\types-and-concepts-context.txt'
$HierarchyOutput = Join-Path $PlatoRoot 'docs\concept-hierarchy.txt'
$StatsFile = Join-Path $PlatoRoot '.temp\types-and-concepts-context-stats.txt'

New-Item -ItemType Directory -Force (Join-Path $PlatoRoot '.temp') | Out-Null
New-Item -ItemType Directory -Force (Split-Path $FlatOutput) | Out-Null

if (-not (Test-Path $LegacySource)) {
    Write-Error "Legacy source folder not found: $LegacySource"
}
if (-not (Test-Path $ForwardSource)) {
    Write-Error "Forward stdlib folder not found: $ForwardSource"
}

Write-Host "Building Plato.ContextExport..."
dotnet build $Project -c Release -v q
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

$utf8 = New-Object System.Text.UTF8Encoding $false

Write-Host "Exporting flat context (legacy/stdlib-legacy)..."
$lines = & dotnet run --project $Project -c Release --no-build -- `
    $LegacySource `
    --diagnostics-file $StatsFile
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
[System.IO.File]::WriteAllLines($FlatOutput, $lines, $utf8)
Write-Host "Wrote $FlatOutput"

Write-Host "Exporting concept hierarchy (stdlib)..."
# Write via a temp file so PowerShell does not re-encode UTF-8 stdout from dotnet.
$HierarchyTemp = Join-Path $PlatoRoot '.temp\concept-hierarchy-raw.txt'
& dotnet run --project $Project -c Release --no-build -- `
    $ForwardSource `
    --hierarchy `
    --output $HierarchyTemp
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
Copy-Item -Force $HierarchyTemp $HierarchyOutput
Write-Host "Wrote $HierarchyOutput"

if (Test-Path $StatsFile) {
    Write-Host "Stats: $StatsFile"
    Get-Content $StatsFile | ForEach-Object { Write-Host "  $_" }
}
