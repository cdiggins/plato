# Consolidation plan — one codebase, one runtime, a rigorous emitter (2026-07-12)

> Plan 1 of 2 (companion: `plato-optimizer-exploration-plan-2026-07-12.md`). Author directive:
> eliminate the V1/V2 split and work on ONE codebase inside the Plato repo, without touching the
> Geometry library that Ara3D.Studio consumes today; complete the runtime port (M5) without losing
> the C#-consumer-facing surface (operators, helper statics, field properties); make the C# emitter
> simple and rigorous enough that improving it is not risky.

## 0. What "V1" and "V2" actually are today (the confusion, inventoried)

| Axis | V1 (frozen-to-be) | V2 (the one codebase) |
|---|---|---|
| Runtime | `Plato.Intrinsics/` (+ byte-synced copy in `ara3d-sdk/src/Plato.Intrinsics`) | `Plato.Intrinsics.V2/` (System.Numerics-backed, method-form) |
| Generated output | `ara3d-sdk/src/Plato.Generated` (default style, wrapper scalars, properties) | `Generated/Plato.Generated.{Unoptimized,Optimized}` (extensions, scalar-erased, `--no-properties`) |
| Recipe | *(no flags)* | `--csharp-style=extensions --scalar=float [--optimize --optimize-arrays --inline --methods --loops] --no-properties` |
| Conformance | `Ara3D.SDK.ConformanceTests{,.V2,.Opt,.Scalar}` (4 suites) | `Ara3D.SDK.ConformanceTests.V2Runtime` |
| Emitter path | `CSharpFunctionBodyWriter` (legacy) + property/method decision web | `TirCSharpBodyWriter` + uniform method-form |
| Gate | `regen-plato.ps1` byte-identity vs ara3d-sdk | `regen-generated.ps1` golden diff + V2Runtime 204/204 |

Five conformance suites, two runtimes, two emitter body-writers, and a flag matrix nobody should
have to hold in their head. Every recent increment paid a "keep V1 byte-identical" tax (the
NoProperties gating forks in TirInliner, the pins, ScalarEraseAnalysis special cases).

## 1. Principles

1. **The frozen artifact is ara3d-sdk, not a live recipe.** Ara3D.Studio consumes
   `ara3d-sdk/src/Plato.Generated` + `Plato.Intrinsics`. Those files stop changing, full stop.
   Protection becomes a **checksum tripwire**, not a regeneration proof — so the Plato repo no
   longer needs the legacy emit path to stay runnable forever just to re-derive frozen bytes.
2. **One recipe.** The V2 recipe is *the* output. `Generated/Plato.Generated.Optimized` is the
   adoption shape; `Unoptimized` is the readable reference. Everything else is deletable.
