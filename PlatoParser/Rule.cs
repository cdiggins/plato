using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;

namespace PlatoParser
{
    public abstract class Rule
    {
        protected abstract ParserState MatchImplementation(ParserState state, ParseCache cache);

        public ParserState Match(ParserState state, ParseCache cache)
        {
            // Set NO_CACHING to true to see performance of naive parser. 
#if NO_CACHING
        return MatchImplementation(state, cache);
#else
            // If we have already parsed this position and rule combination: return it. 
            var prev = cache.PreviousMatch(state.Position);
            if (prev?.Node?.Rule == this)
                return prev;

            try
            {
                var result = MatchImplementation(state, cache);
                cache.Update(result);
                return result;
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Uncaught exception {e}");
                throw;
            }
#endif
        }

        public static Rule operator +(Rule left, Rule right) => new Sequence(left, right);
        public static Rule operator |(Rule left, Rule right) => new Choice(left, right);
        public static Rule operator !(Rule rule) => new NotAt(rule);
        public static implicit operator Rule(string s) => new StringMatchRule(s);
        public static implicit operator Rule(char c) => new CharMatchRule(c);
        public static implicit operator Rule(char[] cs) => new Choice(cs.Select(c => (Rule)c));
        public static implicit operator Rule(string[] xs) => new Choice(xs.Select(x => (Rule)x));
        public static implicit operator Rule(Func<Rule> f) => new RecursiveRule(f);
        public static implicit operator Rule(bool b) => b ? (Rule)AlwaysTrue.Value : AlwaysFalse.Value;
        public string Name { get; } = string.Empty;
        public Rule(string name) => Name = name;
        public abstract Rule WithName(string name);
    }

    public class TokenRule : Rule
    {
        public Rule Rule { get; }
        public TokenRule(Rule r, [CallerMemberName] string name = "") : base(name) => Rule = r;
        public override Rule WithName(string name) => new TokenRule(Rule, name);
        protected override ParserState MatchImplementation(ParserState state, ParseCache cache)
            => Rule.Match(state, cache);
    }

    public class RecursiveRule : Rule
    {
        public Func<Rule> RuleFunc { get; }
        public RecursiveRule(Func<Rule> ruleFunc, [CallerMemberName] string name = "") : base(name) => RuleFunc = ruleFunc;
        public override Rule WithName(string name) => new RecursiveRule(RuleFunc, name);
        protected override ParserState MatchImplementation(ParserState state, ParseCache cache)
            => RuleFunc().Match(state, cache);
    }

    public class StringMatchRule : Rule
    {
        public string Pattern { get; }
        public StringMatchRule(string s, [CallerMemberName] string name = "") : base(name) => Pattern = s;
        public override Rule WithName(string name) => new StringMatchRule(Pattern, name);
        protected override ParserState MatchImplementation(ParserState state, ParseCache cache)
        {
            for (var i = 0; i < Pattern.Length; ++i)
            {
                if (state.AtEnd || state?.Current != Pattern[i])
                    return null;
                var tmp = state?.Advance();
                if (tmp == null) return null;
                state = tmp;
            }
            return state;
        }
    }

    public class AnyCharRule : Rule
    {
        public AnyCharRule([CallerMemberName] string name = "") : base(name) { }
        public override Rule WithName(string name) => new AnyCharRule(name);
        protected override ParserState MatchImplementation(ParserState state, ParseCache cache)
            => state.AtEnd ? null : state.Advance();
        public static AnyCharRule Default { get; } = new AnyCharRule();
    }

    public class CharRangeRule : Rule
    {
        public char Low { get; }
        public char High { get; }
        public CharRangeRule(char low, char high, string name = "") : base(name) => (Low, High) = (low, high);
        public override Rule WithName(string name) => new CharRangeRule(Low, High, name);
        protected override ParserState MatchImplementation(ParserState state, ParseCache cache)
            => state.AtEnd ? null : state.Current >= Low && state.Current <= High ? state.Advance() : null;
    }

    public class CharSetRule : Rule
    {
        public char[] Chars { get; }
        public CharSetRule(params char[] chars) : this(chars, "") { }
        public CharSetRule(char[] chars, [CallerMemberName] string name = "") : base(name) => Chars = chars;
        public override Rule WithName(string name) => new CharSetRule(Chars, name);
        protected override ParserState MatchImplementation(ParserState state, ParseCache cache)
            => state.AtEnd ? null : Chars.Contains(state.Current) ? state.Advance() : null;
    }

    public class EndOfInputRule : Rule
    {
        public EndOfInputRule([CallerMemberName] string name = "") : base(name) { }
        public override Rule WithName(string name) => new EndOfInputRule(name);
        protected override ParserState MatchImplementation(ParserState state, ParseCache cache) => state.AtEnd ? state : null;
        public static EndOfInputRule Default => new EndOfInputRule();
    }

    public class CharMatchRule : Rule
    {
        public char Ch { get; }
        public CharMatchRule(char ch, [CallerMemberName] string name = "") : base(name) => Ch = ch;
        public override Rule WithName(string name) => new CharMatchRule(Ch, name);
        protected override ParserState MatchImplementation(ParserState state, ParseCache cache)
            => state.AtEnd ? null : state.Current == Ch ? state.Advance() : null;
    }

