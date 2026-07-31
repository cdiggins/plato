---
id: plato-231
title: Type-level naturals (const generics) for Plato type system
type: idea
status: idea
priority: "?"
effort: "?"
risk: "?"
area: plato
sprint: 
created: 2026-07-27
closed:
links: [submodules/Plato/Plato.AST/Ast.cs, submodules/Plato/PlatoCompiler/Checking/Monomorphizer.cs, submodules/Plato/PlatoCompiler/Analysis/TypeSubstitutions.cs, tracker/issues/plato-015.md, tracker/issues/plato-230.md]
---

## Idea
Add type-level natural numbers (const generics) to Plato: types and functions may take an
integer parameter alongside type parameters — e.g. `Vector<T, N>`, `Matrix<T, R, C>`,
`FixedArray<T, N>`. The compiler treats `N` as a compile-time value participating in type
identity and substitution. Primary payoffs: (1) fixed-length arrays as first-class types,
(2) small fixed arrays lower to flat structs / stack storage instead of heap `IArray<T>`,
(3) a principled foundation for vectorization/SIMD (the backend knows element count
statically) and for GLSL emission (GLSL vec/mat types ARE fixed-arity — today the mapping
is name-convention-based per concrete type).

## Assumptions
- Monomorphization stays the compilation model (it is — `Monomorphizer.cs` already
  specializes every reified function per concrete type instantiation). Const generics ride
  the same machinery: each distinct `N` is just another instantiation key.
- Only *literal* naturals and *parameter passthrough* are needed initially. No arithmetic
  on type-level numbers (`N+1`, `N*M`) — that is where const-generic systems (Rust took
  years) get expensive. Concat/flatten signatures would need it; defer.
- The stdlib's current pattern — `Vector2/Vector3/Vector4`, `Matrix2x2…4x4` as separately
  written concrete types (plato-src-v3: 154 concepts, 1125 types) — is acceptable today,
  so this is leverage/perf work, not unblocking work.

## Design decisions
- **Full const generics vs fixed-array-only** — a general `N` on any type (big, invasive)
  vs a single built-in `FixedArray<T, N>` type former the compiler knows specially
  (small, contained). Fixed-array-only gets ~70% of the benefit (flat storage,
  known-count loops, unrollability) without touching unification generality.
- **Where N lives in the AST** — new `TypeKind` (e.g. `ConstNat`) so a `TypeExpression`
  argument slot can hold a number (Ast.cs:307 enum + `TypeSubstitutions`/`TypeSubstitution`/
  `TypeChecker`/`TirTypeVerifier` all switch on kind), vs a parallel "const argument list"
  on TypeInstance. New TypeKind is less plumbing but every kind-switch must be audited
  (~6 files in Checking/ + Analysis/).
- **C# lowering** — C# has no const generics, so `FixedArray<Float, 3>` must monomorphize
  to a generated struct (`FixedArrayFloat3`) or map onto existing Vector3-style types.
  Name-mangled struct-per-(T,N) is the honest lowering; dedupe against hand-written
  stdlib types is a policy question.
- **Does N unify?** — can a function be generic over N (`Dot(a: FixedArray<Float, $N>, b: FixedArray<Float, $N>)`)?
  If yes, unifier must carry nat variables (moderate). If no (N always concrete at use
  sites), much cheaper but kills the main API-cleanup payoff.

## Related
- [plato-015](plato-015.md) — perf: specialized extensions, native types, SIMD versions; const
  generics is the type-system substrate that would make the SIMD line principled.
- [plato-230](plato-230.md) / plato-src-v3 — 1125 hand-enumerated types; many (VectorN,
  MatrixRxC, TupleN cap at 10 fields) exist only because arity can't be abstracted.
- [submodules/Plato/Plato.AST/Ast.cs:307] — TypeKind enum, the seam a ConstNat kind enters.
- [submodules/Plato/PlatoCompiler/Checking/Monomorphizer.cs] — existing specialization
  engine; distinct N values become ordinary instantiation keys.
- [submodules/Plato/Plato.GlslWriter] — GLSL target where fixed arity is mandatory
  (`float[3]`, vec3); today bridged by concrete-type naming convention.

