---
id: plato-232
title: Sum types with exhaustive matching for Plato
type: feature
status: done
priority: p2
effort: L
risk: med
area: plato
sprint:
created: 2026-07-27
closed: 2026-07-27
links: [submodules/Plato/docs/plato-sum-types-design-2026-07-27.md, submodules/Plato/docs/plato-sum-types-v3-survey.md, submodules/Plato/plato-test-sum/README.md, tracker/issues/plato-077.md, tracker/issues/plato-079.md, tracker/issues/plato-230.md]
---

## Summary

Add discriminated sum types (tagged unions) with exhaustive `match` to Plato, targeting
C#. Executes the RFC idea [plato-077](plato-077.md): this issue is the design + spec +
test-corpus stage; the parser/AST front end (wave 2) and the stdlib migration (wave 3)
are the implementation increments that follow. Removes the language's biggest partiality
gap — the `XxxKind` + conditionally-valid-fields hand-encoding — and unblocks the stdlib
`Option`/`Result` cleanup ([plato-079](plato-079.md)).

## Design (fixed)

- **Declaration:** `type` gains an `=` case-list body as an alternative to the braced
  field body: `type PathSegment2D = Move(EndPoint: Point2D) | ... | Close;`. Payload-free
  cases (enums) omit parens: `type FillRule = NonZero | EvenOdd;`.
- **Match:** an *expression*, exhaustive, positional binders in case-field declaration
  order, **no default arm** in v1. Errors name the sum type and the missing / unknown /
  duplicated cases.
- **Backend (C#):** one `readonly partial struct` — `int Kind` tag (0-based, declaration
  order) + flattened `Case_Field` fields; private all-fields ctor; one static factory per
  case that zeroes the inactive fields (so struct equality over tag+fields is
  well-defined). `match` lowers **during elaboration** to a tag-conditional chain of
  existing TIR nodes — **no new TIR node**, downstream passes untouched. Generated code
  compiles on net8.0 default LangVersion (C# 12; no `switch`-expression exhaustiveness
  dependency).
- **Diagnostics:** proposed CHK300–CHK307 (+ CHK320 for the non-C# writers rejecting
  sums). Full catalog with message texts in the design doc.
- **Generics:** supported iff the monomorphizer specializes sums per type argument with
  no unifier change (expected yes); else v1 restricts with CHK306. `option.plato` decides.

Full spec: [design doc](../../submodules/Plato/docs/plato-sum-types-design-2026-07-27.md).
Survey (115 `XxxKind` types, ~11 true sums vs ~100 enums, flagship-5, `40-paths` "after"):
[survey doc](../../submodules/Plato/docs/plato-sum-types-v3-survey.md).

## Scope

- **In (v1):** declaration + match syntax; exhaustiveness/name checking; C# tagged-struct
  emission; monomorphic (and, if cheap, generic) sums.
- **Out (v1):** GLSL/TS/Rust emission (CHK320 rejection); nested patterns; guards; default
  arm; **recursive sum types** (flat struct cannot contain itself); `unique`/affine
  interaction; per-case shared fields.

## Related

- [plato-077](plato-077.md) — the RFC idea this executes.
- [plato-079](plato-079.md) — stdlib partiality cleanup, hard-blocked on this.
- [plato-076](plato-076.md) — Gratify kernel port; names this as its top language gap.
- [plato-230](plato-230.md) — plato-src-v3, the survey's source corpus.

## Done means

- [x] Design doc landed: declaration + match syntax, static/dynamic semantics, C#
      emission contract (hand-written PathSegment2D struct), diagnostics catalog.
- [x] Test corpus authored — `plato-test-sum/` — 5 positive + 6 negative fixtures with the
      `// EXPECT: <code>` convention + README.
- [x] v3 survey: 115 `XxxKind` types classified (enum vs sum), flagship-5 recommended,
      `40-paths.plato` "after" drafted.
- [x] **Front end (wave 2):** `=` case-list + `match` parse and type-check; exhaustiveness
      diagnostics CHK300–CHK306 (+CHK320) fire on the negative corpus; positive corpus compiles to
      C# on net8.0 default LangVersion. *(Plato `1d3ed84` AST, `507de64` checking/lowering/emission;
      PlatoTests 126 → 142.)*
- [x] **Migration (wave 3):** the flagship five migrated (`PathSegment2D`, `Paint`,
      `MaskSource2D`, `ScalarFieldNode2D/3D`, `WindowFunction`); `lint plato-src-v3` 0 parse/0 symbol
      errors (4584 → 4549), `plato-src` 193, PlatoTests 142/142. *(Plato `37ed5e9`.)*

## Closing note (2026-07-27)

Shipped: `type X = Case(...fields) | Case | ...;` declarations + exhaustive `match` expressions
(positional binders, no default arm) — grammar (parakeet), AST, symbol resolution, checker
(CHK300–CHK306, CHK320), match lowering during elaboration to a tag-conditional chain over
existing TIR nodes (no new node), and C#-only tagged `readonly partial struct` emission (int
`Kind` + flattened `Case_Field` fields + per-case factories + structural equality). Generics
restricted in v1 (CHK306). Test corpus `plato-test-sum/` + SumTypeCheckingTests (12) +
SumTypeCodegenTests (4, in-proc Roslyn). Wave-3 migrated the five flagship `plato-src-v3`
kind-pattern types to real sums; `26-fields` confirmed non-recursive (operands are `Integer`
node indices) so no substitution was needed.

Commits: parakeet `12aad78` (grammar); Plato `1d3ed84` (wave-1 AST), `507de64` (wave-2
checking/lowering/emission), `37ed5e9` (wave-3 flagship migrations + v3 README + survey).

Remaining (follow-up, not this issue): the ~100 pure-enum `XxxKind` types — the kind-pattern
sweep — plus the deferred features (bare constructors, generic sums, default arm, guards, nested
patterns, recursive sums, GLSL/TS/Rust support). Tracked as [plato-233](plato-233.md).
