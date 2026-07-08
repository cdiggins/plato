// Support machinery for the Plato conformance suite (docs/plato-roadmap.md Phase 1).
// - ValueGen: deterministic, seeded pseudorandom instance construction for generated types (Phase 1.3).
// - KnownFailures: the quarantine manifest (Phase 1.5) mapping (law-or-witness, type) -> reason + review-doc reference.
// - ConformanceRunner: shared invoke/assert logic for law and witness test cases.
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace Ara3D.SDK.ConformanceTests
{
    /// <summary>Deterministic pseudorandom value construction for generated Plato types.</summary>
    public static class ValueGen
    {
        /// <summary>Instances per (type, law) test case.</summary>
        public const int N = 25;

        public const int MaxDepth = 6;

        static readonly float[] SpecialFloats = { 0f, 1f, -1f, 0.5f, -0.5f, 2f, -2f, 0.25f, 3f, -3f };

        // Types verified unconstructible, with the reason (reported by CoverageTests).
        public static readonly SortedDictionary<string, string> Unconstructible = new();

        /// <summary>Stable FNV-1a hash so seeds do not depend on runtime string hashing.</summary>
        public static int StableSeed(string s)
        {
            unchecked
            {
                var h = 2166136261u;
                foreach (var c in s) { h ^= c; h *= 16777619u; }
                return (int)h;
            }
        }

        public static bool CanCreate(Type t) => CanCreate(t, 0, new HashSet<Type>());

        static bool CanCreate(Type t, int depth, HashSet<Type> inProgress)
        {
            if (depth > MaxDepth) return false;
            if (t == typeof(float) || t == typeof(double) || t == typeof(int) ||
                t == typeof(bool) || t == typeof(char) || t == typeof(string))
                return true;
            if (t.IsGenericTypeDefinition || t.IsAbstract || t.IsInterface || t.IsPointer || t.IsByRef)
            {
                Note(t, "interface/abstract/open-generic parameter type");
                return false;
            }
            if (!inProgress.Add(t)) return false; // cyclic
            try
            {
                var ok = PickConstructor(t, depth, inProgress) != null;
                if (!ok)
                    Note(t, "no public constructor whose parameters are all constructible (Number/Integer/Boolean/char/string or recursively constructible structs)");
                return ok;
            }
            finally { inProgress.Remove(t); }
        }

        static void Note(Type t, string reason)
        {
            if (t.Namespace == "Ara3D.Geometry")
                Unconstructible.TryAdd(t.Name, reason);
        }

        static ConstructorInfo PickConstructor(Type t, int depth, HashSet<Type> inProgress)
        {
            // Prefer the constructor with the MOST parameters: for generated Plato types
            // that is the "Regular Constructor" over the declared fields.
            return t.GetConstructors(BindingFlags.Public | BindingFlags.Instance)
                .Where(c => c.GetParameters().Length > 0)
                .OrderByDescending(c => c.GetParameters().Length)
                .FirstOrDefault(c => c.GetParameters().All(p => CanCreate(p.ParameterType, depth + 1, inProgress)));
        }

        public static object Create(Type t, Random rng, int depth = 0)
        {
            if (t == typeof(float)) return NextFloat(rng);
            if (t == typeof(double)) return (double)NextFloat(rng);
            if (t == typeof(int)) return rng.Next(-8, 9);
            if (t == typeof(bool)) return rng.Next(2) == 0;
            if (t == typeof(char)) return (char)('a' + rng.Next(26));
            if (t == typeof(string))
            {
                var len = rng.Next(0, 6);
                var sb = new StringBuilder();
                for (var i = 0; i < len; i++) sb.Append((char)('a' + rng.Next(26)));
                return sb.ToString();
            }
            var ctor = PickConstructor(t, depth, new HashSet<Type> { t })
                ?? throw new InvalidOperationException($"Cannot construct {t.Name}");
            var args = ctor.GetParameters().Select(p => Create(p.ParameterType, rng, depth + 1)).ToArray();
            return ctor.Invoke(args);
        }

        static float NextFloat(Random rng)
        {
            // 30% edge values, 70% uniform in [-10, 10]. Kept in a modest range so that
            // mixed absolute+relative law tolerances (LawEq in laws.plato) are meaningful.
            if (rng.NextDouble() < 0.3)
                return SpecialFloats[rng.Next(SpecialFloats.Length)];
            return (float)(rng.NextDouble() * 20.0 - 10.0);
        }
    }

    public sealed class KnownFailure
    {
        public string Test { get; set; }
        public string Type { get; set; }
        public string Reason { get; set; }
        public string Reference { get; set; }
        public string Key => string.IsNullOrEmpty(Type) ? Test : $"{Type}.{Test}";
    }

    /// <summary>The known-failures manifest (roadmap Phase 1.5): the work queue for the Phase 4 bug-fix wave.</summary>
    public static class KnownFailures
    {
        static readonly Lazy<Dictionary<string, KnownFailure>> LazyMap = new(Load);
        public static Dictionary<string, KnownFailure> Map => LazyMap.Value;

        static Dictionary<string, KnownFailure> Load()
        {
            var path = Path.Combine(TestContext.CurrentContext.TestDirectory, "KnownFailures.json");
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true, ReadCommentHandling = JsonCommentHandling.Skip, AllowTrailingCommas = true };
            using var doc = JsonDocument.Parse(File.ReadAllText(path), new JsonDocumentOptions { CommentHandling = JsonCommentHandling.Skip, AllowTrailingCommas = true });
            var list = doc.RootElement.GetProperty("knownFailures").Deserialize<List<KnownFailure>>(options);
            var map = new Dictionary<string, KnownFailure>(StringComparer.Ordinal);
            foreach (var kf in list)
            {
                if (string.IsNullOrEmpty(kf.Test)) throw new InvalidDataException("KnownFailures.json entry without 'test'");
                if (!map.TryAdd(kf.Key, kf)) throw new InvalidDataException($"Duplicate KnownFailures.json entry: {kf.Key}");
            }
            return map;
        }
    }

    public static class ConformanceRunner
    {
        /// <summary>Unwrap a Plato Boolean (or a System.Boolean) result to a bool.</summary>
        public static bool ToBool(object result)
        {
            if (result is bool b) return b;
            if (result == null) throw new InvalidOperationException("null result");
            var field = result.GetType().GetField("Value");
            if (field != null && field.FieldType == typeof(bool)) return (bool)field.GetValue(result);
            throw new InvalidOperationException($"Cannot convert {result.GetType()} to bool");
        }

        public static bool IsBooleanLike(Type t)
            => t == typeof(bool) || (t.Namespace == "Ara3D.Geometry" && t.Name == "Boolean");

        /// <summary>
        /// Manifest-aware verdict (roadmap Phase 1.5):
        /// known-failing that fails    => Assert.Ignore (suite stays green, bug stays documented);
        /// known-failing that passes   => Assert.Fail   (fix landed: remove the manifest entry);
        /// unknown failure             => real failure.
        /// </summary>
        public static void AssertWithManifest(string key, IReadOnlyList<string> failures, int trials)
        {
            KnownFailures.Map.TryGetValue(key, out var known);
            if (failures.Count > 0)
            {
                var detail = $"{key}: {failures.Count}/{trials} trial(s) failed.\n  " + string.Join("\n  ", failures.Take(5));
                if (known != null)
                    Assert.Ignore($"KNOWN FAILURE ({known.Reference}): {known.Reason}\n{detail}");
                Assert.Fail(detail);
            }
            if (known != null)
                Assert.Fail($"{key} PASSES but is listed in KnownFailures.json ({known.Reference}). Remove the manifest entry.");
            Assert.Pass();
        }

        public static string Describe(object value)
        {
            try { return value?.ToString() ?? "null"; }
            catch (Exception e) { return $"<ToString threw {e.GetType().Name}>"; }
        }
    }
}
