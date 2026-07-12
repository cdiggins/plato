# Plan: remove no-arg *properties* from generated + intrinsic code

> AI-authored planning doc (Claude), 2026-07-11. Diagnosis is verified against the
> emitter source and the checked-in golden; the execution steps are a proposal.

## OUTCOME (2026-07-12): shipped via a V2 runtime, scoped to the Generated libraries

Rather than the full-tree big-bang below, the change was contained inside the Plato module:
- **`Plato.Intrinsics.V2`** — a new shared project, copy of the handwritten runtime with every
  primitive no-arg *property* rewritten as a *method* (fields and pseudo-fields — `X`/`Y`/`Z`,
  `M11`, `Row1-4`, `Plane.Normal`/`D`, `Number.Angle`, `Matrix.Rotation`/`Translation` — stay
  properties, because those names are Plato *fields* elsewhere and keep field-access syntax).
- **Emitter `--no-properties` flag** (superset of `--methods`): empties the handwritten-property
  pins and drops the primitive property carve-out, so generated call sites use `x.Cos()` etc.
- **Both `Generated/` projects** import V2 and regenerate with `--no-properties` → zero `{ get`
  in the output; both build clean.
- **V1 `Plato.Intrinsics` untouched** → default-mode byte-identity and the ara3d-sdk sync gate
  stay green. Nothing outside `submodules/Plato` changed. All `check-all.ps1` gates pass.

Follow-up not done (out of the requested scope): a conformance variant that *executes* the
V2/`--no-properties` output (current suites still run the V1/`--methods` recipes), and migrating
the wider studio/labs consumers per the big-bang plan below.

## The ask

`Plato.Generated.Optimized/_Angle.g.cs` emits members like

```csharp
public Angle Abs { [MethodImpl(AggressiveInlining)] get => new Angle(this.Value.Abs()); }
```

The stated language direction is *no properties — every no-arg member is an extension
(or plain) method*. So `Abs`, `Reciprocal`, `Sqr`, `Sqrt`, `Cos`, `Sin`, `Radians`, … should
be `Abs()`, `Cos()`, etc.

## Why this is NOT a generated-file edit (verified)

1. `--methods` already erases properties **everywhere except the `PrimitiveTypes`**
   (`CSharpWriter.cs:474`) — `Number`, `Integer`, `Boolean`, `Angle`, `Vector2/3/4/8`,
   `Matrix3x2/4x4`, `Quaternion`, `Plane`, `Character`. The Optimized golden is current
   (regen-generated diff = 0), so the properties you see are the emitter's *intended* output.

2. The carve-out exists because those primitive types have **handwritten partial structs in
   `Plato.Intrinsics`** that declare their math members as C# **properties**
   (`Number.Abs`, `Number.Sqrt`, `Number.Reciprocal`, `Angle.Cos`, `Angle.Sin`,
   `Angle.Radians`, …). Generated code and the intrinsics access them with property syntax.

3. Call-site syntax is decided **globally by name** (a name may not be a property on one type
   and a method on another — `CSharpWriter.cs:116-158`). `Number`'s handwritten property names
   (`Abs`, `Sqrt`, `Reciprocal`, `Sign`, `Square`, …) are therefore pinned to property syntax
   on **every** type, which is exactly why `Angle.Abs` stays a property even though its body is
   generated.

4. `Plato.Intrinsics` is the **source of truth synced byte-for-byte into `ara3d-sdk`** and
   diff-gated by `regen-plato.ps1` (repo Hard Rule 2). It cannot fork.

**Consequence:** converting a name to a method is atomic across (a) the emitter carve-out,
(b) the handwritten intrinsic member of that name, and (c) every call site — because a C#
instance property and a same-name extension method cannot usefully coexist (the instance
member hides the extension). There is no green intermediate state per name.

## Blast radius (measured)

- Handwritten property members to convert: `Number` ~22, `Angle` ~8, plus `Vector2/3/4/8`,
  `Matrix*`, `Quaternion`, `Plane`, `Integer`, `Boolean`, `Character` — ~100 `get` sites
  across 15 intrinsic files (many are fields/indexers that STAY).
- Emitter: drop the `isPrimitive` property exemptions in `ExtensionStyleWriter.cs:182-188` &
  `236-239`, empty/trim `HandwrittenPropertySyntaxNames` (`CSharpWriter.cs:68`), keep only
  genuine field / BCL-`Count` pins.
- Consumer call sites: `.Cos/.Sin/.Tan` alone match 439+ across the tree, but MOST are
  unrelated libraries (`labs/Ara3D.Mathematics`, `ACadSharp`, `System.MathF`). The true
  Plato-typed subset must be isolated per type — no clean mechanical filter.
- Mirror every intrinsic edit into `ara3d-sdk/src/Plato.Intrinsics` (byte-identical) and
  regenerate `ara3d-sdk/src/Plato.Generated`.

## What STAYS a property/field (cannot be a method)

- Wrapped fields: `Value`, `X/Y/Z/W`, matrix rows. (Fields, not properties — already fine.)
- BCL obligations: `Count` (`IReadOnlyCollection`), `IReadOnlyList2D.NumRows/NumColumns`.
  These pin names to property syntax and can't be dropped.

## Proposed execution (staged, but each stage is a big-bang across its name-set)

1. **Emitter first, measure breaks.** Remove the primitive property carve-out; regenerate
   Unoptimized+Optimized+conformance. The compile errors ARE the authoritative call-site list
   (both in generated code and in `Plato.Intrinsics`).
2. **Convert `Number` + `Angle` intrinsics** to methods (keystone — `Number` pins the most
   names). Fix intra-intrinsic callers (`a.Pow3`, `Value.Abs()`, `Cubic/Linear/Quadratic`).
3. **Convert the vector/matrix/quaternion intrinsics** similarly.
4. **Sweep external consumers** (studio/examples/labs that use Plato-typed receivers) type by
   type, guided by build errors, not regex.
5. **Sync `ara3d-sdk`** (`regen-plato.ps1 -Apply` for intrinsics; `regen-generated -Apply`),
   run `check-all.ps1`, conformance 142/36/0.

## Open decisions for the author

- **Extension methods vs instance methods** for the intrinsic members? Generated side is
  extension-style; the handwritten intrinsics could expose either. Instance methods are the
  smaller diff and keep `x.Abs()` working; extension methods match the "everything is a free
  function" story. Recommend **instance methods on the structs** for the handwritten side
  (minimal churn, identical call syntax) — the *generated* side already emits extension
  forwarders on top.
- **Consumer scope:** all of studio/labs, or just Plato + conformance + examples for now?
- **Big-bang vs a temporary rename shim** to stage the consumer sweep.
