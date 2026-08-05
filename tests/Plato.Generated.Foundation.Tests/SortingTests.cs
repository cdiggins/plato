using System;
using System.Collections.Generic;
using System.Linq;
using Ara3D.Geometry;
using NUnit.Framework;

namespace Plato.Generated.Foundation.Tests;

/// <summary>
/// Executes the sorting reference bodies (stdlib/foundation/sorting.library.plato):
/// SortedIndices must be a stable sorted permutation and Sort the array it induces.
/// The law file stdlib/tests/sorting.laws.plato states the same properties, but the
/// forward law runner is still blocked (plato-308), so this suite is sorting's
/// executable gate — the role Plato.Triangulation.Tests plays for the triangulator.
/// </summary>
[TestFixture]
public class SortingTests
{
    private static IReadOnlyList<Number> Numbers(IEnumerable<double> xs)
        => xs.Select(x => (Number)(float)x).ToList();

    private static IReadOnlyList<Number> RandomNumbers(int n, int seed)
    {
        var random = new System.Random(seed);
        return Numbers(Enumerable.Range(0, n).Select(_ => random.NextDouble() * 200 - 100));
    }

    private static readonly int[] Sizes = { 0, 1, 2, 3, 4, 7, 8, 64, 100, 1000 };

    [Test]
    public void Sort_OrdersEverySize()
    {
        foreach (var n in Sizes)
        {
            var xs = RandomNumbers(n, seed: n + 1);
            var sorted = xs.Sort((a, b) => a.Value <= b.Value);
            var expected = xs.Select(x => x.Value).OrderBy(v => v).ToArray();
            Assert.That(sorted.Select(x => x.Value), Is.EqualTo(expected), $"n = {n}");
        }
    }

    [Test]
    public void SortedIndices_IsAPermutation()
    {
        foreach (var n in Sizes)
        {
            var xs = RandomNumbers(n, seed: 2 * n + 5);
            var perm = xs.SortedIndices((a, b) => a.Value <= b.Value);
            Assert.That(perm.Count, Is.EqualTo(n), $"n = {n}");
            Assert.That(perm.Select(i => i.Value).OrderBy(v => v),
                Is.EqualTo(Enumerable.Range(0, n)), $"n = {n}");
        }
    }

    [Test]
    public void SortedIndices_IsStableOnTies()
    {
        // A handful of key buckets guarantees ties; equal keys must keep source order.
        var random = new System.Random(42);
        var keys = Enumerable.Range(0, 500).Select(_ => random.Next(4)).ToArray();
        var xs = keys.Select(k => (Integer)k).ToList() as IReadOnlyList<Integer>;
        var perm = xs.SortedIndices((a, b) => a.Value <= b.Value);
        for (var k = 0; k + 1 < perm.Count; k++)
        {
            var i = perm[k].Value;
            var j = perm[k + 1].Value;
            Assert.That(keys[i], Is.LessThanOrEqualTo(keys[j]));
            if (keys[i] == keys[j])
                Assert.That((int)i, Is.LessThan((int)j), "equal keys must keep source order");
        }
    }

    [Test]
    public void Sort_HandlesPresortedReversedAndConstantInputs()
    {
        var ascending = Numbers(Enumerable.Range(0, 33).Select(i => (double)i));
        Assert.That(ascending.Sort((a, b) => a.Value <= b.Value).Select(x => x.Value),
            Is.EqualTo(ascending.Select(x => x.Value)));

        var descending = Numbers(Enumerable.Range(0, 33).Select(i => (double)-i));
        Assert.That(descending.Sort((a, b) => a.Value <= b.Value).Select(x => x.Value),
            Is.EqualTo(descending.Select(x => x.Value).OrderBy(v => v)));

        var constant = Numbers(Enumerable.Repeat(3.5, 17));
        Assert.That(constant.Sort((a, b) => a.Value <= b.Value).Select(x => x.Value),
            Is.EqualTo(constant.Select(x => x.Value)));
    }

    [Test]
    public void Sort_RespectsTheSuppliedComparer()
    {
        var xs = RandomNumbers(64, seed: 9);
        var descending = xs.Sort((a, b) => a.Value >= b.Value);
        var expected = xs.Select(x => x.Value).OrderByDescending(v => v).ToArray();
        Assert.That(descending.Select(x => x.Value), Is.EqualTo(expected));
    }
}
