using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Ara3D.Geometry.CSharpWriter;
using NUnit.Framework;

namespace PlatoTests
{
    /// <summary>
    /// Fast in-proc emit-snapshot gate for the C4 emitter simplification (consolidation plan C4).
    /// Compiles the stdlib once (<see cref="CheckerTestSupport.CompileStdLib"/>), generates the whole
    /// V2 (Unoptimized) library in-memory — <c>--csharp-style=extensions --scalar=float
    /// --no-properties</c> — and pins the emitted line of ~30 representative member bodies against a
    /// committed baseline (<c>emit-snapshot.txt</c>). Seconds, versus the ~60s regen+build+conformance
    /// cycle. The byte-identical goldens (<c>tools/regen-generated.ps1</c>) remain the authoritative
    /// gate; this is the seconds-fast inner loop that makes retiring the legacy writer non-risky, and
    /// reports the exact body that drifted.
    ///
    /// The pins deliberately span the surfaces the C4 steps touch: body-less interface stubs (step 1),
    /// scalar-erased extension methods + wrapper forwarders + float-land casts (step 4), the
    /// property-vs-method call-site decision and pseudo-field access (step 3), moved library members
    /// with <c>()</c> injection, constants-as-methods, and broadcast re-homing.
    ///
    /// Re-baseline (after a reviewed, intended body-shape change): set env
    /// <c>PLATO_UPDATE_EMIT_SNAPSHOT=1</c> and run the test once, then commit the updated file.
    /// </summary>
    [TestFixture]
    public static class EmitSnapshotTests
    {
        // (generated file, unique signature marker). Each marker must match exactly one emitted
        // line (asserted) so a pin is unambiguous; the captured line is the full emitted member.
        private static readonly (string File, string Marker)[] Pins =
        {
            // Scalar-erased Number: intrinsic wrapper-forwarders, float-land body, broadcasts, generic.
            ("_Number.g.cs", "float Abs(this float self)"),
            ("_Number.g.cs", "float Sqrt(this float self)"),
            ("_Number.g.cs", "float Clamp(this float self, float min, float max)"),
            ("_Number.g.cs", "float Lerp(this float a, float b, float t)"),
            ("_Number.g.cs", "int Sign(this float self)"),
            ("_Number.g.cs", "Angle Atan2(this float self, float x)"),
            ("_Number.g.cs", "_T0 Multiply<_T0>(this float scalar"),
            ("_Number.g.cs", "Vector3 Multiply(this float scalar, Vector3 right)"),
            ("_Number.g.cs", "Vector8 Divide(this float scalar, Vector8 right)"),
            ("_Number.g.cs", "bool NotEquals(this Number a, float b)"),
            ("_Number.g.cs", "implicit operator Vector2(Number value)"),
            // Scalar-erased Boolean: native-operator forwarders.
            ("_Boolean.g.cs", "bool Not(this bool b)"),
            ("_Boolean.g.cs", "bool And(this bool a, bool b)"),
            // Non-erased primitive Vector3: pseudo-field property access + method-form forwarders.
            ("_Vector3.g.cs", "float X(this System.Numerics.Vector3 self)"),
            ("_Vector3.g.cs", "Vector3 Normalize(this System.Numerics.Vector3 self)"),
            ("_Vector3.g.cs", "float Length(this System.Numerics.Vector3 self)"),
            ("_Vector3.g.cs", "float Dot(this System.Numerics.Vector3 self, Vector3 right)"),
            // Constants emitted as methods, referencing each other.
            ("Constants.g.cs", "float Pi()"),
            ("Constants.g.cs", "float TwoPi()"),
            ("Constants.g.cs", "float HalfPi()"),
            // Moved library members: tuples, pseudo-fields, method-form calls, () injection.
            ("Vectors.g.cs", "Vector2 Perpendicular(this Vector2 v)"),
            ("Vectors.g.cs", "Vector2 Rotate(this Vector2 v, Angle a)"),
            ("Vectors.g.cs", "Angle Angle(this Vector2 a, Vector2 b)"),
            ("Vectors.g.cs", "bool IsParallel(this Vector2 a, Vector2 b)"),
            // Angle library: scalar and Angle receivers of the same moved name.
            ("Angles.g.cs", "float Turns(this Angle x)"),
            ("Angles.g.cs", "Angle Turns(this float x)"),
            ("Angles.g.cs", "Angle Turns(this int x)"),
            ("Transforms.g.cs", "Vector3 TransformNormal(this Transform3D x, Vector3 v)"),
            // Body-less interface stubs (C4 step 1: emitted off the legacy writer).
            ("_IdentityTransform3D.g.cs", "Quaternion Quaternion =>"),
            ("_Identity2D.g.cs", "Matrix3x2 Matrix =>"),
        };

