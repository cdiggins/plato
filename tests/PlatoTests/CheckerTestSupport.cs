// Builds real compilations and locates repository fixtures for checker and writer tests.
using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using Ara3D.Geometry.AST;
using Ara3D.Geometry.Compiler;
using Ara3D.Logging;
using Ara3D.Parakeet;
using Ara3D.Parsing;

namespace PlatoTests
{
    /// <summary>
    /// Builds a real <see cref="Compilation"/> from .plato source so the checker passes can be
    /// exercised against the standard library (the "stdlib as oracle" strategy). Parser-free
    /// unit tests do not need this; the integration tests do.
    /// </summary>
    public static class CheckerTestSupport
    {
        /// <summary>Walk up to find a repo folder by name: from the test output directory, and
        /// from this source file when the build went to a private --artifacts-path outside the
        /// checkout. Source folders do not all sit at the repo root, so each name is probed at the
        /// root and under the subfolders the restructure moved them into.</summary>
        public static string FindFolder(string name)
            => FindFolderAbove(AppContext.BaseDirectory, name)
               ?? FindFolderAbove(Path.GetDirectoryName(SourceFile()), name)
               ?? throw new DirectoryNotFoundException($"Could not locate '{name}' above " + AppContext.BaseDirectory);

        private static string FindFolderAbove(string start, string name)
        {
            var relatives = new[] { name, Path.Combine("legacy", name), Path.Combine("tests", name) };
            for (var dir = new DirectoryInfo(start); dir != null; dir = dir.Parent)
                foreach (var relative in relatives)
                {
                    var candidate = Path.Combine(dir.FullName, relative);
                    if (Directory.Exists(candidate))
                        return candidate;
                }
            return null;
        }

        private static string SourceFile([System.Runtime.CompilerServices.CallerFilePath] string path = "") => path;

        /// <summary>The shipping stdlib (stdlib-legacy) — what every existing checker fixture uses.</summary>
        public static string FindPlatoSrc()
            => FindFolder("stdlib-legacy");

        /// <summary>The forward vocabulary (stdlib): declarations plus *.library.plato bodies,
        /// in the foundation/geometry/graphics/future tier subfolders.</summary>
        public static string FindForwardStdLib()
            => FindFolder("stdlib");

        /// <summary>The verification policy, `stdlib/verification.json` (plato-388): which tiers
        /// each check covers, the ratchet ceilings, and the codegen recipe. The one copy every
        /// gate, fixture and server reads; `stdlib/VERIFICATION.md` explains it.</summary>
        public sealed record VerificationPolicy(
            IReadOnlyList<string> Tiers,
            IReadOnlyDictionary<string, IReadOnlyList<string>> Checks,
            IReadOnlyDictionary<string, int> Ratchets,
            IReadOnlyList<string> CodegenRecipe)
        {
            public static VerificationPolicy Load(string path)
            {
                using var doc = JsonDocument.Parse(File.ReadAllText(path));
                var root = doc.RootElement;
                static IReadOnlyList<string> Strings(JsonElement e)
                    => e.EnumerateArray().Select(x => x.GetString()!).ToList();
                return new VerificationPolicy(
                    Strings(root.GetProperty("tiers")),
                    root.GetProperty("checks").EnumerateObject()
                        .ToDictionary(p => p.Name, p => Strings(p.Value)),
                    root.GetProperty("ratchets").EnumerateObject()
                        .ToDictionary(p => p.Name, p => p.Value.GetProperty("ceiling").GetInt32()),
                    Strings(root.GetProperty("codegenRecipe")));
            }

            /// <summary>The tiers a check covers, in dependency order.</summary>
            public IReadOnlyList<string> TiersFor(string check) => Checks[check];
        }

        private static readonly Lazy<VerificationPolicy> PolicyFile = new(() =>
            VerificationPolicy.Load(Path.Combine(FindForwardStdLib(), "verification.json")));

        public static VerificationPolicy Policy => PolicyFile.Value;

