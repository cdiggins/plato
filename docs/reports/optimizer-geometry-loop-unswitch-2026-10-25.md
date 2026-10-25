# Geometry loop unswitch qualification — 2026-10-25

Work under compiler-510, the invariant-Boolean loop experiment. Production remains unchanged.
The independent experiment command is the only switch; no production flag is justified yet.

## Candidate selection

The spline helper `AtWrapped` in `stdlib/geometry/splines.library.plato` selects wrapped or
clamped boundary indexing from its `closed` Boolean. It is called repeatedly by Hermite
spline evaluation. Its direct conditional is the strongest narrow candidate: after inlining,
the condition may resolve to the exact outer Boolean parameter of a sampling loop.
The experiment extracts this helper unchanged from its source and adds a new MapRange caller
over Number samples. This isolates the boundary selection cost; it is not a measurement of
an unchanged Hermite spline implementation or complete geometry application.

`RingSlot` in `stdlib/geometry/triangulation.library.plato` selects forward or reversed ring
indexing. The actual `LinkRing` caller uses a mutable Buffer and while loop, outside the
initial MapRange-only contract. Its new MapRange caller tests the helper's arithmetic only.
A positive result would not establish that the real mutable caller can be transformed safely.

`GridParameter` in `stdlib/geometry/meshes.library.plato` chooses an open or closed grid
denominator. Its Boolean conditional is nested inside a division, so the direct callback-body
contract refuses it. The surrounding real tessellation also reads surface properties and
invokes surface evaluation. Those operations cannot be silently treated as immutable leaves.

The source already places the closed-direction choices outside the callback in
`AllQuadFaceIndices`; unswitching has no additional work to remove there. Conditions depending
on sampled points, ray intersections or the induction index are intentionally ineligible.

## Bounded executable plan

1. Extract the unchanged helpers, parse them with foundation and the dedicated callers,
   obtain ground TIR, and capture lowered TIR plus emitted C# with inlining off/on. The probe
   rejects unresolved nodes and records direct condition/outer parameter identity separately.
2. Admit only direct MapRange conditional callbacks whose condition is the exact immutable
   outer Boolean parameter. Executable predicates, nested expressions, statement callbacks,
   mutable locals and producer stages remain refused. Do not widen rules to admit the examples.
3. For an admitted candidate, build a private comparison that splits only the emitted fill
   loop after all original count conversion, normalization and allocation statements. Keep
   a positive-count guard and preserve index bindings and each arm verbatim. A syntax rewrite
   requires both the TIR proof and structural assertions; it is experiment machinery, not
   a proposed production text-rewriting architecture.
4. Compare both Boolean values, negative/zero/positive counts, empty/null/custom input lists,
   count/index read traces and first fault. Include an iteration-dependent condition and a
   delegate condition as refusal controls. Verify one result allocation in both variants.
5. Only after correctness, use a supervisor-granted isolated timing window for warmed paired
   alternating runs, tiny and large inputs, consumed outputs, allocation data and emitted size.
   Compare the JIT baseline. No stable material gain means no new production pass or switch.

## Actual TIR result and decision

The focused probe ran against the supervisor's frozen compiler snapshot at
`b612c392b8998ad719f70027eba1ac7c09d91779`. Its private build passed with no warnings or errors
in 7.98 seconds. Parsing, ground-TIR retrieval, unresolved-node rejection, loop lowering and
body emission succeeded for all five callers with inlining disabled and enabled. This is
actual TIR and emission evidence, not execution of the generated geometry methods.

| Caller | Inlining enabled shape | Narrow admission |
| --- | --- | --- |
| SampleBoundary | The callback retains a call to AtWrapped. | Refused: no direct conditional callback. |
| SampleGrid | Divide contains ToNumber over a conditional whose condition is coerced Boolean. | Refused: the conditional is nested under executable operations. |
| SampleRing | Direct conditional with TirCoerce over the outer forward parameter. | Refused: the condition is not the raw primitive Boolean leaf required by the contract. |
| Variant | Direct conditional compares the induction parameter. | Refused: iteration-dependent computation. |
| UnknownCondition | Direct conditional invokes the supplied delegate. | Refused: executable predicate can fault or vary per call. |

With inlining disabled, every geometry helper remains a call. Capture rewriting introduces
Boolean locals for captured outer parameters; the probe records the post-capture trees too.
In particular, the emitted RingSlot conditional looks simple, but its TIR retains a coercion.
The analyzer does not erase that distinction based on generated spelling. Proving transparent
identity coercions would be a separate change, not a reason to relax this experiment's matcher.

Read-only inspection of the frozen inliner clarifies the likely RingSlot coercion origin:
`TirInliner.cs` wraps substituted scalar arguments with their argument type as FromType,
the resolved primitive parameter type as ToType, and a null ConversionFn. The original
argument here is Boolean and the callee parameter is Boolean. This source path therefore
indicates an identity type annotation, rather than an executable semantic conversion.
The current dump records the node and inner types but not those three metadata properties,
so their resolved equality was not directly asserted by the run. A future bounded slice
could assert null ConversionFn and equal canonical Boolean types before looking through
this one wrapper, following the same explicit rule already used in
`TirInliner.Callables.cs`. It should not use unrestricted coercion stripping.

