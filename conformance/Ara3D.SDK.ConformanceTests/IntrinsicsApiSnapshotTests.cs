using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using NUnit.Framework;

namespace Ara3D.SDK.ConformanceTests
{
    /// <summary>
    /// API-snapshot guard for the handwritten intrinsic struct surface (consolidation plan C3 / M5).
    /// Reflects over the public members of the intrinsic types + their <c>&lt;Type&gt;Intrinsics</c>
    /// extension classes and compares a canonical dump against a committed baseline
    /// (<c>intrinsics-api-snapshot.txt</c>). When M5 moves behaviour from struct instance methods to
    /// extension methods the dump changes — that is a REVIEWABLE diff, and the point is that a
    /// <b>loss</b> (a member vanishing from both the struct and its extension class) cannot slip
    /// through unnoticed. See docs/plato-struct-surface.md for the keep-on-struct contract.
    ///
    /// Re-baseline (after a reviewed, intended surface change): set env
    /// <c>PLATO_UPDATE_API_SNAPSHOT=1</c> and run the test once, then commit the updated file.
    /// </summary>
    [TestFixture]
    public static class IntrinsicsApiSnapshotTests
    {
        // The handwritten intrinsic struct types whose surface is the M5 contract.
        private static readonly HashSet<string> IntrinsicStructs = new HashSet<string>
        {
            "Angle", "Number", "Integer", "Boolean", "Character", "String",
            "Vector2", "Vector3", "Vector4", "Vector8",
            "Matrix3x2", "Matrix4x4", "Quaternion", "Plane",
        };

        private static bool IsInScope(Type t)
            => t.Namespace == "Ara3D.Geometry"
               && (IntrinsicStructs.Contains(t.Name) || t.Name.EndsWith("Intrinsics"));

        private static string Short(Type t)
        {
            if (t.IsByRef) return "ref " + Short(t.GetElementType());
            if (t.IsGenericType)
            {
                var name = t.Name.Contains('`') ? t.Name.Substring(0, t.Name.IndexOf('`')) : t.Name;
                return name + "<" + string.Join(",", t.GetGenericArguments().Select(Short)) + ">";
            }
            return t.Name;
        }

        private static string Params(ParameterInfo[] ps)
            => "(" + string.Join(", ", ps.Select(p =>
                (p.GetCustomAttribute<System.Runtime.CompilerServices.ExtensionAttribute>() != null ? "" : "")
                + Short(p.ParameterType))) + ")";

        private static IEnumerable<string> Members(Type t)
        {
            const BindingFlags F = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            var isExt = t.Name.EndsWith("Intrinsics");

            foreach (var c in t.GetConstructors(F))
                yield return $"ctor {Params(c.GetParameters())}";
            foreach (var f in t.GetFields(F))
                yield return $"field {Short(f.FieldType)} {f.Name}";
            foreach (var p in t.GetProperties(F))
                yield return $"prop {Short(p.PropertyType)} {p.Name} {{{(p.CanRead ? " get" : "")}{(p.CanWrite ? " set" : "")} }}";
            foreach (var m in t.GetMethods(F))
            {
                if (m.IsSpecialName && (m.Name.StartsWith("get_") || m.Name.StartsWith("set_")))
                    continue; // property accessors already covered
                if (m.Name.StartsWith("Law_") || m.Name.StartsWith("Witness_"))
                    continue; // test-only members merged from plato-test-src; not the intrinsic surface
                var kind = m.IsSpecialName ? "op" : (m.IsStatic && !IsExtensionMethod(m) ? "static" : (IsExtensionMethod(m) ? "ext" : "method"));
                yield return $"{kind} {Short(m.ReturnType)} {m.Name}{Params(m.GetParameters())}";
            }
        }

        private static bool IsExtensionMethod(MethodInfo m)
            => m.IsStatic && m.GetCustomAttribute<System.Runtime.CompilerServices.ExtensionAttribute>() != null;

        private static string BuildSnapshot()
        {
            var asm = typeof(Ara3D.Geometry.Vector3).Assembly;
            var sb = new StringBuilder();
            sb.AppendLine("# Plato intrinsics API snapshot (C3/M5 surface guard). See docs/plato-struct-surface.md.");
            foreach (var t in asm.GetTypes().Where(IsInScope).OrderBy(t => t.Name, StringComparer.Ordinal))
            {
                var kind = t.IsValueType ? "struct" : (t.IsClass && t.IsAbstract && t.IsSealed ? "static class" : "class");
                sb.AppendLine($"TYPE {t.Name} ({kind})");
                foreach (var line in Members(t).OrderBy(s => s, StringComparer.Ordinal))
                    sb.AppendLine("  " + line);
            }
            return sb.ToString().Replace("\r\n", "\n");
        }

        private static string BaselinePath([CallerFilePath] string thisFile = "")
            => Path.Combine(Path.GetDirectoryName(thisFile), "intrinsics-api-snapshot.txt");

        [Test]
        public static void IntrinsicSurfaceMatchesBaseline()
        {
            var actual = BuildSnapshot();
            var path = BaselinePath();

            if (Environment.GetEnvironmentVariable("PLATO_UPDATE_API_SNAPSHOT") == "1")
            {
                File.WriteAllText(path, actual);
                Assert.Pass($"Re-baselined API snapshot: {path}");
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

            // Show the first divergence for a fast review.
            var e = expected.Split('\n');
            var a = actual.Split('\n');
            var firstDiff = Enumerable.Range(0, Math.Max(e.Length, a.Length))
                .FirstOrDefault(i => (i < e.Length ? e[i] : "<eof>") != (i < a.Length ? a[i] : "<eof>"));
            TestContext.WriteLine($"First divergence at line {firstDiff + 1}:");
            TestContext.WriteLine($"  baseline: {(firstDiff < e.Length ? e[firstDiff] : "<eof>")}");
            TestContext.WriteLine($"  actual  : {(firstDiff < a.Length ? a[firstDiff] : "<eof>")}");
            Assert.Fail("Intrinsic API surface changed vs baseline. If intended (e.g. an M5 "
                + "instance->extension move), review the diff and re-run with PLATO_UPDATE_API_SNAPSHOT=1 "
                + "to re-baseline; a member vanishing entirely is a LOSS to investigate.");
        }
    }
}
