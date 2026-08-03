---
id: plato-261
title: Write succinct accurate Plato language semantics doc
type: idea
status: done
priority: "?"
effort: M
risk: low
area: plato
sprint: 
created: 2026-07-28
closed: 2026-07-28
links: [submodules/Plato/docs/plato-for-agents.md, submodules/Plato/docs/plato-overview.md, submodules/Plato/README.md, submodules/Plato/docs/compiler-pipeline.md, submodules/Plato/docs/type-checker-handoff.md, submodules/Plato/docs/plato-sum-types-design-2026-07-27.md, plato-257]
---

## Idea

> **Note:** filed when forward vocabulary preferred the `concept` keyword; shipping canonical term is now `interface` (`I*`), with `concept` still a parse alias.

Add a single, normative **language semantics** document for Plato: succinct and accurate enough that agents and humans can answer "what does this construct mean?" without reading the compiler or inferring from examples. Today [`plato-for-agents.md`](../../submodules/Plato/docs/plato-for-agents.md), the submodule [`README.md`](../../submodules/Plato/README.md), and [`plato-overview.md`](../../submodules/Plato/docs/plato-overview.md) cover design intent and the three constructs well, but they are not a semantics reference — evaluation, scoping, overload/constraint resolution, coercions, and interface/`Self` meaning remain implicit in code and in compiler handoffs ([`compiler-pipeline.md`](../../submodules/Plato/docs/compiler-pipeline.md), [`type-checker-handoff.md`](../../submodules/Plato/docs/type-checker-handoff.md)).

## Assumptions

