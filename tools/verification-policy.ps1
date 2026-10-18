# verification-policy.ps1 — dot-sourced reader for stdlib\verification.json, the one file that
# says which stdlib tiers each check covers, the ratchet ceilings and the codegen recipe
# (plato-388). Every PowerShell gate reads the policy through this file instead of carrying a
# tier list of its own; stdlib\VERIFICATION.md explains the policy.
#
#   . (Join-Path $PSScriptRoot 'verification-policy.ps1')
#   $policy = Get-VerificationPolicy                    # this checkout
#   $policy = Get-VerificationPolicy -PlatoRoot <dir>   # another Plato checkout (studio)
#   Get-PolicyTierFolders $policy 'lint'                # absolute tier folders the lint check covers
#   Get-PolicyTierFolders $policy 'lint' -IncludeAll    # ... plus every tier it exempts
#
# Needs no compiler: it reads JSON only, so stage-stdlib.ps1 keeps working while trunk is broken.
# Keep this file ASCII-only (no BOM; PowerShell 5.1 reads it as ANSI).

function Get-VerificationPolicy([string]$PlatoRoot) {
    if (-not $PlatoRoot) { $PlatoRoot = Split-Path -Parent $PSScriptRoot }
    $path = Join-Path $PlatoRoot 'stdlib\verification.json'
    if (-not (Test-Path $path)) { throw "Verification policy not found: $path" }
    $policy = Get-Content $path -Raw -Encoding UTF8 | ConvertFrom-Json
    $policy | Add-Member -NotePropertyName PlatoRoot -NotePropertyValue $PlatoRoot -Force
    $policy
}

# The tiers a check covers, as a list of names. -IncludeAll appends the tiers the check exempts
# (the -IncludeFuture switch of the gates), keeping dependency order.
function Get-PolicyTiers($policy, [string]$check, [switch]$IncludeAll) {
    $covered = @($policy.checks.$check)
    if ($null -eq $policy.checks.$check) { throw "No check named '$check' in stdlib\verification.json" }
    if ($IncludeAll) { @($policy.tiers) } else { $covered }
}

# The same tiers as absolute folders under <PlatoRoot>\<root>. Plato.CLI enumerates each root
# top-directory-only, so gates name the tier folders rather than stdlib\ itself.
function Get-PolicyTierFolders($policy, [string]$check, [switch]$IncludeAll) {
    $base = Join-Path $policy.PlatoRoot $policy.root
    @(Get-PolicyTiers $policy $check -IncludeAll:$IncludeAll | ForEach-Object { Join-Path $base $_ })
}
