using Ara3D.Logging;
using Ara3D.Utils;
using Plato.Compiler;
using Plato.CSharpWriter;
using Logger = Ara3D.Logging.Logger;

namespace Plato.CLI
{
    public static class Program
    {
        public static Config Config { get; } 
            = Config.Current;

        public static void Main(string[] args)
        {
            var logger = Logger.Console;
            
            logger.Log("Opening files");
            var inputFolder = new DirectoryPath(Config.InputFolder);
            var files = inputFolder.GetFiles("*.plato");
            var docs = files.Select(f => new Document(f, logger)).ToList();
            var parsingSuccessful = docs.All(e => e.Parser.Succeeded);
            if (!parsingSuccessful)
            {
                logger.Log("Parsing failed for one of the input files, halting");
                return;
            }
            logger.Log("Parsing succeeded for all files");

            logger.Log("Compiling");
            var trees = docs.Select(e => e.Ast);
            var compilation = new Compilation(logger, trees);
            if (!compilation.CompletedCompilation)
            {
                logger.Log("Compilation was not completed");
                return;
            }

            logger.Log("Writing C# Files");
            var outputFolder = new DirectoryPath(Config.OutputFolder);
            var output = compilation.ToCSharp(outputFolder);
            foreach (var kv in output.Files)
            {
                var fp = outputFolder.RelativeFile(kv.Key);
                logger.Log($"Writing {kv.Key}");
                fp.WriteAllText(kv.Value.ToString());
            }

            logger.Log("Completed");
        }
    }
}
