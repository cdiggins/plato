"""Executable semantic model; this does not transform Plato TIR or emit C#."""
from dataclasses import dataclass, field
from typing import Callable


@dataclass(frozen=True, eq=False)
class Symbol:
    name: str


@dataclass
class State:
    values: dict
    events: list = field(default_factory=list)
    reads: int = 0
    result: list = field(default_factory=list)


@dataclass(frozen=True)
class BooleanRead:
    symbol: Symbol

    def eval(self, state):
        state.reads += 1
        value = state.values[self.symbol]
        if type(value) is not bool:
            raise TypeError("Expected resolved Boolean")
        return value


@dataclass(frozen=True)
class Call:
    function: Callable

    def eval(self, state):
        return self.function(state)


@dataclass(frozen=True)
class Action:
    function: Callable
    writes: frozenset = frozenset()
    nodes: int = 1

    def run(self, state):
        self.function(state)


@dataclass(frozen=True)
class Branch:
    condition: object
    yes: object
    no: object

    def run(self, state):
        (self.yes if self.condition.eval(state) else self.no).run(state)


@dataclass(frozen=True)
class Loop:
    count: int
    index: Symbol
    body: object

    def run(self, state):
        for index in range(max(0, self.count)):
            state.values[self.index] = index
            self.body.run(state)


@dataclass(frozen=True)
class GuardedChoice:
    """Setup/count/result storage are outside this loop-region model."""
    original: Loop
    condition: BooleanRead
    yes: Loop
    no: Loop

    def run(self, state):
        if self.original.count > 0:
            (self.yes if self.condition.eval(state) else self.no).run(state)


@dataclass(frozen=True)
class Decision:
    node: object
    reason: str


def unswitch(loop, outer_booleans, assigned, node_budget=32):
    """First-slice proof: direct condition, initialized Boolean leaf, immutable binding.

    outer_booleans/assigned must be computed from resolved bindings in a future pass.
    Unknown condition expressions, statement prefixes and nested branches fail closed.
    """
    branch = loop.body
    if not isinstance(branch, Branch):
        return Decision(loop, "not a direct branch")
    condition = branch.condition
    if not isinstance(condition, BooleanRead):
        return Decision(loop, "condition is not a total Boolean leaf")
    if condition.symbol is loop.index or condition.symbol not in outer_booleans:
        return Decision(loop, "condition is not an outer Boolean binding")
    if not isinstance(branch.yes, Action) or not isinstance(branch.no, Action):
        return Decision(loop, "nested/control-flow body requires another proof")
    written = assigned | branch.yes.writes | branch.no.writes
    if condition.symbol in written:
        return Decision(loop, "condition binding is assigned")
    if branch.yes.nodes + branch.no.nodes > node_budget:
        return Decision(loop, "duplication budget exceeded")
    yes = Loop(loop.count, loop.index, branch.yes)
    no = Loop(loop.count, loop.index, branch.no)
    return Decision(GuardedChoice(loop, condition, yes, no), "accepted")
