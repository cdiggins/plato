# PROGRESS — content-leads workstream (started 2026-07-09)

Plan: docs/plato-execution-plan-2026-07-09.md (studio). Decisions locked: Fraction,
plato-src writable + plato-src-legacy snapshot, double precision near-term,
unblock function-fields, content-leads.

- [x] Setup: plato-src-legacy snapshot (25 files), execution plan, reassessment doc
- [x] Track 0: compiler associativity fix (AstNodeFactory) + dropped 5 assoc entries; 8/8 green
- [x] Track A: FULL 36-entry bug wave done + A.8 Point2D.Subtract + A.10a dead where-clauses
      (IBounds, IPrimitiveGeometry3D). KnownFailures.json EMPTY. Measures library adds Time IMeasure
      obligations. check-all 8/8 green. Deferred: A.9 intrinsics sigs (risky), A.10 rest (IMeasure
      additive, IDistanceField domain, dead concepts), A.11 ADRs (need user: TRS order, Angle implicit).
- [x] Track C.3: Sdf3D library (7 primitives + 4 CSG ops) + 10 witnesses; 8/8 green, all 4 modes.
- [x] Track C parity: Vector2 Cross/Perpendicular/Rotate + Number InverseLerp/Remap + 5 witnesses.
      Conformance now 200/0. Added to EXISTING libraries (no ordinal churn).
- [ ] Track B: Fraction (type; Lerp thread-through is a breaking ripple, deferred for user review),
      Option/Result, Tolerance, IInnerProduct, function-fields (procedurals unblock = real compiler task)
- [ ] Track D: Plato.Geometry C# lib, double precision, GLSL PoC, optimizer 3.2-3.6
- Findings for author: Vector3(0.0) broadcast lowering papercut (use new Vector3(...)); library-ordinal
  comment churn on new library files. See docs/plato-session-2026-07-09-report.md.
- [ ] Track D: Plato.Geometry lib, double precision, GLSL, optimizer

Baseline: check-all green (8/8) at start. Commit per repo, local only, explicit staging.
