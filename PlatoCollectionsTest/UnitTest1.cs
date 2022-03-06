using NUnit.Framework;
using Plato;
using System.Collections.Generic;
using System.Linq;

namespace PlatoCollectionsTest;

public class Tests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void Test1()
    {
        var x = 42.Range();
        var y = Enumerable.Range(0, 42);
        Assert.True(Compare(x, y));
    }

    public static bool Compare<T>(ISequence<T> s, IEnumerable<T> e)
        => s.ToEnumerable().SequenceEqual(e);
}

public static class Compatibility
{
    public static IEnumerable<T> ToEnumerable<T>(this ISequence<T> self)
        => self.Generator.ToEnumerable();

    public static IEnumerable<T> ToEnumerable<T>(this IGenerator<T> self)
    {
        while (self.HasValue)
        {
            yield return self.Value;
            self = self.Next;
        }
    }
}
