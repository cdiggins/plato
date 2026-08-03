# Plato documentation

Start here. This directory holds four kinds of document, and the kind is visible from where the file
sits and how it is named.

**Naming rule: `ALL-CAPS.md` means normative.** A reader may cite it; changing it is a decision, not
an observation. Everything in `kebab-case.md` is descriptive, historical, or exploratory.

---

## Normative — the rules

| Document | What it settles |
|---|---|
| [`SEMANTICS.md`](SEMANTICS.md) | What Plato constructs mean. The language the checker accepts. |
| [`documentation-conventions.md`](documentation-conventions.md) | What a durable document here may and may not state. |
| [`../stdlib/STYLE_GUIDE.md`](../stdlib/STYLE_GUIDE.md), [`../stdlib/CONVENTIONS.md`](../stdlib/CONVENTIONS.md) | How `.plato` source is written and named. |
| [`../stdlib/VERIFICATION.md`](../stdlib/VERIFICATION.md), [`../stdlib/LIBRARIES.md`](../stdlib/LIBRARIES.md) | What the gates check; what each library is for. |
| [`../AGENTS.md`](../AGENTS.md) | How to work in this repo. |

## Reference — how things currently work

Maintained, descriptive, superseded by editing rather than by writing a successor.

- [`plato-for-agents.md`](plato-for-agents.md) — operations and codegen; the orientation doc.
- [`plato-overview.md`](plato-overview.md) — what Plato is and why.
- [`plato-library-map.md`](plato-library-map.md) — which `Plato.*` artifact is which, and what is frozen.
- [`compiler-pipeline.md`](compiler-pipeline.md), [`type-checker-handoff.md`](type-checker-handoff.md),
  [`plato-emitter-phases.md`](plato-emitter-phases.md) — source to generated code.
- [`plato-intrinsics-surface.md`](plato-intrinsics-surface.md), [`plato-struct-surface.md`](plato-struct-surface.md) — the backend contracts.
- [`affine-types.md`](affine-types.md), [`affine-types-overview.md`](affine-types-overview.md) — the `unique` feature.
- [`verification-inventory.md`](verification-inventory.md) — every tool that checks this repo.

## [`design/`](design) — proposals and specs

Work that is planned, in flight, or specified but not fully executed. These steer decisions; they do
not describe what exists today. When one lands, its content moves into a reference doc and the plan
moves to [`archive/`](archive).

## [`reports/`](reports) — assessments, surveys, harvests

Frozen at the moment they were written. Read them for the analysis, never for current state: a
report is stale by construction. Never edit one to bring it up to date — write a new one.

- [`Plato standard-library comparative study (2026-08-03)`](reports/plato-stdlib-comparative-study-2026-08-03.md)
  — comparison with numerical, graphics-math, computational-geometry, mesh-processing, and
  planar-geometry libraries, with recommendations for Plato's next library work.

## Casual records — not authority

- [`discussions/`](discussions) — raw conversation transcripts.
- [`essays/`](essays) — prose written for humans outside the project.
- [`archive/`](archive) — executed, superseded, or explicitly-not-a-source-of-truth documents, kept
  for provenance. Nothing here should be linked as a reason to do something.

## Generated — never hand-edit

`gate-log.md`, `status-report.html`, `status-report-snapshot.json`, `stdlib-ai-summary.txt`,
`types-and-concepts-*.txt`. Each is produced by a tool in [`../tools/`](../tools); these are the
files allowed to carry measurements, because the date they carry is honest.

---

**Linking rule.** Only normative and reference documents may be cited as authority. Link a report,
essay, or archived plan only with its date visible in the link text, so a reader knows they are
looking at a snapshot.
