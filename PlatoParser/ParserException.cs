using System;

namespace PlatoParser
{
    /// <summary>
    /// Thrown in rare cases by the parser usually indicates a problem in the grammar.
    /// For example if the parser is no longer making progress, it might be because 
    /// of nested ZeroOrMore rules. 
    /// </summary>
    public class ParserException : Exception
    {
        public ParserState LastValidState { get; }
        public ParserException(ParserState lastValidState, string message) : base(message) 
            => LastValidState = lastValidState;
    }
}