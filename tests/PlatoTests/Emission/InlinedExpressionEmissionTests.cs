using System.Reflection;
using Ara3D.Geometry.CSharpWriter;
using NUnit.Framework;
using Boolean = Ara3D.Geometry.Boolean;
using Integer = Ara3D.Geometry.Integer;

namespace PlatoTests;

[TestFixture]
public static class InlinedExpressionEmissionTests
{
    private const string Source = @"
type Integer { }
type Number { }
type Boolean { }
type Object { }
type Function0<R> { }
type Function1<A, R> { }
type Function2<A, B, R> { }
type Function3<A, B, C, R> { }
type Tuple2<A, B> { First: A; Second: B; }
interface IArray<T> { Count(xs: Self): Integer; At(xs: Self, i: Integer): T; }
type Array<T> implements IArray<T> { }
type InlineIndex { Value: Integer; }
type InlinePair { Start: Integer; End: Integer; }
library InlineProbe {
    Pick(flag: Boolean, a: InlineIndex, b: InlineIndex): InlineIndex => flag ? a : b;
    PickArray(flag: Boolean, a: Array<Integer>, b: Array<Integer>): Array<Integer> => flag ? a : b;
    Pair(a: Integer, b: Integer): InlinePair => (a, b);
    ReadIndex(flag: Boolean, xs: Array<Integer>, a: InlineIndex, b: InlineIndex): Integer => xs[flag.Pick(a, b)];
    ReadArray(flag: Boolean, a: Array<Integer>, b: Array<Integer>): Integer => flag.PickArray(a, b)[0];
    ReadCount(flag: Boolean, a: Array<Integer>, b: Array<Integer>): Integer => flag.PickArray(a, b).Count;
    ReadField(a: Integer, b: Integer): Integer => a.Pair(b).End;
}";

    private static Assembly Compile(bool inline, bool loops)
    {
        var dir = Path.Combine(Path.GetTempPath(), "plato-inline-emission-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        try
        {
            File.WriteAllText(Path.Combine(dir, "fixture.plato"), Source);
            var compilation = CheckerTestSupport.CompileFolder(dir);
            Assert.That(compilation.SymbolFactory.Errors, Is.Empty);
            var writer = new CSharpWriter(compilation, "unused-inline-emission")
                { ExtensionStyle = true, InlineCalls = inline, LowerLoops = loops, FuseCombinators = true };
            writer.WriteAll();
            var sources = new[] { "InlineProbe.g.cs", "_InlineIndex.g.cs", "_InlinePair.g.cs" }
                .Select(n => writer.Files[n].ToString()).Prepend("global using System.Collections.Generic; namespace Ara3D.Collections { }").ToArray();
            return CSharpTestCompilation.CompileAndLoad(sources);
        }
        finally
        {
            Directory.Delete(dir, true);
        }
    }

    [TestCase(false, false)]
    [TestCase(true, false)]
    [TestCase(true, true)]
    public static void InlinedReceiversAndIndicesPreserveTheirMeaning(bool inline, bool loops)
    {
        var assembly = Compile(inline, loops);
        var library = assembly.GetType("Ara3D.Geometry.InlineProbe", true)!;
        object Call(string name, params object[] args) => library.GetMethod(name)!.Invoke(null, args)!;
        var index = assembly.GetType("Ara3D.Geometry.InlineIndex", true)!;
        object Index(int n) => Activator.CreateInstance(index, (Integer)n)!;
        IReadOnlyList<Integer> a = new Integer[] { 11, 22 };
        IReadOnlyList<Integer> b = new Integer[] { 33, 44, 55 };
        foreach (var flag in new[] { false, true })
        {
            Assert.That((int)(Integer)Call("ReadIndex", (Boolean)flag, a, Index(0), Index(1)), Is.EqualTo(flag ? 11 : 22));
            Assert.That((int)(Integer)Call("ReadArray", (Boolean)flag, a, b), Is.EqualTo(flag ? 11 : 33));
            Assert.That((int)(Integer)Call("ReadCount", (Boolean)flag, a, b), Is.EqualTo(flag ? 2 : 3));
        }
        Assert.That((int)(Integer)Call("ReadField", (Integer)7, (Integer)9), Is.EqualTo(9));
    }
}

