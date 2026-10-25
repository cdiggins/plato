"""Explicitly invoked frozen-input census; no compiler project builds or timing."""
import hashlib
import importlib.util
import json
from pathlib import Path
import subprocess
import sys
from xml.sax.saxutils import escape

ROOT = Path(__file__).resolve().parents[4]
HERE = Path(__file__).resolve().parent
OUT = ROOT / '.temp/optimizer-geometry/grid-reuse'
SNAPSHOT = ROOT / '.temp/optimizer-geometry/compiler'

if sys.argv[1:] != ['--execute']:
    raise SystemExit('Use --execute only after a supervisor runtime grant; otherwise no work runs.')
manifest_path = SNAPSHOT / 'manifest.json'
manifest_bytes = manifest_path.read_bytes()
manifest = json.loads(manifest_bytes)

def digest(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()

inputs = {path: digest(path) for path in HERE.iterdir() if path.is_file()}

def verify():
    if manifest_path.read_bytes() != manifest_bytes:
        raise RuntimeError('Frozen manifest changed')
    for relative, expected in manifest['snapshot_hashes'].items():
        if digest(SNAPSHOT / relative) != expected:
            raise RuntimeError('Frozen snapshot changed: ' + relative)
    for relative, expected in manifest['source_hashes'].items():
        if digest(SNAPSHOT / 'source' / relative) != expected:
            raise RuntimeError('Frozen source mismatch: ' + relative)
    for path, expected in inputs.items():
        if digest(path) != expected:
            raise RuntimeError('Probe input changed: ' + str(path))

verify()
OUT.mkdir(parents=True, exist_ok=True)
(OUT / 'inputs.json').write_text(json.dumps({'revision': manifest['revision'],
    'manifest_sha256': hashlib.sha256(manifest_bytes).hexdigest(),
    'probe_hashes': {str(p.relative_to(ROOT)): h for p, h in inputs.items()}}, indent=2))
refs = ''.join(f'<Reference Include="{escape(p.stem)}"><HintPath>{escape(str(p))}</HintPath></Reference>'
               for p in SNAPSHOT.glob('*.dll'))
project = OUT / 'Probe.csproj'
project.write_text('<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType>'
    '<TargetFramework>net8.0</TargetFramework><ImplicitUsings>enable</ImplicitUsings>'
    '<EnableDefaultCompileItems>false</EnableDefaultCompileItems></PropertyGroup><ItemGroup>'
    f'<Compile Include="{escape(str(HERE / "Probe.cs"))}"/>' + refs + '</ItemGroup></Project>')
build = subprocess.run(['dotnet', 'build', str(project), '-c', 'Release', '--nologo'],
                       capture_output=True, text=True)
log = OUT / 'build.log'
log.write_text(build.stdout + build.stderr)
sys.dont_write_bytecode = True
spec = importlib.util.spec_from_file_location('recorder', ROOT / 'tools/record-csharp-build-errors.py')
recorder = importlib.util.module_from_spec(spec)
spec.loader.exec_module(recorder)
recorder.SNAPSHOT = OUT / 'build-status.json'
sys.argv = ['record', '--target', 'geometry-grid-reuse', '--log', str(log), '--build-exit', str(build.returncode)]
recorder.main()
verify()
if build.returncode:
    raise SystemExit(build.returncode)
run = subprocess.run(['dotnet', str(OUT / 'bin/Release/net8.0/Probe.dll'),
    str(SNAPSHOT / 'source/stdlib'), str(HERE / 'fixtures.plato'), str(OUT)], capture_output=True, text=True)
(OUT / 'results.txt').write_text(run.stdout + run.stderr)
verify()
print(run.stdout + run.stderr)
raise SystemExit(run.returncode)
