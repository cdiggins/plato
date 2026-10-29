// Checks that a program emitted under --lean-inlining compiles and runs on the C# backend.
using Ara3D.Geometry.Compiler.Types;
using Ara3D.Geometry.CSharpWriter;
using Ara3D.Geometry.Laws;

namespace Plato.Interpreter.Tests;

/// <summary>TKT-1091: the switch reaches the law runner's writer through
/// <see cref="LoopOptimizationOptions"/>, and the attribute-free output still compiles against the
/// handwritten runtime.</summary>
[TestFixture, Category("Slow")]
public class LeanInliningProgramTests
{
    private const string Source = """
        library LeanFixture
        {
            Twice(x: Number): Number => x + x;
            Law_Twice(x: Number): Boolean => Twice(x) == x * 2.0;
        }
        """;

    [Test]
    public void A_lean_program_compiles()
    {
        var program = CSharpProgram.Build(LawRunnerSelectionTests.CompileSource(Source), NumberProfile.Cpu64,
            lowerLoops: true, loopOptimizations: new LoopOptimizationOptions(LeanInlining: true));
        try
        {
            Assert.That(program.DegradedBodies, Is.Empty);
            Assert.That(program.Assembly.GetTypes(), Is.Not.Empty);
        }
        finally
        {
            program.Unload();
        }
    }
}
