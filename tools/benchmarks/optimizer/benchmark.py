"""Prepared-corpus benchmark command line; see README.md for measurement boundaries."""
from __future__ import annotations

import argparse
import contextlib
import ctypes
import datetime as dt
import hashlib
import json
import math
import os
from pathlib import Path
import platform
import random
import re
import subprocess
import sys
import time

HERE = Path(__file__).resolve().parent
ROOT = HERE.parents[2]
RUNTIME = {"DOTNET_TieredCompilation": "1", "DOTNET_TieredPGO": "1", "DOTNET_ReadyToRun": "1"}
MODES = {"quick": (120, 3, 75, 6), "confirm": (600, 7, 150, 12), "corpus": (1800, 7, 150, None)}


def read_json(path):
    return json.loads(Path(path).read_text(encoding="utf-8-sig"), parse_constant=reject_constant)


def reject_constant(value):
    raise ValueError(f"Nonfinite JSON number: {value}")


def write_json(path, value):
    path = Path(path)
    path.parent.mkdir(parents=True, exist_ok=True)
    temporary = path.with_suffix(path.suffix + ".tmp")
    temporary.write_text(json.dumps(value, indent=2, allow_nan=False) + "\n", encoding="utf-8")
    temporary.replace(path)


def file_hash(path):
    with Path(path).open("rb") as stream:
        return hashlib.file_digest(stream, "sha256").hexdigest()


def binary_identity(worker):
    from prepare import file_hashes
    return file_hashes(Path(worker).resolve().parent)


def verify_build_artifacts(builds):
    identities, hosts, workspaces = {}, [], set()
    for build in builds:
        if build.get("status") != "ready":
            continue
        actual = binary_identity(build["worker"])
        if not build.get("output_hashes") or actual != build["output_hashes"]:
            raise ValueError(f"Build artifacts changed or lack a receipt: {build['name']}")
        host = {k: v for k, v in actual.items() if k not in ("Ara3D.StepBrep.dll", "Ara3D.StepBrep.pdb")}
        if not build.get("host_hashes") or host != build["host_hashes"]:
            raise ValueError(f"Host artifacts disagree with their receipt: {build['name']}")
        hosts.append(host)
        workspaces.add(build.get("workspace_key"))
        identities[build["name"]] = actual
    if hosts and any(h != hosts[0] for h in hosts):
        raise ValueError("Host dependencies differ across optimizer recipes")
    if len(workspaces) > 1 or None in workspaces:
        raise ValueError("Recipe artifacts belong to different or unidentified workspaces")
    return identities


def verify_suite_preparation(suite, identities):
    expected = suite.get("preparation_binary_identity")
    if not expected or expected not in identities.values():
        raise ValueError("Suite preparation artifacts are not among the verified builds; prepare fixtures again")
    for case in suite["cases"]:
        if not case.get("fixture_hash") or file_hash(case["fixture"]) != case["fixture_hash"]:
            raise ValueError(f"Fixture changed or lacks its preparation hash: {case['id']}")
        if case.get("preparation_identity") != expected.get("Ara3D.StepBrep.dll"):
            raise ValueError(f"Fixture library identity differs from suite preparation: {case['id']}")


def complete_planned_rows(rows, metadata):
    """Reconstruct absent planned samples so a truncated log cannot look complete."""
    planned = {(block, variant["name"], case["id"], workload)
               for block in range(metadata["parameters"]["blocks"])
               for variant in metadata["variants"] for case in metadata["selection"]["cases"]
               for workload in case["workloads"]}
    observed = {(r.get("block"), r.get("variant"), r.get("case_id"), r.get("workload"))
                for r in rows if isinstance(r, dict)}
    if observed - planned:
        raise ValueError("Saved samples contain identities outside the recorded run plan")
    result = list(rows)
    for block, variant, case_id, workload in sorted(planned - observed):
        result.append(dict(schema=1, block=block, variant=variant, case_id=case_id, workload=workload,
                           status="not_measured", error="Missing planned sample in saved data"))
    return result