        /// <summary>The ceiling a named ratchet enforces (`typecheck`, `lint`, `intrinsics`).
        /// Lowered, never raised: see stdlib/VERIFICATION.md.</summary>
        public static int RatchetCeiling(string ratchet) => Policy.Ratchets[ratchet];

        /// <summary>The tiers that are CONVERTED to C# and LINTED by default (stdlib-377): the
        /// policy's `codegen` check. `future` is absent from it: aspirational vocabulary that must
        /// parse and type-check, but is neither emitted nor linted unless explicitly asked for. The
        /// tier rule (stdlib/README.md) is that nothing reaches into `future`, so the covered tiers
        /// compile on their own with no dangling references.</summary>
        public static IReadOnlyList<string> ShippingTiers => Policy.TiersFor("codegen");

        /// <summary>Every tier, including `future` — the parse/type-check population.</summary>
        public static IReadOnlyList<string> AllTiers => Policy.Tiers;

        /// <summary>The forward corpus is the TIER folders, never the whole `stdlib/` tree
        /// (stdlib-398). `stdlib/tests/` sits there too and is a SEPARATE program: the law packet is
        /// merged with the tiers only by `tools/regen-forward-conformance.ps1`, so a recursive scan
        /// would put law diagnostics under the checker ratchet, which is scoped to the library.</summary>
        public static IReadOnlyList<string> ForwardStdLibTierFolders()
            => AllTiers.Select(t => Path.Combine(FindForwardStdLib(), t)).ToList();

        /// <summary>Every *.plato in the forward tiers — the population the ForwardStdLib* fixtures
        /// parse, check and assert over.</summary>
        public static IReadOnlyList<string> ForwardStdLibFiles()
            => ForwardStdLibTierFolders().SelectMany(PlatoFiles).ToList();

        public static AstNode ParseFile(string path)
        {
            var text = File.ReadAllText(path);
            // Logger.Null: the parser's default logger prints ~10 [Info] lines per file, which
            // swamps real test output when the whole stdlib is compiled per fixture.
            var parser = CommonParsers.PlatoParser(new ParserInput(text, path), Ara3D.Logging.Logger.Null);
            if (!parser.Succeeded)
                throw new Exception($"Parse failed for {path}: {string.Join("; ", parser.ErrorMessages)}");
            return parser.Cst!.ToAst();
        }

        /// <summary>Every *.plato under <paramref name="folder"/>, RECURSIVELY. The forward stdlib
        /// is split into tier subfolders (foundation/geometry/graphics/future); a top-only
        /// enumeration would find zero files there and every assertion downstream would pass
        /// vacuously. stdlib-legacy is flat, so recursion is a no-op for it.</summary>
        public static IReadOnlyList<string> PlatoFiles(string folder)
            => Directory.GetFiles(folder, "*.plato", SearchOption.AllDirectories);

        public static Compilation CompileFolder(string folder)
            => CompileFolders(new[] { folder });

        /// <summary>Compiles the union of several roots as ONE program, the same way
        /// `Plato.CLI lint &lt;root&gt;...` does. A file reachable through two roots is parsed once.</summary>
        public static Compilation CompileFolders(IEnumerable<string> folders)
        {
            var paths = folders.SelectMany(PlatoFiles)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
            var asts = paths.Select(ParseFile).ToList();
            return new Compilation(Ara3D.Logging.Logger.Null, asts);
        }

        public static Compilation CompileStdLib()
            => CompileFolder(FindPlatoSrc());

        /// <summary>Compiles the forward vocabulary — all four tiers, declarations plus the
        /// *.library.plato bodies flat beside them. See <see cref="ForwardStdLibTierFolders"/> for why
        /// this is tier-scoped rather than a recursive walk of `stdlib/`.</summary>
        public static Compilation CompileForwardStdLib()
            => CompileFolders(ForwardStdLibTierFolders());

        /// <summary>The forward vocabulary MINUS `future` — what the lint gate and the C#
        /// codegen actually see by default. See <see cref="ShippingTiers"/>.</summary>
        public static Compilation CompileForwardStdLibShippingTiers()
            => CompileFolders(ShippingTiers.Select(t => Path.Combine(FindForwardStdLib(), t)));
    }
}
