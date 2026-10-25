"""C# statement-inlining ablations using compiler-499's pinned host and input snapshot."""
import argparse
import contextlib
import hashlib
import importlib.util
import json
from pathlib import Path
import re
import runpy
import shutil
import subprocess
import sys
import time
import xml.etree.ElementTree as ET
sys.dont_write_bytecode = True

ROOT = Path(__file__).resolve().parents[2]
spec = importlib.util.spec_from_file_location("brep_inlining", Path(__file__).with_name("brep-inlining.py"))
common = importlib.util.module_from_spec(spec)
spec.loader.exec_module(common)
POLICIES = {"baseline": [], "default": ["--inline"],
            "statements": ["--inline", "--inline-policy=statements"],
            "lambdas": ["--inline", "--inline-policy=lambdas"],
            "iterative": ["--inline", "--inline-policy=iterative"],
            "iterative-large": ["--inline", "--inline-policy=iterative", "--inline-growth=16384"]}


def check_stubs(work, dest):
    checker = work / "brep/library/tools/check_generated.py"
    log = dest / "stub-check.log"
    started, cpu, wall = time.time(), time.process_time(), time.perf_counter()
    previous = sys.argv
    try:
        sys.argv = [str(checker), str(dest / "generated"), "--log", str(dest / "generate.log")]
        with log.open("w", encoding="utf-8") as stream, contextlib.redirect_stdout(stream), contextlib.redirect_stderr(stream):
            try:
                runpy.run_path(str(checker), run_name="__main__")
                code = 0
            except SystemExit as result:
                code = result.code or 0
    finally:
        sys.argv = previous
    return {"exit": code, "started_at_unix": started, "seconds": time.perf_counter() - wall,
            "cpu_seconds": time.process_time() - cpu, "log": str(log),
            "execution": "Pinned checker executed unchanged in the driver process"}


def bootstrap(work, previous):
    if work.exists():
        raise ValueError("Use a new work directory; snapshots are not overwritten")
    work.mkdir(parents=True)
    for name in ("brep", "inputs"):
        shutil.copytree(previous / name, work / name,
                        ignore=shutil.ignore_patterns("bin", "obj", ".git", "*.tar"))
    shutil.copytree(ROOT / "src/Plato.CLI/bin/Release/net8.0", work / "cli")
    for name in ("inputs.json", "host-instrumentation.json"):
        shutil.copy2(previous / name, work / name)
    common.write_json(work / "compiler-identity.json", {
        "revision": subprocess.check_output(["git", "rev-parse", "HEAD"], cwd=ROOT, text=True).strip(),
        "binary_hashes": {p.name: hashlib.sha256(p.read_bytes()).hexdigest() for p in (work / "cli").glob("*.dll")},
        "pinned_inputs_from": str(previous.resolve())})


def generate(work, variants):
    for name in variants:
        dest = work / name
        output = dest / "generated"
        output.mkdir(parents=True, exist_ok=True)
        flags = ["--fuse", "--loops", "--inline-report", *POLICIES[name]]
        result = json.loads((dest / "result.json").read_text()) if (dest / "result.json").exists() else {"variant": name, "flags": flags}
        if result.get("host_build", {}).get("exit") == 0 and result.get("stub_check", {}).get("exit") == 0:
            continue
        if "generation" not in result:
            result["generation"] = common.run(["dotnet", work / "cli/Plato.CLI.dll",
                *sorted((work / "inputs").iterdir()), f"--out={output}", "--profile=cpu64", *flags], dest / "generate.log")
        common.write_json(dest / "result.json", result)
        if result["generation"]["exit"]:
            raise RuntimeError(f"Generation failed: {name}")
        result["stub_check"] = check_stubs(work, dest)
        result["source_bytes"] = sum(p.stat().st_size for p in output.glob("*.g.cs"))
        log = (dest / "generate.log").read_text(encoding="utf-8", errors="replace")
        result["inlining_totals"] = re.findall(r"(?:calls inlined|beta reductions|statement calls inlined|known lambdas inlined):\s+\d+", log)
        common.build_host(work, name, result)
        if result["host_build"]["exit"] or result["stub_check"]["exit"]:
            raise RuntimeError(f"Invalid variant {name}; no source fallback is allowed")


