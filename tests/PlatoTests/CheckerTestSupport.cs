using System;
using System.IO;
using System.Linq;
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
        /// <summary>Walk up from the test output directory to find a repo folder by name.
        /// Source folders do not all sit at the repo root, so each name is probed at the
        /// root and under the subfolders the restructure moved them into.</summary>
        public static string FindFolder(string name)
        {
            var relatives = new[] { name, Path.Combine("legacy", name), Path.Combine("tests", name) };
            for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir != null; dir = dir.Parent)
            {
                foreach (var relative in relatives)
                {
                    var candidate = Path.Combine(dir.FullName, relative);
                    if (Directory.Exists(candidate))
                        return candidate;
                }
            }
            throw new DirectoryNotFoundException(
                $"Could not locate '{name}' above " + AppContext.BaseDirectory);
        }

        /// <summary>The shipping stdlib (stdlib-legacy) — what every existing checker fixture uses.</summary>
        public static string FindPlatoSrc()
            => FindFolder("stdlib-legacy");

        /// <summary>The forward vocabulary (stdlib): declarations plus *.library.plato bodies,
        /// in the foundation/geometry/graphics/future tier subfolders.</summary>
        public static string FindForwardStdLib()
            => FindFolder("stdlib");

        /// <summary>The tiers that are CONVERTED to C# and LINTED by default (stdlib-377).
        /// `future` is deliberately absent: it is aspirational vocabulary that must parse and
        /// type-check, but is neither emitted nor linted unless explicitly asked for. The tier
        /// rule (stdlib/README.md) is that nothing reaches into `future`, so these three compile
        /// on their own with no dangling references.</summary>
        public static readonly IReadOnlyList<string> ShippingTiers =
            new[] { "foundation", "geometry", "graphics" };

        /// <summary>Every tier, including `future` — the parse/type-check population.</summary>
        public static readonly IReadOnlyList<string> AllTiers =
            new[] { "foundation", "geometry", "graphics", "future" };

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

        /// <summary>
        /// Compiles the forward vocabulary. Plato.CLI enumerates *.plato non-recursively, which picks
        /// up both the declaration files and the *.library.plato bodies sitting flat beside them.
        /// </summary>
        public static Compilation CompileForwardStdLib()
            => CompileFolder(FindForwardStdLib());

        /// <summary>The forward vocabulary MINUS `future` — what the lint gate and the C#
        /// codegen actually see by default. See <see cref="ShippingTiers"/>.</summary>
        public static Compilation CompileForwardStdLibShippingTiers()
            => CompileFolders(ShippingTiers.Select(t => Path.Combine(FindForwardStdLib(), t)));
    }
}
