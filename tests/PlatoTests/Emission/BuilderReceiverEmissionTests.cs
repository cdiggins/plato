// Checks emitted calls on affine collection builders.
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Ara3D.Geometry;
using Ara3D.Geometry.CSharpWriter;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;
using PlatoCompilation = Ara3D.Geometry.Compiler.Compilation;

namespace PlatoTests
{
    /// <summary>
    /// TKT-347 (bim-open-toolkit) and compiler-416: a library function whose first parameter is a
    /// builder, List&lt;T&gt; or Buffer&lt;T&gt;, was never written: functions go to the plan of
    /// their first parameter's type, and a unique type has none. The fixture's four functions,
    /// concrete and generic, are generated, compiled with Roslyn against the runtime builders, and
    /// run.
    /// </summary>
    [TestFixture]
    public static class BuilderReceiverEmissionTests
    {
        private static readonly Lazy<string> _extensions = new(() =>
        {
            var ast = CheckerTestSupport.ParseFile(Path.Combine(CheckerTestSupport.FindFolder("plato-test-codegen"), "builder-receivers.plato"));
            var comp = new PlatoCompilation(Ara3D.Logging.Logger.Null, new[] { ast });
            var w = new CSharpWriter(comp, "unused-builder-receivers") { ExtensionStyle = true };
            w.WriteAll();
            return string.Join("\n", w.Files.Values.Select(f => f.ToString()));
        });

        private static readonly Lazy<Assembly> _assembly = new(() =>
        {
            var tpa = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES"))
                .Split(Path.PathSeparator)
                .Where(p => !string.IsNullOrEmpty(p) && File.Exists(p))
                // The stale Ara3D.Geometry package declares the wrapper scalars too (CS0433).
                .Where(p => !string.Equals(Path.GetFileName(p), "Ara3D.Geometry.dll", StringComparison.OrdinalIgnoreCase))
                .Select(p => (MetadataReference)MetadataReference.CreateFromFile(p));
            var source = BuilderMethodsOnly(_extensions.Value);
            var comp = CSharpCompilation.Create("BuilderReceivers_" + Guid.NewGuid().ToString("N"),
                new[] { CSharpSyntaxTree.ParseText(source) }, tpa,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
            using var ms = new MemoryStream();
            var result = comp.Emit(ms);
            Assert.That(result.Success, () => string.Join("\n", result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error))
                + "\n--- source ---\n" + source);
            return Assembly.Load(ms.ToArray());
        });

        /// <summary>The generated extension methods on PlatoList and PlatoBuffer, in one class of
        /// their own: the rest of the fixture's output redeclares the runtime's scalars.</summary>
        private static string BuilderMethodsOnly(string generated)
        {
            var lines = generated.Replace("\r\n", "\n").Split('\n')
                .Where(l => l.Contains("this PlatoList<") || l.Contains("this PlatoBuffer<"));
            return "using Ara3D.Geometry;\nusing System.Runtime.CompilerServices;\nusing static System.Runtime.CompilerServices.MethodImplOptions;\n"
                   + "namespace BuilderReceiverFixture {\npublic static class Generated {\n"
                   + string.Join("\n", lines) + "\n}\n}\n";
        }

        private static MethodInfo Method(string name)
            => _assembly.Value.GetType("BuilderReceiverFixture.Generated", true).GetMethod(name)
               ?? throw new MissingMethodException("BuilderReceiverFixture.Generated", name);

        [TestCase("AddTwice", "this PlatoList<Integer>")]
        [TestCase("AddOnce", "this PlatoList<")]
        [TestCase("SetBoth", "this PlatoBuffer<Integer>")]
        [TestCase("SetOne", "this PlatoBuffer<")]
        public static void BuilderFirstFunctionIsEmitted(string name, string receiver)
            => Assert.That(_extensions.Value.Replace("\r\n", "\n").Split('\n').Any(l => l.Contains($" {name}") && l.Contains(receiver)),
                $"{name} must be emitted as an extension method on its builder ({receiver} ...)");

        [Test]
        public static void ListFirstFunctionCompilesAndRuns()
        {
            var list = new PlatoList<Integer>();
            var result = (PlatoList<Integer>)Method("AddTwice").Invoke(null, new object[] { list, (Integer)7 });
            Assert.That(result.Freeze().Select(i => (int)i), Is.EqualTo(new[] { 7, 7 }));
        }

        [Test]
        public static void BufferFirstFunctionCompilesAndRuns()
        {
            var buffer = new PlatoBuffer<Integer>(3);
            var result = (PlatoBuffer<Integer>)Method("SetBoth").Invoke(null, new object[] { buffer, (Integer)0, (Integer)2, (Integer)5 });
            Assert.That(result.Freeze().Select(i => (int)i), Is.EqualTo(new[] { 5, 0, 5 }));
        }
    }
}
