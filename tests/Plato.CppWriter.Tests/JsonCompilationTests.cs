using NUnit.Framework;
using Ara3D.Geometry.Compiler;
using Ara3D.Geometry.CppWriter;
using Ara3D.Utils;

namespace PlatoCppWriterTests;

[TestFixture]
public class JsonCompilationTests
{
    [Test]
    public void Split_json_units_link_and_preserve_cross_unit_records()
    {
        var output = Path.Combine(Path.GetTempPath(), $"plato-cpu-json-{Guid.NewGuid():N}");
        Directory.CreateDirectory(output);
        var source = Path.Combine(output, "records.plato");
        var records = Enumerable.Range(0, 180).Select(i => $"type Record{i} {{ Value: String; }}");
        File.WriteAllText(source, string.Join("\n", records) + """

            type RecordPair { First: Record0; Last: Record179; }
            library JsonFixture
            {
                EchoPair(pair: RecordPair): RecordPair => pair;
            }
            """);
        var folder = PlatoSource.FindRepoFolder("stdlib/foundation");
        var asts = Directory.GetFiles(folder, "*.plato").Append(source).Select(PlatoSource.ParseFile).ToList();
        var compilation = new Compilation(Ara3D.Logging.Logger.Null, asts);
        Assert.That(compilation.CompletedCompilation, Is.True);
        var options = new CppCpuOptions(false, false, false, false, new[] { "EchoPair" });
        var writer = compilation.ToCpp(new DirectoryPath(output), CppDialect.Cpu, true, options);
        var json = writer.Files.Where(f => f.Key.StartsWith("plato_json") && f.Key.EndsWith(".cpp")).ToArray();
        Assert.That(json.Length, Is.GreaterThan(1));
        var repeat = compilation.ToCpp(new DirectoryPath(output), CppDialect.Cpu, true, options);
        foreach (var file in json)
            Assert.That(repeat.Files[file.Key].ToString(), Is.EqualTo(file.Value.ToString()));
        foreach (var file in writer.Files)
            File.WriteAllText(Path.Combine(output, file.Key), file.Value.ToString());
        File.WriteAllText(Path.Combine(output, "main.cpp"), """
            #include "plato_functions.hpp"
            int main()
            {
                using namespace plato;
                RecordPair value{Record0{u"first"}, Record179{u"last"}};
                RecordPair copy{};
                from_json(to_json(EchoPair(value)), copy);
                if (copy.First.Value != u"first" || copy.Last.Value != u"last") return 1;
                from_json(Json::Object(), copy);
                if (!copy.First.Value.empty() || !copy.Last.Value.empty()) return 2;
                return 0;
            }
            """);
        var vcvars = Toolchain.FindVcVars();
        if (vcvars == null)
            Assert.Ignore("MSVC is required for the native link gate");
        var command = $"\"\"{vcvars}\" >nul && cl /nologo /std:c++17 /EHsc /MP4 *.cpp /Fe:json.exe\"";
        var (exit, log) = Toolchain.Run("cmd.exe", "/c " + command, output, nativeEnvironment: true);
        Assert.That(exit, Is.Zero, log);
        var result = Toolchain.Run(Path.Combine(output, "json.exe"), "", output);
        Assert.That(result.ExitCode, Is.Zero, result.Output);
    }
}
