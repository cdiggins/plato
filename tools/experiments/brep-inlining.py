"""Isolated inlining experiments; see docs/reports/brep-inlining-2026-10-24.md.

Requires an exported plato-brep tree at WORK/brep and regen-compiler.ps1's JSON
result at WORK/compiler.json. Generated code and experimental compilers stay
under WORK; production sources and optimizer defaults are unchanged. Builds use
the repository's log recorder, which also updates its working status snapshot.
"""
import argparse
import csv
import fnmatch
import hashlib
import json
import os
import platform
from pathlib import Path
import random
import re
import shutil
import statistics
import subprocess
import time
import xml.etree.ElementTree as ET

ROOT = Path(__file__).resolve().parents[2]
VARIANTS = {
    "baseline": (120, 8, 4),
    "default": (120, 8, 4),
    "large": (480, 16, 4),
    "huge": (2000, 32, 4),
    "duplicate": (2000, 32, 32),
}


def write_json(path, value):
    path.write_text(json.dumps(value, indent=2) + "\n", encoding="utf-8")


def run(args, log, timeout=600, env=None):
    start = time.perf_counter()
    cpu_seconds = None
    with log.open("w", encoding="utf-8") as output:
        p = subprocess.Popen([str(x) for x in args], cwd=ROOT, stdout=output,
                             stderr=subprocess.STDOUT, env=env)
        try:
            code = p.wait(timeout=timeout)
        except subprocess.TimeoutExpired:
            p.kill()
            p.wait()
            code = -1
        if os.name == "nt":
            import ctypes
            from ctypes import wintypes
            times = [wintypes.FILETIME() for _ in range(4)]
            api = ctypes.WinDLL("kernel32", use_last_error=True).GetProcessTimes
            api.argtypes = [wintypes.HANDLE] + [ctypes.POINTER(wintypes.FILETIME)] * 4
            api.restype = wintypes.BOOL
            if api(wintypes.HANDLE(int(p._handle)), *(ctypes.byref(t) for t in times)):
                cpu_seconds = sum((t.dwHighDateTime << 32) + t.dwLowDateTime for t in times[2:]) / 1e7
    return {"exit": code, "seconds": time.perf_counter() - start, "cpu_seconds": cpu_seconds, "log": str(log)}


def build(project, name, log):
    # Snapshots live below a shared Git checkout. Its HEAD can move during a run;
    # do not let SDK-generated version/SourceLink metadata change unrelated DLLs.
    env = os.environ.copy()
    env.update(EnableSourceControlManagerQueries="false", EnableSourceLink="false",
               IncludeSourceRevisionInInformationalVersion="false")
    return run(["powershell", "-NoProfile", "-ExecutionPolicy", "Bypass", "-File",
                ROOT / "tools/dotnet-build-record.ps1", "-Project", project,
                "-TargetName", "inline-brep-" + name], log, env=env)


def build_host(work, name, result):
    dest = work / name
    target = work / "brep/generated/Ara3D.StepBrep/Generated"
    target.mkdir(exist_ok=True)
    files = list((dest / "generated").glob("*.g.cs"))
    if not files:
        raise ValueError(f"No generated files for {name}")
    names = {p.name for p in files}
    for p in target.glob("*.cs"):
        if p.name not in names:
            p.unlink()
    for p in files:
        text = p.read_text(encoding="utf-8-sig").replace("Ara3D.Geometry", "Ara3D.StepBrep")
        output = target / p.name
        if not output.exists() or output.read_text(encoding="utf-8") != text:
            output.write_text(text, encoding="utf-8")
    result["host_build"] = build(work / "brep/tools/step-corpus-harness/StepCorpusHarness.csproj", name + "-host", dest / "host-build.log")
    if result["host_build"]["exit"] == 0 and result["stub_check"]["exit"] == 0:
        shutil.copytree(work / "brep/tools/step-corpus-harness/bin/Release/net8.0", dest / "bin", dirs_exist_ok=True)
        result["assembly_bytes"] = (dest / "bin/Ara3D.StepBrep.dll").stat().st_size
        if name != "baseline":
            freeze_host(work, name, result)
        result["binary_hashes"] = {p.name: hashlib.sha256(p.read_bytes()).hexdigest() for p in (dest / "bin").glob("*.dll")}
    write_json(dest / "result.json", result)
    print(json.dumps(result), flush=True)


