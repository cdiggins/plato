# Optimizer handbook validation — 2026-10-24

This is a dated measurement record for the [optimizer handbook](../optimizer/README.md), not
a general performance promise. The compiler algorithms were not changed by this documentation
mission. The shared checkout's source fingerprints are recorded in
[Greenhouse evidence](../../tools/optimizer-docs/evidence.json).

## Focused optimizer tests

The command in [evaluation](../optimizer/evaluation.md#repeatable-commands) ran against Release
net8.0 and passed all 67 selected tests, with a reported test duration of 61 seconds. The retained
TRX is `.temp/optimizer-docs/tests/optimizer-handbook.trx`; it contains the emitted-code output and
the allocation experiment below. This local artifact is not a durable checked-in test baseline.

The tests cover inliner TIR shapes, fusion and literal expansion, component expansion,
materializer renames, array loop lowering, flat-map execution/allocation and shared rewrites.
This selection does not establish whole-program equivalence for every optimizer combination.

## Controlled flat-map allocation comparison

`FlatMapFilterTests.SingletonAllocationIsRemovedAgainstTheSameLoweredPath` compares the eligible
singleton-arm emitter with the ordinary copying emitter. Its source is
[FlatMapFilterTests.cs](../../tests/PlatoTests/Optimization/FlatMapFilterTests.cs).

The test reported cpu64 wrapper scalars, net8, C# 12, Release, `loops=true`, `fuse=true`,
`inline=false`, and a materialized `Number[]` source. A same-type TIR callback wrapper forces
the ordinary emitter and emits no conversion. Setup, delegate creation, reflection and assertions
are outside measurement. It warmed each version for 10,000 calls, measured 1,000 calls, and
consumed the result count and last value.

| Input size | Specialized bytes/call | Ordinary bytes/call | Saved bytes/call |
|---:|---:|---:|---:|
| 8 | 176 | 432 | 256 |
| 64 | 1,144 | 3,192 | 2,048 |
| 512 | 8,384 | 24,768 | 16,384 |

For these inputs the specialization removed the temporary singleton-array allocation while
retaining result-list allocation. This is evidence for that emission rule, not for the speed of
the complete pipeline. No elapsed-time speedup was measured. Rerun the test on the target runtime
before using the numbers for a capacity or performance decision.

## Documentation tooling

Greenhouse's documentation tests passed after fixing duplicate database entries for partial types.
The exporter then successfully built Plato's C# writer documentation database, and the maintenance
tool selected optimizer symbols into the linked source map. The original failure was SQLite
error 19, `UNIQUE constraint failed: doc_types.project_name, doc_types.fully_qualified_name`.
The fix enumerates each Roslyn named-type symbol once, retaining members from all partial parts.

The Python maintenance tests cover body edits, additions/deletions, newline normalization,
missing exports, portable source links and rejection of stale evidence after rendering.
The documentation check passes on the reviewed snapshot. These checks detect drift and broken
local links; they do not prove the explanatory prose is correct.

## Integration battery

The studio `check-all.ps1` battery ran once with `-PlatoRoot` pointing at this checkout.
The retained log is `.temp/optimizer-docs/check-all.log`.

| Gate | Result | Seconds |
|---|---|---:|
| External frozen-V1 artifact tripwire | FAIL | 88.4 |
| Legacy stdlib strict lint | PASS | 30.9 |
| Forward stdlib strict lint | PASS | 30.4 |
| External SDK geometry build | PASS | 6.9 |
| External SDK geometry tests | PASS | 18.3 |

The failed gate checks studio's vendored SDK artifacts. The same category of external drift is
already tracked by [repo-473](../../tracker/issues/repo-473.md), which asks for review of those
artifacts and their manifest in their owning checkout. This mission did not change either.
The aggregate battery suppresses individual gate output, so this run alone does not identify
which artifact hashes differ. No manifest was re-baselined.

The repository's `python tools/record-gates.py` run also completed: the CLI build passed in
12 seconds, shipping-stdlib lint passed in 15 seconds, and all 353 Plato tests passed in
221 seconds. It appended [the gate log](../gate-log.md) and updated the local status snapshot.
Full command outputs are retained under `.temp/gate-logs/20261024-190126-420520/`.
