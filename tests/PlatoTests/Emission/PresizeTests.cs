// Pins --presize (TKT-1131): a counted List fill is created with its capacity, and nothing else changes.
using Ara3D.Geometry;
using Ara3D.Geometry.CSharpWriter;
using NUnit.Framework;

namespace PlatoTests.Emission;

using Integer = Ara3D.Geometry.Integer;

/// <summary>
/// A <c>List</c> that a loop fills once per pass up to a stated bound is written
/// <c>new PlatoList&lt;T&gt;(bound)</c> with the switch on; a conditional append, a bound that moves,
/// and an append of whole arrays are left alone; the values are the same either way.
/// </summary>
[TestFixture]
public static class PresizeTests
{
    private static readonly string[] Methods = { "Counted", "FromOne", "Filtered", "Appended", "Stated", "Moving", "Built" };
    // The Integer operators the probes call, which the generated foundation (not in this assembly) defines.
    private const string IntegerOperators = @"namespace Ara3D.Geometry { public static class IntegerProbeOperators {
        public static Integer Add(this Integer a, Integer b) => a + b;
        public static Integer Subtract(this Integer a, Integer b) => a - b;
        public static Integer Multiply(this Integer a, Integer b) => a * b;
        public static Integer Modulo(this Integer a, Integer b) => a % b;
        public static bool LessThan(this Integer a, Integer b) => a.Value < b.Value;
        public static bool Equals(this Integer a, Integer b) => a.Value == b.Value;
    } }";
    private static readonly string[] Files = { "PresizeProbe.g.cs", "Extensions.g.cs" };

    private static EmittedLibrary Build(bool presize, bool concrete = false)
        => EmittedLibrary.Build("PresizeFixtures", "presize.plato", name => Methods.Contains(name), Files,
            writer => (writer.Presize, writer.ConcreteArrays) = (presize, concrete), IntegerOperators,
            concrete ? new[] { $"{CSharpWriter.ConcreteArrayAccessClass}.g.cs" } : Array.Empty<string>());

    private static readonly Lazy<EmittedLibrary> Off = new(() => Build(false));
    private static readonly Lazy<EmittedLibrary> On = new(() => Build(true));
    private static readonly Lazy<EmittedLibrary> Concrete = new(() => Build(true, concrete: true));

    private static IReadOnlyList<Integer> Run(EmittedLibrary library, string name, params object[] args)
        => (IReadOnlyList<Integer>)library.Method(name).Invoke(null, args)!;

    private static IReadOnlyList<Integer> Ints(params int[] xs) => xs.Select(x => (Integer)x).ToArray();

    [Test]
    public static void OffEmitsTheEmptyConstructorEverywhere()
    {
        foreach (var name in Methods.Except(new[] { "Stated", "Built" }))
            Assert.That(Off.Value.Body(name), Does.Contain("new PlatoList<Integer>()"), name);
        Assert.That(Off.Value.Body("Stated"), Does.Contain("new PlatoList<Integer>(n)"));
    }

    [Test]
    public static void ACountedFillIsCreatedWithItsBound()
    {
        Assert.That(On.Value.Body("Counted"), Does.Contain("new PlatoList<Integer>(n)"));
        Assert.That(On.Value.Body("Counted"), Does.Not.Contain("new PlatoList<Integer>()"));
    }

    [Test]
    public static void ACounterThatStartsAboveZeroSubtractsItsStart()
    {
        var body = On.Value.Body("FromOne");
        Assert.That(body, Does.Contain("new PlatoList<Integer>(((Integer)src.Count).Subtract(((Integer)1)))"), body);
    }

    [TestCase("Filtered")]
    [TestCase("Moving")]
    [TestCase("Appended")]
    public static void AShapeWhoseCountIsNotStatedKeepsTheEmptyConstructor(string name)
        => Assert.That(On.Value.Body(name), Does.Contain("new PlatoList<Integer>()"));

    [Test]
    public static void AnExplicitCapacityInTheSourceIsLeftAsWritten()
        => Assert.That(On.Value.Body("Stated"), Does.Contain("new PlatoList<Integer>(n)"));

    [Test]
    public static void ConcreteArraysFreezeABufferToItsOwnArray()
    {
        Assert.That(Off.Value.Body("Built"), Does.Contain(".Freeze()").And.Not.Contain("FreezeExact"));
        Assert.That(Concrete.Value.Body("Built"), Does.Contain(".FreezeExact()"));
        Assert.That(Run(Off.Value, "Built", (Integer)3), Is.Not.InstanceOf<Integer[]>());
        Assert.That(Run(Concrete.Value, "Built", (Integer)3), Is.InstanceOf<Integer[]>());
    }

    [Test]
    public static void ConcreteArraysFreezeACountedListToItsOwnArrayOnlyWhenFull()
    {
        Assert.That(Run(Concrete.Value, "Counted", (Integer)4), Is.InstanceOf<Integer[]>(), "presized: exactly full");
        Assert.That(Run(Concrete.Value, "Filtered", (Integer)7), Is.Not.InstanceOf<Integer[]>(), "3 of 4 slots used");
        Assert.That(Run(Concrete.Value, "Filtered", (Integer)7), Is.EqualTo(Ints(0, 3, 6)));
    }

    [Test]
    public static void ValuesAreTheSameWithTheSwitchOnAndOff()
    {
        foreach (var library in new[] { Off.Value, On.Value, Concrete.Value })
        {
            Assert.That(Run(library, "Counted", (Integer)5), Is.EqualTo(Ints(0, 2, 4, 6, 8)));
            Assert.That(Run(library, "Counted", (Integer)0), Is.Empty);
            Assert.That(Run(library, "Counted", (Integer)(-3)), Is.Empty, "a negative bound is the empty capacity");
            Assert.That(Run(library, "FromOne", Ints(9, 8, 7)), Is.EqualTo(Ints(8, 7)));
            Assert.That(Run(library, "FromOne", Ints()), Is.Empty);
            Assert.That(Run(library, "Filtered", (Integer)7), Is.EqualTo(Ints(0, 3, 6)));
            Assert.That(Run(library, "Appended", Ints(1), Ints(2, 3)), Is.EqualTo(Ints(1, 2, 3)));
            Assert.That(Run(library, "Stated", (Integer)0), Is.EqualTo(Ints(1)));
            Assert.That(Run(library, "Moving", (Integer)6), Is.EqualTo(Ints(0, 1, 2)));
            Assert.That(Run(library, "Built", (Integer)3), Is.EqualTo(Ints(0, 1, 2)));
        }
    }
}
