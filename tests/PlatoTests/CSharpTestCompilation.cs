// Compiles generated C# against the runtime embedded in the test assembly.
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;

namespace PlatoTests;

internal static class CSharpTestCompilation
{
    private static readonly Lazy<IReadOnlyList<MetadataReference>> References = new(() =>
        ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!)
        .Split(Path.PathSeparator)
        .Where(File.Exists)
        // The shipping package duplicates the wrapper types embedded in this test assembly.
        .Where(p => !string.Equals(Path.GetFileName(p), "Ara3D.Geometry.dll", StringComparison.OrdinalIgnoreCase))
        .Select(p => (MetadataReference)MetadataReference.CreateFromFile(p))
        .ToArray());

    public static Assembly CompileAndLoad(params string[] sources)
        => CompileAndLoad(sources, LanguageVersion.Default, OptimizationLevel.Debug);

    public static Assembly CompileAndLoad(IReadOnlyList<string> sources, LanguageVersion languageVersion,
        OptimizationLevel optimizationLevel)
    {
        var trees = sources.Select((source, i) => CSharpSyntaxTree.ParseText(source,
            new CSharpParseOptions(languageVersion), path: $"generated-{i}.cs"));
        var compilation = CSharpCompilation.Create("GeneratedTests_" + Guid.NewGuid().ToString("N"),
            trees, References.Value,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, optimizationLevel: optimizationLevel));
        using var stream = new MemoryStream();
        var result = compilation.Emit(stream);
        Assert.That(result.Success, Is.True, () => "Emitted C# did not compile:\n"
            + string.Join("\n", result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error))
            + "\n--- source ---\n" + string.Join("\n", sources));
        return Assembly.Load(stream.ToArray());
    }
}
