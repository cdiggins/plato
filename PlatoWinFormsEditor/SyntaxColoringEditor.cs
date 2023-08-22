using Parakeet.Demos.Plato;
using Plato.Compiler;
using Plato.Compiler.Ast;

namespace PlatoWinFormsEditor
{
    public class IEditor
    {
        public string Text { get; }
    }

    public class IClient
    {
    }

    public class Line
    {
    }

    public class TextLocation
    {
        public Line Line { get; }
        public int Column { get; }
    }

    public class TextSpan
    {
        public TextLocation Location { get;  }
        public int Length { get; }
    }

    public class Word
    {
        public Word() { }
        public string Text { get; }
        public int Start { get; }
        public int Length { get; }
        public Color Color { get; }
        public ISyntax Syntax { get; }
        public ISemantics Semantics { get; }
    }

    public interface IContext { }
    public interface ISyntax { }
    public interface ISemantics { }

    public interface IProvider
    {
        public ISyntax GetSyntax(string text);
        public IContext GetContext(int position);
        public ISemantics GetSemantics(TextSpan span);
        public IEnumerable<string> GetAutoCompleteOptions(int position);
        public string GetTooltip(ISemantics semantics); 
    }

    public class SyntaxColoringEditor
    {
        // When text changes in the editor, it updates the provider.
        // We feedback and update the syntax. 
        // When something is clicked on the editor, is there some kind of special relevance. 
        // IT needs to provide feedback to the information. 
        public string Text { get; }
    }

    public interface IUndoBuffer 
    { }

    public class Style
    {
        public bool Bold { get; set; }
        public bool Italic { get; set; }
        public bool Underline { get; set; }
        public Color Color { get; set; }

        public Style(Color color, bool bold = false, bool italic = false, bool underline = false)
        {
            Color = color;
            Bold = bold;
            Italic = italic;
            Underline = underline;
        }

        public FontStyle ToFontStyle()
        {
            var r = FontStyle.Regular;
            if (Bold) r |= FontStyle.Bold;
            if (Italic) r |= FontStyle.Italic;
            if (Underline) r |= FontStyle.Underline;
            return r;
        }

        public bool IsRegular()
            => ToFontStyle() == FontStyle.Regular;

        public Font ToFont(Font font)
            => new(font, ToFontStyle());
    }

    public class Styling
    {
        public Dictionary<string, Style> Styles = new()
        {
            //{ "Identifier", new(Color.Blue) },
            { "Literal", new(Color.Green) },
            { "Separator", new(Color.BlueViolet) },
            { "Comment", new(Color.DarkGray, false, true) },
            { "HexLiteral", new(Color.Chocolate) },
            { "BinaryLiteral", new(Color.Chocolate) },
            { "FloatLiteral", new(Color.Chocolate) },
            { "Operator", new(Color.CornflowerBlue) },
            { "IntegerLiteral", new(Color.DarkRed) },
            { "StringLiteral", new(Color.GreenYellow) },
            { "CharLiteral", new(Color.GreenYellow) },
            { "BooleanLiteral", new(Color.DodgerBlue) },
            { "NullLiteral", new(Color.DodgerBlue) },
            { "Unknown", new(Color.DarkOrange) },
            { "TypeKeyword", new(Color.MediumVioletRed, true) },
            { "StatementKeyword", new(Color.MediumAquamarine, true) },
            { "ParameterName", new(Color.BlueViolet) },
            { "TypeName", new(Color.DarkOliveGreen)},
            { "FunctionName", new(Color.DarkCyan) },
            { "FieldName", new(Color.DarkSeaGreen) },
            { "ERROR", new (Color.Crimson, true, false, true )}
        };
    }

    public class Editor
    {
        public string FilePath { get; }
        public string FileName => Path.GetFileName(FilePath);
        public RichTextBox Input { get; }
        public RichTextBox Output { get; }
        public Parser Parser { get; set; }
        public string Text => Input.Text;
        public Styling Styling = new();
        public Dictionary<string, Font> Fonts { get; }
        public CstNodeFactory CstNodeFactory = new CstNodeFactory();
        public AstNodeFactory AstNodeFactory = new AstNodeFactory();

        public Editor(string filePath, RichTextBox input, RichTextBox output, Parser parser)
        {
            Parser = parser;
            FilePath = filePath;
            Output = output;
            Input = input;

            Fonts = Styling.Styles.ToDictionary(kv => kv.Key,
                kv => kv.Value.ToFont(Input.Font));
        }

        public void ApplyStyle(string kind, int start, int length)
        {
            var style = Styling.Styles[kind];
            var selectionStart = Input.SelectionStart;
            var selectionLength = Input.SelectionLength;
            Input.Select(start, length);
            Input.SelectionColor = style.Color;
            if (!style.IsRegular())
            {
                Input.SelectionFont = Fonts[kind];
            }
            Input.Select(selectionStart, selectionLength);
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
                Output.AppendText(message + Environment.NewLine);

            foreach (var error in Parser.ParsingErrors)
            {
                Output.AppendText(error + Environment.NewLine);

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
