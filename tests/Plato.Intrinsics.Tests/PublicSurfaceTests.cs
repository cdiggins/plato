using System.Reflection;
using Ara3D.Geometry;
using NUnit.Framework;

namespace Plato.IntrinsicsTests;

/// <summary>
/// A completeness backstop over the shared project's public surface. It does not replace the
/// behavioural fixtures — it catches a type being added, or changing kind, without anyone
/// noticing here.
/// </summary>
[TestFixture]
public static class PublicSurfaceTests
{
    /// <summary>The scalar wrappers. Each is a struct over exactly one readonly field.</summary>
    private static readonly Type[] Wrappers =
    {
        typeof(Number), typeof(Integer), typeof(Ara3D.Geometry.Boolean),
        typeof(Character), typeof(Ara3D.Geometry.String)
    };

    private static IReadOnlyList<Type> PublicRuntimeTypes()
        => typeof(Number).Assembly
            .GetExportedTypes()
            .Where(t => t.Namespace == "Ara3D.Geometry")
            .OrderBy(t => t.Name)
            .ToList();

    [TestCaseSource(nameof(Wrappers))]
    public static void EveryWrapperIsAStructOverOneReadonlyField(Type t)
    {
        Assert.That(t.IsValueType, $"{t.Name} must be a struct");
        var fields = t.GetFields(BindingFlags.Public | BindingFlags.Instance);
        Assert.That(fields, Has.Length.EqualTo(1), $"{t.Name} must wrap exactly one field");
        Assert.That(fields[0].Name, Is.EqualTo("Value"));
        Assert.That(fields[0].IsInitOnly, $"{t.Name}.Value must be readonly");
    }

    [TestCaseSource(nameof(Wrappers))]
    public static void EveryWrapperIsDefaultConstructibleAndEquatableByValue(Type t)
    {
        var a = Activator.CreateInstance(t);
        var b = Activator.CreateInstance(t);
        Assert.That(a, Is.Not.Null);
        Assert.That(a!.Equals(b));
        Assert.That(a.GetHashCode(), Is.EqualTo(b!.GetHashCode()));
    }

    [Test]
    public static void EveryIntrinsicOrExtensionClassIsStatic()
    {
        var helpers = PublicRuntimeTypes()
            .Where(t => t.IsClass && (t.Name.EndsWith("Intrinsics") || t.Name.EndsWith("Extensions")))
            .ToList();
        Assert.That(helpers, Is.Not.Empty);
        foreach (var t in helpers)
            Assert.That(t.IsAbstract && t.IsSealed, $"{t.Name} must be a static class");
    }

    [Test]
    public static void TheBuilderTypesAreSealedClassesNotStructs()
    {
        foreach (var t in new[] { typeof(PlatoList<>), typeof(PlatoBuffer<>), typeof(FrozenArray<>) })
        {
            Assert.That(t.IsClass, $"{t.Name} carries identity and must be a class");
            Assert.That(t.IsSealed, $"{t.Name} must be sealed");
        }
    }

    [Test]
    public static void NoPublicRuntimeTypeExposesAMutableField()
    {
        foreach (var t in PublicRuntimeTypes())
        foreach (var f in t.GetFields(BindingFlags.Public | BindingFlags.Instance))
            Assert.That(f.IsInitOnly, $"{t.Name}.{f.Name} must be readonly");
    }

    [Test]
    public static void TheExpectedRuntimeTypesAreAllPresent()
    {
        var names = PublicRuntimeTypes().Select(t => t.Name).ToList();
        Assert.That(names, Is.SupersetOf(new[]
        {
            "ArrayIntrinsics", "Boolean", "Character", "FrozenArray`1", "GridExtensions",
            "Integer", "Intrinsics", "Number", "PlatoBuffer`1", "PlatoList`1",
            "PlatoListExtensions", "String"
        }));
    }
}