def actual_runtime_identity(rows):
    identities = set()
    for row in rows:
        if row.get("status") != "ok":
            continue
        runtime = row.get("runtime")
        if not isinstance(runtime, dict) or not runtime:
            raise ValueError("Successful samples lack actual runtime identity")
        identities.add(json.dumps(runtime, sort_keys=True, allow_nan=False))
    if len(identities) > 1:
        raise ValueError("Samples used differing actual runtimes or garbage collector settings")
    return next(iter(identities), None)


def tool_identity():
    return {name: file_hash(HERE / name) for name in ("benchmark.py", "analysis.py", "selection.py", "recipes.py", "prepare.py")}


@contextlib.contextmanager
def operation_lock(work, phase):
    path = Path(work) / ".operation.lock"
    try:
        with path.open("x", encoding="utf-8") as stream:
            json.dump({"pid": os.getpid(), "phase": phase, "started": dt.datetime.now(dt.timezone.utc).isoformat()}, stream)
    except FileExistsError:
        raise ValueError(f"Workspace operation lock exists: {path}. Check the recorded process before removing it.")
    try:
        yield
    finally:
        path.unlink()


@contextlib.contextmanager
def measurement_lock(work):
    with operation_lock(work, "measure"):
        path = Path(work) / ".measure.lock"
        try:
            with path.open("x", encoding="utf-8") as stream:
                json.dump({"pid": os.getpid()}, stream)
        except FileExistsError:
            raise ValueError(f"Measurement lock exists: {path}. Check its process before removing it.")
        try:
            yield
        finally:
            path.unlink()


def invoke_worker(worker, arguments, *, timeout, affinity=None):
    env = os.environ.copy()
    env.update(RUNTIME)
    start = time.perf_counter()
    process = subprocess.Popen(["dotnet", str(worker), *map(str, arguments)],
                               stdout=subprocess.PIPE, stderr=subprocess.PIPE, text=True,
                               encoding="utf-8", errors="replace", env=env)
    try:
        if affinity is not None:
            if os.name != "nt":
                raise ValueError("--affinity currently requires Windows")
            kernel = ctypes.WinDLL("kernel32", use_last_error=True)
            kernel.SetProcessAffinityMask.argtypes = [ctypes.c_void_p, ctypes.c_size_t]
            kernel.SetProcessAffinityMask.restype = ctypes.c_int
            if not kernel.SetProcessAffinityMask(int(process._handle), affinity):
                raise OSError(ctypes.get_last_error(), "Cannot set worker CPU affinity")
        stdout, stderr = process.communicate(timeout=timeout)
    except subprocess.TimeoutExpired:
        process.kill()
        stdout, stderr = process.communicate()
        return {"status": "timeout", "error": f"Worker exceeded {timeout:g}s", "stderr": stderr[-4000:],
                "process_seconds": time.perf_counter() - start}
    except BaseException:
        process.kill()
        process.communicate()
        raise
    try:
        result = json.loads(stdout, parse_constant=reject_constant)
        if not isinstance(result, dict):
            raise ValueError("Worker response must be an object")
        if result.get("schema") != 1:
            raise ValueError("Unsupported worker response schema")
    except (ValueError, TypeError) as error:
        result = {"status": "error", "error": f"Invalid worker response: {error}", "stdout": stdout[-4000:]}
    result["process_seconds"] = time.perf_counter() - start
    if process.returncode:
        result.update(status="error", exit_code=process.returncode)
        result.setdefault("error", "Worker exited unsuccessfully")
    else:
        result.setdefault("status", "ok")
    if stderr:
        result["stderr"] = stderr[-4000:]
    return result


def load_builds(work):
    builds = read_json(Path(work) / "builds.json")
    if not isinstance(builds, list) or not builds:
        raise ValueError("No build variants; run build first")
    names = [b["name"] for b in builds]
    if len(names) != len(set(names)):
        raise ValueError("Duplicate build variant names")
    return builds