No candidate satisfies the original raw-leaf contract. The separately authorized G3U1 slice
below tests the established identity-wrapper rule. Production remains unchanged and no new
production switch is justified without a runtime benefit.

The executed runner extracted and parsed only `compiler/source/stdlib`, validated the source
and snapshot manifest before and after execution, and saved that manifest alongside hashes
of the live experiment inputs. Every copied dependency matched its frozen original; loaded
snapshot dependencies had the expected copy location and hash. Those checks passed. Evidence
is `results.txt`, per-method emitted bodies, `build.log`, `build-status.json`,
`loaded-assemblies.json` and fingerprint files in the private directory.

Prepare without executing:
`python tools/experiments/optimizer-geometry/loop-unswitch/probe.py`

After a granted build/run window:
`python tools/experiments/optimizer-geometry/loop-unswitch/probe.py --execute`

Private evidence goes to `.temp/optimizer-geometry/loop-unswitch/`; the experiment stores source
and assembly fingerprints and checks they remain unchanged through its run.

## G3U1: explicit identity wrapper proof and private comparison

The focused qualification run directly proved SampleRing's FromType, ToType and inner type
are the canonical primitive Boolean, its converter is null, and its leaf references the exact
outer forward parameter. It also rejected constructed nonidentity and converter-backed
coercions. This uses the explicit existing inliner rule rather than stripping arbitrary casts.

The private experiment rewrites emitted C# with Roslyn only after this TIR proof. It replaces
one fill loop with an `n > 0` guarded branch containing two fills. Removing that replacement
recovers the original syntax exactly; count evaluation, checked narrowing, normalization,
captures and allocation remain in place. Each branch retains the original selected arm.
This is experimental emission, not a production TIR pass or an architecture proposal.

Both generated variants compiled in Release at default language version against the frozen
runtime. The harness supplies two small handwritten ABI adapters, Add and Subtract, which
delegate directly to that runtime's existing Integer wrapper operators. These bridge the
generated method-call spelling across the separate runtime assembly; they are not generated
compiler output or new arithmetic implementations. Both variants use the same adapters.
The loop itself comes from actual Plato emission, with only the enabled fill split performed
by the private Roslyn experiment. They agree with each other and an independent ring-index oracle in both directions
at counts -9, 0, 1, 2, 17 and 4096. Counts just outside either end of the int range throw the
same narrowing OverflowException. Structural checks retained exactly one result allocation.
The subsequent small timing run below measures actual allocation bytes. Generated source
grew from 859 to 1265 UTF-8 bytes including formatting; this is not a JIT code-size measure.

The final private build passed in 5.74 seconds with no errors or warnings. An initial harness
build reported CS0433, "The type 'Boolean' exists in both 'Ara3D.Geometry' and 'GeometryRuntime'"
(also Integer). Giving the legacy dependency a non-global reference alias resolved that
harness ambiguity while preserving copied-dependency validation. Input/loaded-DLL checks
passed. Results are in `results-qualification.txt` and `qualified-*-compile.txt`.

Run focused correctness with `probe.py --execute --qualify`. A separate `--timing` switch
enables the bounded comparison after correctness. Results apply only to this newly authored
MapRange caller of the real RingSlot helper; the actual mutable LinkRing geometry
implementation remains outside the admission contract.

## Small exploratory timing: no production benefit established

One serialized run used .NET 8.0.30 X64 with `DOTNET_TieredCompilation=0`, three alternating
paired batches per case, eight warmup calls per variant, and 262144 output elements per batch.
Outputs were consumed. The focused correctness checks and manifest/dependency checks passed
again; the changed private runner built in 4.31 seconds with no warnings or errors.

| Count/direction | Baseline median µs (range) | Split median µs (range) | Split/baseline | Bytes/call, both |
| --- | --- | --- | --- | --- |
| 4096 reverse | 7.23 (3.77–52.83) | 8.68 (3.52–12.31) | 1.201 | 32792 |
| 4096 forward | 4.28 (4.11–6.18) | 5.89 (3.80–101.32) | 1.376 | 32792 |
| 65536 reverse | 111.40 (107.25–210.08) | 105.58 (105.50–118.60) | 0.948 | 524312 |
| 65536 forward | 96.85 (96.15–166.18) | 97.88 (97.88–99.58) | 1.011 | 524312 |

The small sample has large outliers and provides no consistent speed benefit. Allocations are
identical, while generated source is larger. The isolated apparent improvement for the larger
reverse case does not justify a pass, matcher expansion or production flag. Keep this as a
reproducible private experiment and prioritize better-supported optimizations. There was no
JIT disassembly/code-size investigation or repeated timing campaign, in keeping with the
requested light checks. `timings.json` holds every sample and the consumed checksum.

Reproduce only in a granted timing window with `DOTNET_TieredCompilation=0` and
`python -B tools/experiments/optimizer-geometry/loop-unswitch/probe.py --execute --qualify --timing`.
