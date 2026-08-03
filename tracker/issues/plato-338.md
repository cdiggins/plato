---
id: plato-338
title: Index slice sugar xs[a..b]
type: idea
status: idea
priority: "?"
effort: "?"
risk: "?"
area: plato
sprint: 
created: 2026-07-30
closed:
links: [plato-333, plato-319, plato-303, plato-339, submodules/Plato/stdlib/intrinsics-arrays.library.plato, submodules/Plato/docs/SEMANTICS.md]
---

## Idea

Add indexing sugar `xs[a..b]` for a contiguous sub-range of an indexable/array —
desugaring to today’s `Slice` / `SubArray` (or a single chosen intrinsic) instead of
writing `xs.Slice(a, b)`. Same brackets as element indexing (`x[i]` ≡ `At(x, i)` per
language semantics §6); the `a..b` form is the range operand. Echoes C# range syntax
in spirit, but must pick Plato’s half-open vs closed convention explicitly.

## Assumptions

- Authors write enough `Slice`/`SubArray`/`Drop`/`Take` chains that bracket range
  syntax pays for a grammar + checker change.
- `..` inside `[]` can be parsed without breaking existing `x[i]` or array literals
  `[a, b, c]`.
- Half-open vs closed is decided up front (see plato-333); silent mismatch with
  `IntegerInterval` would be worse than no sugar.
- Sugar stays a desugar to an existing verb (`Slice` or `SubArray`), not a new
  runtime interface.

## Design decisions

- **Bounds** — half-open `[a..b)` ≡ `Slice(xs, a, b)` vs closed `[a..=b]` / inclusive
  end. Half-open matches CSR face ranges and C# `..`; closed matches naive “from a
  through b”. Document one; do not inherit IntervalLike’s closed bug (plato-333).
- **Open ends** — support `xs[a..]`, `xs[..b]`, `xs[..]` as Take/Drop/identity, or
  require both endpoints. Open ends are high value; more grammar/cases.
- **Slice vs SubArray** — `Slice(from, to)` vs `SubArray(from, count)`. Sugar should
  pick one desugar target; the other stays a library call.
- **Types** — only `Array` / `Sliceable`, or any `At`+`Count` indexable? Broader is
  nicer; needs an interface obligation.
- **Relation to `IntegerInterval`** — does `a..b` alone (outside brackets) become an
  interval literal later? Out of scope unless cheap; don’t invent free-standing
  `..` in this idea.

## Related

- [plato-333](plato-333.md) — `IntegerInterval` half-open docs vs closed
  `IntervalLike` bodies; any `a..b` sugar must not amplify that bug.
- [plato-319](plato-319.md) — dropped Eval-via-`[]` idea; reinforces that `[]` is
  reserved for `At` / indexing, so range-inside-brackets is a compatible extension
  of indexing, not Eval.
- [plato-303](plato-303.md) — CSR row slices are the main consumer pattern for
  half-open index ranges.
- [plato-339](plato-339.md) — from-end index sugar; may compose as `xs[0..^1]`.
- [intrinsics-arrays.library.plato](../../submodules/Plato/stdlib/intrinsics-arrays.library.plato) —
  `Slice` / `SubArray` / `Take` / `Drop` already exist.
- [../../docs/SEMANTICS.md](../../docs/SEMANTICS.md) —
  §6: `x[i]` ≡ `At(x, i)` only today.

## Approaches

Short term: (1) desugar `xs[a..b]` → `Slice(xs, a, b)` half-open only, both ends
required; (2) add open-end forms once (1) works.
Long term: optional free-standing range expressions feeding Sliceable APIs; align
with a fixed IntegerInterval story.
Adjacent: from-end indices in slice bounds (`xs[^2..^0]`) — see plato-339; can
compose later.

## Bedrock

Extends the existing index-sugar seam (`[]` → `At`) in the parser/semantics so
sub-ranges are the same surface as element access, targeting the `Sliceable` /
array-intrinsic boundary. Future CSR and mesh face-range code gets cheaper call
sites without new collection types. Verdict: **simplest-along-the-grain** —
desugar to `Slice` only; do NOT add a parallel range type or change Slice
semantics in the same change.

## Done means

- [ ] `xs[a..b]` parses and type-checks as the chosen Slice/SubArray form
- [ ] Semantics doc §6 states the desugar and half-open/closed rule
- [ ] At least one forward-stdlib call site (or law) uses the sugar
- [ ] Open-end forms either work or are explicitly deferred in the issue

## Simplest possible implementation

Parser: allow `Expr .. Expr` only inside `[]`; lower to `Slice(xs, a, b)`. No
open ends, no `^=` variants, no free-standing `a..b`.

Pros:
- Tiny surface; reuses Slice; matches how people already read index ranges
Cons:
- Grammar change; must settle half-open vs closed first; open ends deferred means
  `Drop`/`Take` stay verbose
