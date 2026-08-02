using System.Globalization;
using System.Text.Json;
using Ara3D.Geometry;
using NUnit.Framework;
using PlatoBoolean = Ara3D.Geometry.Boolean;
using PlatoString = Ara3D.Geometry.String;

namespace Plato.Generated.Foundation.Tests;

/// <summary>
/// The serialization surface the C# writer synthesizes on every generated struct: real JSON,
/// invariant regardless of the ambient culture, round-trippable through the struct's own
/// Parse/TryParse and through System.Text.Json.
/// </summary>
[TestFixture]
public class JsonSurfaceTests
{
    private static readonly CultureInfo Comma = new CultureInfo("de-DE");

    private static void InCommaCulture(System.Action action)
    {
        var previous = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = Comma;
            action();
        }
        finally
        {
            CultureInfo.CurrentCulture = previous;
        }
    }

    [Test]
    public void ToStringIsJsonObject()
        => Assert.That(new Point3D(1.5f, -2f, 0.25f).ToString(), Is.EqualTo("{\"X\":1.5,\"Y\":-2,\"Z\":0.25}"));

    [Test]
    public void ToStringIsInvariantUnderACommaCulture()
        => InCommaCulture(() =>
        {
            Assert.That(((Number)1.5f).ToString(), Is.EqualTo("1.5"));
            Assert.That(new Point3D(1.5f, 0f, 0f).ToString(), Is.EqualTo("{\"X\":1.5,\"Y\":0,\"Z\":0}"));
        });

    [Test]
    public void NestedStructsNest()
    {
        var pose = new Pose3D(new Point3D(1f, 2f, 3f), Quaternion.Identity());
        var json = pose.ToJson();
        Assert.That(json, Does.StartWith("{\"Position\":{\"X\":1,\"Y\":2,\"Z\":3},\"Orientation\":{"));
        Assert.That(Pose3D.Parse(json), Is.EqualTo(pose));
    }

    [Test]
    public void RoundTripsThroughParse()
    {
        var p = new Point3D(1.5f, -2.25f, 1e-8f);
        Assert.That(Point3D.Parse(p.ToJson()), Is.EqualTo(p));
        Assert.That(Point3D.TryParse(p.ToJson(), out var q), Is.True);
        Assert.That(q, Is.EqualTo(p));
    }

    [Test]
    public void RoundTripsUnderACommaCulture()
        => InCommaCulture(() =>
        {
            var p = new Point3D(1.5f, -2.25f, 0.125f);
            Assert.That(Point3D.Parse(p.ToJson()), Is.EqualTo(p));
        });

    [Test]
    public void ParseToleratesWhitespaceAndUnknownMembers()
        => Assert.That(
            Point3D.Parse(" { \"X\" : 1 , \"W\" : [1,2,{\"a\":\"b\"}] , \"Y\" : 2 , \"Z\" : 3 } "),
            Is.EqualTo(new Point3D(1f, 2f, 3f)));

    [Test]
    public void ParseRejectsMalformedInput()
    {
        Assert.That(Point3D.TryParse("{\"X\":1,\"Y\":2", out _), Is.False);
        Assert.That(Point3D.TryParse("{\"X\":1} trailing", out _), Is.False);
        Assert.That(Point3D.TryParse("[1,2,3]", out _), Is.False);
        Assert.That(Point3D.TryParse((string)null, out _), Is.False);
        Assert.Throws<System.FormatException>(() => Point3D.Parse("nope"));
    }

    [Test]
    public void MissingMembersDefault()
        => Assert.That(Point3D.Parse("{\"X\":1}"), Is.EqualTo(new Point3D(1f, 0f, 0f)));

    [Test]
    public void NonFiniteNumbersRoundTrip()
    {
        var p = new Point3D(float.NaN, float.PositiveInfinity, float.NegativeInfinity);
        Assert.That(p.ToJson(), Is.EqualTo("{\"X\":\"NaN\",\"Y\":\"Infinity\",\"Z\":\"-Infinity\"}"));
        var q = Point3D.Parse(p.ToJson());
        Assert.That(float.IsNaN(q.X), Is.True);
        Assert.That(float.IsPositiveInfinity(q.Y), Is.True);
        Assert.That(float.IsNegativeInfinity(q.Z), Is.True);
    }

    [Test]
    public void ScalarWrappersSerializeAsScalars()
    {
        Assert.That(((Number)1.5f).ToJson(), Is.EqualTo("1.5"));
        Assert.That(((Integer)7).ToJson(), Is.EqualTo("7"));
        Assert.That(((PlatoBoolean)true).ToJson(), Is.EqualTo("true"));
        Assert.That(((Character)'a').ToJson(), Is.EqualTo("\"a\""));
        Assert.That(((PlatoString)"a\"b").ToJson(), Is.EqualTo("\"a\\\"b\""));
    }

    [Test]
    public void ScalarWrappersRoundTrip()
    {
        Assert.That(Number.Parse("1.5"), Is.EqualTo((Number)1.5f));
        Assert.That(Integer.Parse("7"), Is.EqualTo((Integer)7));
        Assert.That(PlatoBoolean.Parse("true"), Is.EqualTo((PlatoBoolean)true));
        Assert.That(Character.Parse("\"a\""), Is.EqualTo((Character)'a'));
        Assert.That(PlatoString.Parse("\"a\\\"b\""), Is.EqualTo((PlatoString)"a\"b"));
    }

    /// <summary>ToString on a string/character wrapper stays the UNQUOTED payload — quoting it
    /// would surface in every interpolation and log line. The IFormattable overload is the JSON
    /// one, and it is the overload nested serialization goes through.</summary>
    [Test]
    public void StringWrapperToStringIsUnquoted()
    {
        Assert.That(((PlatoString)"abc").ToString(), Is.EqualTo("abc"));
        Assert.That(((PlatoString)"abc").ToString(null, null), Is.EqualTo("\"abc\""));
    }

    [Test]
    public void ImplementsTheSystemInterfaces()
    {
        Assert.That(new Point3D(1f, 2f, 3f), Is.InstanceOf<System.IFormattable>());
        Assert.That(new Point3D(1f, 2f, 3f), Is.InstanceOf<System.ISpanFormattable>());
        Assert.That(typeof(System.IParsable<Point3D>).IsAssignableFrom(typeof(Point3D)), Is.True);
        Assert.That(typeof(System.ISpanParsable<Point3D>).IsAssignableFrom(typeof(Point3D)), Is.True);
    }

    [Test]
    public void TryFormatWritesToSpanAndReportsOverflow()
    {
        var p = new Point3D(1f, 2f, 3f);
        var buffer = new char[64];
        Assert.That(p.TryFormat(buffer, out var written, default, null), Is.True);
        Assert.That(new string(buffer, 0, written), Is.EqualTo(p.ToJson()));
        Assert.That(p.TryFormat(new char[2], out var none, default, null), Is.False);
        Assert.That(none, Is.Zero);
    }

    [Test]
    public void IntervalRoundTrips()
    {
        var interval = new NumberInterval(1.5f, 2.5f);
        Assert.That(NumberInterval.Parse(interval.ToJson()), Is.EqualTo(interval));
    }

    /// <summary>A sum type serializes as its honest layout — the Kind discriminant plus the
    /// flattened per-case fields — which is what DataContract already writes, so both round-trip
    /// alike. The old ToString rendered `Move(1, 2)`, which no parser reads back.</summary>
    [Test]
    public void SumTypesRoundTrip()
    {
        var kendall = CorrelationStatistic.Kendall();
        Assert.That(kendall.ToJson(), Is.EqualTo("{\"Kind\":2}"));
        Assert.That(CorrelationStatistic.Parse(kendall.ToJson()), Is.EqualTo(kendall));
        Assert.That(CorrelationStatistic.Parse(kendall.ToJson()).IsKendall().Value, Is.True);
    }

    [Test]
    public void StructsAreReadonly()
        => Assert.That(
            typeof(Point3D).IsDefined(typeof(System.Runtime.CompilerServices.IsReadOnlyAttribute), false),
            Is.True);

    [Test]
    public void CarriesGeneratedCodeAttribute()
        => Assert.That(
            typeof(Point3D).IsDefined(typeof(System.CodeDom.Compiler.GeneratedCodeAttribute), false),
            Is.True);

    // System.Text.Json ignores fields unless they opt in, and cannot assign readonly ones, so the
    // writer emits [JsonInclude] on every field and [JsonConstructor] on the all-fields ctor.
    [Test]
    public void SystemTextJsonRoundTrips()
    {
        var p = new Point3D(1.5f, -2f, 0.25f);
        var json = JsonSerializer.Serialize(p);
        Assert.That(json, Is.EqualTo("{\"X\":1.5,\"Y\":-2,\"Z\":0.25}"));
        Assert.That(JsonSerializer.Deserialize<Point3D>(json), Is.EqualTo(p));
    }

    [Test]
    public void SystemTextJsonAgreesWithTheWriterSurface()
    {
        var pose = new Pose3D(new Point3D(1f, 2f, 3f), Quaternion.Identity());
        Assert.That(JsonSerializer.Serialize(pose), Is.EqualTo(pose.ToJson()));
    }
}
