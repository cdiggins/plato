# Plato.Interpreter

An interpreter for Plato programs, for people who write or verify Plato libraries and want
to run a function or a law without generating C# and building it. It evaluates the
compiler's own resolved, typed intermediate representation (the Typed IR, or TIR, that the
C# writer renders), in the compiler's process, on values it builds itself.

Written 2026-10-17 for ticket TKT-317 of the bim-open-toolkit repository. Experimental:
one day old, with a unit test suite and a run over the forward standard library behind it.
`docs/interpreter.md` is the reference document; `docs/reports/` holds the dated
measurements.

## The problem it solves

Every execution of Plato code used to go through a backend: generate C#, build it, run it.
That loop takes tens of seconds for one tier of the standard library and fails outright
where the generated C# does not compile, which on 2026-10-17 included the geometry tier
and the STEP design folder. The practical consequence was that algorithm bodies were
written in C# first and ported to Plato later, and that the design's two hundred laws had
never been executed.

The interpreter removes the backend from the loop. An edit to a body is followed by one
front-end pass (parse, resolve, type-check) and the laws run immediately. A function the
host must still supply (a bodiless declaration) is reported as a named obligation, not
compiled into a stub that throws.

## What it does not do

- It is not a fast runtime. Values are boxed, arrays are materialized eagerly, and every
  call goes through dynamic dispatch. Use it to check that code is right, not to mesh a
  model.
- It is not a second definition of the language. Where it and the C# backend disagree, the
  differential test in `tests/Plato.Interpreter.Tests` says so, and the disagreement is a
  defect in one of them or a documented divergence (see "Trade-offs").
- It does not parse or type-check on its own. It consumes a completed `Compilation` and
  checks and elaborates each function lazily when it is first reached.
- It does not run the `.spec.plato` contract files of the STEP design. Those need the
  syntax SPEC-EXECUTION.md proposes; the interpreter is the engine that proposal relies on.

## A session

```text
> dotnet src/Plato.CLI/bin/Release/net8.0/Plato.CLI.dll run stdlib/foundation stdlib/geometry stdlib/graphics stdlib/tests --laws --filter=Law_At
compiled 163 files, 9987 functions, profile cpu64 (8.9s)
pass       FoundationLaws.Law_AtStartIsStart  (25 cases)
pass       FoundationLaws.Law_AtEndIsEnd  (25 cases)
pass       FoundationLaws.Law_AtHalfIsCenter  (25 cases)

3 laws: 3 pass; 10 bodies elaborated; laws 0.2s, total 9.1s
```

A failing law prints its first failing witness, one value per parameter, and the value of a
`Measure_<law>` function when the library declares one with the same parameters.
A law with a `Premise_<law>` companion (same parameters, returning `Boolean`) counts the cases
its premise held on, and is reported `vacuous`, a failure, when it held on none.

## How to use it

Prerequisites: the .NET 8 SDK and a clone of this repository with the `parakeet` submodule
filled (`git submodule update --init --recursive`).

Build the command line and run laws over a program:

```bash
dotnet build src/Plato.CLI/Plato.CLI.csproj -c Release
```

```bash
dotnet src/Plato.CLI/bin/Release/net8.0/Plato.CLI.dll run <folder>... --laws [--recursive] [--exclude=<file name>] [--profile=cpu64|gpu32] [--filter=<text>] [--trials=N] [--seed=N] [--call=<Name>]
```

Each folder is read top-level only unless `--recursive` is given; `*.spec.plato` files are
always skipped; the folders compile as one program. `--laws` evaluates every `Law_*`
function. `--call=Name` or `--call=Type.Name` evaluates one function and prints its value.
The exit code is 1 on a parse or compile failure and when a law fails or faults; skipped
laws and host obligations are listed but do not fail the run.

From C#, reference `src/Plato.Interpreter/Plato.Interpreter.csproj`:

```csharp
var interp = new Interpreter(compilation, NumberProfile.Cpu64);
var area = interp.Call("Area", circle);                 // overload chosen from the values
var value = interp.Evaluate(tirNode, environment);      // any expression, any bindings
var results = new LawRunner(interp).RunAll();           // every Law_* over its witnesses
```

To verify the install, run the unit tests:

```bash
dotnet test tests/Plato.Interpreter.Tests/Plato.Interpreter.Tests.csproj --filter FullyQualifiedName~UnitTests
```

## How laws get their values

