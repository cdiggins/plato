using System.Collections.Generic;

namespace PlatoParser
{
    /// <summary>
    /// A mutable class that contains a lookup per character of 
    /// successful parse passes. This is effectively the memoizer
    /// step of a packrate parsers and allows linear time parsing of 
    /// PEG grammars.
    /// 
    /// This also contains all accumulated parse errors. 
    /// </summary>
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