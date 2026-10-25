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
var fixture = Path.Combine(root, "tools/experiments/optimizer-exploration/secondary-census/fixtures.plato");
var asts = Directory.GetFiles(Path.Combine(root, "stdlib/foundation"), "*.plato").Select(Parse).Append(Parse(fixture)).ToList();
var compilation = new Ara3D.Geometry.Compiler.Compilation(Logger.Null, asts);
void Dump(TirNode node, int indent = 0)
{
    var detail = node switch {
        TirCall c => $" name={c.Name} emission={c.EmissionKind} callee={c.Callee?.Name} owner={c.Callee?.OwnerType?.Name}",
        TirParameter p => $" name={p.Def.Name}",
        TirVariable v => $" name={v.Def.Name}",
        TirLoweredLoop l => $" kind={l.Kind} stages={l.SourceStages.Count}",
        _ => ""
    };
    Console.WriteLine(new string(' ', indent) + node.GetType().Name + " type=" + node.Type + detail);
    foreach (var child in node.Children) if (child != null) Dump(child, indent + 2);
}
foreach (var inline in new[] {false, true}) {
var writer = new CSharpWriter(compilation, "unused-secondary-census") { ExtensionStyle = true, LowerLoops = true, InlineCalls = inline };
typeof(CSharpWriter).GetMethod("BuildExtensionPlans", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!.Invoke(writer, null);
foreach (var def in compilation.LibraryDefinitionsByName["SecondaryCensus"].Functions)
{
    var tir = writer.TryGetStaticTir(def);
    if (tir == null) throw new Exception("Missing ground TIR: " + def.Name);
    if (tir.AllNodes.OfType<TirUnresolved>().Any()) throw new Exception("Unresolved TIR: " + def.Name);
    void Require(bool value, string message) { if (!value) throw new Exception(def.Name + ": " + message); }
    var rawCalls = tir.AllNodes.OfType<TirCall>().ToList();
    if (def.Name is "DirectProjection" or "FaultingUnused") {
        var projection = (TirCall)tir.Body;
        Require(projection.Callee.OwnerType.Fields.Any(f => ReferenceEquals(f.Function, projection.Callee)), "projection must resolve to declared field identity");
        Require(projection.Args[0] is TirCall ctor && ctor.EmissionKind == EmissionKind.Constructor, "constructor call shape");
    }
    if (def.Name == "FaultingUnused") Require(tir.AllNodes.OfType<TirInvoke>().Count() == 1, "unused field callback retained");
    if (def.Name == "Invariant") Require(tir.AllNodes.OfType<TirConditional>().Single().Condition is TirParameter p && ReferenceEquals(p.Def, tir.Parameters[1]), "resolved invariant parameter identity");
    if (def.Name == "Variant") Require(tir.AllNodes.OfType<TirConditional>().Single().Condition is TirCall, "iteration condition is executable");
    if (def.Name == "FaultingCondition") Require(tir.AllNodes.OfType<TirConditional>().Single().Condition is TirInvoke, "faulting callback is executable");
    if (def.Name == "Grid") Require(tir.AllNodes.OfType<TirInvoke>().Count() == 4, "four sample calls survive elaboration");
    Console.WriteLine($"METHOD {def.Name} inline={inline}");
    Console.WriteLine("RAW"); Dump(tir.Body);
    var lowered = writer.RunOptimizerPasses(tir, null);
    var loops = lowered.AllNodes.OfType<TirLoweredLoop>().ToList();
    if (def.Name == "OwnedLength") Require(loops.Count == 1 && lowered.AllNodes.OfType<TirLet>().Any(l => l.Value is TirTempRef), "completed producer aliases generated loop result");
    if (def.Name == "ExternalLength") Require(loops.Count == 0, "external input has no owned producer");
    if (def.Name == "Neighbor") Require(loops.Count == 2, "neighbor access remains in second loop");
    if (def.Name == "Grid") Require(loops.Count == 1 && lowered.AllNodes.OfType<TirCall>().Any(c => c.Name == "MapRange"), "nested callback remains runtime MapRange");
    Console.WriteLine("LOWERED"); Dump(TirLambdaCaptureRewriter.Rewrite(lowered.Body));
    Console.WriteLine("EMITTED");
    Console.WriteLine(new TirCSharpBodyWriter(new CSharpTypeWriter(writer, null), lowered, isStatic: true));
}
}


Console.WriteLine("PASS: all actual TIR assertions for both inline recipes");
