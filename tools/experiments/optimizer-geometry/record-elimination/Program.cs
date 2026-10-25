using System.Reflection;
using System.Security.Cryptography;
using System.Text.Json;
using Ara3D.Geometry.AST;
using Ara3D.Geometry.Compiler.Checking;
using Ara3D.Geometry.Compiler.Symbols;
using Ara3D.Geometry.CSharpWriter;
using Ara3D.Logging;
using Ara3D.Parakeet;
using Ara3D.Parsing;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using RecordGeometryExperiment;
using Number = Ara3D.Geometry.Number;

var snapshot = args[0];
var here = args[1];
var output = args[2];
var timing = args[3] == "benchmark";
using var manifest = JsonDocument.Parse(File.ReadAllText(Path.Combine(snapshot, "manifest.json")));
foreach (var assembly in new[] { typeof(Ara3D.Geometry.Compiler.Compilation).Assembly, typeof(CSharpWriter).Assembly,
    typeof(CommonParsers).Assembly, typeof(ParserInput).Assembly, typeof(Number).Assembly, typeof(CSharpCompilation).Assembly })
{
    var name = Path.GetFileName(assembly.Location);
    var expected = manifest.RootElement.GetProperty("snapshot_hashes").GetProperty(name).GetString();
    var actual = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(assembly.Location))).ToLowerInvariant();
    if (actual != expected.ToLowerInvariant()) throw new Exception("Loaded DLL differs: " + name);
    Console.WriteLine($"LOADED {name} {actual} {assembly.Location}");
}
AstNode Parse(string path)
{
    var parser = CommonParsers.PlatoParser(new ParserInput(File.ReadAllText(path), path), Logger.Null);
    if (!parser.Succeeded) throw new Exception(string.Join(";", parser.ErrorMessages));
    return parser.Cst.ToAst();
}
var files = Directory.GetFiles(Path.Combine(snapshot, "source/stdlib/foundation"), "*.plato")
    .Append(Path.Combine(here, "geometry.plato")).Append(Path.Combine(here, "collision.plato"));
var compilation = new Ara3D.Geometry.Compiler.Compilation(Logger.Null, files.Select(Parse).ToList());
if (args[3] == "collision")
{
    Collision.Verify(compilation, output);
    return;
}
foreach (var inline in new[] { false, true })
{
    var writer = new CSharpWriter(compilation, "unused-record-geometry")
        { ExtensionStyle = true, InlineCalls = inline, InlineStatements = inline, LowerLoops = true };
    writer.WriteAll();
    var functions = compilation.LibraryDefinitionsByName["RecordGeometry"].Functions
        .ToDictionary(f => f.Name, f => writer.RunOptimizerPasses(writer.TryGetStaticTir(f), null));
    foreach (var (name, function) in functions)
    {
        Require(!function.AllNodes.OfType<TirUnresolved>().Any(), "resolved " + name);
        var admitted = Projection.TryRewrite(function, out var rewritten);
        Require(admitted == (name is "Coordinate" or "FaultingUnused"), "admission " + name);
        File.WriteAllText(Path.Combine(output, $"{name}-{inline}.tir.txt"), function.Body + "\nON\n" + rewritten.Body);
    }
    var baseline = functions["Coordinate"];
    var getter = (TirCall)baseline.Body;
    var constructor = (TirCall)getter.Args[0];
    TirFunction With(TirNode body) => new(baseline.Original, baseline.Parameters, baseline.ReturnType, body);
    void Refuse(string name, TirNode body) => Require(!Projection.TryRewrite(With(body), out _), name);
    var fakeGetter = new FunctionDef(getter.Callee.Scope, getter.Callee.Name, getter.Callee.OwnerType,
        getter.Callee.ReturnType, null, getter.Callee.Parameters.ToArray());
    Refuse("same name is not field identity", new TirCall(fakeGetter, getter.EmissionKind,
        getter.ParameterTypes, getter.ReturnType, getter.Args, getter.Type, getter.Origin));
    var fakeConstructor = new FunctionDef(constructor.Callee.Scope, constructor.Callee.Name,
        constructor.Callee.OwnerType, constructor.Callee.ReturnType, null, constructor.Callee.Parameters.ToArray());
    var changed = new TirCall(fakeConstructor, constructor.EmissionKind, constructor.ParameterTypes,
        constructor.ReturnType, constructor.Args, constructor.Type, constructor.Origin);
    Refuse("same name is not constructor identity", getter.WithChildren(new[] { changed }));
    Refuse("receiver conversion refuses", getter.WithChildren(new TirNode[] {
        new TirCoerce(constructor, constructor.Type, constructor.Type, null, constructor.Origin) }));
    var off = Build(false);
    var on = Build(true);
    Qualification.Verify(off, on);
    if (timing) Qualification.Measure(off.Coordinate, on.Coordinate, output, inline);

    Qualification.Variant Build(bool enabled)
    {
        var methods = new List<string>();
        foreach (var name in new[] { "Coordinate", "FaultingUnused" })
        {
            var function = functions[name];
            if (enabled && !Projection.TryRewrite(function, out function)) throw new Exception("Lost admission");
            var body = new TirCSharpBodyWriter(new CSharpTypeWriter(writer, null), function, isStatic: true).ToString();
            var signature = name == "Coordinate" ? "Number x, Number y, Number z"
                : "Number x, Func<Number, Number> y, Func<Number, Number> z";
            methods.Add($"public static Number {name}({signature}) {body}");
        }
        var source = "global using System; global using System.Collections.Generic; namespace Ara3D.Geometry { public static class RecordKernel { "
            + string.Join("\n", methods) + " } }";
        var sources = new[] { source, writer.Files["_ProjectionPoint.g.cs"].ToString(), "global using System; global using System.Collections.Generic;" };
        for (var i = 0; i < sources.Length; i++) File.WriteAllText(Path.Combine(output, $"generated-{inline}-{enabled}-{i}.cs.txt"), sources[i]);
        var paths = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")).Split(Path.PathSeparator)
            .Append(typeof(Number).Assembly.Location).Where(File.Exists)
            .Where(p => Path.GetFileName(p) != "Ara3D.Geometry.dll").Distinct();
        var generated = CSharpCompilation.Create("RecordKernel_" + Guid.NewGuid().ToString("N"),
            sources.Select(s => CSharpSyntaxTree.ParseText(s, new CSharpParseOptions(LanguageVersion.Default))),
            paths.Select(p => MetadataReference.CreateFromFile(p)),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, optimizationLevel: OptimizationLevel.Release));
        using var stream = new MemoryStream();
        var result = generated.Emit(stream);
        if (!result.Success) throw new Exception(string.Join("\n", result.Diagnostics.Where(d => d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error)));
        var type = Assembly.Load(stream.ToArray()).GetType("Ara3D.Geometry.RecordKernel");
        return new(type.GetMethod("Coordinate").CreateDelegate<Qualification.Coordinate>(),
            type.GetMethod("FaultingUnused").CreateDelegate<Qualification.Fault>());
    }
}
Console.WriteLine("PASS: generated coordinate projection and ordered unused-field behavior");

void Require(bool condition, string name)
{
    if (!condition) throw new Exception(name);
    Console.WriteLine("PASS " + name);
}
