# Plato

Plato is a strongly typed pure functional programming language designed for high-performance scripting. Syntactically it is a strict subset of C# but has additional semantics and restrictions that facilitate significant optimization of functional code. 

## Why Does Plato Exist

Plato was developed to address the fact that most scripting languages have deficits in the following areas:

* scalability 
* code maintainence
* performance
* type safety

The design goal of Plato is to provide both experienced and inexperienced programmers with a language that has a familiar syntax, and is approachable for both experienced and new developers, and that naturally encourages code that is easily maintained and improved. 

### The Problem of Too Much Flexibility 

Any software worth writing will be maintained, modified, and reused. Many scripting languages are designed on a principle of providing extensive flexibility to the programmer, allowing them to stumble into code that appears to be working through trial and error. This ultimately does not do anyone any favors. It makes it easy for programmers to introduce subtle but crippling design flaws into software that make it hard. 

Good quality code should not only be the domain of experienced and well trained programmers. A programming language can guide people down a path by making it hard to write poor quality code, and easy to write good quality code.

This is a fundamentally different approach to programming language design to most. 

### Can New Programmers Learn a Language Based on Math and Functional Programming

I was part of the team that developed a visual programming language MCG (Max Creation Graph) for Autodesk 3ds Max, a popular 3D and Animation software package. It was based on the functional programming concepts and immutable data structures. It was interesting seeing non-programmers take to it as quickly as they would any other language. Surprisingly, people with some previous programming language experience anecdotally seemed to struggle a bit more, in that they were already used to think in terms 

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

## Mutation Exists as an Illusion

 There are several classes that provide a mutable interface. Some examples:
 
 * `IArrayBuilder`
 * `ILookupBuilder`
 * `IStackBuilder` 
 * `IQueueBuilder`
 * `IListBuilder` 
 
 For example:
 
 ```
var builder = ListBuilder.Create<Int32>();
foreach (var i in Range(0,10))
    builder.Add(i * 10);
foreach (var x in builder)
    Console.WriteLine(x);
```
 
This is equivalent to and is rewritten as:

```
var builder = ListBuilder.Create<Int32>();
foreach (var i in Range(0,10))
    builder = builder.Add(i * 10);
foreach (var x in builder)
    Console.WriteLine(x);
 ```
 
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
* Classes cannot be downcast (i.e. cast from a base class to a derived class) unless it can be determined safely at compile-time 

## Platonic Reflection

Plato only supports reflection on expressions for which the values that can be determined at compile-time. In this case their is no run-time cost. The Plato compiler is a whole program optimizer and because of its strict rules can evaluate a much larger set of expressions at compile-time than the C# compiler. 

## Plato Compilation Optimization

C# maps to IL code in very straightforward and predictable ways. The C# optimizer is very conservative, and does not analyze side effects, and risk any code changes that could change the behavior of code with regards to those side effects. 

The Plato compiler optimizes code with the following assumptions:

* lambdas are primarily used in tight loops
* foreach statements can be rewritten without using iterators 
* the ordering of side effects
* the built-in operations do not map 

Some of the plat 

* Partial evaluation 
* Function inlining
* Lambda lifting
* Stream fusion