def freeze_host(work, name, result):
    if result["host_build"]["exit"] or result["stub_check"]["exit"]:
        raise ValueError(f"Cannot prepare invalid variant {name}")
    for source in (work / "baseline/bin").iterdir():
        if source.is_file() and source.name not in ("Ara3D.StepBrep.dll", "Ara3D.StepBrep.pdb"):
            shutil.copy2(source, work / name / "bin" / source.name)
    result["host_policy"] = "Frozen baseline host and dependencies; only Ara3D.StepBrep.dll differs"
    result["binary_hashes"] = {p.name: hashlib.sha256(p.read_bytes()).hexdigest() for p in (work / name / "bin").glob("*.dll")}
    write_json(work / name / "result.json", result)


def prepare(work):
    info = json.loads((work / "compiler.json").read_text(encoding="utf-8-sig"))
    plato = Path(info["Plato"])
    compiler = work / "cli"
    if not compiler.exists():
        shutil.copytree(Path(info["Cli"]).parent, compiler)
    writer = work / "writer"
    if not writer.exists():
        shutil.copytree(plato / "writers/Plato.CSharpWriter", writer,
                        ignore=shutil.ignore_patterns("bin", "obj"))
    references = "\n".join(f'<Reference Include="{p.stem}"><HintPath>{p}</HintPath></Reference>'
                           for p in compiler.glob("*.dll")
                           if p.stem.startswith("Ara3D.") or p.stem in ("Plato.Compiler", "Plato.AST"))
    (writer / "Plato.CSharpWriter.csproj").write_text(
        '<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><TargetFramework>net8.0</TargetFramework>'
        '<NuGetAudit>false</NuGetAudit></PropertyGroup><ItemGroup>' + references + '</ItemGroup></Project>')
    inputs = work / "inputs"
    if not inputs.exists():
        inputs.mkdir()
        lines = (work / "brep/library/tools/shipped-program.txt").read_text().splitlines()
        records = [line.split("#")[0].split() for line in lines]
        excluded = [x for r in records if r and r[0] == "exclude" for x in r[1:]]
        for r in records:
            if not r or r[0] not in ("stdlib", "layer"):
                continue
            source = plato / "stdlib" / r[1] if r[0] == "stdlib" else work / "brep/library" / r[1]
            dest = inputs / r[1]
            dest.mkdir()
            for p in source.glob("*.plato"):
                patterns = excluded if r[0] == "stdlib" else ["*.spec.plato"]
                if not any(fnmatch.fnmatch(p.name, pattern) for pattern in patterns):
                    shutil.copy2(p, dest / p.name)
    runtime = work / "brep/generated/Ara3D.StepBrep/Intrinsics"
    runtime.mkdir(exist_ok=True)
    for p in runtime.glob("*.cs"):
        p.unlink()
    tree = ET.parse(plato / "src/Plato.Intrinsics/Plato.Intrinsics.projitems")
    for element in tree.iter():
        if element.tag.split("}")[-1] == "Compile":
            name = element.attrib["Include"].replace("$(MSBuildThisFileDirectory)", "")
            source = plato / "src/Plato.Intrinsics" / name
            (runtime / name).write_text(source.read_text(encoding="utf-8-sig").replace("Ara3D.Geometry", "Ara3D.StepBrep"), encoding="utf-8")
    return info


def generate(work, variants):
    info = prepare(work)
    original = (Path(info["Plato"]) / "writers/Plato.CSharpWriter/Optimization/TirInliner.cs").read_text()
    for name in variants:
        dest = work / name
        dest.mkdir(exist_ok=True)
        nodes, passes, cost = VARIANTS[name]
        source = original
        for key, value in [("MaxBodyNodes", nodes), ("MaxPasses", passes), ("MaxCheapCost", cost)]:
            source, count = re.subn(rf"const int {key} = \d+;", f"const int {key} = {value};", source)
            assert count == 1, key
        (work / "writer/Optimization/TirInliner.cs").write_text(source, encoding="utf-8")
        result = {"variant": name, "body_nodes": nodes, "passes": passes, "cheap_cost_exclusive": cost}
        result["compiler_build"] = build(work / "writer/Plato.CSharpWriter.csproj", name + "-writer", dest / "writer-build.log")
        if result["compiler_build"]["exit"]:
            write_json(dest / "result.json", result)
            continue
        shutil.copy2(work / "writer/bin/Release/net8.0/Plato.CSharpWriter.dll", work / "cli/Plato.CSharpWriter.dll")
        output = dest / "generated"
        output.mkdir(exist_ok=True)
        flags = ["--loops", "--fuse"] + ([] if name == "baseline" else ["--inline", "--inline-report"])
        result["flags"] = flags
        result["generation"] = run(["dotnet", work / "cli/Plato.CLI.dll", *sorted((work / "inputs").iterdir()),
                                    f"--out={output}", "--profile=cpu64", *flags], dest / "generate.log")
        if result["generation"]["exit"]:
            write_json(dest / "result.json", result)
            continue
        result["stub_check"] = run(["python", "-I", "-S", work / "brep/library/tools/check_generated.py",
                                    output, "--log", dest / "generate.log"], dest / "stub-check.log")
        files = list(output.glob("*.g.cs"))
        result["source_bytes"] = sum(p.stat().st_size for p in files)
        log = (dest / "generate.log").read_text(encoding="utf-8", errors="replace")
        result["inlining_totals"] = re.findall(r"(?:calls inlined|beta reductions|tuple ctor rewrites):\s+\d+", log)
        # Build even failed stub checks to retain the diagnostic evidence; never benchmark them.
        build_host(work, name, result)


