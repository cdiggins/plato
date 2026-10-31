# Sums and match: one of several cases

A **sum** holds exactly one of several alternatives. `TourChoice<T>` is either `TourSome` with a value or `TourNone` with nothing. It replaces a null.

`match` takes a sum apart. It must name every case, so adding a case later turns each incomplete `match` into an error instead of a surprise.

```plato
type TourChoice<T> = TourSome(Value: T) | TourNone;

library TourSums {
    TourValueOr(c: TourChoice<$T>, fallback: $T): $T
        => match (c) { TourSome(value) => value; TourNone => fallback; };
    TourFound(x: Integer): TourChoice<Integer> => TourChoice.TourSome(x);
    TourMissing(_: Integer): TourChoice<Integer> => TourChoice.TourNone;

    Law_ReferenceTourSomeCase(): Boolean => TourFound(7).TourValueOr(3) == 7;
    Law_ReferenceTourNoneCase(): Boolean => Integer.TourMissing.TourValueOr(3) == 3;
}
```

Source: [sums.plato](../../implementation/examples/tour/sums.plato).

`TourValueOr` returns the payload of `TourSome`, or the fallback for `TourNone`. `TourFound(7)` is a `TourSome`, so the result is `7`. `TourMissing` is a `TourNone`, so the result is the fallback `3`. The parameter `_` names an input the body does not use.

## Exercise

Write `TourIsSome` that reports whether a choice holds a value.

## Answer

```plato
library TourSumsAnswer {
    TourIsSome(c: TourChoice<$T>): Boolean
        => match (c) { TourSome(_) => true; TourNone => false; };
    Law_ReferenceTourExerciseIsSome(): Boolean
        => TourFound(1).TourIsSome && !Integer.TourMissing.TourIsSome;
}
```

Source: [sums-answer.plato](../../implementation/examples/tour/sums-answer.plato). The law says a `TourSome` gives `true` and a `TourNone` does not.

## The rules

The language chapter that owns this idea: [Sums and matching](../../language/sums.md).

Next: [Numbers: Number and Integer](numbers.md).
