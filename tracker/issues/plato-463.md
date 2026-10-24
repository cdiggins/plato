---
id: plato-463
title: Interpreter: whole-model laws still cost about 1 microsecond per call; EdgeReversalPreservesMesh 15 s, a no-memo case minutes
type: debt
status: ready
priority: p2
effort: M
risk: med
area: plato
sprint: 
created: 2026-10-18
closed:
links: [src/Plato.Interpreter/Evaluation/Interpreter.cs, src/Plato.Interpreter/Evaluation/Dispatch.cs, src/Plato.Interpreter/Evaluation/CallMemo.cs, tests/Plato.Interpreter.Tests/SandboxProbe.cs]
---

## Issue

After the per-call work of 2026-10-18 (90685d9a host entry per function, 93ba7552 resolution keys
by type id, 3a01b55a scopes as arrays and no closures on entry, 8f741e0b shared boxes), the
whole-model sandbox laws of `bim-open-toolkit/sandbox/step-brep-mesh` are about twice as fast but
still slow. Measured with `SandboxProbe.Profile_law` (Explicit; `PLATO_LAW=<filters>`,
`PLATO_ALLOC=1` for allocation by type, `PLATO_PROFILE=1` for the per-function profile,
`PLATO_NOMEMO=1 PLATO_TRIALS=n` for a run whose work does not depend on timing), cpu64, on a
shared machine:

| Run | Before (e5c28aaf) | After (8f741e0b) |
|---|---|---|
| `StepToMeshLaws.Law_EdgeReversalPreservesMesh`, 25 cases, memo on: process CPU | 27.9-28.3 s | 14.0-16.2 s |
| the same: bytes allocated | 27.4-28.3 GB | 6.5-10.9 GB |
| `Law_PowerOfTwoScalingScalesPositions`, 2 cases, memo off: process CPU | not measured | 172 s (124 GB allocated) |

What remains, largest first by the allocation sample and the code:

1. Every call allocates its evaluated argument array (`Object[]`, 3.7 of the 6.5 GB) and every
   activation, block and lambda call an `Environment` (1.1 GB). A slot-indexed frame (each
   parameter and local given an index at elaboration) would remove the search and most of these.
2. Interface dispatch still asks `TypeOf` and two dictionary lookups per argument per call
   (`TypeId`, then the shape memo); caching the last shape at each `TirCall` site would answer
   most calls with one reference comparison.
3. `InvokeBody` pays a `CallMemo.StatsOf` dictionary lookup and two `Stopwatch.GetTimestamp`
   calls per call, and `Activate` reads `DateTime.UtcNow` for the budget per call.
4. Without the memo a whole-model case still takes minutes: the work is repeated on equal inputs
   in the sandbox library (constants written as loops, `IsWellFormed` per sample point; see
   `docs/reports/interpreter-speed-2026-10-18.md`), which the memo hides and the C# backend pays.
5. The memo decides by measured time, so two runs of one law do different work (allocation varied
   6.5 to 10.9 GB over three runs of the same law); benchmarks need `PLATO_NOMEMO=1`.
