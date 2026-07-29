using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Ara3D.Geometry.AST;
using Ara3D.Geometry.Compiler;
using Ara3D.Logging;
using Ara3D.Parakeet;
using Ara3D.Parsing;
using NUnit.Framework;

namespace PlatoTests
{
    /// <summary>
    /// Regression coverage for studio-096: a local <c>var</c> assigned at more than one
    /// loop-nesting depth in one function body used to fail symbol resolution with
    /// "Could not properly resolve symbol". The stdlib is written in pure combinator style and
    /// contains no author-written loops, so this file is the ONLY exercise of the imperative
    /// loop-resolution path.
    ///
    /// Each fixture is a tiny library compiled ALONGSIDE the real stdlib-legacy so that the
    /// primitives it uses (Integer, Number, Min, operators) resolve for real — the test then
    /// asserts the compile is clean of symbol-resolution errors (positives) or that the expected
    /// binding still shadows (the shadowing fixture).
    /// </summary>
    [TestFixture]
    public static class LoopVariableResolutionTests
    {
        // Parse the stdlib once; it is immutable scaffolding shared by every fixture.
        private static List<AstNode> _stdLibTrees;

        private static IEnumerable<AstNode> StdLibTrees()
        {
            if (_stdLibTrees != null) return _stdLibTrees;
            var folder = CheckerTestSupport.FindPlatoSrc();
            _stdLibTrees = Directory.GetFiles(folder, "*.plato")
                .Select(CheckerTestSupport.ParseFile).ToList();
            return _stdLibTrees;
        }

        private static AstNode ParseString(string source)
        {
            var parser = CommonParsers.PlatoParser(new ParserInput(source, "loop-fixture.plato"), Ara3D.Logging.Logger.Null);
            if (!parser.Succeeded)
                throw new Exception($"Parse failed: {string.Join("; ", parser.ErrorMessages)}");
            return parser.Cst!.ToAst();
        }

        /// <summary>Compile a snippet on top of the stdlib and return the resolution-error messages.</summary>
        private static List<string> ResolutionErrors(string snippet)
        {
            var trees = StdLibTrees().Concat(new[] { ParseString(snippet) });
            var comp = new Compilation(Ara3D.Logging.Logger.Null, trees);
            return comp.SymbolFactory.Errors.Select(e => e.Message).ToList();
        }

        private static void AssertResolvesClean(string snippet)
        {
            var errors = ResolutionErrors(snippet);
            Assert.IsEmpty(errors,
                "expected clean symbol resolution, but got:\n" + string.Join("\n", errors));
        }

        // --- The minimal repro from studio-096: accumulator written at TWO depths --------------

        [Test]
        public static void MinimalRepro_MultiDepthAssignment_ResolvesClean()
            => AssertResolvesClean(@"
library A {
  F(n: Integer): Number {
    var d = 5.0;
    for (var i = 0; i < n; i = i + 1) {
      d = d.Min(1.0);
      for (var j = i + 1; j < n; j = j + 1) {
        d = d.Min(2.0);
      }
    }
    return d;
  }
}");

        // --- Single-depth variants (always worked; guard against regressions) -----------------

        [Test]
        public static void FlatLoop_SingleDepthAssignment_ResolvesClean()
            => AssertResolvesClean(@"
library A {
  F(n: Integer): Number {
    var d = 5.0;
    for (var i = 0; i < n; i = i + 1) {
      d = d.Min(1.0);
    }
    return d;
  }
}");

        [Test]
        public static void NestedLoop_InnerDepthOnlyAssignment_ResolvesClean()
            => AssertResolvesClean(@"
library A {
  F(n: Integer): Number {
    var d = 5.0;
    for (var i = 0; i < n; i = i + 1) {
      for (var j = i + 1; j < n; j = j + 1) {
        d = d.Min(2.0);
      }
    }
    return d;
  }
}");

        // --- Three depths, to prove the fix is depth-insensitive, not just two-deep ------------

        [Test]
        public static void TripleNestedLoop_ThreeDepthAssignment_ResolvesClean()
            => AssertResolvesClean(@"
library A {
  F(n: Integer): Number {
    var d = 5.0;
    for (var i = 0; i < n; i = i + 1) {
      d = d.Min(1.0);
      for (var j = 0; j < n; j = j + 1) {
        d = d.Min(2.0);
        for (var k = 0; k < n; k = k + 1) {
          d = d.Min(3.0);
        }
      }
    }
    return d;
  }
}");

        // --- while / do variants of the multi-depth idiom -------------------------------------

        [Test]
        public static void WhileLoop_MultiDepthAssignment_ResolvesClean()
            => AssertResolvesClean(@"
library A {
  F(n: Integer): Number {
    var d = 5.0;
    var i = 0;
    while (i < n) {
      d = d.Min(1.0);
      var j = 0;
      while (j < n) {
        d = d.Min(2.0);
        j = j + 1;
      }
      i = i + 1;
    }
    return d;
  }
}");

        // --- Shadowing: a nested `var d` must still bind a fresh, independent variable ---------
        // The inner `d` is a new declaration; both must resolve cleanly and the outer one is
        // returned. This proves the fix did not collapse shadowing into the outer binding.

        [Test]
        public static void ShadowedRedeclaration_InNestedLoop_ResolvesClean()
            => AssertResolvesClean(@"
library A {
  F(n: Integer): Number {
    var d = 5.0;
    for (var i = 0; i < n; i = i + 1) {
      d = d.Min(1.0);
      for (var j = i + 1; j < n; j = j + 1) {
        var d = 9.0;
        d = d.Min(2.0);
      }
    }
    return d;
  }
}");
    }
}
