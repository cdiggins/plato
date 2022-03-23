namespace Plato;

public readonly record struct Slice<T>(IArray<T> Array, int Index, int Count) : ISlice<T>
{
    public T this[int input] => Array[input + Index];
    public IIterator<T> Iterator => new ArrayIterator<T>(this);
    T IMap<int, T>.this[int index] => Array[Index + index];
}
