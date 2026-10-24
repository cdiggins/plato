// Checks C# emission of conditional expressions and their conditions.
using System;
using System.IO;
using System.Linq;
using Ara3D.Geometry.CSharpWriter;
using NUnit.Framework;
using PlatoCompilation = Ara3D.Geometry.Compiler.Compilation;

namespace PlatoTests
{
    /// <summary>
    /// plato-454: a conditional used as the condition of another conditional must be emitted with
    /// its parentheses. `?:` is right-associative in C#, so dropping them still compiles but means
    /// `a ? b : (false ? x : y)`. The fixture is compiled with Roslyn and run over its whole truth
    /// table against the meaning written in Plato.
    /// </summary>
    [TestFixture]
    public static class ConditionalCodegenTests
    {
        private static string Fixture()
            => Path.Combine(CheckerTestSupport.FindFolder("plato-test-codegen"), "conditional-condition.plato");

        private static string EmitLibrary()
        {
            var ast = CheckerTestSupport.ParseFile(Fixture());
            var comp = new PlatoCompilation(Ara3D.Logging.Logger.Null, new[] { ast });
            var w = new CSharpWriter(comp, "unused-conditional-codegen") { ExtensionStyle = true };
            w.WriteAll();
            return w.Files["Conditionals.g.cs"].ToString();
        }

        [Test]
        public static void ConditionAsConditionKeepsItsParentheses()
            => Assert.That(EmitLibrary(), Does.Contain("(a ? b : ((Boolean)false)) ? x : y"));

        [Test]
        public static void ConditionAsConditionRunsWithItsPlatoMeaning()
        {
            var assembly = CSharpTestCompilation.CompileAndLoad("namespace Ara3D.Collections { }", EmitLibrary());
            var pick = assembly.GetType("Ara3D.Geometry.Conditionals", true).GetMethod("Pick");
            object B(bool v) => (Ara3D.Geometry.Boolean)v;
            foreach (var bits in Enumerable.Range(0, 16))
            {
                bool a = (bits & 1) != 0, b = (bits & 2) != 0, x = (bits & 4) != 0, y = (bits & 8) != 0;
                var got = (bool)(Ara3D.Geometry.Boolean)pick.Invoke(null, new[] { B(a), B(b), B(x), B(y) });
                Assert.That(got, Is.EqualTo((a ? b : false) ? x : y), $"a={a} b={b} x={x} y={y}");
            }
        }
    }
}
