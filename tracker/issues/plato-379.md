---
id: plato-379
title: Unify Indexable and MapLike into Indexable<TIndex, TValue>
type: idea
status: idea
priority: "?"
effort: "?"
risk: "?"
area: plato
sprint: 
created: 2026-08-01
closed:
links: [stdlib/foundation/collections-indexable.concepts.plato, stdlib/foundation/collections-containers.concepts.plato, stdlib/foundation/collections-indexable.library.plato, stdlib/foundation/collections-containers.library.plato, stdlib/foundation/primitives.plato, tracker/issues/plato-325.md, tracker/issues/plato-350.md]
---

## Idea

`Indexable<T>` (`collections-indexable.concepts.plato:25`, `Countable` + `At(x, Integer): T`)
and `MapLike<TKey, TValue>` (`collections-containers.concepts.plato:30`, `Countable` +
`ContainsKey` + `ValueAt`) describe the same operation — keyed lookup into a counted
collection — under two names. Proposal: collapse them into one general interface parameterised
on the key, and make integer-indexed collections a named instance of it.

```
interface Indexable<TIndex, TValue> inherits Countable { At(x: Self, index: TIndex): TValue; }
interface ArrayLike<T> inherits Indexable<Integer, T> { }
```

`MapLike` is deleted. `ValueAt` disappears (its name folds into `At`). `ContainsKey` is not
carried into the general interface — key-membership is a separable capability that belongs to a
future `DictionaryLike`, in the same way `Contains` lives on `SetLike` rather than on
`Indexable`.

The mechanism this rests on is ordinary Plato interface inheritance with substituted type
arguments: an implementer writes one function and satisfies the whole chain. Precedent is
already in the folder — `Field<TDomain, TValue> inherits Procedural<TDomain, TValue>`
(`fields.concepts.plato:17`) and its nine specialisations (`ScalarField2D` etc.) each require
exactly one `Eval`. Sub-interfaces redeclare nothing. So `At(x: Self, Integer): T` on `Array`
satisfies both `ArrayLike<T>` and `Indexable<Integer, T>` with a single definition; no
forwarding function is generated or needed.

## Assumptions

- `At` is the frozen name. `primitives-arrays.types.plato` states the compiler maps indexing
  syntax to `At`, and `At` sits in `Linter.MembersImplementedByWriter`, so the C# writer
  synthesises it from a type's shape. Unification must move `ValueAt` → `At`, never the
  reverse.
- The rename is free on the `MapLike` side: `MapLike.ValueAt` has **zero references** in the
  corpus (`plato_references` — the other three `ValueAt` definitions belong to `Curves`,
  `FieldsImplicitsCore`, `TimeVaryingValues` and are unrelated).
- `ContainsKey` has exactly one caller: `DoesNotContainKey`
  (`collections-containers.library.plato:70`).
- No interface in this stdlib uses a default body, so any inherited member is a real obligation
  on every implementer. This is why `ContainsKey` cannot ride along into the general interface
  without taxing every array and vector.
- No `Integer2` / `Integer3` key type exists today, so the 2D/3D unification is not available
  without new types.

## Design decisions

- **Which name generalises** — `Indexable<TIndex, TValue>` with `ArrayLike<T>` as the Integer
  instance, vs keeping `MapLike<TKey, TValue>` as the ancestor and writing
  `Indexable<T> inherits MapLike<Integer, T>`. Same skeleton either way; the diff is smaller
  for the second (MapLike has no implementers to disturb) and the naming is better for the
  first (*index* is the general word; `ArrayLike` matches the existing
  `SetLike`/`StackLike`/`QueueLike` family in the containers file). Choose for the end state,
  not the diff size.
- **Where the ordering helpers live** — the eleven generic bodies in
  `collections-indexable.library.plato` (`First`, `Last`, `Middle`, `LastIndex`,
  `IsValidIndex` ×2, `At(Index)`, `Reduce`, `Map`, `All`, `Any`) all need an Integer index and
  `Count` ordering. They cannot lift to the general interface; they must move to `ArrayLike<$T>`.
  This is the bulk of the mechanical churn.
- **Where `ContainsKey` goes** — dropped now and reintroduced on a future `DictionaryLike`, vs
  kept on the general interface. Keeping it makes every array, vector and mesh owe a bounds test
  that is derivable from `Countable` alone, manufacturing fresh LINT001 findings.
- **Whether `Indexable2D`/`3D` join** — they currently inherit `Indexable<T>` for flattened
  row-major access and add multi-argument `At(x, col, row)` overloads that `xs[col, row]` maps
  to. Recommended: re-point them at `ArrayLike<T>` and change nothing else. Re-expressing them
  as `Indexable<Integer2, T>` is the same move one level up but needs new key structs and
  touches the indexing-syntax mapping — a separate decision, deliberately not coupled here.
- **Does this clear LINT013 honestly?** Under this shape arrays become transitive implementers
  of the general interface, so the `MapLike` finding disappears as a side effect. That is only
  legitimate because arrays genuinely *are* integer-keyed lookups — not because the interface was
  declared true to silence a warning. Worth stating explicitly in the commit, since plato-325
  classifies `MapLike` in its retire bucket.

