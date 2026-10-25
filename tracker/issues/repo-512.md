---
id: repo-512
title: C# build recorder project mode calls missing run_build
type: bug
status: ready
priority: p2
effort: S
risk: low
area: repo
sprint: 
created: 2026-10-25
closed:
links: []
---

Discovered independently by compiler-506 and compiler-507 experiments. tools/record-csharp-build-errors.py main calls run_build when --project is supplied without --log, but no run_build function exists. The intended subprocess body sits after returns in read_log_text. The direct project mode fails with NameError before recording build evidence. The PowerShell wrapper and --log mode remain usable. Small isolated tooling fix; do not conflate with optimizer implementation.

## Done means

- [ ] Restore the missing build function and retain complete build logs.
- [ ] Verify --project mode records successful and failing builds with the correct exit status.
- [ ] Verify existing --log mode remains working.
