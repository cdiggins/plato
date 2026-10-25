# Temporary record elimination experiment — 2026-10-25

Tracked by [compiler-507](../../tracker/issues/compiler-507.md). The isolated prototype is executed;
the [production plan](../design/optimizer-exploration/record-elimination.md) remains unimplemented.
The useful result is negative: this simple struct projection establishes no allocation saving
or convincing timing improvement. Prioritize a real workload census before adding a pass.

## Reproduction and evidence

Run from the repository root:

```powershell
python tools/experiments/optimizer-exploration/record-elimination/eligibility.py
python tools/experiments/optimizer-exploration/record-elimination/run.py
```

Sources are [Program.cs](../../tools/experiments/optimizer-exploration/record-elimination/Program.cs),
the handwritten C# before/after experiment, and
[eligibility.py](../../tools/experiments/optimizer-exploration/record-elimination/eligibility.py),
an executable admission-contract model. Neither is an implementation over Plato TIR. The local
`Number` wrapper mimics the representation shape but is not `Plato.Intrinsics.Number`.

| Gate actually run | Result | Seconds |
| --- | --- | --- |
| Python admission model | Passed acceptance and 10 refusal subcases | <0.01 |
| Isolated Release net8 build | Passed; no errors or warnings | 2.20 |
| C# eager evaluation checks | Passed 8 combinations of first-field, second-field and conversion faults | Included in executable |
| Unsafe projection counterexample | Confirmed that dropping the second field suppresses its fault | Included in executable |
| Warmed paired kernel experiment | Completed 12 alternating-order samples | See individual samples below |

The final measured run used .NET 8.0.30 on Windows 10.0.26200, x64 Intel Family 6 Model 170
Stepping 4, SDK 10.0.400 targeting net8 with default language version. Tiered compilation was
disabled to avoid promotion during the tiny benchmark. Each variant was warmed 20 times before
measuring a million iterations per sample. Delegates and comparison setup were outside timing
and allocation regions; results were consumed and compared. Concurrent sessions mean elapsed
times are noisy. No JIT disassembly, default-tiering profile or actual generated Plato was tested.

| Sample | Construct/project milliseconds | Scalar bindings milliseconds | Bytes in either variant |
| --- | --- | --- | --- |
| 0 | 0.8035 | 0.9239 | 0 |
| 1 | 0.6744 | 0.6835 | 0 |
| 2 | 0.6682 | 0.8090 | 0 |
| 3 | 0.8767 | 1.0038 | 0 |
| 4 | 0.6914 | 0.7049 | 0 |
| 5 | 0.6950 | 0.7103 | 0 |
| 6 | 0.7079 | 0.6915 | 0 |
| 7 | 0.7063 | 0.6912 | 0 |
| 8 | 0.6958 | 0.6940 | 0 |
| 9 | 0.6945 | 0.8388 | 0 |
| 10 | 0.7106 | 0.7777 | 0 |
| 11 | 0.6914 | 0.7084 | 0 |

Private `.temp/optimizer-exploration/record-elimination/` holds `build.log`,
`build-snapshot.json`, `results.txt` and `fingerprints.json`. These are overwritten by reruns.
The measured `Program.cs` SHA-256 was
`c67a44160341bf4b71c2df0dbe47e64f7a11fa1a9434dc898fcd808d068b89da`.
The inspected component unroller SHA-256 was
`0ab5b2a5158781d328f059675e565f56cf371af2001dedb9dee41ee5b06b43ad`.

## Critical interpretation and next step

Zero allocation is expected for small value records; it does not prove scalar replacement in
machine code. Similar timing does not prove identical instructions. The experiment rules out
claiming an obvious allocation win from this example and catches one unsound simplification.
It does not model nontermination dynamically; preservation follows only from retaining calls in
order, and still needs actual generated-code validation. The admission model tests a proposed
contract rather than proving that real compiler metadata supplies its conditions.

Source inspection found writer markers with field names and cast strings, so matching those
strings alone is not an acceptable provenance check. The next bounded step is a census of real
constructor/projection TIR after existing passes, then a compiled fixture that establishes field
identity and conversion context. Broader escape analysis, array dead-code removal and production
pass scheduling were not started. No whole-repository gate or stewardship refresh was needed
for unchanged production sources; the supervisor owns integration gates.

The standard PowerShell recorder would overwrite another session's dirty shared reports.
The approved adapter instead preserves a private build log and calls the same recorder with
`--log` and `--build-exit`, redirecting its snapshot. Its direct build mode also exposed
[repo-512](../../tracker/issues/repo-512.md), an undefined `run_build` function; that unrelated
defect was reported to the supervisor and left untouched. An initial intermediate-output warning
was corrected by assigning the private path before importing SDK properties; the final build
was clean.
