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
- [~] Track C.3: SDF library (sdf3d.plato + witnesses) drafted in scratchpad; landing next.
- [ ] Track B: Fraction (type; Lerp thread-through is a breaking ripple, defer for user review),
      Option/Result, Tolerance, IInnerProduct, function-fields (procedurals unblock = real compiler task)
- [ ] Track D: Plato.Geometry lib, double precision, GLSL, optimizer

Baseline: check-all green (8/8) at start. Commit per repo, local only, explicit staging.
