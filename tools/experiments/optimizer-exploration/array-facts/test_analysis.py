import itertools
import unittest

from analysis import (
    INT_MIN, INT_MAX, ArrayFact, Interval, add, checked_int,
    length_interval, map_range_length, proves_access, proves_loop_access,
)


class ArrayFactsTests(unittest.TestCase):
    def test_exhaustive_length_intervals(self):
        for low in range(-8, 9):
            for high in range(low, 9):
                fact = length_interval(Interval(low, high))
                values = [map_range_length(n) for n in range(low, high + 1)]
                self.assertEqual((fact.low, fact.high), (min(values), max(values)))

    def test_exhaustive_small_access_proofs(self):
        for length, start, stop, offset in itertools.product(
            range(9), range(-3, 12), range(-3, 12), range(-3, 4)
        ):
            actual = bool(range(start, stop)) and all(
                0 <= i + offset < length for i in range(start, stop)
            )
            self.assertEqual(proves_loop_access(length, start, stop, offset), actual)

    def test_exhaustive_interval_addition_small_machine(self):
        intervals = [Interval(a, b) for a in range(-8, 8) for b in range(a, 8)]
        for left, right in itertools.product(intervals, repeat=2):
            result = add(left, right, bits=4)
            values = [a + b for a in range(left.low, left.high + 1)
                      for b in range(right.low, right.high + 1)]
            if min(values) < -8 or max(values) > 7:
                self.assertIsNone(result)
            else:
                self.assertEqual((result.low, result.high), (min(values), max(values)))

    def test_conversion_order_and_boundaries(self):
        for n in (INT_MIN - 1, INT_MAX + 1, -(2**63), 2**63 - 1):
            with self.assertRaises(OverflowError):
                map_range_length(n)
            self.assertIsNone(length_interval(Interval(n, n)))
        self.assertEqual(map_range_length(INT_MIN), 0)
        self.assertEqual(map_range_length(INT_MAX), INT_MAX)
        # Clamping before conversion incorrectly suppresses the negative overflow.
        self.assertEqual(checked_int(max(0, INT_MIN - 1)), 0)

    def test_profile_overflow_and_off_by_one(self):
        for bits in (32, 64):
            maximum = 2 ** (bits - 1) - 1
            self.assertIsNone(add(Interval(maximum, maximum), Interval(1, 1), bits))
            self.assertIsNone(add(Interval(maximum + 1, maximum + 1), Interval(-1, -1), bits))
        self.assertFalse(proves_loop_access(4, 0, 4, 1))
        self.assertTrue(proves_loop_access(4, 0, 3, 1))
        self.assertFalse(proves_loop_access(0, 0, 0, 0))

    def test_length_is_not_capacity_or_evaluation_permission(self):
        index = Interval(3, 3)
        self.assertFalse(proves_access(ArrayFact(Interval(3, 3), True, True), index))
        for stable, materialized in ((False, True), (True, False), (False, False)):
            self.assertFalse(proves_access(ArrayFact(Interval(4, 4), stable, materialized), index))
        self.assertFalse(proves_access(ArrayFact(Interval(0, 4), True, True), index))

    def test_count_replacement_must_keep_eager_callback_fault(self):
        trace = []

        def eager_count():
            for i in range(map_range_length(3)):
                trace.append(i)
                if i == 1:
                    raise ValueError("callback fault")
            return 3

        with self.assertRaisesRegex(ValueError, "callback fault"):
            eager_count()
        self.assertEqual(trace, [0, 1])
        # A length fact predicts successful output size, not successful evaluation.
        self.assertEqual(length_interval(Interval(3, 3)), Interval(3, 3))


if __name__ == "__main__":
    unittest.main(verbosity=2)
