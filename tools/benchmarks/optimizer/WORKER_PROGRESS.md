State: standalone fixture probe verified; CONTRACT R3 acknowledged; no worker-owned processes.
Worker project: worker/OptimizerBenchmark.Worker.csproj; BrepRoot required; output optimizer-benchmark-worker.dll.
Fixture preserves generated private sum constructors; intrinsic JsonConverterAttribute handles private String storage; unsupported shapes report type names.
Workloads: model, projection-grids, edges, faces, refinement, assembly, curve-eval, surface-eval, projection.
Measurement excludes setup, serialization, hashing, contracts and per-item clocks; output retained; borrowed input graph hashes checked outside timing.
Model includes real topology/cache construction, discretization, face meshing, blocking refinement and assembly.
Supervisor smoke builds pass (0 errors/warnings); UnitBox/STEP block workload checks pass; Crescent diagnostic String fixture roundtrip passes (fix build 33.8s).
First-pass face checks compare baseline contract verdict; final meshing checks remain strict. Tags include count bins and spline degree/weight variation.
run-set loads once; per-workload ok/error rows include actual warmup count/time (default minimum 200ms, iteration cap), runtime and GC metadata.
Worker cb8eb7d6/1e4be418 committed; probe build passes (17.7s, 0 errors/warnings); old UnitBox/Crescent fixtures roundtrip through new shipping/unoptimized workers.
