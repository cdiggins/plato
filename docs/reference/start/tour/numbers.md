# Numbers: Number and Integer

`Integer` is a whole number and `Number` is a floating-point number. The literal `7` is an `Integer` and the literal `7.0` is a `Number`. They do not mix silently: convert with `ToNumber`.

```plato
library TourNumbers {
    TourHalf(x: Number): Number => x / 2.0;
    TourWhole(n: Integer): Number => n.ToNumber;

    Law_ReferenceTourLiteralKinds(): Boolean => 7.TourWhole == 7.0;
    Law_ReferenceTourHalf(): Boolean => TourHalf(5.0) == 2.5;
    Law_ReferenceTourAbs(): Boolean => (0.0 - 3.5).Abs == 3.5;
}
```

Source: [numbers.plato](../../implementation/examples/tour/numbers.plato).

`TourHalf(5.0)` is `2.5`. `7.TourWhole` converts to `7.0`. `.Abs` is the absolute value, a standard-library function used in member form; see the [primitives file page](../../api/foundation/primitives.md). Floating-point results are approximate in general, so the examples use values that are exact.

## Exercise

Write `TourMean(a, b)` for two `Number` values.

## Answer

```plato
library TourNumbersAnswer {
    TourMean(a: Number, b: Number): Number => (a + b) / 2.0;
    Law_ReferenceTourExerciseMean(): Boolean => TourMean(2.0, 5.0) == 3.5;
}
```

Source: [numbers-answer.plato](../../implementation/examples/tour/numbers-answer.plato). `TourMean(2.0, 5.0)` is `3.5`.

## The rules

The language chapter that owns this idea: [Number profiles and scientific arithmetic](../../language/numbers.md).

That is the tour. Continue with the [language reference](../../language/README.md), the [cheat sheet](../../language/cheatsheet.md) or the [glossary](../../language/glossary.md).
