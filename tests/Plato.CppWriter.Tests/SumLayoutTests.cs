using NUnit.Framework;
using Ara3D.Geometry.Compiler;
using Ara3D.Geometry.CppWriter;
using Ara3D.Utils;

namespace PlatoCppWriterTests;

/// <summary>Compiles one sum type with cases of every storage kind (plain numbers, an array, a String, nested records) under
/// each sum layout and runs the same checks on the native program: dispatch, equality, hash and JSON round trip must agree.</summary>
[TestFixture]
public class SumLayoutTests
{
    private const string Source = """
        type Pt { X: Number; Y: Number; }
        type Shape = Circle(Radius: Number) | Rect(Width: Number, Height: Number) | Dot | Poly(Points: Array<Pt>) | Label(Text: String, At: Pt) | Seg(A: Pt, B: Pt);
        library SumFixture
        {
            Measure(s: Shape): Number
                => match (s) {
                    Circle(r) => 3.0 * r * r;
                    Rect(w, h) => w * h;
                    Dot => 0.5;
                    Poly(ps) => 1000.0 + ps.Count;
                    Label(t, p) => 2000.0 + p.X;
                    Seg(a, b) => a.X + b.Y;
                };
            MakeCircle(r: Number): Shape => Shape.Circle(r);
            MakeRect(w: Number, h: Number): Shape => Shape.Rect(w, h);
            MakeDot(): Shape => Shape.Dot;
            MakePoly(ps: Array<Pt>): Shape => Shape.Poly(ps);
            MakeLabel(t: String, p: Pt): Shape => Shape.Label(t, p);
            MakeSeg(a: Pt, b: Pt): Shape => Shape.Seg(a, b);
            LabelText(s: Shape): String => match (s) { Label(t, p) => t; _ => ""; };
            SameShape(a: Shape, b: Shape): Boolean => a == b;
        }
        """;

    private static readonly string[] Roots =
        { "Measure", "MakeCircle", "MakeRect", "MakeDot", "MakePoly", "MakeLabel", "MakeSeg", "LabelText", "SameShape" };

    public static IEnumerable<TestCaseData> Layouts()
    {
        yield return new TestCaseData(new CppCpuOptions(false, false, false, false, Roots)).SetName("Sum_flat");
        yield return new TestCaseData(new CppCpuOptions(false, false, false, false, Roots) { SwitchSumDispatch = true }).SetName("Sum_switch");
    }

    [TestCaseSource(nameof(Layouts))]
    public void The_native_program_agrees_whatever_the_layout(CppCpuOptions options)
    {
        var output = Path.Combine(Path.GetTempPath(), $"plato-sum-layout-{Guid.NewGuid():N}");
        Directory.CreateDirectory(output);
        var source = Path.Combine(output, "sums.plato");
        File.WriteAllText(source, Source);
        var folder = PlatoSource.FindRepoFolder("stdlib/foundation");
        var asts = Directory.GetFiles(folder, "*.plato").Append(source).Select(PlatoSource.ParseFile).ToList();
        var compilation = new Compilation(Ara3D.Logging.Logger.Null, asts);
        Assert.That(compilation.CompletedCompilation, Is.True);
        var writer = compilation.ToCpp(new DirectoryPath(output), CppDialect.Cpu, true, options);
        foreach (var file in writer.Files)
            File.WriteAllText(Path.Combine(output, file.Key), file.Value.ToString());
        var units = writer.Files.Keys.Where(f => f.EndsWith(".cpp")).Select(f => $"#include \"{f}\"");
        File.WriteAllText(Path.Combine(output, "main.cpp"), string.Join("\n", units) + """

            int main()
            {
                using namespace plato;
                Pt p{ 1.0, 2.0 }, q{ 3.0, 4.0 };
                double xs[] = { 1.0, 2.0, 3.0 };
                Pt pts[] = { p, q };
                Shape c = MakeCircle(2.0), r = MakeRect(3.0, 4.0), d = MakeDot(), poly = MakePoly(Array<Pt>{ pts, 2 });
                Shape label = MakeLabel(u"a label long enough to live on the heap, not in the string", q), seg = MakeSeg(p, q);
                if (Measure(c) != 12.0 || Measure(r) != 12.0 || Measure(d) != 0.5 || Measure(poly) != 1002.0) return 1;
                if (Measure(label) != 2003.0 || Measure(seg) != 5.0) return 2;
                if (LabelText(label) != u"a label long enough to live on the heap, not in the string" || !LabelText(c).empty()) return 3;
                if (!SameShape(c, MakeCircle(2.0)) || SameShape(c, MakeCircle(2.5)) || SameShape(c, r) || SameShape(d, c)) return 4;
                if (!SameShape(label, MakeLabel(u"a label long enough to live on the heap, not in the string", q)) || SameShape(label, seg)) return 5;
                if (GetHashCode(c) != GetHashCode(MakeCircle(2.0)) || GetHashCode(c) == GetHashCode(r)) return 6;
                for (const Shape* s : { &c, &r, &d, &poly, &label, &seg })
                {
                    Shape back{};
                    from_json(to_json(*s), back);
                    if (!SameShape(*s, back) || Measure(*s) != Measure(back)) return 7;
                }
                Shape zero{};
                if (zero.Kind != 0) return 8;
                return 0;
            }
            """);
        var vcvars = Toolchain.FindVcVars();
        if (vcvars == null)
            Assert.Ignore("MSVC is required for the native execution gate");
        var command = $"\"\"{vcvars}\" >nul && cl /nologo /std:c++17 /O2 /EHsc /bigobj main.cpp /Fe:sums.exe\"";
        var (exit, log) = Toolchain.Run("cmd.exe", "/c " + command, output, nativeEnvironment: true);
        Assert.That(exit, Is.Zero, log);
        var result = Toolchain.Run(Path.Combine(output, "sums.exe"), "", output);
        Assert.That(result.ExitCode, Is.Zero, result.Output);
    }
}
