using System.Collections.Generic;

namespace Plato
{
    public class DebuggerArray<T>
    {
        public readonly int Count;
        public readonly IReadOnlyList<T> Data;

        public DebuggerArray(IArray<T> array)
            => (Count, Data) = (array.Count, array.ToArray());
    }

}