# Plan: bring the reference site to the site guidance

Target: [`docs/design/reference-site-guidance.md`](../docs/design/reference-site-guidance.md).
Current site: [`docs/reference/`](../docs/reference/README.md), built by
[`tools/reference/build.py`](../tools/reference/build.py) and shown by
[`tools/reference-reader/`](../tools/reference-reader/README.md).

The work runs as parallel waves of subagents in one checkout, using the `parallel-wave` skill.
Each chunk has a file fence, and no two chunks in a wave share a file. Each builder commits by
path. A chunk starts only when every chunk it depends on is committed.

## What already exists

Much of the target has a foundation already. The plan extends it rather than replacing it.

| Guidance item | Exists today | Gap |
|---|---|---|
| Tour | `start/` with a quick tour and getting started | Not split into one-idea pages; no exercises; examples not checked |
| Reference | `language/`, `library/`, generated `api/` | Construct pages lack a grammar fragment and a common-mistake section; symbol pages are source slices, not per-symbol pages |
| Cookbook | Nothing | The whole door |
| Design | `formal/`, `evidence/`, `implementation/`, `review/` | No door page, no bibliography, no rejected-alternatives page |
| Playground | Nothing; the reader README names playgrounds as a future panel | Needs a feasibility spike |
| Runnable, checked examples | `check_examples.py` runs law fixtures | Runs a fixed list of implementation fixture groups only |
| Stable URLs | API pages are named by order number | URLs move when declarations are reordered |
| Search | Title and body term match | No symbol or signature search |
| Status labels | Evidence labels in the reference README | No per-page status |
| Versioning | Source revision recorded in reader data | No version pin or switcher |
| Machine-readable | Reader data bundle and `catalog.json` | No public symbol index, plain-text export, or `llms.txt` |
| Static, offline, themed | Reader is static with local assets and two themes | No packaged offline download |

## Wave 0: before any builder starts

These steps are serial and belong to the owner or the supervising session. Items 2 to 4 are
settled in [`tools/reference/CONTRACT.md`](../tools/reference/CONTRACT.md#reference-site-decisions);
the recommendations below are kept for the reasoning, and the contract wins where they differ.

1. **Land the in-flight reference work.** The working tree carries a large uncommitted change
   across `docs/reference/` and `tools/reference-reader/`, plus untracked `start/`, cheatsheet,
   and glossary pages. Builders must start from a committed base, or their commits by path will
   tangle with it. The owner commits, stashes, or discards that change.
2. **Decide the symbol URL scheme.** The guidance asks for URLs derived from names. Overloads
   such as `AlmostEqual` need a rule. Recommended: one page per qualified name,
   `api/<tier>/<library>/<Name>`, listing every overload, with redirects from the old numbered
   paths. Record it in `tools/reference/CONTRACT.md`; every Wave 1 chunk reads it.
3. **Decide the status vocabulary.** Recommended: `future` tier pages are experimental, `legacy`
   pages are frozen, and `foundation`, `geometry`, and `graphics` pages are stable unless a page
   says otherwise. Record it in `tools/reference/CONTRACT.md`.
4. **Fix the example fixture layout.** New runnable examples live under
   `docs/reference/implementation/examples/<group>/` beside the existing groups. Each group holds
   at least one `Law_Reference*` function, so `check_examples.py` runs it the same way.

## Wave 1: independent chunks

All chunks below can run at once. The agent type is `platonic:builder` unless noted.

| Chunk | Goal | File fence | Done when |
|---|---|---|---|
| **A. Builder outputs** | Name-derived API URLs and the frozen alias table; a public symbol index JSON; a plain-text export of the reference; `llms.txt`; the check that tour and cookbook fences come from fixtures | `tools/reference/build.py`, `tools/reference/test_build.py`, regenerated `docs/reference/api/**` | `build.py --check` passes, and a test shows reordering declarations does not change a symbol's URL |
| **B. Example checker** | Run every fixture group found on disk instead of a fixed list; honour `inputs.txt`; move `builders` to it; keep refusing an empty group | `tools/reference/check_examples.py` | Adding a fixture folder makes it checked with no code change |
| **C. Tour** | Split `start/` into one-idea pages, each with an editable example and an exercise; link the cheatsheet and glossary | `docs/reference/start/**`, `docs/reference/implementation/examples/tour/**` | Every tour example is a checked fixture |
| **D. Compare-to pages** | "If you know Python" and "If you know C#" mapping tables | `docs/reference/compare/**` | Each Plato cell links its reference page |
| **E. Cookbook seed** | A first small set of complete problem pages for scientists and engineers, chosen from what the stdlib supports today | `docs/reference/cookbook/**`, `docs/reference/implementation/examples/cookbook/**` | Each recipe's program is a checked fixture with its output shown |
| **F. Construct pages** | Give each `language/` chapter a grammar fragment, a plain semantics statement, a common mistake, and the diagnostic it produces | `docs/reference/language/**` | Each quoted diagnostic matches `language/diagnostics.md` and the compiler |
| **G. Design door** | A door page over `formal/`, `evidence/`, `implementation/`, and `review/`; a bibliography; a rejected-alternatives page drawn from `tracker/decisions/` | `docs/reference/design/**` | No rule is restated, and every claim links its authority |
| **S. Playground spike** | Find out whether the interpreter can run in the browser as a WebAssembly build of the .NET interpreter, or needs a service, and at what cost. Agent type `general-purpose` with the `investigate` skill; throwaway work outside the repo | A dated report under `docs/reports/` only | The report gives a recommendation, its confidence, and what would change it |

Chunks C and E depend on Wave 0 item 4. Chunk A depends on Wave 0 items 2 and 3.

## Wave 2: chunks that need Wave 1

| Chunk | Depends on | Goal | File fence |
|---|---|---|---|
| **H. Symbol page template** | A | Each symbol page shows signature and summary, an example, laws from `stdlib/tests`, precision and cost notes, links to source, tests, and concepts, backlinks, and status | `tools/reference/build.py`, `tools/reference/test_build.py`, regenerated `docs/reference/api/**` |
| **I. Reader doors and search** | A | Five-door navigation, a door label and status badge on every page, symbol and signature search over the symbol index, and a visible source-revision pin | `tools/reference-reader/src/**` |
| **J. Landing page** | C, D, E, G | Rewrite the landing page as one sentence, one runnable program, and the five doors | `docs/reference/README.md` |
| **K. Playground** | S, I | Build it only if the spike recommends it; otherwise record the decision in `tracker/decisions/` | Set by the spike's recommendation |

H, I, and J touch disjoint files and run together.

## Wave 3: review and gate

1. A `platonic:reviewer` subagent reads the integrated diff against the guidance with fresh
   context. It checks per-page contents, the URL contract, and the documentation conventions.
2. A `platonic:fixer` applies its defect findings, one commit each.
3. The supervising session runs the gates once:

```powershell
python tools/reference/build.py --check
python tools/reference/check_examples.py
npm --prefix tools/reference-reader test
npm --prefix tools/reference-reader run build
```

4. A `platonic:sweeper` files the remaining gaps as tracker items.

## Out of scope

- **Version switching.** It needs published language releases first. Wave 2 shows the source
  revision, and a tracker item covers the switcher.
- **Hosting.** Where the static site is served is an owner decision.
- **Normative documents.** `SEMANTICS.md` and `stdlib/CONVENTIONS.md` are linked, not edited.
