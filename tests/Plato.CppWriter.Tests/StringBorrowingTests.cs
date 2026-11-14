using NUnit.Framework;
using Ara3D.Geometry.Compiler;
using Ara3D.Geometry.CppWriter;
using Ara3D.Utils;

namespace PlatoCppWriterTests;

[TestFixture]
public class StringBorrowingTests
{
    /// <summary>Under each literal policy: a String parameter is a const reference and a result a value; a result is
    /// unaffected when the caller's variable is given another String; and a constant that views a literal (PLATO_STR,
    /// the pool) outlives an arena reset, while a make_string_n constant is in the arena.</summary>
    [TestCase("make", false, false)]
    [TestCase("static", true, false)]
    [TestCase("pool", true, true)]
    public void Cpu_strings_are_borrowed_as_parameters_and_values_as_results(string policy, bool staticLiterals, bool pool)
    {
        var output = Path.Combine(Path.GetTempPath(), $"plato-cpu-strings-{Guid.NewGuid():N}");
        Directory.CreateDirectory(output);
        var source = Path.Combine(output, "strings.plato");
        File.WriteAllText(source, """
            type TextRecord { Value: Character; }
            library BorrowingFixture
            {
                EchoText(text: String): String => text;
                EchoRecord(record: TextRecord): TextRecord => record;
                FirstText(text: String): Character => text[0];
                Tail(text: String): String => Substring(text, 1, text.Count);
                UnicodeText(): String => "André Ω 😀";
            }
            """);
        var folder = PlatoSource.FindRepoFolder("stdlib/foundation");
        var asts = Directory.GetFiles(folder, "*.plato").Append(source).Select(PlatoSource.ParseFile).ToList();
        var compilation = new Compilation(Ara3D.Logging.Logger.Null, asts);
        Assert.That(compilation.CompletedCompilation, Is.True);
        var options = new CppCpuOptions(true, true, false, false, new[] { "EchoText", "FirstText", "UnicodeText", "EchoRecord", "Tail" })
        {
            StaticStringLiterals = staticLiterals, StringPool = pool,
        };
        var writer = compilation.ToCpp(new DirectoryPath(output), CppDialect.Cpu, true, options);
        Assert.That(writer.Files["plato_functions.hpp"].ToString(),
            Does.Contain("String EchoText(const String& text)"));
        var spelled = string.Concat(writer.Files.Where(f => f.Key.StartsWith("plato_functions")).Select(f => f.Value.ToString()));
        Assert.That(spelled, Does.Contain(policy == "make" ? "make_string_n(\"" : policy == "static" ? "PLATO_STR(u\"" : "PLATO_SP("));
        Assert.That(writer.Files.ContainsKey("plato_strings.cpp"), Is.EqualTo(pool));
        foreach (var file in writer.Files)
            File.WriteAllText(Path.Combine(output, file.Key), file.Value.ToString());
        var includes = writer.Files.Keys.Where(f => f.EndsWith(".cpp"))
            .Select(f => $"#include \"{f}\"");
        File.WriteAllText(Path.Combine(output, "main.cpp"), $"#define PLATO_TEST_CONSTANTS_ARE_LITERALS {(staticLiterals || pool ? 1 : 0)}\n" + string.Join("\n", includes) + """


            int main()
            {
                using namespace plato;
                String (*echo)(const String&) = EchoText;
                String text = u"first";
                auto saved = echo(text);
                text = u"xirst";   // a String's units are never written: the variable is given another String
                if (saved != u"first" || echo(String(u"temporary")) != u"temporary") return 1;
                if (FirstText(String(u"abc")) != 'a') return 2;
                if (UnicodeText() != u"Andr\u00e9 \u03a9 \U0001f600" || UnicodeText().size() != 10) return 3;
                if (Tail(String(u"Plato")) != u"lato" || !Tail(String()).empty() || Tail(saved).data() != saved.data() + 1) return 6;
                // A constant that views a literal is not in the arena: evaluating it allocates nothing, and it is
                // still the same text after a reset has handed the arena's storage to something else.
                const String constant = UnicodeText();
                const size_t before = arena_bytes();
                UnicodeText();
                if ((arena_bytes() == before) != (PLATO_TEST_CONSTANTS_ARE_LITERALS != 0)) return 7;
                const String arena_text = String(u"Hello, ") + Tail(String(u"=Plato"));
                const std::u16string greeting(arena_text.data(), arena_text.size());   // the host's deep copy
                arena_reset();
                char16_t* reused = string_alloc(64);
                for (int i = 0; i < 64; ++i) reused[i] = u'#';
                if (PLATO_TEST_CONSTANTS_ARE_LITERALS && constant != u"Andr\u00e9 \u03a9 \U0001f600") return 8;
                if (greeting != u"Hello, Plato") return 9;
                TextRecord record{};
                record.Value = u'x';
                from_json(Json::Object(), record);
                if (record.Value != 0) return 4;
                Json populated = Json::Object();
                populated.set("Value", to_json(char16_t(937)));
                from_json(populated, record);
                if (record.Value != 937) return 5;
                return 0;
            }
            """);
        var vcvars = Toolchain.FindVcVars();
        if (vcvars == null)
            Assert.Ignore("MSVC is required for the native execution gate");
        var command = $"\"\"{vcvars}\" >nul && cl /nologo /std:c++17 /EHsc main.cpp /Fe:strings.exe\"";
        var (exit, log) = Toolchain.Run("cmd.exe", "/c " + command, output, nativeEnvironment: true);
        Assert.That(exit, Is.Zero, log);
        var result = Toolchain.Run(Path.Combine(output, "strings.exe"), "", output);
        Assert.That(result.ExitCode, Is.Zero, result.Output);
    }

