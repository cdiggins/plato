---
id: compiler-482
title: Eliminate singleton arrays in lowered FlatMap branches
type: feature
status: in-progress
priority: p1
effort: S
risk: med
area: compiler
sprint: 
created: 2026-10-24
closed:
links: [docs/design/brep-compiler-optimization/fusion-filter.md]
---

Implement only reviewed F1: raw typed conditional singleton arms in an existing lowered FlatMap become conditional append; retain original fallback evaluation and copying. No new call matching, Take rewriting, local fusion, capacity policy, or public ABI change. Review: docs/reports/brep-compiler-optimization-review-2026-10-24.md (CR-01, CR-02, CR-11, CR-12).

## Done means

- [ ] Prove a minimized current BREP projection-candidate shape matches under --loops --fuse without --inline or stripped coercions.
- [ ] Preserve exact element/result typing, selected branch evaluation, conversions, source order and fallback behavior in compiled net8/C#12 tests.
- [ ] Verify retained fallback for unsupported shapes and existing optimizer/emission regressions with explicit flag combinations.
- [ ] Measure warmed allocation reduction against the same lowered ordinary path with equal outputs and retained source/evidence.
- [ ] Update the optimizer guide and record integrated validation, including any pre-existing failures and unrun whole-BREP checks.
