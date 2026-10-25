import contextlib
import io
import json
from pathlib import Path
import tempfile
import unittest
from unittest.mock import patch

import benchmark


class RunnerTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.test_root = benchmark.ROOT / ".temp/optimizer-benchmark-tests"
        cls.test_root.mkdir(parents=True, exist_ok=True)

    def run_samples(self, builds, cases, **kwargs):
        with tempfile.TemporaryDirectory(dir=self.test_root) as folder, contextlib.redirect_stderr(io.StringIO()):
            output = Path(folder) / "samples.jsonl"
            rows = benchmark.run_samples(builds, cases, output, blocks=2, seconds=10,
                target_ms=1, warmup=1, max_iterations=10, timeout=1, seed=19, **kwargs)
            saved = [json.loads(line) for line in output.read_text().splitlines()]
        self.assertEqual(rows, saved)
        return rows

    @staticmethod
    def fixture():
        return [{"id": "box", "fixture": "box.json", "tags": ["plane"], "workloads": ["model", "edges"]}]

    def test_pairing_and_normalization_inputs_preserved(self):
        calls = []

        def worker(path, arguments, **kwargs):
            calls.append((path, arguments, kwargs))
            return {"status": "ok", "samples": [{"schema": 1, "status": "ok", "case_id": "box", "workload": w,
                    "elapsed_ns": 100, "iterations": 2, "allocated_bytes": 0,
                    "digest": "same", "checks": {"valid": True}} for w in ("model", "edges")]}

        builds = [{"name": name, "worker": name + ".dll", "status": "ready"} for name in ("full", "shipping")]
        rows = self.run_samples(builds, self.fixture(), worker_call=worker)
        self.assertEqual(len(calls), 4)
        self.assertEqual({row["case_id"] for row in rows}, {"box"})
        self.assertEqual({row["workload"] for row in rows}, {"model", "edges"})
        self.assertEqual({(r["block"], r["variant"], r["workload"]) for r in rows},
                         {(b, v, w) for b in (0, 1) for v in ("full", "shipping") for w in ("model", "edges")})

    def test_failed_build_is_never_invoked_or_omitted(self):
        def worker(*args, **kwargs):
            self.fail("Unavailable build must never execute")

        rows = self.run_samples([{"name": "broken", "status": "error", "error": "CS0001"}],
                                self.fixture(), worker_call=worker)
        self.assertEqual(len(rows), 4)
        self.assertTrue(all(r["status"] == "error" and r["error"] == "CS0001" for r in rows))

    def test_malformed_or_missing_set_samples_are_preserved_as_errors(self):
        for result in ({"status": "ok", "samples": {}}, {"status": "ok", "samples": [1]},
                       {"status": "ok", "samples": []}):
            rows = self.run_samples([{"name": "full", "status": "ready", "worker": "x.dll"}],
                                   self.fixture(), worker_call=lambda *a, **k: result)
            self.assertTrue(all(r["status"] == "error" for r in rows))

    def test_identical_artifact_alias_does_not_repeat_work(self):
        calls = []

        def worker(*args, **kwargs):
            calls.append(args)
            return {"status": "ok", "samples": [{"schema": 1, "case_id": "box", "workload": w, "elapsed_ns": 100, "iterations": 1,
                    "allocated_bytes": 0, "digest": "same", "checks": {"valid": True}} for w in ("model", "edges")]}

        builds = [{"name": "alias", "status": "ready", "worker": "alias.dll", "alias_of": "full"},
                  {"name": "full", "status": "ready", "worker": "full.dll"}]
        rows = self.run_samples(builds, self.fixture(), worker_call=worker)
        self.assertEqual(len(calls), 2)
        self.assertTrue(all(r["alias_of"] == "full" for r in rows if r["variant"] == "alias"))

    def test_wrong_worker_case_or_schema_cannot_become_scheduled_sample(self):
        for schema, case_id in ((0, "box"), (1, "different-box")):
            result = {"status": "ok", "samples": [{"schema": schema, "case_id": case_id,
                "workload": w, "elapsed_ns": 100, "iterations": 1, "allocated_bytes": 0,
                "digest": "same", "checks": {"valid": True}} for w in ("model", "edges")]}
            rows = self.run_samples([{"name": "full", "status": "ready", "worker": "x.dll"}],
                                   self.fixture(), worker_call=lambda *a, **k: result)
            self.assertTrue(all(r["status"] == "error" for r in rows))

    def test_preexisting_artifact_tampering_and_host_difference_are_rejected(self):
        with tempfile.TemporaryDirectory(dir=self.test_root) as folder:
            root = Path(folder)

            def build(name, host):
                dest = root / name
                dest.mkdir()
                (dest / "worker.dll").write_text(host)
                (dest / "Ara3D.StepBrep.dll").write_text(name)
                output = benchmark.binary_identity(dest / "worker.dll")
                return dict(name=name, status="ready", worker=str(dest / "worker.dll"), output_hashes=output,
                    host_hashes={k: v for k, v in output.items() if k != "Ara3D.StepBrep.dll"}, workspace_key="same")

            full, shipping = build("full", "host"), build("shipping", "host")
            identities = benchmark.verify_build_artifacts([full, shipping])
            self.assertEqual(set(identities), {"full", "shipping"})
            with self.assertRaisesRegex(ValueError, "Artifact alias differs"):
                benchmark.verify_build_artifacts([{**full, "alias_of": "shipping"}, shipping])
            (root / "full/worker.dll").write_text("tampered")
            with self.assertRaisesRegex(ValueError, "artifacts changed"):
                benchmark.verify_build_artifacts([full])
            different = build("other", "different host")
            with self.assertRaisesRegex(ValueError, "Host dependencies differ"):
                benchmark.verify_build_artifacts([shipping, different])

    def test_stale_fixture_preparation_and_modified_fixture_are_rejected(self):
        with tempfile.TemporaryDirectory(dir=self.test_root) as folder:
            fixture = Path(folder) / "fixture.json"
            fixture.write_text("fixture")
            case = dict(id="box", fixture=str(fixture), fixture_hash=benchmark.file_hash(fixture), preparation_identity="library")
            identity = {"Ara3D.StepBrep.dll": "library", "worker.dll": "worker"}
            suite = dict(cases=[case], preparation_binary_identity=identity)
            benchmark.verify_suite_preparation(suite, {"shipping": identity})
            with self.assertRaisesRegex(ValueError, "prepare fixtures again"):
                benchmark.verify_suite_preparation(suite, {"shipping": {"worker.dll": "new"}})
            fixture.write_text("changed")
            with self.assertRaisesRegex(ValueError, "Fixture changed"):
                benchmark.verify_suite_preparation(suite, {"shipping": identity})

    def test_truncated_raw_log_cannot_hide_a_whole_planned_block(self):
        metadata = dict(parameters={"blocks": 2}, variants=[{"name": "full"}], selection={"cases": self.fixture()})
        observed = [dict(schema=1, block=0, variant="full", case_id="box", workload=w, status="ok")
                    for w in ("model", "edges")]
        completed = benchmark.complete_planned_rows(observed, metadata)
        self.assertEqual(len(completed), 4)
        self.assertEqual(len([r for r in completed if r["status"] == "not_measured"]), 2)
        with self.assertRaisesRegex(ValueError, "outside the recorded run plan"):
            benchmark.complete_planned_rows([dict(block=2, variant="full", case_id="box", workload="edges")], metadata)

    def test_compiler_only_revisions_reuse_fixtures_but_require_original_reference(self):
        with tempfile.TemporaryDirectory(dir=self.test_root) as folder:
            root = Path(folder)
            contract = dict(sources="same library/runtime", profile="cpu64", worker_source="same contract")
            def artifact(name):
                dest = root / name
                dest.mkdir()
                worker = dest / "worker.dll"
                worker.write_text("same worker")
                (dest / "Ara3D.StepBrep.dll").write_text(name)
                output = benchmark.binary_identity(worker)
                return dict(name="shipping", status="ready", workspace_key=name, worker=str(worker),
                    output_hashes=output, host_hashes={"worker.dll": output["worker.dll"]}, fixture_compatibility=contract)
            original, candidate = artifact("old compiler"), artifact("new compiler")
            fixture = root / "fixture.json"
            fixture.write_text("prepared input")
            case = dict(id="box", fixture=str(fixture), fixture_hash=benchmark.file_hash(fixture),
                preparation_identity=original["output_hashes"]["Ara3D.StepBrep.dll"])
            suite = dict(cases=[case], preparation_binary_identity=original["output_hashes"],
                preparation_build=original, fixture_compatibility=contract)
            identities = benchmark.verify_build_artifacts([candidate])
            benchmark.verify_suite_preparation(suite, identities, [candidate])
            with self.assertRaisesRegex(ValueError, "prepare fixtures again"):
                benchmark.verify_suite_preparation(suite, identities,
                    [{**candidate, "fixture_compatibility": {**contract, "sources": "changed runtime"}}])
            Path(original["worker"]).write_text("tampered original reference")
            with self.assertRaisesRegex(ValueError, "Original fixture preparation artifacts changed"):
                benchmark.verify_suite_preparation(suite, identities, [candidate])

    def test_actual_runtime_identity_rejects_different_gc_or_missing_metadata(self):
        good = dict(status="ok", runtime={"framework": "net8", "server_gc": True})
        self.assertIsNotNone(benchmark.actual_runtime_identity([good, good]))
        for other in (dict(status="ok"), dict(status="ok", runtime={"framework": "net8", "server_gc": False})):
            with self.assertRaises(ValueError):
                benchmark.actual_runtime_identity([good, other])

    def test_cli_quick_plan_uses_references_and_writes_reviewable_report(self):
        with tempfile.TemporaryDirectory(dir=self.test_root) as folder, contextlib.redirect_stdout(io.StringIO()):
            root = Path(folder)
            builds = []
            for name in ("shipping", "full", "unoptimized", "no-inline"):
                dest = root / name
                dest.mkdir()
                (dest / "worker.dll").write_text("fixed host")
                (dest / "Ara3D.StepBrep.dll").write_text(name)
                output = benchmark.binary_identity(dest / "worker.dll")
                builds.append(dict(name=name, status="ready", worker=str(dest / "worker.dll"), output_hashes=output,
                    host_hashes={k: v for k, v in output.items() if k != "Ara3D.StepBrep.dll"}, workspace_key="same"))
            benchmark.write_json(root / "builds.json", builds)
            fixture = root / "fixture.json"
            fixture.write_text("prepared input")
            case = dict(id="box", fixture=str(fixture), fixture_hash=benchmark.file_hash(fixture),
                preparation_identity=builds[0]["output_hashes"]["Ara3D.StepBrep.dll"], tags=["plane"], workloads=["edges"])
            benchmark.write_json(root / "suite.json", dict(schema=1, cases=[case], preparation_binary_identity=builds[0]["output_hashes"]))

            def measured(variants, cases, output, **options):
                rows = [dict(schema=1, block=0, variant=b["name"], case_id="box", workload="edges", status="ok",
                    elapsed_ns=100, iterations=1, allocated_bytes=0, digest="same", checks={"valid": True}) for b in variants]
                output.write_text("\n".join(json.dumps(r) for r in rows))
                return rows

            args = benchmark.parser().parse_args(["run", "--work", str(root), "--suite", str(root / "suite.json"),
                "--name", "quick", "--blocks", "1", "--holdout-fraction", "0"])
            with patch.object(benchmark, "run_samples", side_effect=measured):
                self.assertEqual(benchmark.command_run(args), 0)
            plan = benchmark.read_json(root / "runs/quick/run.json")
            self.assertEqual({v["name"] for v in plan["variants"]}, {"full", "shipping", "unoptimized"})
            self.assertTrue((root / "runs/quick/report.md").is_file())
            self.assertEqual(benchmark.read_json(root / "runs/quick/report.json")["correctness_anchor"], "unoptimized")
            suite = benchmark.read_json(root / "suite.json")
            suite["errors"] = [{"input": "failed.step", "error": "fixture contract failed"}]
            benchmark.write_json(root / "suite.json", suite)
            args.name = "preparation-errors"
            with patch.object(benchmark, "run_samples", side_effect=measured):
                self.assertEqual(benchmark.command_run(args), 1)
            report = root / "runs/preparation-errors/report.md"
            self.assertIn("failed.step: fixture contract failed", report.read_text())
            rebuilt = root / "reanalyzed.md"
            self.assertEqual(benchmark.command_report(benchmark.parser().parse_args(["report", "--samples",
                str(report.parent / "samples.jsonl"), "--output", str(rebuilt)])), 1)
            self.assertIn("failed.step: fixture contract failed", rebuilt.read_text())

    def test_input_list_rejects_bad_roots_and_records_missing_inputs(self):
        with tempfile.TemporaryDirectory(dir=self.test_root) as folder, contextlib.redirect_stdout(io.StringIO()):
            root = Path(folder)
            inputs = root / "inputs.json"
            build = dict(name="shipping", status="ready", worker="worker.dll", fixture_compatibility={"schema": 1})
            args = benchmark.parser().parse_args(["fixtures", "--work", str(root), "--input-list",
                str(inputs), "--output", str(root / "suite.json")])
            with patch.object(benchmark, "load_builds", return_value=[build]), patch.object(
                    benchmark, "verify_build_artifacts", return_value={"shipping": {"worker": "hash"}}):
                benchmark.write_json(inputs, [])
                with self.assertRaisesRegex(ValueError, "schema:1 input list"):
                    benchmark.prepare_fixtures(args)
                benchmark.write_json(inputs, dict(schema=1, inputs=["missing.step"]))
                self.assertEqual(benchmark.prepare_fixtures(args), 1)
                suite = benchmark.read_json(args.output)
                self.assertEqual(suite["cases"], [])
                self.assertEqual(len(suite["errors"]), 1)
                self.assertEqual(suite["errors"][0]["input"], str(root / "missing.step"))

    def test_incremental_builds_retain_only_a_matching_frozen_host(self):
        shipping = dict(name="shipping", status="ready", workspace_key="same", host_hashes={"worker": "same"}, output_hashes={"all": "same"})
        previous = [shipping, dict(name="full", status="error", error="bounded compiler timeout")]
        current = [shipping, dict(name="no-inline", status="ready")]
        self.assertEqual({b["name"] for b in benchmark.merge_build_results(previous, current)},
                         {"shipping", "full", "no-inline"})
        for replacement in ({**shipping, "workspace_key": "new"},
                            {**shipping, "host_hashes": {"worker": "changed"}},
                            {**shipping, "output_hashes": {"all": "changed"}},
                            dict(name="shipping", status="error")):
            merged = benchmark.merge_build_results(previous, [replacement, current[1]])
            self.assertEqual({b["name"] for b in merged}, {"shipping", "no-inline"})
        old_target = dict(shipping, name="target", output_hashes={"all": "old"})
        alias = dict(old_target, name="alias", alias_of="target")
        stale = dict(old_target, name="stale", workspace_key="old")
        merged = {b["name"]: b for b in benchmark.merge_build_results(
            [shipping, old_target, alias, stale], [shipping, dict(old_target, output_hashes={"all": "new"})])}
        self.assertNotIn("alias_of", merged["alias"])
        self.assertNotIn("stale", merged)

    def test_budget_exhaustion_retains_planned_samples(self):
        with tempfile.TemporaryDirectory(dir=self.test_root) as folder, contextlib.redirect_stderr(io.StringIO()):
            rows = benchmark.run_samples([{"name": "full", "status": "ready", "worker": "x.dll"}],
                self.fixture(), Path(folder) / "rows.jsonl", blocks=2, seconds=0, target_ms=1,
                warmup=1, max_iterations=1, timeout=1, seed=1,
                worker_call=lambda *a, **k: self.fail("Must not execute after budget"))
        self.assertEqual(len(rows), 4)
        self.assertTrue(all(row["status"] == "not_measured" for row in rows))

    def test_exclusive_measurement_lock(self):
        with tempfile.TemporaryDirectory(dir=self.test_root) as folder:
            with benchmark.measurement_lock(folder):
                with self.assertRaisesRegex(ValueError, "lock exists"):
                    with benchmark.measurement_lock(folder):
                        self.fail("Nested measurement must be rejected")
            self.assertFalse((Path(folder) / ".measure.lock").exists())

    def test_build_and_measurement_exclude_each_other_in_both_orders(self):
        with tempfile.TemporaryDirectory(dir=self.test_root) as folder:
            for phase in ("build", "prepare"):
                with benchmark.operation_lock(folder, phase):
                    with self.assertRaisesRegex(ValueError, "lock exists"):
                        with benchmark.measurement_lock(folder):
                            self.fail("Measurement overlaps artifact mutation")
            with benchmark.measurement_lock(folder):
                with self.assertRaisesRegex(ValueError, "lock exists"):
                    with benchmark.operation_lock(folder, "build"):
                        self.fail("Build overlaps measurement")

    def test_fixture_content_and_duplicate_ids(self):
        with tempfile.TemporaryDirectory(dir=self.test_root) as folder:
            root = Path(folder)
            (root / "fixture.json").write_text("{}")
            case = {"id": "one", "fixture": "fixture.json", "workloads": ["model"]}
            benchmark.write_json(root / "suite.json", {"schema": 1, "cases": [case]})
            suite = benchmark.suite_cases(root / "suite.json")
            self.assertEqual(suite["cases"][0]["fixture"], str((root / "fixture.json").resolve()))
            benchmark.write_json(root / "suite.json", {"schema": 1, "cases": [case, case]})
            with self.assertRaisesRegex(ValueError, "Duplicate suite"):
                benchmark.suite_cases(root / "suite.json")


if __name__ == "__main__":
    unittest.main()
