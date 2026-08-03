# regen-triangulation.ps1 — run the polygon triangulator and check that its output tiles.
#
# Rung 6 of stdlib/VERIFICATION.md (execute) for one library. The general rung-6 runner
# (Plato.ForwardConformanceTests) is blocked on plato-308: C# generated from the whole forward
# stdlib does not compile, so nothing in geometry/ can be executed through it. This script
# reaches the same rung for stdlib/geometry/triangulation.library.plato by generating a SUBSET
# that does compile — foundation plus the declaration closure the triangulator needs — and
# running NUnit over it.
#
#   1. stage      foundation/*.plato plus the geometry closure, into .temp\triangulation-src
#   2. close      ask the CLI what is still unresolved and add the file declaring it, until it
#                 resolves. This is what keeps the seed list below from rotting: when stdlib
#                 moves a type, the list self-heals instead of failing.
#   3. generate   src\Plato.CLI over the staged folder, into .temp\triangulation-gen
#   4. test       tests\Plato.Triangulation.Tests, which compiles that output and asserts the
#                 invariant the mapbox/earcut suite uses — emitted triangles tile the polygon,
#                 areas summing to its area, every face counter-clockwise.
#
# Nothing it writes is tracked: .temp/ is gitignored, and the staged subset is a workaround for
# plato-308, not an artifact worth keeping. When plato-308 clears, this script should be deleted
# and its cases moved to the law packet (stdlib/tests/triangulation.laws.plato says which ones
# cannot go there today and why).
#
# Usage: .\tools\regen-triangulation.ps1 [-SkipTest] [-Verbose]
# Exit code: 0 if every executed step passes, 1 otherwise.
param(
    [switch] $SkipTest
)

$ErrorActionPreference = 'Continue'
$root = Split-Path -Parent $PSScriptRoot
. (Join-Path $PSScriptRoot 'gate-timing.ps1')

$src     = Join-Path $root '.temp\triangulation-src'
$gen     = Join-Path $root '.temp\triangulation-gen'
$cli     = Join-Path $root 'src\Plato.CLI'
$project = Join-Path $root 'tests\Plato.Triangulation.Tests\Plato.Triangulation.Tests.csproj'
$results = [System.Collections.Generic.List[object]]::new()

# The geometry files the triangulator's closure needs. Seeds only: step 2 adds whatever else the
# resolver asks for, so this list being incomplete costs a few seconds, not a red gate.
$geometrySeeds = @(
    'geometry.concepts', 'geometry.types', 'geometry.library',
    'planar.types', 'planar.library',
    'polygons.types', 'polygons.library',
    'triangulation.types', 'triangulation.library',
    'meshes.concepts', 'meshes.types', 'meshes.library',
    'topology.types', 'topology.concepts',
    'lines.types', 'lines.library', 'spatial-primitives.types',
    'curves.concepts', 'surfaces-solids.concepts', 'pointclouds-voxels.concepts',
    'voxels.types', 'pointclouds.types'
)

function Run-Step([string]$name, [scriptblock]$block) {
    $sw = [System.Diagnostics.Stopwatch]::StartNew()
    try { $ok = & $block } catch { Write-Host $_; $ok = $false }
    $sw.Stop()
    $result = $(if ($ok) { 'PASS' } else { 'FAIL' })
    Add-GateTiming -Gate $name -Result $result -Seconds $sw.Elapsed.TotalSeconds -Script 'regen-triangulation.ps1'
    $script:results.Add([pscustomobject]@{
        Step = $name; Result = $result; Seconds = [math]::Round($sw.Elapsed.TotalSeconds, 1)
    })
    return $ok
}

# The stdlib file declaring the named type or interface, or $null when nothing does.
function Find-Declaring([string]$name) {
    $pattern = "^\s*(unique\s+)?(primitive\s+)?(type|interface|concept)\s+$([regex]::Escape($name))\b"
    $hit = Get-ChildItem -Path (Join-Path $root 'stdlib') -Filter '*.plato' -Recurse -File |
        Where-Object { $_.FullName -notmatch '\\tests\\' } |
        Where-Object { (Get-Content $_.FullName -Raw) -match $pattern } |
        Select-Object -First 1
    if ($null -eq $hit) { return $null }
    return $hit
}

foreach ($dir in @($src, $gen)) {
    if (Test-Path $dir) { Remove-Item -Recurse -Force $dir }
    New-Item -ItemType Directory -Path $dir -Force | Out-Null
}

$staged = Run-Step 'stage (foundation + geometry closure)' {
    Copy-Item (Join-Path $root 'stdlib\foundation\*.plato') $src
    foreach ($stem in $geometrySeeds) {
        $file = Join-Path $root "stdlib\geometry\$stem.plato"
        if (Test-Path $file) { Copy-Item $file $src }
    }
    return (Get-ChildItem -Path $src -Filter '*.plato').Count -gt 0
}

$closed = $false
if ($staged) {
    $closed = Run-Step 'close (resolve the staged subset)' {
        for ($attempt = 1; $attempt -le 12; $attempt++) {
            $out = & dotnet run --project $cli -c Release -- lint $src 2>&1 | Out-String
            $missing = [regex]::Matches($out, 'Could not find type (\w+)') |
                ForEach-Object { $_.Groups[1].Value } | Sort-Object -Unique
            if ($missing.Count -eq 0) { return $true }

            $added = 0
            foreach ($name in $missing) {
                $file = Find-Declaring $name
                if ($null -eq $file) {
                    Write-Host "  no stdlib file declares '$name'"
                    continue
                }
                if (-not (Test-Path (Join-Path $src $file.Name))) {
                    Copy-Item $file.FullName $src
                    Write-Host "  + $($file.Name) (declares $name)"
                    $added++
                }
            }
            if ($added -eq 0) {
                Write-Host "  unresolved and nothing left to add: $($missing -join ', ')"
                return $false
            }
        }
        Write-Host '  closure did not converge in 12 attempts'
        return $false
    }
}

$generated = $false
if ($closed) {
    $generated = Run-Step 'generate (staged subset -> C#)' {
        & dotnet run --project $cli -c Release -- $src --out=$gen 2>&1 | Out-String | Out-Null
        if ($LASTEXITCODE -ne 0) { return $false }
        return (Get-ChildItem -Path $gen -Filter '*.g.cs').Count -gt 0
    }
}

if ($generated -and -not $SkipTest) {
    Run-Step 'test (tiling invariant)' {
        $out = & dotnet test $project -c Release --nologo 2>&1 | Out-String
        Write-Host $out
        return ($LASTEXITCODE -eq 0)
    } | Out-Null
}

Write-Host ''
$results | Format-Table -AutoSize
$failed = @($results | Where-Object { $_.Result -eq 'FAIL' }).Count
if ($failed -gt 0) {
    Write-Host 'TRIANGULATION GATE FAIL'
    exit 1
}
Write-Host 'TRIANGULATION GATE PASS'
exit 0
