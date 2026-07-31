# Plan: Plato.RustWriter — generate working Rust from Plato

Goal: repeat the TypeScript proof of concept for Rust. Build a `Plato.RustWriter`
that compiles `stdlib-legacy/geometry.plato` to Rust, then port the 12 demo drivers
to Rust snippets that **compile and pass tests** (`cargo test`). No rendering.

This document is self-contained: it records the architecture to mirror, every
Rust design decision with rationale, the Plato-symbol translation table, and
the specific pitfalls hit while building the TypeScript pipeline. Verified
toolchain on this machine: cargo/rustc 1.96.0.

---

## 1. Hard constraints (same as the TypeScript effort)

1. **Never regenerate or modify C# code.** Do not invoke `compilation.ToCSharp`,
   do not touch `ara3d-sdk/toolchain/Plato/Plato.CSharpWriter/`, `PlatoCompiler/`
   (except read), or any generated C#.
2. **Original `.plato` sources are read-only**: `PlatoStandardLibrary/` must not
   change (it feeds C# generation; it currently has 3 pre-existing symbol errors
   — `CirclePoint` — unrelated to us).
3. The demo's Plato source of truth is
   `web/geometry-samples/stdlib-legacy/geometry.plato`. **Reuse it unmodified if
   possible** — the whole point is one source, many languages. If Rust needs a
   library addition (e.g. an explicit `ToNumber` for a Number/Integer mix),
   prefer adding to that file (it regenerates TypeScript too — rerun
   `npm run gen:plato` in `web/geometry-samples` and rerun `npm test` there to
   confirm nothing broke).
4. Do not modify `Plato.TypeScriptWriter` except for true shared bugs.

## 2. Prior art — read these first

| Path | What it is |
|---|---|
| `ara3d-sdk/toolchain/Plato/Plato.TypeScriptWriter/` | The project to mirror, file for file. Read all of it; it is ~1200 lines. |
| `ara3d-sdk/toolchain/Plato/Plato.TypeScriptWriter/README.md` | Output-model documentation and known limitations. |
| `ara3d-sdk/toolchain/Plato/Plato.CLI/Program.cs` | CLI: `[inputFolder] [outputFolder] --typescript`. Add `--rust` the same way. |
| `ara3d-sdk/src/Ara3D.Utils/CodeBuilder.cs` | Indented string-builder base class used by all writers. |
| `web/geometry-samples/stdlib-legacy/geometry.plato` | The curated Plato geometry library (concepts, Angle/Vector2D/Vector3D, Intrinsics, Numbers/Vectors2/Vectors3). |
| `web/geometry-samples/src/plato/plato.g.ts` | The generated TypeScript — the reference for what each Plato construct becomes. |
| `web/geometry-samples/src/samples/*.ts` | The 12 demo drivers to port to Rust. |
| `web/geometry-samples/tests/*.test.ts` | The invariants to port (19 tests: sample invariants + Plato conformance + adapter round-trip). |
| `Ara3D.SDK.sln` | Add the new project next to `Plato.TypeScriptWriter` (copy its GUID recipe: project entry + config block + nesting under folder `{2F8F3E96-A34F-4882-9496-6374618D098D}`). |

Compiler APIs the writers consume (all proven; mirror usage from the TS writer):
`Compilation.ConcreteTypes/AllTypeAndLibraryDefinitions/Libraries.AllConstants()/AllFunctions()`,
`ConcreteType.InterfaceFunctionGroups/UnimplementedFunctions/AllInterfaces`,
`FunctionInstance` (Name, ParameterNames, ParameterTypes, ReturnType, TypeVariables,
Implementation.Body, InterfaceName), `FunctionInstanceKind`
(InterfaceDeclared/InterfaceExtension/Constant/Lambda), `TypeDef`
(Fields, TypeParameters, Inherits, Methods, IsInterface(), IsSelfConstrained()),
`TypeInstance` (Name, ArgsWithSelf, Create), `Operators.NameToUnaryOperator/NameToBinaryOperator`,
`Symbol` tree (Expression/Statement cases — see the body writer), `RewriteLambdasCapturingVars()`.

