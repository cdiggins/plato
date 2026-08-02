---
id: plato-285
title: Split stdlib files by types / libraries / interfaces
type: idea
status: idea
priority: "?"
effort: "?"
risk: "?"
area: plato
sprint: 
created: 2026-07-28
closed:
links: [submodules/Plato/stdlib/README.md, submodules/Plato/stdlib/LIBRARIES.md, plato-283, plato-286]
---

## Idea
Forward `stdlib` is a flat folder (~84 files) mixing domain type declarations, `*.concepts.plato`, and `*.library.plato` bodies. Naming already encodes kind (`domain.plato` / `domain.concepts.plato` / `domain.library.plato`), but physical layout does not. Consider splitting into folders (or clearer packages) by artifact kind — types vs libraries vs interfaces — so ownership, lint scopes, and agent navigation match the mental model.

## Assumptions
- Readers still get lost despite the README layer table and naming convention.
- `Plato.CLI` currently enumerates `*.plato` non-recursively (`TopDirectoryOnly`); any real subfolder split requires recursive lint/codegen first (history: `interface-library/` was flattened for this).
- Domain cohesion (keep `color` types next to `color` library) may matter more than kind cohesion (all interfaces together).

## Design decisions
- **Axis of split** — by kind (`types/`, `interfaces/`, `libraries/`) vs by domain (`color/`, `fields/`, …) vs hybrid (`color/types.plato` + `color/library.plato`). Kind-split matches the request; domain-split matches how people search.
- **Inlining policy** — today some files carry inline `library` blocks (`transforms`, `implicit-sdf`, …). Force extract vs allow exception list.
- **Tooling first vs layout first** — fix recursive folder support before moving files, or stay flat with stricter naming only.

## Related
- [stdlib/README.md](../../submodules/Plato/stdlib/README.md) — naming convention + flatten history + layer map.
- [stdlib/LIBRARIES.md](../../submodules/Plato/stdlib/LIBRARIES.md) — library package ground rules.
- [plato-283](plato-283.md) — color-constants folder as a small pilot of physical grouping.
- [plato-286](plato-286.md) — smaller core subset; layout should make "core" an obvious slice.

## Approaches
Short term: document and enforce the existing naming convention; optional `types.` / `interfaces.` / `libraries.` prefixes only if needed; no folders.
Long term: teach CLI recursive roots (or an explicit manifest); move to domain folders first, or kind folders if kind navigation wins a spike.
Adjacent: plato-286 (core subset as a first-class root or manifest entry).

## Bedrock
Strengthens the **stdlib package boundary**: kind and domain become navigable without opening the README. **Verdict: simplest-along-the-grain** — must NOT move files into subfolders until lint/codegen recurse (or a manifest lists every path); a manifest-driven multi-root is the compatible step.

## Done means
- [ ] Written decision: kind-split vs domain-split vs stay-flat (ADR or issue verdict)
- [ ] If folders: `lint` (and any codegen input walk) covers every `.plato` under the layout
- [ ] README file-map matches on-disk layout
- [ ] No duplicate/orphan library or interface files after the move

## Simplest possible implementation
Keep flat; add a short "where things live" index generated from filenames; fix pain with search not relocation.
- Pros: zero tooling risk; matches today's working lint.
- Cons: does not deliver the requested folder split; defers structural clarity.

## Case against
- **Already tried subfolders.** Flattening `interface-library/` was deliberate; re-introducing folders without tooling is a known footgun.
- **Kind-split fights domain reading.** Implementing `SignedDistanceField2D` means jumping `interfaces/` → `types/` → `libraries/` for one idea; domain folders (or flat + naming) may be better.
- **Large mechanical move.** 84 files + every doc/link/agent prompt that cites paths; high churn, low user-visible value.
- Verdict: **park** physical kind-folders until recursive lint exists; **pursue** only after a spike comparing kind vs domain folders against real navigation tasks. Prefer domain packages over kind buckets if anything moves.
