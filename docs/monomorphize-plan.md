# Monomorphize (scope)

The second sub-pass of Elaborate → Monomorphize → Emit. The elaborate pass produced one
generic/abstract `TirFunction` per solved function — its node types still contain `Self`,
type parameters, and interface (concept) types (a library function over `IVector`, a concept
method over `IArray<$T>`). Monomorphize **specializes** that TIR per concrete instantiation, the
way Plato's compilation model already stamps every generic/concept function into concrete
per-type implementations. It runs in **shadow mode**: the monomorphized TIR feeds no writer, so
off-flag output stays byte-identical.

## The oracle: ReifiedType / ReifiedFunction

Plato already monomorphizes for the current emitter. For every concrete `TypeDef` T,
`ReifiedType` stamps out a `ReifiedFunction` for each field, each method of each implemented
concept, and each applicable library function — substituting `Self → T` and each concept
type-parameter → the argument T binds (`ReifiedType.CreateFunction` applies a
`Func<TypeExpression,TypeExpression>` via `TypeExpression.Replace`). `Compilation.ReifiedFunctions`
is the flattened set. **That set is our correctness oracle**: the count, the shape, and the
ground signature of every instantiation.

Each `ReifiedFunction rf` exposes `Original` (the source `FunctionDef`, which owns a TIR when it
has a body), `ReifiedType.Type` (the concrete T), and the already-ground `ParameterTypes` /
`ReturnType`. We drive monomorphization **directly off this set**, so our instantiations track it
1:1 by construction.

## Substitution model

A `TypeSubstitution` is a name → `TypeExpression` map, applied structurally with the *same*
helper the reifier uses (`TypeExpression.Replace`), keyed by name (`"Self"`, `"T"`, `"$V0"`,
`"IVector"`) exactly like `TypeSubstitutions`. Applying it is total and null-safe.

We **derive** the substitution for `rf` by structurally pairing the original declared signature
against the reified ground signature (`TypeSubstitution.FromSignature`): where an abstract head
differs from the ground head, bind that node's whole structural form to the ground type (`Self`→T,
first-param `IArray<$T>`→T); where heads agree, recurse into type-args (`IArray<T>` vs
`IArray<Number>` binds `T`→`Number`). Keys are structural (`ToString`), matching the reifier's
`te.Equals(pt)` — so `IArray<$T>` maps but a different `IArray<Number>` does not. We then bind
`Self` → the reified type: library-over-interface bodies reference `Self` (e.g. `Reverse`'s
`Self.CreateFromComponents(…)`) even though `Self` is absent from their signature — a gap the
reifier shares — and `Self` always denotes the type being reified onto.

This anchors us to the oracle: applying the derived substitution to the original signature agrees
with applying it to `rf`'s signature — asserted for **every** reified function, not just a count.
The reifier's substitution is identity-based and imperfect (it leaves same-named `$T` variables
unbound in some positions; its `r.Verify()` is disabled for exactly this), so our structural
substitution is a *refinement* that grounds those further — the cross-check is stated as agreement
rather than byte-identity, and is the strong "reproduces the reified signature" claim wherever the
reifier fully succeeded.

## Increment 1 (this run)

1. **`TypeSubstitution`** — name-keyed map; `Apply` via `TypeExpression.Replace`; `IsGround`
   predicate (no `TypeVariable`/`TypeParameter`/`SelfType` node); `FromSignature` pairing.
2. **`Monomorphizer.Specialize(TirFunction, TypeSubstitution)`** — returns a new `TirFunction`
   with every node's `Type`, every `TirCall.ParameterTypes`/`ReturnType`, `TirCoerce.From/ToType`,
   `TirNew.NewType`, etc. substituted. Total — never throws.
3. **Driver** `Monomorphizer.MonomorphizeAll()` — for each `rf` in `Compilation.ReifiedFunctions`:
   derive the substitution, and (when `Original` has a body) specialize its elaborated TIR,
   yielding a `MonomorphizedFunction { Reified, Original, ConcreteType, Substitution, Tir,
   IsFullyGround }`. One entry per reified function.
