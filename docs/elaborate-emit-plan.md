# Elaborate → Monomorphize → Emit (scope)

The checker front-end (normalize → constrain → solve) computes a *solved* view of every
function but feeds nothing. This phase turns that view into a fully-typed, fully-resolved
IR (the **TIR**) that backends can eventually consume, so the writers stop re-deriving
semantics at emit time. It is three sub-passes; only the first is built so far.

```
 solved view (Solver: Substitution + ResolvedCalls)
      │  elaborate     Checking/Elaborator      → TIR                (increment 1 — DONE, shadow)
      ▼
 TIR (typed, resolved, coercions explicit)
      │  monomorphize  (planned) instantiate generics/concepts per concrete use
      ▼
 monomorphized TIR
      │  emit          (planned) retarget CSharp/TS/Rust writers onto TIR
      ▼
 generated code
```

Everything here is **shadow mode**: the TIR feeds no writer, so off-flag output stays
byte-identical (the `regen-plato.ps1` gate still passes).

## Why

The C# writer (`CSharpFunctionBodyWriter`) currently *guesses* semantics at emit time:
`HasArgList`, `MovedNoArgNames`, "is this name a type?", "property vs method", and a
scalar-erasure analysis that re-walks expressions to recover types. Those heuristics are
the accidental complexity the TIR retires: the checker already knows the callee, the
instantiated signature, and every implicit conversion, so it should *record* them once
rather than have three writers re-infer them.

The single most important thing the TIR makes explicit is **implicit conversion**. Today
`Vector3(0.0)` (a `Number → Vector3` broadcast) and `Number → Angle` casts are invisible in
the symbol graph — they are reconstructed from cast relations while emitting. In the TIR an
argument that matched a parameter via a conversion is wrapped in a deliberate
`TirCoerce(inner, fromType, toType, conversionFn)` node. The conversion class becomes a
visible IR node instead of an emit-time inference.

## TIR node shape (`Checking/Tir.cs`)

Every node carries its solved (zonked) `TypeExpression` (`null` on pure statement nodes) and
its originating `Symbol` (for diagnostics). Small model:

- Leaves: `TirLiteral`, `TirParameter`, `TirVariable`, `TirTypeRef`, `TirDefault`.
- `TirCall` — the resolved-call node. Carries the chosen callee `FunctionDef`, the
  **instantiated signature** (`ParameterTypes` + `ReturnType`), the elaborated argument
  nodes, and an **`EmissionKind`** ∈ {InstanceMethod, Property, StaticMethod, Operator,
  Constructor, Conversion, Intrinsic} derived from the callee's `FunctionType` + shape.
- `TirCoerce(Inner, FromType, ToType, ConversionFn)` — an explicit implicit-conversion.
- `TirInvoke` — applying a function *value* (a function-typed parameter/variable/lambda),
  distinct from resolving a named overload.
- Compound: `TirConditional`, `TirNew`, `TirArray`, `TirAssign`, `TirLambda`.
- Statements: `TirBlock`, `TirReturn`, `TirIf`, `TirLoop`.
- `TirUnresolved(Original, Reason, Children)` — total fallback for a call the solver could
  not resolve (keeps the pass total; nothing is ever thrown).

`TirFunction` is the top-level: original `FunctionDef`, parameters, return type, body node.

## Recording solver decisions

`Solver.CommitCandidate` is the single choke point where an overload is bound. It now also
records a `ResolvedCall` per `FunctionCall` (exposed as `Solver.ResolvedCalls`, surfaced on
`TypeCheckResult.ResolvedCalls`): the chosen callee, the instantiated parameter/return
types, the per-argument **match kind** (Exact / Generic / Concept / Conversion), and the
per-argument cast `IFunction` where the kind is Conversion. `MatchArg` was extended to return
its match kind alongside the existing `(ok, cost)` — ok/cost are byte-for-byte unchanged, so
solver ranking is untouched. Recording is purely additive (a dictionary write); the CHK202
common-return commit goes through the same choke point, so it is recorded too; ambiguous
(CHK203) and no-match (CHK201) calls are deliberately not recorded.

## Elaborate pass (`Checking/Elaborator.cs`)

Walks a normalized function body + the solver's `ResolvedCalls` → TIR. For a resolved named
call it emits a `TirCall`, derives `EmissionKind`, and — reading the recorded per-arg match
kinds — wraps any Conversion argument in a `TirCoerce`. It is **total**: an unresolved call
becomes a `TirUnresolved` node plus an `ELB001` info diagnostic; nothing throws.

`EmissionKind` derivation (callee `FunctionType` + call shape), first match wins:
Constructor → Constructor; Cast (or a type-named 1-arg call) → Conversion; Field → Property;
operator-named (`Operators.NamesTo*Operators`) → Operator; Intrinsic → Intrinsic; a no-arg
member access (1 param, `HasArgList == false`) → Property; else 0-param → StaticMethod, ≥1
param → InstanceMethod.

## Next increments (not in this run)

1. **Monomorphize**: specialize each `TirCall` per concrete instantiation; resolve generic
   and concept parameters to ground types; make `TirCoerce` targets concrete. This is where
   the "Self" refinements and generic element inference get committed structurally.
2. **Emit / retarget**: have `CSharpFunctionBodyWriter` (then TS/Rust) consume the TIR and
   delete the emit-time heuristics (`HasArgList`/`MovedNoArgNames`/property guessing/scalar
   re-inference). Land it behind a flag, prove byte-identity against the golden output, then
   flip the default.
