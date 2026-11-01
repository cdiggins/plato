using NUnit.Framework;
using Ara3D.Geometry.Compiler;
using Ara3D.Geometry.CppWriter;
using Ara3D.Utils;

namespace PlatoCppWriterTests;

[TestFixture]
public class StringBorrowingTests
{
    [Test]
    public void Cpu_strings_are_borrowed_as_parameters_and_owned_as_results()
    {
        var output = Path.Combine(Path.GetTempPath(), $"plato-cpu-strings-{Guid.NewGuid():N}");
        Directory.CreateDirectory(output);
        var source = Path.Combine(output, "strings.plato");
        File.WriteAllText(source, """
            library BorrowingFixture
            {
                EchoText(text: String): String => text;
                FirstText(text: String): Character => text[0];
            }
            """);
        var folder = PlatoSource.FindRepoFolder("stdlib/foundation");
        var asts = Directory.GetFiles(folder, "*.plato").Append(source).Select(PlatoSource.ParseFile).ToList();
        var compilation = new Compilation(Ara3D.Logging.Logger.Null, asts);
        Assert.That(compilation.CompletedCompilation, Is.True);
        var options = new CppCpuOptions(true, true, false, false, new[] { "EchoText", "FirstText" });
        var writer = compilation.ToCpp(new DirectoryPath(output), CppDialect.Cpu, true, options);
        Assert.That(writer.Files["plato_functions.hpp"].ToString(),
            Does.Contain("String EchoText(const String& text)"));
        foreach (var file in writer.Files)
            File.WriteAllText(Path.Combine(output, file.Key), file.Value.ToString());
        var includes = writer.Files.Keys.Where(f => f.StartsWith("plato_functions_") && f.EndsWith(".cpp"))
            .Select(f => $"#include \"{f}\"");
        File.WriteAllText(Path.Combine(output, "main.cpp"), string.Join("\n", includes) + """

            int main()
            {
                using namespace plato;
                String (*echo)(const String&) = EchoText;
                String text = "first";
                auto saved = echo(text);
                text[0] = 'x';
                if (saved != "first" || echo(String("temporary")) != "temporary") return 1;
                if (FirstText(String("abc")) != 'a') return 2;
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
}
