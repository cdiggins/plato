# Build the Plato reference

This tool assembles a source-evidenced Markdown reference and the data consumed by the
[React reader](../reference-reader/README.md). It is for documentation authors who need full
library coverage without manually maintaining a second API inventory.

From the repository root, using Python 3.10 or later:

```powershell
python tools/reference/build.py
python tools/reference/build.py --check
python tools/reference/test_build.py
python tools/reference/check_examples.py
python tools/reference/check_formal.py
```

The first command refreshes generated Markdown under `docs/reference/api/` and
`docs/reference/evidence/`, then writes the ignored reader data file
`tools/reference-reader/public/reference.json`. The second command checks generated freshness,
coverage and local links without modifying files. Run the reader's own build after generation.

The example checker executes the isolated teaching fixtures, rejects empty selections and
unfulfilled obligations, and saves full logs under `.temp/reference-example-logs/`. Pass
`--cli PATH` to use a fresh isolated CLI build; `--backend differential --group builders`
compares the foundation-dependent builder fixtures in the interpreter and compiled C#.
The minimal prelude used by the other groups is an interpreter teaching environment, not
a replacement for the C# runtime's full foundation declarations.

The formal checker requires the pinned Lean release and treats warnings as errors. Pass
`--lean PATH` if it is not on PATH or in the repository-local temporary toolchain directory.
It saves checker diagnostics under `.temp/reference-proof-logs/`. The proof model's scope is
defined by [its own guide](../../docs/reference/formal/README.md).

## What is generated

Every `.plato` source directly inside foundation, geometry, graphics, future and the legacy
library receives a component index and small ordered source sections. Splitting occurs between
complete declarations/functions, with a source-line budget for groups of small members. A
single long function stays together. Source comments and implementations are retained; no
algorithm is inferred from a symbol's name. The first/last sections preserve library wrappers,
so a section is evidence rather than a standalone compilation unit.

The JSON catalog records source fingerprints, declaration ranges and their page locations.
Ranges partition each normalized UTF-8 source without omissions or overlap. Normalization
removes a UTF-8 byte-order mark and converts CRLF to LF; hashes deliberately ignore those
encoding-only differences to remain stable across checkouts. Code fences omit boundary
newlines, while linked originals retain their bytes. New source files
join the catalog automatically; empty tiers fail. Removed/renamed sources remove obsolete output
only inside the generator-owned folders. The evidence appendices capture grammar/operator
source and unresolved-note markers. Fingerprints detect comment as well as code changes.

Authored chapters live elsewhere under `docs/reference`. They explain semantics, conventions,
preconditions and related work; generated source pages do not replace that teaching layer.
Only the generator-owned output folders may be regenerated or pruned.

## Links and provenance

The reader data rewrites reference links into hash routes. Other repository source links point
to GitHub at the recorded commit, with Parakeet links pinned to its own submodule revision.
Regenerate after committing new sources if the reader must link to them remotely. The generated
data describes the local content at build time; its commit label alone does not assert a clean
working tree. Source fingerprints provide the separate evidence of exact content.

This tool validates local targets, not external websites or the semantics of prose. The catalog
uses a lossless lexical splitter, not a second compiler parser; it checks balanced source braces
and preserves unrecognized contents in their surrounding section. Type checking, law execution,
C# compilation and theorem checking remain separate checks. See the
[reference](../../docs/reference/README.md) for their different meanings.

## Related work and trade-offs

Documentation systems such as [Sphinx](https://www.sphinx-doc.org/en/master/) and
[Docusaurus](https://docusaurus.io/docs) also assemble text into a navigable reference. This
tool keeps Plato Markdown and exact source coverage in the repository and feeds a small React
reader, leaving execution sandboxes and visualizers as future reader extensions. It gives up a
large documentation framework's plugin ecosystem and does not add an independent semantic parser.

The repository license applies. Report documentation defects through `tools/track.py`; the
[documentation issue register](../../docs/reference/review/documentation-issues.md) explains
findings already captured during the reference audit. Gate measurements live in generated logs
rather than duplicated prose here.
