using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace PlatoParser
{
    // [Mutable]
    public class Grammar 
    {
        public Rule WhitespaceRule { get; protected set; }

        public Rule GetRuleFromName(string name)
        {
            var t = GetType();
            var pi = t.GetProperties().FirstOrDefault(p => p.Name == name);
            if (pi == null) return null;
            return pi.GetValue(this) as Rule;
        }

        public IEnumerable<Rule> GetRules()
            => GetType()
                .GetProperties()
                .Where(pi => typeof(Rule).IsAssignableFrom(typeof(Rule)))
                .Select(pi => pi.GetValue(this) as Rule);

        public static Rule Choice(IEnumerable<Rule> rules, [CallerMemberName] string name = "")
            => new Choice(rules, name);
      
        public static Rule Sequence(IEnumerable<Rule> rules, [CallerMemberName] string name = "")
            => new Sequence(rules, name);

        public static Rule Recursive(Func<Rule> f, [CallerMemberName] string name = "")
            => new RecursiveRule(f, name);

        public Rule Token(Rule r, [CallerMemberName] string name = "")
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentException("Name must not be null");
            if (Lookup.ContainsKey(name)) return Lookup[name];
            r = new TokenRule(r, name);
            r = r.WithName(name);
            Lookup.Add(name, r);
            return r;
        }

        public Rule Phrase(Rule r, [CallerMemberName] string name = "")
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentException("Name must not be null");
            if (Lookup.ContainsKey(name)) return Lookup[name];
            r = new NodeRule(r, WhitespaceRule, name);
            Lookup.Add(name, r);
            return r;
        }

        public Dictionary<string, Rule> Lookup = new Dictionary<string, Rule>();
    }
}