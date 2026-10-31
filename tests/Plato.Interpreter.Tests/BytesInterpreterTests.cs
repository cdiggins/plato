// Checks the Bytes primitive in the interpreter (TKT-1143).
using Ara3D.Geometry.Compiler.Types;
using Ara3D.Geometry.Interpreter;

namespace Plato.Interpreter.Tests;

/// <summary>The interpreter's <see cref="PlatoBytes"/>: a law parameter typed Bytes is generated, not given a
/// zero-field record; an out-of-range At is an error verdict, never a read of the array beyond the view; and
/// a view and its slice share one array.</summary>
[TestFixture]
public class BytesInterpreterTests
{
    private const string Source = """
        library BytesFixture
        {
            Law_BytesParameterIsBytes(b: Bytes): Boolean => b.Count >= 0 && b.SubBytes(0, b.Count) == b;
            Law_SliceCannotSeePastItsView(b: Bytes): Boolean => b.SubBytes(0, 2).Count <= 2;
            Law_AtPastTheViewIsAFault(b: Bytes): Boolean => b.SubBytes(0, 1).At(1) == 0;
            Law_AtInsideTheViewIsTheByte(): Boolean => "abc".ToBytes.SubBytes(1, 1).At(0) == 98;
        }
        """;

    private static readonly Lazy<Ara3D.Geometry.Interpreter.Interpreter> _interp = new(() =>
        new Ara3D.Geometry.Interpreter.Interpreter(LawRunnerSelectionTests.CompileSource(Source), NumberProfile.Cpu64));

    private static List<LawResult> Run()
        => new LawRunner(_interp.Value, new LawRunOptions { Filters = new[] { "BytesFixture" } }).RunAll().ToList();

    [Test]
    public void Bytes_parameters_are_generated()
    {
        var results = Run();
        Assert.That(results.Select(r => r.Name).ToList(), Has.Count.EqualTo(4));
        foreach (var name in new[] { "Law_BytesParameterIsBytes", "Law_SliceCannotSeePastItsView", "Law_AtInsideTheViewIsTheByte" })
            Assert.That(results.Single(r => r.Name.EndsWith(name)).Status, Is.EqualTo(LawStatus.Pass), name);
        Assert.That(results.Single(r => r.Name.EndsWith("Law_BytesParameterIsBytes")).Trials, Is.GreaterThan(1), "sampled, not one value");
    }

    [Test]
    public void At_outside_the_view_is_an_error_not_a_read_of_the_array()
        => Assert.That(Run().Single(r => r.Name.EndsWith("Law_AtPastTheViewIsAFault")).Status, Is.EqualTo(LawStatus.Error));

    [Test]
    public void A_slice_shares_the_array_and_equals_a_copy_of_its_bytes()
    {
        var array = new byte[] { 120, 97, 98, 99, 120 };
        var view = new PlatoBytes(array, 0, 5).Sub(1, 3);
        Assert.That(view.Count, Is.EqualTo(3));
        array[1] = 122;
        Assert.That(view.At(0), Is.EqualTo((byte)122));
        Assert.That(view.Equals(new PlatoBytes(new byte[] { 122, 98, 99 }, 0, 3)), Is.True);
        Assert.That(view.GetHashCode(), Is.EqualTo(new PlatoBytes(new byte[] { 122, 98, 99 }, 0, 3).GetHashCode()));
        Assert.That(view.Sub(-1, 3).Count, Is.EqualTo(2));
        Assert.That(view.Sub(3, 1).Count, Is.EqualTo(0));
    }
}
