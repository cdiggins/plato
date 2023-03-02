namespace PlatoParser
{
    /// <summary>
    /// Parse errors are only created within sequence when a "OnError" node fails. 
    /// They occur because previous tokens in a sequence are unambiguous about what 
    /// is expected to follow. For example, an "if" keyword indicate that a parenthesized
    /// expression must follow, and then another statement, with an optional else clause. 
    /// </summary>
    public class ParseError
    {
        public ParseError(Rule expected, Rule parent, ParserState parentState, ParserState lastState, string message)
        {
            Expected = expected;
            Parent = parent;
            ParentState = parentState;
            LastState = lastState;
            Message = message;
        }

        public Rule Expected { get; }
        public Rule Parent { get; }
        public ParserState ParentState { get; }
        public ParserState LastState { get; }
        public string Message { get; }
    }
}