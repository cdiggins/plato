---
id: compiler-536
title: Attribute full optimizer recipe edge allocation regression
type: problem
status: ready
priority: p2
effort: M
risk: med
area: compiler
sprint: 
created: 2026-10-25
closed:
links: []
---

The complete optimizer recipe now builds after compiler-535, but it is not ready
to replace shipping `--loops --fuse`. On the frozen BlockWithThroughHole case,
edge discretization allocates about 365,041 bytes per operation versus 141,360
for shipping. The increase repeats in the quick and five-process confirmation
runs. Output comparisons pass. Timing remains inconclusive on this shared host;
do not describe this allocation finding as a demonstrated wall-time regression.

Evidence: `.temp/ob535/runs/compiler535-recipe-confirm/report.json` and
`samples.jsonl`; workspace identity
`1c5d0b4de26a93eb6118558c8a1c540d1ce7bb99e386210d82405249a6c84d50`.
BREP input `5057732c0c6858f71fac298535af203226f7f203`, stdlib/runtime
`fd83e4a33b09fd2aa49e59e1b1bbb9f6cdb85a30`, compiler fix `6d64d604`.
The full recipe enables inline, fusion, component expansion, loops,
multi-consumer fusion and exact-output specialization. Pass attribution is not
yet established. This is a bounded pass-ablation investigation before a fix,
not permission to change successful geometry results or skip required checks.

## Done means

- [ ] Reproduce the allocation increase with identical prepared inputs and hosts.
- [ ] Attribute the extra allocation to a pass and emitted call path using ablation.
- [ ] Fix or document a justified recipe exclusion, with output equivalence and
      affected correctness tests retained.
- [ ] Repeat independent process timing and allocation measurements; report
      uncertainty and corpus coverage before recommending shipping flags.
