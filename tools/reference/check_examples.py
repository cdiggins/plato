#!/usr/bin/env python3
"""Execute the reference's isolated law fixtures and retain their raw evidence."""
import argparse
import json
import re
import subprocess
import time
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
FIXTURES = ROOT / 'docs/reference/implementation/examples'
PRELUDE = 'prelude'


def discover_groups(fixtures: Path) -> list:
    """Names of the fixture groups: every folder except the prelude, sorted."""
    return sorted(p.name for p in fixtures.iterdir() if p.is_dir() and p.name != PRELUDE)


def resolve_inputs(group_dir: Path, fixtures: Path, root: Path) -> list:
    """Compilation inputs for a group: its inputs.txt folders, else the prelude folder."""
    listing = group_dir / 'inputs.txt'
    if not listing.exists():
        return [fixtures / PRELUDE]
    inputs = []
    root = root.resolve()
    for line in listing.read_text(encoding='utf-8').splitlines():
        line = line.strip()
        if not line or line.startswith('#'):
            continue
        path = (root / line).resolve()
        if not path.is_relative_to(root):
            raise ValueError(f'{group_dir.name}: input {line!r} lies outside the repo')
        if not path.is_dir():
            raise ValueError(f'{group_dir.name}: input {line!r} does not exist')
        inputs.append(path)
    if not inputs:
        raise ValueError(f'{group_dir.name}: inputs.txt lists no inputs')
    return inputs


def count_laws(group_dir: Path) -> int:
    """Declared Law_Reference functions in a group; refuses a group that declares none."""
    files = list(group_dir.glob('*.plato'))
    if not files:
        raise ValueError(f'{group_dir.name}: no .plato file; refusing an empty success')
    count = sum(len(re.findall(r'\bLaw_Reference\w*\s*\(', p.read_text(encoding='utf-8'))) for p in files)
    if not count:
        raise ValueError(f'{group_dir.name}: no reference laws; refusing an empty success')
    return count


def check(cli: Path, backend: str = 'interpreter', groups=None) -> int:
    logs = ROOT / '.temp/reference-example-logs' / backend
    logs.mkdir(parents=True, exist_ok=True)
    results = []
    for group in groups or discover_groups(FIXTURES):
        inputs = resolve_inputs(FIXTURES / group, FIXTURES, ROOT)
        law_count = count_laws(FIXTURES / group)
        report = logs / f'{group}.tsv'
        report.unlink(missing_ok=True)
        args = ['dotnet', str(cli), 'run', *map(str, inputs), str(FIXTURES / group), '--laws',
                '--filter=Law_Reference', '--trials=1', '--budget=10',
                f'--backend={backend}', f'--report={report}']
        started = time.monotonic()
        process = subprocess.run(args, cwd=ROOT, capture_output=True, text=True, encoding='utf-8', errors='replace')
        seconds = round(time.monotonic() - started, 3)
        output = process.stdout + process.stderr
        (logs / f'{group}.log').write_text(
            f'Command: {subprocess.list2cmdline(args)}\nExit: {process.returncode}\nSeconds: {seconds}\n\n{output}',
            encoding='utf-8')
        rows = [line for line in report.read_text(encoding='utf-8').splitlines()
                if line.strip() and not line.startswith('#')] if report.exists() else []
        # The verdict report must name every declared law. A command selecting no
        # laws, returning only obligations, or failing before a report is not green.
        names = set(re.findall(r'\bLaw_Reference\w+', '\n'.join(rows)))
        ok = process.returncode == 0 and len(names) == law_count and all(
            len(row.split('\t')) >= 2 and row.split('\t')[1] == 'pass' for row in rows)
        results.append({'group': group, 'backend': backend, 'passed': bool(ok),
                        'declaredLaws': law_count, 'reportedLaws': len(names), 'seconds': seconds,
                        'exitCode': process.returncode, 'log': str(logs / f'{group}.log')})
        print(json.dumps(results[-1]), flush=True)
    (logs / 'results.json').write_text(json.dumps(results, indent=2) + '\n', encoding='utf-8')
    return 0 if all(row['passed'] for row in results) else 1


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--cli', type=Path, default=ROOT / 'src/Plato.CLI/bin/Release/net8.0/Plato.CLI.dll')
    parser.add_argument('--backend', choices=('interpreter', 'csharp', 'differential'), default='interpreter')
    parser.add_argument('--group', action='append', help='fixture group to run; repeatable; default all')
    args = parser.parse_args()
    known = discover_groups(FIXTURES)
    unknown = [g for g in args.group or () if g not in known]
    if unknown:
        parser.error(f'unknown group {unknown}; discovered: {", ".join(known)}')
    raise SystemExit(check(args.cli.resolve(), args.backend, args.group))
