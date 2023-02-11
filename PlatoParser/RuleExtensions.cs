
namespace PlatoParser
{
    public static class RuleExtensions
    {
        public static Rule ToCharSetRule(this string s)
            => new CharSetRule(s.ToCharArray());

        public static Rule At(this Rule rule)
            => new At(rule);

        public static Rule Then(this Rule rule, params Rule[] rules)
            => new Sequence(rules.Prepend(rule));

        public static Rule ThenNot(this Rule rule, Rule other)
            => rule.Then(other.NotAt());

        public static Rule Or(this Rule rule, params Rule[] rules)
            => new Choice(rules.Prepend(rule));

        public static Rule NotAt(this Rule rule)
            => new NotAt(rule);

        public static Rule ButNot(this Rule rule, Rule except)
            => except.NotAt().Then(rule);

        public static Rule Optional(this Rule rule)
            => rule.Or(true);

        public static Rule Except(this Rule rule, Rule except)
            => except.NotAt() + rule;

        public static Rule Node(this Rule rule, string name = "")
            => new NodeRule(name, rule);

        public static Rule ZeroOrMore(this Rule rule)
            => new ZeroOrMore(rule);

        public static Rule OneOrMore(this Rule rule)
            => rule.Then(rule.ZeroOrMore());

        public static Rule Repeat(this Rule rule, int n)
            => new RepeatRule(rule, n);

        public static Rule To(this char c1, char c2)
            => new CharRangeRule(c1, c2);

        public static ParserState Parse(this string s, Rule r)
            => r.Match(new ParserState(s, 0, null));
    }
}