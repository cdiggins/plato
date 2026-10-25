using Ara3D.Geometry.AST;
using Ara3D.Geometry.Compiler.Checking;
using Ara3D.Geometry.CSharpWriter;
using Ara3D.Logging;
using Ara3D.Parakeet;
using Ara3D.Parsing;

var root = args[0];
AstNode Parse(string path)
{
    var parser = CommonParsers.PlatoParser(new ParserInput(File.ReadAllText(path), path), Logger.Null);
    if (!parser.Succeeded) throw new Exception(string.Join(";", parser.ErrorMessages));
    return parser.Cst.ToAst();
}
var fixture = Path.Combine(root, "tools/experiments/optimizer-exploration/output-sizing/exact-output.plato");
var asts = Directory.GetFiles(Path.Combine(root, "stdlib/foundation"), "*.plato").Select(Parse).Append(Parse(fixture)).ToList();
var compilation = new Ara3D.Geometry.Compiler.Compilation(Logger.Null, asts);
var writer = new CSharpWriter(compilation, "unused-exact-output") { ExtensionStyle = true, LowerLoops = true };
foreach (var def in compilation.LibraryDefinitionsByName["ExactOutputProbe"].Functions)
{
    var tir = writer.TryGetStaticTir(def);
    Console.WriteLine($"METHOD {def.Name}");
    foreach (var call in tir.AllNodes.OfType<TirCall>().Where(c => c.Name == "FlatMap"))
    {
        Console.WriteLine($"CALL callee={call.Callee?.Name} owner={call.Callee?.OwnerType?.Name} functionType={call.Callee?.FunctionType} emission={call.EmissionKind} type={call.Type} source={call.Args[0].GetType().Name}");
        Console.WriteLine($"CALL canonical={compilation.LibraryDefinitionsByName["Intrinsics"].Functions.Any(f => ReferenceEquals(f,call.Callee))}");
    }
    var lowered = writer.RunOptimizerPasses(tir, null);
    var captured = TirLambdaCaptureRewriter.Rewrite(lowered.Body);
    foreach (var loop in captured.Descendants().OfType<TirLoweredLoop>())
    {
        Console.WriteLine($"LOOP kind={loop.Kind} source={loop.Sources[0].GetType().Name} stages={loop.SourceStages.Count} index={loop.IndexSource} type={loop.Type} elem={loop.ElemType} fn={loop.Fn.GetType().Name}");
        if (loop.Fn is TirLambda lambda)
        {
            Console.WriteLine($"LAMBDA body={lambda.Body.GetType().Name} return={lambda.Type.TypeArgs.Last()}");
            if (lambda.Body is TirArray array)
                Console.WriteLine($"ARRAY width={array.Elements.Count} type={array.Type} exact={array.Type.Equals(loop.Type) && array.Elements.All(e => e.Type.Equals(loop.ElemType))}");
        }
    }
    var body = new TirCSharpBodyWriter(new CSharpTypeWriter(writer, null), lowered, isStatic: true);
    Console.WriteLine(body);
}
