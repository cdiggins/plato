# Fails when the C# literal in CppCpuPrelude.cs differs from plato_cpu.hpp. With -Write, rewrites the .cs from the .hpp.
param([switch]$Write)
$here = Split-Path -Parent $MyInvocation.MyCommand.Path
$hpp = (Get-Content -Raw "$here\plato_cpu.hpp") -replace "`r`n", "`n"
$csPath = Join-Path $here "..\CppCpuPrelude.cs"
if ($Write) {
    $q = '"' * 3
    $cs = "// The CPU (cpu64i32) prelude: plato_cpu.hpp, written verbatim by CppWriter in dialect Cpu.`n// Generated from cpu-prelude-check/plato_cpu.hpp by cpu-prelude-check/sync.ps1 -Write; edit the .hpp, not this file.`nnamespace Ara3D.Geometry.CppWriter`n{`n    public static class CppCpuPrelude`n    {`n        public static string Text => $q`n" + $hpp.TrimEnd("`n") + "`n$q;`n    }`n}`n"
    [IO.File]::WriteAllText($csPath, $cs, (New-Object Text.UTF8Encoding $false))
}
$text = [IO.File]::ReadAllText($csPath) -replace "`r`n", "`n"
$m = [regex]::Match($text, '(?s)Text => """\n(.*)\n""";')
if (-not $m.Success -or $m.Groups[1].Value -ne $hpp.TrimEnd("`n")) { Write-Error "CppCpuPrelude.cs is out of sync with plato_cpu.hpp"; exit 1 }
"in sync"
