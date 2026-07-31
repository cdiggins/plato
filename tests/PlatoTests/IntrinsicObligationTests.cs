using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Ara3D.Geometry.Compiler;
using Ara3D.Geometry.Compiler.Symbols;
using Ara3D.Geometry.CSharpWriter;
using NUnit.Framework;

namespace PlatoTests
{
    /// <summary>
    /// Lints intrinsic obligations against the actual handwritten runtime (plato-330).
    ///
    /// A bodyless library function (<see cref="FunctionType.Intrinsic"/>) is a promise that the
    /// target backend's runtime supplies the member. Nothing checked that promise: plato-308 Root 2
    /// (Angle.Compare/Hash, matrix ColumnCount/ElementAt, Quaternion.Lerp), the Number.Pi/Tau/E/
    /// Epsilon constants, and most recently Matrix RowCount all failed a thousand generated files
    /// downstream instead. Since plato-331 the V2 shared sources compile into this test assembly,
    /// so the check is direct reflection — no erased-signature snapshot mapping needed.
    ///
    /// A runtime counterpart is: an instance/static method, property, or field of that name on the
    /// V2 struct, or an extension method in a V2 static class whose first parameter is the struct
    /// or its scalar-erased C# type (Number intrinsics like Pi live as extensions on float).
    /// Matching is by name and receiver, deliberately not by arity: overload drift is the C#
    /// build's job; this gate exists to catch members that are MISSING entirely.
    /// </summary>
    [TestFixture]
    public static class IntrinsicObligationTests
    {
        private static readonly Assembly V2Assembly = typeof(Ara3D.Geometry.Plane).Assembly;

        // Scoped by the pinned V2 struct NAMES, not by CSharpWriter.PrimitiveTypes: plato-365 is
        // emptying that dictionary down to the scalars, and a PrimitiveTypes-scoped filter would
        // have quietly dropped Angle, Plane, and the matrices out of this gate as each one stopped
        // being a primitive. The runtime still supplies their members, so the obligations still
        // need checking.
        private static readonly IReadOnlyDictionary<string, Type> V2Structs
            = IntrinsicsSurfaceTests.V2PrimitiveStructs().ToDictionary(t => t.Name);

        private static readonly IReadOnlyList<Type> V2StaticClasses
            = V2Assembly.GetTypes()
                .Where(t => t.Namespace == "Ara3D.Geometry"
                            && t.IsClass && t.IsAbstract && t.IsSealed
                            && t.Name != "IntrinsicsTestShims")
                .ToList();

        /// <summary>The erased C# type a primitive can appear as in extension-method receiver
        /// position (e.g. Number intrinsics are extensions on float).</summary>
        private static readonly IReadOnlyDictionary<string, Type> ErasedReceivers
            = new Dictionary<string, Type>
            {
                { "Number", typeof(float) },
                { "Integer", typeof(int) },
                { "Boolean", typeof(bool) },
                { "Character", typeof(char) },
                { "String", typeof(string) },
                { "Matrix3x2", typeof(System.Numerics.Matrix3x2) },
                { "Matrix4x4", typeof(System.Numerics.Matrix4x4) },
                { "Quaternion", typeof(System.Numerics.Quaternion) },
                { "Vector2", typeof(System.Numerics.Vector2) },
                { "Vector3", typeof(System.Numerics.Vector3) },
                { "Vector4", typeof(System.Numerics.Vector4) },
                { "Vector8", typeof(System.Runtime.Intrinsics.Vector256<float>) },
            };

        private static bool HasRuntimeCounterpart(string typeName, string memberName, int arity)
        {
            // Operator-named intrinsics (Add/Negative/LessThan/ShiftLeft...) are synthesized by the
            // writer as wrappers over the C# operator (`Add(this int a, int b) => ((Integer)a) + b`),
            // discharged by V2's operator overloads or the erased primitive's builtin operators. A
            // genuinely missing operator fails the ONE wrapper emission, not a thousand call sites,
            // so the C# build is an adequate gate for those.
            var op = arity == 1 ? Ara3D.Geometry.AST.Operators.NameToUnaryOperator(memberName)
                : arity == 2 ? Ara3D.Geometry.AST.Operators.NameToBinaryOperator(memberName)
                : null;
            if (op != null)
                return true;

            var type = V2Structs[typeName];
            const BindingFlags any = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static;
            if (type.GetMethods(any).Any(m => m.Name == memberName)
                || type.GetProperties(any).Any(p => p.Name == memberName)
                || type.GetFields(any).Any(f => f.Name == memberName))
                return true;

            var receivers = new List<Type> { type };
            if (ErasedReceivers.TryGetValue(typeName, out var erased))
                receivers.Add(erased);
            return V2StaticClasses.Any(c => c
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Any(m =>
                {
                    if (m.Name != memberName)
                        return false;
                    var ps = m.GetParameters();
                    return ps.Length > 0 && receivers.Contains(ps[0].ParameterType);
                }));
        }

        /// <summary>Distinct "Type.Member" keys for every intrinsic obligation on a V2 primitive
        /// receiver with no runtime counterpart. Receivers outside V2 (Array, tuples, delegate
        /// types) are out of scope: their lowering does not target the handwritten runtime.</summary>
        private static IReadOnlyList<string> MissingObligations(Compilation comp)
            => comp.FunctionDefinitions
                .Where(f => f.FunctionType == FunctionType.Intrinsic && f.NumParameters > 0)
                .Select(f => (Receiver: f.GetParameterType(0).Name, f.Name, Arity: f.NumParameters))
                .Where(o => V2Structs.ContainsKey(o.Receiver))
                .Distinct()
                .Where(o => !HasRuntimeCounterpart(o.Receiver, o.Name, o.Arity))
                .Select(o => $"{o.Receiver}.{o.Name}")
                .OrderBy(s => s)
                .ToList();

