---
id: plato-300
title: Prefer arithmetic operators over Add/Multiply method calls in Plato
type: idea
status: done
priority: "?"
effort: "?"
risk: "?"
area: plato
sprint: 
created: 2026-07-29
closed: 2026-07-29
links: [submodules/Plato/stdlib/STYLE_GUIDE.md, submodules/Plato/stdlib/CONVENTIONS.md, submodules/Plato/docs/plato-for-agents.md, submodules/Plato/docs/plato-language-semantics.md, submodules/Plato/stdlib/deformations.library.plato, tracker/issues/plato-299.md]
---

## Idea

In Plato, `+` / `*` / `-` / `/` are sugar for the well-known names `Add` / `Multiply` /
`Subtract` / `Divide` (see `plato-language-semantics.md` §6). Forward stdlib bodies often
spell arithmetic as method chains (`origin.Add(v.Multiply(t))`) even when the same file
already uses operators for scalars (`1.0 + self.Rate * t`). Prefer the operator spelling for
ordinary arithmetic so formulas read as math; keep the method spelling only when it is the
definition site (`Add(a, b) => …`) or when chaining/UFCS genuinely clarifies.

## Assumptions

- Operator and method forms resolve to the same overload group — no semantic difference.
- Agents copy local style; method-heavy exemplars in `stdlib/*.library.plato` teach the worse
  habit.
- `stdlib/CONVENTIONS.md` is the SoT agents open before writing stdlib ([plato-299](plato-299.md)
  already owns expanding it with authoring rules).

## Design decisions

- **Where the rule lives** — one bullet in CONVENTIONS authoring section (via plato-299) vs a
  standalone AUTHORING.md. Prefer CONVENTIONS so there is one place.
- **Scope of cleanup** — document-only first vs also rewrite existing forward-stdlib bodies
  (`deformations`, `lines`, triangles, …). Exemplars matter for AI; a sweep is optional debt.
- **Enforcement** — convention + agent docs only vs a future LINT that flags `.Add(`/`.Multiply(`
  outside definition sites. Lint is optional follow-up; do not block the convention write.

## Related

- [plato-299](plato-299.md) — owns CONVENTIONS authoring/API section; this rule should land
  there as a concrete Plato idiom.
- [plato-295](plato-295.md) — sibling style preference (array literals); same landing zone.
- `plato-language-semantics.md` §6 — operators are names; table is normative.
- `stdlib/deformations.library.plato` — mixed style that triggered the observation.

## Approaches

Short term: add a CONVENTIONS (or plato-for-agents) rule with a before/after example; point
CLAUDE.md / plato-for-agents at it; optionally rewrite `deformations.library.plato` as the
pilot exemplar.

Long term: sweep forward-stdlib library bodies; optional LINT for method-form arithmetic.

Adjacent: unary `-` / `Negative`; comparison operators vs `Equals`/`LessThan` method form
(same sugar table — decide whether the rule covers them too).

## Case against

- **Method chains match UFCS fluency.** Plato is method-call heavy (`p.Lerp`, `o.Dot`);
  forcing operators creates a dual dialect inside one expression.
- **Left-to-right chaining can be clearer for long pipelines** than precedence-sensitive
  `a + b * c` when types are unfamiliar.
- **AIs already emit both**; without exemplar cleanup, a doc rule alone may not stick —
  cleanup cost across many `.library.plato` files may exceed the readability win.
- **Definition sites must stay named** (`Add`, `Multiply`); a blunt lint or blanket rewrite
  risks false positives.

**Verdict: pursue** — document the preference (operators for ordinary arithmetic) and fix a
small pilot exemplar; park a full stdlib sweep and LINT until plato-299's authoring section
exists and the pilot looks good in review.

## Bedrock

Strengthens **`stdlib/CONVENTIONS.md` as the single steering surface for agents writing
Plato**: one explicit arithmetic-spelling idiom next to other authoring rules, with
semantics docs remaining the language reference (not the style guide). **Verdict:
simplest-along-the-grain** — write the rule + one exemplar rewrite; must NOT invent new
operator semantics or mass-rewrite shipping `stdlib-legacy` in the same change.

## Done means

- [x] Authoring convention recorded (CONVENTIONS.md and/or plato-for-agents) with before/after
  — landed under `stdlib/STYLE_GUIDE.md` § Arithmetic spelling (plato-299 split authoring
  out of CONVENTIONS; CONVENTIONS points agents at STYLE_GUIDE for idioms)
- [x] Rule linked from agent entry points (`plato-for-agents.md` / CLAUDE.md) so AIs see it
  — `plato-for-agents.md` § Rules when editing Plato item 6 → STYLE_GUIDE Arithmetic spelling
- [x] At least one mixed-style library body rewritten as exemplar (candidate: deformations)
  — full forward-stdlib library-body sweep (deformations + lines/planes/patches/triangles/
  spatial/planar/polygons/transforms/intervals/…); ~129 method-form call sites → operators
- [x] Scope of Equals/comparison method form explicitly decided (include or defer)
  — **Deferred.** Arithmetic only in this change. Comparison ops (`Equals` / `LessThan` /
  … vs `==` / `<` / …) stay out of scope until a separate decision; STYLE_GUIDE notes the deferral.

## Notes (2026-07-29 session)

- `stdlib-legacy/`: no body arithmetic method-form hits (only a List builder `Add` comment).
- Left alone: comment examples, `Multiply(a, b) => …` definition signatures, collection
  mutator `Add` docs in `primitives-builders.plato`.
- Lint: `lint submodules\Plato\stdlib` → 0 parse / 0 symbol resolution (exit 0).
- Nothing committed (per mission).

## Simplest possible implementation

Add 5–10 lines under CONVENTIONS (or, until plato-299 lands, under "Rules when editing Plato"
in `plato-for-agents.md`): prefer `a + b * t` over `a.Add(b.Multiply(t))`; keep `Add`/`Multiply`
for defining the ops. Rewrite `Eval(Taper3D)` / `Eval(Taper2D)` as the exemplar.

Pros: agents already read those docs; zero compiler change; semantics unchanged.
Cons: existing method-heavy bodies keep teaching the old habit until swept.
