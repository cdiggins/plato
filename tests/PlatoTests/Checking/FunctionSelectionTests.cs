using Ara3D.Geometry.AST;
using Ara3D.Geometry.Compiler;
using Ara3D.Geometry.Compiler.Analysis;
using Ara3D.Geometry.Compiler.Checking;
using Ara3D.Geometry.CSharpWriter;
using Ara3D.Parakeet;
using Ara3D.Parsing;
using NUnit.Framework;

namespace PlatoTests;

[TestFixture]
public class FunctionSelectionTests
{
    private const string Source = """
        primitive Number { }
        interface IBase { Declared(x: Self): Self; }
        interface IChild inherits IBase { }
        interface IBranch inherits IBase { }
        type Item implements IBranch, IChild { Value: Number; Other: Number; }
        type OverrideItem implements IBranch, IChild { Value: Number; Other: Number; }
        library Intrinsics { Native(n: Number): Number; }
        library Choices
        {
            BaseChoice(x: IBase): IBase => x;
            Choose(x: IBase): Number => 11;
            Choose(x: IChild): Number => 22;
            Choose(x: OverrideItem): Number => 33;
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
        var child = Candidate("ChildChoice");
        var ancestor = Candidate("BaseChoice");
        Assert.That(FunctionSelection.ScoreFunction(concrete), Is.EqualTo(-100));
        Assert.That(FunctionSelection.ScoreFunction(child), Is.LessThan(FunctionSelection.ScoreFunction(ancestor)));
        Assert.That(FunctionSelection.ChooseBestFunction(new[] { ancestor, child }, out var count), Is.SameAs(child));
        Assert.That(count, Is.EqualTo(1));
        Assert.That(FunctionSelection.ChooseBestFunction(new[] { ancestor, child, concrete }, out count), Is.SameAs(concrete));
        Assert.That(count, Is.EqualTo(1));
    }

    [Test]
    public void BoundDeclarationsAndImplementationsCarryTheirBindingInterface()
    {
        var child = Candidate("ChildChoice");
        Assert.That(child.Interface, Is.SameAs(child.InterfaceImplementation.Interface));
        Assert.That(child.InterfaceName, Is.EqualTo("IChild"));
        var declaration = _item.DeclaredFunctions.Single(f => f.Name == "Declared");
        Assert.That(declaration.Interface, Is.SameAs(declaration.InterfaceImplementation.Interface));
        Assert.That(declaration.InterfaceName, Is.EqualTo("IBase"));
        Assert.That(Candidate("ConcreteA").Interface, Is.Null);
    }

    [Test]
    public void StandaloneInterfaceFunctionsHaveNoConcreteBinding()
    {
        var declaration = _item.DeclaredFunctions.Single(f => f.Name == "Declared");
        var standalone = new FunctionInstance(declaration.Implementation, null, null, FunctionInstanceKind.InterfaceDeclared);
        var extension = new FunctionInstance(Candidate("ChildChoice").Implementation, null, null, FunctionInstanceKind.InterfaceExtension);
        Assert.That(standalone.Interface, Is.Null);
        Assert.That(extension.Interface, Is.Null);
    }

    [Test]
    public void SameSignatureChoosesChildRegardlessOfOrderAndConcreteOverrideStillWins()
    {
        var group = _item.InterfaceFunctionGroups.Single(g => g[0].Name == "Choose");
        Assert.That(group.Select(f => f.Implementation.Id).Distinct().Count(), Is.EqualTo(2));
        foreach (var candidates in new[] { group.ToArray(), group.AsEnumerable().Reverse().ToArray() })
        {
            var best = FunctionSelection.ChooseBestFunction(candidates, out var count);
            Assert.That(best.Implementation.Parameters[0].Type.Name, Is.EqualTo("IChild"));
            Assert.That(best.InterfaceName, Is.EqualTo("IChild"));
            Assert.That(count, Is.EqualTo(1));
        }
        var overridden = _compilation.ConcreteTypes.Single(t => t.Name == "OverrideItem")
            .InterfaceFunctionGroups.Single(g => g[0].Name == "Choose");
        var concrete = FunctionSelection.ChooseBestFunction(overridden.AsEnumerable().Reverse().ToArray(), out var ties);
        Assert.That(concrete.Interface, Is.Null);
        Assert.That(concrete.Implementation.Parameters[0].Type.Name, Is.EqualTo("OverrideItem"));
        Assert.That(ties, Is.EqualTo(1));
    }

    [Test]
    public void ReificationKeepsTheSelectedBodyAndGeneratedCSharpExecutesIt()
    {
        var group = _item.InterfaceFunctionGroups.Single(g => g[0].Name == "Choose");
        var selected = FunctionSelection.ChooseBestFunction(group, out _);
        var reified = _compilation.ReifiedFunctions.Single(f => f.Name == "Choose" && f.ReifiedType.Name == "Item");
        Assert.That(reified.Original, Is.SameAs(selected.Implementation));
        var mono = new Monomorphizer(_compilation).MonomorphizeAll()
            .Single(f => f.Original == selected.Implementation && f.ConcreteType.Name == "Item");
        Assert.That(mono.IsFullyGround, Is.True);

        var writer = new CSharpWriter(_compilation, "unused-selection-codegen") { ExtensionStyle = true };
        writer.WriteAll();
        Assert.That(writer.DegradedBodies, Is.Empty);
        var assembly = CSharpTestCompilation.CompileAndLoad(GeneratedCSharpFixture.Prelude,
            writer.Files["_Item.g.cs"].ToString(), writer.Files["_OverrideItem.g.cs"].ToString(),
            writer.Files["Interfaces.g.cs"].ToString(), writer.Files["Choices.g.cs"].ToString());
        foreach (var (name, expected) in new[] { ("Item", 22d), ("OverrideItem", 33d) })
        {
            var type = assembly.GetType("Ara3D.Geometry." + name, true)!;
            var method = assembly.GetType("Ara3D.Geometry.Choices", true)!.GetMethods()
                .Single(m => m.Name == "Choose" && m.GetParameters()[0].ParameterType == type);
            var result = GeneratedValueReflection.Call(method, null, GeneratedValueReflection.New(type, 0d, 0d));
            Assert.That(result, Is.EqualTo(expected), name);
        }
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
