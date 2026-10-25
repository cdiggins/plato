---
id: compiler-500
title: Explain allocation growth from B-rep source inlining
type: problem
status: idea
priority: p2
effort: M
risk: low
area: compiler
sprint: 
created: 2026-10-24
closed:
links: [compiler-498, compiler-499]
---

The controlled compiler-498 meshing experiment finds repeatable allocation growth
with current-policy partial source inlining: ODIN-W160 rises from 456,199,920 to
504,794,840 bytes (+10.6%); ELLA-W1 rises from 1,226,239,856 to 1,882,101,232
bytes (+53.5%). JODY-W3 falls about 0.6%. Larger budgets do not materially change
these totals. Meshes and per-face diagnostics match. This is an unexplained
profitability result, not yet a proven semantic or allocation bug.

See docs/reports/brep-inlining-2026-10-24.md for pinned inputs and reproduction.
The same 51 generated files use baseline bodies to bypass compiler-499; repair
that blocker before generalizing to full-program inlining. Profiling and a small
reproducer are a medium-sized follow-up; no default change is justified yet.

The compiler-499 repair removes the file fallback. Its full-program six-policy
rerun confirms allocation growth: ODIN rises from 435.07 to 494.05 MiB (+13.6%),
ELLA from 1,169.43 to 1,801.04 MiB (+54.0%), and JODY falls from 20,970.32 to
20,855.21 MiB (−0.55%). Current and aggressive budgets give essentially identical
allocations; adding component optimization does not change this result. All 54
primary samples match mesh and diagnostic checks. See
`docs/reports/brep-full-inlining-2026-10-24.md` and its data file. This confirms
the effect without bypassed files, but allocation attribution remains unstarted.

## Done means

- [ ] Attribute the extra allocations to generated call paths with allocation profiles.
- [ ] Reduce the behavior to a focused witness and explain the inlining/JIT interaction.
- [ ] Decide and document a fix or accepted tradeoff, then repeat equivalent meshing measurements.
