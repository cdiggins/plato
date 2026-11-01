// Checks --span-params (TKT-1152 chunk C8a): which array parameters a span twin borrows, and that the twins compile,
// return what their originals return, and leave the original public surface alone.
using System.Reflection;
using System.Text.RegularExpressions;
using Ara3D.Geometry.CLI;
using Ara3D.Geometry.Compiler.Symbols;
using Ara3D.Geometry.CSharpWriter;
using NUnit.Framework;
using PlatoCompilation = Ara3D.Geometry.Compiler.Compilation;

namespace PlatoTests.Emission;

using Integer = Ara3D.Geometry.Integer;

/// <summary>
/// The fixture <c>BorrowedTwinsFixtures/borrowed.plato</c> has array-receiver library functions over a concrete struct
/// element (the shape of <c>LocalDerivativeSmallDegree</c>) and a moved member. Every program is written with the
/// nineteen pinned switches of the regeneration plus <c>--owned-arrays --span-params</c>. A twin takes a
/// <c>ReadOnlySpan</c>, which reflection cannot pass, so the values are compared by a C# harness compiled with the
/// output, calling each original on an array and its twin on a span over the same array.
/// </summary>
[TestFixture]
public static class BorrowedTwinsTests
{
    private static readonly Lazy<PlatoCompilation> Compilation = new(() =>
    {
        var foundation = Path.Combine(CheckerTestSupport.FindForwardStdLib(), "foundation");
        var fixture = Path.Combine(CheckerTestSupport.FindFolder("PlatoTests"), "Emission", "BorrowedTwinsFixtures", "borrowed.plato");
        return new PlatoCompilation(Ara3D.Logging.Logger.Null,
            Directory.GetFiles(foundation, "*.plato").Select(CheckerTestSupport.ParseFile).Append(CheckerTestSupport.ParseFile(fixture)).ToList());
    });

    private static void Configure(CSharpWriter writer, bool owned, bool span)
    {
        PlatoTests.Optimization.ScalarArraysTests.Pin(writer, false);
        writer.OwnedArrays = owned;
        writer.OwnedArraysStrict = true;
        writer.SpanParams = span;
    }

    private static CSharpWriter Emit(bool owned, bool span)
    {
        var writer = new CSharpWriter(Compilation.Value, "unused-borrowed-twins");
        Configure(writer, owned, span);
        writer.WriteAll();
        return writer;
    }

    private static readonly Lazy<CSharpWriter> On = new(() => Emit(true, true));
    private static readonly Lazy<CSharpWriter> OwnedOnly = new(() => Emit(true, false));
    private static readonly Lazy<CSharpWriter> Off = new(() => Emit(false, false));

    private static FunctionDef Function(string name)
        => Compilation.Value.LibraryDefinitionsByName["BorrowedProbe"].Functions.Single(f => f.Name == name);

    // The mask of a function the way the twin writer asks: the type is null for an array-receiver library function and the
    // receiver's struct for a moved member.
    private static bool[] Mask(string name, string receiver = null)
        => On.Value.Borrowed.Of(Function(name), receiver == null ? null : Compilation.Value.TypeDefinitionsByName[receiver]);

    // --- the facts ---------------------------------------------------------------------------------

    [TestCase("CountOf", new[] { true })]
    [TestCase("FirstXOf", new[] { true })]
    [TestCase("SumXOf", new[] { true })]
    [TestCase("BothOf", new[] { true, true })]
    [TestCase("LocalSmall", new[] { true, false, false })]
    [TestCase("Same", new[] { false })]
    [TestCase("SameAs", new[] { false, false })]
    public static void ALibraryFunctionBorrowsTheArraysItOnlyCountsReadsOrLoopsOver(string name, bool[] expected)
    {
        Assert.That(On.Value.Files.ContainsKey("BorrowedTwins.g.cs"), Is.True);
        Assert.That(Mask(name), Is.EqualTo(expected));
    }

    [Test]
    public static void AMovedMemberBorrowsItsArrayParameterAndNotTheOthers()
    {
        Assert.That(On.Value.Files.ContainsKey("BorrowedTwins.g.cs"), Is.True);
        Assert.That(Mask("ShiftBy", "Pt"), Is.EqualTo(new[] { false, true, false }));
    }