def suite_cases(path):
    path = Path(path).resolve()
    suite = read_json(path)
    if suite.get("schema") != 1 or not suite.get("cases"):
        raise ValueError("Expected a nonempty schema:1 suite")
    ids = []
    for case in suite["cases"]:
        ids.append(case["id"])
        fixture = Path(case["fixture"])
        if not fixture.is_absolute():
            fixture = path.parent / fixture
        case["fixture"] = str(fixture.resolve(strict=True))
        if not case.get("workloads") or len(case["workloads"]) != len(set(case["workloads"])):
            raise ValueError(f"Missing or duplicate workloads for {case['id']}")
    if len(ids) != len(set(ids)):
        raise ValueError("Duplicate suite case IDs")
    return suite


def run_samples(builds, cases, output, *, blocks, seconds, target_ms, warmup,
                max_iterations, timeout, seed, affinity=None, warmup_ms=200, worker_call=invoke_worker):
    """Each repetition is a fresh-process block; budget exhaustion is a recorded state."""
    output = Path(output)
    output.parent.mkdir(parents=True, exist_ok=True)
    rng = random.Random(seed)
    start = time.perf_counter()
    rows = []
    with output.open("x", encoding="utf-8") as stream:
        for block in range(blocks):
            jobs = list(cases)
            rng.shuffle(jobs)
            for case in jobs:
                workloads = list(case["workloads"])
                rng.shuffle(workloads)
                order = list(builds)
                rng.shuffle(order)
                order.sort(key=lambda b: bool(b.get("alias_of") and any(v["name"] == b["alias_of"] for v in builds)))
                completed = {}
                for build in order:
                    remaining = seconds - (time.perf_counter() - start)
                    if build.get("status") != "ready":
                        result = dict(status=build.get("status", "error"), error=build.get("error", "Build unavailable"))
                    elif build.get("alias_of") in completed:
                        result = {**completed[build["alias_of"]], "alias_of": build["alias_of"]}
                    elif remaining <= 0:
                        result = dict(status="not_measured", error="Run budget exhausted")
                    else:
                        result = worker_call(build["worker"], ["run-set", "--fixture", case["fixture"],
                            "--workloads", ",".join(workloads), "--warmup", warmup, "--target-ms", target_ms,
                            "--warmup-ms", warmup_ms, "--max-iterations", max_iterations],
                            timeout=min(timeout, remaining), affinity=affinity)
                    completed[build["name"]] = result
                    samples = result.get("samples", [])
                    if not isinstance(samples, list) or any(not isinstance(s, dict) for s in samples):
                        result = dict(status="error", error="Malformed worker samples; expected an array of objects")
                        samples = []
                    for workload in workloads:
                        row = {"schema": 1, "block": block, "variant": build["name"],
                               "case_id": case["id"], "workload": workload, "tags": case.get("tags", []),
                               "family": workload, "kind": "model" if workload == "model" else "stage"}
                        found = [s for s in samples if s.get("workload") == workload]
                        if result.get("status") != "ok":
                            sample = {k: v for k, v in result.items() if k != "samples"}
                        elif len(found) != 1:
                            sample = dict(status="error", error="Missing or duplicate worker workload result")
                        else:
                            sample = dict(found[0])
                            sample.setdefault("status", "ok")
                            if sample.get("schema") != 1 or sample.get("case_id") != case["id"]:
                                sample = dict(status="error", error="Worker schema or fixture identity does not match the request")
                            # Process time is shared across workloads, not part of the measured operation.
                            sample["process_seconds"] = result.get("process_seconds", 0) / len(workloads)
                            if result.get("alias_of"):
                                sample["alias_of"] = result["alias_of"]
                        for identity in ("schema", "block", "variant", "case_id", "workload", "family", "kind"):
                            sample.pop(identity, None)
                        row.update(sample)
                        rows.append(row)
                        stream.write(json.dumps(row, allow_nan=False) + "\n")
                        stream.flush()
                    states = sorted({r["status"] for r in rows[-len(workloads):]})
                    print(f"block {block + 1}/{blocks} {case['id']} {build['name']}: {','.join(states)}", file=sys.stderr)
    return rows


