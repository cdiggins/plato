# Interpreter turnaround and first law results, 2026-10-17

A dated report for TKT-317 (the Plato interpreter) in the bim-open-toolkit tracker. It records,
on one machine and one day, how long the interpreter path takes from an edited function body to
law results, how long the generate-build-run path takes for the same program, and what the law
runner reported over the STEP design folder and the stdlib. The design the numbers describe is in
[`../interpreter.md`](../interpreter.md); nothing here is current after the day in the title.

## Setup

- Machine: Intel Core Ultra 7 155H, 22 logical processors, 64 GB, Windows 11 Home 10.0.26200,
  .NET SDK 10.0.400 building net8.0. **The machine was shared** with other agent sessions for the
  whole measurement: between nine and twelve other `dotnet` processes were running, and the
  sandbox folder was being committed to (commit 3cf2c3b8, "Chunk A, layer 4", landed between the
  first and second full interpreter runs, changing 25 functions, two verdicts and the body count).
  Ranges below are wide for that reason; the smallest value of each set is the closest to an idle
  machine.
- Plato at commit f256b068 (the interpreter scaffold), `Plato.CLI` built with
  `dotnet build src/Plato.CLI/Plato.CLI.csproj -c Release`.
- bim-open-toolkit at e3e12760 when the runs started; the sandbox is
  `sandbox/step-brep-mesh`, compiled as the three stdlib tiers plus the sandbox recursively,
  minus the three `brep.*.plato` stdlib files it redeclares and minus `*.spec.plato` (skipped by
  the command itself):

```
dotnet src/Plato.CLI/bin/Release/net8.0/Plato.CLI.dll run stdlib/foundation stdlib/geometry stdlib/graphics \
  <toolkit>/sandbox/step-brep-mesh --laws --recursive \
  --exclude=brep.types.plato --exclude=brep.library.plato --exclude=brep.concepts.plato --profile=cpu64
```

  The stdlib run is `run stdlib/foundation stdlib/geometry stdlib/graphics stdlib/tests --laws`.
- The edited body for the turnaround measurement was `ResolutionAt(magnitude: Number): Number` in
  `00-foundation/foundation.library.plato` (`... .Max(Number.SmallestNormal) + 0.0`), reached by
  `Law_LengthTolerancePositive` through `IsWellFormed`. The file was restored byte-exact
  afterwards (`cmp` against a snapshot taken before the edit).
- Wall times are from `date` around the process; the compile and law times are the ones
  `plato run` prints. Starting the CLI to print its usage costs about 0.2 s, which is the gap
  between the printed total and the wall time.

## Turnaround: the interpreter path

Edit the body, then run the laws. No build step exists on this path; "compile" is the front end
(parse, bind, resolve) for 250 files and about 14,300 functions, before any body is elaborated.

| Run | Compile | Laws | Printed total | Bodies elaborated |
|---|---|---|---|---|
| filtered, `--filter=LengthTolerancePositive`, run 1 | 20.8 s | 0.7 s | 21.5 s | 10 |
| filtered, run 2 | 23.2 s | 0.6 s | 23.8 s | 10 |
| filtered, run 3 | 15.8 s | 0.8 s | 16.6 s | 10 |
| **filtered, median (range)** | **20.8 s (15.8–23.2)** | **0.7 s (0.6–0.8)** | **21.5 s (16.6–23.8)** | |
| full `--laws`, run 1 | 16.3 s | 354.6 s | 370.9 s | 1839 |
| full, run 2 (after commit 3cf2c3b8) | 22.5 s | 749.1 s | 771.6 s | 1891 |
| full, run 3 | 36.3 s | 817.9 s | 854.2 s | 1891 |
| **full, median (range)** | **22.5 s (16.3–36.3)** | **749.1 s (354.6–817.9)** | **771.6 s (370.9–854.2)** | |
| full, classification run, cpu64 | 23.7 s | 452.8 s | 476.5 s | 1838 |
| full, classification run, gpu32 | 16.9 s | 373.8 s | 390.8 s | 1838 |

