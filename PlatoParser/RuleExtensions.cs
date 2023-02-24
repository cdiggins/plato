using System.Runtime.CompilerServices;

namespace PlatoParser
{
    public static class RuleExtensions
    {
        public static Rule ToCharSetRule(this string s, [CallerMemberName] string name = "")
            => new CharSetRule(s.ToCharArray(), name);

        public static Rule At(this Rule rule, [CallerMemberName] string name = "")
            => new At(rule, name);

        public static Rule Then(this Rule rule, Rule other, [CallerMemberName] string name = "")
            => new Sequence(new[] { rule, other }, name);

        public static Rule ThenNot(this Rule rule, Rule other, [CallerMemberName] string name = "")
            => rule.Then(other.NotAt(), name);

        public static Rule Optional(this Rule rule, [CallerMemberName] string name = "")
            => new Optional(rule, name);

        public static Rule Or(this Rule rule, Rule other, [CallerMemberName] string name = "")
            => new Choice(new[] { rule, other }, name);

        public static Rule NotAt(this Rule rule, [CallerMemberName] string name = "")
            => new NotAt(rule, name);

        public static Rule ButNot(this Rule rule, Rule except, [CallerMemberName] string name = "")
            => except.NotAt().Then(rule).WithName(name);

        public static Rule Except(this Rule rule, Rule except, [CallerMemberName] string name = "")
            => (except.NotAt() + rule).WithName(name);

        public static Rule Node(this Rule rule, [CallerMemberName] string name = "")
            => new NodeRule(rule, name);

        public static Rule ZeroOrMore(this Rule rule, [CallerMemberName] string name = "")
            => new ZeroOrMore(rule, name);

        public static Rule OneOrMore(this Rule rule, [CallerMemberName] string name = "")
            => rule.Then(rule.ZeroOrMore()).WithName(name);

        public static Rule To(this char c1, char c2, [CallerMemberName] string name = "")
            => new CharRangeRule(c1, c2, name);

        public static ParserState? Parse(this string s, Rule r, ParseResults results)
            => r.Match(new ParserState(s, 0, null), results);

        public static ParserState? Parse(this string s, Rule r)
            => r.Match(new ParserState(s, 0, null), new ParseResults(s.Length));
    }
}