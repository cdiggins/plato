# Export committed inputs into an isolated experiment directory. Never edits dependencies.
param(
    [string]$BrepRoot = 'C:/Users/cdigg/git/plato-brep',
    [string]$PlatoRoot = (Split-Path -Parent (Split-Path -Parent $PSScriptRoot)),
    [string]$Work,
    [string]$PlatoCommit = '763034a67612fb1ada95891434cba8fc2ecf5707',
    [string]$BrepCommit = '99c8fdee6be8d3fc92f45401321d442e15375316'
)
$ErrorActionPreference = 'Stop'
if (-not $Work) { $Work = Join-Path $PlatoRoot '.temp/inline-brep' }
$Work = [IO.Path]::GetFullPath($Work)
if (Test-Path $Work) { throw 'Use a fresh work directory; preparation never overwrites a previous experiment.' }
New-Item -ItemType Directory -Path $Work | Out-Null
function Export-Input($Source, $Commit, $Destination, $Paths = @()) {
    New-Item -ItemType Directory -Force $Destination | Out-Null
    & git -C $Source archive --format=tar "--output=$Destination.tar" $Commit @Paths
    if ($LASTEXITCODE) { throw "Cannot export $Source at $Commit" }
    & tar -xf "$Destination.tar" -C $Destination
    if ($LASTEXITCODE) { throw "Cannot extract $Destination.tar" }
}
$brep = Join-Path $Work 'brep'
Export-Input $BrepRoot $BrepCommit $brep
$pins = Get-Content (Join-Path $brep 'deps.json') -Raw | ConvertFrom-Json
Export-Input (Join-Path $BrepRoot 'deps/bim-open-data') $pins.'bim-open-data'.commit (Join-Path $brep 'deps/bim-open-data') @('Directory.Build.props', 'Directory.Build.targets', 'src/data/Ara3D.MeshKernel')
Set-Content (Join-Path $brep 'deps/.deps-root') ''
$feed = Join-Path $BrepRoot "deps/ara3d-sdk-packages/$($pins.'ara3d-sdk'.commit)"
if (-not (Test-Path $feed)) { throw "The pinned SDK package feed is required: $feed" }
New-Item -ItemType Directory -Force (Join-Path $brep 'deps/ara3d-sdk-packages') | Out-Null
Copy-Item -LiteralPath $feed -Destination (Join-Path $brep 'deps/ara3d-sdk-packages') -Recurse
$compiler = & (Join-Path $brep 'tools/regen-compiler.ps1') -PlatoRoot $PlatoRoot -PlatoCommit $PlatoCommit -SdkRoot (Join-Path $BrepRoot 'deps/ara3d-sdk') -SdkCommit $pins.'ara3d-sdk'.commit -CacheRoot (Join-Path $Work 'compiler-cache')
$compiler | ConvertTo-Json -Depth 8 | Set-Content (Join-Path $Work 'compiler.json') -Encoding UTF8
$packageHashes = @{}
Get-ChildItem -LiteralPath $feed -Filter '*.nupkg' | ForEach-Object { $packageHashes[$_.Name] = (Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash }
@{ plato = $PlatoCommit; brep = $BrepCommit; meshkernel = $pins.'bim-open-data'.commit; compiler = $compiler.Identity; packageHashes = $packageHashes } | ConvertTo-Json -Depth 8 | Set-Content (Join-Path $Work 'inputs.json') -Encoding UTF8
