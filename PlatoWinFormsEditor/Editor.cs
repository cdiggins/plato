using Ara3D.Logging;
using Ara3D.Parakeet.Grammars;
using Ara3D.Parakeet;
using Ara3D.Utils;
using Plato.Parser;
using Plato.AST;

namespace PlatoWinFormsEditor
{
    public class Editor
    {
        public FilePath FilePath { get; }
        public string BaseFileName => FilePath.GetFileNameWithoutExtension();
        
        public RichTextBox InputEditor { get; }
        public RichTextBox OutputEditor { get; }
        public ParserInput ParserInput { get; }
        
        public Parser Parser { get; }
        public AstNode Ast { get; }
        public Dictionary<string, Font> Fonts { get; }
        public ILogger Logger { get; }

        public Styling Styling = new();

        public Editor(FilePath filePath, 
            RichTextBox inputEditor, 
            RichTextBox outputEditor,
            ILogger parentLogger)
        {
            FilePath = filePath;
            InputEditor = inputEditor;
            OutputEditor = outputEditor;
            Logger = parentLogger.Create($"{BaseFileName} Editor");

            Logger.Log("Initializing fonts");
            Fonts = Styling.Styles.ToDictionary(kv => kv.Key,
                kv => kv.Value.ToFont(InputEditor.Font));

            Logger.Log("Reading file text");
            InputEditor.Text = File.ReadAllText(filePath);
            // NOTE: this line is necessary to get the correct line numbers 
            InputEditor.AppendText(Environment.NewLine);

            Logger.Log("Creating parser input");
            ParserInput = new ParserInput(InputEditor.Text, filePath);

            Logger.Log("Creating parser");
            Parser = new Parser(
                filePath,
                ParserInput,
                PlatoGrammar.Instance.File,
                PlatoTokenGrammar.Instance.Tokenizer,
                Logger);

            Logger.Log("Applying styles and outputting parse errors");
            ApplyStylesAndOutputErrors();

            Logger.Log("Creating AST trees");
            Ast = Parser.CstTree.ToAst();
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

        public void ApplyStylesAndOutputErrors()
        {
            foreach (var node in Parser.TokenNodes)
            {
                if (Styling.Styles.ContainsKey(node.Name))
                {
                    ApplyStyle(node.Name, node.Start, node.Length);
                }
            }

            foreach (var message in Parser.Diagnostics)
                OutputEditor.AppendText(message + Environment.NewLine);

            foreach (var error in Parser.ParsingErrors)
            {
                OutputEditor.AppendText(error + Environment.NewLine);

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
