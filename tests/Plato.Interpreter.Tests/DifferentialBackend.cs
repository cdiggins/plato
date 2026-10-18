using System.Diagnostics;
using System.Reflection;
using PlatoCompilation = Ara3D.Geometry.Compiler.Compilation;
using Ara3D.Geometry.Compiler.Types;
using Ara3D.Geometry.CSharpWriter;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Plato.Interpreter.Tests;

/// <summary>
/// The C# arm: the program emitted in extension style for one Number profile and compiled in
/// process with Roslyn, together with the handwritten runtime (src/Plato.Intrinsics/*.cs) under
/// the preprocessor symbols the generated project would set. The assembly is loaded from memory;
/// its types are reached by reflection only.
/// </summary>
public sealed class DifferentialBackend
{
    public NumberProfile Profile { get; }
    public Assembly Assembly { get; }
    public int FileCount { get; }
    public IReadOnlyList<string> DegradedBodies { get; }
    public TimeSpan EmitElapsed { get; }
    public TimeSpan CompileElapsed { get; }

    private DifferentialBackend(NumberProfile profile, Assembly assembly, int fileCount, IReadOnlyList<string> degraded, TimeSpan emit, TimeSpan compile)
        => (Profile, Assembly, FileCount, DegradedBodies, EmitElapsed, CompileElapsed) = (profile, assembly, fileCount, degraded, emit, compile);

    // What <ImplicitUsings>enable</ImplicitUsings> gives the generated project.
    private const string ImplicitUsings = "global using System; global using System.Collections.Generic; global using System.IO; global using System.Linq; global using System.Net.Http; global using System.Threading; global using System.Threading.Tasks;";

    public static DifferentialBackend Build(PlatoCompilation comp, NumberProfile profile)
    {
        var sw = Stopwatch.StartNew();
        var writer = new CSharpWriter(comp, "unused-differential") { ExtensionStyle = true, Profile = profile };
        writer.WriteAll();
        var emit = sw.Elapsed;

        sw.Restart();
        var symbols = new List<string> { "PLATO_FORWARD_CONCEPTS" };
        if (profile == NumberProfile.Gpu32)
            symbols.Add("PLATO_NUMBER_GPU32");
        var parse = new CSharpParseOptions(preprocessorSymbols: symbols);
        var trees = writer.Files
            .Select(kv => CSharpSyntaxTree.ParseText(kv.Value.ToString(), parse, kv.Key))
            .Concat(IntrinsicsSources().Select(p => CSharpSyntaxTree.ParseText(File.ReadAllText(p), parse, p)))
            .Append(CSharpSyntaxTree.ParseText(ImplicitUsings, parse, "ImplicitUsings.cs"))
            .ToList();
        var compilation = CSharpCompilation.Create(
            $"PlatoDifferential_{profile.Name}_{Guid.NewGuid():N}",
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
        var assembly = Assembly.Load(ms.ToArray());
        return new DifferentialBackend(profile, assembly, writer.Files.Count, writer.DegradedBodies.ToList(), emit, sw.Elapsed);
    }

    private static IEnumerable<string> IntrinsicsSources()
        => Directory.GetFiles(Path.Combine(DifferentialProgram.RepoRoot(), "src", "Plato.Intrinsics"), "*.cs", SearchOption.TopDirectoryOnly);

    // The trusted platform assemblies minus the stale Ara3D.Geometry package, which declares the
    // same wrapper scalars the generated code does (see SumTypeCodegenTests.CompileAndLoad).
    private static IReadOnlyList<MetadataReference> References()
        => ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES"))
            .Split(Path.PathSeparator)
            .Where(p => !string.IsNullOrEmpty(p) && File.Exists(p))
            .Where(p => !string.Equals(Path.GetFileName(p), "Ara3D.Geometry.dll", StringComparison.OrdinalIgnoreCase))
            .Select(p => (MetadataReference)MetadataReference.CreateFromFile(p))
            .ToList();
}
