# The Plato interpreter

This document describes `src/Plato.Interpreter`: an evaluator over the compiler's Typed IR (TIR)
that runs any bodied Plato function, and every `Law_*` function, in the compiler's own process,
with no C# emitted or built. It is aimed at people working on the interpreter, on the law runner,
or on the verification tooling that builds on it (`sandbox/step-brep-mesh/SPEC-EXECUTION.md` in
the bim-open-toolkit repository, whose `plato verify` is the engine's first client beyond laws).

For the pipeline the interpreter sits on, see [`compiler-pipeline.md`](compiler-pipeline.md). For
measurements (how long a run takes, how many laws pass on a given day) see the dated report
[`reports/interpreter-turnaround-2026-10-17.md`](reports/interpreter-turnaround-2026-10-17.md);
this document states design only, as [`documentation-conventions.md`](documentation-conventions.md)
asks.

## Why it exists

Before the interpreter, every change to a Plato body meant compile, generate C#, build the
generated project, then run the laws through the reflection runner in
`tests/conformance/Plato.ForwardConformanceTests`. That loop was slow enough that algorithm bodies
were drafted in C# first and ported later. Plato is pure, its sums are closed, and its laws are
Boolean functions over values the same program can construct, so an evaluator over the TIR is
small, and it can run the laws in the same process as the front end. The interpreter is also the
execution engine the specification tooling needs: a bounded quantifier, an invariant, or a theorem
test line is an expression evaluated against an environment, and that is the one operation the
interpreter exposes.

## Where it sits

```
 source (.plato)  ─parse─▶ AST ─bind─▶ Symbol graph ─normalize, constrain, solve, elaborate─▶ TIR
                                                                                              │
                                              ┌───────────────────────────────────────────────┤
                                              ▼                                               ▼
                                     Interpreter (this document)                    Monomorphizer ─▶ writers
```

The interpreter consumes the elaborated TIR of one `Compilation` (`Interpreter.cs`). It does not
run the monomorphizer: generic bodies run unspecialized, and the decision the monomorphizer makes
statically, which concrete function a call through an interface reaches, is made at run time from
the receiver's value (see Dispatch).

Elaboration is lazy and per function. `Interpreter.GetTir(FunctionDef)` checks and elaborates a
body the first time it is reached and caches the result, so a law over a small corner of a
library costs only the functions it calls, and a program with thousands of functions does not
pay for all of them before the first law runs. `ElaboratedCount` reports how many bodies a run
touched; the `plato run` summary line prints it.

## Values

Interpreter values (`Values.cs`) are ordinary .NET objects. Every value carries enough type for
dispatch to match it against a parameter at run time, which is what lets generic bodies run
without specialization.

| Plato | Representation |
|---|---|
| `Number` | `double`, under every profile (see Number profiles) |
| `Integer` | `long` |
| `Boolean`, `Character`, `String` | `bool`, `char`, `string` |
| a product type | `PlatoRecord`: the `TypeExpression` (with type arguments when known) and one slot per declared field, in declaration order |
| a sum type | `PlatoSum`: the type, the `SumCaseDef` it carries, and the case's payload |
| `Array<T>` | `PlatoArray`: an immutable list plus the element type |
| `List<T>`, `Buffer<T>` | `PlatoBuilder`: the two affine builders; a mutator returns the same builder, `Freeze` hands the storage to a `PlatoArray` and invalidates the builder, as `PlatoList` and `PlatoBuffer` do in the C# runtime |
| a function value | `PlatoFunction`: a lambda closed over the environment it was created in, or a host delegate |
| a type used as a value | `TypeToken`: the receiver of a `_`-parameter function such as `Number.Pi` or `BrepModel.Witness_UnitBox` |

Arrays are eager: every element is materialized when the array is built, so `MapRange` over a
million elements performs a million calls up front. The C# runtime's array is lazy. The law
runner's inputs are small, and eagerness is the simpler choice; a client that needs laziness
would add it to `PlatoArray` rather than to the evaluator.

`ValueEquality` gives structural equality and hashing, field by field, mirroring what the C#
backend generates for a struct: a `Number` compares with `double.Equals`, so NaN equals NaN and
`0.0` equals `-0.0`. `ValueFormat` renders a value for a report (round-trip numbers, nested
records as `Type(f1, f2)`, arrays as `[a, b]`), truncated past a size budget so a failing witness
over a large model stays readable.

## Number profiles

A `NumberProfile` (`src/Plato.Compiler/Types/NumberProfile.cs`, the per-compilation profile of
TKT-315) fixes the width of `Number` and `Integer`. The interpreter takes one in its constructor
and routes every literal and every host intrinsic through `Numbers` (`Numbers.cs`):

- `Number` is always stored as a `double`. Under `cpu64` it is binary64 arithmetic. Under `gpu32`
  each result of `+`, `-`, `*`, `/` and `sqrt` is rounded to binary32 after the binary64
  operation, which gives the correctly rounded binary32 result, so the stored doubles are exactly
  the floats a `gpu32` C# build would hold. Transcendental functions go through `MathF` under
  `gpu32` and `Math` under `cpu64`, as the C# runtime's `NumberMath` alias does.
- `Integer` is a `long`. Under `gpu32` it wraps to 32 bits after every operation, as the C#
  runtime's `int` does; under `cpu64` it is 64-bit storage, exact within 2^53 when converted to
  `Number`.
- Hashes, `MinValue`, `MaxValue`, `Epsilon` and `Pi` follow the profile's width.

A literal typed `Number` by the checker is a `double`; one typed `Integer` is a `long`. The
profile is chosen once per `Interpreter` instance; running the same compilation under both
profiles is two instances over the same `Compilation`.

## Evaluation

`Interpreter.Evaluate(TirNode, Environment)` is a case per TIR node class (the node classes are
in `src/Plato.Compiler/Checking/Tir.cs`). Expressions evaluate to a value; `TirBlock`, `TirIf`,
`TirLoop` and `TirReturn` are executed as statements, and a block body that ends without a
`return` is a fault. `match` needs no case of its own: the elaborator has already lowered it to a
chain of conditionals over the sum's predicate and projection calls (see Dispatch, syntactic
calls).

