> **ARCHIVED 2026-07-16** — analysis distilled into plato-execution-plan-2026-07-09.md. Historical record.

# Plato — Roadmap Reassessment

*Written 2026-07-09 for Christopher Diggins. A status-and-direction reassessment
of the whole Plato workstream against the two goals stated below. This is an
analysis and a high-level roadmap proposal — not an execution plan, and not a
decision. Where it recommends, it recommends; the decisions are yours.*

**The two current goals (as stated):**

1. **Improve the standard library** so it can do what the C# geometry library,
   Sample generators, and modifiers already do — getting that logic into Plato
   for consistency, maintainability, and multi-platform reach; fixing the
   inconsistencies, improving the types, and embedding more domain knowledge.
2. **Improve the quality and performance of the output.**

**Source-of-truth note.** The authoritative docs are `plato-overview.md`
(your vision), `docs/plato-roadmap.md` (execution log + decisions),
`docs/reports/plato-library-review.md` (verified bug catalog), and the Plato source
itself. `docs/archive/recommendation2.md` and `docs/archive/standard-library-recommendations.md`
are **AI-generated idea banks** — now banner-marked as such — and are cited here
only as proposals, never as settled direction.

---

## 1. What has been done so far

The last several weeks built a great deal of **infrastructure and codegen
machinery**, and essentially **zero library correctness or content**. That split
is the single most important fact for this reassessment.

### Done — infrastructure & tooling (Phases 0–3, 6)

