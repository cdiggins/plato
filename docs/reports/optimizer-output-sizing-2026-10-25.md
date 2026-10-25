# Fixed-output FlatMap experiment — 2026-10-25

For [compiler-509](../../tracker/issues/compiler-509.md), an isolated C# executable tested
replacing a growing output List with an exact array. The baseline method is extracted
verbatim from `src/Plato.Intrinsics/ArrayIntrinsics.cs`; the candidate is handwritten C#,
not code emitted by a modified compiler. Both retain per-callback temporary arrays.
The payload is `long`, not Plato's Number wrapper, so wrapper codegen is unverified.

Reproduce from the repository root:

```powershell
python tools/experiments/optimizer-exploration/output-sizing/run.py
```

The runner builds a private net8.0 project, imports the repository build-error recorder
with a private snapshot, and saves `build.log`, `build-status.json`, `fingerprints.txt`
and `results.txt` under `.temp/optimizer-exploration/output-sizing/`. No shared project
references, runtime sources or status report files are modified.

## Checks actually run

The final build passed with no warnings/errors in 4.70 seconds. The executable passed
equality/order/duplicate checks for empty and nonempty inputs and multiple widths; callback
and literal-element fault ordering; zero-width callback faults; overflow refusal; and an
oversized synthetic source whose first callback fault wins instead of an introduced
overflow exception. A deliberately wrong width produced an extra default element,
demonstrating why arbitrary callbacks must be refused rather than guessed.

Runtime was .NET 8.0.30, X64, reporting 22 logical processors. Tiered compilation was
disabled for the final paired run to avoid transitions during measurement. Each size had
150 warmup invocations of each version, followed by five alternating-order rounds with
consumed results. Repetitions per round were `max(5, 262144 / inputCount)`. Input and
delegates were created outside the measurement. Thread allocation counters include result
and callback arrays; stopwatch construction is outside those counters.

| Input count; callback width 3 | Grow List bytes/call | Exact array bytes/call | Median milliseconds/call, List / exact |
| --- | ---: | ---: | ---: |
| 16 | 1,976 | 1,240 | 0.000722 / 0.000591 |
| 4,096 | 459,128 | 295,000 | 0.306256 / 0.254777 |
| 65,536 | about 7,340,524 | about 4,718,688 | 5.686040 / 3.805160 |

Allocation decreased about 36% for the larger inputs without eliminating any callback
arrays. Timing is noisy: at the largest size the exact path was slower in two rounds,
and concurrent agents shared the machine. These are exploratory microbenchmark results,
not evidence of a production or whole-geometry speedup. Small allocation-counter variation
at the largest size is included rather than treated as an exact object-layout assertion.

## Review and limitations

Exact capacity is promising because its allocation mechanism is independent of existing
conditional-singleton optimization. It requires a static width proof, safe source Count,
and an overflow fallback. The prototype deliberately exposes width as a trusted parameter
to model that proof; it is not a safe public API for arbitrary callbacks. Production must
also retain scalar wrappers and conversions and establish recognized-operation provenance.
Unknown-size filtering, two-pass counting, dynamic sources and fused chains are excluded.

An earlier run with default tiered compilation showed unstable small-input timing; the
reported run disables tiering. Neither run validates a live compiler rewrite. Real TIR
shape coverage, wrapper-generated execution, a reserved-List alternative, documentation
freshness and repository-wide gates were not run in this isolated track. The
[execution plan](../design/optimizer-exploration/output-sizing.md) separates these next steps.

Evidence base HEAD during final recording: `340975d68f3a2b8f74e60c1a1ec4b08b19838c9e`.
Actual runtime source SHA-256:
`48febe8abeb7c2b16179876311585dd462059ae2b44557932ef96ac0373a1a2d`.
Experiment source SHA-256:
`fe738ba7c05de4517ff3894dcde513ce487fd2295c2132af0ddbe22be86df90e`.
Runner SHA-256:
`fbf32bcf9ffebae7e79257c10ab99a0a630b9e6cbaad09a5da415280e3ad69c7`.

## Production follow-up: real TIR and bounded implementation

After the user authorized production work, `probe.py` built a private harness against
the existing CLI assemblies and compiled the actual foundation plus a Plato fixture.
Its recorded build passed with no errors/warnings in 8.42 seconds. Both parameter-source
and literal-source `FlatMap(x => [value(x), value(x)])` calls retained canonical
`Intrinsics.FlatMap` identity, a raw `TirLambda`/`TirArray` callback, exact `Number`
element/result types, and no fused source stages. Capture rewriting bound the captured
delegates before the loop without obscuring the raw array. A varying-width conditional
remained `TirConditional` and is outside the matcher. Evidence and assembly fingerprints
are under `.temp/optimizer-exploration/output-sizing/tir-probe/`; rerun with:

```powershell
python tools/experiments/optimizer-exploration/output-sizing/probe.py
```

The production matcher and emitter are now written under the supervisor's P2 ownership
contract. The runtime array guard establishes stable source length even when callbacks
mutate its values; non-array sources preserve the existing cached-count traversal.
The new flag is default-off. Generated-name reservation includes unused lambda parameters
and inline declarations. Callback node count is bounded to limit code duplicated between
the exact and fallback branches.

The supervisor's coordinated Release build and focused run verified the production tests.
All 17 `ExactOutputTests` cases passed. They cover actual wrapper code with flags off/on,
both new flags enabled, loop lowering disabled, captured provenance, literal fault order,
null faults, source mutation, custom-list Count/index traces, zero-width array output,
identity/type/coercion/size refusals, name collisions and warmed allocation comparison.

| Recorded gate | Result | Seconds |
| --- | --- | ---: |
| C# writer build | Pass | 4.46 |
| Test assembly build | Pass | 5.83 |
| Exact-output cases in focused combined run | All passed | Combined run: 27.16 |

The combined run also included multi-consumer tests with separate harness failures;
its duration is not an isolated exact-output test timing and this report does not mark
that entire run green. The supervisor also verified successful CLI generation with both
new options enabled, rejection when required loop lowering is absent, and rejection for
non-C# output.

`AllocationDropsOnRealGeneratedWrapperCode` used 4,096 `Number` wrappers and a literal
callback width of two, retained both per-callback arrays, and compiled off/on variants
with Roslyn Release optimization and the default language version. It warmed each variant
32 times, then measured eight invocations using thread-local allocation counters, checking
each output's length and final value. Input and callback construction were outside the
measured region. The generated baseline allocated **295,200 bytes/call**, compared with
**229,400 bytes/call** with exact allocation: **22.29% less allocation**. This replaces the
earlier handwritten long-payload prototype as the evidence for the implemented pass;
different callback width and runtime types make the two percentages incomparable.

No production execution-time speedup was measured. The huge custom-list test validates
fallback fault order; it does not allocate a real array beyond `Array.MaxLength`. Profile
gpu32, realistic geometry timings and the reserved-List alternative are not measured here.
Repository-wide gates and publication remain the supervisor's integration responsibility.

## Supervisor review

The overflow-fallback check initially lacked an assertion that its expected callback fault actually occurred. The supervisor added that assertion and reran the recorded build and full executable successfully. Allocation results remained consistent; timing remained noisy. The source fingerprint above describes the agent measurement before this test-only correction. No candidate kernel behavior changed.
