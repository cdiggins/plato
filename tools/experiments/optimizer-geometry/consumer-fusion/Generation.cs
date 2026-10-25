using System.Reflection;
using System.Security.Cryptography;
using System.Text.Json;
using Ara3D.Geometry.AST;
using Ara3D.Geometry.Compiler.Checking;
using Ara3D.Geometry.CSharpWriter;
using Ara3D.Logging;
using Ara3D.Parakeet;
using Ara3D.Parsing;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RuntimeNumber = Ara3D.Geometry.Number;

namespace ConsumerGeometry;

internal static class Generation
{
    internal delegate (RuntimeNumber First, RuntimeNumber Second) Kernel(
        IReadOnlyList<RuntimeNumber> xs, IReadOnlyList<RuntimeNumber> ys, IReadOnlyList<RuntimeNumber> zs,
        RuntimeNumber nx, RuntimeNumber ny, RuntimeNumber nz, RuntimeNumber offset);

    internal static AstNode Parse(string path)
    {
        var parser = CommonParsers.PlatoParser(new ParserInput(File.ReadAllText(path), path), Logger.Null);
        if (!parser.Succeeded) throw new Exception(string.Join("; ", parser.ErrorMessages));
        return parser.Cst.ToAst();
    }

    internal static Ara3D.Geometry.Compiler.Compilation Compilation(string stdlib, string? fixture = null, bool geometry = false)
    {
        var files = Directory.GetFiles(Path.Combine(stdlib, "foundation"), "*.plato").AsEnumerable();
        if (geometry) files = files.Concat(Directory.GetFiles(Path.Combine(stdlib, "geometry"), "*.plato"));
        if (fixture != null) files = files.Append(fixture);
        return new Ara3D.Geometry.Compiler.Compilation(Logger.Null, files.Select(Parse).ToList());
    }

