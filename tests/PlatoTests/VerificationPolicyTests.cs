using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace PlatoTests
{
    /// <summary>
    /// plato-388: `stdlib/verification.json` is the one copy of the verification policy. These
    /// fixtures fail when a tier list or a ratchet ceiling reappears in a gate script or a test
    /// outside the manifest and <c>CheckerTestSupport</c> (which reads it).
    /// </summary>
    [TestFixture]
    public static class VerificationPolicyTests
    {
        // Two tier names quoted next to each other in one list: 'foundation', 'geometry' / "geometry","graphics".
        private static readonly Regex TierListLiteral = new(
            @"[""'](?:foundation|geometry|graphics|future)[""']\s*,\s*[""'](?:foundation|geometry|graphics|future)[""']",
            RegexOptions.Compiled);

        // A numeric ceiling for a ratchet: `MaxFoo = 12`, `ceiling = 3`, or a `--ratchet <n>` argument.
        private static readonly Regex CeilingLiteral = new(
            @"\b\w*(?:Max|Ceiling|ceiling|Ratchet)\w*\s*=\s*\d+|--ratchet\b", RegexOptions.Compiled);

        // Ceilings that govern corpora the manifest does not cover (the legacy stdlib and the TIR
        // verifier), so they are not duplicates of a manifest ratchet.
        private static readonly HashSet<string> OtherCorpusCeilings = new()
        {
            "CheckerCompletenessTests.cs", // legacy/stdlib-legacy checker ratchet
            "TirTypeVerifierTests.cs",     // TIR verifier, not stdlib text
        };

        private static string RepoRoot => Path.GetDirectoryName(CheckerTestSupport.FindForwardStdLib())!;

        private static IEnumerable<string> ScannedFiles()
        {
            var tools = Path.Combine(RepoRoot, "tools");
            var tests = Path.Combine(RepoRoot, "tests", "PlatoTests");
            return Directory.GetFiles(tools, "*.ps1")
                .Concat(Directory.GetFiles(tools, "*.py"))
                .Concat(Directory.GetFiles(tests, "*.cs"))
                .Where(f => !Path.GetFileName(f).Equals("CheckerTestSupport.cs", StringComparison.Ordinal)
                         && !Path.GetFileName(f).Equals("VerificationPolicyTests.cs", StringComparison.Ordinal));
        }

        /// <summary>Lines of <paramref name="text"/> matching <paramref name="pattern"/>, comments excluded.</summary>
        public static IEnumerable<string> Offences(string text, Regex pattern)
            => text.Split('\n')
                .Select((line, i) => (line: line.Trim(), n: i + 1))
                .Where(x => !x.line.StartsWith("//") && !x.line.StartsWith("#") && !x.line.StartsWith("///"))
                .Where(x => pattern.IsMatch(x.line))
                .Select(x => $"line {x.n}: {x.line}");

        private static List<string> Scan(Regex pattern, Func<string, bool> skip)
            => ScannedFiles()
                .Where(f => !skip(Path.GetFileName(f)))
                .SelectMany(f => Offences(File.ReadAllText(f), pattern)
                    .Select(o => $"{Path.GetRelativePath(RepoRoot, f)} {o}"))
                .ToList();

        [Test]
        public static void NoTierListLiteralOutsideTheManifest()
        {
            var found = Scan(TierListLiteral, _ => false);
            Assert.That(found, Is.Empty,
                "A tier list literal duplicates stdlib/verification.json. Read the manifest instead "
                + "(CheckerTestSupport.Policy, tools/verification-policy.ps1, json.load):\n" + string.Join("\n", found));
        }

        [Test]
        public static void NoRatchetCeilingOutsideTheManifest()
        {
            var found = Scan(CeilingLiteral, OtherCorpusCeilings.Contains);
            Assert.That(found, Is.Empty,
                "A ratchet ceiling duplicates stdlib/verification.json. Use CheckerTestSupport.RatchetCeiling(...):\n"
                + string.Join("\n", found));
        }

        [Test]
        public static void TheScannersCatchWhatTheyAreMeantTo()
        {
            Assert.That(Offences("$tiers = 'foundation','geometry'", TierListLiteral), Is.Not.Empty);
            Assert.That(Offences("TIERS = [\"geometry\", \"graphics\"]", TierListLiteral), Is.Not.Empty);
            Assert.That(Offences("# 'foundation', 'geometry' in a comment", TierListLiteral), Is.Empty);
            Assert.That(Offences("private const int MaxLintRatchet = 32;", CeilingLiteral), Is.Not.Empty);
            Assert.That(Offences("args = ['--ratchet', '0']", CeilingLiteral), Is.Not.Empty);
            Assert.That(Offences("private static readonly int Max = CheckerTestSupport.RatchetCeiling(\"lint\");", CeilingLiteral), Is.Empty);
        }
    }
}