4. **Concept re-dispatch (direct case only, safe)** — after specialization, a `TirCall` whose
   callee is a concept declaration (`FunctionType.Concept`) and whose receiver specialized to a
   concrete T is re-pointed to T's concrete implementation, found by an unambiguous signature
   lookup in `ReifiedFunctionsByName` (same name, matching ground parameter types, non-concept
   original). **Unique match only** — 0 or >1 leaves the call unchanged and counted as deferred,
   so we never mis-dispatch. EmissionKind is re-derived shape-free from the target.
5. **Tests** — synthetic (`Specialize` grounds types; a concept-method call re-points to a
   hand-built concrete impl) and integration over `plato-src` (total; count fully-ground TIRs;
   assert the derived substitution reproduces every `rf` signature; report re-dispatch coverage).

## Increment 3 (2026-07-10): grounding completed

The deferred classes below are closed. The grounding substitution is now derived from FOUR sources,
in order (`MonomorphizeAll`):

1. **Declared-signature pairing** (`FromSignature(original, …)`) — unchanged; keeps the reifier
   cross-check intact.
2. **Solver-ZONKED-signature pairing** — the TIR body's residual variables appear in *terminal*
   zonked form (`$G14`, not the declared `$T`), because the solver binds through chains; the
   elaborator records the zonked declared signature on `TirFunction` and a second pairing binds
   those terminal names. (Recorded call signatures are also re-zonked LIVE at elaboration —
   `CommitCandidate` snapshots them at commit time, before later unifications land.)
3. **Element-instance walk inside `Pair`** — a generic-interface head replaced by a concrete type
   also determines the interface's element types: `IArrayLike<$T>` ↔ `Vector3` finds Vector3's
   `IArrayLike<Number>` instance (transitive, per-level substituted) and binds `$T → Number`.
4. **Post-specialization residual grounding** — variables the signature cannot reach are paired
   against ground context: return-position types (including a call's PRE-coercion `ReturnType` —
   a `Tuple2<Angle,$r>` under an already-coerced `AnglePair`-typed node) against the reified
   return (Self-refined when it is an interface the Self type implements; a tuple pairs
   element-wise against the concrete struct's fields), and leftover interface instances against
   the Self type's own concept instances. Then re-specialize.

Also: **non-reified entries** — library functions whose first parameter is already a concrete type
(`Matrix(r: LookAt3D)`) are never stamped by the reifier but the writer emits them as members;
`MonomorphizeAll` now covers them directly (their signature is its own ground signature). And
**re-dispatch preserves the elaborator's call-site-shape `EmissionKind`** instead of re-deriving it
from the target (re-derivation turned `this.Matrix` into `this.Matrix()`).

Result: **1998 / 2014** bodied instantiations fully ground; the 16 non-ground remainder have no
emitted body in the default style (they are not writer-eligible). Every writer-eligible
member-instance body is covered — `FallbackDiagnosticsTests` gates this at 0.

## Deferred to increment 2 (historical — closed by increment 3)

The 127 concept calls left un-re-dispatched over the stdlib fall into two understood classes, both
genuinely out of scope for a safe direct pass:

1. **Non-ground argument types** — the call's own argument types still carry residual `$`-variables
   (`CreateFromComponents(Vector3, IArray<$T>)`, `CreateFromComponent(Vector3, $Expr1)`) because the
   checker's element inference did not fully ground them. Dispatch on a non-ground signature is
   refused. Needs the checker/solver to ground the element type first.
2. **No reified concrete implementation** — some concept methods (`CreateFromComponent`,
   `CreateFromComponents`) have *only* a `Concept` declaration reified onto the type; the concrete
   implementation is synthesized field-wise by the writer at emit time and never enters
   `ReifiedFunctionsByName`. Dispatching to it needs the field-wise-generated implementations
   modelled in the TIR.

Also deferred: transitive/multi-level and conversion-mediated dispatch, a call-shape-precise
EmissionKind after re-dispatch, and specializing the top-level parameter *symbols* (their ground
types already live on the body's `TirParameter` nodes and on `Reified.ParameterTypes`). Then:
retarget the writers onto the monomorphized TIR behind a flag and prove byte-identity.
