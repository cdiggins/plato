using Ara3D.Geometry.AST;
using Ara3D.Geometry.Compiler.Checking;
using Ara3D.Geometry.Compiler.Symbols;
using Ara3D.Geometry.CSharpWriter;
using Ara3D.Logging;
using Ara3D.Parakeet;
using Ara3D.Parsing;
using ArrayFactsExperiment;

var snapshot = args[0];
var here = args[1];
var output = args[2];
AstNode Parse(string path)
{
    var parser = CommonParsers.PlatoParser(new ParserInput(File.ReadAllText(path), path), Logger.Null);
    if (!parser.Succeeded)
        throw new Exception(string.Join(";", parser.ErrorMessages));
    return parser.Cst.ToAst();
}
var files = Directory.GetFiles(Path.Combine(snapshot, "source/stdlib/foundation"), "*.plato")
    .Concat(new[] { Path.Combine(here, "geometry.plato"), Path.Combine(output, "helper.plato") });
var compilation = new Ara3D.Geometry.Compiler.Compilation(Logger.Null, files.Select(Parse).ToList());
foreach (var inline in new[] { false, true })
{
    var writer = new CSharpWriter(compilation, "unused")
        { ExtensionStyle = true, LowerLoops = true, InlineCalls = inline, InlineStatements = inline };
    typeof(CSharpWriter).GetMethod("BuildExtensionPlans",
        System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!.Invoke(writer, null);
    foreach (var def in compilation.LibraryDefinitionsByName["ArrayFactGeometry"].Functions)
    {
        var raw = writer.TryGetStaticTir(def) ?? throw new Exception("Missing TIR " + def.Name);
        if (raw.AllNodes.OfType<TirUnresolved>().Any())
            throw new Exception("Unresolved TIR " + def.Name);
        var lowered = writer.RunOptimizerPasses(raw, null);
        string Emit()
        {
            // Equal generation state is required for byte comparison of capture-local names.
            SymbolRewriter.NextId = 0;
            return new TirCSharpBodyWriter(new CSharpTypeWriter(writer, null), lowered, isStatic: true).ToString();
        }
        var before = Emit();
        var analyzer = new ArrayFacts(compilation);
        analyzer.Analyze(lowered.Body);
        Require(Emit() == before, "analysis preserves emitted body");
        File.WriteAllText(Path.Combine(output, $"{def.Name}-{inline}.cs.txt"), before);
        Console.WriteLine($"METHOD {def.Name} inline={inline} facts={analyzer.Observations.Count}");
        foreach (var observation in analyzer.Observations)
            Console.WriteLine($"FACT Count({observation.Alias.Name}) uses completed producer length; "
                + "normalization follows checked int conversion; producer and callback retained");
        if (def.Name == "ExternalRowLength")
            Require(analyzer.Observations.Count == 0, "external storage refused");
        else
        {
            Require(analyzer.Observations.Count == 1, "geometry row and alias length discovered");
            CheckRefusals(lowered, analyzer);
        }
    }
}
Console.WriteLine("PASS: actual-TIR admission, identity, capture, coercion and scope checks");

void Require(bool condition, string name)
{
    if (!condition)
        throw new Exception(name);
    Console.WriteLine("PASS " + name);
}

void CheckRefusals(TirFunction function, ArrayFacts analyzer)
{
    var block = function.Body as TirBlock ?? throw new Exception("Expected lowered block");
    var statements = block.Statements.ToList();
    var loop = statements.OfType<TirLoweredLoop>().Single();
    var binding = statements.OfType<TirLet>().First(x => x.Value is TirTempRef);
    var count = analyzer.Observations.Single().Count;
    var alias = (TirVariable)count.Args[0];
    var tail = statements.FindIndex(x => x is TirReturn);
    void Check(string name, IReadOnlyList<TirNode> nodes, int expected = 0)
    {
        analyzer.Analyze(new TirBlock(nodes, block.Origin));
        Require(analyzer.Observations.Count == expected, name);
    }
    // These are adversarial rewrites of actual lowered nodes, not source-language claims.
    var capture = new TirLambda(Array.Empty<ParameterDef>(), alias, null, null);
    var escaped = statements.ToList();
    escaped.Insert(tail, capture);
    Check("closure creation invalidates every alias", escaped);
    Require(analyzer.Refusals.Contains("capture of an owned alias"), "capture reason recorded");
    var unknown = statements.ToList();
    unknown.Insert(tail, new TirInvoke(capture, Array.Empty<TirNode>(), null, null));
    Check("unknown invocation invalidates aliases", unknown);
    var assigned = statements.ToList();
    assigned.Insert(tail, new TirAssign(alias, alias, alias.Type, null));
    Check("assignment invalidates aliases", assigned);
    var converted = statements.Select(x => ReferenceEquals(x, binding)
        ? (TirNode)new TirLet(binding.Def, new TirCoerce(binding.Value, binding.Type, binding.Type, null, null), binding.Type, binding.Origin)
        : x).ToList();
    Check("even identity-shaped coercions refuse", converted);
    var temp = (TirTempRef)binding.Value;
    var renamed = statements.Select(x => ReferenceEquals(x, binding)
        ? (TirNode)new TirLet(binding.Def, new TirTempRef("deliberately-not-the-producer-name", temp.Type, temp.Origin), binding.Type, binding.Origin)
        : x).ToList();
    Check("mismatched emitted temporary binding refuses", renamed);
    var wrongOrigin = statements.Select(x => ReferenceEquals(x, binding)
        ? (TirNode)new TirLet(binding.Def, new TirTempRef(temp.Name, temp.Type, null), binding.Type, binding.Origin)
        : x).ToList();
    Check("matching spelling without identity refuses", wrongOrigin);
    var wrongBinding = new VariableDef(null, alias.Def.Name, alias.Type, null);
    var spoof = new TirCall(count.Callee, count.EmissionKind, count.ParameterTypes, count.ReturnType,
        new[] { new TirVariable(wrongBinding, alias.Type, alias.Origin) }, count.Type, count.Origin);
    var shadowed = statements.ToList();
    shadowed[tail] = new TirReturn(spoof, null);
    Check("same-spelled distinct binding refuses", shadowed);
    var nested = statements.ToList();
    nested[tail] = new TirBlock(new[] { statements[tail] }, null);
    Check("outer facts do not enter child scope", nested);
    var duplicates = statements.ToList();
    duplicates.Insert(0, loop);
    Check("ambiguous producer origin refuses", duplicates);
    var sameTemp = new TirLoweredLoop(loop.Kind, loop.Sources, loop.Fn, loop.Seed,
        loop.IncludeFirst, loop.TempName, loop.Id + 1, loop.Type, binding.Def, loop.SourceFunction)
        { ElemType = loop.ElemType };
    var duplicateTemp = statements.ToList();
    duplicateTemp.Insert(0, sameTemp);
    Check("duplicate emitted temporary binding refuses", duplicateTemp);
    var noncanonical = new TirLoweredLoop(loop.Kind, loop.Sources, loop.Fn, loop.Seed,
        loop.IncludeFirst, loop.TempName, loop.Id, loop.Type, loop.Origin, null)
        { ElemType = loop.ElemType };
    Check("matching producer kind without intrinsic identity refuses",
        statements.Select(x => ReferenceEquals(x, loop) ? noncanonical : x).ToList());
}