An `Environment` (`Environment.cs`) binds parameters and `var` locals by their definition
symbol, has a parent for nested scopes, and records the concrete type `Self` denotes in the
activation (the receiver's run-time type when the first parameter is abstract, else its declared
type). A lambda captures a flat snapshot of the bindings visible where it was created: the
value-at-creation semantics the C# writer reproduces by hoisting captured locals into copies.

Activations deeper than `MaxDepth` are reported as a fault rather than overflowing the host
stack; `plato run` evaluates on a thread with a large stack so that deep but finite Plato
recursion fits.

## Dispatch

Dispatch (`Dispatch.cs`) decides which function a call reaches once the argument values are
known. There are four routes, tried in this order for a `TirCall`:

1. **Static callee.** The elaborator resolved the call to one `FunctionDef`. A field reads the
   slot; a constructor builds the record; a cast returns its argument; a sum factory builds the
   case; the sum's generated equality compares structurally; a library function with a body runs
   it; a bodiless one goes to the host table.
2. **Interface declaration, re-dispatched.** When the static callee is an interface member, the
   call is resolved again by the receiver's run-time type: a field of that type with the member's
   name, then a library function over the concrete type, then one over an interface the type
   implements, then the shape-derived members below, then a default body on the interface. This
   is the decision the monomorphizer makes statically once the receiver is concrete.
3. **Syntactic call.** A call the elaborator left without a callee: the predicates, projections
   and factories the `match` lowering and the qualified case syntax produce (`IsCircle(c)`,
   `Circle_Radius(c)`, `BrepCurve.Circle(f, r)`), a field read, or a name resolved against the
   argument values by the checker's own tiers (exact, generic, interface, conversion).
4. **Shape-derived members.** The members the C# writer synthesizes from a struct's declared
   shape rather than from Plato source: structural `Equals`, `NotEquals` and `Hash`; `Count` and
   `At` over the fields, or over a single indexable field; `Components`, `NumComponents`,
   `FromComponents`; `FieldNames`, `FieldValues`, `TypeName`.

Overload choice (`ResolveByValues`) scores each candidate by the checker's tiers, summed over
the arguments; ties go to an implementation over an interface declaration, then to a bodied
function, then to declaration order. A bare group name in value position (`One`, `Zero`,
`Epsilon`) resolves to the nullary function returning the expected type, or to the
`_`-receiver function of that type.

## Conversions

The checker admits a fixed set of conversions at a boundary, and the elaborator records most of
them as `TirCoerce` nodes. The interpreter applies the same set, in `CoerceTo`, at every place
the TIR does not spell one out: a function's return, a constructor's fields, and the arguments of
an overload chosen at run time.

