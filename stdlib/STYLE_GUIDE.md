# stdlib style guide

How to **write** forward-stdlib Plato: structure, idioms, comments, and links.
Semantic / world / API rules (Z-up, CCW, matrices, units, …) live in
[`CONVENTIONS.md`](CONVENTIONS.md) — read that first for *what* is true; this
file is *how* to express it.

Derived from the monorepo house rules (`AGENTS.md`, root `coding-style.md`,
`ara3d-sdk/AGENTS.md`, Studio `AGENTS.md`, `docs/csharp-style-guide-for-agents.md`),
adapted to Plato. When this guide and those sources disagree on spirit, prefer
the monorepo guides and fix this page.

---

## Cross-reference

| Document | Holds |
| --- | --- |
| [`CONVENTIONS.md`](CONVENTIONS.md) | Domain invariants: frames, winding, matrices, angles, bounds, color, UV, epsilon, partial-ops |
| **This file** | Authoring: size of functions, literals vs MapRange, comments, Wikipedia links, operators |

Cite conventions in source with `// Convention: see CONVENTIONS.md - <section>`.
Cite this guide only when a non-obvious style choice needs a pointer
(`// Style: see STYLE_GUIDE.md - <section>`).

---

## API-first and geometry priority

Write every library function as if it were public API: obvious from the
signature, easy to relocate, dogfood existing vocabulary before adding types.

For geometry and numeric helpers, prefer properties in this order when they
conflict (same order as SDK / Studio AGENTS):

1. Correct  
2. Composable  
3. Reusable  
4. Functional (inputs → outputs; expressions)  
5. Side-effect free  
6. Succinct  
7. Easily verifiable  

A faster or more imperative variant is a **later, separate** function — never by
compromising the canonical one.

## Small pure building blocks

- Prefer **small** library functions; split anything reusable into its own name.
- Prefer **expressions** (`=> …`) over statement blocks when clear.
- Build new code from existing functions and types; add a type only when two
  real uses exist (no speculative abstraction).
- One clear purpose per function, type, and file.
- Minimize parameters: bundle with existing records or tuples when a cluster
  already has a name.

## Array literals vs `MapRange`

**Fixed, known arity** → array literal `[a, b, …]`. Do **not** write
`N.MapRange(i => i == 0 ? … : …)` for corners, endpoints, slabs, or other
constant-length lists.

```
// Prefer:
Points(t: Triangle2D): Array<Point2D>
    => [t.A, t.B, t.C];

// Not:
Points(t: Triangle2D): Array<Point2D>
    => 3.MapRange(i => i == 0 ? t.A : (i == 1 ? t.B : t.C));
```

**Runtime / data-dependent length** → `MapRange` / `Map` / `Zip` is correct
(`points.Count.MapRange(…)`, polygon kernels, etc.).

## Named constants

Prefer named constants and vocabulary constructors over magic numbers in
bodies (`Angle` via `.Degrees` / `.Turns`, axis sums, empty bounds encodings).
Raw literals are fine for true mathematical constants (`0`, `1`, `2`, `0.5`)
when the meaning is obvious in context.

## Arithmetic spelling — prefer operators

`+` / `*` / `-` / `/` are sugar for `Add` / `Multiply` / `Subtract` / `Divide`
(see `docs/plato-language-semantics.md` §6). In library bodies, prefer operators
so formulas read as math. Keep the named form at **definition sites**
(`Add(a, b) => …`) and when UFCS chaining clarifies.

```
// Prefer:
origin + direction * t

// Not:
origin.Add(direction.Multiply(t))
```

Comparison operators vs `Equals` / `LessThan` method form remain an open style choice.

## Comments and documentation

- Every declaration gets a short `//` doc comment: what it is and any
  **non-obvious** invariants. Do not narrate the code.
- Section banners use `//==`.
- Function comments are one or a few short lines — not essays.
- Never use comments to explain the *change* you are making (that belongs in
  the commit message).

## Wikipedia and external formula links

When a function implements a named formula or construction, prefer a single
Wikipedia (or similarly stable) link in the doc comment over restating the
derivation. Keep the comment about *what* and *preconditions*; let the link
carry the textbook identity.

```
// The magnitude of the cross product of the two side vectors.
// https://en.wikipedia.org/wiki/Parallelogram#Area_formula
Area(g: Parallelogram2D): Number
    => (g.SideA.X * g.SideB.Y - g.SideA.Y * g.SideB.X).Abs;
```

## Vocabulary and naming reminders

(Declaration-level rules also appear in [`README.md`](README.md); repeated here
only as authoring checks.)

- Prefer existing interfaces/types over new ones with overlapping meaning.
- Collection fields: `Array<T>` (or `Array2D` / `Array3D`).
- Cross-array references: typed `Index` wrappers, not raw `Integer`
  ([CONVENTIONS — typed indices](CONVENTIONS.md#typed-indices--1-means-none)).
- Angles: `Angle`, never raw `Number` for angular quantities
  ([CONVENTIONS — angles](CONVENTIONS.md#angles--angle-typed-radians-canonical)).
