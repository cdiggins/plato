"""Keep portable prepared data independent of candidate compiler binaries."""
import json
import os
from pathlib import Path
import shutil
import subprocess
import time

from prepare import BUILD_ENVIRONMENT, digest, file_hashes, identity_hash, verify_cache


def compatibility(workspace, worker_source):
    receipt = json.loads(Path(workspace["manifest"]).read_text(encoding="utf-8"))
    sources = {k: v for k, v in receipt["files"].items()
               if k.startswith(("plato/", "brep/")) and k != "brep/deps/.deps-root"}
    if not sources:
        raise ValueError("Workspace lacks prepared-data source evidence")
    return dict(schema=1, fixture_schema=1, profile="cpu64", sources=identity_hash(sources),
                packages=identity_hash(workspace["identity"]["sdk_packages"]),
                worker_source=identity_hash(file_hashes(worker_source)))


def stamp_builds(builds, workspace, worker_source):
    """Bind compatibility metadata to original sealed inputs, not current HEAD."""
    verify_cache(workspace["root"], workspace["identity"])
    expected_worker = file_hashes(worker_source)
    stable = compatibility(workspace, worker_source)
    for build in builds:
        if build.get("status") != "ready":
            continue
        if build.get("workspace_key") != workspace["key"]:
            raise ValueError("Cannot assign compatibility to a different source workspace")
        folder = Path(build["worker"]).resolve().parent
        while not (folder / "complete.json").is_file():
            if folder == folder.parent:
                raise ValueError("Build has no sealed preparation receipt")
            folder = folder.parent
        receipt = verify_cache(folder)
        identity = receipt["identity"]
        if (identity.get("workspace") != workspace["key"] or identity.get("worker") != expected_worker
                or sorted(identity.get("variant_flags", [])) != sorted(build["flags"])):
            raise ValueError("Build source evidence does not match fixture compatibility")
        result = json.loads((folder / "result.json").read_text(encoding="utf-8"))
        if result["output_hashes"] != build["output_hashes"]:
            raise ValueError("Build output differs from its sealed source evidence")
        build["fixture_compatibility"] = stable
    return stable


def ensure_probe(work, source, recorder, timeout=300):
    """Build the dependency-free reflection tool once, outside measurements."""
    source, work = Path(source), Path(work)
    inputs = {p.name: digest(p) for p in sorted(source.iterdir()) if p.suffix in (".cs", ".csproj")}
    if not inputs:
        raise ValueError("Fixture probe sources are missing")
    folder = work / "fixture-probe" / identity_hash(inputs)[:12]
    receipt_path = folder / "receipt.json"
    output = folder / "bin/Release/net8.0"
    executable = output / "optimizer-fixture-probe.dll"
    if receipt_path.exists():
        receipt = json.loads(receipt_path.read_text(encoding="utf-8"))
        if receipt["sources"] != inputs or receipt["outputs"] != file_hashes(output):
            raise ValueError("Fixture probe artifacts changed")
        return executable
    folder.mkdir(parents=True, exist_ok=True)
    for name in inputs:
        shutil.copyfile(source / name, folder / name)
    for name in ("Directory.Build.props", "Directory.Build.targets"):
        (folder / name).write_text("<Project />\n", encoding="utf-8")
    project = next(folder.glob("*.csproj"))
    result = subprocess.run(["powershell", "-NoProfile", "-File", str(recorder), "-Project", str(project.resolve()),
                             "-TargetName", "optimizer-fixture-probe"], capture_output=True, text=True,
                            timeout=timeout, env={**os.environ, **BUILD_ENVIRONMENT})
    (folder / "build.log").write_text(result.stdout + result.stderr, encoding="utf-8")
    if result.returncode or not executable.is_file():
        raise ValueError("Fixture probe build failed: " + str(folder / "build.log"))
    if inputs != {name: digest(source / name) for name in inputs}:
        raise ValueError("Fixture probe source changed during build")
    receipt_path.write_text(json.dumps(dict(sources=inputs, outputs=file_hashes(output)), indent=2), encoding="utf-8")
    return executable


def reader_runtime_identity(overrides=None):
    return dict(installed=subprocess.check_output(["dotnet", "--list-runtimes"], text=True, timeout=30),
        environment={**{k: v for k, v in os.environ.items() if k.startswith(("DOTNET_", "COMPlus_"))}, **(overrides or {})})


def validate_roundtrips(builds, cases, probe, cache, invoke, timeout=180, runtime_identity=None):
    """Cache exact candidate-reader compatibility by fixture and binary contents."""
    started = time.perf_counter()
    cache = Path(cache)
    cache.mkdir(parents=True, exist_ok=True)
    probe_hashes = file_hashes(Path(probe).parent)
    if runtime_identity is None:
        runtime_identity = reader_runtime_identity()
    checked, reused = 0, 0
    for build in builds:
        if build.get("status") != "ready":
            continue
        for case in cases:
            identity = dict(probe=probe_hashes, binaries=build["output_hashes"], fixture=case["fixture_hash"], runtime=runtime_identity)
            path = cache / (identity_hash(identity) + ".json")
            if path.exists():
                saved = json.loads(path.read_text(encoding="utf-8"))
                if saved.get("identity") != identity or saved.get("status") != "ok":
                    raise ValueError("Invalid fixture validation receipt")
                reused += 1
                continue
            result = invoke(probe, ["--worker", build["worker"], "--fixture", case["fixture"]], timeout=timeout)
            if (result.get("schema") != 1 or result.get("status") != "ok"
                    or result.get("fixture_hash") != case["fixture_hash"]
                    or result.get("worker_hash") != digest(build["worker"])):
                raise ValueError("Candidate cannot read the exact prepared fixture " + case["id"] + " / "
                                 + build["name"] + ": " + str(result.get("error", "identity mismatch")))
            path.write_text(json.dumps(dict(identity=identity, status="ok"), indent=2), encoding="utf-8")
            checked += 1
    if probe_hashes != file_hashes(Path(probe).parent):
        raise ValueError("Fixture probe changed during validation")
    return dict(checked=checked, cached=reused, seconds=time.perf_counter() - started, probe=probe_hashes)
