using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Ara3D.Geometry.Compiler;
using Ara3D.Geometry.Compiler.Analysis;
using Ara3D.Geometry.Compiler.Checking;
using Ara3D.Geometry.Compiler.Symbols;
using Ara3D.Geometry.CSharpWriter;
using NUnit.Framework;

namespace PlatoTests
{
    /// <summary>
    /// The emit-retarget differential (Elaborate → Monomorphize → Emit, increment 1 of "Emit").
    ///
    /// Over the target subset — stdlib functions that monomorphize to a fully-ground
    /// <see cref="TirFunction"/> AND that we can align 1:1 with a body the current
    /// <see cref="CSharpFunctionBodyWriter"/> emits in the DEFAULT C# style — it renders each body
    /// twice: once from the symbol graph via the current writer (the reference), once from the TIR
    /// via the experimental <see cref="TirCSharpBodyWriter"/> (the candidate). It compares them
    /// byte-for-byte and reports the match rate and a ranked mismatch taxonomy.
    ///
    /// This is a MEASUREMENT, not a switch-over: nothing here touches the default emit path (the
    /// harness constructs writer objects in-memory and reads their strings; it writes no files and
    /// calls no default-path entry point). The byte-identity gate (regen-plato.ps1) is unaffected.
    /// </summary>
    [TestFixture]
    public static class EmitDifferentialTests
    {
        private sealed class Row
        {
            public string TypeName;
            public string FnName;
            public bool Aligned;      // a fully-ground TIR was found for this (FunctionDef, type)
            public bool Match;        // reference == candidate, byte-for-byte
            public string Category;   // mismatch bucket (null when Match)
            public string Reference;
            public string Candidate;
        }

        private static Compilation _compilation;
        private static readonly List<Row> _rows = new List<Row>();
        private static int _writerBodiesConsidered;   // bodied functions the writer emitted
        private static int _tirThrew;                 // TIR writer exceptions (must stay 0: totality)
        private static int _refThrew;                 // reference writer exceptions
        private static int _groundBodiedMono;         // size of the target subset (the oracle side)

        [OneTimeSetUp]
        public static void Setup()
        {
            _compilation = CheckerTestSupport.CompileStdLib();

            // The target subset: one fully-ground, bodied TIR per (source function, concrete type).
            var mono = new Monomorphizer(_compilation).MonomorphizeAll();
            var groundBodied = mono.Where(m => m.HasBody && m.IsFullyGround).ToList();
            _groundBodiedMono = groundBodied.Count;

            var tirByKey = new Dictionary<(FunctionDef, string), TirFunction>();
            foreach (var m in groundBodied)
            {
                var key = (m.Original, m.ConcreteType?.Name);
                if (m.Original != null && key.Item2 != null && !tirByKey.ContainsKey(key))
                    tirByKey[key] = m.Tir;
            }

            // A default-mode C# writer, configured exactly like WriteAll("float") but WITHOUT writing
            // any files (we only read body strings). ExtensionStyle / ScalarErase / Optimize stay off.
            var writer = new CSharpWriter(_compilation, "unused-differential-output")
            {
                Namespace = "Ara3D.Geometry",
                FloatType = "float",
            };

            // Drive the writer's own per-type enumeration so the reference strings are the ones that
            // appear in the golden output, then join to the TIR by (FunctionDef, type name).
            foreach (var ct in _compilation.ConcreteTypes)
            {
                var fieldNames = ct.TypeDef.Fields.Select(f => f.Name).ToList();
                var tw = new CSharpTypeWriter(writer, ct.TypeDef);

                foreach (var group in ct.InterfaceFunctionGroups)
                {
                    FunctionInstance f;
                    try { f = writer.Analyzer.ChooseBestFunction(group, out _); }
                    catch { continue; }

                    // Mirror the writer's own skips, then restrict to functions it actually emits a
                    // Plato body for (body-less intrinsics / primitive operators use other paths).
                    if (CSharpConcreteTypeWriter.SkipFunction(f, fieldNames, ct.TypeDef.Name))
                        continue;
                    if (f.Implementation?.Body == null)
                        continue;

                    _writerBodiesConsidered++;

                    var row = new Row { TypeName = ct.TypeDef.Name, FnName = f.Name };

                    string reference = null;
                    try
                    {
                        var fi = tw.ToFunctionInfo(f, ct.TypeDef);
                        reference = new CSharpFunctionBodyWriter(tw, fi, false, false).ToString();
                    }
                    catch { _refThrew++; continue; }
                    row.Reference = reference;

                    if (!tirByKey.TryGetValue((f.Implementation, ct.TypeDef.Name), out var tir))
                    {
                        row.Aligned = false;
                        _rows.Add(row);
                        continue;
                    }
                    row.Aligned = true;

                    string candidate;
                    try { candidate = new TirCSharpBodyWriter(tw, tir).ToString(); }
                    catch { _tirThrew++; _rows.Add(row); continue; }
                    row.Candidate = candidate;

                    row.Match = reference == candidate;
                    if (!row.Match)
                        row.Category = Classify(reference, candidate);
                    _rows.Add(row);
                }
            }
        }

