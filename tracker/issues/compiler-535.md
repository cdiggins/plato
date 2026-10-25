---
id: compiler-535
title: Complete optimizer recipe exceeds BREP generation budget
type: problem
status: in-progress
priority: p1
effort: M
risk: med
area: compiler
sprint:
created: 2026-10-25
closed:
links: [compiler-522]
---

The prepared optimizer benchmark cannot currently time the complete supported C#
recipe on the committed plato-brep shipping program: its generation process exceeds
the configured 900-second setup limit without producing generated C# files. This
blocks whole-recipe performance comparisons, although the shipping and unoptimized
references generate, pass the existing stub checker and build successfully.

## Evidence and reproduction

Observed 2026-10-25 on the shared Windows development machine. The frozen workspace
is `.temp/ob3/workspace.json`, key
`2f445bbef8e4ce82d6d1e53763c59dd33f86a6bdb7a2cd5754155ec1f8261236`.
Plato library/runtime revision: `fd83e4a33b09fd2aa49e59e1b1bbb9f6cdb85a30`;
plato-brep: `5057732c0c6858f71fac298535af203226f7f203`. The supplied CLI binary
closure is hashed separately in that workspace's manifest; it was rebuilt through
the recorded `optimizer-current-cli` build after the generic constructor fix.

The failing command generates the exported foundation, geometry and shipping BREP
library roots with `--profile=cpu64 --inline --fuse --optimize --loops
--fuse-consumers --exact-output`. Python raised `subprocess.TimeoutExpired` after
900 seconds. The failed generation folder is `.temp/ob3/cache/b/e5c0a39b1690`;
the failure is retained in `.temp/ob3/builds.json`. No C# build ran for this recipe.
This run predates timeout-output retention in the benchmark builder, so partial
compiler stdout is unavailable. Do not infer a specific pass or nontermination from
the timeout alone. The follow-up with only inlining removed generated checked C#
and built successfully: generation/checking/source publication took 247.3 seconds,
and build/host freezing took 155.1 seconds. This is a diagnostic in the complete
context; it does not identify an isolated pass defect. The dated compiler-522
verification report retains the artifact identities and experiment evidence.

```powershell
python tools/benchmarks/optimizer/benchmark.py build --work .temp/ob3 --variants full --timeout 900
```

On another machine, follow the benchmark README to prepare the frozen revisions
and an identified compiler first. Source preparation is separate from compiler
binary provenance. Profile the generation phase before proposing algorithm changes.

## Done means

- [ ] Reproduce and identify where generation time is spent under the complete recipe.
- [ ] Implement a bounded correction or document evidence supporting a deliberate larger setup budget.
- [ ] Verify the complete recipe generates checked, buildable C# for the same BREP program and retain timing evidence.
