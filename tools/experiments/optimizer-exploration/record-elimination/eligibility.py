"""Executable admission-contract model; does not inspect actual Plato TIR."""
from dataclasses import dataclass, replace
import unittest


@dataclass(frozen=True)
class Projection:
    resolved_field: bool = True
    generated_data_constructor: bool = True
    exact_field_types: bool = True
    field_index: int = 0
    arity: int = 2
    direct_return_slot: bool = True
    escaping: bool = False
    reassigned: bool = False
    captured: bool = False
    coercion_around_record: bool = False


def eligible(shape):
    """R1 emits every argument to an ordered typed binding, including unused fields."""
    return (shape.resolved_field and shape.generated_data_constructor
            and shape.exact_field_types and 0 <= shape.field_index < shape.arity
            and shape.direct_return_slot and not shape.escaping
            and not shape.reassigned and not shape.captured
            and not shape.coercion_around_record)


class AdmissionTests(unittest.TestCase):
    def test_resolved_direct_projection(self):
        self.assertTrue(eligible(Projection()))

    def test_refusals(self):
        for name, value in [('resolved_field', False), ('generated_data_constructor', False),
                            ('exact_field_types', False), ('field_index', -1),
                            ('field_index', 2), ('direct_return_slot', False),
                            ('escaping', True), ('reassigned', True), ('captured', True),
                            ('coercion_around_record', True)]:
            with self.subTest(name=name, value=value):
                self.assertFalse(eligible(replace(Projection(), **{name: value})))


if __name__ == '__main__':
    unittest.main()