        // --- mismatch taxonomy ---------------------------------------------------

        private static readonly Regex Ws = new Regex(@"\s+", RegexOptions.Compiled);
        private static string NoWs(string s) => Ws.Replace(s ?? "", "");

        private static string Classify(string reference, string candidate)
        {
            if (candidate.Contains("/*unresolved:")) return "unresolved-tir-node";
            if (candidate.Contains("/*?*/")) return "unknown-tir-node";

            var r = NoWs(reference);
            var c = NoWs(candidate);
            if (r == c) return "whitespace-only";

            // The eta-expansion the normalizer performs on a bare member-group reference in value
            // position: the source's `M11` becomes `(_e) => _e.M11()` in the TIR.
            if (candidate.Contains("_eta") || candidate.Contains("=>  _"))
                return "eta-expanded-member-ref";

            var parensOnly = NoParens(c) == NoParens(r);
            var coercionOnly = StripConv(c) == StripConv(r);

            // No-arg member emitted as a method call `x.Sqrt()` where the writer uses property
            // syntax `x.Sqrt` — the EmissionKind (Intrinsic/Operator/InstanceMethod) does not
            // record the member-access shape (HasArgList) the writer keys property-vs-method on.
            if (parensOnly && !coercionOnly)
                return "property-vs-method-parens";

            // An implicit conversion the TIR makes explicit (TirCoerce -> `.Number`, `.Quaternion`)
            // that the current writer leaves to C#'s implicit conversion.
            if (coercionOnly && !parensOnly)
                return "explicit-coercion";

            // Both of the above in one body.
            if (NoParens(StripConv(c)) == NoParens(StripConv(r)))
                return "coercion+parens-compound";

            // A bare nullary/constant reference the writer emits differently than Constants.<X>.
            if (r == c.Replace("Constants.", "") || c == r.Replace("Constants.", ""))
                return "constant-qualification";

            // Tuple vs constructor / new-expression surface.
            if (reference.Contains("new ") ^ candidate.Contains("new "))
                return "constructor/new-form";

            return "structural-other";
        }

        private static string NoParens(string s) => (s ?? "").Replace("()", "");

        private static readonly Regex ConvMember = new Regex(
            @"\.(Vector\d|Vector\dD|Point\dD|Number|Integer|Angle|Boolean|Character|String|Complex|Quaternion|Matrix\w+)",
            RegexOptions.Compiled);

        // Symmetric: strips type-named conversion members from BOTH sides so only a one-sided
        // (extra/missing) coercion changes the comparison.
        private static string StripConv(string s) => ConvMember.Replace(s ?? "", "");

        // --- tests ---------------------------------------------------------------

        [Test]
        public static void HarnessAlignsANonTrivialSubsetAndReferenceMatchesGolden()
        {
            var aligned = _rows.Count(r => r.Aligned);
            Assert.Greater(_writerBodiesConsidered, 0, "the current writer emitted no bodies?");
            Assert.Greater(aligned, 0, "no fully-ground TIR aligned with any current-writer body");

            // Sanity-check the reference extraction against a known golden body: Vector3.SdSphere
            // ships as `=> this.Length.Subtract(r);`.
            var sd = _rows.FirstOrDefault(r => r.TypeName == "Vector3" && r.FnName == "SdSphere");
            if (sd != null)
                Assert.That(sd.Reference, Does.Contain("this.Length.Subtract(r)"),
                    "reference extraction drifted from the golden output");
        }

