# check-stdlib-fast.ps1 — cheap correctness gate for forward-stdlib (stdlib/) content work.
# The inner-loop companion to check-all.ps1: run this after every edit batch, run the full
# battery once at the end of a mission. Two checks, PASS/FAIL table, failure output printed:
#   1. lint --strict       Plato.CLI parse + name resolution over stdlib (expect 0 errors)
#   2. checker ratchet     PlatoTests ForwardStdLibDiagnosticCountDoesNotRegress — type-checks
#                          every stdlib function; ceiling lives in
#                          PlatoTests/ForwardStdLibCheckerTests.cs
#                          (worklist test: SummarizeForwardStdLibDiagnostics)
#   3. index freshness     stdlib/types-and-concepts.txt still matches stdlib/. The rule it
#                          enforces lives in stdlib/AGENTS.md; the gate only regenerates into
#                          .temp/ and compares, so it never writes the tracked file.
# All paths derive from $PSScriptRoot, so this works from any Plato checkout or git worktree.
# Usage: .\tools\check-stdlib-fast.ps1 [-SkipLint] [-SkipRatchet] [-SkipIndex] [-IncludeFuture]
#                                      [-Folders a,b]
#   -Folders lints an explicit set of roots instead of all of stdlib/, each enumerated
#   top-directory-only and compiled as ONE program — the cumulative-tier subset form, e.g.
#   -Folders stdlib\foundation,stdlib\geometry. Relative paths resolve against the repo root.
#   -IncludeFuture adds stdlib\future to the default root list (stdlib-377). It is OFF by
#   default: `future` is aspirational vocabulary that is neither linted nor converted to C#,
#   and its findings would drown the shipping tiers'. It must still parse and type-check,
#   which the checker ratchet below covers — that one always reads all four tiers.
# Exit code: 0 if all executed gates pass, 1 otherwise.
param(
    [switch]$SkipLint,
    [switch]$SkipRatchet,
    [switch]$SkipIndex,
    [switch]$IncludeFuture,
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
    } else {
        # stdlib is split into dependency-ordered tier folders and Plato.CLI enumerates each
        # root top-directory-only, so the default gate names the shipping tiers explicitly.
        # Linting the bare `stdlib` folder would find ZERO files. `future` joins only under
        # -IncludeFuture; nothing reaches into it, so the three compile on their own.
        $tiers = @('foundation','geometry','graphics')
        if ($IncludeFuture) { $tiers += 'future' }
        $tiers | ForEach-Object { Join-Path $root "stdlib\$_" }
    }
    $name = if ($Folders) { "lint --strict ($($Folders -join ' + '))" }
            elseif ($IncludeFuture) { 'lint --strict (stdlib forward + future)' }
            else { 'lint --strict (stdlib forward)' }
    Run-Gate $name 'dotnet' (@(
        'run','--project',(Join-Path $root 'src\Plato.CLI\Plato.CLI.csproj'),
        '-c','Release','--','lint') + $roots + @('--strict'))
}

if (-not $SkipRatchet) {
    Run-Gate 'checker ratchet (forward stdlib)' 'dotnet' @(
        'test',(Join-Path $root 'tests\PlatoTests\PlatoTests.csproj'),
        '-c','Release','--nologo','-v','q',
        '--filter','FullyQualifiedName~ForwardStdLibDiagnosticCountDoesNotRegress')
}

if (-not $SkipIndex) {
    # -IncludeFuture is deliberately not forwarded: the index covers the shipping tiers only.
    Run-Gate 'index freshness (types-and-concepts)' 'powershell' @(
        '-NoProfile','-ExecutionPolicy','Bypass',
        '-File',(Join-Path $PSScriptRoot 'export-types-context.ps1'),
        '-Check','-IndexOnly')
}

$results | Format-Table -AutoSize | Out-String | Write-Host
Write-Host ("Total: {0:n1}s (history: .\tools\gate-timings.ps1)" -f ($results | Measure-Object Seconds -Sum).Sum)
$failed = @($results | Where-Object Result -eq 'FAIL')
if ($failed.Count -gt 0) { Write-Host "FAILED: $($failed.Gate -join ', ')"; exit 1 }
Write-Host 'STDLIB FAST GATE PASS'
exit 0
