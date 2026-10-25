# Plato reference

This reference explains Plato to developers and scientists who know a little programming.
It also provides the evidence and algorithms an implementer needs when building a compatible
language implementation. Start with the language chapters, then read the library domain you need.
Each page can be read independently; links identify prerequisites and the source of each rule.

## How to read this reference

The [language](language/README.md) explains what programs mean. The
[implementation guide](implementation/README.md) connects that meaning to the compiler,
interpreter and C# runtime. The [library guide](library/README.md) explains mathematical objects,
units, coordinate systems, preconditions and operations. The API catalog retains exact
declarations, comments and bodies, organized by tier and source component:

- [Foundation](api/foundation/README.md) contains the scalar, algebra and collection building blocks.
- [Geometry](api/geometry/README.md) describes spatial objects and geometric algorithms.
- [Graphics](api/graphics/README.md) describes images, cameras, scenes and rendering vocabulary.
- [Future](api/future/README.md) contains aspirational declarations excluded from default shipping gates.
- [Legacy](api/legacy/README.md) records the older library, compiled separately from the forward vocabulary.

Read the [formal model](formal/README.md) for precise definitions, theorem statements, proofs
and their limits. Read the [discrepancies](implementation/discrepancies.md) before implementing
behavior at a disputed boundary. The [language improvement register](review/language-improvements.md)
records proposals separately from the language that exists.

The [documentation issue register](review/documentation-issues.md) identifies confusing claims
in older documents and source comments, their consequences, and proposed corrections.

## Authority and evidence

This is a descriptive, teaching reference. Existing [SEMANTICS.md](../SEMANTICS.md) remains the
normative language document; [CONVENTIONS.md](../../stdlib/CONVENTIONS.md) owns library domain
conventions. A conflict is evidence to investigate, not permission to silently change either
authority. Chapters identify both the intended rule and the implementation that disagrees.
Only the compiler, interpreter, C# writer/runtime, source library, tests and explicit decisions
are used as implementation evidence. Other target-language writers are proofs of concept.

The following words have specific meanings here:

| Label | Meaning |
| --- | --- |
| Specified | A normative document or explicit decision states the rule. |
| Implemented | Named source code implements the behavior; this does not prove it correct for every input. |
| Tested | A named executable test exercises the claim; its scope matters. |
| Inferred | The author derives a conclusion from evidence, with the derivation or assumption stated. |
| Unresolved | Sources disagree, an operation is missing, or the behavior has no settled specification. |
| Proved | A theorem follows in the stated formal model; it applies only under that model's assumptions. |

A declaration is not an implementation. A body is not a proof. A passing linter is not a passing
type checker, and successfully emitting C# is not successfully compiling or executing it.
The reference deliberately omits handwritten inventories and gate tallies: generated catalogs
and the [gate log](../gate-log.md) hold measurements instead.

The [dated integration record](implementation/verification-2026-10-25.md) states which checks
were actually run, the failures investigated and the remaining verification limits.

## For implementers and agents

Use the language chapters as the readable contract, the formal model as a small precise
foundation, and the implementation guide to identify the remaining obligations. The
[grammar evidence](evidence/grammar-source.md) and [operator tables](evidence/operator-source.md)
are extracted directly from source. They preserve accepted syntax, including forms the
checked language does not implement. The [source-note register](evidence/source-notes.md)
collects unresolved markers without interpreting them as accepted language features.

This reference is not a claim that the full compiler has been proved sound or that every
standard-library algorithm is correct. An implementation from scratch must settle the explicitly
recorded underspecification and validate compatibility against tests and backend behavior.
When a precise rule is missing, keep an explicit obligation rather than inventing one from C#.

## Build and browse

Run `python tools/reference/build.py` from the repository root to refresh source-derived
Markdown and assemble reader data. Run `python tools/reference/build.py --check` to reject stale
generated evidence or broken local links. The [React reader](../../tools/reference-reader/README.md)
provides a dynamic hierarchical table of contents, search, page links and Markdown downloads.
Markdown remains the durable source; future sandboxes and 2D/3D views belong in the reader layer.

## Related work

Language references such as the [Rust Reference](https://doc.rust-lang.org/reference/)
separate constructs into focused, linkable chapters. This reference follows that useful
organization while making Plato's implementation disagreements explicit. The
[Haskell Report](https://www.haskell.org/onlinereport/)
provides a comparison for algebraic data types and type classes; its non-strict evaluation
model must not be assumed for Plato. These are comparisons, not claims about Plato's provenance.
