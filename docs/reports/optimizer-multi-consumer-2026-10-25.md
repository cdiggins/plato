# Multiple-consumer reduction experiment — 2026-10-25

Tracked by [compiler-506](../../tracker/issues/compiler-506.md). The bounded prototype is executed;
the production pass is not implemented. See the [design and ordered plan](../design/optimizer-exploration/multi-consumer.md).

## Reproduction and evidence

Run `python tools/experiments/optimizer-exploration/multi-consumer/run.py` from the repository root.
The standalone project imports the live `Plato.Intrinsics` shared source, references
`Ara3D.SDK.Core`, and excludes the package's `Ara3D.Geometry` compile assets, following the
existing intrinsic test project. It has no compiler project references and does not regenerate
shared outputs. The runner captures the full build log, then uses the repository build-error
recorder with its snapshot redirected to this experiment's private evidence directory.
This explicitly substitutes for the PowerShell wrapper to preserve another session's reports.
The recorder's direct-build path raises `NameError` for missing `run_build`; [repo-512](../../tracker/issues/repo-512.md)
tracks that separately. The working `--log` path is used here.

Private evidence: `.temp/optimizer-exploration/multi-consumer/build.log`, `build-snapshot.json`,
`results.txt`, and `fingerprints.json`. Re-running replaces those files. Source fingerprints
below identify the reported run despite concurrent commits:

- `Kernels.cs`: `8C148062EB8332B03E17D0E6412CE86AEA4F3A323BD9591500B454B13797F2AA`
- `src/Plato.Intrinsics/Number.cs`: `C0C8F0B6576DF39C2162389914B151D03736EB2F70ED574BAB879DACF91E8E81`
- `src/Plato.Intrinsics/ArrayIntrinsics.cs`: `48FEBE8ABEB7C2B16179876311585DD462059AE2B44557932EF96AC0373A1A2D`

Environment: .NET 8.0.30, Windows 10.0.26200, X64; Release/net8.0 default language version,
cpu64 Number profile. `DOTNET_TieredCompilation=0` makes this a fully optimized non-tiered JIT
experiment; it does not characterize default tiered-PGO deployment. No CPU isolation was used,
and other exploration agents were active.

## Correctness and performance

The real runtime's sequential `Reduce`, handwritten separate lowered loops, and handwritten
combined loop agreed bitwise on both sum and sum of squares for 209 deterministic cases.
These include empty/singleton inputs, cancellation, duplicates, signed zero, infinities,
NaN, subnormals, overflow and randomized finite arrays. The candidate preserves each accumulator's
update order. This is generated-shape validation, not a test of an actual TIR transformation.

Three executable counterexamples also passed: interleaving callbacks changes the first observed
fault; a stateful host `IReadOnlyList` changes values when repeated reads are removed; reassociation
changes a floating result. These establish required refusal conditions, not compiler refusal tests.
Totality, provenance and identity analysis remain unimplemented.

Each benchmark warmed both variants for 100 calls, alternated order over seven samples, consumed
results outside timing, and reports median total milliseconds per sample. Delegates and inputs
were constructed before measurement. Both variants cache Count per loop, matching the live
loop emitter, and use the same `IReadOnlyList<Number>` boundary. Timing excludes runtime Reduce
to avoid crediting callback elimination to fusion.

| Input elements | Calls/sample | Separate ms | Combined ms | Separate / combined | Allocated bytes/call, both |
| --- | --- | --- | --- | --- | --- |
| 16 | 250000 | 27.523 | 14.874 | 1.850 | 0 |
| 4096 | 976 | 22.042 | 12.144 | 1.815 | 0 |
| 1000000 | 8 | 36.093 | 19.426 | 1.858 | 0 |

The local microkernel benefits from fewer indexed interface reads and traversals. It saves no
allocations. No generated/native-code disassembly or end-to-end geometry benchmark was performed,
so these ratios do not establish that the JIT preserves the same benefit in real callers.
More accumulators could regress performance through register pressure.

Build succeeded with no compiler errors. NuGet vulnerability lookup emitted NU1900 because
the service index was unreachable; package restore/build still completed. Early prototype-only
syntax errors and an SDK/runtime extension collision were corrected before this run; the latter
was resolved with the existing test-project exclusion policy, not a runtime modification.
No repository-wide gates were run by this track because it changes no production compiler code.

## Critical disposition and next step

Proceed to a read-only TIR candidate census after compiler ownership is assigned. The real
Clothoid evaluator has adjacent x/y sample reductions, but its callbacks must pass the proposed
totality and source-provenance checks before implementation. Do not release arbitrary sibling
fusion on the strength of this favorable scalar kernel. The first executable experiment is done;
actual compiler recognition, emission, compiled Plato fixtures, optimizer stewardship and
whole-application validation are outstanding.