## 3. New projects

### 3a. `ara3d-sdk/toolchain/Plato/Plato.RustWriter/` (C#, net9.0)

Mirror the TypeScript writer's files:

| File | Analog of | Notes |
|---|---|---|
| `Plato.RustWriter.csproj` | same | References `Ara3D.Utils` + `Plato.Compiler`. |
| `RustWriter.cs` | `TypeScriptWriter.cs` | Orchestrator; native maps; prelude; single output file `plato.rs`; `AllFieldNames` set. |
| `RustTypeWriter.cs` | `TypeScriptTypeWriter.cs` | Type-name resolution; intrinsic-body table; keyword escaping; `WriteTrimmed`. |
| `RustConcreteTypeWriter.cs` | `TypeScriptConcreteTypeWriter.cs` | Structs + impl blocks; extension traits for primitives. |
| `RustFunctionInfo.cs` | `TypeScriptFunctionInfo.cs` | Signatures: methods, statics (associated fns), trait items. |
| `RustFunctionBodyWriter.cs` | `TypeScriptFunctionBodyWriter.cs` | Symbol tree → Rust expressions/statements. |
| `ITypeToRust.cs` | `ITypeToTypeScript.cs` | Includes the FunctionN → closure-type handling. |
| `PlatoAnalyzer.cs` | same | Copy verbatim (language-agnostic; each writer keeps its own copy). |
| `RustWriterExtensions.cs` | same | `compilation.ToRust(folder)`. |
| `README.md` | same | Document the output model. |