3. **Delete tests by deleting what they test.** The 4 legacy conformance suites exist to gate
   recipes we are retiring. They go when the recipes go — semantic coverage of the one codebase is
   V2Runtime (204 checks) + PlatoTests + GeometryTests (the SDK's own).
4. **The struct surface is a contract, not an accident.** M5 moves *behavior* to extensions, but
   the C#-consumer surface — operators, constructors, implicit conversions, `X/Y/Z`-style field
   properties, `Zero/One/UnitX/Default/MinValue`-style statics, BCL obligations — is enumerated in
   a doc and pinned by an API-snapshot test, so a port step can never silently lose it.
5. **Emitter changes must be provable in seconds.** Fast in-proc snapshot tests (the InlinerTests
   pattern) for emitted member bodies, not just the ~60s regen+build+conformance cycle.

## 2. Phases

### C0 — Freeze + tripwire (½ day) — DONE 2026-07-12

- `tools/check-frozen-v1.ps1`: SHA-256 manifest (`tools/frozen-v1.sha256`, 210 files) of
  `ara3d-sdk/src/Plato.Generated/**`, `ara3d-sdk/src/Plato.Intrinsics/**`, AND
  `submodules/Plato/Plato.Intrinsics/**` (so the source-of-truth copy can't be edited either).
  `-Update` re-baselines. Verify runs in **0.3s** vs regen-plato's ~7.7s.
- Swapped into `check-all.ps1` in place of the regen-plato byte-identity gate. regen-plato.ps1
  kept runnable (deleted at C4).
- FROZEN banner added to `Plato.Intrinsics/README.md` (Plato repo). **ara3d-sdk deliberately NOT
  touched** (it's a dirty submodule the author consumes; its intrinsics README already says
  "SYNCED COPY — do not edit"; the tripwire protects those bytes without editing them).
- **Gate:** check-all ALL PASS with the tripwire substituted.
- Deferred from the original C0: the `v1-freeze` git tag (do at end of mission / author's call).

### C1 — One recipe, fewer flags (½–1 day)

- CLI: add `--modern` (name TBD by author) as sugar for the full V2 recipe
  (`extensions + scalar=float + methods + no-properties`, optimizers opt-in). The long-form flags
  remain but the scripts and docs speak one word.
- Collapse `MethodsOnly`/`NoProperties` into ONE writer flag (NoProperties already implies
  MethodsOnly; the distinction dies with V1). Delete the pin sets it guarded
  (`HandwrittenPropertySyntaxNames`, per-plan `KeptNoArgPropertyNames`).
- Delete `--no-tir` (the legacy body path is only reachable from recipes being retired) — but note
  the legacy writer itself is deleted in C4, not here.
- **Gate:** V2 goldens byte-identical under the sugar flag; V2Runtime 204/204.

### C2 — Conformance consolidation — DONE 2026-07-12 (author-authorized)

- Deleted `Ara3D.SDK.ConformanceTests` (V1), `.V2`, `.Opt`, `.Scalar` project files + their 4
  `tools/regen-conformance*.ps1` scripts + the 4 check-all conformance rows. The shared test
  sources the base suite held (`ConformanceSupport.cs`, `LawTests.cs`, `WitnessRunnerTests.cs`,
  `KnownFailures.json`) — which `.V2Runtime` *linked* — were `git mv`'d into `.V2Runtime`, so the
  surviving suite is now **self-contained** (its csproj's external `..\` links removed).
- check-all conformance is now one gate: `conformance (V2 runtime, 204/204)`. The battery dropped
  from 9 gates to 6 (tripwire · V2 golden diffs · lint · conformance · SDK build · GeometryTests).
- Rename `.V2Runtime` → `Ara3D.SDK.ConformanceTests` **deferred to C5** (naming, lower risk as a
  pure rename once nothing else references the folder).
- CLAUDE.md + the regen-v2runtime header updated to the one-suite reality.
- **Gate:** reduced check-all ALL PASS; the moved-source suite builds + runs 204/204 standalone.

### C3 — M5 runtime port — IN PROGRESS 2026-07-12

- **Safety net DONE**: `docs/plato-struct-surface.md` (keep-on-struct contract) +
  `IntrinsicsApiSnapshotTests` (committed baseline `intrinsics-api-snapshot.txt`, runs in the
  conformance gate; Law_/Witness_ filtered). An instance→extension move is a reviewable diff; a
  member lost from both struct and extension is caught.
- **Emitter unblock DONE**: `CSharpConcreteTypeWriter.WriteExtensionMethod` skips the colliding
  `TExtensions.Foo(this T) => self.Foo()` forwarder for NON-erased primitives (Angle, Vector*,
  Matrix*, Quaternion, Plane) under `--no-properties`. Validated: goldens compile + 204/204. The
  forwarders were redundant (instance/extension methods serve `x.Foo()`); scalar-erased primitives
  keep theirs (distinct erased receiver). Removed forwarders from all 9 golden files (both goldens).
- **Angle ported DONE**: trig (Cos/Cosh/Sin/SinCos/Sinh/Tan/Tanh) moved to `AngleIntrinsics`
  extensions; Radians/CompareTo/operators/conversions stay on the struct. Snapshot re-baselined
  (verified move, no loss). check-all ALL PASS.
- **Remaining (mechanical, snapshot-guarded)**: move the behavioural instance methods of
  `Vector2/3/4/8`, `Matrix3x2/4x4`, `Quaternion`, `Plane` to `<Type>Intrinsics`. The emitter is
  already ready; each is a runtime edit + snapshot re-baseline + conformance.

### C3 (original brief) — M5 runtime port, with the surface contract (1–2 days)

- Write `docs/plato-struct-surface.md`: the enumerated keep-on-struct list — fields; constructors;
  operators (C# operators cannot be extensions on our LangVersion anyway); implicit conversions;
  field-properties (`X`, `Y`, `Z`, `M11`…, `Count`, swizzle-like cheap accessors the author wants
  ergonomic); static helpers (`Zero`, `One`, `Unit*`, `Default`, `MinValue/MaxValue`, `Identity`);
  BCL obligations (`IReadOnlyList`, `IEquatable`, `GetHashCode/ToString/Deconstruct`).
- **API-snapshot test first**: reflection dump of the public surface of `Plato.Intrinsics.V2` +
  the Optimized golden into a committed text file; any loss is a test diff, not a discovery.
- Emitter half: stop emitting the colliding wrapper-type forwarder extensions
  (`AngleExtensions.Cos(this Angle)` etc.) — emit direct calls to the intrinsic instead. This was
  the CS0121 blocker; note the forwarder also performed `Number→float` return erasure, which the
  direct call must preserve.
- Port `Angle, Vector2/3/4/8, Matrix3x2, Matrix4x4, Quaternion, Plane` instance methods →
  `<Type>Intrinsics` extension classes (pattern already proven for `Number/Integer/Boolean`).
  Keep everything on the surface-contract list on the struct.
- **Gates:** API snapshot unchanged-or-intentionally-extended · conformance 204/204 · golden diff
  reviewed · optimizer-smoke builds.

### C4 — Emitter separation + the delete list — IN PROGRESS 2026-07-12

**Findings (investigation done):**
- Under the V2 recipe the emitter reports **`legacy fallback bodies: 0`** (2365 TIR bodies). So the
  legacy `CSharpFunctionBodyWriter` never serves a *bodied* member/static under V2.
- BUT it is still reached (`CSharpTypeWriter` line ~258) for **bodiless functions** (interface
  obligations / stubs) without counting as a fallback — so it is not trivially dead. Deleting it
  needs bodiless-function emission migrated to a small dedicated path first. That + retiring the
  default-style emit path (only `regen-plato.ps1` uses it; the V1 library it regenerates is now
  frozen) + deleting the pin machinery is the remaining "2–3 day" core.

**Done (safe, golden-neutral cleanups):**
- Consolidated the duplicate helpers into `TirRewrite`: `IsStatementNode` (was 3 private copies in
  `TirInliner` / `TirComponentUnroller` / `TirCSharpBodyWriter` — the body-writer superset incl.
  `TirLoweredLoop` is the one home) and `StripCoerce` call sites (`TirLoopLowerer` /
  `TirCSharpBodyWriter` now use `TirRewrite.StripCoerce`). Deleted `Plato.CSharpWriter/unused.txt`.
  (`TirLambdaCaptureRewriter.ReplaceNode` stays — it is in `PlatoCompiler`, which cannot reference
  `TirRewrite` in `Plato.CSharpWriter`.)
- **Gate:** both V2 goldens byte-identical (184/184); PlatoTests 103/103.

**Remaining (the payoff, next focused session — write the emit-snapshot rigor first):**
1. Migrate bodiless-function emission off `CSharpFunctionBodyWriter`.
2. Retire the default-style emit path + `regen-plato.ps1` (V1 is frozen; the tripwire guards it).
3. Delete `CSharpFunctionBodyWriter` (~750 lines) + the now-dead fallbacks.
4. Collapse `MethodsOnly`/`NoProperties` to one flag; delete `PropertySyntaxNames` /
   `HandwrittenPropertySyntaxNames` / `StaticNoArgMethodNames` pins and the `NoProperties` forks in
   `TirInliner` (they become unconditional) — the property/method duality the plan calls the risk
   surface. Under V2 the pins are already empty, so this should be golden-neutral.
5. Shrink `ScalarEraseAnalysis` to a pure type substitution under the uniform surface.

### C4 (original brief) — Emitter separation + the delete list (2–3 days; the payoff)

The risk in emitter work today comes from one writer serving six recipes through context-sensitive
decision webs. With C0–C2 done, only one recipe remains, so:

- **Extract a V2-only writer path** (folder `Plato.CSharpWriter/V2/` or equivalent): TIR-only,
  extension-style-only, scalar-erased-only, method-form-only. Uniform rendering rule: *members of
  the struct-surface contract render as member/property syntax; everything else is a method call.*
  No `EmissionKind` property/method arbitration, no pin lookups.
- Then delete, in dependency order: `CSharpFunctionBodyWriter` (legacy body writer, ~750 lines) ·
  `ExtensionStyleWriter` dual modes · the default-style emit path in
  `CSharpConcreteTypeWriter`/`CSharpTypeWriter` · `ScalarEraseAnalysis` (shrinks to a pure type
  substitution under the uniform surface; delete what remains context-sensitive) ·
  `PropertySyntaxNames`/`StaticNoArgMethodNames` · `unused.txt` · the `NoProperties` behavior forks
  in `TirInliner` (they become unconditional) · `TirLoopLowerer.StripCoerce` /
  `TirLambdaCaptureRewriter.ReplaceNode` private copies (point at `TirRewrite`) · the three
  `IsStatementNode` copies (one home).
- **Rigor added in the same phase:** per-function emit-snapshot tests — compile stdlib once
  in-proc, emit a pinned list of ~30 representative member bodies, string-compare against a
  committed golden file. Seconds-fast; this is what makes future emitter work non-risky. Plus the
  `--dump-tir` golden pin from the completion plan (locks optimizer behavior without a C# build).
- **Gates:** V2 goldens byte-identical across the extraction (the extraction itself must be a
  no-op); conformance; snapshot suite green; frozen-v1 tripwire untouched.

### C5 — Naming pass (½ day, optional, last)

Once nothing references the legacy folders: `Plato.Intrinsics` → `Plato.Intrinsics.Legacy` (or
archive branch), `Plato.Intrinsics.V2` → `Plato.Intrinsics`, drop "V2" from suite/doc names.
Purely cosmetic; do it only after C4 so it's a rename, not a merge.

## 3. Order and dependencies

C0 → C1 → C2 are each small and independently shippable. C3 (M5) can run before or after C2 but
**before C4** (the uniform runtime is what makes the uniform rendering rule total). C4 is the big
one and should be one focused mission with the snapshot tests written *first*. C5 floats.

## 4. Risks

- **Deleting green suites (C2)** — mitigated by author sign-off + the fact that V2Runtime covers
  the same 204 semantic checks against the runtime that will actually ship.
- **Losing struct surface in C3** — mitigated by the API-snapshot test written before the port.
- **The C4 extraction silently changing output** — mitigated by requiring byte-identical V2
  goldens for the extraction commit, before any simplification commits follow.
- **Adoption question deliberately out of scope:** migrating Ara3D.Studio itself from the frozen
  V1 library to the V2 output is a separate, later decision; nothing here forces it.

## 5. Rerun crib

```powershell
.\tools\check-all.ps1                          # shrinks as phases land
.\tools\check-frozen-v1.ps1                    # (C0) tripwire
.\tools\regen-conformance-v2runtime.ps1 -Test  # the conformance suite (renamed at C2)
.\tools\regen-generated.ps1                    # V2 golden diff / -Apply
dotnet test submodules\Plato\PlatoTests        # incl. emit-snapshot suite from C4
```
