---
id: plato-339
title: From-end index sugar xs[^n]
type: idea
status: idea
priority: "?"
effort: "?"
risk: "?"
area: plato
sprint: 
created: 2026-07-30
closed:
links: [plato-319, plato-338, submodules/Plato/docs/plato-language-semantics.md, submodules/Plato/stdlib/intrinsics-arrays.library.plato]
---

## Idea

Add from-end indexing sugar so the last element (and near-end indices) need not
repeat `xs.Count - 1`. Candidate spelling `xs[^1]` meaning “one from the end”
(C#-style: `^1` → last, `^2` → second-to-last), or an equivalent compact form.
Today call sites write `xs[xs.Count - 1]`, `xs[xs.Count - 1 - i]` on reverse
walks, grids’ `ColumnCount - 1`, spline knot ends, etc.

## Assumptions

- End-relative access is common enough in geometry/stdlib to justify syntax (or
  at least a named helper) over `Count - 1` arithmetic.
- `^` is available in index position without stealing XOR / other operators (Plato
  operator table today has no `^`; confirm before committing the glyph).
- Sugar desugars to ordinary `At` with a computed index — no new runtime index
  type required for v1 (unlike C# `System.Index`), unless we want composability
  with slice bounds (plato-338).
- `-1` remains the typed-index “none” sentinel elsewhere; from-end sugar must not
  look like that sentinel.

## Design decisions

- **Spelling** — `xs[^1]` (C#) vs `xs[-1]` (Python) vs `xs.Last` / `AtFromEnd(xs, n)`.
  Negative literals collide with “sentinel −1” folklore in mesh indices; `^n` or
  a named function avoids that. `Last` covers only n=1.
- **Origin** — is `^1` the last element (C#) or `^0`? C# uses `^1` = last;
  document hard. Off-by-one here is catastrophic.
- **Sugar vs library** — language form vs `FromEnd(n)` / `AtFromEnd(xs, n)` only.
  Library-only is smallest; language form wins if slices want `xs[^2..]` later.
- **Composable with slices** — if plato-338 lands, do bounds accept `^n`
  (`xs[0..^1]` = drop last)? Powerful; couples the two ideas.
- **Typed indices** — does `faces[^1]` yield `FaceIndex` or `Integer`? Prefer
  preserving element/`At` result type; index expression type may stay Integer.

## Related

- [plato-338](plato-338.md) — `xs[a..b]` slice sugar; natural composition
  (`xs[..^1]`) if both exist.
- [plato-319](plato-319.md) — dropped Eval-via-`[]`; keeps `[]` as At/indexing
  surface — from-end is an index expression, not Eval.
- [plato-language-semantics.md](../../submodules/Plato/docs/plato-language-semantics.md) —
  §6 indexing = `At(x, i)` only.
- Forward stdlib already repeats `Count - 1` (e.g. reverse rings in
  `meshes-polygon-corners.library.plato`, `solids-csg-boolean.library.plato`,
  grid last-cell accessors in `collections-grids.library.plato`).

## Approaches

Short term: (1) library `AtFromEnd(xs, n)` / `Last(xs)` with no grammar change;
(2) `xs[^n]` → `At(xs, xs.Count - n)` once spelling is settled.
Long term: from-end forms inside slice bounds; optional first-class Index value.
Adjacent: `xs.Last` as the n=1 special case if full `^` sugar is deferred.

## Bedrock

Strengthens the index-sugar seam (`[]` → `At`): end-relative positions become
ordinary index expressions, so reverse walks and “last knot/cell” stop
hand-rolling `Count - 1`. Keeps `-1`-as-none on typed indices distinct from
from-end access. Verdict: **simplest-along-the-grain** — prefer desugar to
`At(xs, Count - n)` (or a tiny library helper first); do NOT introduce a C#-like
`Index`/`Range` runtime pair until slice sugar needs it.

## Done means

- [ ] Chosen spelling documented (including whether `^1` or `^0` is last)
- [ ] Last-element and second-to-last access work on Array/Indexable
- [ ] Semantics (or STYLE_GUIDE) warn that this is not the −1 none-sentinel
- [ ] At least one stdlib reverse/last-cell site migrated or a law covers it

## Simplest possible implementation

Add `Last(xs) => xs[xs.Count - 1]` and `AtFromEnd(xs, n) => xs[xs.Count - n]` in
collections; skip grammar. Promote to `xs[^n]` only if call-site noise remains.

Pros:
- No parser work; clear names; easy to delete if unused
Cons:
- Less C#-familiar; won’t compose inside `xs[a..b]` without a follow-up; still
  verbose vs `[^1]`
