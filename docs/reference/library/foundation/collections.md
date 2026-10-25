# Collections, indexing and builders

Read [primitive collections](../../api/foundation/primitives.md), [multidimensional layouts](../../api/foundation/primitives.types.md), [collection interfaces](../../api/foundation/collections.concepts.md), [derived operations](../../api/foundation/collections.library.md), [primitive array bodies](../../api/foundation/primitives.library.md), [jagged types](../../api/foundation/collections-jagged.types.md) and [sorting](../../api/foundation/sorting.library.md).

## Immutable data and explicit construction

`Array<T>` is an ordered immutable collection. A valid element index satisfies `0 <= i < Count`. Immutability does not promise eager evaluation, constant-time access, caching, or the absence of repeated computation. Check the runtime when these matter.

`List<T>` and `Buffer<T>` are unique builders: a growable append buffer and a fixed-size indexed buffer. Rebind each mutation, for example `xs = xs.Add(value)`, and finish with `xs.Freeze()`. Freezing consumes the builder and returns an immutable array; use after consumption is invalid. Do not capture builders in lambdas, put them in fields, or put them inside other containers. Static and runtime enforcement are separate concerns; consult [primitive declarations](../../api/foundation/primitives.md) for the effect table and the language's ownership section for the implementation.

## Three distinct ranges

`Slice(from, exclusiveTo)` uses endpoints: `Slice(2,5)` contains indices 2, 3 and 4. `SubArray(start,count)` uses a count. A geometric interval usually includes both ends; it must not be used to infer array slicing behavior. Establish `0 <= from <= exclusiveTo <= Count` before slicing.

An `IIndex` wrapper names a position in a particular domain: a face index is different from a vertex index. Negative values encode absence under the [typed-index convention](../../../../stdlib/CONVENTIONS.md#typed-indices---1-means-none); test validity before indexing. A nonnegative value still needs an upper-bound check against its owner. Counts and CSR offsets are plain integers rather than typed element references.

## Flattened grids and ragged rows

For `Array3D`, column varies fastest. If `C` is column count and `R` row count:

```text
flat(column,row,layer) = (layer*R + row)*C + column.
```

The element-store length must match the product of extents. A jagged collection stores row boundaries in `Offsets` and payloads in `Values`: row `i` is `Values.Slice(Offsets[i], Offsets[i+1])`. Boundaries are nondecreasing; the last is `Values.Count`; empty data has offsets `[0]`. Empty rows repeat a boundary.

## Sorting and generic containers

`SortedIndices(xs, lessOrEqual)` returns source indices in sorted order, preserving the source order of ties. `Sort` uses that permutation. The comparer must be total and transitive; a NaN-containing ordinary numeric comparison may violate that requirement. The reference body uses bottom-up merge sort. Interfaces such as `IMap`, `ISet`, `IQueue` and `IStack` describe capabilities; their declaration does not promise a concrete hash table or queue implementation.

Bounds checks must also respect evaluation order. The sorting source explicitly uses nested conditionals because Boolean `&&` is an ordinary eager operation: `i < n && xs[i] > 0` must not be assumed to protect the array access. Use a conditional branch to guard a partial operation; see [evaluation](../../language/evaluation.md).

## Related work

[NumPy's array manipulation reference](https://numpy.org/doc/stable/reference/routines.array-manipulation.html) is useful for comparing reshaping and indexing. Plato's ordinary rank-specific records and immutable generic arrays do not imply NumPy broadcasting, strided views, negative indexing, or vectorized execution.
