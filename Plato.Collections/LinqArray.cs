// MIT License - Copyright 2019 (C) VIMaec, LLC.
// MIT License - Copyright (C) Ara 3D, Inc.
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.
//
// LinqArray.cs
// A library for working with pure functional arrays, using LINQ style extension functions.
// Based on code that originally appeared in https://www.codeproject.com/Articles/140138/Immutable-Array-for-NET

// TODO: remove the dependencies on IEnumerable etc. 

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Plato
{

    /// <summary>
    /// Extension functions for working on any object implementing IArray. This overrides
    /// many of the Linq functions providing better performance.
    /// </summary>
    public static class LinqArray
    {
        /// <summary>
        /// Helper function for creating an IArray from the arguments.
        /// </summary>
        public static IArray<T> Create<T>(params T[] self)
            => self.ToIArray();

        /// <summary>
        /// A helper function to enable IArray to support IEnumerable
        /// </summary>
        public static IEnumerable<T> ToEnumerable<T>(this IArray<T> self)
            => Enumerable.Range(0, self.Count).Select(self.ElementAt);

        public static IArray<T> ForEach<T>(this IArray<T> xs, Action<T> f)
        {
            for (var i = 0; i < xs.Count; ++i)
                f(xs[i]);
            return xs;
        }

        /// <summary>
        /// Creates an IArray with the given number of items,
        /// and uses the function to return items.
        /// </summary>
        public static IArray<T> Select<T>(this int count, Func<int, T> f)
            => new FunctionalArray<T>(count, f);

        /// <summary>
        /// Converts any implementation of IList (e.g. Array/List) to an IArray.
        /// </summary>
        public static IArray<T> ToIArray<T>(this T[] self)
            => new ArrayAdapter<T>(self);

        /// <summary>
        /// Creates an IArray by repeating each item in the source a number of times.
        /// </summary>
        public static IArray<T> RepeatElements<T>(this IArray<T> self, int count)
            => Select(count, i => self[i / count]);

        /// <summary>
        /// Creates an IArray starting with a seed value, and applying a function
        /// to each individual member of the array. This is eagerly evaluated.
        /// </summary>
        public static IArray<T> Generate<T>(this T init, int count, Func<T, T> f)
        {
            var r = new T[count];
            for (var i = 0; i < count; ++i)
            {
                r[i] = init;
                init = f(init);
            }
            return r.ToIArray();
        }

        /// <summary>
        /// Returns the first item in the array.
        /// </summary>
        public static T? First<T>(this IArray<T> self, T? @default = default)
            => self.IsEmpty() ? @default : self[0];

        /// <summary>
        /// Returns the last item in the array
        /// </summary>
        public static T? Last<T>(this IArray<T> self, T? @default = default)
            => self.IsEmpty() ? @default : self[self.Count - 1];

        /// <summary>
        /// Returns true if and only if the argument is a valid index into the array.
        /// </summary>
        public static bool InRange<T>(this IArray<T> self, int n)
            => n >= 0 && n < self.Count;

        /// <summary>
        /// A mnemonic for "Any()" that returns false if the count is greater than zero
        /// </summary>
        public static bool IsEmpty<T>(this IArray<T> self)
            => !self.Any();

        /// <summary>
        /// Returns true if there are any elements in the array.
        /// </summary>
        public static bool Any<T>(this IArray<T> self)
            => self.Count != 0;

        /// <summary>
        /// Converts the IArray into a system array.
        /// </summary>
        public static T[] ToArray<T>(this IArray<T> self)
            => self.CopyTo(new T[self.Count]);

        /// <summary>
        /// Copies the IArray into a system array.
        /// </summary>
        public static T[] CopyTo<T>(this IArray<T> self, T[] result, int offset = 0)
        {
            for (var i = 0; i < self.Count; i++)
                result[offset + i] = self[i];
            return result;
        }

        /// <summary>
        /// Converts the IArray into a system List.
        /// </summary>
        public static List<T> ToList<T>(this IArray<T> self)
            => self.ToEnumerable().ToList();

        /// <summary>
        /// Converts the array into a function that returns values from an integer, returning a default value if out of range.
        /// </summary>
        public static Func<int, T?> ToFunction<T>(this IArray<T> self, T? def = default)
            => i => self.InRange(i) ? self[i] : def;

        /// <summary>
        /// Converts the array into a predicate (a function that returns true or false) based on the truth of the given value.
        /// </summary>
        public static Func<int, bool> ToPredicate(this IArray<bool> self)
            => self.ToFunction();

        /// <summary>
        /// Adds all elements of the array to the target collection.
        /// </summary>
        public static U AddTo<T, U>(this IArray<T> self, U other) where U : ICollection<T>
        {
            self.ForEach(other.Add);
            return other;
        }

        /// <summary>
        /// Returns an array generated by applying a function to each element.
        /// </summary>
        public static IArray<U> Select<T, U>(this IArray<T> self, Func<T, U> f)
            => Select(self.Count, i => f(self[i]));

        /// <summary>
        /// Returns an array generated by applying a function to each element.
        /// </summary>
        public static IArray<U> Select<T, U>(this IArray<T> self, Func<T, int, U> f)
            => Select(self.Count, i => f(self[i], i));

        /// <summary>
        /// Returns an array generated by applying a function to each element.
        /// </summary>
        public static IArray<U> SelectIndices<T, U>(this IArray<T> self, Func<int, U> f)
            => self.Count.Select(f);

        /// <summary>
        /// Converts an array of array into a flattened array. Each array is assumed to be of size n.
        /// </summary>
        public static IArray<T> Flatten<T>(this IArray<IArray<T>> self, int n)
            => Select(self.Count * n, i => self[i / n][i % n]);

        /// <summary>
        /// Converts an array of array into a flattened array.
        /// </summary>
        public static IArray<T> Flatten<T>(this IArray<IArray<T>> self)
        {
            var counts = self.Select(x => x.Count).PostAccumulate((x, y) => x + y);
            var r = new T[counts.Last()];
            var i = 0;
            foreach (var xs in self.ToEnumerable())
                xs.CopyTo(r, counts[i++]);
            return r.ToIArray();
        }

        /// <summary>
        /// Returns an array of tuple where each element of the initial array is paired with its index.
        /// </summary>
        public static IArray<(T value, int index)> ZipWithIndex<T>(this IArray<T> self)
            => self.Select((v, i) => (v, i));

        /// <summary>
        /// Returns an array from an array of arrays, where the number of sub-elements is the same for reach array and is known.
        /// </summary>
        public static IArray<T> SelectMany<T>(this IArray<IArray<T>> self, int count)
            => Select(self.Count, i => self[i / count][i % count]);

        /// <summary>
        /// Returns an array given a function that generates an IArray from each member. Eager evaluation.
        /// </summary>
        public static IArray<U> SelectMany<T, U>(this IArray<T> self, Func<T, IArray<U>> func)
        {
            var count = self.Sum(x => func(x).Count);
            var xs = new U[count];
            var offset = 0;
            for (var i = 0; i < self.Count; ++i)
            {
                var sub = func(self[i]);
                sub.CopyTo(xs, offset);
                offset += sub.Count;
            }
            return xs.ToIArray();
        }

        /// <summary>
        /// Returns an array given a function that generates an IArray from each member. Eager evaluation.
        /// </summary>
        public static IArray<U> SelectMany<T, U>(this IArray<T> self, Func<T, int, IArray<U>> func)
        {
            var count = self.Aggregate(0, (acc, x, index) => acc + func(x, index).Count);
            var xs = new U[count];
            var offset = 0;
            for (var i = 0; i < self.Count; ++i)
            {
                var sub = func(self[i], i);
                sub.CopyTo(xs, offset);
                offset += sub.Count;
            }
            return xs.ToIArray();
        }

        /// <summary>
        /// Returns an array given a function that generates a tuple from each member. Eager evaluation.
        /// </summary>
        public static IArray<U> SelectMany<T, U>(this IArray<T> self, Func<T, Tuple<U, U>> func)
        {
            var r = new U[self.Count * 2];
            for (var i = 0; i < self.Count; ++i)
            {
                var tmp = func(self[i]);
                r[i * 2] = tmp.Item1;
                r[i * 2 + 1] = tmp.Item2;
            }

            return r.ToIArray();
        }

        /// <summary>
        /// Returns an array given a function that generates a tuple from each member. Eager evaluation.
        /// </summary>
        public static IArray<U> SelectMany<T, U>(this IArray<T> self, Func<T, Tuple<U, U, U>> func)
        {
            var r = new U[self.Count * 3];
            for (var i = 0; i < self.Count; ++i)
            {
                var tmp = func(self[i]);
                r[i * 3] = tmp.Item1;
                r[i * 3 + 1] = tmp.Item2;
                r[i * 3 + 2] = tmp.Item3;
            }
            return r.ToIArray();
        }

        /// <summary>
        /// Returns an array given a function that generates a tuple from each member. Eager evaluation.
        /// </summary>
        public static IArray<U> SelectMany<T, U>(this IArray<T> self, Func<T, Tuple<U, U, U, U>> func)
        {
            var r = new U[self.Count * 4];
            for (var i = 0; i < self.Count; ++i)
            {
                var tmp = func(self[i]);
                r[i * 4] = tmp.Item1;
                r[i * 4 + 1] = tmp.Item2;
                r[i * 4 + 2] = tmp.Item3;
                r[i * 4 + 3] = tmp.Item3;
            }
            return r.ToIArray();
        }

        /// <summary>
        /// Returns an array generated by applying a function to corresponding pairs of elements in both arrays.
        /// </summary>
        public static IArray<V> Zip<T, U, V>(this IArray<T> self, IArray<U> other, Func<T, U, V> f)
            => Select(Math.Min(self.Count, other.Count), i => f(self[i], other[i]));

        /// <summary>
        /// Returns an array generated by applying a function to corresponding pairs of elements in both arrays.
        /// </summary>
        public static IArray<V> Zip<T, U, V>(this IArray<T> self, IArray<U> other, Func<T, U, int, V> f)
            => Select(Math.Min(self.Count, other.Count), i => f(self[i], other[i], i));

        /// <summary>
        /// Returns an array generated by applying a function to corresponding pairs of elements in both arrays.
        /// </summary>
        public static IArray<W> Zip<T, U, V, W>(this IArray<T> self, IArray<U> other, IArray<V> other2, Func<T, U, V, W> f)
            => Select(Math.Min(Math.Min(self.Count, other.Count), other2.Count), i => f(self[i], other[i], other2[i]));

        /// <summary>
        /// Returns an array generated by applying a function to corresponding pairs of elements in both arrays.
        /// </summary>
        public static IArray<W> Zip<T, U, V, W>(this IArray<T> self, IArray<U> other, IArray<V> other2, Func<T, U, V, int, W> f)
            => Select(Math.Min(Math.Min(self.Count, other.Count), other2.Count), i => f(self[i], other[i], other2[i], i));

        /// <summary>
        /// Returns an array generated by applying a function to corresponding pairs of elements in both arrays.
        /// </summary>
        public static IArray<X> Zip<T, U, V, W, X>(this IArray<T> self, IArray<U> other, IArray<V> other2, IArray<W> other3, Func<T, U, V, W, X> f)
            => Select(Math.Min(Math.Min(self.Count, other.Count), Math.Min(other2.Count, other3.Count)), i => f(self[i], other[i], other2[i], other3[i]));

        /// <summary>
        /// Returns an array generated by applying a function to corresponding pairs of elements in both arrays.
        /// </summary>
        public static IArray<X> Zip<T, U, V, W, X>(this IArray<T> self, IArray<U> other, IArray<V> other2, IArray<W> other3, Func<T, U, V, W, int, X> f)
            => Select(Math.Min(Math.Min(self.Count, other.Count), Math.Min(other2.Count, other3.Count)), i => f(self[i], other[i], other2[i], other3[i], i));

        /// <summary>
        /// Applies a function to each element in the list paired with the next one.
        /// Used to implement adjacent differences for example.
        /// </summary>
        public static IArray<U> ZipEachWithNext<T, U>(this IArray<T> self, Func<T, T, U> f)
            => self.Zip(self.Skip(), f);

        /// <summary>
        /// Returns an IEnumerable containing only elements of the array for which the function returns true on the index.
        /// An IArray is not created automatically because it is an expensive operation that is potentially unneeded.
        /// </summary>
        public static IEnumerable<T> WhereIndices<T>(this IArray<T> self, Func<int, bool> f)
            => self.Where((x, i) => f(i));

        /// <summary>
        /// Returns an IEnumerable containing only elements of the array for which the corresponding mask is true.
        /// An IArray is not created automatically because it is an expensive operation that is potentially unneeded.
        /// </summary>
        public static IEnumerable<T> Where<T>(this IArray<T> self, IArray<bool> mask)
            => self.WhereIndices(mask.ToPredicate());

        /// <summary>
        /// Returns an IEnumerable containing only elements of the array for which the corresponding predicate is true.
        /// </summary>
        public static IEnumerable<T> Where<T>(this IArray<T> self, Func<T, bool> predicate)
            => self.ToEnumerable().Where(predicate);

        /// <summary>
        /// Returns an IEnumerable containing only elements of the array for which the corresponding predicate is true.
        /// </summary>
        public static IEnumerable<T> Where<T>(this IArray<T> self, Func<T, int, bool> predicate)
            => self.ToEnumerable().Where(predicate);

        /// <summary>
        /// Returns an IEnumerable containing only indices of the array for which the function satisfies a specific predicate.
        /// An IArray is not created automatically because it is an expensive operation that is potentially unneeded.
        /// </summary>
        public static IEnumerable<int> IndicesWhere<T>(this IArray<T> self, Func<T, bool> f)
            => self.Indices().Where(i => f(self[i]));

        /// <summary>
        /// Returns an IEnumerable containing only indices of the array for which the function satisfies a specific predicate.
        /// An IArray is not created automatically because it is an expensive operation that is potentially unneeded.
        /// </summary>
        public static IEnumerable<int> IndicesWhere<T>(this IArray<T> self, Func<T, int, bool> f)
            => self.IndicesWhere(i => f(self[i], i));

        /// <summary>
        /// Returns an IEnumerable containing only indices of the array for which the function satisfies a specific predicate.
        /// An IArray is not created automatically because it is an expensive operation that is potentially unneeded.
        /// </summary>
        public static IEnumerable<int> IndicesWhere<T>(this IArray<T> self, Func<int, bool> f)
            => self.Indices().Where(i => f(i));

        /// <summary>
        /// Returns an IEnumerable containing only indices of the array for which booleans in the mask are true.
        /// An IArray is not created automatically because it is an expensive operation that is potentially unneeded.
        /// </summary>
        public static IEnumerable<int> IndicesWhere<T>(this IArray<T> self, IArray<bool> mask)
            => self.IndicesWhere(mask.ToPredicate());

        /// <summary>
        /// Shortcut for ToEnumerable.Aggregate()
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static U Aggregate<T, U>(this IArray<T> self, U init, Func<U, T, U> func)
        {
            for (var i = 0; i < self.Count; ++i)
                init = func(init, self[i]);
            return init;
        }

        /// <summary>
        /// Shortcut for ToEnumerable.Aggregate()
        /// </summary>
        public static U? Aggregate<T, U>(this IArray<T> self, Func<U?, T, U> func)
            => Aggregate(self, default, func);

        /// <summary>
        /// Shortcut for ToEnumerable.Aggregate()
        /// </summary>
        public static U Aggregate<T, U>(this IArray<T> self, U init, Func<U, T, int, U> func)
        {
            for (var i = 0; i < self.Count; ++i)
                init = func(init, self[i], i);
            return init;
        }

        /// <summary>
        /// Shortcut for ToEnumerable.Aggregate()
        /// </summary>
        public static U? Aggregate<T, U>(this IArray<T> self, Func<U?, T, int, U> func)
            => Aggregate(self, default, func);

        /// <summary>
        /// Returns a new array containing the elements in the range of from to to.
        /// </summary>
        public static IArray<T> Slice<T>(this IArray<T> self, int from, int to)
            => Select(to - from, i => self[i + from]);

        /// <summary>
        /// Returns an array of SubArrays of size "size"
        /// the last items that cannot fill an arrat if size "size" will be ignored
        /// </summary>
        public static IArray<IArray<T>> SubArraysFixed<T>(this IArray<T> self, int size)
            => (self.Count / size).Select(i => self.SubArray(i, size));


        /// Returns an array of SubArrays of size "size" plus extras
        /// The extra array is of size count % size if present
        public static IArray<IArray<T>> SubArrays<T>(this IArray<T> self, int size)
            => self.Count % size == 0
                ? self.SubArraysFixed(size)
                : self.SubArraysFixed(size).Append(self.TakeLast(self.Count % size));

        /// <summary>
        /// Returns n elements of the list starting from a given index.
        /// </summary>
        public static IArray<T> SubArray<T>(this IArray<T> self, int from, int count)
            => self.Slice(from, count + from);

        /// <summary>
        /// Returns elements of the array between from and skipping every stride element.
        /// </summary>
        public static IArray<T> Slice<T>(this IArray<T> self, int from, int to, int stride)
            => Select(to - from / stride, i => self[i * stride + from]);

        /// <summary>
        /// Returns a new array containing the elements by taking every nth item.
        /// </summary>
        public static IArray<T> Stride<T>(this IArray<T> self, int n)
            => Select(self.Count / n, i => self[i * n % self.Count]);

        /// <summary>
        /// Returns a new array containing just the first n items.
        /// </summary>
        public static IArray<T> Take<T>(this IArray<T> self, int n)
            => self.Slice(0, n);

        /// <summary>
        /// Returns a new array containing just at most n items.
        /// </summary>
        public static IArray<T> TakeAtMost<T>(this IArray<T> self, int n)
            => self.Count > n ? self.Slice(0, n) : self;

        /// <summary>
        /// Returns a new array containing the elements after the first n elements.
        /// </summary>
        public static IArray<T> Skip<T>(this IArray<T> self, int n = 1)
            => self.Slice(n, self.Count);

        /// <summary>
        /// Returns a new array containing the last n elements.
        /// </summary>
        public static IArray<T> TakeLast<T>(this IArray<T> self, int n = 1)
            => self.Skip(self.Count - n);

        /// <summary>
        /// Returns a new array containing all elements excluding the last n elements.
        /// </summary>
        public static IArray<T> DropLast<T>(this IArray<T> self, int n = 1)
            => self.Count > n ? self.Take(self.Count - n) : self.Empty();

        /// <summary>
        /// Returns a new array by remapping indices
        /// </summary>
        public static IArray<T> MapIndices<T>(this IArray<T> self, Func<int, int> f)
            => self.Count.Select(i => self[f(i)]);

        /// <summary>
        /// Returns a new array that reverses the order of elements
        /// </summary>
        public static IArray<T> Reverse<T>(this IArray<T> self)
            => self.MapIndices(i => self.Count - 1 - i);

        /// <summary>
        /// Uses the provided indices to select elements from the array.
        /// </summary>
        public static IArray<T> SelectByIndex<T>(this IArray<T> self, IArray<int> indices)
            => indices.Select(i => self[i]);

        /// <summary>
        /// Uses the array as indices to select elements from the other array.
        /// </summary>
        public static IArray<T> Choose<T>(this IArray<int> indices, IArray<T> values)
            => values.SelectByIndex(indices);

        /// <summary>
        /// Given indices of sub-arrays groups, this will convert it to arrays of indices (e.g. [0, 2] with a group size of 3 becomes [0, 1, 2, 6, 7, 8])
        /// </summary>
        public static IArray<int> GroupIndicesToIndices(this IArray<int> indices, int groupSize)
            => groupSize == 1
                ? indices : (indices.Count * groupSize).Select(i => indices[i / groupSize] * groupSize + i % groupSize);

        /// <summary>
        /// Return the array separated into a series of groups (similar to DictionaryOfLists)
        /// based on keys created by the given keySelector
        /// </summary>
        public static IEnumerable<IGrouping<TKey, TSource>> GroupBy<TSource, TKey>(this IArray<TSource> self, Func<TSource, TKey> keySelector)
            => self.ToEnumerable().GroupBy(keySelector);

        /// <summary>
        /// Return the array separated into a series of groups (similar to DictionaryOfLists)
        /// based on keys created by the given keySelector and elements chosen by the element selector
        /// </summary>
        public static IEnumerable<IGrouping<TKey, TElem>> GroupBy<TSource, TKey, TElem>(this IArray<TSource> self, Func<TSource, TKey> keySelector, Func<TSource, TElem> elementSelector)
            => self.ToEnumerable().GroupBy(keySelector, elementSelector);

        /// <summary>
        /// Uses the provided indices to select groups of contiguous elements from the array.
        /// This is equivalent to self.SubArrays(groupSize).SelectByIndex(indices).SelectMany();
        /// </summary>
        public static IArray<T> SelectGroupsByIndex<T>(this IArray<T> self, int groupSize, IArray<int> indices)
            => self.SelectByIndex(indices.GroupIndicesToIndices(groupSize));

        /// <summary>
        /// Similar to take, if count is less than the number of items in the array, otherwise uses a modulo operation.
        /// </summary>
        public static IArray<T> Resize<T>(this IArray<T> self, int count)
            => Select(count, i => self[i % self.Count]);

        /// <summary>
        /// Returns an array of the same type with no elements.
        /// </summary>
        public static IArray<T> Empty<T>(this IArray<T> self)
            => self.Take(0);

        /// <summary>
        /// Returns an array of the same type with no elements.
        /// </summary>
        public static IArray<T> Empty<T>()
            => default(T).Repeat(0);

        /// <summary>
        /// Returns a sequence of integers from 0 to 1 less than the number of items in the array, representing indicies of the array.
        /// </summary>
        public static IArray<int> Indices<T>(this IArray<T> self)
            => self.Count.Range();

        /// <summary>
        /// Converts an array of elements into a string representation
        /// </summary>
        public static string Join<T>(this IArray<T> self, string sep = " ")
            => self.Aggregate(new StringBuilder(), (sb, x) => sb.Append(x).Append(sep)).ToString();

        /// <summary>
        /// Concatenates the contents of one array with another.
        /// </summary>
        public static IArray<T> Concatenate<T>(this IArray<T> self, IArray<T> other)
            => Select(self.Count + other.Count, i => i < self.Count ? self[i] : other[i - self.Count]);

        /// <summary>
        /// Returns the index of the first element matching the given item.
        /// </summary>
        public static int IndexOf<T>(this IArray<T> self, T item) where T : IEquatable<T>
            => self.IndexOf(x => x.Equals(item));

        /// <summary>
        /// Returns the index of the first element matching the given item.
        /// </summary>
        public static int IndexOf<T>(this IArray<T> self, Func<T, bool> predicate) 
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
        public static int LastIndexOf<T>(this IArray<T> self, T item) where T : IEquatable<T>
        {
            var n = self.Reverse().IndexOf(item);
            return n < 0 ? n : self.Count - 1 - n;
        }

        /// <summary>
        /// Returns an array that is one element shorter that subtracts each element from its previous one.
        /// </summary>
        public static IArray<int> AdjacentDifferences(this IArray<int> self)
            => self.ZipEachWithNext((a, b) => b - a);

        /// <summary>
        /// Creates a new array that concatenates a unit item list of one item after it.
        /// Repeatedly calling Append would result in significant performance degradation.
        /// </summary>
        public static IArray<T> Append<T>(this IArray<T> self, T x)
            => (self.Count + 1).Select(i => i < self.Count ? self[i] : x);

        /// <summary>
        /// Creates a new array that concatenates the given items to itself.
        /// </summary>
        public static IArray<T> Append<T>(this IArray<T> self, params T[] x)
            => self.Concatenate(x.ToIArray());

        /// <summary>
        /// Creates a new array that concatenates a unit item list of one item before it
        /// Repeatedly calling Prepend would result in significant performance degradation.
        /// </summary>
        public static IArray<T> Prepend<T>(this IArray<T> self, T x)
            => (self.Count + 1).Select(i => i == 0 ? x : self[i - 1]);

        /// <summary>
        /// Returns the element at the nth position, where n is modulo the number of items in the arrays.
        /// </summary>
        public static T ElementAt<T>(this IArray<T> self, int n)
            => self[n];

        /// <summary>
        /// Returns the element at the nth position, where n is modulo the number of items in the arrays.
        /// </summary>
        public static T ElementAtModulo<T>(this IArray<T> self, int n)
            => self.ElementAt(n % self.Count);

        /// <summary>
        /// Returns the Nth element of the array, or a default value if out of range/
        /// </summary>
        public static T? ElementAtOrDefault<T>(this IArray<T> xs, int n, T? defaultValue = default)
            => xs != null && n >= 0 && n < xs.Count ? xs[n] : defaultValue;

        /// <summary>
        /// Counts all elements in an array that satisfy a predicate
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int CountWhere<T>(this IArray<T> self, Func<T, bool> p)
           => self.Aggregate(0, (n, x) => n + (p(x) ? 1 : 0));

        /// <summary>
        /// Counts all elements in an array that are equal to true
        /// </summary>
        public static int CountWhere(this IArray<bool> self)
            => self.CountWhere(x => x);

        /// <summary>
        /// Counts all elements in an array that are equal to a value
        /// </summary>
        public static int CountWhere<T>(this IArray<T> self, T val) where T : IEquatable<T>
            => self.CountWhere(x => x.Equals(val));

        /// <summary>
        /// Returns the minimum element in the list
        /// </summary>
        public static T Min<T>(this IArray<T> self) where T : IComparable<T>
        {
            if (self.Count == 0) throw new ArgumentOutOfRangeException(nameof(self));
            return self.Aggregate(self[0], (a, b) => a.CompareTo(b) < 0 ? a : b);
        }

        /// <summary>
        /// Returns the maximum element in the list
        /// </summary>
        public static T Max<T>(this IArray<T> self) where T : IComparable<T>
        {
            if (self.Count == 0) throw new ArgumentOutOfRangeException(nameof(self));
            return self.Aggregate(self[0], (a, b) => a.CompareTo(b) > 0 ? a : b);
        }

        /// <summary>
        /// Applies a function (like "+") to each element in the series to create an effect similar to partial sums.
        /// </summary>
        public static IArray<T> Accumulate<T>(this IArray<T> self, Func<T, T, T> f)
        {
            var n = self.Count;
            var r = new T[n];
            if (n == 0) return r.ToIArray();
            var prev = r[0] = self[0];
            for (var i = 1; i < n; ++i)
            {
                prev = r[i] = f(prev, self[i]);
            }
            return r.ToIArray();
        }

        /// <summary>
        /// Applies a function (like "+") to each element in the series to create an effect similar to partial sums.
        /// The first value in the array will be zero.
        /// </summary>
        public static IArray<T> PostAccumulate<T>(this IArray<T> self, Func<T, T, T> f, T? init = default)
        {
            if (init == null) throw new ArgumentNullException(nameof(init));
            var n = self.Count;
            var r = new T[n + 1];
            // TODO: this doesn't look kosher. Not sure how to fix it. 
            var prev = r[0] = init;
            if (n == 0) return r.ToIArray();
            for (var i = 0; i < n; ++i)
            {
                prev = r[i + 1] = f(prev, self[i]);
            }
            return r.ToIArray();
        }

        /// <summary>
        /// Returns true if the two lists are the same length, and the elements are the same.
        /// </summary>
        public static bool SequenceEquals<T>(this IArray<T> self, IArray<T> other) where T : IEquatable<T>
            => self == other || (self.Count == other.Count && self.Zip(other, (x, y) => x?.Equals(y) ?? y == null).All(x => x));


        /// <summary>
        /// Creates an array of arrays, split at the given indices
        /// </summary>
        public static IArray<IArray<T>> Split<T>(this IArray<T> self, IArray<int> indices)
            => indices.Prepend(0).Zip(indices.Append(self.Count), (x, y) => self.Slice(x, y));

        /// <summary>
        /// Creates an array of arrays, split at the given index.
        /// </summary>
        public static IArray<IArray<T>> Split<T>(this IArray<T> self, int index)
            => Create(self.Take(index), self.Skip(index));

        /// <summary>
        /// Splits an array of tuples into a tuple of array
        /// </summary>
        public static (IArray<T1>, IArray<T2>) Unzip<T1, T2>(this IArray<(T1, T2)> self)
            => (self.Select(pair => pair.Item1), self.Select(pair => pair.Item2));


        /// <summary>
        /// Returns true if the predicate is true for all of the elements in the array
        /// </summary>
        public static bool All<T>(this IArray<T> self, Func<T, bool> predicate)
            => self.ToEnumerable().All(predicate);

        /// <summary>
        /// Returns true if the predicate is true for any of the elements in the array
        /// </summary>
        public static bool Any<T>(this IArray<T> self, Func<T, bool> predicate)
            => self.ToEnumerable().Any(predicate);

        /// <summary>
        /// Sums items in an array using a selector function that returns integers.
        /// </summary>
        public static long Sum<T>(this IArray<T> self, Func<T, long> func)
            => self.Aggregate(0L, (init, x) => init + func(x));

        /// <summary>
        /// Sums items in an array using a selector function that returns doubles.
        /// </summary>
        public static double Sum<T>(this IArray<T> self, Func<T, double> func)
            => self.Aggregate(0.0, (init, x) => init + func(x));

        /// <summary>
        /// Forces evaluation (aka reification) of the array by creating a copy in memory.
        /// This is useful as a performance optimization, or to force the objects to exist permanently.
        /// </summary>
        public static IArray<T> Evaluate<T>(this IArray<T> x)
            => (x is ArrayAdapter<T>) ? x : x.ToArray().ToIArray();

        /// <summary>
        /// Forces evaluation (aka reification) of the array in parallel.
        /// </summary>
        public static IArray<T> EvaluateInParallel<T>(this IArray<T> x)
            => (x is ArrayAdapter<T>) ? x : x.ToArrayInParallel().ToIArray();

        /// <summary>
        /// Converts to a regular array in paralle;
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="xs"></param>
        /// <returns></returns>
        public static T[] ToArrayInParallel<T>(this IArray<T> xs)
        {
            if (xs.Count == 0)
                return Array.Empty<T>();

            if (xs.Count < Environment.ProcessorCount)
                return xs.ToArray();

            var r = new T[xs.Count];
            var partitioner = Partitioner.Create(0, xs.Count, xs.Count / Environment.ProcessorCount);

            Parallel.ForEach(partitioner, (range, state) =>
            {
                for (var i = range.Item1; i < range.Item2; ++i)
                    r[i] = xs[i];
            });
            return r;
        }

        /// <summary>
        /// Maps pairs of elements to a new array.
        /// </summary>
        public static IArray<U> SelectPairs<T, U>(this IArray<T> xs, Func<T, T, U> f)
            => (xs.Count / 2).Select(i => f(xs[i * 2], xs[i * 2 + 1]));

        /// <summary>
        /// Maps every 3 elements to a new array.
        /// </summary>
        public static IArray<U> SelectTriplets<T, U>(this IArray<T> xs, Func<T, T, T, U> f)
            => (xs.Count / 3).Select(i => f(xs[i * 3], xs[i * 3 + 1], xs[i * 3 + 2]));

        /// <summary>
        /// Maps every 4 elements to a new array.
        /// </summary>
        public static IArray<U> SelectQuartets<T, U>(this IArray<T> xs, Func<T, T, T, T, U> f)
            => (xs.Count / 4).Select(i => f(xs[i * 4], xs[i * 4 + 1], xs[i * 4 + 2], xs[i * 4 + 3]));

        /// <summary>
        /// Returns the number of unique instances of elements in the array.
        /// </summary>
        public static int CountUnique<T>(this IArray<T> xs)
            => xs.ToEnumerable().Distinct().Count();

        /// <summary>
        /// Given an array of elements of type T casts them to a U
        /// </summary>
        public static IArray<U> Cast<T, U>(this IArray<T> xs) where T : U
            => xs.Select(x => (U)x);

        /// <summary>
        /// Returns true if the value is present in the array.
        /// </summary>
        public static bool Contains<T>(this IArray<T> xs, T value)
            => xs.Any(x => x?.Equals(value) ?? false);

        public static T? FirstOrDefault<T>(this IArray<T> xs)
            => xs.Count > 0 ? xs[0] : default;

        public static T FirstOrDefault<T>(this IArray<T> xs, T @default)
            => xs.Count > 0 ? xs[0] : @default;

        public static T? FirstOrDefault<T>(this IArray<T> xs, Func<T, bool> predicate)
            => xs.Where(predicate).FirstOrDefault();

        public static IArray<long> ToLongs(this IArray<int> xs)
            => xs.Select(x => (long)x);

        public static IArray<long> PrefixSums(this IArray<int> self)
            => self.ToLongs().PrefixSums();

        public static IArray<float> PrefixSums(this IArray<float> self)
            => self.Scan(0f, (a, b) => a + b);

        public static IArray<double> PrefixSums(this IArray<double> self)
            => self.Scan(0.0, (a, b) => a + b);

        public static IArray<U> Scan<T,U>(this IArray<T> self, U init, Func<U, T, U> scanFunc)
        {
            if (self.Count == 0)
                return Empty<U>();
            var r = new U[self.Count];
            for (var i = 0; i < self.Count; ++i)
                init = r[i] = scanFunc(init, self[i]);
            return r.ToIArray();
        }

        public static IArray<long> PrefixSums(this IArray<long> counts)
            => counts.Scan(0L, (a, b) => a + b);

        // Similar to prefix sums, but starts at zero.
        // r[i] = Sum(count[0 to i])
        public static IArray<int> CountsToOffsets(this IArray<int> counts)
        {
            var r = new int[counts.Count];
            for (var i = 1; i < counts.Count; ++i)
                r[i] = r[i - 1] + counts[i - 1];
            return r.ToIArray();
        }

        public static IArray<int> OffsetsToCounts(this IArray<int> offsets, int last)
            => offsets.Indices().Select(i => i < offsets.Count - 1 ? offsets[i + 1] - offsets[i] : last - offsets[i]);

        public static IArray<T> SetElementAt<T>(this IArray<T> self, int index, T value)
            => self.SelectIndices(i => i == index ? value : self[i]);

        public static IArray<T> SetFirstElementWhere<T>(this IArray<T> self, Func<T, bool> predicate, T value)
        {
            var index = self.IndexOf(predicate);
            if (index < 0)
                return self;
            return self.SetElementAt(index, value);
        }
    }
}
