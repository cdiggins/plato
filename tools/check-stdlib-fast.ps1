# check-stdlib-fast.ps1 — cheap correctness gate for forward-stdlib (stdlib/) content work.
# The inner-loop companion to check-all.ps1: run this after every edit batch, run the full
# battery once at the end of a mission. Two checks, PASS/FAIL table, failure output printed:
#   1. lint --strict       Plato.CLI parse + name resolution over stdlib (expect 0 errors)
#   2. checker ratchet     PlatoTests ForwardStdLibDiagnosticCountDoesNotRegress — type-checks
#                          every stdlib function; ceiling lives in
#                          PlatoTests/ForwardStdLibCheckerTests.cs
#                          (worklist test: SummarizeForwardStdLibDiagnostics)
# All paths derive from $PSScriptRoot, so this works from any Plato checkout or git worktree.
# Usage: .\tools\check-stdlib-fast.ps1 [-SkipLint] [-SkipRatchet] [-Folders a,b]
#   -Folders lints an explicit set of roots instead of all of stdlib/, each enumerated
#   top-directory-only and compiled as ONE program — the cumulative-tier subset form, e.g.
#   -Folders stdlib\foundation,stdlib\geometry. Relative paths resolve against the repo root.
# Exit code: 0 if all executed gates pass, 1 otherwise.
param(
    [switch]$SkipLint,
    [switch]$SkipRatchet,
    [string[]]$Folders
)

$ErrorActionPreference = 'Continue'
$root = Split-Path -Parent $PSScriptRoot   # Plato repo root (parent of tools/)
$results = [System.Collections.Generic.List[object]]::new()
. (Join-Path $PSScriptRoot 'gate-timing.ps1')

# Unlike check-all's Run-Quiet, failures replay their captured output: this script is an
# agent inner loop, and a bare FAIL row would just force an immediate re-run for the details.
function Run-Gate([string]$name, [string]$file, [string[]]$argList) {
    $sw = [System.Diagnostics.Stopwatch]::StartNew()
    $output = & $file @argList 2>&1
    $ok = ($LASTEXITCODE -eq 0)
    $sw.Stop()
    if (-not $ok) { $output | Out-String | Write-Host }
    $result = $(if ($ok) { 'PASS' } else { 'FAIL' })
    Add-GateTiming -Gate $name -Result $result -Seconds $sw.Elapsed.TotalSeconds -Script 'check-stdlib-fast.ps1'
    $script:results.Add([pscustomobject]@{
        Gate = $name
        Result = $result
        Seconds = [math]::Round($sw.Elapsed.TotalSeconds, 1)
    })
}

if (-not $SkipLint) {
    $roots = if ($Folders) {
        @($Folders | ForEach-Object {
            if ([System.IO.Path]::IsPathRooted($_)) { $_ } else { Join-Path $root $_ } })
    } else { @(Join-Path $root 'stdlib') }
    $name = if ($Folders) { "lint --strict ($($Folders -join ' + '))" } else { 'lint --strict (stdlib forward)' }
    Run-Gate $name 'dotnet' (@(
        'run','--project',(Join-Path $root 'Plato.CLI\Plato.CLI.csproj'),
        '-c','Release','--','lint') + $roots + @('--strict'))
}

if (-not $SkipRatchet) {
    Run-Gate 'checker ratchet (forward stdlib)' 'dotnet' @(
        'test',(Join-Path $root 'PlatoTests\PlatoTests.csproj'),
        '-c','Release','--nologo','-v','q',
        '--filter','FullyQualifiedName~ForwardStdLibDiagnosticCountDoesNotRegress')
}

$results | Format-Table -AutoSize | Out-String | Write-Host
Write-Host ("Total: {0:n1}s (history: .\tools\gate-timings.ps1)" -f ($results | Measure-Object Seconds -Sum).Sum)
$failed = @($results | Where-Object Result -eq 'FAIL')
if ($failed.Count -gt 0) { Write-Host "FAILED: $($failed.Gate -join ', ')"; exit 1 }
Write-Host 'STDLIB FAST GATE PASS'
exit 0
