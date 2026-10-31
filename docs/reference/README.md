# Plato reference

Plato is a programming language for immutable data and numerical algorithms.
This site teaches the language, catalogs the libraries, and records the evidence an
implementer needs for a compatible toolchain.

Each later page stands on its own. A link names a prerequisite or the source of a rule.

## Start

- [What is Plato](start/what-is-plato.md)
- [Quick tour](start/tour.md): one-idea pages, each with a checked example and an exercise
- [If you know Python or C#](compare/README.md): familiar constructs mapped onto Plato, with what is missing and what is new
- [Getting started](start/getting-started.md)

## Where to look

The [language](language/README.md) says what programs mean.

The [library guide](library/README.md) covers mathematical objects, units, coordinate
systems, preconditions, and operations.

The API catalog keeps exact declarations, comments, and bodies, one page per declared name.
Each page carries a status of Supported, Experimental or Legacy, set by its tier:

- [Foundation](api/foundation/README.md) is the scalar, algebra, and collection vocabulary.
- [Geometry](api/geometry/README.md) is spatial objects and geometric algorithms.
- [Graphics](api/graphics/README.md) is images, cameras, scenes, and rendering vocabulary.
- [Future](api/future/README.md) is aspirational vocabulary. The default shipping gates exclude it.
- [Legacy](api/legacy/README.md) is the older library, compiled separately from the forward vocabulary.

The [cookbook](cookbook/README.md) has problem-shaped recipes for scientists and engineers, each a
complete program checked by the law runner.

The [design door](design/README.md) explains why Plato is the way it is: the authority of each
source, a bibliography, and the alternatives that were rejected.

The [implementation guide](implementation/README.md) connects that meaning to the compiler,
the interpreter, and the C# runtime. Read the
[discrepancies](implementation/discrepancies.md) before implementing a disputed boundary.

The [formal model](formal/README.md) states definitions, theorem statements, proofs, and
the limits of those proofs.

Evidence is extracted from source. The [grammar evidence](evidence/grammar-source.md) and
[operator tables](evidence/operator-source.md) keep accepted syntax, including forms the
checked language does not implement. The [source-note register](evidence/source-notes.md)
collects unresolved markers and does not treat them as language features. The
[dated integration record](implementation/verification-2026-10-25.md) states which checks
were run, which failures were investigated, and what verification is still open.

Review is separate from the language that exists. The
[language improvement register](review/language-improvements.md) records proposals. The
[documentation issue register](review/documentation-issues.md) identifies confusing claims
in older documents and source comments, their consequences, and proposed corrections.

## Authority and evidence

This reference teaches the language and describes the implementation.
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
apply. The two links compare organization and type structure. They do not state where
Plato came from.

## Build and browse

Run `python tools/reference/build.py` from the repository root to refresh source-derived
Markdown and assemble reader data. Run `python tools/reference/build.py --check` to reject
stale generated evidence or broken local links. The
[React reader](../../tools/reference-reader/README.md) provides a dynamic hierarchical
table of contents, search, page links, and Markdown downloads. Markdown remains the
durable source. Future sandboxes and 2D/3D views belong in the reader layer.