Reading: one law over one body is a 17 to 24 s round trip, almost all of it the front end over
the whole program; the laws themselves cost under a second. The full sweep of 734 laws is 6 to
14 minutes, and the spread is machine load, not the interpreter (run 1 and the two classification
runs, taken when the machine was quieter, agree within 100 s). `plato run` does not print per-law
times, so which laws dominate is not known; laws over `BrepModel` witnesses that mesh a whole
model are the likely ones.

## Turnaround: the generate-build-run path

The same program through the C# writer. Because `Plato.CLI` enumerates each positional root
top-level only and has no `--exclude`, the inputs were copies in a scratch folder: the three
stdlib tiers minus the three `brep.*.plato` files, and the ten sandbox layer folders minus
`*.spec.plato`. The build used a scratch copy of
`generated/Plato.Generated.Foundation.Unoptimized/Plato.Generated.Foundation.Unoptimized.csproj`
with the `Plato.Intrinsics.projitems` import made absolute and `Ara3DVersion` inlined (the
scratch folder has no `Directory.Build.props`). `generated/` itself was not touched.

```
dotnet Plato.CLI.dll <scratch>/stdlib/foundation <scratch>/stdlib/geometry <scratch>/stdlib/graphics \
  <scratch>/sandbox/00-foundation ... <scratch>/sandbox/09-tooling --out=<scratch>/out-sandbox --csharp-style=extensions
dotnet build Scratch.Generated.csproj -c Release        # in <scratch>/out-sandbox, bin and obj removed first
```

The output folder must exist before the CLI runs; it writes `docs.html` into it without creating
it.

**The sandbox does not compile as C#** (TKT-311's subject). Generation itself succeeds: 1257
`.g.cs` files, 16,802 TIR bodies emitted, one degraded body (`Tween.Change`, from the stdlib's
graphics tier, not the sandbox). The build stops at the C# parser with **160 errors in 7 files**,
all syntax errors (CS1001 ×50, CS1003 ×44, CS1525 ×24, CS1519 ×10, CS1031 ×8, CS1026 ×8,
CS1002 ×8, CS1513 ×6, CS1022 ×2). Three causes are visible from the first lines:

- a Plato parameter named `base` or `short` is emitted as a C# keyword:
  `HammersleyPoints2D(this Bounds2D region, Integer count, Integer base)` in `Sampling.g.cs:33`,
  `RadicalInverse(this int index, Integer base)` in `_Integer.g.cs`, `DecodedEntity(this string
  name, String short, ...)` in `_String.g.cs` and `StepSchemaModel.g.cs:27`;
- a bodiless function is emitted as `=> /*?*/;`: `FaceMeshDump(this IReadOnlyList<FaceMesh>
  meshes) => /*?*/;` in `Extensions.g.cs:2388`, `ToolingRuns() => /*?*/;` in `_LawRun.g.cs`;
- `_EntityDefinition.g.cs` follows from the `short` parameter above.

Because the build fails at the parse stage in about 4 s, the sandbox build time is not a
compile time and is not comparable; the generate-build-run path was therefore measured on
`stdlib/foundation` plus the two law files that resolve against the foundation alone
(`stdlib/tests/foundation.laws.plato`, 29 laws, and `sorting.laws.plato`, 6 laws; the other law
files need the geometry or graphics tier). Same scratch recipe; 284 `.g.cs` files, 0 errors.

