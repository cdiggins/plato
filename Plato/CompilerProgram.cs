using Ara3D.Logging;
using Ara3D.Utils;
using Ara3D.Parakeet;
using Ara3D.Parakeet.Grammars;
using Ara3D.Parsing;
using Plato.AST;
using Plato.CSharpWriter;

namespace Plato
{
    public static class CompilerProgram
    {
        public static ParserInput OpenFile(FilePath filePath)
            => new(filePath.ReadAllText(), filePath);

        public static void Main(string[] args)
        {
            var logger = Logger.Default;

            var currentFolder = PathUtil.GetCallerSourceFolder();
            var inputFolder = currentFolder.RelativeFolder("..", "PlatoStandardLibrary");
            var outputFolder = currentFolder.RelativeFolder("..", "PlatoOutput");

            logger.Log("Opening files");

            var files = inputFolder.GetFiles("*.plato").ToList();
            var inputs = files.Select(OpenFile).ToList();

            logger.Log("Parsing");
            var parsers = inputs.Select(i => CommonParsers.PlatoParser(i, logger)).ToList();
            var parsingSuccess = parsers.All(p => p.Succeeded);
            if (!parsingSuccess)
            {
                logger.Log("Parsing was not successful");
                return;
            }
            
            logger.Log("Creating AST trees");
            var trees = new List<AstNode>();
            foreach (var p in parsers)
            {
                try
                {
                    trees.Add(p.Cst.ToAst());
                }
                catch (Exception ex)
                {
                    logger.LogError($"Error occurred '{ex.Message}' when generating AST from CST for {p.Input.File}");
                    return;
                }
            }
            var compiler = new Compiler.Compilation(logger, trees);

            if (!compiler.CompletedCompilation)
            {
                logger.Log("Compilation was not completed, terminating.");
                return;
            }

            logger.Log("Writing C#");
            var output = compiler.ToCSharp(outputFolder);
            foreach (var kv in output.Files)
            {
                var fp = new FilePath(kv.Key);
                fp.WriteAllText(kv.Value.ToString());
            }


            //Logger.Log("Writing HTML");
            //File.WriteAllText(Path.Combine(outputFolder, "output.plato.html"), Compiler.ToPlatoHtml());

            /*
            Logger.Log("Writing JavaScript");
            var inputFolder = outputFolder;
            var prologue = File.ReadAllText(Path.Combine(inputFolder, "prologue.js"));
            var epilogue = File.ReadAllText(Path.Combine(inputFolder, "epilogue.js"));
            var output = prologue 
                         + Environment.NewLine
                         + Compiler.ToJavaScript() 
                         + Environment.NewLine 
                         + epilogue;
            File.WriteAllText(Path.Combine(outputFolder, "output.js"), output);

            */

            /*
            var vsgFolder = Path.Combine(outputFolder, "vsg");
            FileUtil.CreateAndClearDirectory(vsgFolder);
            var i = 0;
            foreach (var f in Compiler.Graphs)
            {
                var text = JsonSerializer.Serialize(f, new JsonSerializerOptions() { WriteIndented = true });
                var filePath = Path.Combine(vsgFolder, $"{f.Name}_{i++}.json");
                File.WriteAllText(filePath, text);
            }
            */

            //Output += GetConstraintsOutput();
            //Output += GetOperationsOutput();
            //Output += GetTypeGuesserOutput();
        }
    }
}
