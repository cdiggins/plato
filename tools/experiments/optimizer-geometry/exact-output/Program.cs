using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
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
using PlatoCompilation = Ara3D.Geometry.Compiler.Compilation;

var root = args[0];
var snapshot = args[1];
var output = args[2];
var phase = args[3];
var here = Path.Combine(root, "tools/experiments/optimizer-geometry/exact-output");
Directory.CreateDirectory(output);
AstNode Parse(string path)
{
    var parser = CommonParsers.PlatoParser(new ParserInput(File.ReadAllText(path), path), Logger.Null);
    if (!parser.Succeeded) throw new Exception($"{path}: {string.Join("; ", parser.ErrorMessages)}");
    return parser.Cst.ToAst();
}
PlatoCompilation Compile(IEnumerable<string> paths) => new(Logger.Null, paths.Select(Parse).ToList());
var sourceRoot = Path.Combine(snapshot, "source");
try
{
var foundation = Directory.GetFiles(Path.Combine(sourceRoot, "stdlib/foundation"), "*.plato");

if (phase == "census")
{
    var paths = foundation.Concat(Directory.GetFiles(Path.Combine(sourceRoot, "stdlib/geometry"), "*.plato"))
        .Concat(Directory.GetFiles(Path.Combine(sourceRoot, "stdlib/graphics"), "*.plato"));
    var compilation = Compile(paths);
    var lines = new List<string>();
    foreach (var receiver in new[] { "TriangleArray3D", "QuadArray3D", "QuadMesh3D" })
    foreach (var inline in new[] { false, true })
    {
        var name = receiver == "QuadMesh3D" ? "ToTriangleMesh" : "Points";
        var definition = compilation.LibraryDefinitionsByName["Meshes"].Functions.Single(f => f.Name == name && f.Parameters[0].Type.Name == receiver);
        var writer = new CSharpWriter(compilation, "unused-g3") { ExtensionStyle = true, LowerLoops = true, InlineCalls = inline, ExactOutputAllocation = true };
        typeof(CSharpWriter).GetMethod("BuildExtensionPlans", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(writer, null);
        var tir = writer.TryGetStaticTir(definition) ?? writer.TryGetGroundTirByTypeName(definition, receiver);
        if (tir == null) { lines.Add($"{receiver}.{name} inline={inline}: no TIR"); continue; }
        var unresolved = tir.AllNodes.OfType<TirUnresolved>().Count();
        lines.Add($"{receiver}.{name} inline={inline}: unresolved input nodes={unresolved}");
        if (unresolved != 0) { lines.Add("  applicability unqualified: unresolved input TIR"); continue; }
        var lowered = writer.RunOptimizerPasses(tir, null);
        var loweredUnresolved = lowered.AllNodes.OfType<TirUnresolved>().Count();
        if (loweredUnresolved != 0) { lines.Add($"  applicability unqualified: unresolved optimized nodes={loweredUnresolved}"); continue; }
        var captured = TirLambdaCaptureRewriter.Rewrite(lowered.Body);
        var loops = captured.Descendants().OfType<TirLoweredLoop>().Where(l => l.Kind == "FlatMap").ToArray();
        lines.Add($"{receiver}.{name} inline={inline}: FlatMap loops={loops.Length}");
        foreach (var loop in loops)
        {
            var accepted = TirExactOutput.TryGetWidth(loop, compilation, out var width);
            lines.Add($"  accepted={accepted} width={width} fn={loop.Fn.GetType().Name} body={(loop.Fn as TirLambda)?.Body.GetType().Name} sourceStages={loop.SourceStages.Count} indexSource={loop.IndexSource} callee={loop.SourceFunction?.OwnerType?.Name}.{loop.SourceFunction?.Name}");
        }
        var text = new TirCSharpBodyWriter(new CSharpTypeWriter(writer, null), lowered, isStatic: true).ToString();
        File.WriteAllText(Path.Combine(output, $"actual-{receiver}-{name}-inline-{inline}.cs"), text);
        lines.Add($"  exact guard in body={text.Contains("System.Array.MaxLength")}");
    }
    File.WriteAllLines(Path.Combine(output, "census.txt"), lines);
    foreach (var line in lines) Console.WriteLine(line);
    return;
}

var fixtureCompilation = Compile(foundation.Append(Path.Combine(here, "kernels.plato")));
var assemblies = new Dictionary<bool, Assembly>();
var shapes = new List<string>();
var setup = new List<object>();
foreach (var enabled in new[] { false, true })
{
    var setupClock = Stopwatch.StartNew();
    var writer = new CSharpWriter(fixtureCompilation, "unused-g3")
        { ExtensionStyle = true, LowerLoops = true, ExactOutputAllocation = enabled };
    writer.WriteAll();
    var emissionSeconds = setupClock.Elapsed.TotalSeconds;
    var names = fixtureCompilation.LibraryDefinitionsByName["G3Exact"].Functions.Select(f => f.Name).ToHashSet();
    var methods = writer.Files.Values.SelectMany(f => CSharpSyntaxTree.ParseText(f.ToString()).GetRoot()
        .DescendantNodes().OfType<MethodDeclarationSyntax>())
        .Where(m => names.Contains(m.Identifier.Text) && m.Modifiers.Any(SyntaxKind.StaticKeyword)).ToList();
    var required = new[] { "TriangleCorners", "QuadCorners", "SplitQuads", "GatherTriangles", "HelperCorners" };
    foreach (var name in required)
    {
        var method = methods.Single(m => m.Identifier.Text == name);
        var guard = method.ToString().Contains("System.Array.MaxLength");
        if (guard != (enabled && name != "HelperCorners")) throw new Exception($"Unexpected applicability: {name} enabled={enabled} guard={guard}");
        shapes.Add($"{name} enabled={enabled} guard={guard}");
    }
    var kernels = "using System; using System.Collections.Generic; using System.Runtime.CompilerServices; using static System.Runtime.CompilerServices.MethodImplOptions; namespace Ara3D.Geometry { public static class GeneratedKernels { "
        + string.Join("\n", methods.Select(m => m.ToString())) + " } }";
    var sources = writer.Files.Where(f => f.Key.StartsWith("_G3", StringComparison.Ordinal))
        .Select(f => f.Value.ToString()).Append(kernels).Append(File.ReadAllText(Path.Combine(here, "GeneratedHarness.cs")))
        .Prepend("global using System; global using System.Collections.Generic;").ToArray();
    var combined = string.Join("\n", sources);
    File.WriteAllText(Path.Combine(output, $"generated-{enabled}.cs"), combined);
    var referencePaths = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!).Split(Path.PathSeparator)
        .Concat(Directory.GetFiles(snapshot, "*.dll")).Where(File.Exists)
        .Where(p => !string.Equals(Path.GetFileName(p), "Ara3D.Geometry.dll", StringComparison.OrdinalIgnoreCase))
        .GroupBy(Path.GetFileName, StringComparer.OrdinalIgnoreCase).Select(g => g.Last());
    var references = referencePaths.Select(p => MetadataReference.CreateFromFile(p));
    setupClock.Restart();
    var csharp = CSharpCompilation.Create($"GeometryExact_{enabled}_{Guid.NewGuid():N}", sources.Select(s => CSharpSyntaxTree.ParseText(s)), references,
        new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, optimizationLevel: OptimizationLevel.Release));
    using var stream = new MemoryStream();
    var emit = csharp.Emit(stream);
    var diagnostics = string.Join("\n", emit.Diagnostics);
    File.WriteAllText(Path.Combine(output, $"generated-build-{enabled}.log"), diagnostics);
    if (!emit.Success) throw new Exception(diagnostics);
    var compilationSeconds = setupClock.Elapsed.TotalSeconds;
    assemblies[enabled] = Assembly.Load(stream.ToArray());
    setupClock.Restart();
    assemblies[enabled].GetType("Ara3D.Geometry.GeometryHarness")!.GetMethod("Check")!.Invoke(null, null);
    setup.Add(new { enabled, emissionSeconds, compilationSeconds, correctnessSeconds = setupClock.Elapsed.TotalSeconds });
}
File.WriteAllText(Path.Combine(output, $"setup-{phase}.json"), JsonSerializer.Serialize(setup, new JsonSerializerOptions { WriteIndented = true }));
File.WriteAllLines(Path.Combine(output, "shapes.txt"), shapes);
Console.WriteLine("PASS generated geometry off/on equality, winding/order, empty/degenerate/nonfinite, invalid index and ordered vertex faults");
if (phase != "timing") return;

