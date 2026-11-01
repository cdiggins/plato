// Pins --stack-arrays (TKT-1152 C6): which lowered-loop temporaries take a hoisted stackalloc slot, how they print, and
// that the values are the flag-off values.
using System.Reflection;
using System.Text.RegularExpressions;
using Ara3D.Geometry.CSharpWriter;
using NUnit.Framework;

namespace PlatoTests.Emission;

using Integer = Ara3D.Geometry.Integer;
using Number = Ara3D.Geometry.Number;

/// <summary>
/// The fixture <c>OwnedArrayFixtures/stack.plato</c> is emitted under the nineteen pinned flags with
/// <c>--owned-arrays --stack-arrays</c> (strict: a checker violation anywhere in the foundation or the fixture throws),
/// compiled with Roslyn and run, and compared with the build that has <c>--owned-arrays</c> alone and with the value of the
/// immutable meaning. The text shows the slots where a body qualifies and none where it must not have one: a self- or
/// mutually recursive body, an array that is passed on, returned, captured or assigned again, and the windows past the
/// 1 KiB budget of one body.
/// </summary>
[TestFixture]
public static class StackArrayEmissionTests
{
    private static readonly string[] Methods =
    {
        "Digest", "Constant", "Sized", "Rows", "Doubled", "FaultMidFill", "Many", "Recur", "Ping", "Pong", "Identity",
        "Passed", "Returned", "Probe", "Captured", "Reassigned",
    };
    private static readonly string[] Files = { "StackProbe.g.cs", "Extensions.g.cs" };

    private static CSharpWriter lastWriter;

