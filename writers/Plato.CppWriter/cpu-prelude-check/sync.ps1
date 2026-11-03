# Checks the prelude sources in ../prelude: every fragment plato_cpu.hpp includes exists, and no .hpp there is left out
# (an orphan is never embedded into a program that needs it). The fragments are embedded as they are; nothing is
# generated from them, so there is no copy to rewrite. -Write adds the missing #include lines for orphans.
param([switch]$Write)
$here = Split-Path -Parent $MyInvocation.MyCommand.Path
$prelude = Join-Path $here "..\prelude"
$mainPath = Join-Path $prelude "plato_cpu.hpp"
$main = (Get-Content -Raw $mainPath) -replace "`r`n", "`n"
$included = [regex]::Matches($main, '(?m)^#include "(plato_[a-z]+\.hpp)"$') | ForEach-Object { $_.Groups[1].Value }
$files = Get-ChildItem $prelude -Filter *.hpp | ForEach-Object Name
$problems = @()
foreach ($i in $included) { if ($files -notcontains $i) { $problems += "plato_cpu.hpp includes $i, which does not exist" } }
$nested = ($files | Where-Object { $_ -notin @("plato_cpu.hpp", "plato_config.hpp") } | ForEach-Object { (Get-Content -Raw (Join-Path $prelude $_)) } ) -join "`n"
foreach ($f in $files) {
    if ($f -in @("plato_cpu.hpp", "plato_config.hpp") -or $included -contains $f) { continue }
    if ($nested -match [regex]::Escape("#include `"$f`"")) { continue }   # a fragment included by another one (plato_borrow.hpp)
    if ($Write) { $main = $main -replace '(?m)^\n\}\s*$', "#include `"$f`"`n`n}`n"; "added $f" } else { $problems += "$f is not included by plato_cpu.hpp" }
}
if ($Write) { [IO.File]::WriteAllText($mainPath, $main, (New-Object Text.UTF8Encoding $false)) }
if ($problems) { $problems | ForEach-Object { Write-Error $_ }; exit 1 }
"in sync: $($files.Count) files"
