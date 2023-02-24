namespace Plato
{
    [Mutable]
    public interface IMutableList<T> : IMutableArray<T>
    {
        void Add(T x);
    }

    // Note: this explicitly does not inherits from IArray
    // we don't want to run the chance that a mutable array is mistaken for a non-mutable array
    // It might make sense to prevent mutable types from inheriting non-mutable types

    [Mutable]
    public interface IMutableArray<T>
    {
        int Count { get; }
        T this[int index] { get; set; }
        IArray<T> ToIArray();
    }
}