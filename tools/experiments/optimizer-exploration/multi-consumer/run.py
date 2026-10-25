"""Build with the repository recorder redirected to this track's private evidence."""
import hashlib
import importlib.util
import json
import os
from pathlib import Path
import subprocess
import sys

ROOT = Path(__file__).resolve().parents[4]
HERE = Path(__file__).resolve().parent
EVIDENCE = ROOT / '.temp/optimizer-exploration/multi-consumer'
EVIDENCE.mkdir(parents=True, exist_ok=True)
spec = importlib.util.spec_from_file_location('recorder', ROOT / 'tools/record-csharp-build-errors.py')
recorder = importlib.util.module_from_spec(spec)
spec.loader.exec_module(recorder)
recorder.PLATO = EVIDENCE
recorder.SNAPSHOT = EVIDENCE / 'build-snapshot.json'
project = HERE / 'MultiConsumer.csproj'
build = subprocess.run(['dotnet', 'build', str(project), '-c', 'Release', '--nologo', '-v', 'minimal'],
                       capture_output=True, text=True)
build_log = EVIDENCE / 'build.log'
build_log.write_text(build.stdout + build.stderr, encoding='utf-8')
sys.argv = ['record-csharp-build-errors.py', '--target', 'compiler-506', '--project',
            str(project), '--log', str(build_log), '--build-exit', str(build.returncode), '--exit-with-build']
code = recorder.main()
if code:
    sys.exit(code)
sources = list(HERE.glob('*.cs')) + list(HERE.glob('*.csproj'))
sources += list((ROOT / 'src/Plato.Intrinsics').glob('*.cs'))
fingerprints = {str(p.relative_to(ROOT)): hashlib.sha256(p.read_bytes()).hexdigest() for p in sources}
(EVIDENCE / 'fingerprints.json').write_text(json.dumps(fingerprints, indent=2))
environment = dict(os.environ, DOTNET_TieredCompilation='0')
result = subprocess.run(['dotnet', str(HERE / 'bin/Release/net8.0/MultiConsumer.dll')],
                        capture_output=True, text=True, env=environment)
(EVIDENCE / 'results.txt').write_text(result.stdout + result.stderr)
print(result.stdout + result.stderr)
sys.exit(result.returncode)
