# Quick tour

A walk through the language in one sitting. Each page teaches one idea with a working
example, shows what it produces, and ends with a small exercise and its answer. Every
example is a real file in the [tour examples](../implementation/examples/tour/functions.plato)
that the law runner checks, and each page shows the whole file.

For tools and install steps, see [Getting started](getting-started.md).

## The pages, in order

1. [Functions and member forms](tour/functions.md): `Name(a, b)` and `a.Name(b)`, lambdas.
2. [Products: values with named fields](tour/products.md): `type`, construction, field access.
3. [Immutability and local variables](tour/immutability.md): new values instead of changes, `var` in a loop.
4. [Interfaces: what a type can do](tour/interfaces.md): `interface`, `implements`, `Self`.
5. [Generics: one function for many types](tour/generics.md): `Pair<T>` and `$T`.
6. [Sums and match: one of several cases](tour/sums.md): alternatives and exhaustive `match`.
7. [Numbers: Number and Integer](tour/numbers.md): the two scalar kinds and conversion.

## Where to go next

- [Language reference](../language/README.md): the chapter list.
- [Language cheat sheet](../language/cheatsheet.md): shapes at a glance.
- [Glossary](../language/glossary.md): words used throughout this site.
- [Library guide](../library/README.md): geometry, units, and domains.
