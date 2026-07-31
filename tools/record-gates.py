#!/usr/bin/env python3
"""Run the Plato gates and WRITE DOWN what they said.

Two destinations, deliberately different in kind:

  docs/status-report-snapshot.json  CURRENT state, machine-readable. Overwritten each run;
                                    feeds docs/status-report.html via gen-status-report.py.
  docs/gate-log.md                  HISTORY, human-readable. One appended row per run, so a
                                    number that moved can be traced to the day and commit it
                                    moved on.

Neither file is a substitute for the ratchets, which are ENFORCED in tests (PlatoTests:
ForwardStdLibLintTests for lint findings, ForwardStdLibCheckerTests for type-checker
diagnostics). A recorded number nobody asserts on is a number that drifts.

Usage (from the repo root):
  python tools/record-gates.py              # lint + PlatoTests            (~2 min)
  python tools/record-gates.py --full       # + regen/build/run conformance (~5 min)
  python tools/record-gates.py --dry-run    # measure, print, write nothing

Every gate runs even if an earlier one fails: a red gate is a result to record, not a reason
to stop. Exit code is 0 when every gate that ran passed, 1 otherwise.
"""
from __future__ import annotations

import argparse
import json
import pathlib
import re
import subprocess
import time
from datetime import datetime

PLATO = pathlib.Path(__file__).resolve().parents[1]
SNAPSHOT = PLATO / "docs" / "status-report-snapshot.json"
LOG = PLATO / "docs" / "gate-log.md"

CLI = "src/Plato.CLI/Plato.CLI.csproj"
PLATO_TESTS = "tests/PlatoTests/PlatoTests.csproj"
CONFORMANCE = "tests/conformance/Plato.ForwardConformanceTests/Plato.ForwardConformanceTests.csproj"
GENERATED = "tests/conformance/Plato.ForwardConformanceTests/Generated"
TIERS = ["stdlib/foundation", "stdlib/geometry", "stdlib/graphics", "stdlib/future"]

# The shipping recipe (docs/plato-library-map.md). Kept beside the codegen call it feeds.
RECIPE = [
    "--csharp-style=extensions", "--scalar=float", "--optimize", "--optimize-arrays",
    "--inline", "--methods", "--loops", "--no-properties", "--static-abstract",
]


def run(args: list[str]) -> tuple[str, int, float]:
    """Run a command from the repo root; return (combined output, exit code, seconds)."""
    t0 = time.monotonic()
    r = subprocess.run(
        args, cwd=PLATO, capture_output=True, text=True, encoding="utf-8", errors="replace"
    )
    return ((r.stdout or "") + (r.stderr or ""), r.returncode, time.monotonic() - t0)


def parse_test_counts(out: str) -> dict | None:
    """The `dotnet test` summary line, or None when the run never got that far."""
    m = re.search(
        r"Failed:\s*(\d+),\s*Passed:\s*(\d+),\s*Skipped:\s*(\d+),\s*Total:\s*(\d+)", out
    )
    if not m:
        return None
    return {
        "failed": int(m.group(1)), "passed": int(m.group(2)),
        "skipped": int(m.group(3)), "total": int(m.group(4)),
    }


def parse_lint(out: str) -> dict:
    counts = {c: int(n) for c, n in re.findall(r"(LINT\d+) x(\d+)", out.split("lint finding(s)")[-1])}
    sev = {k.lower(): int(v) for k, v in re.findall(r"(Error|Warning|Info) x(\d+)", out)}
    ratchet = re.search(r"Ratchet: (\d+) \(Error (\d+) \+ Warning (\d+)\)", out)
    return {
        "parse_errors": 0 if "Parsing failed" not in out else 1,
        "resolution_errors": int(m.group(1)) if (m := re.search(r"Found (\d+) symbol resolution errors", out)) else 0,
        "errors": int(ratchet.group(2)) if ratchet else sev.get("error", 0),
        "warnings": int(ratchet.group(3)) if ratchet else sev.get("warning", 0),
        "infos": sev.get("info", 0),
        "ratchet": int(ratchet.group(1)) if ratchet else None,
        "counts": counts,
    }


def gate(name: str, result: str, seconds: float, detail: str) -> dict:
    return {"name": name, "result": result, "seconds": f"{seconds:.0f}s", "detail": detail}


