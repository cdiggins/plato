"""Isolated inlining experiments; see docs/reports/brep-inlining-2026-10-24.md.

Requires an exported plato-brep tree at WORK/brep and regen-compiler.ps1's JSON
result at WORK/compiler.json. Generated code and experimental compilers stay
under WORK; production sources and optimizer defaults are unchanged. Builds use
the repository's log recorder, which also updates its working status snapshot.
"""
import argparse
import csv
from collections import Counter, defaultdict
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
    "baseline-opt": (120, 8, 4),
    "default-opt": (120, 8, 4),
    "duplicate-opt": (2000, 32, 32),
}


def write_json(path, value):
    path.write_text(json.dumps(value, indent=2) + "\n", encoding="utf-8")


def run(args, log, timeout=600, env=None):
    started_at_unix = time.time()
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
    return {"exit": code, "started_at_unix": started_at_unix, "seconds": time.perf_counter() - start, "cpu_seconds": cpu_seconds, "log": str(log)}


def build(project, name, log):
    # Snapshots live below a shared Git checkout. Its HEAD can move during a run;
    # do not let SDK-generated version/SourceLink metadata change unrelated DLLs.
    env = os.environ.copy()
    env.update(EnableSourceControlManagerQueries="false", EnableSourceLink="false",
               IncludeSourceRevisionInInformationalVersion="false")
    return run(["powershell", "-NoProfile", "-ExecutionPolicy", "Bypass", "-File",
                ROOT / "tools/dotnet-build-record.ps1", "-Project", project,
                "-TargetName", log.parent.parent.name + "-" + name], log, env=env)


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


def add_mesh_timer(work):
    """Include work between the existing harness clocks; patch only the exported host."""
    folder = work / "brep/tools/step-corpus-harness"
    path = folder / "TimedMesh.cs"
    source = path.read_text(encoding="utf-8-sig")
    if "MeshingElapsedMs" not in source:
        source = source.replace("    public double TotalMs", "    public double MeshingElapsedMs { get; init; }\n\n    public double TotalMs", 1)
        start = "var allocatedBefore = GC.GetTotalAllocatedBytes(precise: true);"
        end = "var allocatedBytes = GC.GetTotalAllocatedBytes(precise: true) - allocatedBefore;"
        assert start in source and end in source and "contractMs, allocatedBytes);" in source
        source = source.replace(start, start + "\n        var meshingStart = System.Diagnostics.Stopwatch.GetTimestamp();", 1)
        source = source.replace(end, "var meshingElapsedMs = System.Diagnostics.Stopwatch.GetElapsedTime(meshingStart).TotalMilliseconds;\n        " + end, 1)
        source = source.replace("contractMs, allocatedBytes);", "contractMs, allocatedBytes) { MeshingElapsedMs = meshingElapsedMs };", 1)
        path.write_text(source, encoding="utf-8")
    path = folder / "Summary.cs"
    source = path.read_text(encoding="utf-8-sig")
    if "MeshingElapsedMs" not in source:
        assert "    static readonly JsonSerializerOptions" in source and "t.ContractMs, t.AllocatedBytes);" in source
        source = source.replace("    static readonly JsonSerializerOptions", "    public double MeshingElapsedMs { get; init; }\n\n    static readonly JsonSerializerOptions", 1)
        source = source.replace("t.ContractMs, t.AllocatedBytes);", "t.ContractMs, t.AllocatedBytes) { MeshingElapsedMs = t.MeshingElapsedMs };", 1)
        path.write_text(source, encoding="utf-8")
    write_json(work / "host-instrumentation.json", {
        "metric": "MeshingElapsedMs", "scope": "tables through assembly, including inter-stage work; excludes prepare and contract checks",
        "source_hashes": {n: hashlib.sha256((folder / n).read_bytes()).hexdigest() for n in ("TimedMesh.cs", "Summary.cs")}
    })


def prepare(work):
    add_mesh_timer(work)
    info = json.loads((work / "compiler.json").read_text(encoding="utf-8-sig"))
    plato = Path(info["Plato"])
    compiler = work / "cli"
    if not compiler.exists():
        shutil.copytree(Path(info["Cli"]).parent, compiler)
    writer = work / "writer"
    if not writer.exists():
        shutil.copytree(plato / "writers/Plato.CSharpWriter", writer,
                        ignore=shutil.ignore_patterns("bin", "obj"))
    shutil.copy2(plato / "writers/Plato.CSharpWriter/CSharpWriter.cs", writer / "CSharpWriter.cs")
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


