# Affine Builder Types

Phase 6 adds a narrow, runtime-checked form of affine builder type to Plato:

```plato
unique type List<T> { }
unique type Buffer<T> { }
```

The `unique` modifier is declaration-site only. A use of `List<T>` or
`Buffer<T>` is not annotated; the type itself carries the affine convention.
For this first phase only the intrinsic builder names `List` and `Buffer` may
be declared `unique`. The compiler rejects the modifier on every other type.

## Runtime Model

`List<T>` is backed by handwritten `PlatoList<T>` in `Plato.Intrinsics`.
It is a growable append builder with:

- `Count` and `At` observe the builder.
- `Add`, `AddRange`, and `Set` mutate in place and return the builder.
- `Freeze()` consumes the builder and returns `IArray<T>` without copying.

`Buffer<T>` is backed by handwritten `PlatoBuffer<T>`. It allocates a fixed
number of slots, writes by index, then freezes:

- `Count` and `At` observe the builder.
- `Set` mutates in place and returns the builder.
- `Freeze()` consumes the builder and returns `IArray<T>` without copying.

Both runtime builders carry a frozen flag. Any use after `Freeze()` throws
`InvalidOperationException`. This gives the safety backstop now, before the
future type-checker can enforce affine occurrence counts statically.

## Source Conventions

Call sites should rebind or chain mutators:

```plato
new List<Integer>().Add(1).Add(2).Freeze()
```

For generic algorithms, use an existing array value to create an empty builder
whose element type is known:

```plato
xs.Reduce(xs.EmptyList(), (acc, x) => predicate(x) ? acc.Add(x) : acc).Freeze()
```

Write `Freeze()` with parentheses. In classic extension-method output, no-arg
library functions that move out of structs are methods, and the explicit call
syntax keeps this API shape clear.

## Compiler Rules In This Phase

Implemented now:

- Parser and AST recognize `unique type`.
- Symbols carry `TypeDef.IsUnique`.
- Only `List` and `Buffer` may be unique.
- C# generation maps `List<T>` to `PlatoList<T>` and `Buffer<T>` to
  `PlatoBuffer<T>`.
- Unique types are not emitted as generated structs.
- LINT006 reports fields whose direct type is unique.
- LINT007 reports fields, parameters, or return values that put a unique type
  inside another generic type.

Deferred to the future type-checker:

- Occurrence counting along control-flow paths.
- Rejecting use after consume statically.
- Rejecting builder capture in lambdas.
- Stronger effect checking from the compiler's `UniqueTypes` table.

## Validation

The conformance-only file `plato-test-src/unique.algorithms.plato` contains
the validation slice. It keeps production output additive while exercising:

- append/freeze with `List<T>`;
- generic `Filter`;
- convex quad triangulation as the ear-clipping proof slice;
- closed-triangle extrusion side indices;
- fixed-size indexed writes with `Buffer<T>`;
- `AddRange` plus `Set`.
