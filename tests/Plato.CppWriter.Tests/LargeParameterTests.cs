using NUnit.Framework;
using Ara3D.Geometry.Compiler;
using Ara3D.Geometry.CppWriter;
using Ara3D.Utils;

namespace PlatoCppWriterTests;

[TestFixture]
public class LargeParameterTests
{
    [TestCase(true, true)]
    [TestCase(true, false)]
    [TestCase(false, true)]
    [TestCase(false, false)]
    public void Readonly_records_borrow_but_assignments_and_returns_own_values(bool borrow, bool borrowReads)
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
                ReadWideArray(values: Array<Wide>): Number => ReadWide(values[0]);
                CopyWideArray(values: Array<Wide>): Wide => values[0];
                ReplaceWideArray(values: Array<Wide>, other: Wide): Wide => ReplaceWide(values[0], other);
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
        var roots = new[] { "ReadWide", "CopyWide", "ReadNarrow", "ReplaceWide", "ReplaceText", "ReadWideArray", "CopyWideArray", "ReplaceWideArray" };
        var options = new CppCpuOptions(false, false, false, false, roots) { BorrowLargeParameters = borrow, BorrowArrayReads = borrowReads };
        var writer = compilation.ToCpp(new DirectoryPath(output), CppDialect.Cpu, true, options);
        var header = writer.Files["plato_functions.hpp"].ToString();
        var parameter = borrow ? "ReadOnlyParameter<Wide>" : "Wide";
        Assert.That(header, Does.Contain($"ReadWide({parameter} value)"));
        Assert.That(header, Does.Contain($"ReplaceWide(Wide value, {parameter} other)"));
        var functions = string.Join("\n", writer.Files.Where(f => f.Key.StartsWith("plato_functions_"))
            .Select(f => f.Value.ToString()));
        Assert.That(functions.Contains("plato::array_at_readonly("), Is.EqualTo(borrowReads));
        Assert.That(header, Does.Contain("ReplaceText(String value, const String& other)"));
        var owned = compilation.ToCpp(new DirectoryPath(output), CppDialect.Cpu, true,
            options with { BorrowLargeParameters = false });
        Assert.That(owned.Files["plato_functions.hpp"].ToString(), Does.Contain("ReadWide(Wide value)"));
        foreach (var file in writer.Files)
            File.WriteAllText(Path.Combine(output, file.Key), file.Value.ToString());
        var includes = writer.Files.Keys.Where(f => f.StartsWith("plato_functions_") && f.EndsWith(".cpp"))
            .Select(f => $"#include \"{f}\"");
        File.WriteAllText(Path.Combine(output, "main.cpp"),  $"#define PLATO_TEST_BORROW {(borrow ? 1 : 0)}\n" + string.Join("\n", includes) + """

            int main()
            {
                using namespace plato;
                using WideArg = std::conditional_t<PLATO_TEST_BORROW != 0, const Wide&, Wide>;
                static_assert(std::is_same_v<decltype(&ReadWide), double(*)(WideArg)>);
                static_assert(std::is_same_v<decltype(&ReadNarrow), double(*)(Narrow)>);
                static_assert(std::is_same_v<decltype(&CopyWide), Wide(*)(WideArg)>);
                Wide a{}; a.A0 = 3; a.A39 = 4; a.Text = u"original";
                if (ReadWide(a) != 7 || ReadWide(Wide{}) != 0) return 1;
                auto copy = CopyWide(a); a.Text[0] = u'x';
                if (copy.Text != u"original") return 2;
                Wide replacement{}; replacement.Text = u"replacement";
                if (ReplaceWide(a, replacement).Text != u"replacement" || a.Text != u"xriginal") return 3;
                if (ReplaceText(u"before", u"after") != u"after") return 4;
                if (ReadNarrow(Narrow{2, 3}) != 5) return 5;
                auto rows = arena_alloc<Wide>(1);
                rows[0].A0 = 9; rows[0].A39 = 2;
                rows[0].Text = u"a heap-backed string payload that must survive scratch reset";
                Array<Wide> xs{rows, 1};
                if (ReadWideArray(xs) != 11) return 6;
                auto saved = CopyWideArray(xs);
                if (ReplaceWideArray(xs, replacement).Text != u"replacement") return 7;
                if (rows[0].Text != saved.Text) return 8;
                arena_reset();
                if (saved.Text != u"a heap-backed string payload that must survive scratch reset") return 9;
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
