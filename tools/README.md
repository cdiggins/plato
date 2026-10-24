# Repository maintenance commands

Scripts here run gates, regenerate artifacts, export source indexes and maintain work tracking. Begin with `check-stdlib-fast.ps1` for the library inner loop, `regen-foundation.ps1` for C# generation, `record-gates.py` for recorded gate results, and `track.py` for tracker updates. [AGENTS.md](../AGENTS.md) owns command policy. Keep entry-point paths stable because documentation and external checkouts invoke them.

Recorded gate runs keep detailed command output under `.temp/gate-logs/` and print the run's directory. Use those logs to diagnose a failed gate before rerunning it. `record-gates.py --dry-run` does not create command logs.
