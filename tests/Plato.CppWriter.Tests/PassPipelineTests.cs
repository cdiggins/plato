// Pins the Cpu pipeline's shape after TKT-1156/TKT-1172: a list of shared pass names, no private C# writer, and one
// TirEmitSource per writer, shared by the inliner.
using System.Reflection;
using NUnit.Framework;
using Ara3D.Geometry.Compiler;
using Ara3D.Geometry.Compiler.Checking;
using Ara3D.Geometry.CppWriter;
using Ara3D.Utils;

namespace PlatoCppWriterTests;

[TestFixture]
public class PassPipelineTests
{
    [Test]
    public void Cpp_pass_order_names_registered_shared_passes()
    {
        Assert.That(CppTirPipeline.PassOrder, Is.EqualTo(new[] { "inline", "constant-counts", "fuse", "fuse-consumers", "sink-bindings", "share-calls", "lower-loops", "hoist-invariants" }));
        Assert.That(PassRegistry.Shared.Resolve(CppTirPipeline.PassOrder).Count, Is.EqualTo(CppTirPipeline.PassOrder.Count));
    }

    [Test]
    public void Pipeline_holds_no_csharp_writer_and_no_tir_source_of_its_own()
    {
        static IEnumerable<FieldInfo> Fields(Type t) => t.GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
            .Concat(t.GetNestedTypes(BindingFlags.NonPublic | BindingFlags.Public).SelectMany(Fields));
        foreach (var f in Fields(typeof(CppTirPipeline)))
        {
            Assert.That(f.FieldType, Is.Not.EqualTo(typeof(TirEmitSource)), f.Name);
            Assert.That(f.FieldType.Namespace, Is.Not.EqualTo("Ara3D.Geometry.CSharpWriter"), f.Name);
        }
    }

    [Test]
    public void Cpu_writer_builds_one_tir_source_and_the_inliner_reads_it()
    {
        var folder = PlatoSource.FindRepoFolder("stdlib/foundation");
        var compilation = new Compilation(Ara3D.Logging.Logger.Null, Directory.GetFiles(folder, "*.plato").Select(PlatoSource.ParseFile).ToList());
        Assert.That(compilation.CompletedCompilation, Is.True);
        var output = new DirectoryPath(Path.Combine(Path.GetTempPath(), "plato-pass-pipeline"));
        var writer = compilation.ToCpp(output, CppDialect.Cpu, true, new CppCpuOptions(true, true, false, false, Array.Empty<string>()));
        var source = typeof(CppWriter).GetField("_tirSource", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(writer);
        Assert.That(source, Is.Not.Null, "the writer built its TIR source");
        var pipeline = typeof(CppWriter).GetProperty("CpuPipeline", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(writer)!;
        var ctx = (PassContext)typeof(CppTirPipeline).GetMethod("Context", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(pipeline, new object[] { writer })!;
        var statics = compilation.FunctionDefinitions.Where(f => writer.TryGetStaticTir(f) != null).Take(20).ToList();
        Assert.That(statics, Is.Not.Empty);
        foreach (var f in statics)
            Assert.That(ctx.InlineHost.TryGetStaticTir(f), Is.SameAs(writer.TryGetStaticTir(f)), f.Name);
    }
}
