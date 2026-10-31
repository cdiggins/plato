# Writer optimization

These passes rewrite typed intermediate representation (typed IR) before a backend renders source. `CSharpWriter.RunOptimizerPasses` schedules the C# pipeline; the [pipeline reference](../../../docs/csharp-optimizer-pipeline.md) defines pass order, flags, and runtime contracts.

`TirInliner` substitutes resolved callee bodies through the `ITirInlineHost` contract, with refusals recorded by `InlineReport`. `TirComponentUnroller` uses the operation patterns in `ComponentUnroller` to replace fixed-component combinators. `TirArrayMaterializer` handles materialization boundaries. `TirFusion` and `TirLiteralUnroller` simplify compatible chains and literal inputs; `TirLoopLowerer` creates markers rendered by `TirCSharpBodyWriter.Loops.cs`. `TirRewrite` supplies shared tree rebuilding and substitution.

These files retain the `Ara3D.Geometry.CSharpWriter` namespace because other backends consume the shared inliner. Emission syntax belongs in the body writers, while changes to pass semantics require the existing optimizer tests under [PlatoTests](../../../tests/PlatoTests/README.md).

## In-place array updates (`--in-place`, TKT-1132)

`TirInPlaceUpdates` (decisions in `TirUpdateFlow`, marker `TirInPlaceSet`) is the last pass, after `TirLoopLowerer`. Plato arrays are values: `xs.SetAt(i, v)` (`Primitives.SetAt` in the foundation) means a new array, and the emitted C# copies all of `xs`. When nothing can observe the old array afterwards, the pass prints `Intrinsics.SetAtInPlace(xs, i, v)` instead: one element is written into `xs` itself and the same array comes back. The source and the interpreter are untouched, so a law that passes in both engines checks every proof. The helper writes only when the list is a `T[]` and copies any other list, so a proof decides whether a copy is saved and never whether the result is right. An index outside the array changes nothing, as in the Plato body.

### The rule

A `SetAt` call is written in place when all of these hold, and is a copy otherwise.

1. **The callee is the foundation's.** Identified by its declaring library (`Primitives`) and signature, not by the name; the call is not inside a lambda (a loop callback included) or a node the pass does not know.
2. **The receiver is new, made in this body.** Either an expression that builds a new array (an array literal; the temporary of a lowered array loop referenced once; a call to `MapRange`, `Map`, `Append`, `Prepend`, `Concatenate`, `Repeat`, or another `SetAt`), or a local variable every value of which is new: declared once, by a binding whose value is new, and assigned only new values. A parameter belongs to the caller. An array read from a record field, returned by any other call, or chosen by a conditional is not known to be new.
3. **Nothing else holds it.** The variable is not named inside a lambda other than a loop callback (a closure can outlive the statement). Each of its other uses is a borrow (`Count`, `At`, or the source of a lowered loop: they read and keep nothing), the receiver of another `SetAt`, or an escape (bound to another name, passed to any other call, put in a constructor or array literal, returned, assigned elsewhere). An escape must come strictly after the statement that holds the update and must share no loop with it unless the loop also contains the declaration; otherwise the holder would see the write.
4. **The old value has no later use.** In the threaded form, a statement `xs = xs.SetAt(...)`, the variable holds the new version afterwards, so reads of `xs` anywhere are reads of the new array. In any other form (`var ys = xs.SetAt(...)`, a return, an argument) every other use of `xs` must finish before the update runs, and the update must lie in no loop that does not also contain the declaration, because the next pass of such a loop would read what the first wrote.

Statements and the branches of an `if` are taken in the order they are written; a use in the other branch counts as after. That only turns a proof into a copy.

### What it leaves alone

An array that arrives from a caller or a call, or sits in a record, is copied: knowing it is unshared needs ownership across calls, the second criterion of TKT-1132. `Append` and `Prepend` build a longer array and are not updates; an append threaded through a loop needs a presized `List` first (TKT-1131). Updates inside lambdas are not rewritten.

### Tests

`tests/PlatoTests/Optimization/InPlaceUpdateTests.cs` reads the tree and the printed C# of each case in `InPlaceFixtures/in-place.plato`: six kept in place (threaded, chained, handed on, two arrays alternating, per-iteration, a literal), eight copied, one for each clause. `tests/Plato.Interpreter.Tests/InPlaceProgramTests.cs` compiles the same program with the flag off and on and runs its laws in the interpreter and in both: the verdicts must be equal and every law must pass, and the compiled code of exactly the in-place cases must change.
