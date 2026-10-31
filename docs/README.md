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

The [hierarchical Plato reference](reference/README.md) teaches the core language and standard
library through small linked chapters, source-derived API pages, formal models and an explicit
discrepancy register. Its [React reader](../tools/reference-reader/README.md) assembles the
Markdown into a searchable browser with a dynamic table of contents.

The [optimizer handbook](optimizer/README.md) explains the C# optimization components, their
motivation, source and test evidence, and related compiler ideas. Its [pipeline reference](csharp-optimizer-pipeline.md)
defines scheduling and runtime contracts.

Maintained, descriptive, superseded by editing rather than by writing a successor.

- [`repository-organization.md`](repository-organization.md) — responsibility groups, file and folder documentation, and safe relocation checks.

- [`plato-for-agents.md`](plato-for-agents.md) — operations and codegen; the orientation doc.
- [`plato-overview.md`](plato-overview.md) — what Plato is and why.
- [`plato-library-map.md`](plato-library-map.md) — which `Plato.*` artifact is which, and what is frozen.
- [`compiler-pipeline.md`](compiler-pipeline.md), [`type-checker-handoff.md`](type-checker-handoff.md),
  [`plato-emitter-phases.md`](plato-emitter-phases.md) — source to generated code.
  [`csharp-optimizer-pipeline.md`](csharp-optimizer-pipeline.md) — the C# writer's optimizer passes, flags, and runtime contract.
- [`plato-intrinsics-surface.md`](plato-intrinsics-surface.md), [`plato-struct-surface.md`](plato-struct-surface.md) — the backend contracts.
- [`interpreter.md`](interpreter.md) — the TIR evaluator, its law runner, and `plato run`; no code generation.
- [`affine-types.md`](affine-types.md), [`affine-types-overview.md`](affine-types-overview.md) — the `unique` feature.
- [`verification-inventory.md`](verification-inventory.md) — every tool that checks this repo.
- [`brand/`](brand/README.md) — voice, colour, type, the root-spiral mark and the wordmark; `brand/tokens.json` holds the values.

## [`design/`](design) — proposals and specs

Work that is planned, in flight, or specified but not fully executed. These steer decisions; they do
not describe what exists today. When one lands, its content moves into a reference doc and the plan
moves to [`archive/`](archive).

- [Indexed tables: convenient construction and traversal](design/indexed-tables-language-proposal.md)
  — proposal for typed table declarations, scoped builders, and acyclic folds, with alternatives
  and explicit backend constraints; not implemented language syntax.
- [Mixed evaluation and required results](../tracker/decisions/2026-10-25-mixed-evaluation-preserves-required-results.md)
  — accepted language-design direction for eager, on-demand and deferred computation, including
  omission of unnecessary faults; implementation and unresolved fault/termination details remain tracked.
- [Reference site guidance](design/reference-site-guidance.md)
  — target structure, page contents, and qualities for the public language reference; the
  execution plan is [`plans/reference-site-plan.md`](../plans/reference-site-plan.md).
- [Checked optimizer transformations](design/checked-optimizer-pipeline.md)
  — rule-specific validation and parallel implementation plan, including migration to the accepted
  evaluation contract without treating current compiler behavior as already verified.

## [`reports/`](reports) — assessments, surveys, harvests

Frozen at the moment they were written. Read them for the analysis, never for current state: a
report is stale by construction. Never edit one to bring it up to date — write a new one.

- [`Interpreter turnaround and first law results (2026-10-17)`](reports/interpreter-turnaround-2026-10-17.md)
  — edit-to-verdict times for the interpreter and the generate-build-run path on the STEP design
  folder, the law classification, and the interpreter defects found.
- [`Plato standard-library comparative study (2026-08-04, revision 2)`](reports/plato-stdlib-comparative-study-2026-08-04-r2.md)
  — expanded comparison adding geometry3Sharp, Unity, Unreal Engine, Godot, and Blender
  to the numerical, geometry, CAD, symbolic-numeric, and graphics-runtime study.
- [`Plato standard-library comparative study (2026-08-04)`](reports/plato-stdlib-comparative-study-2026-08-04.md)
  — earlier expansion covering the OCCT CAD kernel, Wolfram's symbolic-numeric region
  system, and the Three.js browser graphics runtime; retained as a frozen snapshot.
- [`Plato standard-library comparative study (2026-08-03)`](reports/plato-stdlib-comparative-study-2026-08-03.md)
  — initial comparison with numerical, graphics-math, computational-geometry, mesh-processing,
  and planar-geometry libraries; retained as the earlier frozen snapshot.

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