def command_prepare(args):
    from prepare import prepare_workspace
    work = args.work.resolve()
    work.mkdir(parents=True, exist_ok=True)
    if (work / ".measure.lock").exists():
        raise ValueError("Cannot prepare during measurement")
    with operation_lock(work, "prepare"):
        workspace = prepare_workspace(ROOT, args.brep_root.resolve(), args.cli.resolve(), work / "cache",
                                      plato_commit=args.plato_ref, brep_commit=args.brep_ref)
    write_json(work / "workspace.json", workspace)
    print(json.dumps(workspace, indent=2))


def command_build(args):
    from prepare import build_variants
    from recipes import make_recipes
    if (args.work / ".measure.lock").exists():
        raise ValueError("Cannot build during measurement")
    workspace = read_json(args.work / "workspace.json")
    variants = make_recipes(workspace["shipping_flags"], experimental=args.experimental,
                            inline_policies=args.inline_policy or (), growth_limits=args.inline_growth or ())
    if args.variants:
        names = args.variants.split(",")
        unknown = set(names) - {v["name"] for v in variants}
        if unknown:
            raise ValueError(f"Unknown variants: {sorted(unknown)}")
        variants = [v for v in variants if v["name"] in names]
    with operation_lock(args.work, "build"):
        result = build_variants(workspace, variants, HERE / "worker", ROOT / "tools/dotnet-build-record.ps1", timeout=args.timeout)
    descriptions = {v["name"]: v for v in variants}
    result = [{**descriptions[b["name"]], **b} for b in result]
    write_json(args.work / "builds.json", result)
    print(json.dumps(result, indent=2))
    return int(any(b["status"] not in ("ready", "unsupported") for b in result))


def command_fixtures(args):
    with operation_lock(args.work, "fixtures"):
        return prepare_fixtures(args)


def prepare_fixtures(args):
    builds = load_builds(args.work)
    identities = verify_build_artifacts(builds)
    build = next((b for b in builds if b["name"] == args.variant and b["status"] == "ready"), None)
    if build is None:
        raise ValueError(f"Fixture preparation variant unavailable: {args.variant}")
    output = args.output.resolve()
    if output.exists():
        raise ValueError(f"Refusing to overwrite suite: {output}")
    folder = output.parent / (output.stem + "-fixtures")
    folder.mkdir(parents=True, exist_ok=True)
    cases, errors = [], []
    sources = list(args.input or [])
    if args.corpus:
        if not args.corpus.is_dir():
            raise ValueError(f"Corpus directory does not exist: {args.corpus}")
        sources.extend(str(p.resolve()) for p in sorted(args.corpus.rglob("*"))
                       if p.is_file() and p.suffix.lower() in (".step", ".stp"))
    sources = list(dict.fromkeys(sources))
    if args.max_inputs:
        sources = sources[:args.max_inputs]
    if not sources:
        raise ValueError("Supply --input or a nonempty --corpus")
    seen_inputs = set()
    for source in sources:
        input_identity = source if source.startswith("witness:") else file_hash(source)
        if input_identity in seen_inputs:
            continue
        seen_inputs.add(input_identity)
        stem = source[8:] if source.startswith("witness:") else Path(source).stem
        label = re.sub(r"[^A-Za-z0-9_-]+", "-", stem).strip("-")[:40] or "model"
        identity = label + "-" + hashlib.sha256(input_identity.encode()).hexdigest()[:8]
        fixture = folder / f"{identity}.json"
        if fixture.exists():
            raise ValueError(f"Fixture already exists: {fixture}")
        result = invoke_worker(build["worker"], ["prepare", "--input", source, "--output", fixture,
                               "--id", identity, "--chordal", args.chordal], timeout=args.timeout)
        if result["status"] != "ok":
            errors.append({"input": source, **result})
        else:
            if not fixture.is_file():
                raise ValueError("Worker did not write its prepared fixture")
            result.update(fixture=str(fixture), fixture_hash=file_hash(fixture), source=source)
            cases.append(result)
    suite = {"schema": 1, "cases": cases, "errors": errors,
             "preparation_binary_identity": identities[build["name"]], "preparation_variant": build["name"]}
    if verify_build_artifacts(builds) != identities:
        raise ValueError("Build artifacts changed during fixture preparation")
    write_json(output, suite)
    print(json.dumps({"suite": str(output), "prepared": len(cases), "errors": errors}, indent=2))
    return int(bool(errors) or not cases)


