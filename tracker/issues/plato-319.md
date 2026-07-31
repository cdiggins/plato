---
id: plato-319
title: Eval application sugar: (args) vs [args]
type: idea
status: dropped
priority: "?"
effort: "?"
risk: "?"
area: plato
sprint: 
created: 2026-07-29
closed: 2026-07-29
links: [submodules/Plato/stdlib/functional.concepts.plato, submodules/Plato/stdlib/curves.concepts.plato, submodules/Plato/docs/plato-language-semantics.md, submodules/Plato/docs/plato-for-agents.md]
---

## Idea

Add syntax sugar so procedural values read like math: applying a curve/surface/field/easing
writes as a postfix argument list instead of an explicit `.Eval(..)`. Two candidate forms:

1. **`(args)`** — `curve(t)` / `field(p)` desugars to `Eval(curve, t)` / `curve.Eval(t)`.
2. **`[args]`** — `curve[t]` / `field[p]` desugars to the same `Eval` call.

Today `Eval` is already the unifying verb on `Procedural<TDomain, TRange>`
(`stdlib/functional.concepts.plato`); curves/surfaces/fields inherit it. Indexing already
exists as a *different* well-known sugar: `x[i]` ≡ `At(x, i)`
(`docs/plato-language-semantics.md` §6). Operators are the precedent for privileged names
(`Add` → `+`).

## Assumptions

- Authors write enough `c.Eval(t)` / `f.Eval(p)` that the noise matters (stdlib already has
  100+ `Eval` overloads).
- `Eval` stays the *only* application verb; sugar does not invent `Apply` / `Invoke`.
- Call/index resolution and the forward type checker are stable enough that a new sugar does
  not amplify ambiguity bugs.
- One form wins — not both `(..)` *and* `[..]` for `Eval`.

## Design decisions

- **Sugar form — `(args)` vs `[args]`.**
  - `(args)`: matches math and Scala/Kotlin/C++ callable values; leaves `[]` free for `At`.
    Cost: clashes with Plato’s primary free-function call form `Name(args)` — need a crisp
    rule (bound function/type-ctor vs value expression).
  - `[args]`: visually distinct from calls; some languages blur sample/index (MATLAB/Julia).
    Cost: **`[]` is already `At`** — either steal it from indexing, overload by type
    (discrete `At` vs continuous `Eval`), or teach two meanings of the same brackets.
- **Desugar target** — always `Eval(receiver, args…)`, never `EvalAtTime` / `EvalInverse` /
  `HornerEval` / `EvalNode` / etc.
- **Resolution order for `(args)`** — if left side binds as a free function or type-named
  constructor, that wins; only otherwise rewrite to `Eval`. Document beside operators.
- **Multi-arg** — `s(u, v)` / `s[u, v]` → `Eval(s, u, v)` if such overloads exist; single
  domain value (`UV`) remains the preferred stdlib shape.
- **Empty args** — `f()` / `f[]` rejected or meaningless; `Eval` always takes a domain input.
  (No-arg members already omit parens: `v.Magnitude`.)

## Related

- `submodules/Plato/stdlib/functional.concepts.plato` — `Procedural.Eval` is the concept this
  sugar would privilege.
- `submodules/Plato/stdlib/curves.concepts.plato` — curves as `Procedural<Number, Point*>`.
- `submodules/Plato/docs/plato-language-semantics.md` §6 — operators-as-names; `x[i]` ≡ `At(x, i)`.
- `submodules/Plato/docs/plato-for-agents.md` — uniform call syntax `a.b(c)` ≡ `b(a, c)`.

## Approaches

Short term: none — keep writing `.Eval(..)`; document the idea only.
Long term: pick one sugar, implement as parse/desugar to `Eval` (parallel to `[]`→`At` and
operators), update semantics doc + a handful of stdlib call sites as exemplars.
Adjacent: if `[args]` for `Eval` is rejected, consider whether *sampling* of discrete
representationsables and continuous procedurals should ever share a vocabulary (separate idea —
do not fold into this issue).

## Case against

- **`(args)` fights the existing call syntax.** Plato’s main form is already `Name(args)`.
  Value-application sugar forces a second lookup story and will confuse humans and tools
  whenever an identifier could be either a function or a `Procedural` local.
- **`[args]` fights the existing index sugar.** Semantics already fix `x[i]` ≡ `At(x, i)`.
  Reusing brackets for `Eval` either breaks arrays, or overloads `[]` by type so
  `xs[i]` and `curve[t]` look alike but mean different verbs — the opposite of “obvious usage.”
- **Uniformity tax.** Today every name is equal under UFCS; privileging `Eval` is fine only
  if treated like operators. Anything fuzzier is a second member system.
- **Low urgency.** `.Eval(t)` is short, searchable, and already the stdlib convention.
  Checker/emitter work (forward conformance, existentials) is higher leverage.
- **Teachability.** Magic callable/`[]` conventions (Scala `apply`, C# indexers-as-Eval)
  are a recurring footgun for newcomers.

**`(args)` vs `[args]` head-to-head:** prefer **`(args)`** if sugar ships — math notation,
leaves `At`/`[]` alone, matches “callable value.” Reject **`[args]`** for `Eval` unless the
language deliberately merges sample-and-index into one verb (a larger design, not this sugar).

**Verdict: park.** Worth keeping; pursue only after call/`At` resolution and `Procedural`
checking are boring. When revived, default proposal is `(args)` → `Eval`, not `[args]`.

## Bedrock

Strengthens the **well-known-name → surface-syntax** seam already used by operators and
`At`/`[]` (`plato-language-semantics.md` §6 + `Plato.AST/Operators.cs`): one more privileged
name (`Eval`) with a documented desugar, not a new evaluation model. **Verdict:
simplest-along-the-grain** — pure desugar to `Eval`; must NOT add `Apply`/`Invoke`, must NOT
overload `[]` for both `At` and `Eval`, must NOT change `Procedural`’s obligation set.

## Done means

- [ ] Semantics doc states the chosen sugar and its desugar to `Eval` (and explicitly that
      `[]` remains `At` if `(args)` wins)
- [ ] Parser/checker round-trip: `x(y)` (or chosen form) type-checks identically to `x.Eval(y)`
      for a `Procedural` witness
- [ ] Free-function / type-ctor call sites are unchanged (no accidental rewrite to `Eval`)
- [ ] At least one stdlib exemplar file uses the sugar; `.Eval` still accepted everywhere
- [ ] Lint/nav still resolve the underlying `Eval` symbol from sugar sites

## Simplest possible implementation

Desugar-only in the AST builder or early binder: postfix `(args)` on a non-call-bound
expression → `Eval(expr, args…)`. No new TIR node; emitters unchanged.

Pros:
- Tiny surface; reuses existing overload resolution for `Eval`
- Matches math; keeps `[]`/`At` coherent

Cons / risks:
- Call-vs-value disambiguation bugs
- Second special name to teach
- Temptation to also sugar `[args]` and collapse the `At`/`Eval` distinction
