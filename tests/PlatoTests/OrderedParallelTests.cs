using System;
using System.Linq;
using Ara3D.Geometry.Compiler.Utilities;
using NUnit.Framework;

namespace PlatoTests;

[TestFixture]
public class OrderedParallelTests
{
    [Test]
    public void Results_are_in_input_order()
    {
        var items = Enumerable.Range(0, 5000).ToArray();
        Assert.That(OrderedParallel.Map(items, i => i * 2), Is.EqualTo(items.Select(i => i * 2).ToArray()));
    }

    [Test]
    public void One_item_and_no_items_need_no_threads()
    {
        Assert.That(OrderedParallel.Map(new[] { 7 }, i => i + 1), Is.EqualTo(new[] { 8 }));
        Assert.That(OrderedParallel.Map(Array.Empty<int>(), i => i), Is.Empty);
    }

    [Test]
    public void The_earliest_failure_is_the_one_rethrown()
    {
        var items = Enumerable.Range(0, 100).ToArray();
        var e = Assert.Throws<InvalidOperationException>(() => OrderedParallel.Map<int, int>(items,
            i => i % 10 == 3 ? throw new InvalidOperationException(i.ToString()) : i));
        Assert.That(e.Message, Is.EqualTo("3"));
    }

    [Test]
    public void Deep_recursion_fits_the_worker_stack()
    {
        static int Depth(int n) => n == 0 ? 0 : 1 + Depth(n - 1);
        Assert.That(OrderedParallel.Map(new[] { 200000, 200000 }, Depth), Is.EqualTo(new[] { 200000, 200000 }));
    }
}
