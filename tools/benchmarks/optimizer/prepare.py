"""Immutable committed inputs, checked caches, and isolated optimizer builds.

The supplied CLI is a binary input: its hash establishes identity, not its source
provenance. Git snapshots establish provenance only for library/runtime sources.
"""
import fnmatch
import hashlib
import io
import json
import os
import re
import shutil
import subprocess
import sys
import tarfile
import time
import uuid
from pathlib import Path
import xml.etree.ElementTree as ET

from recipes import validate_flags

SCHEMA = 1
BUILD_ENVIRONMENT = {"EnableSourceControlManagerQueries": "false", "EnableSourceLink": "false",
                     "IncludeSourceRevisionInInformationalVersion": "false"}


def _progress(message):
    print("optimizer setup: " + message, file=sys.stderr, flush=True)


def digest(path):
    h = hashlib.sha256()
    with Path(path).open("rb") as stream:
        for block in iter(lambda: stream.read(1024 * 1024), b""):
            h.update(block)
    return h.hexdigest()


def identity_hash(value):
    return hashlib.sha256(json.dumps(value, sort_keys=True, separators=(",", ":")).encode()).hexdigest()


def contained(root, relative):
    """Resolve cache/manifest paths without allowing an escape or a symlink."""
    root = Path(root).resolve()
    relative = Path(relative)
    if relative.anchor or ".." in relative.parts or ":" in str(relative):
        raise ValueError("Unsafe relative path: " + str(relative))
    target = root / relative
    cursor = target
    while cursor != root:
        if cursor.is_symlink() or (hasattr(cursor, "is_junction") and cursor.is_junction()):
            raise ValueError("Links are not cache inputs: " + str(cursor))
        cursor = cursor.parent
    if not target.resolve().is_relative_to(root):
        raise ValueError("Path leaves cache root")
    return target


def file_hashes(root, *, exclude=()):
    root = Path(root)
    result = {}
    for path in sorted(root.rglob("*")):
        name = path.relative_to(root).as_posix()
        contained(root, name)
        if path.is_file() and name not in exclude:
            result[name] = digest(path)
    return result


def write_json(path, value):
    Path(path).write_text(json.dumps(value, indent=2, sort_keys=True) + "\n", encoding="utf-8")


def seal(root, identity, name="complete.json"):
    receipt = dict(schema=SCHEMA, identity=identity, key=identity_hash(identity),
                   files=file_hashes(root, exclude=(name,)))
    write_json(Path(root) / name, receipt)
    return receipt


def verify_cache(root, identity=None, name="complete.json"):
    root = Path(root)
    record = json.loads(contained(root, name).read_text(encoding="utf-8"))
    if record.get("schema") != SCHEMA or record.get("key") != identity_hash(record.get("identity")):
        raise ValueError("Invalid cache receipt identity")
    if identity is not None and record["identity"] != identity:
        raise ValueError("Cache inputs differ")
    for relative in record["files"]:
        contained(root, relative)
    if file_hashes(root, exclude=(name,)) != record["files"]:
        raise ValueError("Cache content changed: " + str(root))
    return record


def _git(root, *args):
    return subprocess.check_output(["git", "-C", str(root), *args])


def export_tree(repo, commit, target, paths=()):
    """Export regular committed files; refuse archive links and unsafe names."""
    archive = _git(repo, "archive", "--format=tar", commit, *paths)
    target = Path(target)
    target.mkdir(parents=True, exist_ok=False)
    with tarfile.open(fileobj=io.BytesIO(archive)) as tar:
        for member in tar:
            path = contained(target, member.name)
            if member.isdir():
                path.mkdir(parents=True, exist_ok=True)
            elif member.isfile():
                path.parent.mkdir(parents=True, exist_ok=True)
                with tar.extractfile(member) as source, path.open("wb") as dest:
                    shutil.copyfileobj(source, dest)
            else:
                raise ValueError("Committed archive contains a link or special file: " + member.name)


def parse_program(text):
    result = dict(stdlib=[], exclude=[], layer=[])
    for line in text.splitlines():
        words = line.split("#", 1)[0].split()
        if not words:
            continue
        if words[0] not in result or len(words) < 2 or (words[0] != "exclude" and len(words) != 2):
            raise ValueError("Malformed shipped-program line: " + line)
        for word in words[1:]:
            contained(Path.cwd(), word)
        result[words[0]].extend(words[1:])
    if not result["stdlib"] or not result["layer"]:
        raise ValueError("Shipped program must name stdlib tiers and layers")
    return result