def command_matrix(args):
    from recipes import make_recipes
    flags = read_json(args.work / "workspace.json")["shipping_flags"] if args.work else ["--loops", "--fuse"]
    print(json.dumps(make_recipes(flags, experimental=args.experimental,
        inline_policies=args.inline_policy or (), growth_limits=args.inline_growth or ()), indent=2))


def command_catalog(args):
    builds = load_builds(args.work)
    verify_build_artifacts(builds)
    build = next((b for b in builds if b["name"] == args.variant and b["status"] == "ready"), None)
    if build is None:
        raise ValueError(f"Catalog variant unavailable: {args.variant}")
    result = invoke_worker(build["worker"], ["list"], timeout=30)
    print(json.dumps(result, indent=2))
    return int(result["status"] != "ok")


def command_discover(args):
    suite = suite_cases(args.suite)
    builds = load_builds(args.work)
    identities = verify_build_artifacts(builds)
    verify_suite_preparation(suite, identities)
    build = next((b for b in builds if b["name"] == args.variant and b["status"] == "ready"), None)
    if build is None:
        raise ValueError(f"Discovery variant unavailable: {args.variant}")
    if args.output.exists():
        raise ValueError(f"Refusing to overwrite discovery: {args.output}")
    rows_path = args.output.with_suffix(".jsonl")
    with measurement_lock(args.work):
        rows = run_samples([build], suite["cases"], rows_path, blocks=1, seconds=args.seconds,
            target_ms=1, warmup=1, max_iterations=1, timeout=args.timeout, seed=1729)
        if verify_build_artifacts(builds) != identities:
            raise ValueError("Build artifacts changed during discovery")
        verify_suite_preparation(suite, identities)
    costs, warmup_costs, operation_costs, errors = {}, {}, {}, []
    for case in suite["cases"]:
        samples = [r for r in rows if r["case_id"] == case["id"]]
        if all(r["status"] == "ok" and r.get("checks") and all(v is True for v in r["checks"].values()) for r in samples):
            costs[case["id"]] = sum(r["process_seconds"] * 1000 for r in samples)
            warmup_costs[case["id"]] = sum(r.get("warmup_elapsed_ms", 0) for r in samples)
            operation_costs[case["id"]] = sum(r["elapsed_ns"] / r["iterations"] / 1e6 for r in samples)
        else:
            errors.extend(r for r in samples if r["status"] != "ok" or not r.get("checks") or not all(v is True for v in r["checks"].values()))
    write_json(args.output, {"schema": 1, "costs": costs, "errors": errors,
        "warmup_ms": warmup_costs, "operation_ms": operation_costs,
        "suite_hash": file_hash(args.suite), "variant": args.variant,
        "cost_basis": "sum of fresh-process stage invocations including startup and fixture load"})
    print(str(args.output))
    return int(bool(errors))


