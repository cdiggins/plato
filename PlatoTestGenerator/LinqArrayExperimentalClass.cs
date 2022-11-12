﻿// MIT License - Copyright 2019 (C) VIMaec, LLC.
// MIT License - Copyright (C) Ara 3D, Inc.
// file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of source code package.
//
// LinqArray.cs
// A library for working with pure functional arrays, using LINQ style extension functions.
// Based on code that originally appeared in https://www.codeproject.com/Articles/140138/Immutable-Array-for-NET

// TODO: remove the dependencies on IEnumerable etc. 


using System.Runtime.CompilerServices;
using System;

// This is how you write the Aggregate function in C# 
public static U Aggregate<T, U>(this IArray<T> self, U init, Func<U, T, U> func)
{
    for (var i = 0; i < self.Count; ++i)
        init = func(init, self[i]);
    return init;
}

// Here is the same function written in Plato
Any Aggregate(IArray self, Any init, Func func)
{
    for (var i = 0; i < self.Count; ++i)
        init = func(init, self[i], i);
    return init;
}

// vaccines ... rage | distemper | bhpp | bordatella




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

    /// 
    /// Extension functions for working on any object implementing IArray. overrides
    /// many of the Linq functions providing better performance.
    /// 
    class LinqArray
    {
        /// 
        /// Helper function for creating an IArray from the arguments.
        /// 
        IArray Create(params Any[] self)
            => self.ToIArray();

        /// 
        /// A helper function to enable IArray to support IEnumerable
        /// 
        IEnumerable ToEnumerable(IArray self)
            => Enumerable.Range(0, self.Count).Select(self.ElementAt);

        IArray ForEach(IArray xs, Action f)
        {
            for (var i = 0; i < xs.Count; ++i)
                f(xs[i]);
            return xs;
        }

        /// 
        /// Creates an IArray with the given number of items,
        /// and uses the function to return items.
        /// 
        IArray Select(int count, Func f)
            => new FunctionalArray(count, f);

        /// 
        /// Converts any implementation of IList (e.g. Array/List) to an IArray.
        /// 
        IArray ToIArray(Any[] self)
            => new ArrayAdapter(self);

        /// 
        /// Creates an IArray by repeating each item in the source a number of times.
        /// 
        IArray RepeatElements(IArray self, int count)
            => Select(count, i => self[i / count]);

        /// 
        /// Creates an IArray starting with a seed value, and applying a function
        /// to each individual member of the array. is eagerly evaluated.
        /// 
        IArray Generate(Any init, int count, Func f)
        {
            var r = new Any[count];
            for (var i = 0; i < count; ++i)
            {
                r[i] = init;
                init = f(init);
            }
            return r.ToIArray();
        }

        /// 
        /// Returns the first item in the array.
        /// 
        Any First(IArray self, Any @default = default)
            => self.IsEmpty() ? @default : self[0];

        /// 
        /// Returns the last item in the array
        /// 
        Any Last(IArray self, Any @default = default)
            => self.IsEmpty() ? @default : self[self.Count - 1];

        /// 
        /// Returns true if and only if the argument is a valid index into the array.
        /// 
        bool InRange(IArray self, int n)
            => n >= 0 && n < self.Count;

        /// 
        /// A mnemonic for "Any()" that returns false if the count is greater than zero
        /// 
        bool IsEmpty(IArray self)
            => !self.Any();

        /// 
        /// Returns true if there are any elements in the array.
        /// 
        bool Any(IArray self)
            => self.Count != 0;

        /// 
        /// Converts the IArray into a system array.
        /// 
        Any[] ToArray(IArray self)
            => self.CopyTo(new Any[self.Count]);

        /// 
        /// Copies the IArray into a system array.
        /// 
        Any[] CopyTo(IArray self, Any[] result, int offset = 0)
        {
            for (var i = 0; i < self.Count; i++)
                result[offset + i] = self[i];
            return result;
        }

        /// 
        /// Converts the IArray into a system List.
        /// 
        List ToList(IArray self)
            => self.ToEnumerable().ToList();

        /// 
        /// Converts the array into a function that returns values from an integer, returning a default value if out of range.
        /// 
        Func ToFunction(IArray self, Any def = default)
            => i => self.InRange(i) ? self[i] : def;

        /// 
        /// Converts the array into a predicate (a function that returns true or false) based on the truth of the given value.
        /// 
        Func ToPredicate(IArray self)
            => self.ToFunction();


        /// 
        /// Returns an array generated by applying a function to each element.
        /// 
        IArray Select(IArray self, Func f)
            => Select(self.Count, i => f(self[i]));

        /// 
        /// Returns an array generated by applying a function to each element.
        /// 
        IArray Select(IArray self, Func f)
            => Select(self.Count, i => f(self[i], i));

        /// 
        /// Returns an array generated by applying a function to each element.
        /// 
        IArray SelectIndices(IArray self, Func f)
            => self.Count.Select(f);

        /// 
        /// Converts an array of array into a flattened array. Each array is assumed to be of size n.
        /// 
        IArray Flatten(IArray self, int n)
            => Select(self.Count * n, i => self[i / n][i % n]);

        /// 
        /// Converts an array of array into a flattened array.
        /// 
        IArray Flatten(IArray self)
        {
            var counts = self.Select(x => x.Count).PostAccumulate((x, y) => x + y);
            var r = new Any[counts.Last()];
            var i = 0;
            foreach (var xs in self.ToEnumerable())
                xs.CopyTo(r, counts[i++]);
            return r.ToIArray();
        }

        /// 
        /// Returns an array of tuple where each element of the initial array is paired with its index.
        /// 
        IArray ZipWithIndex(IArray self)
            => self.Select((v, i) => (v, i));

        /// 
        /// Returns an array from an array of arrays, where the number of sub-elements is the same for reach array and is known.
        /// 
        IArray SelectMany(IArray self, int count)
            => Select(self.Count, i => self[i / count][i % count]);

        /// 
        /// Returns an array given a function that generates an IArray from each member. Eager evaluation.
        /// 
        IArray SelectMany(IArray self, Func func)
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

        /// 
        /// Returns an array given a function that generates an IArray from each member. Eager evaluation.
        /// 
        IArray SelectMany(IArray self, Func func)
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


        /// 
        /// Returns an array generated by applying a function to corresponding pairs of elements in both arrays.
        /// 
        IArray Zip(IArray self, IArray other, Func f)
            => Select(Math.Min(self.Count, other.Count), i => f(self[i], other[i]));

        /// 
        /// Returns an array generated by applying a function to corresponding pairs of elements in both arrays.
        /// 
        IArray Zip(IArray self, IArray other, Func f)
            => Select(Math.Min(self.Count, other.Count), i => f(self[i], other[i], i));

        /// 
        /// Returns an array generated by applying a function to corresponding pairs of elements in both arrays.
        /// 
        IArray Zip(IArray self, IArray other, IArray other2, Func f)
            => Select(Math.Min(Math.Min(self.Count, other.Count), other2.Count), i => f(self[i], other[i], other2[i]));

        /// 
        /// Returns an array generated by applying a function to corresponding pairs of elements in both arrays.
        /// 
        IArray Zip(IArray self, IArray other, IArray other2, Func f)
            => Select(Math.Min(Math.Min(self.Count, other.Count), other2.Count), i => f(self[i], other[i], other2[i], i));

        /// 
        /// Returns an array generated by applying a function to corresponding pairs of elements in both arrays.
        /// 
        IArray Zip(IArray self, IArray other, IArray other2, IArray other3, Func f)
            => Select(Math.Min(Math.Min(self.Count, other.Count), Math.Min(other2.Count, other3.Count)), i => f(self[i], other[i], other2[i], other3[i]));

        /// 
        /// Returns an array generated by applying a function to corresponding pairs of elements in both arrays.
        /// 
        IArray Zip(IArray self, IArray other, IArray other2, IArray other3, Func f)
            => Select(Math.Min(Math.Min(self.Count, other.Count), Math.Min(other2.Count, other3.Count)), i => f(self[i], other[i], other2[i], other3[i], i));

        /// 
        /// Applies a function to each element in the list paired with the next one.
        /// Used to implement adjacent differences for example.
        /// 
        IArray ZipEachWithNext(IArray self, Func f)
            => self.Zip(self.Skip(), f);

        /// 
        /// Returns an IEnumerable containing only elements of the array for which the function returns true on the index.
        /// An IArray is not created automatically because it is an expensive operation that is potentially unneeded.
        /// 
        IEnumerable WhereIndices(IArray self, Func f)
            => self.Where((x, i) => f(i));

        /// 
        /// Returns an IEnumerable containing only elements of the array for which the corresponding mask is true.
        /// An IArray is not created automatically because it is an expensive operation that is potentially unneeded.
        /// 
        IEnumerable Where(IArray self, IArray mask)
            => self.WhereIndices(mask.ToPredicate());

        /// 
        /// Returns an IEnumerable containing only elements of the array for which the corresponding predicate is true.
        /// 
        IEnumerable Where(IArray self, Func predicate)
            => self.ToEnumerable().Where(predicate);

        /// 
        /// Returns an IEnumerable containing only elements of the array for which the corresponding predicate is true.
        /// 
        IEnumerable Where(IArray self, Func predicate)
            => self.ToEnumerable().Where(predicate);

        /// 
        /// Returns an IEnumerable containing only indices of the array for which the function satisfies a specific predicate.
        /// An IArray is not created automatically because it is an expensive operation that is potentially unneeded.
        /// 
        IEnumerable IndicesWhere(IArray self, Func f)
            => self.Indices().Where(i => f(self[i]));

        /// 
        /// Returns an IEnumerable containing only indices of the array for which the function satisfies a specific predicate.
        /// An IArray is not created automatically because it is an expensive operation that is potentially unneeded.
        /// 
        IEnumerable IndicesWhere(IArray self, Func f)
            => self.IndicesWhere(i => f(self[i], i));

        /// 
        /// Returns an IEnumerable containing only indices of the array for which the function satisfies a specific predicate.
        /// An IArray is not created automatically because it is an expensive operation that is potentially unneeded.
        /// 
        IEnumerable IndicesWhere(IArray self, Func f)
            => self.Indices().Where(i => f(i));

        /// 
        /// Returns an IEnumerable containing only indices of the array for which booleans in the mask are true.
        /// An IArray is not created automatically because it is an expensive operation that is potentially unneeded.
        /// 
        IEnumerable IndicesWhere(IArray self, IArray mask)
            => self.IndicesWhere(mask.ToPredicate());

        /// 
        /// Shortcut for ToEnumerable.Aggregate()
        /// 
        Any Aggregate(IArray self, Func func)
            => Aggregate(self, default, func);

        /// 
        /// Shortcut for ToEnumerable.Aggregate()
        /// 
        Any Aggregate(IArray self, Any init, Func func)
        {
            for (var i = 0; i < self.Count; ++i)
                init = func(init, self[i], i);
            return init;
        }

        /// 
        /// Shortcut for ToEnumerable.Aggregate()
        /// 
        Any Aggregate(IArray self, Func func)
            => Aggregate(self, default, func);

        /// 
        /// Returns a new array containing the elements in the range of from to to.
        /// 
        IArray Slice(IArray self, int from, int to)
            => Select(to - from, i => self[i + from]);

        /// 
        /// Returns an array of SubArrays of size "size"
        /// the last items that cannot fill an arrat if size "size" will be ignored
        /// 
        IArray SubArraysFixed(IArray self, int size)
            => (self.Count / size).Select(i => self.SubArray(i, size));


        ///
        /// Returns an array of SubArrays of size "size" plus extras
        /// The extra array is of size count % size if present
        /// 
        IArray SubArrays(IArray self, int size)
            => self.Count % size == 0
                ? self.SubArraysFixed(size)
                : self.SubArraysFixed(size).Append(self.TakeLast(self.Count % size));

        /// 
        /// Returns n elements of the list starting from a given index.
        /// 
        IArray SubArray(IArray self, int from, int count)
            => self.Slice(from, count + from);

        /// 
        /// Returns elements of the array between from and skipping every stride element.
        /// 
        IArray Slice(IArray self, int from, int to, int stride)
            => Select(to - from / stride, i => self[i * stride + from]);

        /// 
        /// Returns a new array containing the elements by taking every nth item.
        /// 
        IArray Stride(IArray self, int n)
            => Select(self.Count / n, i => self[i * n % self.Count]);

        /// 
        /// Returns a new array containing just the first n items.
        /// 
        IArray Take(IArray self, int n)
            => self.Slice(0, n);

        /// 
        /// Returns a new array containing just at most n items.
        /// 
        IArray TakeAtMost(IArray self, int n)
            => self.Count > n ? self.Slice(0, n) : self;

        /// 
        /// Returns a new array containing the elements after the first n elements.
        /// 
        IArray Skip(IArray self, int n = 1)
            => self.Slice(n, self.Count);

        /// 
        /// Returns a new array containing the last n elements.
        /// 
        IArray TakeLast(IArray self, int n = 1)
            => self.Skip(self.Count - n);

        /// 
        /// Returns a new array containing all elements excluding the last n elements.
        /// 
        IArray DropLast(IArray self, int n = 1)
            => self.Count > n ? self.Take(self.Count - n) : self.Empty();

        /// 
        /// Returns a new array by remapping indices
        /// 
        IArray MapIndices(IArray self, Func f)
            => self.Count.Select(i => self[f(i)]);

        /// 
        /// Returns a new array that reverses the order of elements
        /// 
        IArray Reverse(IArray self)
            => self.MapIndices(i => self.Count - 1 - i);

        /// 
        /// Uses the provided indices to select elements from the array.
        /// 
        IArray SelectByIndex(IArray self, IArray indices)
            => indices.Select(i => self[i]);

        /// 
        /// Uses the array as indices to select elements from the other array.
        /// 
        IArray Choose(IArray indices, IArray values)
            => values.SelectByIndex(indices);

        /// 
        /// Given indices of sub-arrays groups, will convert it to arrays of indices (e.g. [0, 2] with a group size of 3 becomes [0, 1, 2, 6, 7, 8])
        /// 
        IArray GroupIndicesToIndices(IArray indices, int groupSize)
            => groupSize == 1
                ? indices : (indices.Count * groupSize).Select(i => indices[i / groupSize] * groupSize + i % groupSize);

        /// 
        /// Return the array separated into a series of groups (similar to DictionaryOfLists)
        /// based on keys created by the given keySelector
        /// 
        IEnumerable> GroupBy(IArray self, Func keySelector)
            => self.ToEnumerable().GroupBy(keySelector);

        /// 
        /// Return the array separated into a series of groups (similar to DictionaryOfLists)
        /// based on keys created by the given keySelector and elements chosen by the element selector
        /// 
        IEnumerable> GroupBy(IArray self, Func keySelector, Func elementSelector)
            => self.ToEnumerable().GroupBy(keySelector, elementSelector);

        /// 
        /// Uses the provided indices to select groups of contiguous elements from the array.
        /// is equivalent to self.SubArrays(groupSize).SelectByIndex(indices).SelectMany();
        /// 
        IArray SelectGroupsByIndex(IArray self, int groupSize, IArray indices)
            => self.SelectByIndex(indices.GroupIndicesToIndices(groupSize));

        /// 
        /// Similar to take, if count is less than the number of items in the array, otherwise uses a modulo operation.
        /// 
        IArray Resize(IArray self, int count)
            => Select(count, i => self[i % self.Count]);

        /// 
        /// Returns an array of the same type with no elements.
        /// 
        IArray Empty(IArray self)
            => self.Take(0);

        /// 
        /// Returns an array of the same type with no elements.
        /// 
        IArray Empty()
            => default(Any).Repeat(0);

        /// 
        /// Returns a sequence of integers from 0 to 1 less than the number of items in the array, representing indicies of the array.
        /// 
        IArray Indices(IArray self)
            => self.Count.Range();

        /// 
        /// Converts an array of elements into a string representation
        /// 
        string Join(IArray self, string sep = " ")
            => self.Aggregate(new StringBuilder(), (sb, x) => sb.Append(x).Append(sep)).ToString();

        /// 
        /// Concatenates the contents of one array with another.
        /// 
        IArray Concatenate(IArray self, IArray other)
            => Select(self.Count + other.Count, i => i < self.Count ? self[i] : other[i - self.Count]);

        /// 
        /// Returns the index of the first element matching the given item.
        /// 
        int IndexOf(IArray self, Any item) where Any : IEquatable
            => self.IndexOf(x => x.Equals(item));

        /// 
        /// Returns the index of the first element matching the given item.
        /// 
        int IndexOf(IArray self, Func predicate)
        {
            for (var i = 0; i < self.Count; ++i)
            {
                if (predicate(self[i]))
                    return i;
            }

            return -1;
        }

        /// 
        /// Returns the index of the last element matching the given item.
        /// 
        int LastIndexOf(IArray self, Any item) where Any : IEquatable
        {
            var n = self.Reverse().IndexOf(item);
            return n < 0 ? n : self.Count - 1 - n;
        }

        /// 
        /// Returns an array that is one element shorter that subtracts each element from its previous one.
        /// 
        IArray AdjacentDifferences(IArray self)
            => self.ZipEachWithNext((a, b) => b - a);

        /// 
        /// Creates a new array that concatenates a unit item list of one item after it.
        /// Repeatedly calling Append would result in significant performance degradation.
        /// 
        IArray Append(IArray self, Any x)
            => (self.Count + 1).Select(i => i < self.Count ? self[i] : x);

        /// 
        /// Creates a new array that concatenates the given items to itself.
        /// 
        IArray Append(IArray self, params Any[] x)
            => self.Concatenate(x.ToIArray());

        /// 
        /// Creates a new array that concatenates a unit item list of one item before it
        /// Repeatedly calling Prepend would result in significant performance degradation.
        /// 
        IArray Prepend(IArray self, Any x)
            => (self.Count + 1).Select(i => i == 0 ? x : self[i - 1]);

        /// 
        /// Returns the element at the nth position, where n is modulo the number of items in the arrays.
        /// 
        Any ElementAt(IArray self, int n)
            => self[n];

        /// 
        /// Returns the element at the nth position, where n is modulo the number of items in the arrays.
        /// 
        Any ElementAtModulo(IArray self, int n)
            => self.ElementAt(n % self.Count);

        /// 
        /// Returns the Nth element of the array, or a default value if out of range/
        /// 
        Any ElementAtOrDefault(IArray xs, int n, Any defaultValue = default)
            => xs != null && n >= 0 && n < xs.Count ? xs[n] : defaultValue;

        /// 
        /// Counts all elements in an array that satisfy a predicate
        /// 
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        int CountWhere(IArray self, Func p)
           => self.Aggregate(0, (n, x) => n + (p(x) ? 1 : 0));

        /// 
        /// Counts all elements in an array that are equal to true
        /// 
        int CountWhere(IArray self)
            => self.CountWhere(x => x);

        /// 
        /// Counts all elements in an array that are equal to a value
        /// 
        int CountWhere(IArray self, Any val) where Any : IEquatable
            => self.CountWhere(x => x.Equals(val));

        /// 
        /// Returns the minimum element in the list
        /// 
        Any Min(IArray self) where Any : IComparable
        {
            if (self.Count == 0) throw new ArgumentOutOfRangeException(nameof(self));
            return self.Aggregate(self[0], (a, b) => a.CompareTo(b) < 0 ? a : b);
        }

        /// 
        /// Returns the maximum element in the list
        /// 
        Any Max(IArray self) where Any : IComparable
        {
            if (self.Count == 0) throw new ArgumentOutOfRangeException(nameof(self));
            return self.Aggregate(self[0], (a, b) => a.CompareTo(b) > 0 ? a : b);
        }

        /// 
        /// Applies a function (like "+") to each element in the series to create an effect similar to partial sums.
        /// 
        IArray Accumulate(IArray self, Func f)
        {
            var n = self.Count;
            var r = new Any[n];
            if (n == 0) return r.ToIArray();
            var prev = r[0] = self[0];
            for (var i = 1; i < n; ++i)
            {
                prev = r[i] = f(prev, self[i]);
            }
            return r.ToIArray();
        }

        /// 
        /// Applies a function (like "+") to each element in the series to create an effect similar to partial sums.
        /// The first value in the array will be zero.
        /// 
        IArray PostAccumulate(IArray self, Func f, Any init = default)
        {
            if (init == null) throw new ArgumentNullException(nameof(init));
            var n = self.Count;
            var r = new Any[n + 1];
            // TODO: doesn't look kosher. Not sure how to fix it. 
            var prev = r[0] = init;
            if (n == 0) return r.ToIArray();
            for (var i = 0; i < n; ++i)
            {
                prev = r[i + 1] = f(prev, self[i]);
            }
            return r.ToIArray();
        }

        /// 
        /// Returns true if the two lists are the same length, and the elements are the same.
        /// 
        bool SequenceEquals(IArray self, IArray other) where Any : IEquatable
            => self == other || (self.Count == other.Count && self.Zip(other, (x, y) => x?.Equals(y) ?? y == null).All(x => x));

        /// 
        /// Creates an array of arrays, split at the given indices
        /// 
        IArray Split(IArray self, IArray indices)
            => indices.Prepend(0).Zip(indices.Append(self.Count), (x, y) => self.Slice(x, y));

        /// 
        /// Creates an array of arrays, split at the given index.
        /// 
        IArray Split(IArray self, int index)
            => Create(self.Take(index), self.Skip(index));

        /// 
        /// Splits an array of tuples into a tuple of array
        /// 
        (IArray, IArray) Unzip(IArray self)
            => (self.Select(pair => pair.Item1), self.Select(pair => pair.Item2));


        /// 
        /// Returns true if the predicate is true for all of the elements in the array
        /// 
        bool All(IArray self, Func predicate)
            => self.ToEnumerable().All(predicate);

        /// 
        /// Returns true if the predicate is true for any of the elements in the array
        /// 
        bool Any(IArray self, Func predicate)
            => self.ToEnumerable().Any(predicate);

        /// 
        /// Sums items in an array using a selector function that returns integers.
        /// 
        long Sum(IArray self, Func func)
            => self.Aggregate(0L, (init, x) => init + func(x));

        /// 
        /// Sums items in an array using a selector function that returns doubles.
        /// 
        double Sum(IArray self, Func func)
            => self.Aggregate(0.0, (init, x) => init + func(x));

        /// 
        /// Forces evaluation (aka reification) of the array by creating a copy in memory.
        /// is useful as a performance optimization, or to force the objects to exist permanently.
        /// 
        IArray Evaluate(IArray x)
            => (x is ArrayAdapter) ? x : x.ToArray().ToIArray();

        /// 
        /// Forces evaluation (aka reification) of the array in parallel.
        /// 
        IArray EvaluateInParallel(IArray x)
            => (x is ArrayAdapter) ? x : x.ToArrayInParallel().ToIArray();

        /// 
        /// Converts to a regular array in paralle;
        /// 
        /// 
        /// 
        /// 
        Any[] ToArrayInParallel(IArray xs)
        {
            if (xs.Count == 0)
                return Array.Empty();

            if (xs.Count < Environment.ProcessorCount)
                return xs.ToArray();

            var r = new Any[xs.Count];
            var partitioner = Partitioner.Create(0, xs.Count, xs.Count / Environment.ProcessorCount);

            Parallel.ForEach(partitioner, (range, state) =>
            {
                for (var i = range.Item1; i < range.Item2; ++i)
                    r[i] = xs[i];
            });
            return r;
        }

        /// 
        /// Maps pairs of elements to a new array.
        /// 
        IArray SelectPairs(IArray xs, Func f)
            => (xs.Count / 2).Select(i => f(xs[i * 2], xs[i * 2 + 1]));

        /// 
        /// Maps every 3 elements to a new array.
        /// 
        IArray SelectTriplets(IArray xs, Func f)
            => (xs.Count / 3).Select(i => f(xs[i * 3], xs[i * 3 + 1], xs[i * 3 + 2]));

        /// 
        /// Maps every 4 elements to a new array.
        /// 
        IArray SelectQuartets(IArray xs, Func f)
            => (xs.Count / 4).Select(i => f(xs[i * 4], xs[i * 4 + 1], xs[i * 4 + 2], xs[i * 4 + 3]));

        /// 
        /// Returns the number of unique instances of elements in the array.
        /// 
        int CountUnique(IArray xs)
            => xs.ToEnumerable().Distinct().Count();

        /// 
        /// Returns true if the value is present in the array.
        /// 
        bool Contains(IArray xs, Any value)
            => xs.Any(x => x?.Equals(value) ?? false);

        Any FirstOrDefault(IArray xs)
            => xs.Count > 0 ? xs[0] : default;

        Any FirstOrDefault(IArray xs, Any @default)
            => xs.Count > 0 ? xs[0] : @default;

        Any FirstOrDefault(IArray xs, Func predicate)
            => xs.Where(predicate).FirstOrDefault();

        IArray ToLongs(IArray xs)
            => xs.Select(x => (long)x);

        IArray PrefixSums(IArray self)
            => self.ToLongs().PrefixSums();

        IArray PrefixSums(IArray self)
            => self.Scan(0f, (a, b) => a + b);

        IArray PrefixSums(IArray self)
            => self.Scan(0.0, (a, b) => a + b);

        IArray Scan(IArray self, Any init, Func scanFunc)
        {
            if (self.Count == 0)
                return Empty();
            var r = new Any[self.Count];
            for (var i = 0; i < self.Count; ++i)
                init = r[i] = scanFunc(init, self[i]);
            return r.ToIArray();
        }

        IArray PrefixSums(IArray counts)
            => counts.Scan(0L, (a, b) => a + b);

        // Similar to prefix sums, but starts at zero.
        // r[i] = Sum(count[0 to i])
        IArray CountsToOffsets(IArray counts)
        {
            var r = new int[counts.Count];
            for (var i = 1; i < counts.Count; ++i)
                r[i] = r[i - 1] + counts[i - 1];
            return r.ToIArray();
        }

        IArray OffsetsToCounts(IArray offsets, int last)
            => offsets.Indices().Select(i => i < offsets.Count - 1 ? offsets[i + 1] - offsets[i] : last - offsets[i]);

        IArray SetElementAt(IArray self, int index, Any value)
            => self.SelectIndices(i => i == index ? value : self[i]);

        IArray SetFirstElementWhere(IArray self, Func predicate, Any value)
        {
            var index = self.IndexOf(predicate);
            if (index < 0)
                return self;
            return self.SetElementAt(index, value);
        }
    }
}
erimental
{

    /// 
    /// Extension functions for working on any object implementing IArray. overrides
    /// many of the Linq functions providing better performance.
    /// 
    class LinqArray
    {
        /// 
        /// Helper function for creating an IArray from the arguments.
        /// 
        IArray Create(params Any[] self)
            => self.ToIArray();

        /// 
        /// A helper function to enable IArray to support IEnumerable
        /// 
        IEnumerable ToEnumerable(IArray self)
            => Enumerable.Range(0, self.Count).Select(self.ElementAt);

        IArray ForEach(IArray xs, Action f)
        {
            for (var i = 0; i < xs.Count; ++i)
                f(xs[i]);
            return xs;
        }

        /// 
        /// Creates an IArray with the given number of items,
        /// and uses the function to return items.
        /// 
        IArray Select(int count, Func f)
            => new FunctionalArray(count, f);

        /// 
        /// Converts any implementation of IList (e.g. Array/List) to an IArray.
        /// 
        IArray ToIArray(Any[] self)
            => new ArrayAdapter(self);

        /// 
        /// Creates an IArray by repeating each item in the source a number of times.
        /// 
        IArray RepeatElements(IArray self, int count)
            => Select(count, i => self[i / count]);

        /// 
        /// Creates an IArray starting with a seed value, and applying a function
        /// to each individual member of the array. is eagerly evaluated.
        /// 
        IArray Generate(Any init, int count, Func f)
        {
            var r = new Any[count];
            for (var i = 0; i < count; ++i)
            {
                r[i] = init;
                init = f(init);
            }
            return r.ToIArray();
        }

        /// 
        /// Returns the first item in the array.
        /// 
        Any First(IArray self, Any @default = default)
            => self.IsEmpty() ? @default : self[0];

        /// 
        /// Returns the last item in the array
        /// 
        Any Last(IArray self, Any @default = default)
            => self.IsEmpty() ? @default : self[self.Count - 1];

        /// 
        /// Returns true if and only if the argument is a valid index into the array.
        /// 
        bool InRange(IArray self, int n)
            => n >= 0 && n < self.Count;

        /// 
        /// A mnemonic for "Any()" that returns false if the count is greater than zero
        /// 
        bool IsEmpty(IArray self)
            => !self.Any();

        /// 
        /// Returns true if there are any elements in the array.
        /// 
        bool Any(IArray self)
            => self.Count != 0;

        /// 
        /// Converts the IArray into a system array.
        /// 
        Any[] ToArray(IArray self)
            => self.CopyTo(new Any[self.Count]);

        /// 
        /// Copies the IArray into a system array.
        /// 
        Any[] CopyTo(IArray self, Any[] result, int offset = 0)
        {
            for (var i = 0; i < self.Count; i++)
                result[offset + i] = self[i];
            return result;
        }

        /// 
        /// Converts the IArray into a system List.
        /// 
        List ToList(IArray self)
            => self.ToEnumerable().ToList();

        /// 
        /// Converts the array into a function that returns values from an integer, returning a default value if out of range.
        /// 
        Func ToFunction(IArray self, Any def = default)
            => i => self.InRange(i) ? self[i] : def;

        /// 
        /// Converts the array into a predicate (a function that returns true or false) based on the truth of the given value.
        /// 
        Func ToPredicate(IArray self)
            => self.ToFunction();

        /// 
        /// Returns an array generated by applying a function to each element.
        /// 
        IArray Select(IArray self, Func f)
            => Select(self.Count, i => f(self[i]));

        /// 
        /// Returns an array generated by applying a function to each element.
        /// 
        IArray Select(IArray self, Func f)
            => Select(self.Count, i => f(self[i], i));

        /// 
        /// Returns an array generated by applying a function to each element.
        /// 
        IArray SelectIndices(IArray self, Func f)
            => self.Count.Select(f);

        /// 
        /// Converts an array of array into a flattened array. Each array is assumed to be of size n.
        /// 
        IArray Flatten(IArray self, int n)
            => Select(self.Count * n, i => self[i / n][i % n]);

        /// 
        /// Converts an array of array into a flattened array.
        /// 
        IArray Flatten(IArray self)
        {
            var counts = self.Select(x => x.Count).PostAccumulate((x, y) => x + y);
            var r = new Any[counts.Last()];
            var i = 0;
            foreach (var xs in self.ToEnumerable())
                xs.CopyTo(r, counts[i++]);
            return r.ToIArray();
        }

        /// 
        /// Returns an array of tuple where each element of the initial array is paired with its index.
        /// 
        IArray ZipWithIndex(IArray self)
            => self.Select((v, i) => (v, i));

        /// 
        /// Returns an array from an array of arrays, where the number of sub-elements is the same for reach array and is known.
        /// 
        IArray SelectMany(IArray self, int count)
            => Select(self.Count, i => self[i / count][i % count]);

        /// 
        /// Returns an array given a function that generates an IArray from each member. Eager evaluation.
        /// 
        IArray SelectMany(IArray self, Func func)
        {
            var count = self.Sum(x => func(x).Count);
            var xs = new Any[count];
            var offset = 0;
            for (var i = 0; i < self.Count; ++i)
            {
                var sub = func(self[i]);
                sub.CopyTo(xs, offset);
                offset += sub.Count;
            }
            return xs.ToIArray();
        }

        /// 
        /// Returns an array given a function that generates an IArray from each member. Eager evaluation.
        /// 
        IArray SelectMany(IArray self, Func func)
        {
            var count = self.Aggregate(0, (acc, x, index) => acc + func(x, index).Count);
            var xs = new Any[count];
            var offset = 0;
            for (var i = 0; i < self.Count; ++i)
            {
                var sub = func(self[i], i);
                sub.CopyTo(xs, offset);
                offset += sub.Count;
            }
            return xs.ToIArray();
        }

        /// 
        /// Returns an array given a function that generates a tuple from each member. Eager evaluation.
        /// 
        IArray SelectMany(IArray self, Func func)
        {
            var r = new Any[self.Count * 2];
            for (var i = 0; i < self.Count; ++i)
            {
                var tmp = func(self[i]);
                r[i * 2] = tmp.Item1;
                r[i * 2 + 1] = tmp.Item2;
            }

            return r.ToIArray();
        }

        /// 
        /// Returns an array given a function that generates a tuple from each member. Eager evaluation.
        /// 
        IArray SelectMany(IArray self, Func func)
        {
            var r = new Any[self.Count * 3];
            for (var i = 0; i < self.Count; ++i)
            {
                var tmp = func(self[i]);
                r[i * 3] = tmp.Item1;
                r[i * 3 + 1] = tmp.Item2;
                r[i * 3 + 2] = tmp.Item3;
            }
            return r.ToIArray();
        }

        /// 
        /// Returns an array given a function that generates a tuple from each member. Eager evaluation.
        /// 
        IArray SelectMany(IArray self, Func func)
        {
            var r = new Any[self.Count * 4];
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

        /// 
        /// Returns an array generated by applying a function to corresponding pairs of elements in both arrays.
        /// 
        IArray Zip(IArray self, IArray other, Func f)
            => Select(Math.Min(self.Count, other.Count), i => f(self[i], other[i]));

        /// 
        /// Returns an array generated by applying a function to corresponding pairs of elements in both arrays.
        /// 
        IArray Zip(IArray self, IArray other, Func f)
            => Select(Math.Min(self.Count, other.Count), i => f(self[i], other[i], i));

        /// 
        /// Returns an array generated by applying a function to corresponding pairs of elements in both arrays.
        /// 
        IArray Zip(IArray self, IArray other, IArray other2, Func f)
            => Select(Math.Min(Math.Min(self.Count, other.Count), other2.Count), i => f(self[i], other[i], other2[i]));

        /// 
        /// Returns an array generated by applying a function to corresponding pairs of elements in both arrays.
        /// 
        IArray Zip(IArray self, IArray other, IArray other2, Func f)
            => Select(Math.Min(Math.Min(self.Count, other.Count), other2.Count), i => f(self[i], other[i], other2[i], i));

        /// 
        /// Returns an array generated by applying a function to corresponding pairs of elements in both arrays.
        /// 
        IArray Zip(IArray self, IArray other, IArray other2, IArray other3, Func f)
            => Select(Math.Min(Math.Min(self.Count, other.Count), Math.Min(other2.Count, other3.Count)), i => f(self[i], other[i], other2[i], other3[i]));

        /// 
        /// Returns an array generated by applying a function to corresponding pairs of elements in both arrays.
        /// 
        IArray Zip(IArray self, IArray other, IArray other2, IArray other3, Func f)
            => Select(Math.Min(Math.Min(self.Count, other.Count), Math.Min(other2.Count, other3.Count)), i => f(self[i], other[i], other2[i], other3[i], i));

        /// 
        /// Applies a function to each element in the list paired with the next one.
        /// Used to implement adjacent differences for example.
        /// 
        IArray ZipEachWithNext(IArray self, Func f)
            => self.Zip(self.Skip(), f);

        /// 
        /// Returns an IEnumerable containing only elements of the array for which the function returns true on the index.
        /// An IArray is not created automatically because it is an expensive operation that is potentially unneeded.
        /// 
        IEnumerable WhereIndices(IArray self, Func f)
            => self.Where((x, i) => f(i));

        /// 
        /// Returns an IEnumerable containing only elements of the array for which the corresponding mask is true.
        /// An IArray is not created automatically because it is an expensive operation that is potentially unneeded.
        /// 
        IEnumerable Where(IArray self, IArray mask)
            => self.WhereIndices(mask.ToPredicate());

        /// 
        /// Returns an IEnumerable containing only elements of the array for which the corresponding predicate is true.
        /// 
        IEnumerable Where(IArray self, Func predicate)
            => self.ToEnumerable().Where(predicate);

        /// 
        /// Returns an IEnumerable containing only elements of the array for which the corresponding predicate is true.
        /// 
        IEnumerable Where(IArray self, Func predicate)
            => self.ToEnumerable().Where(predicate);

        /// 
        /// Returns an IEnumerable containing only indices of the array for which the function satisfies a specific predicate.
        /// An IArray is not created automatically because it is an expensive operation that is potentially unneeded.
        /// 
        IEnumerable IndicesWhere(IArray self, Func f)
            => self.Indices().Where(i => f(self[i]));

        /// 
        /// Returns an IEnumerable containing only indices of the array for which the function satisfies a specific predicate.
        /// An IArray is not created automatically because it is an expensive operation that is potentially unneeded.
        /// 
        IEnumerable IndicesWhere(IArray self, Func f)
            => self.IndicesWhere(i => f(self[i], i));

        /// 
        /// Returns an IEnumerable containing only indices of the array for which the function satisfies a specific predicate.
        /// An IArray is not created automatically because it is an expensive operation that is potentially unneeded.
        /// 
        IEnumerable IndicesWhere(IArray self, Func f)
            => self.Indices().Where(i => f(i));

        /// 
        /// Returns an IEnumerable containing only indices of the array for which booleans in the mask are true.
        /// An IArray is not created automatically because it is an expensive operation that is potentially unneeded.
        /// 
        IEnumerable IndicesWhere(IArray self, IArray mask)
            => self.IndicesWhere(mask.ToPredicate());

        /// 
        /// Shortcut for ToEnumerable.Aggregate()
        /// 
        Any Aggregate(IArray self, Any init, Func func)
        {
            for (var i = 0; i < self.Count; ++i)
                init = func(init, self[i]);
            return init;
        }

        /// 
        /// Shortcut for ToEnumerable.Aggregate()
        /// 
        Any Aggregate(IArray self, Func func)
            => Aggregate(self, default, func);

        /// 
        /// Shortcut for ToEnumerable.Aggregate()
        /// 
        Any Aggregate(IArray self, Any init, Func func)
        {
            for (var i = 0; i < self.Count; ++i)
                init = func(init, self[i], i);
            return init;
        }

        /// 
        /// Shortcut for ToEnumerable.Aggregate()
        /// 
        U Aggregate(IArray self, Func func)
            => Aggregate(self, default, func);

        /// 
        /// Returns a new array containing the elements in the range of from to to.
        /// 
        IArray Slice(IArray self, int from, int to)
            => Select(to - from, i => self[i + from]);

        /// 
        /// Returns an array of SubArrays of size "size"
        /// the last items that cannot fill an arrat if size "size" will be ignored
        /// 
        IArray SubArraysFixed(IArray self, int size)
            => (self.Count / size).Select(i => self.SubArray(i, size));


        /// Returns an array of SubArrays of size "size" plus extras
        /// The extra array is of size count % size if present
        IArray SubArrays(IArray self, int size)
            => self.Count % size == 0
                ? self.SubArraysFixed(size)
                : self.SubArraysFixed(size).Append(self.TakeLast(self.Count % size));

        /// 
        /// Returns n elements of the list starting from a given index.
        /// 
        IArray SubArray(IArray self, int from, int count)
            => self.Slice(from, count + from);

        /// 
        /// Returns elements of the array between from and skipping every stride element.
        /// 
        IArray Slice(IArray self, int from, int to, int stride)
            => Select(to - from / stride, i => self[i * stride + from]);

        /// 
        /// Returns a new array containing the elements by taking every nth item.
        /// 
        IArray Stride(IArray self, int n)
            => Select(self.Count / n, i => self[i * n % self.Count]);

        /// 
        /// Returns a new array containing just the first n items.
        /// 
        IArray Take(IArray self, int n)
            => self.Slice(0, n);

        /// 
        /// Returns a new array containing just at most n items.
        /// 
        IArray TakeAtMost(IArray self, int n)
            => self.Count > n ? self.Slice(0, n) : self;

        /// 
        /// Returns a new array containing the elements after the first n elements.
        /// 
        IArray Skip(IArray self, int n = 1)
            => self.Slice(n, self.Count);

        /// 
        /// Returns a new array containing the last n elements.
        /// 
        IArray TakeLast(IArray self, int n = 1)
            => self.Skip(self.Count - n);

        /// 
        /// Returns a new array containing all elements excluding the last n elements.
        /// 
        IArray DropLast(IArray self, int n = 1)
            => self.Count > n ? self.Take(self.Count - n) : self.Empty();

        /// 
        /// Returns a new array by remapping indices
        /// 
        IArray MapIndices(IArray self, Func f)
            => self.Count.Select(i => self[f(i)]);

        /// 
        /// Returns a new array that reverses the order of elements
        /// 
        IArray Reverse(IArray self)
            => self.MapIndices(i => self.Count - 1 - i);

        /// 
        /// Uses the provided indices to select elements from the array.
        /// 
        IArray SelectByIndex(IArray self, IArray indices)
            => indices.Select(i => self[i]);

        /// 
        /// Uses the array as indices to select elements from the other array.
        /// 
        IArray Choose(IArray indices, IArray values)
            => values.SelectByIndex(indices);

        /// 
        /// Given indices of sub-arrays groups, will convert it to arrays of indices (e.g. [0, 2] with a group size of 3 becomes [0, 1, 2, 6, 7, 8])
        /// 
        IArray GroupIndicesToIndices(IArray indices, int groupSize)
            => groupSize == 1
                ? indices : (indices.Count * groupSize).Select(i => indices[i / groupSize] * groupSize + i % groupSize);

        /// 
        /// Return the array separated into a series of groups (similar to DictionaryOfLists)
        /// based on keys created by the given keySelector
        /// 
        IEnumerable> GroupBy(IArray self, Func keySelector)
            => self.ToEnumerable().GroupBy(keySelector);

        /// 
        /// Return the array separated into a series of groups (similar to DictionaryOfLists)
        /// based on keys created by the given keySelector and elements chosen by the element selector
        /// 
        IEnumerable> GroupBy(IArray self, Func keySelector, Func elementSelector)
            => self.ToEnumerable().GroupBy(keySelector, elementSelector);

        /// 
        /// Uses the provided indices to select groups of contiguous elements from the array.
        /// is equivalent to self.SubArrays(groupSize).SelectByIndex(indices).SelectMany();
        /// 
        IArray SelectGroupsByIndex(IArray self, int groupSize, IArray indices)
            => self.SelectByIndex(indices.GroupIndicesToIndices(groupSize));

        /// 
        /// Similar to take, if count is less than the number of items in the array, otherwise uses a modulo operation.
        /// 
        IArray Resize(IArray self, int count)
            => Select(count, i => self[i % self.Count]);

        /// 
        /// Returns an array of the same type with no elements.
        /// 
        IArray Empty(IArray self)
            => self.Take(0);

        /// 
        /// Returns an array of the same type with no elements.
        /// 
        IArray Empty()
            => default(Any).Repeat(0);

        /// 
        /// Returns a sequence of integers from 0 to 1 less than the number of items in the array, representing indicies of the array.
        /// 
        IArray Indices(IArray self)
            => self.Count.Range();

        /// 
        /// Converts an array of elements into a string representation
        /// 
        string Join(IArray self, string sep = " ")
            => self.Aggregate(new StringBuilder(), (sb, x) => sb.Append(x).Append(sep)).ToString();

        /// 
        /// Concatenates the contents of one array with another.
        /// 
        IArray Concatenate(IArray self, IArray other)
            => Select(self.Count + other.Count, i => i < self.Count ? self[i] : other[i - self.Count]);

        /// 
        /// Returns the index of the first element matching the given item.
        /// 
        int IndexOf(IArray self, Any item) where Any : IEquatable
            => self.IndexOf(x => x.Equals(item));

        /// 
        /// Returns the index of the first element matching the given item.
        /// 
        int IndexOf(IArray self, Func predicate)
        {
            for (var i = 0; i < self.Count; ++i)
            {
                if (predicate(self[i]))
                    return i;
            }

            return -1;
        }

        /// 
        /// Returns the index of the last element matching the given item.
        /// 
        int LastIndexOf(IArray self, Any item) where Any : IEquatable
        {
            var n = self.Reverse().IndexOf(item);
            return n < 0 ? n : self.Count - 1 - n;
        }

        /// 
        /// Returns an array that is one element shorter that subtracts each element from its previous one.
        /// 
        IArray AdjacentDifferences(IArray self)
            => self.ZipEachWithNext((a, b) => b - a);

        /// 
        /// Creates a new array that concatenates a unit item list of one item after it.
        /// Repeatedly calling Append would result in significant performance degradation.
        /// 
        IArray Append(IArray self, Any x)
            => (self.Count + 1).Select(i => i < self.Count ? self[i] : x);

        /// 
        /// Creates a new array that concatenates the given items to itself.
        /// 
        IArray Append(IArray self, params Any[] x)
            => self.Concatenate(x.ToIArray());

        /// 
        /// Creates a new array that concatenates a unit item list of one item before it
        /// Repeatedly calling Prepend would result in significant performance degradation.
        /// 
        IArray Prepend(IArray self, Any x)
            => (self.Count + 1).Select(i => i == 0 ? x : self[i - 1]);

        /// 
        /// Returns the element at the nth position, where n is modulo the number of items in the arrays.
        /// 
        Any ElementAt(IArray self, int n)
            => self[n];

        /// 
        /// Returns the element at the nth position, where n is modulo the number of items in the arrays.
        /// 
        Any ElementAtModulo(IArray self, int n)
            => self.ElementAt(n % self.Count);

        /// 
        /// Returns the Nth element of the array, or a default value if out of range/
        /// 
        Any ElementAtOrDefault(IArray xs, int n, Any defaultValue = default)
            => xs != null && n >= 0 && n < xs.Count ? xs[n] : defaultValue;

        /// 
        /// Counts all elements in an array that satisfy a predicate
        /// 
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        int CountWhere(IArray self, Func p)
           => self.Aggregate(0, (n, x) => n + (p(x) ? 1 : 0));

        /// 
        /// Counts all elements in an array that are equal to true
        /// 
        int CountWhere(IArray self)
            => self.CountWhere(x => x);

        /// 
        /// Counts all elements in an array that are equal to a value
        /// 
        int CountWhere(IArray self, Any val) where Any : IEquatable
            => self.CountWhere(x => x.Equals(val));

        /// 
        /// Returns the minimum element in the list
        /// 
        Any Min(IArray self) where Any : IComparable
        {
            if (self.Count == 0) throw new ArgumentOutOfRangeException(nameof(self));
            return self.Aggregate(self[0], (a, b) => a.CompareTo(b) < 0 ? a : b);
        }

        /// 
        /// Returns the maximum element in the list
        /// 
        Any Max(IArray self) where Any : IComparable
        {
            if (self.Count == 0) throw new ArgumentOutOfRangeException(nameof(self));
            return self.Aggregate(self[0], (a, b) => a.CompareTo(b) > 0 ? a : b);
        }

        /// 
        /// Applies a function (like "+") to each element in the series to create an effect similar to partial sums.
        /// 
        IArray Accumulate(IArray self, Func f)
        {
            var n = self.Count;
            var r = new Any[n];
            if (n == 0) return r.ToIArray();
            var prev = r[0] = self[0];
            for (var i = 1; i < n; ++i)
            {
                prev = r[i] = f(prev, self[i]);
            }
            return r.ToIArray();
        }

        /// 
        /// Applies a function (like "+") to each element in the series to create an effect similar to partial sums.
        /// The first value in the array will be zero.
        /// 
        IArray PostAccumulate(IArray self, Func f, Any init = default)
        {
            if (init == null) throw new ArgumentNullException(nameof(init));
            var n = self.Count;
            var r = new Any[n + 1];
            // TODO: doesn't look kosher. Not sure how to fix it. 
            var prev = r[0] = init;
            if (n == 0) return r.ToIArray();
            for (var i = 0; i < n; ++i)
            {
                prev = r[i + 1] = f(prev, self[i]);
            }
            return r.ToIArray();
        }

        /// 
        /// Returns true if the two lists are the same length, and the elements are the same.
        /// 
        bool SequenceEquals(IArray self, IArray other) where Any : IEquatable
            => self == other || (self.Count == other.Count && self.Zip(other, (x, y) => x?.Equals(y) ?? y == null).All(x => x));

        /// 
        /// Creates an array of arrays, split at the given indices
        /// 
        IArray Split(IArray self, IArray indices)
            => indices.Prepend(0).Zip(indices.Append(self.Count), (x, y) => self.Slice(x, y));

        /// 
        /// Creates an array of arrays, split at the given index.
        /// 
        IArray Split(IArray self, int index)
            => Create(self.Take(index), self.Skip(index));

        /// 
        /// Splits an array of tuples into a tuple of array
        /// 
        (IArray, IArray) Unzip(IArray self)
            => (self.Select(pair => pair.Item1), self.Select(pair => pair.Item2));


        /// 
        /// Returns true if the predicate is true for all of the elements in the array
        /// 
        bool All(IArray self, Func predicate)
            => self.ToEnumerable().All(predicate);

        /// 
        /// Returns true if the predicate is true for any of the elements in the array
        /// 
        bool Any(IArray self, Func predicate)
            => self.ToEnumerable().Any(predicate);

        /// 
        /// Sums items in an array using a selector function that returns integers.
        /// 
        long Sum(IArray self, Func func)
            => self.Aggregate(0L, (init, x) => init + func(x));

        /// 
        /// Sums items in an array using a selector function that returns doubles.
        /// 
        double Sum(IArray self, Func func)
            => self.Aggregate(0.0, (init, x) => init + func(x));

        /// 
        /// Forces evaluation (aka reification) of the array by creating a copy in memory.
        /// is useful as a performance optimization, or to force the objects to exist permanently.
        /// 
        IArray Evaluate(IArray x)
            => (x is ArrayAdapter) ? x : x.ToArray().ToIArray();

        /// 
        /// Forces evaluation (aka reification) of the array in parallel.
        /// 
        IArray EvaluateInParallel(IArray x)
            => (x is ArrayAdapter) ? x : x.ToArrayInParallel().ToIArray();

        /// 
        /// Converts to a regular array in paralle;
        /// 
        /// 
        /// 
        /// 
        Any[] ToArrayInParallel(IArray xs)
        {
            if (xs.Count == 0)
                return Array.Empty();

            if (xs.Count < Environment.ProcessorCount)
                return xs.ToArray();

            var r = new Any[xs.Count];
            var partitioner = Partitioner.Create(0, xs.Count, xs.Count / Environment.ProcessorCount);

            Parallel.ForEach(partitioner, (range, state) =>
            {
                for (var i = range.Item1; i < range.Item2; ++i)
                    r[i] = xs[i];
            });
            return r;
        }

        /// 
        /// Maps pairs of elements to a new array.
        /// 
        IArray SelectPairs(IArray xs, Func f)
            => (xs.Count / 2).Select(i => f(xs[i * 2], xs[i * 2 + 1]));

        /// 
        /// Maps every 3 elements to a new array.
        /// 
        IArray SelectTriplets(IArray xs, Func f)
            => (xs.Count / 3).Select(i => f(xs[i * 3], xs[i * 3 + 1], xs[i * 3 + 2]));

        /// 
        /// Maps every 4 elements to a new array.
        /// 
        IArray SelectQuartets(IArray xs, Func f)
            => (xs.Count / 4).Select(i => f(xs[i * 4], xs[i * 4 + 1], xs[i * 4 + 2], xs[i * 4 + 3]));

        /// 
        /// Returns the number of unique instances of elements in the array.
        /// 
        int CountUnique(IArray xs)
            => xs.ToEnumerable().Distinct().Count();

        /// 
        /// Given an array of elements of type Any casts them to a U
        /// 
        IArray Cast(IArray xs) where Any : U
            => xs.Select(x => (U)x);

        /// 
        /// Returns true if the value is present in the array.
        /// 
        bool Contains(IArray xs, Any value)
            => xs.Any(x => x?.Equals(value) ?? false);

        Any FirstOrDefault(IArray xs)
            => xs.Count > 0 ? xs[0] : default;

        Any FirstOrDefault(IArray xs, Any @default)
            => xs.Count > 0 ? xs[0] : @default;

        Any FirstOrDefault(IArray xs, Func predicate)
            => xs.Where(predicate).FirstOrDefault();

        IArray ToLongs(IArray xs)
            => xs.Select(x => (long)x);

        IArray PrefixSums(IArray self)
            => self.ToLongs().PrefixSums();

        IArray PrefixSums(IArray self)
            => self.Scan(0f, (a, b) => a + b);

        IArray PrefixSums(IArray self)
            => self.Scan(0.0, (a, b) => a + b);

        IArray Scan(IArray self, U init, Func scanFunc)
        {
            if (self.Count == 0)
                return Empty();
            var r = new U[self.Count];
            for (var i = 0; i < self.Count; ++i)
                init = r[i] = scanFunc(init, self[i]);
            return r.ToIArray();
        }

        IArray PrefixSums(IArray counts)
            => counts.Scan(0L, (a, b) => a + b);

        // Similar to prefix sums, but starts at zero.
        // r[i] = Sum(count[0 to i])
        IArray CountsToOffsets(IArray counts)
        {
            var r = new int[counts.Count];
            for (var i = 1; i < counts.Count; ++i)
                r[i] = r[i - 1] + counts[i - 1];
            return r.ToIArray();
        }

        IArray OffsetsToCounts(IArray offsets, int last)
            => offsets.Indices().Select(i => i < offsets.Count - 1 ? offsets[i + 1] - offsets[i] : last - offsets[i]);

        IArray SetElementAt(IArray self, int index, Any value)
            => self.SelectIndices(i => i == index ? value : self[i]);

        IArray SetFirstElementWhere(IArray self, Func predicate, Any value)
        {
            var index = self.IndexOf(predicate);
            if (index < 0)
                return self;
            return self.SetElementAt(index, value);
        }
    }
}