def read_shipping_flags(script):
    match = re.search(r"^\s*\$PinnedFlags\s*=\s*@\(([^)]*)\)", script, re.MULTILINE)
    if not match:
        raise ValueError("Cannot read shipping recipe; supply shipping_flags explicitly")
    return validate_flags(re.findall(r"['\"]([^'\"]+)['\"]", match[1]))


def brep_export_paths(root_entries, program, generated_root_entries=()):
    """The build graph plus shipped Plato sources; omit corpus/docs/tests/assets.

    Keep whole source/project folders so future files imported by their projects
    are not silently omitted. The generated library is rebuilt, so retain only
    its project definition and handwritten root C# files rather than copying its
    cached Generated/ and Intrinsics/ trees.
    """
    configs = [p for p in root_entries if "/" not in p and (
        p.lower() in ("deps.json", "global.json", "nuget.config") or p.endswith((".props", ".targets")))]
    required = ["src", "tools/step-corpus-harness", "tools/regen-step-brep.ps1",
                "generated/Ara3D.StepBrep/Ara3D.StepBrep.csproj",
                "library/tools/shipped-program.txt", "library/tools/check_generated.py",
                "library/tools/generated-stubs.txt"]
    handwritten = ["generated/Ara3D.StepBrep/" + p for p in generated_root_entries
                   if "/" not in p and p.endswith((".cs", ".csproj", ".props", ".targets", ".resx"))]
    return sorted(set(configs + required + handwritten + ["library/" + layer for layer in program["layer"]]))


def _assert_idle(cache_root):
    if (Path(cache_root).resolve().parent / ".measure.lock").exists():
        raise RuntimeError("Benchmark measurements are active; preparation/build is locked")


