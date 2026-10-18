# Interpreter speed on whole-model laws, and defect D5, 2026-10-18

A dated report for TKT-332 in the bim-open-toolkit tracker (the Plato interpreter runs the STEP
mesher's whole-model laws in seconds, and defect D5 is fixed). It records where one hollow-box
law case spent its time, what was changed in `src/Plato.Interpreter`, and the time and verdict of
each of the ten laws that hit the 60 s budget in the sweep of
[`interpreter-turnaround-2026-10-17.md`](interpreter-turnaround-2026-10-17.md), section "Rerun on
2026-10-18". The design is in [`../interpreter.md`](../interpreter.md) and
`src/Plato.Interpreter/README.md`; nothing here is current after the day in the title.

## Setup

- Same machine as the turnaround report. **It was heavily shared**: about 40 `dotnet` processes
  from other sessions were running during the afternoon. The load is visible in the compile time
  `plato run` prints for the same 250 files: 13 to 22 s at the start of the session, 180 to 330 s
  by its end. Each figure below gives the compile time of its run, so the reader can scale it.
- Plato at 062b8dc7 for the "before" figures, and at 8cbf8894 (the D5 fix) plus the memo
  described below for the "after" figures; the memo landed as 93907bd7, the profiler as
  ad14b152.
  bim-open-toolkit at c28772b4. The CLI was built with
  `dotnet build src/Plato.CLI/Plato.CLI.csproj -c Release --artifacts-path <scratchpad>/plato-build`,
  never into `src/*/bin`.
- The sandbox command is the one in the turnaround report's Setup, with `--budget=60` and
  `--filter=` naming the ten laws. Single cases were run by a scratch driver outside the
  repository that compiles the same files, calls one law on given arguments through
  `Interpreter.Invoke`, and prints `CallProfile.Report` (below).

## Where one case spent its time

`Law_HollowBoxOuterMeshVolumePositive(a = 2, b = 1)` (layer 8, `pipeline-witnesses.laws.plato`)
had not returned after 390 s, and was stopped. Run again with a 90 s deadline and the new
per-function profiler attached, it was still inside the model's own validation
(`ValidateBrep`, the first of the three reports `Validated` merges) when the deadline fired; the
mesher proper had not started. The top of the profile, by self time (time in the function's own
body, not in the bodied functions it calls):

| Function | Self | Inclusive | Calls |
|---|---|---|---|
| `BrepFoundation.SmallestNormal` | 19.8 s | 28.1 s | 6,120 |
| `Core.NotEquals` | 12.4 s | 12.5 s | 10,663,116 |
| `Core.LessThan` | 8.3 s | 12.7 s | 3,683,011 |
| `Collections.IsValidIndex` | 6.8 s | 12.7 s | 1,340,068 |
| `Primitives.All` | 4.3 s | 31.5 s | 332,394 |
| `BrepGeometry.ReferenceFuel` | 4.0 s | 4.0 s | 1,563,048 |
| `BrepGeometry.CurveDepth` | 1.6 s | 8.4 s | 521,016 |
| `BrepGeometry.IsAcyclicFrom` (curves) | 1.4 s | 14.2 s | 521,016 |
| `BrepFoundation.RoundingUnit` | 1.3 s | 2.1 s | 13,080 |
| `BrepGeometry.IsWellFormed` | 0.4 s | 26.3 s | 21,708 |

The deadline fired in `SmallestNormal < ResolutionAt < KeepSeedWhereFree < ProjectNear <
Law_ProjectNearRecoversParameter < CheckGrid < CheckSurfaceSamples < ... < GeometryArenaReport <
ValidateBrep < Validated < RunOutcome < MeshModel < WitnessMesh`. Three causes, all of the same
kind, work repeated on equal inputs:

1. **Constants written as loops.** `SmallestNormal` and `RoundingUnit`
   (`00-foundation/foundation.library.plato`) find 2^-1022 and the unit roundoff by halving in a
   `while` loop, about 1,000 and 53 steps, each step several bodied calls (`NotEquals` is
   `!a.Equals(b)` through interface dispatch). Every tolerance check reaches them through
   `ResolutionAt`, so one case evaluated the same two constants 19,200 times: 30 s of the 90.
2. **Well-formedness recomputed per sample.** `CheckGrid` asks `CanEvaluate` at every sample
   point, `CanEvaluate` asks `IsWellFormed` of the whole `GeometrySet`, and `IsWellFormed` walks
   the reference chain from every curve and surface (`IsAcyclicFrom`, `CurveDepth`, with
   `ReferenceFuel` recounted at each step). The set does not change between samples: 26 s of the
   90.
3. **The same model meshed several times per law.** `Law_HollowBox*` write
   `BrepModel.Witness_HollowBox(a, b).WitnessMesh` three times in one expression, and the planted
   bad face laws write `Witness_BoxWithPolyLoopSheet.WitnessOutcome` two to five times; each is
   a full `MeshModel`.

What remains of the cost is the interpreter's own per-call price: about 1.2 microseconds for a
call as small as `NotEquals` (boxing, an environment, run-time dispatch).

## What changed

**Defect D5, 8cbf8894.** `new Buffer<T>(n)` was filled with null where the C# runtime's
`new T[n]` holds `default(T)`. `ReferencedVertexCount` (layer 7) marks cited positions in a
`Buffer<Boolean>` and maps `b ? 1 : 0` over it, so any position no element cites faulted with "a
conditional's condition is not a Boolean: null". The unbudgeted sweep reached that at case
`a = 5.785732621227266, b = 0.5`, whose mesh has such a position. Now each slot holds the element
type's default. `UnitTests.Buffer_slots_never_set_hold_the_default` reproduced the message before
the fix and passes after it; the law at that case now returns true in 4.1 s.

**A memo of expensive calls, 93907bd7** (`CallMemo.cs`). Plato is pure, so a bodied
function applied to equal arguments gives an equal value, and the interpreter answers a repeated
call from a table. A function is remembered once its mean cost per call, with elaboration time
left out, reaches 20 microseconds (`CallMemo.ThresholdMicroseconds`); below that, hashing the
arguments would cost more than it saves. The keys are stricter than Plato's equality: a Number
compares bit for bit (0.0 and -0.0 are two keys, because `1/x` tells them apart), and a record,
sum or array compares its run-time type too (an empty `Array<Integer>` is not an empty
`Array<Number>`, because dispatch inside the call can see the difference). A record, sum or
array caches its hash in itself, so a model passed to a thousand calls is hashed once. A List or
Buffer builder, the one mutable value, is never a key and never a remembered result; a call that
faults is not remembered; each function's table is emptied at 4,096 entries. `Interpreter.Memo =
null` turns it off. All three causes above are absorbed: the constants are computed once per
process, `IsWellFormed` once per geometry set, and each witness meshed once per process, shared
by every law that names it.

**A per-function profiler, ad14b152** (`CallProfile.cs`, `Interpreter.Profiler`): calls, inclusive time
(counted once per outermost activation) and self time per bodied function, collected only when
attached. It produced the table above. `plato run` does not expose it yet; the CLI belongs to
TKT-331 today.

## The ten laws, before and after

Before: the sweep of 2026-10-18 (Plato a966dfc4 with the resolution memo, compile 13.1 s), every
law stopped by the 60 s budget with no verdict. After: one `plato run` with `--filter=` naming
the ten, Plato 8cbf8894 plus the memo, compile 22.0 s; the ten laws share one interpreter, as in
a sweep, so a witness meshed by one law is not meshed again by the next.

| Law | Before | After: verdict | After: time | Mean per case |
|---|---|---|---|---|
| `BrepWitnessesLaws.Law_WitnessIsOracleClean` | budget, 2 cases | fail at case 7, `k = 5` | 6.9 s, 7 cases | 1.0 s |
| `StepToMeshWitnessesLaws.Law_HollowBoxOuterMeshVolumePositive` | budget, 18 cases | pass | 3.8 s, 25 cases | 0.15 s |
| `StepToMeshWitnessesLaws.Law_HollowBoxMeshVolumeIsDifference` | budget, 10 cases | pass | 12.9 s, 25 cases | 0.52 s |
| `StepToMeshWitnessesLaws.Law_PlantedBadFaceIsDiagnosed` | budget, 1 case | pass | 1.5 s, 1 case | 1.5 s |
| `StepToMeshWitnessesLaws.Law_PlantedBadFaceErrorsStayInTheSheet` | budget, 1 case | pass | under 1 s, 14 cases, exhaustive | under 0.1 s |
| `StepToMeshWitnessesLaws.Law_PlantedBadFaceHasTwoErrors` | budget, 1 case | pass | under 1 s, 1 case | under 1 s |
| `StepToMeshWitnessesLaws.Law_WitnessEdgeReversalPreservesMesh` | budget, 16 cases | obligation at case 44 | 35.8 s, 44 cases | 0.81 s |
| `StepToMeshLaws.Law_EveryDiagnosticIsLocated` | budget, 1 case | obligation at case 8 | 6.1 s, 8 cases | 0.76 s |
| `StepToMeshLaws.Law_PowerOfTwoScalingScalesPositions` | budget, 2 cases | pass | 11.6 s, 25 cases | 0.46 s |
| `StepToMeshLaws.Law_ValidationIsIdempotent` | budget, 1 case | error at case 2 | under 1 s, 2 cases | under 0.5 s |

The ten together: 600 s and no verdict before, 78.6 s and ten verdicts after. Every mean is
under 5 s per case. The first case of each law also pays for elaborating the bodies it reaches
(1,674 bodies in 1.8 s over the ten) and fills the memo. Measured alone in a fresh process,
`Law_HollowBoxOuterMeshVolumePositive` took 4.1 s at the D5 case (compile 16.2 s) and 5.8 s at
`a = 2, b = 1` with the profiler attached, which adds its own cost; asked a second time in the same
process, the D5 case took under 0.01 s.

The four verdicts that are not passes are the library's, not the interpreter's:

- **`Law_WitnessIsOracleClean` fails at witness 5.** `ValidateBrep` at the Oracle level reports
  one Error, `Law_DerivativeMatchesCentralDifference` on `KernelCurve(CurveIndex(0))` ("Derivative
  matches central difference does not hold."), found by evaluating
  `BrepModel.WitnessAt(5).ValidateBrep(ConformanceLevel.Oracle).Diagnostics` with `--call`. The
  same case without the memo did not finish in 1,500 s on the loaded machine, so the two were not
  compared directly on it; see the comparison below.
- **`Law_WitnessEdgeReversalPreservesMesh` and `Law_EveryDiagnosticIsLocated` reach a bodiless
  function**, `ExpandedPoles(BrepModel, BrepFaceIndex, Array, FaceMeshingCriteria)` under
  `CompletePeriodicRegion` (layer 5): a genuine host obligation, the periodic-face work of a later
  mesher step.
- **`Law_ValidationIsIdempotent` errors on generated input**: a `ModelMesh` with negative vertex
  indices reaches `At(Array, Integer)` out of range in `CornerNormalAt < RestrictedToFaces`, an
  unguarded precondition of the kind the turnaround report's 13 `At` errors are.

## Regression checks

- The whole interpreter test suite (`tests/Plato.Interpreter.Tests`, 64 tests with the memo on)
  passes. Unit tests: 35 before the memo, 41 with the memo tests (`CallMemoTests`: a repeated
  call is answered from the memo, remembered answers equal evaluated ones, 0.0 and -0.0 are two
  keys, builders are never keys or results, element types keep arrays apart, equal records are
  one key); all pass.
- The stdlib law run (`run stdlib/foundation stdlib/geometry stdlib/graphics stdlib/tests --laws`,
  cpu64, memo on): 148 laws, 137 pass, 8 fail, 3 error, the same laws as on 2026-10-18. Its law
  time was 21.6 s against 1.4 s that morning, with a compile of 275.7 s against 16.5 s: the same
  factor of about 16, the machine's load, not the memo.
- `StdlibLawsTests` (the same laws inside the test suite): pass.
- `DifferentialTests` (the stdlib's laws through the interpreter and through the C# backend
  compiled in-process, cpu64 and gpu32): both pass. They had failed to start from a private
  `--artifacts-path` because `DifferentialProgram` looked for the repository above the test
  output folder; it now uses `TestSupport.RepoRoot`, which also walks up from its source file.
- The memo against no memo, over every other sandbox law (733 with the ten above left out; the
  sandbox had grown to 251 files), by a scratch driver holding two interpreters over one
  compilation and running each law in both, one after the other, under the 60 s budget: 720 laws
  give the same status and the same first failing case, **none differ**, and 13 hit the budget
  only without the memo, so they were not compared:

  ```
  733 laws: 720 same, 0 differ, 13 budget on one side only; laws 333.5s with the memo, 3056.0s without
  ```

  The 13, with the memo's verdict and time: `BrepWitnessesLaws.Law_WitnessIsStructurallyClean`
  (obligation, 43.1 s), `Law_WitnessValidatesClean` (obligation, 3.4 s), `Law_BoxValidatesClean`
  (pass, 4.2 s), `Law_CylinderValidatesClean` (pass, 4.1 s), `Law_WitnessGreenAreaMatchesFacts`
  (fail, 3.6 s), `Law_WitnessDivergenceVolumeMatchesFacts` (obligation, 15.8 s),
  `Law_SharedCornerFailsExactlyTwoLaws` (pass, 1.7 s), `BrepCheckingLaws.Law_FaceAreaMatchesOracle`
  (fail, 2.1 s), `Law_OuterShellExactVolumePositive` (pass), `Law_PlanarFaceAreaMatchesGreen`
  (pass, 1.3 s), `Law_GreenEqualsSphereArea` (pass, 8.3 s),
  `StepToMeshWitnessesLaws.Law_PlantedBadFaceLeavesOthersUnchanged` (pass) and
  `StepToMeshLaws.Law_EdgeReversalPreservesMesh` (pass, 31.6 s). Both sides ran under the same
  load, so the ratio of the two law times, about 9 to 1, is the memo's effect on a sweep.

## What is left

- The per-call price of the interpreter itself (about 1.2 microseconds for a two-argument
  comparison) is unchanged; without the memo a whole-model case still takes minutes.
- The memo's memory over a full 734-law sweep was not measured; each function's table is bounded
  at 4,096 entries, and a remembered mesh keeps its model alive for the process.
- Cold per-law times on an idle machine were not taken; the figures above are from a shared one.
- Library findings for the supervisor (the library is outside this ticket's fence): the two
  constants written as loops, `IsWellFormed` asked per sample point by `CheckGrid`, the laws that
  mesh one model several times in one expression, and the four verdicts above. The C# backend has
  no memo, so causes 1 to 3 cost it too, at native speed.

## Later the same day: top-level Number parameters

The supervisor asked about the "Limits" entry of `../interpreter.md`, which says a law parameter
typed `Number`, `String`, `Character` or `Array<T>` gets a one-value domain (an empty
`Number()` record) and errors. That defect was fixed on 2026-10-17 in 1a3298fa
(`ValueGenerator.FiniteDomain` skips `primitive` declarations), which the pinned CLI 062b8dc7
already contains: `StepToMeshLaws.Law_TighterChordalStaysWithinLooserBound(m, o, chordal: Number)`
passes 25 cases under both 062b8dc7 and 93907bd7. The entry in `../interpreter.md` is stale; that
file is TKT-331's fence today.

No test guarded the fix, so `ValueDomainTests` now runs five laws over top-level `Number`,
`String`, `Character`, `Array<Number>` and `Number` beside `Integer` parameters, each sending its
value through a host intrinsic. Writing it found one more gap: `s.Count` on a `String` was a host
obligation (`ICountable.Count on String`). `String` declares `ICountable`, and the C# runtime
gives the `String` struct `Count` and `At` as members with no Plato declaration behind them; the
interpreter's host table had both entries, but interface dispatch never looked there. A member of
an interface that nothing implements on a primitive receiver now falls back to the host entry
keyed by the arguments' run-time types. The interpreter's 66 tests pass, and the stdlib law run
keeps 137 pass, 8 fail, 3 error (compile 10.0 s, laws 2.0 s, the machine quieter by then).
