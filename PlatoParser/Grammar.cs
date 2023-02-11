using System.Diagnostics;
using System.Runtime.CompilerServices;

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

        public static Rule Node(Rule r, [CallerMemberName] string name = "")
            => new NodeRule(name, r);

        public static Rule Optional(Rule r)
            => r.Or(true);

        public static Rule Recursive(Func<Rule> f)
            => new RecursiveRule(f);
    }
}