def partial(work, variants):
    """Use one common fallback set so every budget is compared on the same files."""
    failed = set()
    for name in variants:
        log = ROOT / f".temp/csharp-build-logs/inline-brep-{name}-host.log"
        text = log.read_text(encoding="utf-8-sig")
        errors = [line for line in text.splitlines() if ": error " in line]
        for line in errors:
            match = re.search(r"[\\/]Generated[\\/]([^\\/]+\.g\.cs)\(\d+,\d+\): error CS\d+:", line)
            if not match:
                raise ValueError(f"Not a generated-body error; cannot fall back: {line}")
            failed.add(match[1])
        result = json.loads((work / name / "result.json").read_text())
        if result["host_build"]["exit"] and not errors:
            raise ValueError(f"Build of {name} failed without usable compiler diagnostics")
    if not failed:
        raise ValueError("No generated errors to fall back from")
    for name in variants:
        dest = work / (name + "-partial")
        shutil.copytree(work / name / "generated", dest / "generated", dirs_exist_ok=True)
        for file in failed:
            shutil.copy2(work / "baseline/generated" / file, dest / "generated" / file)
        result = json.loads((work / name / "result.json").read_text())
        result.update(variant=name + "-partial", original_variant=name, fallback_files=sorted(failed),
                      fallback_reason="Retain identical baseline files for the union of generated C# errors across all compared policies")
        result["source_bytes"] = sum(p.stat().st_size for p in (dest / "generated").glob("*.g.cs"))
        build_host(work, name + "-partial", result)


