namespace PlatoParser
{
    /// <summary>
    /// A class that represents the state of the parser, and the parse tree. 
    /// </summary>
    public class ParserState
    {
        public ParserInput Input { get; }
        public int Position { get; }
        public ParseNode Node { get; }

        public bool AtEnd => Position >= Input.Length;
        public char Current => Input[Position];

        public ParserState(ParserInput input, int position = 0, ParseNode node = null)
            => (Input, Position, Node) = (input, position, node);

        public ParserState Advance()
            => AtEnd ? null : new ParserState(Input, Position + 1, Node);

        public ParserState JumpToEnd()
            => new ParserState(Input, Input.Length, Node);

        public override string ToString()
            => $"Parse state: line {CurrentLineIndex} column {CurrentColumn} position {Position}/{Input.Length} node = {Node}";

        public int CurrentLineIndex
            => Input.GetLineIndex(Position);

        public int CurrentColumn
            => Input.GetColumn(Position);

        public string CurrentLine
            => Input.GetLine(CurrentLineIndex);

        public string Indicator
            => Input.GetIndicator(Position);
    }
}