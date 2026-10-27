// Checks that locals sharing a name in nested scopes emit C# that compiles and keeps its values.
using Ara3D.Geometry;
using NUnit.Framework;

namespace PlatoTests.Emission;

using Integer = Ara3D.Geometry.Integer;
using Number = Ara3D.Geometry.Number;

/// <summary>
/// TKT-1038: C# scopes a local over its whole block, so a local that shares a name with one declared
/// anywhere in an enclosing block is CS0136, whether the inner one comes first (an early-return
/// block) or last. Plato allows the redeclaration, so the writer gives the inner local a distinct
/// name. Sibling scopes and bodies without a clash keep their names.
/// </summary>
[TestFixture]
public static class LocalScopeTests
{
    private static readonly Lazy<EmittedLibrary> Library = new(() => EmittedLibrary.Build(
        "LocalScopeFixtures", "shadow.plato", name => name is "Left" or "Right" or "ShadowEarlyReturn" or "ShadowAfterOuter" or "SiblingScopes" or "ShadowThrice",
        new[] { "LocalScopeProbe.g.cs" }));

    private static Number Call(string name, int p, double a, double b)
        => (Number)Library.Value.Method(name).Invoke(null, new object[] { (Integer)p, (Number)a, (Number)b })!;

    // Arguments are (3, 4): Left picks 3 and Right picks 4.
    [Test]
    public static void InnerLocalBeforeTheOuterOneKeepsItsValue()
    {
        Assert.That((double)Call("ShadowEarlyReturn", 2, 3, 4), Is.EqualTo(4));
        Assert.That((double)Call("ShadowEarlyReturn", 1, 3, 4), Is.EqualTo(3));
    }

    [Test]
    public static void InnerLocalAfterTheOuterOneKeepsItsValue()
    {
        Assert.That((double)Call("ShadowAfterOuter", 2, 3, 4), Is.EqualTo(4));
        Assert.That((double)Call("ShadowAfterOuter", 1, 3, 4), Is.EqualTo(3));
    }

    [Test]
    public static void NestedRedeclarationsKeepTheirValues()
    {
        Assert.That((double)Call("ShadowThrice", 1, 3, 4), Is.EqualTo(4));
        Assert.That((double)Call("ShadowThrice", 0, 3, 4), Is.EqualTo(3));
    }

    [Test]
    public static void SiblingScopesKeepTheirNames()
    {
        var body = Library.Value.Body("SiblingScopes");
        Assert.That(body.Split("var abc").Length - 1, Is.EqualTo(2), body);
        Assert.That(body, Does.Not.Contain("abc_"));
        Assert.That((double)Call("SiblingScopes", 2, 3, 4), Is.EqualTo(3));
        Assert.That((double)Call("SiblingScopes", 3, 3, 4), Is.EqualTo(4));
        Assert.That((double)Call("SiblingScopes", 0, 3, 4), Is.EqualTo(4));
    }
}
