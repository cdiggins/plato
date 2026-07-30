# gate-timings.ps1 — report the recorded cost of gates, tests and checks.
#
# Reads the log written by tools\gate-timing.ps1 (%LOCALAPPDATA%\ara3d\gate-timings.csv,
# or $env:PLATO_GATE_TIMING_LOG) and answers: which gates eat the wall clock, how slow is
# a typical run, and is a gate getting slower.
#
# Usage:
#   .\tools\gate-timings.ps1                 # last 30 days, per-gate summary
#   .\tools\gate-timings.ps1 -Days 7
#   .\tools\gate-timings.ps1 -Gate lint      # only gates whose name matches
#   .\tools\gate-timings.ps1 -Tail 20        # last 20 raw runs, newest first
#   .\tools\gate-timings.ps1 -Failures       # only FAIL rows (failed runs cost time too)
param(
    [int]$Days = 30,
    [string]$Gate,
    [int]$Tail = 0,
    [switch]$Failures,
    [string]$Path
)

$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'gate-timing.ps1')

if (-not $Path) { $Path = Get-GateTimingLog }
if (-not (Test-Path $Path)) {
    Write-Host "No timing log at $Path - run a gate first (they record automatically)." -ForegroundColor Yellow
    exit 0
}

$since = (Get-Date).AddDays(-$Days)
$rows = @(Import-Csv $Path |
    Where-Object { [datetime]$_.timestamp -ge $since } |
    Where-Object { -not $Gate -or $_.gate -like "*$Gate*" -or $_.script -like "*$Gate*" } |
    Where-Object { -not $Failures -or $_.result -eq 'FAIL' })

if ($rows.Count -eq 0) {
    Write-Host "No matching runs in the last $Days day(s)." -ForegroundColor Yellow
    exit 0
}

function Pct([double[]]$values, [double]$p) {
    $sorted = @($values | Sort-Object)
    $idx = [math]::Min($sorted.Count - 1, [math]::Max(0, [int][math]::Ceiling($p * $sorted.Count) - 1))
    return $sorted[$idx]
}

if ($Tail -gt 0) {
    $rows |
        Sort-Object { [datetime]$_.timestamp } -Descending |
        Select-Object -First $Tail |
        Select-Object @{n='when';e={([datetime]$_.timestamp).ToString('MM-dd HH:mm')}},
                      @{n='gate';e={$_.gate}},
                      @{n='result';e={$_.result}},
                      @{n='secs';e={[math]::Round([double]$_.seconds,1)}},
                      @{n='script';e={$_.script}} |
        Format-Table -AutoSize | Out-String | Write-Host
    exit 0
}

$summary = $rows | Group-Object gate | ForEach-Object {
    $secs = @($_.Group | ForEach-Object { [double]$_.seconds })
    $ordered = @($_.Group | Sort-Object { [datetime]$_.timestamp })
    $last = [double]$ordered[-1].seconds
    $median = Pct $secs 0.5
    # Trend: newest run against the median of everything before it. Blank until 3 runs exist.
    $trend = ''
    if ($ordered.Count -ge 3) {
        $prior = Pct @($ordered[0..($ordered.Count - 2)] | ForEach-Object { [double]$_.seconds }) 0.5
        if ($prior -gt 0) {
            $delta = ($last - $prior) / $prior
            if ([math]::Abs($delta) -ge 0.15) { $trend = '{0}{1:p0}' -f $(if ($delta -gt 0) { '+' } else { '' }), $delta }
        }
    }
    [pscustomobject]@{
        Gate      = $_.Name
        Runs      = $_.Count
        Fails     = @($_.Group | Where-Object result -eq 'FAIL').Count
        LastSec   = [math]::Round($last, 1)
        MedianSec = [math]::Round($median, 1)
        P90Sec    = [math]::Round((Pct $secs 0.9), 1)
        MaxSec    = [math]::Round(($secs | Measure-Object -Maximum).Maximum, 1)
        TotalMin  = [math]::Round((($secs | Measure-Object -Sum).Sum) / 60, 1)
        Trend     = $trend
    }
}

# Sorted by total time: the ranking that says where the hours actually went.
$summary | Sort-Object TotalMin -Descending | Format-Table -AutoSize | Out-String | Write-Host

$totalMin = [math]::Round((($rows | ForEach-Object { [double]$_.seconds } | Measure-Object -Sum).Sum) / 60, 1)
$ordered = @($rows | Sort-Object { [datetime]$_.timestamp })
$span = ([datetime]$ordered[-1].timestamp) - ([datetime]$ordered[0].timestamp)
Write-Host ("{0} runs, {1} gates, {2} min total over {3:n1} day(s). Log: {4}" -f $rows.Count, @($summary).Count, $totalMin, $span.TotalDays, $Path)
exit 0
