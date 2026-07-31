---
id: plato-337
title: Rename MapRange to Map
type: idea
status: idea
priority: "?"
effort: "?"
risk: "?"
area: plato
sprint: 
created: 2026-07-30
closed:
links: [plato-304, plato-295, submodules/Plato/stdlib/intrinsics-arrays.library.plato, submodules/Plato/stdlib/STYLE_GUIDE.md, submodules/Plato/Plato.Intrinsics.V2/Integer.cs]
---

## Idea

Rename the intrinsic `MapRange(n: Integer, f: Function1<Integer, $T>): Array<$T>` to
`Map`, so building an array from a count reads as `n.Map(i => …)` — the same verb already
used for element-wise transform on `Array` / `Indexable` / `Array2D`. Interpretation:
overload `Map` by receiver type (Integer vs Array), not a wholesale rename that removes
array-`Map`.

## Assumptions

- Callers prefer one verb for “produce an array by applying a function”; the `Range` suffix
  is noise once the Integer receiver is visible.
- Plato overload resolution distinguishes `Map(Integer, …)` from `Map(Array, …)` cleanly
  at call sites (`count.Map(f)` vs `xs.Map(f)`).
- Forward stdlib is the primary rename surface; shipping `stdlib-legacy` / frozen V1 may
  stay as `MapRange` until a deliberate legacy migration, or rename in lockstep — pick one.
- ~65+ forward-stdlib references (meshes, polygons, kernels, Conway, …) plus V2 C#
  `Integer.MapRange` must move together with the intrinsic declaration.

## Design decisions

- **Overload vs new name** — overload `Map` on Integer (matches the idea) vs keep
  `MapRange` / add alias. Overload unifies vocabulary; alias softens migration but leaves
  two names forever unless deprecated.
- **Legacy / V2 runtime scope** — forward-only rename (intrinsic + stdlib + docs) vs also
  `Plato.Intrinsics.V2/Integer.cs` and any generated consumers. Forward-only is smaller;
  V2 drift confuses dual-runtime readers.
- **Docs / STYLE_GUIDE** — plato-295 taught “fixed arity → literal; variable arity →
  MapRange”. After rename, that convention becomes “→ Map”; update STYLE_GUIDE /
  LIBRARIES wording in the same change.
- **Ambiguous receivers** — anything typed as both countable and indexable must stay
  unambiguous (today `xs.Count.MapRange` is explicit). Confirm checker picks Integer
  overload for `.Count.Map(…)`.

## Related

- [plato-304](plato-304.md) — IArray→Plato port already documents Plato naming `Map` not
  `Select`; lists MapRange among intrinsics; rename aligns the Integer-count builder with
  that naming story.
- [plato-295](plato-295.md) — Done; preferred array literals over MapRange for fixed
  corners. Convention text still says MapRange — update when renaming.
- [stdlib/intrinsics-arrays.library.plato](../../submodules/Plato/stdlib/intrinsics-arrays.library.plato) —
  declares both `Map` (Array) and `MapRange` (Integer) side by side.
- [Plato.Intrinsics.V2/Integer.cs](../../submodules/Plato/Plato.Intrinsics.V2/Integer.cs) —
  C# `Repeat` still calls `n.MapRange`.

## Approaches

Short term: (1) rename intrinsic + mechanical stdlib sweep + STYLE_GUIDE; (2) optional
temporary `MapRange` alias that forwards to `Map` for one milestone, then delete.
Long term: one `Map` verb across count→array and array→array; fewer special-case names in
authoring docs and ports from LinqArray/`ReadOnlyList(count, f)`.
Adjacent: whether `Range(n)` (if it exists / returns indices) should stay distinct from
`n.Map(i => i)` — probably yes; not this issue.

## Bedrock

Strengthens the intrinsic array HOF seam in `intrinsics-arrays.library.plato`: one name
for “map a function over a domain,” domain carried by the first parameter’s type. Makes
future collection ports (plato-304) and authoring docs cheaper — no Map vs MapRange
table. Verdict: **simplest-along-the-grain** — rename + overload only; do NOT collapse
array-`Map` into MapRange semantics or change laziness/eager evaluation.

## Done means

- [ ] Intrinsic is `Map(n: Integer, f: …)`; no remaining `MapRange` declaration in forward stdlib
- [ ] Forward stdlib call sites and STYLE_GUIDE/LIBRARIES use `Map` for count→array
- [ ] V2 `Integer` helpers (at least `Repeat`) match the chosen runtime scope
- [ ] `check-stdlib-fast` (and scoped regen if V2/codegen touched) green

## Simplest possible implementation

Rename the declaration line, replace `MapRange` → `Map` across forward `.plato` + docs,
update V2 `Integer.cs` if in scope. No alias period.

Pros:
- One vocabulary; small mechanical diff; matches how people already say “map over 0..n-1”
Cons:
- Large touch surface in stdlib; brief confusion for readers of old issues/diffs; must
  verify overload resolution at `.Count.Map` sites