- `Integer` to `Number`.
- A tuple to the struct of the same shape (`(x, y)` returned where a `Point2D` is declared).
- A one-field struct to its field's type (the compiler's single-field cast, `Number(a: Angle)`).
- A user conversion: a one-parameter function named after the target type that takes the source
  type.

A `TirCoerce` with a named conversion function applies that function when its parameter accepts
the value, else the structural rules above; a value that fits neither is a fault, because the
checker promised a conversion.

## Host intrinsics and host obligations

The stdlib's bodiless primitives (`stdlib/foundation/intrinsics.library.plato`) are supplied by
`HostIntrinsics` (`HostIntrinsics.cs`): a table keyed by the name and the declared parameter
type names, `Add(Number, Number)`, `Count(Array)`, mirroring the C# runtime in
`src/Plato.Intrinsics` operation for operation, with the profile deciding the width. The array
kernel is the five functions `Count`, `At`, `MapRange`, `Reduce` and `FlatMap`; everything else
over arrays is Plato.

A bodiless function with no table entry is a **host obligation**, raised as
`HostObligationException` with the function's key. The STEP design's deferred algorithms are
declared this way on purpose (`TASKS.md` in the sandbox lists which chunk gives each one a body),
so the law runner reports an obligation by name instead of counting the law as failed, and never
substitutes a default value. An interface member no implementation satisfies is reported the
same way, with the receiver's type.

Faults inside a host operation (a bad index, a division by zero, an invalid cast) become
`EvaluationException`s that carry the chain of Plato functions active at the time, outermost
last, so a report can say where a law faulted.

## The law runner

`LawRunner` (`LawRunner.cs`) is a thin driver over the interpreter; the conventions live here,
not in the evaluator, so a different convention is a different driver over the same engine.

- A **law** is a bodied function named `Law_*` returning `Boolean`. Its parameters are its
  domain.
- A **witness** is `Witness_X(_: T): T`, the sandbox's static-receiver form, or a nullary
  `Witness_X(): T`. Every witness returning a type is evaluated once and cached; a witness that
  faults is left out, and the laws over it report the fault.
- A **measure** is a companion `Measure_<law>` with the law's parameter list. When a law fails or
  faults, the runner evaluates the measure on the failing witness and prints it beside the
  witness, so a report names how far off the result was, not only that it was.
- A `_` parameter ranges over the type token of its declared type.
- **Finite domains are exhausted**: `Boolean`, a sum of payload-free cases, a zero-field type
  (a `primitive` declaration such as `Number`, `String`, `Character`, `Array<T>` or
  `FunctionN<...>` is not one: its values are generated at the top level of a law as in a field,
  fixed in 1a3298fa and guarded by `ValueDomainTests`), and `Integer`, which ranges over a closed
  interval wide enough to reach every witness a selector such as `WitnessAt(k)` can pick plus an out-of-range value. A law whose parameters are
  all finite, with a product no larger than `MaxExhaustive`, runs every case.
- Otherwise the law is **sampled**, `Trials` times, each draw taking a witness or a generated
  value with equal chance when both exist. `ValueGenerator` follows the C# conformance runner's
  policy (a `Number` is one of ten special values three times in ten and otherwise uniform in
  [-10, 10]; an `Integer` is uniform in [-8, 8]; a struct is built field by field to a depth
  limit) and goes further where that runner stops: an array of a few elements, a sum case chosen
  at random, a zero-field type as its only value. Every draw comes from one `Random` seeded from
  a stable hash of the law's name, so a run is reproducible and a failing case can be found
  again.
- A parameter typed by an **interface** ranges over every concrete type implementing it, as the
  C# runner's monomorphization does: the witnesses of each implementer plus generated values of
  the implementers that can be generated.
