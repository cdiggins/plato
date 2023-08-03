using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Parakeet;
using Parakeet.Demos;
using Parakeet.Demos.Plato;
using Plato.Compiler;

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
    }

    public class Styling
    {
        public Dictionary<string, Style> Styles = new()
        {
            { "Identifier", new(Color.Blue) },
            { "Literal", new(Color.Green) },
            { "BinaryOperator", new(Color.BlueViolet) },
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

        public Editor(string filePath, RichTextBox input, RichTextBox output)
        {
            FilePath = filePath;
            Output = output;
            Input = input;
            Input.Text = File.ReadAllText(filePath);
            Parse();
        }

        public void Parse()
        {
            Parser = new Parser(Text, PlatoGrammar.Instance.File,
                CstNodeFactory.Create, AstFromPlatoCst.Convert);

            foreach (var node in Parser.Nodes)
            {
                if (Styling.Styles.ContainsKey(node.Name))
                {
                    var style = Styling.Styles[node.Name];
                    Input.Select(node.Start, node.Length);
                    Input.SelectionColor = style.Color;
                    Input.Select(0,0);
                }
            }

            foreach (var message in Parser.Diagnostics)
                Output.AppendText(message + Environment.NewLine);
            
            foreach (var error in Parser.ParsingErrors)
                Output.AppendText(error + Environment.NewLine);

            // TODO: set the properties. 
            // TODO: set the events. 
            // TODO: get the known things ... highlight them! 
        }
    }
}
