using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Ara3D.Geometry.CSharpWriter;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;

namespace PlatoTests
{
    /// <summary>
    /// Codegen gate for WHERE-CLAUSE BOUNDS ON A LIBRARY FUNCTION (plato-393). The checking half is
    /// <see cref="FunctionConstraintTests"/>; this is the other end of the same claim â€” that a
    /// declared function bound reaches the generated C# as a real constraint on a real body.
    ///
    /// The fixture is the shape the issue exists for and the one the stdlib's de Casteljau now uses:
    /// an operation on a BARE element of an UNBOUNDED container, where nothing in the signature
    /// could ever have supplied the bound by inheritance. Without the clause,
    /// <c>TirEmitSource.IsOpenGenericEmittable</c> refuses such a body and emits a throwing stub â€”
    /// correctly, since an unconstrained C# type parameter cannot reach <c>Lerp</c>.
    ///
    /// The strong gate is the last test: the emitted C# is compiled in-proc with Roslyn and run, the
    /// only thing that proves the clause is both PRESENT and SUFFICIENT.
    /// </summary>
    [TestFixture]
    public static class FunctionConstraintCodegenTests
    {
        private const string Source = @"
type Number { }
type Boolean { }

concept Interpolatable
{
    Lerp(a: Self, b: Self, t: Number): Self;
}

type Point2D
    implements Interpolatable
{
    X: Number;
    Y: Number;
}

// UNBOUNDED, and it must stay that way: it stands in for the primitive Array<T>, which is
// unbounded for every element type in the language.
type Bag<T>
{
    Item: T;
}

type Timeline
{
    Name: Boolean;
}

library Points
{
    // Deliberately trivial: the fixture is about where-clauses and body emission, not arithmetic.
    Lerp(a: Point2D, b: Point2D, t: Number): Point2D => b;
}

library Mixers
{
    // The payoff shape. The receiver (Timeline) is not generic, so `$T` stays this FUNCTION's own
    // C# type parameter and the clause has to be emitted on the METHOD; the container is
    // unbounded, so the clause is the only thing that can license `Lerp` on a bare element.
    MixAt(line: Timeline, xs: Bag<$T>, t: Number): $T where $T: Interpolatable
        => xs.Item.Lerp(xs.Item, t);

    // The control case: same body, no clause. Nothing about its emission may change, and it stays
    // a stub â€” an unbounded C# type parameter genuinely cannot reach Lerp.
    MixLoosely(line: Timeline, xs: Bag<$T>, t: Number): $T
        => xs.Item.Lerp(xs.Item, t);
}
";

