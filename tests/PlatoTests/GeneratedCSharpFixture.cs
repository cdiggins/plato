// Emits isolated Plato fixtures while retaining resolution checks and temporary-file cleanup.
using Ara3D.Geometry.CSharpWriter;
using NUnit.Framework;

namespace PlatoTests;

internal static class GeneratedCSharpFixture
{
    public const string Prelude = @"
namespace Ara3D.Collections { }
namespace Ara3D.Geometry
{
    public static class Intrinsics
    {
        public static int CombineHashCodes(params object[] xs)
        {
            var h = 17;
            foreach (var x in xs) h = h * 31 + (x?.GetHashCode() ?? 0);
            return h;
        }
    }
}";

    public static CSharpWriter Emit(string source, string tempPrefix, string outputName)
    {
        var dir = Path.Combine(Path.GetTempPath(), tempPrefix + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        try
        {
            File.WriteAllText(Path.Combine(dir, "corpus.plato"), source);
            var comp = CheckerTestSupport.CompileFolder(dir);
            CollectionAssert.IsEmpty(comp.SymbolFactory.Errors.Select(e => e.ToString()).ToArray(),
                "the fixture must resolve cleanly or every assertion below would be vacuous");
            var w = new CSharpWriter(comp, outputName)
            {
                ExtensionStyle = true,
            };
            w.WriteAll();
            return w;
        }
        finally
        {
            try { Directory.Delete(dir, true); } catch { /* best effort */ }
        }
    }

    public static string File_(CSharpWriter w, string name)
    {
        Assert.IsTrue(w.Files.ContainsKey(name),
            $"expected an emitted file '{name}'; got: {string.Join(", ", w.Files.Keys)}");
        return w.Files[name].ToString();
    }
}
