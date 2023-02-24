using System;

namespace Plato
{
    [Mutable]
    public class ArrayBuilder<T> : IMutableList<T>
    {
        T[] Values { get; set; } = Array.Empty<T>();
        public int Count { get; private set; }
        public bool IsFrozen { get; private set; }
        public IArray<T> ToIArray()
        {
            IsFrozen = true;
            // TODO: the "Array" . ToIArray() is not safe.
            // We might end up changing it, in which case this would need to be rewritten. 
            return Values.ToIArray().Take(Count);
        }
        const int SizeIncreaseFactor = 2;
        const int InitialAllocationSize = 16;
        public void Add(T x)
        {
            if (IsFrozen)
            {
                throw new InvalidOperationException();
            }
            if (Values.Length == Count)
            {
                var newSize = Values.Length > 0
                    ? Values.Length * SizeIncreaseFactor
                    : InitialAllocationSize;
                var tmp = new T[newSize];
                Array.Copy(Values, tmp, Values.Length);
                Values = tmp;
            }
            Values[++Count] = x;
        }
        public T this[int index]
        {
            get 
            { 
                return Values[index]; 
            }
            set 
            {
                if (IsFrozen) throw new InvalidOperationException();
                Values[index] = value; 
            }
        }
    }
}
