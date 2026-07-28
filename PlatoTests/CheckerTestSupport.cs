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
        /// <summary>Walk up from the test output directory to find a repo folder by name.</summary>
        public static string FindFolder(string name)
        {
            for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir != null; dir = dir.Parent)
            {
                var candidate = Path.Combine(dir.FullName, name);
                if (Directory.Exists(candidate))
                    return candidate;
            }
            throw new DirectoryNotFoundException(
                $"Could not locate '{name}' above " + AppContext.BaseDirectory);
        }

        /// <summary>The shipping stdlib (stdlib-legacy) — what every existing checker fixture uses.</summary>
        public static string FindPlatoSrc()
            => FindFolder("stdlib-legacy");

        /// <summary>The forward vocabulary (stdlib): declarations plus *.library.plato bodies, flat.</summary>
        public static string FindForwardStdLib()
            => FindFolder("stdlib");

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

        public static Compilation CompileFolder(string folder)
        {
            var asts = Directory.GetFiles(folder, "*.plato").Select(ParseFile).ToList();
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
    }
}
