# Exact-output geometry qualification
- Contract G3; writes only this subtree, private exact-output evidence, dated exact-output report.
- State: correctness/census and single reserved limited timing run passed; stable handoff.
- Priorities: triangle/quad corner flattening, A-C quad split, indexed triangle unwelding.
- Actual Triangle/QuadArray Points qualify with existing --inline; QuadMesh fan caller still refuses.
- No compiler changes requested; default-off matcher stays bounded.
- Uses snapshot/source/stdlib; validates manifest, private DLL copies, loaded paths and pre/post hashes.
- Triangle/quad-split allocations fell~41.7%/~28.6%; opaque control unchanged; latency too noisy to qualify.
- Build3.80s, correctness26.54s, census45.96s, timing phase11.43s; no processes running.