- A law no value can be built for is **skipped** with the reason (a type variable, a function
  parameter, a builder, nesting deeper than the generator's limit with no witness).

The result per law is one of `pass`, `fail` (the first failing witness and the measure), `error`
(a fault, with the Plato call chain), `obligation` (the bodiless function reached), or `skipped`
(the reason). The generator's sampling policy is deliberately the same as the C# runner's, so
that a law that fails on generated input in one engine fails in the other.

## `plato run`

```
Plato.CLI run <folder>... (--laws | --call=<Name>) [--recursive] [--exclude=<file>]
                          [--profile=cpu64|gpu32] [--filter=<text>[,<text>...]] [--laws-from=<file>]
                          [--report=<file>] [--baseline=<file>]
                          [--trials=N] [--seed=N] [--budget=<seconds>]
```

`RunCommand.cs` compiles the roots as one program (top-level files, or every subfolder with
`--recursive`; `*.spec.plato` is always skipped, because the specification syntax is not yet in
the grammar) and runs it in this process. `--laws` prints one line per law and a summary line
with the counts, the number of bodies elaborated, and the compile and law times separately.
`--call=<Name>` invokes a nullary function, or a `_`-receiver one as `Type.Name`, and prints its
value. `--filter` takes a comma-separated list and keeps the laws whose full name
(`Owner.Law_Name`) contains any entry, ignoring case, so an entry names a law, its class, or both;
`--laws-from=<file>` adds the entries of a file, one per line, blank lines and `#` comments
skipped. Both may repeat, and the run covers the union. An entry that selects no law is reported
on stderr, so a misspelt name in a list does not pass silently. `--trials` and `--seed` set the
sampling, and `--budget` stops one law after that many seconds and reports it as an error. The exit code is 1 on a parse or compile failure and when any law fails or errors;
skipped laws and host obligations are listed but do not fail the run.

`--report=<file>` writes a verdict file: one tab-separated line per law, sorted by name, holding
the full name, the verdict (`pass`, `fail`, `error`, `obligation`, `skipped`) and the first
failing witness, the fault or the reason, flattened to one line. Two overloads with one name
carry their parameter types. A committed verdict file records which laws held at a commit, and
two of them diff line by line. `--baseline=<file>` compares the run with a verdict file and
prints every law whose verdict changed (`regressed`, `changed`, `new`, or, in a run without
filters, `removed`); only verdicts are compared, never the witness text. With `--baseline` the
exit code says only whether something regressed: 1 when a law the baseline records as passing
fails, errors, is skipped, becomes an obligation, or (in an unfiltered run) is gone; laws that
already failed in the baseline do not fail the check. The baseline is read before the run, so
`--report` and `--baseline` may name the same file to check and then advance it.
`LawReport` and `LawVerdict` in `LawRunner.cs` hold the format and the comparison.

## Warm law runs

`plato run` compiles the whole program before its first law, every time; on a large program
that compilation is nearly all of a filtered run. `WarmLawRunner` (`src/Plato.Navigation`)
keeps it between runs for a long-lived host, and the `plato_laws` tool of the navigation MCP
server is that host. Its arguments are the CLI's flags (`folders`, `recursive`, `exclude`,
`filter`, `lawsFrom`, `profile`, `trials`, `seed`, `budget`, `report`, `baseline`), and it
chooses files through the same `LawProgram.SourceFiles`, so the two reach the same verdicts.

Each run reads the files and compares their contents with the last run's:

- **Nothing changed**: the compilation and the interpreter are reused, with every body already
  elaborated, so the run costs the laws alone.
- **A file changed**: only that file is parsed again (the runner keeps its own `ParseCache`), and
  the program is compiled once more, because the compiler has no incremental mode. The
  interpreter is rebuilt on the new compilation.