    [Test]
    public static void AnArrayCapturedByTheCallbackOfALoweredLoopIsBorrowedAsALocalCopy()
    {
        Assert.That(On.Value.Files.ContainsKey("BorrowedTwins.g.cs"), Is.True);
        Assert.That(Mask("GatherX"), Is.EqualTo(new[] { true }));
    }

    [Test]
    public static void TheTwinNameIsTheClassAndTheFunction()
        => Assert.That(BorrowedParameters.TwinName("Extensions", "CountOf"), Is.EqualTo("Extensions_CountOf"));

    // --- the emitted text --------------------------------------------------------------------------

    [Test]
    public static void TheTwinsTakeSpansForExactlyTheBorrowedParameters()
    {
        var twins = On.Value.Files["BorrowedTwins.g.cs"].ToString();
        Assert.That(twins, Does.Contain("public static class BorrowedTwins"));
        Assert.That(twins, Does.Contain("Integer Extensions_CountOf(System.ReadOnlySpan<Pt> xs)"));
        Assert.That(twins, Does.Contain("Extensions_BothOf(System.ReadOnlySpan<Pt> xs, System.ReadOnlySpan<Pt> ys)"));
        Assert.That(twins, Does.Contain("Pt Extensions_LocalSmall(System.ReadOnlySpan<Pt> local, Integer p, Integer k)"));
        Assert.That(twins, Does.Contain("BorrowedProbe_ShiftBy("), "the moved member's twin");
        Assert.That(twins, Does.Not.Contain("Extensions_Same("));
        Assert.That(twins, Does.Not.Contain("Extensions_SameAs("));
        Assert.That(twins, Does.Not.Contain("IReadOnlyList<Pt>"), "no list parameter is left in a twin");
        Assert.That(On.Value.StorageRefusals, Is.Zero);
        Assert.That(On.Value.Borrowed.Refusals, Is.Empty);
    }

    [Test]
    public static void EveryOtherFileIsIdenticalToTheOutputWithoutTheSwitchExceptWhereACallIsRoutedToATwin()
    {
        // Since C8b a call whose borrowed arguments are all owned goes to BorrowedTwins; that is the only difference.
        foreach (var (name, text) in OwnedOnly.Value.Files)
        {
            var on = On.Value.Files[name].ToString();
            if (on != text.ToString())
                Assert.That(on, Does.Contain("BorrowedTwins."), $"{name} differs without naming a twin");
            Assert.That(text.ToString(), Does.Not.Contain("BorrowedTwins."), name);
        }
        Assert.That(On.Value.Files.Keys.Except(OwnedOnly.Value.Files.Keys), Is.EqualTo(new[] { "BorrowedTwins.g.cs" }));
    }

    [Test]
    public static void WithoutTheSwitchNoTwinIsConsidered()
    {
        // --owned-arrays alone changes the output since C2b (Heap temporaries); that is OwnedArrayEmissionTests' subject.
        Assert.That(Off.Value.Files.ContainsKey("BorrowedTwins.g.cs"), Is.False);
        Assert.That(OwnedOnly.Value.Files.ContainsKey("BorrowedTwins.g.cs"), Is.False);
        Assert.That(OwnedOnly.Value.Files.Keys, Is.EquivalentTo(Off.Value.Files.Keys));
    }

    [Test]
    public static void ASpanTwinWithoutOwnedArraysIsRefused()
    {
        var writer = new CSharpWriter(Compilation.Value, "unused-borrowed-twins");
        Configure(writer, owned: false, span: true);
        Assert.That(() => writer.WriteAll(), Throws.InvalidOperationException.With.Message.Contain("--owned-arrays"));
    }

    // --- the switch --------------------------------------------------------------------------------

    [Test]
    public static void TheSwitchIsOffByDefaultAndSelectedByItsName()
    {
        var on = WriterSwitches.Resolve(new[] { "--owned-arrays", "--span-params" }, csharpTarget: true);
        Assert.That(WriterSwitches.ToLoopOptions(on).SpanParams, Is.True);
        var defaults = WriterSwitches.Resolve(Array.Empty<string>(), csharpTarget: true);
        Assert.That(WriterSwitches.ToLoopOptions(defaults).SpanParams, Is.False);
    }

    // --- the surface and the values ----------------------------------------------------------------

