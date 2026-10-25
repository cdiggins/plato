import random
import unittest
from model import Action, BooleanRead, Branch, Call, GuardedChoice, Loop, State, Symbol, unswitch


class UnswitchTests(unittest.TestCase):
    def setUp(self):
        self.flag, self.index = Symbol("flag"), Symbol("i")
        self.yes = Action(lambda s: s.result.append(s.values[self.index] * 3 + 1))
        self.no = Action(lambda s: s.result.append(s.values[self.index] - 7))

    def loop(self, count, condition=None, yes=None, no=None):
        return Loop(count, self.index, Branch(condition or BooleanRead(self.flag),
                                             yes or self.yes, no or self.no))

    def rewrite(self, loop, **kwargs):
        return unswitch(loop, {self.flag}, frozenset(), **kwargs)

    def test_differential_sizes_flags_and_generated_cases(self):
        rng = random.Random(510)
        counts = [-10, -1, 0, 1, 2, 17, 100] + [rng.randrange(-10, 200) for _ in range(200)]
        for count in counts:
            for flag in (False, True):
                original = self.loop(count)
                rewrite = self.rewrite(original)
                self.assertEqual("accepted", rewrite.reason)
                before, after = State({self.flag: flag}), State({self.flag: flag})
                original.run(before)
                rewrite.node.run(after)
                self.assertEqual(before.result, after.result)
                self.assertEqual(before.events, after.events)
                self.assertEqual(max(0, count), before.reads)
                self.assertEqual(int(count > 0), after.reads)

    def test_empty_does_not_read_condition_even_in_model(self):
        for count in (-1, 0):
            self.rewrite(self.loop(count)).node.run(State({}))

    def test_body_fault_and_unselected_fault_preserved(self):
        def fault(s):
            s.events.append("body fault")
            raise ValueError("body")
        for flag in (False, True):
            loop = self.loop(3, yes=Action(fault))
            states = [State({self.flag: flag}), State({self.flag: flag})]
            for node, state in zip([loop, self.rewrite(loop).node], states):
                if flag:
                    with self.assertRaisesRegex(ValueError, "body"):
                        node.run(state)
                else:
                    node.run(state)
            self.assertEqual(states[0].events, states[1].events)
            self.assertEqual(states[0].result, states[1].result)

    def test_refuse_mutable_capture_and_show_wrong_result(self):
        def toggle(s):
            s.result.append("yes")
            s.values[self.flag] = False
        loop = self.loop(3, yes=Action(toggle, frozenset({self.flag})))
        self.assertEqual("condition binding is assigned", self.rewrite(loop).reason)
        before, after = State({self.flag: True}), State({self.flag: True})
        loop.run(before)
        GuardedChoice(loop, BooleanRead(self.flag), Loop(3, self.index, loop.body.yes),
                      Loop(3, self.index, loop.body.no)).run(after)
        self.assertNotEqual(before.result, after.result)

    def test_global_assignment_summary_and_symbol_identity(self):
        loop = self.loop(3)
        self.assertEqual("condition binding is assigned",
                         unswitch(loop, {self.flag}, {self.flag}).reason)
        shadow = Symbol("flag")
        self.assertEqual("accepted", unswitch(loop, {self.flag}, {shadow}).reason)
        self.assertNotEqual("accepted", self.rewrite(self.loop(3, BooleanRead(shadow))).reason)

    def test_refuse_iteration_condition_and_unknown_outer(self):
        for symbol in (self.index, Symbol("external")):
            self.assertNotEqual("accepted", self.rewrite(self.loop(3, BooleanRead(symbol))).reason)

    def test_refuse_calls_and_show_zero_trip_fault(self):
        def fault(s):
            raise ValueError("condition")
        loop = self.loop(0, Call(fault))
        loop.run(State({}))
        self.assertNotEqual("accepted", self.rewrite(loop).reason)
        with self.assertRaisesRegex(ValueError, "condition"):
            loop.body.condition.eval(State({}))

    def test_refuse_calls_with_observable_evaluation_count(self):
        def condition(s):
            s.events.append("condition")
            return True
        loop = self.loop(3, Call(condition))
        self.assertNotEqual("accepted", self.rewrite(loop).reason)
        state = State({})
        loop.run(state)
        self.assertEqual(["condition"] * 3, state.events)

    def test_hoisting_fault_changes_preceding_fault_order(self):
        def prefix(s):
            raise ValueError("prefix")
        def condition(s):
            raise ValueError("condition")
        loop = Loop(1, self.index, Action(prefix))
        self.assertNotEqual("accepted", self.rewrite(loop).reason)
        with self.assertRaisesRegex(ValueError, "prefix"):
            loop.run(State({}))
        with self.assertRaisesRegex(ValueError, "condition"):
            Call(condition).eval(State({}))

    def test_refuse_nested_branches_and_budget(self):
        nested = self.loop(3, yes=self.loop(3).body)
        self.assertNotEqual("accepted", self.rewrite(nested).reason)
        self.assertEqual("duplication budget exceeded", self.rewrite(self.loop(3), node_budget=1).reason)


if __name__ == "__main__":
    unittest.main(verbosity=2)
