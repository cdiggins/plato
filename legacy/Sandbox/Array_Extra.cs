using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PlatoTypeInference
{
    internal class Array_Extra
    {
        /// <summary>
        /// Returns true if the two lists are the same length, and the elements are the same.
        /// </summary>
        bool SequenceEquals(Array self, Array other) where Any : IEquatable<Any>
            => self == other || (self.Count == other.Count && self.Zip(other, (x, y) => x?.Equals(y) ?? y == null).All(x => x));

        /// <summary>
        /// Returns the number of unique instances of elements in the array.
        /// </summary>
        int CountUnique(Array xs)
            => xs.ToEnumerable().Distinct().Count();

        /// <summary>
        /// Applies a function (like "+") to each element in the series to create an effect similar to partial sums.
        /// The first value in the array will be zero.
        /// </summary>
        Array PostAccumulate(Array self, Func2 f, Any init = default)
        {
            if (init == null) throw new ArgumentNullException(nameof(init));
            var n = self.Count;
            // TODO: this is tricky! How do I know the actual type to allocate? 
            var r = new Any[n + 1];
            var prev = r[0] = init;
            for (var i = 0; i < n; ++i)
            {
                prev = r[i + 1] = f(prev, self[i]);
            }
            return Create(r);
        }

        /// <summary>
        /// Counts all elements in an array that are equal to a value
        /// </summary>
        int CountWhere(Array self, Any val) where Any : IEquatable<Any>
            => self.CountWhere(x => x.Equals(val));

        /// <summary>
        /// Returns the minimum element in the list
        /// </summary>
        Any Min(Array self) where Any : IComparable<Any>
        {
            if (self.Count == 0) throw new ArgumentOutOfRangeException(nameof(self));
            return self.Aggregate(self[0], (a, b) => a.CompareTo(b) < 0 ? a : b);
        }

        /// <summary>
        /// Returns the maximum element in the list
        /// </summary>
        Any Max(Array self) where Any : IComparable<Any>
        {
            if (self.Count == 0) throw new ArgumentOutOfRangeException(nameof(self));
            return self.Aggregate(self[0], (a, b) => a.CompareTo(b) > 0 ? a : b);
        }

        /// <summary>
        /// Given indices of sub-arrays groups, this will convert it to arrays of indices (e.g. [0, 2] with a group size of 3 becomes [0, 1, 2, 6, 7, 8])
        /// </summary>
        Array GroupIndicesToIndices(Array indices, int groupSize)
            => groupSize == 1
                ? indices : (indices.Count * groupSize).Select(i => indices[i / groupSize] * groupSize + i % groupSize);

        /// <summary>
        /// Return the array separated into a series of groups (similar to DictionaryOfLists)
        /// based on keys created by the given keySelector
        /// </summary>
        IEnumerable<IGrouping<TKey, TSource>> GroupBy<TSource, TKey>(Array self, Func1 keySelector)
            => self.ToEnumerable().GroupBy(keySelector);

        /// <summary>
        /// Return the array separated into a series of groups (similar to DictionaryOfLists)
        /// based on keys created by the given keySelector and elements chosen by the element selector
        /// </summary>
        IEnumerable<IGrouping<TKey, TElem>> GroupBy<TSource, TKey, TElem>(Array self, Func1 keySelector, Func1 elementSelector)
            => self.ToEnumerable().GroupBy(keySelector, elementSelector);

        /// <summary>
        /// Uses the provided indices to select groups of contiguous elements from the array.
        /// This is equivalent to self.SubArrays(groupSize).SelectByIndex(indices).SelectMany();
        /// </summary>
        Array SelectGroupsByIndex(Array self, int groupSize, Array indices)
            => self.SelectByIndex(indices.GroupIndicesToIndices(groupSize));

        /// <summary>
        /// Returns the index of the first element matching the given item.
        /// </summary>
        int IndexOf(Array self, Any item) where Any : IEquatable<Any>
            => self.IndexOf(x => x.Equals(item));

        /// <summary>
        /// Returns the index of the first element matching the given item.
        /// </summary>
        int IndexOf(Array self, Func1 predicate)
        {
            for (var i = 0; i < self.Count; ++i)
            {
                if (predicate(self[i]))
                    return i;
            }

            return -1;
        }

        /// <summary>
        /// Returns the index of the last element matching the given item.
        /// </summary>
        int LastIndexOf(Array self, Any item) where Any : IEquatable<Any>
        {
            var n = self.Reverse().IndexOf(item);
            return n < 0 ? n : self.Count - 1 - n;
        }
    }
}
