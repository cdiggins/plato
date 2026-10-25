"""Read-only TIR qualification; execute only during a supervisor-granted runtime window."""
import argparse
import hashlib
import importlib.util
import json
from pathlib import Path
import re
import subprocess
import sys
from xml.sax.saxutils import escape

ROOT = Path(__file__).resolve().parents[4]
HERE = Path(__file__).resolve().parent
OUT = ROOT / '.temp/optimizer-geometry/array-facts'
SNAPSHOT = ROOT / '.temp/optimizer-geometry/compiler'


def digest(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def validate_snapshot():
    manifest = json.loads((SNAPSHOT / 'manifest.json').read_text(encoding='utf-8-sig'))
    for name, expected in manifest['snapshot_hashes'].items():
        path = SNAPSHOT / name
        if not path.is_file() or digest(path) != expected:
            raise RuntimeError('Frozen snapshot mismatch: ' + name)
    for name, expected in manifest['source_hashes'].items():
        if name.replace('\\', '/').startswith('stdlib/'):
            if digest(SNAPSHOT / 'source' / name) != expected:
                raise RuntimeError('Frozen stdlib source mismatch: ' + name)
    return manifest


if __name__ == '__main__':
    parser = argparse.ArgumentParser()
    parser.add_argument('--execute', action='store_true')
    args = parser.parse_args()
    if not args.execute:
        raise SystemExit('Prepared source only. Use --execute after the runtime window is granted.')
    manifest = validate_snapshot()
    OUT.mkdir(parents=True, exist_ok=True)
    inputs = [p for p in HERE.iterdir() if p.is_file()]
    inputs += [SNAPSHOT / 'manifest.json']
    fingerprints = {str(p): digest(p) for p in inputs}
    (OUT / 'input-fingerprints.json').write_text(json.dumps(fingerprints, indent=2), encoding='utf-8')
    text = (SNAPSHOT / 'source/stdlib/geometry/meshes.library.plato').read_text(encoding='utf-8')
    helper = re.search(r'^    GridParameter\([^;]+?;', text, re.MULTILINE)
    if helper is None:
        raise RuntimeError('Exact GridParameter extraction failed')
    (OUT / 'helper.plato').write_text('library GeometryArrayFactHelper\n{\n' + helper[0] + '\n}\n', encoding='utf-8')
    references = ''.join(f'<Reference Include="{escape(p.stem)}"><HintPath>{escape(str(p))}</HintPath></Reference>'
                         for p in SNAPSHOT.glob('*.dll') if p.name != 'Ara3D.Geometry.dll')
    sources = ''.join(f'<Compile Include="{escape(str(HERE / name))}"/>' for name in ('ArrayFacts.cs', 'Program.cs'))
    project = OUT / 'ArrayFactsProbe.csproj'
    project.write_text('<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType>'
                      '<TargetFramework>net8.0</TargetFramework><ImplicitUsings>enable</ImplicitUsings>'
                      '<EnableDefaultCompileItems>false</EnableDefaultCompileItems>'
                      '</PropertyGroup><ItemGroup>' + sources + references + '</ItemGroup></Project>', encoding='utf-8')
    result = subprocess.run(['dotnet', 'build', str(project), '-c', 'Release', '--nologo'],
                            cwd=OUT, capture_output=True, text=True)
    log = OUT / 'build.log'
    log.write_text(result.stdout + result.stderr, encoding='utf-8')
    sys.dont_write_bytecode = True
    spec = importlib.util.spec_from_file_location('recorder', ROOT / 'tools/record-csharp-build-errors.py')
    recorder = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(recorder)
    recorder.SNAPSHOT = OUT / 'build-status.json'
    sys.argv = ['recorder', '--target', 'geometry-array-facts', '--log', str(log), '--build-exit', str(result.returncode)]
    recorder.main()
    if result.returncode:
        print(result.stdout + result.stderr)
        raise SystemExit(result.returncode)
    binary = OUT / 'bin/Release/net8.0/ArrayFactsProbe.dll'
    for original in SNAPSHOT.glob('*.dll'):
        copied = binary.parent / original.name
        if copied.exists() and digest(copied) != digest(original):
            raise RuntimeError('Copied assembly differs: ' + original.name)
    run = subprocess.run(['dotnet', str(binary), str(SNAPSHOT), str(HERE), str(OUT)],
                         cwd=OUT, capture_output=True, text=True)
    (OUT / 'results.txt').write_text(run.stdout + run.stderr, encoding='utf-8')
    validate_snapshot()
    if any(digest(Path(path)) != expected for path, expected in fingerprints.items()):
        raise RuntimeError('Experiment inputs changed during execution')
    print(run.stdout + run.stderr)
    raise SystemExit(run.returncode)
