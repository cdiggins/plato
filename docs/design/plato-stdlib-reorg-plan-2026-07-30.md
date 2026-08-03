# stdlib reorganization plan — `.types.plato` suffix + domain-tier folders

**Date:** 2026-07-30 · **Status:** PROPOSED · **Scope:** `submodules/Plato/stdlib` (402 files: ~400 `.plato` sources + 4 docs)

---

## Rationale

Why move anything at all, and why this shape:

1. **The suffix grammar is two-thirds finished.** Today `<stem>.concepts.plato` holds
   interfaces and `<stem>.library.plato` holds one library block — but type files are
   *bare* `<stem>.plato`. Bare is the one suffix you cannot grep for (`*.plato` matches
   everything) and the one you must *know* means "types". Renaming every bare file to
   `<stem>.types.plato` completes the grammar: **every file's suffix names its
   declaration kind**, `ls`, glob, and lint filters all become trivial
   (`*.types.plato`, `*.concepts.plato`, `*.library.plato`), and no file is ever
   ambiguous to a human or an agent. Cost is near zero: it is a pure `git mv` (100%
   rename similarity, history preserved), no content changes, because Plato files do
   not reference each other by filename.

2. **One flat folder of 400 files no longer serves either audience.** Humans cannot
   browse it; agents pay full-folder lint even when touching one domain; and the
   priority split the project actually has (geometry/graphics now, physics/science
   later) is invisible in the tree. Folders make the priority *structural*.

3. **Domain names beat abstract tier names.** The original idea was
   `core / extended / future`. Rejected: "extended" tells a human nothing about what
   is inside, and the boundary between core and extended is a judgment call nobody can
   reconstruct. Instead the folders are **named by domain, ordered by dependency**,
   and the tier property (compile subsets, priority) falls out of the ordering. A
   newcomer reading the folder names learns the library's actual shape; an agent gets
   the same compile-subset benefit the abstract names would have given.

4. **The folders double as compile subsets.** They are dependency-ordered and
   *cumulative*: `foundation` lints alone; `geometry` lints with `foundation`;
   `graphics` with both; `future` with everything. That gives a faster gate for the
   common case (geometry/graphics work never compiles physics), quarantines
   aspirational domains so their debt never blocks priority work, and gives
   multi-agent waves a natural collision-free partition (one agent per folder — no
   fences needed across folders).

5. **Assignment is by the existing README layer table, not by re-judging 400 files.**
   The layer table already encodes dependency order and appears once per file. The
   move maps layers → folders mechanically, with a short explicit exception list
   (below) where a layer straddles the priority boundary. This keeps the move
   reviewable: the diff is `git mv` plus a handful of documented judgment calls.

6. **Priority policy embodied:** 2D/3D geometry and graphics are the product
   (Ara 3D Studio). Physics, engineering, scientific computation, geo-spatial are
   aspirational — real vocabulary worth keeping, but they must never tax the
   priority path. Hence they land in `future/`, the one abstract name retained,
   because there the *message is the status*, not the domain.

---

## Target layout

```
stdlib/
  README.md  CONVENTIONS.md  STYLE_GUIDE.md  LIBRARIES.md
  foundation/   # layers 1–7 + math substrate: primitives, algebra, collections,
                # numbers (+special fns), quantities, time, vectors, matrices, points,
                # axes, intervals, transforms, color, intrinsics, random, statistics,
                # polynomials, graphs
  geometry/     # layers 8–11 + differential geometry: shapes, lines/planes, polygons,
                # curves, splines, surfaces, solids, fields, SDF/implicits, noise,
                # sampling, topology, meshes, pointclouds, voxels, spatial structures
                # & queries
  graphics/     # layers 12–15: easing, keyframes, motion graphics, skeletal animation,
                # paths, text, vector styling, scene2d, color science, imaging,
                # texturing, cameras, lights, materials, render, scene3d
  future/       # aspirational: physics (collision, joints, kinematics, particles,
                # rigid dynamics), signals, optimization, uncertainty, engineering,
                # geo-spatial, scientific data, higher dimensions
```

Suffix grammar after the move — **exact, no bare files**:

| Suffix | Contents |
|---|---|
| `<stem>.types.plato` | only `type` declarations |
| `<stem>.concepts.plato` | only `interface` blocks |
| `<stem>.library.plato` | exactly one `library` block named after the stem |

Compile subsets (cumulative):

```
lint stdlib/foundation
lint stdlib/foundation stdlib/geometry
lint stdlib/foundation stdlib/geometry stdlib/graphics
lint stdlib/foundation stdlib/geometry stdlib/graphics stdlib/future   # = today's full gate
```

Rule: a folder may reference only itself and folders above it. `future` may reach
everything; nothing reaches into `future`.

---

## Folder assignment (by README layer)

Mechanical mapping; exceptions called out explicitly.

