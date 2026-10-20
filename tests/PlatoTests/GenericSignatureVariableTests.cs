using System;
using System.IO;
using System.Linq;
using Ara3D.Geometry.Compiler;
using Ara3D.Geometry.Compiler.Checking;
using Ara3D.Geometry.Compiler.Symbols;
using NUnit.Framework;

namespace PlatoTests
{
    /// <summary>
    /// A generic function's own signature variable (`$T`) is a constant inside its body: it is
    /// universally quantified, so nothing in the body may decide what it is. Two solver faults
    /// let a large program decide it anyway (found 2026-10-20 by the STEP mesher library in
    /// bim-open-toolkit, whose `DifferencesTable(ds: Array&lt;LocatedDifference&lt;$T&gt;&gt;, name)`
    /// reported CHK201 with `$T` printed as `$Ret3` once nine more folders were in the program):
    ///
    /// 1. A Phase-1 equality between `$T` and a fresh variable bound `$T := $Ret3`, so every
    ///    type mentioning `$T` zonked to one mentioning a hole, and the diagnostics said `$Ret3`.
    /// 2. An argument type mentioning the rigid `$T` counted as "not ground", so the call that
    ///    fixes a lambda's parameter type (`ds.Map(d => ...)`) waited for the forced phase, where
    ///    the lambda body's member accesses run first in source order. A field name shared with
    ///    an unrelated type (`Row` on `RowDump`) then committed the lambda parameter to that type.
    ///
    /// The fixture is the smallest program with both shapes and no stdlib.
    /// </summary>
    [TestFixture]
    public static class GenericSignatureVariableTests
    {
        private const string Prelude = @"
type Number { }
type Integer { }
type Boolean { }
type Object { }
type String { }
type Function1<T0, TR> { }

// Declared before Bag, so Other.Row is the first candidate of the tied `Row` overloads.
type Other
{
    Row: Integer;
    Tag: String;
}

type Bag<T>
{
    Item: T;
    Row: Integer;
    Slot: Integer;
}

library Ops
{
    Map(xs: Bag<$T>, f: Function1<$T, $U>): Bag<$U>;
    Pack(a: Integer, b: Integer): String;
}
";

        private static Compilation Compile(string source)
        {
            var dir = Path.Combine(Path.GetTempPath(), "plato-sigvar-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);
            try
            {
                File.WriteAllText(Path.Combine(dir, "corpus.plato"), source);
                var c = CheckerTestSupport.CompileFolder(dir);
                CollectionAssert.IsEmpty(c.SymbolFactory.Errors.Select(e => e.ToString()).ToArray(),
                    "the fixture must resolve cleanly or every assertion below would be vacuous");
                return c;
            }
            finally
            {
                try { Directory.Delete(dir, true); } catch { /* best effort */ }
            }
        }

        private static TypeCheckResult CheckOf(Compilation c, string functionName)
            => new TypeChecker(c).CheckAll().Single(r => r.Function?.Name == functionName);

        private static string Show(TypeCheckResult r)
            => string.Join("\n", r.Diagnostics.Select(d => d.ToString()));

        [Test]
        public static void SignatureVariableIsNeverReplacedByAFreshHole()
        {
            // `name(ds.Item)` equates `Function1<$T, String>` with `Function1<$Ret, $Apply>`. The
            // flexible `$Ret` must bind toward the signature's `$T`, never the other way round.
            var c = Compile(Prelude + @"
library Names
{
    NameOf(ds: Bag<$T>, name: Function1<$T, String>): String
        => name(ds.Item);
}
");
            var r = CheckOf(c, "NameOf");
            Assert.IsTrue(r.Succeeded, Show(r));
            var t = TypeExpression.CreateTypeVar(null, "$T");
            Assert.AreEqual("$T", r.Solver.Zonk(t).ToString(),
                "the body's view of the signature variable must stay the signature variable");
        }

        [Test]
        public static void LambdaParameterComesFromTheHigherOrderCallNotFromASharedFieldName()
        {
            // `d.Row` has two candidates with one return type (Bag.Row and Other.Row). If the solver
            // resolves it before `Map` has fixed `d`, it commits the first candidate and types `d`
            // as `Other`, after which `d.Slot` has no overload (CHK201). `Map`'s first argument
            // mentions only the rigid `$T`, so it is ground and must resolve in the fixpoint.
            var c = Compile(Prelude + @"
library Rows
{
    RowsOf(ds: Bag<Bag<$T>>): Bag<String>
        => ds.Map(d => d.Row.Pack(d.Slot));
}
");
            var r = CheckOf(c, "RowsOf");
            Assert.IsTrue(r.Succeeded, Show(r));
        }
    }
}
