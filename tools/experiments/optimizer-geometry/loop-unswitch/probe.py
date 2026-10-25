"""Extract unchanged geometry helpers and inspect TIR against the supervisor's frozen DLLs."""
import argparse
import hashlib
import importlib.util
import json
import pathlib
import re
import subprocess
import sys

ROOT = pathlib.Path(__file__).resolve().parents[4]
HERE = pathlib.Path(__file__).resolve().parent
OUT = ROOT / '.temp/optimizer-geometry/loop-unswitch'
SNAPSHOT = ROOT / '.temp/optimizer-geometry/compiler'
MANIFEST = SNAPSHOT / 'manifest.json'

def validate_snapshot():
    if not MANIFEST.is_file():
        raise RuntimeError('Supervisor frozen compiler manifest is not ready')
    manifest_bytes = MANIFEST.read_bytes()
    manifest = json.loads(manifest_bytes)
    for collection, prefix in [('snapshot_hashes', ''), ('source_hashes', 'source')]:
        for name, expected in manifest[collection].items():
            path = SNAPSHOT / prefix / name
            if not path.is_file() or hashlib.sha256(path.read_bytes()).hexdigest() != expected:
                raise RuntimeError('Frozen snapshot validation failed: ' + str(path))
    return manifest_bytes

parser = argparse.ArgumentParser()
parser.add_argument('--execute', action='store_true', help='Build/run only during a granted resource window')
parser.add_argument('--qualify', action='store_true', help='Focused identity proof and private generated off/on correctness; no timing')
parser.add_argument('--timing', action='store_true', help='Requires an explicit separate serialized timing grant')
args = parser.parse_args()
if args.timing and not (args.execute and args.qualify):
    parser.error('--timing requires --execute --qualify')
OUT.mkdir(parents=True, exist_ok=True)
manifest_bytes = validate_snapshot()
(OUT / 'snapshot-manifest.json').write_bytes(manifest_bytes)
helpers = []
stdlib = SNAPSHOT / 'source/stdlib'
if not (stdlib / 'foundation').is_dir():
    raise RuntimeError('Supervisor frozen source snapshot is not ready')
inputs = list((stdlib / 'foundation').glob('*.plato')) + list(HERE.glob('*'))
for file, name in [('splines.library.plato', 'AtWrapped'), ('meshes.library.plato', 'GridParameter'),
                   ('triangulation.library.plato', 'RingSlot')]:
    path = stdlib / 'geometry' / file
    inputs.append(path)
    text = path.read_text(encoding='utf-8')
    match = re.search(r'^    ' + name + r'\([^;]+?;', text, re.MULTILINE)
    if not match:
        raise RuntimeError('Exact helper extraction failed: ' + name)
    helpers.append(match.group(0))
(OUT / 'helpers.plato').write_text('library GeometrySourceHelpers\n{\n' + '\n'.join(helpers) + '\n}\n', encoding='utf-8')
inputs = [p for p in inputs if p.is_file()]
fingerprints = {str(p): hashlib.sha256(p.read_bytes()).hexdigest() for p in inputs}
(OUT / 'source-fingerprints.txt').write_text('\n'.join(f'{v} {p}' for p, v in fingerprints.items()), encoding='utf-8')
if not args.execute:
    if validate_snapshot() != manifest_bytes:
        raise RuntimeError('Manifest changed during preparation')
    print('Prepared exact-source helpers; no build or runtime executed.')
    raise SystemExit(0)
dlls = list(SNAPSHOT.glob('*.dll'))
if not any(p.name == 'Plato.CSharpWriter.dll' for p in dlls):
    raise RuntimeError('Supervisor frozen compiler snapshot is not ready')
hashes = {str(p): hashlib.sha256(p.read_bytes()).hexdigest() for p in dlls}
(OUT / 'assembly-fingerprints.txt').write_text('\n'.join(f'{v} {p}' for p, v in hashes.items()), encoding='utf-8')
refs = ''.join(f'<Reference Include="{p.stem}"><HintPath>{p}</HintPath>'
    + ('<Aliases>legacy</Aliases>' if p.name == 'Ara3D.Geometry.dll' else '') + '</Reference>' for p in dlls)
project = OUT / 'Probe.csproj'
project.write_text('<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType>'
    '<TargetFramework>net8.0</TargetFramework><ImplicitUsings>enable</ImplicitUsings>'
    '<EnableDefaultCompileItems>false</EnableDefaultCompileItems></PropertyGroup><ItemGroup>'
    f'<Compile Include="{HERE / "Probe.cs"}"/><Compile Include="{HERE / "Qualification.cs"}"/><Compile Include="{HERE / "Timing.cs"}"/>' + refs + '</ItemGroup></Project>', encoding='utf-8')
build = subprocess.run(['dotnet', 'build', str(project), '-c', 'Release', '--nologo'], capture_output=True, text=True)
log = OUT / 'build.log'
log.write_text(build.stdout + build.stderr, encoding='utf-8')
spec = importlib.util.spec_from_file_location('recorder', ROOT / 'tools/record-csharp-build-errors.py')
recorder = importlib.util.module_from_spec(spec)
sys.dont_write_bytecode = True
spec.loader.exec_module(recorder)
recorder.SNAPSHOT = OUT / 'build-status.json'
sys.argv = ['record', '--target', 'geometry-unswitch', '--log', str(log), '--build-exit', str(build.returncode)]
recorder.main()
if build.returncode:
    print(build.stdout + build.stderr)
    raise SystemExit(build.returncode)
copied = OUT / 'bin/Release/net8.0'
for path, value in hashes.items():
    copy = copied / pathlib.Path(path).name
    if not copy.is_file() or hashlib.sha256(copy.read_bytes()).hexdigest() != value:
        raise RuntimeError('Snapshot dependency was not copied identically: ' + str(copy))
run = subprocess.run(['dotnet', str(copied / 'Probe.dll'), str(ROOT), str(OUT), str(SNAPSHOT)]
    + (['--qualify'] if args.qualify else []) + (['--timing'] if args.timing else []), capture_output=True, text=True)
(OUT / ('results-qualification.txt' if args.qualify else 'results.txt')).write_text(run.stdout + run.stderr, encoding='utf-8')
if validate_snapshot() != manifest_bytes:
    raise RuntimeError('Manifest changed during probe')
for path, value in fingerprints.items() | hashes.items():
    if hashlib.sha256(pathlib.Path(path).read_bytes()).hexdigest() != value:
        raise RuntimeError('Input changed during probe: ' + path)
if run.returncode == 0:
    loaded = json.loads((OUT / 'loaded-assemblies.json').read_text(encoding='utf-8'))
    expected = {pathlib.Path(path).name: value for path, value in hashes.items()}
    for assembly in loaded:
        path = pathlib.Path(assembly['Path'])
        if path.name in expected and (assembly['Sha256'] != expected[path.name]
                or path.parent.resolve() != copied.resolve()):
            raise RuntimeError('Loaded dependency differs from frozen copy: ' + str(path))
    for path, value in hashes.items():
        copy = copied / pathlib.Path(path).name
        if hashlib.sha256(copy.read_bytes()).hexdigest() != value:
            raise RuntimeError('Copied dependency changed during probe: ' + str(copy))
print(run.stdout + run.stderr)
raise SystemExit(run.returncode)