| Step | Run 1 | Run 2 | Run 3 | Median (range) |
|---|---|---|---|---|
| generate, sandbox program (then fails to build) | 35.5 s | 28.6 s | 29.6 s | 29.6 s (28.6–35.5) |
| build, sandbox program (stops at 160 syntax errors) | 4.1 s | | | not a compile |
| generate, foundation + its laws | 9.1 s | 17.7 s | 14.4 s | 14.4 s (9.1–17.7) |
| build generated project, clean, foundation + its laws | 12.1 s | 9.4 s | 9.6 s | 9.6 s (9.4–12.1) |
| build the law runner project over the same C# | 15.4 s | | | one run |
| run the laws (`dotnet test --no-build`), wall | 4.8 s | 2.8 s | 3.0 s | 3.0 s (2.8–4.8) |
| the runner's own duration for 37 (type, law) cases | 153 ms | | | |
| interpreter, same program, `plato run --laws` wall | 4.7 s | 4.8 s | 4.3 s | 4.7 s (4.3–4.8) |
| of which compile / laws | 4.0 / 0.4 s | 4.2 / 0.3 s | 3.8 / 0.3 s | |

The law runner on the C# path is `tests/conformance/Plato.ForwardConformanceTests`, whose
`Generated/` folder and project file hold another session's uncommitted work, so it was not built
in place: its four source files and `KnownFailures.json` were copied to a scratch folder with the
intrinsics import made absolute and `Generated/` filled from the foundation generation above. It
reflects over `Law_*` members and ran 37 (type, law) cases, all passing. The documented driver,
`regen-forward-conformance.ps1` in the studio repository, was not used (it passes a retired flag,
as PLATO-PRIMER.md B.2 notes).

Reading: for a program the writer accepts, edit-to-verdict on the C# path is generate + build +
test, 14 + 10 + 3 s, about 27 s median and 21 to 35 s in range, against 4.7 s for the interpreter
on the same program; the gap is the two builds. For the sandbox, the C# path has no verdict at all
today.

## Law results over the sandbox

734 laws found (a `Law_*` function with a body returning `Boolean`), cpu64 and gpu32 identical
law by law:

| Status | Count | Cause classes |
|---|---|---|
| pass | 301 | 110 exhaustive, 191 sampled |
| obligation | 215 | 166 genuine host obligations across 15 bodiless sandbox functions; 49 are the dispatch defect D2 below, reported as `IOriginBased.Offset on Vector3D` |
| skipped | 83 | 73 `GeometrySet: nesting deeper than 6 in Number, and no Witness_* returns GeometrySet`; 8 `BSplineSurfaceData: type variable T, and no Witness_* returns BSplineSurfaceData`; 2 `PipelineState: nesting deeper than 6 in Integer, and no Witness_* returns PipelineState` |
| error | 69 | all 69 are the generator defect D1 below |
| fail | 66 | genuine law failures under the runner's sampling policy, see below |

The 166 genuine obligations, by function (the law count each blocks): `DecodeRaw(Part21File,
DecodePolicy)` 42, `SlotOf(Part21File, Integer, String, String)` 25, `Discretize(BrepModel,
BrepEdgeIndex, DiscretizationCriteria, SegmentFloors)` 25, `EdgeTableOf(ModelMesh)` 13,
`ComponentsOf(ModelMesh)` 12, `Reduced(RawDecodedModel)` 11, `ParseStep(String)` 10,
`ToSplitTriangleMesh3D(ModelMesh)` 9, `Unwrap(BrepModel, BrepFaceIndex, UvLoop)` 7,
`FlatRecord(Array, String)` 5, `InstancePosition(Part21File, StepInstanceId)` 2,
`AssembleModelMesh(BrepModel, Array, MeshVertexNumbering)` 2, `TotalTurning(BrepModel,
BrepEdgeIndex)` 1, `MeanSpeed(BrepModel, BrepFaceIndex, UvRegion, SurfaceParameter)` 1,
`EdgeSeeds(BrepModel, BrepEdgeIndex)` 1. These are the bodies TASKS.md assigns to chunks A to H;
`ParseStep` is the host parser. After commit 3cf2c3b8, `Discretize`, `EdgeSeeds` and
`TotalTurning` had bodies and their laws moved to fail (2) or pass.

