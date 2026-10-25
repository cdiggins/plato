import io
from contextlib import nullcontext
import hashlib
import json
import os
from pathlib import Path
import shutil
import subprocess
import sys
import tarfile
import unittest
from unittest.mock import patch
import uuid
from types import SimpleNamespace
import prepare

from prepare import (contained, export_tree, identity_hash, parse_program, read_shipping_flags,
                     seal, verify_cache, _assert_idle, build_variants, brep_export_paths, _generate, file_hashes)

TEST_ROOT = Path(__file__).resolve().parents[3] / ".temp/optimizer-prepare-tests"


class PrepareTests(unittest.TestCase):
    def setUp(self):
        TEST_ROOT.mkdir(parents=True, exist_ok=True)
        self.root = TEST_ROOT / uuid.uuid4().hex
        self.root.mkdir()

    def tearDown(self):
        # Only this test's freshly created directory, never a caller-supplied path.
        self.assertTrue(self.root.resolve().is_relative_to(TEST_ROOT.resolve()))
        shutil.rmtree(self.root)

    def test_cache_detects_modified_missing_and_added_files(self):
        path = self.root / "binary.dll"
        path.write_bytes(b"one")
        seal(self.root, {"compiler": "a"})
        verify_cache(self.root, {"compiler": "a"})
        path.write_bytes(b"two")
        with self.assertRaises(ValueError):
            verify_cache(self.root)
        path.write_bytes(b"one")
        extra = self.root / "extra"
        extra.write_bytes(b"extra")
        with self.assertRaises(ValueError):
            verify_cache(self.root)
        extra.unlink()
        path.unlink()
        with self.assertRaises(ValueError):
            verify_cache(self.root)

    def test_cache_rejects_wrong_identity_and_receipt_path_escape(self):
        seal(self.root, {"compiler": "a"})
        with self.assertRaises(ValueError):
            verify_cache(self.root, {"compiler": "b"})
        receipt = self.root / "complete.json"
        record = json.loads(receipt.read_text())
        for name in ("../outside", "/absolute", "C:/outside", "nested\\outside", "nested//outside",
                     "nested/./outside", "", "bad\0name"):
            with self.subTest(name=name):
                record["files"] = {name: "bad"}
                receipt.write_text(json.dumps(record))
                with self.assertRaisesRegex(ValueError, "Unsafe cache file name"):
                    verify_cache(self.root)
        for files in ([42], None):
            record["files"] = files
            receipt.write_text(json.dumps(record))
            with self.assertRaisesRegex(ValueError, "Invalid cache file manifest"):
                verify_cache(self.root)
        record["files"] = {42: "bad"}
        with patch("prepare.json.loads", return_value=record), self.assertRaisesRegex(ValueError, "Unsafe cache file name"):
            verify_cache(self.root)

    def test_nested_hashes_and_exact_exclusions(self):
        nested = self.root / "nested/deeper"
        nested.mkdir(parents=True)
        (self.root / "root.txt").write_bytes(b"root text")
        (nested / "binary.bin").write_bytes(b"\0\xff\x01")
        (self.root / "nested/skip.bin").write_bytes(b"excluded")
        self.assertEqual(file_hashes(self.root, exclude=("nested/skip.bin",)), {
            "root.txt": hashlib.sha256(b"root text").hexdigest(),
            "nested/deeper/binary.bin": hashlib.sha256(b"\0\xff\x01").hexdigest(),
        })

    def test_file_and_directory_symlinks_are_rejected_even_when_excluded(self):
        target_file = self.root / "target.txt"
        target_file.write_bytes(b"target")
        target_dir = self.root / "target-dir"
        target_dir.mkdir()
        (target_dir / "child.txt").write_bytes(b"child")
        seal(self.root, {"input": "links"})
        for target in (target_file, target_dir):
            link = self.root / "alias"
            try:
                link.symlink_to(target, target_is_directory=target.is_dir())
            except (OSError, NotImplementedError) as exc:
                self.skipTest("Symlink creation unavailable: " + str(exc))
            try:
                with self.assertRaisesRegex(ValueError, "Links are not cache inputs"):
                    file_hashes(self.root, exclude=("alias",))
                with self.assertRaisesRegex(ValueError, "Links are not cache inputs"):
                    verify_cache(self.root)
                with self.assertRaisesRegex(ValueError, "Links are not cache inputs"):
                    file_hashes(link)
            finally:
                link.unlink()

    def test_symlink_metadata_is_rejected_without_opening_target(self):
        metadata = SimpleNamespace(st_mode=prepare.stat.S_IFLNK, st_reparse_tag=None)
        entry = SimpleNamespace(name="alias", path=str(self.root / "alias"), stat=lambda **kwargs: metadata)
        with patch("prepare.os.scandir", return_value=nullcontext([entry])), patch("prepare.digest", side_effect=AssertionError("must not open target")):
            with self.assertRaisesRegex(ValueError, "Links are not cache inputs"):
                file_hashes(self.root, exclude=("alias",))

    @unittest.skipUnless(os.name == "nt", "Windows junctions only")
    def test_windows_junction_is_rejected(self):
        import _winapi
        if not hasattr(_winapi, "CreateJunction"):
            self.skipTest("Junction creation unavailable")
        target = self.root / "target"
        target.mkdir()
        (target / "child.txt").write_bytes(b"child")
        seal(self.root, {"input": "junction"})
        junction = self.root / "junction"
        try:
            _winapi.CreateJunction(str(target), str(junction))
        except OSError as exc:
            self.skipTest("Junction creation unavailable: " + str(exc))
        try:
            with self.assertRaisesRegex(ValueError, "Links are not cache inputs"):
                file_hashes(self.root)
            with self.assertRaisesRegex(ValueError, "Links are not cache inputs"):
                verify_cache(self.root)
            with self.assertRaisesRegex(ValueError, "Links are not cache inputs"):
                file_hashes(junction)
        finally:
            junction.rmdir()

    def test_path_containment(self):
        for name in ("../outside", "/absolute", "C:/outside", "nested/../../outside"):
            with self.subTest(name=name), self.assertRaises(ValueError):
                contained(self.root, name)
        self.assertEqual(contained(self.root, "inside/a.cs"), self.root / "inside/a.cs")

    def test_archive_rejects_links_and_escaping_members(self):
        for name, kind in (("../escape", tarfile.REGTYPE), ("link", tarfile.SYMTYPE)):
            stream = io.BytesIO()
            with tarfile.open(fileobj=stream, mode="w") as archive:
                member = tarfile.TarInfo(name)
                member.type = kind
                member.linkname = "outside"
                archive.addfile(member)
            with patch("prepare._git", return_value=stream.getvalue()), self.assertRaises(ValueError):
                export_tree("repo", "head", self.root / uuid.uuid4().hex)

    def test_program_uses_real_list_and_rejects_malformed_lines(self):
        actual = parse_program("# comment\nstdlib foundation\nexclude brep.* remeshing.*\nlayer 00-foundation\n")
        self.assertEqual(actual, dict(stdlib=["foundation"], exclude=["brep.*", "remeshing.*"], layer=["00-foundation"]))
        for text in ("stdlib foundation", "stdlib ../outside\nlayer foo", "unknown something", "stdlib"):
            with self.subTest(text=text), self.assertRaises(ValueError):
                parse_program(text)

    def test_shipping_recipe_and_identity_order(self):
        self.assertEqual(read_shipping_flags("$PinnedFlags = @('--loops', '--fuse')"), ["--loops", "--fuse"])
        self.assertEqual(identity_hash({"a": 1, "b": 2}), identity_hash({"b": 2, "a": 1}))

    def test_source_closure_keeps_projects_and_configs_without_corpus(self):
        paths = brep_export_paths(["Directory.Build.props", "Directory.Build.targets", "Directory.Packages.props",
                                   "deps.json", "NuGet.Config", "global.json", "README.md", "corpus", "tests", "docs"],
                                  dict(layer=["00-foundation", "01-geometry"]),
                                  ["Ara3D.StepBrep.csproj", "GeometrySet.Constructors.cs", "FutureHelper.cs", "Generated", "Intrinsics"])
        self.assertTrue(set(("src", "tools/step-corpus-harness", "Directory.Build.props", "Directory.Build.targets",
                             "Directory.Packages.props", "deps.json", "NuGet.Config", "global.json",
                             "generated/Ara3D.StepBrep/Ara3D.StepBrep.csproj", "library/01-geometry")).issubset(paths))
        self.assertIn("generated/Ara3D.StepBrep/GeometrySet.Constructors.cs", paths)
        self.assertIn("generated/Ara3D.StepBrep/FutureHelper.cs", paths)
        self.assertNotIn("generated/Ara3D.StepBrep/Generated", paths)
        self.assertNotIn("generated/Ara3D.StepBrep/Intrinsics", paths)
        self.assertFalse(set(("README.md", "corpus", "tests", "docs", "generated")).intersection(paths))

    def test_build_and_prepare_refuse_active_measurement(self):
        cache = self.root / "cache"
        (self.root / ".measure.lock").touch()
        with self.assertRaises(RuntimeError):
            _assert_idle(cache)

    def test_generation_delegates_host_obligations_to_committed_checker(self):
        # A small checker fixture exercises authority/log wiring without requiring
        # a sibling checkout or running the real compiler in a Python unit test.
        checker = '''import pathlib, re, sys
output = pathlib.Path(sys.argv[1])
log = pathlib.Path(sys.argv[sys.argv.index('--log') + 1]).read_text()
allowed = set(pathlib.Path(__file__).with_name('generated-stubs.txt').read_text().splitlines())
failures = []
for file in output.glob('*.cs'):
    text = file.read_text()
    name = re.search(r'object (\\w+)\\(', text).group(1)
    if 'is declared without a body' not in text:
        failures.append('degraded ' + name)
    elif name not in allowed:
        failures.append('new bodiless ' + name)
if 'DEGRADED' in log:
    failures.append('degraded compiler log')
print('\\n'.join(failures) if failures else 'accepted baseline host obligation')
sys.exit(bool(failures))
'''
        real_run = subprocess.run
        for label, name, message, compiler_log, accepted in (
                ("known", "ParseStep", "is declared without a body", "", True),
                ("new", "UnknownHost", "is declared without a body", "", False),
                ("degraded", "CountedFlips", "no ground TIR", "", False),
                ("logged", "ParseStep", "is declared without a body", "DEGRADED bodies: CountedFlips", False)):
            with self.subTest(label=label):
                case = self.root / label
                brep = case / "build/brep"
                tools = brep / "library/tools"
                tools.mkdir(parents=True)
                (tools / "shipped-program.txt").write_text("stdlib foundation\nlayer 00-foundation\n")
                (tools / "check_generated.py").write_text(checker)
                (tools / "generated-stubs.txt").write_text("ParseStep\n")
                layer = brep / "library/00-foundation"
                layer.mkdir()
                (layer / "brep.plato").write_text("library Brep {}")
                (brep / "generated/Ara3D.StepBrep").mkdir(parents=True)
                foundation = case / "source/plato/stdlib/foundation"
                foundation.mkdir(parents=True)
                (foundation / "basic.plato").write_text("primitive Number {}")
                runtime = case / "source/plato/src/Plato.Intrinsics"
                runtime.mkdir(parents=True)
                (runtime / "Plato.Intrinsics.projitems").write_text('<Project><ItemGroup><Compile Include="$(MSBuildThisFileDirectory)Scalar.cs" /></ItemGroup></Project>')
                (runtime / "Scalar.cs").write_text("namespace Ara3D.Geometry; public struct Scalar {}")

                def run(command, **kwargs):
                    if command[0] == "dotnet":
                        output = Path(next(arg.removeprefix("--out=") for arg in command if arg.startswith("--out=")))
                        (output / "Library.g.cs").write_text('public static object ' + name + '() => throw new System.NotImplementedException("plato: ' + name + ' ' + message + '");')
                        return SimpleNamespace(returncode=0, stdout=compiler_log, stderr="")
                    self.assertEqual(command[0:3], ["python", "-I", "-S"])
                    return real_run([sys.executable, *command[1:]], **kwargs)

                workspace = dict(root=str(case / "source"), cli="supplied.dll")
                log = case / "build/compiler.log"
                with patch("prepare.subprocess.run", side_effect=run):
                    if accepted:
                        self.assertGreaterEqual(_generate(workspace, brep, [], log, 10), 0)
                    else:
                        with self.assertRaisesRegex(ValueError, "Generated code check failed"):
                            _generate(workspace, brep, [], log, 10)
                evidence = (case / "build/generated-check.log").read_text()
                self.assertIn("accepted baseline host obligation" if accepted else "degraded" if label != "new" else "new bodiless", evidence)

    def test_compiler_and_checker_timeouts_preserve_partial_logs_and_failure(self):
        for phase in ("compiler", "checker"):
            with self.subTest(phase=phase):
                case = self.root / phase
                brep = case / "build/brep"
                tools = brep / "library/tools"
                tools.mkdir(parents=True)
                (tools / "shipped-program.txt").write_text("stdlib foundation\nlayer 00-foundation\n")
                layer = brep / "library/00-foundation"
                layer.mkdir()
                (layer / "brep.plato").write_text("library Brep {}")
                (brep / "generated/Ara3D.StepBrep").mkdir(parents=True)
                foundation = case / "source/plato/stdlib/foundation"
                foundation.mkdir(parents=True)
                (foundation / "basic.plato").write_text("primitive Number {}")
                error = subprocess.TimeoutExpired("test command", 10,
                                                 output=b"partial stdout\xff\n" if phase == "compiler" else "partial stdout\n",
                                                 stderr="partial stderr\n" if phase == "compiler" else b"partial stderr\xff\n")

                def run(command, **kwargs):
                    if phase == "compiler" or command[0] == "python":
                        raise error
                    output = Path(next(arg.removeprefix("--out=") for arg in command if arg.startswith("--out=")))
                    (output / "Unit.g.cs").write_text("public struct Unit {}")
                    return SimpleNamespace(returncode=0, stdout="compiler complete\n", stderr="")

                log = case / "build/compiler.log"
                with patch("prepare.subprocess.run", side_effect=run), self.assertRaises(subprocess.TimeoutExpired) as raised:
                    _generate(dict(root=str(case / "source"), cli="supplied.dll"), brep, [], log, 10)
                self.assertIs(raised.exception, error)
                if phase == "compiler":
                    self.assertEqual(log.read_text(encoding="utf-8"), "partial stdout\ufffd\npartial stderr\n")
                    self.assertFalse((log.parent / "generated-check.log").exists())
                else:
                    self.assertEqual(log.read_text(encoding="utf-8"), "compiler complete\n")
                    self.assertEqual((log.parent / "generated-check.log").read_text(encoding="utf-8"), "partial stdout\npartial stderr\ufffd\n")
                self.assertFalse((brep / "generated/Ara3D.StepBrep/Generated").exists())

    def test_failed_baseline_does_not_become_ready(self):
        workspace = self.root / "cache/workspaces/key"
        workspace.mkdir(parents=True)
        identity = {"input": "fixture"}
        seal(workspace, identity)
        worker = self.root / "worker"
        worker.mkdir()
        recorder = self.root / "recorder.ps1"
        recorder.write_text("test")
        data = dict(root=str(workspace), identity=identity, key=identity_hash(identity), shipping_flags=[])
        with patch("prepare._generate", side_effect=ValueError("test generation failure")):
            results = build_variants(data, [dict(name="shipping", flags=[]), dict(name="full", flags=["--inline"])], worker, recorder)
            retried = build_variants(data, [dict(name="shipping", flags=[]), dict(name="full", flags=["--inline"])], worker, recorder)
        self.assertEqual([r["status"] for r in results], ["error", "error"])
        self.assertIn("baseline failed", results[1]["error"])
        self.assertEqual([r["status"] for r in retried], ["error", "error"])
        self.assertEqual(len(list((self.root / "cache/b").glob("failed-*"))), 1)

    def test_build_freezes_host_and_deduplicates_equal_flags(self):
        workspace = self.root / "cache/workspaces/key"
        (workspace / "brep").mkdir(parents=True)
        identity = {"input": "fixture"}
        seal(workspace, identity)
        worker = self.root / "worker"
        worker.mkdir()
        (worker / "Worker.csproj").write_text('<Project><PropertyGroup><AssemblyName>worker</AssemblyName></PropertyGroup></Project>')
        recorder = self.root / "recorder.ps1"
        recorder.write_text("test")
        data = dict(root=str(workspace), identity=identity, key=identity_hash(identity), shipping_flags=[])
        builds = []

        def build(command, **kwargs):
            self.assertEqual(kwargs["env"]["EnableSourceControlManagerQueries"], "false")
            self.assertEqual(kwargs["env"]["EnableSourceLink"], "false")
            self.assertEqual(kwargs["env"]["IncludeSourceRevisionInInformationalVersion"], "false")
            project = Path(command[command.index("-Project") + 1])
            output = project.parent / "bin/Release/net8.0"
            output.mkdir(parents=True)
            (output / "worker.runtimeconfig.json").write_text("{}")
            (output / "worker.dll").write_bytes(b"worker-" + str(len(builds)).encode())
            (output / "host.dll").write_bytes(b"host-" + str(len(builds)).encode())
            (output / "Ara3D.StepBrep.dll").write_bytes(b"generated-" + str(len(builds)).encode())
            builds.append(command)
            return SimpleNamespace(returncode=0, stdout="ok", stderr="")

        variants = [dict(name="full", flags=["--inline"]), dict(name="shipping", flags=[]),
                    dict(name="same", flags=["--inline"])]
        with patch("prepare._generate", return_value=0.1), patch("prepare.subprocess.run", side_effect=build):
            results = build_variants(data, variants, worker, recorder)
            cached = build_variants(data, variants, worker, recorder)
        self.assertEqual(len(builds), 2)
        self.assertEqual([r["status"] for r in results], ["ready"] * 3)
        self.assertEqual(results[0]["host_hashes"], results[1]["host_hashes"])
        self.assertEqual(results[0]["build_environment"]["EnableSourceLink"], "false")
        self.assertEqual(results[2]["alias_of"], "full")
        self.assertEqual(results, cached)
        # Completed caches are evidence: corruption is an error, never a rebuild.
        (Path(results[1]["worker"]).parent / "host.dll").write_bytes(b"corrupt")
        rejected = build_variants(data, variants, worker, recorder)
        self.assertEqual([r["status"] for r in rejected], ["error"] * 3)
        self.assertFalse(list((self.root / "cache/b").glob("failed-*")))

    def build_source_fixture(self, label):
        base = self.root / label
        workspace = base / "cache/w/key"
        (workspace / "brep").mkdir(parents=True)
        identity = {"input": label}
        seal(workspace, identity)
        worker = base / "worker"
        worker.mkdir()
        (worker / "Worker.csproj").write_text('<Project><PropertyGroup><AssemblyName>worker</AssemblyName></PropertyGroup></Project>')
        (worker / "Program.cs").write_text("original worker source")
        recorder = base / "recorder.ps1"
        recorder.write_text("test")
        return dict(root=str(workspace), identity=identity, key=identity_hash(identity), shipping_flags=[]), worker, recorder

    @staticmethod
    def write_mock_build_output(command):
        project = Path(command[command.index("-Project") + 1])
        output = project.parent / "bin/Release/net8.0"
        output.mkdir(parents=True)
        (output / "worker.runtimeconfig.json").write_text("{}")
        (output / "worker.dll").write_bytes(b"worker")
        (output / "Ara3D.StepBrep.dll").write_bytes(b"generated")
        return SimpleNamespace(returncode=0, stdout="ok", stderr="")

    def test_worker_source_changes_before_during_or_after_copy_fail_build(self):
        real_copytree = shutil.copytree
        for phase in ("after-generation", "during-copy", "after-build"):
            with self.subTest(phase=phase):
                data, worker, recorder = self.build_source_fixture(phase)

                def generate(*args):
                    if phase == "after-generation":
                        (worker / "Program.cs").write_text("changed before copy")
                    return 0.1

                def copytree(source, target, *args, **kwargs):
                    result = real_copytree(source, target, *args, **kwargs)
                    if phase == "during-copy" and Path(source) == worker:
                        (Path(target) / "Program.cs").write_text("changed copied source")
                    return result

                def build(command, **kwargs):
                    result = self.write_mock_build_output(command)
                    if phase == "after-build":
                        (worker / "Program.cs").write_text("changed during compilation")
                    return result

                with patch("prepare._generate", side_effect=generate), patch("prepare.shutil.copytree", side_effect=copytree), patch("prepare.subprocess.run", side_effect=build) as run:
                    results = build_variants(data, [dict(name="shipping", flags=[])], worker, recorder)
                self.assertEqual(results[0]["status"], "error")
                self.assertIn("worker source changed", results[0]["error"].lower())
                if phase != "after-build":
                    run.assert_not_called()
                if phase == "during-copy":
                    project = next((self.root / phase / "cache/b").glob("*/brep/tools/optimizer-worker/Worker.csproj"))
                    self.assertNotIn("BrepRoot", project.read_text())
                self.assertFalse(list((self.root / phase / "cache/b").rglob("complete.json")))

    def test_builder_change_during_compilation_cannot_publish_new_identity(self):
        data, worker, recorder = self.build_source_fixture("builder-change")
        real_digest = prepare.digest
        changed = False

        def digest(path):
            if changed and Path(path).resolve() == Path(prepare.__file__).resolve():
                return "changed-builder"
            return real_digest(path)

        def build(command, **kwargs):
            nonlocal changed
            result = self.write_mock_build_output(command)
            changed = True
            return result

        with patch("prepare.digest", side_effect=digest), patch("prepare._generate", return_value=0.1), patch("prepare.subprocess.run", side_effect=build):
            results = build_variants(data, [dict(name="shipping", flags=[])], worker, recorder)
        self.assertEqual(results[0]["status"], "error")
        self.assertIn("builder changed", results[0]["error"])
        self.assertFalse(list((self.root / "builder-change/cache/b").rglob("complete.json")))

    def test_changed_workspace_source_aborts_before_generation(self):
        data, worker, recorder = self.build_source_fixture("workspace-change")
        source = Path(data["root"]) / "brep/Dependency.cs"
        source.write_text("original dependency")
        seal(data["root"], data["identity"])
        source.write_text("changed dependency")
        with patch("prepare._generate") as generate, self.assertRaisesRegex(ValueError, "Cache content changed"):
            build_variants(data, [dict(name="shipping", flags=[])], worker, recorder)
        generate.assert_not_called()


if __name__ == "__main__":
    unittest.main()
