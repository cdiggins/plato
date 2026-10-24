// Verifies component types in generated IArrayLike members.
using Ara3D.Geometry.CSharpWriter;
using NUnit.Framework;

namespace PlatoTests;

[TestFixture]
public static class ArrayLikeCodegenTests
{
    private static string Emit(string declaration, string componentInterface = "interface IArrayLike<T> { }")
    {
        var folder = Path.Combine(Path.GetTempPath(), "plato397-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        try
        {
            File.WriteAllText(Path.Combine(folder, "fixture.plato"), @"
type Number { }
type Boolean { }
type Integer { }
" + componentInterface + "\n" + declaration);
            var compilation = CheckerTestSupport.CompileFolder(folder);
            Assert.That(compilation.SymbolFactory.Errors, Is.Empty);
            var writer = new CSharpWriter(compilation, "unused-arraylike-codegen") { ExtensionStyle = true };
            writer.WriteAll();
            return writer.Files["_Sample.g.cs"].ToString();
        }
        finally
        {
            Directory.Delete(folder, true);
        }
    }

    [TestCase("Boolean")]
    [TestCase("Integer")]
    public static void FieldlessTypeUsesItsDeclaredComponent(string component)
        => AssertFieldless(Emit($"type Sample implements IArrayLike<{component}> {{ }}"), component);

    [Test]
    public static void InheritedGenericInterfaceSubstitutesItsComponent()
        => AssertFieldless(Emit("interface IFlagLike<T> inherits IArrayLike<T> { } type Sample implements IFlagLike<Boolean> { }"), "Boolean");

    [Test]
    public static void InheritedNestedComponentSubstitutesItsTypeArgument()
    {
        var source = Emit("type Box<T> { Value: T; } interface IFlagLike<T> inherits IArrayLike<Box<T>> { } type Sample implements IFlagLike<Boolean> { }");
        Assert.That(source, Does.Contain("IReadOnlyList<Box<Boolean>> Components()"));
        Assert.That(source, Does.Contain("CreateFromComponent(Box<Boolean> x)"));
    }

    [Test]
    public static void InheritedComponentResolvesAReplacementWithTypeArguments()
    {
        var source = Emit("type Box<T> { Value: T; } interface IFlagLike<T> inherits IArrayLike<T> { } interface IWrapped<U> inherits IFlagLike<Box<U>> { } type Sample implements IWrapped<Boolean> { }");
        Assert.That(source, Does.Contain("IReadOnlyList<Box<Boolean>> Components()"));
        Assert.That(source, Does.Contain("CreateFromComponent(Box<Boolean> x)"));
    }

    private static void AssertFieldless(string source, string component)
    {
        Assert.That(source, Does.Contain($"IReadOnlyList<{component}> Components()"));
        Assert.That(source, Does.Contain($"CreateFromComponent({component} x)"));
        var assembly = CSharpTestCompilation.CompileAndLoad(
            "global using System; global using System.Collections.Generic; namespace Ara3D.Collections { } namespace Ara3D.Geometry { public interface IArrayLike<T> { } public interface IFlagLike<T> : IArrayLike<T> { } }",
            source);
        var sample = assembly.GetType("Ara3D.Geometry.Sample", true)!;
        var value = Activator.CreateInstance(sample);
        Assert.That((int)(Ara3D.Geometry.Integer)sample.GetMethod("NumComponents")!.Invoke(value, null)!, Is.Zero);
    }

    [Test]
    public static void MissingComponentNamesTheAffectedType()
        => Assert.That(() => Emit("type Sample implements IArrayLike { }", "interface IArrayLike { }"),
            Throws.TypeOf<InvalidOperationException>().With.Message.Contains("IArrayLike component type for Sample"));

    [Test]
    public static void FieldfulTypeKeepsItsComponentAndValues()
    {
        var source = Emit("type Sample implements IArrayLike<Boolean> { A: Boolean; B: Boolean; }");
        Assert.That(source, Does.Contain("IReadOnlyList<Boolean> Components()"));
        Assert.That(source, Does.Contain("Intrinsics.MakeArray<Boolean>(A, B)"));
        Assert.That(source, Does.Contain("CreateFromComponent(Boolean x) => new Sample(x, x)"));
    }
}
