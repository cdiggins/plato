"""Run only under the supervisor's build or timing reservation."""
import argparse
import hashlib
import importlib.util
import json
import os
from pathlib import Path
import subprocess
import sys

ROOT = Path(__file__).resolve().parents[4]
HERE = Path(__file__).resolve().parent
EVIDENCE = ROOT / '.temp/optimizer-geometry/consumer-fusion'
parser = argparse.ArgumentParser()
parser.add_argument('mode', choices=['build', 'census', 'verify', 'benchmark'])
parser.add_argument('--snapshot', required=True, type=Path)
parser.add_argument('--binaries', required=True, type=Path)
parser.add_argument('--stdlib', required=True, type=Path)
args = parser.parse_args()
EVIDENCE.mkdir(parents=True, exist_ok=True)
snapshot = args.snapshot.resolve()
if args.binaries.resolve() != snapshot or args.stdlib.resolve() != snapshot / 'source/stdlib':
    raise RuntimeError('Use the frozen flat binary directory and its source/stdlib directory.')
manifest_path = snapshot / 'manifest.json'
manifest_bytes = manifest_path.read_bytes()
manifest = json.loads(manifest_bytes)
snapshot_hashes = manifest['snapshot_hashes']


def digest(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def verify_snapshot():
    if manifest_path.read_bytes() != manifest_bytes:
        raise RuntimeError('Snapshot manifest changed during this invocation.')
    for relative, expected in snapshot_hashes.items():
        if digest(snapshot / relative) != expected.lower():
            raise RuntimeError('Frozen input changed: ' + relative)


def harness_hashes():
    files = sorted(HERE.glob('*.cs')) + sorted(HERE.glob('*.csproj')) + sorted(HERE.glob('*.plato')) + [Path(__file__).resolve()]
    return {str(path.relative_to(ROOT)): digest(path) for path in files}


def copied_hashes():
    copied = {}
    for relative, expected in snapshot_hashes.items():
        if '/' in relative.replace('\\', '/') or not relative.lower().endswith('.dll'):
            continue
        path = EVIDENCE / 'bin/net8.0' / relative
        if not path.exists():
            continue
        actual = digest(path)
        if actual != expected.lower():
            raise RuntimeError('Copied binary differs from frozen snapshot: ' + relative)
        copied[relative] = actual
    for required in ['Plato.Compiler.dll', 'Plato.CSharpWriter.dll', 'GeometryRuntime.dll', 'Microsoft.CodeAnalysis.CSharp.dll']:
        if required not in copied:
            raise RuntimeError('Required frozen binary was not copied: ' + required)
    return copied


verify_snapshot()
inputs = {'revision': manifest['revision'], 'manifest_sha256': hashlib.sha256(manifest_bytes).hexdigest(), 'sources': harness_hashes()}
stamp_path = EVIDENCE / 'build-inputs.json'
if args.mode == 'build':
    project = HERE / 'ConsumerGeometry.csproj'
    result = subprocess.run(['dotnet', 'build', str(project), '-t:Rebuild', '-c', 'Release', '--nologo', '-v', 'minimal',
                             '-p:Snapshot=' + str(args.binaries.resolve())], capture_output=True, text=True)
    log = EVIDENCE / 'build.log'
    log.write_text(result.stdout + result.stderr, encoding='utf-8')
    spec = importlib.util.spec_from_file_location('geometry_recorder', ROOT / 'tools/record-csharp-build-errors.py')
    recorder = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(recorder)
    recorder.PLATO = EVIDENCE
    recorder.SNAPSHOT = EVIDENCE / 'build-snapshot.json'
    sys.argv = ['record-csharp-build-errors.py', '--target', 'compiler-506-geometry', '--project', str(project),
                '--log', str(log), '--build-exit', str(result.returncode), '--exit-with-build']
    code = recorder.main()
    print(result.stdout + result.stderr)
    verify_snapshot()
    if harness_hashes() != inputs['sources']:
        raise RuntimeError('Harness sources changed during the build.')
    if code == 0:
        inputs['copies'] = copied_hashes()
        inputs['assembly_sha256'] = digest(EVIDENCE / 'bin/net8.0/ConsumerGeometry.dll')
        stamp_path.write_text(json.dumps(inputs, indent=2), encoding='utf-8')
    sys.exit(code)
stamp = json.loads(stamp_path.read_text(encoding='utf-8'))
if stamp['manifest_sha256'] != inputs['manifest_sha256'] or stamp['sources'] != inputs['sources']:
    raise RuntimeError('Harness or frozen snapshot changed; rebuild before execution.')
if stamp['copies'] != copied_hashes() or stamp['assembly_sha256'] != digest(EVIDENCE / 'bin/net8.0/ConsumerGeometry.dll'):
    raise RuntimeError('Built harness or copied dependencies changed; rebuild before execution.')
command = ['dotnet', str(EVIDENCE / 'bin/net8.0/ConsumerGeometry.dll'), str(ROOT), str(args.snapshot.resolve()),
           str(args.stdlib.resolve()), str(EVIDENCE), args.mode]
result = subprocess.run(command, capture_output=True, text=True, env=dict(os.environ, DOTNET_TieredCompilation='0'))
(EVIDENCE / f'{args.mode}.log').write_text(result.stdout + result.stderr, encoding='utf-8')
print(result.stdout + result.stderr)
verify_snapshot()
if inputs['sources'] != harness_hashes() or stamp['copies'] != copied_hashes():
    raise RuntimeError('Inputs changed during execution; results are invalid.')
if stamp['assembly_sha256'] != digest(EVIDENCE / 'bin/net8.0/ConsumerGeometry.dll'):
    raise RuntimeError('Harness assembly changed during execution; results are invalid.')
sys.exit(result.returncode)
