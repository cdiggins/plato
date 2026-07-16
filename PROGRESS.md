# PROGRESS — fixed-size unroll + cast cleanup, on a small corpus — 2026-07-13

- [x] Inc1: plato-spike-src → plato-src-small (self-contained Point3D/Bounds3D/Line3D corpus);
      Small/ tree = Plato.Small.Runtime + Plato.Small.Generated.Unoptimized + Plato.Small.Optimized
      + regen-small.ps1 + README. Both build; reproduce Deform/Eval/ZipComponents.
- [x] Inc2: cheap-projection vector sources → Line3D.Eval unrolls to new Point3D(...).
- [x] Inc3: inline TirArray-literal bodies (Corners) + unroll Map/Zip/Reduce/All/Any/Reverse over
      fixed-size TirArray → Bounds3D.Deform loses its loop; TirArray prints typed MakeArray<T>.
- [x] Inc4: drop redundant coerce<X→X> casts on provably-primitive inners (literal / erased param /
      real component field / scalar-operator result). Eval fully clean.
- [~] Inc5: Eval _var copies already gone via Inc2; broad copy-let removal DEFERRED (would need the
      out-of-scope legacy TS/Rust symbol writer changed too — reverted the shared capture-rewriter edit).
- [x] Goldens applied (both recipes changed by cast cleanup); both projects build. Emit snapshot
      re-baselined. PlatoTests 116/116. Conformance 205/205.
- [x] check-all: ALL GATES PASS (tripwire, regen-diff, lint, conformance 204, SDK build, GeometryTests).
