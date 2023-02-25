namespace PlatoParser
{
    // [Mutable]
    public class ParseCache
    {
        private readonly ParserState[] _states;

        public ParseCache(int count)
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