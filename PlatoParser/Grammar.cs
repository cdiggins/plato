using System.Runtime.CompilerServices;
using System.Xml.Linq;

namespace PlatoParser
{

    // [Mutable]
    public class Grammar 
    {
        public Rule? WhitespaceRule { get; protected set; }

        public Rule? GetRuleFromName(string name)
        {
            var t = GetType();
            var pi = t.GetProperties().FirstOrDefault(p => p.Name == name);
            if (pi == null) return null;
            return pi.GetValue(this) as Rule;
        }

        public static Rule Choice(IEnumerable<Rule> rules, [CallerMemberName] string name = "")
            => new Choice(rules, name);
      
        public static Rule Sequence(IEnumerable<Rule> rules, [CallerMemberName] string name = "")
            => new Sequence(rules, name);

        public static Rule Recursive(Func<Rule> f)
            => new RecursiveRule(f);

        public Rule Token(Rule r, [CallerMemberName] string name = "")
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentException("Name must not be null");
            if (Lookup.ContainsKey(name)) return Lookup[name];
            r = r.WithName(name);
            Lookup.Add(name, r);
            return r;
        }

        public Rule Phrase(Rule r, bool createNode = true, [CallerMemberName] string name = "")
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentException("Name must not be null");
            if (Lookup.ContainsKey(name)) return Lookup[name];
            if (createNode)
            {
                r = new NodeRule(r);
            }
            if (WhitespaceRule != null)
            {
                // Parse the whitespace, but don't put it in the node
                r = r.Then(WhitespaceRule);
            }
            r = r.WithName(name);
            Lookup.Add(name, r);
            return r;
        }

        public Dictionary<string, Rule> Lookup = new Dictionary<string, Rule>();
    }
}