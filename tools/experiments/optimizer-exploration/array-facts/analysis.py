"""Conservative arithmetic model, not a Plato TIR pass or runtime benchmark."""
from dataclasses import dataclass

INT_MIN = -(2**31)
INT_MAX = 2**31 - 1


@dataclass(frozen=True)
class Interval:
    low: int
    high: int

    def __post_init__(self):
        if self.low > self.high:
            raise ValueError("Empty intervals are represented by None")


def checked_int(value):
    if not INT_MIN <= value <= INT_MAX:
        raise OverflowError("Integer to collection index conversion")
    return value


def map_range_length(count):
    # Integer.MapRange converts before ArrayIntrinsics handles negative counts.
    return max(0, checked_int(count))


def length_interval(counts):
    """No certificate if any input can fault during the collection conversion."""
    if counts.low < INT_MIN or counts.high > INT_MAX:
        return None
    return Interval(max(0, counts.low), max(0, counts.high))


def add(left, right, bits):
    """Refuse wrapping arithmetic rather than interpreting it as unbounded math."""
    minimum, maximum = -(2 ** (bits - 1)), 2 ** (bits - 1) - 1
    if any(x.low < minimum or x.high > maximum for x in (left, right)):
        return None
    low, high = left.low + right.low, left.high + right.high
    if low < minimum or high > maximum:
        return None
    return Interval(low, high)


@dataclass(frozen=True)
class ArrayFact:
    length: Interval
    stable_owned: bool
    materialized: bool


def proves_access(fact, indices):
    """A sufficient proof for all combinations; no unchecked access is emitted."""
    return (
        fact.materialized and fact.stable_owned and indices is not None
        and 0 <= indices.low <= indices.high < fact.length.low
        and indices.high <= INT_MAX
    )


def proves_loop_access(length, start, stop, offset, bits=64):
    """Analyze start <= i < stop with i + offset; empty loops prove no access."""
    if stop <= start:
        return False
    indices = add(Interval(start, stop - 1), Interval(offset, offset), bits)
    return proves_access(ArrayFact(Interval(length, length), True, True), indices)
