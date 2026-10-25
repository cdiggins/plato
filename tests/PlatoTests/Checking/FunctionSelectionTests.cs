using Ara3D.Geometry.AST;
using Ara3D.Geometry.Compiler;
using Ara3D.Geometry.Compiler.Analysis;
using Ara3D.Parakeet;
using Ara3D.Parsing;
using NUnit.Framework;

namespace PlatoTests;

[TestFixture]
public class FunctionSelectionTests
{
    private const string Source = """
        primitive Number { }
        interface IBase { }
        interface IChild inherits IBase { }
        type Item implements IChild { }
        library Intrinsics { Native(n: Number): Number; }
        library Choices
        {
            BaseChoice(x: IBase): IBase => x;
            ChildChoice(x: IChild): IChild => x;
            ConcreteA(x: Item): Item => x;
            ConcreteB(x: Item): Item => x;
        }
        """;

    private Compilation _compilation = null!;
    private ConcreteType _item = null!;

    [OneTimeSetUp]
    public void CompileCandidates()
    {
        var parser = CommonParsers.PlatoParser(new ParserInput(Source, "selection.plato"), Ara3D.Logging.Logger.Null);
        Assert.That(parser.Succeeded, Is.True, parser.ParserErrorsString);
        _compilation = new Compilation(Ara3D.Logging.Logger.Null, new[] { parser.Cst!.ToAst() });
        Assert.That(_compilation.SymbolFactory.Errors, Is.Empty);
        _item = _compilation.ConcreteTypes.Single(t => t.Name == "Item");
    }

    private FunctionInstance Candidate(string name)
        => _item.ImplementedFunctions.Single(f => f.Name == name);

    [Test]
    public void IntrinsicsWinBeforeConcreteAndInterfaces()
    {
        var native = _compilation.ConcreteTypes.Single(t => t.Name == "Number")
            .ConcreteFunctions.Single(f => f.Name == "Native");
        Assert.That(FunctionSelection.ScoreFunction(native), Is.EqualTo(-200));
        Assert.That(FunctionSelection.ChooseBestFunction(
            new[] { Candidate("BaseChoice"), Candidate("ConcreteA"), native }, out var count), Is.SameAs(native));
        Assert.That(count, Is.EqualTo(1));
    }

    [Test]
    public void ConcreteWinsBeforeInterfacesAndNearerInterfaceWinsBeforeAncestor()
    {
        var concrete = Candidate("ConcreteA");
        var child = CopyInterfaceCandidate("ChildChoice");
        var ancestor = CopyInterfaceCandidate("BaseChoice");
        // compiler-496: normal construction leaves Interface null. Supply it only to
        // exercise the existing distance branch without changing production behavior.
        var field = typeof(FunctionInstance).GetField("<Interface>k__BackingField",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!;
        field.SetValue(child, child.InterfaceImplementation.Interface);
        field.SetValue(ancestor, ancestor.InterfaceImplementation.Interface);
        Assert.That(FunctionSelection.ScoreFunction(concrete), Is.EqualTo(-100));
        Assert.That(FunctionSelection.ScoreFunction(child), Is.LessThan(FunctionSelection.ScoreFunction(ancestor)));
        Assert.That(FunctionSelection.ChooseBestFunction(new[] { ancestor, child }, out var count), Is.SameAs(child));
        Assert.That(count, Is.EqualTo(1));
        Assert.That(FunctionSelection.ChooseBestFunction(new[] { ancestor, child, concrete }, out count), Is.SameAs(concrete));
        Assert.That(count, Is.EqualTo(1));
    }

    private FunctionInstance CopyInterfaceCandidate(string name)
    {
        var candidate = Candidate(name);
        return candidate.InterfaceImplementation.AnalyzeConceptFunction(
            candidate.Implementation, FunctionInstanceKind.InterfaceImplemented);
    }

    [Test]
    public void ConstructedInterfaceCandidatesKeepTheirCurrentConcreteScore()
    {
        var implementation = _item.AllInterfaces.Single(i => i.Name == "IChild");
        var candidate = implementation.AnalyzeConceptFunction(
            Candidate("ChildChoice").Implementation, FunctionInstanceKind.InterfaceImplemented);
        Assert.That(candidate.Interface, Is.Null, "compiler-496 is a separate behavior change");
        Assert.That(FunctionSelection.ScoreFunction(candidate), Is.EqualTo(-100));
    }

    [Test]
    public void TiesKeepInputOrderAndCountDistinctImplementations()
    {
        var a = Candidate("ConcreteA");
        var duplicate = _item.CreateFunctionInstance(a.Implementation, FunctionInstanceKind.ConcreteType);
        var b = Candidate("ConcreteB");
        Assert.That(duplicate, Is.Not.SameAs(a));
        Assert.That(FunctionSelection.ChooseBestFunction(new[] { a, duplicate }, out var count), Is.SameAs(a));
        Assert.That(count, Is.EqualTo(1));
        Assert.That(FunctionSelection.ChooseBestFunction(new[] { b, a, duplicate }, out count), Is.SameAs(b));
        Assert.That(count, Is.EqualTo(2));
        Assert.That(FunctionSelection.ChooseBestFunction(new[] { a, b }, out count), Is.SameAs(a));
        Assert.That(count, Is.EqualTo(2));
    }

    [Test]
    public void EmptyCandidatesRetainTheFailureContract()
    {
        var count = 0;
        var error = Assert.Throws<Exception>(() => FunctionSelection.ChooseBestFunction(Array.Empty<FunctionInstance>(), out count));
        Assert.That(error!.Message, Is.EqualTo("No results: could not find a best function."));
        Assert.That(count, Is.EqualTo(1));
    }
}