def instrument_stages(work, plato):
    """Record typed-tree counts in the isolated compiler, never production sources."""
    path = work / "writer/CSharpWriter.cs"
    source = "using System.Linq;\n" + (plato / "writers/Plato.CSharpWriter/CSharpWriter.cs").read_text(encoding="utf-8-sig")
    start = "            if (TirDumpDir == null)\n            {"
    probe = r'''
                var evidence = System.Environment.GetEnvironmentVariable("PLATO_INLINE_STAGE_EVIDENCE");
                void RecordStage(string stage)
                {
                    if (evidence == null || tir?.Body == null) return;
                    var nodes = tir.Body.Descendants().ToList();
                    var row = new {
                        owner = fi?.OwnerType?.Name, function = fi?.Name ?? tir.Original?.Name, stage,
                        nodes = nodes.GroupBy(n => n.GetType().Name).ToDictionary(g => g.Key, g => g.Count()),
                        calls = nodes.OfType<TirCall>().GroupBy(n => n.Name ?? "<unknown>")
                            .ToDictionary(g => g.Key, g => g.Count())
                    };
                    System.IO.File.AppendAllText(evidence, System.Text.Json.JsonSerializer.Serialize(row) + "\n");
                }
                RecordStage("input");
'''
    assert start in source
    source = source.replace(start, start + probe, 1)
    for call, stage in [("TirInliner.Inline(tir, this, fi?.OwnerType?.Name, out _)", "inline"),
                        ("TirFusion.Rewrite(tir, this)", "fusion"),
                        ("TirComponentUnroller.UnrollFunction(tir, fi, this)", "components"),
                        ("TirArrayMaterializer.Rewrite(tir, this)", "materialization"),
                        ("TirLoopLowerer.Rewrite(tir, this)", "loops")]:
        statement = "tir = " + call + ";"
        assert statement in source
        source = source.replace(statement, statement + f' RecordStage("{stage}");', 1)
    path.write_text(source, encoding="utf-8")


def generate(work, variants, stage_evidence=False):
    info = prepare(work)
    if stage_evidence:
        instrument_stages(work, Path(info["Plato"]))
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
            raise RuntimeError(f"Experimental compiler build failed: {name}")
        shutil.copy2(work / "writer/bin/Release/net8.0/Plato.CSharpWriter.dll", work / "cli/Plato.CSharpWriter.dll")
        output = dest / "generated"
        output.mkdir(exist_ok=True)
        flags = ["--loops", "--fuse"] + ([] if name.startswith("baseline") else ["--inline", "--inline-report"])
        if name.endswith("-opt"):
            flags.append("--optimize")
        env = os.environ.copy()
        if stage_evidence:
            evidence = dest / "stages.jsonl"
            evidence.write_text("")
            env["PLATO_INLINE_STAGE_EVIDENCE"] = str(evidence)
            result["stage_evidence"] = str(evidence)
        result["flags"] = flags
        result["generation"] = run(["dotnet", work / "cli/Plato.CLI.dll", *sorted((work / "inputs").iterdir()),
                                    f"--out={output}", "--profile=cpu64", *flags], dest / "generate.log", env=env)
        if result["generation"]["exit"]:
            write_json(dest / "result.json", result)
            continue
        result["stub_check"] = run(["python", "-I", "-S", work / "brep/library/tools/check_generated.py",
                                    output, "--log", dest / "generate.log"], dest / "stub-check.log")
        files = list(output.glob("*.g.cs"))
        result["source_bytes"] = sum(p.stat().st_size for p in files)
        log = (dest / "generate.log").read_text(encoding="utf-8", errors="replace")
        result["inlining_totals"] = re.findall(r"(?:calls inlined|beta reductions|tuple ctor rewrites):\s+\d+", log)
        if stage_evidence:
            totals = defaultdict(lambda: {"functions": 0, "nodes": Counter(), "calls": Counter()})
            for line in evidence.read_text().splitlines():
                row = json.loads(line)
                stage = totals[row["stage"]]
                stage["functions"] += 1
                stage["nodes"].update(row["nodes"])
                stage["calls"].update(row["calls"])
            result["stage_totals"] = dict(totals)
        # Build even failed stub checks to retain the diagnostic evidence; never benchmark them.
        build_host(work, name, result)


def partial(work, variants):
    """Use one common fallback set so every budget is compared on the same files."""
    failed = set()
    for name in variants:
        log = ROOT / f".temp/csharp-build-logs/{work.name}-{name}-host.log"
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


def set_process_affinity(mask):
    """Temporary process-local affinity, inherited by measured child processes."""
    if os.name != "nt":
        raise ValueError("--affinity currently requires Windows")
    import ctypes
    kernel = ctypes.WinDLL("kernel32", use_last_error=True)
    get = kernel.GetProcessAffinityMask
    get.argtypes = [ctypes.c_void_p, ctypes.POINTER(ctypes.c_size_t), ctypes.POINTER(ctypes.c_size_t)]
    get.restype = ctypes.c_int
    current, system = ctypes.c_size_t(), ctypes.c_size_t()
    if not get(ctypes.c_void_p(-1), ctypes.byref(current), ctypes.byref(system)):
        raise ctypes.WinError(ctypes.get_last_error())
    if not mask or mask & ~system.value:
        raise ValueError("Affinity must select available processors")
    set_mask = kernel.SetProcessAffinityMask
    set_mask.argtypes = [ctypes.c_void_p, ctypes.c_size_t]
    set_mask.restype = ctypes.c_int
    if not set_mask(ctypes.c_void_p(-1), mask):
        raise ctypes.WinError(ctypes.get_last_error())
    return current.value


