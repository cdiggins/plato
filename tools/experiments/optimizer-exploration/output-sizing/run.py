"""Run an isolated generated-shape experiment; never rebuild shared projects."""
import hashlib
import importlib.util
import os
import pathlib
import subprocess
import sys

ROOT = pathlib.Path(__file__).resolve().parents[4]
HERE = pathlib.Path(__file__).resolve().parent
OUT = ROOT / '.temp/optimizer-exploration/output-sizing'
OUT.mkdir(parents=True, exist_ok=True)
runtime = ROOT / 'src/Plato.Intrinsics/ArrayIntrinsics.cs'
source = runtime.read_text(encoding='utf-8')
start = source.index('        public static IReadOnlyList<TResult> FlatMap')
body = source[start:source.index('\n    }', start)]
(OUT / 'Baseline.cs').write_text('namespace OutputSizing;\npublic static class Baseline\n{\n' + body + '\n}\n', encoding='utf-8')
(OUT / 'OutputSizing.cs').write_text((HERE / 'OutputSizing.cs').read_text(encoding='utf-8'), encoding='utf-8')
project = OUT / 'OutputSizing.csproj'
project.write_text('<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net8.0</TargetFramework><ImplicitUsings>enable</ImplicitUsings><EnableNETAnalyzers>false</EnableNETAnalyzers></PropertyGroup></Project>', encoding='utf-8')
fingerprints = {str(p.relative_to(ROOT)): hashlib.sha256(p.read_bytes()).hexdigest() for p in [runtime, HERE / 'OutputSizing.cs', HERE / 'run.py']}
(OUT / 'fingerprints.txt').write_text(str(fingerprints), encoding='utf-8')
build = subprocess.run(['dotnet', 'build', str(project), '-c', 'Release', '--nologo'], cwd=OUT, capture_output=True, text=True)
log = OUT / 'build.log'
log.write_text(build.stdout + build.stderr, encoding='utf-8')
spec = importlib.util.spec_from_file_location('recorder', ROOT / 'tools/record-csharp-build-errors.py')
recorder = importlib.util.module_from_spec(spec)
sys.dont_write_bytecode = True
spec.loader.exec_module(recorder)
recorder.SNAPSHOT = OUT / 'build-status.json'
sys.argv = ['recorder', '--target', 'output-sizing', '--log', str(log), '--build-exit', str(build.returncode)]
recorder.main()
if build.returncode:
    print(build.stdout + build.stderr)
    raise SystemExit(build.returncode)
environment = dict(os.environ, DOTNET_TieredCompilation='0')
run = subprocess.run(['dotnet', str(OUT / 'bin/Release/net8.0/OutputSizing.dll')], cwd=OUT, env=environment, capture_output=True, text=True)
(OUT / 'results.txt').write_text(run.stdout + run.stderr, encoding='utf-8')
print(run.stdout + run.stderr)
raise SystemExit(run.returncode)
