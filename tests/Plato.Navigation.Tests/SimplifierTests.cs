using Ara3D.Geometry.Navigation;
using Ara3D.Utils;
using NUnit.Framework;

namespace Plato.Navigation.Tests;

/// <summary>Each SIM rule proven on a minimal in-memory corpus, plus the negative cases that make
/// the rules safe: a conversion overload with the wrong arity, a constant's own body, a scalar
/// constant that must not be matched everywhere.</summary>
[TestFixture]
public static class SimplifierTests
{
    private const string Types = """
        type Vector2D { X: Number; Y: Number; }
        type Point2D { X: Number; Y: Number; }
        """;

    private const string Constants = """
        library Constants
        {
            UnitX(_: Vector2D): Vector2D
                => Vector2D(1.0, 0.0);

            UnitY(_: Vector2D): Vector2D
                => Vector2D(0.0, 1.0);

            PoundPerTon(_: Number): Number
                => 2000.0;
        }
        """;

    private static ParsedFile Parse(string name, string text)
        => BoundSnapshot.ParseFile(new SourceFile(0, new FilePath(name), text));

    private static List<SimplifyEdit> Check(string text, params (string Name, string Text)[] rest)
        => Simplifier.Check(rest.Select(r => Parse(r.Name, r.Text))
            .Prepend(Parse("subject.library.plato", text)).ToList());

    private static List<SimplifyEdit> CheckWithLibrary(string text)
        => Check(text, ("types.plato", Types), ("constants.library.plato", Constants));

    private static string Simplify(string text)
        => Simplifier.Apply(text, CheckWithLibrary(text));

    [Test]
    public static void ConstructorNameDropsInReturnPosition()
    {
        var text = """
            library Axes2D
            {
                Point2D(self: Axis2D): Point2D
                    => match (self) {
                        X => Point2D(1.0, 0.0);
                        Y => Point2D(0.0, 1.0);
                    };
            }
            """;
        var edits = CheckWithLibrary(text);
        Assert.That(edits.Select(e => e.Code), Is.EqualTo(new[] { "SIM001", "SIM001" }));
        Assert.That(Simplify(text), Does.Contain("X => (1.0, 0.0);").And.Contain("Y => (0.0, 1.0);"));
    }

    [Test]
    public static void NamedConstantBeatsConstructorDrop()
    {
        var text = """
            library Axes2D
            {
                Vector2D(self: Axis2D): Vector2D
                    => match (self) {
                        X => Vector2D(1.0, 0.0);
                        Y => Vector2D(0.0, 1.0);
                    };
            }
            """;
        var edits = CheckWithLibrary(text);
        Assert.That(edits.Select(e => e.Code), Is.EqualTo(new[] { "SIM002", "SIM002" }));
        Assert.That(Simplify(text),
            Does.Contain("X => Vector2D.UnitX;").And.Contain("Y => Vector2D.UnitY;"));
    }

    [Test]
    public static void NamedConstantFiresOutsideReturnPosition()
    {
        var text = """
            library Uses
            {
                Shifted(p: Point2D): Point2D
                    => p.Translate(Vector2D(1.0, 0.0));
            }
            """;
        var edits = CheckWithLibrary(text);
        Assert.That(edits.Select(e => e.Code), Is.EqualTo(new[] { "SIM002" }));
        Assert.That(Simplify(text), Does.Contain("p.Translate(Vector2D.UnitX)"));
    }

    [Test]
    public static void ConstantsAreNotRewrittenIntoThemselves()
        => Assert.That(Simplifier.Check(new[]
            {
                Parse("types.plato", Types),
                Parse("constants.library.plato", Constants)
            }),
            Is.Empty);

    [Test]
    public static void ScalarConstantsAreNotMatched()
    {
        var text = """
            library Weights
            {
                Limit(_: Number): Number
                    => 2000.0;
            }
            """;
        Assert.That(CheckWithLibrary(text), Is.Empty, "a bare scalar body is not a structured constant");
    }

    [Test]
    public static void WrongArityIsNotAConstructor()
    {
        var text = """
            library Conversions
            {
                Vector2D(self: Point2D): Vector2D
                    => Vector2D(self);
            }
            """;
        Assert.That(CheckWithLibrary(text), Is.Empty, "a one-argument conversion is not the field-wise constructor");
    }

    [Test]
    public static void UnknownTypeIsLeftAlone()
    {
        var text = """
            library Foreign
            {
                Widget(x: Number): Widget
                    => Widget(x, x);
            }
            """;
        Assert.That(CheckWithLibrary(text), Is.Empty, "no field count for Widget, so no claim about its constructor");
    }

    [Test]
    public static void ApplyRejectsStaleOffsets()
    {
        var text = """
            library Axes2D
            {
                Point2D(self: Axis2D): Point2D
                    => Point2D(1.0, 0.0);
            }
            """;
        var edits = CheckWithLibrary(text);
        Assert.That(edits, Is.Not.Empty);
        Assert.That(() => Simplifier.Apply(text.Replace("Point2D(1.0, 0.0)", "Point2D(9.0, 9.0)"), edits),
            Throws.ArgumentException);
    }
}
