// Checks emitted member calls with lambda-valued receivers.
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Ara3D.Geometry.CSharpWriter;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;
using PlatoCompilation = Ara3D.Geometry.Compiler.Compilation;

namespace PlatoTests
{
    /// <summary>
    /// TKT-353 (bim-open-toolkit): a call whose first argument is a lambda is written in method
    /// form, `lambda.Name(rest)`, and C# rejects a member access on a lambda (CS0023: a lambda has
    /// no type until it is converted to a delegate). The STEP mesher's B-spline inverse hit it with
    /// `MinimizeOverDomain(t => curve.Eval(g, t), ...)`. The fixture is generated, compiled with
    /// Roslyn and run.
    /// </summary>
    [TestFixture]
    public static class LambdaReceiverEmissionTests
    {
        /// <summary>The library class, and the extension class that holds ApplyTwice: a function
        /// whose first parameter is a function type is an extension method on System.Func.</summary>
        private static string[] EmitFiles()
        {
            var ast = CheckerTestSupport.ParseFile(Path.Combine(CheckerTestSupport.FindFolder("plato-test-codegen"), "lambda-receiver.plato"));
            var comp = new PlatoCompilation(Ara3D.Logging.Logger.Null, new[] { ast });
            var w = new CSharpWriter(comp, "unused-lambda-receiver") { ExtensionStyle = true };
            w.WriteAll();
            return new[] { w.Files["LambdaReceivers.g.cs"].ToString(), w.Files["Extensions.g.cs"].ToString() };
        }

        [Test]
        public static void FunctionFirstFunctionIsAnExtensionOnFunc()
            => Assert.That(EmitFiles()[1], Does.Contain("ApplyTwice(this System.Func<Integer, Integer> f"),
                "a function whose first parameter is Function1 must be emitted, as an extension method on System.Func");

        [Test]
        public static void LambdaReceiverCompilesAndRuns()
        {
            var tpa = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES"))
                .Split(Path.PathSeparator)
                .Where(p => !string.IsNullOrEmpty(p) && File.Exists(p))
                // The stale Ara3D.Geometry package declares the wrapper scalars too (CS0433).
                .Where(p => !string.Equals(Path.GetFileName(p), "Ara3D.Geometry.dll", StringComparison.OrdinalIgnoreCase))
                .Select(p => (MetadataReference)MetadataReference.CreateFromFile(p));
            var files = EmitFiles();
            var source = string.Join(System.Environment.NewLine, files);
            var comp = CSharpCompilation.Create("LambdaReceiver_" + Guid.NewGuid().ToString("N"),
                files.Prepend("namespace Ara3D.Collections { }").Select(f => CSharpSyntaxTree.ParseText(f)), tpa,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
            using var ms = new MemoryStream();
            var result = comp.Emit(ms);
            Assert.That(result.Success, () => string.Join("\n", result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error))
                + "\n--- source ---\n" + source);

            var secondTwice = Assembly.Load(ms.ToArray()).GetType("Ara3D.Geometry.LambdaReceivers", true).GetMethod("SecondTwice");
            var got = (Ara3D.Geometry.Integer)secondTwice.Invoke(null, new object[] { (Ara3D.Geometry.Integer)1, (Ara3D.Geometry.Integer)7 });
            Assert.That((int)got, Is.EqualTo(7), "the lambda, applied twice through the delegate it was converted to, returns its captured k");
        }
    }
}
