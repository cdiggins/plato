// Checks array equality agrees across law execution backends.
using Ara3D.Geometry.Compiler.Types;
using Ara3D.Geometry.Interpreter;
using Ara3D.Geometry.Laws;

namespace Plato.Interpreter.Tests;

/// <summary>
/// TKT-355: a record or sum value that holds an array equals a copy built separately, on both
/// backends. The interpreter compared arrays element by element and the generated C# compared an
/// IReadOnlyList field by reference, so a B-spline model built twice differed from itself
/// compiled (Law_WitnessIsSameAsItself on the STEP mesher's witness 7).
/// </summary>
[TestFixture]
public class ArrayEqualityAgreementTests
{
    private const string Source = """
        type Polyline
        {
            Points: Array<Number>;
        }

        type Ragged
        {
            Rows: Array<Array<Number>>;
        }

        type Path
            = Empty
            | Through(Points: Array<Number>);

        library ArrayEqualityFixture
        {
            Built(x: Number): Polyline => new Polyline([x, 2.0, 3.0]);
            BuiltRagged(x: Number): Ragged => new Ragged([[x], [2.0, 3.0]]);
            BuiltPath(x: Number): Path => Through([x, 2.0]);
            Law_RecordWithArrayEqualsACopy(x: Number): Boolean => Built(x).Equals(Built(x));
            Law_NestedArraysEqualACopy(x: Number): Boolean => BuiltRagged(x).Equals(BuiltRagged(x));
            Law_SumWithArrayEqualsACopy(x: Number): Boolean => BuiltPath(x) == BuiltPath(x);
            Law_DifferentElementsDiffer(x: Number): Boolean => !Built(x).Equals(Built(x + 1.0)) || x + 1.0 == x;
        }
        """;

    [Test]
    public void Both_backends_pass_every_array_equality_law()
    {
        var compilation = LawRunnerSelectionTests.CompileSource(Source);
        var options = new LawRunOptions { Filters = new[] { "ArrayEqualityFixture" } };
        var interp = new Ara3D.Geometry.Interpreter.Interpreter(compilation, NumberProfile.Cpu64);
        var interpreted = new LawRunner(interp, options).RunAll();
        var compiled = new LawRunner(LawBackends.Create(LawBackendKind.CSharp, interp), interp, options).RunAll();
        string Verdicts(IEnumerable<LawResult> rs)
            => string.Join("\n", rs.OrderBy(r => r.Name, StringComparer.Ordinal).Select(r => $"{r.Name} {r.Status} {r.Message}"));
        TestContext.Out.WriteLine(Verdicts(compiled));
        Assert.That(compiled, Has.Count.EqualTo(4));
        Assert.That(compiled.Select(r => r.Status), Has.All.EqualTo(LawStatus.Pass), Verdicts(compiled));
        Assert.That(interpreted.Select(r => r.Status), Has.All.EqualTo(LawStatus.Pass), Verdicts(interpreted));
    }
}
