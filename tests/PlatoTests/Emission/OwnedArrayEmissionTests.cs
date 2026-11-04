// Pins --owned-arrays for lowered-loop temporaries (TKT-1152 C2b): Heap storage where consumers read the array,
// the interface where they do not, and the same values either way.
using System.Reflection;
using System.Text.RegularExpressions;
using Ara3D.Geometry.CSharpWriter;
using Ara3D.Geometry.Compiler.Checking;
using Ara3D.Geometry.Compiler.Symbols;
using NUnit.Framework;

namespace PlatoTests.Emission;

using Integer = Ara3D.Geometry.Integer;
using Number = Ara3D.Geometry.Number;

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
    private const string IntegerOperators = @"global using NumberValue = System.Double;
    namespace Ara3D.Geometry { public static class IntegerProbeOperators {
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
        "TwoLoops", "Paired2", "FlatMapRagged", "FlatMapExact", "Reassigned",
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
        => value is IEnumerable<Integer> xs ? "[" + string.Join(",", xs.Select(x => x.Value)) + "]"
            : value is Number number ? ((double)number).ToString(System.Globalization.CultureInfo.InvariantCulture) : ((Integer)value).Value.ToString();

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
        yield return new TestCaseData("TwoLoops", new object[] { (Integer)6 }, "43").SetName("TwoLoops: a let-bound array read by two loops");
        yield return new TestCaseData("Paired2", new object[] { (Integer)3, (Number)2.0 }, "73").SetName("Paired2: a multi-consumer loop over an owned source");
        yield return new TestCaseData("FlatMapRagged", new object[] { (Integer)3 }, "[0,0,2,0,3,6]").SetName("FlatMapRagged: a FlatMap over an owned source");
        yield return new TestCaseData("FlatMapExact", new object[] { (Integer)3 }, "[0,1,1,2,4,5]").SetName("FlatMapExact: exact output over an owned source");
        yield return new TestCaseData("Reassigned", new object[] { (Integer)4 }, "6").SetName("Reassigned: a local assigned again keeps the interface");
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
    public static void ALetBoundArrayIsDeclaredAsAnArrayAndReadDirectlyByTwoLoops()
    {
        var body = On.Value.Body("TwoLoops");
        Assert.That(body, Does.Contain("Integer[] kept = _lp0;").And.Contain("Integer[] doubled = _lp1;"), body);
        Assert.That(body, Does.Contain("var _s1 = kept;").And.Contain("var _s2 = kept;"), body);
        Assert.That(body, Does.Contain("_s1.Length").And.Contain("_s2[_i2]").And.Contain("doubled[checked((int)(5L))]").And.Contain("kept.Length"), body);
        Assert.That(body, Does.Not.Contain("ConcreteArrayAccess").And.Not.Contain("IReadOnlyList"), body);
        Assert.That(Off.Value.Body("TwoLoops"), Does.Contain("var kept = _lp0;").And.Contain("ConcreteArrayAccess.Count(_s1)"));
    }

    [Test]
    public static void ALetBoundArrayThatIsAssignedAgainStaysTheInterface()
        => Assert.That(On.Value.Body("Reassigned"), Does.Contain("var kept = ((IReadOnlyList<Integer>)_lp0);").And.Contain("kept = ((IReadOnlyList<Integer>)_lp1);"));

    [Test]
    public static void ALetBoundArrayCapturedByAClosureStaysTheInterface()
        => Assert.That(On.Value.Body("Captured"), Does.Contain("IReadOnlyList<Integer> a = ").Or.Contain("var a = ((IReadOnlyList<Integer>)"));

    [Test]
    public static void AMultiConsumerLoopReadsAnOwnedSourceWithoutAConversion()
    {
        var body = On.Value.Body("Paired2");
        Assert.That(body, Does.Contain("Number[] samples = _lp0;").And.Contain("var _mc0_source = samples;"), body);
        Assert.That(body, Does.Contain("_mc0_source.Length").And.Contain("_mc0_source[_mc0_index]"), body);
        Assert.That(body, Does.Not.Contain("ConcreteArrayAccess").And.Not.Contain("IReadOnlyList"), body);
        Assert.That(Off.Value.Body("Paired2"), Does.Contain("ConcreteArrayAccess.Count(_mc0_source)"));
    }

    [Test]
    public static void AFlatMapReadsAnOwnedSourceDirectlyWithAndWithoutExactOutput()
    {
        var ragged = On.Value.Body("FlatMapRagged");
        Assert.That(ragged, Does.Contain("var _s1 = kept;").And.Contain("_s1.Length").And.Contain("_s1[_i1]"), ragged);
        Assert.That(ragged, Does.Not.Contain("ConcreteArrayAccess.Count(_s1)").And.Not.Contain("(IReadOnlyList<Integer>)kept"), ragged);
        var exact = On.Value.Body("FlatMapExact");
        // The source is an array already: the run-time test for one is gone, the size test stays.
        Assert.That(exact, Does.Contain("if (_exactSize1 <= System.Array.MaxLength)").And.Not.Contain("is Integer[]"), exact);
        Assert.That(exact, Does.Contain("var _eval1_1 = _s1[_i1];"), exact);
        Assert.That(Off.Value.Body("FlatMapExact"), Does.Contain("_s1 is Integer[] &&"));
    }

    [Test]
    public static void FewerTypeTestedReadsRemainWithTheFlagOn()
        => Assert.That(Regex.Matches(On.Value.Text, @"ConcreteArrayAccess\.(At|Count)\(").Count,
            Is.LessThan(Regex.Matches(Off.Value.Text, @"ConcreteArrayAccess\.(At|Count)\(").Count));
}

/// <summary>
/// TKT-1152 C2c with <c>--in-params</c>: an element of an owned array of large structs (<c>owned-large.plato</c>,
/// forty Integers) is read where it lies, with no <c>RefAccess.RefAt</c> call and no type test: a <c>ref readonly</c>
/// local is <c>ref x[i]</c>, and an argument or receiver is the element itself, which C# passes by reference without
/// a copy. Flag off, the same reads go through the accessor. Values are equal.
/// </summary>
[TestFixture]
public static class OwnedArrayLargeElementTests
{
    private static readonly string[] Methods = { "Make", "Ends", "ByRefLocal", "ByArgument", "ByReceiver" };

    private static EmittedLibrary Build(bool ownedArrays)
        => EmittedLibrary.Build("OwnedArrayFixtures", "owned-large.plato", name => Methods.Contains(name),
            new[] { "OwnedLargeProbe.g.cs" }, writer => OwnedArrayEmissionTests.Pin(writer, ownedArrays), null,
            "_OwnLarge.g.cs", $"{LargeStructPassing.HelperFile}", $"{CSharpWriter.ConcreteArrayAccessClass}.g.cs");

    private static readonly Lazy<EmittedLibrary> Off = new(() => Build(false));
    private static readonly Lazy<EmittedLibrary> On = new(() => Build(true));

    [TestCase("ByRefLocal", 48)]
    [TestCase("ByArgument", 47)]
    [TestCase("ByReceiver", 43)]
    public static void EachReadGivesTheFlagOffValue(string name, long expected)
    {
        foreach (var library in new[] { Off.Value, On.Value })
            Assert.That(((Integer)library.Method(name).Invoke(null, new object[] { (Integer)4, (Integer)2 })!).Value, Is.EqualTo(expected));
    }

    [Test]
    public static void AReferenceLocalIsARefIntoTheOwnedArray()
    {
        var on = On.Value.Body("ByRefLocal");
        Assert.That(on, Does.Contain("Make[] rows = ").Or.Contain("OwnLarge[] rows = "), on);
        Assert.That(on, Does.Contain("ref readonly var w = ref rows[").And.Not.Contain("RefAccess"), on);
        Assert.That(Off.Value.Body("ByRefLocal"), Does.Contain("ref readonly var w = ref RefAccess.RefAt(rows"));
    }

    [TestCase("ByArgument")]
    [TestCase("ByReceiver")]
    public static void AnArgumentOrReceiverIsTheElementItself(string name)
    {
        var on = On.Value.Body(name);
        Assert.That(on, Does.Contain("rows[").And.Not.Contain("RefAccess").And.Not.Contain("ConcreteArrayAccess"), on);
        Assert.That(Off.Value.Body(name), Does.Contain("RefAccess.RefAt(rows"));
    }
}

/// <summary>
/// TKT-1152 C2c: under the flag a node the printer has no case for would print as <c>/*?*/</c> and leave a hole in the
/// program; the checker refuses the body instead (rule S8).
/// </summary>
[TestFixture]
public static class OwnedArrayUnprintableNodeTests
{
    private sealed class Mystery : TirNode
    {
        public Mystery(params TirNode[] children) : base(null, null) => Kids = children;
        public TirNode[] Kids { get; }
        public override IEnumerable<TirNode> Children => Kids;
    }

    private sealed class Target : IStorageTarget
    {
        public bool Accepts(ArrayStorage value, ConsumerKind consumer) => CSharpStorageRules.Accepts(value, consumer);
        public bool MayConvert(ArrayStorage from, ArrayStorage to) => CSharpStorageRules.MayConvert(from, to);
        public bool IsObserver(TirCall call) => false;
        public bool IsUnmanaged(TypeExpression element) => false;
        public int SizeOf(TypeExpression element) => 0;
        public bool OnCallCycle(FunctionDef function) => true;
    }

    [Test]
    public static void ANodeWithoutAPrinterCaseIsRefusedNotPrintedAsAComment()
    {
        var violations = StorageChecker.Check(new TirStorageScope(new Mystery(new Mystery()), new StoragePlan()), new Target());
        Assert.That(violations.Select(v => v.Rule), Is.EqualTo(new[] { StorageChecker.Rule.S8_Unprintable, StorageChecker.Rule.S8_Unprintable }));
    }

    [Test]
    public static void ANodeWithACaseIsNotRefused()
    {
        var literal = new TirDefault(null, null);
        Assert.That(StorageChecker.Check(new TirStorageScope(literal, new StoragePlan()), new Target()), Is.Empty);
        Assert.That(TirCSharpBodyWriter.PrintsAsNode(literal), Is.True);
    }
}
