> **EXECUTED 2026-07-28** — plato-263 closed. Plato commits: `78a70cc` (de-number), `3e8f93b` (folders), `e10ffbd` (plan ticks). Studio: `6174ec0`. Archived to `docs/archive/`.
# plato-263 — stdlib rename plan (folders + v3 file de-numbering)

Date: 2026-07-28. Author: session with Christopher. Tracker: `tracker/issues/plato-263.md`.
Status: **approved for execution** — name pair and file-naming rules were decided by the user;
do not re-open those decisions.

## Goal

Two renames, one mission:

1. **Folder renames** (plato-263): the stdlib generations get role-revealing names.
2. **File renames inside the v3 tree**: drop the `NN-` number prefixes; kind suffixes for
   pure files; reading order moves from filenames to the README.

No semantic change anywhere. No content moves between files (one exception: none — "suffix
pure files only" was chosen precisely to avoid splits). No recipe, codegen, or compiler
behavior change.

## Decisions (locked — user-approved 2026-07-28)

| Decision | Choice |
|----------|--------|
| Name pair | `stdlib` ← `plato-src-v3` (forward vocabulary owns the clean name); `stdlib-legacy` ← `plato-src` (still ships, drives `Plato.Generated`) |
| Test tree | `stdlib-legacy-tests` ← `plato-test-src` (its laws/witnesses compile against the legacy tree) |
| Frozen snapshot | `stdlib-snapshot-2026-07-09` ← `plato-src-legacy` (avoids two folders named "legacy"; content stays byte-frozen, rename only) |
| File-kind pattern | Dot form: `X.concepts.plato` / `X.library.plato`. Mixed domain files stay plain `X.plato`. No `.types` suffix exists yet (no pure-types file warrants it today) |
| Mixed files | **No splits.** A file containing concepts + types keeps the plain domain name |
| Subfolders | None added. `concept-library/` stays a subfolder (it is deliberately excluded from `lint stdlib` because `Plato.CLI` enumeration is non-recursive — preserve that) |
| Reading order | Ordered file index in `stdlib/README.md` replaces numeric prefixes |

Rationale highlights (for the record, not for re-litigation): "legacy" ships but is not the
forward direction — each folder's README banner states its role in the first line;
number prefixes cost renumber churn, tab-completion noise, and collapse past ~100 files;
dot suffixes glob cleanly (`*.concepts.plato`) while every file still ends `.plato` so
`GetFiles("*.plato")`, lint, and editor associations are untouched.

## Preconditions

- **Worktree check (do this first).** `submodules/Plato/.claude/worktrees/cpp-opencl-writer`
  holds a full copy of `plato-src-v3`. If that branch is still in flight, either land it
  first or get explicit user sign-off to proceed and accept rename-vs-edit conflicts.
- Clean commit point for the files about to change (per repo rules). Do NOT touch
  `parakeet/` (pre-existing dirty sub-submodule) or any pre-existing dirty files.
- Baseline gates green before starting: run
  `dotnet run --project submodules\Plato\Plato.CLI -c Release -- lint submodules\Plato\plato-src-v3`
  (expect 0 parse / 0 resolution errors) and note the current conformance pass count.

## Part 1 — file renames inside `plato-src-v3/` (do this BEFORE the folder rename, separate commit)

All renames via `git mv` so history follows.

### Rule

- `NN-concepts-X.plato` → `X.concepts.plato` (pure concept files)
- every other `NN-X.plato` → `X.plato`
- `concept-library/NN-X.library.plato` → `concept-library/X.library.plato`

### Explicit map (top level, 71 files)

| From | To |
|------|----|
| 00-primitives.plato | primitives.plato |
| 01-concepts-core.plato | core.concepts.plato |
| 02-concepts-algebra.plato | algebra.concepts.plato |
| 03-concepts-collections.plato | collections.concepts.plato |
| 04-concepts-functional.plato | functional.concepts.plato |
| 05-numbers.plato | numbers.plato |
| 06-quantities.plato | quantities.plato |
| 07-time.plato | time.plato |
| 08-vectors.plato | vectors.plato |
| 09-matrices.plato | matrices.plato |
| 10-rotations.plato | rotations.plato |
| 11-points.plato | points.plato |
| 12-intervals-bounds.plato | intervals-bounds.plato |
| 13-transforms.plato | transforms.plato |
| 14-color.plato | color.plato |
| 15-concepts-geometry.plato | geometry.concepts.plato |
| 16-lines.plato | lines.plato |
| 17-planar-shapes.plato | planar-shapes.plato |
| 18-spatial-primitives.plato | spatial-primitives.plato |
| 19-polygons.plato | polygons.plato |
| 20-concepts-curves-surfaces.plato | curves-surfaces.concepts.plato |
| 21-curves-2d.plato | curves-2d.plato |
| 22-curves-3d.plato | curves-3d.plato |
| 23-splines.plato | splines.plato |
| 24-surfaces.plato | surfaces.plato |
| 25-solids.plato | solids.plato |
| 26-fields.plato | fields.plato |
| 27-implicit-sdf.plato | implicit-sdf.plato |
| 28-noise.plato | noise.plato |
| 29-sampling-grids.plato | sampling-grids.plato |
| 30-topology.plato | topology.plato |
| 31-meshes.plato | meshes.plato |
| 32-mesh-attributes.plato | mesh-attributes.plato |
| 33-pointclouds-voxels.plato | pointclouds-voxels.plato |
| 34-spatial-structures.plato | spatial-structures.plato |
| 35-spatial-queries.plato | spatial-queries.plato |
| 36-easing.plato | easing.plato |
| 37-keyframes-tracks.plato | keyframes-tracks.plato |
| 38-skeletal-animation.plato | skeletal-animation.plato |
| 39-motion-graphics.plato | motion-graphics.plato |
| 40-paths.plato | paths.plato |
| 41-vector-styling.plato | vector-styling.plato |
| 42-text.plato | text.plato |
| 43-scene2d.plato | scene2d.plato |
| 44-color-spaces.plato | color-spaces.plato |
| 45-images.plato | images.plato |
| 46-image-processing.plato | image-processing.plato |
| 47-texturing.plato | texturing.plato |
| 48-cameras.plato | cameras.plato |
| 49-lights.plato | lights.plato |
| 50-materials.plato | materials.plato |
| 51-scene3d.plato | scene3d.plato |
| 52-render-settings.plato | render-settings.plato |
| 53-kinematics.plato | kinematics.plato |
| 54-rigid-dynamics.plato | rigid-dynamics.plato |
| 55-collision.plato | collision.plato |
| 56-joints-constraints.plato | joints-constraints.plato |
| 57-particles-simulation.plato | particles-simulation.plato |
| 58-statistics.plato | statistics.plato |
| 59-random.plato | random.plato |
| 60-signals.plato | signals.plato |
| 61-polynomials.plato | polynomials.plato |
| 62-optimization.plato | optimization.plato |
| 63-uncertainty.plato | uncertainty.plato |
| 64-differential-geometry.plato | differential-geometry.plato |
| 65-graphs.plato | graphs.plato |
| 66-engineering.plato | engineering.plato |
| 67-scientific-data.plato | scientific-data.plato |
| 68-geo-spatial.plato | geo-spatial.plato |
| 69-higher-dimensions.plato | higher-dimensions.plato |
| 70-intrinsics.plato | intrinsics.plato |

If the directory has drifted from this list (files added/removed since 2026-07-28), apply
the rule, not the table, and note the drift in the commit message.

### concept-library/ (9 files)

`01-core-algebra.library.plato` → `core-algebra.library.plato`, and likewise for
`03-collections-functional`, `06-numeric-structures`, `12-intervals-transforms`,
`15-geometry`, `20-curves-surfaces`, `26-fields-implicits`, `30-meshes-spatial`,
`36-domain-traits`.

### README rewrite (same commit)

`plato-src-v3/README.md` currently encodes order via numbers ("Layers and file map" table
uses ranges like "00-14"; the cross-domain registry cites owner files as "15 (A)"). Rewrite:

1. Replace the layer table's numeric ranges with explicit file-name lists, same layer
   grouping, same order. This ordered index is now the canonical reading order — say so
   in one sentence above the table.
2. In the "Cross-domain name registry" table, replace numeric owner refs with file names
   (`15 (A)` → `geometry.concepts.plato`). Keep the agent letters if they still serve.
3. Update the "Foundation files" bullet list names (`00-primitives.plato` → `primitives.plato`, ...).
4. Update the intrinsics-policy sentence that cites `70-intrinsics.plato` → `intrinsics.plato`,
   and the self-containment sentence citing `00-primitives.plato`.
5. Add one "Naming" bullet to Conventions: files are `domain.plato`; pure concept files
   `domain.concepts.plato`; library files `domain.library.plato`; no number prefixes;
   reading order lives in this README's layer table.
6. `concept-library/ARCHITECTURE.md`: update any numbered file references the same way.
7. `ItemIndex` convention bullet says "(file 05)" — change to "(numbers.plato)".

### Other references to numbered v3 filenames

Grep and fix live docs only:

```
rg -l "0[0-9]-|[1-6][0-9]-|70-intrinsics" submodules/Plato/docs submodules/Plato/lessons --glob "*.md"
```

Update: `docs/SEMANTICS.md`, `lessons/v1/*.md` where they cite numbered v3
files, `docs/plato-257-lessons-v1-*.md` if still live. Leave `docs/archive/**` untouched.
Studio-side: `docs/reports/plato-v3-vocabulary-report.md` cites numbered files — update; leave
closed tracker issues and `tracker/DONE.md` untouched (historical record; policy below).

### Gate for Part 1

```
dotnet run --project submodules\Plato\Plato.CLI -c Release -- lint submodules\Plato\plato-src-v3
```

0 parse errors, 0 symbol-resolution errors, finding count unchanged from baseline.
Commit (Part 1 = one commit in the Plato submodule, pathspec only the renamed/edited files).

## Part 2 — folder renames (separate commit(s))

`git mv` in the Plato submodule:

- `plato-src-v3` → `stdlib`
- `plato-src` → `stdlib-legacy`
- `plato-test-src` → `stdlib-legacy-tests`
- `plato-src-legacy` → `stdlib-snapshot-2026-07-09` (rename ONLY; contents remain frozen —
  never regenerate or edit anything inside)

### README banners (first line of each folder's README, add if missing)

- `stdlib/README.md`: "**Forward stdlib vocabulary** — declarations only, no bodies yet.
  Codegen and Studio still ship from `stdlib-legacy`."
- `stdlib-legacy/README.md`: "**Shipping stdlib** — drives `Plato.Generated` and Studio.
  Writable. New *vocabulary* goes in `stdlib/`; runtime/body fixes go here."
- `stdlib-legacy-tests/README.md`: one line: law/witness libraries for `stdlib-legacy`.
- `stdlib-snapshot-2026-07-09/`: add a one-line `README.md` if absent: frozen pre-refactor
  snapshot, reference only.

### Reference sweep — Plato submodule

Find everything:

```
rg -l "plato-src|plato-test-src" submodules/Plato --glob "!parakeet/**" --glob "!.claude/**"
```

Known touch points (verified 2026-07-28; the grep is authoritative if drifted):

**Code / config (behavioral — must be right):**
- `Plato.CLI/Config.cs:10` — default `InputFolder` = `"plato-src"` → `"stdlib-legacy"`
  (codegen default stays the tree with bodies).
- `vscode-plato/src/extension.ts:13` — `CORPUS_DIR_NAMES = ["plato-src", "plato-test-src", "plato-src-v3"]`
  → `["stdlib", "stdlib-legacy", "stdlib-legacy-tests"]`. Rebuild `out/` (`npm run compile`
  in `vscode-plato/`) so `out/extension.js` matches; update `vscode-plato/README.md` examples.
- `Plato.Navigation.Tests/Corpus.cs` and `IncrementalIndexerTests.cs` — corpus paths.
- `PlatoTests/CheckerTestSupport.cs`, `CheckerDiagnosticsSummaryTests.cs` — source paths.
- Regen scripts + csprojs that compile from `plato-src` by path:
  `Small/regen-small.ps1` + both `Small/**/*.csproj`, `csg/regen-csg.ps1` +
  `csg/Ara3D.Csg.Tests/CsgSupport.cs`, `earcut/regen-earcut.ps1` +
  `earcut/Ara3D.Earcut.Tests/EarcutSupport.cs`, `optimizer-smoke/regen-smoke.ps1` + its
  csprojs, `Generated/Plato.Generated.{Unoptimized,Optimized}.csproj`,
  `demos/rust/geometry-samples/gen-plato.ps1`, `demos/typescript/geometry-samples/package.json`,
  `tools/export-types-context.ps1`.
- `Plato.CSharpWriter/*.cs`, `Plato.Intrinsics*/**.cs`, `Plato.CppWriter.Tests/PlatoSource.cs` —
  most hits are comments; fix the strings that are paths, fix comments opportunistically.

**Docs / guides:**
- `submodules/Plato/CLAUDE.md` — Layout section rewrites (`plato-src` → `stdlib-legacy` etc.),
  the lint example command, and the frozen-snapshot bullet. State the mapping once:
  "stdlib = forward vocabulary (declarations); stdlib-legacy = shipping generation".
- `docs/plato-for-agents.md`, `docs/plato-library-map.md` — same mapping, same sweep.
- `Plato.Navigation/README.md` — root discovery defaults/examples.
- `plato-src-v3/README.md` title line ("plato-src-v3 — ...") → "stdlib — ...".

### Reference sweep — studio repo

```
rg -l "plato-src|plato-test-src" tools tests labs docs .claude CLAUDE.md AGENTS.md
```

Known touch points:
- `tools/check-all.ps1`, `tools/regen-conformance.ps1`, `tools/regen-generated.ps1` — paths.
- `labs/PlatoNavigationMcp/Program.cs:66-67` — root discovery
  (`plato-src` / `plato-test-src` → `stdlib-legacy` / `stdlib-legacy-tests`; decide whether
  to add `stdlib` as a third root — default NO for now, v3 indexing is opt-in per plato-256;
  note the choice in the commit message).
- `tests/PlatoNavigationMcp.Tests/ServerFixture.cs` — corpus path.
- `labs/PlatoNavigationMcp/README.md`, `eval/*.md` — examples.
- Studio `CLAUDE.md` / root `AGENTS.md` / `.claude/skills/plato-mcp/**` if they name the
  folders (grep decides).
- Live docs (`docs/plato-*.md` not in `docs/archive/`) that give commands with the old paths.

**Historical-reference policy:** `docs/archive/**`, closed tracker issues, `tracker/DONE.md`,
and dated session reports keep old names — they describe the past. Update only live docs,
agent guides, and anything an agent would execute.

### Gates for Part 2 (all must pass before commit)

1. `dotnet run --project submodules\Plato\Plato.CLI -c Release -- lint submodules\Plato\stdlib` — 0/0.
2. `dotnet run --project submodules\Plato\Plato.CLI -c Release -- lint submodules\Plato\stdlib-legacy` — matches baseline.
3. `.\tools\check-frozen-v1.ps1` — must still pass untouched (it tracks Intrinsics/Generated
   artifacts, not the renamed folders; if it fails, STOP — something was edited, not renamed).
4. `.\tools\regen-conformance.ps1 -Test` — 0 fail, pass count = baseline.
5. `.\tools\regen-generated.ps1` — no diff (rename must not change emitted output).
6. `dotnet test` on `Plato.Navigation.Tests` and `PlatoTests`; studio-side
   `tests/PlatoNavigationMcp.Tests`.
7. `.\tools\check-all.ps1` once at the end.

## Commit / push plan

Per repo rules: current branch (no new branches), pathspec commits only, push after each.

1. Plato submodule commit A — Part 1 file renames + README rewrite + numbered-name doc fixes.
2. Plato submodule commit B — Part 2 folder renames + submodule-side reference sweep.
   (A+B may be two commits or B split further; each gated.)
3. Studio commit — tools/labs/tests/docs sweep + Plato submodule pointer bump, single commit.
4. Close `plato-263` (`python tools/track.py close plato-263 --outcome "..."`) noting the
   chosen names and this plan doc; also record in the issue that the v3 de-numbering shipped
   with it.

## Out of scope (explicitly)

- Merging the two libraries, moving content between files, changing recipes or codegen flags.
- Renaming `Plato.Generated` projects (adjacent idea in plato-263 — file separately if wanted).
- Adding `stdlib` to navigation/MCP default roots (plato-256 owns dual-root behavior).
- Anything under `parakeet/` or inside `stdlib-snapshot-2026-07-09/`.

## Done means

- [x] All v3 files renamed per map; no `NN-` prefixes remain; lint 0/0
- [x] `stdlib/README.md` carries the ordered layer index + naming convention bullet
- [x] Four folders renamed; banners in place; mapping stated once in CLAUDE.md + plato-for-agents.md
- [x] Reference sweeps done (both greps return only archive/historical hits)
- [x] vscode-plato rebuilt; navigation + checker + MCP tests green
- [x] regen-generated no-diff; conformance 0 fail; check-frozen-v1 green; check-all green
- [x] Commits pushed; plato-263 closed with outcome
