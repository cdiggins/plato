---
id: repo-474
title: Preserve gate command output for failure diagnosis
type: debt
status: in-progress
priority: p2
effort: S
risk: low
area: repo
sprint: 
created: 2026-10-23
closed:
links: []
---

`tools/record-gates.py` captures full command output but retains only counts in the snapshot and gate log. A failing long test run therefore loses test names and messages, forcing another run to diagnose it. Preserve per-command UTF-8 logs under `.temp/gate-logs/` and print their directory after a recorded run. Keep `--dry-run` free of new log writes and preserve command exit/count behavior.

## Done means

- [ ] Recorded runs retain each command's stdout/stderr without changing parsed results or exit status; dry-run logging remains disabled.