def profile(work, variants, corpus):
    """Use a separate host with phase events; keep traced runs out of timing samples."""
    folder = work / "profiles"
    folder.mkdir(exist_ok=True)
    host = work / "profile-host"
    if not (folder / "host-build.json").exists():
        shutil.copytree(work / "brep/tools/step-corpus-harness", host,
                        ignore=shutil.ignore_patterns("bin", "obj"), dirs_exist_ok=True)
        progress = host / "Progress.cs"
        original = progress.read_text(encoding="utf-8")
        progress.write_text(original.replace('Console.WriteLine($"stage',
            'ProfilePhases.Log.Stage(stage);\n        Console.WriteLine($"stage'), encoding="utf-8")
        (host / "ProfilePhases.cs").write_text('''using System.Diagnostics.Tracing;
namespace Ara3D.Step.CorpusHarness;
[EventSource(Name = "Plato-Experiment-Phases")]
public sealed class ProfilePhases : EventSource {
    public static readonly ProfilePhases Log = new();
    [Event(1)] public void Stage(string stage) => WriteEvent(1, stage);
}
''', encoding="utf-8")
        project = ET.parse(host / "StepCorpusHarness.csproj")
        for group in project.getroot().findall("ItemGroup"):
            for reference in group.findall("ProjectReference"):
                group.remove(reference)
        references = ET.SubElement(project.getroot(), "ItemGroup")
        for dll in (work / "baseline/bin").glob("*.dll"):
            if dll.name == "step-corpus-harness.dll":
                continue
            reference = ET.SubElement(references, "Reference", Include=dll.stem)
            ET.SubElement(reference, "HintPath").text = str(dll)
        project.write(host / "StepCorpusHarness.csproj", encoding="utf-8")
        result = common.build(host / "StepCorpusHarness.csproj", "profile-host", folder / "host-build.log")
        if result["exit"]:
            raise RuntimeError("Profile host failed to build")
        common.write_json(folder / "host-build.json", result)
    for name in variants:
        verified = json.loads((work / name / "result.json").read_text())
        if verified["host_build"]["exit"] or verified["stub_check"]["exit"]:
            raise ValueError(f"Refusing to profile invalid variant {name}")
        for dll, expected in verified["binary_hashes"].items():
            if hashlib.sha256((work / name / "bin" / dll).read_bytes()).hexdigest() != expected:
                raise ValueError(f"Binary changed before profiling: {name}/{dll}")
        binary = folder / name / "bin"
        shutil.copytree(work / name / "bin", binary, dirs_exist_ok=True)
        for suffix in ("dll", "pdb"):
            shutil.copy2(host / f"bin/Release/net8.0/step-corpus-harness.{suffix}", binary)
        common.write_json(binary.parent / "binary-hashes.json", {
            p.name: hashlib.sha256(p.read_bytes()).hexdigest() for p in binary.glob("*.dll")})
    trace = Path.home() / ".dotnet/tools/dotnet-trace.exe"
    reader = ROOT / "tools/experiments/AllocationProfile/bin/Release/net8.0/AllocationProfile.dll"
    common.write_json(folder / "configuration.json", {
        "tool": subprocess.check_output([trace, "--version"], text=True).strip(),
        "profile": "gc-verbose", "provider": "Plato-Experiment-Phases",
        "analysis": "GC allocation tick bytes, second run, tables/edges/faces/refine/assemble",
        "reader_sha256": hashlib.sha256(reader.read_bytes()).hexdigest(),
        "timings": "Traced runs excluded from performance samples"})
    results = json.loads((folder / "results.json").read_text()) if (folder / "results.json").exists() else []
    for case in ("ODIN-W160.STEP", "ELLA-W1.STEP", "JODY-W3.STEP"):
        source = next(corpus.rglob(case))
        for name in variants:
            if any(r["variant"] == name and r["case"] == case for r in results):
                continue
            dest = folder / name
            prefix = dest / source.stem
            nettrace = prefix.with_suffix(".nettrace")
            result = common.run([trace, "collect", "--profile", "gc-verbose",
                "--providers", "Plato-Experiment-Phases", "--output", nettrace,
                "--", "dotnet", dest / "bin/step-corpus-harness.dll", "time", source,
                prefix, "--warmup"], prefix.with_suffix(".trace.log"), timeout=600)
            result.update(variant=name, case=case)
            if result["exit"]:
                raise RuntimeError(f"Trace failed: {name}/{case}")
            result["analysis"] = common.run(["dotnet", reader, nettrace], prefix.with_suffix(".allocations.json"))
            if result["analysis"]["exit"]:
                raise RuntimeError(f"Trace analysis failed: {name}/{case}")
            analysis = json.loads(prefix.with_suffix(".allocations.json").read_text(encoding="utf-8-sig"))
            if analysis["eventsLost"] or analysis["runs"] != 2:
                raise RuntimeError(f"Incomplete allocation trace: {name}/{case}")
            results.append(result)
            common.write_json(folder / "results.json", results)
            print(f"Profiled {name} {case}", flush=True)


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("action", choices=["bootstrap", "generate", "measure", "profile"])
    parser.add_argument("--work", type=Path, required=True)
    parser.add_argument("--previous", type=Path)
    parser.add_argument("--variants", nargs="+", choices=POLICIES, default=list(POLICIES))
    parser.add_argument("--corpus", type=Path)
    parser.add_argument("--repeats", type=int, default=3)
    parser.add_argument("--run-name", default="runs-tiered")
    args = parser.parse_args()
    work = args.work.resolve()
    if not work.is_relative_to(ROOT / ".temp"):
        parser.error("Experiment output must be inside this checkout's .temp directory")
    if args.action == "bootstrap":
        if args.previous is None:
            parser.error("bootstrap needs --previous with the compiler-499 work snapshot")
        bootstrap(work, args.previous.resolve())
    elif args.action == "generate":
        generate(work, args.variants)
    else:
        if args.corpus is None or args.repeats < 1 or not re.fullmatch(r"[A-Za-z0-9_-]+", args.run_name):
            parser.error("measure needs --corpus, positive repeats and a simple run directory name")
        import ctypes
        api = ctypes.WinDLL("kernel32", use_last_error=True).SetThreadExecutionState
        api.argtypes = [ctypes.c_uint]
        api.restype = ctypes.c_uint
        previous = api(0x80000001)
        if not previous:
            raise OSError("Cannot prevent idle sleep during measurements")
        try:
            if args.action == "profile":
                profile(work, args.variants, args.corpus)
            else:
                common.measure(work, args.variants, args.corpus, args.repeats, True, args.run_name)
        finally:
            api(previous)


if __name__ == "__main__":
    main()
