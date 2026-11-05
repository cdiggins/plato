// Parses a compiler input document and retains its text, parser state and diagnostics.
using Ara3D.Geometry.AST;
using Ara3D.Logging;
using Ara3D.Parakeet;
using Ara3D.Parakeet.Cst;
using Ara3D.Parakeet.Grammars;
using Ara3D.Parsing;
using Ara3D.Utils;

namespace Ara3D.Geometry.CLI
{
    public class Document
    {
        // A grammar property rebuilds its whole rule expression on every read before it returns the rule it
        // cached the first time, so CommonParsers.PlatoParser pays for the entire grammar on every file.
        private static readonly Rule StartRule = PlatoGrammar.Instance.StartRule;

        public string Text { get; }
        public ParserInput Input { get; }
        public Parser Parser { get; }
        public AstNode Ast { get; }

        public Document(FilePath filePath, ILogger Logger)
        {
            try
            {
                Logger.Log($"Reading {filePath.GetFileName()}");
                Text = File.ReadAllText(filePath);
                Input = new ParserInput(Text, filePath);

                Logger.Log($"Parsing");
                Parser = new Parser(Input, StartRule, AllCstFactories.Plato, Logger);

                Logger.Log($"Creating AST");
                Ast = Parser.Cst?.ToAst();

                if (Parser.ErrorMessages.Count == 0)
                {
                    Logger.Log("Success");
                }

                foreach (var err in Parser.ErrorMessages)
                {
                    Logger.Log(err);
                }
            }

            catch (Exception ex)
            {
                var msg = $"Unhandled exception {ex}";
                Logger.Log(msg);
            }
        }
    }
}