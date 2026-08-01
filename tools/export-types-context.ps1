<#
.SYNOPSIS
    Export the compressed types/concepts index for agent context.

.DESCRIPTION
    Runs Plato.ContextExport over:
      1. The shipped stdlib/ tiers, one section per tier →
         docs/stdlib-index.txt  (tracked)
         The `future` tier is excluded: it is declared, not shipped.
      2. Flat declaration dump of legacy/stdlib-legacy →
         docs/types-and-concepts-context.txt  (tracked)
         .temp/types-and-concepts-context-stats.txt  (gitignored)

    Re-run whenever stdlib/ changes; both outputs are tracked, so a stale
    index shows up as an uncommitted diff.

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
$IndexOutput = Join-Path $PlatoRoot 'docs\stdlib-index.txt'
$StatsFile = Join-Path $PlatoRoot '.temp\types-and-concepts-context-stats.txt'
$TempDir = Join-Path $PlatoRoot '.temp'

# Shipped tiers, in dependency order. `future` is deliberately absent.
$Tiers = @('foundation', 'geometry', 'graphics')

New-Item -ItemType Directory -Force $TempDir | Out-Null
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

Write-Host "Exporting stdlib index (stdlib, excluding future)..."
$indexLines = New-Object System.Collections.Generic.List[string]
foreach ($tier in $Tiers) {
    $tierSource = Join-Path $ForwardSource $tier
    if (-not (Test-Path $tierSource)) {
        Write-Error "Stdlib tier folder not found: $tierSource"
    }

    # Write via a temp file so PowerShell does not re-encode UTF-8 stdout from dotnet.
    $tierTemp = Join-Path $TempDir "stdlib-index-$tier.txt"
    & dotnet run --project $Project -c Release --no-build -- `
        $tierSource `
        --output $tierTemp
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

    $indexLines.Add("// ---- $tier ----")
    $indexLines.AddRange([string[]][System.IO.File]::ReadAllLines($tierTemp))
}
[System.IO.File]::WriteAllLines($IndexOutput, $indexLines, $utf8)
Write-Host "Wrote $IndexOutput"

Write-Host "Exporting flat context (legacy/stdlib-legacy)..."
$lines = & dotnet run --project $Project -c Release --no-build -- `
    $LegacySource `
    --diagnostics-file $StatsFile
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
[System.IO.File]::WriteAllLines($FlatOutput, $lines, $utf8)
Write-Host "Wrote $FlatOutput"

if (Test-Path $StatsFile) {
    Write-Host "Stats: $StatsFile"
    Get-Content $StatsFile | ForEach-Object { Write-Host "  $_" }
}
