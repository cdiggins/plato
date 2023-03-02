using System.Collections.Generic;

namespace PlatoParser
{
    // [Mutable]
    public class ParserCache
    {
        private readonly ParserState[] _states;

        public List<ParseError> Errors { get; } 
            = new List<ParseError>(); 

        public ParserCache(int count)
            => _states = new ParserState[count + 1];

        public void Update(ParserState state)
        {
            if (state?.Node == null) return;
            _states[state.Node.Start] = state;
        }

        public ParserState PreviousMatch(int pos)
            => pos >= _states.Length ? null : _states[pos];
    }
}