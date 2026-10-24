#!/usr/bin/env python3
"""Check law-run process exits, including matching failures in both engines."""
import argparse
import json
from pathlib import Path
import re
import subprocess
import tempfile
import time

ROOT = Path(__file__).resolve().parents[1]
CLI = ROOT / "src/Plato.CLI/bin/Release/net8.0/Plato.CLI.dll"
SOURCE = """
library ExitFixture
{
    Law_Passes(b: Boolean): Boolean => true;
    Law_Fails(b: Boolean): Boolean => false;
    Law_Errors(b: Boolean): Boolean
    {
        var values = [1];
        return values[2] == 1;
    }
    Law_Vacuous(b: Boolean): Boolean => true;
    Premise_Vacuous(b: Boolean): Boolean => false;
}
"""


def check(cli: Path, folder: Path, backend: str, law: str, verdict: str,
          expected: int, baseline: Path | None = None) -> None:
    args = ["dotnet", str(cli), "run", str(ROOT / "stdlib/foundation"), str(folder),
            "--laws", f"--backend={backend}", f"--filter=ExitFixture.{law}",
            "--budget=0", f"--report={folder / 'verdicts.tsv'}"]
    if baseline is not None:
        args.append(f"--baseline={baseline}")
    run = subprocess.run(args, cwd=ROOT, capture_output=True, text=True,
                         encoding="utf-8", errors="replace", timeout=120)
    if run.returncode != expected or not re.search(rf"(?m)^{verdict} +ExitFixture\.{law} +", run.stdout):
        raise AssertionError(f"{backend}/{law}: expected {verdict}, exit {expected}; "
                             f"got exit {run.returncode}\n{run.stdout}\n{run.stderr}")
    if backend == "differential" and "differential: 0 mismatch(es)" not in run.stdout:
        raise AssertionError(f"expected matching engines\n{run.stdout}")


def main() -> None:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--cli", type=Path, default=CLI)
    args = parser.parse_args()
    start = time.monotonic()
    with tempfile.TemporaryDirectory(prefix="law-run-exit-") as temporary:
        folder = Path(temporary)
        (folder / "fixture.plato").write_text(SOURCE, encoding="utf-8")
        cases = 0
        for backend in ("interpreter", "csharp", "differential"):
            for law, verdict, expected in (("Law_Passes", "pass", 0), ("Law_Fails", "fail", 1),
                                           ("Law_Errors", "error", 1), ("Law_Vacuous", "vacuous", 1)):
                check(args.cli.resolve(), folder, backend, law, verdict, expected)
                cases += 1
        baseline = folder / "baseline.tsv"
        baseline.write_text((folder / "verdicts.tsv").read_text(encoding="utf-8"), encoding="utf-8")
        check(args.cli.resolve(), folder, "differential", "Law_Vacuous", "vacuous", 0, baseline)
        print(json.dumps({"cases": cases + 1, "seconds": round(time.monotonic() - start, 1)}))


if __name__ == "__main__":
    main()
