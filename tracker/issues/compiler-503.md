---
id: compiler-503
title: Assess component expansion for forward vector vocabulary
type: problem
status: idea
priority: p2
effort: M
risk: med
area: compiler
sprint: 
created: 2026-10-24
closed:
links: [compiler-499]
---

During compiler-499's full B-rep matrix, --optimize without inlining produced no
TirComponentAccess or TirConstructorCall nodes; its small changes came from fixed-array
rules. ComponentUnroller.BuildComponentFieldTable accepts only concrete IArrayLike
implementors. The pinned B-rep program's input files contain no IArrayLike declaration
or use; forward Number3 instead implements IVector. TirComponentUnroller and
CSharpWriter.GetComponentPlatoType depend on that legacy table.

This is a coverage/design question, not proof that every forward vector body should
be unrolled or that a broader pass will be faster. It limits what the current flag
can tell us about inlining enabling component optimization. Investigate the intended
forward contracts and relevant remaining combinator shapes before changing guards.

## Done means

- [ ] Identify representative forward vector/geometry operations and whether their emitted code needs component expansion.
- [ ] Decide whether to adapt the pass to forward contracts or document/retire its legacy-specific path.
- [ ] Validate any implemented transformations with executable equivalence and paired meshing/allocation measurements.
