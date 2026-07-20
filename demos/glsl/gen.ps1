# Generates one plato.glsl per demo from src/_core.plato + src/<demo>.plato.
# Run from anywhere; paths are anchored to this script.
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$cli  = Join-Path $root '..\..\Plato.CLI'
$src  = Join-Path $root 'src'
$out  = Join-Path $root 'out'
$tmp  = Join-Path $root '.build'
New-Item -ItemType Directory -Force $out | Out-Null

$demos = 'sdf', 'mandelbrot', 'bezier', 'polygon', 'solids', 'voxel', 'ripples', 'tube'
foreach ($d in $demos) {
    $dir = Join-Path $tmp $d
    New-Item -ItemType Directory -Force $dir | Out-Null
    Get-ChildItem $dir -Filter *.plato | Remove-Item -Force
    Copy-Item (Join-Path $src '_core.plato') (Join-Path $dir '_core.plato') -Force
    Copy-Item (Join-Path $src "$d.plato")    (Join-Path $dir "$d.plato")    -Force
    dotnet run --project $cli -c Release --no-build -- $dir $dir --glsl 2>$null |
        Select-String 'GLSL functions' | ForEach-Object { "$d : $_" }
    Copy-Item (Join-Path $dir 'plato.glsl') (Join-Path $out "$d.glsl") -Force
}
Remove-Item -Recurse -Force $tmp
"Done. Output in $out"
