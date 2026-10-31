// Runs short-circuit fixtures interpreted and compiled under the pinned writer switches, and reads the emitted C#.
using System.Text.RegularExpressions;
using Ara3D.Geometry.Compiler.Types;
using Ara3D.Geometry.CSharpWriter;
using Ara3D.Geometry.Interpreter;
using Ara3D.Geometry.Laws;

namespace Plato.Interpreter.Tests;

/// <summary>
/// TKT-1141: Boolean <c>&amp;&amp;</c> and <c>||</c> evaluate their right operand only when the left does not
/// decide (docs/SEMANTICS.md section 6). Every right operand below throws when it is evaluated on the inputs its
/// left operand rules out: an out-of-range <c>At</c>, alone, inside a loop that the loop lowerer and the fuser
/// rewrite, and as a call that <c>--share-calls</c> would bind once. The laws must give the same verdict in the
/// interpreter, in compiled C# with no switches, and in compiled C# with the switches the plato-brep pin uses
/// (<c>--lower-scalars</c> makes the Boolean a <c>bool</c>); a hoisted right operand would throw in the compiled runs.
/// </summary>
[TestFixture, Category("Slow")]
public class ShortCircuitProgramTests
{
    private const string Source = """
        library ScGuard
        {
            Pick(xs: Array<Integer>, i: Integer): Integer => xs[i];
            AndAt(xs: Array<Integer>): Boolean => xs.Count > 3 && xs[3] == 0;
            OrAt(xs: Array<Integer>): Boolean => xs.Count <= 3 || xs[3] == 0;
            AndFn(xs: Array<Integer>): Boolean => And(xs.Count > 3, xs[3] == 0);
            OrFn(xs: Array<Integer>): Boolean => Or(xs.Count <= 3, xs[3] == 0);
            AndLoop(xs: Array<Integer>): Boolean => xs.Count > 3 && MapRange(4, i => xs[i]).Filter(x => x != 0).Count == 0;
            OrLoop(xs: Array<Integer>): Boolean => xs.Count <= 3 || MapRange(4, i => xs[i]).Filter(x => x != 0).Count == 0;
            FusedLoop(xs: Array<Integer>): Boolean => xs.Count > 3 && MapRange(4, i => i).Map(i => xs[i]).Filter(x => x != 0).Count == 0;
            AndCall(xs: Array<Integer>): Boolean => xs.Count > 3 && Pick(xs, 3) == Pick(xs, 3);
            OrCall(xs: Array<Integer>): Boolean => xs.Count <= 3 || Pick(xs, 3) == Pick(xs, 3);
            Chain(xs: Array<Integer>): Boolean => xs.Count > 3 && xs[3] == 1 || xs.Count > 1 && xs[1] == 2 || xs.Count == 0;
            AnyGuard(xs: Array<Integer>): Boolean => MapRange(xs.Count, i => i).Any(i => i < 1 && xs[i] == 0);

            Law_AndAt(k: Integer): Boolean => !AndAt(MapRange(3, i => k)) && AndAt(MapRange(5, i => k)) == (k == 0);
            Law_OrAt(k: Integer): Boolean => OrAt(MapRange(3, i => k)) && OrAt(MapRange(5, i => k)) == (k == 0);
            Law_AndFn(k: Integer): Boolean => !AndFn(MapRange(3, i => k)) && AndFn(MapRange(5, i => k)) == (k == 0);
            Law_OrFn(k: Integer): Boolean => OrFn(MapRange(3, i => k)) && OrFn(MapRange(5, i => k)) == (k == 0);
            Law_AndLoop(k: Integer): Boolean => !AndLoop(MapRange(3, i => k)) && AndLoop(MapRange(5, i => k)) == (k == 0);
            Law_OrLoop(k: Integer): Boolean => OrLoop(MapRange(3, i => k)) && OrLoop(MapRange(5, i => k)) == (k == 0);
            Law_FusedLoop(k: Integer): Boolean => !FusedLoop(MapRange(3, i => k)) && FusedLoop(MapRange(5, i => k)) == (k == 0);
            Law_AndCall(k: Integer): Boolean => !AndCall(MapRange(3, i => k)) && AndCall(MapRange(5, i => k));
            Law_OrCall(k: Integer): Boolean => OrCall(MapRange(3, i => k)) && OrCall(MapRange(5, i => k));
            Law_Chain(k: Integer): Boolean => Chain(MapRange(2, i => 2)) && !Chain(MapRange(1, i => 5)) && Chain(MapRange(0, i => k));
            Law_AnyGuard(k: Integer): Boolean => AnyGuard(MapRange(2, i => k)) == (k == 0);
        }
        """;

    private static readonly string[] LawNames =
    {
        "ScGuard.Law_AndAt", "ScGuard.Law_OrAt", "ScGuard.Law_AndFn", "ScGuard.Law_OrFn", "ScGuard.Law_AndLoop", "ScGuard.Law_OrLoop",
        "ScGuard.Law_FusedLoop", "ScGuard.Law_AndCall", "ScGuard.Law_OrCall", "ScGuard.Law_Chain", "ScGuard.Law_AnyGuard",
    };

    private static readonly LoopOptimizationOptions PinnedSwitches = new(FuseConsumers: true, ExactOutputAllocation: true,
        LeanInlining: true, ShareCalls: true, LowerScalars: true);

