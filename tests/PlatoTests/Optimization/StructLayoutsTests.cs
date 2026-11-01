// Checks StructLayouts (TKT-1152 C4): the flattened size and the unmanaged test.
using Ara3D.Geometry.Compiler.Symbols;
using Ara3D.Geometry.CSharpWriter;
using NUnit.Framework;

namespace PlatoTests.Optimization;

/// <summary>
/// A type is unmanaged when it is a numeric or boolean scalar, or a generated struct all of whose
/// fields are; an array, string, interface or unbound type variable makes it managed. Sizes follow
/// the cpu64 profile (Number 8 bytes, Integer 4) with natural alignment.
/// </summary>
[TestFixture]
public static class StructLayoutsTests
{
    private static readonly Lazy<(Ara3D.Geometry.Compiler.Compilation Compilation, StructLayouts Layouts)> Setup = new(() =>
    {
        var foundation = Path.Combine(CheckerTestSupport.FindForwardStdLib(), "foundation");
        var fixture = Path.Combine(CheckerTestSupport.FindFolder("PlatoTests"), "Optimization", "StructLayoutsFixtures", "struct-layouts.plato");
        var compilation = new Ara3D.Geometry.Compiler.Compilation(Ara3D.Logging.Logger.Null,
            Directory.GetFiles(foundation, "*.plato").Select(CheckerTestSupport.ParseFile)
                .Append(CheckerTestSupport.ParseFile(fixture)).ToList());
        var writer = new CSharpWriter(compilation, "unused-struct-layouts") { ExtensionStyle = true };
        writer.WriteAll();
        return (compilation, new StructLayouts(writer));
    });

    private static StructLayouts Layouts => Setup.Value.Layouts;

    private static TypeDef Def(string name)
        => Setup.Value.Compilation.ConcreteTypes.First(c => c.TypeDef.Name == name).TypeDef;

    private static TypeExpression Type(string name, params TypeExpression[] args)
        => new(Def(name), args);

    [Test]
    public static void Scalars_AreUnmanaged()
    {
        Assert.That(Layouts.IsUnmanaged(Type("Number")), Is.True);
        Assert.That(Layouts.IsUnmanaged(Type("Integer")), Is.True);
        Assert.That(Layouts.IsUnmanaged(Type("Boolean")), Is.True);
        Assert.That(Layouts.IsUnmanaged(Type("String")), Is.False);
    }

    [Test]
    public static void VectorLikeStruct_IsUnmanaged_WithItsFlattenedSize()
    {
        Assert.That(Layouts.IsUnmanaged(Def("SlVec")), Is.True);
        Assert.That(Layouts.SizeOf(Def("SlVec")), Is.EqualTo(3 * 8));
    }

    [Test]
    public static void NestedStructOfStructs_IsUnmanaged_WithAlignedSize()
    {
        Assert.That(Layouts.IsUnmanaged(Def("SlSegment")), Is.True);
        // Two 24-byte vectors, a bool padded to the 8-byte alignment.
        Assert.That(Layouts.SizeOf(Def("SlSegment")), Is.EqualTo(56));
    }

    [Test]
    public static void StructHoldingAnArray_IsManaged()
    {
        Assert.That(Layouts.IsUnmanaged(Def("SlTable")), Is.False);
        Assert.That(Layouts.IsUnmanaged(Def("SlText")), Is.False);
    }

    [Test]
    public static void StructHoldingAnInterface_IsManaged()
        => Assert.That(Layouts.IsUnmanaged(Def("SlHolder")), Is.False);

    [Test]
    public static void GenericStruct_IsUnmanagedOnlyWhenItsArgumentsAre()
    {
        var number = Type("Number");
        Assert.That(Layouts.IsUnmanaged(Def("SlBox")), Is.False, "unbound type variable");
        Assert.That(Layouts.IsUnmanaged(Type("SlBox", number)), Is.True);
        Assert.That(Layouts.IsUnmanaged(Type("SlBox", Type("SlVec"))), Is.True);
        Assert.That(Layouts.IsUnmanaged(Type("SlBox", Type("SlTable"))), Is.False);
        Assert.That(Layouts.SizeOf(Type("SlBox", Type("SlVec"))), Is.EqualTo(32));
    }
}
