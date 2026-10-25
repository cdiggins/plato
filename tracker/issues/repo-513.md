---
id: repo-513
title: Separate slow corpus tests from routine regression runs
type: debt
status: in-progress
priority: p2
effort: M
risk: low
area: repo
sprint: 
created: 2026-10-25
closed:
links: []
---

## Done means
- [ ] Identify expensive compiler tests from measured timings and keep focused regression checks in the default run.
- [ ] Tag slow checks and diagnostic reports; provide default, slow, and full commands while preserving ratchets in recorded gates.
- [ ] Verify default and full selections and record timing and test results.
