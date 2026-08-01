# Documentation conventions

**How to write a durable document in this repo, and — the part that keeps getting violated — what
must never go in one.**

Applies to durable prose under `docs/`, `stdlib/*.md`, `tracker/readme.md`, and every `README.md`.
Does not apply to dated snapshots (`*-YYYY-MM-DD.md`), `docs/archive/**`, or generated files
(`docs/gate-log.md`, `docs/status-report*.{html,json}`) — those exist *to* record measurements at a
moment, and the date in the name is the honest label.

For README voice and structure specifically, the `write-readme` skill is the reference; this file
governs the facts a document is allowed to state, whatever its shape.

---

## The rule

**A durable document states design, not measurements.**

Design is what somebody decided and why: the gate ladder, the `future` tier's lower bar, why the
lint ratchet is a test and not a script. It stays true until somebody decides otherwise, and when
they do, editing the doc is part of that decision.

A measurement is a count of what happens to exist right now: files, symbols, findings, diagnostics,
ratchet ceilings, test tallies, generated-file counts, timings. It goes stale on its own, without
anybody deciding anything — often within days, sometimes while the document is still being written.

**Stale numbers are worse than absent ones.** A reader cannot distinguish a stale number from a
fresh one, so one rotten figure discredits the whole document. And a second copy of an enforced
value — a ratchet ceiling, say — goes stale *silently* while the enforced copy moves.

### The three-month test

Before writing any number, ask: **will this still be true in three months?**

If no, it is a measurement. Do not write it. Name its authority instead — the constant that holds
it, the command that measures it, or the log that records it.

### Keep / drop

**Keep:**

- Normative limits that are part of a specification — "the `TupleN` surface stops at 10 fields", "a
  doc-comment block caps at 12 lines". These are decisions, not censuses.
- Historical incident figures tied to a fixed past event — "that shape produced 40 CS0736 errors".
  The event is over; the number cannot drift.
- Orders of magnitude, stated as such — "runs in seconds", "fires in the thousands", "over a
  thousand generated files". Useful for calibration, immune to drift.
- Identifiers, paths, flags, rule codes, issue ids.

**Drop:**

- Scope preambles that inventory a directory ("`stdlib/`, 424 files — foundation 133, geometry
  192 …"). They read as rigour and add nothing a reader can act on.
- "Current state (measured `<date>`)" tables. The date looks responsible and is exactly the trap: a
  reader who sees a date trusts the content and has no way to know it rotted.
- Restated ratchet ceilings, finding counts, diagnostic counts, test tallies (`196/196`), generated
  file counts, per-gate timings.
- Any number that a script already produces. If it has a generator, linking to the generated
  artifact is strictly better than typing it.

### Say where the number lives

Replace the number with its authority. In this repo:

| Question | Authority |
|---|---|
| what do the gates say right now? | `plato_check`, or `python tools/record-gates.py --dry-run` |
| what did they say at commit X? | `docs/gate-log.md` |
| current machine-readable state | `docs/status-report-snapshot.json` |
| a ratchet ceiling | the constant in the test that enforces it |
| how long a gate takes | `.\tools\gate-timings.ps1` |
| what is left on a burn-down | `python tools/track.py show <id>` |

### Declare it in the document

A document that deliberately omits measurements should say so, in one sentence, where a reader would
expect the numbers. Otherwise the next well-meaning author helpfully adds them back.
[`docs/stdlib-verification-ladder.md`](stdlib-verification-ladder.md) is the worked example.

### When a document and a gate disagree, the gate is right

Status prose rots faster than anything else in the repo: several status blocks here describe
blockers that were fixed after they were written. So never quote a status claim without
re-measuring, and prefer writing the precedence rule over writing the status.

---

## Other conventions

- **One authority per fact.** Before writing something a different file already states, link to that
  file instead. Two copies of a rule become two different rules.
- **Link, do not summarize.** A summary of a document that lives next door is a second copy under
  the previous rule.
- **Name the mechanism, not the moment.** "The corpus floor keeps an empty enumeration from passing"
  survives; "the corpus floor is 300" does not.
- **Prefer a rule to a report.** "A ceiling is lowered, never raised" is durable; "the ceiling is 33"
  is not.
- **Mark what is uncertain.** If a claim is inferred rather than measured, say so in the sentence.
  Confidence is a fact about the claim and does not drift.

---

## Enforcement

Today: this document plus the repo-local `write-docs` skill (`.claude/skills/write-docs/`), which
loads it for any agent writing or editing documentation. There is no mechanical check yet — the
rules above are deliberately being allowed to settle first.

When a checker is written (`tools/lint-docs.py`), the decision already taken is that it is
**opt-in**: a document is linted only if it carries an explicit marker, so new durable docs opt in
and no existing file breaks. That mirrors how `StyleChecker` relates to `stdlib/STYLE_GUIDE.md` —
the prose rule comes first, the mechanical rule follows once it is worth encoding.
