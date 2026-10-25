"""Generated TIR projection experiment; execution requires the supervisor's runtime window."""
import argparse
import hashlib
import importlib.util
import json
from pathlib import Path
import subprocess
import sys
from xml.sax.saxutils import escape

ROOT = Path(__file__).resolve().parents[4]
HERE = Path(__file__).resolve().parent
OUT = ROOT / '.temp/optimizer-geometry/record-elimination'
SNAPSHOT = ROOT / '.temp/optimizer-geometry/compiler'


def digest(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def validate():
    manifest = json.loads((SNAPSHOT / 'manifest.json').read_text(encoding='utf-8-sig'))
    for name, expected in manifest['snapshot_hashes'].items():
        if digest(SNAPSHOT / name) != expected:
            raise RuntimeError('Frozen input mismatch: ' + name)
    for name, expected in manifest['source_hashes'].items():
        if name.replace('\\', '/').startswith('stdlib/') and digest(SNAPSHOT / 'source' / name) != expected:
            raise RuntimeError('Frozen source mismatch: ' + name)


if __name__ == '__main__':
    parser = argparse.ArgumentParser()
    parser.add_argument('--execute', action='store_true')
    parser.add_argument('--mode', choices=('verify', 'benchmark', 'collision'), default='verify')
    args = parser.parse_args()
    if not args.execute:
        raise SystemExit('Source preparation only. Pass --execute in a granted runtime window.')
    validate()
    OUT.mkdir(parents=True, exist_ok=True)
    fingerprints = {str(p): digest(p) for p in HERE.iterdir() if p.is_file()}
    fingerprints[str(SNAPSHOT / 'manifest.json')] = digest(SNAPSHOT / 'manifest.json')
    (OUT / 'input-fingerprints.json').write_text(json.dumps(fingerprints, indent=2), encoding='utf-8')
    references = ''.join(f'<Reference Include="{escape(p.stem)}"><HintPath>{escape(str(p))}</HintPath></Reference>'
                         for p in SNAPSHOT.glob('*.dll') if p.name != 'Ara3D.Geometry.dll')
    sources = ''.join(f'<Compile Include="{escape(str(HERE / name))}"/>'
                      for name in ('Program.cs', 'Projection.cs', 'Qualification.cs', 'Collision.cs'))
    project = OUT / 'RecordGeometry.csproj'
    project.write_text('<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType>'
                      '<TargetFramework>net8.0</TargetFramework><ImplicitUsings>enable</ImplicitUsings>'
                      '<EnableDefaultCompileItems>false</EnableDefaultCompileItems></PropertyGroup><ItemGroup>'
                      + sources + references + '</ItemGroup></Project>', encoding='utf-8')
    result = subprocess.run(['dotnet', 'build', str(project), '-c', 'Release', '--nologo'],
                            cwd=OUT, capture_output=True, text=True)
    log = OUT / 'build.log'
    log.write_text(result.stdout + result.stderr, encoding='utf-8')
    sys.dont_write_bytecode = True
    spec = importlib.util.spec_from_file_location('recorder', ROOT / 'tools/record-csharp-build-errors.py')
    recorder = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(recorder)
    recorder.SNAPSHOT = OUT / 'build-status.json'
    sys.argv = ['recorder', '--target', 'geometry-record-elimination', '--log', str(log), '--build-exit', str(result.returncode)]
    recorder.main()
    if result.returncode:
        print(result.stdout + result.stderr)
        raise SystemExit(result.returncode)
    binary = OUT / 'bin/Release/net8.0/RecordGeometry.dll'
    for original in SNAPSHOT.glob('*.dll'):
        copied = binary.parent / original.name
        if copied.exists() and digest(copied) != digest(original):
            raise RuntimeError('Copied DLL mismatch: ' + original.name)
    run = subprocess.run(['dotnet', str(binary), str(SNAPSHOT), str(HERE), str(OUT), args.mode],
                         cwd=OUT, capture_output=True, text=True)
    (OUT / ('results-' + args.mode + '.txt')).write_text(run.stdout + run.stderr, encoding='utf-8')
    validate()
    if any(digest(Path(path)) != expected for path, expected in fingerprints.items()):
        raise RuntimeError('Experiment inputs changed')
    print(run.stdout + run.stderr)
    raise SystemExit(run.returncode)
