namespace PlatoParser
{
    public class ParserState
    {
        public ParserInput Input { get; }
        public int Position { get; }
        public ParseNode Node { get; }

        public bool AtEnd => Position >= Input.Length;
        public char Current => Input[Position];

        public ParserState(ParserInput input, int position, ParseNode node)
            => (Input, Position, Node) = (input, position, node);

        public ParserState Advance()
            => AtEnd ? null : new ParserState(Input, Position + 1, Node);

        public ParserState JumpToEnd()
            => new ParserState(Input, Input.Length, Node);

        public override string ToString()
            => $"Parser state position {Position}/{Input.Length} Node = {Node}"; 
    }
}