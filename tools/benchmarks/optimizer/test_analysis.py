import math
import unittest

from analysis import analyze, render_markdown


def samples(count=5, ratio=0.8, case_id="a", workload="surface", family="geometry"):
    rows = []
    for block in range(count):
        for variant, time, iterations in (("full", 100, 1), ("shipping", 150, 1), ("candidate", 200*ratio, 2)):
            rows.append(dict(schema=1, block=block, variant=variant, case_id=case_id,
                             workload=workload, status="ok", elapsed_ns=time, iterations=iterations,
                             allocated_bytes=0, digest="same", checks={"finite": True}, family=family))
    return rows


def comparison(report, variant="candidate", control="full", case_id="a"):
    return next(c for c in report["comparisons"] if c["variant"] == variant and c["control"] == control
                and c["case_id"] == case_id)


class AnalysisTests(unittest.TestCase):
    def test_pairs_normalize_actual_iterations_and_allow_zero_allocation(self):
        result = comparison(analyze(samples()))
        self.assertEqual(result["status"], "faster")
        self.assertAlmostEqual(result["time_ratio"], 0.8)
        self.assertEqual(result["allocated_bytes_per_operation"], 0)

    def test_small_samples_are_inconclusive(self):
        result = comparison(analyze(samples(2)))
        self.assertEqual(result["status"], "inconclusive")
        self.assertNotIn("time_ratio_ci95", result)

    def test_missing_duplicate_failed_or_disagreeing_pairs_have_no_speedup(self):
        for defect in ("missing", "duplicate", "digest", "checks", "error", "timeout"):
            with self.subTest(defect=defect):
                rows = samples()
                candidate = rows[2]
                if defect == "missing":
                    rows.remove(candidate)
                elif defect == "duplicate":
                    rows.append(candidate.copy())
                elif defect == "digest":
                    candidate["digest"] = "different"
                elif defect == "checks":
                    candidate["checks"] = {"finite": False}
                else:
                    candidate["status"] = defect
                result = comparison(analyze(rows))
                self.assertEqual(result["status"], "invalid")
                self.assertNotIn("time_ratio", result)

    def test_bad_numbers_and_missing_checks_never_produce_speedups(self):
        invalid = [("elapsed_ns", float("nan")), ("elapsed_ns", math.inf), ("elapsed_ns", 0),
                   ("iterations", 0), ("iterations", True), ("iterations", 1.5),
                   ("allocated_bytes", -1), ("allocated_bytes", math.inf), ("checks", {}),
                   ("checks", {"ok": 1}), ("digest", ""), ("schema", 2)]
        for field, value in invalid:
            with self.subTest(field=field, value=value):
                rows = samples()
                rows[2][field] = value
                self.assertEqual(comparison(analyze(rows))["status"], "invalid")

    def test_malformed_identity_invalidates_comparisons(self):
        report = analyze(samples()+[{"status": "ok"}])
        self.assertTrue(report["malformed_rows"])
        self.assertNotIn("time_ratio", comparison(report))

    def test_planned_variant_and_budget_skips_are_explicit(self):
        report = analyze(samples(), ["full", "candidate", "absent"])
        self.assertEqual(comparison(report, "absent")["status"], "unmeasured")
        rows = samples()
        for r in rows:
            if r["variant"] == "candidate":
                r["status"] = "not_measured"
        self.assertEqual(comparison(analyze(rows))["status"], "unmeasured")

    def test_family_balance_excludes_models_and_duplicate_face_weight(self):
        rows = samples(ratio=0.5, case_id="geometry", family="geometry")
        for i in range(10):
            rows += samples(ratio=2, case_id=f"meshing-{i}", family="meshing")
        rows += samples(ratio=100, case_id="model", workload="model")
        report = analyze(rows)
        summary = next(s for s in report["summaries"] if s["variant"] == "candidate" and s["control"] == "full")
        self.assertAlmostEqual(summary["balanced_time_ratio"], 1)
        self.assertEqual(summary["expected_workloads"], 11)
        self.assertTrue(report["model_totals"])
        self.assertTrue(report["worst_regressions"])

    def test_noisy_results_and_threshold(self):
        rows = samples(ratio=1)
        for r in rows:
            if r["variant"] == "candidate":
                r["elapsed_ns"] *= 0.5 if r["block"] % 2 else 2
        self.assertEqual(comparison(analyze(rows))["status"], "inconclusive")
        self.assertEqual(comparison(analyze(samples(ratio=1.01)))["status"], "equivalent")
        self.assertEqual(comparison(analyze(samples(ratio=1.2)))["status"], "slower")

    def test_custom_baseline_and_report_include_errors(self):
        report = analyze(samples(), baseline="candidate")
        self.assertEqual(comparison(report, "full", "candidate")["status"], "slower")
        self.assertIn("95%", render_markdown(report))

    def test_matching_pairs_still_reject_nondeterministic_output_across_blocks(self):
        rows = samples()
        for row in rows:
            if row["block"] == 1:
                row["digest"] = "changed"
        result = comparison(analyze(rows))
        self.assertEqual(result["status"], "invalid")
        self.assertNotIn("speedup", result)

    def test_all_timeouts_stay_explicit_and_are_not_speedups(self):
        rows = samples()
        for row in rows:
            if row["variant"] == "candidate":
                row["status"] = "timeout"
        result = comparison(analyze(rows))
        self.assertIn("timeout", result["observed_states"])
        self.assertNotIn("speedup", result)


if __name__ == "__main__":
    unittest.main()