def prepare_workspace(plato_root, brep_root, cli, cache_root, *, plato_commit="HEAD", brep_commit="HEAD",
                      mesh_root=None, sdk_root=None, shipping_flags=None):
    """Snapshot committed sources and supplied binary closure into a verified cache.

    sdk_root may name the pin-specific package folder or its parent feed folder.
    Package bytes are hashed; a folder named after a pin is not proof of a build.
    """
    _assert_idle(cache_root)
    _progress("resolving committed source and supplied compiler inputs")
    plato_root, brep_root, cli = map(lambda p: Path(p).resolve(), (plato_root, brep_root, cli))
    cache_root = Path(cache_root).resolve()
    plato_sha = _git(plato_root, "rev-parse", "--verify", plato_commit + "^{commit}").decode().strip()
    brep_sha = _git(brep_root, "rev-parse", "--verify", brep_commit + "^{commit}").decode().strip()
    pins = json.loads(_git(brep_root, "show", brep_sha + ":deps.json"))
    mesh_root = Path(mesh_root or brep_root / "deps/bim-open-data").resolve()
    mesh_sha = _git(mesh_root, "rev-parse", "--verify", pins["bim-open-data"]["commit"] + "^{commit}").decode().strip()
    sdk_pin = pins["ara3d-sdk"]["commit"]
    feed = Path(sdk_root or brep_root / "deps/ara3d-sdk-packages").resolve()
    feed = feed / sdk_pin if (feed / sdk_pin).is_dir() else feed
    packages = sorted(feed.glob("*.nupkg"))
    if not packages:
        raise ValueError("Pinned SDK package feed is absent: " + str(feed))
    if not cli.is_file():
        raise ValueError("Supplied compiler DLL is absent: " + str(cli))
    # Copy the complete supplied directory, including deps/runtimeconfig and satellite files.
    cli_files = file_hashes(cli.parent)
    dotnet = subprocess.check_output(["dotnet", "--version"], text=True).strip()
    script = _git(brep_root, "show", brep_sha + ":tools/regen-step-brep.ps1").decode("utf-8-sig")
    program = parse_program(_git(brep_root, "show", brep_sha + ":library/tools/shipped-program.txt").decode("utf-8-sig"))
    root_entries = _git(brep_root, "ls-tree", "--name-only", brep_sha).decode("utf-8").splitlines()
    generated_root_entries = _git(brep_root, "ls-tree", "--name-only", brep_sha + ":generated/Ara3D.StepBrep").decode("utf-8").splitlines()
    brep_paths = brep_export_paths(root_entries, program, generated_root_entries)
    plato_paths = ["src/Plato.Intrinsics", *["stdlib/" + tier for tier in program["stdlib"]]]
    flags = read_shipping_flags(script) if shipping_flags is None else validate_flags(shipping_flags)
    identity = dict(schema=SCHEMA, plato_sources=plato_sha, brep=brep_sha, mesh=mesh_sha,
                    sdk_pin=sdk_pin, sdk_packages={p.name: digest(p) for p in packages},
                    cli_files=cli_files, cli_name=cli.name, cli_provenance="supplied binary; source revision unverified",
                    shipping_flags=flags, dotnet=dotnet, builder_sha256=digest(__file__))
    identity["brep_export_paths"] = brep_paths
    identity["plato_export_paths"] = plato_paths
    key = identity_hash(identity)
    # Keep source paths below Windows MAX_PATH. Full hashes remain in receipts;
    # a short-prefix collision fails identity validation rather than reusing data.
    final = cache_root / "w" / key[:12]
    if final.exists():
        _progress("verifying cached workspace " + str(final))
        verify_cache(final, identity)
    else:
        final.parent.mkdir(parents=True, exist_ok=True)
        stage = final.parent / (".p-" + uuid.uuid4().hex[:8])
        stage.mkdir()
        _progress("exporting committed plato-brep source")
        export_tree(brep_root, brep_sha, stage / "brep", brep_paths)
        _progress("exporting committed Plato stdlib and runtime")
        export_tree(plato_root, plato_sha, stage / "plato", plato_paths)
        mesh = stage / "brep/deps/bim-open-data"
        # gitlinks export as empty folders, never as the live linked checkout.
        if mesh.exists():
            if any(mesh.iterdir()):
                raise ValueError("Exported dependency path is nonempty")
            mesh.rmdir()
        mesh.parent.mkdir(parents=True, exist_ok=True)
        (mesh.parent / ".deps-root").write_text("Prepared benchmark dependency root\n", encoding="utf-8")
        _progress("exporting pinned MeshKernel source")
        export_tree(mesh_root, mesh_sha, mesh, ("src/data/Ara3D.MeshKernel", "Directory.Build.props", "Directory.Build.targets"))
        # Use the host's package/version rules for MeshKernel, too.
        for name in ("Directory.Build.props", "Directory.Build.targets"):
            shutil.copyfile(stage / "brep" / name, mesh / name)
        dest_feed = stage / "brep/deps/ara3d-sdk-packages" / sdk_pin
        dest_feed.mkdir(parents=True)
        for package in packages:
            shutil.copyfile(package, dest_feed / package.name)
        _progress("copying supplied compiler binary closure")
        shutil.copytree(cli.parent, stage / "cli")
        _progress("hashing copied workspace and sealing its receipt")
        receipt = seal(stage, identity)
        copied_cli = {name.removeprefix("cli/"): value for name, value in receipt["files"].items() if name.startswith("cli/")}
        if copied_cli != cli_files:
            raise ValueError("Supplied compiler closure changed during copy")
        feed_prefix = dest_feed.relative_to(stage).as_posix() + "/"
        copied_packages = {name.removeprefix(feed_prefix): value for name, value in receipt["files"].items() if name.startswith(feed_prefix)}
        if copied_packages != identity["sdk_packages"]:
            raise ValueError("Package feed changed during copy")
        stage.rename(final)
        _progress("workspace ready " + str(final))
    return dict(key=key, root=str(final), brep_root=str(final / "brep"), cli=str(final / "cli" / cli.name),
                shipping_flags=flags, manifest=str(final / "complete.json"), identity=identity)


