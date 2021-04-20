# Plato

April 20th Status: The source code of this project currently only contains utilities for compiling C#. 

Plato is a strongly typed pure functional programming language designed for high-performance scripting. 
Syntactically it is a strict subset of C# but has additional semantics and restrictions that facilitate 
optimization of functional code. 

## Why Does Plato Exist

Plato was developed to address the fact that most scripting languages have deficits in the following areas:

* scalability 
* code maintainence
* performance
* type safety

The design of Plato strives to provide both experienced and inexperienced programmers with a language with a 
familiar and easy to learn syntax, that naturally encourages code that is easily maintained and improved. 

### The Problem of Too Much Flexibility 

Any software worth writing will be maintained, modified, and reused. Many languages are designed 
on the principle of providing a core set of rules and primitives that can be combined in various ways
that may be surprising and novel, and produce curious and unexpected results. 

This has the advantage of allowing users to invent new use cases to show up, but it also tends to make it easy for 
programmers, especially inexperienced ones, to introduce subtle but crippling design flaws into software 
that can be hard to find and undo. 

JavaScript is a good example of this. It is a language which is very small and elegant, and very flexible, enabling many
different methods of using it. However large scale code produced in JavaScript is hard to validate and maintain. 

Good quality code should not only be the domain of experienced and well trained programmers. A programming language can 
guide people down a path by making it hard to write poor quality code, and easy to write good quality code.

### Can New Programmers Learn a Language Based on Math and Functional Programming

I was the architect for the visual programming language MCG (Max Creation Graph) first released as part of Autodesk 3ds Max 2016. 
It was based on the functional programming concepts and immutable data structures. 
It was interesting seeing non-programmers take to it as quickly as they would any other language. 
Surprisingly, people with some previous programming language experience anecdotally seemed to struggle a bit more, in 
that they were already used to think in terms of mutation. 

## How does Plato differ from C#

Plato is a more restricted language than C#, with less syntax, and non-mutable collections. 
There are also more types and operations built into the language specification, particularly around immutable collections.

## Unique Types

Plato allows certain types to be declared as [UniqueTypes](https://en.wikipedia.org/wiki/Uniqueness_type). A value of a unique type can only 
ever have a single reference to it, in other words it can't be shared. Unique types are similar to and often used interchangably 
with linear type.

The syntax for declaring a unique type is as follows: 

```
    [UniqueType]
    public interface IInputStream<T>
    {
        (IInputStream<T>, T) Read();
        bool HasValue { get; }
    }
```

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
 
This is syntactic sugar for:

```
var builder = ListBuilder.Create<Int32>();
foreach (var i in Range(0,10))
    builder = builder.Add(i * 10);
foreach (var x in builder)
    Console = Console.WriteLine(x);
 ```

Further more this is rewritten as:

```
var builder = Range(0,10)
  .Aggregate(ListBuilder.Create<Int32>(), (builder, i) => builder.Add(i * 10));
Console = builder
  .Aggregate(Console, Console.WriteLine);
 ```
 
Which becomes:

```
Console = Range(0,10)
  .Aggregate(ListBuilder.Create<Int32>(), (builder, i) => builder.Add(i * 10));
  .Aggregate(Console, Console.WriteLine);
```
  
## Platonic Reflection

Plato only supports reflection on expressions for which the values that can be determined at compile-time. This means that reflection in Plato has no run-time cost. 
The Plato compiler uses a whole program optimizer and because of its strict rules can evaluate a much larger set of expressions at compile-time than the C# compiler. 

# Plato Optimization

In C# it is considered good practice to not have side effects in lambda functions, particularly when using LINQ. However this is just a guideline, 
programmers are free to do so. The language semantics are such that if a side-effect happens in a LINQ expression, it does so in a predictable manner. This significantly 
limits what an optimizing compiler can do in terms of term rewriting and applying optimizations, because it has to guarantee that those side-effects occur as expected 
regardless of whether a particular optimization is applied or not. 

Plato does not have side effects, and more expressions can be evaluated at compile-time, so the compiler has a wider range of optimization heurisitics it can apply. 

The Plato optimizer assumes a coding style where developers:
* Prefer code that is succinct
* Prefer to not to prematurely optimize code 
* Frequently use lambdas 
* Frequently use interfaces and other abstractions 
* Use many small functions 

## Lambda Subexpression Elimination

One of the most effective optimizations performed by Plato is is pre-computation of sub-expressions in lambdas. This is a technique closely related to 
[Common Subexpression Elimination](https://en.wikipedia.org/wiki/Common_subexpression_elimination).

Consider the following code for computing the variance in C#: 

```
float Sum(IEnumerable<float> values)
   => values.Aggregate(0, (acc, x) => acc + x);

float Count(IEnumerable<float> values)
   => values.Aggregate(0, (acc, x) => acc + y);

float Average(IEnumerable<float> values) 
   => MySum(values) / MyCount(values); 

float Sqr(float x) 
   => x * x;

float MyVariance(IEnumerable<float> values)
   => MySum(values.Select(x => Sqr(x - MyAverage(values))));
```

This function `MyVariance` is very inefficient, it has `O(N^2)` complexity, 
because the `MyAverage` function will be called for each item in the list. 

A programmer would naturally rewrite the function as: 

```
float MyVariance(IEnumerable<float> values)
{
   var avg = MyAverage(values);
   return MySum(values.Select(x => Sqr(x - avg)));
}
```

The problem with placing the onus for this on the programmer is that, the code is more complicated, and the programmer has to know that 
the function `MyAverage` has a significant compute cost in order to do the rewrite. 

This rewrite is performed automatically by the Plato compiler. The compiler does this by:

1. identifying the lambda expression `x => Sqr(x - MyAverage(values))` inside of `MyVariance` (call it `lambda1`)
2. identifying that the sub-expression `MyAverage(values)` has no dependence on arguments to lambda1` 
3. lifting the sub-expression `MyAverage(values)` out of the lambda and assigning it to a variable.

In general expressions the rule is that any expression within a lambda that is not dependent on the lambda arguments, 
is replaced with a variable. 

## Aggressive Inlining

Plato inlining heuristics are very aggressive and performed early to maximize the impact of other optimization passes. 

## Fusion 

The Plato language and compiler leverages the built-in understanding of core semantics of collections, allowing substantial high-level fusion optimizations 
(e.g. combining `Select` or `Where` statements, and so-forth). 
