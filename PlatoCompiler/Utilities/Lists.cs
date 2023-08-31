using System.Collections;
using System.Collections.Generic;

namespace Plato.Compiler.Utilities
{
    public interface ILists<T> : IReadOnlyList<IReadOnlyList<T>>
    { }

    public class Lists<T> : ILists<T>
    {
        public List<IReadOnlyList<T>> Data { get; } = new List<IReadOnlyList<T>>();

        public void Add(IReadOnlyList<T> list)
            => Data.Add(list);

        public IEnumerator<IReadOnlyList<T>> GetEnumerator()
            => Data.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => ((IEnumerable)Data).GetEnumerator();

        public int Count => Data.Count;

        public IReadOnlyList<T> this[int index] => Data[index];
    }
}