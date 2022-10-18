using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plato;

public class Generator<T> : IGenerator<T>
{
    public Generator(Func<T, (T, bool)> func, bool hasValue, T current)
        => (NextFunc, HasValue, Current) = (func, hasValue, current);

    public Func<T, (T, bool)> NextFunc { get; }
    public IGenerator<T>? Next
    {
        get
        {
            if (!HasValue) return null;
            var (nextValue, nextHasValue) = NextFunc(Current);
            return new Generator<T>(NextFunc, nextHasValue, nextValue);
        }
    }
    public bool HasValue { get; }
    public T Current { get; }
}

//====================================================
// Mutable Types 
//====================================================

// Builders (and anything with the "Unique" attribute) are mutable classes,
// but there can only be one reference to an instance at a time.
// It can't be captured in a closure, or stored in a field or array, etc. 
// Unlike other types these have setter operations. 
// The Plato code analyzer assures that instances of Unique types respect 
// the uniqueness guarantee. 

[Unique]
public interface IArrayBuilder<T>
{
    int Count { get; }
    T this[int index] { get; set; }
    IArray<T> ToArray();
}

[Unique]
public interface IListBuilder<T> : IArrayBuilder<T>
{
    int Capacity { get; set; }
    new int Count { get; set; }
    IListBuilder<T> Append(ISequence<T> value);
}

