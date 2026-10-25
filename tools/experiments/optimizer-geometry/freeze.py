"""Reuse a verified CLI cache and compile its exact wrapper sources privately."""
import argparse, hashlib, importlib.util, json, shutil, subprocess, sys
from pathlib import Path
ROOT = Path(__file__).resolve().parents[3]
OUT = ROOT / '.temp/optimizer-geometry'
DEST = OUT / 'compiler'
def digest(p): return hashlib.sha256(p.read_bytes()).hexdigest()
def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--cache', required=True, type=Path)
    cache = parser.parse_args().cache.resolve()
    receipt = cache / 'complete.json'
    receipt_hash = digest(receipt)
    record = json.loads(receipt.read_text(encoding='utf-8-sig'))
    identity = json.loads(record['identity'])
    expected = {r['path']: r['hash'].lower() for r in record['files']}
    actual = {p.relative_to(cache).as_posix() for p in cache.rglob('*') if p.is_file() and p != receipt}
    if actual != set(expected): raise RuntimeError('Cache file set changed')
    for name, value in expected.items():
        if digest(cache / name) != value: raise RuntimeError('Cache changed: ' + name)
    print('Verified committed cache', identity['plato'], flush=True)
    if (DEST / 'manifest.json').exists(): raise RuntimeError('Preserve existing snapshot; refusing overwrite')
    shutil.copytree(cache / 'build/bin/Plato.CLI/release', DEST, dirs_exist_ok=True)
    sources = {}
    for relative, value in expected.items():
        if relative.startswith(('plato/src/', 'plato/writers/', 'plato/stdlib/', 'plato/parakeet/')) and not {'bin','obj','.temp'}.intersection(Path(relative).parts):
            name = relative.removeprefix('plato/')
            target = DEST / 'source' / name
            target.parent.mkdir(parents=True, exist_ok=True)
            shutil.copy2(cache / relative, target)
            sources[name] = value
    runtime = OUT / 'runtime'
    runtime.mkdir(exist_ok=True)
    project = runtime / 'GeometryRuntime.csproj'
    refs = ''.join(f'<Reference Include="{p.stem}"><HintPath>{p}</HintPath></Reference>' for p in DEST.glob('*.dll') if p.name != 'Ara3D.Geometry.dll')
    project.write_text('<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><TargetFramework>net8.0</TargetFramework><ImplicitUsings>enable</ImplicitUsings><AllowUnsafeBlocks>true</AllowUnsafeBlocks><EnableDefaultCompileItems>false</EnableDefaultCompileItems></PropertyGroup><Import Project="' + str(DEST / 'source/src/Plato.Intrinsics/Plato.Intrinsics.projitems') + '"/><ItemGroup>' + refs + '</ItemGroup></Project>', encoding='utf-8')
    log = OUT / 'runtime-build.log'
    with log.open('w', encoding='utf-8') as stream:
        result = subprocess.run(['dotnet','build',str(project),'-c','Release','--nologo','-v','minimal','-p:UseSharedCompilation=false'], cwd=ROOT, stdout=stream, stderr=subprocess.STDOUT)
    spec = importlib.util.spec_from_file_location('recorder', ROOT / 'tools/record-csharp-build-errors.py')
    recorder = importlib.util.module_from_spec(spec)
    sys.dont_write_bytecode = True
    spec.loader.exec_module(recorder)
    recorder.SNAPSHOT = OUT / 'build-status.json'
    sys.argv = ['record','--target','geometry-runtime','--log',str(log),'--build-exit',str(result.returncode)]
    recorder.main()
    if result.returncode:
        print(log.read_text(encoding='utf-8'))
        raise SystemExit(result.returncode)
    shutil.copy2(runtime / 'bin/Release/net8.0/GeometryRuntime.dll', DEST)
    for name, value in sources.items():
        if digest(DEST / 'source' / name) != value: raise RuntimeError('Source copy changed: '+name)
    for name, value in expected.items():
        if name.startswith('build/bin/Plato.CLI/release/') and digest(DEST / name.removeprefix('build/bin/Plato.CLI/release/')) != value: raise RuntimeError('Binary copy changed: '+name)
    if digest(receipt) != receipt_hash: raise RuntimeError('Cache manifest changed')
    manifest = {'revision': identity['plato'], 'source_hashes': sources, 'cache_manifest': str(receipt), 'cache_manifest_sha256': receipt_hash, 'runtime_project_sha256': digest(project), 'runtime_profile':'cpu64', 'snapshot_hashes':{p.relative_to(DEST).as_posix():digest(p) for p in DEST.rglob('*') if p.is_file()}}
    (DEST / 'manifest.json').write_text(json.dumps(manifest, indent=2)+'\n',encoding='utf-8')
    print('Frozen compiler/runtime ready:', DEST)
if __name__ == '__main__': main()
