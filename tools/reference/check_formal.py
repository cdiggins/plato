#!/usr/bin/env python3
"""Check the reference proofs with the pinned Lean release and retain diagnostics."""
import argparse
import json
import shutil
import subprocess
import time
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
FORMAL = ROOT / 'docs/reference/formal'


def check(lean: str) -> int:
    expected = (FORMAL / 'lean-toolchain').read_text(encoding='utf-8').strip().split(':')[-1].lstrip('v')
    version = subprocess.check_output([lean, '--version'], cwd=ROOT, text=True).strip()
    if not version.startswith(f'Lean (version {expected},'):
        raise ValueError(f'expected pinned Lean {expected}, found {version}')
    args = [lean, '-DwarningAsError=true', str(FORMAL / 'Core.lean')]
    started = time.monotonic()
    result = subprocess.run(args, cwd=ROOT, capture_output=True, text=True, encoding='utf-8', errors='replace')
    seconds = round(time.monotonic() - started, 3)
    logs = ROOT / '.temp/reference-proof-logs'
    logs.mkdir(parents=True, exist_ok=True)
    (logs / 'lean.log').write_text(
        f'{version}\nCommand: {subprocess.list2cmdline(args)}\nExit: {result.returncode}\nSeconds: {seconds}\n\n'
        + result.stdout + result.stderr, encoding='utf-8')
    print(json.dumps({'version': version, 'exitCode': result.returncode, 'seconds': seconds,
                      'log': str(logs / 'lean.log')}))
    return result.returncode


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    local = ROOT / '.temp/lean-proof/lean-4.19.0-windows/bin/lean.exe'
    parser.add_argument('--lean', default=str(local) if local.exists() else shutil.which('lean'))
    args = parser.parse_args()
    if not args.lean:
        parser.error('install the pinned Lean toolchain or pass --lean PATH')
    raise SystemExit(check(args.lean))