Func<long> Job(bool enabled, string name, int count)
    => (Func<long>)assemblies[enabled].GetType("Ara3D.Geometry.GeometryHarness")!.GetMethod("Create")!.Invoke(null, new object[] { name, count })!;
long checksum = 0;
(double Ms, long Bytes) Measure(Func<long> action, int repetitions)
{
    var startBytes = GC.GetAllocatedBytesForCurrentThread();
    var startTime = Stopwatch.GetTimestamp();
    for (var i = 0; i < repetitions; i++) checksum += action();
    return (Stopwatch.GetElapsedTime(startTime).TotalMilliseconds / repetitions,
        (GC.GetAllocatedBytesForCurrentThread() - startBytes) / repetitions);
}
var rows = new List<object>();
Console.WriteLine($"Runtime={RuntimeInformation.FrameworkDescription}; architecture={RuntimeInformation.ProcessArchitecture}; CPUs={Environment.ProcessorCount}; tiered={Environment.GetEnvironmentVariable("DOTNET_TieredCompilation")}");
foreach (var name in new[] { "TriangleCorners", "SplitQuads", "HelperCorners" })
foreach (var count in new[] { 2048, 16384 })
{
    var off = Job(false, name, count);
    var on = Job(true, name, count);
    if (off() != on()) throw new Exception("paired checksum");
    for (var i = 0; i < 8; i++) { checksum += off(); checksum += on(); }
    var repetitions = Math.Max(4, 65536 / count);
    for (var round = 0; round < 3; round++)
    {
        var a = round % 2 == 0 ? Measure(off, repetitions) : Measure(on, repetitions);
        var b = round % 2 == 0 ? Measure(on, repetitions) : Measure(off, repetitions);
        var baseline = round % 2 == 0 ? a : b;
        var optimized = round % 2 == 0 ? b : a;
        rows.Add(new { name, count, repetitions, round, baselineMs = baseline.Ms, optimizedMs = optimized.Ms, baselineBytes = baseline.Bytes, optimizedBytes = optimized.Bytes });
        Console.WriteLine($"{name} n={count} round={round} baseline_ms={baseline.Ms:F6} optimized_ms={optimized.Ms:F6} baseline_bytes={baseline.Bytes} optimized_bytes={optimized.Bytes}");
    }
}
File.WriteAllText(Path.Combine(output, "timings.json"), JsonSerializer.Serialize(rows, new JsonSerializerOptions { WriteIndented = true }));
Console.WriteLine($"Consumed checksum={checksum}");
}
finally
{
    var loaded = AppDomain.CurrentDomain.GetAssemblies().Where(a => !a.IsDynamic && !string.IsNullOrEmpty(a.Location))
        .Select(a => new { name = Path.GetFileName(a.Location), location = a.Location,
            sha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(a.Location))).ToLowerInvariant() }).ToArray();
    File.WriteAllText(Path.Combine(output, $"loaded-assemblies-{phase}.json"), JsonSerializer.Serialize(loaded, new JsonSerializerOptions { WriteIndented = true }));
}
