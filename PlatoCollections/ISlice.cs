namespace Plato;

public partial interface ISlice<T>
{
    ISlice<T> Slice(int from, int count)
        => new Slice<T>(Array, Index + from, count);
}

public partial record Slice<T>(IArray<T> Array, int Index, int Count)
    : ISlice<T>
{
    public T this[int input] => Array[input + Index];
    public IGenerator<T> Generator => new ArrayGenerator<T>(this);
}
