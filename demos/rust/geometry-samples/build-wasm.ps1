# Builds the crate for wasm32 and copies the module next to the demo page.
# Prerequisite: rustup target add wasm32-unknown-unknown
$root = Split-Path -Parent $MyInvocation.MyCommand.Path
Push-Location $root
try {
    cargo build --release --target wasm32-unknown-unknown
    if ($LASTEXITCODE -ne 0) { throw "cargo build failed" }
    Copy-Item "$root\target\wasm32-unknown-unknown\release\geometry_samples.wasm" "$root\web\geometry_samples.wasm" -Force
    Write-Host "web/geometry_samples.wasm updated"
} finally {
    Pop-Location
}
