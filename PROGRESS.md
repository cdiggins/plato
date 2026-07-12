# PROGRESS — golden V2 → pure-function shape (2026-07-11)

Prior mission logs live in git history of this file.
Target recipe: `--csharp-style=extensions --scalar=float --optimize --optimize-arrays --inline --methods --loops`

1. [x] TirInliner aggressive DONE: passes 3→8, budget 40→120, used-once compound args, nullary
       (constant) inlining, lambda-site inlining (lambda-free callees), alpha-rename colliding
       lambda params (`{p}_{N}`, counter reset in WriteAll), eta-lambdas opaque, emittability check
       (IsCallableOnReceiver vs plans), substitution-boundary type check + TirCoerce scalar tags,
       TirCSharpBodyWriter.AuthoritativePrim (coerce-tag/literal/own-origin receiver analysis,
       gated on InlineCalls). Gates: Scalar 204/204 w/ --inline (script updated), regen-plato
       identical, golden-v2 identical, PlatoTests 87/87.
2. [x] --methods DONE: zero `{ get` in output; erased method interfaces; kept members erased
       methods; Constants.Pi() static methods; indexers dropped (At() + receiver-typed call
       sites); explicit-impl forwarders for field/pinned obligations; PropertySyntaxNames
       (fields+pseudo+pinned+Count/NumColumns/NumRows+prim handwritten) + per-plan
       GeneratedNoArgStaticNames drive "()"; RestoreCastType (callee param type → float vs
       wrapper); broadcast TirCoerce emits wrapper cast; WitnessRunner accepts static methods;
       scalar script flags += --methods, Law check bool|Boolean. Intrinsics: Vector3.UnitZ →
       new Vector3(0,0,1) (synced). Gates: Scalar 204/204, V1 conformance 204/204, regen-plato
       identical.
3. [x] --loops DONE: TirLoopLowerer (new) lowers Map/MapIdx/MapRange/Zip2/Zip3/Reduce/All/Any/
       Reverse/WithNext/MapPairs|Triplets|Quartets on IArray receivers to for-loop statement
       markers (TirLoweredLoop/TirTempRef); lambda-literal bodies inlined as loop locals,
       delegate params invoked; element type from producing-fn type, skips generic-element
       ($T) bodies (left to library methods; --optimize unrolls component fanout at call
       sites); array temps use .Length. Runs after materializer at all 4 emit sites. Gates:
       Scalar 204/204 full recipe, regen-plato 164/164 identical.

NEW DIRECTION (2026-07-11, author): test infra for optimizer-phase development.
5. [~] Minimal subset: manifest-min.txt = objective 21-file greedy closure (158 gen files vs 184
       full, only ~14% smaller — plato-src NOT layered: core.library.Sample->LinearSpace->interval
       ->Circle, intrinsics->Plane, geometry.*->curves+transforms). Fires inline/optimize/loops,
       NOT optimize-arrays (no meshes). Genuinely-tiny needs synthetic .plato or re-layering: OPEN
       DECISION for author.
6. [x] --dump-tir=<dir> DONE: CSharpWriter.RunOptimizerPasses (unifies the 4 emit sites) records
       input(5/6)+changed phases 7-10, one <Type>.tir.txt per owner. regen-plato 164/164 identical.
7. [x] 2x2 smoke csprojs DONE: optimizer-smoke/ {Full,Min}x{NonOpt,Opt} real csprojs on earcut
       template (smoke.props + regen-smoke.ps1). All 4 dotnet build PASS (184/184/158/158).
       Docs: optimizer-smoke/README.md + docs/plato-emitter-phases.md.
Recipe/golden/docs update + check-all still pending as final step (task #4).
Constraint: default/off-flag output byte-identical (regen-plato.ps1); V1/V2/Opt suites untouched.