def command_run(args):
    from selection import select_cases
    from analysis import analyze, render_markdown
    suite = suite_cases(args.suite)
    all_builds = load_builds(args.work)
    identities = verify_build_artifacts(all_builds)
    verify_suite_preparation(suite, identities)
    builds = all_builds
    if args.mode == "quick" and not args.variants:
        builds = [b for b in builds if b["name"] in ("shipping", "full", "unoptimized")]
    if args.variants:
        requested = [b["name"] for b in builds] if args.variants == "all" else args.variants.split(",")
        unknown = set(requested) - {b["name"] for b in builds}
        if unknown:
            raise ValueError(f"Unknown variants: {sorted(unknown)}")
        builds = [b for b in builds if b["name"] in requested]
    seconds, blocks, target, max_cases = MODES[args.mode]
    discovery = read_json(args.discovery) if args.discovery else None
    if discovery and discovery.get("suite_hash") != file_hash(args.suite):
        raise ValueError("Discovery results belong to another suite revision")
    block_count = args.blocks or blocks
    target_ms = args.target_ms or target
    requested_seconds = args.seconds or seconds
    selection_costs = None
    selection_budget = None
    if discovery:
        selection_costs = {}
        for case in suite["cases"]:
            case_id, count = case["id"], len(case["workloads"])
            if case_id not in discovery["costs"]:
                raise ValueError("Discovery lacks costs for every supplied fixture; repeat discovery or omit --discovery")
            extra_warmup = max(0, args.warmup_ms * count - discovery.get("warmup_ms", {}).get(case_id, 0),
                               (args.warmup - 1) * discovery.get("operation_ms", {}).get(case_id, 0))
            selection_costs[case_id] = 1.2 * (discovery["costs"][case_id] + extra_warmup + target_ms * count)
        if args.mode == "quick":
            unique = {b.get("alias_of", b["name"]) for b in builds if b["status"] == "ready"}
            selection_budget = requested_seconds * 1000 / max(1, len(unique) * block_count)
    selection = select_cases(suite, mode=args.mode, max_cases=args.max_cases or max_cases or len(suite["cases"]),
                             discovery=selection_costs, budget_ms=selection_budget,
                             affected=args.affected, holdout_fraction=args.holdout_fraction, seed=str(args.seed))
    if discovery:
        selection["cost_basis"] = "discovery process time plus estimated requested warmup/batch costs"
    cases = selection["cases"]
    if not cases:
        raise ValueError("Selection produced no cases")
    name = args.name or dt.datetime.now(dt.timezone.utc).strftime("%Y%m%dT%H%M%S%fZ")
    if Path(name).name != name or name in (".", ".."):
        raise ValueError("Run name must be a single directory name")
    folder = args.work / "runs" / name
    folder.mkdir(parents=True, exist_ok=False)
    identities = {b["name"]: identities[b["name"]] for b in builds if b["status"] == "ready"}
    parameters = dict(mode=args.mode, seconds=args.seconds or seconds, blocks=args.blocks or blocks,
                      target_ms=args.target_ms or target, warmup=args.warmup,
                      warmup_ms=args.warmup_ms,
                      max_iterations=args.max_iterations, timeout=args.timeout, seed=args.seed, affinity=args.affinity)
    metadata = {"schema": 1, "created_utc": dt.datetime.now(dt.timezone.utc).isoformat(),
                "baseline": args.baseline,
                "tool_identity": tool_identity(),
                "host": {"platform": platform.platform(), "processor": platform.processor(), "cpu_count": os.cpu_count()},
                "runtime_env": {**{k: v for k, v in os.environ.items() if k.startswith(("DOTNET_", "COMPlus_"))}, **RUNTIME},
                "parameters": parameters, "variants": builds, "selection": selection,
                "suite_hash": file_hash(args.suite), "binary_identity": identities}
    write_json(folder / "run.json", metadata)
    with measurement_lock(args.work):
        if {b["name"]: verify_build_artifacts([b])[b["name"]] for b in builds if b["status"] == "ready"} != identities:
            raise ValueError("Build artifacts changed before measurement")
        rows = run_samples(builds, cases, folder / "samples.jsonl", **parameters_without_mode(parameters))
        after = verify_build_artifacts(builds)
        if identities != after:
            write_json(folder / "integrity-error.json", {"before": identities, "after": after})
            raise ValueError("Binaries changed during measurement; run is invalid")
        verify_suite_preparation(suite, verify_build_artifacts(all_builds))
        if file_hash(args.suite) != metadata["suite_hash"]:
            raise ValueError("Suite manifest changed during measurement; run is invalid")
        if tool_identity() != metadata["tool_identity"]:
            raise ValueError("Benchmark tools changed during measurement; run is invalid")
    result = analyze(rows, builds, baseline=args.baseline)
    result["coverage"] = selection
    write_json(folder / "report.json", result)
    (folder / "report.md").write_text(render_markdown(result), encoding="utf-8")
    print(str(folder / "report.md"))
    return int(any(row["status"] not in ("ok", "not_measured", "unsupported") for row in rows)
               or any(row["status"] == "ok" and (not row.get("checks") or any(v is not True for v in row["checks"].values())) for row in rows)
               or bool(result.get("malformed_rows"))
               or any(c["status"] == "invalid" and "unsupported" not in c.get("observed_states", [])
                      for c in result.get("comparisons", [])))