def _generate(workspace, brep, flags, log, timeout):
    program = parse_program((brep / "library/tools/shipped-program.txt").read_text(encoding="utf-8-sig"))
    inputs = brep.parent / "inputs"
    inputs.mkdir()
    roots = []
    for tier in program["stdlib"]:
        source = contained(Path(workspace["root"]) / "plato/stdlib", tier)
        dest = contained(inputs, tier)
        dest.mkdir(parents=True)
        for path in sorted(source.glob("*.plato")):
            if not any(fnmatch.fnmatchcase(path.name, pattern) for pattern in program["exclude"]):
                shutil.copyfile(path, dest / path.name)
        roots.append(str(dest))
    library = inputs / "library"
    library.mkdir()
    for layer in program["layer"]:
        for path in sorted(contained(brep / "library", layer).glob("*.plato")):
            if not path.name.endswith(".spec.plato"):
                dest = library / path.name
                if dest.exists():
                    raise ValueError("Duplicate library file: " + path.name)
                shutil.copyfile(path, dest)
    roots.append(str(library))
    output = brep.parent / "emitted"
    output.mkdir()
    started = time.perf_counter()
    run = subprocess.run(["dotnet", workspace["cli"], *roots, "--out=" + str(output), "--profile=cpu64", *flags],
                         capture_output=True, text=True, timeout=timeout)
    text = run.stdout + run.stderr
    log.write_text(text, encoding="utf-8")
    if run.returncode:
        raise ValueError("Compiler failed; see " + str(log))
    files = sorted(output.glob("*.g.cs"))
    if not files:
        raise ValueError("Compiler emitted no generated C#")
    # The committed checker distinguishes declared host obligations from dropped
    # compiler bodies. Only the former may use its committed baseline; every
    # degraded body and every new bodiless declaration remains a failure.
    check = subprocess.run(["python", "-I", "-S", str(brep / "library/tools/check_generated.py"), str(output), "--log", str(log)],
                           capture_output=True, text=True, timeout=timeout)
    checker_log = log.parent / "generated-check.log"
    checker_log.write_text(check.stdout + check.stderr, encoding="utf-8")
    if check.returncode:
        raise ValueError("Generated code check failed; see " + str(checker_log) + ": " + check.stdout + check.stderr)
    project = brep / "generated/Ara3D.StepBrep"
    for name in ("Generated", "Intrinsics"):
        folder = project / name
        if folder.exists():
            for file in folder.glob("*.cs"):
                file.unlink()
        folder.mkdir(exist_ok=True)
    for file in files:
        (project / "Generated" / file.name).write_text(file.read_text(encoding="utf-8-sig").replace("Ara3D.Geometry", "Ara3D.StepBrep"), encoding="utf-8")
    runtime = Path(workspace["root"]) / "plato/src/Plato.Intrinsics"
    tree = ET.parse(runtime / "Plato.Intrinsics.projitems")
    for node in tree.iter():
        if node.tag.rsplit("}", 1)[-1] == "Compile":
            name = node.attrib["Include"].replace("$(MSBuildThisFileDirectory)", "").replace("\\", "/")
            source = contained(runtime, name)
            target = contained(project / "Intrinsics", name)
            target.parent.mkdir(parents=True, exist_ok=True)
            target.write_text(source.read_text(encoding="utf-8-sig").replace("Ara3D.Geometry", "Ara3D.StepBrep"), encoding="utf-8")
    return time.perf_counter() - started


