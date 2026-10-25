---
id: compiler-530
title: Bare generic function receiver throws during library classification
type: bug
status: idea
priority: p2
effort: M
risk: med
area: compiler
sprint: 
created: 2026-10-25
closed:
links: [docs/reference/implementation/discrepancies.md]
---

Observed by the executable docs-523 fixture under a fresh isolated CLI build. A function with
a bare signature variable in receiver position and an explicit bound fails before checking:

```plato
interface IReferenceValue { ReferenceValue(x: Self): Integer; }
library Example {
    F(x: $T): Integer where $T: IReferenceValue => x.ReferenceValue;
}
```

`Compilation.AddLibraryFunctionsToReifiedTypes` throws
`First parameter type in function ... F(x: $T): Integer [Library]; not a concrete type or interface`.
Its classification handles concrete and interface receivers but not a signature variable;
the declared bound does not change that branch. The CLI exits 1 without a normal located
diagnostic. A concrete/constructed receiver with a separate generic argument works in the
revised executable reference fixture.

Decide whether the language supports bare bounded receivers. Implement their library grounding
if supported; otherwise reject them with a clear diagnostic before classification. Do not infer
the intended language restriction from this raw exception alone.

## Done means

- [ ] Add a minimal positive/negative fixture for a bounded bare-variable receiver.
- [ ] Support its grounding or emit a located explicit rejection instead of an exception.
- [ ] Reconcile the generic receiver rule in SEMANTICS and the hierarchical reference.