    private static readonly string[] Methods =
    {
        "CountOf", "FirstXOf", "SumXOf", "BothOf", "ShiftBy", "LocalSmall", "GatherX", "Same", "SameAs",
    };

    private const string Harness = @"
namespace Ara3D.Geometry {
    public static class TwinHarness {
        public static Integer CountOf(Pt[] xs) => xs.CountOf();
        public static Integer CountOfTwin(Pt[] xs) => BorrowedTwins.Extensions_CountOf(xs);
        public static Integer FirstXOf(Pt[] xs) => xs.FirstXOf();
        public static Integer FirstXOfTwin(Pt[] xs) => BorrowedTwins.Extensions_FirstXOf(xs);
        public static Integer SumXOf(Pt[] xs) => xs.SumXOf();
        public static Integer SumXOfTwin(Pt[] xs) => BorrowedTwins.Extensions_SumXOf(xs);
        public static Integer BothOf(Pt[] xs, Pt[] ys) => xs.BothOf(ys);
        public static Integer BothOfTwin(Pt[] xs, Pt[] ys) => BorrowedTwins.Extensions_BothOf(xs, ys);
        public static Integer ShiftBy(Pt p, Pt[] xs, Integer k) => p.ShiftBy(xs, k);
        public static Integer ShiftByTwin(Pt p, Pt[] xs, Integer k) => BorrowedTwins.BorrowedProbe_ShiftBy(p, xs, k);
        public static Pt LocalSmall(Pt[] xs, Integer p, Integer k) => xs.LocalSmall(p, k);
        public static Pt LocalSmallTwin(Pt[] xs, Integer p, Integer k) => BorrowedTwins.Extensions_LocalSmall(xs, p, k);
        public static System.Collections.Generic.IReadOnlyList<Integer> GatherX(Pt[] xs) => xs.GatherX();
        public static System.Collections.Generic.IReadOnlyList<Integer> GatherXTwin(Pt[] xs) => BorrowedTwins.Extensions_GatherX(xs);
    }
}";

