"""Generate/qualify actual Plato geometry kernels against the supervisor's frozen snapshot.

Timing is a separate explicitly selected phase requiring the supervisor's timing reservation.
"""
import argparse
import hashlib
import importlib.util
import json
import os
import pathlib
import subprocess
import sys
import time

ROOT = pathlib.Path(__file__).resolve().parents[4]
HERE = pathlib.Path(__file__).resolve().parent
OUT = ROOT / '.temp/optimizer-geometry/exact-output'
parser = argparse.ArgumentParser()
parser.add_argument('phase', choices=['census', 'correctness', 'timing'])
parser.add_argument('--snapshot', type=pathlib.Path, default=ROOT / '.temp/optimizer-geometry/compiler')
parser.add_argument('--no-build', action='store_true')
args = parser.parse_args()
snapshot = args.snapshot.resolve()
manifest_path = snapshot / 'manifest.json'
if not (snapshot / 'GeometryRuntime.dll').is_file() or not manifest_path.is_file():
    raise SystemExit('Frozen snapshot with GeometryRuntime.dll required; no working-tree fallback is allowed.')
OUT.mkdir(parents=True, exist_ok=True)
build_dir = OUT / 'harness'
build_dir.mkdir(exist_ok=True)

def digest(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()

def validate_snapshot():
    manifest = json.loads(manifest_path.read_text(encoding='utf-8-sig'))
    for relative, expected in manifest['snapshot_hashes'].items():
        path = snapshot / relative
        if not path.is_file() or digest(path) != expected:
            raise SystemExit(f'Frozen snapshot mismatch: {relative}')
    for relative, expected in manifest['source_hashes'].items():
        path = snapshot / 'source' / relative
        if not path.is_file() or digest(path) != expected:
            raise SystemExit(f'Frozen source mismatch: {relative}')
    return manifest

manifest = validate_snapshot()
input_paths = [manifest_path, *sorted(snapshot.glob('*.dll')),
               *[HERE / name for name in ['Program.cs', 'GeneratedHarness.cs', 'kernels.plato', 'run.py']]]
input_paths += sorted((snapshot / 'source/stdlib').rglob('*.plato'))
fingerprints = {str(p.relative_to(ROOT)): digest(p) for p in input_paths}
(OUT / f'fingerprints-{args.phase}.json').write_text(json.dumps(fingerprints, indent=2), encoding='utf-8')

spec = importlib.util.spec_from_file_location('recorder', ROOT / 'tools/record-csharp-build-errors.py')
recorder = importlib.util.module_from_spec(spec)
sys.dont_write_bytecode = True
spec.loader.exec_module(recorder)
recorder.SNAPSHOT = OUT / 'build-status.json'

def record(target, log, code):
    sys.argv = ['recorder', '--target', target, '--log', str(log), '--build-exit', str(code)]
    recorder.main()

binary = build_dir / 'bin/Release/net8.0/GeometryExact.dll'
binding_path = build_dir / 'build-binding.json'

def verify_copied_references():
    copied = {}
    for original in snapshot.glob('*.dll'):
        if original.name == 'Ara3D.Geometry.dll':
            continue
        path = binary.parent / original.name
        if not path.is_file() or digest(path) != digest(original):
            raise SystemExit(f'Copied snapshot assembly mismatch: {original.name}')
        copied[original.name] = digest(path)
    return copied

if not args.no_build:
    references = ''.join(f'<Reference Include="{p.stem}"><HintPath>{p}</HintPath></Reference>'
                         for p in snapshot.glob('*.dll') if p.name != 'Ara3D.Geometry.dll')
    (build_dir / 'Program.cs').write_bytes((HERE / 'Program.cs').read_bytes())
    project = build_dir / 'GeometryExact.csproj'
    project.write_text('<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType>'
                      '<TargetFramework>net8.0</TargetFramework><ImplicitUsings>enable</ImplicitUsings>'
                      '</PropertyGroup><ItemGroup>' + references + '</ItemGroup></Project>', encoding='utf-8')
    result = subprocess.run(['dotnet', 'build', str(project), '-c', 'Release', '--nologo'], cwd=build_dir, capture_output=True, text=True)
    log = OUT / 'harness-build.log'
    log.write_text(result.stdout + result.stderr, encoding='utf-8')
    record('geometry-exact-harness', log, result.returncode)
    if result.returncode:
        print(result.stdout + result.stderr)
        raise SystemExit(result.returncode)
    binding_path.write_text(json.dumps({'inputs': fingerprints, 'binary_sha256': digest(binary),
                                        'copied_references': verify_copied_references()}, indent=2), encoding='utf-8')
else:
    binding = json.loads(binding_path.read_text())
    if binding['inputs'] != fingerprints or binding['binary_sha256'] != digest(binary) or digest(build_dir / 'Program.cs') != digest(HERE / 'Program.cs'):
        raise SystemExit('Harness or frozen input changed: rebuild before running.')
    if binding['copied_references'] != verify_copied_references():
        raise SystemExit('Copied references changed since build.')

start = time.perf_counter()
result = subprocess.run(['dotnet', str(binary), str(ROOT), str(snapshot), str(OUT), args.phase],
                        cwd=build_dir, env=dict(os.environ, DOTNET_TieredCompilation='0'), capture_output=True, text=True)
elapsed = time.perf_counter() - start
validate_snapshot()
verify_copied_references()
if fingerprints != {str(p.relative_to(ROOT)): digest(p) for p in input_paths}:
    raise SystemExit('Input changed during phase; results are invalid.')
loaded_path = OUT / f'loaded-assemblies-{args.phase}.json'
loaded = json.loads(loaded_path.read_text()) if loaded_path.exists() else []
snapshot_dlls = {p.name.lower(): digest(p) for p in snapshot.glob('*.dll')}
for assembly in loaded:
    name = assembly['name'].lower()
    if name in snapshot_dlls:
        if assembly['sha256'] != snapshot_dlls[name] or pathlib.Path(assembly['location']).resolve().parent != binary.parent.resolve():
            raise SystemExit(f'Unexpected loaded compiler/runtime assembly: {assembly}')
(OUT / f'{args.phase}.log').write_text(result.stdout + result.stderr, encoding='utf-8')
(OUT / f'{args.phase}-status.json').write_text(json.dumps({'exit_code': result.returncode, 'wall_seconds': elapsed}), encoding='utf-8')
for enabled in ['False', 'True']:
    log = OUT / f'generated-build-{enabled}.log'
    if log.exists() and args.phase != 'census':
        failed = 'error CS' in log.read_text()
        record(f'geometry-exact-generated-{enabled}', log, int(failed))
print(result.stdout + result.stderr)
print(f'Phase {args.phase}: exit={result.returncode} wall_seconds={elapsed:.3f}')
raise SystemExit(result.returncode)
