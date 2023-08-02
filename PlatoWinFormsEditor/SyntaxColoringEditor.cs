using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlatoWinFormsEditor
{

    public class LineLocation
    {

    }

    public class TextLocation
    {
        public LineLocation Line { get; }
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
        public string Text { get;  }
        public Func<ISyntax, Color> ColorFromSyntax { get; }
        public Func<TextSpan, ISemantics> SemanticsFromSpan { get; }
    }

    public interface IUndoBuffer 
    { }

}