## Effort / risk assessment
- **Fixed-array-only variant**: parser (type-arg grammar accepts integer literal), one new
  TypeKind + audit of ~6 kind-switch sites, monomorphizer key extension, C# writer struct
  synthesis, GLSL writer `T[N]` mapping. Est. **M–L (1–2 focused weeks)**, risk **medium**
  — contained, but the type checker is at 70/859 residual diagnostics and TIR is sole C#
  body writer, so any unification change lands on a hot, recently-stabilized path.
- **Full const generics with nat variables**: unification, variance, inference through
  function types, error messages. Est. **XL (month+)**, risk **high**. Rust's history says
  arithmetic-free is tractable, arithmetic is a tarpit.
- **Benefits**: stdlib compression (dozens of VectorN/TupleN families → generic defs);
  flat/stack small-array reps (today every `IArray<T>` is heap `PlatoList`); loop
  unrolling + SIMD with statically-known counts (plato-015); tighter GLSL mapping.
  None of these are *blocked* today — they're bought with duplication.

## Approaches
Short term:
1. **Spike doc, no code**: write 5 stdlib signatures (Dot, Map, Zip, Matrix mul, Concat)
   as they'd look with const generics; decide unification question on paper. ~half day.
2. **FixedArray-only built-in** behind the compiler flag pattern (like `--no-properties`,
   `--optimize-arrays`): parser + ConstNat kind + monomorphize + C# struct synthesis.
3. Piggyback on optimizer: teach `--optimize-arrays` stage to recognize statically-sized
   array literals and lower to structs *without* surface syntax — perf win, zero type-system risk.
Long term: generic-over-N stdlib (one Vector<N> def), SIMD lowering keyed on N∈{2,3,4,8,16},
Matrix<R,C> unifying all 9 matrix types, GLSL arrays; **unit/dimension checking** as a
direct application — dimensions are integer exponent vectors (`Unit<M, Kg, S, …>` with
signed type-level integers), so the same const-argument machinery in TypeInstance carries
compile-time dimensional analysis (note: needs *signed* nats and exponent arithmetic on
multiply/divide, so it lands only after the arithmetic question is answered — it is the
strongest motivation FOR eventually doing limited type-level arithmetic).
Adjacent ideas worth their own issue: type-level nat *arithmetic* as a general facility.

## Bedrock
The seam this strengthens is `TypeInstance`/`TypeKind` (Plato.AST/Ast.cs:307): making type
arguments a sum of {type, nat} rather than types-only is the one-time generalization that
later carries SIMD widths, unit exponents, and matrix shapes without new machinery. The
monomorphizer already provides the evaluation strategy for free. Verdict:
**simplest-along-the-grain** — the simple version (FixedArray-only, no nat variables, no
arithmetic) must NOT special-case fixed arrays *outside* the type-argument representation
(e.g. as a magic name-mangled string) — N must live in the TypeInstance so the general
form stays reachable.

## Done means
(capture-stage; firm up before idea→ready)
- [ ] Decision recorded (ADR): fixed-array-only vs full const generics, and whether N unifies
- [ ] `FixedArray<T, N>` parses, type-checks, and monomorphizes to a flat C# struct
- [ ] One stdlib consumer (e.g. a Polygon with N verts or a Bezier<N>) compiles through it
- [ ] GLSL writer emits `T[N]` or vecN for FixedArray
- [ ] No regression in conformance suite / 859-diagnostic baseline

## Simplest possible implementation
`FixedArray<T, N>` as a compiler-known type former: parser accepts integer literal in type-arg
position only for this one name; ConstNat TypeKind holds it; monomorphizer key includes N;
C# writer emits one struct per (T, N) with fields `E0..E(N-1)` implementing `IArray<T>`.
No nat variables, no arithmetic, no user-defined const params.
- Get: stack-flat small arrays, static count for unroll/SIMD, GLSL arrays, proof of the seam.
- Give up / risk: stdlib can't be generic over N yet (no signature compression); one more
  TypeKind every future kind-switch must handle; touches recently-stabilized TIR/checker path;
  struct-per-(T,N) codegen bloat if N used indiscriminately.