The 66 failures split in two. Most are laws over generated values that violate a precondition
the type states and the law does not guard: `Law_LengthTolerancePositive` on
`LengthTolerance(0)`, `Law_BSplineCurveDegreePositive` on a degree of -7, `Law_EdgeTableHasOneCountPerEdge`
on an index of -2, and the like. The runner's policy (deliberately the C# conformance runner's)
draws such values, so these are findings about the laws' guards, not about the bodies, as
PLATO-PRIMER.md B.1 anticipates. A smaller set runs over `BrepModel` witnesses
(`Law_UndiscretizedExactlyWhenUnusable`, `Law_LowestParameterIsDomainStart`,
`Law_NumberingHasOneEdgeRunPerEdge`, `Law_EveryFaceIsMeshed`, and others whose first parameter is
`m: BrepModel`); those are the laws chunk A is working through, and this report does not judge
them. The full outputs are not kept; the command above reproduces them.

## Law results over the stdlib

148 laws, cpu64 and gpu32 identical: 43 pass, 3 fail, 84 error, 18 obligation. Of the 84 errors,
81 are D1 and 3 are `At(Array, Integer): Index was out of range` in
`TriangulationLaws.Law_HoledTriangulation*` on a generated polygon with one point, a genuine
fault on garbage input of the kind the law file's preamble warns about. All 18 obligations are
D2 (`IOriginBased.Offset on Vector2D` 11, `on Vector3D` 5, `on Duration` 1, `IVector.Broadcast
on Duration` 1). The 3 failures (`Law_BoxIsBoxSlice` with negative half extents, the two
hyperboloid laws with a reversed interval) are unguarded preconditions.

The same laws through the C# runner settle which engine is wrong: over foundation plus its two
law files the C# runner passes all 37 cases, including `Law_AddBetweenAreInverse2D`,
`Law_Bounds2DContainsCenter`, `Law_OriginIsEpoch`, `Law_ParameterOfInvertsAt` and
`Law_RemapToSelfIsIdentity`, which the interpreter reports as obligation (D2) or error (D1).

## Interpreter defects found

Grouped by root cause; the messages are the ones `plato run` prints. Both appear under both
profiles.

**D1. A law parameter of a primitive type other than `Boolean` or `Integer` gets an empty record
as its whole domain.** `ValueGenerator.FiniteDomain` returns a single zero-field `PlatoRecord`
for any concrete, non-sum `TypeDef` with no fields, and the primitives `Number`, `String`,
`Character`, `Array<T>` and `FunctionN<...>` match that rule before `CanGenerate` is consulted.
The witness column shows it: `extent = Number()`, `xs = Array()`, `holds = Function1()`,
`law = String()`. Every error line in both runs carries one. The value reaches the first host
intrinsic and faults with a cast. Messages, with one example law each and the count over the
sandbox (cpu64) plus the stdlib:

