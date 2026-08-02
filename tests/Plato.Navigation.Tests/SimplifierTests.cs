using Ara3D.Geometry.Navigation;
using Ara3D.Utils;
using NUnit.Framework;

namespace Plato.Navigation.Tests;

/// <summary>Each SIM rule proven on a minimal in-memory corpus, plus the negative cases that make
/// the rules safe: a conversion overload with the wrong arity, a constant's own body, a scalar
/// constant that must not be matched everywhere, and — the plato-404 regression — a constructor call
/// that is a branch rather than the result.</summary>
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
    public static void ConstructorNameDropsInResultPosition()
    {
        var text = """
            library Axes2D
            {
                Point2D(self: Axis2D): Point2D
                    => Point2D(0.0, 0.0);
            }
            """;
        var edits = CheckWithLibrary(text);
        Assert.That(edits.Select(e => e.Code), Is.EqualTo(new[] { "SIM001" }));
        Assert.That(Simplify(text), Does.Contain("=> (0.0, 0.0);"));
    }

    [Test]
    public static void ConstructorNameDropsInReturnStatement()
    {
        var text = """
            library Axes2D
            {
                Shifted(p: Point2D): Point2D
                {
                    var dx = 1.0;
                    return Point2D(p.X + dx, p.Y);
                }
            }
            """;
        var edits = CheckWithLibrary(text);
        Assert.That(edits.Select(e => e.Code), Is.EqualTo(new[] { "SIM001" }));
        Assert.That(Simplify(text), Does.Contain("return (p.X + dx, p.Y);"));
    }

    /// <summary>plato-404: a match arm is unified with its sibling arms, not with the declared
    /// return type, so a bare tuple there stops unifying with the named type the others carry.</summary>
    [Test]
    public static void ConstructorNameStaysInMatchArms()
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
        Assert.That(CheckWithLibrary(text), Is.Empty, "a match arm has no declared type to fall back on");
    }

    /// <summary>plato-404, the `ColorAtParameter` / `CanonicalAxis` shape: the branches of a
    /// conditional unify with each other first.</summary>
    [Test]
    public static void ConstructorNameStaysInConditionalBranches()
    {
        var text = """
            library Axes2D
            {
                Fallback(p: Point2D, empty: Boolean): Point2D
                    => empty ? Point2D(0.0, 0.0) : p;
            }
            """;
        Assert.That(CheckWithLibrary(text), Is.Empty, "a conditional branch has no declared type to fall back on");
    }

    [Test]
    public static void ConstructorNameStaysInArgumentPosition()
    {
        var text = """
            library Axes2D
            {
                Shifted(p: Point2D): Point2D
                    => p.Translate(Point2D(2.0, 3.0));
            }
            """;
        Assert.That(CheckWithLibrary(text), Is.Empty, "an argument is checked against the parameter, not the result");
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

/// <summary>SIM003 and SIM004, the duplicated-body rules (plato-407), on the shapes that were found
/// by hand: the IIndex accessors SdfNodeIndex spelled out for itself, and the families of one-line
/// bodies (Width over the rasters, Centroid over the centre-symmetric shapes) whose receivers shared
/// an interface nobody had used yet.</summary>
[TestFixture]
public static class SimplifierDuplicateBodyTests
{
    private const string Concepts = """
        interface IIndex
        {
            Value(x: Self): Integer;
        }

        interface IImage
        {
            Size(x: Self): IntegerSize2D;
        }

        interface ICentroid2D
        {
            Centroid(x: Self): Point2D;
        }
        """;

    private const string Indices = """
        library Collections
        {
            // The digest of a typed index.
            Hash(self: IIndex): Integer
                => self.Value.Hash;

            // The natural total order on typed indices.
            LessThanOrEquals(a: IIndex, b: IIndex): Boolean
                => a.Value <= b.Value;
        }
        """;

    private static ParsedFile Parse(string name, string text)
        => BoundSnapshot.ParseFile(new SourceFile(0, new FilePath(name), text));

    private static List<SimplifyEdit> Check(params (string Name, string Text)[] files)
        => Simplifier.Check(files.Select(f => Parse(f.Name, f.Text)).ToList());

    /// <summary>The `1ae4ecc` case: `Hash(self: SdfNodeIndex)` was byte-identical to the derived
    /// `Hash(self: IIndex)`, and `LessThanOrEquals` matches even though its second parameter is the
    /// concrete type where the interface version spells the interface.</summary>
    [Test]
    public static void InterfaceDerivedBodyMakesTheConcreteCopyRemovable()
    {
        var types = """
            type SdfNodeIndex
                implements IIndex
            {
                Value: Integer;
            }
            """;
        var subject = """
            library FieldsImplicits
            {
                // A node index hashes as its underlying position.
                Hash(self: SdfNodeIndex): Integer
                    => self.Value.Hash;

                // Node indices order by position in the node array.
                LessThanOrEquals(a: SdfNodeIndex, b: SdfNodeIndex): Boolean
                    => a.Value <= b.Value;
            }
            """;
        var edits = Check(("fields.library.plato", subject), ("fields.types.plato", types),
            ("collections.concepts.plato", Concepts), ("collections.library.plato", Indices));

        Assert.That(edits.Select(e => e.Code), Is.EqualTo(new[] { "SIM003", "SIM003" }));
        Assert.That(edits[0].Message, Does.Contain("IIndex").And.Contain("SdfNodeIndex"));

        var applied = Simplifier.Apply(subject, edits);
        Assert.Multiple(() =>
        {
            Assert.That(applied, Does.Not.Contain("SdfNodeIndex"), "both concrete copies and their comments go");
            Assert.That(applied, Does.Contain("library FieldsImplicits"));
        });
    }

    [Test]
    public static void ADifferentBodyIsNotDerived()
    {
        var types = """
            type SdfNodeIndex
                implements IIndex
            {
                Value: Integer;
            }
            """;
        var subject = """
            library FieldsImplicits
            {
                Hash(self: SdfNodeIndex): Integer
                    => self.Value.Hash + 1;
            }
            """;
        Assert.That(Check(("fields.library.plato", subject), ("fields.types.plato", types),
                ("collections.concepts.plato", Concepts), ("collections.library.plato", Indices)),
            Is.Empty, "the interface derives a different value");
    }

    [Test]
    public static void ATypeThatDoesNotImplementTheInterfaceKeepsItsBody()
    {
        var types = """
            type SdfNodeIndex
            {
                Value: Integer;
            }
            """;
        var subject = """
            library FieldsImplicits
            {
                Hash(self: SdfNodeIndex): Integer
                    => self.Value.Hash;
            }
            """;
        Assert.That(Check(("fields.library.plato", subject), ("fields.types.plato", types),
                ("collections.concepts.plato", Concepts), ("collections.library.plato", Indices)),
            Is.Empty, "nothing derives Hash for a type outside IIndex");
    }

    /// <summary>The `1ae4ecc` Width family: seven rasters, one body, one interface they all carry.
    /// Reported, never applied — deriving it is a vocabulary decision.</summary>
    [Test]
    public static void SharedInterfaceFamilyIsReportedNotApplied()
    {
        var types = """
            type Bitmap implements IImage { Size: IntegerSize2D; }
            type FloatImage implements IImage { Size: IntegerSize2D; }
            type DepthImage implements IImage { Size: IntegerSize2D; }
            """;
        var subject = """
            library Images
            {
                Width(self: Bitmap): Integer
                    => self.Size.Width;

                Width(self: FloatImage): Integer
                    => self.Size.Width;

                Width(self: DepthImage): Integer
                    => self.Size.Width;
            }
            """;
        var edits = Check(("images.library.plato", subject), ("images.types.plato", types),
            ("images.concepts.plato", Concepts));

        Assert.That(edits.Select(e => e.Code), Is.EqualTo(new[] { "SIM004" }));
        Assert.Multiple(() =>
        {
            Assert.That(edits[0].Applicable, Is.False);
            Assert.That(edits[0].Message, Does.Contain("IImage").And.Contain("Bitmap").And.Contain("3 types"));
            Assert.That(Simplifier.Apply(subject, edits), Is.EqualTo(subject), "SIM004 never rewrites");
        });
    }

    /// <summary>The `f5fc7ca` Centroid family: the receivers spell the parameter differently, which
    /// is exactly the "modulo the receiver's name" the rule has to see through.</summary>
    [Test]
    public static void ReceiverNamesDoNotSplitAFamily()
    {
        var types = """
            type Circle implements ICentroid2D { Center: Point2D; }
            type Annulus implements ICentroid2D { Center: Point2D; }
            type RegularPolygon implements ICentroid2D { Center: Point2D; }
            """;
        var subject = """
            library Planar
            {
                Centroid(c: Circle): Point2D
                    => c.Center;

                Centroid(a: Annulus): Point2D
                    => a.Center;

                Centroid(g: RegularPolygon): Point2D
                    => g.Center;
            }
            """;
        var edits = Check(("planar.library.plato", subject), ("planar.types.plato", types),
            ("geometry.concepts.plato", Concepts));

        Assert.That(edits.Select(e => e.Code), Is.EqualTo(new[] { "SIM004" }));
        Assert.That(edits[0].Message, Does.Contain("ICentroid2D"));
    }

    [Test]
    public static void ASmallFamilyIsNotReported()
    {
        var types = """
            type Bitmap implements IImage { Size: IntegerSize2D; }
            type FloatImage implements IImage { Size: IntegerSize2D; }
            """;
        var subject = """
            library Images
            {
                Width(self: Bitmap): Integer
                    => self.Size.Width;

                Width(self: FloatImage): Integer
                    => self.Size.Width;
            }
            """;
        Assert.That(Check(("images.library.plato", subject), ("images.types.plato", types),
                ("images.concepts.plato", Concepts)),
            Is.Empty, $"two identical bodies are below the family size of {Simplifier.FamilySize}");
    }

    [Test]
    public static void AFamilyWithNoSharedInterfaceIsNotReported()
    {
        var types = """
            type Bitmap implements IImage { Size: IntegerSize2D; }
            type FloatImage { Size: IntegerSize2D; }
            type DepthImage { Size: IntegerSize2D; }
            """;
        var subject = """
            library Images
            {
                Width(self: Bitmap): Integer
                    => self.Size.Width;

                Width(self: FloatImage): Integer
                    => self.Size.Width;

                Width(self: DepthImage): Integer
                    => self.Size.Width;
            }
            """;
        Assert.That(Check(("images.library.plato", subject), ("images.types.plato", types),
                ("images.concepts.plato", Concepts)),
            Is.Empty, "with no interface in common there is nothing to derive the body on");
    }
}
