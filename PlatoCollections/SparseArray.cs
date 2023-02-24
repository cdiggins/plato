using System;

namespace Plato
{

    public class SparseArray<T> : IArray<T>
    {
        public class SparseArrayIterator : IIterator<T>
        {
            public SparseArray<T> Array { get; }
            public int Index { get; }
            public int InternalIndex { get; }

            public SparseArrayIterator(SparseArray<T> array, int index = 0, int internalIndex = 0)
            {
                Array = array;
                Index = index;
                InternalIndex = internalIndex;
            }

            public IIterator<T> Next => new SparseArrayIterator(Array, Index + 1, BetweenNodes ? InternalIndex : InternalIndex + 1);
            public bool HasValue => InternalIndex < Array.Count;
            public bool BetweenNodes => Array.Values[InternalIndex].index > Index;
            public T Value => BetweenNodes ? Array.DefaultValue : Array.Values[InternalIndex].value;
        }

        public T DefaultValue;
        public IArray<(int index, T value)> Values { get; }

        public int Count 
            => Values.Count == 0 ? 0 : Values[Values.Count-1].index;

        public IIterator<T> Iterator 
            => throw new NotImplementedException();

        public T this[int input] 
            => ElementAtInternalIndex(FindInternalIndex(input));

        public SparseArray(T defaultValue, IArray<(int index, T value)> indexedValues)
        {
            DefaultValue = defaultValue;
            Values = indexedValues.OrderBy(ab => ab.index);
        }

        private int MidPointGuess(int index, int begin, int end)
        {
            // The closer that this algorithm gets to the correct index 
            // The better the algorithmic complexity. 
            // Basically the idea is that the range of values might be quite close. 
            // var rangeStart = Values[begin].index;
            // var rangeEnd = Values[end].index;
            // var percent = index - rangeStart / (rangeEnd - rangeStart)
            // var guess = begin + percent * end - begin;
            // The thing is that we might not even need this.
            // The naive algorithmic complexity is O(Log(M)) where M is the number of items stored, 
            // not the size of the array. So for extremely sparse arrays, this is extremely fast. 
            return begin + (end - begin) / 2;
        }

        private T ElementAtInternalIndex(int internalIndex)        
            => internalIndex < 0 || internalIndex > Values.Count ? DefaultValue : Values[internalIndex].value;

        public int FindInternalIndex(int index)
        {
            if (Count == 0) return -1;
            return FindInternalIndex(index, 0, Values.Count / 2 - 1, Values.Count - 1);
        }

        public int FindInternalIndex(int index, int begin, int mid, int end)
        {
            var b = Values[mid].index;
            if (index == b)
            {
                return mid;
            }
            else if (index < b)
            {
                if (begin >= mid) return -1;
                var newMid = MidPointGuess(index, begin, mid - 1);
                return FindInternalIndex(index, begin, newMid, mid - 1);
            }
            else
            {
                if (end <= mid) return -1;
                var newMid = MidPointGuess(index, mid + 1, end);
                return FindInternalIndex(index, mid + 1, newMid, end);
            }
        }
    }
}
