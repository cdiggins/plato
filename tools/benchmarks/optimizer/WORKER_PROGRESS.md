State: verified against isolated shipping-generated source; CONTRACT R2 acknowledged; no worker-owned processes.
Worker project: worker/OptimizerBenchmark.Worker.csproj; BrepRoot required; output optimizer-benchmark-worker.dll.
Portable fixture saves baseline prepared model and every stage prerequisite; generated private sum constructors supported.
Workloads: model, projection-grids, edges, faces, refinement, assembly, curve-eval, surface-eval, projection.
Measurement excludes setup, serialization, hashing, contracts and per-item clocks; output retained; borrowed input graph hashes checked outside timing.
Model includes real topology/cache construction, discretization, face meshing, blocking refinement and assembly.
Supervisor smoke build passes (0 errors/warnings, 41.4s); UnitBox and STEP block fixtures roundtrip exactly and all seven applicable workloads pass.
First-pass face checks compare baseline contract verdict; final meshing checks remain strict. Tags include count bins and spline degree/weight variation.
run-set loads once; per-workload ok/error rows include actual warmup count/time (default minimum 200ms, iteration cap), runtime and GC metadata.
Commit turn granted; owned worker/** and WORKER_PROGRESS.md only; full recipe matrix verification remains supervisor-owned.