    internal static Kernel Build(Ara3D.Geometry.Compiler.Compilation compilation, string evidence, bool enabled)
    {
        var writer = new CSharpWriter(compilation, "unused-geometry-consumer")
            { ExtensionStyle = true, LowerLoops = true, FuseConsumers = enabled };
        writer.WriteAll();
        var definition = compilation.LibraryDefinitionsByName["GeometryConsumerProbe"].Functions.Single();
        var raw = writer.TryGetStaticTir(definition) ?? throw new Exception("Missing geometry kernel TIR.");
        if (raw.AllNodes.OfType<TirUnresolved>().Any()) throw new Exception("Unresolved geometry kernel TIR.");
        var optimized = writer.RunOptimizerPasses(raw, null);
        var markers = optimized.AllNodes.OfType<TirMultiReduction>().Count();
        if (markers != (enabled ? 1 : 0)) throw new Exception($"Unexpected fused marker count: {markers}.");
        File.WriteAllText(Path.Combine(evidence, $"tir-{enabled}.txt"), $"RAW\n{raw.Body}\nLOWERED\n{optimized.Body}\n");
        var method = writer.Files.Where(f => f.Key is "GeometryConsumerProbe.g.cs" or "Extensions.g.cs")
            .SelectMany(f => CSharpSyntaxTree.ParseText(f.Value.ToString()).GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>())
            .Single(m => m.Identifier.Text == "PlaneMoments");
        var loopCount = method.DescendantNodes().OfType<ForStatementSyntax>().Count();
        if (loopCount != (enabled ? 2 : 3)) throw new Exception($"Unexpected loop count: {loopCount}.");
        var source = "using System; using System.Collections.Generic; using System.Runtime.CompilerServices; "
            + "using static System.Runtime.CompilerServices.MethodImplOptions; namespace Ara3D.Geometry { public static class GeometryKernel { "
            + method.ToFullString() + string.Join("\n", NumberAdapters(writer))
            + @"public static (Number, Number) Run(IReadOnlyList<Number> xs, IReadOnlyList<Number> ys, IReadOnlyList<Number> zs,
Number nx, Number ny, Number nz, Number offset) { var result = PlaneMoments(xs, ys, zs, nx, ny, nz, offset); return (result.First, result.Second); } } }";
        var sources = new[] { source, writer.Files["_PlaneResidualMoments.g.cs"].ToString(), "global using System; global using System.Collections.Generic;" };
        for (var i = 0; i < sources.Length; i++) File.WriteAllText(Path.Combine(evidence, $"generated-{enabled}-{i}.cs"), sources[i]);
        var paths = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!).Split(Path.PathSeparator)
            .Append(typeof(RuntimeNumber).Assembly.Location).Where(File.Exists)
            .Where(p => Path.GetFileName(p) != "Ara3D.Geometry.dll").Distinct().ToList();
        var assembly = CSharpCompilation.Create("GeometryKernel_" + Guid.NewGuid().ToString("N"),
            sources.Select(s => CSharpSyntaxTree.ParseText(s, new CSharpParseOptions(LanguageVersion.Default))),
            paths.Select(p => MetadataReference.CreateFromFile(p)),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, optimizationLevel: OptimizationLevel.Release));
        using var stream = new MemoryStream();
        var result = assembly.Emit(stream);
        if (!result.Success) throw new Exception(string.Join("\n", result.Diagnostics.Where(d => d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error)));
        var loaded = Assembly.Load(stream.ToArray());
        Console.WriteLine($"Generated mode={enabled}, paired markers={markers}, loops={loopCount}, default language, Release.");
        return loaded.GetType("Ara3D.Geometry.GeometryKernel", true)!.GetMethod("Run")!.CreateDelegate<Kernel>();
    }

    private static IEnumerable<string> NumberAdapters(CSharpWriter writer)
    {
        // Keep the writer's operator bodies; adapt only instance receivers across the runtime assembly boundary.
        var methods = CSharpSyntaxTree.ParseText(writer.Files["_Number.g.cs"].ToString()).GetRoot()
            .DescendantNodes().OfType<MethodDeclarationSyntax>()
            .Where(m => m.Identifier.Text is "Add" or "Multiply"
                && !m.Modifiers.Any(SyntaxKind.StaticKeyword) && m.ParameterList.Parameters.Count == 1
                && m.ParameterList.Parameters[0].Type!.ToString() == "Number").ToList();
        if (methods.Count != 2) throw new Exception("Missing generated Number operator bodies.");
        var receiver = SyntaxFactory.Parameter(SyntaxFactory.Identifier("self"))
            .WithType(SyntaxFactory.ParseTypeName("Number"))
            .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.ThisKeyword)));
        return methods.Select(m => m.ReplaceNodes(m.DescendantNodes().OfType<ThisExpressionSyntax>(), (_, _) => SyntaxFactory.IdentifierName("self"))
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.StaticKeyword))
            .WithParameterList(m.ParameterList.WithParameters(m.ParameterList.Parameters.Insert(0, receiver)))
            .NormalizeWhitespace().ToFullString());
    }

    internal static void Census(string stdlib, string evidence)
    {
        var compilation = Compilation(stdlib, geometry: true);
        var writer = new CSharpWriter(compilation, "unused-geometry-census") { LowerLoops = true, FuseConsumers = true };
        var candidates = compilation.LibraryDefinitionsByName.Values.SelectMany(l => l.Functions)
            .Where(f => f.Name is "BoundsOfPoints" or "EdgeLengthRatio"
                || f.Name == "Eval" && f.Parameters[0].Type.Name == "Clothoid2D").ToList();
        var rows = new List<object>();
        foreach (var definition in candidates)
        {
            var tir = writer.TryGetStaticTir(definition) ?? throw new Exception("Missing candidate TIR: " + definition);
            var unresolved = tir.AllNodes.OfType<TirUnresolved>().Count();
            var paired = TirMultiConsumerFusion.Rewrite(tir, writer).AllNodes.OfType<TirMultiReduction>().Count();
            var label = definition.Name + "-" + definition.Parameters[0].Type.ToString().Replace('<', '_').Replace('>', '_');
            File.WriteAllText(Path.Combine(evidence, label + ".tir.txt"), tir.Body.ToString());
            rows.Add(new { label, unresolved, paired });
            Console.WriteLine($"Census {label}: unresolved={unresolved}, fused pairs={paired}.");
        }
        if (candidates.Count != 4) throw new Exception($"Expected four specifically selected unchanged callers; got {candidates.Count}.");
        File.WriteAllText(Path.Combine(evidence, "census.json"), JsonSerializer.Serialize(rows, new JsonSerializerOptions { WriteIndented = true }));
    }

    internal static void Fingerprint(string snapshot, string stdlib, string fixture, string evidence)
    {
        var files = Directory.GetFiles(snapshot, "*.dll", SearchOption.AllDirectories)
            .Concat(Directory.GetFiles(stdlib, "*.plato", SearchOption.AllDirectories)).Append(fixture).Distinct();
        var hashes = files.ToDictionary(p => p, p => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(p))));
        File.WriteAllText(Path.Combine(evidence, "input-hashes.json"), JsonSerializer.Serialize(hashes, new JsonSerializerOptions { WriteIndented = true }));
    }

    internal static void VerifyLoadedAssemblies(string snapshot, string evidence, string stage)
    {
        using var manifest = JsonDocument.Parse(File.ReadAllText(Path.Combine(snapshot, "manifest.json")));
        var expected = manifest.RootElement.GetProperty("snapshot_hashes").EnumerateObject()
            .ToDictionary(p => p.Name, p => p.Value.GetString()!, StringComparer.OrdinalIgnoreCase);
        var required = new[] { typeof(Ara3D.Geometry.Compiler.Compilation).Assembly, typeof(CSharpWriter).Assembly,
            typeof(RuntimeNumber).Assembly, typeof(CSharpCompilation).Assembly };
        var loaded = new Dictionary<string, object>();
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies().Concat(required).Distinct())
        {
            if (assembly.IsDynamic || string.IsNullOrEmpty(assembly.Location)) continue;
            var name = Path.GetFileName(assembly.Location);
            if (!expected.TryGetValue(name, out var hash))
            {
                if (required.Contains(assembly)) throw new Exception("Loaded required assembly absent from manifest: " + name);
                continue;
            }
            var actual = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(assembly.Location)));
            if (!actual.Equals(hash, StringComparison.OrdinalIgnoreCase)) throw new Exception("Loaded assembly differs from frozen manifest: " + assembly.Location);
            loaded[name] = new { path = assembly.Location, sha256 = actual };
        }
        File.WriteAllText(Path.Combine(evidence, $"loaded-assemblies-{stage}.json"), JsonSerializer.Serialize(loaded, new JsonSerializerOptions { WriteIndented = true }));
    }
}
