# PROGRESS — consolidation plan (Plan 1) — 2026-07-12

Plan: docs/plato-consolidation-plan-2026-07-12.md. Goal: retire V1/V2 split into ONE codebase,
freeze the ara3d-sdk Geometry artifacts (never touch), make the emitter simple + rigorous.
(Optimizer M0-M4 already landed + pushed: a7488f8. Optimizer exploration = Plan 2, separate.)

- [x] C0 Freeze + tripwire: tools/check-frozen-v1.ps1 + tools/frozen-v1.sha256 (210 files:
      ara3d-sdk Plato.Generated + Plato.Intrinsics + submodules/Plato/Plato.Intrinsics).
      Swapped into check-all (0.3s vs 7.7s). Plato.Intrinsics/README.md FROZEN banner. ara3d-sdk
      NOT touched. check-all ALL PASS.
- [x] C2 Conformance consolidation: deleted V1/.V2/.Opt/.Scalar suite csprojs + 4 regen scripts +
      check-all rows; MOVED shared sources (ConformanceSupport/LawTests/WitnessRunnerTests/
      KnownFailures) into V2Runtime (now self-contained, external links removed). check-all: 9->6
      gates, ALL PASS; suite 204/204 standalone. Rename V2Runtime->ConformanceTests deferred to C5.
- [ ] C1 One recipe, fewer flags: --modern sugar; collapse MethodsOnly/NoProperties (now unblocked
      — Scalar suite retired) + delete pins.
- [~] C3 M5 runtime port: SAFETY NET done (docs/plato-struct-surface.md + IntrinsicsApiSnapshotTests,
      runs in conformance gate). EMITTER UNBLOCK done (WriteExtensionMethod skips colliding
      forwarders for non-erased primitives under --no-properties; validated 204/204, goldens
      compile). ANGLE ported (trig -> AngleIntrinsics; snapshot re-baselined, no loss). REMAINING
      (mechanical, snapshot-guarded, has per-type nuance e.g. Vector3 instance/static Dot duality):
      Vector2/3/4/8, Matrix3x2/4x4, Quaternion, Plane. Pattern: move behavioural instance methods to
      <Type>Intrinsics (keep operators/conversions/withers/statics/field-props/obligations), regen
      goldens, re-baseline snapshot (PLATO_UPDATE_API_SNAPSHOT=1), check-all.
- [~] C4 Emitter deletion (big deletion): step0 emit-snapshot gate (PlatoTests/emit-snapshot.txt,
      ~30 pins) DONE; step1 bodiless stubs off legacy writer DONE; step2 DELETED
      CSharpFunctionBodyWriter (~854 lines) + UseTir + --no-tir/--csharp-style=default + regen-plato
      + 5 oracle tests (fallback provably 0 both recipes; TIR now sole C# body writer). Gates:
      goldens 184/184 both, conformance 205/205, PlatoTests 97/97. step3 DONE: collapsed
      MethodsOnly->NoProperties (1 flag); deleted dead StaticNoArgMethodNames; TirInliner forks
      unconditional; PropertySyntaxNames->StructSurfacePropertyNames (uniform rule). RECONCILED:
      HandwrittenPropertySyntaxNames (Number shim) + StructSurfacePropertyNames (global call-site
      surface) kept — deleting them changes goldens. step4: ScalarEraseAnalysis no dead code; the
      genuine shrink LANDED as an intended-output change: WriteScalarCastArg collapses redundant
      idempotent ((float)(((float)x))) double-casts (726->174), goldens refreshed+compile,
      conformance 205/205, snapshot re-baselined.
- [~] C5 Naming pass: DONE rename V2Runtime suite -> Ara3D.SDK.ConformanceTests (folder/csproj/
      namespace + regen-conformance-v2runtime.ps1 -> regen-conformance.ps1 + check-all + CLAUDE.md;
      205/205, check-all ALL PASS). Intrinsics.V2 -> Intrinsics still pending (needs V1 deleted first).
- [x] Cleanup: reconciled the two drifted frozen files (Vector3.cs UnitZ->literal portability fix;
      _Tube.g.cs associativity parenthesization fix — both intentional, kept; tripwire green).
      Added docs/plato-library-map.md (the V1/V2/golden reference) linked from CLAUDE.md.
- [ ] C5 Naming pass (optional, last).

Gates: check-all ALL PASS; V2Runtime 204/204. Frozen artifacts (ara3d-sdk) must stay byte-frozen
(tripwire). V2 goldens diff-gated (regen-generated.ps1).
