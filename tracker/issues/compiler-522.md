---
id: compiler-522
title: Prepared-corpus optimizer performance benchmark for plato-brep
type: feature
status: done
priority: p1
effort: L
risk: med
area: compiler
sprint:
created: 2026-10-25
closed: 2026-10-25
links: []
---

Build a reusable benchmark for the complete Plato C# optimizer pipeline on prepared
plato-brep geometry and meshing workloads. STEP parsing and artifact construction
are setup costs, recorded separately from repeated measurements.

## Done means

- [x] Isolated, content-identified source/build preparation and dependency-aware full-recipe ablations are executable and tested.
- [x] Prepared fixtures run real geometry and meshing workloads without file I/O or STEP parsing in the measured region; outputs are checked across variants.
- [x] Budgeted quick/confirm/corpus runs preserve raw samples and report allocation, uncertainty, regressions, coverage and failure states honestly.
- [x] Integrated tests and an actual prepared-workload experiment pass; usage, limitations and reproducible commands are documented.

## Work record

The parallel work contract and recovery ledger are in
`tools/benchmarks/optimizer/CONTRACT.md`. Existing root PROGRESS.md belongs to another mission.

Implementation and verification are recorded in
`docs/reports/prepared-optimizer-benchmark-2026-10-25.md`, with compressed raw samples
and identified inputs under `docs/reports/data/compiler-522-2026-10-25/`.
The available shipping, unoptimized and complete-without-inlining configurations
passed every applicable workload across the nine-model integration corpus. Fixture
reuse is independent of compiler-only revisions and checks original provenance plus
byte-exact candidate readers. The test suite passes with one platform privilege skip.

The complete recipe's generation timeout is a separate compiler problem,
`compiler-535`; the benchmark preserves it as a failed configuration. Its other
feature removals and experimental parameter recipes are implemented and unit-tested,
but a complete runtime study awaits that generation problem. The brief corpus run
does not establish quick-suite predictive validity or a preferred production recipe.
The required studio battery retains the existing `repo-473` checksum failure;
all executed relevant builds, lints, PlatoTests and geometry tests pass.
