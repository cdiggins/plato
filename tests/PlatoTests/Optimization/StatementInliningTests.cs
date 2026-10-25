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
type InlineBox { Value: Integer; }
library Operators {
    Add(a: Integer, b: Integer): Integer;
    Divide(a: Integer, b: Integer): Integer;
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
}";

    [TestCase(false)]
    [TestCase(true)]
    public static void ClonedLocalsAndUnusedActualsPreserveExecution(bool inline)
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
                { ExtensionStyle = true, InlineStatements = inline };
            writer.WriteAll();
            var source = writer.Files["StatementProbe.g.cs"].ToString();
            if (inline)
                Assert.That(source, Does.Contain("_inline1"), "Fixture must exercise statement expansion.");
            var assembly = CSharpTestCompilation.CompileAndLoad(new[] { source, writer.Files["_InlineBox.g.cs"].ToString(),
                @"global using System.Collections.Generic; namespace Ara3D.Collections { }
namespace Ara3D.Geometry { public static class ProbeOperators {
public static int Calls;
public static Integer Add(this Integer a, Integer b) => (Integer)((int)a + (int)b);
public static Integer Divide(this Integer a, Integer b) => (Integer)((int)a / (int)b);
public static Integer Tick(this Integer x) { Calls++; return x; }
public static Integer First(this Integer x) => throw new System.InvalidOperationException(""first"");
public static Integer Second(this Integer x) => throw new System.InvalidOperationException(""second"");
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
        }
        finally
        {
            Directory.Delete(dir, true);
        }
    }
}
