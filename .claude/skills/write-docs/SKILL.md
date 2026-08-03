---
name: write-docs
description: Read BEFORE writing or editing any durable document in this repo — docs/*.md, stdlib/*.md, tracker/readme.md, AGENTS.md, any README. Enforces the no-drifting-facts rule (state design, never measurements) plus the one-authority-per-fact rules from docs/documentation-conventions.md. Use when the user says "document X", "write a doc", "update the docs", "write this up", or when a task produces a lasting write-up. Skip for dated snapshots, docs/archive/**, and generated files.
argument-hint: [path to the document being written or edited]
---

# Writing durable docs in the Plato repo

Normative source: [`docs/documentation-conventions.md`](../../../docs/documentation-conventions.md).
Read it when a case is not covered below. This skill is the working checklist.

Worked example of the style: [`stdlib/VERIFICATION.md`](../../../stdlib/VERIFICATION.md).

## The one rule that keeps getting violated

**A durable document states design, not measurements.**

Before writing *any* number, apply the **three-month test**: will this still be true in three
months? If no, it is a measurement — do not write it. Name its authority instead.

Stale numbers are worse than absent ones: a reader cannot tell a stale figure from a fresh one, so
one rotten number discredits the document. A second copy of an enforced value (a ratchet ceiling)
rots silently while the enforced copy moves.

### Drop on sight

- Scope preambles that inventory a directory ("`stdlib/`, 424 files — foundation 133, …").
- "Current state (measured `<date>`)" tables. The date makes it *more* dangerous, not less.
- Restated ratchet ceilings, finding counts, diagnostic counts, test tallies, generated-file
  counts, per-gate timings.
- Any number a script already produces.

### Keep

- Normative limits from a spec ("`TupleN` stops at 10 fields").
- Historical incident figures tied to a finished event ("that shape produced 40 CS0736 errors").
- Orders of magnitude, stated as such ("runs in seconds", "fires in the thousands").
- Identifiers, paths, flags, rule codes, issue ids.

### Say where the number lives instead

| Question | Authority |
|---|---|
| what do the gates say now? | `plato_check`, or `python tools/record-gates.py --dry-run` |
| what did they say at commit X? | `docs/gate-log.md` |
| a ratchet ceiling | the constant in the test that enforces it |
| how long a gate takes | `.\tools\gate-timings.ps1` |
| burn-down status | `python tools/track.py show <id>` |

## Also

- **One authority per fact.** If another file already states it, link — do not restate. Two copies
  become two different rules.
- **Name the mechanism, not the moment.** "The corpus floor stops an empty enumeration from
  passing", not "the corpus floor is 300".
- **Prefer a rule to a report.** "A ceiling is lowered, never raised" outlives "the ceiling is 33".
- **When a document and a gate disagree, the gate is right.** Several status blocks in this repo
  describe blockers that were fixed after they were written. Re-measure before quoting status prose.
- **Declare the omission.** Where a reader expects numbers, say in one sentence that the document
  records none and why — otherwise the next author helpfully adds them back.
- **Mark inferred claims as inferred.** Confidence is a fact about the claim; it does not drift.

## Scope

Applies to `docs/*.md`, `docs/design/**`, `stdlib/*.md`, `tracker/readme.md`, `AGENTS.md`, and every
`README.md`. [`docs/README.md`](../../../docs/README.md) says which tier a document is in.

Does **not** apply to dated snapshots (`*-YYYY-MM-DD.md`), `docs/reports/**`, `docs/essays/**`,
`docs/discussions/**`, `docs/archive/**`, or generated files (`docs/gate-log.md`,
`docs/status-report*`, `docs/types-and-concepts-*.txt`) — recording a moment is their whole purpose,
and the date in the name is the honest label.

For README voice and structure, use the `write-readme` skill; this skill governs which facts any
document may state.

## Before you finish

Re-read your own draft hunting only for numbers, and delete or re-home each one that fails the
three-month test. This is the step that gets skipped — the rule is easy to agree with while
drafting and easy to violate in the same paragraph.
