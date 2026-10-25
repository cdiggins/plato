import io
import json
from pathlib import Path
import shutil
import tarfile
import unittest
from unittest.mock import patch
import uuid
from types import SimpleNamespace

from prepare import (contained, export_tree, identity_hash, parse_program, read_shipping_flags,
                     seal, verify_cache, _assert_idle, build_variants, brep_export_paths)

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
        record["files"]["../outside"] = "bad"
        receipt.write_text(json.dumps(record))
        with self.assertRaises(ValueError):
            verify_cache(self.root)

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


if __name__ == "__main__":
    unittest.main()