A law is a Boolean function named `Law_*`. For each parameter the runner collects values
from three sources: `Witness_X(_: T): T` functions of the same program (the STEP design's
convention), the finite domain of a type with few values (Boolean, an enumeration-style
sum, a type with no fields), and seeded pseudorandom generation with the same policy as the
C# conformance runner (a Number is one of ten special values three times in ten and
otherwise uniform in [-10, 10]; an Integer is small; a record is built field by field). A
law whose parameters all have small finite domains is exhausted; otherwise it is sampled.
A parameter typed by an interface ranges over every concrete type that implements it. A law
for which no value can be built is skipped with the reason.

## Trade-offs

- **Generic bodies run unspecialized.** The C# backend stamps a copy of each generic
  function per concrete type. The interpreter runs the one generic body and dispatches an
  interface member on the receiver's run-time type, mirroring the monomorphizer's rule. This
  keeps the interpreter small and its cost proportional to what a law reaches, at the price
  of dynamic dispatch on every such call.
- **Number is a double under every profile.** Under gpu32 each result is rounded to binary32
  and the transcendental functions go through `MathF`, so the stored values are the floats
  the C# gpu32 build holds. Agreement with C# means the same Boolean verdict, not always the
  same last bit of a transcendental.
- **Arrays compare structurally.** The generated C# compares array fields by reference,
  which is a known defect listed in SPEC-EXECUTION.md. The interpreter chose the meaning the
  language intends; the differential test allows this divergence explicitly.
- **Arrays are eager.** The C# runtime's `MapRange` is lazy. Eagerness is simpler and costs
  memory only at sizes the law runner does not reach.
- **Expensive calls are remembered.** Plato is pure, so `CallMemo.cs` answers a repeated call
  of an expensive bodied function from a table instead of evaluating it again: a law that
  meshes its model three times in one expression, a constant written as a loop, a depth
  recursion over the same references. A function joins the table once its mean cost, without
  elaboration, reaches `CallMemo.ThresholdMicroseconds`; keys compare Numbers bit for bit and
  values with their run-time types, and builders are never keys or remembered results. The
  price is memory, bounded per function by `CallMemo.MaxEntriesPerFunction`, and an evaluation
  whose cost no longer shows how often the library repeats itself; set `Interpreter.Memo` to
  null to evaluate every call, as the C# backend does.
- **Laziness per function, not per program.** Checking and elaborating a body on first use
  means a law over a small part of a library pays only for what it calls, and means the
  interpreter never reports a type error in a function nothing reaches.

## Tested and untested

Tested on 2026-10-17, commit f256b068 and the three commits after it:

- 34 unit tests over a fixture compiled with `stdlib/foundation`: products, tuples returned
  as structs, sums and nested `match`, arrays and the five host array primitives, closures
  over `var` locals, block bodies with `while` and `if`, both builders, Integer and Number
  mixing, interface dispatch through a generic body, `_`-receiver statics, `default`,
  recursion, the host-obligation report, and both Number profiles.
- The forward standard library's law packet (`stdlib/tests`) under cpu64, with the laws
  that fail traced to laws unguarded against generated inputs rather than to the
  interpreter (`tests/Plato.Interpreter.Tests/StdlibLawsTests.cs` lists them).

Untested or in progress on 2026-10-17: the differential comparison against the C# backend
under both profiles (being built in `tests/Plato.Interpreter.Tests/Differential*.cs`); the
STEP design folder end to end; deep recursion limits; performance beyond the law runner's
sizes.

## Beyond verification: what the same machinery enables

An evaluator over the typed IR is the first of several tools that share one shape: a walk
over a resolved, typed, pure program with a per-node interpretation. None of the following
exists yet; each is listed with what it would reuse and what it would add.

- **A verifier.** SPEC-EXECUTION.md's `plato verify` discharges laws, contracts and
  invariants by the strongest engine that can. The interpreter is its evaluation engine:
  `Evaluate(node, environment)` is what a bounded quantifier or an `ensures` clause needs,
  and exhausting a finite domain is a proof, not a sample.
- **Compile-time evaluation.** A closed term (a constant witness, a literal construction
  site, a `requires` on literal arguments) has a value the compiler can compute while
  checking, and every backend can then emit the value instead of the expression.
- **Partial evaluation and inlining.** Evaluate what is known, leave the rest as residual
  TIR. Plato is pure, so a call with some constant arguments specializes safely; the C#
  writer's inliner (`TirInliner`) does the syntactic half of this already and could consume
  values instead of only expressions.
