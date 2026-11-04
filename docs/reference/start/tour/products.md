# Products: values with named fields

A **product** is a value that has all of its fields at once. `type TourPoint { X: Integer; Y: Integer; }` declares one. It is the Plato counterpart of a record or a struct.

```plato
type TourPoint { X: Integer; Y: Integer; }

library TourProducts {
    TourMakePoint(x: Integer, y: Integer): TourPoint => (x, y);
    TourSwap(p: TourPoint): TourPoint => (p.Y, p.X);
    TourSum(p: TourPoint): Integer => p.X + p.Y;

    Law_ReferenceTourFields(): Boolean => TourMakePoint(3, 7).Y == 7;
    Law_ReferenceTourSwap(): Boolean => TourMakePoint(3, 7).TourSwap.X == 7;
    Law_ReferenceTourConstructor(): Boolean => TourPoint(3, 7).X == 3;
}
```

Source: [products.plato](../../implementation/examples/tour/products.plato).

`TourPoint(3, 7)` builds a point by giving the fields in order. A function whose result type is `TourPoint` can also return a bare tuple such as `(x, y)`, and the compiler fills the fields by position. `p.X` reads a field, and it is the same member form as a function call.

`TourSwap` does not modify `p`. It builds a new point.

## Exercise

Write `TourFlip` that returns a point with both fields negated. Subtract each field from `0`.

## Answer

```plato
library TourProductsAnswer {
    TourFlip(p: TourPoint): TourPoint => TourPoint(0 - p.X, 0 - p.Y);
    Law_ReferenceTourExerciseFlip(): Boolean => TourPoint(3, 7).TourFlip.X == 0 - 3;
}
```

Source: [products-answer.plato](../../implementation/examples/tour/products-answer.plato). The law checks that flipping `TourPoint(3, 7)` gives an `X` of `0 - 3`.

## The rules

The language chapter that owns this idea: [Products and construction](../../language/products.md).

Next: [Immutability and local variables](immutability.md).