def build_variants(workspace, variants, worker_source, recorder, *, timeout=900):
    """Build shipping first and hold its host closure fixed for every recipe.

    All cache identities include the worker source, builder, recorder and workspace.
    Failed/unsupported recipes remain explicit results and are never measured.
    """
    root = Path(workspace["root"]).resolve()
    cache_root = root.parent.parent
    _assert_idle(cache_root)
    verify_cache(root, workspace["identity"])
    worker_source, recorder = Path(worker_source).resolve(), Path(recorder).resolve()
    source_hashes = file_hashes(worker_source)
    variants = list(variants)
    shipping = next((v for v in variants if v["name"] == "shipping"),
                    dict(name="shipping", flags=workspace["shipping_flags"], kind="reference"))
    ordered = [shipping] + [v for v in variants if v["name"] != "shipping"]
    results, flags_seen, outputs_seen = {}, {}, {}
    baseline = None
    for variant in ordered:
        name = variant["name"]
        try:
            if not re.fullmatch(r"[a-zA-Z0-9_-]+", name):
                raise ValueError("Unsafe variant name")
            if variant.get("status") == "unsupported" or variant.get("kind") == "unsupported":
                results[name] = dict(variant, status="unsupported", error=variant.get("description", "Unsupported recipe"))
                continue
            flags = validate_flags(variant["flags"])
            canonical = tuple(sorted(flags))
            if canonical in flags_seen:
                results[name] = dict(results[flags_seen[canonical]], name=name, alias_of=flags_seen[canonical], flags=flags)
                continue
            identity = dict(schema=SCHEMA, workspace=workspace["key"], variant_flags=flags,
                            worker=source_hashes, recorder=digest(recorder), builder=digest(__file__),
                            build_environment=BUILD_ENVIRONMENT,
                            baseline=baseline["closure_hash"] if baseline else None)
            folder = cache_root / "b" / identity_hash(identity)[:12]
            receipt = folder / "complete.json"
            if folder.exists() and not receipt.exists():
                # Preserve interrupted/failed diagnostics; complete corrupt caches
                # must fail verification rather than quietly becoming cache misses.
                source = contained(folder.parent, folder.name)
                failed = contained(folder.parent, "failed-" + uuid.uuid4().hex[:8])
                _progress("preserving incomplete build " + str(source) + " as " + str(failed))
                source.rename(failed)
            if folder.exists():
                _progress("verifying cached recipe " + name)
                record = verify_cache(folder, identity)
                result = json.loads((folder / "result.json").read_text())
            else:
                folder.mkdir(parents=True)
                brep = folder / "brep"
                _progress("copying immutable inputs for recipe " + name)
                shutil.copytree(root / "brep", brep)
                _progress("generating recipe " + name)
                generation_seconds = _generate(workspace, brep, flags, folder / "compiler.log", timeout)
                worker = brep / "tools/optimizer-worker"
                shutil.copytree(worker_source, worker)
                projects = list(worker.glob("*.csproj"))
                if len(projects) != 1:
                    raise ValueError("Worker must contain one csproj")
                project = projects[0]
                xml = ET.parse(project)
                props = ET.SubElement(xml.getroot(), "PropertyGroup")
                ET.SubElement(props, "BrepRoot").text = str(brep)
                xml.write(project, encoding="utf-8", xml_declaration=True)
                started = time.perf_counter()
                _progress("building recorded worker for recipe " + name)
                build = subprocess.run(["powershell", "-NoProfile", "-File", str(recorder), "-Project", str(project),
                                        "-TargetName", "optimizer-" + name], capture_output=True, text=True, timeout=timeout,
                                       env={**os.environ, **BUILD_ENVIRONMENT})
                (folder / "build.log").write_text(build.stdout + build.stderr, encoding="utf-8")
                if build.returncode:
                    raise ValueError("Worker build failed; see " + str(folder / "build.log"))
                output = worker / "bin/Release/net8.0"
                runtimeconfigs = list(output.glob("*.runtimeconfig.json"))
                if len(runtimeconfigs) != 1:
                    # Referenced harness can contribute a second executable runtimeconfig.
                    assembly = xml.getroot().find(".//AssemblyName")
                    stem = assembly.text if assembly is not None else project.stem
                else:
                    stem = runtimeconfigs[0].name.removesuffix(".runtimeconfig.json")
                executable = output / (stem + ".dll")
                if not executable.exists():
                    raise ValueError("Worker output DLL missing: " + str(executable))
                varying = {"Ara3D.StepBrep.dll", "Ara3D.StepBrep.pdb"}
                before = file_hashes(output)
                host = {k: v for k, v in before.items() if k not in varying}
                if baseline:
                    frozen = Path(baseline["worker"]).parent
                    expected = baseline["host_hashes"]
                    if set(host) != set(expected):
                        raise ValueError("Host dependency file set changed across recipes")
                    for relative in expected:
                        shutil.copyfile(contained(frozen, relative), contained(output, relative))
                    host = {k: v for k, v in file_hashes(output).items() if k not in varying}
                    if host != expected:
                        raise ValueError("Host dependency freeze failed")
                closure = file_hashes(output)
                result = dict(name=name, status="ready", worker=str(executable), flags=flags,
                              generation_seconds=generation_seconds, build_seconds=time.perf_counter() - started,
                              assembly_bytes=(output / "Ara3D.StepBrep.dll").stat().st_size,
                              host_hashes=host, closure_hash=identity_hash(closure), output_hashes=closure,
                              build_environment=BUILD_ENVIRONMENT,
                              workspace_key=workspace["key"])
                write_json(folder / "result.json", result)
                _progress("sealing successful recipe " + name)
                seal(folder, identity)
            results[name] = result
            if name == "shipping":
                baseline = result
            if baseline is None:
                raise ValueError("Shipping host baseline unavailable")
            flags_seen[canonical] = name
            if result["closure_hash"] in outputs_seen:
                result = dict(result, alias_of=outputs_seen[result["closure_hash"]])
                results[name] = result
            else:
                outputs_seen[result["closure_hash"]] = name
        except (ValueError, OSError, subprocess.SubprocessError) as exc:
            results[name] = dict(name=name, status="error", flags=variant.get("flags", []), error=str(exc))
            if name == "shipping":
                for remaining in ordered[1:]:
                    unsupported = remaining.get("status") == "unsupported" or remaining.get("kind") == "unsupported"
                    results[remaining["name"]] = dict(name=remaining["name"], status="unsupported" if unsupported else "error",
                                                       flags=remaining.get("flags", []),
                                                       error=remaining.get("description") if unsupported else "Shipping host baseline failed: " + str(exc))
                break
    return [results[v["name"]] for v in variants]
