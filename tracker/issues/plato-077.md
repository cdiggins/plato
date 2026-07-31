---
id: plato-077
title: Sum types + pattern matching in Plato (RFC first)
type: idea
status: idea
priority: "?"
effort: "?"
risk: "?"
area: plato
sprint: 
created: 2026-07-19
closed:
links: [submodules/Plato/docs/plato-overview.md, tracker/issues/plato-076.md]
---

## Idea

Add discriminated unions + pattern matching to Plato, RFC-first. The design doc
decides: declaration syntax, how the monomorphizer lowers unions on the C#
target (tagged readonly struct vs class hierarchy — the zero-boxing goal pushes
toward tagged struct), match syntax, and exhaustiveness checking in the type
checker. Implementation is a separate, larger increment after the RFC.

Motivation stands without any UI work: plato-overview.md ("What it's bad at")
already names partiality the weakest part of the type story — `CanInvert:
Boolean` conventions, ray-miss junk values, C#-side `Tuple2<T, Boolean>` hacks.
Also the top addable language gap for [[plato-076]] (Gratify kernel intents).

## Related

- [plato-076](plato-076.md) — spin-off origin; kernel port wants this but can
  limp without (tagged Integer hack).
- [plato-079](plato-079.md) — the stdlib cleanup this unblocks (hard-blocked on
  this issue).
