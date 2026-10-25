using System.Reflection;
using Ara3D.Geometry.Compiler.Checking;
using Ara3D.Geometry.CSharpWriter;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Number = Ara3D.Geometry.Number;
using Integer = Ara3D.Geometry.Integer;

namespace RecordGeometryExperiment;

internal static class Collision
{
    internal static void Verify(Ara3D.Geometry.Compiler.Compilation compilation, string output)
    {
        var writer = new CSharpWriter(compilation, "unused-record-collision") { ExtensionStyle = true };
        typeof(CSharpWriter).GetMethod("BuildExtensionPlans", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(writer, null);
        var record = compilation.ConcreteTypes.Single(c => c.TypeDef.Name == "ProjectionPoint");
        writer.WriteFile("_ProjectionPoint.g.cs", () => writer.WriteTypeImplementation(record));
        var def = compilation.LibraryDefinitionsByName["RecordCollision"].Functions.Single();
        var original = writer.TryGetStaticTir(def);
        var parameter = original.AllNodes.OfType<TirLambda>().Single().Parameters.Single();
        if (parameter.Name != "projectionField0") throw new Exception("Expected collision parameter");
        if (!Projection.TryRewrite(original, out var rewritten)) throw new Exception("Expected admission");
        if (rewritten.AllNodes.OfType<TirLet>().Any(l => l.Def.Name == parameter.Name))
            throw new Exception("Local shadows unused lambda parameter");
        var methods = new List<string>();
        foreach (var (name, function) in new[] { ("Off", original), ("On", rewritten) })
        {
            var body = new TirCSharpBodyWriter(new CSharpTypeWriter(writer, null), function, isStatic: true).ToString();
            methods.Add($"public static Number {name}(Integer n, Number x, Number z) {body}");
        }
        var source = "global using System; global using System.Collections.Generic; namespace Ara3D.Geometry { public static class CollisionKernel { "
            + string.Join("\n", methods) + " } }";
        File.WriteAllText(Path.Combine(output, "collision-generated.cs.txt"), source);
        var paths = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")).Split(Path.PathSeparator)
            .Append(typeof(Number).Assembly.Location).Where(File.Exists)
            .Where(p => Path.GetFileName(p) != "Ara3D.Geometry.dll").Distinct();
        var sources = new[] { source, writer.Files["_ProjectionPoint.g.cs"].ToString(), "global using System; global using System.Collections.Generic;" };
        var generated = CSharpCompilation.Create("Collision_" + Guid.NewGuid().ToString("N"),
            sources.Select(s => CSharpSyntaxTree.ParseText(s, new CSharpParseOptions(LanguageVersion.Default))),
            paths.Select(p => MetadataReference.CreateFromFile(p)),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, optimizationLevel: OptimizationLevel.Release));
        using var stream = new MemoryStream();
        var result = generated.Emit(stream);
        if (!result.Success) throw new Exception(string.Join("\n", result.Diagnostics.Where(d => d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error)));
        var type = Assembly.Load(stream.ToArray()).GetType("Ara3D.Geometry.CollisionKernel");
        var off = type.GetMethod("Off").CreateDelegate<Func<Integer, Number, Number, Number>>();
        var on = type.GetMethod("On").CreateDelegate<Func<Integer, Number, Number, Number>>();
        if ((double)off(1, 7, 9) != 7 || (double)on(1, 7, 9) != 7) throw new Exception("Collision result changed");
        Console.WriteLine("PASS unused lambda parameter collision: renamed local, generated off/on compile and execute");
    }
}
