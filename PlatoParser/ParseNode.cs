namespace PlatoParser;

public class ParseNode
{
    public int Start { get; }
    public int End { get; }
    public Rule Rule { get; }
    public string Type => Rule.Name;
    public ParseNode? Previous { get; }
    public string Input { get; }
    public int Count => Math.Max(End - Start, 0);
    public string Contents => Input.Substring(Start, Count); 
    public override string ToString()
        => $"{Type}:{Start}:{End}:{EllidedContents}";

    public const int MaxLength = 20;

    public string EllidedContents 
        => Count < MaxLength 
        ? Contents : $"{Contents.Substring(0, MaxLength - 1)}...";
        
    public ParseNode(string input, Rule rule, int start, int end, ParseNode? previous)
        => (Input, Rule, Start, End, Previous) = (input, rule, start, end, previous);
}