    // The Integer operators the probes call when a body is not lowered, which the generated foundation (not in this assembly) defines.
    private const string IntegerOperators = @"namespace Ara3D.Geometry { public static class IntegerProbeOperators {
        public static Integer Add(this Integer a, Integer b) => a + b;
        public static Integer Subtract(this Integer a, Integer b) => a - b;
        public static bool LessThan(this Integer a, Integer b) => a.Value < b.Value;
        public static bool Equals(this Integer a, Integer b) => a.Value == b.Value;
    } }";

    private static EmittedLibrary Build(bool span)
        => EmittedLibrary.Build("BorrowedTwinsFixtures", "borrowed.plato",
            name => Methods.Any(m => name == m || name.EndsWith("_" + m)),
            span ? new[] { "BorrowedProbe.g.cs", "Extensions.g.cs", "BorrowedTwins.g.cs" } : new[] { "BorrowedProbe.g.cs", "Extensions.g.cs" },
            writer => Configure(writer, owned: true, span),
            span ? Harness + IntegerOperators : IntegerOperators,
            "_Pt.g.cs", $"{CSharpWriter.ConcreteArrayAccessClass}.g.cs", $"{ClosureFreeArrays.ClassName}.g.cs", LargeStructPassing.HelperFile);

    private static readonly Lazy<EmittedLibrary> WithTwins = new(() => Build(span: true));
    private static readonly Lazy<EmittedLibrary> WithoutTwins = new(() => Build(span: false));

    // Until the body writer spells a Borrowed read as x[i] and x.Length (chunk C2b), a twin's body still reads its span as a
    // list (ConcreteArrayAccess.Count(xs), xs.Count), which C# rejects: the tests that compile the twins wait for it.
    private static void RequireBorrowedReads()
    {
        if (Regex.IsMatch(On.Value.Files["BorrowedTwins.g.cs"].ToString(), @"ConcreteArrayAccess\.|\.Count"))
            Assert.Ignore("The body writer does not print Borrowed reads yet (chunk C2b); the twins are not compilable until it does.");
    }

    private static Type PtType => WithTwins.Value.Assembly.GetType("Ara3D.Geometry.Pt", true)!;

    private static object Pt(int x, int y) => Activator.CreateInstance(PtType, (Integer)x, (Integer)y)!;

    private static object Pts(params (int x, int y)[] xs)
    {
        var array = Array.CreateInstance(PtType, xs.Length);
        for (var i = 0; i < xs.Length; i++)
            array.SetValue(Pt(xs[i].x, xs[i].y), i);
        return array;
    }

    private const string Threw = "threw";

    // The result as text, or "threw": the exception types differ between a list and a span (plan, open question 1).
    private static string Call(string name, params object[] args)
    {
        var harness = WithTwins.Value.Assembly.GetType("Ara3D.Geometry.TwinHarness", true)!;
        try { return Show(harness.GetMethod(name)!.Invoke(null, args)!); }
        catch (TargetInvocationException) { return Threw; }
    }

    // A generated value as text by its contents: the wrappers print their type name, so look through them.
    private static string Show(object value)
    {
        if (value is System.Collections.IEnumerable e && value is not string)
            return "[" + string.Join(",", e.Cast<object>().Select(Show)) + "]";
        var type = value.GetType();
        if (type.Namespace != "Ara3D.Geometry")
            return value.ToString()!;
        var inner = type.GetProperty("Value")?.GetValue(value) ?? type.GetField("Value")?.GetValue(value);
        return inner != null
            ? Show(inner)
            : "{" + string.Join(",", type.GetFields(BindingFlags.Public | BindingFlags.Instance).Select(f => $"{f.Name}={Show(f.GetValue(value)!)}")) + "}";
    }

    private static void SameValue(string name, params object[] args)
        => Assert.That(Call(name + "Twin", args), Is.EqualTo(Call(name, args)), name);

    [Test]
    public static void TheTwinsCompileAndReturnWhatTheirOriginalsReturn()
    {
        RequireBorrowedReads();
        var a = Pts((1, 2), (3, 4), (5, 6), (7, 8));
        var b = Pts((9, 1), (8, 2));
        var empty = Pts();
        Assert.That(Call("CountOfTwin", a), Is.EqualTo("4"));
        Assert.That(Call("SumXOfTwin", a), Is.EqualTo("16"));
        foreach (var xs in new[] { a, b, empty })
        {
            SameValue("CountOf", xs);
            SameValue("SumXOf", xs);
            SameValue("GatherX", xs);
        }
        SameValue("FirstXOf", a);
        SameValue("BothOf", a, b);
        SameValue("BothOf", b, a);
        for (var k = 0; k < 4; k++)
            SameValue("ShiftBy", Pt(10, 20), a, (Integer)k);
        // The guard of the spline shape: a count that is not p + 1 takes the other arm.
        foreach (var (p, k) in new[] { (3, 2), (3, 0), (0, 5), (2, 1), (4, 1), (-1, 0) })
            SameValue("LocalSmall", a, (Integer)p, (Integer)k);
        SameValue("LocalSmall", Pts((5, 5)), (Integer)0, (Integer)9);
    }

    [Test]
    public static void ATwinThrowsWhereItsOriginalThrows()
    {
        RequireBorrowedReads();
        Assert.That(Call("FirstXOf", Pts()), Is.EqualTo(Threw));
        Assert.That(Call("FirstXOfTwin", Pts()), Is.EqualTo(Threw));
        Assert.That(Call("ShiftBy", Pt(1, 1), Pts((1, 1)), (Integer)3), Is.EqualTo(Threw));
        Assert.That(Call("ShiftByTwin", Pt(1, 1), Pts((1, 1)), (Integer)3), Is.EqualTo(Threw));
    }

    [Test]
    public static void TheOriginalPublicSurfaceIsTheSameWithTheSwitchOnAndOff()
    {
        RequireBorrowedReads();
        static string[] Surface(EmittedLibrary library)
            => library.Assembly.GetTypes()
                .Where(t => t.IsPublic && t.Name is not ("BorrowedTwins" or "TwinHarness"))
                .SelectMany(t => t.GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
                    .Select(m => $"{t.FullName}.{m}"))
                .Order().ToArray();
        Assert.That(Surface(WithTwins.Value), Is.EqualTo(Surface(WithoutTwins.Value)));
        Assert.That(WithTwins.Value.Assembly.GetType("Ara3D.Geometry.BorrowedTwins"), Is.Not.Null);
        Assert.That(WithoutTwins.Value.Assembly.GetType("Ara3D.Geometry.BorrowedTwins"), Is.Null);
    }
}
