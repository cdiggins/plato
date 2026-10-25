"""Executable model, not a Plato compiler pass. Run with Python 3 standard library."""
from dataclasses import dataclass, replace
import json
import unittest


@dataclass(frozen=True)
class Proof:
    # These are supplied proof obligations, not analysis inferred by this model.
    symbol: str | None = "resolved-sample"
    capture_identity: str | None = "immutable-grid"
    total: bool = True
    immutable: bool = True
    exact_integer_keys: bool = True
    ordered_rectangular_traversal: bool = True

    def permits_reuse(self):
        return bool(self.symbol and self.capture_identity and self.total
                    and self.immutable and self.exact_integer_keys
                    and self.ordered_rectangular_traversal)


def cells(width, height):
    for y in range(height):
        for x in range(width):
            yield ((x, y), (x + 1, y), (x, y + 1), (x + 1, y + 1))


def evaluate(width, height, sample, proof=None):
    """Return cell corner tuples, evaluation trace, and peak cache value slots.

    Fill on demand in source order. Evict rows before the next cell row;
    no speculative border sampling and no cross-invocation cache lifetime.
    """
    if width < 0 or height < 0:
        raise ValueError("model dimensions must be nonnegative")
    reuse = proof is not None and proof.permits_reuse()
    cache, trace, output = {}, [], []
    peak = 0
    for corners in cells(width, height):
        if reuse and corners[0][0] == 0:
            y = corners[0][1]
            for key in list(cache):
                if key[1] < y:
                    del cache[key]
        values = []
        for key in corners:
            if not reuse or key not in cache:
                trace.append(key)
                value = sample(*key)
                if reuse:
                    cache[key] = value
                    peak = max(peak, len(cache))
            else:
                value = cache[key]
            values.append(value)
        output.append(tuple(values))
    return output, trace, peak


def sample(x, y):
    return (x * 13 - y * 7) ** 2


class ReuseTests(unittest.TestCase):
    def test_grid_shapes_and_first_demand_order(self):
        for width in range(9):
            for height in range(9):
                baseline, trace, _ = evaluate(width, height, sample)
                reused, visits, peak = evaluate(width, height, sample, Proof())
                self.assertEqual(baseline, reused)
                self.assertEqual(visits, list(dict.fromkeys(trace)))
                self.assertEqual(len(visits), (width + 1) * (height + 1)
                                 if width and height else 0)
                self.assertLessEqual(peak, 2 * (width + 1))

    def test_empty_does_not_sample_even_a_faulting_callback(self):
        def fault(*_):
            raise RuntimeError("must remain uncalled")
        for shape in ((0, 0), (0, 5), (5, 0)):
            self.assertEqual(evaluate(*shape, fault, Proof()), ([], [], 0))

    def test_boundary_never_reads_outside_required_nodes(self):
        def bounded(x, y):
            self.assertTrue(0 <= x <= 3 and 0 <= y <= 2)
            return sample(x, y)
        self.assertEqual(evaluate(3, 2, bounded)[0],
                         evaluate(3, 2, bounded, Proof())[0])

    def test_every_missing_proof_refuses_rewrite(self):
        baseline = evaluate(3, 2, sample)
        for name, value in (("symbol", None), ("capture_identity", None),
                            ("total", False), ("immutable", False),
                            ("exact_integer_keys", False),
                            ("ordered_rectangular_traversal", False)):
            self.assertEqual(evaluate(3, 2, sample, replace(Proof(), **{name: value})),
                             baseline)

    def test_stateful_callback_is_an_unsafe_counterexample(self):
        def run(proof):
            counter = 0
            def changing(x, y):
                nonlocal counter
                counter += 1
                return counter
            return evaluate(2, 1, changing, proof)[0]
        self.assertNotEqual(run(None), run(Proof()))  # A lying certificate is unsafe.
        self.assertEqual(run(None), run(Proof(immutable=False)))

    def test_different_captures_cannot_share_a_coordinate_cache(self):
        first, second = lambda x: x + 1, lambda x: x + 10
        wrong_cache = {0: first(0)}
        self.assertNotEqual(wrong_cache[0], second(0))
        # Correct model caches live within one invocation only.
        self.assertNotEqual(evaluate(1, 1, lambda x, y: first(x), Proof())[0],
                            evaluate(1, 1, lambda x, y: second(x), Proof())[0])

    def test_eager_prefill_changes_first_fault(self):
        def fail(x, y):
            if (x, y) in ((0, 1), (2, 0)):
                raise RuntimeError(str((x, y)))
            return 0
        with self.assertRaisesRegex(RuntimeError, r"\(0, 1\)"):
            evaluate(2, 1, fail)
        with self.assertRaisesRegex(RuntimeError, r"\(2, 0\)"):
            for y in range(2):
                for x in range(3):
                    fail(x, y)

    def test_integer_key_does_not_authorize_coordinate_reassociation(self):
        origin, step, index = 1e16, 1.0, 1
        self.assertNotEqual((origin + index * step) + step,
                            origin + (index + 1) * step)


def evidence():
    width, height = 64, 64
    original, trace, _ = evaluate(width, height, sample)
    reused, visits, peak = evaluate(width, height, sample, Proof())
    assert original == reused
    return {"model": "2D rectangular corner stencil; not emitted C#",
            "cells": [width, height], "baseline_sample_calls": len(trace),
            "reuse_sample_calls": len(visits), "peak_cached_value_slots": peak,
            "full_grid_value_slots": (width + 1) * (height + 1),
            "timing_measured": False}


if __name__ == "__main__":
    result = unittest.TextTestRunner(verbosity=2).run(
        unittest.defaultTestLoader.loadTestsFromTestCase(ReuseTests))
    print(json.dumps(evidence(), indent=2))
    raise SystemExit(0 if result.wasSuccessful() else 1)
