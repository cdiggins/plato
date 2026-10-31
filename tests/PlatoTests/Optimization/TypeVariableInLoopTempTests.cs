// TKT-1070: a lowered loop must never name a solver type variable in the C# it declares.
using System;
using System.IO;
using System.Text.RegularExpressions;
using Ara3D.Geometry.CSharpWriter;
using NUnit.Framework;

namespace PlatoTests
{
    /// <summary>
    /// TKT-1070: with <c>--loops</c> (and <c>--fuse</c>) a generic function whose lambda builds a
    /// generic record, <c>rows.Map(r => Located(t, r.Row))</c> returning
    /// <c>Array&lt;Located&lt;$T&gt;&gt;</c>, lowered to a loop that declared its output as
    /// <c>new Located&lt;$T&gt;[n]</c>. The lowerer's <c>IsRenderableConcrete</c> checked only the
    /// element type's own name, so a type variable nested in a type argument got through, and the
    /// emitted C# did not compile (CS1056 at the <c>$</c>) in the STEP mesher's layer 09
    /// (<c>InTable</c> over <c>LocatedDifference&lt;T&gt;</c>).
    /// </summary>
    [TestFixture]
    public static class TypeVariableInLoopTempTests
    {
        private const string Source = @"
type Number { }
type Integer { }
type Boolean { }
type Object { }

type Function0<TR> { }
type Function1<T0, TR> { }
type Function2<T0, T1, TR> { }

type Array<T> { }

library Intrinsics
{
    Count(xs: Array<$T>): Integer;
    At(xs: Array<$T>, n: Integer): $T;
    MapRange(n: Integer, f: Function1<Integer, $T>): Array<$T>;
    Reduce(xs: Array<$T>, acc: $U, f: Function2<$U, $T, $U>): $U;
    Map(xs: Array<$T>, f: Function1<$T, $U>): Array<$U>;
}

type RowDiff
{
    Row: Integer;
    Slot: Integer;
}

type Located<T>
{
    Table: T;
    Row: Integer;
    Slot: Integer;
}

library Kernels
{
    InTable(rows: Array<RowDiff>, t: $T): Array<Located<$T>>
        => rows.Map(r => Located(t, r.Row, r.Slot));
}
";

        private static string Emit(bool fuse)
        {
            var dir = Path.Combine(Path.GetTempPath(), "plato-tkt1070-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);
            try
            {
                File.WriteAllText(Path.Combine(dir, "corpus.plato"), Source);
                var w = new CSharpWriter(CheckerTestSupport.CompileFolder(dir), "unused-tkt1070")
                    { LowerLoops = true, FuseCombinators = fuse };
                w.WriteAll();
                Assert.IsTrue(w.Files.ContainsKey("Extensions.g.cs"), "no generated Extensions.g.cs");
                var text = w.Files["Extensions.g.cs"].ToString();
                TestContext.WriteLine(text);
                return text;
            }
            finally
            {
                try { Directory.Delete(dir, true); } catch { /* best effort */ }
            }
        }

        [TestCase(false)]
        [TestCase(true)]
        public static void LoweredLoopNeverNamesATypeVariable(bool fuse)
        {
            var text = Emit(fuse);
            Assert.That(text, Does.Not.Contain("$"), "the emitted C# names a solver type variable");
            Assert.That(Regex.IsMatch(text, @"InTable<_T0>"), "InTable lost its generic parameter");
        }
    }
}
