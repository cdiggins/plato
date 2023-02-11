namespace PlatoParser
{
    public interface IRuleOrGrammar
    { }

    public interface IGrammar : IRuleOrGrammar
    {
        Rule Root { get; }
    }

    public class Grammar : IGrammar
    {
        public Rule Root { get; }

        public static Rule Choice(params Rule[] rules)
            => new Choice(rules);

        public static Rule Sequence(params Rule[] rules)
            => new Sequence(rules);

        public static Rule Node(Rule r)
            => new NodeRule(r);
    }
}