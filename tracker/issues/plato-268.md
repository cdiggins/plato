---
id: plato-268
title: Pick canonical type-class keyword: interface vs concept vs trait
type: idea
status: idea
priority: "?"
effort: "?"
risk: "?"
area: plato
sprint: 
created: 2026-07-28
closed:
links: [plato-261, plato-230, plato-263]
---

> **Outcome (terminology pass, 2026-08):** the canonical user-facing keyword is now interface with I* names; concept remains a parse alias. The debate body below is historical (it compares the two spellings as they stood when this idea was filed).

## Idea

Decide the **canonical user-facing name** for Plato's type-class construct among `interface`, `concept`, and `trait`. Explicitly reject `type-class` / `typeclass` as the primary term — accurate but too jargon-heavy and confusing for the intended audience. Today `interface` and `concept` are aliases ([plato-261](plato-261.md)); the dual vocabulary already splits stdlibs and docs.

## Assumptions

- The language keeps one construct (Haskell-style type classes / C# interfaces-with-Self), only the spellings/docs need a winner.
- Forward stdlib (`plato-src-v3`) already prefers `concept`; shipping `plato-src` uses `interface` + `I`-prefix.
- Renaming folders ([plato-263](plato-263.md)) is separate; this is the keyword/docs decision.

## Design decisions

- **Winner** — `concept` (C++20-ish, matches v3) vs `interface` (C#/familiar, matches shipping) vs `trait` (Rust/Scala familiarity).
- **Loser handling** — keep as forever-alias vs deprecate with warning vs remove after migration.
- **Naming convention coupling** — does `concept` imply drop `I`-prefix (already true in v3)? Does `interface` imply keep it?
- **Docs term** — even if both parse, which word appears in the language reference and error messages?

## Related

- [plato-261](plato-261.md) — semantics doc already notes alias and uses `concept` as forward direction.
- [plato-230](plato-230.md) — v3 corpus committed to `concept`.
- [plato-263](plato-263.md) — stdlib folder rename; recipe-based names may mirror this choice.

## Approaches

Short term: ADR that picks one word for all new docs/errors; keep parse alias.
Long term: migrate shipping stdlib keywords (and maybe `I`-prefixes) to match; optionally soft-deprecate the alias.
Adjacent: error-message wording pass once the term is fixed.

## Case against

- **Alias is fine.** Forcing a winner creates migration churn for little semantic gain.
- **Audience split.** C# Studio users hear "interface"; geometry/math users may prefer "concept"; Rust people want "trait" — any pick alienates someone.
- **`concept` overload.** C++ concepts are similar-but-not-identical; name collision in search/docs.
- **`trait` overload.** Rust traits bring orphan-rule / coherence baggage Plato doesn't share.

**Verdict: pursue** — pick one canonical *documentation and error* term now (likely `concept` given v3 + plato-261), keep parse aliases until a deliberate deprecation. Drop only if we explicitly decide "permanent dual synonym" in an ADR.

## Bedrock

Strengthens the **one construct ↔ one word** invariant in the language reference, diagnostics, and agent guides — reduces "are interfaces different from concepts?" thrash. **Verdict: simplest-along-the-grain** — ADR + docs/error wording; must NOT remove the parse alias or mass-rewrite `plato-src` in the same change.

## Done means

- [ ] ADR or issue decision: winner + alias policy
- [ ] Language reference / agent docs use only the winner (alias noted once)
- [ ] New diagnostics/examples use the winner

## Simplest possible implementation

Write a one-page ADR: choose `concept` (or rival), document why not the others / not type-class, keep lexer aliases.

Pros: cheap; unblocks consistent teaching  
Cons: doesn't fix existing `interface` corpus until migration