        [Test]
        public static void TirBodyWriterIsTotalOverTheGroundSubset()
        {
            // The experimental writer must never throw over the aligned ground subset.
            Assert.AreEqual(0, _tirThrew, "TirCSharpBodyWriter threw on some ground bodies (not total)");
        }

        [Test]
        public static void ReportByteMatchRateAndMismatchTaxonomy()
        {
            var aligned = _rows.Where(r => r.Aligned && r.Candidate != null).ToList();
            var matched = aligned.Count(r => r.Match);
            var rate = aligned.Count == 0 ? 0.0 : 100.0 * matched / aligned.Count;

            TestContext.WriteLine("=== Emit differential: TIR body writer vs current CSharpFunctionBodyWriter (default style) ===");
            TestContext.WriteLine($"stdlib fully-ground bodied monomorphized functions (target subset) : {_groundBodiedMono}");
            TestContext.WriteLine($"current-writer bodies enumerated                                   : {_writerBodiesConsidered}");
            TestContext.WriteLine($"  aligned to a fully-ground TIR (measured subset)                  : {aligned.Count}");
            TestContext.WriteLine($"  not aligned (no ground TIR for that (fn,type))                   : {_rows.Count(r => !r.Aligned)}");
            TestContext.WriteLine($"reference-writer exceptions                                        : {_refThrew}");
            TestContext.WriteLine($"TIR-writer exceptions                                              : {_tirThrew}");
            TestContext.WriteLine("");
            TestContext.WriteLine($"BYTE-IDENTICAL bodies : {matched} / {aligned.Count}  ({rate:F1} %)");
            TestContext.WriteLine("");

            var buckets = aligned.Where(r => !r.Match)
                .GroupBy(r => r.Category)
                .OrderByDescending(g => g.Count())
                .ToList();
            TestContext.WriteLine($"Mismatch taxonomy ({aligned.Count - matched} mismatches):");
            foreach (var b in buckets)
                TestContext.WriteLine($"  {b.Count(),5}  {b.Key}");

            // Projected rates if increment 2 closes the top buckets (arithmetic on the counts):
            int Count(params string[] cats) => buckets.Where(b => cats.Contains(b.Key)).Sum(b => b.Count());
            var afterParens = matched + Count("property-vs-method-parens", "coercion+parens-compound");
            var afterCoerce = afterParens + Count("explicit-coercion");
            TestContext.WriteLine("");
            TestContext.WriteLine("Projected (arithmetic on bucket counts):");
            TestContext.WriteLine($"  + property-shape bit on TirCall (property-vs-method) -> {afterParens}/{aligned.Count} ({100.0 * afterParens / aligned.Count:F1} %)");
            TestContext.WriteLine($"  + suppress implicit-widening coercions at emit       -> {afterCoerce}/{aligned.Count} ({100.0 * afterCoerce / aligned.Count:F1} %)");

            TestContext.WriteLine("");
            TestContext.WriteLine("Examples per bucket (reference  |  candidate):");
            foreach (var b in buckets)
            {
                TestContext.WriteLine($"-- {b.Key} --");
                foreach (var r in b.Take(4))
                {
                    TestContext.WriteLine($"   [{r.TypeName}.{r.FnName}]");
                    TestContext.WriteLine($"     ref : {Trim(r.Reference)}");
                    TestContext.WriteLine($"     tir : {Trim(r.Candidate)}");
                }
            }

            // A measurement, not a brittle gate: just prove the harness produced a real number.
            Assert.Greater(aligned.Count, 0);
        }

        private static string Trim(string s)
        {
            s = (s ?? "").Replace("\r", "").Replace("\n", "\\n");
            return s.Length > 200 ? s.Substring(0, 200) + " …" : s;
        }
    }
}
