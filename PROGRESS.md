# PROGRESS — consolidation plan (Plan 1) — 2026-07-12

Plan: docs/plato-consolidation-plan-2026-07-12.md. Goal: retire V1/V2 split into ONE codebase,
freeze the ara3d-sdk Geometry artifacts (never touch), make the emitter simple + rigorous.
(Optimizer M0-M4 already landed + pushed: a7488f8. Optimizer exploration = Plan 2, separate.)

- [x] C0 Freeze + tripwire: tools/check-frozen-v1.ps1 + tools/frozen-v1.sha256 (210 files:
      ara3d-sdk Plato.Generated + Plato.Intrinsics + submodules/Plato/Plato.Intrinsics).
      Swapped into check-all (0.3s vs 7.7s). Plato.Intrinsics/README.md FROZEN banner. ara3d-sdk
      NOT touched. check-all ALL PASS.
- [ ] C1 One recipe, fewer flags: --modern sugar; collapse MethodsOnly/NoProperties; delete pins;
      retire --no-tir surface (legacy writer deleted at C4, not here).
- [ ] C2 Conformance consolidation: delete 4 legacy suites + scripts + check-all rows; rename
      V2Runtime -> the conformance suite. (Author sign-off noted in plan; user authorized "execute".)
- [ ] C3 M5 runtime port: struct-surface contract doc + API-snapshot test FIRST; drop colliding
      forwarders; port Angle/Vector*/Matrix*/Quaternion/Plane intrinsics to <Type>Intrinsics.
- [ ] C4 Emitter extraction + delete list: V2-only writer; emit-snapshot tests; delete legacy
      body writer, ScalarEraseAnalysis remnants, pins, NoProperties forks. (The payoff.)
- [ ] C5 Naming pass (optional, last).

Gates: check-all ALL PASS; V2Runtime 204/204. Frozen artifacts (ara3d-sdk) must stay byte-frozen
(tripwire). V2 goldens diff-gated (regen-generated.ps1).
