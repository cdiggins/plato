using System.Runtime.CompilerServices;
using Ara3D.Geometry.Compiler.Types;
using Ara3D.Geometry.Interpreter;
using Ara3D.Geometry.Laws;
using Ara3D.Geometry.Navigation;
using NUnit.Framework;

namespace Plato.Navigation.Tests;

[TestFixture]
[NonParallelizable]
public class WarmLawBudgetTests
{
    private static string Foundation([CallerFilePath] string source = "")
    {
        var directory = Path.GetDirectoryName(source) ?? throw new DirectoryNotFoundException("source has no directory: " + source);
        for (var dir = new DirectoryInfo(directory); dir != null; dir = dir.Parent)
            if (Directory.Exists(Path.Combine(dir.FullName, "stdlib", "foundation")))
                return Path.Combine(dir.FullName, "stdlib", "foundation");
        throw new DirectoryNotFoundException("no stdlib/foundation above " + source);
    }

    [TestCase(LawBackendKind.Interpreter, 1)]
    [TestCase(LawBackendKind.Interpreter, 2)]
    [TestCase(LawBackendKind.CSharp, 1)]
    [TestCase(LawBackendKind.CSharp, 2)]
    [TestCase(LawBackendKind.Differential, 1)]
    [TestCase(LawBackendKind.Differential, 2)]
    public void Cold_and_reused_programs_agree_with_a_finite_law_budget(LawBackendKind kind, int parallel)
    {
        var folder = Path.Combine(Path.GetTempPath(), "warm-budget-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        try
        {
            File.WriteAllText(Path.Combine(folder, "laws.plato"), """
                type WarmBudgetValue { Value: Integer; Step: Function1<Integer, Integer>; }
                library WarmBudgetFixture
                {
                    Witness_Value(_: WarmBudgetValue): WarmBudgetValue => new WarmBudgetValue(2, x => x + 1);
                    Law_First(x: WarmBudgetValue): Boolean => x.Value == 2;
                    Law_Second(x: WarmBudgetValue): Boolean => x.Value > 0;
                }
                """);
            var request = new LawRunRequest
            {
                Folders = new[] { Foundation(), folder }, Profile = NumberProfile.Cpu64, Backend = kind,
                Options = new LawRunOptions { Filters = new[] { "WarmBudgetFixture" }, BudgetSeconds = 2, MaxParallel = parallel },
            };
            var runner = new WarmLawRunner();
            var cold = runner.Run(request);
            var warm = runner.Run(request);
            Assert.That(cold.Errors, Is.Empty);
            Assert.That(warm.Errors, Is.Empty);
            Assert.That(cold.Results, Has.Count.EqualTo(2));
            Assert.That(cold.Results.Select(r => r.Status), Is.All.EqualTo(LawStatus.Pass));
            Assert.That(warm.Results.Select(r => (r.Name, r.Status, r.Trials)),
                Is.EqualTo(cold.Results.Select(r => (r.Name, r.Status, r.Trials))));
            Assert.That(warm.Recompiled, Is.False);
            if (kind != LawBackendKind.Interpreter)
                Assert.That(warm.BackendReused, Is.True);
        }
        finally { Directory.Delete(folder, recursive: true); }
    }
}
