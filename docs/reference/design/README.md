# Design

This door is for researchers and educators who want to know why Plato is the way it is. It
does not teach the language. The [tour](../start/tour.md) and the [language guide](../language/README.md)
do that. It points at the places where Plato's meaning, its evidence, and its rejected
alternatives are kept, and it says how far each place can be trusted.

Each page behind this door states what it settles. This page restates none of it.

## How much authority each place has

The labels are the ones in the [reference README](../README.md#authority-and-evidence). A
label qualifies a claim, not a whole document, so a page can carry several.

| Place | What it settles | Authority |
|---|---|---|
| [SEMANTICS.md](../../SEMANTICS.md) | What Plato constructs mean, and the language the checker accepts. | Specified. The normative language document. |
| [CONVENTIONS.md](../../../stdlib/CONVENTIONS.md) | The domain conventions of the standard library. | Specified. Normative for the library. |
| [Decision records](../../../tracker/decisions/README.md) | Why a design choice went one way, and what it gave up. | Specified for the choice it records. A superseded record says so in its header. |
| [Rejected alternatives](rejected-alternatives.md) | A reader's guide to those records, with what each one turned down. | None of its own. It links the record that holds the rule. |
| [Formal model](../formal/README.md) | A small mathematical language, with Lean proofs about it. | Proved, in the stated model only. The page states that the proofs do not cover the compiler, the interpreter, or the library. |
| [Evidence](../evidence/source-notes.md) | Grammar, operator tables, and unresolved source markers, extracted from source. | Implemented. The extraction is generated, and it records forms the checked language does not implement. |
| [Implementation guide](../implementation/README.md) | What each stage of the toolchain does and which source file is its authority. | Implemented and Tested, with Inferred conclusions marked. Its [discrepancy register](../implementation/discrepancies.md) is Unresolved. |
| [Review registers](../review/language-improvements.md) | Proposed language changes and confusing claims in older documents. | Unresolved. They record proposals and doubts, not decisions. |
| [Proposals in docs/design](../../design/README.md) | Work that is planned, in flight, or specified but not fully executed. | None as a description of what exists. They steer decisions. |
| [Bibliography](bibliography.md) | The outside works this repository cites or compares against. | None. A citation shows a comparison, not an origin. |

A conflict between a normative document and the implementation is evidence to investigate, as
the reference README says. It is not permission to change the normative document.

## Where to start

- To learn what a construct means, read the [language guide](../language/README.md), then the
  chapter's link into SEMANTICS.md.
- To learn why a rule exists, read [rejected alternatives](rejected-alternatives.md).
- To learn what is proved and what is not, read the [formal model](../formal/README.md) and its
  [implementation bridge](../formal/implementation-bridge.md).
- To reimplement or audit Plato, read the [implementation guide](../implementation/README.md)
  and its [discrepancy register](../implementation/discrepancies.md) first.
- To cite this reference or the works it compares against, read the [bibliography](bibliography.md).

## What this door does not record

It records no counts of pages, symbols, theorems, or tests. The generated catalogs, the
[gate log](../../gate-log.md), and the dated verification checkpoints hold measurements.
