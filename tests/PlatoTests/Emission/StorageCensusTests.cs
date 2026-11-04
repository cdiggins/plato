// An explicit census of the storage layer over a real program (TKT-1152 C6b): not part of any run.
using Ara3D.Geometry.CSharpWriter;
using NUnit.Framework;

namespace PlatoTests.Emission;

/// <summary>
/// Generates a program with the pinned switches plus <c>--owned-arrays --stack-arrays --span-params</c> (and, when asked,
/// <c>--scalar-arrays</c>) in strict mode, writes the output to a scratch folder, and prints the counts the plan's measurements
/// ask for. Run with <c>STORAGE_CENSUS_INPUTS</c> (folders separated by ';') and <c>STORAGE_CENSUS_OUT</c> set, e.g.
/// <c>dotnet test --filter Census</c>. Explicit: it needs a program outside this repository.
/// </summary>
[TestFixture, Explicit("needs STORAGE_CENSUS_INPUTS and STORAGE_CENSUS_OUT")]
public static class StorageCensusTests
{
    [TestCase(false)]
    [TestCase(true)]
    public static void Census(bool scalarArrays)
    {
        // The parser recurses deeply on the largest files: the compiler's own command line runs on a big stack.
        Exception failure = null;
        var thread = new Thread(() => { try { Run(scalarArrays); } catch (Exception e) { failure = e; } }, 512 * 1024 * 1024);
        thread.Start();
        thread.Join();
        if (failure != null)
            throw new InvalidOperationException(failure.Message, failure);
    }

    private static void Run(bool scalarArrays)
    {
        var inputs = Environment.GetEnvironmentVariable("STORAGE_CENSUS_INPUTS")!.Split(';');
        var outRoot = Environment.GetEnvironmentVariable("STORAGE_CENSUS_OUT")!;
        var asts = inputs.SelectMany(d => Directory.GetFiles(d, "*.plato")).Select(CheckerTestSupport.ParseFile).ToList();
        var compilation = new Ara3D.Geometry.Compiler.Compilation(Ara3D.Logging.Logger.Null, asts);
        var folder = Path.Combine(outRoot, scalarArrays ? "with-scalar-arrays" : "without");
        var writer = new CSharpWriter(compilation, folder)
        {
            ExtensionStyle = true,
            Profile = Ara3D.Geometry.Compiler.Types.NumberProfile.TryParse("cpu64i32"),
        };
        OwnedArrayEmissionTests.Pin(writer, true);
        (writer.StackArrays, writer.SpanParams, writer.ScalarArrays) = (true, true, scalarArrays);
        writer.WriteAll();
        Directory.CreateDirectory(folder);
        foreach (var (name, text) in writer.Files)
            File.WriteAllText(Path.Combine(folder, name), text.ToString());
        var report = string.Join(Environment.NewLine, new[]
        {
            $"scalar-arrays: {scalarArrays}",
            $"bodies checked: {writer.StorageBodiesChecked}",
            $"checker refusals: {writer.StorageRefusals} {string.Join("; ", writer.StorageRefusalLog.Take(20))}",
            $"stack slots placed: {writer.StackSlotsPlaced}",
            $"stack refusals: {string.Join(", ", writer.StackRefusals.Select(r => $"{r.Key} {r.Value}"))}",
            $"twins written: {writer.Borrowed.TwinsWritten} borrowing {writer.Borrowed.ParametersBorrowed} parameters; refused: {writer.Borrowed.Refusals.Count}",
            $"twin calls routed: {writer.TwinCallsRouted}",
            $"degraded bodies: {writer.DegradedBodies.Count}",
        });
        File.WriteAllText(Path.Combine(folder, "census.txt"), report);
        TestContext.Out.WriteLine(report);
    }
}
