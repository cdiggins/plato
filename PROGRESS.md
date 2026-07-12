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
- [ ] C4 Emitter extraction + delete list: V2-only writer; emit-snapshot tests; delete legacy
      body writer, ScalarEraseAnalysis remnants, pins, NoProperties forks. (The payoff.)
- [ ] C5 Naming pass (optional, last).

Gates: check-all ALL PASS; V2Runtime 204/204. Frozen artifacts (ara3d-sdk) must stay byte-frozen
(tripwire). V2 goldens diff-gated (regen-generated.ps1).
