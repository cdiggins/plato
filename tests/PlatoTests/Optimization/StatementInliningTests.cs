using Ara3D.Geometry.CSharpWriter;
using NUnit.Framework;
using Integer = Ara3D.Geometry.Integer;

namespace PlatoTests;

[TestFixture]
public static class StatementInliningTests
{
    private static string Fixtures
        => Path.Combine(CheckerTestSupport.FindFolder("PlatoTests"), "Optimization", "StatementInliningFixtures");

    [TestCase(false, false, false, false)]
    [TestCase(true, false, false, false)]
    [TestCase(true, true, false, false)]
    [TestCase(true, true, true, false)]
    [TestCase(true, true, true, true)]
    public static void ClonedLocalsAndUnusedActualsPreserveExecution(bool inline, bool lambdas, bool simplify, bool optimize)
    {
        var compilation = CheckerTestSupport.CompileFolder(Fixtures);
        Assert.That(compilation.SymbolFactory.Errors, Is.Empty,
            string.Join("\n", compilation.SymbolFactory.Errors.Select(e => e.Message)));
        var writer = new CSharpWriter(compilation, "unused-statement")
            { ExtensionStyle = true, InlineStatements = inline, EliminateKnownLambdas = lambdas, SimplifyInlineStatements = simplify, LowerLoops = true, Optimize = optimize };
        writer.WriteAll();
        var source = writer.Files["StatementProbe.g.cs"].ToString();
        if (inline)
        {
            Assert.That(source, Does.Contain("_inline1"), "Fixture must exercise statement expansion.");
            Assert.That(source, Does.Not.Contain(".Loop()").And.Not.Contain(".Twice()").And.Not.Contain(".Unbox()"));
            Assert.That(source, Does.Contain("goto _inline"));
        }
        var assembly = CSharpTestCompilation.CompileAndLoad(new[] { source, writer.Files["_InlineBox.g.cs"].ToString(),
            File.ReadAllText(Path.Combine(Fixtures, "runtime.cs.txt")) });
        if (optimize)
        {
            var tree = Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(source);
            var method = tree.GetRoot().DescendantNodes().OfType<Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax>()
                .Single(m => m.Identifier.Text == "LiteralRegion");
            Assert.That(method.ToString(), Does.Not.Contain(".Map("), "Expansion into a region must retain component/literal-array rewriting.");
        }
        if (optimize)
        {
            var tree = Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(source);
            var deferred = tree.GetRoot().DescendantNodes().OfType<Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax>()
                .Single(m => m.Identifier.Text == "Deferred");
            Assert.That(deferred.ToString(), Does.Not.Contain(".Map("), "A surviving closure must not cancel a valid literal-array rewrite.");
        }
        var library = assembly.GetType("Ara3D.Geometry.StatementProbe", true)!;
        var callback = (Func<Integer, IReadOnlyList<Integer>>)library.GetMethod("Deferred")!.Invoke(null,
            new object[] { (Integer)3 })!;
        Assert.That(callback((Integer)5).Select(n => (int)n), Is.EqualTo(new[] { 4, 6 }));
        Assert.That(((IReadOnlyList<Integer>)library.GetMethod("LiteralRegion")!.Invoke(null,
            new object[] { (Integer)3 })!).Select(n => (int)n), Is.EqualTo(new[] { 4, 4 }));
        Assert.That((int)(Integer)library.GetMethod("AliasEntry")!.Invoke(null,
            new object[] { (Integer)3 })!, Is.EqualTo(4));
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
        foreach (var count in new[] { 0, 1, 3 })
        {
            var calls = assembly.GetType("Ara3D.Geometry.ProbeOperators")!.GetField("Calls")!;
            calls.SetValue(null, 0);
            Assert.That((int)(Integer)library.GetMethod("Countdown")!.Invoke(null,
                new object[] { (Integer)count })!, Is.Zero);
            Assert.That(calls.GetValue(null), Is.EqualTo(count + 1), "Evaluate every loop test, including the terminating test.");
        }
        Assert.That((int)(Integer)library.GetMethod("ProducerStatements")!.Invoke(null,
            new object[] { (Integer)3 })!, Is.EqualTo(6));
        Assert.That(((IReadOnlyList<Integer>)library.GetMethod("BothEntry")!.Invoke(null,
            new object[] { (Integer)3 })!).Select(n => (int)n), Is.EqualTo(new[] { 2, 3, 4 }),
            "Repeated known callbacks need independent lexical identities at each loop stage.");
        Assert.That(((IReadOnlyList<Integer>)library.GetMethod("NestedRanges")!.Invoke(null,
            new object[] { (Integer)3 })!).Select(n => (int)n), Is.EqualTo(new[] { 0, 1, 2 }));
        Assert.That(((IReadOnlyList<Integer>)library.GetMethod("EmptyCallback")!.Invoke(null,
            new object[] { (Integer)0 })!), Is.Empty, "An empty outer loop must not evaluate callback prerequisites.");
        var nestedRanges = TirLoopLowerer.Rewrite(writer.TestGetGroundTir("Integer", "NestedRanges"), writer);
        Assert.That(nestedRanges.AllNodes.OfType<TirLoweredLoop>().Count(), Is.EqualTo(2));
        var producer = TirLoopLowerer.Rewrite(writer.TestGetGroundTir("Integer", "ProducerStatements"), writer);
        Assert.That(producer.AllNodes.OfType<TirLoweredLoop>().Count(), Is.EqualTo(1),
            "A statement-bodied producer must stay fused into the consumer loop.");
        if (inline)
        {
            var countdown = TirInliner.InlineStatements(writer.TestGetGroundTir("Integer", "Countdown"), writer, "Integer", lambdas, simplify);
            Assert.That(countdown.AllNodes.OfType<TirGuardedLoop>(), Is.Not.Empty);
        }
        Assert.That((int)(Integer)library.GetMethod("StatementSum")!.Invoke(null,
            new object[] { (Integer)3 })!, Is.EqualTo(6));
        Assert.That(((IReadOnlyList<Integer>)library.GetMethod("NestedReturns")!.Invoke(null,
            new object[] { (Integer)3 })!).Select(n => (int)n), Is.EqualTo(new[] { 1, 2, 3 }));
        var sum = TirLoopLowerer.Rewrite(writer.TestGetGroundTir("Integer", "StatementSum"), writer);
        Assert.That(sum.AllNodes.OfType<TirLoweredLoop>().Any(l => l.Kind == "Reduce"), Is.True,
            "Statement callbacks must retain reduction loop lowering.");
        Assert.That(((IReadOnlyList<Integer>)library.GetMethod("ArrayCalls")!.Invoke(null,
            new object[] { (Integer)3 })!).Select(n => (int)n), Is.EqualTo(new[] { 6, 6 }));
        var callOrder = Assert.Throws<System.Reflection.TargetInvocationException>(() => library.GetMethod("CallOrder")!.Invoke(null,
            new object[] { (Integer)0, (Func<Integer, Integer, Integer>)((a, b) => a) }));
        Assert.That(callOrder!.InnerException!.Message, Is.EqualTo("first"));
        if (inline)
            foreach (var name in new[] { "ArrayCalls", "BoxCalls" })
            {
                var expanded = TirInliner.InlineStatements(writer.TestGetGroundTir("Integer", name), writer, "Integer", lambdas, simplify);
                Assert.That(expanded.AllNodes.OfType<Ara3D.Geometry.Compiler.Checking.TirCall>().Any(c => c.Name == "Twice"), Is.False);
            }
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
            foreach (var name in new[] { "LambdaEntry", "Nested", "AliasEntry" })
            {
                var original = writer.TestGetGroundTir("Integer", name);
                var expanded = TirInliner.InlineStatements(original, writer, "Integer", true);
                Assert.That(expanded.AllNodes.OfType<Ara3D.Geometry.Compiler.Checking.TirInvoke>(), Is.Empty,
                    $"{name}: {expanded.Body}; " + string.Join("; ", expanded.AllNodes.OfType<Ara3D.Geometry.Compiler.Checking.TirInvoke>()
                        .Select(i => $"target={i.Target.Type}, result={i.Type}, args={string.Join(",", i.Args.Select(a => a.Type))}")));
                Assert.That(expanded.AllNodes.OfType<Ara3D.Geometry.Compiler.Checking.TirLambda>(), Is.Empty, name);
            }
    }
}
