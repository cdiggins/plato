using Ara3D.Geometry.AST;
using Ara3D.Geometry.Compiler.Checking;
using Ara3D.Geometry.CSharpWriter;
using Ara3D.Logging;
using Ara3D.Parakeet;
using Ara3D.Parsing;

var root = args[0];
var output = args[1];
var snapshot = args[2];
var qualify = args.Contains("--qualify");
AstNode Parse(string path)
{
    var parser = CommonParsers.PlatoParser(new ParserInput(File.ReadAllText(path), path), Logger.Null);
    if (!parser.Succeeded) throw new Exception(string.Join(";", parser.ErrorMessages));
    return parser.Cst.ToAst();
}
var files = Directory.GetFiles(Path.Combine(snapshot, "source/stdlib/foundation"), "*.plato")
    .Concat(new[] { Path.Combine(output, "helpers.plato"), Path.Combine(root,
        "tools/experiments/optimizer-geometry/loop-unswitch/fixtures.plato") });
var compilation = new Ara3D.Geometry.Compiler.Compilation(Logger.Null, files.Select(Parse).ToList());
foreach (var inline in new[] { false, true })
{
    if (qualify && !inline) continue;
    var writer = new CSharpWriter(compilation, "unused")
        { ExtensionStyle = true, LowerLoops = true, InlineCalls = inline, InlineStatements = inline };
    typeof(CSharpWriter).GetMethod("BuildExtensionPlans",
        System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!.Invoke(writer, null);
    foreach (var def in compilation.LibraryDefinitionsByName["UnswitchGeometry"].Functions)
    {
        if (qualify && def.Name != "SampleRing") continue;
        var raw = writer.TryGetStaticTir(def) ?? throw new Exception("Missing TIR " + def.Name);
        if (raw.AllNodes.OfType<TirUnresolved>().Any()) throw new Exception("Unresolved " + def.Name);
        var lowered = writer.RunOptimizerPasses(raw, null);
        Console.WriteLine($"METHOD {def.Name} inline={inline}");
        Dump(lowered.Body, 0);
        var body = new TirCSharpBodyWriter(new CSharpTypeWriter(writer, null), lowered, isStatic: true).ToString();
        File.WriteAllText(Path.Combine(output, $"{def.Name}-{inline}.cs.txt"), body);
        Console.WriteLine(body);
        if (qualify) Qualification.Run(lowered, writer, body, output, args.Contains("--timing"));
        foreach (var loop in lowered.AllNodes.OfType<TirLoweredLoop>())
        {
            var bodyNode = (loop.Fn as TirLambda)?.Body;
            var conditional = bodyNode as TirConditional;
            var leaf = conditional?.Condition as TirParameter;
            var assigned = lowered.AllNodes.OfType<TirAssign>().Any(a =>
                a.LValue is TirParameter parameter && ReferenceEquals(parameter.Def, leaf?.Def));
            var boolean = compilation.TypeDefinitionsByName["Boolean"];
            var admitted = loop.Kind == "MapRange" && loop.SourceStages.Count == 0
                && TirIntrinsicIdentity.IsIntrinsic(loop.SourceFunction, compilation, "MapRange")
                && leaf != null && ReferenceEquals(leaf.Type.Def, boolean)
                && boolean.IsPrimitiveDeclaration && !assigned
                && lowered.Parameters.Any(p => ReferenceEquals(p, leaf.Def));
            Console.WriteLine($"CANDIDATE kind={loop.Kind} direct={conditional != null} outerBoolean={admitted}");
        }
        Console.WriteLine("AFTER CAPTURE REWRITE");
        Dump(TirLambdaCaptureRewriter.Rewrite(lowered.Body), 0);
    }
}
var loaded = AppDomain.CurrentDomain.GetAssemblies()
    .Where(a => !a.IsDynamic && !string.IsNullOrEmpty(a.Location))
    .Select(a => new { Path = a.Location, Sha256 = Convert.ToHexString(
        System.Security.Cryptography.SHA256.HashData(File.ReadAllBytes(a.Location))).ToLowerInvariant() });
File.WriteAllText(Path.Combine(output, "loaded-assemblies.json"),
    System.Text.Json.JsonSerializer.Serialize(loaded, new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));
void Dump(TirNode node, int depth)
{
    Console.WriteLine(new string(' ', depth) + node.GetType().Name + " " + node.Type
        + (node is TirCall c ? " " + c.Name : node is TirParameter p ? " " + p.Def.Name : ""));
    foreach (var child in node.Children) if (child != null) Dump(child, depth + 2);
}
