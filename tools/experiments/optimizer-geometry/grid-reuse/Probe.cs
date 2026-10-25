using Ara3D.Geometry.AST;
using Ara3D.Geometry.Compiler.Checking;
using Ara3D.Geometry.CSharpWriter;
using Ara3D.Logging;
using Ara3D.Parakeet;
using Ara3D.Parsing;

var source = args[0];
var fixture = args[1];
var output = args[2];
AstNode Parse(string path)
{
    var parser = CommonParsers.PlatoParser(new ParserInput(File.ReadAllText(path), path), Logger.Null);
    if (!parser.Succeeded) throw new Exception(string.Join(";", parser.ErrorMessages));
    return parser.Cst.ToAst();
}
var paths = new[] { "foundation", "geometry" }.SelectMany(tier =>
    Directory.GetFiles(Path.Combine(source, tier), "*.plato")).Append(fixture).ToList();
var compilation = new Ara3D.Geometry.Compiler.Compilation(Logger.Null, paths.Select(Parse).ToList());
var fixtures = compilation.LibraryDefinitionsByName["GridGeometryProbe"].Functions.ToList();
var marching = compilation.LibraryDefinitionsByName.Values.SelectMany(l => l.Functions)
    .Where(f => f.Name is "MarchingCubesLattice" or "MarchingCubesCornerValue").ToList();
if (marching.Count == 0) throw new Exception("No unchanged marching-cubes functions found");
foreach (var inline in new[] { false, true })
{
    var writer = new CSharpWriter(compilation, "unused-grid-probe")
    { ExtensionStyle = true, LowerLoops = true, InlineCalls = inline, InlineStatements = inline };
    typeof(CSharpWriter).GetMethod("BuildExtensionPlans",
        System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!.Invoke(writer, null);
    foreach (var def in fixtures.Concat(marching))
    {
        var raw = writer.TryGetStaticTir(def);
        if (raw == null) { Console.WriteLine($"REFUSE {def.Name}: no ground TIR"); continue; }
        if (raw.AllNodes.OfType<TirUnresolved>().Any()) throw new Exception("Unresolved: " + def.Name);
        var lowered = writer.RunOptimizerPasses(raw, null);
        Console.WriteLine($"METHOD {def.Name} inline={inline}");
        foreach (var pair in new[] { ("RAW", raw.Body), ("LOWERED", lowered.Body),
                                    ("CAPTURED", TirLambdaCaptureRewriter.Rewrite(lowered.Body)) })
        {
            Console.WriteLine(pair.Item1);
            Dump(pair.Item2, 0);
        }
        var loops = lowered.AllNodes.OfType<TirLoweredLoop>().ToList();
        Console.WriteLine($"SUMMARY loops={loops.Count} nestedLoopPairs={loops.Sum(l => l.Children.Sum(c => CountLoops(c)))} invokes={lowered.AllNodes.OfType<TirInvoke>().Count()}");
        // Parameter reference identity is evidence of a common callable, not proof of its captures.
        var invokes = raw.AllNodes.OfType<TirInvoke>().ToList();
        if (def.Name == "Rectangular")
        {
            var callback = raw.Parameters.Single(p => p.Name == "sample");
            if (invokes.Count != 4 || invokes.Any(i => i.Target is not TirParameter p || !ReferenceEquals(p.Def, callback)))
                throw new Exception("Rectangular callback identity changed");
            Console.WriteLine("REFUSE callback parameter: exact binding established; capture environment and totality unknown");
        }
        var primitiveNumber = compilation.TypeDefinitionsByName["Number"];
        bool Total(TirNode n) => ReferenceEquals(n.Type?.Def, primitiveNumber)
            && primitiveNumber.IsPrimitiveDeclaration && (n switch
            {
                TirLiteral => true,
                TirParameter p => raw.Parameters.Any(a => ReferenceEquals(a, p.Def))
                    && !raw.AllNodes.OfType<TirAssign>().Any(a => a.LValue is TirParameter q && ReferenceEquals(q.Def, p.Def)),
                TirCall c => (TirIntrinsicIdentity.IsIntrinsic(c.Callee, compilation, "Add")
                          || TirIntrinsicIdentity.IsIntrinsic(c.Callee, compilation, "Multiply"))
                    && c.Args.Count == 2 && c.ParameterTypes.Count == 2
                    && c.ParameterTypes.SequenceEqual(c.Args.Select(a => a.Type))
                    && Equals(c.ReturnType, c.Type) && c.Args.All(Total),
                _ => false
            });
        Console.WriteLine($"BODY_TOTAL_NARROW={Total(raw.Body)} (Number arithmetic only; conversions/schedules not certified)");
        File.WriteAllText(Path.Combine(output, $"{def.Name}-{inline}.cs.txt"),
            new TirCSharpBodyWriter(new CSharpTypeWriter(writer, null), lowered, isStatic: true).ToString());
    }
}
int CountLoops(TirNode n) => (n is TirLoweredLoop ? 1 : 0) + n.Children.Sum(CountLoops);
void Dump(TirNode node, int depth)
{
    var detail = node switch
    {
        TirCall c => $" call={c.Name} resolved={c.Callee != null} intrinsicMap={TirIntrinsicIdentity.IsIntrinsic(c.Callee, compilation, "MapRange")}",
        TirLoweredLoop l => $" loop={l.Kind} intrinsicMap={TirIntrinsicIdentity.IsIntrinsic(l.SourceFunction, compilation, "MapRange")}",
        TirParameter p => $" parameter={p.Def.Name}",
        TirVariable v => $" variable={v.Def.Name}",
        _ => ""
    };
    Console.WriteLine(new string(' ', depth) + node.GetType().Name + detail);
    foreach (var child in node.Children) if (child != null) Dump(child, depth + 2);
}
