// Compiles a fixture library against the forward foundation and loads its emitted C#.
using System.Reflection;
using Ara3D.Geometry.CSharpWriter;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Framework;

namespace PlatoTests.Emission;

/// <summary>The C# a fixture library emits, compiled with Roslyn against the runtime embedded in the
/// test assembly. Only the fixture's own methods and the records it declares are kept: the
/// foundation supplies types for the checker, and the test assembly supplies them at run time.</summary>
internal sealed class EmittedLibrary
{
    public string Text { get; }
    public Assembly Assembly { get; }

    private EmittedLibrary(string text, Assembly assembly) => (Text, Assembly) = (text, assembly);

    /// <summary>Emits the fixture (under Emission/<paramref name="fixtureFolder"/>) in extension style and
    /// keeps the methods of <paramref name="methodFiles"/> whose names pass <paramref name="keepMethod"/>,
    /// plus the generated <paramref name="recordFiles"/> whole. A function lands in the library's own file
    /// or, when its receiver is an array, in Extensions.g.cs.</summary>
    public static EmittedLibrary Build(string fixtureFolder, string fixtureFile, Func<string, bool> keepMethod,
        string[] methodFiles, params string[] recordFiles)
        => Build(fixtureFolder, fixtureFile, keepMethod, methodFiles, null, null, recordFiles);

    /// <summary>As above, with <paramref name="configure"/> applied to the writer before it writes
    /// (a switch under test) and <paramref name="extraSource"/>, if any, compiled in with the output (the
    /// operators of a scalar that the test assembly does not carry).</summary>
    public static EmittedLibrary Build(string fixtureFolder, string fixtureFile, Func<string, bool> keepMethod,
        string[] methodFiles, Action<CSharpWriter> configure, string extraSource, params string[] recordFiles)
    {
        var foundation = Path.Combine(CheckerTestSupport.FindForwardStdLib(), "foundation");
        var fixture = Path.Combine(CheckerTestSupport.FindFolder("PlatoTests"), "Emission", fixtureFolder, fixtureFile);
        var asts = Directory.GetFiles(foundation, "*.plato").Select(CheckerTestSupport.ParseFile)
            .Append(CheckerTestSupport.ParseFile(fixture));
        var compilation = new Ara3D.Geometry.Compiler.Compilation(Ara3D.Logging.Logger.Null, asts.ToList());
        var writer = new CSharpWriter(compilation, "unused-emitted-library") { ExtensionStyle = true };
        configure?.Invoke(writer);
        writer.WriteAll();
        Assert.That(writer.Files.Keys, Is.SupersetOf(methodFiles.Concat(recordFiles)), "emitted files");
        var sources = recordFiles.Select(f => writer.Files[f].ToString())
            .Concat(methodFiles.Select(f => KeepMethods(writer.Files[f].ToString(), keepMethod)))
            .Prepend("global using System; global using System.Collections.Generic;")
            .Concat(extraSource == null ? Array.Empty<string>() : new[] { extraSource })
            .ToArray();
        var assembly = CSharpTestCompilation.CompileAndLoad(sources, LanguageVersion.CSharp12, OptimizationLevel.Release);
        return new EmittedLibrary(string.Join(Environment.NewLine, sources), assembly);
    }

    public MethodInfo Method(string name)
        => Assembly.GetTypes().SelectMany(t => t.GetMethods()).Single(m => m.Name == name);

    /// <summary>The text of one emitted method, for tests that assert on its shape.</summary>
    public string Body(string name)
        => CSharpSyntaxTree.ParseText(Text).GetRoot().DescendantNodes()
            .OfType<MethodDeclarationSyntax>().Single(m => m.Identifier.Text == name).ToString();

    private static string KeepMethods(string source, Func<string, bool> keep)
    {
        var root = CSharpSyntaxTree.ParseText(source).GetRoot();
        return root.RemoveNodes(root.DescendantNodes().OfType<MethodDeclarationSyntax>()
            .Where(m => !keep(m.Identifier.Text)), SyntaxRemoveOptions.KeepNoTrivia)!.ToFullString();
    }
}
