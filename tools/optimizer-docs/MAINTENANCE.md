# Optimizer Documentation Stewardship

This is an agent/developer runbook, separate from the [reader handbook](../../docs/optimizer/README.md).
Read the repository's [documentation conventions](../../docs/documentation-conventions.md) first.

## Ownership and review

The pipeline reference owns scheduling and exact runtime contracts. Component guides own rationale,
algorithm explanations, boundaries and evidence interpretation. Greenhouse owns symbol extraction;
`update.py` normalizes its output into a deterministic source map. Do not generate explanations by
copying source comments: some retain retired scalar-erasure and legacy-writer assumptions.

`manifest.json` declares monitored source globs, required public types and reader pages. Globs detect
new/deleted implementation and test files as well as edits. Changes to the CLI, runtime, TIR helpers
or emitter can invalidate an explanation even if the optimizer file itself did not change. Extend
the manifest when an algorithm acquires another dependency; this is declared coverage, not a
complete semantic dependency graph. New passes need a component guide and a required type entry.

## Refresh after an algorithm change

1. Inspect the implementation and its tests. Use Greenhouse `explore` on the changed symbol to
   inspect callers/callees; directory mode may widen to its enclosing project. Use `.csproj` mode
   when resolved project references matter. Run changed behavioral tests, not only text assertions.
2. Update the affected component page, overview and pipeline reference as appropriate. Cover the
   core transformation, rationale, accepted/refused cases, flag interactions, tests and evidence of
   effectiveness. Cite primary external references and distinguish analogy from implemented scope.
3. Regenerate source evidence with a built Greenhouse CLI:

   ```powershell
   python tools/optimizer-docs/update.py refresh --greenhouse C:/path/to/Greenhouse.Cli.dll
   ```

   Build Greenhouse's CLI from its checkout if needed. The required interface is `docs build` and
   `docs export --format json`; partial-type deduplication is required for Plato's writer project.
   `refresh` invokes Roslyn extraction, rejects empty/incomplete exports, normalizes paths, drops
   timestamps and captures source hashes. It does not approve prose. Temporary database/export
   files live under `.temp/optimizer-docs/`. `evidence.json` and `source-map.md` are generated.
4. Review the diff, then explicitly record the explanation of the review:

   ```powershell
   python tools/optimizer-docs/update.py accept-review --note 'Explain the algorithm change and which guides were updated, or why their claims remain accurate.'
   python tools/optimizer-docs/update.py check
   python -m unittest discover -s tools/optimizer-docs -p 'test_*.py'
   ```

5. Commit source, tests, guides, generated map, evidence and review together. Put dated measurement
   results in `docs/reports/`; link them instead of adding volatile numbers to the guides. Keep
   tracker progress and gate records according to the repository instructions.

## Automatic enforcement

The `Optimizer documentation` GitHub workflow runs on pushes and pull requests, using Python only.
It regenerates the map from committed Greenhouse evidence, compares it with the committed file,
checks local links and rejects source hashes that differ from either extraction or prose review.
Algorithm-body edits are detected even when public signatures are unchanged. Normalized newlines
keep Windows and Linux checkouts equivalent.

CI intentionally does not run an unattended language model or rewrite design prose. It fails with
the changed paths and this runbook, requiring the author/agent to refresh and review in the same
change. `accept-review` is an explicit attestation, not proof of semantic accuracy; reviewers must
inspect its note and prose diff. Configure this workflow as a required branch check if merges must
be blocked; a workflow file alone cannot change repository branch-protection settings.

`render` regenerates Markdown from the committed evidence without needing Greenhouse. It cannot
clear stale-source failures. `refresh` does not silently bless an outdated explanation, and
`accept-review` refuses evidence from different source inputs. Keep the full evidence/check path
working rather than updating hashes by hand.

Do not overwrite a concurrent session's root `PROGRESS.md`. Use the mission tracker/commit record
for recovery when that scratch slot is occupied.