def parameters_without_mode(parameters):
    return {k: v for k, v in parameters.items() if k != "mode"}


def command_report(args):
    from analysis import analyze, render_markdown
    rows = [json.loads(line, parse_constant=reject_constant) for line in args.samples.read_text(encoding="utf-8").splitlines() if line.strip()]
    manifest = args.samples.parent / "run.json"
    metadata = read_json(manifest) if manifest.exists() else {}
    if metadata:
        rows = complete_planned_rows(rows, metadata)
    result = analyze(rows, metadata.get("variants"), baseline=args.baseline or metadata.get("baseline", "full"))
    if metadata.get("selection"):
        result["coverage"] = metadata["selection"]
    result["planned_coverage_verified"] = bool(metadata)
    if not metadata:
        result["limitations"].append("No run manifest supplied; planned coverage cannot be verified from observed rows alone.")
    write_json(args.output.with_suffix(".json"), result)
    args.output.write_text(render_markdown(result), encoding="utf-8")
    print(args.output)


def command_validate(args):
    from analysis import validate_prediction
    quick = read_json(args.quick / "run.json")
    corpus = read_json(args.corpus / "run.json")
    for name in ("suite_hash", "runtime_env", "host", "tool_identity"):
        if quick[name] != corpus[name]:
            raise ValueError(f"Selected and corpus runs differ in {name}")
    if quick["parameters"].get("affinity") != corpus["parameters"].get("affinity"):
        raise ValueError("Selected and corpus runs used different CPU affinity")
    for variant in set(quick["binary_identity"]) & set(corpus["binary_identity"]):
        if quick["binary_identity"][variant] != corpus["binary_identity"][variant]:
            raise ValueError(f"Selected and corpus binaries differ for {variant}")
    for name in ("target_ms", "warmup", "warmup_ms", "max_iterations"):
        if quick["parameters"].get(name) != corpus["parameters"].get(name):
            raise ValueError(f"Selected and corpus measurement settings differ in {name}")
    runtime = []
    for folder in (args.quick, args.corpus):
        rows = [json.loads(line, parse_constant=reject_constant) for line in
                (folder / "samples.jsonl").read_text(encoding="utf-8").splitlines() if line.strip()]
        runtime.append(actual_runtime_identity(rows))
    if runtime[0] != runtime[1]:
        raise ValueError("Selected and corpus samples used different actual runtime settings")
    result = validate_prediction(read_json(args.quick / "report.json"),
                                 read_json(args.corpus / "report.json"), control=args.control)
    write_json(args.output, result)
    print(json.dumps(result, indent=2))


def positive_number(value):
    number = float(value)
    if not math.isfinite(number) or number <= 0:
        raise argparse.ArgumentTypeError("Expected a finite positive number")
    return number


def positive_int(value):
    number = int(value)
    if number <= 0:
        raise argparse.ArgumentTypeError("Expected a positive integer")
    return number


