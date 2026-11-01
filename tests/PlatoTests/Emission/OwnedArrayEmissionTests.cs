// Pins --owned-arrays for lowered-loop temporaries (TKT-1152 C2b): Heap storage where consumers read the array,
// the interface where they do not, and the same values either way.
using System.Reflection;
using System.Text.RegularExpressions;
using Ara3D.Geometry.CSharpWriter;
using NUnit.Framework;

namespace PlatoTests.Emission;

using Integer = Ara3D.Geometry.Integer;

/// <summary>
/// The fixture <c>OwnedArrayFixtures/owned.plato</c> has one function per place a lowered-loop temporary can sit: the
/// shapes of TKT-505 (a Reduce seed, a FlatMap source, a conditional join, a match arm, a generic argument, a return,
/// a real lambda's capture, a loop-carried variable) and the places that read the array (a Zip's sources, MapPairs,
/// Count, At). It is emitted under the nineteen pinned flags with and without <c>--owned-arrays</c> (strict: a checker
/// violation anywhere in the foundation or the fixture throws), compiled with Roslyn and run; each shape gives the
/// value of the immutable meaning in both builds, and the text shows the array where the temporary is owned.
/// </summary>
[TestFixture]
public static class OwnedArrayEmissionTests
{
    private const string IntegerOperators = @"namespace Ara3D.Geometry { public static class IntegerProbeOperators {
        public static Integer Add(this Integer a, Integer b) => a + b;
        public static Integer Subtract(this Integer a, Integer b) => a - b;
        public static Integer Multiply(this Integer a, Integer b) => a * b;
        public static Integer Modulo(this Integer a, Integer b) => a % b;
        public static bool LessThan(this Integer a, Integer b) => a.Value < b.Value;
        public static bool GreaterThan(this Integer a, Integer b) => a.Value > b.Value;
        public static bool LessThanOrEquals(this Integer a, Integer b) => a.Value <= b.Value;
        public static bool GreaterThanOrEquals(this Integer a, Integer b) => a.Value >= b.Value;
        public static bool Equals(this Integer a, Integer b) => a.Value == b.Value;
    } }";

    private static readonly string[] Methods =
    {
        "Digest", "Zipped", "Paired", "ReadTwice", "DirectReads", "ReadAt", "ReduceSeed", "FlatMapSource", "Join", "Pick",
        "Identity", "GenericArgument", "Returned", "Probe", "Captured", "MadeInClosure", "Carried", "PerPass",
    };
    private static readonly string[] Files = { "OwnedProbe.g.cs", "Extensions.g.cs" };

    /// <summary>The nineteen pinned flags of the regeneration, plus the storage layer when asked.</summary>
    internal static void Pin(CSharpWriter writer, bool ownedArrays)
    {
        (writer.LowerLoops, writer.FuseCombinators, writer.FuseConsumers, writer.ExactOutputAllocation) = (true, true, true, true);
        (writer.LeanInlining, writer.ShareCalls, writer.LowerScalars, writer.ConcreteArrays, writer.Presize) = (true, true, true, true, true);
        (writer.InPlaceUpdates, writer.SinkBindings, writer.BindScrutinees, writer.ShareCallsWide) = (true, true, true, true);
        (writer.ClosureFree, writer.InParameters, writer.SwitchMatch, writer.HoistInvariants) = (true, true, true, true);
        (writer.CacheConstants, writer.PrimitiveCompare, writer.LowerMore, writer.SpecializeCallbacks) = (true, true, true, true);
        (writer.OwnedArrays, writer.OwnedArraysStrict) = (ownedArrays, ownedArrays);
    }

    internal static EmittedLibrary Build(bool ownedArrays)
        => EmittedLibrary.Build("OwnedArrayFixtures", "owned.plato", name => Methods.Contains(name), Files,
            writer => Pin(writer, ownedArrays), IntegerOperators,
            $"{CSharpWriter.ConcreteArrayAccessClass}.g.cs", "_OwnedShape.g.cs");

    private static readonly Lazy<EmittedLibrary> Off = new(() => Build(false));
    private static readonly Lazy<EmittedLibrary> On = new(() => Build(true));

    private static object Call(EmittedLibrary library, string name, params object[] args)
        => library.Method(name).Invoke(null, args)!;

    private static object Shape(EmittedLibrary library, string kind, params object[] args)
        => library.Assembly.GetType("Ara3D.Geometry.OwnedShape", true)!.GetMethod(kind)!.Invoke(null, args)!;

    private static string Show(object value)
        => value is IEnumerable<Integer> xs ? "[" + string.Join(",", xs.Select(x => x.Value)) + "]" : ((Integer)value).Value.ToString();

    private static readonly IReadOnlyList<Integer> Nine = new Integer[] { 9 };
    private static readonly IReadOnlyList<Integer> NineEight = new Integer[] { 9, 8 };

    // Each shape and its value under the immutable meaning. Both builds must give it.
    private static IEnumerable<TestCaseData> Shapes()
    {
        yield return new TestCaseData("Zipped", new object[] { (Integer)4 }, "[0,4,12,24]").SetName("Zipped: two owned sources");
        yield return new TestCaseData("Paired", new object[] { (Integer)4 }, "[1,5]").SetName("Paired: one owned source");
        yield return new TestCaseData("ReadTwice", new object[] { (Integer)6 }, "36").SetName("ReadTwice: a let-bound temporary");
        yield return new TestCaseData("DirectReads", new object[] { (Integer)4 }, "7").SetName("DirectReads: At and Count");
        yield return new TestCaseData("ReadAt", new object[] { (Integer)3, (Integer)2 }, "3").SetName("ReadAt: an element");
        yield return new TestCaseData("ReduceSeed", new object[] { (Integer)3 }, "[3,5,7]").SetName("ReduceSeed: a Reduce seed");
        yield return new TestCaseData("FlatMapSource", new object[] { (Integer)5 }, "[0,1,2,3,6,7,12,13,20,21]").SetName("FlatMapSource: a FlatMap source");
        yield return new TestCaseData("Join", new object[] { (Integer)4, Nine }, "[10,11,12,13]").SetName("Join: the owned arm");
        yield return new TestCaseData("Join", new object[] { (Integer)1, NineEight }, "[9,8]").SetName("Join: the parameter arm");
        yield return new TestCaseData("GenericArgument", new object[] { (Integer)3 }, "[3,4,5]").SetName("GenericArgument: a generic argument");
        yield return new TestCaseData("Returned", new object[] { (Integer)4 }, "[0,1,4,9]").SetName("Returned: a return");
        yield return new TestCaseData("Captured", new object[] { (Integer)4 }, "5").SetName("Captured: a real lambda capture");
        yield return new TestCaseData("MadeInClosure", new object[] { (Integer)4 }, "4").SetName("MadeInClosure: made inside a lambda");
        yield return new TestCaseData("Carried", new object[] { (Integer)4 }, "[3,4,5,6]").SetName("Carried: assigned in a loop");
        yield return new TestCaseData("PerPass", new object[] { (Integer)3 }, "12").SetName("PerPass: declared in a loop");
    }

    [TestCaseSource(nameof(Shapes))]
    public static void EachShapeCompilesAndGivesTheFlagOffValue(string name, object[] args, string expected)
    {
        Assert.That(Show(Call(Off.Value, name, args)), Is.EqualTo(expected), "flag off");
        Assert.That(Show(Call(On.Value, name, args)), Is.EqualTo(expected), "flag on");
    }

    [TestCase("Short", "[0,1,2,3]", 4)]
    [TestCase("Wide", "[0,2,4,6]", 4)]
    [TestCase("Plain", "[100,101]", 2)]
    public static void EachArmOfAMatchGivesTheFlagOffValue(string kind, string expected, int n)
    {
        foreach (var library in new[] { Off.Value, On.Value })
        {
            var shape = Shape(library, kind, (Integer)(kind == "Plain" ? 100 : 2));
            Assert.That(Show(Call(library, "Pick", shape, (Integer)n)), Is.EqualTo(expected));
        }
    }

    [Test]
    public static void ReadingPastTheEndFaultsWithTheTypeOfTheStorage()
    {
        var off = Assert.Throws<TargetInvocationException>(() => Call(Off.Value, "ReadAt", (Integer)3, (Integer)5))!;
        var on = Assert.Throws<TargetInvocationException>(() => Call(On.Value, "ReadAt", (Integer)3, (Integer)5))!;
        Assert.That(off.InnerException, Is.TypeOf<ArgumentOutOfRangeException>(), "the interface throws what it always threw");
        Assert.That(on.InnerException, Is.TypeOf<IndexOutOfRangeException>(), "the array throws its own (plan open question 1)");
    }

    private static readonly Regex Declaration = new(@"\b(?:IReadOnlyList<Integer>|Integer\[\]) _lp\d+ = _a\d+;");
    private static readonly Regex HeapDeclaration = new(@"\bInteger\[\] _lp\d+ = _a\d+;");

    [Test]
    public static void FlagOffDeclaresEveryTemporaryAsTheInterface()
    {
        Assert.That(HeapDeclaration.Matches(Off.Value.Text), Is.Empty);
        Assert.That(Declaration.Matches(Off.Value.Text), Has.Count.GreaterThan(15));
        Assert.That(Off.Value.Body("Zipped") + Off.Value.Body("DirectReads"), Does.Not.Contain(".Length"));
    }

    [Test]
    public static void FlagOnDeclaresEveryArrayFillingTemporaryAsAnArray()
    {
        // Every temporary of this fixture is filled by a Map, MapRange, Zip or MapPairs loop, declared once and
        // never updated, so each interface declaration of the flag-off build is an array with the flag on.
        Assert.That(HeapDeclaration.Matches(On.Value.Text), Has.Count.EqualTo(Declaration.Matches(Off.Value.Text).Count));
        Assert.That(Regex.Matches(On.Value.Text, @"IReadOnlyList<Integer> _lp\d+ = _a"), Is.Empty);
        // A FlatMap result is not an array-filling kind: it keeps the interface, as in the flag-off build.
        Assert.That(Regex.Matches(On.Value.Text, @"IReadOnlyList<Integer> _lp\d+;").Count,
            Is.EqualTo(Regex.Matches(Off.Value.Text, @"IReadOnlyList<Integer> _lp\d+;").Count).And.GreaterThan(0));
    }

    [Test]
    public static void ASourceOfALoopIsReadAsAnArrayWithNoTypeTest()
    {
        var zipped = On.Value.Body("Zipped");
        Assert.That(zipped, Does.Contain("Integer[] _eval2 = _lp0;").And.Contain("Integer[] _eval3 = _lp1;"), zipped);
        Assert.That(zipped, Does.Contain("_s2a.Length").And.Contain("_s2b[_i2]"), zipped);
        Assert.That(zipped, Does.Not.Contain("ConcreteArrayAccess"), zipped);
        Assert.That(Off.Value.Body("Zipped"), Does.Contain("ConcreteArrayAccess.At(_s2a, _i2)"));
    }

    [Test]
    public static void ACountAndAnAtOnATemporaryAreReadAsAnArray()
    {
        var reads = On.Value.Body("DirectReads");
        Assert.That(reads, Does.Contain("_eval4.Length").And.Contain("_eval1[checked((int)(_eval2.Value))]"), reads);
        Assert.That(reads, Does.Not.Contain("ConcreteArrayAccess"), reads);
        Assert.That(Off.Value.Body("DirectReads"), Does.Contain("ConcreteArrayAccess.At("));
    }

    [TestCase("ReduceSeed", "(IReadOnlyList<Integer>)_lp0")]
    [TestCase("Returned", "return ((IReadOnlyList<Integer>)_lp0);")]
    [TestCase("Join", "((IReadOnlyList<Integer>)_lp0)")]
    [TestCase("GenericArgument", "((IReadOnlyList<Integer>)_eval1).Identity()")]
    [TestCase("Carried", "last = ((IReadOnlyList<Integer>)_lp1);")]
    [TestCase("Pick", "((IReadOnlyList<Integer>)_lp")]
    public static void AConsumerOfTheInterfaceReceivesTheArrayThroughAConversion(string name, string expected)
        => Assert.That(On.Value.Body(name), Does.Contain(expected), On.Value.Body(name));

    [Test]
    public static void ALetBoundTemporaryStaysTheInterfaceUntilNamedLocalsTakeStorage()
        => Assert.That(On.Value.Body("ReadTwice"), Does.Contain("var kept = ((IReadOnlyList<Integer>)_lp0);"));

    [Test]
    public static void FewerTypeTestedReadsRemainWithTheFlagOn()
        => Assert.That(Regex.Matches(On.Value.Text, @"ConcreteArrayAccess\.(At|Count)\(").Count,
            Is.LessThan(Regex.Matches(Off.Value.Text, @"ConcreteArrayAccess\.(At|Count)\(").Count));
}
