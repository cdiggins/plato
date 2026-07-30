# stage-stdlib.ps1 — gate stdlib edits against a PINNED Plato.CLI snapshot.
#
# Staging area (outside every git tree): C:\Users\cdigg\git\plato-staging
#   bin\     pinned Plato.CLI publish snapshot (refresh with -Snapshot while trunk is green)
#   stdlib\  scratch copy of the repo stdlib (refreshed by default on every run)
#
# Why: long-running compiler agents may leave the trunk Plato.CLI broken or half-built.
# Library work gates here against a known-good binary instead, so stdlib iteration
# never blocks on (or falsely fails from) compiler churn.
#
# Usage:
#   tools/stage-stdlib.ps1                  # sync repo stdlib -> staging, lint with pinned CLI
#   tools/stage-stdlib.ps1 -NoSync          # re-lint the staging copy as-is
#   tools/stage-stdlib.ps1 -Snapshot        # rebuild Plato.CLI (Release) and re-pin the binary
#   tools/stage-stdlib.ps1 -Folders a,b     # lint multiple roots (post-reorg tier subsets)
#
# -Folders bypasses the staging sync and lints the named folders IN PLACE (each enumerated
# top-directory-only, all compiled as one program), so tier subsets read as they will in the
# gate: -Folders stdlib\foundation,stdlib\geometry. Without it the whole stdlib is staged.
# NOTE: more than one root needs a pinned CLI built after multi-root support landed (2026-07-30).
# An older snapshot silently lints only the FIRST folder — re-pin with -Snapshot while trunk is green.
#
# Exit code = Plato.CLI exit code. Last lines of output carry the finding-count summary;
# compare against the recorded baseline in baseline.txt (written on every full run).

param(
    [switch] $NoSync,
    [switch] $Snapshot,
    [string[]] $Folders
)

$ErrorActionPreference = 'Stop'

# Wall-clock of the staged lint lands in the shared timing log (tools\gate-timing.ps1).
. (Join-Path $PSScriptRoot 'gate-timing.ps1')
$gateRun = Start-GateRun $(if ($Snapshot) { 'stage-stdlib -Snapshot' } else { 'stage-stdlib (lint)' })
trap { Complete-GateRun $gateRun 'FAIL' $_.Exception.Message; break }

$repo    = Split-Path $PSScriptRoot -Parent          # submodules/Plato
$staging = 'C:\Users\cdigg\git\plato-staging'
$bin     = Join-Path $staging 'bin'
$cli     = Join-Path $bin 'Plato.CLI.dll'

if ($Snapshot) {
    Write-Host "Rebuilding Plato.CLI (Release) and re-pinning snapshot..."
    dotnet build (Join-Path $repo 'Plato.CLI\Plato.CLI.csproj') -c Release --nologo -v q
    if ($LASTEXITCODE -ne 0) { throw "Plato.CLI build failed; snapshot NOT updated." }
    New-Item -ItemType Directory -Force $bin | Out-Null
    Copy-Item (Join-Path $repo 'Plato.CLI\bin\Release\net8.0\*') $bin -Recurse -Force
    Write-Host "Pinned: $cli"
}

if (-not (Test-Path $cli)) { throw "No pinned CLI at $cli - run with -Snapshot first (while trunk builds green)." }

if (-not $Folders) {
    $target = Join-Path $staging 'stdlib'
    if (-not $NoSync) {
        # /MIR so deletes/renames in the repo propagate (and /E-style recursion carries the
        # tier subfolders); staging copy is disposable.
        robocopy (Join-Path $repo 'stdlib') $target /MIR /NJH /NJS /NDL /NFL | Out-Null
        if ($LASTEXITCODE -ge 8) { throw "robocopy failed ($LASTEXITCODE)" }
    }
    # Each root is enumerated top-directory-only, so name the tier folders rather than the
    # staging root — linting the root itself would find ZERO files.
    $Folders = @('foundation','geometry','graphics','future') | ForEach-Object { Join-Path $target $_ }
}

$out = & dotnet $cli lint @Folders 2>&1
$code = $LASTEXITCODE
$summary = $out | Select-Object -Last 3
$out | Set-Content (Join-Path $staging 'last-lint.txt') -Encoding utf8
$summary | Set-Content (Join-Path $staging 'last-summary.txt') -Encoding utf8
$baselineFile = Join-Path $staging 'baseline.txt'   # first green run pins the comparison baseline; delete to re-pin
if (-not (Test-Path $baselineFile)) { $summary | Set-Content $baselineFile -Encoding utf8 }
Write-Host ($summary -join "`n")
Write-Host "Full output: $staging\last-lint.txt"
Exit-GateRun $gateRun $code
