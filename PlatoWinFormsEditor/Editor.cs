using System.Text;
using Ara3D.Logging;
using Ara3D.Parakeet;
using Ara3D.Parsing;
using Ara3D.Utils;
using Ara3D.Geometry.AST;

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
        public StringBuilder LogBuilder { get; } = new StringBuilder();

        public string LogString => LogBuilder.ToString();
        public string AstString { get; }
        public string TokensString { get; }
        public string ParseNodesString { get; }
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
            Logger = new Logger(LogWriter.Create(OnLogMessage), BaseFileName);
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
                TokensString = Tokenizer.ParserNodesString;

                Logger.Log("Applying styles");
                ApplyStyles();

                Logger.Log($"Parsing");
                Parser = CommonParsers.PlatoParser(Input, Logger);
                ParseNodesString = Parser.ParserNodesString;
                ParseTreeString = Parser.ParseXml;
                CstString = Parser.CstXml;
                ErrorsString = Parser.ParserErrorsString;

                Logger.Log($"Highlighting Errors");
                HighlightErrors();

                Logger.Log($"Creating AST");
                Ast = Parser.Cst?.ToAst();
                AstString = Ast?.ToXml() ?? "";

                Logger.Log($"Successfully completed editor creation for {BaseFileName}");
                if (ErrorsString.IsNullOrWhiteSpace())
                    ErrorsString = "Parsing Success";
            }
            catch (Exception ex)
            {
                var msg = $"Unhandled exception {ex}";
                ErrorsString += msg;
                Logger.Log(msg);
            }
        }

        public void OnLogMessage(string logMsg)
        {
            LogBuilder.AppendLine(logMsg);
        }

        public void ApplyStyle(string kind, ParserRange range)
        {
            ApplyStyle(kind, range.BeginPosition, range.EndPosition);
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
                if (node.IsEnd && Styling.Styles.ContainsKey(node.Name))
                {
                    ApplyStyle(node.Name, node.Start, node.Length);
                }
            }
        }

        public void HighlightErrors()
        {
            foreach (var error in Parser.ParserErrors)
            {
                ApplyStyle("ERROR", error.Range);
            }

            // TODO: set the properties. 
            // TODO: set the events. 
            // TODO: get the known things ... highlight them! 
        }
    }
}
