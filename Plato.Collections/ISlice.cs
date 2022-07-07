namespace Plato
{
    public readonly struct Slice<T> : ISlice<T>
    {
        public Slice(IArray<T> array, int index, int count)
            => (Array, Index, Count) = (array, index, count);
        public IArray<T> Array { get; }
        public int Index { get; }
        public int Count { get; }
        public T this[int input] => Array[input + Index];
        public IIterator<T> Iterator => new ArrayIterator<T>(this);
        T IMap<int, T>.this[int index] => Array[Index + index];
    }
}