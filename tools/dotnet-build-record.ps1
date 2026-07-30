<#
.SYNOPSIS
    Run `dotnet build` on a Plato C# project and record error counts into the status snapshot.

.DESCRIPTION
    Wraps `dotnet build`, writes a log under .temp/csharp-build-logs/, then runs
    tools/record-csharp-build-errors.py so docs/status-report-snapshot.json always
    carries the latest C# error totals (by CS code and by category).

    Prefer this over a bare `dotnet build` for Plato projects so the status report stays honest.

.EXAMPLE
    .\tools\dotnet-build-record.ps1 -Project .\conformance\Plato.ForwardConformanceTests\Plato.ForwardConformanceTests.csproj -TargetName forward-conformance
    .\tools\dotnet-build-record.ps1 -Project .\Generated\Plato.Generated.Unoptimized\Plato.Generated.Unoptimized.csproj -TargetName generated-unoptimized
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$Project,

    [Parameter(Mandatory = $true)]
    [string]$TargetName,

    [string]$Configuration = 'Release',

    # When set, a failing build still exits 0 after recording (diagnostic builds).
    [switch]$RecordOnly
)

$ErrorActionPreference = 'Stop'

# C# builds are a large share of gate wall-clock, so each one is timed into the shared log
# (tools\gate-timing.ps1) alongside its error counts.
. (Join-Path $PSScriptRoot 'gate-timing.ps1')
$gateRun = Start-GateRun "build: $TargetName"
trap { Complete-GateRun $gateRun 'FAIL' $_.Exception.Message; break }

$PlatoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$ProjectPath = if ([System.IO.Path]::IsPathRooted($Project)) { $Project } else { Join-Path $PlatoRoot $Project }
if (-not (Test-Path $ProjectPath)) { throw "Project not found: $ProjectPath" }

$logDir = Join-Path $PlatoRoot '.temp\csharp-build-logs'
New-Item -ItemType Directory -Force -Path $logDir | Out-Null
$logPath = Join-Path $logDir "$TargetName.log"

Write-Host "== dotnet build ($Configuration) -> record as '$TargetName' =="
Write-Host "   project: $ProjectPath"
Write-Host "   log:     $logPath"

# UTF-8 log (PowerShell `*>` defaults to UTF-16, which the recorder must not mis-read).
$prevEAP = $ErrorActionPreference
$ErrorActionPreference = 'Continue'
$output = & dotnet build $ProjectPath -c $Configuration --nologo -v minimal 2>&1
$buildExit = $LASTEXITCODE
$ErrorActionPreference = $prevEAP
$utf8NoBom = New-Object System.Text.UTF8Encoding $false
[System.IO.File]::WriteAllLines($logPath, @($output | ForEach-Object { "$_" }), $utf8NoBom)

$py = Get-Command python -ErrorAction SilentlyContinue
if (-not $py) { $py = Get-Command python3 -ErrorAction SilentlyContinue }
if (-not $py) { throw "python not found; cannot record C# build errors" }

& $py.Source (Join-Path $PlatoRoot 'tools\record-csharp-build-errors.py') `
    --target $TargetName `
    --log $logPath `
    --project $ProjectPath `
    --configuration $Configuration `
    --build-exit $buildExit
if ($LASTEXITCODE -ne 0) { throw "record-csharp-build-errors.py failed (exit $LASTEXITCODE)" }

# Also refresh the HTML report so the committed page matches the snapshot when someone opens it.
& $py.Source (Join-Path $PlatoRoot 'tools\gen-status-report.py') | Out-Null

if ($RecordOnly) {
    Write-Host ("Recorded build errors for '{0}' (build exit {1}; -RecordOnly so script exit 0)." -f $TargetName, $buildExit)
    Complete-GateRun $gateRun $(if ($buildExit -eq 0) { 'PASS' } else { 'FAIL' }) "record-only, build exit $buildExit"
    exit 0
}
Exit-GateRun $gateRun $buildExit
