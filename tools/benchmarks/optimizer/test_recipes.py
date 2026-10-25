import unittest
from recipes import FULL_FLAGS, make_recipes, validate_flags, without_feature


class RecipeTests(unittest.TestCase):
    def test_loop_ablation_removes_dependencies(self):
        recipe = next(r for r in make_recipes(["--loops", "--fuse"]) if r["name"] == "no-loops")
        self.assertEqual(set(recipe["removed_flags"]), {"--loops", "--fuse-consumers", "--exact-output"})
        self.assertEqual(recipe["flags"], ["--inline", "--fuse", "--optimize"])

    def test_inline_ablation_removes_policy_and_growth(self):
        flags = [*FULL_FLAGS, "--inline-policy=statements", "--inline-growth=128"]
        self.assertFalse(any("inline" in f for f in without_feature(flags, "--inline")))

    def test_ordinary_full_does_not_enable_experimental_policy(self):
        recipes = make_recipes(["--loops"])
        self.assertEqual(next(r for r in recipes if r["name"] == "full")["flags"], list(FULL_FLAGS))
        self.assertEqual(next(r for r in recipes if r["name"] == "optimize-arrays")["status"], "unsupported")

    def test_invalid_dependency_and_parameter_matrices_rejected(self):
        for flags in (["--fuse-consumers"], ["--exact-output"], ["--optimize-arrays"],
                      ["--inline-policy=statements"], ["--inline", "--inline-growth=128"],
                      ["--inline", "--inline-policy=unknown"],
                      ["--inline", "--inline-policy=iterative", "--inline-growth=0"]):
            with self.subTest(flags=flags), self.assertRaises(ValueError):
                validate_flags(flags)

    def test_sweeps_are_opt_in_and_one_factor(self):
        with self.assertRaises(ValueError):
            make_recipes([], growth_limits=[128])
        recipes = make_recipes([], experimental=True, inline_policies=["lambdas", "statements"], growth_limits=[128, 256])
        self.assertEqual(len([r for r in recipes if r.get("experimental")]), 4)
        self.assertEqual(next(r for r in recipes if r["name"] == "inline-statements-growth-128")["flags"],
                         [*FULL_FLAGS, "--inline-policy=statements", "--inline-growth=128"])


if __name__ == "__main__":
    unittest.main()