    public class NodeRule : Rule
    {
        public Rule Rule { get; }
        public Rule Eat { get; }

        public NodeRule(Rule rule, Rule eat, [CallerMemberName] string name = "") : base(name) => (Rule, Eat) = (rule, eat);
        public override Rule WithName(string name) => new NodeRule(Rule, Eat, name);

        protected override ParserState MatchImplementation(ParserState state, ParseCache cache)
        {
            var start = state.Position;
            var result = Rule.Match(state, cache);
            if (result is null) return null;
            var node = new ParseNode(result.Input, this, start, result.Position, result.Node);
            var r = new ParserState(result.Input, result.Position, node);
            // Parse the data to eat 
            var tmp = Eat?.Match(r, cache);
            if (tmp != null) return tmp;
            return r;
        }
    }

    public class ZeroOrMore : Rule
    {
        public Rule Rule { get; }

        public ZeroOrMore(Rule rule, [CallerMemberName] string name = "") : base(name) => Rule = rule;
        public override Rule WithName(string name) => new ZeroOrMore(Rule, name);

        protected override ParserState MatchImplementation(ParserState state, ParseCache cache)
        {
            var curr = state;
            var next = Rule.Match(curr, cache);
            while (next != null)
            {
                curr = next;
                next = Rule.Match(curr, cache);
                if (next != null)
                {
                    if (next.Position <= curr.Position)
                    {
                        throw new Exception("Parser is no longer making progress");
                    }
                }
            }
            return curr;
        }
    }

    public class Optional : Rule
    {
        public Rule Rule { get; }

        public Optional(Rule rule, [CallerMemberName] string name = "") : base(name) => Rule = rule;
        public override Rule WithName(string name) => new Optional(Rule, name);

        protected override ParserState MatchImplementation(ParserState state, ParseCache cache)
            => Rule.Match(state, cache) ?? state;
    }

    public class Sequence : Rule
    {
        public Rule[] Rules { get; }

        public Sequence(Rule[] rules, [CallerMemberName] string name = "") : base(name) => Rules = rules;
        public Sequence(params Rule[] rules) : this(rules, "") { }
        public Sequence(IEnumerable<Rule> rules, string name = "") : this(rules.ToArray(), name) { }
        public override Rule WithName(string name) => new Sequence(Rules, name);

        protected override ParserState MatchImplementation(ParserState state, ParseCache cache)
        {
            var newState = state;
            foreach (var rule in Rules)
            {
                newState = rule.Match(newState, cache);
                if (newState == null) return null;
            }

            return newState;
        }
    }

    public class Choice : Rule
    {
        public Rule[] Rules { get; }

        public Choice(IEnumerable<Rule> rules, [CallerMemberName] string name = "") : this(rules.ToArray(), name) { }
        public Choice(Rule[] rules, string name = "") : base(name) => Rules = rules;
        public Choice(params Rule[] rules) : this(rules, "") { }
        public override Rule WithName(string name) => new Choice(Rules, name);

        protected override ParserState MatchImplementation(ParserState state, ParseCache cache)
        {
            foreach (var rule in Rules)
            {
                var newState = rule.Match(state, cache);
                if (newState != null) return newState;
            }

            return null;
        }
    }

    public class AlwaysTrue : Rule
    {
        public AlwaysTrue([CallerMemberName] string name = "") : base(name) { }
        public override Rule WithName(string name) => new AlwaysTrue(name);
        protected override ParserState MatchImplementation(ParserState state, ParseCache cache) => state;
        public static readonly AlwaysTrue Value = new AlwaysTrue();
    }

    public class AlwaysFalse : Rule
    {
        public AlwaysFalse([CallerMemberName] string name = "") : base(name) { }
        public override Rule WithName(string name) => new AlwaysFalse(name);
        protected override ParserState MatchImplementation(ParserState state, ParseCache cache) => null;
        public static readonly AlwaysFalse Value = new AlwaysFalse();
    }

    public class At : Rule
    {
        public Rule Rule { get; }
        public At(Rule rule, [CallerMemberName] string name = "") : base(name) => (Rule) = (rule);
        public override Rule WithName(string name) => new At(Rule, name);
        protected override ParserState MatchImplementation(ParserState state, ParseCache cache)
            => Rule.Match(state, cache) != null ? state : null;
    }

    public class NotAt : Rule
    {
        public Rule Rule { get; }
        public NotAt(Rule rule, [CallerMemberName] string name = "") : base(name) => (Rule) = (rule);
        public override Rule WithName(string name) => new NotAt(Rule, name);
        protected override ParserState MatchImplementation(ParserState state, ParseCache cache)
            => Rule.Match(state, cache) == null ? state : null;
    }

    public class Recovery : Rule
    {
        public Rule Rule { get; }
        public Recovery(Rule rule, [CallerMemberName] string name = "") : base(name) => Rule = rule;
        public override Rule WithName(string name) => new Recovery(Rule, name);
        protected override ParserState MatchImplementation(ParserState state, ParseCache cache) => state;
    }
}