## Related

- [plato-325](plato-325.md) — the LINT013 triage. It currently files `MapLike` under
  "vocabulary ahead of its types → retire". This issue is the alternative disposition for that
  one finding: unify rather than delete. Whichever lands first should update the other.
- [plato-350](plato-350.md) — reduce `Indexable` boilerplate for implicitly indexable types.
  Touches the same synthesis seam; if `At` synthesis becomes automatic for single-collection
  wrappers, the cost of any extra obligation on `ArrayLike` changes, which affects the
  `ContainsKey` decision above.
- [plato-304](plato-304.md) — LinqArray port guidance; already states the repo noun is
  `Array` / interface `Indexable<$T>`. A rename to `ArrayLike` needs to be reflected there.
- `stdlib/geometry/meshes-volumetric.library.plato:53` — carries an inline comment citing
  LINT013 on `MapLike` as the reason for a slower path. Revisit if `MapLike` changes shape.

## Approaches

Short term:

1. **Rename-only (option D)** — keep `MapLike` as the ancestor name, rename `ValueAt` → `At`,
   move `ContainsKey` out, write `Indexable<T> inherits MapLike<Integer, T>`. Smallest possible
   diff: the containers interfaces file plus one helper. Gets the semantics with almost no churn,
   but leaves "MapLike" as the ancestor of every array, which reads wrong.
2. **Full shape (option B, preferred)** — `Indexable<TIndex, TValue>` + `ArrayLike<T>`, helpers
   re-headed onto `ArrayLike`, `MapLike` deleted. ~16 `Indexable` references and 11 helper
   signatures, all mechanical; `plato_check` makes the verify loop seconds rather than minutes.
3. **Do nothing** — leave both interfaces and let plato-325 retire `MapLike` outright. Honest if
   no dictionary type is ever coming.

Long term: with the key parameterised, a `DictionaryLike` (adding `ContainsKey`, `Keys`,
`Values`) slots in beside `ArrayLike` with no further interface surgery, and the 2D/3D family can
later be re-expressed as `Indexable<Integer2, T>` if key structs arrive.

Adjacent ideas worth their own issue:

- Introduce `Integer2` / `Integer3` key types and re-express `Indexable2D`/`3D` as instances of
  the general interface.
- A concrete `Dictionary` type, which is the thing that decides whether `MapLike`'s surface was
  ever worth keeping.

## Bedrock

The seam is the **collection lookup vocabulary** in `stdlib/foundation/collections-*.plato`.
Today it has two independent entry points for one operation, which means every future keyed
collection has to pick a side and every generic helper has to be written against one of them.
Parameterising the key makes "lookup by something" a single point in the hierarchy, so adding
`DictionaryLike` later is a leaf addition rather than a second parallel tower, and generic code
that only needs `At` + `Count` can be written once. It also removes the standing invitation to
re-add `ValueAt`-style aliases, because there is one name and the compiler already owns it.

Verdict: **simplest-along-the-grain**. The simple version must NOT (a) keep both `At` and
`ValueAt` reachable — a forwarding member reintroduces exactly the two-spellings problem the
unification exists to remove; (b) push `ContainsKey` onto the general interface to keep `MapLike`
callers working, since that taxes every array with an obligation the key type cannot justify;
(c) fold `Indexable2D`/`3D` in during the same change, which would entangle the indexing-syntax
mapping with a pure vocabulary refactor.

## Done means

- [ ] One lookup interface parameterised on the key; `ValueAt` no longer exists in
      `stdlib/foundation/`.
- [ ] `Array`, the vector interfaces and `Face`/`TriMesh` each declare exactly one `At` and
      satisfy the whole chain — no forwarding bodies introduced.
- [ ] The eleven `collections-indexable.library.plato` helpers are re-headed onto the
      Integer-keyed interface and still resolve.
- [ ] `ContainsKey` / `DoesNotContainKey` either removed or relocated to a dictionary interface,
      with the choice recorded here.
- [ ] LINT013 count drops by the `MapLike` finding and no LINT001 findings are added;
      plato-325 updated to reflect the disposition.
- [ ] `tools/check-stdlib-fast.ps1` passes.

## Simplest possible implementation

Approach 1 above: edit `collections-containers.concepts.plato` only — rename `ValueAt` to `At`,
delete `ContainsKey` (and `DoesNotContainKey` in the library file), then change
`interface Indexable<T> inherits Countable` to `inherits MapLike<Integer, T>` and drop its now
redundant `At` declaration. Roughly four line edits, no helper signatures touched, because every
existing `Indexable<$T>` helper header keeps working unchanged.

- **You get:** the semantics settled and the LINT013 finding resolved for the price of a
  four-line diff; the naming question can be answered later by a pure rename.
- **You give up / risk:** `MapLike` ends up the declared ancestor of every array and vector,
  which reads as a category error in the source even though it type-checks; and a later rename
  to `Indexable<TIndex, TValue>` / `ArrayLike<T>` pays the full 16-reference churn anyway, so
  the saving is deferred rather than avoided.
