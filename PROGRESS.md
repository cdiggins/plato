# PROGRESS — content-leads workstream (started 2026-07-09)

Plan: docs/plato-execution-plan-2026-07-09.md (studio). Decisions locked: Fraction,
plato-src writable + plato-src-legacy snapshot, double precision near-term,
unblock function-fields, content-leads.

- [x] Setup: plato-src-legacy snapshot (25 files), execution plan, reassessment doc
- [x] Track 0: compiler associativity fix (AstNodeFactory) + dropped 5 assoc entries; 8/8 green
- [~] Track A: A.1-A.6 done (MagnitudeSquared, constants, Barycentric/SmootherStep, 8 curves,
      Triangle2D area, Bounds2D corners, dup Points, ArcMin/Sec); 26 manifest entries cleared, 8/8 green.
      Remaining: A.7 Time IMeasure (5 entries), A.8 Point2D.Subtract, A.9 intrinsics sigs, A.10 type-surface, A.11 ADRs
- [ ] Track B: Fraction, Option/Result, Tolerance, IInnerProduct, function-fields
- [ ] Track C: content (ports, surfaces, SDF, PRNG, deformers…)
- [ ] Track D: Plato.Geometry lib, double precision, GLSL, optimizer

Baseline: check-all green (8/8) at start. Commit per repo, local only, explicit staging.
