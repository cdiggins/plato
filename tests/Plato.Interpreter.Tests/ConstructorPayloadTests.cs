using System.Reflection;
using Ara3D.Geometry.Compiler.Checking;
using Ara3D.Geometry.Compiler.Types;
using Ara3D.Geometry.Laws;

namespace Plato.Interpreter.Tests;

[TestFixture]
[NonParallelizable]
public class ConstructorPayloadTests
{
    private static MonomorphizedFunction Helper()
    {
        var source = File.ReadAllText(Path.Combine(TestContext.CurrentContext.TestDirectory,
            "fixtures", "generic-concrete-first.plato"));
        var compilation = LawRunnerSelectionTests.CompileSource(source);
        return new Monomorphizer(compilation).MonomorphizeAll().Single(m =>
            m.Original?.Name == "CountedFlips" && m.ConcreteType?.Name == "Integer");
    }

    private static TirCall Constructor(MonomorphizedFunction helper)
        => helper.Tir.AllNodes.OfType<TirCall>().Single(c => c.EmissionKind == EmissionKind.Constructor);

    private static TirFunction Body(MonomorphizedFunction helper, TirNode body)
        => new(helper.Original, helper.Tir.Parameters, helper.Tir.ReturnType, body);

    [Test]
    public void Constructor_payload_is_open_generic_and_emittable()
    {
        var helper = Helper();
        var constructor = Constructor(helper);
        Assert.That(helper.IsFullyGround, Is.False);
        Assert.That(TypeSubstitution.IsGround(constructor.Args[0].Type), Is.False);
        Assert.That(TirEmitSource.IsOpenGenericEmittable(helper.Tir, helper.Original), Is.True);
    }

    [Test]
    public void Constructor_does_not_hide_unresolved_argument()
    {
        var helper = Helper();
        var constructor = Constructor(helper);
        var args = constructor.Args.ToArray();
        args[0] = new TirUnresolved(null, "structural negative control");
        Assert.That(TirEmitSource.IsOpenGenericEmittable(
            Body(helper, constructor.WithChildren(args)), helper.Original), Is.False);
    }

    [Test]
    public void Constructor_does_not_license_nested_abstract_dispatch()
    {
        var helper = Helper();
        var constructor = Constructor(helper);
        var payload = constructor.Args[0];
        var dispatch = new TirCall(null, EmissionKind.InstanceMethod,
            new[] { payload.Type }, payload.Type, new[] { payload }, payload.Type, null, "Unlicensed");
        Assert.That(TirEmitSource.IsOpenGenericEmittable(Body(helper, dispatch), helper.Original), Is.False);
        var args = constructor.Args.ToArray();
        args[0] = dispatch;
        Assert.That(TirEmitSource.IsOpenGenericEmittable(
            Body(helper, constructor.WithChildren(args)), helper.Original), Is.False);
    }

    [Test]
    public void Emitted_helper_remains_a_generic_definition()
    {
        var source = File.ReadAllText(Path.Combine(TestContext.CurrentContext.TestDirectory,
            "fixtures", "generic-concrete-first.plato"));
        var compilation = LawRunnerSelectionTests.CompileSource(source);
        var helper = new Monomorphizer(compilation).MonomorphizeAll().Single(m =>
            m.Original?.Name == "CountedFlips" && m.ConcreteType?.Name == "Integer");
        Assert.That(new TirEmitSource(compilation).TryGetOpenGenericTir(helper.Original, "Integer"), Is.Not.Null);
        var interpreter = new Ara3D.Geometry.Interpreter.Interpreter(compilation, NumberProfile.Cpu64);
        var compiled = CSharpBackend.Build(interpreter);
        var methods = compiled.Program.Assembly.GetTypes().SelectMany(t => t.GetMethods(
            BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance))
            .Where(m => m.Name == "CountedFlips").ToArray();
        Assert.That(methods, Is.Not.Empty);
        Assert.That(methods.Select(m => m.IsGenericMethodDefinition), Is.All.True);
    }
}
