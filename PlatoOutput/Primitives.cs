

public partial class Array1<T> : Array<T>
{
    private T[] _data;
    public Integer Count => _data.Length;
    public T At(Integer n) => _data[n];
    public static implicit operator T[](Array1<T> self) => self._data;
    public static implicit operator Array1<T>(T[] self) => new Array1<T>(self);
    public Array1(T[] data) => _data = data;
}
