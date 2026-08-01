<#
.SYNOPSIS
    Export the compressed types/concepts index for agent context.

.DESCRIPTION
    Runs Plato.ContextExport over:
      1. The shipping stdlib/ tiers as one list — every concept first, then
         every type, each group sorted by name →
         stdlib/types-and-concepts.txt  (tracked)
         The `future` tier is excluded: it is declared, not shipped.
      2. Flat declaration dump of legacy/stdlib-legacy →
         docs/types-and-concepts-context.txt  (tracked)
         .temp/types-and-concepts-context-stats.txt  (gitignored)

    Re-run whenever stdlib/ changes; both outputs are tracked, so a stale
    index shows up as an uncommitted diff.

.PARAMETER Check
    Do not write anything. Regenerate into .temp/ and compare against the
    tracked file, exiting 1 if they differ. This is the freshness gate
    check-stdlib-fast.ps1 runs.

.PARAMETER IndexOnly
    Skip the stdlib-legacy dump; only handle stdlib/types-and-concepts.txt.

.EXAMPLE
    .\tools\export-types-context.ps1
    From studio root: .\submodules\Plato\tools\export-types-context.bat

.EXAMPLE
    .\tools\export-types-context.ps1 -Check -IndexOnly
#>
param(
    [switch]$Check,
    [switch]$IndexOnly
)

$ErrorActionPreference = 'Stop'

$PlatoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$Project = Join-Path $PlatoRoot 'src\Plato.ContextExport\Plato.ContextExport.csproj'
$LegacySource = Join-Path $PlatoRoot 'legacy\stdlib-legacy'
$ForwardSource = Join-Path $PlatoRoot 'stdlib'
$FlatOutput = Join-Path $PlatoRoot 'docs\types-and-concepts-context.txt'
$IndexOutput = Join-Path $PlatoRoot 'stdlib\types-and-concepts.txt'
$StatsFile = Join-Path $PlatoRoot '.temp\types-and-concepts-context-stats.txt'
$TempDir = Join-Path $PlatoRoot '.temp'

# Shipped tiers, in dependency order. `future` is deliberately absent.
$Tiers = @('foundation', 'geometry', 'graphics')

New-Item -ItemType Directory -Force $TempDir | Out-Null
New-Item -ItemType Directory -Force (Split-Path $FlatOutput) | Out-Null

if (-not $IndexOnly -and -not (Test-Path $LegacySource)) {
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
$tierSources = @($Tiers | ForEach-Object {
    $tierSource = Join-Path $ForwardSource $_
    if (-not (Test-Path $tierSource)) {
        Write-Error "Stdlib tier folder not found: $tierSource"
    }
    $tierSource
})

# One invocation over every shipping tier, so concepts and types are each sorted across the
# whole library rather than per folder. Write via a temp file so PowerShell does not re-encode
# UTF-8 stdout from dotnet.
$IndexTemp = Join-Path $TempDir 'types-and-concepts.txt'
& dotnet run --project $Project -c Release --no-build -- `
    @tierSources `
    --output $IndexTemp
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
$indexLines = [string[]][System.IO.File]::ReadAllLines($IndexTemp)

if ($Check) {
    # Compare against the tracked file without touching it. Line-by-line, so the
    # answer does not depend on the checkout's line endings.
    $tracked = if (Test-Path $IndexOutput) { [string[]][System.IO.File]::ReadAllLines($IndexOutput) } else { @() }
    $diff = @(Compare-Object $tracked $indexLines)
    if ($diff.Count -gt 0) {
        Write-Host "STALE: $IndexOutput does not match stdlib/."
        Write-Host "$($diff.Count) line(s) differ (<= only in the file, => only in stdlib/):"
        $diff | Select-Object -First 10 | ForEach-Object { Write-Host "  $($_.SideIndicator) $($_.InputObject)" }
        Write-Host "Regenerate with: .\tools\export-types-context.bat"
        exit 1
    }
    Write-Host "Fresh: $IndexOutput matches stdlib/."
    if ($IndexOnly) { exit 0 }
}
else {
    [System.IO.File]::WriteAllLines($IndexOutput, $indexLines, $utf8)
    Write-Host "Wrote $IndexOutput"
    if ($IndexOnly) { exit 0 }
}

Write-Host "Exporting flat context (legacy/stdlib-legacy)..."
$lines = & dotnet run --project $Project -c Release --no-build -- `
    $LegacySource `
    --diagnostics-file $StatsFile
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

if ($Check) {
    $trackedFlat = if (Test-Path $FlatOutput) { [string[]][System.IO.File]::ReadAllLines($FlatOutput) } else { @() }
    if (@(Compare-Object $trackedFlat ([string[]]$lines)).Count -gt 0) {
        Write-Host "STALE: $FlatOutput does not match legacy/stdlib-legacy."
        Write-Host "Regenerate with: .\tools\export-types-context.bat"
        exit 1
    }
    Write-Host "Fresh: $FlatOutput matches legacy/stdlib-legacy."
}
else {
    [System.IO.File]::WriteAllLines($FlatOutput, $lines, $utf8)
    Write-Host "Wrote $FlatOutput"
}

if (Test-Path $StatsFile) {
    Write-Host "Stats: $StatsFile"
    Get-Content $StatsFile | ForEach-Object { Write-Host "  $_" }
}
