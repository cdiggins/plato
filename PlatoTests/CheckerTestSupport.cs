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
        /// <summary>Walk up from the test output directory to find the repo's plato-src folder.</summary>
        public static string FindPlatoSrc()
        {
            for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir != null; dir = dir.Parent)
            {
                var candidate = Path.Combine(dir.FullName, "plato-src");
                if (Directory.Exists(candidate))
                    return candidate;
            }
            throw new DirectoryNotFoundException(
                "Could not locate 'plato-src' above " + AppContext.BaseDirectory);
        }

        public static AstNode ParseFile(string path)
        {
            var text = File.ReadAllText(path);
            var parser = CommonParsers.PlatoParser(new ParserInput(text, path));
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
    }
}