- The implemented language (what the compiler + type checker accept and emit) is the source of truth; the doc describes *current* semantics, not aspirational features. **Correction (2026-07-28 review):** sum types + `match` are IN the language as of plato-232 (v1: monomorphic sums, exhaustive match, no default arm — `SumTypeChecker` CHK300–307, used throughout `plato-src-v3`), so the doc must cover them as shipping semantics. Affine types (`unique` keyword) parse but are NOT implemented — explicitly marked "reserved, not in language".
- Two stdlib generations share ONE language: `plato-src` (V2 recipe, `interface` keyword, `I`-prefix names) and `plato-src-v3` (`concept` keyword, no prefix, sum types). The doc is library-agnostic; it notes `concept`/`interface` are aliases and uses `concept` (the forward direction) in examples.
- The grammar (parakeet `PlatoGrammar`) is deliberately more permissive than the language (it parses `while`/`throw`/`try`/`is`/`as` etc. for recovery and C#-familiarity). The doc is normative over the *checked* language — the subset the pipeline compiles and the stdlib uses — and says so explicitly.
- Audience is agents + library authors first; compiler implementers still use the pipeline/handoff docs.
- Succinct beats encyclopedic: prefer one short canonical file over expanding overview/README into a second marketing pass.

## Design decisions

- **Home path** — new `submodules/Plato/docs/SEMANTICS.md` vs. expanding `plato-for-agents.md`. Separate file keeps agent ops/codegen guidance distinct from normative meaning; agents doc then links to it.
- **Normative vs descriptive** — "this is what the language means" (normative for authors) vs. "this is what the compiler currently does" (tied to TIR passes). Prefer normative wording checked against the checker; note known gaps where behavior is underspecified.
- **Depth of type system** — surface rules (implements, `Self`, library UFCS, tuple construction, conversions, operators) vs. full constraint-solver tiers from `compiler-pipeline.md`. Surface rules first; deep solver detail stays in the handoff docs with a one-paragraph pointer.
- **Examples** — minimal invented snippets vs. citations from `plato-src`. Prefer tiny self-contained examples plus one real stdlib cite when accuracy needs it.

## Related

- [submodules/Plato/docs/plato-for-agents.md](../../submodules/Plato/docs/plato-for-agents.md) — current agent entry; should link to the new semantics doc, not absorb it.
- [submodules/Plato/docs/plato-overview.md](../../submodules/Plato/docs/plato-overview.md) — design rationale / lineage; keep as "why", not "what it means".
- [submodules/Plato/README.md](../../submodules/Plato/README.md) — pitch + taste; link out for semantics.
- [submodules/Plato/docs/compiler-pipeline.md](../../submodules/Plato/docs/compiler-pipeline.md) / [type-checker-handoff.md](../../submodules/Plato/docs/type-checker-handoff.md) — implementer truth for constrain/solve/elaborate; not author-facing semantics.
- [plato-257](plato-257.md) — geometry textbook; different audience (learners of math via Plato), not a language reference.
- [submodules/Plato/docs/plato-sum-types-design-2026-07-27.md](../../submodules/Plato/docs/plato-sum-types-design-2026-07-27.md) — normative source for sum/match v1 rules (CHK300–307); semantics doc summarizes, does not duplicate.
- `submodules/Plato/plato-src-v3/` + `plato-test-sum/` — live usage evidence for `concept` syntax and sum types; spot-check fixtures.

## Approaches

Short term:
1. One new markdown file (~2–4 pages): constructs, purity rules, name resolution / UFCS, typing of interfaces & libraries, construction/coercion conveniences, explicit non-features.
2. Thin rewrite of the "Language (three constructs)" section in `plato-for-agents.md` into a link + 5-line summary pointing at the semantics doc.
3. Cross-check each claim against a small set of `plato-src` / lint examples so the doc cannot invent behavior the compiler rejects.

Long term: keep the doc as the single place language RFCs (e.g. sum types) must update when they land; optional extract of a formal grammar appendix later.

Adjacent ideas worth their own issue:
- Formal grammar / syntax reference separate from semantics.
- "Semantics vs emit" per-backend notes (what GLSL/C++ cannot represent) as a short sibling doc.

## Bedrock

Strengthens the Plato authoring seam: one normative file agents and humans treat as the language contract, so `plato-for-agents.md` stays ops/codegen and compiler handoffs stay implementation. Makes future language RFCs cheaper (edit one semantics file + link from plans). **Verdict: simplest-along-the-grain** — simple version must NOT duplicate the type-checker handoff or rewrite the README pitch; it must NOT claim unimplemented features as current semantics.

## Done means

- [x] `submodules/Plato/docs/SEMANTICS.md` exists and covers: the three constructs, sum types + `match` (v1 rules: monomorphic, exhaustive, positional binders, no default arm), purity/restrictions, UFCS & name/overload resolution (the exact<generic<concept<conversion tiers, author view), interfaces/`Self`/monomorphization, construction/conversion/operator rules (incl. the operator-name table and `_: Type` statics), grammar-vs-checked-language distinction, explicit non-features (affine `unique` reserved; no generic sums; no doubles; no modules)
- [x] Claims are accurate against current compiler acceptance (spot-checked with `Plato.CLI lint` on a scratch fixture exercising the claimed conveniences, plus the existing `plato-test-sum/` fixtures; no aspirational features presented as shipping). The spot check caught and the doc now records two nuances: tuple expressions resolve via the stdlib `TupleN` types, and unmet `implements` obligations are LINT001 + a throw stub, not compile errors.
- [x] Works for both stdlib generations: `concept`/`interface` alias noted; examples valid in `plato-src-v3` style
- [x] `plato-for-agents.md` (and README) link to it as the semantics entry
- [x] Doc stays succinct (~300 lines, readable in one sitting; solver depth stays in the handoff docs)

Done 2026-07-28 — Plato commit `55c109f`.

## Simplest possible implementation

Write one new markdown file by consolidating and tightening what README + `plato-for-agents` + overview already say, then fill the gaps (resolution, coercions, what is *not* in the language) by reading `compiler-pipeline.md` and verifying with a few lint/compile examples. Add two inbound links; leave overview/handoff docs otherwise untouched.

Pros:
- Closes the documented gap without a docs site or new tooling
- Gives agents a single authoritative file
- Low risk: documentation-only

Cons:
- Can go stale when the checker changes unless linked from language-change PRs
- Temptation to over-specify solver internals and lose succinctness
- Does not replace interactive learning ([plato-257](plato-257.md))

## Case against

- **Existing docs may be "good enough."** Agents already ship work from `plato-for-agents.md`; another doc risks duplication and drift across three entry points.
- **"Accurate" is expensive.** True semantics live in the constraint solver; a short doc that is wrong in edge cases is worse than no doc. Requires careful verification, not a rewrite of marketing prose.
- **Language is still moving** (sum types, V3 library, emitter flags). A normative doc written now may need frequent edits or become a false contract.
- **Wrong audience mix.** Mixing author semantics with agent ops in one file recreates the current muddle; splitting creates more files to maintain.

**Verdict: pursue** — the gap is real (conceptual overview ≠ semantics), effort is bounded, and a single short normative file with inbound links is the right shape. The one parking condition (a large in-flight language surface change) is resolved: sum types landed 2026-07-27 (plato-232), so writing now captures them instead of being invalidated by them.
