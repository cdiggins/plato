# Syntax for Constraining Type Parameters 

Musings on the Plato language design. 

## 2024-01-10

While working on the [Plato mathematics library](https://github.com/cdiggins/Plato.Mathematics.NET) 
I encountered an interesting challenge while working on the `Interval` concept which I thought 
I would write about. Specifically the syntax of Plato currently only allows type parameters 
to express a single constraint, and it turns out to be inadequate.  

A concept is kind of like an interface, it defines a set of operations. 
Functions can then be written which apply to all types which implement the interface, 
kind of like extension methods in C#. 

Here is the first naive version of the Interval concept. 

```plato
concept Interval<TValue: Comparable>
{
    Min(x: Self): TValue;
    Max(x: Self): TValue;
}
```

In a concept the keyword `Self`` is an implicit type parameter, representing the type that is passed. 

The idea is that an interval can be defined over numbers, points, even times.
The only requirement is that I can Compare over that value, which is 
described by the `Comparable` concept: 

```plato
concept Comparable
{
    Compare(x: Self, y: Self): Integer;
}
```

Some example of types implementing the `Interval` concept are: 

```plato
type AngleInterval implements Interval<Angle>
{
    Start: Angle;
    End: Angle;
}
type RealInterval implements Interval<Number>
{
    A: Number;
    B: Number;
}
type Line2D implements Interval<Point2D>
{
    A: Point2D;
    B: Point2D;
}
```

The advantage of Plato is that in a library I can define a function like `Contains` which works 
on all types which implement the `Interval` concept. 

```plato
    Contains(x: Interval, value: Comparable): Boolean 
        => x.Min <= value && value <= x.Max;
```


The thing is that if we look at most intervals, they usually have other properties. 
For example usually you can interpolate between the two values. If you have that constraint,
then some additional functions can be defined on `Interval`, which make it even more useful. 
Now I can define functions like the following `Clamp` function in a very generic way. 

```plato
    Clamp(x: Interval, value: Interpolatable): Interpolatable 
        => value.Lerp(x.Min, x.Max, value.Unlerp(x.Min, x.Max).Clamp(0, 1));
```

The function `Lerp` is basically a weighted average, while `Unlerp` computes the weighting towards
given a value and two others. The `Interpolatable` concept is described as follows:

```plato
concept Interpolatable
{
    Lerp(a: Self, b: Self, amount: Number): Self;
    Unlerp(v: Self, a: Self, b: Self): Number;
}
```

The problem now becomes, if an `Interval` requires both constraints how do I combine them? The language syntax as it 
stands currently only allows you to provide one concept as a constraint to a type 
parameter. 

One option I considered was the syntax of `TypeScript` and to start enabling an algebra of types: 

```plato
concept Interval<TValue: Interpolatable & Comparable>
{
    Min(x: Self): TValue;
    Max(x: Self): TValue;
}
```

As powerful as this is, it implies being able to use other operators for types in different places and could quickly get out of control. 
I think introducing more symbols would make Plato too unapproachable, for what it strive to accomplish.  

If I nudge the Plato syntax a bit closer to C# it could instead look like this:

```plato
concept Interval<TValue>
    where TValue: Interpolatable, Comparable
{
    Min(x: Self): TValue;
    Max(x: Self): TValue;
}
```

This gives us more freedom, and doesn't force us to merge concepts or create new concepts artificially. 

## Conclusion

I have pretty much decided now to go the route of expressing constraints like we do in C#. 
Making a syntax change this big now requires some significant refactoring, but I think leaving out the possibility 
of multiple constraints for type parameters would be too limiting in the long run. 
