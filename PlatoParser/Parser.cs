using System.Diagnostics;

namespace PlatoParser;

public class ParseNode
{
    public int Start { get; }
    public int End { get; }
    public string Type { get; }
    public ParseNode Previous { get; }
    public string Input { get; }
    public int Count => Math.Max(End - Start, 0);
    public string Contents => Input.Substring(Start, Count); 
    public override string ToString()
        => $"{Type}:{Start}:{End}:{EllidedContents}";

    public const int MaxLength = 20;

    public string EllidedContents 
        => Count < MaxLength 
        ? Contents : $"{Contents.Substring(0, MaxLength - 1)}...";
        
    public ParseNode(string input, string type, int start, int end, ParseNode previous)
        => (Input, Type, Start, End, Previous) = (input, type, start, end, previous);
}

public class ParserState
{
    public string Input { get; }
    public int Position { get; }
    public ParseNode Node { get; }

    public bool AtEnd => Position >= Input.Length;
    public char Current => Input[Position];

    public ParserState(string input, int position, ParseNode node)
        => (Input, Position, Node) = (input, position, node);

    public ParserState? Advance()
        => AtEnd ? null : new ParserState(Input, Position + 1, Node);

    public ParserState Match(Rule r)
    {
        // All matching comes through here
        var tmp = r.Match(this);
        if (tmp != null)
        {
            Debug.WriteLine($"Matched rule {r.Name}");
        }
        return tmp;
    }
}
