# Generics: one function for many types

A **generic** type or function works for several types. `TourPair<T>` is a pair whose two fields share a type `T`. In a function signature, `$T` is a type variable that the compiler fills in at each call.

```plato
type TourPair<T> { Left: T; Right: T; }

library TourGenerics {
    TourSwapPair(p: TourPair<$T>): TourPair<$T> => (p.Right, p.Left);
    TourLeftOf(p: TourPair<$T>): $T => p.Left;

    Law_ReferenceTourSwapInteger(): Boolean => TourSwapPair(TourPair(1, 2)).Left == 2;
    Law_ReferenceTourSwapNumber(): Boolean => TourSwapPair(TourPair(1.5, 2.5)).Left == 2.5;
}
```

Source: [generics.plato](../../implementation/examples/tour/generics.plato).

`TourSwapPair` swaps the fields of a pair of anything. The first law uses integers and the second uses numbers, and neither needed a second copy of the function. The fields are `Left` and `Right` because the standard library already uses `First` for a collection function.

## Exercise

Write `TourRightOf` that returns the `Right` field of any pair.

## Answer

```plato
library TourGenericsAnswer {
    TourRightOf(p: TourPair<$T>): $T => p.Right;
    Law_ReferenceTourExerciseRight(): Boolean => TourRightOf(TourPair(1, 2)) == 2;
}
```

Source: [generics-answer.plato](../../implementation/examples/tour/generics-answer.plato). For `TourPair(1, 2)` the result is `2`.

## The rules

The language chapter that owns this idea: [Generics, inference variables, and bounds](../../language/generics.md).

Next: [Sums and match: one of several cases](sums.md).
