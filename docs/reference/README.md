# Plato reference

Plato is a small language for immutable data and numerical algorithms: you write a
function once, state its laws beside it, and the toolchain checks both.

Here is a complete program. It defines a type, three functions, and three laws about them.

```plato
type TourPoint { X: Integer; Y: Integer; }

library TourProducts {
    TourMakePoint(x: Integer, y: Integer): TourPoint => (x, y);
    TourSwap(p: TourPoint): TourPoint => (p.Y, p.X);
    TourSum(p: TourPoint): Integer => p.X + p.Y;

    Law_ReferenceTourFields(): Boolean => TourMakePoint(3, 7).Y == 7;
    Law_ReferenceTourSwap(): Boolean => TourMakePoint(3, 7).TourSwap.X == 7;
    Law_ReferenceTourConstructor(): Boolean => TourPoint(3, 7).X == 3;
}
```

This is [`products.plato`](implementation/examples/tour/products.plato), and the law runner
checks it: each `Law_` function must return `true`. The
[products page](start/tour/products.md) explains it line by line.

## The five doors

- **Tour** is for anyone new to Plato. [The quick tour](start/tour.md) is a short walk
  through the language, one idea per page, each with a checked example and an exercise.
  If you know another language, start from [Python](compare/python.md) or
  [C#](compare/csharp.md), or see [all comparisons](compare/README.md). Also see
  [What is Plato](start/what-is-plato.md) and [Getting started](start/getting-started.md).
- **Reference** is for everyone who needs an exact answer. The [language](language/README.md)
  says what programs mean, and the [library guide](library/README.md) covers mathematical
  objects, units, coordinate systems, and operations. The API catalog has one page per
  declared name, set by tier:
  [Foundation](api/foundation/README.md) (scalars, algebra, collections),
  [Geometry](api/geometry/README.md) (spatial objects and algorithms),
  [Graphics](api/graphics/README.md) (images, cameras, scenes),
  [Future](api/future/README.md) (aspirational, outside the default gates), and
  [Legacy](api/legacy/README.md) (the older library, compiled separately).
- **Cookbook** is for scientists and engineers. The [cookbook](cookbook/README.md) holds
  problem-shaped recipes, each a complete program checked by the law runner.
- **Design** is for researchers and implementers. The [design door](design/README.md)
  explains why Plato is the way it is, with a bibliography and the rejected alternatives.
  Behind it sit the [implementation guide](implementation/README.md) (read the
  [discrepancies](implementation/discrepancies.md) before implementing a disputed
  boundary), the [formal model](formal/README.md), the
  [grammar](evidence/grammar-source.md) and [operator](evidence/operator-source.md) evidence,
  the [source-note register](evidence/source-notes.md), the
  [dated integration record](implementation/verification-2026-10-25.md), the
  [language improvement register](review/language-improvements.md), and the
  [documentation issue register](review/documentation-issues.md).
- **Playground** is for everyone who wants to try an example. It is a route of the reader,
  not a Markdown page, so this page does not link to it. Open it from the reader's header.

## Authority and evidence

This reference teaches the language and describes the implementation. The
[design door](design/README.md) holds the longer account of each source's authority.
[SEMANTICS.md](../SEMANTICS.md) is the normative language document.
[CONVENTIONS.md](../../stdlib/CONVENTIONS.md) is the normative source for library domain
conventions. A conflict with either document is evidence to investigate, not permission
to change that document. Where a chapter states a rule and an implementation that
disagrees, both stay.

| Label | Meaning |
| --- | --- |
| Specified | A normative document or explicit decision states the rule. |
| Implemented | Named source code implements the behavior. This does not prove it for every input. |
| Tested | A named executable test exercises the claim. Its scope matters. |
| Inferred | The author derives a conclusion from evidence and states the derivation or the assumption. |
| Unresolved | Sources disagree, an operation is missing, or the behavior has no settled specification. |
| Proved | A theorem follows in the stated formal model, under that model's assumptions. |

Implementation evidence is the compiler, the interpreter, the C# writer and runtime, the
source library, tests, and explicit decisions. Other target-language writers are proofs of
concept.

A declaration is not an implementation. A body is not a proof. A passing linter is not a
passing type checker. Emitting C# is not compiling that C# or running it.

This reference omits handwritten inventories and gate tallies. Generated catalogs and the
[gate log](../gate-log.md) hold those measurements.

When a precise rule is missing, record the obligation. Do not invent the rule from C#.

Chapter layout follows the [Rust Reference](https://doc.rust-lang.org/reference/). The
[Haskell Report](https://www.haskell.org/onlinereport/) is a comparison for algebraic data
types and type classes. Plato is eager, so that report's non-strict evaluation does not
apply.

## Build and browse

Run `python tools/reference/build.py` from the repository root to refresh source-derived
Markdown and assemble reader data. Run `python tools/reference/build.py --check` to reject
stale generated evidence or broken local links. The
[React reader](../../tools/reference-reader/README.md) provides a dynamic hierarchical
table of contents, search, page links, and Markdown downloads. Markdown remains the
durable source. Future sandboxes and 2D/3D views belong in the reader layer.