        /// <summary>The forward stdlib's KNOWN undischarged obligations — a worklist to SHRINK,
        /// never grow. The original 22 entries (measured when the gate landed 2026-07-30) were
        /// discharged as a V2 content increment the same day; new entries need a reason.</summary>
        private static readonly IReadOnlyList<string> ForwardKnownMissing = Array.Empty<string>();

        private static IReadOnlyList<string> AssertScopeSane(Compilation comp, string corpusName)
        {
            var obligations = comp.FunctionDefinitions
                .Count(f => f.FunctionType == FunctionType.Intrinsic && f.NumParameters > 0
                            && V2Structs.ContainsKey(f.GetParameterType(0).Name));
            TestContext.WriteLine($"{corpusName}: {obligations} intrinsic obligations on V2 primitive receivers");
            Assert.Greater(obligations, 0, $"{corpusName} declared no in-scope obligations — scope filter is broken.");

            var missing = MissingObligations(comp);
            foreach (var m in missing)
                TestContext.WriteLine($"  MISSING {m}");
            return missing;
        }

        [Test]
        public static void ForwardStdLibIntrinsicObligationsAreDischargedOrPinned()
        {
            var missing = AssertScopeSane(CheckerTestSupport.CompileForwardStdLib(), "stdlib (forward)");
            var newGaps = missing.Except(ForwardKnownMissing).ToList();
            var fixedGaps = ForwardKnownMissing.Except(missing).ToList();
            Assert.IsEmpty(newGaps,
                "NEW intrinsic obligations with no Plato.Intrinsics counterpart. Add the member " +
                "to V2 (method form — see IntrinsicsSurfaceTests) or fix the declaration; do NOT " +
                "add to ForwardKnownMissing without a reason. Without the member the generated C# " +
                "fails with CS1061 a thousand files downstream — or worse, silently at first use. " +
                "New: " + string.Join(", ", newGaps));
            Assert.IsEmpty(fixedGaps,
                "These obligations are now discharged — delete them from ForwardKnownMissing so the " +
                "worklist only shrinks: " + string.Join(", ", fixedGaps));
        }

        /// <summary>stdlib-legacy obligations the V2 runtime deliberately stopped serving. Like
        /// <see cref="ForwardKnownMissing"/> this list only shrinks — but unlike it, an entry here
        /// is not automatically a defect: legacy generation is retired (goldens and the
        /// byte-identity diff-gate went on 2026-07-30), and where the two corpora disagree about a
        /// SHAPE the forward declaration wins by design.</summary>
        private static readonly IReadOnlyList<string> LegacyKnownMissing = new[]
        {
            // plato-365: Plane's shape is now the forward declaration (Normal: Direction3D,
            // Distance: Number, Hesse form). stdlib-legacy asks for the System.Numerics parity
            // surface — `D` (which is -Distance) plus the two With* over it. Re-adding `D` would
            // re-import exactly the sign ambiguity this increment removed.
            "Plane.D",
            "Plane.WithD",
        };

        [Test]
        public static void LegacyStdLibIntrinsicObligationsAreDischarged()
        {
            var missing = AssertScopeSane(CheckerTestSupport.CompileStdLib(), "stdlib-legacy");
            Assert.IsEmpty(missing.Except(LegacyKnownMissing),
                "stdlib-legacy declares intrinsic obligations with no Plato.Intrinsics counterpart " +
                "(V2 was built against legacy; this set was empty on 2026-07-30). New: " +
                string.Join(", ", missing.Except(LegacyKnownMissing)));
            Assert.IsEmpty(LegacyKnownMissing.Except(missing),
                "These legacy obligations are discharged again — delete them from LegacyKnownMissing.");
        }

        // The plato-308 Root 2 shape in miniature: one obligation V2 discharges, one it never will.
        private const string SyntheticSource = @"
type Number { }
type Integer { }
type Boolean { }
type Character { }
type String { }
type Object { }

type Function0<TR> { }
type Function1<T0, TR> { }
type Function2<T0, T1, TR> { }
type Function3<T0, T1, T2, TR> { }

library Intrinsics
{
    Abs(self: Number): Number;
    Frobnicate987(self: Number): Number;
}
";

        [Test]
        public static void RuleFlagsAMissingMemberAndOnlyThat()
        {
            var dir = System.IO.Path.Combine(System.IO.Path.GetTempPath(),
                "plato-330-synthetic-" + Guid.NewGuid().ToString("N"));
            System.IO.Directory.CreateDirectory(dir);
            try
            {
                System.IO.File.WriteAllText(System.IO.Path.Combine(dir, "corpus.plato"), SyntheticSource);
                var missing = MissingObligations(CheckerTestSupport.CompileFolder(dir));
                Assert.AreEqual(new[] { "Number.Frobnicate987" }, missing,
                    "The rule must flag exactly the obligation V2 cannot discharge.");
            }
            finally
            {
                System.IO.Directory.Delete(dir, true);
            }
        }
    }
}