    /// <summary>A String is a view, so a call that returns one is not a scalar region: the region would roll the arena back
    /// under the String's code units. RegionText (GoldenFixtureScalarRegions.plato) returns a view of an array it mapped
    /// in the arena; with scalar regions on, the two Strings RegionTexts returns must still read their text after more
    /// has been allocated.</summary>
    [Test]
    public void A_call_returning_a_String_is_not_a_scalar_region()
    {
        var options = GoldenEmissionTests.CpuDefaults(GoldenEmissionTests.ScalarRegionRoots, "--cpp-scalar-regions");
        var output = Path.Combine(Path.GetTempPath(), $"plato-cpu-string-regions-{Guid.NewGuid():N}");
        Directory.CreateDirectory(output);
        var writer = GoldenEmissionTests.SharedScalarRegions.Value.ToCpp(new DirectoryPath(output), CppDialect.Cpu, true, options);
        var functions = string.Concat(writer.Files.Where(f => f.Key.StartsWith("plato_functions")).Select(f => f.Value.ToString()));
        Assert.That(functions, Does.Contain("plato::arena_rollback"), "the fixture's scalar calls are still regions");
        var texts = functions.Substring(functions.LastIndexOf("Array<String> RegionTexts(", StringComparison.Ordinal));
        texts = texts.Substring(0, texts.IndexOf("\n\n", StringComparison.Ordinal));   // a blank line ends a definition
        Assert.That(texts, Does.Contain("RegionText(").And.Not.Contain("arena_mark"));
        foreach (var file in writer.Files)
            File.WriteAllText(Path.Combine(output, file.Key), file.Value.ToString());
        var includes = writer.Files.Keys.Where(f => f.EndsWith(".cpp")).Select(f => $"#include \"{f}\"");
        File.WriteAllText(Path.Combine(output, "main.cpp"), string.Join("\n", includes) + """


            int main()
            {
                using namespace plato;
                const int32_t first[] = { 80, 108, 97, 116, 111 }, second[] = { 118, 105, 101, 119, 115, 33 };
                const Array<String> texts = RegionTexts(Array<int32_t>{ first, 5 }, Array<int32_t>{ second, 6 });
                char16_t* later = string_alloc(4096);
                for (int i = 0; i < 4096; ++i) later[i] = u'#';
                if (texts.count != 2) return 1;
                if (texts.data[0] != u"Plato" || texts.data[1] != u"views!") return 2;
                return 0;
            }
            """);
        var vcvars = Toolchain.FindVcVars();
        if (vcvars == null)
            Assert.Ignore("MSVC is required for the native execution gate");
        var command = $"\"\"{vcvars}\" >nul && cl /nologo /std:c++17 /O2 /EHsc main.cpp /Fe:regions.exe\"";
        var (exit, log) = Toolchain.Run("cmd.exe", "/c " + command, output, nativeEnvironment: true);
        Assert.That(exit, Is.Zero, log);
        var result = Toolchain.Run(Path.Combine(output, "regions.exe"), "", output);
        Assert.That(result.ExitCode, Is.Zero, result.Output);
    }
}
