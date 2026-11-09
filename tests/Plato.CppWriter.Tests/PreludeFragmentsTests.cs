using System.Text.RegularExpressions;
using Ara3D.Geometry.CppWriter;
using NUnit.Framework;

namespace PlatoCppWriterTests;

/// <summary>
/// The prelude is fragments with options as macros. The gate of the split (cpp-opt-prelude): for the default options
/// and for W2 (BorrowMinBytes 32, BorrowNonTrivial, BorrowReads), the preprocessed text of the new prelude equals that
/// of the old single-file prelude as <c>TextFor</c> patched it. The old text is read from the commit before the split.
/// </summary>
[TestFixture]
public class PreludeFragmentsTests
{
    private const string OldCommit = "8b6efe01";
    private const string NL = "\n";

    private static readonly CppCpuOptions W2 = CppCpuOptions.Default with { BorrowMinBytes = 32, BorrowNonTrivial = true, BorrowReads = true };

    private static string OldText(CppCpuOptions o)
    {
        var (exit, text) = Toolchain.Run("git", $"show {OldCommit}:writers/Plato.CppWriter/cpu-prelude-check/plato_cpu.hpp",
            PlatoSource.FindRepoFolder("writers/Plato.CppWriter"));
        if (exit != 0)
            Assert.Ignore($"commit {OldCommit} is not available");
        if (o.BorrowMinBytes != 256 || o.BorrowNonTrivial)
        {
            var condition = $"sizeof(T) >= {o.BorrowMinBytes}" + (o.BorrowNonTrivial ? " || !std::is_trivially_copyable<T>::value" : "");
            text = text.Replace("template <class T> using ReadOnlyParameter = std::conditional_t<(sizeof(T) >= 256), const T&, T>;",
                $"template <class T> using ReadOnlyParameter = std::conditional_t<({condition}), const T&, T>;");
        }
        if (o.BorrowReads)
            text = text.Replace("template <class T> inline ReadOnlyParameter<T> array_at_readonly(Array<T> xs, int32_t i)",
                "template <class T> inline const T& array_at_readonly(Array<T> xs, int32_t i)");
        // TKT-1208: make_literal was added to plato_array.hpp after the split (--cpp-literal-calls).
        text = text.Replace("    template <class T> inline int32_t ToCount(Array<T> xs) { return xs.count; }",
            "    template <class T, class... Es>" + NL +
            "    inline Array<T> make_literal(const Es&... es)" + NL +
            "    {" + NL +
            "        constexpr int32_t n = static_cast<int32_t>(sizeof...(Es));" + NL +
            "        T* p = arena_alloc<T>(n);" + NL +
            "        int32_t i = 0;" + NL +
            "        ((p[i++] = es), ...);" + NL +
            "        return Array<T>{ p, n };" + NL +
            "    }" + NL + NL +
            "    template <class T> inline int32_t ToCount(Array<T> xs) { return xs.count; }");
        return text;
    }

    private static string Preprocess(string vcvars, string folder, string name, string defines)
    {
        File.WriteAllText(Path.Combine(folder, "tu.cpp"), $"#include \"{name}\"\n");
        var command = $"\"\"{vcvars}\" >nul && cl /nologo /std:c++17 /EP {defines} \"tu.cpp\"\"";
        var (exit, output) = Toolchain.Run("cmd.exe", "/c " + command, folder, nativeEnvironment: true);
        Assert.That(exit, Is.Zero, output);
        // The standard headers expand too; they are the same on both sides. Whitespace and blank lines carry no meaning.
        // The #line directives name the temporary folder, which differs between the two sides.
        output = Regex.Replace(output, @"plato-prelude-[a-z0-9-]+-[0-9a-f]{32}", "DIR");
        // assert expands __FILE__ and __LINE__, which name the fragment and its line.
        output = Regex.Replace(output, @"DIR[^""]*\.hpp"", \(unsigned\)\(\d+\)", "SITE");
        return Regex.Replace(output, @"\s+", " ");
    }

    private static string Write(string name, IEnumerable<KeyValuePair<string, string>> files)
    {
        var folder = Path.Combine(Path.GetTempPath(), $"plato-prelude-{name}-{Guid.NewGuid():N}");
        Directory.CreateDirectory(folder);
        foreach (var file in files)
            File.WriteAllText(Path.Combine(folder, file.Key), file.Value);
        return folder;
    }

    [TestCase("default")]
    [TestCase("w2")]
    public void Fragments_preprocess_to_the_old_single_file_prelude(string which)
    {
        var vcvars = Toolchain.FindVcVars();
        if (vcvars == null) Assert.Ignore("MSVC is required to preprocess");
        var options = which == "w2" ? W2 : CppCpuOptions.Default;
        var oldFolder = Write("old-" + which, new[] { new KeyValuePair<string, string>("plato_cpu.hpp", OldText(options)) });
        var newFolder = Write("new-" + which, CppCpuPrelude.FilesFor(options));
        var expected = Preprocess(vcvars!, oldFolder, "plato_cpu.hpp", "");
        var actual = Preprocess(vcvars!, newFolder, "plato_cpu.hpp", "");
        Assert.That(actual.Length, Is.GreaterThan(10000));
        Assert.That(actual, Is.EqualTo(expected));
    }

    [Test]
    public void A_define_on_the_command_line_selects_the_option_without_regenerating()
    {
        var vcvars = Toolchain.FindVcVars();
        if (vcvars == null) Assert.Ignore("MSVC is required to preprocess");
        var folder = Write("define", CppCpuPrelude.FilesFor(CppCpuOptions.Default));
        var fromFlags = Preprocess(vcvars!, folder, "plato_cpu.hpp", "/DPLATO_BORROW_MIN_BYTES=32 /DPLATO_BORROW_NON_TRIVIAL=1 /DPLATO_BORROW_READS=1");
        var generated = Preprocess(vcvars!, Write("define-w2", CppCpuPrelude.FilesFor(W2)), "plato_cpu.hpp", "");
        Assert.That(fromFlags, Is.EqualTo(generated));
    }

    [Test]
    public void Config_carries_the_option_values_and_the_default_file_matches_the_default_options()
    {
        var files = CppCpuPrelude.FilesFor(CppCpuOptions.Default).ToDictionary(f => f.Key, f => f.Value);
        Assert.That(files["plato_config.hpp"], Does.Contain("#define PLATO_BORROW_MIN_BYTES 256").And.Contain("#define PLATO_BORROW_READS 0"));
        var w2 = CppCpuPrelude.ConfigFor(W2);
        Assert.That(w2, Does.Contain("#define PLATO_BORROW_MIN_BYTES 32").And.Contain("#define PLATO_BORROW_NON_TRIVIAL 1").And.Contain("#define PLATO_BORROW_READS 1"));
        Assert.That(files.Keys, Does.Contain("plato_cpu.hpp").And.Contain("plato_borrow.hpp").And.Contain("plato_json.hpp"));
    }
}
