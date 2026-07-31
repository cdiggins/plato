using Ara3D.Geometry.AST;
using Ara3D.Logging;
using Ara3D.Parakeet;
using Ara3D.Parsing;
using Ara3D.Utils;

namespace Ara3D.Geometry.CLI
{
    public class Document
    {
        public string Text { get; }
        public ParserInput Input { get; }
        public Parser Tokenizer { get; }
        public Parser Parser { get; }
        public AstNode Ast { get; }

        public Document(FilePath filePath, ILogger Logger)
        {
            try
            {
                Logger.Log($"Reading {filePath.GetFileName()}");
                Text = File.ReadAllText(filePath);
                Input = new ParserInput(Text, filePath);

                Logger.Log($"Tokenization");
                Tokenizer = CommonParsers.PlatoTokenizer(Input, Logger);

                Logger.Log($"Parsing");
                Parser = CommonParsers.PlatoParser(Input, Logger);

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