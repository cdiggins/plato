using Ara3D.Geometry.Compiler.Checking;
using Ara3D.Geometry.Compiler.Types;
using Ara3D.Geometry.Interpreter;
using Ara3D.Geometry.Laws;

namespace Plato.Interpreter.Tests;

/// <summary>
/// TKT-510: an interface method's Self, once an argument has bound it to a concrete type, accepts
/// only that type or a conversion to it. `SampleIx(k) == k` bound Self to SampleIx and took the bare
/// Integer too; the C# writer emitted `new SampleIx(k).Equals(k)`, which bound
/// SampleIx.Equals(object) and was false for equal values, while the interpreter answered true. A
/// wrapper compared with its payload is now a type error (compare .Value); a comparison of two
/// values of one type is unchanged and agrees on both backends.
/// </summary>
[TestFixture]
public class SelfBindingTests
{
    private const string Source = """
        type SampleIx
            implements IValue, IHashable, IComparable, IIndex
        {
            Value: Integer;
        }

        type IxPair
            implements IValue
        {
            From: SampleIx;
            To: SampleIx;
        }

        library SelfBindingFixture
        {
            Law_SameIndexIsEqual(k: Integer): Boolean
                => SampleIx(k) == SampleIx(k);
            Law_SameIndexInLambdaAndChain(k: Integer): Boolean
                => [IxPair(SampleIx(k), SampleIx(k + 1))].Any(c => c.From == SampleIx(k) && c.To == SampleIx(k + 1));
            Law_ValuesAreEqual(k: Integer): Boolean
                => SampleIx(k).Value == k;
        }

        library SelfBindingMixed
        {
            WrapperEqualsPayload(k: Integer): Boolean
                => SampleIx(k) == k;
            PayloadEqualsWrapper(k: Integer): Boolean
                => k == SampleIx(k);
        }
        """;

    [Test]
    public void A_wrapper_compared_with_its_payload_is_a_type_error()
    {
        var compilation = LawRunnerSelectionTests.CompileSource(Source);
        var errors = new TypeChecker(compilation).CheckAll()
            .Where(r => r.Function?.OwnerType?.Name == "SelfBindingMixed" || r.Function?.OwnerType?.Name == "SelfBindingFixture")
            .SelectMany(r => r.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Select(d => (r.Function.Name, d.Code, d.Message)))
            .ToList();
        TestContext.Out.WriteLine(string.Join("\n", errors));
        Assert.That(errors.Where(e => e.Name == "WrapperEqualsPayload").Select(e => e.Code), Has.Member("CHK201"));
        Assert.That(errors.Where(e => e.Name == "PayloadEqualsWrapper").Select(e => e.Code), Has.Member("CHK201"));
        Assert.That(errors.Where(e => e.Name.StartsWith("Law_")), Is.Empty, "same-type comparisons check clean");
    }

    [Test]
    public void Same_type_comparisons_agree_on_both_backends()
    {
        var compilation = LawRunnerSelectionTests.CompileSource(Source.Replace("""
        library SelfBindingMixed
        {
            WrapperEqualsPayload(k: Integer): Boolean
                => SampleIx(k) == k;
            PayloadEqualsWrapper(k: Integer): Boolean
                => k == SampleIx(k);
        }
        """, ""));
        var interp = new Ara3D.Geometry.Interpreter.Interpreter(compilation, NumberProfile.Cpu64);
        var options = new LawRunOptions { Filters = new[] { "SelfBindingFixture" } };
        var backend = new DifferentialBackend(new InterpreterBackend(interp), CSharpBackend.Build(interp));
        var results = new LawRunner(backend, interp, options).RunAll();
        TestContext.Out.WriteLine(string.Join("\n", results.Select(r => r.ToString())));
        Assert.That(results, Has.Count.EqualTo(3));
        Assert.That(results.Select(r => r.Status), Has.All.EqualTo(LawStatus.Pass));
        Assert.That(backend.Mismatches, Is.Empty);
    }
}
