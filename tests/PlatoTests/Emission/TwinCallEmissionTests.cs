// Pins the routing of calls to span twins (TKT-1152 C8b): which calls go to BorrowedTwins, how their arguments are
// converted, and that the values are the values of the original calls.
using System.Reflection;
using System.Text.RegularExpressions;
using Ara3D.Geometry.CSharpWriter;
using NUnit.Framework;

namespace PlatoTests.Emission;

using Integer = Ara3D.Geometry.Integer;

/// <summary>
/// The fixture <c>OwnedArrayFixtures/twin-calls.plato</c> is emitted under the nineteen pinned flags with
/// <c>--owned-arrays --stack-arrays --span-params</c> (strict), compiled with Roslyn and run, and compared with the build
/// that has <c>--owned-arrays --stack-arrays</c> only and with the value of the immutable meaning. A call goes to the twin of
/// its callee when every borrowed argument is an array the body owns: a Stack slot is lent as a <c>ReadOnlySpan</c>, a Heap
/// array is wrapped. A parameter array, a generic callee and a callee that keeps its array stay as they were.
/// </summary>
[TestFixture]
public static class TwinCallEmissionTests
{
    private static readonly string[] Methods =
    {
        "Pick", "SumOf", "Both", "Same", "First", "StackArg", "HeapArg", "TwoOwned", "MixedOwned", "GenericCall",
        "RecursiveCaller", "PastEnd", "ViaPick", "Depth", "PassOnCaller", "DepthCaller", "Extensions_Pick", "Extensions_SumOf",
        "Extensions_Both", "Extensions_ViaPick",
    };
    private static readonly string[] Files = { "TwinProbe.g.cs", "Extensions.g.cs", "BorrowedTwins.g.cs" };

    private static CSharpWriter lastWriter;