| README layer | Folder | Exceptions / notes |
|---|---|---|
| 1–7 (primitives … intrinsics) | `foundation` | none |
| 8 (geometry interfaces & shapes) | `geometry` | none |
| 9 (curves, splines, surfaces, solids) | `geometry` | none |
| 10 (fields, SDF, noise, sampling) | `geometry` | none |
| 11 (topology, meshes, spatial) | `geometry` | none |
| 12 (animation & motion) | `graphics` | none |
| 13 (vector graphics, text, 2D scenes) | `graphics` | none |
| 14 (color science & imaging) | `graphics` | none |
| 15 (rendering) | `graphics` | none |
| 16 (physics & simulation) | `future` | all of it: `collision*`, `joints-*`, `kinematics*`, `particles-*`, `rigid-dynamics*` |
| 17 (math, statistics, signals, optimization) | **split** | `polynomials*`, `random*`, `statistics*`, `numbers-special-*` → `foundation` (math substrate used by geometry sampling/noise and imaging). `signals*`, `optimization*`, `uncertainty*` → `future` (scientific computation). |
| 18 (advanced & applied) | **split** | `differential-geometry-*` → `geometry` (it *is* priority geometry). `graphs*` → `foundation` (generic container; `fields-graphs` in geometry layer 10 references `Graph`, so graphs must sit below geometry). `engineering-*`, `geo-spatial*`, `scientific-data*`, `higher-dimensions*` → `future`. |

Judgment calls, spelled out:

- **`kinematics*` → `future`, not `graphics`.** It reads as physics vocabulary. If
  the in-tier lint (Phase 3 verification) shows motion-graphics or skeletal-animation
  referencing it, the referenced declarations move to `graphics` (or the files split);
  the rule is *move the needed file down / dependent domain wins*, never a
  cross-tier upward reference.
- **`uncertainty*` → `future`** even though `Tolerance` is registry-listed: its
  documented meaning is engineering acceptance allowance. If any priority-tier file
  references it, same resolution rule applies.
- **`quantities-electromagnetic/thermal/…` stay in `foundation`** (layer 3): tiny
  type-only files, and photometric/material quantities are used by lights/materials
  in `graphics`. Not worth a split.
- **`collision*` → `future`**, but `spatial-queries-*` (raycast, overlap, proximity)
  stay in `geometry` — those are geometry queries the product uses; collision
  *response/contacts* is physics.

Approximate sizes after the move: foundation ≈ 120 files, geometry ≈ 150,
graphics ≈ 75, future ≈ 55. (Exact counts fall out of the move script.)

---

## Tooling and doc changes required

1. **Plato.CLI multi-root enumeration.** `Program.cs:101` / `:197` enumerate
   `*.plato` with `TopDirectoryOnly` over a single folder. Change: accept **multiple
   folder arguments** (each still enumerated top-only). Explicit multi-root is
   preferred over a recursive flag — it keeps subset compiles expressible and makes
   the cumulative-tier rule visible in every command line.
2. **Gates.** `tools/check-stdlib-fast.ps1`, the git hooks, and the `ForwardStdLib`
   test update to the four-root invocation; add per-tier fast variants
   (`-Tier geometry` lints foundation+geometry only).
3. **Navigation MCP.** Relaunch with the four folder roots (`--root` per folder) so
   go-to-definition keeps working. (Recipe in the relaunch memory/doc.)
4. **Docs.** `README.md`: layer table gains a Folder column; file-count paragraph and
   naming section updated for `.types.plato`; validation section shows the tier
   commands. `LIBRARIES.md` / `CONVENTIONS.md`: update the ~30 filename citations
   (`matrices.plato` → `foundation/matrices.types.plato`, etc.). `AGENTS.md` /
   `docs/working-on-plato.md`: agents pick the smallest tier command that covers
   their touched folders.

---

## Phases

**Phase 1 — suffix rename (independent, do first).**
Script: `git mv <stem>.plato <stem>.types.plato` for every bare type file (~190).
No content edits to `.plato` sources. Update filename mentions in the four docs.
Gate: full lint before and after must report identical counts (0 parse / 0 resolve;
LINT001/LINT003 unchanged).

**Phase 2 — CLI multi-root + gate updates. LANDED 2026-07-30.** `lint <folder>...` takes any
number of roots (each top-only, union compiled as one program); codegen takes multiple roots
when `--out=<folder>` names the output, since that frees every positional to be an input.
One root reproduces the previous behavior exactly — verified: all 2806 findings byte-identical
to the pre-change (pinned) CLI, and the full stdlib split across two scratch roots lints to the
same findings as the single folder, while either half alone fails to compile. `check-stdlib-fast.ps1`
gained `-Folders`; `stage-stdlib.ps1 -Folders` already forwarded its array. Tier folders do not
exist yet, so nothing calls multi-root in anger until Phase 3.

**Phase 3 — folder moves.** Script emits `git mv` per the assignment table. Then the
cumulative tier lints run bottom-up; every upward reference found is resolved by the
*dependent-domain-wins* rule (move the referenced file down, or the referencing file
up) and recorded as an amendment to the exception list. Gate: four-root full lint
identical to pre-move; each cumulative subset lints clean.

**Phase 4 — docs, MCP relaunch, tracker.** README/LIBRARIES/CONVENTIONS citations,
Navigation MCP roots, file a tracker note closing the move with the final exception
list.

Phases 1 and 2 are independent and can run in parallel (different files). Phase 3
depends on both. Each phase is one commit (pathspec-scoped), revertible on its own.

---

## Risks

- **Hidden upward references** (the `fields-graphs` → `Graph` case found during
  planning is exactly the species). Mitigated: Phase 3's bottom-up subset lints
  surface every one mechanically; the resolution rule is decided in advance.
- **In-flight agent work** holding paths to old filenames (worktrees, staged copies).
  Mitigated: land between waves; the rename is `git mv`, so rebase resolves renames.
- **External references** to `stdlib/<file>.plato` (tracker issues, memory notes,
  older docs) go stale. Accepted: the suffix mapping is mechanical
  (`X.plato` → `<folder>/X.types.plato`), and docs inside the repo are updated in
  Phases 1/4.