def parser():
    cli = argparse.ArgumentParser(description=__doc__)
    commands = cli.add_subparsers(dest="command", required=True)
    matrix = commands.add_parser("matrix", help="Display concrete recipes and dependency closures")
    matrix.add_argument("--work", type=Path)
    matrix.add_argument("--experimental", action="store_true")
    matrix.add_argument("--inline-policy", action="append")
    matrix.add_argument("--inline-growth", action="append", type=positive_int)
    matrix.set_defaults(func=command_matrix)
    catalog = commands.add_parser("catalog", help="List available witness models and supported workloads")
    catalog.add_argument("--work", type=Path, required=True)
    catalog.add_argument("--variant", default="shipping")
    catalog.set_defaults(func=command_catalog)
    prepare = commands.add_parser("prepare", help="Snapshot committed sources and compiler binaries")
    prepare.add_argument("--work", type=Path, required=True)
    prepare.add_argument("--brep-root", type=Path, default=ROOT.parent / "plato-brep")
    prepare.add_argument("--cli", type=Path, default=ROOT / "src/Plato.CLI/bin/Release/net8.0/Plato.CLI.dll")
    prepare.add_argument("--plato-ref", default="HEAD")
    prepare.add_argument("--brep-ref", default="HEAD")
    prepare.set_defaults(func=command_prepare)
    build = commands.add_parser("build", help="Generate, compile and cache variant artifacts")
    build.add_argument("--work", type=Path, required=True)
    build.add_argument("--variants", help="Comma-separated recipe names; default all")
    build.add_argument("--experimental", action="store_true")
    build.add_argument("--inline-policy", action="append")
    build.add_argument("--inline-growth", action="append", type=positive_int)
    build.add_argument("--timeout", type=positive_number, default=900)
    build.set_defaults(func=command_build)
    fixtures = commands.add_parser("fixtures", help="Parse/prepare fixture inputs outside measurement")
    fixtures.add_argument("--work", type=Path, required=True)
    fixtures.add_argument("--input", action="append")
    fixtures.add_argument("--corpus", type=Path)
    fixtures.add_argument("--max-inputs", type=positive_int)
    fixtures.add_argument("--variant", default="shipping")
    fixtures.add_argument("--output", type=Path, required=True)
    fixtures.add_argument("--chordal", type=positive_number, default=0.1)
    fixtures.add_argument("--timeout", type=positive_number, default=180)
    fixtures.set_defaults(func=command_fixtures)
    discover = commands.add_parser("discover", help="Measure setup-inclusive case costs for compact selection")
    discover.add_argument("--work", type=Path, required=True)
    discover.add_argument("--suite", type=Path, required=True)
    discover.add_argument("--output", type=Path, required=True)
    discover.add_argument("--variant", default="shipping")
    discover.add_argument("--seconds", type=positive_number, default=900)
    discover.add_argument("--timeout", type=positive_number, default=60)
    discover.set_defaults(func=command_discover)
    run = commands.add_parser("run", help="Run prepared workloads with a bounded wall-clock budget")
    run.add_argument("--work", type=Path, required=True)
    run.add_argument("--suite", type=Path, required=True)
    run.add_argument("--discovery", type=Path)
    run.add_argument("--mode", choices=MODES, default="quick")
    run.add_argument("--name")
    run.add_argument("--variants")
    run.add_argument("--baseline", default="full")
    run.add_argument("--seconds", type=positive_number)
    run.add_argument("--blocks", type=positive_int)
    run.add_argument("--target-ms", type=positive_number)
    run.add_argument("--max-cases", type=positive_int)
    run.add_argument("--warmup", type=positive_int, default=3)
    run.add_argument("--warmup-ms", type=positive_number, default=200)
    run.add_argument("--max-iterations", type=positive_int, default=100000)
    run.add_argument("--timeout", type=positive_number, default=60)
    run.add_argument("--seed", type=int, default=1729)
    run.add_argument("--affinity", type=lambda x: int(x, 0))
    run.add_argument("--holdout-fraction", type=float, default=0.2)
    run.add_argument("--affected", action="append")
    run.set_defaults(func=command_run)
    report = commands.add_parser("report", help="Analyze saved raw samples without rerunning")
    report.add_argument("--samples", type=Path, required=True)
    report.add_argument("--output", type=Path, required=True)
    report.add_argument("--baseline")
    report.set_defaults(func=command_report)
    validate = commands.add_parser("validate", help="Compare selected-case conclusions against their measured holdout")
    validate.add_argument("--quick", type=Path, required=True, help="Selected-case run directory")
    validate.add_argument("--corpus", type=Path, required=True, help="Corpus run directory")
    validate.add_argument("--output", type=Path, required=True)
    validate.add_argument("--control", default="shipping")
    validate.set_defaults(func=command_validate)
    return cli


def main(argv=None):
    args = parser().parse_args(argv)
    try:
        return args.func(args) or 0
    except (ValueError, OSError, KeyError, RuntimeError, subprocess.SubprocessError) as error:
        print(f"error: {error}", file=sys.stderr)
        return 1


if __name__ == "__main__":
    raise SystemExit(main())