    private static readonly Lazy<Ara3D.Geometry.Compiler.Compilation> Compilation = new(() => LawRunnerSelectionTests.CompileSource(Source));

    private sealed record Runs(IReadOnlyList<LawResult> Interpreted, IReadOnlyList<LawResult> Plain, IReadOnlyList<LawResult> Pinned);

    private static readonly Lazy<Runs> AllRuns = new(() =>
    {
        var options = new LawRunOptions { Filters = new[] { "ScGuard" } };
        var interp = new Ara3D.Geometry.Interpreter.Interpreter(Compilation.Value, NumberProfile.Cpu64);
        var interpreted = new LawRunner(interp, options).RunAll();
        IReadOnlyList<LawResult> Compiled(bool pinned)
        {
            var program = pinned
                ? CSharpProgram.Build(Compilation.Value, NumberProfile.Cpu64, lowerLoops: true, fuse: true, loopOptimizations: PinnedSwitches)
                : CSharpProgram.Build(Compilation.Value, NumberProfile.Cpu64);
            return new LawRunner(CSharpBackend.Over(program, interp), interp, options).RunAll();
        }
        return new Runs(interpreted, Compiled(false), Compiled(true));
    });

    private static string Verdicts(IEnumerable<LawResult> results)
        => string.Join("\n", results.OrderBy(r => r.Name, StringComparer.Ordinal).Select(r => $"{r.Name} {r.Status.ToString().ToLowerInvariant()} {r.Trials}"));

    [Test]
    public void Every_guarded_law_passes_interpreted_and_compiled_with_and_without_the_pinned_switches()
    {
        var runs = AllRuns.Value;
        Assert.That(runs.Interpreted.Select(r => r.Name), Is.EquivalentTo(LawNames));
        var failed = runs.Interpreted.Concat(runs.Plain).Concat(runs.Pinned).Where(r => r.Status != LawStatus.Pass).Select(r => r.ToString());
        Assert.That(failed, Is.Empty);
    }

    [Test]
    public void Verdicts_are_equal_in_the_three_runs()
    {
        var runs = AllRuns.Value;
        Assert.That(Verdicts(runs.Plain), Is.EqualTo(Verdicts(runs.Interpreted)));
        Assert.That(Verdicts(runs.Pinned), Is.EqualTo(Verdicts(runs.Interpreted)));
    }

    private static string EmitScGuard(bool pinned)
    {
        var writer = new CSharpWriter(Compilation.Value, "unused-short-circuit") { ExtensionStyle = true, Profile = NumberProfile.Cpu64 };
        if (pinned)
        {
            writer.LowerLoops = true;
            writer.FuseCombinators = true;
            writer.FuseConsumers = writer.ExactOutputAllocation = writer.LeanInlining = writer.ShareCalls = writer.LowerScalars = true;
        }
        writer.WriteAll();
        return string.Join("\n", writer.Files.Where(kv => kv.Value.ToString().Contains("AndAt")).Select(kv => kv.Value.ToString()));
    }

    private static readonly Lazy<string> PlainText = new(() => EmitScGuard(false));
    private static readonly Lazy<string> PinnedText = new(() => EmitScGuard(true));

    private static string EmittedBody(string method, bool pinned)
    {
        var text = pinned ? PinnedText.Value : PlainText.Value;
        var start = Regex.Match(text, $@"Boolean {method}\(").Index;
        Assert.That(text, Does.Contain($"Boolean {method}("), $"{method} was not emitted");
        var end = text.IndexOf("public static ", start + 1, StringComparison.Ordinal);
        return text.Substring(start, (end < 0 ? text.Length : end) - start);
    }

    [TestCase("AndAt", false)]
    [TestCase("AndAt", true)]
    [TestCase("AndLoop", true)]
    [TestCase("AndCall", true)]
    public void And_is_emitted_as_a_conditional_whose_false_arm_is_the_constant(string method, bool pinned)
        => Assert.That(EmittedBody(method, pinned), Does.Match(@"(\?[^;]*:\s*[^;]*\(false\)|\?[^;]*:\s*\(*\(Boolean\)false|_eval\d+ = false)"));

    [TestCase("OrAt", false)]
    [TestCase("OrAt", true)]
    [TestCase("OrLoop", true)]
    [TestCase("OrCall", true)]
    public void Or_is_emitted_as_a_conditional_whose_true_arm_is_the_constant(string method, bool pinned)
        => Assert.That(EmittedBody(method, pinned), Does.Match(@"(\?\s*[^;:]*\(true\)|\?\s*\(+\(Boolean\)true|_eval\d+ = true)"));

    [Test]
    public void The_right_operand_is_not_read_before_the_guard()
    {
        // A hoisted right operand would show up as a read or a loop before the guard's Count test.
        foreach (var method in new[] { "AndLoop", "OrLoop", "FusedLoop", "AndCall", "OrCall" })
        {
            var body = EmittedBody(method, true);
            var guard = Regex.Match(body, @"Count");
            Assert.That(guard.Success, $"{method}: {body}");
            var firstRead = Regex.Match(body, @"(Pick\(|\bxs\[|\.At\(|\bfor\b|\bwhile\b)");
            if (firstRead.Success)
                Assert.That(firstRead.Index, Is.GreaterThan(guard.Index), $"{method}: the right operand comes before the guard\n{body}");
        }
    }
}
