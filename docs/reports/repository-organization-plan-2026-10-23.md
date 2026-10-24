# Repository organization plan

Issue: [repo-468](../../tracker/issues/repo-468.md), source organization and navigation documentation.
Contract: organization-v1. Checkout: `C:\Users\cdigg\git\plato`, current branch `main`.

Readers should find the code for a responsibility from its folder guide, then understand a source file's role from its opening comment. Existing behavior, namespaces, APIs, CLI paths and stdlib tier enumeration remain stable. No new runtime abstraction is required for a move or a partial-class split.

## Independent chunks

| Owner | Writable source fence | Chunk and check | Dependencies |
|---|---|---|---|
| Compiler Sol | `src/Plato.Compiler`, `src/Plato.AST`, `src/Plato.Parser`, `src/PlatoTypeInference` | Group checking/AST responsibilities, split cohesive large units when safe, add headers and guides; inspect declaration preservation and diff | Contract acknowledged |
| Writers Sol | `writers/` | Group emission, optimization and support responsibilities, split cohesive large emitters when safe, add headers and guides; inspect declaration preservation and diff | Contract acknowledged |
| Runtime Sol | All other projects beneath `src/`, excluding `src/README.md` | Group execution, laws and navigation responsibilities, split cohesive large units when safe, add headers and guides; inspect path consumers and diff | Contract acknowledged |
| Supervisor | All other paths, tracker, plan, build outputs and aggregate logs | Audit remaining owned files/folders, organize tests where useful, repair moved-path references, run checks and publish | Independent audit starts immediately; integration follows stopped writers |

Each agent may write its checkpoint only in `.temp/organization/<owner>.md` in addition to its fence. No agent runs builds, restores or generators while another is editing shared dependencies. Supervisor owns those resources and all tracker edits. Each chunk should take minutes, with further splits reported before committing. Staging/committing is serialized through explicit supervisor grants. Agents commit their own paths and do not push. Supervisor grants temporary ownership of the corresponding issue checkbox during a commit turn. Unrelated staged work is never included or removed.

## Verification

Each source chunk receives structural/diff checks before its commit. User authorized batching executable tests at the end. Integration builds through `tools/dotnet-build-record.ps1`, then runs relevant existing tests, `python tools/record-gates.py`, and studio's `tools/check-all.ps1` once. Logs distinguish failures from this change, established baseline failures and environmental blockers. Capture the tested revision and confirm source inputs remain stable.

Initial unrelated changes: `docs/status-report-snapshot.json`, `docs/status-report.html`, and another mission's untracked `PROGRESS.md`. Preserve these; save copies before gate scripts update reports. Git status uses `-c diff.ignoreSubmodules=all` because traversal of `parakeet` metadata is denied in the sandbox. Do not modify external submodules.

## Considered and rejected

- Moving stdlib files into deeper domain folders would break top-directory-only input enumeration and contradict its flat-tier contract. Keep tiers flat and improve navigation documentation. Revisit only with an explicit compiler/input-policy change.
- A blanket rewrite of generated sources or historical snapshots would be overwritten or alter evidence. Document these ownership and format exceptions in folder guides and the audit.
- Performance changes without a demonstrated issue add risk to a structural cleanup. Perform only clearly justified independent improvements, with their own tracker record and commit; otherwise file the finding.

## Checkpoints

Plan recorded; implementation pending. Root `PROGRESS.md` belongs to another mission and is preserved.
