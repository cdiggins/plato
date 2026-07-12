# PROGRESS — optimizer completion plan (2026-07-12)

Plan: docs/plato-optimizer-completion-plan-2026-07-12.md. Recipe:
`--csharp-style=extensions --scalar=float --optimize --optimize-arrays --inline --methods --loops --no-properties`

- [x] M0 InlineReport: InlineRefusal enum + InlineReport.cs; TirInliner records refusals;
      --inline-report CLI flag prints table (stderr). Stdlib: 1747 inlined; dominant refusal
      ReturnTypeMismatch was the tuple family (M1 target).
- [x] M0 tests: InlinerTests.cs (~5s) via CSharpWriter.TestGetGroundTir /
      TirEmitSource.TryGetGroundTirByNames; matchers AssertNoDelegateInvoke/AssertCollapsed/
      CountNodes. Point3D fully collapses; Bounds3D drops delegate; TriangleMesh3D -> ctor.
- [x] M1 tuple ctor rewrite: root Tuple_N + struct return with matching field-ctor arity ->
      TirConstructorCall (position/erasure-independent). Deleted InlineTails + tailPosition.
      Guard: refuse any inline leaving a residual invoked-lambda IIFE. Excludes Tuple-named
      return types. Cheap-projection relaxation: TryBetaReduce takes a cheapness predicate;
      IsCheapProjection treats pure FIELD projections over a cheap receiver (self.A, p.X) as
      cheap, so β-reduction fires when a lambda reads p.X/p.Y/p.Z -> the immediate-apply family
      collapses too (Triangle/Quad/Line/Ray Transform => new T(new Vector3((float)this.A.X,
      ...).Transform(t.Matrix), ...)). Computed properties (Magnitude) still block. Mesh +
      tuple-helper family (UnitCircle->Point2D, curve Evals) collapse. Golden refreshed.
      GATES: check-all ALL PASS; V2Runtime 204/204; PlatoTests 103/103; regen-plato identical.
- [x] Cost-based IsCheapProjection: ProjectionCost over TIR (fields/components + whitelisted
      scalar/vector operators cheap; allocations/loops/computed-members/opaque-intrinsics expensive;
      MaxCheapCost=4). Wired into β-reduction AND (V2-gated) the multi-use compound-arg + insideLambda
      gates. Refusals MultiUseCompound 671->521, InsideLambda 174->100; inlined 2724->2942, tuple ctor
      271->345. Guard InvokeResultNavigated: refuse a lambda arg whose application is a member-call
      receiver (f(x).Foo() drops the delegate-return coercion; fixed Ray3D). V1 keeps frozen baseline.
- [x] M4 main relaxations = the cost-model wiring (the MultiUseCompound + InsideLambda buckets).
      Remaining M4 (nested-lambda, 6 refusals) low-value/high-risk: deferred.
- [x] M2 measure: optimizer-smoke/Bench (extern-alias A/B). Triangle3D.Transform ~1.0x (JIT already
      devirtualizes the monomorphic delegate); TriangleMesh3D.Transform 0.77x (eager materialization
      of the STORED result array costs allocation the lazy Map avoids). See plan doc M2.
- [ ] M3 (loop fusion / statement inlining) + M5 (non-erased runtime port): DATA-DEFERRED per M2 +
      plan "measure-first / don't build speculatively" — marginal payoff on this stdlib (transient
      fuseable arrays tiny; big arrays stored; fuseable chains are non-hot law functions). Go/no-go
      with author.

Constraint: off-flag byte-identical (regen-plato); V1 Scalar 204/204; V2Runtime 204/204;
golden refresh in same change as emitter-behavior change.
