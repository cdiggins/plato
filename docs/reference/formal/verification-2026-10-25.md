# Formal verification checkpoint — 2026-10-25

State: verified; awaiting supervisor integration.

The checker reports:

```text
Lean (version 4.19.0, x86_64-w64-windows-gnu, commit 6caaee842e94, Release)
```

From the repository root, the actual successful command was:

```powershell
& .temp/lean-proof/lean-4.19.0-windows/bin/lean.exe docs/reference/formal/Core.lean
```

Result: exit code `0`, no diagnostics. A further run with `-DwarningAsError=true` also exited `0`. A repeat of `--version` and the full source check completed in `2.745249` seconds. This dated checkpoint records the observation; durable teaching pages do not duplicate changing check measurements.

The proof source imports only Lean's standard `Init` module. Inspection found no `sorry`, `admit`, or custom `axiom` declarations in `Core.lean`. The checked results include independent typing/inference equivalence, unique typing, terminal evaluation safety allowing faults, deterministic total evaluation, recursive conditional-fold outcome preservation, Boolean conjunction typing, constant-conditional rules, and an eager-conjunction counterexample.

Scope exclusions are explicit in the teaching pages: there is no proof about full Plato inference, machine numeric profiles, small-step progress/preservation, or compiler/runtime refinement. The deterministic evaluator theorem is the graph-of-a-function result, not a second independently verified semantics.

An initial attempt failed before elaboration while the official toolchain was still being extracted (`Init.olean` absent); a following attempt could not find `Std`. The successful source minimizes dependencies to `Init` and was checked after that module became available. Those earlier attempts are not passing evidence.

Files owned by this track: this `formal/` subtree only. Repository gates are the supervisor's responsibility. No compiler, runtime, standard-library, tracker, or other author files were edited here.
