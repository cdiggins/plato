<#
.SYNOPSIS
    Regenerates the self-contained Plato.Small.* projects from plato-src-small and (by
    default) builds them. A fast optimizer-spike loop, independent of the full library.

.DESCRIPTION
    Two variants, both extension-style + scalar-erased, linking the tiny Plato.Small.Runtime
    (NOT Plato.Intrinsics.V2):

        Unoptimized : --csharp-style=extensions --scalar=float --no-properties
        Optimized   : + --optimize --optimize-arrays --inline --methods --loops

    Overwrites each project's *.g.cs in place (this is a dev aid, not a diff-gated golden).
    -NoBuild skips the build; -DumpTir also writes per-phase TIR next to the optimized output.

.EXAMPLE
    .\regen-small.ps1            # regenerate + build both
    .\regen-small.ps1 -DumpTir   # also dump TIR for the optimized variant
#>
param(
    [switch]$NoBuild,
    [switch]$DumpTir,
    [string]$Configuration = 'Release'
)

$ErrorActionPreference = 'Stop'
$smallDir  = $PSScriptRoot
$platoRepo = Split-Path -Parent $smallDir
$cliProj   = Join-Path $platoRepo 'Plato.CLI\Plato.CLI.csproj'
$inputDir  = Join-Path $platoRepo 'plato-src-small'

$variants = @(
    @{ Name = 'Plato.Small.Generated.Unoptimized'
       Flags = @('--csharp-style=extensions', '--scalar=float', '--no-properties') },
    @{ Name = 'Plato.Small.Optimized'
       Flags = @('--csharp-style=extensions', '--scalar=float', '--optimize', '--optimize-arrays', '--inline', '--methods', '--loops', '--no-properties') }
)

Write-Host "== Building Plato.CLI ($Configuration) =="
dotnet build $cliProj -c $Configuration -v minimal --nologo | Out-Null
if ($LASTEXITCODE -ne 0) { throw "dotnet build failed (exit $LASTEXITCODE)" }

foreach ($v in $variants) {
    $projDir = Join-Path $smallDir $v.Name
    Write-Host ""
    Write-Host "== $($v.Name):  $($v.Flags -join ' ') =="

    # Clear stale .g.cs (keep the .csproj), then regenerate in place.
    Get-ChildItem $projDir -File -Filter '*.g.cs' -ErrorAction SilentlyContinue | Remove-Item -Force
    $flags = @($v.Flags)
    if ($DumpTir -and $v.Name -eq 'Plato.Small.Optimized') {
        $flags += "--dump-tir=$(Join-Path $projDir 'tir-dump')"
    }
    dotnet run --project $cliProj -c $Configuration --no-build -- $inputDir $projDir @flags | Out-Null
    if ($LASTEXITCODE -ne 0) { throw "Plato.CLI failed for $($v.Name) (exit $LASTEXITCODE)" }

    # docs.html / interfaces.txt side-products aren't wanted in the project folder.
    Get-ChildItem $projDir -File -Include 'docs.html','interfaces.txt' -ErrorAction SilentlyContinue | Remove-Item -Force
    $n = @(Get-ChildItem $projDir -File -Filter '*.g.cs').Count
    Write-Host "   generated $n .g.cs files"
}

if (-not $NoBuild) {
    Write-Host ""
    Write-Host "== Building both Small projects =="
    foreach ($v in $variants) {
        dotnet build (Join-Path $smallDir $v.Name) -c $Configuration -v minimal --nologo | Out-Null
        if ($LASTEXITCODE -ne 0) { throw "build failed for $($v.Name) (exit $LASTEXITCODE)" }
        Write-Host "   $($v.Name): build OK"
    }
}

Write-Host ""
Write-Host "Done." -ForegroundColor Green