        private static CSharpWriter BuildV2Library()
        {
            var w = new CSharpWriter(CheckerTestSupport.CompileStdLib(), "unused-emit-snapshot")
            {
                ExtensionStyle = true,
                ScalarErase = true,
                MethodsOnly = true,
                NoProperties = true,
            };
            w.WriteAll("float");
            return w;
        }

        private static string ExtractLine(CSharpWriter w, string file, string marker)
        {
            Assert.IsTrue(w.Files.ContainsKey(file), $"emit snapshot: no generated file '{file}'");
            var lines = w.Files[file].ToString().Replace("\r\n", "\n").Split('\n');
            var matches = lines.Where(l => l.Contains(marker)).ToList();
            Assert.AreEqual(1, matches.Count,
                $"emit snapshot: marker '{marker}' matched {matches.Count} line(s) in {file} (must be unique)");
            return matches[0].Trim();
        }

        private static string BuildSnapshot()
        {
            var w = BuildV2Library();
            var sb = new StringBuilder();
            sb.AppendLine("# Plato V2 emit snapshot (C4 emitter gate). Recipe: --csharp-style=extensions --scalar=float --no-properties.");
            sb.AppendLine("# Re-baseline with PLATO_UPDATE_EMIT_SNAPSHOT=1. Authoritative gate is tools/regen-generated.ps1.");
            foreach (var (file, marker) in Pins)
            {
                sb.AppendLine($"@ {file} :: {marker}");
                sb.AppendLine("  " + ExtractLine(w, file, marker));
            }
            return sb.ToString().Replace("\r\n", "\n");
        }

        private static string BaselinePath([CallerFilePath] string thisFile = "")
            => Path.Combine(Path.GetDirectoryName(thisFile), "emit-snapshot.txt");

        [Test]
        public static void V2MemberBodiesMatchBaseline()
        {
            var actual = BuildSnapshot();
            var path = BaselinePath();

            if (Environment.GetEnvironmentVariable("PLATO_UPDATE_EMIT_SNAPSHOT") == "1")
            {
                File.WriteAllText(path, actual);
                Assert.Pass($"Re-baselined emit snapshot: {path}");
                return;
            }

            if (!File.Exists(path))
            {
                File.WriteAllText(path, actual);
                Assert.Fail($"No baseline existed; wrote initial snapshot to {path}. Review and commit it.");
                return;
            }

            var expected = File.ReadAllText(path).Replace("\r\n", "\n");
            if (expected == actual)
                return;

            var e = expected.Split('\n');
            var a = actual.Split('\n');
            var firstDiff = Enumerable.Range(0, Math.Max(e.Length, a.Length))
                .FirstOrDefault(i => (i < e.Length ? e[i] : "<eof>") != (i < a.Length ? a[i] : "<eof>"));
            TestContext.WriteLine($"First divergence at line {firstDiff + 1}:");
            TestContext.WriteLine($"  baseline: {(firstDiff < e.Length ? e[firstDiff] : "<eof>")}");
            TestContext.WriteLine($"  actual  : {(firstDiff < a.Length ? a[firstDiff] : "<eof>")}");
            Assert.Fail("V2 emit snapshot changed vs baseline. If intended (a reviewed emitter change), "
                + "confirm tools/regen-generated.ps1 still passes, then re-run with PLATO_UPDATE_EMIT_SNAPSHOT=1.");
        }
    }
}
