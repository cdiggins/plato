# Plato.Collections

Plato.Collections is a library of thread-safe immutable collections for .NET Standard 2.0 inspired by LINQ.

The definition of the interfaces can be found at [Collections.cs](Collections.cs).

## About Plato 

Plato.Collections is written using [Plato](https://github.com/cdiggins/plato), a strict subset of C# that
leverages Roslyn source generators to enforce rules and add extra code. 

Plato enforces that code is pure functional. In other words classes are immutable and thread-safe and functions 
have no side-effects. 

## Design Principles

Plato.Collections follow a simple set of design principles:

1. All collections are immutable, side-effect free, and thread-safe. 
2. Required API for an Interfaces should each be as small as possible
3. Each interface should describe a single well-defined concept 
4. Data types with different algorithmic complexity need different representations 
5. Low-level performance concerns beyond should be primarily the concern of optimization tools  
6. Construction of new instances of interfaces is expressed as extension methods 

## How Plato.Collections Differs from LINQ and System.Collections

Plato redesigned the entire hierarchy of collections, interface, and enumerable types 
from scratch while enforcing immutability and thread-safety.

Most types in the `Plato.Collections` namespace provides the same LINQ operations 
as `IEnumerable`, with the only difference being the types that are returned. 

Some of the `Plato.Collections` interfaces include the following:

```csharp
interface IMap<TDomain, TCoDomain> {}
interface IArray<T> : IMap<int, T>, ISequence<T> {}
interface ISet<T> {}
interface IGenerator<T> {}
interface ISequence<T> {}
interface IContainer<T> : ISet<T> {}
interface IList<T> : ISequence<T> {} 
interface ISorted<T> {}
interface ISortedArray<T> : IArray<T>, ISorted<T> {}
interface ISortedBag<T> : IBag<T>, ISorted<T> {}
interface ISortedContainer<T> : IContainer<T>, ISorted<T> {}
interface ISortedSequence<T> : ISequence<T>, ISortedBag<T> {}
interface ITree<T> : ISequence<T> {}
interface ISortedTree<T> : ITree<T>, ISorted<T> {}
interface IHeap<T> : ITree<T>, ISorted<T> {}
interface IStack<T> {}
interface IQueue<T> {}
interface IDeque<T> {}
interface IPriorityQueue<TKey, TValue> : ISorted<TKey>, IStack<(TKey, TValue)> {}
interface IDictionary<TKey, TValue> : IMap<TKey, TValue>, ISorted<TKey>, ISet<TKey> {}
interface IMultiDictionary<TKey, TValue> : IDictionary<TKey, ISequence<TValue>>  {}
interface IBiDictionary<TKey, TValue> : IDictionary<TKey, TValue> {}
interface ISubSequence<T> : ISequence<T> {}
interface ISlice<T> : IArray<T> {}
```

These interfaces are defined in [Collections.cs](Collections.cs).

## Enumerator Example

In pseudo-code the default .NET libraries an enumerator looks like:

```csharp
interface IEnumerator<T> 
{    
    T Current { get; }
    bool MoveNext(); 
    void Reset();
    void Dispose();
}
```

All methods and properties in an enumerator have potential side-effects, even the `Current` property
can trigger an error in the case that is "invalidated due to changes made in the collection".

Plato offers a side-effect-free alternative called `IGenerator` designed for immutable collections:

```csharp
interface IGenerator<T>
{
    T Current { get; }
    IGenerator<T>? Next { get; }
    bool HasValue { get; }
}
```

Unlike `IEnumerator` the `IGenerator` will never throw exceptions. If `Current` is queried while `HasValue` is 
`false` then a default value will be returned. 

### Replacing IEnumerable with ISequence 

In order to differentiate from `IEnumerable`, Plato introduces a new core interface that many collection 
derive from called `ISequence` which has one property:

```csharp
interface ISequence<T>
{
    IGenerator<T> Generator { get; }
}
```

## IArray vs LINQ on Array

LINQ operations are available on arrays through the C# System libraries. The problem is that once a LINQ operation (like Select`, or `Take`)
is performed on an array the result is an `IEnumerable`, and no longer an array. 

This means that there is no longer any guarantee of the algorithmic complexity of O(1) for element indexing or retrieving the number of 
elements, when it would be trivial to provide those guarantees. 

Plato offers an interface called `IArray` that has a number of LINQ style operations that return `IArray` ensuring the same algorithmic 
properties. 

```csharp
interface IArray<T> 
{
    int Count { get; }
    this[int n] { get; }
}
```

## Similar Work

The `IArray<T>` implementation is based on the [LinqArray](https://github.com/vimaec/LinqArray) library, which in turn
is based on article at CodeProject.com called [LINQ for Immutable Arrays](https://www.codeproject.com/Articles/517728/LINQ-for-Immutable-Arrays). 


## IMap as a generalization of IArray and IDictionary 

Dictionaries and arrays both map from some input type (a domain) to an output type (a codomain). In the case of arrays the domain is integers. 

An `IMap` represents the abstract concept of a mathematical mapping. The difference between an `IMap` and an `IDictionary` is that a dictionary 
is a specialization of an `IMap` that has a finite set of ordered keys and values.

This is illustrated by the interface definition. 

```csharp
interface IDictionary<TKey, TValue> : 
    IMap<TKey, TValue>, ISorted<TKey>, ISet<TKey>
{ }
```

## Sets 

A set is defined by the ability simply to query membership:

```csharp
interface ISet<T>
{
    bool Contains(T item);
}
```

By keeping the interface minimal, it is possible to implement infinite sets, and provide efficient representation of operations 
like set complement.

## Explicit Ordering 

Collections that have been created with a specific ordering, store the ordering function explicitly. This has an impact on algorithmic 
complexity of certain operation. For example an `ISortedSequence` that has the property of faster search `O(Log N)` compared to the 
regular `O(N)` for the `IndexOf` operation, and related derived operations.

```csharp
interface ISorted<T> 
{
    Func<T, T, int> Ordering { get; }
}

interface ISortedBag<T> : IBag<T>, ISorted<T>
{
    ISortedSequence<T> ToSequence();
}
```



