# Emit retarget (scope)

The third sub-pass of Elaborate → Monomorphize → Emit. Elaborate + Monomorphize produce a
fully-typed, fully-resolved, fully-ground **TIR** (`Checking/Tir.cs`) for 624 stdlib functions. This
sub-pass retargets the C# writer to *read* that TIR instead of re-deriving semantics at emit time —
so `CSharpFunctionBodyWriter`'s heuristics (`HasArgList`, `MovedNoArgNames`, property-vs-method
guessing, call-site `()` injection, scalar re-inference) can eventually be deleted. This increment is
a **measurement**, not a switch-over: it proves, byte-for-byte, how close a TIR-based body writer
gets to the current writer, so the eventual default flip is de-risked by data.

Everything here is **off the default path**. Nothing in the production pipeline
(`CSharpWriter` → `CSharpConcreteTypeWriter` → `CSharpFunctionBodyWriter`, driven by `Plato.CLI`)
references the new code. The byte-identity gate (`tools/regen-plato.ps1`) stays green
(**164 identical, 0 differing**).

## Target: the default C# style, function bodies

First target is the **default** C# style (what `regen-plato.ps1` compares — not extension/scalar/
optimize) and function **bodies**, where the emit-time heuristics live. The subset is the 624
functions that monomorphize to fully-ground TIR, restricted to those a body is actually emitted for
(bodied, non-skipped instance members). Signatures, operator/cast/field scaffolding, and the
`--optimize`/`--scalar`/extension styles are out of scope for this increment.

## Deliverables

- **`Plato.CSharpWriter/TirCSharpBodyWriter.cs`** — renders a C# body string from a ground
  `TirFunction`, mapping `EmissionKind` → syntax directly (Property → `a.X`; InstanceMethod →
  `a.M(args)`; Constructor → `new T(args)`; Conversion → `a.T`; Intrinsic/StaticMethod → call form;
  `TirCoerce` → the explicit conversion), with **no `HasArgList` guessing**. Type/self/namespace
  rendering and the `{ get … }` / `=>` / `;` framing are delegated to the same `CSharpTypeWriter`
  the reference uses, so every measured difference is attributable to how the *expression tree* is
  emitted (symbol-graph heuristics vs. TIR).
- **`PlatoTests/EmitDifferentialTests.cs`** — the differential harness. Drives the writer's own
  per-type enumeration (`ConcreteTypes` → `InterfaceFunctionGroups` → `ChooseBestFunction`, with the
  writer's `SkipFunction`) to obtain the reference body strings that appear in the golden output;
  joins each to the ground TIR by `(FunctionDef, concrete-type name)`; renders the TIR candidate;
  compares byte-for-byte; and reports the match rate and a ranked mismatch taxonomy.

## Off-by-default flag strategy (for the eventual flip)

The retarget lands behind a `CSharpWriter.UseTir` flag (default **false**), analogous to the existing
`ExtensionStyle` / `ScalarErase` / `Optimize` flags. When false, `CSharpFunctionBodyWriter` runs
unchanged and output is byte-identical. When true, `WriteBody` dispatches to the TIR writer for
functions that have a ground TIR and falls back to the current writer otherwise (so partial-TIR and
un-aligned functions never regress). The flag flips to true only after the differential reaches
byte-identity on the target subset.

## Differential methodology

For each aligned `(FunctionDef, type)`: `reference = CSharpFunctionBodyWriter(...).ToString()` vs.
`candidate = TirCSharpBodyWriter(...).ToString()`, compared as raw strings. Mismatches are bucketed
by *reconciling transform* — each bucket is **defined** as "these become byte-identical after exactly
this one change", so the bucket sizes are a rigorous scope for the remaining work, not a guess. The
harness is total (the TIR writer must never throw over the ground subset) and writes no files.

## Measured result (this increment)

Target subset 624 ground bodied TIRs; 611 aligned to a current-writer body (97.9% of the subset;
reference/TIR-writer exceptions: 0/0).

| | count | rate |
|---|---:|---:|
| **byte-identical** | **474 / 611** | **77.6 %** |
| property-vs-method-parens (`x.Sqrt()` vs `x.Sqrt`) | 98 | |
| explicit-coercion (`this.K.Number` vs `this.K`) | 33 | |
| eta-expanded member ref (`(_e)=>_e.M11()` vs `M11`) | 4 | |
| coercion + parens (compound) | 2 | |
| structural-other | 0 | |

Every mismatch is explained by one of four understood mechanisms — the taxonomy fully accounts for
the gap.

## Criteria for the eventual default flip

1. **Byte-identity on the target subset** (this differential → 100% on aligned ground TIRs).
2. **Behind the `UseTir` flag** with the current writer as fallback; full `regen-plato.ps1` green
   with the flag *on*, on the whole library (not just the aligned subset).
3. **Flip the default** only when (2) holds across default style, then extend to
   extension/scalar/optimize styles and the TS/Rust writers.

## Recommended increment 2 (close the taxonomy, in order)

1. **Property-shape on `TirCall`** (closes 98 + 2 → ~93.6%). The writer keys property-vs-method on
   call *shape* (`Args.Count == 1 && !HasArgList`), but `Elaborator.DeriveEmissionKind` checks
   `Intrinsic`/`Operator` *before* the no-arg-member `Property` shape, so no-arg intrinsic/operator
   members (`Sqrt`, `Cos`, `Negative`, …) are classified non-Property and the TIR writer emits `()`.
   Fix in the elaborator: test the `NumParameters == 1 && !HasArgList` property shape first (it is
   shadow-mode, so byte-identity is unaffected), or carry an explicit `IsMemberAccess` bit on
   `TirCall`.
2. **Suppress implicit-widening coercions at emit** (closes 33 + 2 → ~99.0%). Solver-inserted
   `TirCoerce` widenings (Integer→Number, Self→interface) are emitted implicitly by the current
   writer; render `TirCoerce` inner-only while keeping explicit conversion *calls*
   (`TirCall[Conversion]`, e.g. the `Vector3(0.0)` broadcast → `.Vector3`) as-is.
3. **Eta-expansion round-trip** (closes the last ~4). The normalizer eta-expands a bare member-group
   reference in value position (`M11` → `(_e) => _e.M11()`); recover the bare reference at emit, or
   have Emit consume the pre-normalization tree for these leaves.