| Message | Example law | Sandbox | Stdlib |
|---|---|---|---|
| `IsNaN(Number): Unable to cast object of type 'Ara3D.Geometry.Interpreter.PlatoRecord' to type 'System.Double'.` | `BrepFoundationLaws.Law_LengthToleranceResolvable` (in `IsFinite < Law_...`) | 8 | 23 |
| `LessThanOrEquals(Number, Number): Unable to cast ... 'PlatoRecord' to type 'System.Double'.` | `BrepTopologyLaws.Law_ScalingScalesExtent` (in `LessThan < GreaterThan < Law_...`) | 29 | 20 |
| `Subtract(Number, Number): Unable to cast ... to 'System.Double'.` | `BrepFoundationLaws.Law_ShortestDifferenceAtMostHalfPeriod` | 5 | 15 |
| `Multiply(Number, Number): Unable to cast ... to 'System.Double'.` | `BrepFoundationLaws.Law_ParameterToleranceAtMovesWithinLength` | 1 | 3 |
| `Add(Number, Number)`, `Negative(Number)`: same cast | stdlib only | 0 | 6 |
| `Count(Array): Unable to cast ... 'PlatoRecord' to type 'Ara3D.Geometry.Interpreter.PlatoArray'.` | `BrepFoundationLaws.Law_IntegerTotalAddsOverConcatenation` (in `Concatenate < Law_...`) | 12 | 13 |
| `Reduce(Array, $U, Function2): Unable to cast ... to 'PlatoArray'.` | `EdgeDiscretizingLaws.Law_RefineKeepsUnrequestedEdges` (in `Any < IsRequested < All < IsRefinable < Law_...`) | 8 | 0 |
| `invoking a non-function value Function1()` / `Function2()` | `BrepCheckingLaws.Law_CheckAllHoldsExactlyWhenEverySampleHolds` (in `CheckAll < Law_...`) | 6 | 0 |
| `no field 'X' on Point2D` (the `t = Number()` is unused; the fault is D3) | `SpecialNumericsLaws.Law_SuperEllipseUnitCirclePerimeter` | 0 | 1 |

Total: 69 sandbox laws and 81 stdlib laws have no verdict until this is fixed. Nested
occurrences of the same types, as fields of a generated record, are generated correctly.

**D2. A static callee chosen for a type-variable-typed expression is applied without
re-resolution, and the failure is reported as a host obligation.** `Add(x: IOriginBased<$D>,
delta: $D) => x.FromOffset(x.Offset + delta)` in `stdlib/foundation/algebra.library.plato`: the
checker resolves the inner `x.Offset + delta`, whose operand types are the type variable `$D`,
to that same generic `Add`. The interpreter applies the resolved callee (a library function, not
an interface declaration, so it is not re-dispatched) with a `Vector3D` receiver, and the
`x.Offset` inside it then has no implementer, so the runner prints
`host obligation: IOriginBased.Offset on Vector3D (in Add < Add < Lerp < ...)`. Nothing is
bodiless; the C# runner passes these laws. Counts: sandbox 49 (every law whose body reaches
`Lerp`, `MidPoint`, `Center`, `FramePoint`, `BoxCorners`, or `Point3D + Vector3D` on a witness,
for example `BrepWitnessesLaws.Law_UnitBoxIsBoxOfOnes`, `BrepCheckingLaws.Law_WitnessValidatesClean`,
`StepToMeshWitnessesLaws.Law_WitnessHasStatedCounts`); stdlib 18 (`FoundationLaws.Law_AddBetweenAreInverse2D`,
`Law_Bounds2DContainsCenter`, `Law_LerpAtZeroIsStart2D`, `Law_OriginIsEpoch` via `IVector.Broadcast
on Duration` in `Zero < Origin`, and others). The 49 sandbox cases are the ones that matter most
for chunk A: every witness-based law over a box or a mesh goes through `Lerp` or `BoxCorners`.

**D3. `Type.Name` in value position passes the type token to a value parameter.**
`Point2D.Origin` with `Origin(x: IOriginBased<$D>): IOriginBased<$D>` (the receiver is `x`, not
`_`): `ResolveBareName` hands a `TypeToken` to `x`, and `Offset(p: Point2D)` faults with
`no field 'X' on Point2D (in Offset < Origin < Law_SuperEllipseUnitCirclePerimeter)`. One stdlib
law; the sandbox does not use the form.

## Not measured, and why

- Per-law times: `plato run` prints only the totals. The slow laws in the 6-to-14-minute sweep
  were not identified.
- The sandbox through the C# path: it does not compile (160 syntax errors above), so there is no
  C# build time, no C# law time, and no differential comparison for the sandbox.
- The full stdlib through the C# path: the three tiers plus all of `stdlib/tests` were not
  generated here; the foundation tier with its two law files stands in, as the task allowed.
- An idle machine: see Setup. Every figure above was taken while other sessions were building
  and committing.
