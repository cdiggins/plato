# Plato

Plato is a strongly typed functional programming language designed for high-performance scripting. Syntactically it is a strict subset of C# but has additional semantics and restrictions that facilitate optimization of functional code.  

## How does Plato differ from C#

Plato has more types and operations built into the language specification, particularly around immutable collections. 

* `IEnumerable<T>`
* `IArray<T>`
* `ISet<T>`
* `IList<T>` 
* `IRange<T>`
* `IComparable<T>`
* `ILookup<TKey, TValue>`
* `IGrouping<TKey, TValue>`
* `ITuple<THead, TRest>`
* `INullable<TValue>`

 There are several mutable classes: 
 
 * `IArrayBuilder`
 * `ILookupBuilder`
 * `IStackBuilder` 
 * 
 
## Plato Rules

* Lambdas cannot modify state 
* Function calls can be made in any order, at the discretion of the compiler, if the input maps to the output as expected 
* Functions with side-effects can be applied non-deterministically 
* The compiler can o
* All structs are immutable
* Class fields are readonly by default 
* Methods that modify state have to identified using the `Impure` attribute
* Mutable class fields have to identified using the `Impure` attribute
* Classes with mutable methods or mutable classe fields have to be identified using the `Impure` attribute 
* All structs have precise layout semantics when serialized and deserialized.
* Class and interface references can't be null, unless they derive from nullable 
* Virtual, abstract, private,and protected functions are prohibited

## Platonic Reflection

Plato only supports reflection on expressions for which the values that can be determined at compile-time. In this case their is no run-time cost. The Plato compiler is a whole program optimizer and because of its strict rules can evaluate a much larger set of expressions at compile-time than the C# compiler. 

## Plato Compilation Optimization

The Plato compiler optimizes code with the following assumptions:

* lambdas are primarily used in tight loops
* side effects can 
* para

Some of the plat 

* Partial evaluation 
* Function inlining
* Lambda lifting
* Stream fusion