| Area | State | Evidence |
|---|---|---|
| **P0 Safety net** | ✅ Done | Golden baseline tags; `regen-plato.ps1` byte-identity + intrinsics-sync gate; source⇄generated drift reconciled (163 files identical); API snapshot (550 types / 8,334 members); BenchmarkDotNet baseline; Sphere/Cylinder collision documented |
| **P1 Verification harness** | ✅ Done (incl. linter) | `plato-test-src` laws + witnesses; conformance **142 pass / 36 ignored-known / 0 fail**; `KnownFailures.json` (36 entries = the Phase 4 queue); `plato lint` LINT001–005 (246 baseline findings); associativity bug diagnosed |
| **P2 Extension-method emitter (V2)** | ✅ Done (2.1–2.5); switchover **cancelled** | `--csharp-style=extensions` (now classic extension methods, no C# 14 dependency); repo restructuring (intrinsics/conformance/golden moved into Plato repo); `--scalar=wrapper\|float` erasure; `golden/Plato.Generated.V2` in the adoption shape |
| **P3 Optimizer, stage 1** | ⚠️ Only 3.1 done | `--optimize` component-op unrolling: Lerp 31.5×, ClampZeroOne 11×, allocations → 0. 3.2–3.6 (beta reduction, const-folding, CSE/SinCos fusion, IArray devirt, hygiene) not started |
| **P6 Affine `unique` types** | ⚠️ Runtime slice done | `unique type List<T>/Buffer<T>`, runtime-checked freeze, `plato-src/unique.plato`, validation proofs. Static occurrence-counting pass (6.6) deferred to the type checker |

Gate battery is green (8/8) and every change shipped behind a differential gate.
This is disciplined, high-quality engineering. The verification harness in
particular is exactly the "make the library verifiable" investment the review
argued for, and it already paid off — it found four defects beyond the manual
review (right-associative additive chains, ArcMinutes/ArcSeconds, Time stubs,
Triangle2D signed-area sign).

### Not done — the actual library

- **Phase 4 (bug-fix wave) has not started.** All ~36 catalogued math bugs are
  still shipping in `plato-src`, including the worst one (`MagnitudeSquared`
  divides by component count) and the **compiler** associativity bug that makes
  any `+`/`−` chain emit wrong.
- **No library content has been added** beyond `unique.plato`. The stdlib is
  still the same ~3,500 lines / 25 files. None of the SDF / surfaces / deformers
  / PRNG / curve-machinery content from `../reports/plato-library-roadmap-ideas.md` exists.
- **Phase 5 (double precision)** deferred. **Phase 7 (GLSL PoC)** planned, not
  started. **Full type checker** deferred.

**One-sentence summary:** the workstream built the scaffolding to safely change
the library, then stopped before changing the library.

---

## 2. What is still currently planned

Per the handoff doc (which explicitly supersedes older docs), the approved order is:

1. **Phase 6 affine types** — now effectively done.
2. **Phase 7 — GLSL proof of concept** (next): a `sdf3d.plato` of ~10 IQ
   primitives + a `Plato.GlslWriter`, proving one source → a third target.
3. **Phase 4 — bug-fix wave**: compiler associativity fix *first*, then burn down
   the 36-entry manifest (three entries need an ADR + your decision: TRS/Pose
   composition order, removing the implicit `Number→Angle`, Magnitude/Length
   unification).
4. **Backlog (unordered, ask first)**: harvest KitchenSink; PRNG + noise; static
   affine pass; double precision; the new-from-scratch C# library; and the
   library-content expansion from `../reports/plato-library-roadmap-ideas.md`.

The decisive observation: **the thing your goal #1 most wants — library content —
is scheduled dead last**, in the "ask first" backlog, behind a portability PoC
and a bug wave.

---

## 3. Inconsistencies & contradictions in the documentation

1. **`Amount` vs `Fraction` for the lerp parameter — direct contradiction.**
   `standard-library-recommendations.md` §3 recommends `Amount`. But
   `../design/naming-fraction-and-rational-types.md` argues *against* `Amount` (it is
   already the field name on `UniformScale2D`, `Scale2D`, `Scaling3D`) and
   recommends `Fraction`. Two AI conversations reached opposite conclusions, and
   `Amount` is genuinely already taken. **Needs your ruling.** (Both agree on
   `Rational` for exact p/q, so that half is settled.)

2. **"Fix the 36 bugs in plato-src" vs "build a new library from scratch."** The
   2026-07-08 direction change says `Ara3D.Geometry` keeps V1 indefinitely and a
   new library is built later from the V2 golden shape — yet Phase 4 fixes bugs
   in the *current* `plato-src`. These only conflict if "new library" means
   throwing away `plato-src`. It almost certainly means a new **C# consumer /
   packaging**; `plato-src` is the durable asset that feeds *every* target, so
   fixing it is never wasted. **This should be stated explicitly** — a future
   agent could reasonably read "new library from scratch" as "don't bother fixing
   the old source."

3. **The freeze vs. goal #1's velocity.** Ground rule 1 freezes `plato-src`
   until Phase 4. Additive *new files* are allowed, but most of the
   type/consistency improvements goal #1 wants require *editing existing files*,
   which is gated behind a Phase 4 that hasn't started. The freeze's original
   justification ("protect an unverified library") is now largely discharged by
   the harness — so the freeze is currently costing more than it protects.

4. **Stale conformance counts.** `CLAUDE.md` and `plato-for-agents.md` pin the
   expected result at "142 pass / 36 ignored / 0 fail", but the Phase 6 note
   records the suite has since grown to **185 total tests / 53 witnesses**. The
   canonical number is quoted inconsistently across docs.

5. **Type-surface defects are real but unscheduled.** `recommendation2.md` §1
   flags structural defects — `IBounds<TValue,TDelta>` and
   `IPrimitiveGeometry3D<PrimitiveT>` constrain a nonexistent `T`;
   `IDistanceField` inherits `IProcedural<Vector2,Number>` but declares
   `Distance(Point2D)`; `IPolyLine`/`ICurve1D` are dead concepts with no
   implementors; `IMeasure` isn't additive; `IWholeNumber` inherits a lossy
   `Lerp`. The linter *catches* the constraint bugs (they're in the 246-finding
   baseline), but the `KnownFailures` manifest only tracks **math-law** failures,
   so these structural defects are in no work queue.

6. **`Amount`/hit-record proposals build on a contested name.** The
   standard-library hit records (`Hit3D { … Parameter: Amount }`) depend on the
   unresolved #1. Downstream proposals inherit the ambiguity.

7. **`procedurals.plato` is commented out** — the "most intellectually
   interesting idea in the codebase" (per the overview) and the compositional
   heart of the SDF/effector/deformer vision **does not compile yet**. It is
   blocked on compiler support (function-typed fields, or generic constraints in
   function bodies). Every doc that leans on `IProcedural<TIn,TOut>` as a
   unifier is, today, leaning on something inert.

None of these are damaging on their own; together they mean a future agent could
pick a naming convention that collides, "fix" a library that's about to be
replaced, or treat `IProcedural` as working. Worth a short reconciliation pass.

---

## 4. How the current direction aligns with the goals

**Goal 1 (library content & quality): weak near-term alignment.** The next
planned work (GLSL PoC) is a *breadth-of-targets* bet, not library content. Phase
4 improves correctness but not breadth. The content that maps directly to goal 1
— surfaces (`roadmap-ideas` §2 ↔ your `SurfaceGenerators`), deformers/modifiers
(§3 ↔ your Deformers), cloners/generators (§5 ↔ your Sample generators), SDFs
(§1), PRNG/noise (§9), curve machinery (§7) — is entirely in the deferred
backlog. **The roadmap-ideas doc is, in effect, the spec for goal 1**, and it's
scheduled last.

**Goal 2 (output quality & performance): partial alignment.** Performance:
optimizer 3.1 is a real, measured win, but 3.2–3.6 are unstarted. Quality:
scalar erasure and the extension-method shape are cleaner output (done); *double
precision* — a genuine quality need for your BIM/IFC/large-coordinate world — is
deferred despite the `--scalar` groundwork making it a short step. Correctness
(also a facet of "quality") is Phase 4, unstarted.

**The strategic read:** infrastructure-first was the *right* discipline — you
cannot safely expand a library you cannot verify, and now you can verify it. But
the project is at the classic inflection point where scaffolding can keep
deferring the payload. You've now stated the goals explicitly, which reads as a
signal to pivot from machinery to content.

---

## 5. What's worth reassessing

1. **The GLSL-before-content ordering.** The Phase 7 PoC *needs* an SDF library
   as its content — the handoff even specifies writing a throwaway `sdf3d.plato`
   for it. But the SDF catalog is *also* the #1-ranked library-content item
   (`roadmap-ideas` §13) and serves goal 1 directly. Writing the real SDF
   catalog first means GLSL rides on it for free instead of on a throwaway.
   **The SDF catalog is a shared dependency of both goal 1 and Phase 7** — which
   argues for building it as real content, then doing GLSL on top.

2. **Pull the compiler associativity fix to the very front — ahead of any new
   content.** It is currently the first item *inside* Phase 4, which sits after
   GLSL. But it poisons *any* `+`/`−` chain, including new additive content
   (SDFs, surfaces, curves all mix `+`/`−`). You cannot safely author new library
   content until it's fixed. This is the true first domino, and right now it's
   several phases deep.

3. **The freeze + Phase 4 gating.** With the harness live, fixing the 36 bugs is
   cheap and safe and directly serves goal 1's "fix the inconsistencies." Doing
   it *before* content also honors the review's through-line: *"a bigger library
   on an untested base just multiplies the surface for the next Rose/Lissajous."*

4. **Fold the type-surface defects (§3.5 above) into the correctness work.** They
   won't surface as law failures, so they need to be explicitly queued, not left
   to rot in the lint baseline.

5. **The full type checker's ROI just went up.** The overview argues it's the
   highest-ROI feature for agent-maintainability. If goal 1 means authoring a
   *lot* of new Plato, a fast, source-anchored checker accelerates every line of
   it — the current lint pass is a down payment, not the asset. Worth
   reconsidering its deferral in light of the content push.

6. **Guard against semantic-type explosion.** The AI docs propose *many* new
   types (Direction/Normal/UnitVector, Tolerance, Optional/Result,
   Amount/UnitAmount/Percent/Probability, a full measures layer) *and* domain
   wishlists (DSP, electronics, ML, genetics). The review §3 warns the opposite
   direction: the library already has functionless types that are liabilities.
   The reconciling discipline: **a new type earns its place only with the
   functions and laws that consume it.** The genetics/DSP/electronics wishlists
   are out of scope for a geometry/numerics kernel and should be explicitly
   declined (or parked far away), not carried as latent roadmap.

7. **Double precision vs. your actual domain.** You live in BIM/IFC
   (large coordinates). `Number = float` is a real correctness ceiling there, the
   overview calls it "conspicuously missing," and `--scalar` already did the hard
   plumbing. This may deserve promotion from "deferred" on goal-2 grounds.

---

## 6. Design decisions & next steps worth considering

These are the decisions that gate the content work; several are yours to make.

- **Unblock function-valued fields / `procedurals`, or commit to combinator
  types.** This is the keystone for SDF booleans, domain warps, falloff
  effectors, curve reparameterization — the compositional layer under half of
  goal 1. Route 1 (compiler support for function-typed fields) is the real fix;
  route 2 (pure combinator types, monomorphized at the use site) works for
  statically-known types — which is *exactly* the GLSL case — but not a dynamic
  C# scene graph. A pragmatic split: combinator types now (unblocks SDF + GLSL),
  function fields later (unblocks dynamic composition). **Decision needed.**

- **`Option<T>` / `Result<T,E>` (library-level, no language change).** Both AI
  docs and the review flag partiality as the weakest part of the type story.
  Ray/segment intersection, matrix inversion, closest-point-on-empty all need
  honest return types instead of `-1.0` sentinels and success-flag tuples. A
  2-field value type + functions unblocks the entire query layer. High leverage,
  low cost.

- **`IInnerProduct` / norm concept, packaged with the `MagnitudeSquared` fix.**
  `recommendation2.md` calls the missing inner-product concept the single
  highest-leverage concept addition; the `Magnitude`/`Length` divergence is the
  worst bug. They're the same neighborhood — fixing `MagnitudeSquared` and adding
  `IInnerProduct { Dot; Length; Normalize }` together unblocks generic
  projection / Gram–Schmidt / closest-point once.

- **`Tolerance` + `IApproximate` as first-class.** The harness already had to
  hand-roll a mixed abs+rel `LawEq` because the library's relative-only
  `AlmostEqual` is unusable near zero. Promote that into the library (also feeds
  the P1.4 tolerance policy).

- **Write the C#-vs-Plato split policy down (review §4.5).** *Pure total
  functions over values → Plato; mutation/data-structures/I/O/SIMD → C#.* This
  one page governs how much of your C# geometry library actually migrates —
  i.e. it's the operating rule for goal 1.

- **Resolve the naming set and write `NAMING.md`.** At minimum: `Amount` vs
  `Fraction` (§3.1); the semantic-scalar family; unit constructors read as
  `x.Degrees`; no synonyms (`Magnitude`/`Length`, `Sqr`/`Pow2`/`Square`,
  `Skip`/`Drop`…). Enforce via the linter. This is a decision only you can make,
  and it should precede the content wave so new content is named consistently.

- **Add the missing `Radians(x: Number): Angle` constructor**, then decide
  whether to close the implicit `Number→Angle` hole (breaking — needs an ADR).

---

## 7. Recommended roadmap (high-level) with alternatives

The organizing principle I'd propose: **correct the base, add the foundational
concepts, then pour in content — with GLSL and precision riding on that content
rather than preceding it.** Everything here is additive-file-friendly except the
Phase 4 edits, which the harness now makes safe.

### Recommended sequence

**Track 0 — the one true prerequisite (do immediately, standalone).**
Fix the **compiler associativity bug** (`AstNodeFactory.cs`, already prototyped &
reverted). Until this lands, *no new `+`/`−` content is trustworthy*. Gate: the 5
assoc witnesses flip green; regen diff matches the predicted ~16 files.

**Track A — correct & clean the base (serves goal 1's "fix inconsistencies").**
- A1. Phase 4 manifest burn-down (the 36 math bugs), `MagnitudeSquared` first
  (unblocks ~10 entries).
- A2. Type-surface defects (recommendation2 §1–2): broken constraints, dead
  concepts, `IMeasure` additive, `IDistanceField` domain, `IInnerProduct` +
  Magnitude/Length unification.
- A3. The three ADR-gated decisions (TRS order, Angle implicit, naming) — your call.

**Track B — foundational concepts (enable generic content).**
- B1. `Option<T>`/`Result`, `Tolerance`/`IApproximate`.
- B2. The function-field / combinator decision (§6) — unblocks the compositional
  layer.
- B3. `NAMING.md` + the C#/Plato split policy, both linter-enforced.

**Track C — content (the heart of goal 1; `roadmap-ideas` is the spec).**
- C1. Port the stranded pure C# (review §4.4): angle utils, axis machinery,
  point/line queries, bounds ops — direct goal-1 hits that shrink `GeometryUtil`.
- C2. Surfaces library — `Eval(solid, uv)` (review §4.1): fulfills `ISolid`,
  deletes the ChatGPT-drafted C#, resolves Sphere/Cylinder.
- C3. **SDF 2D+3D catalog + operators** (§1): highest content-per-line, and the
  content the GLSL PoC needs anyway.
- C4. PRNG + noise (§9): unblocks scatter / jitter / displacement / deformers.
- C5. Surface constructors + RMF frames (§2.2–2.3); space-warp deformers +
  falloff fields (§3.1, §0.1) — this is your Deformers/Sample-generators family.
- C6. Curve machinery (§7), interval arithmetic (§11), mass properties (§8),
  mesh ops (§3.2), Conway/icosphere (§4.2) — as appetite allows.

**Track D — targets & performance (serves goal 2 + portability).**
- D1. **GLSL PoC (Phase 7)** — now lands *on top of* the real C3 SDF catalog.
- D2. Double precision (`--scalar=double` + `Plato.Intrinsics.Double`) — finish
  the plumbing that scalar erasure started; serves goal 2 for BIM.
- D3. Optimizer 3.2–3.6.
- D4. (Bigger bet) the native type checker — accelerates all of Track C if goal 1
  means sustained Plato authoring.

Tracks A/B are sequential-ish; C is where the bulk of goal 1 lives and can be
parallelized per-family; D rides on C.

### Alternatives & why you might pick them

| Option | Order | Choose it when | Cost |
|---|---|---|---|
| **Rec. (correctness-first)** | Track 0 → A → B → C → D | You want a correct base and to serve both goals in dependency order; GLSL gets real content for free | Delays the multi-target "proof" and the fun content |
| **Alt 1 (handoff's current)** | GLSL PoC → Phase 4 → content | You want the *one-source-many-targets* thesis proven/demoed early (marketing & de-risking value); GLSL is independent of the bug wave | Serves goal 1 last; the PoC's SDF is throwaway; correctness debt lingers under it |
| **Alt 2 (content-first)** | Track 0 → content as additive files → Phase 4 opportunistically | You want fastest *visible* goal-1 progress and are willing to fix bugs lazily | Violates the review's "don't build on an untested base"; **only safe *after* Track 0** — the assoc bug makes new `+`/`−` content wrong too |
| **Alt 3 (precision-first)** | Track 0 → A1 → double precision → content | BIM correctness (goal 2) is the burning need right now | Front-loads intrinsics work before the content that would exercise it |

**The invariant across all four: Track 0 (the compiler associativity fix) comes
first.** It's the only item that gates everything else, and today it sits behind
GLSL. If you take nothing else from this report, pull that fix to the front.

### My recommendation in one line

Do **Track 0 now**, then **Track A (correct the base)**, then start pouring
**Track C content** (SDF → surfaces → PRNG/deformers) with **GLSL and double
precision riding on it** — i.e. reorder the handoff so *library content leads and
the portability PoC follows the content it needs*, because that is the ordering
in which one unit of work serves both of your stated goals at once.

---

## 8. Open questions for you

1. **Naming:** `Amount` or `Fraction` for the lerp parameter? (Blocks consistent
   content naming.)
2. **"New library from scratch"** — new C# consumer only, with `plato-src` as the
   durable source? (Confirms Phase 4 bug fixes carry forward.)
3. **Function-valued fields:** unblock `procedurals` in the compiler now, or ship
   combinator types and defer? (Gates the SDF/effector layer's dynamic side.)
4. **Precision:** is double-precision a near-term goal-2 priority given BIM, or
   still deferred?
5. **Sequence:** content-leads (my rec.) or portability-PoC-leads (handoff)?
