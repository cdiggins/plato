# gate-timing.ps1 — shared duration log for every Plato/studio gate, test and check.
#
# Dot-source it, then either time individual gates (Add-GateTiming) or the whole script
# (Start-GateRun / Exit-GateRun). Every call appends one row to a CSV so gate COST is
# tracked over time, not just PASS/FAIL:
#
#     timestamp,machine,repo,script,gate,result,seconds,detail
#
# The log is MACHINE-GLOBAL by default (%LOCALAPPDATA%\ara3d\gate-timings.csv), not per
# checkout: gates run from worktrees, submodule copies and the staging area, and the point
# of the log is one aggregate answer to "where is the wall-clock going". Override with
# $env:PLATO_GATE_TIMING_LOG (a path), or disable with $env:PLATO_GATE_TIMING=0.
#
# Read it with: .\tools\gate-timings.ps1

# $PSCommandPath inside a function is the file the FUNCTION lives in (this helper), so the
# script name is taken from the call stack instead - otherwise every row says gate-timing.ps1.
function Get-CallerScriptName {
    $frame = @(Get-PSCallStack | Where-Object { $_.ScriptName -and $_.ScriptName -ne $PSCommandPath })[0]
    if ($frame) { return (Split-Path -Leaf $frame.ScriptName) }
    return 'interactive'
}

function Get-GateTimingLog {
    if ($env:PLATO_GATE_TIMING_LOG) { return $env:PLATO_GATE_TIMING_LOG }
    $dir = Join-Path $env:LOCALAPPDATA 'ara3d'
    return (Join-Path $dir 'gate-timings.csv')
}

function Add-GateTiming {
    param(
        [Parameter(Mandatory)][string]$Gate,
        [Parameter(Mandatory)][string]$Result,     # PASS | FAIL | SKIP
        [Parameter(Mandatory)][double]$Seconds,
        [string]$Script = (Get-CallerScriptName),
        [string]$Detail = ''
    )
    if ($env:PLATO_GATE_TIMING -eq '0') { return }

    $path = Get-GateTimingLog
    $dir = Split-Path -Parent $path
    if ($dir -and -not (Test-Path $dir)) { New-Item -ItemType Directory -Force $dir | Out-Null }

    # Repo = the checkout the gate ran from, so worktree runs stay distinguishable in one log.
    $repo = try { (git rev-parse --show-toplevel 2>$null) } catch { '' }
    if (-not $repo) { $repo = (Get-Location).Path }

    function Esc([string]$s) { '"' + (($s -replace '"', '""') -replace '[\r\n]+', ' ') + '"' }
    $row = @(
        Esc((Get-Date).ToString('s'))
        Esc($env:COMPUTERNAME)
        Esc($repo)
        Esc($Script)
        Esc($Gate)
        Esc($Result)
        [math]::Round($Seconds, 2)
        Esc($Detail)
    ) -join ','

    $header = 'timestamp,machine,repo,script,gate,result,seconds,detail'
    # Concurrent agents share this file; retry briefly rather than failing a gate over a log write.
    for ($i = 0; $i -lt 5; $i++) {
        try {
            if (-not (Test-Path $path)) { [System.IO.File]::WriteAllText($path, $header + "`r`n") }
            [System.IO.File]::AppendAllText($path, $row + "`r`n")
            return
        }
        catch { Start-Sleep -Milliseconds 60 }
    }
    Write-Host "  (gate timing not recorded: $path locked)" -ForegroundColor DarkGray
}

<# Whole-script timing. Usage:
     . "$PSScriptRoot\gate-timing.ps1"
     $run = Start-GateRun 'regen-generated'
     trap { Complete-GateRun $run 'FAIL'; throw }
     ...
     Exit-GateRun $run 0
#>
function Start-GateRun {
    param([Parameter(Mandatory)][string]$Name, [string]$Script = (Get-CallerScriptName))
    return [pscustomobject]@{
        Name      = $Name
        Script    = $Script
        Stopwatch = [System.Diagnostics.Stopwatch]::StartNew()
        Recorded  = $false
    }
}

function Complete-GateRun {
    param([Parameter(Mandatory)]$Run, [Parameter(Mandatory)][string]$Result, [string]$Detail = '')
    if (-not $Run -or $Run.Recorded) { return }
    $Run.Recorded = $true
    $Run.Stopwatch.Stop()
    $secs = $Run.Stopwatch.Elapsed.TotalSeconds
    Add-GateTiming -Gate $Run.Name -Result $Result -Seconds $secs -Script $Run.Script -Detail $Detail
    Write-Host ("[timing] {0}: {1} in {2:n1}s" -f $Run.Name, $Result, $secs) -ForegroundColor DarkGray
}

function Exit-GateRun {
    param([Parameter(Mandatory)]$Run, [int]$Code = 0, [string]$Detail = '')
    Complete-GateRun $Run $(if ($Code -eq 0) { 'PASS' } else { 'FAIL' }) $Detail
    exit $Code
}