def measure(full: bool) -> tuple[list[dict], dict, dict]:
    """Run the gates. Returns (gate rows, lint block, extras for the log row)."""
    gates: list[dict] = []
    extras: dict = {}

    out, code, secs = run(["dotnet", "build", CLI, "-c", "Release", "--nologo", "-v", "q"])
    gates.append(gate("Plato.CLI build (Release)", "PASS" if code == 0 else "FAIL", secs,
                      "0 errors" if code == 0 else "build failed — later gates are unreliable"))

    out, code, secs = run(["dotnet", "run", "--project", CLI, "-c", "Release", "--no-build",
                           "--", "lint", *TIERS, "--strict"])
    lint = parse_lint(out)
    extras["lint"] = lint
    gates.append(gate(
        "lint --strict (four stdlib tiers)", "PASS" if code == 0 else "FAIL", secs,
        f"{lint['errors']} error / {lint['warnings']} warning / {lint['infos']} info; "
        f"ratchet {lint['ratchet']}"))

    out, code, secs = run(["dotnet", "test", PLATO_TESTS, "-c", "Release", "--nologo", "-v", "q"])
    t = parse_test_counts(out)
    extras["platotests"] = t
    gates.append(gate("PlatoTests (incl. both ratchets)", "PASS" if code == 0 else "FAIL", secs,
                      f"{t['passed']} passed / {t['failed']} failed" if t else "did not report"))

    if not full:
        return gates, lint, extras

    out, code, secs = run(["dotnet", "run", "--project", CLI, "-c", "Release", "--no-build", "--",
                           *TIERS, "tests/stdlib-tests", f"--out={GENERATED}", *RECIPE])
    files = len(list((PLATO / GENERATED).glob("*.g.cs"))) if (PLATO / GENERATED).is_dir() else 0
    degraded = int(m.group(1)) if (m := re.search(r"DEGRADED bodies[^:]*: (\d+)", out)) else None
    extras["codegen"] = {"files": files, "degraded": degraded}
    gates.append(gate("forward-stdlib codegen (full recipe)", "PASS" if code == 0 else "FAIL", secs,
                      f"{files} .g.cs, {degraded} degraded bodies"))

    out, code, secs = run(["dotnet", "test", CONFORMANCE, "-c", "Release", "--nologo", "-v", "q"])
    t = parse_test_counts(out)
    extras["conformance"] = t
    gates.append(gate("forward conformance (build + law runner)", "PASS" if code == 0 else "FAIL",
                      secs, f"{t['passed']} passed / {t['failed']} failed / {t['skipped']} skipped"
                      if t else "did not build"))

    return gates, lint, extras


def write_snapshot(gates: list[dict], lint: dict) -> None:
    snap = json.loads(SNAPSHOT.read_text(encoding="utf-8")) if SNAPSHOT.exists() else {}
    stamp = datetime.now().astimezone().isoformat(timespec="seconds")
    snap["updated_at"] = stamp
    snap["note"] = f"Written by tools/record-gates.py at {stamp}."
    # Gates this run measured replace their same-named rows; rows it did not run are kept
    # (with their old timestamp) rather than silently dropped.
    measured = {g["name"]: g for g in gates}
    kept = [g for g in snap.get("gates", []) if g["name"] not in measured]
    snap["gates"] = gates + [dict(g, result=g.get("result", "?"), detail="(stale — not run this time) "
                                 + g.get("detail", "")) for g in kept]
    snap["lint"] = {"strict_exit": 0 if lint["errors"] == 0 else 1, **lint}
    SNAPSHOT.write_text(json.dumps(snap, indent=2) + "\n", encoding="utf-8")


HEADER = """# Gate log

Append-only history of gate runs, written by `tools/record-gates.py`. Current machine-readable
state lives in `docs/status-report-snapshot.json`; the ratchets themselves are enforced in
`tests/PlatoTests` (`ForwardStdLibLintTests`, `ForwardStdLibCheckerTests`), not here.

Read a row as: what the gates said at that commit. A number that moved between two rows is a
change somebody made — start at the commit in the second row.

| Date | Commit | lint ratchet (E+W) | lint info | PlatoTests | codegen | conformance |
|---|---|---|---|---|---|---|
"""


def append_log(extras: dict) -> None:
    if not LOG.exists():
        LOG.write_text(HEADER, encoding="utf-8")
    sha = subprocess.run(["git", "rev-parse", "--short", "HEAD"], cwd=PLATO,
                         capture_output=True, text=True).stdout.strip() or "?"
    lint = extras.get("lint", {})
    pt = extras.get("platotests")
    cg = extras.get("codegen")
    cf = extras.get("conformance")
    row = (f"| {datetime.now().strftime('%Y-%m-%d')} | `{sha}` "
           f"| {lint.get('ratchet', '?')} ({lint.get('errors', '?')}+{lint.get('warnings', '?')}) "
           f"| {lint.get('infos', '?')} "
           f"| {f'{pt['passed']}/{pt['total']}' if pt else '—'} "
           f"| {f'{cg['files']} files, {cg['degraded']} degraded' if cg else '—'} "
           f"| {f'{cf['passed']} pass / {cf['failed']} fail / {cf['skipped']} skip' if cf else '—'} |\n")
    with LOG.open("a", encoding="utf-8") as f:
        f.write(row)


def main() -> int:
    ap = argparse.ArgumentParser(description=__doc__,
                                 formatter_class=argparse.RawDescriptionHelpFormatter)
    ap.add_argument("--full", action="store_true",
                    help="also regenerate the forward stdlib and run the conformance law runner")
    ap.add_argument("--dry-run", action="store_true", help="measure and print; write nothing")
    args = ap.parse_args()

    gates, lint, extras = measure(args.full)
    width = max(len(g["name"]) for g in gates)
    for g in gates:
        print(f"{g['result']:5} {g['name']:{width}}  {g['seconds']:>5}  {g['detail']}")

    if args.dry_run:
        print("\n--dry-run: nothing written.")
    else:
        write_snapshot(gates, lint)
        append_log(extras)
        print(f"\nwrote {SNAPSHOT.relative_to(PLATO)}")
        print(f"appended {LOG.relative_to(PLATO)}")

    return 0 if all(g["result"] == "PASS" for g in gates) else 1


if __name__ == "__main__":
    raise SystemExit(main())
