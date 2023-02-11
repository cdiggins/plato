namespace PlatoParser;

public abstract class Rule 
{
    public abstract ParserState? Match(ParserState state);
    public static Rule operator +(Rule left, Rule right) => new Sequence(left, right);
    public static Rule operator |(Rule left, Rule right) => new Choice(left, right);
    public static Rule operator !(Rule rule) => new NotAt(rule);
    public static implicit operator Rule(string s) => new StringMatchRule(s);
    public static implicit operator Rule(char c) => new CharMatchRule(c);
    public static implicit operator Rule(char[] cs) => new Choice(cs.Select(c => (Rule)c));
    public static implicit operator Rule(Func<Rule> f) => new RecursiveRule(f);
    public static implicit operator Rule(bool b) => b ? AlwaysTrue.Value : AlwaysFalse.Value;
}

public class RecursiveRule : Rule
{
    public Func<Rule> RuleFunc { get; }
    public RecursiveRule(Func<Rule> ruleFunc) => RuleFunc = ruleFunc;
    public override ParserState? Match(ParserState state)
        => RuleFunc().Match(state);
}

public class StringMatchRule : Rule
{
    public string Pattern { get; }
    public StringMatchRule(string s) => Pattern = s;
    public override ParserState? Match(ParserState state)
    {
        if (state.AtEnd)
            return null;
        for (var i=0; i < Pattern.Length; ++i)
        {
            if (state?.Current != Pattern[i])
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
    public override ParserState? Match(ParserState state)
        => state.AtEnd ? null : state.Advance();

    public static AnyCharRule Default { get; } = new AnyCharRule();
}

public class CharRangeRule : Rule
{
    public char Low { get; }
    public char High { get; }
    public CharRangeRule(char low, char high) => (Low, High) = (low, high);
    public override ParserState? Match(ParserState state)
        => state.AtEnd ? null : state.Current >= Low && state.Current <= High ? state.Advance() : null;
}

public class CharSetRule : Rule
{
    public char[] Chars { get; }
    public CharSetRule(params char[] chars) => Chars = chars;
    public override ParserState? Match(ParserState state)
        => state.AtEnd ? null : Chars.Contains(state.Current) ? state.Advance() : null;
}

public class EndOfInputRule : Rule
{
    public override ParserState? Match(ParserState state)
        => state.AtEnd ? state : null;
    public static EndOfInputRule Default => new EndOfInputRule();
}

public class CharMatchRule : Rule
{
    public char Ch { get; }
    public CharMatchRule(char ch) => Ch = ch;
    public override ParserState? Match(ParserState state)
        => state.AtEnd ? null : state.Current == Ch ? state.Advance() : null;
}

public class NodeRule : Rule
{
    public string Name { get; }
    public Rule Rule { get; }

    public NodeRule(string name, Rule rule)
        => (Name, Rule) = (name, rule);

    public override ParserState? Match(ParserState state)
    {
        var start = state.Position;
        var result = Rule.Match(state);
        if (result is null) return null;
        var node = new ParseNode(result.Input, Name, start, result.Position, result.Node);
        return new ParserState(state.Input, result.Position, node);
    }
}

public class ZeroOrMore : Rule
{
    public Rule Rule { get; }

    public ZeroOrMore(Rule rule)
        => Rule = rule;

    public override ParserState? Match(ParserState? state)
    {
        for (var next = state; next != null; next = Rule.Match(next))
            state = next;
        return state;
    }
}

public class RepeatRule : Rule
{
    public Rule Rule { get; }
    public int Count { get; }

    public RepeatRule(Rule rule, int n)
        => (Rule, Count) = (rule, n);

    public override ParserState? Match(ParserState? state)
    {
        var i = 0;        
        for (var next = state; next != null && i < Count; next = Rule.Match(next), i++)
            state = next;
        if (i != Count)
            return null;
        return state;
    }
}

public class Sequence : Rule
{
    public Sequence(params Rule[] rules)
        => Rules = rules.ToArray();

    public Sequence(IEnumerable<Rule> rules)
        : this(rules.ToArray())
    {  }

    public Rule[] Rules { get; }

    public override ParserState? Match(ParserState state)
    {
        var newState = state;
        foreach (var rule in Rules)
        {
            newState = rule.Match(newState);
            if (newState == null) return null;
        }

        return newState;
    }
}

public class Choice : Rule
{
    public Choice(IEnumerable<Rule> rules)
        : this(rules.ToArray())
    { }

    public Choice(params Rule[] rules)
        => Rules = rules;

    public Rule[] Rules { get; }

    public override ParserState? Match(ParserState state)
    {
        foreach (var rule in Rules)
        {
            var newState = rule.Match(state);
            if (newState != null) return newState;
        }

        return null;
    }
}

public class AlwaysTrue : Rule
{
    public override ParserState? Match(ParserState state)
        => state;

    public static AlwaysTrue Value = new AlwaysTrue();
}

public class AlwaysFalse : Rule
{
    public override ParserState? Match(ParserState state)
        => null;

    public static AlwaysFalse Value = new AlwaysFalse();
}

public class At : Rule
{
    public Rule Rule { get; }

    public At(Rule rule)
        => (Rule) = (rule);

    public override ParserState? Match(ParserState state)
        => Rule.Match(state) != null ? state : null;
}

public class NotAt : Rule
{
    public Rule Rule { get; }

    public NotAt(Rule rule)
        => (Rule) = (rule);

    public override ParserState? Match(ParserState state)
        => Rule.Match(state) == null ? state : null;
}

