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
    /// Codegen gate for DECLARED TYPE-PARAMETER BOUNDS (plato-382, phase C) â€” the phase that carries
    /// `type Tween&lt;T&gt; where T: Interpolatable` into the generated C#. Three things are under
    /// test, all from one in-test fixture rather than the stdlib (no shipping declaration carries a
    /// bound yet):
    ///
    ///   * the STRUCT declaration gets `where T : Interpolatable&lt;T&gt;`;
    ///   * a library function whose own signature variable inherits the bound gets the same clause
    ///     on the METHOD;
    ///   * the bodies that call a bound-supplied member on a bare `T` emit as REAL code, where
    ///     before phase C `TirEmitSource.IsOpenGenericEmittable` refused them as throwing stubs.
    ///
    /// The strong gate is the last block: the emitted C# is compiled in-proc with Roslyn and run,
    /// which is the only thing that proves the `where` clauses are both present AND sufficient.
    /// </summary>
    [TestFixture]
    public static class TypeConstraintCodegenTests
    {
        // A bounded generic type whose member operates ON its parameter (Sample -> Lerp on a bare
        // T), plus a library function over a NON-generic receiver that mentions Tween<$T> in a
        // later parameter â€” that one keeps `$T` as its OWN C# type parameter, so it is where the
        // function-level `where` clause has to appear.
        private const string Source = @"
type Number { }
type Boolean { }

interface Interpolatable
{
    Lerp(a: Self, b: Self, t: Number): Self;
}

type Point2D
    implements Interpolatable
{
    X: Number;
    Y: Number;
}

type Tween<T>
    where T: Interpolatable
{
    From: T;
    To: T;
}

type Timeline
{
    Name: Boolean;
}

// Generic but UNBOUNDED: the control case â€” nothing about its emission may change.
type Crate<T>
{
    Item: T;
}

library Points
{
    // Deliberately trivial: the fixture is about where-clauses and body emission, not arithmetic
    // (Number here is the erased scalar, with no operators declared in this tiny world).
    Lerp(a: Point2D, b: Point2D, t: Number): Point2D => b;
}

library Tweens
{
    // Member of Tween<T>: the residual variable IS the struct's parameter, so the clause the body
    // needs is the one on the struct.
    Sample(x: Tween<$T>, t: Number): $T => x.From.Lerp(x.To, t);

    // Member of Timeline: `$T` stays this function's own type parameter, so the clause must be on
    // the METHOD.
    SampleAt(line: Timeline, x: Tween<$T>, t: Number): $T => x.From.Lerp(x.To, t);
}
";

        // The same world plus a bounded generic SUM. Kept out of `Source` so the product-path
        // assertions above are unaffected by a declaration that CHK306 still rejects.
        private const string SumSource = Source + @"
// A bounded generic sum. CHK306 rejects generic sums today (plato-079 is the lift), but the sum
// writer takes its `where` clauses from the same TypeParameterDef.Constraints the product writer
// does, and nothing about bounds is record-specific â€” this pins that, so lifting CHK306 does not
// have to rediscover whether bounds survive the sum path.
type Blend<T>
    where T: Interpolatable
    = Held(Value: T) | Ramp(From: T, To: T);
";

        private static CSharpWriter Emit(string source = null)
        {
            var dir = Path.Combine(Path.GetTempPath(), "plato382c-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);
            try
            {
                File.WriteAllText(Path.Combine(dir, "corpus.plato"), source ?? Source);
                var comp = CheckerTestSupport.CompileFolder(dir);
                CollectionAssert.IsEmpty(comp.SymbolFactory.Errors.Select(e => e.ToString()).ToArray(),
                    "the fixture must resolve cleanly or every assertion below would be vacuous");
                var w = new CSharpWriter(comp, "unused-bounds-codegen")
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

        // --- (1) the where clause on the emitted struct --------------------------

        [Test]
        public static void BoundedGenericType_EmitsTheWhereClauseOnTheStruct()
        {
            var src = File_(Emit(), "_Tween.g.cs");
            StringAssert.Contains("public readonly partial struct Tween<T>", src);
            StringAssert.Contains("where T : Interpolatable<T>", src);
        }

        [Test]
        public static void AnUnboundedGenericTypeIsUnchanged()
        {
            // The scope line, in the emitter as in the checker: a declaration with no `where`
            // clause emits exactly what it emitted before bounds existed.
            var src = File_(Emit(), "_Crate.g.cs");
            StringAssert.Contains("public readonly partial struct Crate<T>", src);
            StringAssert.DoesNotContain("where", src);
        }

        [Test]
        public static void ABoundedGenericSum_EmitsTheWhereClauseThroughTheSumPath()
        {
            // Groundwork for plato-079 only â€” this does NOT lift CHK306, which still rejects a
            // generic sum in the checker. What it proves is that when CHK306 is lifted the emitter
            // is already correct: the sum struct carries the declared bound, so a case payload of
            // type T can be operated on exactly as a product field can.
            var src = File_(Emit(SumSource), "_Blend.g.cs");
            StringAssert.Contains("public readonly partial struct Blend<T>", src);
            StringAssert.Contains("where T : Interpolatable<T>", src);
        }

        // --- (2) the where clause on an emitted generic function ------------------

        [Test]
        public static void ALibraryFunctionInheritingABound_EmitsTheWhereClauseOnTheMethod()
        {
            // SampleAt's receiver (Timeline) is not generic, so extension style moves it into the
            // library's static class and `$T` stays the FUNCTION's own type parameter â€” the case
            // where the clause has to be on the method rather than on a struct.
            var src = File_(Emit(), "Tweens.g.cs");
            var line = src.Split('\n').FirstOrDefault(l => l.Contains("SampleAt"));
            Assert.IsNotNull(line, "SampleAt must be emitted as a library extension method\n" + src);
            StringAssert.Contains("SampleAt<_T0>", line);
            StringAssert.Contains("where _T0 : Interpolatable<_T0>", line);
        }

        // --- (3) the body is real, not a throwing stub ---------------------------

        [Test]
        public static void ABoundLicensedCallOnABareTypeParameter_EmitsARealBody()
        {
            var w = Emit();
            var tween = File_(w, "_Tween.g.cs");
            var sample = tween.Split('\n').FirstOrDefault(l => l.Contains(" Sample("));
            Assert.IsNotNull(sample, "Tween<T>.Sample must be emitted\n" + tween);
            StringAssert.DoesNotContain("NotImplementedException", sample);
            StringAssert.Contains("Lerp(", sample);

            var sampleAt = File_(w, "Tweens.g.cs").Split('\n').FirstOrDefault(l => l.Contains("SampleAt"));
            Assert.IsNotNull(sampleAt);
            StringAssert.DoesNotContain("NotImplementedException", sampleAt);
            StringAssert.Contains("Lerp(", sampleAt);

            CollectionAssert.IsEmpty(w.DegradedBodies,
                "no body in this fixture may degrade to a throwing stub");
        }

        // --- the strong gate: the emitted C# compiles and runs --------------------

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

        /// <summary>Builds a generated struct from plain BCL values, wrapping each argument in the
        /// scalar WRAPPER its constructor actually declares (Number/Integer/...). Since scalar
        /// erasure was retired 2026-08-01 these constructors take Number, not float, and
        /// Activator.CreateInstance does not apply the implicit conversion.</summary>
        private static object New(System.Type t, params object[] args)
        {
            var ctor = t.GetConstructors().First(c => c.GetParameters().Length == args.Length);
            var ps = ctor.GetParameters();
            var converted = args
                .Select((a, i) => ps[i].ParameterType.IsInstanceOfType(a)
                    ? a
                    : Activator.CreateInstance(ps[i].ParameterType, a))
                .ToArray();
            return ctor.Invoke(converted);
        }
        /// <summary>Invokes a generated method, wrapping plain BCL arguments in the scalar WRAPPER
        /// the parameter declares, and unwrapping a wrapper result back to its payload.</summary>
        private static object Call(System.Reflection.MethodInfo m, object receiver, params object[] args)
        {
            var ps = m.GetParameters();
            var converted = args
                .Select((a, i) => ps[i].ParameterType.IsInstanceOfType(a)
                    ? a
                    : Activator.CreateInstance(ps[i].ParameterType, a))
                .ToArray();
            return Unwrap(m.Invoke(receiver, converted));
        }

        /// <summary>Reads the payload out of a generated scalar wrapper returned via reflection.</summary>
        private static object Unwrap(object v)
        {
            var f = v?.GetType().GetField("Value");
            return f == null ? v : f.GetValue(v);
        }
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
                "BoundsCodegen_" + Guid.NewGuid().ToString("N"),
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
        public static void TheEmittedBoundedTypeCompilesAndRuns()
        {
            var w = Emit();
            var asm = CompileAndLoad(Prelude,
                File_(w, "_Tween.g.cs"),
                File_(w, "_Point2D.g.cs"),
                File_(w, "_Timeline.g.cs"),
                File_(w, "Interfaces.g.cs"),
                File_(w, "Tweens.g.cs"));

            var point = asm.GetType("Ara3D.Geometry.Point2D", true);
            var tweenOpen = asm.GetType("Ara3D.Geometry.Tween`1", true);

            // The C# constraint is REAL and reflectable, not just text in a file: T's constraint is
            // the generated Interpolatable<T> interface, and closing Tween<T> at Point2D is legal
            // only because Point2D implements it.
            var t = tweenOpen.GetGenericArguments()[0];
            CollectionAssert.AreEqual(new[] { "Interpolatable`1" },
                t.GetGenericParameterConstraints().Select(x => x.Name).ToArray());
            var tween = tweenOpen.MakeGenericType(point);

            var from = New(point, 1f, 2f);
            var to = New(point, 3f, 4f);
            var value = New(tween, from, to);

            // Sample's body really runs (the fixture's Lerp returns b), which it could not do while
            // the body was a throwing stub.
            var sampled = Call(tween.GetMethod("Sample"), value, 0.5f);
            Assert.AreEqual(3f, Unwrap(point.GetField("X").GetValue(sampled)));
            Assert.AreEqual(4f, Unwrap(point.GetField("Y").GetValue(sampled)));
        }
    }
}
