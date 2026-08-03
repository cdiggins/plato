---
id: plato-269
title: Differential: plato-src-v3 vs plato-src
type: idea
status: idea
priority: "?"
effort: "?"
risk: "?"
area: plato
sprint: 
created: 2026-07-28
closed:
links: [plato-230, plato-263, docs/reports/plato-source-vocabulary-comparison.md, docs/reports/plato-v3-vocabulary-report.md]
---

## Idea

Produce an up-to-date **differential** between the new Plato library (`plato-src-v3`) and the old/shipping one (`plato-src`): what types/interfaces exist only on one side, naming/recipe changes (`interface`+`I` vs `concept`), missing bodies, and migration priorities. Prior art compared `plato-src`↔`plato-src-v2` ([docs/reports/plato-source-vocabulary-comparison.md](../../docs/reports/plato-source-vocabulary-comparison.md)) and summarized v3 alone ([docs/reports/plato-v3-vocabulary-report.md](../../docs/reports/plato-v3-vocabulary-report.md)); this idea is the **v3 ↔ shipping** gap analysis agents and humans need for porting.

## Assumptions

- Both trees remain for a while; decisions need a map, not vibes.
- Counts/names can be extracted mechanically from declarations; semantic "same type?" needs human/agent judgment.
- Output belongs in `docs/` (durable), not `.temp/`.

## Design decisions

- **Match key** — exact name vs normalized (`IVector3`↔`Vector3`) vs manual synonym table.
- **Artifact shape** — one markdown report vs spreadsheet/JSON + summary narrative.
- **Depth** — name inventory only vs also interface lattices, libraries/bodies, and "port next" ranking.
- **Refresh** — one-shot report vs script regenerable on demand.

## Related

- [docs/reports/plato-source-vocabulary-comparison.md](../../docs/reports/plato-source-vocabulary-comparison.md) — v1↔v2; pattern to reuse, not duplicate as-is.
- [docs/reports/plato-v3-vocabulary-report.md](../../docs/reports/plato-v3-vocabulary-report.md) — v3 inventory; open migration questions.
- [plato-230](plato-230.md) / [plato-263](plato-263.md) — v3 creation; possible rename.
- [plato-270](plato-270.md) — sibling differential vs `Ara3D.Geometry`.

## Approaches

Short term: script declaration inventories + normalized-name join; write a short "only-in-old / only-in-new / renamed" report with top port priorities.
Long term: living dashboard or CI check that fails on silent divergence policies.
Adjacent: automated "can this v3 type compile against old bodies?" probes.

## Case against

- **Reports already exist.** Refreshing may be busywork if no migration is scheduled.
- **False synonyms.** Mechanical name matching mislabels intentional splits (e.g. richer v3 types).
- **Stale the day it ships** unless scripted — v3 still moves.

**Verdict: pursue** as a **scripted inventory + short narrative**, timed before any serious port from old→new. Park if rename ([plato-263](plato-263.md)) is about to land (run after rename to avoid double path churn).

## Bedrock

Strengthens the **dual-stdlib migration map** — an explicit join between shipping and forward corpora so port work isn't rediscovered ad hoc. **Verdict: simplest-along-the-grain** — regenerable inventory script + markdown summary; must NOT attempt behavioral equivalence proofs in v1.

## Done means

- [ ] Report (or regenerated artifact) lists only-old / only-new / likely-renames
- [ ] Method for name matching documented
- [ ] Top migration priorities called out (even if provisional)

## Simplest possible implementation

Parse both trees for `type`/`concept`/`interface` names; normalize `I` prefix; emit markdown tables + a one-page executive summary into `docs/`.

Pros: cheap; immediately useful for agents  
Cons: no semantic depth; synonym misses need hand fixes
