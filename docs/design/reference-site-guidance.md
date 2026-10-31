# Reference site guidance

This document states what the Plato reference site should be: its structure, what each page
contains, and the qualities every page keeps. It is a design target, not a description of the
site today. The current site is the [hierarchical reference](../reference/README.md) and its
[React reader](../../tools/reference-reader/README.md). The execution plan that moves the site
toward this target is [`plans/reference-site-plan.md`](../../plans/reference-site-plan.md).

The audience is junior software developers, scientists, engineers, and computer science
researchers and educators. A page that serves one of them must not lose the others.

This document records no counts of pages, symbols, or examples. The reference builder and the
generated catalogs hold those.

## Structure

The site has five doors off the landing page. Every page belongs to exactly one door and says
which.

The landing page is one sentence saying what the language is for, one short program the reader
can run, and the five doors.

| Door | Reader | What it is |
|---|---|---|
| **Tour** | Junior developers, educators | A linear, runnable walk through the language that fits in one sitting. Each page introduces one idea, shows it in an editable example, and ends with a small exercise. |
| **Reference** | Everyone, usually arriving from search | Syntax, types, concepts, semantics, and the standard library. One page per construct and one page per symbol, each at a permanent URL. |
| **Cookbook** | Scientists, engineers | Problem-shaped pages such as "fit a curve", "integrate an ODE", "transform a point cloud", or "call this from C#". Each is a complete program with its output shown. |
| **Design** | Researchers, educators | Rationale, formal grammar, semantics, the laws the library satisfies, rejected alternatives, and a citable bibliography. |
| **Playground** | Everyone | A full-page editor with shareable links, so examples, bug reports, and homework all point at a URL. |

The existing sections map onto the doors as follows. `start/` becomes the Tour. `language/`,
`library/`, and `api/` become the Reference. `formal/`, `evidence/`, `implementation/`, and
`review/` sit behind the Design door. The Cookbook and the Playground are new.

## Contents of a reference page

The symbol page earns the site's trust, so it gets the most care.

A page for a function or type shows, in this order:

1. **Signature and one-line summary**, readable without scrolling.
2. **A runnable example with its output**, checked by the build so it cannot rot.
3. **The laws or properties it satisfies.** Plato's `Law_*` functions make this possible, and
   few language references offer it. A tested law persuades a scientist more than prose does.
4. **Precision and cost notes.** Which number profile applies, what happens at the edges, and
   whether it allocates.
5. **Links to source, tests, and the concepts it implements**, and backlinks from everything
   that uses it.
6. **Version history**: added in, changed in, deprecated in.

A page for a language construct replaces the laws with the grammar fragment and a plain
statement of the semantics. It then shows the common mistake and the diagnostic the compiler
reports for it.

## Characteristics every page keeps

- **Everything runs.** Every example is executable, and the build checks every example. A dead
  example loses all four audiences at once.
- **Stable, guessable URLs.** A symbol's URL derives from its name and never changes. Educators
  link it in slides, researchers cite it, and search engines rank it. Generated page names that
  depend on declaration order break this rule.
- **Search first.** Every page has a search box that finds symbols by name, by signature shape,
  and by prose. Most visits are a lookup, not a read.
- **Versioned with a visible pin.** The reader always knows which language version a page
  describes, can switch versions, and finds old versions still online.
- **Honest about status.** Each page carries a status, so a junior developer knows what to
  avoid and a researcher knows what is settled. The status names and their meaning are set in the
  [reference contract](../../tools/reference/CONTRACT.md#status). Status sits beside the evidence
  labels in the reference README, which qualify individual claims.
- **One register.** Short sentences, no marketing. The Tour uses plain words; the Reference uses
  full precision. The [brand voice](../brand/README.md) governs tone.
- **Compare-to pages.** "If you know Python" and "If you know C#" tables map familiar constructs
  to Plato ones. They make juniors feel at home and show researchers what is new.
- **Machine-readable.** A JSON index of every symbol, a plain-text export of the whole
  reference, and an `llms.txt`. Tools and agents are an audience, and they feed the humans.
- **Fast and static.** No login and no tracking. The site works offline as one download, has a
  light and dark theme, and reads well on a phone. It is generated from the source tree, so the
  documentation cannot drift from the compiler.

## Models worth studying

Each of these does one part of the target well:

- The Rust book with docs.rs, for the split between a teaching book and per-symbol pages.
- The Julia manual, for doctests that keep examples honest.
- The Go tour, for a runnable linear introduction.
- MDN, for consistent per-page structure and compatibility notes.
- The Lean documentation, for showing proofs beside definitions.

## Constraints from this repository

- Markdown under `docs/reference/` stays the durable source. The reader and any playground
  consume it; they do not own content.
- [`SEMANTICS.md`](../SEMANTICS.md) and [`stdlib/CONVENTIONS.md`](../../stdlib/CONVENTIONS.md)
  remain normative. Site pages teach and link them; they do not restate rules.
- Pages follow [`documentation-conventions.md`](../documentation-conventions.md): design, not
  measurements.
- A runnable example must be checked by the law runner or an equivalent gate. An example the
  build cannot run is labelled as unchecked rather than presented as working.
