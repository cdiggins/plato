# Reference verification, 2026-10-25

This dated snapshot records checks actually run for [docs-523](../../../tracker/issues/docs-523.md), the hierarchical reference and React reader. It is evidence from this integration session. It is not a promise that every later checkout passes. The mission changes documentation, examples, and reference tooling. It does not repair the compiler, or the scientific algorithms the audit identified.

| Check | Result | Evidence and scope |
| --- | --- | --- |
| Reference generation and local links | Passed | `python tools/reference/build.py --check` checks generated freshness, covered roots and reference targets. |
| Generator tests | Passed | `python tools/reference/test_build.py` exercises source partitioning, comment/string handling, multiline calls, link offsets and path restrictions. |
| Reader helper tests | Passed | `npm --prefix tools/reference-reader test` checks routing, tree construction, search and fenced-code heading exclusion. |
| React production build | Passed | The data-backed build runs the generator before Vite. Mermaid produces a large-chunk warning; full build output is retained in the working log. |
| Pinned formal proof source | Passed | Lean 4.19.0 checks Core.lean with warnings treated as errors; the recorded checker subprocess took 2.143 seconds. The theorems concern the bounded core, not the full compiler. |
| Interpreter teaching examples | Passed | Every selected Law_Reference function in products, sums, functions, loops and builders reports pass. The checker rejects empty selection, obligations and stale reports. |
| Differential builder examples | Passed | The foundation-dependent List/Freeze examples agree between interpreter and compiled C#; the recorded subprocess took 150.249 seconds. |
| Recorded CLI build | Passed | The normal-layout recorded run reports 18 seconds and no build errors. |
| Recorded shipping-tier strict lint | Passed | The normal-layout run reports 28 seconds; warning/info findings remain recorded in the gate log. |
| Recorded PlatoTests | Passed | The normal-layout suite reports 396 passed and no failures in 278 seconds, including both library ratchets. |
| Studio full battery | Qualified failure | Both library lints, SDK build and geometry tests pass; the frozen SDK runtime checksum gate fails. See repo-532 below. |
| Preview identity and opening | Passed | The dedicated endpoint returns the Plato Reference HTML title and the expected reference dataset before dispatch to the default external browser. |
| Browser visual automation | Not verified | Browser/app control timed out. HTTP identity checks and an external-browser launch do not establish visual rendering or every interaction. |

## Verification environment and failures investigated

The shared default CLI build initially encountered CS2012: the compiler output DLL was in use by another process. An isolated artifacts build passed. Running the entire test suite in an isolated layout then exposed [repo-531](../../../tracker/issues/repo-531.md): fixture lookup mistook `bin/PlatoTests` for the source test folder. Every failure in that run was a missing fixture. The normal-layout recorded rerun passes. Both runs remain in the gate log.

[repo-532](../../../tracker/issues/repo-532.md) captures the studio checksum failure. Its SDK copies of PlatoList.cs, PlatoBuffer.cs, and Number.cs differ from the recorded frozen baseline. The reference mission did not modify them and did not rebaseline the checksum. This is separate from Plato's writable forward sources and generated caches.

Port 5178 initially answered with another application's HTML, Platonic Agora. The reader now uses `http://127.0.0.1:5391/`, binds that loopback address explicitly, and was restarted with local-network access for the external browser. Its title and dataset identity were checked before reopening it. A successful HTTP status is not enough to verify the preview.

## Retained logs and rerun commands

Working logs are under `.temp/reference-example-logs/`, `.temp/reference-proof-logs/`, `.temp/csharp-build-logs/reference-cli-isolated.log`, `.temp/reference-reader-build.log`, and `.temp/reference-check-all.log`. The isolated gate run is under `.temp/gate-logs/20261025-114436-991105/`. The normal-layout run is under `.temp/gate-logs/20261025-115700-369340/`. These temporary files may be overwritten. The [gate history](../../gate-log.md) and the tracker issues are the durable records.

```powershell
python tools/reference/build.py --check
python tools/reference/test_build.py
python tools/reference/check_examples.py
python tools/reference/check_examples.py --backend differential --group builders
python tools/reference/check_formal.py
npm --prefix tools/reference-reader test
npm --prefix tools/reference-reader run build
python tools/record-gates.py
```

The proof checker requires its pinned toolchain. The example checker requires a built CLI. See the [tool guide](../../../tools/reference/README.md) for explicit executable overrides.
