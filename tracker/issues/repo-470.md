---
id: repo-470
title: Remove tracked Python bytecode cache
type: debt
status: done
priority: p3
effort: S
risk: low
area: repo
sprint: 
created: 2026-10-23
closed: 2026-10-23
links: []
---

`tools/__pycache__/record-gates.cpython-313.pyc` is a tracked interpreter-specific cache even though `.gitignore` already ignores that directory. It adds stale binary churn beside the maintained Python source and is recreated automatically by Python.

## Done means

- [x] Remove the tracked cache and verify the source still parses without writing bytecode.
