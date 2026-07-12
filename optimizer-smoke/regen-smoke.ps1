<#
.SYNOPSIS
    Regenerates the optimizer smoke-test projects and (optionally) compiles them.

.DESCRIPTION
    Four variants on two axes:
        SUBSET     full = all of plato-src ; min = the files in manifest-min.txt
        OPTIMIZER  nonopt = adoption shape only ; opt = + inline/optimize/optimize-arrays/loops

    The adoption SHAPE is constant across all four (--csharp-style=extensions --scalar=float
    --methods): native primitives, extension functions, no properties. The OPTIMIZER passes are
    the axis under test. Each variant's C# lands in Smoke.<Subset>.<Opt>\Generated and is a real
    csproj, so `-Build` runs `dotnet build` as a compilation smoke test.

    Use -DumpTir to also write the per-phase TIR (layers 5-10) next to each variant's output.

.EXAMPLE
    .\regen-smoke.ps1                    # regenerate all four
    .\regen-smoke.ps1 -Which min -Build  # regenerate + compile just the two minimal variants
    .\regen-smoke.ps1 -Build -DumpTir    # all four, compiled, with TIR dumps
#>
[CmdletBinding()]
param(
    [ValidateSet('all', 'min', 'full')] [string]$Which = 'all',
    [switch]$Build,
    [switch]$DumpTir,
    [string]$Configuration = 'Release'
)

$ErrorActionPreference = 'Stop'
$here      = $PSScriptRoot
$platoRoot = Split-Path -Parent $here
$cliProj   = Join-Path $platoRoot 'Plato.CLI\Plato.CLI.csproj'
$platoSrc  = Join-Path $platoRoot 'plato-src'
$manifest  = Join-Path $here 'manifest-min.txt'

foreach ($p in @($cliProj, $platoSrc, $manifest)) {
    if (-not (Test-Path $p)) { throw "Expected path not found: $p" }
}

Write-Host "== Building Plato.CLI ($Configuration) =="
dotnet build $cliProj -c $Configuration -v minimal --nologo | Out-Null
if ($LASTEXITCODE -ne 0) { throw "dotnet build failed (exit $LASTEXITCODE)" }

# The minimal subset: copy the manifest-listed files into a temp input folder.
$minInput = Join-Path $env:TEMP 'plato-smoke\min-src'
function Get-MinInput {
    if (Test-Path $minInput) { Remove-Item -Recurse -Force $minInput }
    New-Item -ItemType Directory -Force $minInput | Out-Null
    $names = Get-Content $manifest | ForEach-Object { $_.Trim() } |
        Where-Object { $_ -and -not $_.StartsWith('#') }
    foreach ($n in $names) {
        $src = Join-Path $platoSrc $n
        if (-not (Test-Path $src)) { throw "manifest-min.txt lists '$n' but it is not in plato-src" }
        Copy-Item $src (Join-Path $minInput $n)
    }
    Write-Host ("   minimal subset: {0} .plato files" -f $names.Count)
    return $minInput
}

$shape = @('--csharp-style=extensions', '--scalar=float', '--methods')
$opt   = @('--inline', '--optimize', '--optimize-arrays', '--loops')

$variants = @()
if ($Which -in @('all', 'full')) {
    $variants += @{ Name = 'Full.NonOpt'; Input = $platoSrc; Flags = $shape }
    $variants += @{ Name = 'Full.Opt';    Input = $platoSrc; Flags = $shape + $opt }
}
if ($Which -in @('all', 'min')) {
    $min = Get-MinInput
    $variants += @{ Name = 'Min.NonOpt'; Input = $min; Flags = $shape }
    $variants += @{ Name = 'Min.Opt';    Input = $min; Flags = $shape + $opt }
}

$results = @()
foreach ($v in $variants) {
    $proj = Join-Path $here "Smoke.$($v.Name)"
    $gen  = Join-Path $proj 'Generated'
    $flags = @($v.Flags)
    if ($DumpTir) { $flags += "--dump-tir=$(Join-Path $proj 'tir-dump')" }

    Write-Host ""
    Write-Host "== Smoke.$($v.Name) ($($v.Flags -join ' ')) =="
    if (Test-Path $gen) { Remove-Item -Recurse -Force $gen }
    New-Item -ItemType Directory -Force $gen | Out-Null

    dotnet run --project $cliProj -c $Configuration --no-build -- $v.Input $gen @flags | Out-Null
    if ($LASTEXITCODE -ne 0) { throw "Plato.CLI failed for Smoke.$($v.Name) (exit $LASTEXITCODE)" }
    $count = @(Get-ChildItem $gen -File -Filter '*.g.cs').Count
    Write-Host ("   generated {0} .g.cs files" -f $count)

    $buildResult = 'skipped'
    if ($Build) {
        dotnet build (Join-Path $proj "Smoke.$($v.Name).csproj") -c $Configuration -v minimal --nologo
        $buildResult = $(if ($LASTEXITCODE -eq 0) { 'PASS' } else { 'FAIL' })
    }
    $results += [pscustomobject]@{ Variant = "Smoke.$($v.Name)"; Files = $count; Build = $buildResult }
}

Write-Host ""
$results | Format-Table -AutoSize | Out-String | Write-Host
if ($Build -and ($results | Where-Object Build -eq 'FAIL')) {
    Write-Host "FAILED: $(($results | Where-Object Build -eq 'FAIL').Variant -join ', ')"
    exit 1
}
Write-Host "Done."
exit 0