Wire-up: add to `Ara3D.SDK.sln`; add `--rust` to `Plato.CLI` (reference the new
project in `Plato.CLI.csproj`; extend the arg handling in `Program.cs` — keep
the default C# path byte-identical). **The CLI does not create output folders —
create them before running** (this bit us once).

### 3b. `rust/geometry-samples/` (cargo crate, studio repo)

```
rust/geometry-samples/
├── Cargo.toml            # no dependencies; edition 2021+
├── gen-plato.ps1         # dotnet run --project ../../ara3d-sdk/toolchain/Plato/Plato.CLI --
│                         #   ../../web/geometry-samples/stdlib-legacy ./src --rust
├── src/
│   ├── lib.rs            # #![allow(non_snake_case, dead_code)] ; mod plato; mod core; mod samples;
│   ├── plato.rs          # GENERATED — do not edit
│   ├── core/
│   │   ├── mod.rs
│   │   ├── types.rs      # Drawable enum { Mesh{positions,indices,..}, Lines{..}, Points{..} }
│   │   ├── mesh_builder.rs  # compute_vertex_normals, grid_mesh, append_box_edges, vertex_at
│   │   └── random.rs     # mulberry32 port (deterministic, same seeds as TS)
│   └── samples/
│       ├── mod.rs        # registry: Vec of (id, build fn)
│       ├── parametric_surface.rs, icosphere.rs, terrain.rs, delaunay.rs,
│       ├── convex_hull.rs, spline_tube.rs, octree.rs, bvh.rs,
│       ├── half_edge.rs, raycast.rs, poisson_disk.rs, marching_squares.rs
└── tests/
    ├── plato_conformance.rs  # port of tests/plato.test.ts (algebra identities)
    └── sample_invariants.rs  # port of tests/samples.test.ts (12 invariants)
```

Convention split: the **generated Plato API keeps PascalCase** (`v.Length()`,
`v.X`, matching C#), enabled by `#![allow(non_snake_case)]`; hand-written
driver plumbing (loops, Vec handling) is ordinary Rust. This is deliberate —
the demo's point is that the Plato calls read identically in C#/TS/Rust.

## 4. Rust output model (the design decisions)

### 4.1 Primitives → native types + extension traits

| Plato | Rust | Fluent mechanism |
|---|---|---|
| Number | `f64` | `pub trait NumberExt { fn Sqrt(self) -> f64; ... }` + `impl NumberExt for f64` |
| Integer | `i64` | `pub trait IntegerExt { ... }` + `impl IntegerExt for i64` |
| Boolean | `bool` | `BooleanExt` |
| String | `String` (params/returns), trait on `str` (`&self`) | `StringExt` — low priority, geometry lib doesn't use it |
| Character | `char` | `CharExt` if needed |
| Dynamic/Type | skip / `()` | as in TS (`unknown`) |

This is the Rust analog of the TS prototype extension, and it is *idiomatic*
Rust (extension traits). `(0.5).Turns().Cos()` and `x.Sqrt().Clamp(0.0, 1.0)`
work on plain values. Callers need the traits in scope → emit a prelude module
and have samples `use crate::plato::*;` (put `pub use` of all traits at module
top level so a single glob import suffices).

**Improvement over TS**: `Number` and `Integer` are distinct Rust types, so the
name-collision problem (shared `Number.prototype`) disappears — `Integer.Divide`
(truncating, which `i64 /` does natively) and `Number.Divide` coexist. Remove
the "first-writer-wins" comment for Rust; keep per-trait claimed-name dedupe
only for true duplicates.

### 4.2 Concrete types → Copy structs + inherent impls

```rust
#[derive(Clone, Copy, PartialEq, Debug, Default)]
pub struct Vector3D { pub X: f64, pub Y: f64, pub Z: f64 }

impl Vector3D {
    pub fn new(x: f64, y: f64, z: f64) -> Self { Vector3D { X: x, Y: y, Z: z } }
    pub fn WithX(self, x: f64) -> Vector3D { Vector3D::new(x, self.Y, self.Z) }
    pub fn Equals(self, other: Vector3D) -> bool { self == other }   // via PartialEq
    // ... generated member functions, receivers by value (Copy)
    pub fn Length(self) -> f64 { self.LengthSquared().Sqrt() }
}
```

- Derive `Clone, Copy, PartialEq, Debug, Default` on every concrete type
  (derives add generic bounds automatically for `TupleN<T0,..>`).
- `Default` replaces the TS `static get Default` (body writer emits
  `Vector3D::default()` for type-as-value symbols; native primitives emit
  `0.0` / `0` / `false` literals as in TS).
- All functions are **methods, never getters** (Rust has no properties, so this
  falls out naturally); fields are public struct fields (`v.X`).
- Statics (Plato first param `_`) → associated functions (`Type::Name(...)`).
- Multiple `impl` blocks are legal — emission order is forgiving (nicer than TS).
- Optional (recommended, cheap): also emit `impl std::ops::Add/Sub/Mul/Neg`
  forwarding to the Plato methods when the type has them, so hand-written Rust
  can use `a + b`. Gate behind a bool on the writer; not needed for the POC tests.

### 4.3 Concepts → traits (declaration only)

`concept Comparable { Compare(a: Self, b: Self): Integer; }` →
`pub trait Comparable: Copy { fn Compare(self, b: Self) -> i64; }`
(`inherits` → supertraits). Do **not** generate `impl Trait for Type` blocks in
the POC — inherent methods carry the demos; trait impls are a later milestone.
Skip Array/Array2D/Array3D concepts (special-cased, as in TS).

### 4.4 IArray → trait + Vec-backed Arr

Don't port the closure-based `Arr` (boxed `dyn Fn` loses `Copy` and fights the
borrow checker for no demo value). Use:

```rust
pub trait IArray<T: Copy> { fn At(&self, n: i64) -> T; fn Count(&self) -> i64; }

#[derive(Clone, Debug, Default)]
pub struct Arr<T: Copy> { pub items: Vec<T> }
impl<T: Copy> Arr<T> {
    pub fn new(items: Vec<T>) -> Self { Arr { items } }
    pub fn At(&self, n: i64) -> T { self.items[n as usize] }
    pub fn Count(&self) -> i64 { self.items.len() as i64 }
    pub fn Map<U: Copy>(&self, f: impl Fn(T) -> U) -> Arr<U> { ... }
    pub fn Reduce<A>(&self, init: A, f: impl Fn(A, T) -> A) -> A { ... }
}
```

- Prelude: `pub mod Intrinsics { pub fn MakeArray<T: Copy>(items: Vec<T>) -> Arr<T>; pub fn Range(n: i64) -> Arr<i64>; }`.
- Generated IArray library functions with a **generic element** become methods
  on `Arr<T>` (same elemVar-substitution mechanics as
  `TypeScriptTypeWriter.WriteArrayMethod` — substitution dictionary maps the
  Plato type variable to `T`); **concrete-element** functions become free
  functions `pub fn Sum(xs: &Arr<f64>) -> f64` (same split as TS; detection:
  `fi.TypeVariables.Contains(elemVar)` where
  `elemVar = fi.ParameterTypes[0].ArgsWithSelf.LastOrDefault()?.Name`).
- ArrayLiteral symbol → `Intrinsics::MakeArray(vec![...])`.
- `At/Count` for array-implementing concrete types: generate as in TS
  (`GenerateFunc`): match/ternary chain over fields (`if n == 0 { self.A } else ...`),
  `Count` returns the field count.

### 4.5 Constants

`pub mod Constants { pub fn Pi() -> f64 { 3.1415926535897931 } }`.
**Body-writer difference from TS**: zero-arg constant references must emit
`Constants::Pi()` — with `::` and with parens (TS emitted `Constants.Pi` as a
getter). Two call sites in the body writer handle constants (the
zero-arg FunctionCall case and the FunctionGroupRefSymbol case); fix both.

## 5. Body-writer translation table

Start from `TypeScriptFunctionBodyWriter.cs` and change:

| Plato symbol | TypeScript emitted | Rust emit |
|---|---|---|
| expression body | `{ return expr; }` | `{ expr }` (expression body — cleaner) or `{ return expr; }`; pick `{ expr }` |
| ConditionalExpression | `(c) ? a : b` | `if c { a } else { b }` (valid expression) |
| ReturnStatement | `return e;` | `return e;` |
| LoopStatement | `while (c) {..}` | `while c {..}` |
| IfStatement | `if (c) {..} else {..}` | `if c {..} else {..}` |
| VariableDef | `let x = e;` | `let x = e;` (add `mut` only if Assignment targets it — scan body; or always `let mut` and allow unused_mut) |
| Assignment | `a = b` | `a = b` |
| Literal Number | `1.5` / `1` | ensure decimal: `1` → `1.0` (append `.0` when no `.`/`e`); Rust has **no implicit int→float** |
| Literal Integer | `3` | `3i64` when used as a method receiver, else `3` |
| Literal Boolean/String/Char | native | native (`"s"` is `&str` — fine for params typed `&str`; avoid String params in the curated lib) |
| FunctionCall (method) | `a.Foo(b)` | `a.Foo(b)` — identical |
| FunctionCall (field, 1-arg, name ∈ AllFieldNames) | `a.X` | `a.X` — identical (reuse the AllFieldNames mechanism) |
| FunctionCall on literal receiver | `(1).Foo()` | `1i64.Foo()` / `1.0.Foo()` (suffix; parens also fine) |
| FunctionCall on ternary receiver | `(c ? a : b).Foo()` | `(if c { a } else { b }).Foo()` — keep the parenthesize-receiver logic (Literal, ConditionalExpression, Lambda, Assignment) |
| static call `Type.F(x)` | `Type.F(x)` | `Type::F(x)` — TypeExpression in expression position emits `Name::` handling; simplest: emit `Type::F` by special-casing when Args[0] is TypeExpression |
| zero-arg constant | `Constants.Pi` | `Constants::Pi()` |
| NewExpression | `new T(args)` | `T::new(args)`; native target → passthrough `(arg)` (same guard as TS) |
| Lambda | `(a, b) => body` | `|a, b| body` (block bodies: `|a, b| { .. }`) |
| Tuple call | `new Tuple2(a,b)` | `Tuple2::new(a, b)` |
| TypeExpression as value | `T.Default` / native literal | `T::default()` / `0.0`/`0`/`false` |
| param0 ref (non-static) | `this` | `self` |
| `default` keyword | `(undefined as any)` | `Default::default()` |
| direct call of param/var | `f(a, b)` | `f(a, b)` |

Keyword escaping: extend the reserved set to Rust keywords (`fn let mut match
impl self Self type move ref box loop in where use mod pub crate super enum
struct trait const static as dyn async await unsafe extern`); escape with a
leading `_` (or `r#` raw identifiers — `_` prefix is simpler and matches TS).

## 6. Intrinsics table (`RustTypeWriter.TryGetIntrinsicBody`)

Receivers are `self` (by-value f64/i64/bool, `&self` for Arr/str).

| Key | Body |
|---|---|
| Number.Sqrt/Abs/Floor | `self.sqrt()` / `self.abs()` / `self.floor()` |
| Number.Pow | `self.powf(y)` |
| Number.Compare | `if self < y { -1 } else if self > y { 1 } else { 0 }` |
| Number.Acos/Asin/Atan | `Angle::new(self.acos())` etc. |
| Angle.Cos/Sin/Tan | `self.Radians.cos()` etc. |
| Integer.Divide | `self / y` (i64 division truncates natively) |
| Integer.ToNumber | `self as f64` |
| Integer.Range | `Intrinsics::Range(self)` |
| operators (`IsOperator`, native result) | unary `-self`, `!self`; binary `self {op} y`; `==`/`!=` stay (PartialEq) |
| Boolean.And/Or/Not | via operator path (`&&`, `||`, `!`) |
| unknown bodiless | `unimplemented!("Type.Name")` |

## 7. The 12 Rust sample snippets

Port each TS driver ~1:1 (they are already fluent-Plato; the Plato calls
translate verbatim, only the plumbing changes):

| Sample | Port notes |
|---|---|
| parametric_surface | `u.Turns().Cos()` identical; grid_mesh takes a closure `impl Fn(f64, f64) -> Vector3D` |
| icosphere | Vec<Vector3D> + midpoint cache (HashMap<(usize,usize), usize> — cleaner than string keys) |
| terrain | hash via `(ix * 127.1 + iz * 311.7).sin() * 43758.5453` fract; `v00.Lerp(v10, fx)` |
| delaunay | Vector2D; the super-triangle/cavity algorithm ports directly; HashMap for boundary edges |
| convex_hull | sort indices with `sort_by(|a,b| ... partial_cmp)`; `turn` = `(a-o).Cross(b-o)` via `.Subtract/.Cross` |
| spline_tube | fully fluent; `i.Clamp(0, n)` on i64 needs an Integer Clamp — either add `Clamp(x: Integer, ...)` to geometry.plato Intrinsics/Numbers, or use Rust `i.clamp(0, n)` in driver plumbing (acceptable: it's plumbing) |
| octree | recursive enum or struct with `Option<Box<[OctreeNode; 8]>>` — use `Vec<OctreeNode>` children like TS for simplicity |
| bvh | `Box<BvhNode>` for left/right; axis select via match returning a closure or an accessor fn |
| half_edge | direct port; `Vec<HalfEdge>` |
| raycast | Möller–Trumbore is the showcase — the Plato lines are identical to TS/C# |
| poisson_disk | direct port |
| marching_squares | CASES table as `const CASES: [&[usize]; 16]` or `Vec<Vec<usize>>` |

Each sample: `pub fn build() -> Vec<Drawable>` + exported algorithm fns for the
tests. `Drawable` is a plain enum; positions `Vec<f64>`, indices `Vec<u32>`.

Tests (ports of the TS suites; same seeds via the mulberry32 port so results
match across languages):
- `plato_conformance.rs`: fluent number ops, Turns/Degrees trig, vector algebra
  identities (add/sub round-trip, cross orthogonality, normalize length,
  lerp endpoints, midpoint, perpendicular, reflect), With functions, Default.
- `sample_invariants.rs`: all-samples finite/non-empty; icosphere Euler
  characteristic V−E+F=2 & unit radius; Delaunay empty circumcircle; hull CCW +
  containment; spline knot interpolation; octree exactly-one-leaf containment;
  BVH partition + centroid containment; half-edge twin/next/one-ring invariants
  + smoothing variance ratio < 0.35; raycast center hit t≈2 (tol 0.06) + miss;
  Poisson pairwise ≥ r; marching-squares |f−iso| < 0.25.

## 8. Execution phases (with gates)

**Phase 0 — Orientation (~15 min).** Read the prior-art files (§2). Run the
existing pipeline once to see it green:
`cd web/geometry-samples && npm test` (19 passing) and
`dotnet build ara3d-sdk/toolchain/Plato/Plato.CLI/Plato.CLI.csproj`.

**Phase 1 — Writer skeleton (gate: C# builds).** Create the project (copy the
TS writer wholesale, rename namespaces/classes TypeScript→Rust), add to sln,
add `--rust` to the CLI. Get it compiling before changing emission logic.

**Phase 2 — Output model (gate: `plato.rs` compiles under rustc).**
Implement §4–§6 top-down: prelude (Intrinsics mod, Arr, IArray trait) → traits
for concepts → extension traits for primitives → structs/impls → Constants mod
→ body writer. Iterate:
`dotnet run --project ...Plato.CLI -- web/geometry-samples/stdlib-legacy rust/geometry-samples/src --rust`
then `cargo build` in the crate (create `rust/geometry-samples` with lib.rs and
an empty samples mod first, so cargo has something to chew on). Budget the most
time here; expect 5–10 generate/compile iterations.

**Phase 3 — Conformance tests (gate: `cargo test plato_conformance` green).**
Port `plato.test.ts`. This proves the generated library semantics match
TypeScript's before any sample exists.

**Phase 4 — Samples + invariants (gate: full `cargo test` green).** Port the 12
drivers and `samples.test.ts`. Do icosphere + raycast first (they feed bvh /
half_edge / raycast and have the crispest tests).

**Phase 5 — Wrap-up.** `cargo clippy` (fix or allow), README for the crate
(pipeline diagram, how to regenerate, conformance-test story), update
`Plato.RustWriter/README.md`, re-run the TS pipeline (`npm test`) to prove
nothing regressed, and run the constraint audit:
`git -C ara3d-sdk status --short` must show only `Plato.RustWriter/`, `Plato.CLI`
(csproj + Program.cs), and nothing under `Plato.CSharpWriter`, `PlatoCompiler`,
or `PlatoStandardLibrary`.

## 9. Pitfalls learned from the TypeScript build (checklist)

1. **Create output folders before running the CLI** — it throws otherwise.
2. **Numeric literals**: `ToLiteralString(1.0)` yields `"1"`. TS didn't care;
   Rust hard-errors on int/float mixing. Force `.0` on Number literals. This is
   the single most likely source of compile errors.
3. **No implicit Integer→Number conversions in Rust.** If generation of
   `geometry.plato` produces a mix (watch `Count()`-derived values flowing into
   f64 math), fix in the .plato source with `ToNumber` rather than hacking the
   writer. The current geometry.plato is believed clean (all-f64 math).
4. **Receiver parenthesization**: literals and ternaries as receivers must be
   wrapped/suffixed (`(if c {a} else {b}).Norm()`, `1.0f64.Sqrt()`); this bug
   produced silently-wrong TS once (`c ? a : b.Normalize()`).
5. **Field access vs method call**: Plato field reads are 1-arg function calls
   in the symbol tree with **no marker**. Reuse the `AllFieldNames` set built in
   the writer ctor from `ConcreteTypes[].TypeDef.Fields` (see
   `TypeScriptWriter` ctor + `WriteFunctionCall`).
6. **No overloading in inherent impls**: keep the claimed-names dedupe
   (`TryClaimMember` / `ClaimedNames`) per type and per extension trait.
   Overloads across Number/Integer are fine in Rust (separate types).
7. **SkipFunction subtleties**: skip functions named after fields, the
   `IgnoredFunctions` set, self-casts (`f.Name == SimpleName`), and
   Array-interface functions *except* `At`/`Count` (types need them; C#'s
   IReadOnlyList shortcut doesn't exist here either).
8. **The array concept is named `Array`** in the current dialect (grammar
   accepts both `concept` and `interface` since the Parakeet change) — the
   writer must treat `Array`/`IArray` names equivalently (`ArrayInterfaceNames`).
9. **Sub-writer indentation**: `CodeBuilder.Write(multiline)` doesn't reset
   line-start state; use the `WriteTrimmed` pattern (trim trailing newline, then
   `WriteLine`) everywhere a rendered chunk is inserted (constants, types,
   array methods).
10. **elemVar substitution** for Arr methods: only lift functions whose first
    parameter's element is a *type variable* (`fi.TypeVariables.Contains(elemVar)`);
    concrete-element functions become free functions, or you'll corrupt
    signatures (this was a real bug: `Sum(xs: Array<Number>)` briefly became
    `Sum(): T` on the generic interface).
11. **Compiler scaffolding**: any .plato input set must declare TupleN (needed
    for every multi-field type), Function0..N, Character/Dynamic/Type/Error,
    and the Array concept, or `Compilation` throws `Value cannot be null (def)`
    from `SymbolFactory.CreateTuple`/`TypeResolver.CreateType`. geometry.plato
    already has all of this.
12. **Rust naming warnings**: `#![allow(non_snake_case, dead_code)]` at the top
    of the generated file *and* the crate's lib.rs, or the build drowns in
    warnings (`cargo test` still passes, but keep it clean).
13. **`Error` struct**: fine in Rust (no global to shadow), but don't name the
    generated trait module `std`-anything.
14. **Trait-in-scope**: fluent number calls need the extension traits imported.
    Emit everything in one module and have consumers `use crate::plato::*;`.
    Add one conformance test that deliberately calls `(0.5).Turns().Cos()` to
    catch scope regressions.
15. **Determinism**: port mulberry32 exactly (u32 wrapping arithmetic —
    `wrapping_add`/`wrapping_mul`, `>>>` → `>>` on u32) so cross-language test
    outputs stay comparable.

## 10. Open decisions (defaults chosen; flag if you disagree)

1. **PascalCase Plato API in Rust** (`v.Length()`, `pub X`) with
   `allow(non_snake_case)` — parity with C#/TS wins over Rust idiom. A future
   writer flag could emit snake_case.
2. **Integer → i64** (indexes cast internally with `as usize`).
3. **Traits for concepts are declaration-only** in the POC (no impl generation).
4. **`std::ops` operator impls**: optional milestone, off by default.
5. **Arr is Vec-backed** (not lazy/closure-based). Revisit if laziness matters.
6. **String support minimal** (geometry demos don't use it).

## 11. Definition of done

- `dotnet run --project ...Plato.CLI -- web/geometry-samples/stdlib-legacy rust/geometry-samples/src --rust`
  regenerates `src/plato.rs` from the **same** .plato file that generates the
  TypeScript library.
- `cargo build` clean; `cargo test` green (conformance + 12 sample invariants,
  mirroring the 19 TS tests).
- `web/geometry-samples`: `npm test` still 19/19 (no TS regression).
- Constraint audit (§8 Phase 5) clean: no C# regenerated, no original .plato
  sources modified, C# writer untouched.
