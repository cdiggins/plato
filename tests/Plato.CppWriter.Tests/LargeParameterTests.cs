using NUnit.Framework;
using Ara3D.Geometry.Compiler;
using Ara3D.Geometry.CppWriter;
using Ara3D.Utils;

namespace PlatoCppWriterTests;

[TestFixture]
public class LargeParameterTests
{
    [Test]
    public void Readonly_records_borrow_but_assignments_and_returns_own_values()
    {
        var output = Path.Combine(Path.GetTempPath(), $"plato-cpu-borrow-{Guid.NewGuid():N}");
        Directory.CreateDirectory(output);
        var source = Path.Combine(output, "borrow.plato");
        var fields = string.Join(" ", Enumerable.Range(0, 40).Select(i => $"A{i}: Number;"));
        File.WriteAllText(source, "type Wide { Text: String; " + fields + " }\n" + """
            type Narrow { X: Number; Y: Number; }
            library BorrowFixture
            {
                ReadWide(value: Wide): Number => value.A0 + value.A39;
                CopyWide(value: Wide): Wide => value;
                ReadNarrow(value: Narrow): Number => value.X + value.Y;
                ReplaceWide(value: Wide, other: Wide): Wide
                {
                    value = other;
                    return value;
                }
                ReplaceText(value: String, other: String): String
                {
                    value = other;
                    return value;
                }
            }
            """);
        var folder = PlatoSource.FindRepoFolder("stdlib/foundation");
        var asts = Directory.GetFiles(folder, "*.plato").Append(source).Select(PlatoSource.ParseFile).ToList();
        var compilation = new Compilation(Ara3D.Logging.Logger.Null, asts);
        Assert.That(compilation.CompletedCompilation, Is.True);
        var roots = new[] { "ReadWide", "CopyWide", "ReadNarrow", "ReplaceWide", "ReplaceText" };
        var options = new CppCpuOptions(false, false, false, false, roots);
        var writer = compilation.ToCpp(new DirectoryPath(output), CppDialect.Cpu, true, options);
        var header = writer.Files["plato_functions.hpp"].ToString();
        Assert.That(header, Does.Contain("ReadWide(ReadOnlyParameter<Wide> value)"));
        Assert.That(header, Does.Contain("ReplaceWide(Wide value, ReadOnlyParameter<Wide> other)"));
        Assert.That(header, Does.Contain("ReplaceText(String value, const String& other)"));
        var owned = compilation.ToCpp(new DirectoryPath(output), CppDialect.Cpu, true,
            options with { BorrowLargeParameters = false });
        Assert.That(owned.Files["plato_functions.hpp"].ToString(), Does.Contain("ReadWide(Wide value)"));
        foreach (var file in writer.Files)
            File.WriteAllText(Path.Combine(output, file.Key), file.Value.ToString());
        var includes = writer.Files.Keys.Where(f => f.StartsWith("plato_functions_") && f.EndsWith(".cpp"))
            .Select(f => $"#include \"{f}\"");
        File.WriteAllText(Path.Combine(output, "main.cpp"), string.Join("\n", includes) + """

            int main()
            {
                using namespace plato;
                static_assert(std::is_same_v<decltype(&ReadWide), double(*)(const Wide&)>);
                static_assert(std::is_same_v<decltype(&ReadNarrow), double(*)(Narrow)>);
                static_assert(std::is_same_v<decltype(&CopyWide), Wide(*)(const Wide&)>);
                Wide a{}; a.A0 = 3; a.A39 = 4; a.Text = u"original";
                if (ReadWide(a) != 7 || ReadWide(Wide{}) != 0) return 1;
                auto copy = CopyWide(a); a.Text[0] = u'x';
                if (copy.Text != u"original") return 2;
                Wide replacement{}; replacement.Text = u"replacement";
                if (ReplaceWide(a, replacement).Text != u"replacement" || a.Text != u"xriginal") return 3;
                if (ReplaceText(u"before", u"after") != u"after") return 4;
                if (ReadNarrow(Narrow{2, 3}) != 5) return 5;
                return 0;
            }
            """);
        var vcvars = Toolchain.FindVcVars();
        if (vcvars == null)
            Assert.Ignore("MSVC is required for the native execution gate");
        var command = $"\"\"{vcvars}\" >nul && cl /nologo /std:c++17 /O2 /EHsc main.cpp /Fe:borrow.exe\"";
        var (exit, log) = Toolchain.Run("cmd.exe", "/c " + command, output, nativeEnvironment: true);
        Assert.That(exit, Is.Zero, log);
        var result = Toolchain.Run(Path.Combine(output, "borrow.exe"), "", output);
        Assert.That(result.ExitCode, Is.Zero, result.Output);
    }
}
