# PROGRESS — TIR-all-styles + checker-intrinsics + solids mission (2026-07-10)

Prior mission logs live in git history of this file.

- [x] Item 1 TIR emit everywhere: extension (183/183), TS (plato.g.ts identical, 1932 bodies),
      Rust (identical, 1948), optimize (TirComponentUnroller, 164/164), scalar (ScalarEraseAnalysis
      over Origin symbols, 183/183). scalar+optimize combo stays legacy (WriteBody guard).
      Legacy CSharpFunctionBodyWriter still exists as --no-tir fallback + differential reference.
- [x] Item 2 intrinsics declared to checker: Number MinValue/MaxValue/RSRE/Linear/Quadratic/Cubic,
      IOrderable Equals/NotEquals, Angle(x: Number) cast (+ Number.Angle wrapper property).
      Diagnostics 78 -> 68 (CHK201 22 -> 13). CheckerDiagnosticsSummaryTests prints the worklist.
      Rule-tightening (Self-unifies-anything etc.) still deferred until count is lower.
- [x] Item 3 solids content: library Solids in solids.plato — Eval/ClosedX/ClosedY for all 11
      ISolid types (NGonPoint/SquarePoint/UnitSphere helpers). Numerically probe-verified.
- [x] Item 4 optimizer stage 2 increment 1 (--optimize-arrays): MapEager/MapRangeEager intrinsics
      + TirArrayMaterializer (constructor/tuple args + multi-ref lets; 10 stdlib sites, all mesh
      Deform/To3D). Opt conformance runs both flags (204/204 + MapEager check);
      ArrayMaterializerTests pins footprint; probe 1.37x. Remaining increments in
      docs/optimizer-stage2-plan.md (fusion, struct functors, consumer-side loops).
- [x] Golden V2 = optimized adoption shape BY DEFAULT: regen-golden-v2.ps1 (extensions+scalar+
      --optimize --optimize-arrays), check-all gate, scalar conformance runs same flags; TIR
      enabled for scalar+optimize combo (TirComponentAccess.ScalarComponentPrim + node-aware
      scalar decisions); golden refreshed (139 files, +Solids.g.cs).
- [x] Final gates: check-all ALL PASS (incl golden V2 + Scalar), PlatoTests 87/87, regen applied.
