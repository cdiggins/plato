using NUnit.Framework;
using Ara3D.Geometry.Compiler;
using Ara3D.Geometry.CppWriter;
using Ara3D.Utils;

namespace PlatoCppWriterTests;

/// <summary>--cpp-borrow-reads, --cpp-borrow-nontrivial and --cpp-borrow-min-bytes: off by default and byte-identical
/// to the default prelude when off; when on, array reads, loop elements and copies of unwritten names bind by const
/// reference, written names and parameters stay owned, and the native program computes the same values.</summary>
[TestFixture]
public class BorrowReadsTests
{
    private const string Fixture = """
        type Tagged { Name: String; X: Number; }
        type Pair { A: Number; B: Number; }
        library BorrowReadsFixture
        {
            NameLength(t: Tagged): Integer => t.Name.Count;
            SumX(xs: Array<Tagged>): Number => xs.Map((t) => t.X).Reduce(0.0, (a, b) => a + b);
            Lengths(xs: Array<Tagged>): Array<Integer> => xs.Map((t) => NameLength(t));
            FirstName(xs: Array<Tagged>): String => xs[0].Name;
            KeepFirst(xs: Array<Tagged>): Tagged
            {
                var first = xs[0];
                return first;
            }
            Alias(t: Tagged): Number
            {
                var u = t;
                return u.X;
            }
            Rewritten(t: Tagged, other: Tagged): Number
            {
                var u = t;
                u = other;
                return u.X + t.X;
            }
            PairSum(p: Pair): Number => p.A + p.B;
        }
        """;

    private static (Compilation, string) Compile()
    {
        var output = Path.Combine(Path.GetTempPath(), $"plato-cpu-borrow-reads-{Guid.NewGuid():N}");
        Directory.CreateDirectory(output);
        var source = Path.Combine(output, "borrow-reads.plato");
        File.WriteAllText(source, Fixture);
        var folder = PlatoSource.FindRepoFolder("stdlib/foundation");
        var asts = Directory.GetFiles(folder, "*.plato").Append(source).Select(PlatoSource.ParseFile).ToList();
        var compilation = new Compilation(Ara3D.Logging.Logger.Null, asts);
        Assert.That(compilation.CompletedCompilation, Is.True);
        return (compilation, output);
    }

    private static readonly string[] Roots = { "NameLength", "SumX", "Lengths", "FirstName", "KeepFirst", "Alias", "Rewritten", "PairSum" };

    private static string Functions(CppWriter w) => string.Join("\n", w.Files.Where(f => f.Key.StartsWith("plato_functions_"))
        .Select(f => f.Value.ToString()));

    [Test]
    public void Default_options_keep_the_default_prelude()
    {
        Assert.That(CppCpuPrelude.TextFor(CppCpuOptions.Default), Is.EqualTo(CppCpuPrelude.Text));
        Assert.That(CppCpuPrelude.TextFor(null), Is.EqualTo(CppCpuPrelude.Text));
        var custom = CppCpuPrelude.TextFor(CppCpuOptions.Default with { BorrowMinBytes = 32, BorrowNonTrivial = true, BorrowReads = true });
        Assert.That(custom, Does.Contain("std::conditional_t<(sizeof(T) >= 32 || !std::is_trivially_copyable<T>::value), const T&, T>"));
        Assert.That(custom, Does.Contain("inline const T& array_at_readonly(Array<T> xs, int32_t i)"));
    }

    [TestCase(false)]
    [TestCase(true)]
    public void Borrowed_reads_bind_by_reference_and_compute_the_same_values(bool borrow)
    {
        var (compilation, output) = Compile();
        var options = new CppCpuOptions(true, false, false, false, Roots) { BorrowReads = borrow, BorrowNonTrivial = borrow };
        var writer = compilation.ToCpp(new DirectoryPath(output), CppDialect.Cpu, true, options);
        var functions = Functions(writer);
        var prelude = writer.Files["plato_cpu.hpp"].ToString();
        Assert.That(prelude.Contains("inline const T& array_at_readonly("), Is.EqualTo(borrow));
        Assert.That(functions.Contains("const auto& "), Is.EqualTo(borrow), functions);
        Assert.That(functions.Contains("const Tagged& first = "), Is.EqualTo(borrow), functions);
        Assert.That(functions.Contains("const Tagged& u = "), Is.EqualTo(borrow), functions);
        // A local that is written again stays an owned copy.
        Assert.That(functions, Does.Contain("Tagged u = t;"));
        var header = writer.Files["plato_functions.hpp"].ToString();
        Assert.That(header, Does.Contain("ReadOnlyParameter<Tagged> t"));
        foreach (var file in writer.Files)
            File.WriteAllText(Path.Combine(output, file.Key), file.Value.ToString());
        var includes = writer.Files.Keys.Where(f => f.StartsWith("plato_functions_") && f.EndsWith(".cpp"))
            .Select(f => $"#include \"{f}\"");
        File.WriteAllText(Path.Combine(output, "main.cpp"), $"#define PLATO_TEST_BORROW {(borrow ? 1 : 0)}\n" + string.Join("\n", includes) + """

            int main()
            {
                using namespace plato;
                using TaggedArg = std::conditional_t<PLATO_TEST_BORROW != 0, const Tagged&, Tagged>;
                static_assert(std::is_same_v<decltype(&NameLength), int32_t(*)(TaggedArg)>);
                static_assert(std::is_same_v<decltype(&PairSum), double(*)(Pair)>);
                auto rows = arena_alloc<Tagged>(3);
                rows[0].Name = u"a heap-backed name longer than the small-string buffer"; rows[0].X = 1;
                rows[1].Name = u"bb"; rows[1].X = 2;
                rows[2].Name = u"ccc"; rows[2].X = 4;
                Array<Tagged> xs{ rows, 3 };
                if (SumX(xs) != 7) return 1;
                auto lengths = Lengths(xs);
                if (lengths.count != 3 || lengths.data[1] != 2 || lengths.data[2] != 3) return 2;
                auto name = FirstName(xs);
                auto kept = KeepFirst(xs);
                rows[0].Name[0] = u'x';
                if (name[0] != u'a' || kept.Name[0] != u'a') return 3;
                if (Alias(rows[1]) != 2) return 4;
                if (Rewritten(rows[1], rows[2]) != 6) return 5;
                if (PairSum(Pair{ 2, 3 }) != 5) return 6;
                arena_reset();
                if (kept.Name.size() != 54 || name.size() != 54) return 7;
                return 0;
            }
            """);
        var vcvars = Toolchain.FindVcVars();
        if (vcvars == null)
            Assert.Ignore("MSVC is required for the native execution gate");
        var command = $"\"\"{vcvars}\" >nul && cl /nologo /std:c++17 /O2 /EHsc /W4 main.cpp /Fe:borrow-reads.exe\"";
        var (exit, log) = Toolchain.Run("cmd.exe", "/c " + command, output, nativeEnvironment: true);
        Assert.That(exit, Is.Zero, log);
        var result = Toolchain.Run(Path.Combine(output, "borrow-reads.exe"), "", output);
        Assert.That(result.ExitCode, Is.Zero, result.Output);
    }
}
