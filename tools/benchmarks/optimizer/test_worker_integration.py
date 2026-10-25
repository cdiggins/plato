"""Optional executable checks: set PLATO_BENCHMARK_WORKER to a built worker DLL."""
import json
import os
from pathlib import Path
import tempfile
import unittest

import benchmark


@unittest.skipUnless(os.environ.get("PLATO_BENCHMARK_WORKER"), "Set PLATO_BENCHMARK_WORKER for real generated-code checks")
class WorkerIntegrationTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.worker = Path(os.environ["PLATO_BENCHMARK_WORKER"]).resolve(strict=True)
        root = benchmark.ROOT / ".temp/optimizer-integration-tests"
        root.mkdir(parents=True, exist_ok=True)
        cls.temporary = tempfile.TemporaryDirectory(dir=root)
        cls.folder = Path(cls.temporary.name)
        cls.fixture = cls.folder / "fixture.json"
        cls.preparation = benchmark.invoke_worker(cls.worker, ["prepare", "--input", "witness:UnitBox",
            "--output", cls.fixture, "--id", "integration-box", "--chordal", "0.1"], timeout=60)
        if cls.preparation["status"] != "ok":
            raise AssertionError(cls.preparation)

    @classmethod
    def tearDownClass(cls):
        cls.temporary.cleanup()

    def test_prepared_wrapper_and_sum_values_round_trip_exactly(self):
        second = self.folder / "second.json"
        result = benchmark.invoke_worker(self.worker, ["prepare", "--input", "witness:UnitBox",
            "--output", second, "--id", "integration-box", "--chordal", "0.1"], timeout=60)
        self.assertEqual(result["status"], "ok", result)
        self.assertEqual(self.fixture.read_bytes(), second.read_bytes())
        self.assertEqual(benchmark.file_hash(self.fixture), self.preparation["fixture_hash"])

    def test_real_stages_return_deterministic_valid_measured_outputs(self):
        workloads = self.preparation["workloads"]
        result = benchmark.invoke_worker(self.worker, ["run-set", "--fixture", self.fixture,
            "--workloads", ",".join(workloads), "--warmup", "1", "--warmup-ms", "1",
            "--target-ms", "1", "--max-iterations", "2"], timeout=60)
        self.assertEqual(result["status"], "ok", result)
        self.assertEqual({s["workload"] for s in result["samples"]}, set(workloads))
        for sample in result["samples"]:
            with self.subTest(workload=sample["workload"]):
                self.assertEqual(sample["status"], "ok", sample)
                self.assertGreater(sample["elapsed_ns"], 0)
                self.assertGreater(sample["iterations"], 0)
                self.assertGreaterEqual(sample["allocated_bytes"], 0)
                self.assertTrue(sample["checks"])
                self.assertTrue(all(v is True for v in sample["checks"].values()), sample)
                self.assertTrue(sample["checks"]["input_unchanged"])
                self.assertTrue(sample["checks"]["repeat_digest_matches"])

    def test_bad_fixture_schema_and_absent_workload_are_errors(self):
        data = json.loads(self.fixture.read_text())
        data["schema"] = 999
        bad = self.folder / "invalid.json"
        bad.write_text(json.dumps(data), encoding="utf-8")
        result = benchmark.invoke_worker(self.worker, ["run", "--fixture", bad, "--workload", "model"], timeout=30)
        self.assertEqual(result["status"], "error")
        self.assertIn("Unsupported fixture schema", result["error"])
        result = benchmark.invoke_worker(self.worker, ["run", "--fixture", self.fixture,
            "--workload", "absent-workload"], timeout=30)
        self.assertEqual(result["status"], "error")
        self.assertIn("absent", result["error"])


if __name__ == "__main__":
    unittest.main()
