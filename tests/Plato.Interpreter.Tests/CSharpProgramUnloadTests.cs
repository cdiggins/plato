// Checks unloading of dynamically compiled law assemblies.
using System.Runtime.CompilerServices;
using Ara3D.Geometry.Compiler.Types;
using Ara3D.Geometry.Laws;

namespace Plato.Interpreter.Tests;

/// <summary>
/// TKT-479: a compiled law program lives in a collectible load context, so a long-lived host can
/// unload it. Loaded into the default context, every program the warm server recompiled stayed in
/// memory for the server's life (15 GB after three hours of edits on 2026-10-22).
/// </summary>
[TestFixture]
public class CSharpProgramUnloadTests
{
    private const string Source = """
        library UnloadFixture
        {
            Law_Holds(b: Boolean): Boolean => true;
        }
        """;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference BuildAndUnload()
    {
        var program = CSharpProgram.Build(LawRunnerSelectionTests.CompileSource(Source), NumberProfile.Cpu64);
        Assert.That(program.Assembly.GetTypes(), Is.Not.Empty);
        var context = program.ContextReference();
        program.Unload();
        return context;
    }

    [Test]
    public void An_unloaded_program_is_collected()
    {
        var context = BuildAndUnload();
        for (var i = 0; i < 20 && context.IsAlive; i++)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
        Assert.That(context.IsAlive, Is.False, "the program's load context is still alive after Unload");
    }
}