    internal static EmittedLibrary Build(bool stack)
        => EmittedLibrary.Build("OwnedArrayFixtures", "stack.plato", name => Methods.Contains(name), Files,
            writer =>
            {
                OwnedArrayEmissionTests.Pin(writer, true);
                writer.StackArrays = stack;
                if (stack)
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

    private static readonly Lazy<EmittedLibrary> Heap = new(() => Build(false));
    private static readonly Lazy<EmittedLibrary> Stack = new(() => Build(true));

    private static object Call(EmittedLibrary library, string name, params object[] args)
        => library.Method(name).Invoke(null, args)!;

    private static string Show(object value)
        => value is IEnumerable<Integer> xs ? "[" + string.Join(",", xs.Select(x => x.Value)) + "]"
            : value is Number number ? ((double)number).ToString(System.Globalization.CultureInfo.InvariantCulture) : ((Integer)value).Value.ToString();

    private static readonly IReadOnlyList<Integer> FourFiveSix = new Integer[] { 4, 5, 6 };
    private static readonly IReadOnlyList<Integer> NoElements = Array.Empty<Integer>();

    // Each shape and its value under the immutable meaning. Both builds must give it.
    private static IEnumerable<TestCaseData> Shapes()
    {
        yield return new TestCaseData("Constant", new object[] { (Integer)7 }, "40").SetName("Constant: a constant count");
        yield return new TestCaseData("Sized", new object[] { (Integer)5 }, "5006").SetName("Sized: under the capacity");
        yield return new TestCaseData("Sized", new object[] { (Integer)16 }, "16006").SetName("Sized: at the capacity");
        yield return new TestCaseData("Sized", new object[] { (Integer)17 }, "17006").SetName("Sized: one past the capacity falls back to the heap");
        yield return new TestCaseData("Sized", new object[] { (Integer)40 }, "40006").SetName("Sized: far past the capacity falls back to the heap");
        yield return new TestCaseData("Sized", new object[] { (Integer)0 }, "0").SetName("Sized: a count of zero is the empty array");
        yield return new TestCaseData("Sized", new object[] { (Integer)(-3) }, "0").SetName("Sized: a negative count is the empty array");
        yield return new TestCaseData("Rows", new object[] { (Integer)3 }, "[14,29,44]").SetName("Rows: a slot reused on every pass of the outer loop");
        yield return new TestCaseData("Rows", new object[] { (Integer)20 }, "[14,29,44,59,74,89,104,119,134,149,164,179,194,209,224,239,254,269,284,299]")
            .SetName("Rows: twenty passes");
        yield return new TestCaseData("Rows", new object[] { (Integer)0 }, "[]").SetName("Rows: no pass");
        yield return new TestCaseData("Doubled", new object[] { FourFiveSix }, "13").SetName("Doubled: a map over a parameter array");
        yield return new TestCaseData("Doubled", new object[] { NoElements }, "0").SetName("Doubled: a map over an empty array");
        yield return new TestCaseData("Many", new object[] { (Integer)3, (Number)2.0 }, "52").SetName("Many: past the body budget");
        yield return new TestCaseData("Many", new object[] { (Integer)30, (Number)2.0 }, "79").SetName("Many: past the capacity and the budget");
        yield return new TestCaseData("Recur", new object[] { (Integer)2 }, "17").SetName("Recur: self-recursive");
        yield return new TestCaseData("Ping", new object[] { (Integer)2 }, "5").SetName("Ping: mutually recursive");
        yield return new TestCaseData("Passed", new object[] { (Integer)4 }, "5").SetName("Passed: given to a call");
        yield return new TestCaseData("Returned", new object[] { (Integer)4 }, "[0,1,4,9]").SetName("Returned: returned");
        yield return new TestCaseData("Captured", new object[] { (Integer)4 }, "5").SetName("Captured: a real lambda reads it");
        yield return new TestCaseData("Reassigned", new object[] { (Integer)4 }, "6").SetName("Reassigned: assigned again");
        yield return new TestCaseData("FaultMidFill", new object[] { (Integer)2 }, "2").SetName("FaultMidFill: no element faults");
    }

    [TestCaseSource(nameof(Shapes))]
    public static void EachShapeCompilesAndGivesTheFlagOffValue(string name, object[] args, string expected)
    {
        Assert.That(Show(Call(Heap.Value, name, args)), Is.EqualTo(expected), "owned arrays only");
        Assert.That(Show(Call(Stack.Value, name, args)), Is.EqualTo(expected), "stack arrays");
    }

    [Test]
    public static void ASlotIsReusedByEveryPassAndHoldsNoStaleElement()
    {
        // The same method, called again after a longer and a shorter pass: nothing of the earlier call shows.
        var library = Stack.Value;
        foreach (var n in new[] { 20, 1, 20, 3, 0, 5 })
            Assert.That(Show(Call(library, "Rows", (Integer)n)), Is.EqualTo(Show(Call(Heap.Value, "Rows", (Integer)n))), $"n = {n}");
        foreach (var n in new[] { 16, 3, 17, 1, 40, 2 })
            Assert.That(Show(Call(library, "Sized", (Integer)n)), Is.EqualTo(Show(Call(Heap.Value, "Sized", (Integer)n))), $"n = {n}");
    }

    [Test]
    public static void ACallbackThatFaultsMidFillFaultsAlikeAndLeavesNoStateBehind()
    {
        foreach (var library in new[] { Heap.Value, Stack.Value })
        {
            var fault = Assert.Throws<TargetInvocationException>(() => Call(library, "FaultMidFill", (Integer)5))!;
            Assert.That(fault.InnerException, Is.TypeOf<DivideByZeroException>());
            Assert.That(Show(Call(library, "FaultMidFill", (Integer)2)), Is.EqualTo("2"), "a later call is not affected");
        }
    }

    private static readonly Regex Stackalloc = new(@"stackalloc");

    private static int Slots(string method) => Stackalloc.Matches(Stack.Value.Body(method)).Count;

    [TestCase("Constant", 1)]
    [TestCase("Sized", 1)]
    [TestCase("Rows", 3)]
    [TestCase("Doubled", 1)]
    [TestCase("FaultMidFill", 1)]
    [TestCase("Many", 8)]
    [TestCase("Recur", 0)]
    [TestCase("Ping", 0)]
    [TestCase("Pong", 0)]
    [TestCase("Passed", 0)]
    [TestCase("Returned", 0)]
    [TestCase("Captured", 0)]
    [TestCase("Reassigned", 0)]
    public static void TheSlotsOfEachBody(string method, int slots)
        => Assert.That(Slots(method), Is.EqualTo(slots), Stack.Value.Body(method));

    [Test]
    public static void NoBodyStackallocsWithoutTheFlag()
        => Assert.That(Heap.Value.Text, Does.Not.Contain("stackalloc"));

    [Test]
    public static void ASlotIsASpanSpelledOutAndDeclaredBeforeTheOutermostLoop()
    {
        var rows = Stack.Value.Body("Rows");
        var slot = rows.IndexOf("stackalloc", StringComparison.Ordinal);
        Assert.That(slot, Is.GreaterThanOrEqualTo(0));
        Assert.That(Regex.IsMatch(rows, @"System\.Span<Integer> _slot\d+ = stackalloc Integer\[12\];"), rows);
        Assert.That(slot, Is.LessThan(rows.IndexOf("for (", StringComparison.Ordinal)), "declared before the outer loop");
        Assert.That(rows, Does.Not.Contain("var _slot"), "never var over a stackalloc");
        Assert.That(Regex.IsMatch(rows, @"System\.Span<Integer> _a\d+ = _n\d+ <= 12 \? _slot\d+\.Slice\(0, _n\d+\) : new Integer\[_n\d+\];"), rows);

        var sized = Stack.Value.Body("Sized");
        Assert.That(Regex.IsMatch(sized, @"stackalloc Integer\[16\]"), sized);
        Assert.That(Regex.IsMatch(sized, @"<= 16 \? _slot\d+\.Slice"), sized);
    }

    [Test]
    public static void TheBudgetLeavesTheLastWindowsOnTheHeap()
    {
        var many = Stack.Value.Body("Many");
        Assert.That(Regex.Matches(many, @"stackalloc Number\[16\]").Count, Is.EqualTo(8), many);
        Assert.That(Regex.Matches(many, @"Number\[\] _lp\d+ = _a\d+;").Count, Is.EqualTo(2), many);
        Assert.That(lastWriter.StackRefusals.Keys, Does.Contain(TirStorageResolver.StackRefusal.Budget));
    }

    [Test]
    public static void TheWriterCountsSlotsAndRefusalsByReason()
    {
        _ = Stack.Value;
        Assert.That(lastWriter.StackSlotsPlaced, Is.EqualTo(15), "1 + 1 + 3 + 1 + 1 + 8 slots of the fixture; none in the foundation");
        Assert.That(lastWriter.StackRefusals.Keys, Does.Contain(TirStorageResolver.StackRefusal.Cycle));
        Assert.That(lastWriter.StackRefusals.Keys, Does.Contain(TirStorageResolver.StackRefusal.Escapes));
        Assert.That(lastWriter.StorageRefusals, Is.EqualTo(0), "no checker refusal");
        TestContext.Out.WriteLine($"slots placed: {lastWriter.StackSlotsPlaced}; refusals: {string.Join(", ", lastWriter.StackRefusals.Select(r => $"{r.Key} {r.Value}"))}");
    }
}
