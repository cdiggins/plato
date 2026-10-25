using Ara3D.Geometry.CSharpWriter;
using NUnit.Framework;
using Integer = Ara3D.Geometry.Integer;

namespace PlatoTests;

[TestFixture]
public static class StatementInliningTests
{
    private const string Source = @"
type Integer { }
type Boolean { }
type Number { }
type Object { }
type Function1<A, R> { }
type Function2<A, B, R> { }
type Array<T> { }
type InlineBox { Value: Integer; }
library Intrinsics {
    Add(a: Integer, b: Integer): Integer;
    Divide(a: Integer, b: Integer): Integer;
    Subtract(a: Integer, b: Integer): Integer;
    LessThan(a: Integer, b: Integer): Boolean;
    GreaterThan(a: Integer, b: Integer): Boolean;
    Equals(a: Integer, b: Integer): Boolean;
    MapRange(n: Integer, f: Function1<Integer, $T>): Array<$T>;
    Count(xs: Array<$T>): Integer;
    Reduce(xs: Array<$T>, seed: $R, f: Function2<$R, $T, $R>): $R;
    Tick(x: Integer): Integer;
    First(x: Integer): Integer;
    Second(x: Integer): Integer;
}
library StatementProbe {
    Twice(x: Integer): Integer { var local = x + x; return local; }
    Ignore(x: Integer, unused: Integer): Integer { var local = x; return local; }
    Entry(x: Integer): Integer { var _inline0 = x.Twice; return _inline0.Twice; }
    Fault(x: Integer): Integer => x.Ignore(1 / x);
    Once(x: Integer): Integer => x.Tick.Twice;
    Order(x: Integer): Integer => x.First.Ignore(x.Second);
    Unbox(x: InlineBox): Integer { var local = x; return local; }
    BoxEntry(x: InlineBox): Integer => x.Unbox;
    Loop(x: Integer): Integer {
        var local = 0;
        while (local < x) {
            if (local > 2) { return local; }
            local = local + 1;
        }
        return local;
    }
    AfterLoop(x: Integer): Integer => x.Loop + 7;
    Guard(flag: Boolean, x: Integer): Integer => flag ? x.Twice : x.First.Ignore(x.Second);
    Sibling(x: Integer): Integer => x.First.Ignore(x.Second.Twice);
    Rec(x: Integer): Integer { var local = x; return local > 0 ? (local - 1).Rec : 0; }
    StartRec(x: Integer): Integer => x.Rec;
    Apply(x: Integer, f: Function1<Integer, Integer>): Integer { var g = f; return g(x); }
    LambdaEntry(x: Integer): Integer => x.Apply(y => { var z = y + x; return z; });
    Nested(x: Integer): Integer => x.Apply(y => y.Apply(z => z + x + y));
    Unknown(x: Integer, f: Function1<Integer, Integer>): Integer => x.Apply(f);
    Mutable(x: Integer): Integer {
        var captured = x;
        var result = x.Apply(y => y + captured);
        captured = captured + 1;
        return result;
    }
    Choose(x: Integer, order: Integer): Integer { var local = order + 0; return local == 0 ? x.Twice : x.First; }
    Simplified(x: Integer): Integer => x.Choose(0);
    Build(n: Integer, f: Function1<Integer, Integer>): Array<Integer> { var values = n.MapRange(f); return values; }
    ScopedRange(n: Integer): Array<Integer> => n.Build(i => i + 1);
    StatementSum(n: Integer): Integer => n.MapRange(i => i + 1).Reduce(0, (sum, value) => { var next = sum + value; return next; });
    NestedReturns(n: Integer): Array<Integer> => n.MapRange(i => { var result = i.Apply(j => { var value = j + 1; return value; }); return result; });
    ComputedRange(n: Integer): Array<Integer> => (n + 1).MapRange(i => i + 1);
    GuardedRange(flag: Boolean, n: Integer): Array<Integer> => flag ? n.MapRange(i => i + 1) : n.First.MapRange(i => i);
    ArrayOrder(n: Integer): Integer => n.First.Ignore(n.MapRange(i => i.Second).Count);
    StatementRange(n: Integer): Array<Integer> => n.Build(i => { var value = i + 1; return value; });
}";

