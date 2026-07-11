# Optimizer stage 2 — loop-into-buffer lowering (starting plan, 2026-07-10)

Stage 2 per the roadmap: "loop-into-buffer lowering, functional-in-place — needs P6."
P6's runtime slice is DONE (`unique type List<T>` / `Buffer<T>` intrinsics, `plato-src/unique.plato`),
so this is now unblocked. Nothing here is implemented yet; this doc pins the scope and order so the
work starts warm.

## The problem

`Map` / `MapRange` / `Zip` return LAZY views (`Ara3D.Collections.ReadOnlyList<T>` wraps a
`Func<int,T>`; see `Plato.Intrinsics/ArrayExtensions.cs`). Every element access re-evaluates the
whole upstream pipeline. For single-pass consumers (Reduce, Sum, mesh vertex fills) this is fine and
allocation-free; for multi-pass consumers (anything indexed more than once — face-index generation
over point grids, bounds+deform pipelines) the recomputation is the dominant cost (see the 0.5
benchmark baseline: 432 MB folding 1M points).

## The lowering

Materialize pipeline RESULTS into a `Buffer<T>` (fixed-size, write-by-index, zero-copy `Freeze` to
`IArray<T>`) when (a) the length is statically known at the producer (`MapRange(n, f)`, `Map` over a
concrete-length source) and (b) the consumer profile justifies it. Purity makes the transform
always SOUND; the analysis is only about when it is PROFITABLE.

## Increments (each flag-gated, conformance-Opt + benchmark gated, in order)

1. **DONE 2026-07-10 — eager intrinsics + emission-time rewrite of multi-consumed results**
   (`--optimize-arrays`, `TirArrayMaterializer.cs`). `MapEager`/`MapRangeEager` in
   `Plato.Intrinsics/ArrayExtensions.cs` (direct array fill — the `T[]` IS the frozen buffer;
   overload set mirrors `Map`'s exactly so the rename is overload-transparent). Rewrite positions
   (found by measuring the stdlib — let-bound Maps don't occur; STORED Maps do): direct arguments
   of constructors and struct-building tuples (`Deform`, `To3D` — mapped point sets stored into
   mesh structs, 10 stdlib sites), plus multi-referenced/loop-referenced lets (future code).
   Gates: Opt conformance runs `--optimize --optimize-arrays`, 204/204 + a MapEager presence
   check; `ArrayMaterializerTests` pins the footprint (only Map→MapEager renames, nothing else).
   Probe (1M `Vector3` points, stored result, 4-pass consumption): 1.37× — interface-dispatch
   consumption dominates the micro-shape; the win scales with callback cost/pipeline depth, and
   each element now evaluates exactly once regardless of consumer count.
2. **`Map∘Map` / `MapRange∘Map` fusion** (3.5 overlap): collapse adjacent lazy maps into one
   callback before deciding eager/lazy, so materialization never captures an avoidable indirection.
3. **Struct-functor emission for surviving HOFs** (3.2 overlap): `TFunc : struct, IFunc<T,R>`
   instead of `Func<T,R>` for known literal lambdas, killing the delegate allocation the eager fill
   loop would otherwise pay per call site.
4. **Consumer-side lowering:** single-pass consumers (`Reduce`, `Sum`, `All`) over a lazy pipeline
   emit a fused `for` loop directly — no buffer at all (the "functional-in-place" end state).

## Notes for the implementer

- The TIR now drives ALL emit paths (default/extension/scalar/optimize C# + TS/Rust), so a
  TIR-level rewrite covers every style; mirror `TirComponentUnroller`'s structure (post-order
  rewrite, marker nodes, all-or-nothing per body, byte-identical off-flag).
- Gate battery: `regen-conformance-opt.ps1 -Test` (142/36/0 semantics preserved bit-for-bit) plus a
  benchmark delta recorded in the roadmap (0.5 baseline, `tests/Ara3D.SDK.Benchmarks` — don't
  modify it; scratch probes only).
- Element type at the rewrite site is KNOWN from the TIR node types — no emit-time re-inference.
