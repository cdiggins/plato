#!/usr/bin/env python3
"""Check the built CLI's generation and failure paths; optionally compare an older CLI."""
import argparse
import hashlib
import json
from pathlib import Path
import re
import subprocess
import tempfile
import time

ROOT = Path(__file__).resolve().parents[1]
CLI = ROOT / "src/Plato.CLI/bin/Release/net8.0/Plato.CLI.dll"
BACKENDS = ("csharp", "typescript", "rust", "glsl", "cpp", "cuda")


def cases(folder: Path) -> dict[str, tuple[list[str], int]]:
    sources = {
        "a": "type Boolean { }\n",
        "b": "library Picks { Pick(a: Boolean, b: Boolean): Boolean => a ? b : false; }\n",
        "bad": "type { invalid",
        "empty": None,
    }
    for name, source in sources.items():
        path = folder / name
        path.mkdir()
        if source is not None:
            (path / "fixture.plato").write_text(source, encoding="utf-8")
    output = folder / "output"
    result = {}
    for backend in (*BACKENDS, "duplicate"):
        (output / backend).mkdir(parents=True)
    for backend in BACKENDS:
        flags = [] if backend == "csharp" else ["--" + backend]
        result[backend] = ([str(folder / "a"), str(folder / "b"), f"--out={output / backend}", *flags], 0)
    result["lint"] = (["lint", str(folder / "a"), str(folder / "b"), "--strict"], 0)
    result["duplicate-roots"] = ([str(folder / "a"), str(folder / "a"), str(folder / "b"),
                                  f"--out={output / 'duplicate'}"], 0)
    for name in ("empty", "missing", "bad"):
        result[f"lint-{name}"] = (["lint", str(folder / name), "--strict"], 1)
        result[name] = ([str(folder / name), f"--out={output / 'csharp'}"], 1)
    for name, args in {
        "ambiguous": ["a", "b", "c"],
        "scalar": ["--scalar=float"],
        "style": ["--csharp-style=legacy"],
        "profile": ["--profile=invalid"],
        "foreign-profile": ["--typescript", "--profile=cpu64"],
        "lint-usage": ["lint"],
    }.items():
        result[name] = (args, 1)
    return result


def check(cli: Path, folder: Path, commands: dict) -> dict:
    output = folder / "output"
    # These files belong only to this invocation's temporary fixture.
    for path in output.rglob("*"):
        if path.is_file():
            path.unlink()
    results = {}
    for name, (args, expected) in commands.items():
        run = subprocess.run(["dotnet", str(cli), *args], cwd=ROOT, capture_output=True,
                             text=True, encoding="utf-8", errors="replace", timeout=90)
        if run.returncode != expected:
            raise AssertionError(f"{name}: expected exit {expected}, got {run.returncode}\n{run.stdout}\n{run.stderr}")
        results[name] = {"exit": run.returncode, "stderr": run.stderr.strip()}
    for backend in BACKENDS:
        if not any((output / backend).iterdir()):
            raise AssertionError(f"{backend}: successful exit without generated files")
    # POC writers include wall-clock banners; all other bytes remain part of the comparison.
    results["files"] = {
        str(path.relative_to(output)): hashlib.sha256(re.sub(
            rb"(?m)^// Created on .*\r?$", b"// Created on <timestamp>", path.read_bytes())).hexdigest()
        for path in sorted(output.rglob("*")) if path.is_file()
    }
    return results


def main() -> None:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--cli", type=Path, default=CLI)
    parser.add_argument("--baseline", type=Path, help="Previously built CLI to compare outputs and diagnostics")
    args = parser.parse_args()
    start = time.monotonic()
    (ROOT / ".temp").mkdir(exist_ok=True)
    with tempfile.TemporaryDirectory(prefix="cli-check-", dir=ROOT / ".temp") as temporary:
        folder = Path(temporary)
        commands = cases(folder)
        baseline = check(args.baseline.resolve(), folder, commands) if args.baseline else None
        result = check(args.cli.resolve(), folder, commands)
        if baseline is not None and baseline != result:
            raise AssertionError("CLI behavior changed: " + ", ".join(k for k in result if baseline.get(k) != result[k]))
        print(json.dumps({"cases": len(commands), "generated_files": len(result["files"]),
                          "baseline_matched": baseline is not None, "seconds": round(time.monotonic() - start, 1)}))


if __name__ == "__main__":
    main()
