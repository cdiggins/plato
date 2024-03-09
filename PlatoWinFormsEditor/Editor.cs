using Ara3D.Logging;
using Ara3D.Parakeet;
using Ara3D.Parsing;
using Ara3D.Utils;
using Plato.AST;

namespace PlatoWinFormsEditor
{
    /// <summary>
    /// Note this is almost completely generic.
    /// There is only a small dependency on the AST.
    /// </summary>
    public class Editor
    {
        public FilePath FilePath { get; }
        public string BaseFileName => FilePath.GetFileNameWithoutExtension();
        
        public RichTextBox InputEditor { get; }
        
        public ParserInput Input { get; }
        public Parser Parser { get; }
        public Parser Tokenizer { get; }

        public AstNode? Ast { get; }
        public Dictionary<string, Font> Fonts { get; }
        public ILogger Logger { get; }

        public string AstString { get; }
        public string TokensString { get; }
        public string ParseTreeString { get; }
        public string CstString { get; }
        public string ErrorsString { get; }

        public Styling Styling = new();

        public Editor(FilePath filePath, 
            RichTextBox inputEditor, 
            ILogger logger)
        {
            FilePath = filePath;
            InputEditor = inputEditor;
            Logger = logger;
            Logger.Log("Initializing fonts");
            Fonts = Styling.Styles.ToDictionary(kv => kv.Key,
                kv => kv.Value.ToFont(InputEditor.Font));

            try
            {
                Logger.Log($"Creating editor for {BaseFileName}");
                Logger.Log($"Reading file text {filePath}");
                InputEditor.Text = File.ReadAllText(filePath);
                // NOTE: this line is necessary to get the correct line numbers 
                InputEditor.AppendText(Environment.NewLine);

                // Don't allow the user to edit code. 
                InputEditor.ReadOnly = true;

                Logger.Log($"Creating input");
                Input = new ParserInput(InputEditor.Text, filePath);

                Logger.Log($"Tokenization");
                Tokenizer = CommonParsers.PlatoTokenizer(Input, Logger);

                Logger.Log($"Parsing");
                Parser = CommonParsers.PlatoParser(Input, Logger);

                Logger.Log($"Creating AST");
                Ast = Parser.Cst?.ToAst();

                Logger.Log("Applying styles");
                ApplyStyles();

                Logger.Log($"Creating strings");
                AstString = Ast?.ToXml() ?? "";
                CstString = Parser.CstXml;
                TokensString = Tokenizer.ParserNodes.JoinStrings(Environment.NewLine);
                ParseTreeString = Parser.ParseXml;
                ErrorsString = Parser.ErrorMessages.JoinStrings(Environment.NewLine);

                Logger.Log($"Successfully completed editor creation for {BaseFileName}");
            }
            catch (Exception ex)
            {
                Logger.Log($"Unhandled exception {ex}");
            }
        }

        public void ApplyStyle(string kind, int start, int length)
        {
            var style = Styling.Styles[kind];
            var selectionStart = InputEditor.SelectionStart;
            var selectionLength = InputEditor.SelectionLength;
            InputEditor.Select(start, length);
            InputEditor.SelectionColor = style.Color;
            if (!style.IsRegular())
            {
                InputEditor.SelectionFont = Fonts[kind];
            }
            InputEditor.Select(selectionStart, selectionLength);
        }

        public void ApplyStyles()
        {
            foreach (var node in Tokenizer.ParserNodes)
            {
                if (Styling.Styles.ContainsKey(node.Name))
                {
                    ApplyStyle(node.Name, node.Start, node.Length);
                }
            }

            foreach (var error in Parser.ParserErrors)
            {
                var pos1 = error.ParentState.Position;
                var pos2 = error.State.Position;
                var cnt = pos2 - pos1;
                if (cnt > 5)
                {
                    cnt = 5;
                    pos1 = pos2 - cnt;
                }

                ApplyStyle("ERROR", pos1, cnt);
            }

            // TODO: set the properties. 
            // TODO: set the events. 
            // TODO: get the known things ... highlight them! 
        }
    }
}
