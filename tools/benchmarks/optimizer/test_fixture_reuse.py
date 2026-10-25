import json
from pathlib import Path
import tempfile
import unittest

import benchmark
import fixture_reuse


class FixtureReuseTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.root = benchmark.ROOT / ".temp/fixture-reuse-tests"
        cls.root.mkdir(parents=True, exist_ok=True)

    def test_compatibility_ignores_compiler_and_commit_but_tracks_actual_dependencies(self):
        with tempfile.TemporaryDirectory(dir=self.root) as folder:
            root = Path(folder)
            worker = root / "worker"
            worker.mkdir()
            (worker / "Reader.cs").write_text("reader contract")
            manifest = root / "complete.json"
            files = {"plato/stdlib/library.plato": "library", "brep/src/model.cs": "model",
                     "cli/Plato.CLI.dll": "old compiler", "brep/deps/.deps-root": "old machine path"}
            benchmark.write_json(manifest, dict(files=files))
            workspace = dict(manifest=str(manifest), identity=dict(sdk_packages={"sdk": "bytes"},
                plato_sources="old commit", cli_files={"cli": "old"}))
            original = fixture_reuse.compatibility(workspace, worker)
            workspace["identity"].update(plato_sources="compiler-only commit", cli_files={"cli": "new"})
            files.update({"cli/Plato.CLI.dll": "new compiler", "brep/deps/.deps-root": "new machine path"})
            benchmark.write_json(manifest, dict(files=files))
            self.assertEqual(original, fixture_reuse.compatibility(workspace, worker))
            files["plato/stdlib/library.plato"] = "changed library"
            benchmark.write_json(manifest, dict(files=files))
            self.assertNotEqual(original, fixture_reuse.compatibility(workspace, worker))
            files["plato/stdlib/library.plato"] = "library"
            benchmark.write_json(manifest, dict(files=files))
            (worker / "Reader.cs").write_text("changed reader")
            self.assertNotEqual(original, fixture_reuse.compatibility(workspace, worker))

    def test_roundtrip_cache_binds_probe_fixture_and_complete_candidate_closure(self):
        with tempfile.TemporaryDirectory(dir=self.root) as folder:
            root = Path(folder)
            probe_folder = root / "probe"
            probe_folder.mkdir()
            probe = probe_folder / "probe.dll"
            probe.write_text("probe implementation")
            worker = root / "worker.dll"
            worker.write_text("worker implementation")
            case = dict(id="box", fixture="prepared.json", fixture_hash="original fixture")
            build = dict(name="full", status="ready", worker=str(worker), output_hashes={"library": "original"})
            calls = []

            def invoke(*args, **kwargs):
                calls.append(args)
                return dict(schema=1, status="ok", fixture_hash=case["fixture_hash"], worker_hash=benchmark.file_hash(worker))

            options = dict(builds=[build], cases=[case], probe=probe, cache=root / "checks", invoke=invoke,
                           runtime_identity={"framework": "test runtime"})
            self.assertEqual(fixture_reuse.validate_roundtrips(**options)["checked"], 1)
            self.assertEqual(fixture_reuse.validate_roundtrips(**options)["cached"], 1)
            self.assertEqual(len(calls), 1)
            build["output_hashes"]["library"] = "new compiler output"
            self.assertEqual(fixture_reuse.validate_roundtrips(**options)["checked"], 1)
            case["fixture_hash"] = "different prepared data"
            self.assertEqual(fixture_reuse.validate_roundtrips(**options)["checked"], 1)
            probe.write_text("new probe")
            self.assertEqual(fixture_reuse.validate_roundtrips(**options)["checked"], 1)
            self.assertEqual(len(calls), 4)
            options["runtime_identity"] = {"framework": "updated runtime"}
            self.assertEqual(fixture_reuse.validate_roundtrips(**options)["checked"], 1)
            self.assertEqual(len(calls), 5)

    def test_failed_candidate_roundtrip_cannot_be_cached(self):
        with tempfile.TemporaryDirectory(dir=self.root) as folder:
            root = Path(folder)
            probe = root / "probe.dll"
            probe.write_text("probe")
            worker = root / "worker.dll"
            worker.write_text("worker")
            for result in (dict(schema=1, status="error", error="layout changed"),
                           dict(schema=1, status="ok", fixture_hash="wrong", worker_hash="wrong")):
                with self.assertRaisesRegex(ValueError, "Candidate cannot read"):
                    fixture_reuse.validate_roundtrips([dict(name="full", status="ready", worker=str(worker), output_hashes={})],
                        [dict(id="box", fixture="input.json", fixture_hash="expected")], probe, root / "checks",
                        lambda *args, **kwargs: result, runtime_identity={"framework": "test runtime"})
                self.assertEqual(list((root / "checks").iterdir()), [])


if __name__ == "__main__":
    unittest.main()