- **Abstract interpretation.** Replace the value domain. Intervals give range analysis
  (an index proven inside `[0, Count)`, a divisor proven nonzero); a sign or parity domain
  gives cheaper facts; a units domain checks that a tolerance in metres never meets one in
  radians. The dispatch and the walk stay; only `Values.cs` and the intrinsic table change.
- **Symbolic execution.** Let a Number be a term rather than a double, and a `match` or a
  conditional fork a path with a constraint. The paths of a law, with their constraints,
  are the input an SMT solver needs, and a path whose constraint is unsatisfiable is dead
  code the linter can report.
- **Test generation and shrinking.** The law runner samples; a runner that keeps the trace
  of which branches each case took can steer generation toward uncovered branches, and a
  failing witness can be shrunk by re-evaluating on smaller candidates.
- **Mutation testing.** Perturb one TIR node (flip a comparison, drop an arm) and rerun the
  laws; a mutant every law survives is a law that tests nothing.
- **Differential testing of backends.** The interpreter is a reference each writer can be
  compared against, in both directions: a backend that disagrees has a bug or a documented
  divergence, and a law that passes everywhere is evidence across languages.
- **Profiling and cost models.** Counting calls, allocations and array sizes per function
  during evaluation gives the asymptotic shape of a library function on real inputs before
  any backend exists for the target.
- **A notebook or REPL.** `Call` and `Evaluate` are enough for an interactive session over a
  library, and the toolkit's notebook could show a Plato value, a table of witnesses, or a
  mesh built by a Plato function without a build step.
- **A second type checker.** Running a body on the default values of its parameter types
  and checking the result's run-time type against the declared return type catches what a
  permissive static check lets through (the `Self`-in-return-position gap VERIFICATION.md
  notes).
- **Documentation examples.** Every example in a doc comment could be evaluated by a gate
  so that documentation never shows an output the code no longer produces.

## Related work

Interpreting a compiler's own typed intermediate representation is the design of GHC's
interpreter over Core and of Rust's constant evaluator over MIR; both run the same IR the
code generator consumes so that the two cannot drift. Property-based testing of Boolean
laws with generated and shrunk inputs is QuickCheck's model, and the `Law_` convention here
is the C# conformance runner's, which predates the interpreter. Discharging obligations by
evaluation, solving and proof in one tool is what Dafny and Why3 do; SPEC-EXECUTION.md
adopts that shape and names the interpreter as its first engine. This project differs in
scope, not in kind: one language, one IR, one process, and a law runner whose witnesses come
from the program itself.

## Organization

| Path | Purpose |
|---|---|
| `Interpreter.cs` | Evaluation of each TIR node kind; running a body; the public `Call`, `Invoke`, `Evaluate` |
| `Dispatch.cs` | Run-time overload resolution, interface re-dispatch, syntactic calls, conversions, `default`, construction |
| `Values.cs` | The value model: records, sums, arrays, builders, functions, type tokens; structural equality; formatting |
| `CallMemo.cs` | The memo of expensive calls and the stricter equality its keys use |
| `CallProfile.cs` | Calls, inclusive and self time per bodied function, collected when `Interpreter.Profiler` is set |
| `Numbers.cs` | The arithmetic a `NumberProfile` gives Number and Integer |
| `HostIntrinsics.cs` | The host half of `intrinsics.library.plato`, keyed by declared signature |
| `Environment.cs` | Bindings in scope during evaluation; the snapshot a lambda captures |
| `LawRunner.cs` | `Law_*` discovery, value supply, exhaustion or sampling, results; `ILawBackend`, the seam `src/Plato.Laws` plugs the compiled program into |
| `ValueGenerator.cs` | Seeded pseudorandom values and finite domains |
| `Exceptions.cs` | Evaluation faults with the Plato call chain; host obligations |
| `../Plato.CLI/RunCommand.cs` | The `plato run` command |
| `../../tests/Plato.Interpreter.Tests/` | Unit tests, the stdlib law run, the C# differential |

## Who it is for

For the authors of the forward standard library and of the STEP design, who want a law
result seconds after an edit; for the verification work SPEC-EXECUTION.md plans; and for
anyone comparing a backend against a reference. Not yet for anyone who needs speed, a
notebook, or the contract syntax the design folder's `.spec.plato` files use.

Report problems in this repository's tracker (`tracker/`, `python tools/track.py new`).
License: see `LICENSE.txt` at the repository root.
