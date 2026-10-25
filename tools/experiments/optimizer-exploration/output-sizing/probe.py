"""Inspect real TIR using existing compiled assemblies; never rebuild shared projects."""
import hashlib
import importlib.util
import pathlib
import subprocess
import sys

ROOT = pathlib.Path(__file__).resolve().parents[4]
HERE = pathlib.Path(__file__).resolve().parent
OUT = ROOT / '.temp/optimizer-exploration/output-sizing/tir-probe'
OUT.mkdir(parents=True, exist_ok=True)
assemblies = ROOT / 'src/Plato.CLI/bin/Release/net8.0'
references = ''.join(f'<Reference Include="{p.stem}"><HintPath>{p}</HintPath></Reference>' for p in assemblies.glob('*.dll'))
(OUT / 'Program.cs').write_text((HERE / 'TirProbe.cs').read_text(), encoding='utf-8')
project = OUT / 'TirProbe.csproj'
project.write_text('<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net8.0</TargetFramework><ImplicitUsings>enable</ImplicitUsings></PropertyGroup><ItemGroup>' + references + '</ItemGroup></Project>', encoding='utf-8')
(OUT / 'fingerprints.txt').write_text('\n'.join(f'{p.name} {hashlib.sha256(p.read_bytes()).hexdigest()}' for p in assemblies.glob('Plato.*.dll')), encoding='utf-8')
build = subprocess.run(['dotnet', 'build', str(project), '-c', 'Release', '--nologo'], cwd=OUT, capture_output=True, text=True)
log = OUT / 'build.log'
log.write_text(build.stdout + build.stderr, encoding='utf-8')
spec = importlib.util.spec_from_file_location('recorder', ROOT / 'tools/record-csharp-build-errors.py')
recorder = importlib.util.module_from_spec(spec)
sys.dont_write_bytecode = True
spec.loader.exec_module(recorder)
recorder.SNAPSHOT = OUT / 'build-status.json'
sys.argv = ['recorder', '--target', 'output-sizing-tir', '--log', str(log), '--build-exit', str(build.returncode)]
recorder.main()
if build.returncode:
    print(build.stdout + build.stderr)
    raise SystemExit(build.returncode)
run = subprocess.run(['dotnet', str(OUT / 'bin/Release/net8.0/TirProbe.dll'), str(ROOT)], cwd=OUT, capture_output=True, text=True)
(OUT / 'results.txt').write_text(run.stdout + run.stderr, encoding='utf-8')
print(run.stdout + run.stderr)
raise SystemExit(run.returncode)