A run can also type-check (`TypeCheckScope`, the tool's `types` argument). `ChangedFiles`, the
tool's default, checks only the functions declared in files whose text changed since the last
check and keeps every other file's last errors, so an edit costs a fraction of a second of
checking; `All` checks every function once per compilation, which also catches a caller in an
unchanged file that the edit broke. Type errors come back with file, line, code and message,
the same location rule `plato_check` uses.

`Plato.Navigation.CLI lawbench` measures the cases on any set of folders: cold, unchanged, and
after three edits to one file (`--types` or `--all-types` adds the type check, `--phases` the
compiler's timed phase log). Point `--touch` at a copy, since the bench rewrites that file and
restores it.

Measured on 2026-10-18 over the STEP library (the three stdlib tiers plus a copy of
`sandbox/step-brep-mesh`, recursive, the three `brep.*` stdlib files excluded, `cpu64`, one law
selected; 250 files, about 14,400 functions), on a machine shared with other sessions:

| Run | Time | Where it goes |
|---|---|---|
| `plato run --laws --filter=<one law>`, cold CLI | about 33 s | 32.5 s compiling, 0.3 s on the law |
| Warm, first run (`lawbench`, 15:55) | 19.8 s | 9 to 10 s parsing, 9 to 12 s compiling |
| Warm, no file changed (`lawbench` and `plato_laws`) | 0.4 s | the law alone |
| Warm, one file changed, writer tables built in the constructor (`lawbench`, 15:55 to 15:59) | 8.6 to 12.6 s | 0.1 s parsing, 7.9 to 11.8 s compiling |
| Warm, one file changed, writer tables built on first use (`lawbench`, 16:00) | 5.7 s | 0.1 s parsing, 5.0 s compiling, 0.6 s on the law |
| `plato_laws` round trip after a one-body edit, with the changed-file type check (18:05) | 4.8 to 6.0 s (six runs: 4.8, 4.9, 4.9, 5.4, 5.5, 6.0) | 4.1 to 5.5 s compiling, 0.2 to 0.3 s type-checking the file, 0.3 to 0.4 s on the law |

Between 16:05 and 17:55 the machine ran ten to twenty-five times slower under outside load, and
readings from then were discarded; one 18:05 round trip of 24 s, whose compile alone took 16.5 s,
was taken during such a spike and is left out of the range above. The compiler's own phase log
(`lawbench --phases`) shows where a recompile went before the writer tables moved: resolution
about 0.7 s, reifying library functions about 3.5 s, and building the code writers'
`LibraryFunctions` and `ConcreteTypes` tables about 5.3 s. The interpreter reads neither table,
so `Compilation` builds them on first use; the generated C# of the foundation tier is
byte-identical either way. A check of every function took 6 to 21 s on this program in these runs, which
is why `ChangedFiles` is the default. What remains of an edit is whole-program resolution and
reification, so getting reliably under 5 s needs an incremental compiler, not more from this
runner.

The STEP design folder compiles with the three stdlib tiers plus the sandbox under `--recursive`,
excluding the three `brep.*.plato` stdlib files the sandbox redeclares; the exact invocation is in
the dated report.

## The public API

Three entry points, on `Interpreter`:

- `Call(string name, params object[] args)` chooses the overload as the checker would from the
  values' types and applies it.
- `Invoke(FunctionDef f, params object[] args)` applies one known function: its body, the host
  intrinsic it names, or the value a constructor, field, cast or sum factory denotes.
- `Evaluate(TirNode node, Environment env)` evaluates one TIR expression against an environment.

`Evaluate` is the operation `SPEC-EXECUTION.md`'s `plato verify` builds on: a bounded quantifier
binds its variable in an `Environment` and evaluates its body; an invariant is an expression over
the fields bound in an environment; a theorem's test line is a law with its own domain. No second
engine is needed for any of them.

## Where the interpreter and the C# backend differ

These are known and deliberate. The differential test the ticket asks for (the stdlib's laws run
both ways, compared law by law under both profiles) is the check that no other divergence has
crept in.

- **Array equality** is structural here and by reference in the generated C#, where a record's
  `Equals` compares an array field with `==` on `IReadOnlyList<T>`. A law that compares two
  separately built arrays passes here and fails there; the sandbox's primer records this as the
  backend's defect, not the interpreter's.
- **Arrays are eager** here and lazy in the C# runtime. The observable difference is cost, and
  the point at which a faulting element function faults.
- **Hash seeds.** The C# backend combines field hashes with `HashCode.Combine`, whose seed is per
  process, so no hash value is comparable across runs in either engine; the interpreter's hash is
  stable within a process and consistent with its equality.
- **String ordering** follows the C# runtime: `string.Compare` with the current culture, which is
  what the generated code calls.

## Limits

What the interpreter does not do yet, found by running the STEP design folder and the stdlib
laws; the dated report has the counts and the exact messages.

- A static callee is applied as resolved, and only an interface *declaration* is re-dispatched.
  When the checker resolved a call on a type-variable-typed expression to a generic *library*
  function whose parameter is an interface (`Add(x: IOriginBased<$D>, delta: $D)` chosen for
  `x.Offset + delta`, whose operands are vectors), the interpreter runs that body with a receiver
  that does not implement the interface, and the next member call on it is reported as a host
  obligation (`IOriginBased.Offset on Vector3D`) that no body could discharge. The fix is to
  re-resolve a static callee whose declared parameters the run-time arguments do not fit, by the
  same tiers the syntactic route uses.
- `Type.Name` in value position, where `Name`'s first parameter is a value rather than `_`
  (`Point2D.Origin` with `Origin(x: IOriginBased<$D>)`), passes the type token as the value and
  faults at the first field read.
- `--exclude` matches a file name anywhere under the roots; there is no way to exclude a folder.
