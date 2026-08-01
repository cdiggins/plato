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
        var fields = t.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.That(fields, Has.Length.EqualTo(1), $"{t.Name} must wrap exactly one field");
        Assert.That(fields[0].IsInitOnly, $"{t.Name}.{fields[0].Name} must be readonly");
        // `Value` is that field on the wrappers over a value type; String normalizes its null
        // default away behind a property over a private field (plato-383), so accept either.
        Assert.That((MemberInfo?)t.GetProperty("Value") ?? t.GetField("Value"), Is.Not.Null,
            $"{t.Name} must expose Value");
    }

    /// <summary>
    /// plato-383: a wrapper's <c>default</c> is its zero value, so every member readable off it
    /// without arguments must answer rather than throw. Reflective on purpose — this is the test
    /// that catches the next wrapper added over a reference type.
    /// </summary>
    [TestCaseSource(nameof(Wrappers))]
    public static void EveryWrapperSurvivesObservationOfItsDefault(Type t)
    {
        var d = Activator.CreateInstance(t);
        const BindingFlags declared =
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly;

        foreach (var p in t.GetProperties(declared).Where(p => p.GetIndexParameters().Length == 0))
            Assert.DoesNotThrow(() => Observe(() => p.GetValue(d)), $"{t.Name}.{p.Name} on default");

        foreach (var m in t.GetMethods(declared)
                     .Where(m => !m.IsSpecialName && !m.IsGenericMethod && m.GetParameters().Length == 0))
            Assert.DoesNotThrow(() => Observe(() => m.Invoke(d, null)), $"{t.Name}.{m.Name}() on default");
    }

    /// <summary>Reads through a reflective call, rethrowing what the member itself threw.</summary>
    private static void Observe(Func<object?> read)
    {
        try
        {
            read();
        }
        catch (TargetInvocationException e) when (e.InnerException != null)
        {
            throw e.InnerException;
        }
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