def measure(work, variants, corpus, repeats, tiered):
    cases = [next(corpus.rglob(name)) for name in ["ODIN-W160.STEP", "ELLA-W1.STEP", "JODY-W3.STEP"]]
    env = os.environ.copy()
    env["DOTNET_TieredCompilation"] = "1" if tiered else "0"
    env["DOTNET_ReadyToRun"] = "1" if tiered else "0"
    runs = work / ("runs-tiered" if tiered else "runs")
    runs.mkdir(exist_ok=True)
    for variant in variants:
        result = json.loads((work / variant / "result.json").read_text())
        if result["host_build"]["exit"] or result["stub_check"]["exit"]:
            raise ValueError(f"Refusing to benchmark invalid variant {variant}")
        for name, expected in result["binary_hashes"].items():
            actual = hashlib.sha256((work / variant / "bin" / name).read_bytes()).hexdigest()
            if actual != expected:
                raise ValueError(f"Binary changed since build: {variant}/{name}")
    baseline_result = json.loads((work / "baseline/result.json").read_text())
    for variant in variants:
        result = json.loads((work / variant / "result.json").read_text())
        for name, expected in baseline_result["binary_hashes"].items():
            if name != "Ara3D.StepBrep.dll" and result["binary_hashes"].get(name) != expected:
                raise ValueError(f"Non-treatment assembly differs from baseline: {variant}/{name}")
    write_json(runs / "configuration.json", {
        "repeats": repeats, "warmup_runs": 1, "parallel": False,
        "DOTNET_TieredCompilation": env["DOTNET_TieredCompilation"],
        "DOTNET_ReadyToRun": env["DOTNET_ReadyToRun"],
        "platform": platform.platform(), "cpu_count": os.cpu_count(),
        "dotnet_sdk": subprocess.check_output(["dotnet", "--version"], text=True).strip(),
        "dotnet_runtimes": subprocess.check_output(["dotnet", "--list-runtimes"], text=True),
        "inputs": {p.name: hashlib.sha256(p.read_bytes()).hexdigest() for p in cases},
        "random_seed": 498,
    })
    order = []
    rng = random.Random(498)
    for repeat in range(repeats):
        for case in cases:
            shuffled = variants.copy()
            rng.shuffle(shuffled)
            for variant in shuffled:
                dll = work / variant / "bin/step-corpus-harness.dll"
                if not dll.exists():
                    continue
                prefix = runs / f"{case.stem}-{repeat}-{variant}"
                result = run(["dotnet", dll, "time", case, prefix, "--warmup"], prefix.with_suffix(".log"), timeout=600, env=env)
                result.update(variant=variant, case=case.name, repeat=repeat)
                summary = prefix.with_suffix(".summary.json")
                if summary.exists():
                    result["summary"] = json.loads(summary.read_text())
                    result["summary"]["MeshingMs"] = sum(result["summary"][k] for k in
                        ("TablesMs", "EdgesMs", "FacesMs", "RefineMs", "AssembleMs"))
                    result["summary"]["ProcessCpuSeconds"] = result.get("cpu_seconds")
                    tables = {}
                    for kind in ("edges", "faces", "contract"):
                        table = prefix.with_suffix(f".{kind}.csv")
                        if table.exists():
                            with table.open(newline="", encoding="utf-8-sig") as source:
                                rows = [{k: v for k, v in row.items() if k != "ms" and not k.endswith("_ms")}
                                        for row in csv.DictReader(source)]
                            tables[kind] = hashlib.sha256(json.dumps(rows, sort_keys=True).encode()).hexdigest()
                    result["summary"]["table_hashes"] = tables
                order.append(result)
                write_json(runs / "runs.json", order)
                print(f'{variant} {case.name} repeat {repeat}: {result["exit"]}, {result["seconds"]:.1f}s', flush=True)
    rows = []
    for case in cases:
        baseline = [r["summary"] for r in order if r["variant"] == "baseline" and r["case"] == case.name and "summary" in r]
        for variant in variants:
            samples = [r["summary"] for r in order if r["variant"] == variant and r["case"] == case.name and "summary" in r]
            if not samples:
                continue
            fields = ["Status", "Digest", "Check", "FacesMeshed", "EdgesThrew", "FacesThrew", "Failures", "Contract", "Boundary", "ClosedShellsClosed", "table_hashes"]
            row = {"case": case.name, "variant": variant, "samples": len(samples),
                   "matches_baseline": bool(baseline) and baseline[0].get("Status") == "done"
                   and all(s.get(k) == baseline[0].get(k) for s in samples for k in fields)}
            for field in ["MeshingMs", "TotalMs", "PrepareMs", "TablesMs", "EdgesMs", "FacesMs", "RefineMs", "AssembleMs", "AllocatedBytes", "ProcessCpuSeconds"]:
                values = [s.get(field) for s in samples]
                if all(v is not None for v in values):
                    row[field] = {"median": statistics.median(values), "min": min(values), "max": max(values)}
            rows.append(row)
    write_json(runs / "summary.json", rows)


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("action", choices=["generate", "rebuild", "partial", "freeze-host", "measure"])
    parser.add_argument("--work", type=Path, default=ROOT / ".temp/inline-brep")
    parser.add_argument("--variants", nargs="+", choices=list(VARIANTS) + [n + "-partial" for n in VARIANTS if n != "baseline"], default=list(VARIANTS))
    parser.add_argument("--corpus", type=Path)
    parser.add_argument("--repeats", type=int, default=3)
    parser.add_argument("--tiered", action="store_true")
    args = parser.parse_args()
    args.work = args.work.resolve()
    if args.action in ("generate", "partial") and any(n not in VARIANTS for n in args.variants):
        parser.error("generate and partial accept original policy names only")
    if args.repeats < 1:
        parser.error("repeats must be positive")
    if args.action == "generate":
        generate(args.work, args.variants)
    elif args.action == "rebuild":
        for name in args.variants:
            build_host(args.work, name, json.loads((args.work / name / "result.json").read_text()))
    elif args.action == "partial":
        partial(args.work, [n for n in args.variants if n != "baseline"])
    elif args.action == "freeze-host":
        for name in args.variants:
            if name != "baseline":
                freeze_host(args.work, name, json.loads((args.work / name / "result.json").read_text()))
    else:
        if args.corpus is None:
            parser.error("measure requires --corpus")
        measure(args.work, args.variants, args.corpus, args.repeats, args.tiered)