        private static CSharpWriter Emit(string source = null)
        {
            var dir = Path.Combine(Path.GetTempPath(), "plato393c-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);
            try
            {
                File.WriteAllText(Path.Combine(dir, "corpus.plato"), source ?? Source);
                var comp = CheckerTestSupport.CompileFolder(dir);
                CollectionAssert.IsEmpty(comp.SymbolFactory.Errors.Select(e => e.ToString()).ToArray(),
                    "the fixture must resolve cleanly or every assertion below would be vacuous");
                var w = new CSharpWriter(comp, "unused-function-bounds-codegen")
                {
                    ExtensionStyle = true,
                };
                w.WriteAll("float");
                return w;
            }
            finally
            {
                try { Directory.Delete(dir, true); } catch { /* best effort */ }
            }
        }

        private static string File_(CSharpWriter w, string name)
        {
            Assert.IsTrue(w.Files.ContainsKey(name),
                $"expected an emitted file '{name}'; got: {string.Join(", ", w.Files.Keys)}");
            return w.Files[name].ToString();
        }

        private static string LineContaining(string src, string needle)
        {
            var line = src.Split('\n').FirstOrDefault(l => l.Contains(needle));
            Assert.IsNotNull(line, $"expected an emitted line containing '{needle}'\n" + src);
            return line;
        }

        // --- the where clause on the emitted method -------------------------------

        [Test]
        public static void ADeclaredFunctionBound_EmitsTheWhereClauseOnTheMethod()
        {
            var line = LineContaining(File_(Emit(), "Mixers.g.cs"), "MixAt");
            StringAssert.Contains("MixAt<_T0>", line);
            StringAssert.Contains("where _T0 : Interpolatable<_T0>", line);
        }

        [Test]
        public static void AFunctionWithoutAClause_IsUnchanged()
        {
            // The scope line in the emitter: a function that declares no bound emits exactly what it
            // emitted before function bounds existed.
            var line = LineContaining(File_(Emit(), "Mixers.g.cs"), "MixLoosely");
            StringAssert.Contains("MixLoosely<_T0>", line);
            StringAssert.DoesNotContain("where", line);
        }

        // --- the body is real, not a throwing stub --------------------------------

        [Test]
        public static void ABoundLicensedCallOnABareSignatureVariable_EmitsARealBody()
        {
            var w = Emit();
            var mixAt = LineContaining(File_(w, "Mixers.g.cs"), "MixAt");
            StringAssert.DoesNotContain("NotImplementedException", mixAt);
            StringAssert.Contains("Lerp(", mixAt);

            // ... and the unbounded twin still degrades, which is what makes the line above a
            // consequence of the clause rather than of anything else in the fixture.
            var loose = LineContaining(File_(w, "Mixers.g.cs"), "MixLoosely");
            StringAssert.Contains("NotImplementedException", loose);
        }

        // --- the strong gate: the emitted C# compiles and runs ---------------------

        // Enough runtime for the emitted files to stand alone: the namespace the header `using`
        // names, and the hash helper the struct scaffolding calls.
        private const string Prelude = @"
namespace Ara3D.Collections { }
namespace Ara3D.Geometry
{
    public static class Intrinsics
    {
        public static int CombineHashCodes(params object[] xs)
        {
            var h = 17;
            foreach (var x in xs) h = h * 31 + (x?.GetHashCode() ?? 0);
            return h;
        }
    }
}";

        private static Assembly CompileAndLoad(params string[] sources)
        {
            var trees = sources.Select(s => CSharpSyntaxTree.ParseText(s)).ToArray();
            var refs = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES"))
                .Split(Path.PathSeparator)
                .Where(p => !string.IsNullOrEmpty(p) && System.IO.File.Exists(p))
                // The stale Ara3D.Geometry PACKAGE also declares Ara3D.Geometry.Number/Boolean/...,
                // and so does this test assembly (it compiles src/Plato.Intrinsics in as a shared
                // project). Referencing both is CS0433 for every wrapper scalar the emitted code
                // names. Drop the package: the current runtime is the one the generated projects
                // actually compile against. (Invisible before 2026-08-01, when erasure meant the
                // emitted code said float/bool and never named a wrapper.)
                .Where(p => !string.Equals(System.IO.Path.GetFileName(p), "Ara3D.Geometry.dll", System.StringComparison.OrdinalIgnoreCase))
                .Select(p => (MetadataReference)MetadataReference.CreateFromFile(p))
                .ToList();
            var comp = CSharpCompilation.Create(
                "FunctionBoundsCodegen_" + Guid.NewGuid().ToString("N"),
                trees, refs,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
            using var ms = new MemoryStream();
            var result = comp.Emit(ms);
            if (!result.Success)
                Assert.Fail("Emitted C# did not compile:\n" + string.Join("\n", result.Diagnostics
                    .Where(d => d.Severity == DiagnosticSeverity.Error)
                    .Select(d => d.ToString())));
            ms.Position = 0;
            return Assembly.Load(ms.ToArray());
        }

        [Test]
        public static void TheEmittedBoundedFunctionCompilesAndRuns()
        {
            var w = Emit();
            var asm = CompileAndLoad(Prelude,
                File_(w, "_Point2D.g.cs"),
                File_(w, "_Bag.g.cs"),
                File_(w, "_Timeline.g.cs"),
                File_(w, "Interfaces.g.cs"),
                File_(w, "Mixers.g.cs"));

            var point = asm.GetType("Ara3D.Geometry.Point2D", true);
            var bagOpen = asm.GetType("Ara3D.Geometry.Bag`1", true);
            var mixers = asm.GetType("Ara3D.Geometry.Mixers", true);

            // The C# constraint is REAL and reflectable, not just text in a file: the METHOD's own
            // type parameter carries the generated Interpolatable<T> interface.
            var mixAt = mixers.GetMethods().Single(m => m.Name == "MixAt");
            CollectionAssert.AreEqual(new[] { "Interpolatable`1" },
                mixAt.GetGenericArguments()[0].GetGenericParameterConstraints()
                    .Select(x => x.Name).ToArray());

            var bag = bagOpen.MakeGenericType(point);
            var value = Activator.CreateInstance(bag, Activator.CreateInstance(point, 3f, 4f));

            // The body really runs (the fixture's Lerp returns b), which it could not do while the
            // body was a throwing stub.
            var mixed = mixAt.MakeGenericMethod(point)
                .Invoke(null, new[] { Activator.CreateInstance(asm.GetType("Ara3D.Geometry.Timeline", true), true), value, 0.5f });
            Assert.AreEqual(3f, point.GetField("X").GetValue(mixed));
            Assert.AreEqual(4f, point.GetField("Y").GetValue(mixed));
        }
    }
}
