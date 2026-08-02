# regen-foundation.ps1 — run the C# codegen recipe and prove the result still compiles.
#
# The codegen rung of stdlib/VERIFICATION.md has no gate script: check-stdlib-fast.ps1 stops at
# lint + type-check, and the byte-identity gate that used to cover emission was retired 2026-07-30
# together with the goldens. What was left was a copy-pasted CLI invocation in three README files.
# This script is that invocation, plus the build that gives it meaning.
#
# It is NOT a golden gate. generated/ is ordinary cached output and staleness is acceptable
# (generated/README.md), so a non-empty -WhatIf diff is information, not a failure.
#
#   1. generate   src\Plato.CLI over stdlib\foundation, --csharp-style=extensions, into
#                 generated\Plato.Generated.Foundation.Unoptimized. Stale *.g.cs are cleared
#                 first so a type deleted from the library stops being emitted.
#   2. build      the generated project (net8.0, DEFAULT LangVersion — hard rule 3).
#   3. test       optional (-Test): tests\Plato.Generated.Foundation.Tests.
#
# Usage: .\tools\regen-foundation.ps1 [-WhatIf] [-Test] [-Flags '--optimize','--inline']
#   -WhatIf   generate into .temp\regen-foundation and report how many files would change,
#             leaving the tracked output untouched. Use it to see whether the checked-in
#             emission still matches the library before deciding to refresh it. It does not
#             build: the .csproj globs *.g.cs from its own folder, so only a real regeneration
#             is compilable, and a preview is a drift question rather than a codegen question.
#   -Flags    extra CLI flags for an experimental recipe. The tracked project is emitted with
#             none, and its .csproj header comment is the record of that; passing -Flags writes
#             output the header no longer describes, so use it with -WhatIf or revert after.
# Exit code: 0 if every executed step passes, 1 otherwise.
param(
    [switch]   $WhatIf,
    [switch]   $Test,
    [string[]] $Flags = @()
)

$ErrorActionPreference = 'Continue'
$root = Split-Path -Parent $PSScriptRoot   # Plato repo root (parent of tools/)
$results = [System.Collections.Generic.List[object]]::new()
. (Join-Path $PSScriptRoot 'gate-timing.ps1')

$source  = Join-Path $root 'stdlib\foundation'
$tracked = Join-Path $root 'generated\Plato.Generated.Foundation.Unoptimized'
$project = Join-Path $tracked 'Plato.Generated.Foundation.Unoptimized.csproj'
$dest    = if ($WhatIf) { Join-Path $root '.temp\regen-foundation' } else { $tracked }

function Run-Step([string]$name, [scriptblock]$block) {
    $sw = [System.Diagnostics.Stopwatch]::StartNew()
    try { $ok = & $block } catch { Write-Host $_; $ok = $false }
    $sw.Stop()
    $result = $(if ($ok) { 'PASS' } else { 'FAIL' })
    Add-GateTiming -Gate $name -Result $result -Seconds $sw.Elapsed.TotalSeconds -Script 'regen-foundation.ps1'
    $script:results.Add([pscustomobject]@{
        Step = $name; Result = $result; Seconds = [math]::Round($sw.Elapsed.TotalSeconds, 1)
    })
    return $ok
}

if (-not (Test-Path $dest)) { New-Item -ItemType Directory -Path $dest -Force | Out-Null }
# Clearing first is what makes this a regeneration rather than an overlay: the CLI only writes the
# types it emits, so a *.g.cs left from a deleted type would survive and still compile.
Get-ChildItem -Path $dest -Filter '*.g.cs' -File -ErrorAction SilentlyContinue | Remove-Item -Force

$generated = Run-Step "generate (stdlib\foundation$(if ($Flags) { ' ' + ($Flags -join ' ') }))" {
    $out = & dotnet run --project (Join-Path $root 'src\Plato.CLI\Plato.CLI.csproj') -c Release -- `
        $source $dest '--csharp-style=extensions' @Flags
    $ok = ($LASTEXITCODE -eq 0)
    # Generate mode exits 0 when an individual body fails to lower — those become throwing stubs
    # and are only visible on this console line, so a silent green here would be a wrong green.
    $degraded = @($out | Where-Object { $_ -match 'DEGRADED bodies' })
    if ($degraded) { $out | Where-Object { $_ -match 'DEGRADED|degraded:' } | Write-Host; $ok = $false }
    if (-not $ok) { $out | Select-Object -Last 20 | Write-Host }
    $ok
}

if ($generated -and $WhatIf) {
    $changed = 0; $added = 0; $removed = 0
    $freshNames = @{}
    foreach ($f in Get-ChildItem -Path $dest -Filter '*.g.cs' -File) {
        $freshNames[$f.Name] = $true
        $old = Join-Path $tracked $f.Name
        if (-not (Test-Path $old)) { $added++ }
        elseif ((Get-FileHash $old).Hash -ne (Get-FileHash $f.FullName).Hash) { $changed++ }
    }
    foreach ($f in Get-ChildItem -Path $tracked -Filter '*.g.cs' -File) {
        if (-not $freshNames.ContainsKey($f.Name)) { $removed++ }
    }
    Write-Host "WhatIf: changed $changed, added $added, removed $removed (tracked output untouched; fresh copy in $dest)"
}

if ($generated -and -not $WhatIf) {
    Run-Step 'build (net8.0 Release)' {
        & dotnet build $project -c 'Release' --nologo -v q
        $LASTEXITCODE -eq 0
    } | Out-Null

    if ($Test) {
        Run-Step 'Plato.Generated.Foundation.Tests' {
            & dotnet test (Join-Path $root 'tests\Plato.Generated.Foundation.Tests') -c Release --nologo -v q
            $LASTEXITCODE -eq 0
        } | Out-Null
    }
}

$results | Format-Table -AutoSize | Out-String | Write-Host
Write-Host ("Total: {0:n1}s (history: .\tools\gate-timings.ps1)" -f ($results | Measure-Object Seconds -Sum).Sum)
$failed = @($results | Where-Object Result -eq 'FAIL')
if ($failed.Count -gt 0) { Write-Host "FAILED: $($failed.Step -join ', ')"; exit 1 }
Write-Host 'FOUNDATION CODEGEN PASS'
exit 0
