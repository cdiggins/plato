# Immutability and local variables

Values never change after they are made. To get a different value, build a new one. `TourWithHigh` returns a new range with another upper end and leaves the old range alone.

Inside a function whose body uses braces, `var` makes a local variable that an algorithm can update in a loop. The variable lives only inside that call, so callers cannot observe the update.

```plato
type TourRange { Low: Integer; High: Integer; }

library TourImmutability {
    TourWithHigh(r: TourRange, high: Integer): TourRange => (r.Low, high);
    TourSpan(r: TourRange): Integer => r.High - r.Low;
    TourSumTo(n: Integer): Integer {
        var total = 0;
        var i = 1;
        while (i <= n) {
            total = total + i;
            i = i + 1;
        }
        return total;
    }

    Law_ReferenceTourNewValue(): Boolean => TourRange(1, 5).TourWithHigh(9).High == 9;
    Law_ReferenceTourOriginalKept(): Boolean => TourRange(1, 5).High == 5;
    Law_ReferenceTourSumTo(): Boolean => TourSumTo(4) == 10;
}
```

Source: [immutability.plato](../../implementation/examples/tour/immutability.plato).

`TourRange(1, 5).TourWithHigh(9)` has a `High` of `9`, while `TourRange(1, 5)` still has `5`. `TourSumTo(4)` runs the loop for `1 + 2 + 3 + 4` and returns `10`.

A function with a braced body ends in `return`. A function written with `=>` is a single expression.

## Exercise

Write `TourProductTo(n)` that computes `1 * 2 * ... * n` with a loop.

## Answer

```plato
library TourImmutabilityAnswer {
    TourProductTo(n: Integer): Integer {
        var product = 1;
        var i = 1;
        while (i <= n) {
            product = product * i;
            i = i + 1;
        }
        return product;
    }
    Law_ReferenceTourExerciseProduct(): Boolean => TourProductTo(4) == 24;
}
```

Source: [immutability-answer.plato](../../implementation/examples/tour/immutability-answer.plato). `TourProductTo(4)` is `24`.

## The rules

The language chapter that owns this idea: [Locals, loops, and builders](../../language/state-and-builders.md).

Next: [Interfaces: what a type can do](interfaces.md).
