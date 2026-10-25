"""Build with the repo recorder, redirected to this experiment's private evidence."""
import importlib.util
import hashlib
import json
import os
from pathlib import Path
import subprocess
import sys

root = Path(__file__).resolve().parents[4]
evidence = root / '.temp/optimizer-exploration/record-elimination'
spec = importlib.util.spec_from_file_location('recorder', root / 'tools/record-csharp-build-errors.py')
recorder = importlib.util.module_from_spec(spec)
spec.loader.exec_module(recorder)
recorder.PLATO = evidence
recorder.SNAPSHOT = evidence / 'build-snapshot.json'
project = Path(__file__).with_name('RecordElimination.csproj')
evidence.mkdir(parents=True, exist_ok=True)
inputs = [Path(__file__), project, project.with_name('Program.cs'),
          root / 'writers/Plato.CSharpWriter/CSharpWriter.cs',
          root / 'writers/Plato.CSharpWriter/Optimization/TirComponentUnroller.cs',
          root / 'src/Plato.Compiler/Checking/Tir.cs']
(evidence / 'fingerprints.json').write_text(json.dumps({
    str(p.relative_to(root)): hashlib.sha256(p.read_bytes()).hexdigest() for p in inputs
}, indent=2), encoding='utf-8')
log = evidence / 'build.log'
with log.open('w', encoding='utf-8') as output:
    build = subprocess.run(['dotnet', 'build', str(project), '-c', 'Release', '--nologo'],
                           stdout=output, stderr=subprocess.STDOUT)
sys.argv = ['record', '--project', str(project), '--log', str(log),
            '--build-exit', str(build.returncode), '--target', 'compiler-507', '--exit-with-build']
code = recorder.main()
if code:
    raise SystemExit(code)
env = dict(os.environ, DOTNET_TieredCompilation='0')
run = subprocess.run(['dotnet', str(evidence / 'bin/net8.0/RecordElimination.dll')],
                     text=True, capture_output=True, env=env)
(evidence / 'results.txt').write_text(run.stdout + run.stderr, encoding='utf-8')
print(run.stdout + run.stderr)
raise SystemExit(run.returncode)