    internal static EmittedLibrary Build(bool twins)
        => EmittedLibrary.Build("OwnedArrayFixtures", "twin-calls.plato", name => Methods.Contains(name), twins ? Files : Files[..2],
            writer =>
            {
                OwnedArrayEmissionTests.Pin(writer, true);
                (writer.StackArrays, writer.SpanParams) = (true, twins);
                if (twins)
                    lastWriter = writer;
            },
            @"global using NumberValue = System.Double;
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
    } }",
            $"{CSharpWriter.ConcreteArrayAccessClass}.g.cs");

    private static readonly Lazy<EmittedLibrary> Plain = new(() => Build(false));
    private static readonly Lazy<EmittedLibrary> Twins = new(() => Build(true));

    private static object Call(EmittedLibrary library, string name, params object[] args)
        => library.Method(name).Invoke(null, args)!;

    private static readonly IReadOnlyList<Integer> SevenEight = new Integer[] { 7, 8 };

    private static IEnumerable<TestCaseData> Shapes()
    {
        yield return new TestCaseData("StackArg", new object[] { (Integer)2 }, 756).SetName("StackArg: a slot lent to two twins");
        yield return new TestCaseData("StackArg", new object[] { (Integer)(-1) }, -342).SetName("StackArg: negative elements");
        yield return new TestCaseData("HeapArg", new object[] { (Integer)5 }, 210).SetName("HeapArg: a heap array lent to one twin");
        yield return new TestCaseData("HeapArg", new object[] { (Integer)40 }, 280).SetName("HeapArg: past the slot capacity");
        yield return new TestCaseData("TwoOwned", new object[] { (Integer)3 }, 16).SetName("TwoOwned: two borrowed arguments");
        yield return new TestCaseData("TwoOwned", new object[] { (Integer)30 }, 43).SetName("TwoOwned: one of them on the heap");
        yield return new TestCaseData("MixedOwned", new object[] { (Integer)3, SevenEight }, 6).SetName("MixedOwned: a parameter keeps the original");
        yield return new TestCaseData("GenericCall", new object[] { (Integer)4 }, 7).SetName("GenericCall: a generic callee has no twin");
        yield return new TestCaseData("RecursiveCaller", new object[] { (Integer)2 }, 234).SetName("RecursiveCaller: a recursive body lends a heap array");
        yield return new TestCaseData("PassOnCaller", new object[] { (Integer)5 }, 425).SetName("PassOnCaller: a twin that lends its span on");
        yield return new TestCaseData("DepthCaller", new object[] { (Integer)4 }, 6).SetName("DepthCaller: a recursive callee keeps the original");
    }

    [TestCaseSource(nameof(Shapes))]
    public static void EachShapeGivesTheValueOfTheOriginalCalls(string name, object[] args, int expected)
    {
        Assert.That(((Integer)Call(Plain.Value, name, args)).Value, Is.EqualTo(expected), "without twins");
        Assert.That(((Integer)Call(Twins.Value, name, args)).Value, Is.EqualTo(expected), "with twins");
    }

    [Test]
    public static void AReadPastTheEndInsideATwinFaultsAsTheOriginalDoes()
    {
        foreach (var library in new[] { Plain.Value, Twins.Value })
            Assert.Throws<TargetInvocationException>(() => Call(library, "PastEnd", (Integer)3));
    }

    [Test]
    public static void TheOriginalsKeepTheirSignaturesAndStayUsableAsMethodGroups()
    {
        var pick = Twins.Value.Assembly.GetTypes().SelectMany(t => t.GetMethods()).Single(m => m.Name == "Pick");
        Assert.That(pick.GetParameters()[0].ParameterType, Is.EqualTo(typeof(IReadOnlyList<Integer>)));
        var group = Delegate.CreateDelegate(typeof(Func<IReadOnlyList<Integer>, Integer, Integer>), pick);
        Assert.That(((Func<IReadOnlyList<Integer>, Integer, Integer>)group)(new Integer[] { 5, 6, 7 }, 2).Value, Is.EqualTo(703));
        var twin = Twins.Value.Method("Extensions_Pick");
        Assert.That(twin.GetParameters()[0].ParameterType, Is.EqualTo(typeof(ReadOnlySpan<Integer>)));
    }

    private static string Body(string name) => Twins.Value.Body(name);

    [Test]
    public static void ASlotIsLentAsAReadOnlySpanAndAHeapArrayIsWrapped()
    {
        var stack = Body("StackArg");
        Assert.That(stack, Does.Contain("stackalloc"), stack);
        Assert.That(Regex.Matches(stack, @"BorrowedTwins\.Extensions_(Pick|SumOf)\(\(\(System\.ReadOnlySpan<Integer>\)w\)").Count, Is.EqualTo(2), stack);

        var heap = Body("HeapArg");
        Assert.That(heap, Does.Not.Contain("stackalloc"), heap);
        Assert.That(heap, Does.Contain("BorrowedTwins.Extensions_Pick(new System.ReadOnlySpan<Integer>("), heap);
        Assert.That(heap, Does.Contain("Same("), "the callee that keeps the array is still called");
    }

    [Test]
    public static void ACallWithAParameterArrayAGenericCalleeAndAKeptArrayStayOnTheOriginal()
    {
        Assert.That(Body("MixedOwned"), Does.Not.Contain("BorrowedTwins."));
        Assert.That(Body("GenericCall"), Does.Not.Contain("BorrowedTwins."));
        Assert.That(Twins.Value.Text, Does.Not.Contain("Extensions_Same"));
        Assert.That(Twins.Value.Text, Does.Not.Contain("Extensions_First"));
    }

    [Test]
    public static void ATwinHandsItsSpanOnToTheTwinsOfTheFunctionsItCalls()
    {
        var twins = Twins.Value.Text;
        Assert.That(twins, Does.Contain("Integer Extensions_ViaPick(System.ReadOnlySpan<Integer> xs, Integer k)"));
        Assert.That(twins, Does.Match(@"Extensions_ViaPick\([^)]*\)\s*=>[^;]*BorrowedTwins\.Extensions_Pick\(xs, k\)[^;]*BorrowedTwins\.Extensions_SumOf\(xs\)"), "the span goes on as it is");
        Assert.That(Body("PassOnCaller"), Does.Contain("BorrowedTwins.Extensions_ViaPick("));
        Assert.That(twins, Does.Not.Contain("Extensions_Depth"), "a function that hands its array on to itself borrows nothing");
        Assert.That(Body("DepthCaller"), Does.Not.Contain("BorrowedTwins."));
    }

    [Test]
    public static void ARecursiveBodyHasNoSlotButStillCallsTheTwin()
    {
        var body = Body("RecursiveCaller");
        Assert.That(body, Does.Not.Contain("stackalloc"), body);
        Assert.That(body, Does.Contain("BorrowedTwins.Extensions_SumOf(new System.ReadOnlySpan<Integer>("), body);
    }

    [Test]
    public static void TheWriterCountsTheRoutedCalls()
    {
        _ = Twins.Value;
        Assert.That(lastWriter.TwinCallsRouted, Is.GreaterThanOrEqualTo(8));
        Assert.That(lastWriter.StorageRefusals, Is.EqualTo(0));
        Assert.That(lastWriter.Borrowed.Refusals, Is.Empty);
        TestContext.Out.WriteLine($"twin calls routed: {lastWriter.TwinCallsRouted}; slots placed: {lastWriter.StackSlotsPlaced}; twins written: {lastWriter.Borrowed.TwinsWritten}");
    }
}