def measure(work, variants, corpus, repeats, tiered, run_name=None, affinity=None):
    cases = [next(corpus.rglob(name)) for name in ["ODIN-W160.STEP", "ELLA-W1.STEP", "JODY-W3.STEP"]]
    env = os.environ.copy()
    env["DOTNET_TieredCompilation"] = "1" if tiered else "0"
    env["DOTNET_ReadyToRun"] = "1" if tiered else "0"
    runs = work / (run_name or ("runs-tiered" if tiered else "runs"))
    if (runs / "runs.json").exists():
        raise ValueError("Measurement results already exist; select a fresh --run-name")
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
        "started_at_unix": time.time(),
        "repeats": repeats, "warmup_runs": 1, "parallel": False,
        "DOTNET_TieredCompilation": env["DOTNET_TieredCompilation"],
        "DOTNET_ReadyToRun": env["DOTNET_ReadyToRun"],
        "platform": platform.platform(), "cpu_count": os.cpu_count(),
        "affinity_mask": hex(affinity) if affinity is not None else None,
        "affinity_cpu_count": affinity.bit_count() if affinity is not None else None,
        "dotnet_sdk": subprocess.check_output(["dotnet", "--version"], text=True).strip(),
        "dotnet_runtimes": subprocess.check_output(["dotnet", "--list-runtimes"], text=True),
        "inputs": {p.name: hashlib.sha256(p.read_bytes()).hexdigest() for p in cases},
        "random_seed": 498,
        "host_instrumentation": json.loads((work / "host-instrumentation.json").read_text()) if (work / "host-instrumentation.json").exists() else None,
        "idle_sleep_prevention": "Windows ES_SYSTEM_REQUIRED for measurement process lifetime",
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
            for field in ["MeshingElapsedMs", "MeshingMs", "TotalMs", "PrepareMs", "TablesMs", "EdgesMs", "FacesMs", "RefineMs", "AssembleMs", "AllocatedBytes", "ProcessCpuSeconds"]:
                values = [s.get(field) for s in samples]
                if all(v is not None for v in values):
                    row[field] = {"median": statistics.median(values), "min": min(values), "max": max(values)}
            rows.append(row)
    write_json(runs / "summary.json", rows)
    complete = (len(order) == len(cases) * len(variants) * repeats
                and len(rows) == len(cases) * len(variants)
                and all(r["samples"] == repeats for r in rows))
    if not complete or any(r["exit"] != 0 for r in order) or not all(r["matches_baseline"] for r in rows):
        raise RuntimeError("Incomplete or non-equivalent measurement; see runs.json and summary.json")


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("action", choices=["generate", "rebuild", "partial", "freeze-host", "measure"])
    parser.add_argument("--work", type=Path, default=ROOT / ".temp/inline-brep")
    parser.add_argument("--variants", nargs="+", choices=list(VARIANTS) + [n + "-partial" for n in VARIANTS if n != "baseline"], default=["baseline", "default", "large", "huge", "duplicate"])
    parser.add_argument("--corpus", type=Path)
    parser.add_argument("--repeats", type=int, default=3)
    parser.add_argument("--tiered", action="store_true")
    parser.add_argument("--stage-evidence", action="store_true")
    parser.add_argument("--run-name")
    parser.add_argument("--affinity", type=lambda s: int(s, 0))
    args = parser.parse_args()
    args.work = args.work.resolve()
    if args.run_name and not re.fullmatch(r"[A-Za-z0-9_-]+", args.run_name):
        parser.error("--run-name must be a simple directory name")
    if args.action in ("generate", "partial") and any(n not in VARIANTS for n in args.variants):
        parser.error("generate and partial accept original policy names only")
    if args.repeats < 1:
        parser.error("repeats must be positive")
    if args.action == "generate":
        generate(args.work, args.variants, args.stage_evidence)
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
        previous_affinity = set_process_affinity(args.affinity) if args.affinity is not None else None
        previous_execution_state = None
        if os.name == "nt":
            import ctypes
            api = ctypes.WinDLL("kernel32", use_last_error=True).SetThreadExecutionState
            api.argtypes = [ctypes.c_uint]
            api.restype = ctypes.c_uint
            previous_execution_state = api(0x80000001)  # continuous + system required; display may sleep
            if not previous_execution_state:
                raise OSError("Cannot request idle-sleep prevention for measurements")
        try:
            measure(args.work, args.variants, args.corpus, args.repeats, args.tiered, args.run_name, args.affinity)
        finally:
            if previous_execution_state is not None:
                api(previous_execution_state)
            if previous_affinity is not None:
                set_process_affinity(previous_affinity)
