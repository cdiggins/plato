---
id: repo-474
title: Preserve gate command output for failure diagnosis
type: debt
status: done
priority: p2
effort: S
risk: low
area: repo
sprint: 
created: 2026-10-23
closed: 2026-10-23
links: []
---

`tools/record-gates.py` captures full command output but retains only counts in the snapshot and gate log. A failing long test run therefore loses test names and messages, forcing another run to diagnose it. Preserve per-command UTF-8 logs under `.temp/gate-logs/` and print their directory after a recorded run. Keep `--dry-run` free of new log writes and preserve command exit/count behavior.

## Done means

- [x] Recorded runs retain each command's stdout/stderr without changing parsed results or exit status; dry-run logging remains disabled.

Verified with an actual child command that writes both output streams and exits 7, plus isolated command-dispatch checks for disabled dry-run logging and distinct gate log paths. No compiler or law execution behavior changes.
