---
id: compiler-522
title: Prepared-corpus optimizer performance benchmark for plato-brep
type: feature
status: in-progress
priority: p1
effort: L
risk: med
area: compiler
sprint: 
created: 2026-10-25
closed:
links: []
---

Build a reusable benchmark for the complete Plato C# optimizer pipeline on prepared
plato-brep geometry and meshing workloads. STEP parsing and artifact construction
are setup costs, recorded separately from repeated measurements.

## Done means

- [ ] Isolated, content-identified source/build preparation and dependency-aware full-recipe ablations are executable and tested.
- [ ] Prepared fixtures run real geometry and meshing workloads without file I/O or STEP parsing in the measured region; outputs are checked across variants.
- [ ] Budgeted quick/confirm/corpus runs preserve raw samples and report allocation, uncertainty, regressions, coverage and failure states honestly.
- [ ] Integrated tests and an actual prepared-workload experiment pass; usage, limitations and reproducible commands are documented.

## Work record

The parallel work contract and recovery ledger are in
`tools/benchmarks/optimizer/CONTRACT.md`. Existing root PROGRESS.md belongs to another mission.
