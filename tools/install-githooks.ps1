<#
.SYNOPSIS
    Point this Plato clone's git hooks at tools/githooks (status-report pre-commit).

.EXAMPLE
    .\tools\install-githooks.ps1
#>
$ErrorActionPreference = 'Stop'
$PlatoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
Push-Location $PlatoRoot
try {
    $hooks = Join-Path $PlatoRoot 'tools\githooks'
    if (-not (Test-Path (Join-Path $hooks 'pre-commit'))) {
        Write-Error "Missing $hooks\pre-commit"
    }
    git config core.hooksPath tools/githooks
    Write-Host "core.hooksPath -> tools/githooks"
    Write-Host "Commits will refresh docs/status-report.html via tools/gen-status-report.py"
}
finally {
    Pop-Location
}