    [TestCase(false, false, false)]
    [TestCase(true, false, false)]
    [TestCase(true, true, false)]
    [TestCase(true, true, true)]
    public static void ClonedLocalsAndUnusedActualsPreserveExecution(bool inline, bool lambdas, bool simplify)
    {
        var dir = Path.Combine(Path.GetTempPath(), "plato-statement-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        try
        {
            File.WriteAllText(Path.Combine(dir, "fixture.plato"), Source);
            var compilation = CheckerTestSupport.CompileFolder(dir);
            Assert.That(compilation.SymbolFactory.Errors, Is.Empty,
                string.Join("\n", compilation.SymbolFactory.Errors.Select(e => e.Message)));
            var writer = new CSharpWriter(compilation, "unused-statement")
                { ExtensionStyle = true, InlineStatements = inline, EliminateKnownLambdas = lambdas, SimplifyInlineStatements = simplify, LowerLoops = true };
            writer.WriteAll();
            var source = writer.Files["StatementProbe.g.cs"].ToString();
            if (inline)
            {
                Assert.That(source, Does.Contain("_inline1"), "Fixture must exercise statement expansion.");
                Assert.That(source, Does.Not.Contain(".Loop()").And.Not.Contain(".Twice()").And.Not.Contain(".Unbox()"));
                Assert.That(source, Does.Contain("goto _inline"));
            }
            var assembly = CSharpTestCompilation.CompileAndLoad(new[] { source, writer.Files["_InlineBox.g.cs"].ToString(),
                @"global using System.Collections.Generic; namespace Ara3D.Collections { }
namespace Ara3D.Geometry { public static class ProbeOperators {
public static int Calls;
public static R Reduce<T,R>(this IReadOnlyList<T> xs, R seed, System.Func<R,T,R> f) {
foreach (var x in xs) seed = f(seed, x);
return seed;
}
public static Integer Count<T>(this IReadOnlyList<T> xs) => (Integer)xs.Count;
public static Integer Add(this Integer a, Integer b) => (Integer)((int)a + (int)b);
public static Integer Divide(this Integer a, Integer b) => (Integer)((int)a / (int)b);
public static Integer Subtract(this Integer a, Integer b) => (Integer)((int)a - (int)b);
public static Boolean LessThan(this Integer a, Integer b) => (Boolean)((int)a < (int)b);
public static Boolean GreaterThan(this Integer a, Integer b) => (Boolean)((int)a > (int)b);
public static Integer Tick(this Integer x) { Calls++; return x; }
public static Integer First(this Integer x) => throw new System.InvalidOperationException(""first"");
public static Integer Second(this Integer x) => throw new System.InvalidOperationException(""second"");
public static IReadOnlyList<T> MapRange<T>(this Integer n, System.Func<Integer, T> f) {
var result = new T[System.Math.Max(0, (int)n)];
for (var i = 0; i < result.Length; i++) result[i] = f((Integer)i);
return result;
}
} }" });
            var library = assembly.GetType("Ara3D.Geometry.StatementProbe", true)!;
            Assert.That((int)(Integer)library.GetMethod("Entry")!.Invoke(null, new object[] { (Integer)7 })!, Is.EqualTo(28));
            var error = Assert.Throws<System.Reflection.TargetInvocationException>(
                () => library.GetMethod("Fault")!.Invoke(null, new object[] { (Integer)0 }));
            Assert.That(error!.InnerException, Is.TypeOf<DivideByZeroException>());
            Assert.That((int)(Integer)library.GetMethod("Once")!.Invoke(null, new object[] { (Integer)5 })!, Is.EqualTo(10));
            Assert.That(assembly.GetType("Ara3D.Geometry.ProbeOperators")!.GetField("Calls")!.GetValue(null), Is.EqualTo(1));
            var ordered = Assert.Throws<System.Reflection.TargetInvocationException>(
                () => library.GetMethod("Order")!.Invoke(null, new object[] { (Integer)0 }));
            Assert.That(ordered!.InnerException!.Message, Is.EqualTo("first"));
            var box = Activator.CreateInstance(assembly.GetType("Ara3D.Geometry.InlineBox")!, (Integer)13)!;
            Assert.That((int)(Integer)library.GetMethod("BoxEntry")!.Invoke(null, new[] { box })!, Is.EqualTo(13));
            foreach (var n in new[] { 0, 1, 5 })
                Assert.That((int)(Integer)library.GetMethod("AfterLoop")!.Invoke(null, new object[] { (Integer)n })!,
                    Is.EqualTo(Math.Min(n, 3) + 7));
            Assert.That((int)(Integer)library.GetMethod("Guard")!.Invoke(null,
                new object[] { (Ara3D.Geometry.Boolean)true, (Integer)9 })!, Is.EqualTo(18));
            var sibling = Assert.Throws<System.Reflection.TargetInvocationException>(
                () => library.GetMethod("Sibling")!.Invoke(null, new object[] { (Integer)0 }));
            Assert.That(sibling!.InnerException!.Message, Is.EqualTo("first"));
            Assert.That((int)(Integer)library.GetMethod("StartRec")!.Invoke(null, new object[] { (Integer)4 })!, Is.Zero);
            Assert.That((int)(Integer)library.GetMethod("LambdaEntry")!.Invoke(null, new object[] { (Integer)4 })!, Is.EqualTo(8));
            Assert.That((int)(Integer)library.GetMethod("Nested")!.Invoke(null, new object[] { (Integer)4 })!, Is.EqualTo(12));
            Assert.That((int)(Integer)library.GetMethod("Unknown")!.Invoke(null,
                new object[] { (Integer)4, (Func<Integer, Integer>)(n => (Integer)((int)n + 1)) })!, Is.EqualTo(5));
            Assert.That((int)(Integer)library.GetMethod("Mutable")!.Invoke(null, new object[] { (Integer)4 })!, Is.EqualTo(8));
            Assert.That((int)(Integer)library.GetMethod("Simplified")!.Invoke(null, new object[] { (Integer)4 })!, Is.EqualTo(8));
            foreach (var count in new[] { 0, 1, 3 })
            {
                Assert.That(((IReadOnlyList<Integer>)library.GetMethod("ScopedRange")!.Invoke(null, new object[] { (Integer)count })!).Select(n => (int)n),
                    Is.EqualTo(Enumerable.Range(1, count)));
                Assert.That(((IReadOnlyList<Integer>)library.GetMethod("StatementRange")!.Invoke(null, new object[] { (Integer)count })!).Select(n => (int)n),
                    Is.EqualTo(Enumerable.Range(1, count)));
            }
            Assert.That((int)(Integer)library.GetMethod("StatementSum")!.Invoke(null,
                new object[] { (Integer)3 })!, Is.EqualTo(6));
            Assert.That(((IReadOnlyList<Integer>)library.GetMethod("NestedReturns")!.Invoke(null,
                new object[] { (Integer)3 })!).Select(n => (int)n), Is.EqualTo(new[] { 1, 2, 3 }));
            var sum = TirLoopLowerer.Rewrite(writer.TestGetGroundTir("Integer", "StatementSum"), writer);
            Assert.That(sum.AllNodes.OfType<TirLoweredLoop>().Any(l => l.Kind == "Reduce"), Is.True,
                "Statement callbacks must retain reduction loop lowering.");
            var computed = TirLoopLowerer.Rewrite(writer.TestGetGroundTir("Integer", "ComputedRange"), writer);
            Assert.That(computed.AllNodes.OfType<TirLoweredLoop>().Count(), Is.EqualTo(1),
                "Turning on statement inlining must retain loops with computed arguments.");
            Assert.That(((IReadOnlyList<Integer>)library.GetMethod("ComputedRange")!.Invoke(null,
                new object[] { (Integer)2 })!).Select(n => (int)n), Is.EqualTo(new[] { 1, 2, 3 }));
            Assert.That(((IReadOnlyList<Integer>)library.GetMethod("GuardedRange")!.Invoke(null,
                new object[] { (Ara3D.Geometry.Boolean)true, (Integer)2 })!).Select(n => (int)n), Is.EqualTo(new[] { 1, 2 }));
            {
                var arrayOrder = Assert.Throws<System.Reflection.TargetInvocationException>(
                    () => library.GetMethod("ArrayOrder")!.Invoke(null, new object[] { (Integer)1 }));
                Assert.That(arrayOrder!.InnerException!.Message, Is.EqualTo("first"),
                    "A later argument's lowered loop cannot execute before an earlier argument.");
            }
            if (lambdas)
            {
                var original = writer.TestGetGroundTir("Integer", "StatementRange");
                var expanded = TirInliner.InlineStatements(original, writer, "Integer", true, simplify);
                var lowered = TirLoopLowerer.Rewrite(expanded, writer);
                Assert.That(lowered.AllNodes.OfType<TirLoweredLoop>(), Is.Not.Empty, "Statement callback must become a loop.");
                Assert.That(source, Does.Contain("_callback"), "Statement callback must emit a scoped result.");
            }
            if (simplify)
            {
                var simplified = TirInliner.InlineStatements(writer.TestGetGroundTir("Integer", "Simplified"), writer, "Integer", true, true);
                Assert.That(simplified.AllNodes.OfType<Ara3D.Geometry.Compiler.Checking.TirConditional>(), Is.Empty);
                Assert.That(simplified.AllNodes.OfType<Ara3D.Geometry.Compiler.Checking.TirCall>().Any(c => c.Name == "First"), Is.False);
                writer.StatementNodeBudget = 1;
                var input = writer.TestGetGroundTir("Integer", "Nested");
                var bounded = TirInliner.InlineStatements(input, writer, "Integer", true, true);
                Assert.That(bounded.AllNodes.Count(), Is.LessThanOrEqualTo(input.AllNodes.Count() + 1));
                writer.StatementNodeBudget = 4096;
            }
            if (lambdas)
            {
                var mutable = TirInliner.InlineStatements(writer.TestGetGroundTir("Integer", "Mutable"), writer, "Integer", true);
                Assert.That(mutable.AllNodes.OfType<Ara3D.Geometry.Compiler.Checking.TirInvoke>(), Is.Not.Empty,
                    "Mutable captures must remain explicit delegate calls.");
            }
            if (lambdas)
                foreach (var name in new[] { "LambdaEntry", "Nested" })
                {
                    var original = writer.TestGetGroundTir("Integer", name);
                    var expanded = TirInliner.InlineStatements(original, writer, "Integer", true);
                    Assert.That(expanded.AllNodes.OfType<Ara3D.Geometry.Compiler.Checking.TirInvoke>(), Is.Empty,
                        $"{name}: {expanded.Body}; " + string.Join("; ", expanded.AllNodes.OfType<Ara3D.Geometry.Compiler.Checking.TirInvoke>()
                            .Select(i => $"target={i.Target.Type}, result={i.Type}, args={string.Join(",", i.Args.Select(a => a.Type))}")));
                    Assert.That(expanded.AllNodes.OfType<Ara3D.Geometry.Compiler.Checking.TirLambda>(), Is.Empty, name);
                }
        }
        finally
        {
            Directory.Delete(dir, true);
        }
    }
}
