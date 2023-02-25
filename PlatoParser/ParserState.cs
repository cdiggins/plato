namespace PlatoParser
{
    public class ParserState
    {
        public string Input { get; }
        public int Position { get; }
        public ParseNode Node { get; }

        public bool AtEnd => Position >= Input.Length;
        public char Current => Input[Position];

        public ParserState(string input, int position, ParseNode node)
            => (Input, Position, Node) = (input, position, node);

        public ParserState Advance()
            => AtEnd ? null : new ParserState(Input, Position + 1, Node);
    }
}