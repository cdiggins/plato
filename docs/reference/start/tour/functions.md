# Functions and member forms

A **function** takes inputs and returns one value. Its body is a single expression after `=>`. Calling it changes nothing: the same inputs give the same result.

Write `Name(a, b)` to call a function. Write `a.Name(b)` for the same call. The two forms mean the same thing, and the dotted form reads left to right when calls are chained.

```plato
library TourFunctions {
    TourDouble(x: Integer): Integer => x * 2;
    TourAdd(a: Integer, b: Integer): Integer => a + b;
    TourApplyTwice(f: Function1<Integer, Integer>, x: Integer): Integer => f(f(x));

    Law_ReferenceTourDouble(): Boolean => TourDouble(21) == 42;
    Law_ReferenceTourMemberForm(): Boolean => 21.TourDouble == TourDouble(21);
    Law_ReferenceTourMemberArgs(): Boolean => 3.TourAdd(4) == TourAdd(3, 4);
    Law_ReferenceTourLambda(): Boolean => TourApplyTwice(n => n + 10, 1) == 21;
}
```

Source: [functions.plato](../../implementation/examples/tour/functions.plato).

`TourDouble(21)` is `42`. `21.TourDouble` is the same call with the first argument moved before the dot. `3.TourAdd(4)` is `TourAdd(3, 4)`. `n => n + 10` is a **lambda**, a function with no name, and `TourApplyTwice` calls the function it receives two times.

Each `Law_Reference...` function states a result, and the law runner checks it, so the results above are verified rather than claimed. Functions live in a `library` block. The names carry a `Tour` prefix so they cannot collide with the standard library.

## Exercise

Write `TourTriple(x: Integer): Integer` that multiplies by three, and call it in member form on `5`.

## Answer

```plato
library TourFunctionsAnswer {
    TourTriple(x: Integer): Integer => x * 3;
    Law_ReferenceTourExerciseTriple(): Boolean => 5.TourTriple == 15;
}
```

Source: [functions-answer.plato](../../implementation/examples/tour/functions-answer.plato). The law passes because `5.TourTriple` equals `15`.

## The rules

The language chapter that owns this idea: [Functions, receivers, and lambdas](../../language/functions.md) and [Reading and writing source](../../language/syntax.md).

Next: [Products: values with named fields](products.md).
