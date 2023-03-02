using System;

namespace PlatoParser
{
    public class ParserException : Exception
    {
        public ParserState LastValidState { get; }
        public ParserException(ParserState lastValidState, string message) : base(message) 
            => LastValidState = lastValidState;
    }
}