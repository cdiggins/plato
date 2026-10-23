using System.Diagnostics;
using System.Reflection;
using System.Runtime.Loader;
using PlatoCompilation = Ara3D.Geometry.Compiler.Compilation;
using Ara3D.Geometry.Compiler.Types;
using Ara3D.Geometry.CSharpWriter;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Ara3D.Geometry.Laws;

/// <summary>
/// A Plato program as a loaded .NET assembly: emitted in extension style for one Number profile
/// and compiled in process with Roslyn, together with the handwritten runtime
/// (src/Plato.Intrinsics/*.cs, embedded in this assembly) under the preprocessor symbols the
/// generated project would set. The assembly is loaded from memory into a collectible load
/// context of its own, so a long-lived host (the warm law server) can <see cref="Unload"/> a
/// program it no longer needs; its types are reached by reflection only
/// (<see cref="CSharpBackend"/>, <see cref="HostValues"/>).
/// </summary>
public sealed class CSharpProgram
{
    /// <summary>The namespace the writer puts every generated type in.</summary>
    public const string Namespace = "Ara3D.Geometry";

    public NumberProfile Profile { get; }
    public Assembly Assembly { get; }
    public int FileCount { get; }
    public IReadOnlyList<string> DegradedBodies { get; }
    public TimeSpan EmitElapsed { get; }
    public TimeSpan CompileElapsed { get; }
    private readonly AssemblyLoadContext _context;

    private CSharpProgram(NumberProfile profile, AssemblyLoadContext context, Assembly assembly, int fileCount, IReadOnlyList<string> degraded, TimeSpan emit, TimeSpan compile)
        => (Profile, _context, Assembly, FileCount, DegradedBodies, EmitElapsed, CompileElapsed) = (profile, context, assembly, fileCount, degraded, emit, compile);

    /// <summary>Starts unloading the program's assembly (TKT-479). The memory comes back once no
    /// object, delegate or running thread refers to its code: a law still running on an abandoned
    /// thread keeps it until that law returns. The program must not be used afterwards.</summary>
    public void Unload() => _context.Unload();

    /// <summary>A weak reference to the load context, for a test that checks it was collected.</summary>
    public WeakReference ContextReference() => new(_context);

    // What <ImplicitUsings>enable</ImplicitUsings> gives the generated project.
    private const string ImplicitUsings = "global using System; global using System.Collections.Generic; global using System.IO; global using System.Linq; global using System.Net.Http; global using System.Threading; global using System.Threading.Tasks;";

    /// <summary>Emits and compiles; throws <see cref="InvalidOperationException"/> listing the
    /// first C# errors when the generated program does not compile. <paramref name="lowerLoops"/>
    /// is the CLI's <c>--loops</c> (TKT-505): a differential run then checks the lowered loops
    /// against the interpreter.</summary>
    public static CSharpProgram Build(PlatoCompilation comp, NumberProfile profile, bool lowerLoops = false)
    {
        var sw = Stopwatch.StartNew();
        var writer = new CSharpWriter.CSharpWriter(comp, "unused-in-memory") { ExtensionStyle = true, Profile = profile, LowerLoops = lowerLoops };
        writer.WriteAll();
        var emit = sw.Elapsed;

        sw.Restart();
        var symbols = new List<string> { "PLATO_FORWARD_CONCEPTS" };
        if (profile == NumberProfile.Gpu32)
            symbols.Add("PLATO_NUMBER_GPU32");
        var parse = new CSharpParseOptions(preprocessorSymbols: symbols);
        var trees = writer.Files
            .Select(kv => CSharpSyntaxTree.ParseText(kv.Value.ToString(), parse, kv.Key))
            .Concat(IntrinsicsSources().Select(s => CSharpSyntaxTree.ParseText(s.Text, parse, s.Name)))
            .Append(CSharpSyntaxTree.ParseText(ImplicitUsings, parse, "ImplicitUsings.cs"))
            .ToList();
        var compilation = CSharpCompilation.Create(
            $"PlatoLaws_{profile.Name}_{Guid.NewGuid():N}",
            trees, References(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true, optimizationLevel: OptimizationLevel.Release));
        using var ms = new MemoryStream();
        var result = compilation.Emit(ms);
        if (!result.Success)
        {
            var errors = result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToList();
            throw new InvalidOperationException($"the generated C# did not compile under {profile} ({errors.Count} errors):\n"
                + string.Join("\n", errors.Take(40).Select(d => d.ToString())));
        }
        // A collectible context per program: Assembly.Load(bytes) put every program in the default
        // context, which never unloads, so a warm server kept each recompiled program forever.
        var context = new AssemblyLoadContext(compilation.AssemblyName, isCollectible: true);
        ms.Position = 0;
        var assembly = context.LoadFromStream(ms);
        return new CSharpProgram(profile, context, assembly, writer.Files.Count, writer.DegradedBodies.ToList(), emit, sw.Elapsed);
    }

    /// <summary>The handwritten runtime, from this assembly's embedded copy of src/Plato.Intrinsics.</summary>
    public static IEnumerable<(string Name, string Text)> IntrinsicsSources()
    {
        var asm = typeof(CSharpProgram).Assembly;
        foreach (var name in asm.GetManifestResourceNames().Where(n => n.StartsWith("Intrinsics/", StringComparison.Ordinal)).OrderBy(n => n, StringComparer.Ordinal))
        {
            using var reader = new StreamReader(asm.GetManifestResourceStream(name)!);
            yield return (name, reader.ReadToEnd());
        }
    }

    // The trusted platform assemblies minus the stale Ara3D.Geometry package, which declares the
    // same wrapper scalars the generated code does.
    private static IReadOnlyList<MetadataReference> References()
        => ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES"))
            .Split(Path.PathSeparator)
            .Where(p => !string.IsNullOrEmpty(p) && File.Exists(p))
            .Where(p => !string.Equals(Path.GetFileName(p), "Ara3D.Geometry.dll", StringComparison.OrdinalIgnoreCase))
            .Select(p => (MetadataReference)MetadataReference.CreateFromFile(p))
            .ToList();
}
