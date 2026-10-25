import unittest

from selection import select_cases


def case(name, tags, workloads=None):
    return dict(id=name, fixture=name+".json", tags=tags, workloads=workloads or ["surface"])


class SelectionTests(unittest.TestCase):
    def test_covers_dimensions_and_avoids_repeated_faces(self):
        suite = dict(cases=[case("a", ["family:analytic", "small"]),
                            case("b", ["family:analytic", "small"]),
                            case("c", ["family:spline", "large"], ["projection"])])
        result = select_cases(suite, holdout_fraction=0)
        self.assertEqual(len(result["cases"]), 2)
        self.assertEqual(result["uncovered_dimensions"], [])
        self.assertIsNone(result["estimated_ms"])

    def test_holdout_reproducible_across_input_order_and_never_claims_prediction(self):
        cases = [case(str(i), ["family:"+str(i)]) for i in range(10)]
        a = select_cases(dict(cases=cases))
        b = select_cases(dict(cases=cases[::-1]))
        self.assertEqual(a, b)
        self.assertTrue(a["holdout"])
        self.assertFalse(set(a["holdout"]) & {c["id"] for c in a["cases"]})
        self.assertFalse(a["prediction_validated"])
        self.assertTrue(a["uncovered_dimensions"])

    def test_runtime_budget_and_cost_weighting(self):
        suite = dict(cases=[case("slow", ["a", "b"]), case("fast", ["a"]), case("other", ["b"])])
        result = select_cases(suite, holdout_fraction=0, budget_ms=3,
                              discovery={"slow": 100, "fast": 1, "other": 2})
        self.assertEqual({c["id"] for c in result["cases"]}, {"fast", "other"})
        self.assertEqual(result["estimated_ms"], 3)
        self.assertFalse(result["uncovered_dimensions"])
        with self.assertRaises(ValueError):
            select_cases(suite, budget_ms=3)

    def test_confirm_affected_and_corpus_includes_reserved(self):
        suite = dict(cases=[case("a", ["one"]), case("b", ["two"], ["mesh"])])
        result = select_cases(suite, mode="confirm", affected=["mesh"])
        self.assertEqual([c["id"] for c in result["cases"]], ["b"])
        self.assertEqual(len(select_cases(suite, mode="corpus")["cases"]), 2)
        with self.assertRaises(ValueError):
            select_cases(suite, mode="confirm")

    def test_max_cases_empty_single_and_invalid_input(self):
        suite = dict(cases=[case(str(i), [str(i)]) for i in range(8)])
        self.assertEqual(len(select_cases(suite, max_cases=2)["cases"]), 2)
        self.assertEqual(select_cases(dict(cases=[]))["cases"], [])
        self.assertEqual(select_cases(dict(cases=[case("a", ["a"])]))["holdout"], [])
        with self.assertRaises(ValueError):
            select_cases(dict(cases=[case("a", []), case("a", [])]))
        for costs in ({"0": float("nan")}, {"0": -1}, {"0": True}):
            with self.assertRaises(ValueError):
                select_cases(suite, discovery=costs)

    def test_confirm_narrows_workloads_without_mutating_suite(self):
        import copy
        suite = dict(cases=[case("a", ["one"], ["projection", "mesh", "model"]),
                            case("b", ["two"], ["projection", "surface"])])
        original = copy.deepcopy(suite)
        selected = select_cases(suite, mode="confirm", affected=["projection"])
        self.assertEqual([c["workloads"] for c in selected["cases"]], [["projection"], ["projection"]])
        self.assertNotIn("workload:mesh", selected["covered_dimensions"])
        self.assertIn("workload:mesh", selected["uncovered_dimensions"])
        selected = select_cases(suite, mode="confirm", affected=["a", "projection"])
        self.assertEqual(next(c for c in selected["cases"] if c["id"] == "a")["workloads"],
                         ["projection", "mesh", "model"])
        self.assertEqual(suite, original)


if __name__ == "__main__":
    unittest.main()
