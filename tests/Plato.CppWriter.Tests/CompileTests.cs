using Ara3D.Geometry.Compiler;
using Ara3D.Geometry.CppWriter;
using Ara3D.Utils;
using NUnit.Framework;

namespace PlatoCppWriterTests;

/// <summary>
/// The V1 gate for the C++ / CUDA backend: generate from real Plato source, then prove the
/// generated code COMPILES. Nothing here checks runtime values yet — see the README.
/// </summary>
[TestFixture]
public class CompileTests
{
    public enum Library { Demo, StandardLibrary }

    private static readonly Dictionary<Library, Compilation> Compilations = new();

    private static Compilation Compile(Library library)
    {
        lock (Compilations)
        {
            if (!Compilations.TryGetValue(library, out var c))
            {
                c = library == Library.Demo
                    ? PlatoSource.CompileDemoLibrary()
                    : PlatoSource.CompileStandardLibrary();
                Compilations[library] = c;
            }
            return c;
        }
    }

    private static string OutputDirectory(string name)
    {
        var dir = Path.Combine(Path.GetTempPath(), "plato-cpp-tests", name);
        if (Directory.Exists(dir))
            Directory.Delete(dir, true);
        Directory.CreateDirectory(dir);
        return dir;
    }

    /// <summary>Generates one dialect into its own directory and returns the writer and the file written.</summary>
    private static (CppWriter Writer, string File) Generate(Library library, CppDialect dialect, bool inlineCalls = true)
    {
        var dir = OutputDirectory($"{library}-{dialect}");
        var writer = Compile(library).ToCpp(new DirectoryPath(dir), dialect, inlineCalls);
        var file = "";
        foreach (var kv in writer.Files)
        {
            file = Path.Combine(dir, kv.Key);
            File.WriteAllText(file, kv.Value.ToString());
        }
        Assert.That(writer.FunctionsEmitted, Is.GreaterThan(0), "nothing was emitted");
        TestContext.Out.WriteLine(
            $"{library} / {dialect.DisplayName()}: {writer.FunctionsEmitted} functions emitted, {writer.Skipped.Count} skipped (inline={inlineCalls})");
        return (writer, file);
    }

    private static void AssertCompiles(int exitCode, string output, string what)
    {
        if (exitCode == 0)
            return;
        var errors = output.Split('\n')
            .Where(l => l.Contains(": error") || l.Contains("error:"))
            .Take(15);
        Assert.Fail($"{what} did not compile (exit {exitCode}):\n{string.Join("\n", errors)}");
    }

    [TestCase(Library.Demo)]
    [TestCase(Library.StandardLibrary)]
    public void Generated_Cpp_Compiles(Library library)
    {
        var vcvars = Toolchain.FindVcVars();
        if (vcvars == null)
            Assert.Ignore("No MSVC installation found (vcvars64.bat)");

        var (_, file) = Generate(library, CppDialect.Cpp);
        // /TP: compile the .hpp as a C++ translation unit in its own right.
        var (exit, output) = Toolchain.CompileWithMsvc(vcvars!, Path.GetDirectoryName(file)!, file, "/TP");
        AssertCompiles(exit, output, $"{library} C++");
    }

    [TestCase(Library.Demo)]
    [TestCase(Library.StandardLibrary)]
    public void Generated_Cuda_Compiles(Library library)
    {
        var nvcc = Toolchain.FindNvcc();
        var vcvars = Toolchain.FindVcVars();
        if (nvcc == null && vcvars == null)
            Assert.Ignore("Neither the CUDA Toolkit nor MSVC is installed");

        var (_, file) = Generate(library, CppDialect.Cuda);
        var dir = Path.GetDirectoryName(file)!;

        if (nvcc != null)
        {
            TestContext.Out.WriteLine($"CUDA gate: nvcc ({nvcc})");
            var (exit, output) = Toolchain.CompileWithNvcc(nvcc, dir, file, vcvars);
            AssertCompiles(exit, output, $"{library} CUDA (nvcc)");
            return;
        }

        // No toolkit: compile the .cu as host C++ against the stand-in CUDA header.
        TestContext.Out.WriteLine("CUDA gate: MSVC + shim header (no CUDA Toolkit installed)");
        var shim = Path.Combine(dir, "shim");
        Directory.CreateDirectory(shim);
        File.WriteAllText(Path.Combine(shim, "cuda_runtime.h"), Toolchain.CudaShimHeader);
        var (exitCode, msvcOutput) = Toolchain.CompileWithMsvc(vcvars!, dir, file, $"/TP /I \"{shim}\"");
        AssertCompiles(exitCode, msvcOutput, $"{library} CUDA (MSVC shim)");
    }

    /// <summary>
    /// The whole design rests on the two dialects sharing one emitter: everything after the
    /// preamble must be identical, so a C++ compile really does gate the CUDA output too.
    /// </summary>
    [TestCase(Library.Demo)]
    [TestCase(Library.StandardLibrary)]
    public void Dialects_Differ_Only_In_The_Preamble(Library library)
    {
        var cpp = Generate(library, CppDialect.Cpp).Writer.Files.Values.Single().ToString();
        var cuda = Generate(library, CppDialect.Cuda).Writer.Files.Values.Single().ToString();
        Assert.That(CodeSection(cuda), Is.EqualTo(CodeSection(cpp)));
    }

    /// <summary>
    /// The generated code proper: everything after the dialect's preamble and before the
    /// trailing comment block, which names the dialect in its skip reasons.
    /// </summary>
    private static string CodeSection(string file)
    {
        var start = file.IndexOf("// ---- Structs ----", StringComparison.Ordinal);
        var end = file.IndexOf("// ---- Skipped", StringComparison.Ordinal);
        Assert.That(start, Is.GreaterThan(0));
        return end > start ? file[start..end] : file[start..];
    }

    /// <summary>
    /// C++ needs a declaration before use, so the writer emits every prototype up front. One
    /// prototype per emitted function, and a definition for each: a mismatch means a function
    /// was pruned from one list but not the other, which the compiler would only catch when
    /// something happened to call it.
    /// </summary>
    [TestCase(Library.Demo)]
    [TestCase(Library.StandardLibrary)]
    public void Every_Emitted_Function_Has_A_Prototype(Library library)
    {
        var (writer, _) = Generate(library, CppDialect.Cpp);
        var text = writer.Files.Values.Single().ToString();

        var start = text.IndexOf("// ---- Function prototypes ----", StringComparison.Ordinal);
        var end = text.IndexOf("// ---- Function definitions ----", StringComparison.Ordinal);
        Assert.That(start, Is.GreaterThan(0));
        Assert.That(end, Is.GreaterThan(start));

        var prototypes = text[start..end]
            .Split('\n')
            .Count(l => l.TrimStart().StartsWith("PLATO_FN") && l.TrimEnd().EndsWith(";"));
        Assert.That(prototypes, Is.EqualTo(writer.FunctionsEmitted));
    }
}
