# Geometry qualification of consumer fusion

This experiment qualifies `--fuse-consumers --loops` using the supervisor's frozen compiler,
runtime assemblies and stdlib snapshot. It never builds shared projects or changes the matcher.
`geometry.plato` is a **new representative geometry kernel**, not an unchanged stdlib caller.
It computes signed plane-residual bias and squared-error sums over a point cloud in three
coordinate arrays. A unit normal makes the residual a signed distance. This is motivated by
`stdlib/geometry/geometry.library.plato`'s fit-quality/residual definitions and squared-error
objective; its signed first moment is deliberately distinct from the stdlib's unsigned residual.

`census` inspects real TIR for unchanged Clothoid evaluation, 2D/3D point bounds, and mesh edge
length ratio. It records acceptance/refusal without broadening the classifier. `verify` generates
and executes both recipes, requiring the source-materialization loop to remain and the two fold
loops to become one. It checks each moment bitwise, simple geometric oracles, exceptional values,
and producer read order/failure. `benchmark` reruns correctness before warmed alternating timings
and per-thread allocation measurements. Timing requires an exclusive supervisor reservation.

The Roslyn harness uses actual generated code with a standalone GeometryRuntime assembly built
from the exact frozen intrinsic sources. Generated Number Add/Multiply instance bodies become test-local extensions by changing
only the receiver/signature because a compiled runtime assembly cannot accept new partial struct
members. The generated result record and kernel bodies are otherwise used as emitted. The tiny
host bridge returns both fields as a tuple so both results are consumed without reflection in the
timed loop. This does not qualify an entire generated foundation assembly or GPU profile.

Run the supervisor-approved mode with `python tools/experiments/optimizer-geometry/consumer-fusion/run.py`
followed by `build`, `census`, `verify`, or `benchmark`, and supply `--snapshot`, `--binaries`, and
`--stdlib` paths from the frozen snapshot. Build logs go through the existing recorder, redirected
to this experiment's private evidence directory. Runtime logs, generated C#, TIR, input hashes,
correctness results and raw timing samples stay in `.temp/optimizer-geometry/consumer-fusion/`.

The binary directory is the flat snapshot root; the stdlib is its `source/stdlib` subtree.
The runner verifies every frozen-manifest hash before and after execution and records the
snapshot revision. Build stamps bind harness sources, fixture, compiler manifest, executable,
and copied dependencies; stale harnesses are rejected. The process also verifies the hashes
of actually loaded snapshot assemblies. Benchmark processes disable tiered compilation with
`DOTNET_TieredCompilation=0`; warmup therefore precedes full-optimization steady-state samples
without depending on a background tier promotion threshold.
