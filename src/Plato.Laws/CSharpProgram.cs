using System.Diagnostics;
using System.Reflection;
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
/// generated project would set. The assembly is loaded from memory; its types are reached by
/// reflection only (<see cref="CSharpBackend"/>, <see cref="HostValues"/>).
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

    private CSharpProgram(NumberProfile profile, Assembly assembly, int fileCount, IReadOnlyList<string> degraded, TimeSpan emit, TimeSpan compile)
        => (Profile, Assembly, FileCount, DegradedBodies, EmitElapsed, CompileElapsed) = (profile, assembly, fileCount, degraded, emit, compile);

    // What <ImplicitUsings>enable</ImplicitUsings> gives the generated project.
    private const string ImplicitUsings = "global using System; global using System.Collections.Generic; global using System.IO; global using System.Linq; global using System.Net.Http; global using System.Threading; global using System.Threading.Tasks;";

    /// <summary>Emits and compiles; throws <see cref="InvalidOperationException"/> listing the
    /// first C# errors when the generated program does not compile.</summary>
    public static CSharpProgram Build(PlatoCompilation comp, NumberProfile profile)
    {
        var sw = Stopwatch.StartNew();
        var writer = new CSharpWriter.CSharpWriter(comp, "unused-in-memory") { ExtensionStyle = true, Profile = profile };
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
        var assembly = Assembly.Load(ms.ToArray());
        return new CSharpProgram(profile, assembly, writer.Files.Count, writer.DegradedBodies.ToList(), emit, sw.Elapsed);
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
