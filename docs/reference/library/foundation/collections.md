# Collections, indexing and builders

A valid element index is a position in an immutable array. A slice, a count, and a geometric interval are three different ranges.

## Immutable data and explicit construction

`Array<T>` is an ordered immutable collection. A valid element index satisfies `0 <= i < Count`. Immutability does not promise eager evaluation, constant-time access, caching, or the absence of repeated computation. Check the runtime when these matter. Plato's ordinary rank-specific records and immutable generic arrays do not imply [NumPy](https://numpy.org/doc/stable/reference/routines.array-manipulation.html) broadcasting, strided views, negative indexing, or vectorized execution.

`List<T>` and `Buffer<T>` are unique builders: a growable append buffer and a fixed-size indexed buffer. Rebind each mutation, for example `xs = xs.Add(value)`, and finish with `xs.Freeze()`. Freezing consumes the builder and returns an immutable array. Use after consumption is invalid. Do not capture builders in lambdas, put them in fields, or put them inside other containers. Static enforcement and runtime enforcement are separate. The [primitive declarations](../../api/foundation/primitives.md) have the effect table, and [locals and builders](../../language/state-and-builders.md) has the language rule.

## Three distinct ranges

`Slice(from, exclusiveTo)` uses endpoints. `Slice(2,5)` contains indices 2, 3, and 4. `SubArray(start,count)` uses a count. A geometric interval usually includes both ends. Do not use that convention to infer array slicing. Establish `0 <= from <= exclusiveTo <= Count` before slicing.

An `IIndex` wrapper names a position in a particular domain. A face index is different from a vertex index. Negative values encode absence under the [typed-index convention](../../../../stdlib/CONVENTIONS.md#typed-indices---1-means-none). Test validity before indexing. A nonnegative value still needs an upper-bound check against its owner. Counts and CSR offsets are plain integers rather than typed element references.

## Flattened grids and ragged rows

For `Array3D`, column varies fastest. If `C` is the column count and `R` is the row count:

```text
flat(column,row,layer) = (layer*R + row)*C + column.
```

The element-store length must match the product of the extents. A jagged collection stores row boundaries in `Offsets` and payloads in `Values`. Row `i` is `Values.Slice(Offsets[i], Offsets[i+1])`. Boundaries are nondecreasing, and the last is `Values.Count`. Empty data has offsets `[0]`. Empty rows repeat a boundary.

## Sorting and generic containers

`SortedIndices(xs, lessOrEqual)` returns source indices in sorted order and preserves the source order of ties. `Sort` uses that permutation. The comparer must be total and transitive. A NaN-containing ordinary numeric comparison may violate that requirement. The reference body uses bottom-up merge sort. Interfaces such as `IMap`, `ISet`, `IQueue`, and `IStack` describe capabilities. Their declaration does not promise a concrete hash table or queue implementation.

Bounds checks must also respect evaluation order. The sorting source uses nested conditionals because Boolean `&&` is an ordinary eager operation. The expression `i < n && xs[i] > 0` must not be assumed to protect the array access. Use a conditional branch to guard a partial operation. See [evaluation](../../language/evaluation.md).

## Where to look

- [Primitive collections](../../api/foundation/primitives.md), including the builder effect table, and [multidimensional layouts](../../api/foundation/primitives.types.md)
- [Collection interfaces](../../api/foundation/collections.concepts.md) and [derived operations](../../api/foundation/collections.library.md)
- [Primitive array bodies](../../api/foundation/primitives.library.md)
- [Jagged types](../../api/foundation/collections-jagged.types.md)
- [Sorting](../../api/foundation/sorting.library.md)
