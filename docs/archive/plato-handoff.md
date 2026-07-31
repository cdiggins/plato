> **SUPERSEDED, ARCHIVED 2026-07-16** — superseded by plato-execution-plan-2026-07-09.md. Historical record; do not execute.

# Plato Workstream — Handoff Document

*Written 2026-07-08 for a successor agent/model taking over execution. Read this fully before doing
anything. When this document conflicts with older docs, THIS document wins.*

---

## 1. The overall goal, in three sentences

Plato is Christopher Diggins' pure language for writing geometry libraries once and compiling them to
many targets (C# today; TypeScript/GLSL/Rust later). The library had never been systematically verified;
we built the verification, modernized and optimized the C# code generation, and are now working through:
affine builder types → a GLSL proof of concept → fixing all 36 catalogued bugs. The guiding principle:
**every change is proven by an automated gate before it is committed — no exceptions.**

## 2. Priorities, in order (the user has explicitly approved this sequence)

1. **Phase 6 — affine `List<T>`/`Buffer<T>`** (may already be complete when you take over — see §5).
2. **Phase 7 — GLSL proof of concept** (§6).
3. **Phase 4 — bug-fix wave**: compiler associativity fix FIRST, then the 36-entry manifest (§7).
4. Backlog after that, in no fixed order (§8): consolidate/cleanup, then stop and ask the user.

**Cancelled — do not do:** the "2.6/2.7 switchover" of Ara3D.Geometry to V2 output. The user will build
a new library from scratch later; V2 (`submodules/Plato/golden/Plato.Generated.V2`) is its reference
shape only. Double-precision emitting is DEFERRED — do not start it.

## 3. Where everything is

| Thing | Location |
|---|---|
| Monorepo root (studio repo) | `C:\Users\cdigg\git\studio` |
| Plato compiler + stdlib + intrinsics + conformance (own git repo, branch `main`) | `submodules\Plato` — **read its `CLAUDE.md` before working there** |
| SDK consuming Plato output (own git repo, branch `main`) | `ara3d-sdk` — read its `CLAUDE.md` |
| Execution plan + status (the log of everything done) | `docs\plato-roadmap.md` |
| Bug catalog (what Phase 4 fixes) | `docs\plato-library-review.md` §1 + §8 |
| Compiler associativity bug diagnosis (the Phase 4 opener) | `docs\plato-assoc-bug-diagnosis.md` |
| Future library content ideas (NOT current work) | `docs\plato-library-roadmap-ideas.md` |
| Gate battery (run after every mission) | `.\tools\check-all.ps1` from the studio root |
| Regeneration + drift gate | `.\tools\regen-plato.ps1` (`-Apply` to sync) |
| Conformance regen scripts | `.\tools\regen-conformance{,-v2,-opt,-scalar}.ps1 -Test` |

## 4. Non-negotiable rules (violating these has broken things before)

1. **Never push to any remote.** Local commits only. The user reviews and pushes.
2. **Commit per repo, in dependency order**: Plato repo first, then ara3d-sdk (if touched), then studio
   (which bumps the submodule pointers). Commit messages end with
   `Co-Authored-By: Claude Fable 5 <noreply@anthropic.com>` (update the model name to your own).
3. **Never touch the user's working state**: `Ara3D.Studio.sln`, `ara3d-sdk\Ara3D.SDK.sln`,
   `ext\Ara3D.IfcLoader`, `wip\`, `tests\Ara3D.IfcMeshingComparison`, `diff2.json`, the `parakeet`
   sub-submodule (dirty; never stage), `submodules\Plato\.temp\`, the untracked
   `Plato.AST\PlatoDeclarationWriter.cs` / `PlatoFormatOptions.cs`, and the modified Plato-repo
   `.gitignore` and `CLAUDE.md` header (user edits, uncommitted). When staging, ALWAYS name files
   explicitly — never `git add -A` at repo root.
4. **The Plato CLI exits 0 even when compilation fails.** After any generation, verify the output file
   count (~165+ .g.cs) and build the result. Never trust the exit code alone.
5. **Off-flag emitter output must stay byte-identical** — `regen-plato.ps1` is the gate. If your change
   alters default-mode output unintentionally, that is a bug in your change.
6. `Plato.Intrinsics` source of truth is in the Plato repo; ara3d-sdk holds a synced copy. Edit only the
   Plato-repo copy, then `regen-plato.ps1 -Apply` to sync. Never let them diverge.
7. **Mission protocol** (details in the Plato repo `CLAUDE.md`): keep a `PROGRESS.md` while working
   (sessions get killed by usage limits — it makes resume cheap); finish by writing the roadmap DONE
   note, a `COMMIT_MSG.txt` draft, and running `check-all.ps1`; report ≤300 words.
8. **When unsure, stop and ask the user.** Specifically stop for: anything that changes shipped behavior
   outside the approved Phase 4 list; any design decision not already recorded; any gate that fails for
   a reason you cannot fully explain.

## 5. Step 1 — Phase 6 affine types (check state first)

A mission was launched for this (agent may have finished, crashed, or partially completed). Determine
the state:

```powershell
# Signs of completion: these exist and check-all passes
ls submodules\Plato\plato-src\unique.plato
ls submodules\Plato\plato-test-src\unique.algorithms.plato
ls submodules\Plato\docs\affine-types.md
ls submodules\Plato\COMMIT_MSG.txt     # draft commit message = mission finished
.\tools\check-all.ps1
```

- **If complete and gates pass:** commit using `COMMIT_MSG.txt` (then delete that file), stage only the
  mission's files (grammar/compiler edits, new intrinsics files, `unique.plato`,
  `unique.algorithms.plato`, `affine-types.md`, conformance changes, plus the `-Apply`-synced additive
  files in `ara3d-sdk\src\Plato.Generated` and `src\Plato.Intrinsics`), commit all three repos, done.
- **If partial:** read its `PROGRESS.md` (or infer from `git status`), and continue the mission. The
  full specification is in `docs\plato-roadmap.md` Phase 6 plus the design decisions:
  **one keyword** (`unique type List<T> { }`, declaration-site only, only List/Buffer allowed);
  runtime-checked (frozen flag, use-after-freeze throws); `Freeze` returns `IArray<T>` without copying;
  mutators return the builder; three validation algorithms in `plato-test-src` (ear-clipping,
  polygon extrusion, Filter) with `Witness_` tests; conformance totals grow beyond 178 with zero
  regressions in the existing 142 pass / 36 ignored-known.
- **If nothing was done:** execute that specification from scratch.

## 6. Step 2 — Phase 7 GLSL proof of concept

Goal: prove one Plato source compiles to a *third* target. Keep scope minimal — this is a PoC.

1. New additive file `submodules\Plato\plato-src\sdf3d.plato`: a `library Sdf3D` of ~10 signed-distance
   functions from https://iquilezles.org/articles/distfunctions/ — Sphere, Box, RoundBox, Torus,
   Capsule, CappedCylinder, plus `OpUnion/OpSubtract/OpIntersect` (min/max/−) and smooth-min
   (`OpSmoothUnion`, polynomial k-blend). Free functions over `Vector3`/`Number` ONLY — no `IArray`,
   no `String`, no tuples-as-types beyond what GLSL vec3 handles. Each function cites its URL in a
   comment. IMPORTANT: this is additive to plato-src → after adding, run `regen-plato.ps1 -Apply`
   (additive-only check), build the SDK, run `check-all.ps1`. Add 3–5 `Witness_` tests in a new
   `plato-test-src\sdf.witness.plato` (e.g. `SdSphere(origin, 1) == -1`).
2. New writer project `submodules\Plato\Plato.GlslWriter\` + CLI flag `--glsl`. Translate ONLY the
   supported subset: concrete types with Number fields → `vec2/vec3/vec4/float/int/bool` (Vector2/3/4
   map to native GLSL vectors; Angle → float radians), library functions → free GLSL functions
   (UFCS flattened: `p.Length` → `length(p)` where an intrinsic mapping exists, else emit a helper),
   no-arg functions → zero-arg functions. Maintain an explicit intrinsic-mapping table
   (Sqrt→sqrt, Dot→dot, Length→length, Min/Max→min/max, Clamp→clamp, Abs→abs, Lerp→mix…).
   **Reject everything else with a clear diagnostic naming the construct and source location** — do
   not attempt generality. Acceptance: `--glsl` over a folder containing ONLY sdf3d.plato (+ its
   dependencies' subset) produces a `.glsl` file that passes validation with a GLSL validator if one
   is available (`glslangValidator` — check availability; if absent, gate on careful manual review +
   the CPU conformance step below).
3. Conformance: write a small C# test (new project `submodules\Plato\conformance\GlslPoC.Tests` or a
   console check) that evaluates each SDF at ~20 seeded sample points via the C# build and compares
   against the same formulas evaluated by a tiny C# *interpreter of the emitted GLSL* — OR, simpler and
   acceptable: verify the emitted GLSL text against golden expected output checked into
   `submodules\Plato\golden\glsl\` (golden-file test). Choose the simpler path; state which.
4. Optional stretch (skip if any friction): a single-file HTML raymarcher embedding the generated GLSL,
   saved under `submodules\Plato\demos\`. Do not spend more than one session on this.

## 7. Step 3 — Phase 4 bug wave (approved by the user; this changes shipped behavior)

### 7a. The compiler associativity fix — do this FIRST and SEPARATELY

Read `docs\plato-assoc-bug-diagnosis.md`. The fix (already prototyped once and reverted):
`submodules\Plato\Plato.AST\AstNodeFactory.cs` — (1) the precedence rebalance uses strict `<` where it
needs `<=` (~line 24; all Plato binary operators are left-associative); (2) prefix operators must be
applied BEFORE the postfix loop folds binary operations (~lines 118–122).

Procedure:
1. In `submodules\Plato\conformance\Ara3D.SDK.ConformanceTests\KnownFailures.json`, REMOVE the five
   associativity entries (`Witness_SmoothStepAtHalf`, `Witness_SmoothStepAtOne`,
   `Witness_HermiteStartsAtP0`, `Witness_HermiteEndsAtP1`, `Witness_CatmullRomEndsAtP2`).
2. Apply the two AstNodeFactory fixes.
3. `regen-plato.ps1` will now show diffs — EXPECTED: ~16 files / ~93 members (32 behavior-changing:
   SmoothStep, Hermite±Derivative, CatmullRom±Derivative, Quadratic/CubicBezierSecondDerivative,
   vector `FromOne`; the rest are value-identical re-groupings). If the diff is wildly larger,
   stop and investigate. Then `-Apply`.
4. Run `check-all.ps1`. Expect: the five formerly-quarantined witnesses now pass; everything else
   unchanged. If any OTHER KnownFailures entry starts passing, verify it is genuinely downstream of the
   associativity fix before removing it from the manifest (likely candidates: none known — investigate).
5. Commit (Plato repo: compiler fix + manifest + conformance; ara3d-sdk: applied regen; studio: bump).

### 7b. The manifest burn-down — one commit per fix

For EACH remaining entry in `KnownFailures.json` (31 after 7a), strictly in this loop:
1. Remove the manifest entry → run the relevant conformance suite → confirm the test now FAILS
   (if it passes already, the entry was stale — note it and move on).
2. Apply the source fix. The fix for every entry is specified in `docs\plato-library-review.md`
   (§1.1–1.9 and §8) — file, line, and corrected expression are all written out. Fix in
   `submodules\Plato\plato-src\*.plato` ONLY (never the generated C#).
3. `regen-plato.ps1 -Apply` → `check-all.ps1` → all green → commit all repos with the review-doc
   section reference in the message.

Suggested order (dependency-aware): §1.1 MagnitudeSquared first (it unblocks 10 entries:
Magnitude/Length laws + Normalize + Vector3MagnitudeOf236Is7 + Triangle3DArea) → §1.2 constants →
§1.4 SmootherStep + Barycentric → §1.5 the six curve formulas → §1.6 Bounds2D.Corners + duplicate
Points → §8.2 ArcMinutes/ArcSeconds → §8.3 Time stubs (implement the four IMeasure obligations for
Time in plato-src, mirroring how other measures do it) → §8.4 Triangle2D signed area →
`Point2D.Subtract(Vector2)` stub (mirror the Point3D fix from Phase 0).

**Three entries need a decision the user has NOT made — write a short ADR in `docs\` and ASK, do not
just fix**: TRS/Pose composition order (§1.7), removing the implicit `Number→Angle` conversion
(§1.10 — breaking change), Magnitude-vs-Length naming unification (§1.1 second-order question).

After the burn-down: `KnownFailures.json` should be empty or contain only the three ADR-blocked
entries; conformance counts become ~(178+new) pass / ≤3 ignored / 0 fail. Also fix the handwritten C#
bugs from §1.11 (`Within` overloads, `LineLineDistance` clamp, the 1e-15-vs-float epsilon) in
`ara3d-sdk\src\Ara3D.Geometry\GeometryUtil.cs` — plain C# edits, gated by SDK build + GeometryTests.
Finally: flip the linter to `--strict` in `check-all.ps1` ONLY if LINT counts are addressed — otherwise
leave as-is and note remaining counts.

## 8. Backlog after the above (ask the user before starting any)

- Harvest KitchenSink (easings, InverseLerp/Remap, correct cycloids) into plato-src, then delete it —
  see `docs\plato-library-review.md` §3/§4.
- The bloat-removal list (review §3) and 2D/3D parity additions (review/ideas docs).
- Deterministic PRNG + noise library (prerequisite for scatter/jitter content).
- The static affine occurrence-counting pass (rides the future type-checker).
- Double precision (`--scalar=double` + `Plato.Intrinsics.Double`, namespace `Ara3D.Geometry.Double`).
- The new from-scratch library (user-led; V2 golden output is its reference shape).

## 9. Current committed state (as of this handoff)

All commits are LOCAL (nothing pushed). Studio history tells the story:
`d906ec7` P0 → `e29f46c` P1 harness → `9cc4ed6` linter+diagnosis → `fad7ee9` P2 emitter →
`18d89dc` API diff → `d7b3753` P3.1 optimizer → `ef38be9` repo restructuring → `5b5f69d` classic
extension methods → `0ab560b` process enablers → `84277c5` direction change → `38ee567` scalar erasure.
Gate battery state at handoff: **8/8 PASS** (regen-diff, lint, conformance V1/V2/Opt/Scalar, SDK build,
GeometryTests). Baseline tags: `plato-generated-baseline-2026-07` (ara3d-sdk),
`plato-roadmap-baseline-2026-07` (studio).

Open user-side items (do not action, just be aware): the Plato repo `.gitignore` slimming and `CLAUDE.md`
header edit are uncommitted user changes; `Ara3D.Studio.sln` references two projects at pre-move paths.
