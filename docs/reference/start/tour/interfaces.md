# Interfaces: what a type can do

An **interface** lists operations a type promises to have. `Self` stands for the type that implements the interface. A type opts in with `implements`.

```plato
interface TourScalable {
    TourScale(a: Self, k: Number): Self;
}

type TourVec implements TourScalable { X: Number; Y: Number; }

library TourInterfaces {
    TourScale(a: TourVec, k: Number): TourVec => TourVec(a.X * k, a.Y * k);
    TourDoubled(a: TourScalable): TourScalable => a.TourScale(2.0);

    Law_ReferenceTourDoubled(): Boolean => TourDoubled(TourVec(1.5, 4.0)).X == 3.0;
    Law_ReferenceTourDoubledY(): Boolean => TourVec(1.5, 4.0).TourDoubled.Y == 8.0;
}
```

Source: [interfaces.plato](../../implementation/examples/tour/interfaces.plato).

`TourScalable` promises a `TourScale` operation. `TourVec implements TourScalable`, and the library supplies `TourScale(a: TourVec, ...)`. `TourDoubled` accepts any `TourScalable` and returns the same kind of value: given a `TourVec` you get a `TourVec`, so `.X` and `.Y` still work.

There is no inheritance of fields or code. An interface only describes operations, and the library supplies them.

## Exercise

Write `TourHalved` that scales by `0.5`, and check the `Y` of `TourVec(1.5, 4.0)`.

## Answer

```plato
library TourInterfacesAnswer {
    TourHalved(a: TourScalable): TourScalable => a.TourScale(0.5);
    Law_ReferenceTourExerciseHalved(): Boolean => TourVec(1.5, 4.0).TourHalved.Y == 2.0;
}
```

Source: [interfaces-answer.plato](../../implementation/examples/tour/interfaces-answer.plato). The `Y` of the halved vector is `2.0`.

## The rules

The language chapter that owns this idea: [Interfaces, capabilities, and Self](../../language/interfaces.md).

Next: [Generics: one function for many types](generics.md).
