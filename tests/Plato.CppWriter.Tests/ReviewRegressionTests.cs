// Runs generated C++ under the Cpu defaults for the defects a review found: affine builders hoisted out of a loop
// (TKT-1202), a Boolean chain without outer parentheses (TKT-1203) and a user predicate named like a case (TKT-1204).
using NUnit.Framework;
using Ara3D.Geometry.Compiler;
using Ara3D.Geometry.CppWriter;
using Ara3D.Utils;

namespace PlatoCppWriterTests;

[TestFixture]
public class ReviewRegressionTests
{
    private const string Source = """
        type Sh = Circle(R: Number) | Square(S: Number) | Other(O: Number);
        library ReviewFixture
        {
            FreshList(xs: Array<Number>): Array<Array<Number>> => xs.Map(x => new List<Number>().Add(0.0).Add(x).Freeze());
            FreshBuffer(xs: Array<Number>): Array<Array<Number>> => xs.Map(x => new Buffer<Number>(1).Set(0, x).Freeze());
            NotAllPositive(v: Vector3D): Boolean => !v.ComponentArray.All(x => x > 0.0);
            AllPositiveIs(v: Vector3D, b: Boolean): Boolean => v.ComponentArray.All(x => x > 0.0) == b;
            IsCircle(s: Sh): Boolean => match (s) { Circle(r) => r > 0.0; Square(q) => q > 0.0; Other(o) => false; };
            Classify(s: Sh): Integer => IsCircle(s) ? 1 : (IsSquare(s) ? 2 : 3);
            Matched(s: Sh): Integer => match (s) { Circle(r) => 1; Square(q) => 2; Other(o) => 3; };
            MakeSquare(x: Number): Sh => Sh.Square(x);
            MakeOther(x: Number): Sh => Sh.Other(x);
        }
        """;

    private static readonly string[] Roots =
    {
        "FreshList", "FreshBuffer", "NotAllPositive", "AllPositiveIs", "IsCircle", "Classify", "Matched", "MakeSquare", "MakeOther",
    };

    private const string Main = """


        using namespace plato;

        static int Rows(Array<Array<double>> rows, int width)
        {
            double expected[3] = { 1, 2, 3 };
            if (rows.count != 3) return 1;
            for (int32_t i = 0; i < 3; i++)
            {
                if (rows.data[i].count != width) return 2;
                if (width == 2 && rows.data[i].data[0] != 0.0) return 3;
                if (rows.data[i].data[width - 1] != expected[i]) return 4;
            }
            return 0;
        }

        int main()
        {
            double values[3] = { 1, 2, 3 };
            auto xs = make_array(values, 3);
            if (Rows(FreshList(xs), 2)) return 10;
            if (Rows(FreshBuffer(xs), 1)) return 11;
            Vector3D mixed{}; mixed.X = 1; mixed.Y = -1; mixed.Z = 1;
            Vector3D positive{}; positive.X = 1; positive.Y = 2; positive.Z = 3;
            if (!NotAllPositive(mixed)) return 20;
            if (NotAllPositive(positive)) return 21;
            if (!AllPositiveIs(mixed, false) || AllPositiveIs(mixed, true)) return 22;
            if (!AllPositiveIs(positive, true) || AllPositiveIs(positive, false)) return 23;
            Sh square = MakeSquare(2.0);
            Sh other = MakeOther(2.0);
            if (!IsCircle(square) || IsCircle(other)) return 30;
            if (Classify(square) != 1 || Classify(other) != 3) return 31;
            if (Matched(square) != 2 || Matched(other) != 3) return 32;
            return 0;
        }
        """;

    [Test]
    public void Generated_code_keeps_the_source_meaning_under_the_Cpu_defaults()
    {
        var output = Path.Combine(Path.GetTempPath(), $"plato-review-{Guid.NewGuid():N}");
        Directory.CreateDirectory(output);
        var source = Path.Combine(output, "review.plato");
        File.WriteAllText(source, Source);
        var folder = PlatoSource.FindRepoFolder("stdlib/foundation");
        var asts = Directory.GetFiles(folder, "*.plato").Where(f => !Path.GetFileName(f).StartsWith("random.") && !Path.GetFileName(f).StartsWith("statistics.")).Append(source).Select(PlatoSource.ParseFile).ToList();
        var compilation = new Compilation(Ara3D.Logging.Logger.Null, asts);
        Assert.That(compilation.CompletedCompilation, Is.True);
        var writer = compilation.ToCpp(new DirectoryPath(output), CppDialect.Cpu, true, GoldenEmissionTests.CpuDefaults(Roots));
        foreach (var file in writer.Files)
            File.WriteAllText(Path.Combine(output, file.Key), file.Value.ToString());
        var includes = writer.Files.Keys.Where(f => f.EndsWith(".cpp")).Select(f => $"#include \"{f}\"");
        File.WriteAllText(Path.Combine(output, "main.cpp"), string.Join("\n", includes) + Main);
        var vcvars = Toolchain.FindVcVars();
        if (vcvars == null)
            Assert.Ignore("MSVC is required for the native execution gate");
        var command = $"\"\"{vcvars}\" >nul && cl /nologo /std:c++17 /O2 /EHsc /bigobj main.cpp /Fe:review.exe\"";
        var (exit, log) = Toolchain.Run("cmd.exe", "/c " + command, output, nativeEnvironment: true);
        Assert.That(exit, Is.Zero, log);
        var result = Toolchain.Run(Path.Combine(output, "review.exe"), "", output);
        Assert.That(result.ExitCode, Is.Zero, $"output folder {output}\n{result.Output}");
    }
}
